using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.TCC.Integration
{
    /// <summary>
    /// Float型の入力アクション
    /// ※値は管理クラスによって毎フレーム更新される
    /// </summary>
    [System.Serializable]
    public struct FloatAction
    {
        /// <summary>現在の値</summary>
        public float value;

        // 前回値のキャッシュ
        private float _previousValue;

        // しきい値（変化検出用）
        private float _threshold;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// このフレームで値が変化したかどうか
        /// </summary>
        public bool Changed { get; private set; }

        /// <summary>
        /// 前フレームとの差分値
        /// </summary>
        public float Delta { get; private set; }

        /// <summary>
        /// しきい値を超えて増加したかどうか
        /// </summary>
        public bool Increased => Delta > _threshold;

        /// <summary>
        /// しきい値を超えて減少したかどうか
        /// </summary>
        public bool Decreased => Delta < -_threshold;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 値の初期化
        /// </summary>
        /// <param name="threshold">変化検出のしきい値（デフォルト: 0.1f）</param>
        public void Initialize(float threshold = 0.1f)
        {
            value = 0f;
            _previousValue = 0f;
            _threshold = threshold;
            Changed = false;
            Delta = 0f;
        }

        /// <summary>
        /// 入力アクションのリセット
        /// </summary>
        public void Reset()
        {
            value = 0f;
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
            Changed = Mathf.Abs(Delta) > Mathf.Epsilon;

            // 前フレーム情報として格納
            _previousValue = value;
        }
    }


    // ----------------------------------------------------------------------------
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FloatAction))]
    internal class FloatActionEditor : PropertyDrawer
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
