using UnityEngine;

namespace Nitou.CustomHierarchy
{
    /// <summary>
    /// ヒエラルキー拡張用のダミーオブジェクト．
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Nitou/Hierarchy Object")]
    public class HierarchyObject : MonoBehaviour
    {
        public enum Mode
        {
            UseSettings = 0,
            None = 1,
            RemoveInPlayMode = 2,
            RemoveInBuild = 3
        }

        [SerializeField] Mode _hierarchyObjectMode = Mode.UseSettings;

        /// <summary>
        /// モード
        /// </summary>
        public Mode HierarchyObjectMode => _hierarchyObjectMode;
    }
}