using System;
using System.IO;

namespace Nitou.IO
{
    /// <summary>
    /// Unityプロジェクト内のアセットパスを表す不変の構造体
    /// </summary>
    /// <remarks>
    /// Assets/またはPackages/配下のファイル・ディレクトリパスを型安全に扱うためのラッパー型。
    /// 内部的にパスの正規化を行い、プラットフォーム間の差異を吸収する。
    /// </remarks>
    [Serializable]
    public readonly struct AssetPath : IEquatable<AssetPath>, IComparable<AssetPath>
    {
        private readonly string _path;
        
        
        #region Properties

        /// <summary>
        /// パスが有効なアセットパスかどうかを取得する
        /// </summary>
        public bool IsValid => !IsEmpty && (IsInAssets || IsInPackages);

        /// <summary>
        /// パスがAssetsフォルダ内のものかどうかを取得する
        /// </summary>
        public bool IsInAssets => _path?.StartsWith("Assets/") == true;

        /// <summary>
        /// パスがPackagesフォルダ内のものかどうかを取得する
        /// </summary>
        public bool IsInPackages => _path?.StartsWith("Packages/") == true;

        /// <summary>
        /// パスが空かどうかを取得する
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(_path);

        /// <summary>
        /// ファイル拡張子を取得する（ドット付き）
        /// </summary>
        public string Extension => Path.GetExtension(_path);

        /// <summary>
        /// 拡張子なしのパスを取得する
        /// </summary>
        public string PathWithoutExtension => HasExtension ? 
            _path.Substring(0, _path.Length - Extension.Length) : _path;

        /// <summary>
        /// 拡張子を持つかどうかを取得する
        /// </summary>
        public bool HasExtension => !string.IsNullOrEmpty(Extension);

        /// <summary>
        /// ファイル名またはディレクトリ名を取得する
        /// </summary>
        public string FileName => Path.GetFileName(_path);

        /// <summary>
        /// ファイル名を拡張子なしで取得する
        /// </summary>
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(_path);

        /// <summary>
        /// 親ディレクトリのパスを取得する
        /// </summary>
        public AssetPath ParentDirectory
        {
            get
            {
                if (IsEmpty) return Empty;
                var parent = Path.GetDirectoryName(_path)?.Replace('\\', '/');
                return new AssetPath(parent ?? string.Empty);
            }
        }

        /// <summary>
        /// 空のAssetPathを取得する
        /// </summary>
        public static AssetPath Empty => new AssetPath(string.Empty);

        #endregion

        #region Factory Methods

        /// <summary>
        /// Assets/からの相対パスでAssetPathを生成する
        /// </summary>
        /// <param name="relativePath">Assets/からの相対パス</param>
        /// <returns>生成されたAssetPath</returns>
        /// <exception cref="ArgumentException">パスが無効な場合</exception>
        public static AssetPath FromAssets(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("Path cannot be null or empty", nameof(relativePath));

            var normalizedPath = NormalizePath(relativePath);
            
            // すでにAssets/で始まっている場合はそのまま使用
            if (normalizedPath.StartsWith("Assets/"))
                return new AssetPath(normalizedPath);
            
            // Assets/を付加
            return new AssetPath($"Assets/{normalizedPath}");
        }

        /// <summary>
        /// Assets/からの相対パスでAssetPathの生成を試みる
        /// </summary>
        /// <param name="relativePath">Assets/からの相対パス</param>
        /// <param name="assetPath">生成されたAssetPath</param>
        /// <returns>生成に成功した場合はtrue</returns>
        public static bool TryFromAssets(string relativePath, out AssetPath assetPath)
        {
            try
            {
                assetPath = FromAssets(relativePath);
                return true;
            }
            catch
            {
                assetPath = Empty;
                return false;
            }
        }

        /// <summary>
        /// Packages/からの相対パスでAssetPathを生成する
        /// </summary>
        /// <param name="packagePath">パッケージ名を含むパス</param>
        /// <returns>生成されたAssetPath</returns>
        /// <exception cref="ArgumentException">パスが無効な場合</exception>
        public static AssetPath FromPackages(string packagePath)
        {
            if (string.IsNullOrEmpty(packagePath))
                throw new ArgumentException("Path cannot be null or empty", nameof(packagePath));

            var normalizedPath = NormalizePath(packagePath);
            
            // すでにPackages/で始まっている場合はそのまま使用
            if (normalizedPath.StartsWith("Packages/"))
                return new AssetPath(normalizedPath);
            
            // Packages/を付加
            return new AssetPath($"Packages/{normalizedPath}");
        }

        /// <summary>
        /// フルパスからAssetPathを生成する（寛容）
        /// </summary>
        /// <param name="fullPath">フルパス</param>
        /// <returns>生成されたAssetPath</returns>
        public static AssetPath FromFullPath(string fullPath)
        {
            return new AssetPath(fullPath);
        }

        /// <summary>
        /// フルパスからAssetPathの生成を試みる
        /// </summary>
        /// <param name="fullPath">フルパス</param>
        /// <param name="assetPath">生成されたAssetPath</param>
        /// <returns>有効なアセットパスの場合はtrue</returns>
        public static bool TryFromFullPath(string fullPath, out AssetPath assetPath)
        {
            assetPath = new AssetPath(fullPath);
            return assetPath.IsValid;
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// 指定されたパス文字列からAssetPathを生成する（寛容なコンストラクタ）
        /// </summary>
        /// <param name="path">アセットパス文字列</param>
        /// <remarks>
        /// このコンストラクタは例外を投げない。不正なパスの場合はIsValidがfalseになる。
        /// </remarks>
        public AssetPath(string path)
        {
            _path = NormalizePath(path);
        }

        /// <summary>
        /// このパスに相対パスを結合する
        /// </summary>
        /// <param name="relativePath">結合する相対パス</param>
        /// <returns>結合された新しいAssetPath</returns>
        public AssetPath Combine(string relativePath)
        {
            if (IsEmpty) return new AssetPath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return this;

            var combined = Path.Combine(_path, relativePath).Replace('\\', '/');
            return new AssetPath(combined);
        }

        /// <summary>
        /// このパスに複数の相対パスを結合する
        /// </summary>
        /// <param name="paths">結合する相対パス群</param>
        /// <returns>結合された新しいAssetPath</returns>
        public AssetPath Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return this;

            var result = this;
            foreach (var path in paths)
            {
                result = result.Combine(path);
            }
            return result;
        }

        /// <summary>
        /// ファイルの拡張子を変更する
        /// </summary>
        /// <param name="newExtension">新しい拡張子（ドット付き）</param>
        /// <returns>拡張子が変更された新しいAssetPath</returns>
        public AssetPath ChangeExtension(string newExtension)
        {
            if (IsEmpty) return this;

            var newPath = Path.ChangeExtension(_path, newExtension);
            return new AssetPath(newPath);
        }

        /// <summary>
        /// 指定されたベースパスからの相対パスを取得する
        /// </summary>
        /// <param name="basePath">ベースとなるパス</param>
        /// <returns>相対パス文字列</returns>
        public string GetRelativePath(AssetPath basePath)
        {
            if (basePath.IsEmpty || IsEmpty) return _path ?? string.Empty;

            var baseUri = new Uri(basePath._path + "/", UriKind.Relative);
            var targetUri = new Uri(_path, UriKind.Relative);
            
            // 簡易的な相対パス計算
            if (_path.StartsWith(basePath._path + "/"))
            {
                return _path.Substring(basePath._path.Length + 1);
            }
            
            return _path;
        }

        /// <summary>
        /// プロジェクトパスとして文字列を取得する
        /// </summary>
        /// <returns>プロジェクトルートからのパス</returns>
        public string ToProjectPath() => _path ?? string.Empty;

        /// <summary>
        /// Assets/またはPackages/を除いた相対パスを取得する
        /// </summary>
        /// <returns>相対パス文字列</returns>
        public string ToRelativePath()
        {
            if (IsInAssets) return _path.Substring(7); // "Assets/".Length = 7
            if (IsInPackages) return _path.Substring(9); // "Packages/".Length = 9
            return _path ?? string.Empty;
        }

        /// <summary>
        /// パス文字列を取得する
        /// </summary>
        public override string ToString()
        {
            return _path ?? string.Empty;
        }
        
        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// 指定されたAssetPathと等しいかどうかを判定する
        /// </summary>
        public bool Equals(AssetPath other)
        {
            return string.Equals(_path, other._path, StringComparison.Ordinal);
        }

        /// <summary>
        /// 指定されたオブジェクトと等しいかどうかを判定する
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is AssetPath other && Equals(other);
        }

