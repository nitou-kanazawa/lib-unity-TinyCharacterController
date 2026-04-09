namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイ効果に紐づくタグ情報を表す構造体です。
    /// Core レイヤの構造体として、抽象インターフェースを使用します。
    /// </summary>
    public struct GameplayEffectTags
    {
        /// <summary>
        /// このゲームプレイ効果を表すタグ。
        /// </summary>
        public IGameplayTag AssetTag;

        /// <summary>
        /// この効果が付与するタグ。
        /// </summary>
        public IGameplayTag[] GrantedTags;

        /// <summary>
        /// この効果が「オン」とみなされるためのタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer OngoingTagRequirements;

        /// <summary>
        /// この効果を適用するために必要なタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer ApplicationTagRequirements;

        /// <summary>
        /// この効果を削除するためのタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer RemovalTagRequirements;

        /// <summary>
        /// これらのタグに一致する効果を削除します。
        /// </summary>
        public IGameplayTag[] RemoveGameplayEffectsWithTag;
    }
}

