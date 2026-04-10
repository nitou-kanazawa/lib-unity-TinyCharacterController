using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// 視線上のオブジェクト情報にアクセスするためのインターフェース．
    /// </summary>
    public interface IAimCheck
    {
        /// <summary>
        /// 視線上にオブジェクトがある場合は True．
        /// </summary>
        bool IsHit { get; }

        /// <summary>
        /// RayCast の発射起点．
        /// </summary>
        Vector3 Origin { get; }

        /// <summary>
        /// Origin から視線上の点への方向．
        /// </summary>
        Vector3 Direction { get; }

        /// <summary>
        /// 視線上の点の位置．
        /// </summary>
        Vector3 Point { get; }

        /// <summary>
        /// 視線上の点までの距離．
        /// </summary>
        float Distance { get; }

        /// <summary>
        /// 視線上の点における法線ベクトル．
        /// </summary>
        Vector3 Normal { get; }
    }
}