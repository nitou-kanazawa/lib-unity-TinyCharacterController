using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// REF:
// - Hatena: 最小と最大の値を管理する構造体を作りたいの https://www.urablog.xyz/entry/2017/06/14/094730

namespace Nitou
{
    /// <summary>
    /// 範囲を<see cref="float"/>型で表す構造体
    /// </summary>
    [System.Serializable]
    public struct RangeFloat : IRangeValue<float>
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public float Min
        {
            get => _min;
            set => _min = Mathf.Min(_max, value);
        }

        public float Max
        {
            get => _max;
            set => _max = Mathf.Max(_min, value);
        }

        public float Mid => _min + (_max - _min) / 2f;

        public float Length => Mathf.Abs(_max - _min);

        public float Random => _min < _max ? UnityEngine.Random.Range(_min, _max) : _min;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public RangeFloat(float min, float max)
        {
            _min = min;
            _max = Mathf.Max(min, max); // ※minを基準とする
        }

        /// <summary>
        /// 値が範囲内か調べる．
        /// </summary>
        public bool Contains(float value) => _min <= value && value <= _max;

        /// <summary>
        /// パラメータ t (0~1)で線形補間した値を返す．
        /// </summary>
        public float Lerp(float t)
        {
            return Mathf.Lerp(_min, _max, t);
        }

        /// <summary>
        /// パラメータ t で線形補間した値を返す
        /// </summary>
        public float LerpUnclamped(float t)
        {
            return Mathf.LerpUnclamped(_min, _max, t);
        }

        /// <summary>
        /// パラメータtを取得する
        /// </summary>
        public float InverseLerp(float value)
        {
            return Mathf.InverseLerp(_min, _max, value);
        }

        /// <summary>
        /// 値を範囲内に制限する
        /// </summary>
        public float Clamp(float value)
        {
            return Mathf.Clamp(value, _min, _max);
        }


        // ----------------------------------------------------------------------------
        // Static Method

        /// <summary>
        /// 他の範囲の値を現在の範囲にリマップします。
        /// </summary>
        public static float Remap(float value, RangeFloat from, RangeFloat to)
        {
            float t = from.InverseLerp(value);
            return to.Lerp(t);
        }
    }


    public static partial class FloatExtensions
    {
        /// <summary>
        /// 値を範囲内に制限する拡張メソッド
        /// </summary>
        public static float Clamp(this float value, RangeFloat range)
        {
            return range.Clamp(value);
        }

        /// <summary>
        /// 値が範囲内にあるかどうかを判定する拡張メソッド
        /// </summary>
        public static bool IsInRange(this float value, RangeFloat range)
        {
            return range.Contains(value);
        }
    }
}


#if UNITY_EDITOR
namespace Nitou.Inspector
{
    [CustomPropertyDrawer(typeof(RangeFloat))]
    internal class RangeFloatEditor : RangeValueEditor
    {
        protected override void ValidateValue(SerializedProperty minProperty, SerializedProperty maxProperty)
        {
            // 小さい数値を基準にして、大きい数値が小さい数値より小さくならないようにしてみよう。
            if (maxProperty.floatValue < minProperty.floatValue)
            {
                minProperty.floatValue = maxProperty.floatValue;
            }
        }
    }
#endif
}