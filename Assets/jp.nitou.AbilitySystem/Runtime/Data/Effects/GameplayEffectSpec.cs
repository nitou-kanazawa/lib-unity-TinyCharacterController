using System;
using Nitou.AbilitySystem.Core;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// ゲームプレイ効果スペックの実装です。
    /// </summary>
    [Serializable]
    public class GameplayEffectSpec : IGameplayEffectSpec
    {
        /// <summary>
        /// このスペックの元となるゲームプレイ効果アセット。
        /// </summary>
        public GameplayEffectAsset GameplayEffect { get; private set; }

        /// <inheritdoc />
        public IGameplayEffectDefinition Definition => GameplayEffect;

        /// <inheritdoc />
        public float DurationRemaining { get; private set; }

        /// <inheritdoc />
        public float TotalDuration { get; private set; }

        /// <summary>
        /// Period 設定（Data レイヤ用、Unity シリアライズ用）。
        /// </summary>
        public GameplayEffectPeriod PeriodDefinition { get; private set; }

        /// <inheritdoc />
        public float TimeUntilPeriodTick { get; private set; }

        /// <inheritdoc />
        public float Level { get; private set; }

        /// <summary>
        /// ソースとなるアビリティシステム。
        /// </summary>
        private IAbilitySystem _source;

        /// <summary>
        /// ターゲットとなるアビリティシステム。
        /// </summary>
        private IAbilitySystem? _target;

        /// <inheritdoc />
        public IAbilitySystem Source => _source;

        /// <inheritdoc />
        public IAbilitySystem? Target => _target;

        /// <inheritdoc />
        public Core.AttributeValue? SourceCapturedAttribute { get; set; }

        /// <summary>
        /// 新しいゲームプレイ効果スペックを生成します。
        /// </summary>
        /// <param name="gameplayEffect">ゲームプレイ効果アセット。</param>
        /// <param name="source">ソースとなるアビリティシステム。</param>
        /// <param name="level">効果のレベル。</param>
        /// <returns>生成されたスペック。</returns>
        public static GameplayEffectSpec CreateNew(GameplayEffectAsset gameplayEffect, IAbilitySystem source, float level = 1)
        {
            return new GameplayEffectSpec(gameplayEffect, source, level);
        }

        private GameplayEffectSpec(
            GameplayEffectAsset gameplayEffect,
            IAbilitySystem sourceSystem,
            float level = 1)
        {
            if (gameplayEffect == null)
                throw new ArgumentNullException(nameof(gameplayEffect));
            if (sourceSystem == null)
                throw new ArgumentNullException(nameof(sourceSystem));

            GameplayEffect = gameplayEffect;
            _source = sourceSystem;

            // マグニチュード計算器を初期化
            for (var i = 0; i < GameplayEffect.gameplayEffect.Modifiers.Length; i++)
            {
                GameplayEffect.gameplayEffect.Modifiers[i].modifierMagnitude.Initialize(this);
            }

            Level = level;

            // 継続時間を計算
            if (GameplayEffect.gameplayEffect.DurationModifier != null)
            {
                DurationRemaining = GameplayEffect.gameplayEffect.DurationModifier.CalculateMagnitude(this).GetValueOrDefault()
                                    * GameplayEffect.gameplayEffect.DurationMultiplier;
                TotalDuration = DurationRemaining;
            }

            // 周期処理の設定
            var period = GameplayEffect.Period;
            PeriodDefinition = GameplayEffect.PeriodData;
            TimeUntilPeriodTick = period.Period;
            if (period.ExecuteOnApplication)
            {
                TimeUntilPeriodTick = 0;
            }
        }

        /// <inheritdoc />
        public IGameplayEffectSpec WithTarget(IAbilitySystem target)
        {
            _target = target;
            return this;
        }

        /// <inheritdoc />
        public IGameplayEffectSpec WithLevel(float level)
        {
            Level = level;
            return this;
        }

        /// <summary>
        /// 総継続時間を設定します。
        /// </summary>
        public void SetTotalDuration(float totalDuration)
        {
            TotalDuration = totalDuration;
        }

        /// <inheritdoc />
        public void SetDuration(float duration)
        {
            DurationRemaining = duration;
        }

        /// <inheritdoc />
        public void UpdateRemainingDuration(float deltaTime)
        {
            DurationRemaining -= deltaTime;
        }

        /// <inheritdoc />
        public IGameplayEffectSpec TickPeriodic(float deltaTime, out bool executePeriodicTick)
        {
            TimeUntilPeriodTick -= deltaTime;
            executePeriodicTick = false;
            if (TimeUntilPeriodTick <= 0)
            {
                var period = GameplayEffect.Period;
                TimeUntilPeriodTick = period.Period;

                // 周期が有効な場合のみ実行
                if (period.Period > 0)
                {
                    executePeriodicTick = true;
                }
            }

            return this;
        }
    }
}
