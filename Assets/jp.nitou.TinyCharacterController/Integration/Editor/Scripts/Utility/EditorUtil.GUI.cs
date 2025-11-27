#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

// [参考]
//  _: Unityのエディタ拡張で FoldOut をかっこよくするのをやってみた https://tips.hecomi.com/entry/2016/10/15/004144
//  _: 編集不可のパラメータをInspectorに表示する https://kazupon.org/unity-no-edit-param-view-inspector/
//  Hatena: インデント付きでGUI.Buttonを表示する https://neptaco.hatenablog.jp/entry/2019/05/18/234426

namespace Nitou.EditorShared {
    public static partial class EditorUtil {

        /// <summary>
        /// GUI描画関連のメソッド集．
        /// </summary>
        public static class GUI {

            /// ----------------------------------------------------------------------------
            #region Object Field

            /// <summary>
            /// MonoBehaviourファイルへの参照を表示する．
            /// </summary>
            public static void MonoBehaviourField<T>(T target) where T : MonoBehaviour {

                using (new EditorGUI.DisabledGroupScope(true)) {
                    EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(target), typeof(T), false);
                }
            }

            /// <summary>
            /// Scriptable Objectファイルへの参照を表示する．
            /// </summary>
            public static void ScriptableObjectField<T>(T target) where T : ScriptableObject {

                using (new EditorGUI.DisabledGroupScope(true)) {
                    EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(target), typeof(T), false);
                }
            }

