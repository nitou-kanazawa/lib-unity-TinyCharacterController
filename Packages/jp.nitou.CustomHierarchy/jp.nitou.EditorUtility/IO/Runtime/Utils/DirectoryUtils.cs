using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// [REF]
//  コガネブログ: 区切り文字にスラッシュを使用して指定したディレクトリ内のファイル名を返す関数 https://baba-s.hatenablog.com/entry/2015/07/29/100000
//  _: C#ファイル／フォルダ操作術。すぐに使えるサンプルコード付き https://resanaplaza.com/2024/02/23/%E3%80%90%E5%AE%9F%E8%B7%B5%E3%80%91c%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB%EF%BC%8F%E3%83%95%E3%82%A9%E3%83%AB%E3%83%80%E6%93%8D%E4%BD%9C%E8%A1%93%E3%80%82%E3%81%99%E3%81%90%E3%81%AB%E4%BD%BF%E3%81%88/#google_vignette

namespace Nitou.IO
{
    /// <summary>
    /// ディレクトリ操作に関する汎用メソッド集．
    /// パスの区切り文字は「\\」ではなく「/」を使用する．
    /// </summary>
    public static class DirectoryUtils
    {
        #region 判定

        /// <summary>
        /// ディレクトリ存在チェック．
        /// </summary>
        public static bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// ディレクトリ存在チェック．
        /// 存在しない場合は<see cref="DirectoryNotFoundException"/>を投げる．
        /// </summary>
        public static void ExistsOrThrow(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("Directory is not exist :" + path);
            }
        }

        /// <summary>
        /// ディレクトリ存在チェック．
        /// 存在しない場合は<see cref="DirectoryNotFoundException"/>を投げる．
        /// </summary>
        public static void ExistsOrThrow(IEnumerable<string> paths)
        {
            foreach (var path in paths)
                ExistsOrThrow(path);
        }

        #endregion


        #region 取得 (Array)

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリのパスを全て取得する．
        /// </summary>
        public static string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリのパスを全て取得する．
        /// </summary>
        public static string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリのパスを全て取得する．
        /// </summary>
        public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetDirectories(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFiles(string path)
        {
            return Directory.GetFiles(path)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFileSystemEntries(string path)
        {
            return Directory.GetFileSystemEntries(path)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return Directory.GetFileSystemEntries(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得する．
        /// </summary>
        public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFileSystemEntries(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter())
                            .ToArray();
        }

        #endregion


        #region 取得 (IEnumerable)

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリのパスを列挙する．
        /// </summary>
        public static IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.EnumerateDirectories(path)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// 指定したディレクトリに含まれるディレクトリのパスを、検索パターンを指定して列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return Directory.EnumerateDirectories(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリに含まれるディレクトリのパスを、検索パターンと検索オプションを指定して列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateDirectories(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリ内のファイルのパスを列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリ内の指定した検索パターンに一致するファイルのパスを列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリ内の指定した検索パターンに一致するファイルのパスを、検索オプションを指定して列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            return Directory.EnumerateFileSystemEntries(path)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを、検索パターンを指定して列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            return Directory.EnumerateFileSystemEntries(path, searchPattern)
                            .Select(c => c.ReplaceDelimiter());
        }

        /// <summary>
        /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを、検索パターンと検索オプションを指定して列挙します。
        /// </summary>
        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption)
                            .Select(c => c.ReplaceDelimiter());
        }

        #endregion


        #region

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<string> EnumerateDirectoryNames(string path)
        {
            return Directory.EnumerateDirectories(path)
                            .Select(dirPath => PathUtils.GetFileName(dirPath));
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<string> EnumerateFileNames(string path)
        {
            return Directory.EnumerateFiles(path)
                            .Select(dirPath => Path.GetFileName(dirPath));
        }

        #endregion


        #region 検索

        /// <summary>
        /// ディレクトリ検索．
        /// </summary>
        public static List<string> Find(string direPath)
        {
            ExistsOrThrow(direPath);
            return Directory.GetDirectories(direPath).ToList();
        }

        /// <summary>
        /// ディレクトリ検索(再帰)
        /// </summary>
        /// <param name="direPath">ディレクトリ</param>
        /// <returns>ディレクトリリスト</returns>
        public static List<string> RecurFind(string direPath)
        {
            var subDirePaths = Find(direPath);
            if (subDirePaths.Count == 0)
            {
                return new List<string> { direPath };
            }
            else
            {
                return subDirePaths.Aggregate(new List<string>(), (acc, d) =>
                {
                    acc.AddRange(RecurFind(d));
                    return acc;
                });
            }
        }

        #endregion


        #region ディレクトリ操作

        // [REF] Microsoft Learn: ディレクトリをコピーする https://learn.microsoft.com/ja-jp/dotnet/standard/io/how-to-copy-directories

        /// <summary>
        /// ディレクトリを再帰的にコピーする．
        /// </summary>
        public static void CopyDirectory(string sourceDir, string destDir)
        {
            // コピー先のフォルダが存在しない場合は作成する
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                try
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                catch (Exception ex)
                {
                    // エラーが発生した場合はエラーメッセージを表示して処理を継続する
                    Debug.LogWarning("ファイルのコピー中にエラーが発生しました: " + ex.Message);
                }
            }

            // Copy directories
            foreach (string folder in Directory.GetDirectories(sourceDir))
            {
                try
                {
                    string destFolder = Path.Combine(destDir, Path.GetFileName(folder));
                    CopyDirectory(folder, destFolder); // サブフォルダを再帰的にコピーする
                }
                catch (Exception ex)
                {
                    // エラーが発生した場合はエラーメッセージを表示して処理を継続する
                    Debug.LogWarning("フォルダのコピー中にエラーが発生しました: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// ディレクトリ内の要素を全て削除する．
        /// </summary>
        public static void Clear(string path)
        {
            // [NOTE] 一度ディレクトリごと削除してから再作成する
            Directory.Delete(path);
            Directory.CreateDirectory(path);
        }

        #endregion
    }
}