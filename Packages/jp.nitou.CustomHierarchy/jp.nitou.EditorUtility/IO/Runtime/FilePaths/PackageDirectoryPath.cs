using System;
using UnityEngine;

namespace Nitou.IO
{
    /// <summary>
    /// UPM用の自作パッケージのディレクトリパス指定用のクラス．
    /// </summary>
    public sealed class PackageDirectoryPath
    {
        // NOTE: ディレクトリは開発時は"Assets/"以下に，配布後は"Packages/"以下にあるものと想定する．

        private enum Mode
        {
            // 配布後
            Upm,

            // 開発プロジェクト内
            Normal,

            // 
            NotExist,
        }

        // 相対パス
        private readonly string _upmRelativePath;
        private readonly string _normalRelativePath;

        private readonly Mode _mode;


        /// <summary>
        /// Package配布後のパッケージパス
        /// </summary>
        public string UpmPath => $"Packages/{_upmRelativePath}".ReplaceDelimiter();

        /// <summary>
        /// 開発プロジェクトでのアセットパス
        /// </summary>
        public string NormalPath => $"Assets/{_normalRelativePath}".ReplaceDelimiter();


        // ----------------------------------------------------------------------------
        // Pubic Method

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public PackageDirectoryPath(string relativePath = "jp.nitou.mylib")
            : this(relativePath, relativePath) { }

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public PackageDirectoryPath(string upmRelativePath = "jp.nitou.mylib", string normalRelativePath = "Plugins/MyLib")
        {
            _upmRelativePath = upmRelativePath ?? throw new ArgumentNullException(nameof(upmRelativePath));
            _normalRelativePath = normalRelativePath ?? throw new ArgumentNullException(nameof(normalRelativePath));

            // 現在のパスを判定する
            _mode = CheckDirectoryLocation();
        }


        // ----------------------------------------------------------------------------
        // Pubic Method

        /// <summary>
        /// Projectディレクトリを起点としたパス．
        /// </summary>
        public string ToProjectPath()
        {
            return _mode switch
            {
                Mode.Upm => UpmPath,
                Mode.Normal => NormalPath,
                _ => ""
            };
        }

        /// <summary>
        /// 絶対パス．
        /// </summary>
        public string ToAbsolutePath() => PathUtils.GetFullPath(ToProjectPath());


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// ディレクトリの位置を判定する．
        /// </summary>
        private Mode CheckDirectoryLocation()
        {
            if (DirectoryUtils.Exists(UpmPath)) return Mode.Upm;
            if (DirectoryUtils.Exists(NormalPath)) return Mode.Normal;

            Debug.LogError($"Directory not found in both UPM and normal paths: \n" +
                           $"  [{UpmPath}] and \n" +
                           $"  [{NormalPath}]");
            return Mode.NotExist;
        }
    }
}