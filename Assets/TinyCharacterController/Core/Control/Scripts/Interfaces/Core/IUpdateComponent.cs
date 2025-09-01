namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// コンポーネントを更新するためのインターフェース．
    /// このコンポーネントを実行するには、同じオブジェクト上に<see cref="BrainBase"/>を継承したオブジェクトが存在する必要がある．
    /// </summary>
    public interface IUpdateComponent
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