using System;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using UniRx;
using UnityEngine;
using Sirenix.OdinInspector;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Effect
{
    /// <summary>
    /// アクターに重力を適用するコンポーネント．
    ///
    /// Gravity で設定された速度で下方向の加速度を追加する．
    /// 加速度の倍率は各キャラクターごとに設定可能．
    /// 地面と接触している間は加速度がない．
    /// 着地と離陸のタイミングでイベントが実行される．
    /// ジャンプなど上下移動するコンポーネントがこの値を操作する場合がある．
    /// </summary>
    [AddComponentMenu(MenuList.MenuEffect + nameof(Gravity))]
    [DisallowMultipleComponent]
    public sealed class Gravity : MonoBehaviour,
                                  IGravity, IGravityEvent,
                                  IEffect,
                                  IEarlyUpdateComponent
    {
        public enum State
        {
            Air = 0,
            Ground = 1,
        }

        [Title("Parameters")]
        /// <summary>
        /// 重力の倍率．
        /// Mulply by the ti<see cref="Physics.gravity"/> value.
        /// </summary>
        [Tooltip("Gravity multiplier")]
        [PropertyRange(0, 10)]
        [SerializeField, Indent]
        float _gravityScale = 1f;

        // Events
        private readonly Subject<float> _onLandingSubject = new();
        private readonly Subject<Unit> _onLeaveSubject = new();

        // 
        private IGroundContact _groundCheck;
        private CharacterSettings _settings;
        private State _state;
        private Vector3 _impactPower;
        private Vector3 _velocity;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 処理順序．
        /// </summary>
        int IEarlyUpdateComponent.Order => Order.Gravity;

        /// <summary>
        /// 現在の落下速度．
        /// 落下中は負の値、上昇中は正の値．
        /// </summary>
        public float FallSpeed => _velocity.y;

        /// <summary>
        /// 現在の状態．
        /// </summary>
        public State CurrentState => _state;

        /// <summary>
        /// 現在フレームで地面を離れたかどうか．
        /// </summary>
        public bool IsLeaved { get; private set; }

        /// <summary>
        /// 現在フレームで着地したかどうか．
        /// </summary>
        public bool IsLanded { get; private set; }

        /// <summary>
        /// 重力スケール．
        /// 2倍速で落下する場合は2、低重力環境の場合は0.5．
        /// </summary>
        public float GravityScale
        {
            get => _gravityScale;
            set => _gravityScale = Mathf.Clamp(value, 0, 10);
        }

        Vector3 IEffect.Velocity => _velocity;


        /// <summary>
        /// 着地時に通知するObservable．
        /// </summary>
        public IObservable<float> OnLanding => _onLandingSubject;

        /// <summary>
        /// 離陸時に通知するObservable．
        /// </summary>
        public IObservable<Unit> OnLeave => _onLeaveSubject;


        // ----------------------------------------------------------------------------
        // LifeCycle Event
        private void Awake()
        {
            _settings = GetComponentInParent<CharacterSettings>();
            _settings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
        }

        private void Start()
        {
            _state = IsGrounded ? State.Ground : State.Air;
        }

        private void OnDisable()
        {
            _velocity = Vector3.zero;
        }

        private void OnDestroy()
        {
            _onLandingSubject.Dispose();
            _onLeaveSubject.Dispose();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            if (!enabled) return;

            IsLeaved = false;
            IsLanded = false;

            ApplyGravity(deltaTime);
            CalculateGroundState();

            // If in contact with the ground, set acceleration to 0
            if (IsGroundedStrictly)
                _velocity = Vector3.zero;
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 落下速度を設定する．
        /// 例えば、落下を停止する場合は Vector3.Zero を指定する．
        /// </summary>
        /// <param name="velocity">新しい速度．</param>
        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        ///  <inheritdoc/>
        public void ResetVelocity()
        {
            SetVelocity(Vector3.zero);
        }


        /// ----------------------------------------------------------------------------
        // Private Method

        private bool IsGrounded => _groundCheck.IsOnGround && FallSpeed <= 0;

        private bool IsGroundedStrictly => _groundCheck.IsFirmlyOnGround && FallSpeed < 0;

        private void ApplyGravity(float dt)
        {
            var fallSpeed = Physics.gravity * (_gravityScale * dt);
            var angle = Vector3.Angle(Vector3.up, _groundCheck.GroundSurfaceNormal);

            if (angle > 45 && _velocity.y < 0 && _groundCheck.DistanceFromGround < _settings.Radius * 0.5f)
            {
                _velocity += Vector3.ProjectOnPlane(fallSpeed, _groundCheck.GroundSurfaceNormal);
            }
            else
            {
                _velocity += fallSpeed;
            }
        }

        private void CalculateGroundState()
        {
            var newState = GetCurrentState(_state);

            if (_state != newState)
            {
                switch (newState)
                {
                    case State.Ground:
                        IsLanded = true;
                        _onLandingSubject.OnNext(FallSpeed);
                        break;
                    case State.Air:
                        IsLeaved = true;
                        _onLeaveSubject.OnNext(Unit.Default);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _state = newState;
        }

        /// <summary>
        /// 現在の状態を取得する．
        /// </summary>
        private State GetCurrentState(State preState)
        {
            return preState switch
            {
                State.Ground => IsGrounded ? State.Ground : State.Air,
                State.Air => IsGroundedStrictly ? State.Ground : State.Air,
                _ => throw new ArgumentOutOfRangeException(nameof(preState), preState, null)
            };
        }
    }
}