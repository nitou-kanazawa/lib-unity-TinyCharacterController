#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nitou.EditorShared;

namespace Nitou.CustomHierarchy.EditorSctipts
{
    internal sealed class HierarchySeparatorDrawer : HierarchyDrawer
    {
        private static Color SeparatorColor => new(0.5f, 0.5f, 0.5f);


        /// ----------------------------------------------------------------------------
        // Public Method
        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            // GameObject取得
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            if (!gameObject.TryGetComponent<HierarchySeparator>(out _)) return;

            // 背景描画
            DrawBackground(instanceID, selectionRect);

            // 固有描画
            var lineRect = selectionRect.AddY(selectionRect.height * 0.5f).AddXMax(14f).SetHeight(1f);
            EditorGUI.DrawRect(lineRect, SeparatorColor);
        }
    }
}
#endif