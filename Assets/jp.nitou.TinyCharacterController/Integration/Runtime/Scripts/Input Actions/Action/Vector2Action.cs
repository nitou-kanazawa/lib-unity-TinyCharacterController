using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.TCC.Integration
{
    /// <summary>
    /// Vector2型の入力アクション
    /// ※値は管理クラスによって毎フレーム更新される
    /// </summary>
    [System.Serializable]
    public struct Vector2Action
    {
        /// <summary>
        /// 現在の値
        /// </summary>
        public Vector2 value;

        // 前回値のキャッシュ
        private Vector2 _previousValue;

        // しきい値（変化検出用）
        private float _threshold;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// Returns true if the value is not equal to zero (e.g. When pressing a D-pad)
        /// </summary>
        public bool Detected => value != Vector2.zero;

        /// <summary>
        /// Returns true if the x component is positive.
        /// </summary>
        public bool Right => value.x > 0;

        /// <summary>
        /// Returns true if the x component is negative.
        /// </summary>
        public bool Left => value.x < 0;

        /// <summary>
        /// Returns true if the y component is positive.
        /// </summary>
        public bool Up => value.y > 0;

        /// <summary>
        /// Returns true if the y component is negative.
        /// </summary>
        public bool Down => value.y < 0;

        // ----- 状態追跡プロパティ

        /// <summary>
        /// このフレームで値が変化したかどうか
        /// </summary>
        public bool Changed { get; private set; }

        /// <summary>
        /// 前フレームとの差分ベクトル
        /// </summary>
        public Vector2 Delta { get; private set; }

        /// <summary>
        /// 大きさ（マグニチュード）が変化したかどうか
        /// </summary>
        public bool MagnitudeChanged { get; private set; }

        /// <summary>
        /// 大きさの差分
        /// </summary>
        public float MagnitudeDelta { get; private set; }

        /// <summary>
        /// 方向が変化したかどうか（しきい値以上の角度変化）
        /// </summary>
        public bool DirectionChanged { get; private set; }

        /// <summary>
        /// 方向の変化角度（度数法）
        /// </summary>
        public float DirectionAngleDelta { get; private set; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 値の初期化
        /// </summary>
        /// <param name="threshold">変化検出のしきい値（デフォルト: 0.1f）</param>
        public void Initialize(float threshold = 0.1f)
        {
            value = Vector2.zero;
            _previousValue = Vector2.zero;
            _threshold = threshold;
            Changed = false;
            Delta = Vector2.zero;
            MagnitudeChanged = false;
            MagnitudeDelta = 0f;
            DirectionChanged = false;
            DirectionAngleDelta = 0f;
        }

        /// <summary>
        /// 入力アクションのリセット
        /// </summary>
        public void Reset()
        {
            value = Vector2.zero;
            Changed = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float dt)
        {
            // 差分の計算
            Delta = value - _previousValue;

            // 変化の検出
            Changed = Delta.sqrMagnitude > Mathf.Epsilon;

            // 大きさの変化
            float currentMagnitude = value.magnitude;
            float previousMagnitude = _previousValue.magnitude;
            MagnitudeDelta = currentMagnitude - previousMagnitude;
            MagnitudeChanged = Mathf.Abs(MagnitudeDelta) > _threshold;

            // 方向の変化（両方がゼロでない場合のみ計算）
            if (currentMagnitude > Mathf.Epsilon && previousMagnitude > Mathf.Epsilon)
            {
                DirectionAngleDelta = Vector2.Angle(_previousValue, value);
                DirectionChanged = DirectionAngleDelta > _threshold * 10f;  // しきい値の10倍（度数）
            }
            else
            {
                DirectionAngleDelta = 0f;
                DirectionChanged = false;
            }

            // 前フレーム情報として格納
            _previousValue = value;
        }
    }


    // ----------------------------------------------------------------------------
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Vector2Action))]
    internal class Vector2ActionEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                SerializedProperty value = property.FindPropertyRelative("value");

                // label
                Rect fieldRect = position;
                fieldRect.height = EditorGUIUtility.singleLineHeight;
                fieldRect.width = 100;

                EditorGUI.LabelField(fieldRect, label);

                // field
                fieldRect.x += 110;
                EditorGUI.PropertyField(fieldRect, value, GUIContent.none);
            }
        }
    }
#endif
}
