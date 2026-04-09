using System.Collections.Generic;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 実行中のゲームプレイ効果インスタンスを表します。
    /// </summary>
    public interface IGameplayEffectInstance
    {
        /// <summary>
        /// このインスタンスに対応するゲームプレイ効果スペックを取得します。
        /// </summary>
        IGameplayEffectSpec Spec { get; }
    }

    /// <summary>
    /// ゲームプレイ効果の適用とライフサイクルを管理するシステムです。
    /// </summary>
    public interface IGameplayEffectSystem
    {
        /// <summary>
        /// 現在アクティブな全てのゲームプレイ効果インスタンスを取得します。
        /// </summary>
        IReadOnlyList<IGameplayEffectInstance> ActiveEffects { get; }

        /// <summary>
        /// 指定したゲームプレイ効果スペックを適用します。
        /// 継続時間や種別に応じて内部状態を更新します。
        /// </summary>
        /// <param name="spec">適用するゲームプレイ効果スペック。</param>
        void ApplyEffect(IGameplayEffectSpec spec);

        /// <summary>
        /// 指定したゲームプレイ効果インスタンスを強制的に削除します。
        /// </summary>
        /// <param name="instance">削除するインスタンス。</param>
        void RemoveEffect(IGameplayEffectInstance instance);

        /// <summary>
        /// 経過時間に応じて全てのアクティブな効果を更新します。
        /// 継続時間の減少や周期処理、終了判定などを行います。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        void Tick(float deltaTime);
    }
}


