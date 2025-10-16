namespace Nitou.BatchProcessor
{
    /// <summary>
    /// バッチ処理の対象コンポーネントのインターフェース．
    /// </summary>
    public interface IComponentIndex
    {
        /// <summary>
        /// コンポーネントのインデックス．
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// コンポーネントが登録状態かどうか．
        /// </summary>
        bool IsRegistered { get; }
    }
}