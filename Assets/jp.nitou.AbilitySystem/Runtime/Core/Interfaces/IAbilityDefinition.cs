namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// アビリティの静的なデータ定義を表します。
    /// ScriptableObject などのアセットとして実装されます。
    /// </summary>
    public interface IAbilityDefinition
    {
        /// <summary>
        /// アビリティの表示名．
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// アビリティに紐づくタグ情報．
        /// </summary>
        AbilityTags Tags { get; }

        /// <summary>
        /// このアビリティを起動する際に支払うコストのゲームプレイ効果定義．
        /// コストが無い場合は null を返す
        /// </summary>
        IGameplayEffectDefinition? Cost { get; }

        /// <summary>
        /// このアビリティに紐づくクールダウンのゲームプレイ効果定義を取得します。
        /// クールダウンが無い場合は null を返します。
        /// </summary>
        IGameplayEffectDefinition? Cooldown { get; }

        /// <summary>
        /// 指定したオーナーに紐づいたアビリティスペックを生成します。
        /// </summary>
        /// <param name="owner">このアビリティを所持するアビリティシステム。</param>
        /// <returns>生成されたアビリティスペック。</returns>
        IAbilitySpec CreateSpec(IAbilitySystem owner);
    }
}


