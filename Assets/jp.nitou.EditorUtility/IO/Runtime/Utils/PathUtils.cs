using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

// [REF]
//  qiita: C#(Unity)でのファイルパスの取得 https://qiita.com/oishihiroaki/items/1a082f3bb32f2e3d88a0
//  はなちる: 絶対パスをAssets/~に変換する https://www.hanachiru-blog.com/entry/2018/10/12/204022
//  _ : フルパスをAssetsパスに変換する方法 https://mizutanikirin.net/unity-assetspath

namespace Nitou.IO
{
    public enum Delimiter
    {
        Slash,
        BackSlash,
    }

    /// <summary>
    /// パス取得に関する汎用メソッド集．
    /// パスの区切り文字は「\\」ではなく「/」を使用する．
    /// </summary>
    public static class PathUtils
    {
        /// <summary> 永続化用のデータフォルダパス．</summary>
        public static string PersistentDataPath => Application.persistentDataPath;
        
        /// <summary>ストリーミング用のデータパス．</summary>
        public static string StreamingAssetsPath => Application.streamingAssetsPath;
        
        /// <summary>
        /// 
        /// </summary>
        public static string DataPath => Application.dataPath;
        
        /// <summary>
        /// 
        /// </summary>
        public static string TemporaryCachePath => Application.temporaryCachePath;


        // --------------------------------------------------------------------

        #region パスの結合

        /// <summary>
        /// path1とpath2を結合したパスを生成する．
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// path1~path3を結合したパスを生成する．
        /// </summary>
        public static string Combine(string path1, string path2, string path3)
        {
            return Path.Combine(path1, path2, path3)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// path1~path4を結合したパスを生成する．
        /// </summary>
        public static string Combine(string path1, string path2, string path3, string path4)
        {
            return Path.Combine(path1, path2, path3, path4)
                       .ReplaceDelimiter();
        }

        /// <summary>
        ///paths全てを結合したパスを生成
        /// </summary>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths)
                       .ReplaceDelimiter();
        }

        #endregion


        // --------------------------------------------------------------------
        // パスの加工

        /// <summary>
        /// 拡張子を変更する．
        /// </summary>
        public static string ChangeExtension(string path, string extension)
        {
            return Path.ChangeExtension(path, extension)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// 相対パスから絶対パスを取得する．
        /// </summary>
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// 相対パスからAssetsパスを取得する．
        /// </summary>
        public static string GetPathFromAssets(string path)
        {
            return (new Uri(Application.dataPath))
                   .MakeRelativeUri(new Uri(path))
                   .ToString();
        }


        #region 取得

        /// <summary>
        /// ディレクトリの名前を取得する．
        /// </summary>
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// ファイルパスからファイル名を取得する．
        /// </summary>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// 拡張子なしのファイル名を取得する．
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path)
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// ファイルパスから拡張子を取得する．
        /// </summary>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        /// ファイル名に使用できない文字を全て取得する．
        /// </summary>
        public static char[] GetInvalidFileNameChars()
        {
            return Path.GetInvalidFileNameChars();
        }

        /// <summary>
        /// パスに使用できない文字を全て取得する．
        /// </summary>
        public static char[] GetInvalidPathChars()
        {
            return Path.GetInvalidPathChars();
        }

