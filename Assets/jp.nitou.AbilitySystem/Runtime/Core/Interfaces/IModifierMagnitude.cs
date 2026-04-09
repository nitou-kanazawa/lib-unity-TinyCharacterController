namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// モディファイアマグニチュードを表す抽象インターフェースです。
    /// Core レイヤはこのインターフェースのみに依存し、Data レイヤの具象型には依存しません。
    /// </summary>
    public interface IModifierMagnitude
    {
        /// <summary>
        /// スペックの初期化時に呼び出されます。
        /// </summary>
        /// <param name="spec">ゲームプレイ効果スペック。</param>
        void Initialize(IGameplayEffectSpec spec);

        /// <summary>
        /// マグニチュードを計算します。
        /// </summary>
        /// <param name="spec">ゲームプレイ効果スペック。</param>
        /// <returns>計算されたマグニチュード。計算できない場合は null。</returns>
        float? CalculateMagnitude(IGameplayEffectSpec spec);
    }
}

