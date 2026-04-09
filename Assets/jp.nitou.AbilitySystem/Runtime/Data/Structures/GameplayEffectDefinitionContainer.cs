using System;
using Nitou.AbilitySystem.Data;

namespace Nitou.AbilitySystem
{
    [Serializable]
    public struct GameplayEffectDefinitionContainer
    {
        /// <summary>
        /// The duration of this GE.  Instant GE are applied immediately and then removed, and Infinite and Has Duration are persistent and remain applied.
        /// </summary>
        public DurationPolicy DurationPolicy;

        public ModifierMagnitudeAsset DurationModifier;

        /// <summary>
        /// The duration of this GE, if the GE has a finite duration
        /// </summary>
        public float DurationMultiplier;

        /// <summary>
        /// The attribute modifications that this GE provides
        /// </summary>
        public GameplayEffectModifier[] Modifiers;

        /// <summary>
        /// Other GE to apply to the source ability system, based on presence of tags on source
        /// </summary>
        public ConditionalGameplayEffectContainer[] ConditionalGameplayEffects;
    }
}