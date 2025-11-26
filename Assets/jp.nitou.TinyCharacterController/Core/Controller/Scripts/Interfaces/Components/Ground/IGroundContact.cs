using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components
{
    /// <summary>
    /// アクターが地面に接触しているかどうかを判断するコンポーネントにアクセスするためのインターフェース.
    /// </summary>
    public interface IGroundContact
    {
        /// <summary>
        /// 接地状態かの判定. 
        /// ※着地時の事前判定などに用いる大まかな判定
        /// </summary>
        bool IsOnGround { get; }

        /// <summary>
        /// 接地状態かの厳密な判定.
        /// </summary>
        bool IsFirmlyOnGround { get; }

        /// <summary>
        /// 地面からの相対的な距離.
        /// </summary>
        float DistanceFromGround { get; }

        /// <summary>
        /// 現在の地面の法線ベクトル.
        /// </summary>
        Vector3 GroundSurfaceNormal { get; }

        /// <summary>
        /// 地面と接している点.
        /// </summary>
        Vector3 GroundContactPoint { get; }
    }
}