using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

// [REF]
// _: ファイル・ディレクトリ関連util https://ameblo.jp/ka-neda/entry-12779824591.html

namespace Nitou.IO
{
    /// <summary>
    /// ファイル操作に関する汎用メソッド集．
    /// </summary>
    public static class FileUtils
    {
        #region 判定

        /// <summary>
        /// ファイルの存在チェック．
        /// </summary>
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// ファイルの存在チェック．
        /// </summary>
        public static bool Exists(IEnumerable<string> paths)
        {
            return paths.All(path => Exists(path));
        }

        /// <summary>
        /// ファイルの存在チェック．
        /// 存在しない場合は<see cref="FileNotFoundException"/>を投げる．
        /// </summary>
        public static void ExistsOrTheow(string path)
        {
            if (!Exists(path))
            {
                throw new FileNotFoundException("File is not exist :" + path);
            }
        }

        /// <summary>
        /// ファイルの存在チェック．
        /// 存在しない場合は<see cref="FileNotFoundException"/>を投げる．
        /// </summary>
        public static void ExistsOrThrow(IEnumerable<string> paths)
        {
            foreach (var path in paths)
                ExistsOrTheow(path);
        }

        #endregion


        /// <summary>
        /// ファイル検索．
        /// </summary>
        /// <param name="directoryPath">ディレクトリ</param>
        /// <param name="filter">取得条件</param>
        /// <returns>ファイルのリスト</returns>
        public static List<string> Find(string directoryPath, Predicate<string> filter)
        {
            DirectoryUtils.ExistsOrThrow(directoryPath);
            return Directory.GetFiles(directoryPath)
                            .Where(p => filter(p))
                            .ToList();
        }

        /// <summary>
        /// ファイル検索(再帰)
        /// </summary>
        /// <param name="directoryPath">ディレクトリ</param>
        /// <returns>ファイルリスト</returns>
        public static List<string> RecurFind(string directoryPath)
        {
            return RecurFind(directoryPath, FileFilter.All());
        }

        /// <summary>
        /// ファイル検索(再帰)
        /// </summary>
        /// <param name="d">ディレクトリ</param>
        /// <param name="filter">取得条件</param>
        /// <returns>ファイルリスト</returns>
        public static List<string> RecurFind(string d, Predicate<string> filter)
        {
            return
                DirectoryUtils
                    .RecurFind(d)
                    .Aggregate(new List<string>(), (acc, t) =>
                    {
                        acc.AddRange(Find(t, filter));
                        return acc;
                    })
                    .ToList();
        }
    }


    /// <summary>
    /// ファイル検索用のフィルタ．
    /// </summary>
    public sealed class FileFilter
    {
        /// <summary>
        /// フィルタ無し
        /// </summary>
        /// <returns>true</returns>
        public static Predicate<string> All()
        {
            return t => true;
        }

        /// <summary>
        /// ファイル名正規表現
        /// </summary>
        /// <param name="r">正規表現</param>
        /// <returns>フィルタ関数</returns>
        public static Predicate<string> FileReg(string r)
        {
            return t => Regex.IsMatch(Path.GetFileName(t), r);
        }

        /// <summary>
        /// フルパス正規表現
        /// </summary>
        /// <param name="r">正規表現</param>
        /// <returns>フィルタ関数</returns>
        public static Predicate<string> FullReg(string r)
        {
            return t => Regex.IsMatch(t, r);
        }

        /// <summary>
        /// 拡張子
        /// </summary>
        /// <param name="s">拡張子</param>
        /// <returns>フィルタ関数</returns>
        public static Predicate<string> Extension(string s)
        {
            return t => t.EndsWith("." + s);
        }
    }
}