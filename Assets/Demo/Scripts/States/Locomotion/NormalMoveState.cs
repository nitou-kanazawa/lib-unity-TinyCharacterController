using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;

namespace Project.Actor
{
    using StateBase = Nitou.TCC.AI.FMS.State<ActorCore, ActorFMS.SetupParam>;

    /// <summary>
    /// 戦闘時の基本移動ステート．
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NormalMoveState : ActorState
    {
        [Title("Movement")]
        [SerializeField, Indent] private bool _canRun = true;

        [SerializeField, Indent] private float _speedMultipiler = 1f;

        [Title("Animations")]
        [SerializeField, Indent] private TransitionAssetBase _brendTreeAsset;

        [SerializeField] private StringAsset _parameterName;

        // 内部処理用
        private Parameter<float> _parameter;
        private SmoothedFloatParameter _smoothedParameter;


        /// <inheritdoc/>
        protected override void OnInitialized(ActorFMS.SetupParam param)
        {
            base.OnInitialized(param);

            // Animation
            _parameter = Anim.Animancer.Parameters.GetOrCreate<float>(_parameterName);
            _smoothedParameter = new SmoothedFloatParameter(Anim.Animancer, _parameter, smoothTime: 0.1f);
        }

        private void OnDestroy()
        {
            _smoothedParameter?.Dispose();
        }


        // ----------------------------------------------------------------------------
        // Override Method (ステート処理)

        /// <inheritdoc/>
        public override void EnterBehaviour(float dt, StateBase fromState)
        {
            Debug.Log("Normal Move");

            // 移動コンポーネントの設定
            //Movement.LookingDirection.CurrentMode = LookingDirection.Mode.Movement;  // ※移動方向を向く
            //Movement.PlanarMovement.CanRun = _canRun;
            //Movement.PlanarMovement.SpeedMultipiler = _speedMultipiler;

            // アニメーション再生
            OnStartMotion();
            Anim.Animancer.Play(_brendTreeAsset);

            // debug
            Context.SetBodyColor(Color.antiqueWhite);
        }

        /// <inheritdoc/>
        public override void ExitBehaviour(float dt, StateBase toState)
        {
            // アニメーション終了
            OnEndMotion();
        }

        /// <inheritdoc/>
        public override void UpdateBehaviour(float dt)
        {
            // 平面移動
            MoveControl.Move(InputActions.movement.value);
        }

        /// <inheritdoc/>
        public override void PostUpdateBehaviour(float dt)
        {
            // アニメーター速度設定
            // var isJustEntered = StateMachine.StateElapsedTime < 0.5f; // ※遷移直後はdampingをOFF
            // if (isJustEntered)
            //     _smoothedParameter.CurrentValue = MoveControl.CurrentSpeed;
            // else
            //     _smoothedParameter.TargetValue = MoveControl.CurrentSpeed;
            //
            // _moveAnim.State.Parameter = _smoothPlanarVelocity.GetNext(MoveControl.CurrentSpeed, dampTime);
            _parameter.Value = MoveControl.CurrentSpeed;
        }


        // ----------------------------------------------------------------------------
        // Protected Method (Animation)

        protected override void OnStartMotion()
        {
            // アニメーション補間用
            // _smoothPlanarVelocity.Reset(MoveControl.CurrentSpeed, 0);
        }

        protected override void OnEndMotion() { }


        // ----------------------------------------------------------------------------
        // Override Method (遷移条件)

        /// <summary>
        /// 自ステートへの遷移条件
        /// </summary>
        public override bool CheckEnterTransition(StateBase fromState) => true;

        /// <summary>
        /// 他ステートへの遷移条件
        /// </summary>
        public override void CheckExitTransition()
        {
            // 接地状態の場合，
            if (Context.IsGrounded)
            {
                // ロックオン
                if (InputActions.lockon.Canceled && StateMachine.StateElapsedTime > 0.1f)
                {
                    StateMachine.EnqueueTransition<StrafeMoveState>();
                    return;
                }


                //    // 回避ステップ
                //    if (InputActions.dodge.Started) {
                //        StateMachine.EnqueueTransition<DashState>();
                //    }

                //    // 攻撃１
                //    if (InputActions.attack1.Started) {
                //        RequestAttackState(AttackType.Attack1, 1.2f);
                //        return;
                //    }
                //    // 攻撃2
                //    if (InputActions.attack2.Started) {
                //        RequestAttackState(AttackType.Attack2, 1f);
                //        return;
                //    }
            }
        }


        // ----------------------------------------------------------------------------
        // Public Method (その他)
        public override string GetName() => "Normal Move";

        public override Color GetColor() => Color.skyBlue;
    }
}