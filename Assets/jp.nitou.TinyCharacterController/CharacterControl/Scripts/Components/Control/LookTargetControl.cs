using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// Components that face the direction of <see cref="Target"/>.
    /// 
    /// If priority is higher than the others, the character will face the Target.
    /// If Target is not set, the <see cref="Priority"/> of this component is 0.
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(LookTargetControl))]
    [DisallowMultipleComponent]
    public sealed class LookTargetControl : MonoBehaviour,
                                     ITurn
    {
        /// <summary>
        /// Transform of the target to point at.
        /// If this value is null, priority is disabled.
        /// </summary>
        // [AllowsNull]
        public Transform Target;

        /// <summary>
        /// Priority of turn.
        /// </summary>
        [Header("Turn Priority and Speed")]
        public int Priority = 10;

        /// <summary>
        /// Speed of orientation change.
        /// If this value is -1, it immediately faces the target.
        /// </summary>
        [SerializeField, Range(-1, 30)]
        public int TurnSpeed = 15;

        int IPriority<ITurn>.Priority => Target != null ? Priority : 0;

        int ITurn.TurnSpeed => TurnSpeed;

        float ITurn.YawAngle
        {
            get
            {
                if (Target == null)
                    return 0;

                var delta = Target.position - transform.position;

                return (delta.sqrMagnitude > 0) ? Vector3.SignedAngle(Vector3.forward, delta, Vector3.up) : transform.rotation.eulerAngles.y;
            }
        }
    }
}