#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Tools
{
    /// <summary>
    /// �v���~�e�B�u�`��̃��b�V���v���n�u���Ǘ�����f�[�^�x�[�X
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnviormentModelDatabase",
        menuName = "Scriptable Objects/" + "Editor/Enviorment Mesh List"
    )]
    internal class EnviormentModelDatabase : SingletonScriptableObject<EnviormentModelDatabase>
    {
        [FoldoutGroup("Basic"), Indent] [LabelText("Floor")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _floor;

        [FoldoutGroup("Basic"), Indent] [LabelText("wall")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _wall;
    }
}
#endif