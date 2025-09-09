#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using PackageInfo = Nitou.CustomHierarchy.PackageInfo;

// [参考]
//  qiita: Unityで独自の設定のUIを提供できるSettingsProviderの紹介と設定ファイルの保存について https://qiita.com/sune2/items/a88cdee6e9a86652137c

namespace Nitou.CustomHierarchy.EditorSctipts
{
    internal class HierarchySettingsProvider : SettingsProvider
    {
        private SerializedObject _settings;

        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HierarchySettingsProvider(string path, SettingsScope scopes) : base(path, scopes) { }

        /// <summary>
        /// このメソッドが重要です
        /// 独自のSettingsProviderを返すことで、設定項目を追加します
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            // ※第三引数のkeywordsは、検索時にこの設定項目を引っかけるためのキーワード
            return new HierarchySettingsProvider(PackageInfo.ProjectSettingsMenuPath, SettingsScope.Project)
            {
                label = "Hierarchy Settings",
                keywords = new HashSet<string>(new[] { "Nitou, Inspector, Hierarchy" })
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var preferences = HierarchySettingsSO.instance;

            // ※ScriptableSingletonを編集可能にする
            preferences.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;

            _settings = new SerializedObject(preferences);
        }

        /// <summary>
        /// GUI描画．
        /// </summary>
        public override void OnGUI(string searchContext)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10f);
                using (new EditorGUILayout.VerticalScope())
                {
                    using (var changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(_settings.FindProperty("hierarchyObjectMode"));

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Drawer", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(_settings.FindProperty("showHierarchyToggles"), new GUIContent("Show Toggles"));
                        EditorGUILayout.PropertyField(_settings.FindProperty("showComponentIcons"));
                        var showTreeMap = _settings.FindProperty("showTreeMap");
                        EditorGUILayout.PropertyField(showTreeMap);
                        if (showTreeMap.boolValue)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(_settings.FindProperty("treeMapColor"), new GUIContent("Color"));
                            EditorGUI.indentLevel--;
                        }

                        var showSeparator = _settings.FindProperty("showSeparator");
                        EditorGUILayout.PropertyField(showSeparator, new GUIContent("Show Row Separator"));
                        if (showSeparator.boolValue)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(_settings.FindProperty("separatorColor"), new GUIContent("Color"));
                            EditorGUI.indentLevel--;
                            var showRowShading = _settings.FindProperty("showRowShading");
                            EditorGUILayout.PropertyField(showRowShading);
                            if (showRowShading.boolValue)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(_settings.FindProperty("evenRowColor"));
                                EditorGUILayout.PropertyField(_settings.FindProperty("oddRowColor"));
                                EditorGUI.indentLevel--;
                            }
                        }

                        if (changeCheck.changed)
                        {
                            _settings.ApplyModifiedProperties();
                            HierarchySettingsSO.instance.Save();
                        }
                    }
                }
            }
        }
    }
}
#endif