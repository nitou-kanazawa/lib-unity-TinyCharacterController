#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nitou.CustomHierarchy.EditorSctipts
{
    internal class HierarchyRowSeparatorDrawer : HierarchyDrawer
    {
        /// ----------------------------------------------------------------------------
        // Public Method
        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            // 
            var settings = HierarchySettingsSO.instance;
            if (!settings.ShowSeparator) return;

            // 対象領域
            var rect = new Rect { y = selectionRect.y, width = selectionRect.width + selectionRect.x, height = 1, x = 0 };
            EditorGUI.DrawRect(rect, settings.SeparatorColor);

            if (!settings.ShowRowShading) return;
            selectionRect.width += selectionRect.x;
            selectionRect.x = 0;
            selectionRect.height -= 1;
            selectionRect.y += 1;
            EditorGUI.DrawRect(selectionRect, Mathf.FloorToInt((selectionRect.y - 4) / 16 % 2) == 0 ? settings.EvenRowColor : settings.OddRowColor);
        }
    }
}
#endif