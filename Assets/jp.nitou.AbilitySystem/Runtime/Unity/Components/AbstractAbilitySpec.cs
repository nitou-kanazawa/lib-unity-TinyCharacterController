using System.Collections;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;

namespace Nitou.AbilitySystem.Unity.Components
{
    public struct AbilityCooldownTime
    {
        public float TimeRemaining;
        public float TotalDuration;
    }

    /// <summary>
    /// アビリティスペックの抽象基底クラスです。
    /// IAbilitySpec を実装し、Core レイヤの IAbilitySystem のみに依存します。
    /// Unity 固有の処理（コルーチン起動など）は、AbilitySystemCharacter 側で処理されます。
    /// </summary>
    public abstract class AbstractAbilitySpec : IAbilitySpec
    {
        /// <summary>
        /// The ability this AbilitySpec is linked to
        /// </summary>
        public AbstractAbilityScriptableObject Ability;

        /// <summary>
        /// The owner of the AbilitySpec - Core レイヤの IAbilitySystem
        /// </summary>
        private readonly IAbilitySystem _owner;

        /// <summary>
        /// Ability level
        /// </summary>
        public float Level { get; set; }

        /// <summary>
        /// Is this AbilitySpec currently active?
        /// </summary>
        public bool isActive;

        /// <inheritdoc />
        public IAbilityDefinition Definition => Ability;

        /// <inheritdoc />
        public IAbilitySystem Owner => _owner;

        /// <inheritdoc />
        public bool IsActive => isActive;

