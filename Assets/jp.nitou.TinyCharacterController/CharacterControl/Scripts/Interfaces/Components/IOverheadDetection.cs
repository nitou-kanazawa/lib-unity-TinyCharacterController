using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components{
    
    /// <summary>
    /// 頭上のオブジェクトとの接触時の動作を返すインターフェース．
    /// 接触の有無・接触位置・接触オブジェクトを返す．
    /// </summary>
    public interface IOverheadDetection{

        /// <summary>
        /// 頭上の接触があるかどうか．
        /// </summary>
        bool IsHeadContact { get;  }

        /// <summary>
        /// コライダーが判定範囲内にある場合は True を返す．
        /// IsHitCollision とは異なり、頭上にオブジェクトがあるかどうかの判定に使用する．
        /// </summary>
        bool IsObjectOverhead { get; }

        /// <summary>
        /// 頭が接触しているコライダーを返す．何も接触していない場合は null を返す．
        /// </summary>
        GameObject ContactedObject { get;  }

        /// <summary>
        /// 接触位置．接触していない場合は接触可能な最大距離の位置を返す．
        /// </summary>
        Vector3 ContactPoint { get; }
    }
}
