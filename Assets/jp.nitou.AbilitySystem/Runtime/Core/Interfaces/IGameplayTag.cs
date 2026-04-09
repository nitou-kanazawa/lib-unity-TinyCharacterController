namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイタグを表す抽象インターフェースです。
    /// Core レイヤはこのインターフェースのみに依存し、Data レイヤの具象型には依存しません。
    /// </summary>
    public interface IGameplayTag
    {
        /// <summary>
        /// このタグの親タグを取得します。
        /// </summary>
        IGameplayTag? Parent { get; }

        /// <summary>
        /// このタグが指定されたタグの子孫であるかを判定します。
        /// </summary>
        /// <param name="other">判定対象のタグ。</param>
        /// <param name="searchLimit">探索の上限回数（無限ループ防止）。</param>
        /// <returns>子孫である場合は true。</returns>
        bool IsDescendantOf(IGameplayTag other, int searchLimit = 4);
    }
}

