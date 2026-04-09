using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;
using UnityEngine;

namespace Nitou.AbilitySystem.Data.ModifierMagnitude
{
    [CreateAssetMenu(menuName = "Gameplay Ability System/Gameplay Effect/Modifier Magnitude/Simple Float")]
    public class SimpleFloatModifierMagnitude : ModifierMagnitudeScriptableObject
    {
        [SerializeField]
        private AnimationCurve ScalingFunction;

        public override void Initialise(IGameplayEffectSpec spec)
        {
        }

        public override float? CalculateMagnitude(IGameplayEffectSpec spec)
        {
            return ScalingFunction.Evaluate(spec.Level);
        }
    }
}