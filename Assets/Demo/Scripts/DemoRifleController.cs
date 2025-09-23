using UnityEngine;
using Nitou.TCC;
using Nitou.Utility;

public sealed class DemoRifleController : MonoBehaviour
{
    [SerializeField] Transform _player;
    [SerializeField] GameObjectPool _pool;
    [SerializeField] PooledGameObject _prefab;
    
    [SerializeField] float _speed = 1000;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var bullet = GameObjectPool.Get(_prefab);
            bullet.transform.position = _player.position + Vector3.up;
            bullet.transform.rotation = _player.rotation;
            
            var rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(_speed * _player.forward);
        }
    }
}