namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 属性修飾のタイプを表す列挙型です。
    /// </summary>
    public enum AttributeModifierType
    {
        /// <summary>
        /// 加算修飾（BaseValue + Add）。
        /// </summary>
        Add,

        /// <summary>
        /// 乗算修飾（BaseValue * (1 + Multiply)）。
        /// </summary>
        Multiply,

        /// <summary>
        /// 上書き修飾（OverrideValue で上書き）。
        /// </summary>
        Override
    }
}