        /// <summary>
        /// Default constructor.  Initialises the AbilitySpec from the AbstractAbilityScriptableObject
        /// </summary>
        /// <param name="ability">Ability</param>
        /// <param name="owner">Owner - Core レイヤの IAbilitySystem</param>
        public AbstractAbilitySpec(AbstractAbilityScriptableObject ability, IAbilitySystem owner)
        {
            this.Ability = ability ?? throw new System.ArgumentNullException(nameof(ability));
            this._owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public bool CanActivate()
        {
            return CanActivateAbility();
        }

        /// <inheritdoc />
        public IEnumerator Activate()
        {
            return TryActivateAbility();
        }

        /// <inheritdoc />
        public void Cancel()
        {
            CancelAbility();
        }

        /// <summary>
        /// Try activating the ability.  Remember to use StartCoroutine() since this 
        /// is a couroutine, to allow abilities to span more than one frame.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator TryActivateAbility()
        {
            if (!CanActivateAbility()) yield break;

            isActive = true;
            yield return PreActivate();
            yield return ActivateAbility();
            EndAbility();
        }

        /// <summary>
        /// Checks if this ability can be activated
        /// </summary>
        /// <returns></returns>
        public virtual bool CanActivateAbility()
        {
            return !isActive
                   && CheckGameplayTags()
                   && CheckCost()
                   && CheckCooldown().TimeRemaining <= 0;
        }

        /// <summary>
        /// Cancels the ability, if it is active
        /// </summary>
        public abstract void CancelAbility();

        /// <summary>
        /// Checks if Gameplay Tag requirements allow activating this ability
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckGameplayTags();

        /// <summary>
        /// Check if this ability is on cooldown
        /// </summary>
        /// <returns></returns>
        public virtual AbilityCooldownTime CheckCooldown()
        {
            if (this.Ability.CooldownAsset == null) return new AbilityCooldownTime();
            var cooldownTags = this.Ability.CooldownAsset.Tags.GrantedTags;

            float longestCooldown = 0f;
            float maxDuration = 0f;

            // IAbilitySystem から GameplayEffectSystem を取得
            var effectSystem = _owner.GameplayEffectSystem;
            if (effectSystem != null)
            {
                var activeEffects = effectSystem.ActiveEffects;
                for (var i = 0; i < activeEffects.Count; i++)
                {
                    var effect = activeEffects[i];
                    var grantedTags = effect.Spec.Definition.Tags.GrantedTags;
                    for (var iTag = 0; iTag < grantedTags.Length; iTag++)
                    {
                        for (var iCooldownTag = 0; iCooldownTag < cooldownTags.Length; iCooldownTag++)
                        {
                            if (ReferenceEquals(grantedTags[iTag], cooldownTags[iCooldownTag]))
                            {
                                if (effect.Spec.Definition.DurationPolicy == DurationPolicy.Infinite)
                                    return new AbilityCooldownTime()
                                    {
                                        TimeRemaining = float.MaxValue,
                                        TotalDuration = 0
                                    };

                                var durationRemaining = effect.Spec.DurationRemaining;

                                if (durationRemaining > longestCooldown)
                                {
                                    longestCooldown = durationRemaining;
                                    maxDuration = effect.Spec.TotalDuration;
                                }
                            }
                        }
                    }
                }
            }

            return new AbilityCooldownTime()
            {
                TimeRemaining = longestCooldown,
                TotalDuration = maxDuration
            };
        }

        /// <summary>
        /// Method to activate before activating this ability.  This method is run after activation checks.
        /// </summary>
        protected abstract IEnumerator PreActivate();

        /// <summary>
        /// The logic that dictates what the ability does.  Targetting logic should be placed here.
        /// Gameplay Effects are applied in this method.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator ActivateAbility();

        /// <summary>
        /// Method to run once the ability ends
        /// </summary>
        public virtual void EndAbility()
        {
            this.isActive = false;
        }

        /// <summary>
        /// Checks whether the activating character has enough resources to activate this ability
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckCost()
        {
            if (this.Ability.CostAsset == null) return true;

            // IAbilitySystem から属性システムを取得
            var attributeSystem = _owner.AttributeSystem;
            if (attributeSystem == null) return true;

            // GameplayEffectSpec を生成（IAbilitySystem を直接使用）
            var geSpec = GameplayEffectSpec.CreateNew(
                gameplayEffect: this.Ability.CostAsset,
                source: _owner,
                level: this.Level);

            if (geSpec == null) return true;

            // If this isn't an instant cost, then assume it passes cooldown check
            if (geSpec.GameplayEffect.gameplayEffect.DurationPolicy != DurationPolicy.Instant) return true;

            for (var i = 0; i < geSpec.GameplayEffect.gameplayEffect.Modifiers.Length; i++)
            {
                var modifier = geSpec.GameplayEffect.gameplayEffect.Modifiers[i];

                // Only worry about additive.  Anything else passes.
                if (modifier.ModifierTypeValue != Core.AttributeModifierType.Add) continue;
                var costValue = (modifier.modifierMagnitude.CalculateMagnitude(geSpec) * modifier.Multiplier).GetValueOrDefault();

                if (!attributeSystem.TryGetValue(modifier.Attribute, out var attributeValue))
                {
                    continue;
                }

                // The total attribute after accounting for cost should be >= 0 for the cost check to succeed
                if (attributeValue.CurrentValue + costValue < 0) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an Ability System has all the listed tags
        /// </summary>
        /// <param name="abilitySystem">Ability System</param>
        /// <param name="tags">List of tags to check</param>
        /// <returns>True, if the Ability System has all tags</returns>
        protected virtual bool HasAllTags(IAbilitySystem abilitySystem, IGameplayTag[] tags)
        {
            if (abilitySystem == null) return true;
            if (tags == null || tags.Length == 0) return true;

            var ownedTags = new System.Collections.Generic.List<IGameplayTag>(abilitySystem.GetOwnedTags());
            return GameplayTagQuery.HasAllTags(ownedTags, tags);
        }

        /// <summary>
        /// Checks if an Ability System has none of the listed tags
        /// </summary>
        /// <param name="abilitySystem">Ability System</param>
        /// <param name="tags">List of tags to check</param>
        /// <returns>True, if the Ability System has none of the tags</returns>
        protected virtual bool HasNoneTags(IAbilitySystem abilitySystem, IGameplayTag[] tags)
        {
            if (abilitySystem == null) return true;
            if (tags == null || tags.Length == 0) return true;

            var ownedTags = new System.Collections.Generic.List<IGameplayTag>(abilitySystem.GetOwnedTags());
            return GameplayTagQuery.HasNoneTags(ownedTags, tags);
        }
    }
}
