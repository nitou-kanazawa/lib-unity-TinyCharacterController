#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Nitou.IO;

namespace Nitou.EditorShared
{
    /// <summary>
    /// <see cref="Resources"/>ライクに非Resourcesフォルダのアセットを読み込むためのクラス
    /// </summary>
    /// <remarks>
    /// AssetDatabaseのラッパーとして、AssetPath型を使用した型安全なアセット読み込みを提供する
    /// </remarks>
    public static class AssetsLoader
    {
        #region Public Methods - Single Asset Loading

        /// <summary>
        /// 指定されたパスからアセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="assetPath">アセットパス</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static T Load<T>(AssetPath assetPath)
            where T : Object
        {
            if (!assetPath.IsValid)
            {
                Debug.LogWarning($"Invalid asset path: {assetPath}");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath.ToString());
        }

        /// <summary>
        /// 指定されたパスからアセットを読み込む（Object型）
        /// </summary>
        /// <param name="assetPath">アセットパス</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static Object Load(AssetPath assetPath)
        {
            return Load<Object>(assetPath);
        }


        /// <summary>
        /// パス文字列から直接アセットを読み込む（後方互換性のため）
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="path">アセットパス文字列</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static T Load<T>(string path)
            where T : Object
        {
            return Load<T>(new AssetPath(path));
        }

        /// <summary>
        /// 複数のパスセグメントを結合してアセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="basePath">ベースパス</param>
        /// <param name="relativePath">相対パス</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static T Load<T>(AssetPath basePath, string relativePath, string fileName)
            where T : Object
        {
            var fullPath = basePath.Combine(relativePath, fileName);
            return Load<T>(fullPath);
        }

        /// <summary>
        /// ファイルのアセットパス(拡張子も含める)と型を設定し、Objectを読み込む．
        /// </summary>
        public static T Load<T>(PackageDirectoryPath packagePath, string relativePath, string fileName)
            where T : Object
        {
            var path = PathUtils.Combine(packagePath.ToProjectPath(), relativePath, fileName);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        #endregion

        // ----------------------------------------------------------------------------

        #region Public Method (複数ロード)

        /// <summary>
        /// 指定されたディレクトリ内のすべてのアセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="directoryPath">ディレクトリパス</param>
        /// <param name="searchOption">検索オプション（デフォルト：サブディレクトリも含む）</param>
        /// <returns>読み込まれたアセットのリスト</returns>
        public static List<T> LoadAll<T>(AssetPath directoryPath, SearchOption searchOption = SearchOption.AllDirectories)
            where T : Object
        {
            if (!directoryPath.IsValid)
            {
                Debug.LogWarning($"Invalid directory path: {directoryPath}");
                return new List<T>();
            }

            if (!directoryPath.IsDirectory())
            {
                Debug.LogWarning($"The specified path is not a directory: {directoryPath}");
                return new List<T>();
            }

            return LoadAllInternal<T>(directoryPath.ToString(), searchOption);
        }

        /// <summary>
        /// 指定されたディレクトリ内のすべてのアセットを読み込む（Object型）
        /// </summary>
        /// <param name="directoryPath">ディレクトリパス</param>
        /// <param name="searchOption">検索オプション</param>
        /// <returns>読み込まれたアセットのリスト</returns>
        public static List<Object> LoadAll(AssetPath directoryPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return LoadAll<Object>(directoryPath, searchOption);
        }

        /// <summary>
        /// 複数のパスセグメントを結合してディレクトリ内のアセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="basePath">ベースパス</param>
        /// <param name="relativePath">相対パス</param>
        /// <param name="searchOption">検索オプション</param>
        /// <returns>読み込まれたアセットのリスト</returns>
        public static List<T> LoadAll<T>(AssetPath basePath, string relativePath, SearchOption searchOption = SearchOption.AllDirectories)
            where T : Object
        {
            var fullPath = basePath.Combine(relativePath);
            return LoadAll<T>(fullPath, searchOption);
        }

        public static List<T> LoadAll<T>(PackageDirectoryPath packagePath, string relativePath, SearchOption searchOption = SearchOption.AllDirectories)
            where T : Object
        {
            var path = PathUtils.Combine(packagePath.ToProjectPath(), relativePath);
            return LoadAllInternal<T>(path, searchOption);
        }

        #endregion

        // ----------------------------------------------------------------------------
        
        #region Public Methods - Filtered Loading

        /// <summary>
        /// 指定された拡張子のアセットのみを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="directoryPath">ディレクトリパス</param>
        /// <param name="extensions">対象の拡張子（ドット付き）</param>
        /// <returns>読み込まれたアセットのリスト</returns>
        public static List<T> LoadAllWithExtensions<T>(AssetPath directoryPath, params string[] extensions)
            where T : Object
        {
            if (!directoryPath.IsValid || !directoryPath.IsDirectory())
            {
                Debug.LogWarning($"Invalid or non-existent directory: {directoryPath}");
                return new List<T>();
            }

            var allAssets = LoadAll<T>(directoryPath);
            if (extensions == null || extensions.Length == 0)
                return allAssets;

            return allAssets.Where(asset =>
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string extension = Path.GetExtension(assetPath);
                return extensions.Contains(extension);
            }).ToList();
        }

