using UnityEngine;

namespace Nitou.Pool
{
    public interface IPooledObject
    {
        /// <summary>
        /// 対応するGameObjectを取得する。
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// オブジェクトを識別するためのインスタンスIDを取得する。
        /// </summary>
        int InstanceId { get; }

        
        /// <summary>
        /// オブジェクトが使用中かどうかを示す。
        /// </summary>
        bool IsUsed { get; }

        /// <summary>
        /// オブジェクトを初期化する。
        /// GameObjectPoolによって呼び出される。
        /// </summary>
        /// <param name="owner">プレハブを生成する所有者</param>
        /// <param name="hasRigidbody">プレハブが<see cref="Rigidbody"/>を持つ場合はtrue</param>
        void Initialize(IGameObjectPool owner, bool hasRigidbody);

        /// <summary>
        /// コンポーネントを解放する。
        /// </summary>
        void Release();
        
        /// <summary>
        /// オブジェクトが取得されたときに呼び出される。
        /// </summary>
        void OnGet();

        /// <summary>
        /// オブジェクトが解放されたときに呼び出される。
        /// </summary>
        void OnRelease();
    }
}