            /// <summary>
            /// PropertyFieldをReadOnlyで表示する．
            /// </summary>
            public static void ReadOnlyPropertyField(SerializedProperty property, params GUILayoutOption[] options) {
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.PropertyField(property, options);
                }
            }

            #endregion


            /// ----------------------------------------------------------------------------
            #region Button

            // [参考]
            //  zenn:  EditorWindow で TextField と同じ幅のボタンを配置する https://zenn.dev/kobi32768/articles/01f34751878fc8

            /// <summary>
            /// GUI.Buttonをインデント付きで表示する
            /// </summary>
            public static bool FieldSizeButton(GUIContent content) {
                Rect rect = EditorGUILayout.GetControlRect(true);
                rect = EditorGUI.PrefixLabel(rect, new GUIContent("Label"));
                return UnityEngine.GUI.Button(rect, content);
            }

            /// <summary>
            /// GUI.Buttonをインデント付きで表示する
            /// </summary>
            public static bool IndentedButton(GUIContent content) {
                var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                return UnityEngine.GUI.Button(rect, content);
            }

            /// <summary>
            /// GUI.Buttonをインデント付きで表示する
            /// </summary>
            public static bool IndentedButton(string content) =>
                IndentedButton(new GUIContent(content));


            //public static bool ToggleButton(bool isOn, GUIContent on, GUIContent off, 
            //    System.Action<bool> onClick = null, params GUILayoutOption[] options) {

            //    if( GUILayout.Button(isOn ? on : off, options)){

            //        onClick?.Invoke(isOn);
            //    }
            //}

            #endregion

            /// <summary>
            /// 
            /// </summary>
            public static void EnumToggles(SerializedProperty property) {
                // Enum のフィールドをトグルボタンとして表示
                //using (new EditorGUILayout.PropertyScope(label, property)) {

                    // 現在のEnumのインデックスを取得
                    EditorGUI.BeginChangeCheck();
                    int value = property.enumValueIndex;
                    string[] enumNames = property.enumDisplayNames;

                    // トグルボタンを横に並べて表示
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < enumNames.Length; i++) {
                        if (GUILayout.Toggle(value == i, enumNames[i], "Button")) {
                            value = i;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // インデックスが変更された場合、プロパティに新しい値を設定
                    if (EditorGUI.EndChangeCheck()) {
                        property.enumValueIndex = value;
                    }
                //}

            }


            /// ----------------------------------------------------------------------------
            #region Decoration

            /// <summary>
            /// 仕切り線を表示する
            /// </summary>
            public static void HorizontalLine(Color color, int thickness = 1, int padding = 10, bool useIndentLevel = false) {

                using (new EditorGUILayout.HorizontalScope()) {
                    var splitterRect = EditorGUILayout.GetControlRect(false, GUILayout.Height(thickness + padding));
                    splitterRect = EditorGUI.IndentedRect(splitterRect);
                    splitterRect.height = thickness;
                    splitterRect.y += padding / 2;

                    EditorGUI.DrawRect(splitterRect, color);
                }
            }

            /// <summary>
            /// 仕切り線を表示する
            /// </summary>
            public static void HorizontalLine(int thickness = 1, int padding = 10, bool useIndentLevel = false) =>
                HorizontalLine(EditorColors.ButtonBackgroundHover, thickness, padding, useIndentLevel);

            public static void Line(int height = 1) {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
            }

            #endregion



            /// ----------------------------------------------------------------------------
            #region Foldout

            // [参考]
            //  _: Unity のエディタ拡張で FoldOut をかっこよくするのをやってみた https://tips.hecomi.com/entry/2016/10/15/004144 
            //  github: https://github.com/Unity-Technologies/MissilesPerfectMaster/blob/master/Assets/CinematicEffects/Common/Editor/EditorGUIHelper.cs

            /// <summary>
            /// Foldout可能なヘッダーを表示する
            /// </summary>
            public static bool FoldoutHeader(string title, bool display) {

                // ParticleSystemで使用されているGUI Style
                var style = new GUIStyle("ShurikenModuleTitle") {
                    font = new GUIStyle(EditorStyles.label).font,
                    fontSize = 12,
                    border = new RectOffset(15, 7, 4, 4),
                    fixedHeight = 22,
                    contentOffset = new Vector2(20f, -2f),
                };

                // 領域の描画
                var rect = GUILayoutUtility.GetRect(16f, 22f, style);
                UnityEngine.GUI.Box(rect, title, style);

                var e = Event.current;

                // の描画
                if (e.type == EventType.Repaint) {
                    var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
                    EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
                }

                // foldのON/OFF切り替え
                else if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
                    display = !display;
                    e.Use();
                }

                return display;
            }

            public static bool Header(SerializedProperty group, SerializedProperty enabledField) {
                var display = group == null || group.isExpanded;
                var enabled = enabledField != null && enabledField.boolValue;
                var title = group == null ? "Unknown Group" : ObjectNames.NicifyVariableName(group.displayName);

                Rect rect = GUILayoutUtility.GetRect(16f, 22f, Styles.folderHeader);
                UnityEngine.GUI.Box(rect, title, Styles.folderHeader);

                Rect toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
                if (Event.current.type == EventType.Repaint)
                    Styles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

                var e = Event.current;

                if (e.type == EventType.MouseDown) {
                    if (toggleRect.Contains(e.mousePosition) && enabledField != null) {
                        enabledField.boolValue = !enabledField.boolValue;
                        e.Use();
                    } else if (rect.Contains(e.mousePosition) && group != null) {
                        display = !display;
                        group.isExpanded = !group.isExpanded;
                        e.Use();
                    }
                }
                return display;
            }

            /// <summary>
            /// 
            /// </summary>
            public class FoldoutGroupScope : UnityEngine.GUI.Scope {

                // 内部スコープ要素
                private readonly EditorGUILayout.FadeGroupScope _fadeGroup;
                private readonly EditorGUI.IndentLevelScope _indentScope;
                private readonly EditorGUILayout.VerticalScope _verticalScope;

                public bool Visible => _fadeGroup.visible;

                /// <summary>
                /// コンストラクタ
                /// </summary>
                public FoldoutGroupScope(string headerTitle, AnimBool animBool, bool withBackdrop = false) {

                    if (withBackdrop) {
                        _verticalScope = new EditorGUILayout.VerticalScope(UnityEngine.GUI.skin.box);
                    }

                    animBool.target = EditorUtil.GUI.FoldoutHeader(headerTitle, animBool.target);
                    _fadeGroup = new EditorGUILayout.FadeGroupScope(animBool.faded);
                    _indentScope = new EditorGUI.IndentLevelScope();

                }

                /// <summary>
                /// 終了処理
                /// </summary>
                protected override void CloseScope() {
                    _fadeGroup?.Dispose();
                    _indentScope?.Dispose();
                    _verticalScope?.Dispose();
                }
            }

            #endregion


            /// ----------------------------------------------------------------------------
            #region  Misc

            public static void UrlLabel(string displayText, string url) {

                GUILayout.Label(displayText, EditorStyles.linkLabel);
                Rect rect = GUILayoutUtility.GetLastRect();

                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                var nowEvent = Event.current;
                if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition)) {
                    Help.BrowseURL(url);
                }
            }

            /// <summary>
            /// 区間のオンオフ切り替え
            /// </summary>
            static public bool DrawHeader(string text, string key, bool forceOn) {
                bool state = EditorPrefs.GetBool(key, true);

                GUILayout.Space(3f);
                if (!forceOn && !state) UnityEngine.GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(3f);

                UnityEngine.GUI.changed = false;

                if (!GUILayout.Toggle(true, "<b><size=11>" + text + "</size></b>", "dragtab", GUILayout.MinWidth(20f))) state = !state;
                if (UnityEngine.GUI.changed) EditorPrefs.SetBool(key, state);

                GUILayout.Space(2f);
                GUILayout.EndHorizontal();
                UnityEngine.GUI.backgroundColor = Color.white;
                if (!forceOn && !state) GUILayout.Space(3f);
                return state;
            }
            #endregion
        }
    }
}
#endif


