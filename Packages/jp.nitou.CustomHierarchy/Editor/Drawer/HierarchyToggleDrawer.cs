#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using Nitou.EditorShared;

// REF:
//  - _: Hierarchy でオブジェクトのコンポーネント一覧をアイコン表示 https://www.midnightunity.net/unity-extension-hierarchy-show-components/
//  - github : Alchemy/HierarchyToggleDrawer.cs https://github.com/AnnulusGames/Alchemy/blob/main/Alchemy/Assets/Alchemy/Editor/Hierarchy/HierarchyToggleDrawer.cs

namespace Nitou.CustomHierarchy.EditorSctipts
{
    internal sealed class HierarchyToggleDrawer : HierarchyDrawer
    {
        private const float TOGGLE_SIZE = 16f;
        private const float ICON_SIZE = 14f;
        private const float RIGHT_OFFSET = 2.7f;

        private static readonly Color _disableColor = new(1.0f, 1.0f, 1.0f, 0.5f);


        // ----------------------------------------------------------------------------
        // Public Method

        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            // GameObject取得
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            if (gameObject.TryGetComponent<HierarchyObject>(out _)) return; // ※ダミーオブジェクトははじく

            // 設定データ
            //var settings = nToolsSettings.GetOrCreateSettings();
            var settings = HierarchySettingsSO.instance;


            // トグルボタン
            if (settings.ShowHierarchyToggles)
            {
                var rect = selectionRect;
                rect.x = rect.xMax - RIGHT_OFFSET; // ※右端に配置
                rect.width = TOGGLE_SIZE;

                // Active状態の反映
                var active = GUI.Toggle(rect, gameObject.activeSelf, string.Empty);
                if (active != gameObject.activeSelf)
                {
                    Undo.RecordObject(gameObject, $"{(active ? "Activate" : "Deactivate")} GameObject '{gameObject.name}'");
                    gameObject.SetActive(active);
                    EditorUtility.SetDirty(gameObject);
                }
            }

            // アイコン
            if (settings.ShowComponentIcons)
            {
                // 描画位置
                var rect = selectionRect;
                rect.x = rect.xMax - ((settings.ShowHierarchyToggles ? TOGGLE_SIZE : 0f) + RIGHT_OFFSET);
                rect.y += 1f;
                rect.width = ICON_SIZE;
                rect.height = ICON_SIZE;

                // オブジェクトが所持しているコンポーネント一覧を取得
                var components = gameObject.GetComponents<Component>()
                                           //.AsEnumerable()           // ※Transform & RectTransformを表示する場合はこっち
                                           .Where(x => x is not Transform)
                                           .Reverse();

                // アイコンの描画
                var existsScriptIcon = false;
                foreach (var component in components)
                {
                    var image = AssetPreview.GetMiniThumbnail(component);
                    if (image == null) continue;

                    if (image == EditorUtil.Icons.scriptIcon.image)
                    {
                        if (existsScriptIcon) continue;
                        existsScriptIcon = true;
                    }

                    var color = component.IsEnabled() ? Color.white : _disableColor;
                    DrawIcon(ref rect, image, color);
                }
            }
        }


        /// ----------------------------------------------------------------------------
        // Static Method
        private static void DrawIcon(ref Rect rect, Texture image, Color color)
        {
            using (new EditorUtil.GUIColorScope(color))
            {
                GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
                rect.x -= rect.width;
            }
        }
    }

    
    internal static class ComponentExtensions
    {
        /// <summary>
        /// コンポーネントが有効かどうかを確認する拡張メソッド
        /// </summary>
        public static bool IsEnabled(this Component self)
        {
            var property = self.GetType().GetProperty("enabled", typeof(bool));
            return (bool)(property?.GetValue(self, null) ?? true);
        }
    }
}
#endif