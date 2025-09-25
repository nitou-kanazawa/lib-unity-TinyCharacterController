using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;
using Nitou.TCC.Controller.Control;
using Nitou.TCC.Controller.Core;

// REF:
// - [Animancer - 03-02 Directional Mixers](https://kybernetik.com.au/animancer/docs/samples/mixers/directional/)

namespace Project.Actor
{
    using StateBase = Nitou.TCC.Implements.State<ActorCore, ActorFMS.SetupParam>;

    public sealed class StrafeMoveState : ActorState
    {
        [Title("Animations")]
        [SerializeField, Indent] private TransitionAssetBase _brendTreeAsset;

        // Animation Parameters
        [SerializeField, Indent] private StringAsset _horizontalSpeedName;
        [SerializeField, Indent] private StringAsset _verticalSpeedName;
        
        [Unit("Second")]
        [SerializeField, Indent] private float _parameterSmoothTime = 0.15f;
        [Unit("Meter")]
        [SerializeField, Indent] private float _StopProximity = 0.1f;
        
        // Control
        private CursorLookControl _cursorLookControl;
        
        // 内部処理用
        private Parameter<float> _verticalSpeedParameter;
        private Parameter<float> _horizontalSpeedParameter;
        private SmoothedVector2Parameter _smoothedParameter;

        
        /// <inheritdoc/>
        protected override void OnInitialized(ActorFMS.SetupParam param)
        {
            base.OnInitialized(param);
            
            // Control
            _cursorLookControl = Settings.GetActorComponent<CursorLookControl>(CharacterComponent.Control);

            // Animation
            Anim.Animancer.States.GetOrCreate(_brendTreeAsset);
            _smoothedParameter = new SmoothedVector2Parameter(
                Anim.Animancer,
                keyX: _horizontalSpeedName,
                keyY: _verticalSpeedName,
                smoothTime: _parameterSmoothTime);
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
            Debug.Log("Lock On");
            Context.IsLockOn = true;
            
            // Control
            _cursorLookControl.TurnPriority = 3;

            // アニメーション再生
            OnStartMotion();
            Anim.Animancer.Play(_brendTreeAsset);
            
            // Debug
            Context.SetBodyColor(Color.deepSkyBlue);
        }

        /// <inheritdoc/>
        public override void ExitBehaviour(float dt, StateBase toState)
        {
            Debug.Log("Lock Off");
            Context.IsLockOn = false;
            
            // Control
            _cursorLookControl.TurnPriority = -1;
            
            // アニメーション終了
            OnEndMotion();
        }

        /// <inheritdoc/>
        public override void UpdateBehaviour(float dt)
        {
            // 移動処理
            MoveControl.Move(InputActions.movement.value);
            
            // 視点操作
            _cursorLookControl.LookTargetPoint(screenPosition: Input.mousePosition);

            PostCharacterSimulation(dt);
        }

        /// <inheritdoc/>
        public override void PostUpdateBehaviour(float dt)
        {
            // アニメーター速度設定
            
            _smoothedParameter.TargetValue = new Vector2(MoveControl.LocalVelocity.x, MoveControl.LocalVelocity.z);
            
            // MoveControl.LocalVelocity;
            
            // var isJustEntered = StateMachine.StateElapsedTime < 0.5f; // ※遷移直後はdampingをOFF
            // if (isJustEntered)
            //     _smoothedParameter.CurrentValue = MoveControl.CurrentSpeed;
            // else
            //     _smoothedParameter.TargetValue = MoveControl.CurrentSpeed;
            //
            // _moveAnim.State.Parameter = _smoothPlanarVelocity.GetNext(MoveControl.CurrentSpeed, dampTime);
            // _parameter.Value = MoveControl.CurrentSpeed;
        }

        
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
                // ロックオフ
                if (InputActions.lockon.Canceled && StateMachine.StateElapsedTime > 0.1f)
                {
                    Debug.Log("Request Lock Off");
                    StateMachine.EnqueueTransition<NormalMoveState>();
                    return;
                }

            }
        }
        

        // ----------------------------------------------------------------------------

        #region Public Method (その他)

        public override string GetName() => "Strafe Move";

        public override Color GetColor() => Color.deepSkyBlue;

        #endregion
    }
}