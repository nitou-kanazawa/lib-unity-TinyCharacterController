#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nitou.CustomHierarchy
{
    internal static class MenuList
    {
        public static class Prefix
        {
            /// <summary>
            /// ダミーオブジェクト
            /// </summary>
            public const string DammyObject = "GameObject/Dammy Object/";
        }

        public static class Order
        {
            public const int DammyObject = 0;
        }
    }

    internal static class HierarchyObjectCreationMenu
    {
        /// <summary>
        /// ヘッダーの生成
        /// </summary>
        [MenuItem(
            MenuList.Prefix.DammyObject + "Header",
            priority = MenuList.Order.DammyObject
        )]
        private static void CreateHeader(MenuCommand menuCommand)
        {
            var obj = new GameObject("Header");
            obj.AddComponent<HierarchyHeader>();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// フォルダの生成
        /// </summary>
        [MenuItem(
            MenuList.Prefix.DammyObject + "Folder",
            priority = MenuList.Order.DammyObject
        )]
        private static void CreateFolder(MenuCommand menuCommand)
        {
            var obj = new GameObject("Folder");
            obj.AddComponent<HierarchyFolder>();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }

        /// <summary>
        /// セパレータの生成
        /// </summary>
        [MenuItem(
            MenuList.Prefix.DammyObject + "Separator",
            priority = MenuList.Order.DammyObject
        )]
        private static void CreateSeparator(MenuCommand menuCommand)
        {
            var obj = new GameObject("Separator");
            obj.AddComponent<HierarchySeparator>();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }
    }
}
#endif