#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// REF:
//  - qiita: Unity5のGUIクラスに追加されたScopeについて https://qiita.com/kyusyukeigo/items/4642ae85d6ff075acf31
//  - hatena: EditorWindowで使えるScope一覧 https://hacchi-man.hatenablog.com/entry/2019/12/20/002444

namespace Nitou.EditorShared
{
    public static partial class EditorUtil
    {
        // ----------------------------------------------------------------------------

        #region Color Scope

        /// <summary>
        /// GUI.color設定をスコープで管理するためのクラス
        /// </summary>
        public sealed class GUIColorScope : UnityEngine.GUI.Scope
        {
            private readonly Color _oldColor;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public GUIColorScope(Color color)
            {
                _oldColor = UnityEngine.GUI.color;
                UnityEngine.GUI.color = color;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected override void CloseScope()
            {
                UnityEngine.GUI.color = _oldColor;
            }
        }


        /// <summary>
        /// GUI.backgroundColorを設定をスコープで管理するためのクラス
        /// </summary>
        public sealed class GUIBackgroundColorScope : UnityEngine.GUI.Scope
        {
            private readonly Color _oldColor;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public GUIBackgroundColorScope(Color color)
            {
                _oldColor = UnityEngine.GUI.backgroundColor;
                UnityEngine.GUI.backgroundColor = color;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected override void CloseScope()
            {
                UnityEngine.GUI.backgroundColor = _oldColor;
            }
        }


        /// <summary>
        /// GUI.backgroundColorを設定をスコープで管理するためのクラス
        /// </summary>
        public sealed class GUIContentColorScope : UnityEngine.GUI.Scope
        {
            private readonly Color _oldColor;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public GUIContentColorScope(Color color)
            {
                _oldColor = UnityEngine.GUI.contentColor;
                UnityEngine.GUI.contentColor = color;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected override void CloseScope()
            {
                UnityEngine.GUI.contentColor = _oldColor;
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Indent Scope

        /// <summary>
        /// インデント設定をスコープで管理するためのクラス
        /// </summary>
        public sealed class IndentScope : UnityEngine.GUI.Scope
        {
            private readonly int _oldIndentLevel;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public IndentScope()
            {
                _oldIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel++; // ※インデントをひとつ上げる
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public IndentScope(int indentLevel)
            {
                _oldIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = indentLevel;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected override void CloseScope()
            {
                EditorGUI.indentLevel = _oldIndentLevel;
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Condition

        /// <summary>
        /// 
        /// </summary>
        public sealed class EnableScope : UnityEngine.GUI.Scope
        {
            private readonly bool _oldEnabled;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public EnableScope(bool enabled)
            {
                _oldEnabled = UnityEngine.GUI.enabled;
                UnityEngine.GUI.enabled = enabled;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected override void CloseScope()
            {
                UnityEngine.GUI.enabled = _oldEnabled;
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Misc

        /// <summary>
        /// 同次変換行列を設定するスコープ
        /// </summary>
        public class RotateScope : UnityEngine.GUI.Scope
        {
            private readonly Matrix4x4 _oldMatrix;

            public RotateScope(float angle, Vector2 pivotPoint)
            {
                _oldMatrix = UnityEngine.GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, pivotPoint);
            }

            protected override void CloseScope()
            {
                UnityEngine.GUI.matrix = _oldMatrix;
            }
        }

        #endregion
    }
}
#endif