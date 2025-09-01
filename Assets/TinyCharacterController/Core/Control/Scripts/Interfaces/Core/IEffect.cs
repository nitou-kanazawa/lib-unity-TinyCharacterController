using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// アクターに加速度を与えるためのインターフェース．
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// 追加する加速度．
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// 加速度をリセットする．
        /// </summary>
        void ResetVelocity();
    }
}