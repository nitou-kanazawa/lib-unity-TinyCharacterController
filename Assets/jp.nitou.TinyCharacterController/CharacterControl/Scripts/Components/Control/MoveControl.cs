using UnityEngine;
using Sirenix.OdinInspector;
// using nitou.Attributes;
using Nitou.TCC.Foundation;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Control
{
    using CharacterControl.Interfaces.Core;
    using CharacterControl.Interfaces.Components;
    using CharacterControl.Core;
    using CharacterControl.Shared;

    [AddComponentMenu(MenuList.MenuControl + nameof(MoveControl))]
    [DisallowMultipleComponent]
    // [RequireInterface(typeof(IGroundContact))]
    public sealed class MoveControl : ComponentBase,
                                      IMove,
                                      ITurn,
                                      IPriorityLifecycle<ITurn>,
                                      IUpdateComponent
    {
        /// <summary>
        /// Determines if MoveControl is used to move the character.
        /// If a higher value than other Priority is set, this component is used.
        /// </summary>
        [Title("Movement settings")]
        [GUIColor("green")]
        [SerializeField, Indent] private int _movePriority = 1;

        /// <summary>
        /// アクターの最大移動速度．
        /// </summary>
        [SerializeField, Indent] private float _moveSpeed = 4;

        /// <summary>
        /// ブレーキ力．
        /// </summary>
        [SerializeField, Indent] private float _brakePower = 12;

        /// <summary>
        /// 加速度．
        /// </summary>
        [SerializeField, Indent] private float _accelerator = 6;

        /// <summary>
        /// 移動可能な傾斜角度。
        /// 地形の角度がこの値以下の場合、その地形に沿って移動する．
        /// </summary>
        [SerializeField, Indent] private float _angle = 45;

        /// <summary>
        /// 移動可能方向を制限するための任意軸．値はグローバル座標系．
        /// Vector3.zero以外が指定されると、アクターはその軸方向のみに移動する．
        /// </summary>
        [SerializeField, Indent] private Vector3 _lockAxis = Vector3.zero;


        /// <summary>
        /// Threshold to determine if the character is in motion.
        /// If the value falls below this threshold, set <see cref="IsMove"/> to False.
        /// </summary>
        [Range(0, 1)] 
        [SerializeField, Indent] private float _moveStopThreshold = 0.2f;


        /// <summary>
        /// Determines if MoveControl is used for character orientation.
        /// If a higher value is set compared to other priorities, this component is used.
        /// </summary>
        [Title("Turning Settings")]
        [GUIColor("green")]
        [SerializeField, Indent] private int _turnPriority = 1;

        /// <summary>
        /// アクター回転速度．
        /// （Priorityが高いときのみ適用）
        /// </summary>
        [PropertyRange(-1, 50)]
        [SerializeField, Indent] private int _turnSpeed = 15;

        /// <summary>
        /// Threshold to determine if the character's orientation has reached.
        /// If it's 0, orientation changes immediately after stopping the movement.
        /// If it's 1, the orientation is updated until it reaches the target orientation.
        /// </summary>
        [Range(0, 1)] 
        [SerializeField, Indent] private float _turnStopThreshold = 0;

        // references
        private Transform _transform;
        private IBrain _brain;
        private IGroundContact _groundCheck;

        // state
        private bool _hasGroundCheck;
        private Vector3 _moveDirection = Vector3.forward;
        private float _currentSpeed;
        private Vector2 _inputValue;
        private bool _hasInput;
        private float _yawAngle;
        private bool _isTurning;


        // ----------------------------------------------------------------------------

        #region Property

        int IUpdateComponent.Order => Order.Control;

        /// <summary>
        /// 
        /// </summary>
        public MovementReference MovementReference => CharacterSettings.MovementReference;

        /// <summary>
        /// 現在の移動速度．
        /// <see cref="IPriority{IMove}.Priority">優先度</see>/>が0の場合、移動速度も0になる．
        /// </summary>
        public float CurrentSpeed
        {
            get => (_movePriority > 0 || _currentSpeed < _moveStopThreshold) ? _currentSpeed : 0;
            set => _currentSpeed = value;
        }

        /// <summary>
        /// ワールド座標での移動ベクトル．
        /// 値は Update 時に更新される．
        /// </summary>
        public Vector3 MoveVelocity { get; private set; }

        /// <summary>
        /// 現在の速度．
        /// </summary>
        public Vector3 Velocity
        {
            get => _moveDirection * _currentSpeed;
            set
            {
                _moveDirection = value.normalized;
                _currentSpeed = value.magnitude;
            }
        }

        /// <summary>
        /// キャラクター基準の移動ベクトル．
        /// </summary>
        public Vector3 LocalVelocity => Quaternion.Inverse(_transform.rotation) * MoveVelocity;

        /// <summary>
        /// ワールド座標でキャラクターが移動したい方向．
        /// この値に Speed を掛けたものが実際の移動量となる．
        /// </summary>
        public Vector3 Direction { get; private set; }

        /// <summary>
        /// キャラクター視点での移動方向．
        /// </summary>
        public Vector3 LocalDirection => Quaternion.Inverse(_transform.rotation) * Direction;

        /// <summary>
        /// 前フレームからの回転角度の変化量．
        /// この値は TurnSpeed を考慮しない．
        /// </summary>
        public float DeltaTurnAngle { get; private set; }

        /// <summary>
        /// キャラクターの最大移動速度．
        /// </summary>
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        /// <summary>
        /// 回転速度．
        /// </summary>
        public int TurnSpeed
        {
            get => _turnSpeed;
            set => _turnSpeed = value;
        }

        /// <summary>
        /// MoveControl がキャラクターの移動に使用されるかどうかを決定する優先度．
        /// 他の Priority より高い値が設定されている場合、このコンポーネントが使用される．
        /// </summary>
        public int MovePriority
        {
            get => _movePriority;
            set
            {
                IsMove = value != 0;
                _movePriority = value;
            }
        }

        /// <summary>
        /// <see cref="ITurn"/> としての優先度．
        /// 0以下の場合は使用されない．
        /// </summary>
        public int TurnPriority
        {
            get => _turnPriority;
            set => _turnPriority = value;
        }

        /// <summary>
        /// 移動方向の変化量（degree）．
        /// </summary>
        public float DeltaDirectionAngle => Vector3.SignedAngle(_transform.forward, _moveDirection, Vector3.up);

        /// <summary>
        /// キャラクターの移動軸が制限されている場合は true．
        /// </summary>
        public bool IsLockAxis => _lockAxis.sqrMagnitude > 0;

        /// <summary>
        /// キャラクターが移動中の場合は true．
        /// 移動は <see cref="_moveStopThreshold"/> に基づいて判定される．
        /// </summary>
        public bool IsMove { get; private set; }

        #endregion Property


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        float ITurn.YawAngle => _yawAngle;

        void IPriorityLifecycle<ITurn>.OnUpdateWithHighestPriority(float deltaTime)
        {
            if (_hasInput)
            {
                _yawAngle = Vector3.SignedAngle(Vector3.forward, _moveDirection, Vector3.up);
            }
        }

        void IPriorityLifecycle<ITurn>.OnAcquireHighestPriority()
        {
            if (IsMove == false)
                _yawAngle = _brain.YawAngle;
        }

        void IPriorityLifecycle<ITurn>.OnLoseHighestPriority() { }

        int IPriority<ITurn>.Priority => IsMove || _isTurning ? _turnPriority : 0;
        int IPriority<IMove>.Priority => _currentSpeed > _moveStopThreshold ? _movePriority : 0;


        // ----------------------------------------------------------------------------
        #region Lifecycle Events
        
        private void OnDestroy() {
        }

        void IUpdateComponent.OnUpdate(float dt)
        {
            using (new ProfilerScope(nameof(MoveControl)))
            {
                ProcessMove(dt);
                ProcessTurn(dt);
            }
        }
        #endregion


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// スティック入力に応じて移動する．
        /// </summary>
        /// <param name="leftStick">移動方向．</param>
        public void Move(Vector2 leftStick)
        {
            // 入力情報をキャッシュする
            _inputValue = leftStick;
            _hasInput = leftStick.sqrMagnitude > 0;
        }


        // ----------------------------------------------------------------------------
        // Private Method

        private void ProcessMove(float dt)
        {
            // 移動入力があった場合
            if (_hasInput)
            {
                // 移動方向
                var preDirection = _moveDirection;
                MovementReference.UpdateInputData(_inputValue);
                _moveDirection = MovementReference.ModifieredInputVector;

                if (IsLockAxis)
                {
                    var dot = Vector3.Dot(_moveDirection, _lockAxis);
                    _moveDirection = _lockAxis * Mathf.Round(Mathf.Clamp(dot * 100, -1, 1));
                }

                _currentSpeed = Mathf.Lerp(_currentSpeed, _moveSpeed, _accelerator * dt);
                DeltaTurnAngle = Vector3.SignedAngle(preDirection, _moveDirection, Vector3.up);
            }
            // 移動入力がない場合，
            else
            {
                DeltaTurnAngle = 0;
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _brakePower * dt);
                if (_currentSpeed < _moveStopThreshold)
                    _currentSpeed = 0;
            }
        }

        private void ProcessTurn(float dt)
        {
            // 地面情報に応じて移動方向を決定する
            var normal = _hasGroundCheck && _groundCheck.IsOnGround ? _groundCheck.GroundSurfaceNormal : Vector3.up;
            normal = Vector3.Angle(Vector3.up, normal) < _angle ? normal : Vector3.up;
            Direction = Vector3.ProjectOnPlane(_moveDirection, normal);

            //
            MoveVelocity = Direction * _currentSpeed;
            IsMove = _currentSpeed > _moveStopThreshold;
            _isTurning = Vector3.Angle(_transform.forward, _moveDirection) > (1 - _turnStopThreshold) * 360;
        }

        protected override void OnComponentInitialized()
        {
            CharacterSettings.TryGetComponent(out _transform);
            CharacterSettings.TryGetComponent(out _brain);
            _hasGroundCheck = CharacterSettings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
        } 

        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            var offset = new Vector3(0, 0.1f, 0);
            var position = transform.position + offset;

            if (IsLockAxis)
            {
                var size = Vector3.one * 0.2f;
                var p1 = position + _lockAxis * 5;
                var p2 = position - _lockAxis * 5;

                var color = Colors.Green;
                color.a = 0.2f;
                NGizmo.DrawCube(p1, size, color);
                NGizmo.DrawCube(p2, size, color);
                NGizmo.DrawLine(p1, p2, Colors.Green);
            }

            // show line about Move velocities.
            NGizmo.DrawRay(position, MoveVelocity, Colors.Green);
        }
#endif
    }
}