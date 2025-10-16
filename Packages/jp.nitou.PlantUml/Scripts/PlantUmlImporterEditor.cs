using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Nitou.PlantUml
{
    [CustomEditor(typeof(PlantUmlImporter))]
    internal class PlantUmlImporterEditor : ScriptedImporterEditor
    {
        // プレビューサイズの設定
        private const float MAX_PREVIEW_WIDTH = 400f;
        private const float MAX_PREVIEW_HEIGHT = 300f;
        private const float MIN_PREVIEW_SIZE = 100f;

        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private Texture2D _previewTexture;

        // プレビュー表示モード
        private enum PreviewMode
        {
            FitToBox, // ボックス内に収める（アスペクト比保持）
            FixedSize, // 固定サイズ
            FixedAspectRatio, // 固定アスペクト比
            ScaleToWidth // 幅に合わせてスケール
        }

        private PreviewMode _previewMode = PreviewMode.FitToBox;
        private float _previewScale = 1.0f;

        public override void OnInspectorGUI()
        {
            // デフォルトのインスペクタ表示
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PlantUML Operations", EditorStyles.boldLabel);

            // .pumlファイルの内容を読み込む
            string assetPath = AssetDatabase.GetAssetPath(target);
            if (File.Exists(assetPath))
            {
                string content = File.ReadAllText(assetPath);

                // ボタン類
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Generate Diagram", GUILayout.Height(30)))
                {
                    GenerateDiagram(content);
                }

                if (GUILayout.Button("Open in Browser", GUILayout.Height(30)))
                {
                    OpenInBrowser(content);
                }

                if (GUILayout.Button("Open in External Editor", GUILayout.Height(30)))
                {
                    OpenInExternalEditor(assetPath);
                }

                EditorGUILayout.EndHorizontal();

                // エラーメッセージ表示
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
                }

                // ローディング表示
                if (_isLoading)
                {
                    EditorGUILayout.HelpBox("Generating diagram...", MessageType.Info);
                }

                // プレビュー表示
                if (_previewTexture != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

                    // プレビュー設定
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Display Mode:", GUILayout.Width(100));
                    _previewMode = (PreviewMode)EditorGUILayout.EnumPopup(_previewMode, GUILayout.Width(150));

                    if (_previewMode != PreviewMode.FixedSize && _previewMode != PreviewMode.FixedAspectRatio)
                    {
                        EditorGUILayout.LabelField("Scale:", GUILayout.Width(45));
                        _previewScale = EditorGUILayout.Slider(_previewScale, 0.1f, 2.0f);
                    }

                    EditorGUILayout.EndHorizontal();

                    // プレビュー表示
                    DrawPreview();

                    // 画像情報表示
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Original Size: {_previewTexture.width} x {_previewTexture.height}",
                        EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawPreview()
        {
            if (_previewTexture == null) return;

            float displayWidth, displayHeight;

            switch (_previewMode)
            {
                case PreviewMode.FitToBox:
                    (displayWidth, displayHeight) = CalculateFitToBox(
                        _previewTexture.width, _previewTexture.height,
                        MAX_PREVIEW_WIDTH * _previewScale,
                        MAX_PREVIEW_HEIGHT * _previewScale);
                    break;

                case PreviewMode.FixedSize:
                    displayWidth = MAX_PREVIEW_WIDTH;
                    displayHeight = MAX_PREVIEW_HEIGHT;
                    break;

                case PreviewMode.FixedAspectRatio:
                    // 16:9のアスペクト比で固定
                    displayWidth = MAX_PREVIEW_WIDTH;
                    displayHeight = MAX_PREVIEW_WIDTH * 9f / 16f;
                    break;

                case PreviewMode.ScaleToWidth:
                    float maxWidth = Mathf.Min(EditorGUIUtility.currentViewWidth - 40, MAX_PREVIEW_WIDTH * _previewScale);
                    float aspectRatio = (float)_previewTexture.height / _previewTexture.width;
                    displayWidth = maxWidth;
                    displayHeight = displayWidth * aspectRatio;
                    break;

                default:
                    displayWidth = _previewTexture.width;
                    displayHeight = _previewTexture.height;
                    break;
            }

            // 最小サイズを保証
            displayWidth = Mathf.Max(displayWidth, MIN_PREVIEW_SIZE);
            displayHeight = Mathf.Max(displayHeight, MIN_PREVIEW_SIZE);

            // センタリング用のレイアウト
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // プレビューの背景（オプション）
            EditorGUI.DrawRect(
                new Rect(GUILayoutUtility.GetRect(displayWidth + 4, displayHeight + 4)),
                new Color(0.2f, 0.2f, 0.2f, 0.3f));

            // プレビュー画像描画
            Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight);

            if (_previewMode == PreviewMode.FixedSize || _previewMode == PreviewMode.FixedAspectRatio)
            {
                // ScaleAndCropモードで描画（画像を枠に合わせて切り抜き）
                GUI.DrawTextureWithTexCoords(rect, _previewTexture,
                    CalculateTexCoords(_previewTexture, displayWidth, displayHeight));
            }
            else
            {
                // 通常の描画（アスペクト比保持）
                EditorGUI.DrawPreviewTexture(rect, _previewTexture);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ボックス内に収めるためのサイズ計算
        private (float width, float height) CalculateFitToBox(float originalWidth, float originalHeight, float maxWidth, float maxHeight)
        {
            float aspectRatio = originalHeight / originalWidth;
            float displayWidth = originalWidth;
            float displayHeight = originalHeight;

            // 幅が制限を超える場合
            if (displayWidth > maxWidth)
            {
                displayWidth = maxWidth;
                displayHeight = displayWidth * aspectRatio;
            }

            // 高さが制限を超える場合
            if (displayHeight > maxHeight)
            {
                displayHeight = maxHeight;
                displayWidth = displayHeight / aspectRatio;
            }

            return (displayWidth, displayHeight);
        }

        // 固定サイズ/アスペクト比での表示用のTexCoords計算
        private Rect CalculateTexCoords(Texture2D texture, float displayWidth, float displayHeight)
        {
            float textureAspect = (float)texture.width / texture.height;
            float displayAspect = displayWidth / displayHeight;

            float u = 0, v = 0, width = 1, height = 1;

            if (textureAspect > displayAspect)
            {
                // テクスチャが横長：上下をクロップ
                float scale = displayAspect / textureAspect;
                v = (1 - scale) * 0.5f;
                height = scale;
            }
            else
            {
                // テクスチャが縦長：左右をクロップ
                float scale = textureAspect / displayAspect;
                u = (1 - scale) * 0.5f;
                width = scale;
            }

            return new Rect(u, v, width, height);
        }

        private void GenerateDiagram(string content)
        {
            _isLoading = true;
            _errorMessage = "";

            string encodedContent = EncodePlantUML(content);
            string url = $"http://www.plantuml.com/plantuml/png/{encodedContent}";

            EditorApplication.delayCall += () => DownloadDiagram(url);
        }

        private void DownloadDiagram(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] imageData = client.DownloadData(url);

                    if (_previewTexture != null)
                    {
                        DestroyImmediate(_previewTexture);
                    }

                    _previewTexture = new Texture2D(2, 2);
                    if (!_previewTexture.LoadImage(imageData))
                    {
                        _errorMessage = "Failed to load image data";
                    }
                }
            }
            catch (System.Exception e)
            {
                _errorMessage = $"Error: {e.Message}";
            }
            finally
            {
                _isLoading = false;
                Repaint();
            }
        }

        private void OpenInBrowser(string content)
        {
            string encodedContent = EncodePlantUML(content);
            string url = $"http://www.plantuml.com/plantuml/uml/{encodedContent}";
            Application.OpenURL(url);
        }

        private void OpenInExternalEditor(string path)
        {
            EditorUtility.OpenWithDefaultApp(path);
        }

        // PlantUMLのエンコード処理
        private string EncodePlantUML(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (var compressed = new MemoryStream())
            {
                using (var deflate = new System.IO.Compression.DeflateStream(
                           compressed,
                           System.IO.Compression.CompressionMode.Compress))
                {
                    deflate.Write(data, 0, data.Length);
                }

                byte[] compressedData = compressed.ToArray();
                return EncodePlantUMLBase64(compressedData);
            }
        }

        private string EncodePlantUMLBase64(byte[] data)
        {
            var result = new StringBuilder();

            for (int i = 0; i < data.Length; i += 3)
            {
                if (i + 2 < data.Length)
                {
                    result.Append(Encode6bit((data[i] >> 2) & 0x3F));
                    result.Append(Encode6bit(((data[i] & 0x3) << 4) | ((data[i + 1] >> 4) & 0xF)));
                    result.Append(Encode6bit(((data[i + 1] & 0xF) << 2) | ((data[i + 2] >> 6) & 0x3)));
                    result.Append(Encode6bit(data[i + 2] & 0x3F));
                }
                else if (i + 1 < data.Length)
                {
                    result.Append(Encode6bit((data[i] >> 2) & 0x3F));
                    result.Append(Encode6bit(((data[i] & 0x3) << 4) | ((data[i + 1] >> 4) & 0xF)));
                    result.Append(Encode6bit((data[i + 1] & 0xF) << 2));
                }
                else
                {
                    result.Append(Encode6bit((data[i] >> 2) & 0x3F));
                    result.Append(Encode6bit((data[i] & 0x3) << 4));
                }
            }

            return result.ToString();
        }

        private char Encode6bit(int b)
        {
            if (b < 10) return (char)(b + '0');
            if (b < 36) return (char)(b - 10 + 'A');
            if (b < 62) return (char)(b - 36 + 'a');
            if (b == 62) return '-';
            if (b == 63) return '_';
            return '?';
        }
    }
}