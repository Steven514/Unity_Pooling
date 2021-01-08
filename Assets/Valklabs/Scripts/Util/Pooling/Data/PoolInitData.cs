using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valklabs.Util.Pooling
{
    [Serializable]
    public class PoolInitData<T> where T : MonoBehaviour, IPoolable
    {
        [Header("The object being pooled.")]
        [SerializeField] private T _prefab;

        [Header("The transform the pooled objects will sit in once inactive.")]
        [SerializeField] private Transform _pooledContainer;

        [Header("The max amount of pooled objects. A value of 0 or less will ignore any max pool size.")]
        [SerializeField] private int _maxPoolSize = 0;                          //Values of 0 or less will make the pool ignore a max count. Setting this higher than 0 will make it so the pooler will not create more instances once reaching that count.

        [Header("Trigger custom events on the pool controller when an instance is created.")]
        [SerializeField] private UnityEvent _optionalOnCreatedUnityEvent;       //Optional. Can be left null.

        [Header("Trigger custom events on the pool controller when an instance is spawned.")]
        [SerializeField] private UnityEvent _optionalOnSpawnUnityEvent;         //Optional. Can be left null.

        [Header("Trigger custom events on the pool controller when an instance is despawned.")]
        [SerializeField] private UnityEvent _optionalOnDespawnUnityEvent;       //Optional. Can be left null.

        public T prefab => _prefab;
        public Transform pooledContainer => _pooledContainer;
        public int maxPoolSize => _maxPoolSize;
        public UnityEvent optionalOnCreatedUnityEvent => _optionalOnCreatedUnityEvent;
        public UnityEvent optionalOnSpawnUnityEvent => _optionalOnSpawnUnityEvent;
        public UnityEvent optionalOnDespawnUnityEvent => _optionalOnDespawnUnityEvent;

    }
}
