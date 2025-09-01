using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components
{
    /// <summary>
    /// Interface for accessing the results of gravity's behavior
    /// </summary>
    public interface IGravity
    {
        /// <summary>
        /// 落下速度 [unit/sec]
        /// </summary>
        float FallSpeed { get; }

        /// <summary>
        /// Override gravity acceleration
        /// </summary>
        void SetVelocity(Vector3 velocity);

        /// <summary>
        /// Multiplier for the gravity acting on the character
        /// </summary>
        float GravityScale { get; }

        /// <summary>
        /// 現在のフレームでキャラクターが地面から離れた場合はTrue．
        /// </summary>
        bool IsLeaved { get; }

        /// <summary>
        /// 現在のフレームでキャラクターが着地した場合はTrue．
        /// </summary>
        bool IsLanded { get; }
    }
}