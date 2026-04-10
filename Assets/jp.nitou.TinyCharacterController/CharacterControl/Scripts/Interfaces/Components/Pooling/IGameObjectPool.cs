using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// GameObjectPool の登録用インターフェース．
    /// オブジェクトの取得・返却・破棄、およびマネージャーからの検索に使用する．
    /// </summary>
    public interface IGameObjectPool
    {
        /// <summary>
        /// プレハブ ID（検索に使用）．
        /// </summary>
        int PrefabID { get; }

        /// <summary>
        /// このコンポーネントが属するシーン．
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// オブジェクトプールからオブジェクトを取得する．
        /// </summary>
        IPooledObject Get();

        /// <summary>
        /// 位置と回転を指定してオブジェクトプールからオブジェクトを取得する．
        /// </summary>
        IPooledObject Get(in Vector3 position, in Quaternion rotation);

        /// <summary>
        /// キャッシュされたオブジェクトを返却する．
        /// </summary>
        void Release(IPooledObject obj);
    }
}