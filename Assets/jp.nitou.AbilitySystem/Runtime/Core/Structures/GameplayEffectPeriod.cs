namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイ効果の周期処理を表す構造体です。
    /// </summary>
    public struct GameplayEffectPeriod
    {
        /// <summary>
        /// 周期処理の間隔（秒）。
        /// </summary>
        public float Period;

        /// <summary>
        /// 適用時に即座に実行するかどうか。
        /// </summary>
        public bool ExecuteOnApplication;
    }
}

