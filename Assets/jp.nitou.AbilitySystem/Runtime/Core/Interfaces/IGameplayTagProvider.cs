using System.Collections.Generic;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイタグを提供するオブジェクトを表します。
    /// </summary>
    public interface IGameplayTagProvider
    {
        /// <summary>
        /// このオブジェクトが現在保持している全てのタグを列挙します。
        /// </summary>
        /// <returns>所持しているタグの列挙。</returns>
        IEnumerable<IGameplayTag> GetOwnedTags();
    }
}


