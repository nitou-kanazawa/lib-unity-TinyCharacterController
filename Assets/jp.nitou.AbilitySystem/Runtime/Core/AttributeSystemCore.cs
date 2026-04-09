using System.Collections.Generic;
using Nitou.AbilitySystem.Core;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 属性値とその修飾を管理するコア実装です。
    /// Unity 非依存であり、Unity コンポーネントなどから利用されることを想定しています。
    /// </summary>
    public sealed class AttributeSystemCore : IAttributeSystem
    {
        /// <summary>
        /// 管理対象の属性値一覧です。
        /// </summary>
        private readonly List<AttributeValue> _attributeValues;

        /// <summary>
        /// 属性からインデックスへのキャッシュです。
        /// </summary>
        private readonly Dictionary<IAttribute, int> _attributeIndexCache = new();

        /// <summary>
        /// キャッシュが無効化されているかどうかを表します。
        /// </summary>
        private bool _attributeDictStale;

        /// <summary>
        /// 新しい <see cref="AttributeSystemCore"/> を生成します。
        /// </summary>
        /// <param name="initialAttributes">初期化に使用する属性一覧。</param>
        public AttributeSystemCore(IReadOnlyList<IAttribute> initialAttributes)
        {
            _attributeValues = new List<AttributeValue>(initialAttributes.Count);
            for (var i = 0; i < initialAttributes.Count; i++)
            {
                _attributeValues.Add(new AttributeValue
                {
                    Attribute = initialAttributes[i],
                    Modifier = AttributeModifier.Zero(),
                });
            }

            MarkAttributesDirty();
            GetAttributeCache();
        }

        /// <summary>
        /// 属性キャッシュを無効化し、次回アクセス時に再構築されるようにマークします。
        /// </summary>
        public void MarkAttributesDirty()
        {
            _attributeDictStale = true;
        }

        /// <inheritdoc />
        public bool TryGetValue(IAttribute attribute, out AttributeValue value)
        {
            var cache = GetAttributeCache();
            if (cache.TryGetValue(attribute, out var index))
            {
                value = _attributeValues[index];
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public void SetBaseValue(IAttribute attribute, float value)
        {
            var cache = GetAttributeCache();
            if (!cache.TryGetValue(attribute, out var index)) return;

            var attributeValue = _attributeValues[index];
            attributeValue.BaseValue = value;
            _attributeValues[index] = attributeValue;
        }

        /// <inheritdoc />
        public void ResetAll()
        {
            for (var i = 0; i < _attributeValues.Count; i++)
            {
                var defaultAttribute = new AttributeValue
                {
                    Attribute = _attributeValues[i].Attribute
                };
                _attributeValues[i] = defaultAttribute;
            }

            MarkAttributesDirty();
        }

        /// <inheritdoc />
        public void ResetModifiers()
        {
            for (var i = 0; i < _attributeValues.Count; i++)
            {
                var attributeValue = _attributeValues[i];
                attributeValue.Modifier = default;
                _attributeValues[i] = attributeValue;
            }
        }

        /// <inheritdoc />
        public void ApplyModifier(IAttribute attribute, AttributeModifier modifier)
        {
            var cache = GetAttributeCache();
            if (!cache.TryGetValue(attribute, out var index)) return;

            var value = _attributeValues[index];
            value.Modifier = value.Modifier.Combine(modifier);
            _attributeValues[index] = value;
        }

        /// <summary>
        /// 属性キャッシュを取得します。必要に応じて再構築を行います。
        /// </summary>
        /// <returns>属性インデックスキャッシュ。</returns>
        private Dictionary<IAttribute, int> GetAttributeCache()
        {
            if (_attributeDictStale)
            {
                _attributeIndexCache.Clear();
                for (var i = 0; i < _attributeValues.Count; i++)
                {
                    _attributeIndexCache[_attributeValues[i].Attribute] = i;
                }

                _attributeDictStale = false;
            }

            return _attributeIndexCache;
        }

        /// <inheritdoc />
        public void UpdateCurrentValues()
        {
            for (var i = 0; i < _attributeValues.Count; i++)
            {
                var attributeValue = _attributeValues[i];
                _attributeValues[i] = attributeValue.Attribute.CalculateCurrentAttributeValue(attributeValue, _attributeValues);
            }
        }

        /// <summary>
        /// 属性値一覧への参照を取得します。
        /// Unity コンポーネントなどがイベント用に参照することを想定しています。
        /// </summary>
        public List<AttributeValue> Values => _attributeValues;
    }
}


