#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nitou.IO
{
    /// <summary>
    /// AssetPath構造体のUnity依存拡張メソッド
    /// </summary>
    public static class AssetPathExtensions
    {
        #region Validation Methods

        /// <summary>
        /// アセットが実際に存在するかどうかを確認する
        /// </summary>
        /// <param name="path">確認するアセットパス</param>
        /// <returns>存在する場合はtrue</returns>
        public static bool Exists(this AssetPath path)
        {
            if (!path.IsValid) return false;
            
            string fullPath = path.ToString();
            return File.Exists(fullPath) || Directory.Exists(fullPath);
        }

        /// <summary>
        /// パスがファイルを指しているかどうかを確認する
        /// </summary>
        /// <param name="path">確認するアセットパス</param>
        /// <returns>ファイルの場合はtrue</returns>
        public static bool IsFile(this AssetPath path)
        {
            if (!path.IsValid) return false;
            return File.Exists(path.ToString());
        }

        /// <summary>
        /// パスがディレクトリを指しているかどうかを確認する
        /// </summary>
        /// <param name="path">確認するアセットパス</param>
        /// <returns>ディレクトリの場合はtrue</returns>
        public static bool IsDirectory(this AssetPath path)
        {
            if (!path.IsValid) return false;
            return Directory.Exists(path.ToString());
        }

        #endregion

        #region GUID Methods

        /// <summary>
        /// アセットパスからGUIDを取得する
        /// </summary>
        /// <param name="path">GUIDを取得するアセットパス</param>
        /// <returns>GUID文字列（存在しない場合は空文字列）</returns>
        public static string ToGUID(this AssetPath path)
        {
            if (!path.IsValid) return string.Empty;
            return AssetDatabase.AssetPathToGUID(path.ToString());
        }

        /// <summary>
        /// GUIDからAssetPathを生成する
        /// </summary>
        /// <param name="guid">GUID文字列</param>
        /// <returns>対応するAssetPath（存在しない場合はEmpty）</returns>
        public static AssetPath FromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return AssetPath.Empty;
            
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? AssetPath.Empty : new AssetPath(path);
        }

        #endregion

        #region Asset Loading Methods

        /// <summary>
        /// アセットを読み込む
        /// </summary>
        /// <typeparam name="T">読み込むアセットの型</typeparam>
        /// <param name="path">読み込むアセットのパス</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static T Load<T>(this AssetPath path) where T : Object
        {
            if (!path.IsValid) return null;
            return AssetDatabase.LoadAssetAtPath<T>(path.ToString());
        }

        /// <summary>
        /// アセットを読み込む（Object型）
        /// </summary>
        /// <param name="path">読み込むアセットのパス</param>
        /// <returns>読み込まれたアセット（失敗時はnull）</returns>
        public static Object Load(this AssetPath path)
        {
            return path.Load<Object>();
        }

        /// <summary>
        /// アセットをメインアセットとして読み込む
        /// </summary>
        /// <param name="path">読み込むアセットのパス</param>
        /// <returns>メインアセット（失敗時はnull）</returns>
        public static Object LoadMainAsset(this AssetPath path)
        {
            if (!path.IsValid) return null;
            return AssetDatabase.LoadMainAssetAtPath(path.ToString());
        }

        /// <summary>
        /// 指定パスのすべてのアセットを読み込む
        /// </summary>
        /// <param name="path">読み込むアセットのパス</param>
        /// <returns>読み込まれたアセット配列</returns>
        public static Object[] LoadAllAssets(this AssetPath path)
        {
            if (!path.IsValid) return new Object[0];
            return AssetDatabase.LoadAllAssetsAtPath(path.ToString());
        }

        #endregion

        #region Asset Type Methods

        /// <summary>
        /// アセットの型を取得する
        /// </summary>
        /// <param name="path">型を取得するアセットのパス</param>
        /// <returns>アセットの型（取得できない場合はnull）</returns>
        public static System.Type GetAssetType(this AssetPath path)
        {
            if (!path.IsValid) return null;
            return AssetDatabase.GetMainAssetTypeAtPath(path.ToString());
        }

        /// <summary>
        /// 指定した型のアセットかどうかを確認する
        /// </summary>
        /// <typeparam name="T">確認する型</typeparam>
        /// <param name="path">確認するアセットのパス</param>
        /// <returns>指定した型の場合はtrue</returns>
        public static bool IsAssetType<T>(this AssetPath path) where T : Object
        {
            var type = path.GetAssetType();
            if (type == null) return false;
            return typeof(T).IsAssignableFrom(type);
        }

        #endregion

        #region Import Methods

        /// <summary>
        /// アセットを再インポートする
        /// </summary>
        /// <param name="path">再インポートするアセットのパス</param>
        /// <param name="options">インポートオプション</param>
        public static void Reimport(this AssetPath path, ImportAssetOptions options = ImportAssetOptions.Default)
        {
            if (!path.IsValid) return;
            AssetDatabase.ImportAsset(path.ToString(), options);
        }

        /// <summary>
        /// アセットを更新する（変更を反映）
        /// </summary>
        /// <param name="path">更新するアセットのパス</param>
        public static void Refresh(this AssetPath path)
        {
            if (!path.IsValid) return;
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        #endregion

        #region Creation/Deletion Methods

        /// <summary>
        /// ディレクトリを作成する
        /// </summary>
        /// <param name="path">作成するディレクトリのパス</param>
        /// <returns>作成に成功した場合はtrue</returns>
        public static bool CreateDirectory(this AssetPath path)
        {
            if (!path.IsValid) return false;
            if (path.IsDirectory()) return true;

            string fullPath = path.ToString();
            string parentPath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(fullPath);

            if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(folderName))
                return false;

            string guid = AssetDatabase.CreateFolder(parentPath, folderName);
            return !string.IsNullOrEmpty(guid);
        }

        /// <summary>
        /// アセットを削除する
        /// </summary>
        /// <param name="path">削除するアセットのパス</param>
        /// <returns>削除に成功した場合はtrue</returns>
        public static bool Delete(this AssetPath path)
        {
            if (!path.IsValid || !path.Exists()) return false;
            return AssetDatabase.DeleteAsset(path.ToString());
        }

        /// <summary>
        /// アセットを移動またはリネームする
        /// </summary>
        /// <param name="sourcePath">移動元のパス</param>
        /// <param name="destinationPath">移動先のパス</param>
        /// <returns>移動に成功した場合はtrue</returns>
        public static bool MoveTo(this AssetPath sourcePath, AssetPath destinationPath)
        {
            if (!sourcePath.IsValid || !destinationPath.IsValid) return false;
            if (!sourcePath.Exists()) return false;

            string error = AssetDatabase.MoveAsset(sourcePath.ToString(), destinationPath.ToString());
            return string.IsNullOrEmpty(error);
        }

        /// <summary>
        /// アセットをコピーする
        /// </summary>
        /// <param name="sourcePath">コピー元のパス</param>
        /// <param name="destinationPath">コピー先のパス</param>
        /// <returns>コピーに成功した場合はtrue</returns>
        public static bool CopyTo(this AssetPath sourcePath, AssetPath destinationPath)
        {
            if (!sourcePath.IsValid || !destinationPath.IsValid) return false;
            if (!sourcePath.Exists()) return false;

            return AssetDatabase.CopyAsset(sourcePath.ToString(), destinationPath.ToString());
        }

        #endregion

        #region Selection Methods

        /// <summary>
        /// アセットを選択状態にする
        /// </summary>
        /// <param name="path">選択するアセットのパス</param>
        public static void Select(this AssetPath path)
        {
            if (!path.IsValid) return;
            
            var asset = path.Load();
            if (asset != null)
            {
                Selection.activeObject = asset;
            }
        }

        /// <summary>
        /// アセットをPingする（Project windowでハイライト）
        /// </summary>
        /// <param name="path">Pingするアセットのパス</param>
        public static void Ping(this AssetPath path)
        {
            if (!path.IsValid) return;
            
            var asset = path.Load();
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        #endregion

        #region Dependency Methods

        /// <summary>
        /// このアセットが依存しているアセットのパスを取得する
        /// </summary>
        /// <param name="path">依存関係を調べるアセットのパス</param>
        /// <param name="recursive">再帰的に取得するか</param>
        /// <returns>依存アセットのパス配列</returns>
        public static AssetPath[] GetDependencies(this AssetPath path, bool recursive = true)
        {
            if (!path.IsValid) return new AssetPath[0];

            string[] dependencies = AssetDatabase.GetDependencies(path.ToString(), recursive);
            var result = new AssetPath[dependencies.Length];
            
            for (int i = 0; i < dependencies.Length; i++)
            {
                result[i] = new AssetPath(dependencies[i]);
            }
            
            return result;
        }

        #endregion
    }
}
#endif