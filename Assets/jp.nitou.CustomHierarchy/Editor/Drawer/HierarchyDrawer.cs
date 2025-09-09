#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nitou.EditorShared;

namespace Nitou.CustomHierarchy.EditorSctipts
{
    /// <summary>
    /// ヒエラルキードロワーの基底クラス
    /// </summary>
    internal abstract class HierarchyDrawer
    {
        // ※HierarchyItemCallbackとして登録する
        public abstract void OnGUI(int instanceID, Rect selectionRect);


        /// ----------------------------------------------------------------------------
        // Inner Method
        protected static Rect GetBackgroundRect(Rect selectionRect)
        {
            return selectionRect.AddXMax(20f);
        }

        protected static void DrawBackground(int instanceID, Rect selectionRect)
        {
            var backgroundRect = GetBackgroundRect(selectionRect);

            Color backgroundColor;
            var e = Event.current;
            var isHover = backgroundRect.Contains(e.mousePosition);

            if (Selection.Contains(instanceID))
            {
                backgroundColor = EditorColors.HighlightBackground;
            }
            else if (isHover)
            {
                backgroundColor = EditorColors.HighlightBackgroundInactive;
            }
            else
            {
                backgroundColor = EditorColors.WindowBackground;
            }

            EditorGUI.DrawRect(backgroundRect, backgroundColor);
        }
    }
}
#endif