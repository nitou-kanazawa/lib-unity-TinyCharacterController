using UnityEngine;

namespace Nitou.CustomHierarchy
{
    /// <summary>
    /// ヒエラルキーにヘッダーを描画するコンポーネント．
    /// </summary>
    [AddComponentMenu("Nitou/Hierarchy Header")]
    public sealed class HierarchyHeader : HierarchyObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Color of the Hierarchy displayed in the Inspector.
        /// </summary>
        [ColorUsage(false)] [SerializeField] private Color _menuColor = Color.white;
#endif
    }
}