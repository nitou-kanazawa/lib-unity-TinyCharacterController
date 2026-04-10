using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// 重力の動作結果にアクセスするためのインターフェース．
    /// </summary>
    public interface IGravity
    {
        /// <summary>
        /// 落下速度 [unit/sec]
        /// </summary>
        float FallSpeed { get; }

        /// <summary>
        /// 重力加速度を上書きする．
        /// </summary>
        void SetVelocity(Vector3 velocity);

        /// <summary>
        /// キャラクターに作用する重力の倍率．
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