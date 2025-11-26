#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

//  [REF]
//  �R�K�l�u���O: MenuItem��Hierarchy��Create���j���[�����삷�鎞�̂���@ https://baba-s.hatenablog.com/entry/2022/09/10/192926_2
//  qiita: ScriptableObject ���p�X�w��ł͂Ȃ��������Ď擾���� https://qiita.com/Toshizabeth/items/b76c615fd979475bfb6d

namespace Nitou.TCC.Tools {

    /// <summary>
    /// �v���~�e�B�u�`��Ɋւ��郁�j���[�R�}���h
    /// </summary>
    public static class PrimitiveModelCreationMenu {

        private const string CREATE_LOWPOLY_MODEL = "GameObject/3D Object/LowPoly/";
        private const string CREATE_REVERSE_MODEL = "GameObject/3D Object/Reverse/";


        /// ----------------------------------------------------------------------------
        // Public Method (Lowpoly Mesh)

        [MenuItem(CREATE_LOWPOLY_MODEL + "Sphere")]
        public static void Create_LowpolySphere(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.LowpolySphere);

        [MenuItem(CREATE_LOWPOLY_MODEL + "Capsule")]
        public static void Create_LowpolyCapsule(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.LowpolyCapsule);

        [MenuItem(CREATE_LOWPOLY_MODEL + "Cylinder")]
        public static void Create_LowpolyCylinder(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.LowpolyCylinder);

        [MenuItem(CREATE_LOWPOLY_MODEL + "Cone")]
        public static void Create_LowpolyCone(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.LowpolyCone);


        /// ----------------------------------------------------------------------------
        // Public Method (Reverse Mesh)

        [MenuItem(CREATE_REVERSE_MODEL + "Cube")]
        public static void Create_ReverseCube(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.ReverseCube);

        [MenuItem(CREATE_REVERSE_MODEL + "Sphere")]
        public static void Create_ReverseSphere(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.ReverseSphere);

        [MenuItem(CREATE_REVERSE_MODEL + "Cylinder")]
        public static void Create_ReverseCylinder(MenuCommand menuCommand) => CreateGameObject(menuCommand, PrimitiveType.ReverseCylinder);


        /// ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// ���f���̃C���X�^���X�𐶐�����
        /// </summary>
        private static void CreateGameObject(MenuCommand menuCommand, PrimitiveType type)
        {
            if (!PrimitiveModelDatabase.Instance.TryGetPrefab(type, out var meshObj))
                return;
            
            // �I�u�W�F�N�g����
            var gameObject = GameObject.Instantiate(meshObj);
            gameObject.name = type.ToString();

            // 
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);

            // Undo�ݒ�
            Undo.RegisterCreatedObjectUndo(gameObject, gameObject.name);

            // �I����Ԃɐݒ�
            Selection.activeObject = gameObject;
        }
    }
}
#endif