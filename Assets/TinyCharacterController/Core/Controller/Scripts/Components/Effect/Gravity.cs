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
    /// It adds downward acceleration at the speed set in Gravity.
    /// The acceleration multiplier can be multiplied for each character.
    /// There is no acceleration while in contact with the ground.
    /// Events are executed at the timing of landing and takeoff.
    /// Components that move up and down, such as jumping, may manipulate this value.
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
        /// Multiply by the <see cref="Physics.gravity"/> value.
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
        // Properity

        /// <summary>
        /// 処理順序．
        /// </summary>
        int IEarlyUpdateComponent.Order => Order.Gravity;

        /// <summary>
        /// 現在の落下速度.
        /// Negative value if falling, positive value if rising.
        /// </summary>
        public float FallSpeed => _velocity.y;

        /// <summary>
        /// 現在の状態.
        /// </summary>
        public State CurrentState => _state;

        /// <summary>
        /// 現在フレームで地面を離れたかどうか
        /// </summary>
        public bool IsLeaved { get; private set; }

        /// <summary>
        /// 現在フレームで着地したかどうか
        /// </summary>
        public bool IsLanded { get; private set; }

        /// <summary>
        /// 重力スケール．
        /// 2 for a 2x faster fall, 0.5 for a lower gravity environment.
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
            _settings.TryGetActorComponent(ActorComponent.Check, out _groundCheck);
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
        /// Set the fall speed.
        /// For example, to stop the fall, specify Vector3.Zero.
        /// </summary>
        /// <param name="velocity">new velocity</param>
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