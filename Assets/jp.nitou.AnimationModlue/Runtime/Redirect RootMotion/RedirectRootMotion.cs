using UnityEngine;
using Sirenix.OdinInspector;

// [参考]
//  _: Redirect Root Motion https://kybernetik.com.au/animancer/docs/manual/other/redirect-root-motion/

namespace Nitou.AnimationModule
{
    /// <summary>
    /// <see cref="Animator"/>のルートモーションを別オブジェクトへ適用するためのコンポーネント．
    /// </summary>
    /// <remarks>
    /// This can be useful if the character's <see cref="Rigidbody"/> or <see cref="CharacterController"/> is on a
    /// parent of the <see cref="UnityEngine.Animator"/> to keep the model separate from the logical components.
    /// </remarks>
    [RequireComponent(typeof(Animator))]
    public abstract class RedirectRootMotion<T> : MonoBehaviour
        where T : Component
    {
        [Title("Target")]
        [Tooltip("The Animator which provides the root motion")]
        [SerializeField, Indent]
        private Animator _animator;

        [Tooltip("The object which the root motion will be applied to")]
        [SerializeField, Indent]
        private T _target;


        /// <summary>
        /// ルートモーションが適用される<see cref="Animator"/>．
        /// </summary>
        public Animator Animator => _animator;

        /// <summary>
        /// ルートモーションが適用されるオブジェクト．
        /// </summary>
        public T Target => _target;

        /// <summary>
        /// ルートモーションが有効かどうか．
        /// </summary>
        public bool ApplyRootMotion {
            get
            {
                if (Target == null || Animator == null)
                    return false;
                
                return Animator.applyRootMotion;
            }
    }


        // ----------------------------------------------------------------------------
        // MonoBehaviour Method
        
        protected virtual void OnValidate()
        {
            if (_animator == null)
                gameObject.TryGetComponent(out _animator);

            if (_target == null)
            {
                // デフォルトでは親オブジェクトから取得する
                _target = transform.parent.GetComponentInParent<T>();
            }
        }

        protected abstract void OnAnimatorMove();
    }
}