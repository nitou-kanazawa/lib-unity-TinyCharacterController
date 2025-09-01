using UnityEngine;
using Nitou.TCC.Controller.Interfaces.Core;

namespace Nitou.TCC.Controller.Interfaces.Components {

    /// <summary>
    /// Interface for accessing the behavior results of the Brain
    /// </summary>
    public interface IBrain {
        
        /// <summary>
        /// Velocity based on the character's direction
        /// </summary>
        Vector3 LocalVelocity { get; }

        /// <summary>
        /// Velocity of the currently active Control
        /// </summary>
        Vector3 ControlVelocity { get; }

        /// <summary>
        /// Total Velocity of Effects
        /// </summary>
        Vector3 EffectVelocity { get; }

        /// <summary>
        /// Final Velocity
        /// </summary>
        Vector3 TotalVelocity { get; }

        
        /// <summary>
        /// Move of the currently selected Control by the character
        /// </summary>
        IMove CurrentMove { get; }

        /// <summary>
        /// Turn of the currently selected Control by the character
        /// </summary>
        ITurn CurrentTurn { get; }


        /// <summary>
        /// Current character movement speed
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Speed at which the character's direction is updated. If -1, it is updated immediately.
        /// </summary>
        int TurnSpeed { get; }

        /// <summary>
        /// Character's orientation
        /// </summary>
        float YawAngle { get; }

        /// <summary>
        /// Difference between the character's orientation in the current frame and the previous frame
        /// </summary>
        float DeltaTurnAngle { get; }
    }
}
