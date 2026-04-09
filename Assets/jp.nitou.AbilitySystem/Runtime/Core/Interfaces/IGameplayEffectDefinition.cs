namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイ効果（Gameplay Effect）の静的な定義を表します。
    /// 効果の継続時間や属性修正内容などを保持します。
    /// </summary>
    public interface IGameplayEffectDefinition
    {
        /// <summary>
        /// このゲームプレイ効果の継続ポリシーを取得します。
        /// </summary>
        DurationPolicy DurationPolicy { get; }

        /// <summary>
        /// 継続時間の基礎値を取得します。
        /// モディファイアによるスケーリングの前提となる値です。
        /// </summary>
        float BaseDuration { get; }

        /// <summary>
        /// 継続時間を算出するためのモディファイアを取得します。
        /// 使用しない場合は null を返します。
        /// </summary>
        IModifierMagnitude? DurationMagnitude { get; }

        /// <summary>
        /// このゲームプレイ効果が適用する属性修正の一覧を取得します。
        /// </summary>
        GameplayEffectModifier[] Modifiers { get; }

        /// <summary>
        /// 条件付きで適用される追加のゲームプレイ効果定義の一覧を取得します。
        /// </summary>
        ConditionalGameplayEffectContainer[] ConditionalEffects { get; }

        /// <summary>
        /// このゲームプレイ効果に紐づくタグ情報を取得します。
        /// </summary>
        GameplayEffectTags Tags { get; }

        /// <summary>
        /// このゲームプレイ効果の周期処理に関する定義を取得します。
        /// </summary>
        GameplayEffectPeriod Period { get; }
    }
}


