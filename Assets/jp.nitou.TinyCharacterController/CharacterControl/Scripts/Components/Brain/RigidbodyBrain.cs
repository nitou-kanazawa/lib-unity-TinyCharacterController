using Nitou.BatchProcessor;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.TCC.Foundation;
using Unity.Mathematics;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Core
{
    /// <summary>
    /// <see cref="UnityEngine.Rigidbody"/> を使用して動作する Brain．
    /// <see cref="UnityEngine.CapsuleCollider"/> と <see cref="UnityEngine.Rigidbody"/> で動作する．
    /// キャラクターは <see cref="_stepHeight"/> の値で決定される高さまで上昇し、移動中は <see cref="_skinWidth"/> 値で定義される余白を持つ位置で停止する．
    /// <see cref="UnityEngine.CapsuleCollider"/> の高さと幅は <see cref="CharacterSettings.Height"/> と <see cref="CharacterSettings.Radius"/> によって決定される．
    /// 正しく機能するには、<see cref="IGravity"/> と <see cref="IGroundContact"/> が必要．
    /// </summary>
    [AddComponentMenu(MenuList.MenuBrain + nameof(RigidbodyBrain))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(Order.UpdateBrain)]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterSettings))]
    [RequireComponent(typeof(CapsuleCollider))]
    // [RequireInterface(typeof(IGravity))]
    // [RequireInterface(typeof(IGroundContact))]
    public class RigidbodyBrain : BrainBase, IActorSettingUpdateReceiver
    {
        /// <summary>
        /// キャラクターが移動できる軸の設定．
        /// </summary>
        [SerializeField] private bool3 _freezeAxis = new(false, false, false);

        /// <summary>
        /// キャラクターと壁の間に設定される幅．
        /// </summary>
        [Range(0, 1f)]
        [SerializeField] private float _skinWidth = 0.08f;

        /// <summary>
        /// キャラクターが乗り越えられる段差の高さ．
        /// </summary>
        [Range(0.01f, 1f)]
        [SerializeField] private float _stepHeight = 0.2f;

        // Components
        private IGroundContact _groundCheck;
        private Rigidbody _rigidbody;
        private EarlyUpdateBrainBase _earlyUpdate;
        private Vector3 _lockAxis = Vector3.one;

        // Field
        private static readonly RaycastHit[] RaycastHits = new RaycastHit[5];


        /// <summary>
        /// Vector3 形式の FreezeAxis の内容．
        /// </summary>
        public Vector3 LockAxis
        {
            get => _lockAxis;
            set => SetFreezeAxis(value.x > 0.5f, value.y > 0.5f, value.z > 0.5f);
        }

        /// <summary>
        /// キャラクターが移動できる軸の設定．
        /// </summary>
        public bool3 FreezeAxis => _freezeAxis;

        /// <summary>
        /// 開始フレームの座標に基づいてキャラクターの頭の位置を取得する．
        /// カプセルの計算に使用される．
        /// </summary>
        private Vector3 HeadPosition => Position + new Vector3(0, Settings.Height - Settings.Radius, 0);

        /// <summary>
        /// 開始フレームの座標に基づいてキャラクターの足元の位置を取得する．
        /// カプセルの計算に使用される．
        /// </summary>
        private Vector3 FootPosition => Position + new Vector3(0, Settings.Radius, 0);

        /// <summary>
        /// コンポーネントの更新タイミング．
        /// </summary>
        public override UpdateTiming Timing => UpdateTiming.FixedUpdate;

        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        private void Reset()
        {
            // Collect components used in calculations.
            // Do not use caching as this method might be called outside of runtime.
            TryGetComponent(out Rigidbody rig);
            TryGetComponent(out CharacterSettings settings);
            TryGetComponent(out CapsuleCollider col);

            // Update Rigidbody properties
            rig.constraints = RigidbodyConstraints.FreezeRotation;
            rig.useGravity = false;
            rig.linearDamping = 5;
            rig.mass = settings.Mass;

            // Update collider shape
            UpdateColliderSettings(col, settings);
        }

        private void OnValidate()
        {
            // Collect components used in calculations.
            // Do not use caching as this method might be called outside of runtime.
            TryGetComponent(out CharacterSettings settings);
            TryGetComponent(out CapsuleCollider col);
            TryGetComponent(out _rigidbody);

            // Update settings outside of components
            UpdateColliderSettings(col, settings);

            // Update the axis
            SetFreezeAxis(_freezeAxis.x, _freezeAxis.y, _freezeAxis.z);
        }

        private void Awake()
        {
            // Collect components
            GatherComponents();

            Initialize();

            // Add a component that aggregates pre-processing
            // The initial status is set to disabled to account for the possibility of the Brain being disabled.
            _earlyUpdate = gameObject.AddComponent<EarlyFixedUpdateBrain>();
            _earlyUpdate.enabled = false;

            // Update FreezeAxis
            SetFreezeAxis(_freezeAxis.x, _freezeAxis.y, _freezeAxis.z);
        }

        private void OnEnable()
        {
            // Activate TCC-related components
            _earlyUpdate.enabled = true;
        }

        private void OnDisable()
        {
            // Deactivate TCC-related components
            _earlyUpdate.enabled = false;
        }

        private void FixedUpdate()
        {
            UpdateBrain();
        }

        private void Update()
        {
            // Rigidbody から Transform に位置と回転を適用する．
            // この処理がないと、ワープのタイミングによってキャラクターが残像を残す．
            // 計算順序の問題により、Update の後、LateUpdate の前に実行する必要がある．
            CachedTransform.position = _rigidbody.position;
            CachedTransform.rotation = _rigidbody.rotation;
        }

        #endregion


        /// <summary>
        /// Update freeze position.
        /// This setting is reflected in <see cref="_rigidbody"/>.
        /// </summary>
        /// <param name="x">lock x axis</param>
        /// <param name="y">lock y axis</param>
        /// <param name="z">lock z axis</param>
        public void SetFreezeAxis(bool x, bool y, bool z)
        {
            // Update values used in the inspector
            _freezeAxis.x = x;
            _freezeAxis.y = y;
            _freezeAxis.z = z;

            // Update values used in calculations
            _lockAxis.x = x ? 0 : 1;
            _lockAxis.y = y ? 0 : 1;
            _lockAxis.z = z ? 0 : 1;

            // Update Rigidbody behavior based on RigidbodyBrain settings
            var initialConstraints = RigidbodyConstraints.FreezeRotation;
            if (_freezeAxis.x)
                initialConstraints |= RigidbodyConstraints.FreezePositionX;
            if (_freezeAxis.y)
                initialConstraints |= RigidbodyConstraints.FreezePositionY;
            if (_freezeAxis.z)
                initialConstraints |= RigidbodyConstraints.FreezePositionZ;
            _rigidbody.constraints = initialConstraints;
        }


        /// <summary>
        /// Get components.
        /// </summary>
        private void GatherComponents()
        {
            TryGetComponent(out _groundCheck);
            TryGetComponent(out _rigidbody);
        }

        /// <summary>
        /// Update collider settings based on <see cref="settings"/>.
        /// </summary>
        /// <param name="caps">Capsule collider size</param>
        /// <param name="settings">Character settings</param>
        private static void UpdateColliderSettings(CapsuleCollider caps, CharacterSettings settings)
        {
            caps.radius = settings.Radius;
            caps.height = settings.Height;
            caps.center = new Vector3(0, settings.Height / 2, 0);
        }


        // ----------------------------------------------------------------------------

        #region override Methods

        /// <summary>
        /// Set the final character position.
        /// </summary>
        /// <param name="total">Final movement vector</param>
        /// <param name="deltaTime">Delta time</param>
        protected override void ApplyPosition(in Vector3 total, float deltaTime)
        {
            // var totalVelocity = WallCorrection(total);
            // totalVelocity = Vector3.Scale(_lockAxis, totalVelocity) ;

            var totalVelocity = Vector3.Scale(_lockAxis, total);

            // Get the movement vector on the XZ plane
            var horizontal = totalVelocity;
            horizontal.y = 0;

            var lastPosition = Position;

            // Overcome obstacles based on FootStep
            lastPosition += CalculateFootStep(lastPosition, horizontal, deltaTime);

            // If grounded, adjust position to adhere to the ground
            lastPosition += FitGround(totalVelocity, lastPosition);

            // If there's a wall in the movement direction, adjust the position to avoid penetration
            lastPosition += CalculateSkinWidth(lastPosition, horizontal);

            // 位置を適用する．

            // _rigidbody.MovePosition( lastPosition );
            _rigidbody.position = lastPosition;
            _rigidbody.linearVelocity = totalVelocity;
        }

        /// <summary>
        /// Update the final character orientation.
        /// </summary>
        /// <param name="rotation">Character orientation</param>
        protected override void ApplyRotation(in Quaternion rotation)
        {
            _rigidbody.MoveRotation(rotation);
        }

        /// <summary>
        /// Cache the coordinates.
        /// </summary>
        /// <param name="position">Coordinate</param>
        /// <param name="rotation">Rotation</param>
        protected override void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            position = _rigidbody.position;
            rotation = _rigidbody.rotation;
        }

        /// <summary>
        /// Update the position of the RigidbodyBrain.
        /// Unlike Warp, it is immediately reflected and affected by Control and Effect.
        /// </summary>
        /// <param name="position">new position</param>
        /// <returns>can warp</returns>
        protected override void SetPositionDirectly(in Vector3 position)
        {
            // Update the Rigidbody's coordinates
            _rigidbody.MovePosition(position);
        }

        /// <summary>
        /// Update the orientation of the RigidbodyBrain.
        /// Unlike Warp, it is immediately reflected and affected by Control and Effect.
        /// </summary>
        /// <param name="rotation">New orientation</param>
        protected override void SetRotationDirectly(in Quaternion rotation)
        {
            // Update rigidbody coordinates
            _rigidbody.rotation = rotation;
        }

        protected override void MovePosition(in Vector3 newPosition)
        {
            var position = BrainUtils.LimitAxis(Position, newPosition, _freezeAxis);

            _rigidbody.Move(position, Rotation);
        }

        #endregion


        /// <summary>
        /// <see cref="_skinWidth"/> の値に基づいて、キャラクターの許容移動幅を計算する．
        /// </summary>
        /// <param name="position">計算の開始座標</param>
        /// <param name="direction">評価する方向</param>
        /// <returns>補正オフセット</returns>
        private Vector3 CalculateSkinWidth(in Vector3 position, in Vector3 direction)
        {
            if (direction == Vector3.zero)
                return Vector3.zero;

            var footPosition = position + new Vector3(0, Settings.Radius, 0);
            var headPosition = position + new Vector3(0, Settings.Height - Settings.Radius, 0);
            var offset = new Vector3(0, _stepHeight, 0);

            // 目的地を確認し、地形がある場合は地形として考慮する
            var count = Physics.CapsuleCastNonAlloc(
                footPosition + offset, headPosition, Settings.Radius, direction.normalized,
                RaycastHits, _skinWidth, Settings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // 接触がない場合は補正不要
            if (count == 0)
                return Vector3.zero;

            // 自身のコライダーを除外して、最も遠い位置でヒットしたものを取得する
            // コライダーがない場合は Vector3.zero を返す
            if (GetFarthestHit(RaycastHits, count, out var hit))
            {
                // 反射位置から SkinWidth の分だけキャラクターを離す
                // 反射には LockAxis の設定を使用する
                var inverseVelocity = hit.normal * (_skinWidth - hit.distance);

                // _lockAxis を確認し、移動不可の方向を補正する
                inverseVelocity = Vector3.Scale(_lockAxis, inverseVelocity);
                return inverseVelocity;
            }
            else
            {
                // 補正不要
                return Vector3.zero;
            }
        }

        /// <summary>
        /// 最も遠い Raycast ヒットを取得する．
        /// </summary>
        /// <param name="raycastHits">確認する RayCast のリスト</param>
        /// <param name="count">RayCast の数</param>
        /// <param name="result">結果</param>
        private bool GetFarthestHit(in RaycastHit[] raycastHits, int count, out RaycastHit result)
        {
            // 複数のヒットを考慮して、最小の評価距離を取得する
            var distance = 0f;
            result = default(RaycastHit);
            var hasHit = false;

            // 最大の座標を取得する
            for (var i = 0; i < count; i++)
            {
                var hit = raycastHits[i];
                if (hit.distance < distance || Settings.IsOwnCollider(raycastHits[i].collider))
                    continue;

                distance = hit.distance;
                result = raycastHits[i];
                hasHit = true;
            }

            return hasHit;
        }

        /// <summary>
        /// If grounded and falling, adjust the position to fit the ground.
        /// </summary>
        /// <param name="velocity">Vector</param>
        /// <param name="position">Current coordinate</param>
        /// <returns>Correction offset</returns>
        private Vector3 FitGround(in Vector3 velocity, in Vector3 position)
        {
            // If grounded and falling, adjust to the position on the ground
            if (_groundCheck.IsFirmlyOnGround && velocity.y < 0)
            {
                return new Vector3(0, -_groundCheck.DistanceFromGround, 0);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// <see cref="_stepHeight"/> の値に基づいて障害物を乗り越える．
        /// </summary>
        /// <param name="position">現在の座標</param>
        /// <param name="velocity">水平ベクトル</param>
        /// <param name="deltaTime">デルタ時間</param>
        /// <returns>補正オフセット</returns>
        private Vector3 CalculateFootStep(in Vector3 position, in Vector3 velocity, float deltaTime)
        {
            // キャラクターの半径位置から下方向にレイを投げて段差を検出する
            var distance = velocity.magnitude * deltaTime + Settings.Radius;
            var origin = position + velocity.normalized * distance;
            var ray = new Ray(origin + new Vector3(0, _stepHeight, 0), Vector3.down);
            if (Physics.Raycast(ray, out var hit, _stepHeight,
                    Settings.EnvironmentLayer, QueryTriggerInteraction.Ignore) == false)
            {
                return Vector3.zero;
            }

            // 現在の傾斜と段差の高さを比較し、
            // 段差が傾斜よりも急な場合は位置を調整する
            var offset = _stepHeight - hit.distance;
            var groundAngle = Vector3.Angle(Vector3.up, _groundCheck.GroundSurfaceNormal);
            var targetAngle = Mathf.Atan(offset / Settings.Radius) * Mathf.Rad2Deg;

            return targetAngle > groundAngle ? new Vector3(0, offset, 0) : Vector3.zero;
        }


        /// <summary>
        /// 目的地に障害物がある場合、移動方向を補正する．
        /// </summary>
        /// <param name="velocity">現在の方向</param>
        /// <returns>補正された方向</returns>
        private Vector3 WallCorrection(Vector3 velocity)
        {
            // 移動していない場合は処理を終了
            if (velocity == Vector3.zero)
                return velocity;

            var deltaTime = Time.fixedDeltaTime;
            var moveDirection = velocity.normalized;
            var speed = velocity.magnitude;

            // Raycast が評価する距離
            // キャラクターの幅 + 遊び + 移動距離
            var distance = (Settings.Radius + _skinWidth) + (speed * deltaTime);

            // 足元検出を回避するように位置を調整
            var stepHeight = new Vector3(0, _stepHeight, 0);
            var isHit = Physics.CapsuleCast(FootPosition + stepHeight, HeadPosition, Settings.Radius,
                moveDirection, out var hit, distance, Settings.EnvironmentLayer, QueryTriggerInteraction.Ignore);

            // 方向を補正
            var result = isHit ? Vector3.ProjectOnPlane(velocity, hit.normal) : velocity;

            return result;
        }

        /// <summary>
        /// Called when CharacterSettings values change.
        /// </summary>
        /// <param name="settings">Updated Settings</param>
        void IActorSettingUpdateReceiver.OnUpdateSettings(CharacterSettings settings)
        {
            var capsuleCollider = GetComponent<CapsuleCollider>();
            var rig = GetComponent<Rigidbody>();

            // Update values
            UpdateColliderSettings(capsuleCollider, settings);
            rig.mass = settings.Mass;
        }
    }
}