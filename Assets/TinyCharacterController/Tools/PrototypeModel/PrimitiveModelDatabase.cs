#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Nitou.TCC.Tools
{
    public enum PrimitiveType
    {
        // Lowpoly Meshs
        LowpolySphere,
        LowpolyCapsule,
        LowpolyCylinder,
        LowpolyCone,

        // Reverse Meshs
        ReverseCube,
        ReverseSphere,
        ReverseCylinder,
    }

    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(
        fileName = "PrimitiveModelDatabase",
        menuName = "Scriptable Objects/" + "Editor/Primitive Mesh List"
    )]
    internal sealed class PrimitiveModelDatabase : SingletonScriptableObject<PrimitiveModelDatabase>, IModelDatabase
    {
        /// ----------------------------------------------------------------------------

        #region Field

        [FoldoutGroup("Lowpoly"), Indent] [LabelText("Sphere")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _lowpolySphere;

        [FoldoutGroup("Lowpoly"), Indent] [LabelText("Capsule")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _lowpolyCapsule;

        [FoldoutGroup("Lowpoly"), Indent] [LabelText("Cylinder")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _lowpolyCylinder;

        [FoldoutGroup("Lowpoly"), Indent] [LabelText("Cone")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _lowpolyCone;


        [FoldoutGroup("Reverse"), Indent] [LabelText("Cube")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _ReverseCube;

        [FoldoutGroup("Reverse"), Indent] [LabelText("Sphere")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _ReverseSphere;

        [FoldoutGroup("Reverse"), Indent] [LabelText("Cylinder")] [PreviewField, AssetsOnly] [SerializeField]
        private GameObject _ReverseCylinder;

        #endregion


        #region Public Method

        /// <summary>
        /// �w�肵����ނ̃��f�����擾����
        /// </summary>
        public bool TryGetPrefab(PrimitiveType type, out GameObject meshPrefab)
        {
            meshPrefab = type switch
            {
                // Lowpoly Meshs
                PrimitiveType.LowpolySphere => _lowpolySphere,
                PrimitiveType.LowpolyCapsule => _lowpolyCapsule,
                PrimitiveType.LowpolyCylinder => _lowpolyCylinder,
                PrimitiveType.LowpolyCone => _lowpolyCone,

                // Reverse Meshs
                PrimitiveType.ReverseCube => _ReverseCube,
                PrimitiveType.ReverseSphere => _ReverseSphere,
                PrimitiveType.ReverseCylinder => _ReverseCylinder,
                _ => null
            };

            return meshPrefab != null;
        }
        #endregion
    }
}
#endif