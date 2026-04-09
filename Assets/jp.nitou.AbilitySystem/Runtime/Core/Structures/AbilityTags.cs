namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// アビリティに紐づくタグ情報を表す構造体です。
    /// Core レイヤの構造体として、抽象インターフェースを使用します。
    /// </summary>
    public struct AbilityTags
    {
        /// <summary>
        /// このアビリティを表すタグ。
        /// </summary>
        public IGameplayTag AssetTag;

        /// <summary>
        /// これらのタグを持つアクティブなアビリティをキャンセルします。
        /// </summary>
        public IGameplayTag[] CancelAbilitiesWithTags;

        /// <summary>
        /// これらのタグを持つアビリティの起動をブロックします。
        /// </summary>
        public IGameplayTag[] BlockAbilitiesWithTags;

        /// <summary>
        /// アビリティがアクティブな間、これらのタグが付与されます。
        /// </summary>
        public IGameplayTag[] ActivationOwnedTags;

        /// <summary>
        /// オーナーキャラクターが持つべきタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer OwnerTags;

        /// <summary>
        /// ソースキャラクターが持つべきタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer SourceTags;

        /// <summary>
        /// ターゲットキャラクターが持つべきタグ要件。
        /// </summary>
        public GameplayTagRequireIgnoreContainer TargetTags;
    }
}

