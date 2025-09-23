using System;

namespace Nitou
{
    /// <summary>
    /// 値型の範囲を表すインターフェース．
    /// </summary>
    public interface IRangeValue<TValue>
        where TValue : struct
    {
        /// <summary>
        /// 最小値．
        /// </summary>
        TValue Min { get; set; }

        /// <summary>
        /// 最大値．
        /// </summary>
        TValue Max { get; set; }

        /// <summary>
        /// 中央値．
        /// </summary>
        TValue Mid { get; }

        /// <summary>
        /// 範囲の長さ．
        /// </summary>
        TValue Length { get; }

        /// <summary>
        /// 範囲内のランダムな値．
        /// </summary>
        TValue Random { get; }

        /// <summary>
        /// 値が範囲内か調べる．
        /// </summary>
        bool Contains(TValue value);

        /// <summary>
        /// 値を範囲内に制限する．
        /// </summary>
        TValue Clamp(TValue value);
    }


    public static class RangeValueExtensions
    {
        /// <summary>
        /// 範囲を 0 から 1 の範囲に正規化する拡張メソッド．
        /// </summary>
        public static float GetNormalized<TValue>(this IRangeValue<TValue> range, TValue value)
            where TValue : struct, IConvertible
        {
            var min = Convert.ToSingle(range.Min);
            var max = Convert.ToSingle(range.Max);
            var current = Convert.ToSingle(value);

            return (current - min) / (max - min);
        }

        /// <summary>
        /// 指定された値を新しい範囲にスケールする拡張メソッド．
        /// </summary>
        public static TValue ScaleToRange<TValue>(this IRangeValue<TValue> range, TValue value, IRangeValue<TValue> newRange)
            where TValue : struct, IConvertible
        {
            var normalized = range.GetNormalized(value);
            var newMin = Convert.ToSingle(newRange.Min);
            var newMax = Convert.ToSingle(newRange.Max);

            return (TValue)Convert.ChangeType(newMin + (newMax - newMin) * normalized, typeof(TValue));
        }
    }
}