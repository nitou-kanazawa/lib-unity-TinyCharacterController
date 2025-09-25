using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Nitou.TCC;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Control;
using Nitou.TCC.Inputs;

namespace Project.Actor
{
    /// <summary>
    /// アクターのステート基底クラス
    /// </summary>
    public abstract class ActorState : Nitou.TCC.Implements.State<ActorCore, ActorFMS.SetupParam>
    {
        // Reference
        public CharacterSettings Settings { get; private set; }
        public ActorAnimation Anim { get; private set; }

        // Control
        protected MoveControl MoveControl { get; private set; }
        protected JumpControl JumpControl { get; private set; }

        // Input
        public ActorBrain Brain { get; private set; }

        /// <summary>
        /// 入力アクション．
        /// </summary>
        public ActorActions InputActions => Brain.CharacterActions;

        
        // ----------------------------------------------------------------------------
        // Public Method (PhysicsUpdate処理)

        /// <summary>
        /// キャラクターの物理シミュレーション前の処理
        /// </summary>
        public virtual void PreCharacterSimulation(float dt) { }

        /// <summary>
        /// キャラクターの物理シミュレーション後の処理
        /// </summary>
        public virtual void PostCharacterSimulation(float dt) { }


        /// ----------------------------------------------------------------------------
        // Protected Method (Animation)

        protected virtual void OnStartMotion() { }
        protected virtual void OnEndMotion() { }


        // ----------------------------------------------------------------------------
        // Protected Method

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void OnInitialized(ActorFMS.SetupParam param) {
            // Reference
            Settings = param.actorSettings;
            Brain = param.actorBrain;
            Anim = param.actorAnimation;

            // Control
            MoveControl = Settings.GetActorComponent<MoveControl>(CharacterComponent.Control);
            JumpControl = Settings.GetActorComponent<JumpControl>(CharacterComponent.Control);
        }


        // ----------------------------------------------------------------------------
        // Protected Method (State Transition)

        /*

        /// <summary>
        /// AttackStateへの遷移リクエスト
        /// </summary>
        protected void RequestAttackState(AttackType type, float speed = 1f) {
            var attackState = StateMachine.GetState<AttackState>();
            if (attackState == null) return;

            attackState.SetNextAnimation(type, speed);
            StateMachine.EnqueueTransition(attackState);
        }

        */
    }
}