using Unity.Mathematics;
using UnityEngine;
using Sirenix.OdinInspector;
using Nitou.BatchProcessor;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.TCC.Foundation;

namespace Nitou.TCC.CharacterControl.Core
{
    /// <summary>
    /// <see cref="UnityEngine.CharacterController"/> を使用して動作する Brain．
    /// Agentの高さと幅は <see cref="CharacterSettings.Height"/> と <see cref="CharacterSettings.Radius"/> によって決定される．
    /// </summary>
    [AddComponentMenu(MenuList.MenuBrain + "Character Brain")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(Order.UpdateBrain)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterSettings))]
    public sealed class CharacterBrain : BrainBase, IActorSettingUpdateReceiver
    {
        /// <summary>
        /// キャラクターを移動させるためのコンポーネントへの参照．
        /// </summary>
        private CharacterController _controller;

        /// <summary>
        /// 事前計算を実行するためのコンポーネント．
        /// 内部からコンポーネントのON/OFFを切り替えるために必要．
        /// </summary>
        private EarlyUpdateBrainBase _earlyUpdate;

        /// <summary>
        /// キャラクターが移動できる軸の設定．
        /// </summary>
        [SerializeField, Indent] private bool3 _freezeAxis = new(false, false, false);

        /// <summary>
        /// Rigidbody と衝突したときに押すことが可能かどうか．
        /// </summary>
        [SerializeField, Indent] private bool _pushable = true;

        [DisableInPlayMode]
        [SerializeField, Indent] private bool _detectCollisions = true;

        private Vector3 _lockAxis = Vector3.one;
        private static readonly Collider[] Colliders = new Collider[5];
        private IGroundContact _groundCheck;
        private bool _hasGroundCheck;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// Vector3 形式の FreezeAxis の内容．
        /// </summary>
        public Vector3 LockAxis
        {
            get => _lockAxis;
            set => SetFreezeAxis(value.x < 0.5f, value.y < 0.5f, value.z < 0.5f);
        }

        /// <summary>
        /// キャラクターが移動できる軸の設定．
        /// </summary>
        public bool3 FreezeAxis => _freezeAxis;

        /// <summary>
        /// 更新タイミング．CharacterBrain は Update フェーズで更新される．
        /// </summary>
        public override UpdateTiming Timing => UpdateTiming.Update;


        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        private void Awake()
        {
            base.Initialize();
            GatherComponents();

            // Apply LockAxis.
            SetFreezeAxis(_freezeAxis.x, _freezeAxis.y, _freezeAxis.z);

            _controller.detectCollisions = _detectCollisions;
        }

        private void OnEnable()
        {
            // Activate the component for pre-calculation.
            _earlyUpdate.enabled = true;
        }

        private void OnDisable()
        {
            // Deactivate the component for pre-calculation.
            _earlyUpdate.enabled = false;
        }

        private void Update()
        {
            UpdateBrain();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If push is disabled, do not perform pushing when colliding.
            if (_pushable == false)
                return;

            // push other character brain.
            if (hit.collider.TryGetComponent(out CharacterBrain brain))
                brain.PushedOtherController(hit.moveDirection * TotalVelocity.magnitude, Settings.Mass);

            // On contact with an object, if the object is operating with a rigidbody, push it out.
            var body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic)
                return;
            var pushDir = hit.moveDirection;
            body.AddForce(pushDir * Settings.Mass, ForceMode.Force);
        }

        #endregion


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// フリーズ位置を更新する．
        /// </summary>
        /// <param name="x">X軸をロックするかどうか．</param>
        /// <param name="y">Y軸をロックするかどうか．</param>
        /// <param name="z">Z軸をロックするかどうか．</param>
        public void SetFreezeAxis(bool x, bool y, bool z)
        {
            _lockAxis.x = x ? 0 : 1;
            _lockAxis.y = y ? 0 : 1;
            _lockAxis.z = z ? 0 : 1;
            _freezeAxis.x = x;
            _freezeAxis.y = y;
            _freezeAxis.z = z;
        }

        /// <summary>
        /// CharacterSettings が更新されたときのコールバック．
        /// </summary>
        /// <param name="settings">更新された CharacterSettings．</param>
        void IActorSettingUpdateReceiver.OnUpdateSettings(CharacterSettings settings)
        {
            // Controller が設定されていない場合は取得する
            if (_controller == null) TryGetComponent(out _controller);

            // 高さ、中心点、幅を取得する
            _controller.height = settings.Height - _controller.skinWidth * 2;
            _controller.center = new Vector3(0, settings.Height * 0.5f + _controller.skinWidth, 0);
            _controller.radius = settings.Radius;
        }


        // ----------------------------------------------------------------------------
        // Protected Method

        /// <summary>
        /// キャラクターの位置を更新する．
        /// </summary>
        /// <param name="total">最終的な位置．</param>
        /// <param name="deltaTime">デルタタイム．</param>
        protected override void ApplyPosition(in Vector3 total, float deltaTime)
        {
            var totalVelocity = Vector3.Scale(_lockAxis, total);
            var velocity = totalVelocity * deltaTime;

            // GroundCheck が存在する場合、地面に合わせて位置を補正する
            if (_hasGroundCheck && _groundCheck.IsFirmlyOnGround && totalVelocity.y <= 0)
            {
                var distance = _groundCheck.DistanceFromGround;
                velocity -= new Vector3(0, distance, 0);
            }

            // CharacterController が有効な場合、CharacterController のコンテキスト内で移動する
            // それ以外の場合は、Transform でキャラクターを移動させる
            if (_controller.enabled)
            {
                _controller.Move(velocity);
            }
            else
            {
                CachedTransform.position += velocity;
            }

            CachedTransform.position = BrainUtils.LimitAxis(Position, CachedTransform.position, _freezeAxis);
        }

        /// <summary>
        /// キャラクターの回転を適用する．
        /// </summary>
        /// <param name="rotation">最終的な回転．</param>
        protected override void ApplyRotation(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        /// <summary>
        /// 初期位置と回転を取得する．
        /// </summary>
        /// <param name="position">取得する位置．</param>
        /// <param name="rotation">取得する回転．</param>
        protected override void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            CachedTransform.GetPositionAndRotation(out position, out rotation);
        }

        /// <summary>
        /// 位置を更新する．
        /// この処理では、位置が即座に更新され、他の Control や Effect の影響を受けない．
        /// </summary>
        /// <param name="position">新しい位置．</param>
        protected override void SetPositionDirectly(in Vector3 position)
        {
            // 座標を更新するために、CharacterController を一時的に停止する必要がある
            if (_controller.enabled)
            {
                _controller.enabled = false;
                CachedTransform.position = position;
                _controller.enabled = true;
            }
            else
            {
                // 位置を更新する
                CachedTransform.position = position;
            }
        }

        /// <summary>
        /// キャラクターの回転を更新する．
        /// </summary>
        /// <param name="rotation">新しい回転．</param>
        protected override void SetRotationDirectly(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        protected override void MovePosition(in Vector3 newPosition)
        {
            var position = BrainUtils.LimitAxis(Position, newPosition, _freezeAxis);

            if (_controller.enabled)
            {
                // Since CharacterController is enabled, it cannot be moved by Transform.
                _controller.Move(position - CachedTransform.position);
            }
            else
            {
                CachedTransform.position = position;
            }
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// <see cref="CharacterBrain"/> で使用するコンポーネントのリストを収集して追加する．
        /// </summary>
        private void GatherComponents()
        {
            TryGetComponent(out CachedTransform);
            TryGetComponent(out _controller);
            _hasGroundCheck = TryGetComponent(out _groundCheck);
            _earlyUpdate = gameObject.GetOrAddComponent<EarlyUpdateBrain>();

            // すぐに実行されないように、開始時にコンポーネントを無効化する
            _earlyUpdate.enabled = false;
        }

        /// <summary>
        /// 他のキャラクター Brain を押す．
        /// </summary>
        /// <param name="direction">押す方向．</param>
        /// <param name="mass">キャラクターの質量．</param>
        private void PushedOtherController(Vector3 direction, float mass)
        {
            // ゼロ除算を防ぐため、質量が無効な場合は早期リターン
            if (Settings.Mass <= 0 || mass <= 0)
                return;

            var max = Mathf.Max(Settings.Mass, mass);
            var rate = (mass / Settings.Mass) / max;

            var velocity = Vector3.Scale(direction, new Vector3(1, 0f, 1)) * rate * Time.deltaTime;
            velocity.y = 0.001f; // CharacterController のバグを回避

            _controller.Move(velocity);
        }


        /// ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void Reset()
        {
            // Update settings such as CharacterController.
            var settings = GetComponent<CharacterSettings>();
            ((IActorSettingUpdateReceiver)this).OnUpdateSettings(settings);
        }

        private void OnValidate()
        {
            // Update the settings to match the Inspector for changes during gameplay.
            SetFreezeAxis(_freezeAxis.x, _freezeAxis.y, _freezeAxis.z);
        }
#endif
    }
}