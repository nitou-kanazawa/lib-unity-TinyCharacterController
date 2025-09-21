using UnityEngine;

namespace Nitou.AnimationModule
{
    /// <summary>
    /// A component which takes the root motion from an <see cref="Animator"/> and applies it to a
    /// <see cref="CharacterController"/>.
    /// </summary>
    [AddComponentMenu("Nitou/Anim Module/Redirect Root Motion To Character Controller")]
    public sealed class RedirectRootMotionToCharacterController : RedirectRootMotion<CharacterController>
    {
        protected override void OnAnimatorMove()
        {
            if (!ApplyRootMotion) return;

            Target.Move(Animator.deltaPosition);
            Target.transform.rotation *= Animator.deltaRotation;
        }
    }
}