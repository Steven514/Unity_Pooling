using System;
using UnityEngine;
using Valklabs.Util.Pooling;

namespace Valklabs.Examples.Pooling
{
    public class ExamplePooledBullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 5;

        private Action<IPoolable> _onReturnToPool;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnCreate()
        {
            Debug.Log($"A new instance of {gameObject.name} has been added to the pooler!");
        }


        public void OnSpawn(Action<IPoolable> onReturnToPool)
        {
            //REQUIRED: Always cache the onReturnToPool so we know which pool to send the object back to
            //This should be the first thing you do on any OnSpawn(...) IPoolable object
            _onReturnToPool = onReturnToPool;
    
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);

            //Stop current velocity
            if (_rb != null)
            {
                _rb.velocity = Vector3.zero;
            } 

            //REQUIRED: Always call the _onReturnToPool inside your OnDespawn. The order in which you call it can varry depending on your needs.
            _onReturnToPool?.Invoke(this);
        }

        public void Fire()
        {
            _rb?.AddForce(transform.forward * _speed, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            //For this example, don't despawn when we hit another bullet or if other is null.
            if(other == null || other.TryGetComponent(out ExamplePooledBullet otherBullet))
            {
                return;
            }

            OnDespawn();
        }
    }
}

