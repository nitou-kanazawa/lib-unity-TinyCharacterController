using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// Updates the character's position.
    /// When updating the position through warping, do not perform movement using Control or SetVelocity.
    /// </summary>
    public interface IWarp
    {
        /// <summary>
        /// Warps the target.
        /// </summary>
        void Warp(Vector3 position, Vector3 direction);

        /// <summary>
        /// Updates the target's position.
        /// Does not update the direction.
        void Warp(Vector3 position);

        /// <summary>
        /// Updates the target's rotation.
        /// Does not update the position.
        void Warp(Quaternion rotation);

        /// <summary>
        /// Updates the target's position.
        /// Considers obstacles instead of directly moving the coordinates.
        /// </summary>
        void Move(Vector3 position);
    }
}