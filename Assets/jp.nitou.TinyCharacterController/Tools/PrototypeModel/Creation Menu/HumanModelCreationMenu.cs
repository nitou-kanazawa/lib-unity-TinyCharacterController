#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Nitou.TCC.Tools {

    /// <summary>
    /// �v���~�e�B�u�`��Ɋւ��郁�j���[�R�}���h
    /// </summary>
    public static class HumanModelCreationMenu {

        private const string CREATE_HUMAN_MODEL = "GameObject/3D Object/Human/";


        #region Public Method (Lowpoly Mesh)

        [MenuItem(CREATE_HUMAN_MODEL + "Runner")]
        public static void Create_Runner(MenuCommand menuCommand) => CreateGameObject(menuCommand, HumanType.Runner);

        [MenuItem(CREATE_HUMAN_MODEL + "Warrior")]
        public static void Create_Warrior(MenuCommand menuCommand) => CreateGameObject(menuCommand, HumanType.Warrior);

        [MenuItem(CREATE_HUMAN_MODEL + "SD")]
        public static void Create_SD(MenuCommand menuCommand) => CreateGameObject(menuCommand, HumanType.SD);
        #endregion



        #region Private Method

        /// <summary>
        /// ���f���̃C���X�^���X�𐶐�����
        /// </summary>
        private static void CreateGameObject(MenuCommand menuCommand, HumanType type) {

            if (HumanModelDatabase.Instance.TryGetPrefab(type, out var meshObj)) {

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
        #endregion 
        
    }
}
#endif