        /// <summary>
        /// 指定のパスからルートへのパスだけを取得する．
        /// </summary>
        public static string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path)
                       .ReplaceDelimiter();
        }

        #endregion


        // --------------------------------------------------------------------

        #region パスの判定

        // [REF] 
        //  MS-Learn: FileAttributes列挙型 https://learn.microsoft.com/ja-jp/dotnet/api/system.io.fileattributes?view=net-8.0
        //  _: ファイルの属性を取得/設定 https://csharp.nomux2.net/file-attribute/

        /// <summary>
        /// ルートパスか確認する．
        /// </summary>
        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        /// <summary>
        /// ディレクトリか確認する．
        /// </summary>
        public static bool IsDirectory(string path)
        {
            if (!Directory.Exists(path)) return false;

            return File.GetAttributes(path)
                       .HasFlag(FileAttributes.Directory);
        }

        /// <summary>
        /// 標準ファイルか確認する．
        /// </summary>
        public static bool IsNormalFile(string path)
        {
            if (!File.Exists(path)) return false;

            return File.GetAttributes(path)
                       .HasFlag(FileAttributes.Normal);
        }

        /// <summary>
        /// 拡張子が付いているか確認する．
        /// </summary>
        public static bool HasExtension(string path)
        {
            return Path.HasExtension(path);
        }

        /// <summary>
        /// 同じ拡張か確認する．
        /// </summary>
        public static bool AreExtensionsEqual(string path1, string path2)
        {
            return string.Equals(
                PathUtils.GetExtension(path1),
                PathUtils.GetExtension(path2),
                StringComparison.OrdinalIgnoreCase
            );
        }

        #endregion


        // --------------------------------------------------------------------

        #region 呼び出し元の取得

        // [REF]
        //  kanのメモ帳: 移動する可能性のある任意のアセットやディレクトリ(フォルダ)のパスを取得する方法 https://kan-kikuchi.hatenablog.com/entry/Get_Path_May_Move
        //  MSDoq: CallerFilePathAttributeクラス https://learn.microsoft.com/ja-jp/dotnet/api/system.runtime.compilerservices.callerfilepathattribute?view=netframework-4.8

        /// <summary>
        /// 呼び出し元のパスを取得
        /// </summary>
        public static string GetSelfPath([CallerFilePath] string sourceFilePath = "")
        {
            return ReplaceDelimiter(sourceFilePath);
        }

        /// <summary>
        /// 呼び出し元のパス(Assetsから)を取得
        /// </summary>
        public static string GetSelfPathFromAssets([CallerFilePath] string sourceFilePath = "")
        {
            return (new Uri(Application.dataPath))
                   .MakeRelativeUri(new Uri(sourceFilePath))
                   .ToString();
        }

        #endregion


        // --------------------------------------------------------------------
        // その他

        /// <summary>
        /// ランダムなファイル名を取得
        /// </summary>
        /// <returns></returns>
        public static string GetRandomFileName()
        {
            return Path.GetRandomFileName()
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// 一時ファイルのパスをランダムに作成
        /// </summary>
        public static string GetTempFileName()
        {
            return Path.GetTempFileName()
                       .ReplaceDelimiter();
        }

        /// <summary>
        /// 一時ファイルがあるディレクトリへのパスを取得
        /// </summary>
        public static string GetTempPath()
        {
            return Path.GetTempPath()
                       .ReplaceDelimiter();
        }


        /// <summary>
        /// Windows で使われることがある区切り文字"\"をMacやUnity用の区切り文字"/"に置き換える．
        /// </summary>
        public static string ReplaceDelimiter(this string path)
        {
            return path.Replace("\\", "/");
        }
        
        /// <summary>
        /// パスを正規化する
        /// </summary>
        public static string NormalizeDelimiter(this string path)
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


        // --------------------------------------------------------------------

        #region パスの変換（string拡張メソッド）

        /// <summary>
        /// フルパスをアセット以下パス(Assets/..)に変換する
        /// </summary>
        public static string ToAssetsPath(this string fullPath)
        {
            // "Assets/"位置を取得
            int startIndex = fullPath.IndexOf("Assets/", System.StringComparison.Ordinal);
            if (startIndex == -1)
            {
                startIndex = fullPath.IndexOf("Assets\\", System.StringComparison.Ordinal);
            }

            // ※含まれない場合は，空文字を返す
            if (startIndex == -1) return "";

            // 加工後パスを返す
            string assetPath = fullPath.Substring(startIndex);
            return assetPath;
        }


        public static string GetParentDirectory(string filepath, int n = 1)
        {
            string dir = filepath;
            for (int i = 0; i < n; i++)
            {
                dir = Directory.GetParent(dir).FullName;
            }

            return dir.ToAssetsPath();
        }

        #endregion
    }
}