        /// <summary>
        /// 指定された名前パターンに一致するアセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="directoryPath">ディレクトリパス</param>
        /// <param name="pattern">名前パターン（ワイルドカード使用可）</param>
        /// <returns>読み込まれたアセットのリスト</returns>
        public static List<T> LoadAllWithPattern<T>(AssetPath directoryPath, string pattern)
            where T : Object
        {
            if (!directoryPath.IsValid || !directoryPath.IsDirectory())
            {
                Debug.LogWarning($"Invalid or non-existent directory: {directoryPath}");
                return new List<T>();
            }

            string directoryStr = directoryPath.ToString();
            var files = Directory.GetFiles(directoryStr, pattern, SearchOption.AllDirectories);

            var assetList = new List<T>();
            foreach (string filePath in files)
            {
                // .metaファイルをスキップ
                if (filePath.EndsWith(".meta")) continue;

                var normalizedPath = filePath.Replace('\\', '/');
                T asset = AssetDatabase.LoadAssetAtPath<T>(normalizedPath);
                if (asset != null)
                {
                    assetList.Add(asset);
                }
            }

            return assetList;
        }

        #endregion

        // ----------------------------------------------------------------------------
        
        #region Public Methods - Asset Finding

        /// <summary>
        /// プロジェクト全体から指定された型のアセットを検索する
        /// </summary>
        /// <typeparam name="T">検索するアセットの型</typeparam>
        /// <returns>見つかったアセットのリスト</returns>
        public static List<T> FindAssetsOfType<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        /// <summary>
        /// 指定されたディレクトリ内から特定の型のアセットを検索する
        /// </summary>
        /// <typeparam name="T">検索するアセットの型</typeparam>
        /// <param name="searchPath">検索するディレクトリパス</param>
        /// <returns>見つかったアセットのリスト</returns>
        public static List<T> FindAssetsOfType<T>(AssetPath searchPath) where T : Object
        {
            if (!searchPath.IsValid)
            {
                Debug.LogWarning($"Invalid search path: {searchPath}");
                return new List<T>();
            }

            string[] searchInFolders = { searchPath.ToString() };
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchInFolders);
            var assets = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        #endregion

        // ----------------------------------------------------------------------------

        #region Private Method

        /// <summary>
        /// 指定されたディレクトリから全アセットを読み込む（内部実装）
        /// </summary>
        private static List<T> LoadAllInternal<T>(string directoryPath, SearchOption searchOption)
            where T : Object
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"Directory does not exist: {directoryPath}");
                return new List<T>();
            }

            // 指定したディレクトリに入っている全ファイルを取得
            var filePaths = Directory.EnumerateFiles(directoryPath, "*", searchOption);

            // 取得したファイルの中からアセットだけリストに追加する
            var assetList = new List<T>();
            foreach (string filePath in filePaths)
            {
                // .metaファイルをスキップ
                if (filePath.EndsWith(".meta")) continue;

                // パスを正規化
                string normalizedPath = filePath.Replace('\\', '/');
                T asset = AssetDatabase.LoadAssetAtPath<T>(normalizedPath);

                if (asset != null)
                {
                    assetList.Add(asset);
                }
            }

            return assetList;
        }

        #endregion
    }


    /// <summary>
    /// AssetsLoader用の拡張メソッド
    /// </summary>
    internal static class AssetsLoaderExtensions
    {
        /// <summary>
        /// nullでない場合のみリストに追加する
        /// </summary>
        public static void AddIfNotNull<T>(this List<T> list, T item) where T : class
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
    }
}
#endif