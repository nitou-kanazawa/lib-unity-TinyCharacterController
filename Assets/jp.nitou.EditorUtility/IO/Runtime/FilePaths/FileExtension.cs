using System;

namespace Nitou.IO {

    /// <summary>
    /// �t�@�C���g���q��\���N���X
    /// </summary>
    public sealed class FileExtension : IEquatable<FileExtension>{

        /// <summary>
        /// �g���q�̕�����i�s���I�h�t���j�D
        /// </summary>
        public string Extension { get; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// �R���X�g���N�^�D
        /// </summary>
        private FileExtension(string extension) {
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));

            if (!extension.StartsWith("."))
                throw new ArgumentException("Extension must start with a dot ('.').", nameof(extension));

            Extension = extension.ToLowerInvariant();
        }

        /// <summary>
        /// �g���q����v���Ă��邩���肵�܂�
        /// </summary>
        public override bool Equals(object obj) {
            return obj is FileExtension other 
                && this.Equals(other);
        }

        /// <summary>
        /// IEquatable ����
        /// </summary>
        public bool Equals(FileExtension other) {
            if (other is null) return false;
            return Extension == other.Extension;
        }


        /// <summary>
        /// �g���q�̕�����\����Ԃ��܂�
        /// </summary>
        public override string ToString() => Extension;

        /// <summary>
        /// �n�b�V���l��Ԃ��D
        /// </summary>
        public override int GetHashCode() => Extension.GetHashCode();


        /// ----------------------------------------------------------------------------
        #region Static

        public static bool operator ==(FileExtension left, FileExtension right) {
            if (ReferenceEquals(left, right)) return true; 
            if (left is null || right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(FileExtension left, FileExtension right) => !(left == right);

        /// <summary>
        /// �W���I�Ȋg���q���
        /// </summary>
        public static class Standard {
            public static readonly FileExtension Txt = new (".txt");
            public static readonly FileExtension Json = new (".json");
            public static readonly FileExtension Csv = new (".csv");
            public static readonly FileExtension Xml = new (".xml");
            public static readonly FileExtension Ini = new (".ini");
            public static readonly FileExtension Jpg = new (".jpg");
            public static readonly FileExtension Png = new (".png");
            public static readonly FileExtension Mp3 = new (".mp3");
            public static readonly FileExtension Mp4 = new (".mp4");
            public static readonly FileExtension Asset = new (".asset");
        }
        #endregion
    }
}
