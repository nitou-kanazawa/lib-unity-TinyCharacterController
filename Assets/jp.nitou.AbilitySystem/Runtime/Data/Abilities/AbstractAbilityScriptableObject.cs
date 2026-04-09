using System.Linq;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// アビリティの ScriptableObject 実装です。
    /// </summary>
    public abstract class AbstractAbilityScriptableObject : ScriptableObject, IAbilityDefinition
    {
        /// <summary>
        /// アビリティの表示名
        /// </summary>
        [SerializeField] private string AbilityName;

        /// <summary>
        /// アビリティのタグ情報（Data レイヤ版）
        /// </summary>
        [SerializeField] public AbilityTags AbilityTags;

        /// <summary>
        /// このアビリティを起動する際に支払うコストのゲームプレイ効果
        /// </summary>
        [SerializeField] public GameplayEffectAsset CostAsset;

        /// <summary>
        /// このアビリティに紐づくクールダウンのゲームプレイ効果
        /// </summary>
        [SerializeField] public GameplayEffectAsset CooldownAsset;

        // キャッシュ用フィールド
        private Core.AbilityTags _cachedTags;
        private bool _tagsCacheInitialized;

        /// <inheritdoc />
        public string DisplayName => AbilityName;

        /// <inheritdoc />
        public Core.AbilityTags Tags
        {
            get
            {
                if (!_tagsCacheInitialized)
                {
                    InitializeTagsCache();
                }
                return _cachedTags;
            }
        }

        /// <inheritdoc />
        public IGameplayEffectDefinition? Cost => CostAsset;

        /// <inheritdoc />
        public IGameplayEffectDefinition? Cooldown => CooldownAsset;

        /// <inheritdoc />
        public abstract IAbilitySpec CreateSpec(IAbilitySystem owner);

        /// <summary>
        /// Tags のキャッシュを初期化します。
        /// </summary>
        private void InitializeTagsCache()
        {
            _cachedTags = new Core.AbilityTags
            {
                AssetTag = AbilityTags.AssetTag,
                CancelAbilitiesWithTags = AbilityTags.CancelAbilitiesWithTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>(),
                BlockAbilitiesWithTags = AbilityTags.BlockAbilitiesWithTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>(),
                ActivationOwnedTags = AbilityTags.ActivationOwnedTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>(),
                OwnerTags = ConvertTagRequireIgnoreContainer(AbilityTags.OwnerTags),
                SourceTags = ConvertTagRequireIgnoreContainer(AbilityTags.SourceTags),
                TargetTags = ConvertTagRequireIgnoreContainer(AbilityTags.TargetTags)
            };
            _tagsCacheInitialized = true;
        }

        private Core.GameplayTagRequireIgnoreContainer ConvertTagRequireIgnoreContainer(GameplayTagRequireIgnoreContainer container)
        {
            // 構造体なので null チェックの代わりに、配列が null かどうかをチェック
            if (container.RequireTags == null && container.IgnoreTags == null)
            {
                return new Core.GameplayTagRequireIgnoreContainer
                {
                    RequireTags = System.Array.Empty<IGameplayTag>(),
                    IgnoreTags = System.Array.Empty<IGameplayTag>()
                };
            }

            return new Core.GameplayTagRequireIgnoreContainer
            {
                RequireTags = container.RequireTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>(),
                IgnoreTags = container.IgnoreTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>()
            };
        }

        /// <summary>
        /// Unity エディタで値が変更されたときにキャッシュを無効化します。
        /// </summary>
        private void OnValidate()
        {
            _tagsCacheInitialized = false;
        }
    }
}
