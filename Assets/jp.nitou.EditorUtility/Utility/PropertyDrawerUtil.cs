# if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// REF:
//  - LIGHT11: PropertyDrawerでデフォルトのGUIを描画する https://light11.hatenadiary.com/entry/2019/05/13/215814

namespace Nitou.EditorShared
{
    /// <summary>
    /// <see cref="PropertyDrawer"/>型の基本的な拡張メソッド集
    /// </summary>
    public static class PropertyDrawerUtil
    {
        /// <summary>
        /// デフォルトのGUIを表示する
        /// </summary>
        public static void DrawDefaultGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property = property.serializedObject.FindProperty(property.propertyPath); // ←何やってる？

            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(fieldRect, label, property))
            {
                // [TODO] 実装
            }
        }

        public static float GetDefaultPropertyHeight()
        {
            // TODO: 実装

            return 0f;
        }
    }
}
#endif