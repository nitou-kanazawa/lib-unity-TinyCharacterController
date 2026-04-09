using System;
using System.Collections.Generic;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Unity.Components
{
    public sealed class AttributeSystemComponent : MonoBehaviour
    {
        [SerializeField] private AbstractAttributeEventHandler[] AttributeSystemEvents;

        /// <summary>
        /// Attribute sets assigned to the game character
        /// </summary>
        [SerializeField] private List<AttributeAsset> Attributes = new();

        private AttributeSystemCore _core;
        private readonly List<Core.AttributeValue> prevAttributeValues = new();

        /// <summary>
        /// このコンポーネントが内部で使用している属性システムコアを取得します。
        /// </summary>
        public IAttributeSystem Core => _core;


        #region Lifecycle Events

        private void Awake()
        {
            _core = new AttributeSystemCore(Attributes);
        }

        private void LateUpdate()
        {
            UpdateAttributeCurrentValues();
        }

        #endregion


        /// <summary>
        /// Gets the value of an attribute.  Note that the returned value is a copy of the struct, so modifying it
        /// does not modify the original attribute
        /// </summary>
        /// <param name="attribute">Attribute to get value for</param>
        /// <param name="value">Returned attribute</param>
        /// <returns>True if attribute was found, false otherwise.</returns>
        public bool GetAttributeValue(AttributeAsset attribute, out Core.AttributeValue value)
        {
            return _core.TryGetValue(attribute, out value);
        }

        public void SetAttributeBaseValue(AttributeAsset attribute, float value)
        {
            _core.SetBaseValue(attribute, value);
        }

        /// <summary>
        /// Sets value of an attribute.  Note that the out value is a copy of the struct, so modifying it
        /// does not modify the original attribute
        /// </summary>
        /// <param name="attribute">Attribute to set</param>
        /// <param name="modifier">How to modify the attribute</param>
        /// <param name="value">Copy of newly modified attribute</param>
        /// <returns>True, if attribute was found.</returns>
        public bool UpdateAttributeModifiers(AttributeAsset attribute, Core.AttributeModifier modifier, out Core.AttributeValue value)
        {
            _core.ApplyModifier(attribute, modifier);
            return _core.TryGetValue(attribute, out value);
        }


        /// <summary>
        /// Add attributes to this attribute system.  Duplicates are ignored.
        /// </summary>
        /// <param name="attributes">Attributes to add</param>
        public void AddAttributes(params AttributeAsset[] attributes)
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                this.Attributes.Add(attributes[i]);
            }

            // 再構築して新しい属性を反映させる
            _core = new AttributeSystemCore(Attributes);
        }

        /// <summary>
        /// Remove attributes from this attribute system.
        /// </summary>
        /// <param name="attributes">Attributes to remove</param>
        public void RemoveAttributes(params AttributeAsset[] attributes)
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                this.Attributes.Remove(attributes[i]);
            }

            // 再構築して削除を反映させる
            _core = new AttributeSystemCore(Attributes);
        }

        public void ResetAll()
        {
            _core.ResetAll();
        }

        public void ResetAttributeModifiers()
        {
            _core.ResetModifiers();
        }

        
        public void UpdateAttributeCurrentValues()
        {
            prevAttributeValues.Clear();
            prevAttributeValues.AddRange(_core.Values);

            _core.UpdateCurrentValues();

            var currentValues = _core.Values;
            for (var i = 0; i < this.AttributeSystemEvents.Length; i++)
            {
                this.AttributeSystemEvents[i].PreAttributeChange(this, prevAttributeValues, ref currentValues);
            }
        }
    }
}