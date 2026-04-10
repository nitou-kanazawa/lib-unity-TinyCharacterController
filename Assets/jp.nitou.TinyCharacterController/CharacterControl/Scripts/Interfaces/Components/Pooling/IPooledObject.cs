using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    public interface IPooledObject
    {
        /// <summary>
        /// 対応する GameObject．
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// オブジェクトを識別するインスタンス ID．
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        /// オブジェクトが使用中の場合は True．
        /// </summary>
        bool IsUsed { get; }

        /// <summary>
        /// オブジェクトの初期化処理．
        /// GameObjectPool によって呼び出される．
        /// </summary>
        void Initialize(IGameObjectPool owner, bool hasRigidbody);

        /// <summary>
        /// コンポーネントを返却する．
        /// </summary>
        void Release();

        /// <summary>
        /// オブジェクトが取得されたときに呼び出される．
        /// </summary>
        void OnGet();

        /// <summary>
        /// オブジェクトが返却されたときに呼び出される．
        /// </summary>
        void OnRelease();
    }
}