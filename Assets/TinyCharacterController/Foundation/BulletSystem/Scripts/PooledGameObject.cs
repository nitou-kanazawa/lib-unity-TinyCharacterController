using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Nitou.BatchProcessor;
using Sirenix.OdinInspector;

namespace Nitou.Utility
{
    /// <summary>
    /// A component used to manage objects that are pooled by GameObjectPool.
    /// Can be obtained with <see cref="IGameObjectPool.Get()"/> and released with <see cref="IGameObjectPool.Release(IPooledObject)"/>. 
    /// </summary>
    /// <seealso cref="GameObjectPool"/>
    [AddComponentMenu("Nitou/Pool/" + nameof(PooledGameObject))]
    public class PooledGameObject : ComponentBase, IPooledObject
    {
        /// <summary>
        /// Determines whether to use the lifetime feature.
        /// </summary>
        [DisableInPlayMode]
        [SerializeField] private bool _isUseLifetime;

        /// <summary>
        /// The duration for which the object remains valid.
        /// <see cref="_isUseLifetime"/> が有効な時に使用されます．
        /// </summary>
        [DisableInPlayMode]
        [SerializeField] private float _lifeTime;

        /// <summary>
        /// Only available when `_isUseLifetime` is enabled.
        /// Callback invoked when the object is released.
        /// </summary>
        public UnityEvent OnReleaseByLifeTime = new();

        private IGameObjectPool _owner;
        private Rigidbody _rigidbody;
        private bool _hasRigidbody;
        private int _instanceId = 0;
        private bool _initializedInstanceId = false;


        /// <summary>
        /// Returns true if the PooledGameObject is used.
        /// </summary>
        public bool IsUsed { get; private set; }

        /// <summary>
        /// Time at which the PooledGameObject is released.
        /// </summary>
        public float ReleaseTime { get; private set; }

        /// <summary>
        /// Returns true if the PooledGameObject is currently playing.
        /// </summary>
        public bool IsPlaying => IsUsed && ((_isUseLifetime && Time.timeSinceLevelLoad > ReleaseTime) || !_isUseLifetime);

        /// <summary>
        /// Instance ID.
        /// </summary>
        int IPooledObject.InstanceId
        {
            get
            {
                if (_initializedInstanceId)
                    return _instanceId;

                _instanceId = gameObject.GetInstanceID();
                _initializedInstanceId = true;
                return _instanceId;
            }
        }

        GameObject IPooledObject.GameObject => gameObject;

        
        #region Public Method

        /// <summary>
        /// Initializes the element when retrieved from the pool by GameObjectPool.
        /// </summary>
        /// <param name="owner">The owner</param>
        /// <param name="hasRigidbody">Determines if the object has a Rigidbody</param>
        void IPooledObject.Initialize(IGameObjectPool owner, bool hasRigidbody)
        {
            _owner = owner;
            _hasRigidbody = hasRigidbody;
            if (hasRigidbody)
                TryGetComponent(out _rigidbody);
        }

        /// <summary>
        /// Releases the object back to the pool.
        /// </summary>
        public void Release()
        {
            if (IsUsed && _owner != null && gameObject != null)
                _owner.Release(this);
        }

        /// <summary>
        /// Callbacks when GameObjects are retrieved or created from the pool
        /// </summary>
        void IPooledObject.OnGet()
        {
            // If there is a Rigidbody, initialize its velocity and angular velocity.
            if (_hasRigidbody && _rigidbody.isKinematic == false)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            // If Lifetime is enabled, calculate the release time.
            if (_isUseLifetime)
            {
                ReleaseTime = _lifeTime + Time.timeSinceLevelLoad;
                PooledGameObjectSystem.Register(this, UpdateTiming.Update);
            }

            IsUsed = true;
        }

        /// <summary>
        /// Callbacks for returning GameObjects to the pool
        /// </summary>
        void IPooledObject.OnRelease()
        {
            // If Lifetime is enabled, unregister the element.
            if (_isUseLifetime)
            {
                PooledGameObjectSystem.Unregister(this, UpdateTiming.Update);
            }

            IsUsed = false;
        }

        #endregion
    }
}