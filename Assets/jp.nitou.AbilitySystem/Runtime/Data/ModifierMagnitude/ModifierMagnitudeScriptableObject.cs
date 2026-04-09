using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data.ModifierMagnitude
{
    /// <summary>
    /// GameplayEffect の係数（マグニチュード）を計算するための抽象 ScriptableObject です。
    /// v2 では Core の <see cref="IGameplayEffectSpec"/> を入力として扱います。
    /// </summary>
    public abstract class ModifierMagnitudeScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Function called when the spec is first initialised (e.g. by the Instigator/Source Ability System)
        /// </summary>
        /// <param name="spec">Gameplay Effect Spec (IGameplayEffectSpec)</param>
        public abstract void Initialise(IGameplayEffectSpec spec);

        /// <summary>
        /// Function called when the magnitude is calculated, usually after the target has been assigned
        /// </summary>
        /// <param name="spec">Gameplay Effect Spec (IGameplayEffectSpec)</param>
        /// <returns>Calculated magnitude.</returns>
        public abstract float? CalculateMagnitude(IGameplayEffectSpec spec);
    }
}