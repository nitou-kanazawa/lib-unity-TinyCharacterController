using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    public abstract class ModifierMagnitudeAsset : ScriptableObject
    {
        /// <summary>
        /// Function called when the spec is first initialised (e.g. by the Instigator/Source Ability System)
        /// </summary>
        /// <param name="spec">Gameplay Effect Spec</param>
        public abstract void Initialize();

        /// <summary>
        /// Function called when the magnitude is calculated, usually after the target has been assigned
        /// </summary>
        /// <param name="spec">Gameplay Effect Spec</param>
        /// <returns></returns>
        public abstract float? CalculateMagnitude();
    }
}