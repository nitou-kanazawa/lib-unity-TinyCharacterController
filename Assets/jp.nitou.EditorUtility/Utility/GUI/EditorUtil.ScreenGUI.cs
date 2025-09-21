#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// REF:
//  - hatena: Editor拡張で使えるUI機能の概要 (GUI / GUILayout / EditorGUI / EditorGUILayout) https://yotiky.hatenablog.com/entry/unity_editorextension-guisummary
//  - zenn: UnityでコードからGUIを作る https://zenn.dev/kumas/books/325ed71592f6f5/viewer/0f1174
//  - LIGHT11: エディタ拡張チートシート https://light11.hatenadiary.com/entry/2018/07/08/134405
//  - qiita: エディタ拡張で仕切り線を描く https://qiita.com/Gok/items/96e8747269bf4a2a9cc5

namespace Nitou.EditorShared
{
    public static partial class EditorUtil
    {
        /// <summary>
        /// スクリーン座標指定でGameWindow上にGUIを描画するためのUtilクラス
        /// </summary>
        public static class ScreenGUI
        {
            private static readonly Texture2D _lineTexture;

            /// <summary>
            /// コンストラクタ．
            /// </summary>
            static ScreenGUI()
            {
                _lineTexture = new Texture2D(1, 1);
            }


            #region Basic Method

            /// <summary>
            /// スクリーン座標を指定してボックスを表示する．
            /// </summary>
            public static void Box(Rect screenRect)
            {
                UnityEngine.GUI.Box(Convetor.ScreenToGUI(screenRect), "");
            }

            /// <summary>
            /// スクリーン座標を指定して補助線を描画する．
            /// </summary>
            public static void AuxiliaryLine(Vector2 position, float width, Color color)
            {
                // Rect
                var horizontalRect = new Rect(0f, position.y, position.x, width);
                var verticalRect = new Rect(position.x, 0f, width, position.y);

                using (new EditorUtil.GUIColorScope(color))
                {
                    UnityEngine.GUI.DrawTexture(Convetor.ScreenToGUI(horizontalRect), _lineTexture);
                    UnityEngine.GUI.DrawTexture(Convetor.ScreenToGUI(verticalRect), _lineTexture);
                }
            }

            /// <summary>
            /// スクリーン座標を指定して線分を描画する．
            /// </summary>
            public static void Line(Vector2 screenPos1, Vector2 screenPos2, Color color, float width = 5f)
            {
                // スクリーン座標をGUI座標に変換
                var p1 = Convetor.ScreenToGUI(screenPos1);
                var p2 = Convetor.ScreenToGUI(screenPos2);

                // 線幅設定（線幅に対応するのはエディタのHandlesのみ）
                using (new HandleUtil.ColorScope(color))
                {
                    Handles.DrawAAPolyLine(width, p1, p2);
                }
            }

            /// <summary>
            /// スクリーン座標を指定して線分を描画する．
            /// </summary>
            public static void Outline(Rect screenRect, Color color, float width = 5f)
            {
                // スクリーン座標の矩形をGUI座標に変換
                var points = new Vector3[]
                {
                    Convetor.ScreenToGUI(new Vector2(screenRect.xMin, screenRect.yMin)),
                    Convetor.ScreenToGUI(new Vector2(screenRect.xMin, screenRect.yMax)),
                    Convetor.ScreenToGUI(new Vector2(screenRect.xMax, screenRect.yMax)),
                    Convetor.ScreenToGUI(new Vector2(screenRect.xMax, screenRect.yMin)),
                    Convetor.ScreenToGUI(new Vector2(screenRect.xMin, screenRect.yMin)),
                };

                // 矩形の各辺に線を描画
                using (new HandleUtil.ColorScope(color))
                {
                    Handles.DrawAAPolyLine(width, points);
                }
            }

            #endregion


            #region Label

            /// <summary>
            /// GUI.Label()のラップメソッド
            /// </summary>
            public static void Label(Vector2 screenPos, string text = "",
                                     int fontSize = 20, TextAnchor alignment = TextAnchor.LowerLeft, Color? color = null)
            {
                Style.label.fontSize = fontSize;
                Style.label.alignment = alignment;
                Style.label.normal.textColor = color ?? Color.white;

                // 描画範囲
                var size = Style.label.CalcSize(new GUIContent(text));
                var rect = new Rect(screenPos - size, size * 2f);

                // デバッグ
                //UnityEngine.GUI.Box(Convetor.ScreenToGUI(rect), "");

                // ラベル描画
                UnityEngine.GUI.Label(Convetor.ScreenToGUI(rect), text, Style.label);
            }

            #endregion


            private static class Convetor
            {
                /// <summary>
                /// スクリーン座標からGUI座標への変換．
                /// </summary>
                public static Rect ScreenToGUI(Rect screenRect)
                {
                    // Screen座標系
                    var screenPos = screenRect.position;

                    // GUI座標系
                    var guiPos = ScreenToGUI(screenPos);
                    var guiRect = new Rect(guiPos + (Vector2.down * screenRect.height), screenRect.size);

                    return guiRect;
                }

                /// <summary>
                /// スクリーン座標からGUI座標への変換．
                /// </summary>
                public static Vector2 ScreenToGUI(Vector2 screenPos)
                {
                    return new Vector2(screenPos.x, Screen.height - screenPos.y);
                }
            }

            private static class Style
            {
                // キャッシュ
                public readonly static GUIStyle borderLine;
                public readonly static GUIStyle label;
                public readonly static GUIStyle box;

                static Style()
                {
                    // 補助線
                    borderLine = new GUIStyle(UnityEngine.GUI.skin.box);
                    borderLine.normal.background = Texture2D.blackTexture;
                    borderLine.normal.textColor = Color.clear; // ※背景を透明にする

                    // ラベル
                    label = new GUIStyle(UnityEngine.GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerCenter,
                        fontSize = 10
                    };

                    // ボックス
                    box = new GUIStyle(UnityEngine.GUI.skin.box);
                    box.normal.background = Texture2D.grayTexture;
                }
            }
        }
    }
}
#endif