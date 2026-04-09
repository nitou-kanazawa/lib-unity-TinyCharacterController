namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 実行時に生成されるゲームプレイ効果のスペック（インスタンス）を表します。
    /// レベルや残り時間などの状態を保持します。
    /// </summary>
    public interface IGameplayEffectSpec
    {
        /// <summary>
        /// 元となるゲームプレイ効果の定義を取得します。
        /// </summary>
        IGameplayEffectDefinition Definition { get; }

        /// <summary>
        /// この効果のレベルを取得します。
        /// </summary>
        float Level { get; }

        /// <summary>
        /// 効果の残り継続時間を取得します。
        /// </summary>
        float DurationRemaining { get; }

        /// <summary>
        /// 効果の総継続時間を取得します。
        /// </summary>
        float TotalDuration { get; }

        /// <summary>
        /// 次の周期処理が発生するまでの残り時間を取得します。
        /// </summary>
        float TimeUntilPeriodTick { get; }

        /// <summary>
        /// この効果を発生させたソースのアビリティシステムを取得します。
        /// </summary>
        IAbilitySystem Source { get; }

        /// <summary>
        /// この効果のターゲットとなるアビリティシステムを取得します。
        /// ターゲットが未設定の場合は null です。
        /// </summary>
        IAbilitySystem? Target { get; }

        /// <summary>
        /// ソース側からキャプチャされた属性値を取得します。
        /// キャプチャしていない場合は null です。
        /// </summary>
        AttributeValue? SourceCapturedAttribute { get; }

        /// <summary>
        /// ターゲットを設定した新しいスペックを返します。
        /// </summary>
        /// <param name="target">設定するターゲット。</param>
        /// <returns>ターゲットが設定されたスペック。</returns>
        IGameplayEffectSpec WithTarget(IAbilitySystem target);

        /// <summary>
        /// レベルを設定した新しいスペックを返します。
        /// </summary>
        /// <param name="level">設定するレベル。</param>
        /// <returns>レベルが設定されたスペック。</returns>
        IGameplayEffectSpec WithLevel(float level);

        /// <summary>
        /// 残り継続時間を直接設定します。
        /// </summary>
        /// <param name="duration">設定する残り時間。</param>
        void SetDuration(float duration);

        /// <summary>
        /// 経過時間に応じて残り継続時間を更新します。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        void UpdateRemainingDuration(float deltaTime);

        /// <summary>
        /// 周期処理のタイマーを更新し、必要であれば周期処理を実行すべきかどうかを判定します。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        /// <param name="executePeriodicTick">周期処理を実行すべき場合 true。</param>
        /// <returns>このインスタンス自身。</returns>
        IGameplayEffectSpec TickPeriodic(float deltaTime, out bool executePeriodicTick);
    }
}


