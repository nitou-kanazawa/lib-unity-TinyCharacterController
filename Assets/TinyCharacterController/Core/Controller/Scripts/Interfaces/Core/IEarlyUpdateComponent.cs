namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// EarlyUpdateBrainのタイミングでコンポーネントを実行するためのインターフェース．
    /// BrainがFixedUpdateで動作する場合はFixedUpdateの前に、Updateで動作する場合はUpdateの前に実行される．
    /// </summary>
    public interface IEarlyUpdateComponent
    {
        /// <summary>
        /// 処理順序．
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 更新処理．
        /// </summary>
        void OnUpdate(float deltaTime);
    }
}