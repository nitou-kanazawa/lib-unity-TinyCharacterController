#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Nitou.EditorShared
{
    public static partial class EditorUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Icons
        {
            public static readonly GUIContent scriptIcon = EditorGUIUtility.IconContent("cs Script Icon");
        }
    }
}
#endif