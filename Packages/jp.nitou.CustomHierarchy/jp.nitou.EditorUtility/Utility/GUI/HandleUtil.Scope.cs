#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Nitou.EditorShared
{
    public static partial class HandleUtil
    {
        #region Color Scope

        /// <summary>
        /// Handles.color設定をスコープで管理するためのクラス
        /// </summary>
        public sealed class ColorScope : System.IDisposable
        {
            private readonly Color _oldColor;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public ColorScope(Color color)
            {
                _oldColor = Handles.color;
                Handles.color = color;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            public void Dispose()
            {
                Handles.color = _oldColor;
            }
        }

        #endregion
    }
}
#endif