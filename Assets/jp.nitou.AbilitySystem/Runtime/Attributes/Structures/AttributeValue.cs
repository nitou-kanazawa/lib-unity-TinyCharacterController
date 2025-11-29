using System;
using Nitou.AbilitySystem.Data;

namespace Nitou.AbilitySystem.Components
{
    [Serializable]
    public struct AttributeValue
    {
        public AttributeAsset attribute;
        public float baseValue;
        public float currentValue;
        public AttributeModifier modifier;
    }

    public struct AttributeModifier
    {
        public float add;
        public float multiply;
        public float overrideValue;

        public AttributeModifier Combine(AttributeModifier other)
        {
            other.add += this.add;
            other.multiply += this.multiply;
            other.overrideValue = this.overrideValue;
            return other;
        }

        public static AttributeModifier Zero() => new ()
        {
            add = 0f,
            multiply = 0f,
            overrideValue = 0,
        };
    }
}