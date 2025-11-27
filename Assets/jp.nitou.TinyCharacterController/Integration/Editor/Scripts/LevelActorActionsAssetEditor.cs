#if UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using Nitou.EditorShared;
using UnityEngine;
using UnityEditor;


namespace Nitou.TCC.Integration.EditorScripts
{
    [CustomEditor(typeof(ActorActionsAsset))]
    internal class LevelActorActionsAssetEditor : Editor
    {
        // ファイル名定数
        private const string ActorActionsFileName = "ActorActions";

        // SerializedProperty
        private SerializedProperty boolActions = null;
        private SerializedProperty floatActions = null;
        private SerializedProperty vector2Actions = null;


        // ----------------------------------------------------------------------------
        // Lifecycle Events

        private void OnEnable()
        {
            boolActions = serializedObject.FindProperty("boolActions");
            floatActions = serializedObject.FindProperty("floatActions");
            vector2Actions = serializedObject.FindProperty("vector2Actions");
        }

        public override void OnInspectorGUI()
        {
            EditorUtil.GUI.ScriptableObjectField((ActorActionsAsset)target);

            serializedObject.Update();

            EditorUtil.GUI.HorizontalLine();
            {
                EditorGUILayout.PropertyField(boolActions, true);
                EditorGUILayout.PropertyField(floatActions, true);
                EditorGUILayout.PropertyField(vector2Actions, true);
            }
            EditorUtil.GUI.HorizontalLine();

            EditorGUILayout.Space();

            // ヘルプボックス
            EditorGUILayout.HelpBox(
                "Click the button to generate the \"ActorActions.cs\" file based on the configured actions. " +
                "This allows you to create custom input actions without modifying the original code.",
                MessageType.Info
            );

            // コード生成ボタン
            if (GUILayout.Button("Generate ActorActions.cs"))
            {
                // 確認ダイアログ
                bool isYes = EditorUtility.DisplayDialog(
                    "Generate ActorActions",
                    "This will create or replace the \"ActorActions.cs\" file. Are you sure you want to continue?",
                    "Yes", "No");

                if (isYes)
                {
                    GenerateActorActionsFile();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        // ----------------------------------------------------------------------------
        // Private Methods

        /// <summary>
        /// ActorActions.csファイルを生成
        /// </summary>
        private void GenerateActorActionsFile()
        {
            // 既存のActorActions.csを探す
            var targetFilePath = AssetDatabase.FindAssets(ActorActionsFileName + " t:script")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => string.Equals(
                    Path.GetFileNameWithoutExtension(path),
                    ActorActionsFileName,
                    StringComparison.CurrentCultureIgnoreCase));

            // ディレクトリパスを決定（既存ファイルのディレクトリ or Assetsフォルダ）
            var directoryPath = !string.IsNullOrEmpty(targetFilePath)
                ? Path.GetDirectoryName(targetFilePath)
                : "Assets";

            // ファイル保存ダイアログ
            var filePath = EditorUtility.SaveFilePanel(
                "Save ActorActions.cs",
                directoryPath,
                ActorActionsFileName,
                "cs");

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("[ActorActionsAssetEditor] File generation cancelled.");
                return;
            }

            // コード生成
            try
            {
                var boolActionsArray = GetStringArray(boolActions);
                var floatActionsArray = GetStringArray(floatActions);
                var vector2ActionsArray = GetStringArray(vector2Actions);

                var generator = new ActorActionsGenerator(
                    boolActionsArray,
                    floatActionsArray,
                    vector2ActionsArray,
                    className: ActorActionsFileName,
                    enableInputBuffer: true  // InputBuffer機能を有効化
                );

                string generatedCode = generator.Generate();

                // ファイルに書き込み
                File.WriteAllText(filePath, generatedCode);

                Debug.Log($"[ActorActionsAssetEditor] Successfully generated: {filePath}");

                // Unityにアセットを再読込させる
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ActorActionsAssetEditor] Failed to generate ActorActions.cs: {ex.Message}");
            }
        }

        /// <summary>
        /// SerializedPropertyから文字列配列を取得
        /// </summary>
        private static string[] GetStringArray(SerializedProperty arrayProperty)
        {
            if (arrayProperty == null || !arrayProperty.isArray)
                return Array.Empty<string>();

            var result = new string[arrayProperty.arraySize];
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                result[i] = arrayProperty.GetArrayElementAtIndex(i).stringValue ?? string.Empty;
            }
            return result;
        }
    }
}

#endif
