using System.Collections.Generic;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 属性を表す抽象インターフェースです。
    /// Core レイヤはこのインターフェースのみに依存し、Data レイヤの具象型には依存しません。
    /// </summary>
    public interface IAttribute
    {
        /// <summary>
        /// 属性の表示名．
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 属性の現在値を計算する．
        /// </summary>
        /// <param name="attributeValue">計算対象の属性値。</param>
        /// <param name="otherAttributeValues">他の属性値のリスト（依存属性の計算に使用）。</param>
        /// <returns>計算された属性値。</returns>
        AttributeValue CalculateCurrentAttributeValue(AttributeValue attributeValue, IReadOnlyList<AttributeValue> otherAttributeValues);
    }
}

