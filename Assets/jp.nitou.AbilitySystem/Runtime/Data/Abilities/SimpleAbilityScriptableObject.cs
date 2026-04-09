using System.Collections;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// Simple Ability that applies a Gameplay Effect to the activating character
    /// </summary>
    [CreateAssetMenu(menuName = "Gameplay Ability System/Abilities/Simple Ability")]
    public class SimpleAbilityScriptableObject : AbstractAbilityScriptableObject
    {
        /// <summary>
        /// Gameplay Effect to apply
        /// </summary>
        public GameplayEffectAsset GameplayEffect;

        /// <inheritdoc />
        public override IAbilitySpec CreateSpec(IAbilitySystem owner)
        {
            if (owner == null) throw new System.ArgumentNullException(nameof(owner));
            var spec = new SimpleAbilitySpec(this, owner);
            spec.Level = owner.Level;
            return spec;
        }

        /// <summary>
        /// The Ability Spec is the instantiation of the ability.  Since the Ability Spec
        /// is instantiated for each character, we can store stateful data here.
        /// </summary>
        public class SimpleAbilitySpec : AbstractAbilitySpec
        {
            public SimpleAbilitySpec(AbstractAbilityScriptableObject abilitySO, IAbilitySystem owner) : base(abilitySO, owner)
            {
            }

            /// <summary>
            /// What to do when the ability is cancelled.  We don't care about there for this example.
            /// </summary>
            public override void CancelAbility() { }

            /// <summary>
            /// What happens when we activate the ability.
            /// 
            /// In this example, we apply the cost and cooldown, and then we apply the main
            /// gameplay effect
            /// </summary>
            /// <returns></returns>
            protected override IEnumerator ActivateAbility()
            {
                // Apply cost and cooldown
                if (this.Ability.CooldownAsset != null)
                {
                    var cdSpec = GameplayEffectSpec.CreateNew(this.Ability.CooldownAsset, Owner, Level);
                    Owner.GameplayEffectSystem.ApplyEffect(cdSpec);
                }

                if (this.Ability.CostAsset != null)
                {
                    var costSpec = GameplayEffectSpec.CreateNew(this.Ability.CostAsset, Owner, Level);
                    Owner.GameplayEffectSystem.ApplyEffect(costSpec);
                }

                // Apply primary effect
                if ((this.Ability as SimpleAbilityScriptableObject)?.GameplayEffect != null)
                {
                    var effectSpec = GameplayEffectSpec.CreateNew(
                        (this.Ability as SimpleAbilityScriptableObject).GameplayEffect,
                        Owner,
                        Level);
                    Owner.GameplayEffectSystem.ApplyEffect(effectSpec);
                }

                yield return null;
            }

            /// <summary>
            /// Checks to make sure Gameplay Tags checks are met. 
            /// 
            /// Since the target is also the character activating the ability,
            /// we can just use Owner for all of them.
            /// </summary>
            /// <returns></returns>
            public override bool CheckGameplayTags()
            {
                var tags = this.Ability.Tags;
                return HasAllTags(Owner, tags.OwnerTags.RequireTags)
                       && HasNoneTags(Owner, tags.OwnerTags.IgnoreTags)
                       && HasAllTags(Owner, tags.SourceTags.RequireTags)
                       && HasNoneTags(Owner, tags.SourceTags.IgnoreTags)
                       && HasAllTags(Owner, tags.TargetTags.RequireTags)
                       && HasNoneTags(Owner, tags.TargetTags.IgnoreTags);
            }

            /// <summary>
            /// Logic to execute before activating the ability.  We don't need to do anything here
            /// for this example.
            /// </summary>
            /// <returns></returns>
            protected override IEnumerator PreActivate()
            {
                yield return null;
            }
        }
    }
}
