namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイ効果の属性修飾を表す構造体です。
    /// Core レイヤの構造体として、抽象インターフェースを使用します。
    /// </summary>
    public struct GameplayEffectModifier
    {
        /// <summary>
        /// 修飾対象の属性。
        /// </summary>
        public IAttribute Attribute;

        /// <summary>
        /// 修飾のタイプ。
        /// </summary>
        public AttributeModifierType ModifierType;

        /// <summary>
        /// マグニチュード計算器。
        /// </summary>
        public IModifierMagnitude ModifierMagnitude;

        /// <summary>
        /// マグニチュードの乗数。
        /// </summary>
        public float Multiplier;
    }
}

