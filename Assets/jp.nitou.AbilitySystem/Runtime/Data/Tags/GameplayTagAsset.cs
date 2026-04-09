using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// ゲームプレイタグの ScriptableObject 実装です。
    /// </summary>
    [CreateAssetMenu(
        menuName = "Ability System/Tag",
        fileName = "GameplayTag"
    )]
    public class GameplayTagAsset : ScriptableObject, IGameplayTag
    {
        [SerializeField] private GameplayTagAsset _parent;

        /// <inheritdoc />
        public IGameplayTag? Parent => _parent;

        /// <summary>
        /// Data レイヤ用の Parent プロパティ（型安全なアクセス用）。
        /// </summary>
        public GameplayTagAsset ParentAsset => _parent;

        /// <inheritdoc />
        public bool IsDescendantOf(IGameplayTag other, int searchLimit = 4)
        {
            int i = 0;
            IGameplayTag? tag = _parent;
            while (searchLimit > i++)
            {
                if (tag == null)
                    return false;

                if (ReferenceEquals(tag, other))
                    return true;

                tag = tag.Parent;
            }

            return false;
        }
    }
}