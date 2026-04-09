using System.Collections;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 実行時にインスタンス化されたアビリティを表します。
    /// アビリティごとの状態と起動ロジックを保持します。
    /// </summary>
    public interface IAbilitySpec
    {
        /// <summary>
        /// このスペックの元となるアビリティ定義を取得します。
        /// </summary>
        IAbilityDefinition Definition { get; }

        /// <summary>
        /// このアビリティスペックの所有者となるアビリティシステムを取得します。
        /// </summary>
        IAbilitySystem Owner { get; }

        /// <summary>
        /// このアビリティのレベルを取得または設定します。
        /// </summary>
        float Level { get; set; }

        /// <summary>
        /// アビリティが現在アクティブかどうかを取得します。
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 現在の状態と条件に基づき、このアビリティを起動できるかどうかを判定します。
        /// </summary>
        /// <returns>起動可能な場合は true、それ以外は false。</returns>
        bool CanActivate();

        /// <summary>
        /// アビリティを起動します。
        /// コルーチンとして実行されることを想定しています。
        /// </summary>
        /// <returns>アビリティ処理を表す IEnumerator。</returns>
        IEnumerator Activate();

        /// <summary>
        /// アビリティがアクティブな場合、強制的にキャンセルします。
        /// </summary>
        void Cancel();
    }
}


