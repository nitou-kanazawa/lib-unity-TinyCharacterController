using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// キャラクターの位置を更新するインターフェース．
    /// ワープによる位置更新時は、Control や SetVelocity による移動を行わないこと．
    /// </summary>
    public interface IWarp
    {
        /// <summary>
        /// 指定した位置・方向にワープする．
        /// </summary>
        void Warp(Vector3 position, Vector3 direction);

        /// <summary>
        /// 指定した位置にワープする．方向は更新しない．
        /// </summary>
        void Warp(Vector3 position);

        /// <summary>
        /// 指定した回転にワープする．位置は更新しない．
        /// </summary>
        void Warp(Quaternion rotation);

        /// <summary>
        /// 指定した位置へ移動する．座標を直接変更せず、障害物を考慮する．
        /// </summary>
        void Move(Vector3 position);
    }
}