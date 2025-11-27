using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Core
{
    /// <summary>
    /// Transform以外の座標や向きを一貫してアクセスするためのインターフェース．
    /// Update前に値をキャッシュし、Get操作ではキャッシュされた値を使用する．
    /// Set操作では変更が即座にターゲットコンポーネントに反映される．
    /// </summary>
    public interface ITransform
    {
        /// <summary>
        /// 位置．
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// 方向．
        /// </summary>
        Quaternion Rotation { get; set; }
    }
}