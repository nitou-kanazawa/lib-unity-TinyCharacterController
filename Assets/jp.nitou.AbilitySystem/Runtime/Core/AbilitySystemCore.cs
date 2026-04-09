using System.Collections.Generic;
using System.Linq;
using Nitou.AbilitySystem.Core;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// IAbilitySystem の標準的な実装です。
    /// アビリティの付与と起動を管理し、ゲームプレイ効果システムと連携します。
    /// </summary>
    public sealed class AbilitySystemCore : IAbilitySystem
    {
        private readonly List<IAbilitySpec> _grantedAbilities = new();
        private readonly HashSet<IAbilitySpec> _grantedAbilitiesSet = new(); // O(1) 検索用

        /// <summary>
        /// このアビリティシステムに紐づくゲームプレイ効果システムです。
        /// </summary>
        private readonly IGameplayEffectSystem _effectSystem;

        /// <summary>
        /// このアビリティシステムに紐づく属性システムです。
        /// 必要に応じてアビリティ内で参照されます。
        /// </summary>
        private readonly IAttributeSystem _attributeSystem;

        /// <summary>
        /// このアビリティシステムに紐づくタグ一覧です。
        /// </summary>
        private readonly HashSet<IGameplayTag> _ownedTags = new(); // O(1) 検索用

        /// <summary>
        /// 新しい <see cref="AbilitySystemCore"/> を生成します。
        /// </summary>
        /// <param name="effectSystem">紐づけるゲームプレイ効果システム。</param>
        /// <param name="attributeSystem">紐づける属性システム。</param>
        public AbilitySystemCore(IGameplayEffectSystem effectSystem, IAttributeSystem attributeSystem)
        {
            _effectSystem = effectSystem;
            _attributeSystem = attributeSystem;
        }

        /// <inheritdoc />
        public IAttributeSystem AttributeSystem => _attributeSystem;

        /// <inheritdoc />
        public IGameplayEffectSystem GameplayEffectSystem => _effectSystem;

        /// <inheritdoc />
        public IReadOnlyList<IAbilitySpec> GrantedAbilities => _grantedAbilities;

        /// <inheritdoc />
        public float Level { get; set; } = 1f;

        /// <inheritdoc />
        public void GrantAbility(IAbilitySpec spec)
        {
            if (spec == null) return;

            // O(1) 検索で重複チェック
            if (_grantedAbilitiesSet.Add(spec))
            {
                _grantedAbilities.Add(spec);
            }
        }

        /// <inheritdoc />
        public void RevokeAbility(IAbilitySpec spec)
        {
            if (spec == null) return;

            if (_grantedAbilitiesSet.Remove(spec))
            {
                _grantedAbilities.Remove(spec);
            }
        }

        /// <inheritdoc />
        public void RevokeAbilitiesWithTag(IGameplayTag tag)
        {
            if (tag == null) return;

            // 逆順で削除（インデックスのずれを防ぐ）
            for (var i = _grantedAbilities.Count - 1; i >= 0; i--)
            {
                var abilityTags = _grantedAbilities[i].Definition.Tags;
                if (ReferenceEquals(abilityTags.AssetTag, tag))
                {
                    var spec = _grantedAbilities[i];
                    _grantedAbilitiesSet.Remove(spec);
                    _grantedAbilities.RemoveAt(i);
                }
            }
        }

        /// <inheritdoc />
        public bool TryActivateAbility(IAbilitySpec spec)
        {
            if (spec == null) return false;

            // O(1) 検索で付与済みかチェック
            if (!_grantedAbilitiesSet.Contains(spec))
            {
                return false;
            }

            if (!spec.CanActivate())
            {
                return false;
            }

            // 実際のコルーチン起動は Unity の側で行う必要があるため、
            // ここでは true/false の判定のみを行います。
            return true;
        }

        /// <inheritdoc />
        public IEnumerable<IGameplayTag> GetOwnedTags()
        {
            return _ownedTags;
        }

        /// <summary>
        /// このアビリティシステムにタグを追加します。
        /// </summary>
        /// <param name="tag">追加するタグ。</param>
        public void AddTag(IGameplayTag tag)
        {
            if (tag != null)
            {
                _ownedTags.Add(tag);
            }
        }

        /// <summary>
        /// このアビリティシステムからタグを削除します。
        /// </summary>
        /// <param name="tag">削除するタグ。</param>
        public void RemoveTag(IGameplayTag tag)
        {
            if (tag != null)
            {
                _ownedTags.Remove(tag);
            }
        }
    }
}
