using UnityEngine;

namespace Nitou.TCC.CharacterControl.Smb{

    /// <summary>
    /// 
    /// </summary>
    public class ResetTriggerOnStateMachineExit : StateMachineBehaviour{

        [SerializeField] string _triggerName;

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash){
            animator.ResetTrigger(_triggerName);
        }
    }
}