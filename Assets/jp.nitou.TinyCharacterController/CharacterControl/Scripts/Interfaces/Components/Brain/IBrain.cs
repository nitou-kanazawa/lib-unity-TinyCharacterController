using Nitou.TCC.CharacterControl.Interfaces.Core;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// Brain の動作結果にアクセスするためのインターフェース．
    /// </summary>
    public interface IBrain
    {
        /// <summary>
        /// キャラクターの向きを基準とした速度．
        /// </summary>
        Vector3 LocalVelocity { get; }

        /// <summary>
        /// 現在アクティブな Control の速度．
        /// </summary>
        Vector3 ControlVelocity { get; }

        /// <summary>
        /// Effect の合計速度．
        /// </summary>
        Vector3 EffectVelocity { get; }

        /// <summary>
        /// 最終的な速度（ControlVelocity + EffectVelocity）．
        /// </summary>
        Vector3 TotalVelocity { get; }


        /// <summary>
        /// 現在選択されている Move コンポーネント．
        /// </summary>
        IMove CurrentMove { get; }

        /// <summary>
        /// 現在選択されている Turn コンポーネント．
        /// </summary>
        ITurn CurrentTurn { get; }


        /// <summary>
        /// 現在のキャラクターの移動速度．
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// キャラクターの向きを更新する速度．-1 の場合は即座に更新される．
        /// </summary>
        int TurnSpeed { get; }

        /// <summary>
        /// キャラクターの向き（Yaw角）．
        /// </summary>
        float YawAngle { get; }

        /// <summary>
        /// 現在フレームと前フレームのキャラクターの向きの差分．
        /// </summary>
        float DeltaTurnAngle { get; }
    }
}