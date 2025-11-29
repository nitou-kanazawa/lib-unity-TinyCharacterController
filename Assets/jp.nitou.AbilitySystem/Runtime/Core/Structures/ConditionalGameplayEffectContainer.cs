using System;
using Nitou.AbilitySystem.Data;

namespace Nitou.AbilitySystem
{
    [Serializable]
    public struct ConditionalGameplayEffectContainer
    {
        public GameplayEffectAsset GameplayEffect;
        public GameplayTagAsset[] RequiredSourceTags;
    }
}