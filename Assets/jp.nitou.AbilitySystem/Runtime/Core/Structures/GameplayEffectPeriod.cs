using System;

namespace Nitou.AbilitySystem
{
    [Serializable]
    public class GameplayEffectPeriod
    {
        /// <summary>
        /// Period at which to tick this GE
        /// </summary>
        public float Period;

        /// <summary>
        /// Whether to execute GE on first application (true) or wait until the first tick (false)
        /// </summary>
        public bool ExecuteOnApplication;
    }
}