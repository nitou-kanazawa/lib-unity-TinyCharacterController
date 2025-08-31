using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// Vector2型の入力アクション．
    /// ※値は管理クラスによって毎フレーム更新される．
    /// </summary>
    [System.Serializable]
    public struct Vector2Action
    {
        /// <summary>
        /// 現在の値．
        /// </summary>
        public Vector2 value;


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


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 入力アクションのリセット．
        /// </summary>
        public void Reset() => value = Vector2.zero;
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