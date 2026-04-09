namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 属性値とその修飾を表す構造体です。
    /// Core レイヤの構造体として、Data レイヤに依存しません。
    /// </summary>
    public struct AttributeValue
    {
        /// <summary>
        /// この属性値に対応する属性の参照。
        /// </summary>
        public IAttribute Attribute;

        /// <summary>
        /// 属性のベース値。
        /// </summary>
        public float BaseValue;

        /// <summary>
        /// 属性の現在値（修飾を適用した後の値）。
        /// </summary>
        public float CurrentValue;

        /// <summary>
        /// 属性に適用されている修飾値。
        /// </summary>
        public AttributeModifier Modifier;
    }

    /// <summary>
    /// 属性に適用する修飾値を表す構造体です。
    /// </summary>
    public struct AttributeModifier
    {
        /// <summary>
        /// 加算修飾値。
        /// </summary>
        public float Add;

        /// <summary>
        /// 乗算修飾値。
        /// </summary>
        public float Multiply;

        /// <summary>
        /// 上書き修飾値（0 以外の場合、この値で上書き）。
        /// </summary>
        public float OverrideValue;

        /// <summary>
        /// 2 つの修飾値を結合します。
        /// </summary>
        /// <param name="other">結合する修飾値。</param>
        /// <returns>結合された修飾値。</returns>
        public AttributeModifier Combine(AttributeModifier other)
        {
            return new AttributeModifier
            {
                Add = this.Add + other.Add,
                Multiply = this.Multiply + other.Multiply,
                OverrideValue = other.OverrideValue != 0 ? other.OverrideValue : this.OverrideValue
            };
        }

        /// <summary>
        /// ゼロの修飾値を取得します。
        /// </summary>
        public static AttributeModifier Zero() => new AttributeModifier
        {
            Add = 0f,
            Multiply = 0f,
            OverrideValue = 0f
        };
    }
}

