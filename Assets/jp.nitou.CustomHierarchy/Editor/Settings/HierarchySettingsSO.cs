#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Nitou.CustomHierarchy.EditorSctipts
{
    /// <summary>
    /// Editorで参照するプロジェクト固有の設定データ
    /// </summary>
    [FilePath(
        "ProjectSettings/HierarchySettingsSO.asset",
        FilePathAttribute.Location.ProjectFolder
    )]
    internal sealed class HierarchySettingsSO : ScriptableSingleton<HierarchySettingsSO>
    {
        [SerializeField] private HierarchyObjectMode hierarchyObjectMode = HierarchyObjectMode.RemoveInBuild;
        [SerializeField] private bool showHierarchyToggles;
        [SerializeField] private bool showComponentIcons;
        [SerializeField] private bool showTreeMap;
        [SerializeField] private Color treeMapColor = new(0.53f, 0.53f, 0.53f, 0.45f);
        [SerializeField] private bool showSeparator;
        [SerializeField] private bool showRowShading;
        [SerializeField] private Color separatorColor = new(0.19f, 0.19f, 0.19f, 0f);
        [SerializeField] private Color evenRowColor = new(0f, 0f, 0f, 0.07f);
        [SerializeField] private Color oddRowColor = Color.clear;

        // Property
        public HierarchyObjectMode HierarchyObjectMode => hierarchyObjectMode;
        public bool ShowHierarchyToggles => showHierarchyToggles;
        public bool ShowComponentIcons => showComponentIcons;
        public bool ShowTreeMap => showTreeMap;
        public Color TreeMapColor => treeMapColor;
        public bool ShowSeparator => showSeparator;
        public bool ShowRowShading => showRowShading;
        public Color SeparatorColor => separatorColor;
        public Color EvenRowColor => evenRowColor;
        public Color OddRowColor => oddRowColor;

        /// <summary>
        /// データを保存する
        /// </summary>
        public void Save() => Save(true);
    }
}
#endif