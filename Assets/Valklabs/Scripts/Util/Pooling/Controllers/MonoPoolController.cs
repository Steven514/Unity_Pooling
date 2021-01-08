using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valklabs.Util.Pooling
{
    public class MonoPoolController<T> : IPoolController<T> where T : MonoBehaviour, IPoolable
    {
        protected Stack<T> _pool;
        protected IList<T> _allInstances;               //Holds a reference of all the instances made (and pre made) from pooling. This is used for cleanup rather than just checking the stack, as we may want to request to cleanup while a pooled instance is no longer in the stack, ensuring we don't miss that instance.
                                                       
        protected T _prefab;                            //The prefab that will be created. Assigned from InitPool
        protected int _maxPooledObjectsCount = 0;       //Set the max amount of pooled objects allowed to be created during run time on Get(). Value of 0 or less will remove any max amount limit. This will be automatically increased if preexisting pooled objects are being added to pool and have a higher count than max.
        protected Transform _poolContainer;             //The container the pooled objects will reside in the scene hierarchy when inactive. If intending to have pooled objects live from scene-to-scene, make sure the parent (and this Pool Controller) are on objects that 'dont destroy on load'.)

        protected UnityEvent _onCreatedUnityEvent;      //triggers extra events when an instance is created. Can be left null.
        protected UnityEvent _onSpawnUnityEvent;        //triggers extra events when an instance is pooled/spawned. Can be left null.
        protected UnityEvent _onDespawnUnityEvent;      //triggers extra events when an instance is returned to pool/despawned. Can be left null.

        #region Init
        /// <summary>
        /// Initialize The Pool.
        /// </summary>
        /// <param name="prefab">The prefab this pooler will be pooling for creation and spawning.</param>
        /// <param name="pooledContainer">The container/object that the pooled instances will exist inside of.</param>
        /// <param name="maxCount">The max amount of instance this pool controller can create. A value of 0 or less will remove any max count. If setting a max count over 0, and adding pre existing instances of poolable objects to the pool, the max count will make sure to auto size to be atleast the value of the pre existing count to be pooled.</param>
        /// <param name="onCreatedUnityEvent">Can be left null. Otherwise, pass in some UnityEvents that happen when a new instance is created for pooling.</param>
        /// <param name="onSpawnUnityEvent">Can be left null. Otherwise, pass in some UnityEvents that happen when an instance is pooled.</param>
        /// <param name="onDespawnUnityEvent">Can be left null. Otherwise, pass in some UnityEvents that happen when an instance is returned to pool.</param>
        public MonoPoolController(T prefab, Transform pooledContainer, int maxCount = 0, UnityEvent onCreatedUnityEvent = null, UnityEvent onSpawnUnityEvent = null, UnityEvent onDespawnUnityEvent = null)
        {
            Init(prefab, pooledContainer, maxCount, onCreatedUnityEvent, onSpawnUnityEvent, onDespawnUnityEvent);
        }

        protected virtual void Init(T prefab, Transform pooledContainer, int maxCount, UnityEvent onCreatedUnityEvent, UnityEvent onSpawnUnityEvent, UnityEvent onDespawnUnityEvent)
        {
            if (prefab == null)
            {
                Debug.LogError("Pooling prefab is null!");
                return;
            }

            if (pooledContainer == null)
            {
                Debug.LogError("Pool Container is null!");
                return;
            }

            _prefab = prefab;
            _poolContainer = pooledContainer;
            _maxPooledObjectsCount = maxCount;
            _onCreatedUnityEvent = onCreatedUnityEvent;
            _onSpawnUnityEvent = onSpawnUnityEvent;
            _onDespawnUnityEvent = onDespawnUnityEvent;
            _allInstances = new List<T>();
            _pool = new Stack<T>();

            AddPrecreatedInstancesToPoolable();
        }

        /// <summary>
        /// Checks for any pre created instances in children of _poolContainer in this scene on init, then adds it to the pool
        /// </summary>
        protected virtual void AddPrecreatedInstancesToPoolable()
        {
            if (_poolContainer == null)
            {
                return;
            }   

            //Get all the instances of this pooled type already created in scene to auto add them to the poolable list 
            T[] instances = _poolContainer.GetComponentsInChildren<T>(true);

            //Any pre-created instances will be added to the pooled stack for poolable objects
            if (instances != null)
            {
                //We've set our max pooled count to be lower than the amount of currently created instances. Increase the max pooled count to the amount already created in scene.
                //Only readjust max count if we've said we want a max count
                if (_maxPooledObjectsCount > 0 && _maxPooledObjectsCount < instances.Length)
                {
                    _maxPooledObjectsCount = instances.Length;
                }

                foreach (T instance in instances)
                {
                    if (instance != null)
                    {
                        OnPrePopulatedPooledItemInit(instance);
                    }
                }
            }
        }

        protected virtual void OnPrePopulatedPooledItemInit(T instance)
        {
            instance.gameObject.SetActive(false);
            OnAddingNewInstance(instance);
            Add(instance);
        }
        #endregion

        #region Create
        protected virtual T CreateNewInstance()
        {
            if (_prefab == null)
            {
                return null;
            }

            T instance = MonoBehaviour.Instantiate(_prefab, _poolContainer);
            if (instance != null)
            {
                OnAddingNewInstance(instance);
            }
            return instance;
        }

        protected virtual void OnAddingNewInstance(T instance)
        {
            AddToInstanceList(instance);
            instance?.OnCreate();

            //Trigger an event on the pool controller when a new instance is created.
            _onCreatedUnityEvent?.Invoke();
        }

        private bool IsAllowedToCreateInstance()
        {
            if (_maxPooledObjectsCount <= 0 || _allInstances.Count < _maxPooledObjectsCount)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Stack Commands
        protected void Add(T instance)
        {
            if (instance == null || _pool == null)
            {
                Debug.LogError("instance and/or poolstack is null -- can't add to pool");
                return;
            }

            _pool.Push(instance);
        }

        protected T Remove()
        {
            if (_pool == null || _pool.Count == 0)
            {
                return null;
            }

            return _pool.Pop();
        }
        #endregion

        #region Instance List
        protected void AddToInstanceList(T instance)
        {
            instance.transform.SetParent(_poolContainer);
            _allInstances.Add(instance);
        }
        #endregion

        #region Get Pooled Object
        /// <summary>
        /// Gets an instance of the pooled object from the stack.
        /// </summary>
        public virtual T Get()
        {
            if (_pool == null)
            {
                Debug.LogError("_poolstack is null, cannot get!");
                return null;
            }

            T instance = Remove();
            if (instance == null && IsAllowedToCreateInstance() == true) //No available pooled objects. Create new instance if allowed
            {
                instance = CreateNewInstance();
            }

            if (instance != null)
            {
                //Make aware of what pool to return to on despawn when activating
                instance.OnSpawn(OnReturn);

                //Trigger an event on the pool controller when a an instance is spawned.
                _onSpawnUnityEvent?.Invoke();
            }

            return instance;
        }

        /// <summary>
        /// Gets an instance of the pooled object from the stack. Sets location without changing the parent. Consider 'Get(Transform spawnAtTransform, bool isWorldPositionStays)' if you require the pooled object to follow another object.
        /// </summary>
        /// <param name="spawnAt">The location the object will spawn at.</param>
        public virtual T Get(Vector3 spawnAt)
        {
            T instance = Get();
            if (instance != null)
            {
                instance.transform.position = spawnAt;
            }

            return instance;
        }

        /// <summary>
        /// Gets an instance of the pooled object from the stack. Sets location without changing the parent. Consider 'Get(Transform spawnAtTransform, bool isWorldPositionStays, Quaternion rotation)' if you require the pooled object to follow another object.
        /// </summary>
        /// <param name="spawnAt">The location the object will spawn at.</param>
        /// <param name="rotation">The rotation the object will spawn with.</param>
        public virtual T Get(Vector3 spawnAt, Quaternion rotation)
        {
            T instance = Get(spawnAt);
            if (instance != null)
            {
                instance.transform.rotation = rotation;
            }
            return instance;
        }

        /// <summary>
        /// Gets an instance of the pooled object from the stack. Adds object to another object's transform. Consider 'Get(Vector3 spawnAt)' if you don't require the pooled object to follow another transform.
        /// </summary>
        /// <param name="spawnAtTransform">The transform requesting to spawn this object. The spawned object will set this transform as it's parent and zero out it's local position.</param>
        /// <param name="isWorldPositionStays">the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before.</param>
        public virtual T Get(Transform spawnAtTransform, bool isWorldPositionStays)
        {
            T instance = Get();
            if (instance != null)
            {
                instance.transform.SetParent(spawnAtTransform, isWorldPositionStays);
                instance.transform.localPosition = Vector3.zero;
            }

            return instance;
        }

        /// <summary>
        /// Gets an instance of the pooled object from the stack. Adds object to another object's transform. Consider 'Get(Vector3 spawnAt, Quaternion rotation)' if you don't require the pooled object to follow another transform.
        /// </summary>
        /// <param name="spawnAtTransform">The transform requesting to spawn this object. The spawned object will set this transform as it's parent and zero out it's local position.</param>
        /// <param name="isWorldPositionStays">the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before.</param>
        /// <param name="rotation">The rotation the object will spawn with. if 'isWorldPositionStays' is true, this will be applied after; resulting in this rotation.</param>
        public virtual T Get(Transform spawnAtTransform, bool isWorldPositionStays, Quaternion rotation)
        {
            T instance = Get();
            if (instance != null)
            {
                instance.transform.SetParent(spawnAtTransform, isWorldPositionStays);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.rotation = rotation;
            }

            return instance;
        }
        #endregion

        #region Callbacks
        protected virtual void OnReturn(IPoolable poolable)
        {
            if (poolable == null)
            {
                return;
            }

            T instance = poolable as T;
            if (instance == null)
            {
                return;
            }
               
            instance.transform.SetParent(_poolContainer);
            Add(instance);

            //Trigger an event on the pool controller when a an instance is despawned.
            _onDespawnUnityEvent?.Invoke();
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Cleans up memory when no longer needing this MonoPoolController and it's pooled instances. Destroys the pooled instances.
        /// </summary>
        public virtual void OnCleanUp()
        {
            CleanPoolInstances();

            _onCreatedUnityEvent = null;
            _onSpawnUnityEvent = null;
            _onDespawnUnityEvent = null;

            _pool?.Clear();
            _pool = null;
            _allInstances?.Clear();
            _allInstances = null;
        }

        protected void CleanPoolInstances()
        {
            if (_allInstances == null)
            {
                return;
            }

            foreach (T item in _allInstances)
            {
                if (item != null)
                {
                    MonoBehaviour.Destroy(item.gameObject);
                }
            }
        }
        #endregion
    }
}

