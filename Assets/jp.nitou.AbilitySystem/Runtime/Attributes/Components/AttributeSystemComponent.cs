using System;
using System.Collections.Generic;
using Nitou.AbilitySystem.Data;
using UnityEngine;

namespace Nitou.AbilitySystem.Components
{
    public sealed class AttributeSystemComponent : MonoBehaviour
    {
        [SerializeField] private AbstractAttributeEventHandler[] AttributeSystemEvents;

        /// <summary>
        /// Attribute sets assigned to the game character
        /// </summary>
        [SerializeField] private List<AttributeAsset> Attributes = new();

        [SerializeField] private List<AttributeValue> AttributeValues = new();

        private bool mAttributeDictStale;
        private List<AttributeValue> prevAttributeValues = new();


        public Dictionary<AttributeAsset, int> mAttributeIndexCache { get; private set; } = new();


        #region Lifecycle Events

        private void Awake()
        {
            InitialiseAttributeValues();
            MarkAttributesDirty();
            GetAttributeCache();
        }

        private void LateUpdate()
        {
            UpdateAttributeCurrentValues();
        }

        #endregion


        /// <summary>
        /// Marks attribute cache dirty, so it can be recreated next time it is required
        /// </summary>
        public void MarkAttributesDirty()
        {
            this.mAttributeDictStale = true;
        }


        /// <summary>
        /// Gets the value of an attribute.  Note that the returned value is a copy of the struct, so modifying it
        /// does not modify the original attribute
        /// </summary>
        /// <param name="attribute">Attribute to get value for</param>
        /// <param name="value">Returned attribute</param>
        /// <returns>True if attribute was found, false otherwise.</returns>
        public bool GetAttributeValue(AttributeAsset attribute, out AttributeValue value)
        {
            // If dictionary is stale, rebuild it
            var attributeCache = GetAttributeCache();

            // We use a cache to store the index of the attribute in the list, so we don't
            // have to iterate through it every time
            if (attributeCache.TryGetValue(attribute, out var index))
            {
                value = AttributeValues[index];
                return true;
            }


            // No matching attribute found
            value = new AttributeValue();
            return false;
        }

        public void SetAttributeBaseValue(AttributeAsset attribute, float value)
        {
            // If dictionary is stale, rebuild it
            var attributeCache = GetAttributeCache();
            if (!attributeCache.TryGetValue(attribute, out var index)) return;
            var attributeValue = AttributeValues[index];
            attributeValue.baseValue = value;
            AttributeValues[index] = attributeValue;
        }

        /// <summary>
        /// Sets value of an attribute.  Note that the out value is a copy of the struct, so modifying it
        /// does not modify the original attribute
        /// </summary>
        /// <param name="attribute">Attribute to set</param>
        /// <param name="modifierType">How to modify the attribute</param>
        /// <param name="value">Copy of newly modified attribute</param>
        /// <returns>True, if attribute was found.</returns>
        public bool UpdateAttributeModifiers(AttributeAsset attribute, AttributeModifier modifier, out AttributeValue value)
        {
            // If dictionary is stale, rebuild it
            var attributeCache = GetAttributeCache();

            // We use a cache to store the index of the attribute in the list, so we don't
            // have to iterate through it every time
            if (attributeCache.TryGetValue(attribute, out var index))
            {
                // Get a copy of the attribute value struct
                value = AttributeValues[index];
                value.modifier = value.modifier.Combine(modifier);

                // Structs are copied by value, so the modified attribute needs to be reassigned to the array
                AttributeValues[index] = value;
                return true;
            }

            // No matching attribute found
            value = new AttributeValue();
            return false;
        }


        /// <summary>
        /// Add attributes to this attribute system.  Duplicates are ignored.
        /// </summary>
        /// <param name="attributes">Attributes to add</param>
        public void AddAttributes(params AttributeAsset[] attributes)
        {
            // If this attribute already exists, we don't need to add it.  For that, we need to make sure the cache is up to date.
            var attributeCache = GetAttributeCache();

            for (var i = 0; i < attributes.Length; i++)
            {
                if (attributeCache.ContainsKey(attributes[i]))
                {
                    continue;
                }

                this.Attributes.Add(attributes[i]);
                attributeCache.Add(attributes[i], this.Attributes.Count - 1);
            }
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

            // Update attribute cache
            GetAttributeCache();
        }

        public void ResetAll()
        {
            for (var i = 0; i < this.AttributeValues.Count; i++)
            {
                var defaultAttribute = new AttributeValue();
                defaultAttribute.attribute = this.AttributeValues[i].attribute;
                this.AttributeValues[i] = defaultAttribute;
            }
        }

        public void ResetAttributeModifiers()
        {
            for (var i = 0; i < this.AttributeValues.Count; i++)
            {
                var attributeValue = this.AttributeValues[i];
                attributeValue.modifier = default;
                this.AttributeValues[i] = attributeValue;
            }
        }

        
        private void InitialiseAttributeValues()
        {
            this.AttributeValues = new List<AttributeValue>();
            for (var i = 0; i < Attributes.Count; i++)
            {
                this.AttributeValues.Add(new AttributeValue()
                    {
                        attribute = this.Attributes[i],
                        modifier = AttributeModifier.Zero(),
                    }
                );
            }
        }

        private Dictionary<AttributeAsset, int> GetAttributeCache()
        {
            if (mAttributeDictStale)
            {
                mAttributeIndexCache.Clear();
                for (var i = 0; i < AttributeValues.Count; i++)
                {
                    mAttributeIndexCache.Add(AttributeValues[i].attribute, i);
                }

                this.mAttributeDictStale = false;
            }

            return mAttributeIndexCache;
        }

        public void UpdateAttributeCurrentValues()
        {
            prevAttributeValues.Clear();
            for (var i = 0; i < this.AttributeValues.Count; i++)
            {
                var _attribute = this.AttributeValues[i];
                prevAttributeValues.Add(_attribute);
                this.AttributeValues[i] = _attribute.attribute.CalculateCurrentAttributeValue(_attribute, this.AttributeValues);
            }

            for (var i = 0; i < this.AttributeSystemEvents.Length; i++)
            {
                this.AttributeSystemEvents[i].PreAttributeChange(this, prevAttributeValues, ref this.AttributeValues);
            }
        }
    }
}