using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// キャラクターの移動制御インターフェース．
    /// </summary>
    public interface IMove : IPriority<IMove>
    {
        /// <summary>
        /// 移動速度 [m]．
        /// </summary>
        Vector3 MoveVelocity { get; }
    }
}