#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Nitou.Inspector {
    internal abstract class RangeValueEditor : PropertyDrawer {

        protected static readonly GUIContent _minLabel = new ("min");
        protected static readonly GUIContent _maxLabel = new ("max");


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            // 値の取得
            var minProperty = property.FindPropertyRelative("_min");
            var maxProperty = property.FindPropertyRelative("_max");
            ValidateValue(minProperty, maxProperty);

            label = EditorGUI.BeginProperty(position, label, property);

            // プロパティの名前部分を表示
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            // MinとMaxの2つのプロパティを表示するので、残りのフィールドを半分こ。
            contentPosition.width /= 2.0f;

            EditorGUIUtility.labelWidth = 45f;
            EditorGUI.PropertyField(contentPosition, minProperty);

            contentPosition.x += contentPosition.width;
            EditorGUI.PropertyField(contentPosition, maxProperty);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        protected abstract void ValidateValue(SerializedProperty minProperty, SerializedProperty maxProperty);
    }
}
#endif
