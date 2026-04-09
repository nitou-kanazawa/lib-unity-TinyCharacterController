using System.Collections.Generic;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    public abstract class AbstractAttributeEventHandler : ScriptableObject
    {
        public abstract void PreAttributeChange(AttributeSystemComponent attributeSystem, List<AttributeValue> prevAttributeValues, ref List<AttributeValue> currentAttributeValues);
    }
}