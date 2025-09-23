using UnityEngine;

// REF:
// - LIGHT11: https://light11.hatenadiary.com/entry/2018/05/31/002207

namespace Nitou.AnimationModule
{
    public static class AnimatorExtensions
    {

        public static TStateType GetCurrentState<TStateType>(this Animator animator, int layerIndex = 0)
            where TStateType : System.Enum
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.GetState<TStateType>();
        }

        public static TStateType GetState<TStateType>(this AnimatorStateInfo stateInfo)
            where TStateType : System.Enum
        {
            TStateType ret = default(TStateType);
            foreach (var state in System.Enum.GetValues(typeof(TStateType)))
            {
                if (stateInfo.IsName(state.ToString()))
                {
                    ret = (TStateType)state;
                    break;
                }
            }
            return ret;
        }
    }
}