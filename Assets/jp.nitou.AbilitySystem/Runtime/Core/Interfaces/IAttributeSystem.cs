namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// 属性値とその修飾を管理するシステムです。
    /// </summary>
    public interface IAttributeSystem
    {
        /// <summary>
        /// 指定した属性の現在の値を取得する．
        /// 属性が存在しない場合は false を返す．
        /// </summary>
        /// <param name="attribute">取得したい属性。</param>
        /// <param name="value">取得された属性値。</param>
        /// <returns>属性が見つかった場合は true、それ以外は false。</returns>
        bool TryGetValue(IAttribute attribute, out AttributeValue value);

        /// <summary>
        /// 指定した属性のベース値を設定します。
        /// 属性が存在しない場合は何もしません。
        /// </summary>
        /// <param name="attribute">対象の属性。</param>
        /// <param name="value">設定するベース値。</param>
        void SetBaseValue(IAttribute attribute, float value);

        /// <summary>
        /// 全ての属性を初期状態にリセットする．
        /// </summary>
        void ResetAll();

        /// <summary>
        /// 全ての属性に対して、修飾値のみをリセットする．
        /// ベース値は変更しない．
        /// </summary>
        void ResetModifiers();

        /// <summary>
        /// 指定した属性に修飾値を適用する．
        /// 実際の現在値への反映は <see cref="UpdateCurrentValues"/> 呼び出し時に行われます。
        /// </summary>
        /// <param name="attribute">対象の属性。</param>
        /// <param name="modifier">適用する修飾値。</param>
        void ApplyModifier(IAttribute attribute, AttributeModifier modifier);

        /// <summary>
        /// ベース値と修飾値に基づき、全ての属性の現在値を再計算します。
        /// </summary>
        void UpdateCurrentValues();
    }
}


