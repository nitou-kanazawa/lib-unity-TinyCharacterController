using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nitou.Utility
{
    /// <summary>
    /// <see cref="GameObject"/>プールを構築するためのインターフェース。
    /// オブジェクトの取得、返却、破棄、およびマネージャーからのオブジェクト検索に使用される。
    /// </summary>
    public interface IGameObjectPool
    {
        /// <summary>
        /// プレハブID（検索に使用）。
        /// </summary>
        int PrefabID { get; }

        /// <summary>
        /// コンポーネントが属するシーン。
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// プールからオブジェクトを取得する。
        /// </summary>
        /// <returns>キャッシュから再利用されるオブジェクト。</returns>
        IPooledObject Get();

        /// <summary>
        /// プールからオブジェクトを取得する。
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">回転</param>
        /// <returns>キャッシュから再利用されるオブジェクト。</returns>
        IPooledObject Get(in Vector3 position, in Quaternion rotation);

        /// <summary>
        /// キャッシュされたオブジェクトを解放する。
        /// </summary>
        /// <param name="obj">解放するオブジェクト</param>
        void Release(IPooledObject obj);
    }
}