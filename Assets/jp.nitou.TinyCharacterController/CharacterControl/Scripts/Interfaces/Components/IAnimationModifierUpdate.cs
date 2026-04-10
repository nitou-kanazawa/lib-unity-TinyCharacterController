namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// Animator の処理に割り込むコールバックを提供するインターフェース．
    /// コンポーネントの実行タイミングを確認し、任意のタイミングで割り込むことができる．
    /// </summary>
    public interface IAnimationModifierUpdate
    {
        /// <summary>
        /// Animator の Transform 計算後に割り込むコールバック．
        /// </summary>
        void OnUpdate();
    }
}