using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// An interface for registration in <see cref="TCC.Manager.GameObjectPoolManager"/>.
    /// Used for constructing GameObject pools.
    /// Utilized for object acquisition, return, destruction, and also for searching objects from the manager.
    /// </summary>
    public interface IGameObjectPool
    {
        /// <summary>
        /// Prefab ID (used for searching).
        /// </summary>
        int PrefabID { get; }

        /// <summary>
        /// The scene to which the component belongs.
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// Retrieve an object from the object pool.
        /// </summary>
        IPooledObject Get();

        /// <summary>
        /// Retrieve an object from the object pool.
        /// </summary>
        IPooledObject Get(in Vector3 position, in Quaternion rotation);

        /// <summary>
        /// Release a cached object.
        /// </summary>
        void Release(IPooledObject obj);
    }
}