using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using UnityEngine;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Effect
{
    /// <summary>
    /// A component that sets a custom acceleration for a character.
    /// The acceleration is set externally and is not changed by the component.
    /// The acceleration set here is reflected in the character by the Brain.
    /// </summary>
    [AddComponentMenu(MenuList.MenuEffect + nameof(AdditionalVelocity))]
    [DisallowMultipleComponent]
    public sealed class AdditionalVelocity : MonoBehaviour,
                                             IEffect
    {
        [SerializeField] private Vector3 _velocity;

        /// <inheritdoc/>
        public Vector3 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        /// <summary>
        /// Speed to move
        /// </summary>
        public float Speed => Velocity.magnitude;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <inheritdoc/>
        public void ResetVelocity()
        {
            Velocity = Vector3.zero;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            var startPosition = transform.position;
            var endPosition = startPosition + Velocity;

            NGizmo.DrawRay(startPosition, Velocity, Colors.Blue);
            NGizmo.DrawSphere(endPosition, 0.1f, Colors.Blue);
        }
#endif
    }
}