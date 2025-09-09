#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Nitou.IO;
using Nitou.EditorShared;

namespace Nitou.CustomHierarchy.EditorSctipts
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class HierarchyTreeMapDrawer : HierarchyDrawer
    {
        private static readonly Dictionary<string, Texture2D> TextureCached = new();

        // リソースパス
        private static readonly string REATIVE_PATH = "Textures";

        // ファイル名
        private static readonly string CURRENT_TEXTURE = "tree_map_current.png";
        private static readonly string LAST_TEXTURE = "tree_map_last.png";
        private static readonly string LEVEL_TEXTURE = "tree_map_level.png";
        private static readonly string LINE_TEXTURE = "tree_map_line.png";


        public static Texture2D TreeMapCurrent
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapCurrent), out var tex);
                if (tex != null) return tex;

                // Texture読み込み
                tex = AssetsLoader.Load<Texture2D>(PackageInfo.PackagePath, REATIVE_PATH, CURRENT_TEXTURE);
                TextureCached[nameof(TreeMapCurrent)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLast
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLast), out var tex);
                if (tex != null) return tex;

                // Texture読み込み
                tex = AssetsLoader.Load<Texture2D>(PackageInfo.PackagePath, REATIVE_PATH, LAST_TEXTURE);
                TextureCached[nameof(TreeMapLast)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLevel
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLevel), out var tex);
                if (tex != null) return tex;

                // Texture読み込み
                tex = AssetsLoader.Load<Texture2D>(PackageInfo.PackagePath, REATIVE_PATH, LEVEL_TEXTURE);
                TextureCached[nameof(TreeMapLevel)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLine
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLine), out var tex);
                if (tex != null) return tex;

                // Texture読み込み
                tex = AssetsLoader.Load<Texture2D>(PackageInfo.PackagePath, REATIVE_PATH, LINE_TEXTURE);
                TextureCached[nameof(TreeMapLine)] = tex;
                return tex;
            }
        }


        /// <summary>
        /// 描画処理
        /// </summary>
        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;

            var settings = HierarchySettingsSO.instance;

            var tempColor = GUI.color;

            if (settings.ShowTreeMap)
            {
                selectionRect.width = 14;
                selectionRect.height = 16;

                int childCount = gameObject.transform.childCount;
                int level = Mathf.RoundToInt(selectionRect.x / 14f);
                var t = gameObject.transform;
                Transform parent = null;

                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    selectionRect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount == 0)
                        {
                            GUI.color = settings.TreeMapColor;
                            GUI.DrawTexture(selectionRect, TreeMapLine);
                        }

                        t = gameObject.transform;
                    }
                    else if (i == 1)
                    {
                        GUI.color = settings.TreeMapColor;
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() == gameObject.scene.rootCount - 1)
                            {
                                GUI.DrawTexture(selectionRect, TreeMapLast);
                            }
                            else
                            {
                                GUI.DrawTexture(selectionRect, TreeMapCurrent);
                            }
                        }
                        else if (t.GetSiblingIndex() == parent.childCount - 1)
                        {
                            GUI.DrawTexture(selectionRect, TreeMapLast);
                        }
                        else
                        {
                            GUI.DrawTexture(selectionRect, TreeMapCurrent);
                        }

                        t = parent;
                    }
                    else
                    {
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() != gameObject.scene.rootCount - 1) GUI.DrawTexture(selectionRect, TreeMapLevel);
                        }
                        else if (t.GetSiblingIndex() != parent.childCount - 1) GUI.DrawTexture(selectionRect, TreeMapLevel);

                        t = parent;
                    }

                    if (t != null) parent = t.parent;
                    else break;
                }

                GUI.color = tempColor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static AssetPath GetAssetPath(string assetName)
        {
            var fullPath = PathUtils.Combine(PackageInfo.PackagePath.ToAbsolutePath(), REATIVE_PATH, assetName);
            return AssetPath.FromFullPath(fullPath);
        }
    }
}
#endif