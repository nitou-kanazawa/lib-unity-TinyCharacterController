
namespace Nitou.TCC.CharacterControl.Interfaces.Core{

    /// <summary>
    /// コンポーネントの優先度に関連したライフサイクルコールバックを定義するインターフェース．
    /// </summary>
    public interface IPriorityLifecycle<T>{

        /// <summary>
        /// 最高優先度を保持している間に、定期的に呼び出されるコールバック．
        /// </summary>
        void OnUpdateWithHighestPriority(float deltaTime);

        /// <summary>
        /// 最高優先度を獲得した際に呼び出されるコールバック．
        /// </summary>
        void OnAcquireHighestPriority();

        /// <summary>
        /// 最高優先度を失った際に呼び出されるコールバック．
        /// </summary>
        void OnLoseHighestPriority();
    }

}