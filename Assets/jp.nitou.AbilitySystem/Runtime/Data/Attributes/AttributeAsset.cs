using System;
using System.Collections.Generic;
using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// 属性の ScriptableObject 実装です。
    /// </summary>
    [CreateAssetMenu(
        menuName = "Ability System/Attribute",
        fileName = "AbilityAttribute"
    )]
    public class AttributeAsset : ScriptableObject, IAttribute
    {
        /// <summary>
        /// 属性の表示名。
        /// </summary>
        [SerializeField] private string _name;

        /// <inheritdoc />
        public string Name => _name;

        /// <summary>
        /// 属性の現在値を計算します。
        /// </summary>
        public AttributeValue CalculateCurrentAttributeValue(AttributeValue attributeValue, IReadOnlyList<AttributeValue> otherAttributeValues)
        {
            attributeValue.CurrentValue = (attributeValue.BaseValue + attributeValue.Modifier.Add) * (attributeValue.Modifier.Multiply + 1);

            if (attributeValue.Modifier.OverrideValue != 0)
            {
                attributeValue.CurrentValue = attributeValue.Modifier.OverrideValue;
            }

            return attributeValue;
        }
    }
}