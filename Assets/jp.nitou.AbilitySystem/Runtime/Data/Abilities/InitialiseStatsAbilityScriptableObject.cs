using System.Collections;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    [CreateAssetMenu(menuName = "Gameplay Ability System/Abilities/Stat Initialisation")]
    public class InitialiseStatsAbilityScriptableObject : AbstractAbilityScriptableObject
    {
        public GameplayEffectAsset[] InitialisationGE;

        /// <inheritdoc />
        public override IAbilitySpec CreateSpec(IAbilitySystem owner)
        {
            if (owner == null) throw new System.ArgumentNullException(nameof(owner));
            var spec = new InitialiseStatsAbility(this, owner);
            spec.Level = owner.Level;
            return spec;
        }

        public class InitialiseStatsAbility : AbstractAbilitySpec
        {
            public InitialiseStatsAbility(AbstractAbilityScriptableObject abilitySO, IAbilitySystem owner) : base(abilitySO, owner)
            {
            }

            public override void CancelAbility()
            {
            }

            public override bool CheckGameplayTags()
            {
                var tags = this.Ability.Tags;
                return HasAllTags(Owner, tags.OwnerTags.RequireTags)
                        && HasNoneTags(Owner, tags.OwnerTags.IgnoreTags);
            }

            protected override IEnumerator ActivateAbility()
            {
                // Apply cost and cooldown (if any)
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

                // 属性の現在値を更新
                Owner.AttributeSystem.UpdateCurrentValues();

                InitialiseStatsAbilityScriptableObject abilitySO = this.Ability as InitialiseStatsAbilityScriptableObject;
                if (abilitySO?.InitialisationGE != null)
                {
                    for (var i = 0; i < abilitySO.InitialisationGE.Length; i++)
                    {
                        var effectSpec = GameplayEffectSpec.CreateNew(abilitySO.InitialisationGE[i], Owner, Level);
                        Owner.GameplayEffectSystem.ApplyEffect(effectSpec);
                        Owner.AttributeSystem.UpdateCurrentValues();
                    }
                }

                yield break;
            }

            protected override IEnumerator PreActivate()
            {
                yield return null;
            }
        }
    }
}
