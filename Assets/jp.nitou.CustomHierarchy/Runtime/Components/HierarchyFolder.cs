using System.Collections.Generic;
using UnityEngine;

namespace Nitou.CustomHierarchy
{
    /// <summary>
    /// ヒエラルキーにフォルダを描画するコンポーネント．
    /// </summary>
    [AddComponentMenu("Nitou/Hierarchy Folder")]
    public sealed class HierarchyFolder : HierarchyObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// UI comment.
        /// </summary>
        [Multiline(15)] [SerializeField] string _comment;

        /// <summary>
        /// Color of the Hierarchy displayed in the Inspector.
        /// </summary>
        [ColorUsage(false)] [SerializeField] Color _menuColor = Color.white;

        /// <summary>
        /// Display objects below the GameObjectFolder.
        /// </summary>
        [SerializeField] bool _isVisible = false;

        public List<GameObject> ChildObjects = new();

        private void Reset()
        {
            // Update the object's position when it is created.
            transform.position = Vector3.zero;
            transform.hideFlags = HideFlags.HideInInspector;
        }
#endif
    }
}