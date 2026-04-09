using System;
using Nitou.AbilitySystem.Data;

namespace Nitou.AbilitySystem
{
    [Serializable]
    public struct GameplayTagRequireIgnoreContainer
    {
        /// <summary>
        /// All of these tags must be present
        /// </summary>
        public GameplayTagAsset[] RequireTags;

        /// <summary>
        /// None of these tags can be present
        /// </summary>
        public GameplayTagAsset[] IgnoreTags;
    }
}