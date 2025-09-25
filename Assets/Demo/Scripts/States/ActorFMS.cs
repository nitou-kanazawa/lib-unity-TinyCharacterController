using System;
using System.Collections.Generic;
using UnityEngine;
using Nitou;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Inputs;

namespace Project.Actor
{
    public class ActorFMS : Nitou.TCC.Implements.SimpleFMS<ActorCore, ActorFMS.SetupParam>
    {
        /// ----------------------------------------------------------------------------
        // Protected Method 
        protected override void OnInitialize(SetupParam param)
        {
            // �C�x���g�o�^
            //param.characterActor.OnPreSimulation += PreCharacterSimulation;
            //param.characterActor.OnPostSimulation += PostCharacterSimulation;
        }


        /// ----------------------------------------------------------------------------
        // Private Method
        public void PreCharacterSimulation(float dt)
        {
            var state = CurrentState as ActorState;
            state?.PreCharacterSimulation(dt);
        }

        public void PostCharacterSimulation(float dt)
        {
            var state = CurrentState as ActorState;
            state?.PostUpdateBehaviour(dt);
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// <see cref="ActorFMS"/> �̃Z�b�g�A�b�v�p�f�[�^
        /// </summary>
        public class SetupParam : Nitou.TCC.Implements.StateSetupParam
        {
            public readonly CharacterSettings actorSettings;
            public readonly ActorBrain actorBrain;
            public readonly ActorAnimation actorAnimation;

            /// <summary>
            /// �R���X�g���N�^�D
            /// </summary>
            public SetupParam(
                CharacterSettings actorSettings,
                ActorBrain actorBrain,
                ActorAnimation actorAnimation
            )
            {
                if (actorSettings == null) throw new ArgumentNullException(nameof(actorSettings));
                if (actorBrain == null) throw new ArgumentNullException(nameof(actorBrain));
                if (actorAnimation == null) throw new ArgumentNullException(nameof(actorAnimation));

                this.actorSettings = actorSettings;
                this.actorBrain = actorBrain;
                this.actorAnimation = actorAnimation;
            }
        }
    }
}