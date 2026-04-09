using UnityEngine;
using UnityEngine.Events;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Sirenix.OdinInspector;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// ジャンプの動作を制御するコンポーネント．
    /// Jump メソッドを実行すると Gravity を制御して上方向に移動する．
    /// Priority はジャンプ操作中のみ機能する．
    /// TurnPriority は移動方向が水平方向に設定されている場合のみ有効となる．
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(JumpControl))]
    [DisallowMultipleComponent]
    // [RequireInterface(typeof(IGravity))]
    // [RequireInterface(typeof(IGroundContact))]
    // [RequireInterface(typeof(IOverheadDetection))]
    public sealed class JumpControl : ComponentBase,
                                      IMove,
                                      ITurn,
                                      IUpdateComponent
    {
        /// <summary>
        /// ジャンプの高さ．
        /// </summary>
        [Title("Settings")]
        [Tooltip("JumpHeight")]
        [SerializeField, Indent] public float JumpHeight = 3;

        /// <summary>
        /// 空中ジャンプの最大回数．
        /// </summary>
        [Tooltip("Areal Jump Count.")]
        [SerializeField, Indent] public int MaxAerialJumpCount = 0;

        /// <summary>
        /// 空気抵抗（減速率）．
        /// </summary>
        [SerializeField, Indent] public float Drag = 0;

        /// <summary>
        /// キャラクターが方向を変える速度．
        /// -1 の場合、即座に向きを変える．
        /// </summary>
        [PropertyRange(-1, 50)]
        [SerializeField, Indent] private int _turnSpeed;

        /// <summary>
        /// <see cref="Jump"/> を先行入力できる時間．
        /// この時間内にジャンプ可能になった場合、自動的にジャンプする．
        /// </summary>
        [Title("Input Settings")]
        [PropertyRange(0, 1)]
        [SerializeField, Indent] private float _standbyTime = 0.05f;

        /// <summary>
        /// 移動プライオリティ．
        /// </summary>
        [Title("Priority Settings")]
        [GUIColor("green")]
        [SerializeField, Indent] private int _movePriority;

        /// <summary>
        /// 旋回プライオリティ．
        /// </summary>
        [GUIColor("green")]
        [SerializeField, Indent] private int _turnPriority;

        /// <summary>
        /// ジャンプがリクエストされたときのコールバック．
        /// <see cref="IsAllowJump"/> に関わらず、ジャンプが実行可能な場合に呼び出される．
        /// </summary>
        [Title("Callbacks")]
        public UnityEvent OnReadyToJump;

        /// <summary>
        /// ジャンプ直前に呼び出されるコールバック．
        /// </summary>
        public UnityEvent OnJump;


        // references
        private IGroundContact _groundCheck;
        private IGravity _gravity;
        private IOverheadDetection _head;

        //
        private float _requestJump = -1; // ジャンプがリクエストされた時刻．-1 はリクエスト無効状態．
        private bool _requestJumpIncrement = false; // ジャンプ回数を増加させるかどうか．
        private float _leaveTime = 0; // 地面から離れた経過時間．_standbyTime 内に離れた場合も接地と判定するために使用する．
        private float _readyTime = 0; // ジャンプ可能になってからの経過時間．
        private Vector3 _velocity;
        private float _yawAngle;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 処理オーダー．
        /// </summary>
        int IUpdateComponent.Order => Order.Control;

        /// <summary>
        /// 空中ジャンプ回数．
        /// </summary>
        public int AerialJumpCount { get; private set; } = 0;

        /// <summary>
        /// このフレームでジャンプが開始した場合 True．
        /// </summary>
        public bool IsJumpStart { get; private set; } = false;

        /// <summary>
        /// このフレームでジャンプ準備が開始した場合 True．
        /// </summary>
        public bool IsReadyToJumpStart { get; private set; }

        /// <summary>
        /// ジャンプ準備中の場合 True．
        /// </summary>
        public bool IsReadyToJump { get; private set; } = false;

        /// <summary>
        /// ジャンプ中の場合 True．
        /// </summary>
        public bool IsJumping { get; private set; } = false;

        /// <summary>
        /// ジャンプ準備から Allow が True になるまでの経過時間．
        /// 遅延時間でジャンプの強度を変えたいときに使用する．
        /// </summary>
        public float TimeSinceReady => IsReadyToJump ? _readyTime : 0;

        /// <summary>
        /// ジャンプを許可するかどうか．
        /// </summary>
        public bool IsAllowJump { get; set; } = true;

        /// <summary>
        /// ジャンプの方向．通常は上方向 (0,1,0) を使用する．
        /// 壁ジャンプやダッシュジャンプの場合はその方向のベクトルを設定する．
        /// キャラクターの向きは考慮されない．使用時には正規化される．
        /// </summary>
        public Vector3 JumpDirection { get; set; } = Vector3.up;

        public Vector3 Velocity => _velocity;


        private bool IsCanJump => _groundCheck.IsFirmlyOnGround || AerialJumpCount < MaxAerialJumpCount;

        private float HeightToJumpPower => Mathf.Sqrt(JumpHeight * -2.0f * _gravity.GravityScale * Physics.gravity.y);


        int IPriority<IMove>.Priority => IsJumping ? _movePriority : 0;
        int IPriority<ITurn>.Priority => IsJumping && _velocity.sqrMagnitude > 0 ? _turnPriority : 0;

        Vector3 IMove.MoveVelocity => _velocity;
        int ITurn.TurnSpeed => _turnSpeed;
        float ITurn.YawAngle => _yawAngle;


        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        protected override void OnComponentInitialized()
        {
            CharacterSettings.TryGetActorComponent(CharacterComponent.Effect, out _gravity);
            CharacterSettings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
            CharacterSettings.TryGetActorComponent(CharacterComponent.Check, out _head);
        }

        private void OnDestroy() {
        }

        void IUpdateComponent.OnUpdate(float deltaTime)
        {
            // Initialize
            IsJumpStart = false;
            IsReadyToJumpStart = false;

            // 地面から離れた経過時間を計算する．空中ジャンプの判定に使用する．
            _leaveTime = (_groundCheck.IsFirmlyOnGround && _gravity.FallSpeed <= 0) ? 0 : _leaveTime + deltaTime;
            if (IsReadyToJump)
                _readyTime += deltaTime;

            // ジャンプベクトルを減衰させる．
            _velocity = _leaveTime == 0 ? Vector3.zero : Vector3.Lerp(_velocity, Vector3.zero, Drag * Time.deltaTime);

            CalculateContactEnvironment();

            // 可能であればジャンプの準備をする．
            if (Time.time < _requestJump && IsCanJump && IsReadyToJump == false)
            {
                ReadyJump();
                _readyTime = 0;
            }

            // ジャンプが許可されている場合はジャンプする．
            if (IsReadyToJump && IsAllowJump)
            {
                ForceJump(_requestJumpIncrement);
            }
        }

        #endregion


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// ジャンプをリクエストし、ジャンプ可能なタイミングでジャンプする．
        /// ジャンプ不可のタイミングでリクエストが来た場合、_standbyTime の間ジャンプリクエストを保持する．
        /// （つまり、ジャンプ可能になった瞬間に処理される．）
        /// 即座にジャンプしない点に注意する．
        /// </summary>
        /// <param name="incrementJumpCount">ジャンプ回数をカウントするかどうか</param>
        public void Jump(bool incrementJumpCount = true)
        {
            _requestJumpIncrement = incrementJumpCount;
            _requestJump = Time.time + _standbyTime;
        }

        /// <summary>
        /// AllowJump と JumpCount を無視して強制的にジャンプする．
        /// この処理は即座に実行される．
        /// </summary>
        /// <param name="incrementJumpCount">ジャンプ回数を +1 するかどうか</param>
        public void ForceJump(bool incrementJumpCount = true)
        {
            // 空中の場合はジャンプ回数を +1 する．地面から離れていない場合は空中ジャンプ回数を数えない．
            if (incrementJumpCount && _leaveTime > 0)
                AerialJumpCount += 1;

            IsJumpStart = true;
            OnJump?.Invoke();

            // 初期化
            _requestJump = -1;
            IsReadyToJump = false;
            _readyTime = 0;

            // ジャンプ方向と力に基づいてジャンプ強度を計算する．
            // ベクトルは XZ 軸のみ；Y 軸は Gravity の値を使用する．
            var direction = JumpDirection.normalized;
            _velocity = new Vector3(direction.x, 0, direction.z) * HeightToJumpPower;
            _gravity.SetVelocity(new Vector3(0, direction.y, 0) * HeightToJumpPower);

            // 速度がゼロでない場合はヨー角を計算する．
            if (_velocity != Vector3.zero)
                _yawAngle = Vector3.SignedAngle(Vector3.forward, _velocity, Vector3.up);

            IsJumping = true;
        }

        /// <summary>
        /// ジャンプ回数・ベクトル・ジャンプ中の判定をリセットする．
        /// </summary>
        public void ResetJump()
        {
            AerialJumpCount = 0;
            IsJumping = false;
            _velocity = Vector3.zero;
            IsReadyToJump = false;
            IsReadyToJumpStart = false;
        }


        /// ----------------------------------------------------------------------------
        // Private Method
        private void ReadyJump()
        {
            IsReadyToJump = true;
            IsReadyToJumpStart = true;
            OnReadyToJump?.Invoke();
        }

        private void CalculateContactEnvironment()
        {
            // 頭部に何かが接触した場合、速度をゼロにする．
            if (_head.IsHeadContact && _gravity.FallSpeed > 0)
            {
                _gravity.SetVelocity(Vector3.zero);
            }

            // 着地時にジャンプ回数をリセットする．
            if (_groundCheck.IsFirmlyOnGround && _gravity.FallSpeed <= 0)
            {
                ResetJump();
            }
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            const float cursorRadius = 0.1f;
            if (CharacterSettings == null)
                GatherCharacterSettings();

            var position = transform.position;
            var width = CharacterSettings.Radius;

            if (_leaveTime > 0)
            {
                var characterCenter = position + new Vector3(0, CharacterSettings.Height * 0.5f, 0);
                var velocityOffset = _velocity + new Vector3(0, _gravity.FallSpeed, 0);
                var velocityPosition = characterCenter + velocityOffset * 0.3f;
                Gizmos.DrawLine(characterCenter, velocityPosition);
                NGizmo.DrawSphere(velocityPosition, cursorRadius, Colors.Blue);
            }
            else
            {
                var top = position + new Vector3(0, JumpHeight, 0);
                var size = new Vector3(CharacterSettings.Radius, 0, CharacterSettings.Radius);
                NGizmo.DrawCube(top, size, Colors.Blue);
                Gizmos.DrawLine(position, top);
            }
        }
#endif
    }
}