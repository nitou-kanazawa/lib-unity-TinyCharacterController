using System.Linq;
using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    [CreateAssetMenu(
        menuName = "Ability System/GameplayEffectAsset",
        fileName = "GameplayEffectAsset"
    )]
    public class GameplayEffectAsset : ScriptableObject, IGameplayEffectDefinition
    {
        [SerializeField] public GameplayEffectDefinitionContainer gameplayEffect;

        [SerializeField] public GameplayEffectTags gameplayEffectTags;

        [SerializeField] public GameplayEffectPeriod PeriodData;

        // キャッシュ用フィールド
        private Core.GameplayEffectModifier[] _cachedModifiers;
        private Core.ConditionalGameplayEffectContainer[] _cachedConditionalEffects;
        private Core.GameplayEffectTags _cachedTags;
        private Core.GameplayEffectPeriod _cachedPeriod;
        private bool _cacheInitialized;

        /// <inheritdoc />
        public DurationPolicy DurationPolicy => gameplayEffect.DurationPolicy;

        /// <inheritdoc />
        public float BaseDuration => 0f; // BaseDuration はモディファイアで計算されるため、ここでは 0 を返す

        /// <inheritdoc />
        public IModifierMagnitude? DurationMagnitude => gameplayEffect.DurationModifier;

        /// <inheritdoc />
        public Core.GameplayEffectModifier[] Modifiers
        {
            get
            {
                if (!_cacheInitialized)
                {
                    InitializeCache();
                }
                return _cachedModifiers;
            }
        }

        /// <inheritdoc />
        public Core.ConditionalGameplayEffectContainer[] ConditionalEffects
        {
            get
            {
                if (!_cacheInitialized)
                {
                    InitializeCache();
                }
                return _cachedConditionalEffects;
            }
        }

        /// <inheritdoc />
        public Core.GameplayEffectTags Tags
        {
            get
            {
                if (!_cacheInitialized)
                {
                    InitializeCache();
                }
                return _cachedTags;
            }
        }

        /// <inheritdoc />
        public Core.GameplayEffectPeriod Period
        {
            get
            {
                if (!_cacheInitialized)
                {
                    InitializeCache();
                }
                return _cachedPeriod;
            }
        }

        /// <summary>
        /// キャッシュを初期化します。
        /// </summary>
        private void InitializeCache()
        {
            // Modifiers のキャッシュ
            if (gameplayEffect.Modifiers == null)
            {
                _cachedModifiers = System.Array.Empty<Core.GameplayEffectModifier>();
            }
            else
            {
                _cachedModifiers = gameplayEffect.Modifiers.Select(m => new Core.GameplayEffectModifier
                {
                    Attribute = m.Attribute,
                    ModifierType = m.ModifierTypeValue,
                    ModifierMagnitude = m.modifierMagnitude,
                    Multiplier = m.Multiplier
                }).ToArray();
            }

            // ConditionalEffects のキャッシュ
            if (gameplayEffect.ConditionalGameplayEffects == null)
            {
                _cachedConditionalEffects = System.Array.Empty<Core.ConditionalGameplayEffectContainer>();
            }
            else
            {
                _cachedConditionalEffects = gameplayEffect.ConditionalGameplayEffects.Select(c => new Core.ConditionalGameplayEffectContainer
                {
                    GameplayEffect = c.GameplayEffect,
                    RequiredSourceTags = c.RequiredSourceTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>()
                }).ToArray();
            }

            // Tags のキャッシュ
            _cachedTags = new Core.GameplayEffectTags
            {
                AssetTag = gameplayEffectTags.AssetTag,
                GrantedTags = gameplayEffectTags.GrantedTags?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>(),
                OngoingTagRequirements = ConvertTagRequireIgnoreContainer(gameplayEffectTags.OngoingTagRequirements),
                ApplicationTagRequirements = ConvertTagRequireIgnoreContainer(gameplayEffectTags.ApplicationTagRequirements),
                RemovalTagRequirements = ConvertTagRequireIgnoreContainer(gameplayEffectTags.RemovalTagRequirements),
                RemoveGameplayEffectsWithTag = gameplayEffectTags.RemoveGameplayEffectsWithTag?.Cast<IGameplayTag>().ToArray() ?? System.Array.Empty<IGameplayTag>()
            };

            // Period のキャッシュ
            _cachedPeriod = new Core.GameplayEffectPeriod
            {
                Period = PeriodData?.Period ?? 0f,
                ExecuteOnApplication = PeriodData?.ExecuteOnApplication ?? false
            };

            _cacheInitialized = true;
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
            _cacheInitialized = false;
        }
    }
}
