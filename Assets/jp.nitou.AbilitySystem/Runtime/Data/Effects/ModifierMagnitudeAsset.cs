using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// GameplayEffect の係数（マグニチュード）を計算するための抽象 ScriptableObject です。
    /// v2 では Core の <see cref="IGameplayEffectSpec"/> を入力として扱います。
    /// </summary>
    public abstract class ModifierMagnitudeAsset : ScriptableObject, IModifierMagnitude
    {
        /// <inheritdoc />
        public abstract void Initialize(IGameplayEffectSpec spec);

        /// <inheritdoc />
        public abstract float? CalculateMagnitude(IGameplayEffectSpec spec);
    }
}