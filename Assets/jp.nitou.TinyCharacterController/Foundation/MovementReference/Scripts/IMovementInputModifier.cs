using UnityEngine;

namespace Nitou.TCC.Foundation
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMovementInputModifier
    {
        /// <summary>
        /// 補正後の入力ベクトル．
        /// </summary>
        Vector3 ModifieredInputVector { get; }

        /// <summary>
        /// ユーザー入力値を更新する．
        /// </summary>
        void UpdateInputData(Vector2 movementInput);

        /// <summary>
        /// ユーザー入力値をリセットする．
        /// </summary>
        void ResetInputData();
    }
}