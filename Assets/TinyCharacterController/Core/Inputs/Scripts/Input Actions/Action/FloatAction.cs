using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// Float型の入力アクション．
    /// ※値は管理クラスによって毎フレーム更新される．
    /// </summary>
    [System.Serializable]
    public struct FloatAction
    {
        /// <summary>現在の値．</summary>
        public float value;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 入力アクションのリセット．
        /// </summary>
        public void Reset() => value = 0f;
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