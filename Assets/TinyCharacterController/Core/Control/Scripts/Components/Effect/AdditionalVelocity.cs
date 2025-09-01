using UnityEngine;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Shared;

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

        /// <summary>
        /// ���x�D
        /// </summary>
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

        /// <summary>
        /// Reset velocity.
        /// </summary>
        public void ResetVelocity()
        {
            Velocity = Vector3.zero;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        // TODO: Gizmosの修正
        /*
        private void OnDrawGizmosSelected()
        {
            var startPosition = transform.position;
            var endPosition = startPosition + Velocity;

            Gizmos_.DrawRay(startPosition, Velocity, Colors.Blue);
            Gizmos_.DrawSphere(endPosition, 0.1f, Colors.Blue);
        }
        */
#endif
    }
}