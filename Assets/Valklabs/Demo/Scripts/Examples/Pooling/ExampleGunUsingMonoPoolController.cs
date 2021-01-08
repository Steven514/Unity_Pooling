using UnityEngine;
using Valklabs.Util.Pooling;

namespace Valklabs.Examples.Pooling
{
    public class ExampleGunUsingMonoPoolController : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField] private float _fireIntervalTimer = 0.1f;

        [Header("Pool Settings")]
        [SerializeField] private PoolInitData<ExamplePooledBullet> _poolInitData;   //Not required, but faster to setup your settings for init

        private IPoolController<ExamplePooledBullet> _bulletPooler = null;

        private float _fireCooldown = 0f;

        private void Awake()
        {
            // This will call the Init(...) function within the constructor
            _bulletPooler = new MonoPoolController<ExamplePooledBullet>(_poolInitData.prefab,
                                                                        _poolInitData.pooledContainer,
                                                                        _poolInitData.maxPoolSize,
                                                                        _poolInitData.optionalOnCreatedUnityEvent,
                                                                        _poolInitData.optionalOnSpawnUnityEvent,
                                                                        _poolInitData.optionalOnDespawnUnityEvent);

            //Quick way commented out below to init if you dont care about max size, and extra UnityEvents
            //_bulletPooler = new MonoPoolController<ExamplePooledBullet>(_test.prefab, _test.pooledContainer);
        }


        private void Update()
        {
            _fireCooldown -= Time.deltaTime;
            if (Input.GetKey(KeyCode.Space) && _fireCooldown <= 0)
            {
                OnFireBullet();
            }
        }

        private void OnFireBullet()
        {
            //Override methods for Get() that allow for quick ways to set transform, rotations, etc
            //ExamplePooledBullet bullet = _bulletPooler?.Get();                                      //-- If you don't want to preset any pos/rot
            //ExamplePooledBullet bullet = _bulletPooler?.Get(transform.position);                    // -- If you want to preset pos, but not rot
            ExamplePooledBullet bullet = _bulletPooler?.Get(transform.position, transform.rotation);  // -- If you want to preset pos and rot
            //Make sure to do your sanity (null) check as it can return null if the pool stack is empty and you have a max pool count set.
            if (bullet != null)
            {
                _fireCooldown = _fireIntervalTimer;
                bullet.Fire();
            }
        }

        private void OnDestroy()
        {
            //Don't forget to cleanup your pooler(s) when no longer needing them
            _bulletPooler?.OnCleanUp();
            _bulletPooler = null;
        }
    }
}

