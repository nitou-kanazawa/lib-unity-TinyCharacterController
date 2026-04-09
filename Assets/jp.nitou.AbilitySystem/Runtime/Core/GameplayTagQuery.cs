using System.Collections.Generic;
using System.Linq;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイタグに関する共通的な判定処理を提供します。
    /// </summary>
    public static class GameplayTagQuery
    {
        /// <summary>
        /// 所持タグ集合が、指定された全てのタグを含んでいるかを判定します。
        /// </summary>
        /// <param name="ownedTags">所持しているタグの列挙。</param>
        /// <param name="required">全て含まれているべきタグ。</param>
        /// <param name="includeHierarchy">タグの親子関係も考慮する場合は true。</param>
        /// <returns>条件を満たす場合は true。</returns>
        public static bool HasAllTags(IEnumerable<IGameplayTag> ownedTags, IGameplayTag[] required, bool includeHierarchy = true)
        {
            if (required == null || required.Length == 0)
            {
                return true;
            }

            // パフォーマンス最適化: IEnumerable を一度だけ列挙して HashSet に変換
            // 階層関係のチェックが必要な場合は List も保持
            HashSet<IGameplayTag> ownedSet = null;
            List<IGameplayTag> ownedList = null;

            if (includeHierarchy)
            {
                ownedList = ownedTags as List<IGameplayTag> ?? ownedTags.ToList();
                ownedSet = new HashSet<IGameplayTag>(ownedList);
            }
            else
            {
                ownedSet = ownedTags as HashSet<IGameplayTag> ?? new HashSet<IGameplayTag>(ownedTags);
            }

            for (var i = 0; i < required.Length; i++)
            {
                var requiredTag = required[i];
                var found = false;

                // 階層関係を考慮しない場合は HashSet で高速検索
                if (!includeHierarchy)
                {
                    found = ownedSet.Contains(requiredTag);
                }
                else
                {
                    // 階層関係を考慮する場合は、直接一致または子孫関係をチェック
                    foreach (var owned in ownedList)
                    {
                        if (ReferenceEquals(owned, requiredTag))
                        {
                            found = true;
                            break;
                        }

                        if (owned.IsDescendantOf(requiredTag))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 所持タグ集合が、指定されたいずれのタグも含んでいないかを判定します。
        /// </summary>
        /// <param name="ownedTags">所持しているタグの列挙。</param>
        /// <param name="forbidden">含まれていてはならないタグ。</param>
        /// <param name="includeHierarchy">タグの親子関係も考慮する場合は true。</param>
        /// <returns>条件を満たす場合は true。</returns>
        public static bool HasNoneTags(IEnumerable<IGameplayTag> ownedTags, IGameplayTag[] forbidden, bool includeHierarchy = true)
        {
            if (forbidden == null || forbidden.Length == 0)
            {
                return true;
            }

            // パフォーマンス最適化: IEnumerable を一度だけ列挙して HashSet に変換
            HashSet<IGameplayTag> ownedSet = null;
            List<IGameplayTag> ownedList = null;

            if (includeHierarchy)
            {
                ownedList = ownedTags as List<IGameplayTag> ?? ownedTags.ToList();
                ownedSet = new HashSet<IGameplayTag>(ownedList);
            }
            else
            {
                ownedSet = ownedTags as HashSet<IGameplayTag> ?? new HashSet<IGameplayTag>(ownedTags);
            }

            for (var i = 0; i < forbidden.Length; i++)
            {
                var forbiddenTag = forbidden[i];

                // 階層関係を考慮しない場合は HashSet で高速検索
                if (!includeHierarchy)
                {
                    if (ownedSet.Contains(forbiddenTag))
                    {
                        return false;
                    }
                }
                else
                {
                    // 階層関係を考慮する場合は、直接一致または子孫関係をチェック
                    foreach (var owned in ownedList)
                    {
                        if (ReferenceEquals(owned, forbiddenTag))
                        {
                            return false;
                        }

                        if (owned.IsDescendantOf(forbiddenTag))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 所持タグ集合が、指定された要件コンテナを満たしているかを判定します。
        /// </summary>
        /// <param name="ownedTags">所持しているタグの列挙。</param>
        /// <param name="requirements">必須タグおよび無視タグの要件コンテナ。</param>
        /// <param name="includeHierarchy">タグの親子関係も考慮する場合は true。</param>
        /// <returns>条件を満たす場合は true。</returns>
        public static bool MatchesRequirements(
            IEnumerable<IGameplayTag> ownedTags,
            GameplayTagRequireIgnoreContainer requirements,
            bool includeHierarchy = true)
        {
            return HasAllTags(ownedTags, requirements.RequireTags, includeHierarchy)
                   && HasNoneTags(ownedTags, requirements.IgnoreTags, includeHierarchy);
        }
    }
}
