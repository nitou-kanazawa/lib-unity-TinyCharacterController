using System;
using UnityEngine;

namespace Nitou.IO {

    /// <summary>
    /// UPM�p�̎���p�b�P�[�W�̃f�B���N�g���p�X�w��p�̃N���X�D
    /// </summary>
    public sealed class PackageDirectoryPath {

        // [NOTE] �f�B���N�g���͊J������"Assets/"�ȉ��ɁC�z�z���"Packages/"�ȉ��ɂ�����̂Ƒz�肷��D

        public enum Mode {
            // �z�z��
            Upm,
            // �J���v���W�F�N�g��
            Normal,
            // 
            NotExist,
        }

        // ���΃p�X
        private readonly string _upmRelativePath;
        private readonly string _normalRelativePath;

        private readonly Mode _mode;


        /// <summary>
        /// Package�z�z��̃p�b�P�[�W�p�X
        /// </summary>
        public string UpmPath => $"Packages/{_upmRelativePath}".ReplaceDelimiter();

        /// <summary>
        /// �J���v���W�F�N�g�ł̃A�Z�b�g�p�X
        /// </summary>
        public string NormalPath => $"Assets/{_normalRelativePath}".ReplaceDelimiter();


        // ----------------------------------------------------------------------------
        // Pubic Method

        /// <summary>
        /// �R���X�g���N�^�D
        /// </summary>
        public PackageDirectoryPath(string relativePath = "com.nitou.nLib") 
            : this(relativePath, relativePath) {}

        /// <summary>
        /// �R���X�g���N�^�D
        /// </summary>
        public PackageDirectoryPath(string upmRelativePath = "com.nitou.nLib", string normalRelativePath = "Plugins/NLib") {
            _upmRelativePath = upmRelativePath ?? throw new ArgumentNullException(nameof(upmRelativePath));
            _normalRelativePath = normalRelativePath ?? throw new ArgumentNullException(nameof(normalRelativePath)); ;

            // ���݂̃p�X�𔻒肷��
            _mode = CheckDirectoryLocation();
        }


        // ----------------------------------------------------------------------------
        // Pubic Method

        /// <summary>
        /// Project�f�B���N�g�����N�_�Ƃ����p�X�D
        /// </summary>
        public string ToProjectPath() {
            return _mode switch {
                Mode.Upm => UpmPath,
                Mode.Normal => NormalPath,
                _ => ""
            };
        }

        /// <summary>
        /// ��΃p�X�D
        /// </summary>
        public string ToAbsolutePath() => PathUtils.GetFullPath(ToProjectPath());


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// �f�B���N�g���̈ʒu�𔻒肷��D
        /// </summary>
        private Mode CheckDirectoryLocation() {

            if (DirectoryUtils.Exists(UpmPath)) return Mode.Upm;
            if (DirectoryUtils.Exists(NormalPath)) return Mode.Normal;

            Debug.LogError($"Directory not found in both UPM and normal paths: \n" +
                    $"  [{UpmPath}] and \n" +
                    $"  [{NormalPath}]");
            return Mode.NotExist;
        }
    }
}
