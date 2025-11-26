#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Tools
{
    public enum HumanType
    {
        Runner,
        Warrior,
        SD,
    }

    /// <summary>
    /// �l�^���b�V���v���n�u���Ǘ�����f�[�^�x�[�X
    /// </summary>
    [CreateAssetMenu(
        fileName = "HumanModelDatabase",
        menuName = "Scriptable Objects/" + "Editor/Human Mesh List"
    )]
    internal sealed class HumanModelDatabase : SingletonScriptableObject<HumanModelDatabase>, IModelDatabase
    {
        [FoldoutGroup("Male"), Indent] [LabelText("Runner")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _runner;

        [FoldoutGroup("Male"), Indent] [LabelText("Warrior")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _warrior;

        [FoldoutGroup("Female"), Indent] [LabelText("SD")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _sd;


        #region Public Method

        /// <summary>
        /// �w�肵����ނ̃��f�����擾����
        /// </summary>
        public bool TryGetPrefab(HumanType type, out GameObject meshPrefab)
        {
            meshPrefab = type switch
            {
                // Male Meshs
                HumanType.Runner => _runner,
                HumanType.Warrior => _warrior,
                HumanType.SD => _sd,
                _ => null
            };

            return meshPrefab != null;
        }

        #endregion
    }
}
#endif