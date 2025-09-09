#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace Nitou.IO
{
    public static class EditorPathUtils
    {
        /// <summary>
        /// �I�𒆂̃A�Z�b�g�̃p�X���擾����
        /// </summary>
        public static string GetSelectedAssetPath() =>
            AssetDatabase.GetAssetPath(Selection.activeInstanceID);


        /// --------------------------------------------------------------------

        #region �p�X�̕ϊ��istring�g�����\�b�h�j

        /// <summary>
        /// �A�Z�b�g�p�X���擾����
        /// </summary>
        public static string GetAssetPath(this ScriptableObject scriptableObject)
        {
            var mono = MonoScript.FromScriptableObject(scriptableObject);
            return AssetDatabase.GetAssetPath(mono).Replace("\\", "/");
        }

        /// <summary>
        /// �A�Z�b�g�̐e�t�H���_�p�X���擾����
        /// </summary>
        public static string GetAssetParentFolderPath(this ScriptableObject scriptableObject, int n = 1)
        {
            var filePath = scriptableObject.GetAssetPath();

            return PathUtils.GetParentDirectory(filePath, n);
        }

        #endregion


        /// --------------------------------------------------------------------
        // 

        /// <summary>
        /// �t�H���_�̃A�Z�b�g�p�X���������Ď擾����
        /// </summary>
        public static string GetFolderPath(string folderName, string parentFolderName)
        {
            // ���S�t�@�C����������������Ȃ̂ɒ���
            string[] guids = AssetDatabase.FindAssets(folderName);
            foreach (var guid in guids)
            {
                // �Ώۃt�H���_���
                var folderPath = AssetDatabase.GUIDToAssetPath(guid);

                // �e�t�H���_���
                var parentFolderPath = PathUtils.GetDirectoryName(folderPath);
                var parentFolder = PathUtils.GetFileName(parentFolderPath);

                // �e�t�H���_�܂ň�v���Ă���Ȃ�C�m��Ƃ���
                if (parentFolder == parentFolderName)
                {
                    return folderPath;
                }
            }

            return "";
        }
    }
}
#endif