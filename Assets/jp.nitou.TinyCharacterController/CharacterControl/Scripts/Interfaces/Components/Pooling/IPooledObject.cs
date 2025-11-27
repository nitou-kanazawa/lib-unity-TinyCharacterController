using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    public interface IPooledObject
    {
        /// <summary>
        /// The corresponding GameObject.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// An instance ID to identify the object.
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        /// True if the object is used.
        /// </summary>
        bool IsUsed { get; }

        /// <summary>
        /// オブジェクトの初期化処理.
        /// GameObjectPoolによって呼び出される.
        /// </summary>
        void Initialize(IGameObjectPool owner, bool hasRigidbody);

        /// <summary>
        /// Release the component.
        /// </summary>
        void Release();

        /// <summary>
        /// Called when the object is retrieved.
        /// </summary>
        void OnGet();

        /// <summary>
        /// Called when the object is released.
        /// </summary>
        void OnRelease();
    }
}