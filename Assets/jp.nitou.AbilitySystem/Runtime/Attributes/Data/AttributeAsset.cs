using System;
using System.Collections.Generic;
using UnityEngine;
using Nitou.AbilitySystem.Components;

namespace Nitou.AbilitySystem.Data
{
    public sealed class PreAttributeChangedEventData
    {
        public AttributeSystemComponent AttributeSystem { get; }
        public float Value { get; }

        public PreAttributeChangedEventData(AttributeSystemComponent attributeSystem, float value)
        {
            AttributeSystem = attributeSystem;
            Value = value;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(
        menuName = "Ability System/Attribute",
        fileName = "AbilityAttribute"
    )]
    public class AttributeAsset : ScriptableObject
    {
        /// <summary>
        /// Firenly name of this attribute. Used for display purpose only.
        /// </summary>
        public string Name;

        public event Action<PreAttributeChangedEventData> OnPreAttributeChanged;


        public void PublishPreAttributeChangedEvent(PreAttributeChangedEventData eventData)
        {
            OnPreAttributeChanged?.Invoke(eventData);
        }

        public virtual AttributeValue CalculateInitialValue(AttributeValue attributeValue, List<AttributeValue> otherAttributeValues)
        {
            return attributeValue;
        }

        public virtual AttributeValue CalculateCurrentAttributeValue(AttributeValue attributeValue, List<AttributeValue> otherAttributeValues)
        {
            attributeValue.currentValue = (attributeValue.baseValue + attributeValue.modifier.add) * (attributeValue.modifier.multiply + 1);

            if (attributeValue.modifier.overrideValue != 0)
            {
                attributeValue.currentValue = attributeValue.modifier.overrideValue;
            }

            return attributeValue;
        }
    }
}