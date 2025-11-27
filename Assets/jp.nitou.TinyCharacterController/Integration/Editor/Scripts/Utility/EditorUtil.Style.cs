#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// [参考]
//  hatena: EditorWindow で GUIStyle を使う際の注意 https://hacchi-man.hatenablog.com/entry/2020/03/17/220000
//  hatena: 色別の GUIStyle をキャッシュするクラス https://hacchi-man.hatenablog.com/entry/2020/08/16/220000

namespace Nitou.EditorShared {
    public static partial class EditorUtil {

        /// <summary>
        /// Editor拡張で使用する汎用的な<see cref="GUIStyle"/>のライブラリ
        /// </summary>
        public static partial class Styles {

            // Folder
            public static GUIStyle folderHeader;
            public static GUIStyle folderToggleHeader;
            public static GUIStyle headerCheckbox;

            // 定数
            private const float HeadingSpace = 22.0f;

            // Text
            public static GUIStyle textArea;


            /// <summary>
            /// コンストラクタ
            /// </summary>
            static Styles(){
                
                folderHeader = new GUIStyle("ShurikenModuleTitle") {
                    font = new GUIStyle(EditorStyles.label).font,
                    fontSize = 12,
                    border = new RectOffset(15, 7, 4, 4),
                    fixedHeight = HeadingSpace,
                    contentOffset = new Vector2(20f, -2f),
                };

                folderToggleHeader = new GUIStyle("ShurikenEmitterTitle") {
                    font = new GUIStyle(EditorStyles.label).font,
                    fontSize = 12,
                    border = new RectOffset(15, 7, 4, 4),
                    fixedHeight = HeadingSpace,
                    contentOffset = new Vector2(20f, -2f),
                };

                headerCheckbox = new GUIStyle("ShurikenCheckMark");

                // テキスト
                textArea = new GUIStyle(EditorStyles.textArea) {
                    wordWrap = false,
                };
            }

            public static GUIStyle XmlText() {
                return new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.UpperLeft,
                    richText = true,
                };
            }




        }

    }
}

#endif