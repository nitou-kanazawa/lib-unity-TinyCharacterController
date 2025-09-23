using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou
{
    /// <summary>
    /// <see cref="int"/>型で範囲を表す構造体．
    /// </summary>
    public class RangeInt : IRangeValue<int>
    {
        [SerializeField] private int _min;
        [SerializeField] private int _max;

        public int Min
        {
            get => _min;
            set => _min = Mathf.Min(_min, value);
        }

        public int Max
        {
            get => _max;
            set => _max = Mathf.Max(_max, value);
        }

        public int Mid => _min + (_max - _min) / 2;

        public int Length => Mathf.Abs(_max - _min);

        public int Random => _min < _max ? UnityEngine.Random.Range(_min, _max + 1) : _min;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public RangeInt(int min, int max)
        {
            _min = min;
            _max = Mathf.Max(min, max);
        }

        public bool Contains(int value) => _min <= value && value <= _max;

        public int Clamp(int value) => Mathf.Clamp(value, _min, _max);
    }
}


#if UNITY_EDITOR
namespace Nitou.Inspector
{
    [CustomPropertyDrawer(typeof(RangeInt))]
    internal class RangeIntEditor : RangeValueEditor
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
}
#endif