        /// <summary>
        /// ハッシュコードを取得する
        /// </summary>
        public override int GetHashCode()
        {
            return _path?.GetHashCode() ?? 0;
        }

        #endregion

        #region IComparable Implementation

        /// <summary>
        /// 指定されたAssetPathと比較する
        /// </summary>
        public int CompareTo(AssetPath other)
        {
            return string.Compare(_path, other._path, StringComparison.Ordinal);
        }

        #endregion

        
        #region Private Methods

        /// <summary>
        /// パスを正規化する
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // バックスラッシュをスラッシュに変換
            path = path.Replace('\\', '/');

            // 連続するスラッシュを単一に
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }

            // "./", "../"を解決（簡易版）
            path = path.Replace("/./", "/");
            
            // 末尾のスラッシュを削除（ルート以外）
            if (path.Length > 1 && path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }

            return path;
        }

        #endregion
        
        #region Operators

        /// <summary>
        /// 等価演算子
        /// </summary>
        public static bool operator ==(AssetPath left, AssetPath right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 非等価演算子
        /// </summary>
        public static bool operator !=(AssetPath left, AssetPath right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 文字列への暗黙的変換
        /// </summary>
        public static implicit operator string(AssetPath path)
        {
            return path._path ?? string.Empty;
        }

        /// <summary>
        /// 文字列からの明示的変換
        /// </summary>
        public static explicit operator AssetPath(string path)
        {
            return new AssetPath(path);
        }

        #endregion

    }
}