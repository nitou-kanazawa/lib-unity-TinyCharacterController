using UnityEngine;

namespace Nitou.AnimationModule
{
    /// <summary>
    /// <see cref="Animator"/>のルートモーションを別オブジェクトの<see cref="Transform"/>へ適用するためのコンポーネント．
    /// </summary>
    [AddComponentMenu("Nitou/Anim Module/Redirect Root Motion To Transform")]
    public sealed class RedirectRootMotionToTransform : RedirectRootMotion<Transform>
    {
        protected override void OnAnimatorMove()
        {
            if (!ApplyRootMotion) return;

            Target.position += Animator.deltaPosition;
            Target.rotation *= Animator.deltaRotation;
        }
    }
}