namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 条件付きで適用されるゲームプレイ効果を表す構造体です。
    /// </summary>
    public struct ConditionalGameplayEffectContainer
    {
        /// <summary>
        /// 適用するゲームプレイ効果定義。
        /// </summary>
        public IGameplayEffectDefinition GameplayEffect;

        /// <summary>
        /// ソースに必要なタグ。
        /// </summary>
        public IGameplayTag[] RequiredSourceTags;
    }
}

