using System;
using UnityEngine;
using Nitou.TCC;
// using Nitou.Pool;
using uPools;

public sealed class DemoRifleController : MonoBehaviour
{
    [SerializeField] Transform _player;
    // [SerializeField] GameObjectPool _pool;
    // [SerializeField] PooledGameObject _prefab;
    
    [SerializeField ] GameObject _prefab;
    
    private GameObjectPool _pool;
    
    [SerializeField] float _speed = 1000;

    private void Start()
    {
        SharedGameObjectPool.Prewarm(_prefab,10);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var bullet = SharedGameObjectPool.Rent(_prefab);
            // var bullet = GameObjectPool.Get(_prefab);
            bullet.transform.position = _player.position + Vector3.up;
            bullet.transform.rotation = _player.rotation;
            
            var rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(_speed * _player.forward);
        }
    }
}