using UnityEngine;

namespace Nitou.TCC.Controller.Interfaces.Components{

    /// <summary>
    /// 現在位置からターゲット位置までの線形補間移動を制御するためのインターフェース．
    /// </summary>
    internal interface ILinearInterpolationMove{

        /// <summary>
        /// 線形補間移動の進行度を設定する．
        /// </summary>
        void SetNormalizedTime(float moveAmount, float turnAmount);

        /// <summary>
        /// 線形補間移動のトランジションを開始する．
        /// </summary>
        void Play(PropertyName id);

        /// <summary>
        /// 線形補間移動のトランジションを終了する．
        /// </summary>
        void Stop(PropertyName id);

        /// <summary>
        /// Playを実行せずにターゲット位置へ移動する．
        /// </summary>
        void FitTargetWithoutPlay(PropertyName id);
    }
}