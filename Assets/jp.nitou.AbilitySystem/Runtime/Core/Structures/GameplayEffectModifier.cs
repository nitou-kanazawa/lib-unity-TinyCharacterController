using System;
using Nitou.AbilitySystem.Components;
using Nitou.AbilitySystem.Data;
using UnityEngine.Serialization;

namespace Nitou.AbilitySystem
{
    
    [Serializable]
    public struct GameplayEffectModifier
    {
        public AttributeAsset Attribute;
        public AttributeModifierType ModifierType;
        [FormerlySerializedAs("ModifireMagnitude")]
        public ModifierMagnitudeAsset modifierMagnitude;
        public float Multipiler;
    }
}