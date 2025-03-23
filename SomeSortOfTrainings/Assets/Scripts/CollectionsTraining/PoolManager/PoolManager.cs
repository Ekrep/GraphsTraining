using System.Collections.Generic;
using UnityEngine;
using MyPoolable = PoolSystem.Poolable.IPoolable;

namespace PoolSystem
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;
        private Dictionary<string, Queue<MonoBehaviour>> _pools = new();
        private Dictionary<string, MonoBehaviour> _prefabPools = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void CreatePool<T>(string poolName, T poolObject, int amount, Transform poolParentObject) where T : MonoBehaviour, MyPoolable
        {
            if (_pools.ContainsKey(poolName))
            {
                var createdObject = Instantiate(poolObject, poolParentObject);
                AssignItemToPool(poolName, createdObject);
            }
            else
            {
                Queue<T> pool = new();
                for (int i = 0; i < amount; i++)
                {
                    var createdObject = Instantiate(poolObject, poolParentObject);
                    pool.Enqueue(createdObject);
                    createdObject.OnCreatedForPool();
                    createdObject.OnAssignPool();
                }
                _pools[poolName] = new Queue<MonoBehaviour>(pool);
                _prefabPools[poolName] = poolObject;
            }
        }

        public void RemovePool(string poolName)
        {
            _pools.Remove(poolName);
            _prefabPools.Remove(poolName);
        }

        public void DeletePool<T>(string poolName) where T : MonoBehaviour, MyPoolable
        {
            if (!_pools.ContainsKey(poolName)) return;

            foreach (var obj in _pools[poolName])
            {
                if (obj is T castedObj)
                {
                    castedObj.OnDeletePool();
                    Destroy(castedObj);
                }
            }

            _pools.Remove(poolName);
            _prefabPools.Remove(poolName);
        }

        public Queue<T> GetPool<T>(string poolName) where T : MonoBehaviour, MyPoolable
        {
            if (!_pools.TryGetValue(poolName, out var existingPool))
            {
                Debug.LogError("Pool doesn't exist");
                return null;
            }

            Queue<T> typedPool = new();
            foreach (var obj in existingPool)
            {
                if (obj is T castedObj)
                    typedPool.Enqueue(castedObj);
                else
                    Debug.LogWarning($"Object in pool {poolName} cannot be cast to type {typeof(T)}");
            }
            return typedPool;
        }

        public void DebugPool()
        {
            foreach (var pool in _pools)
            {
                Debug.Log(pool.Key);
                Debug.Log(pool.Value.GetType());
                Debug.Log(pool.Value.Count + "Pool Count");
            }
        }

        public T DequeueItemFromPool<T>(string poolName, Transform parentObjWhenInstantiated = null) where T : MonoBehaviour, MyPoolable
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError("Pool doesn't exist");
                return null;
            }

            if (pool.TryDequeue(out var result) && result is T pooleable)
            {
                pooleable.OnDequeuePool();
                pooleable.transform.SetParent(parentObjWhenInstantiated);
                return pooleable;
            }
            else
            {
                var instantiatedObject = Instantiate(_prefabPools[poolName], parentObjWhenInstantiated);
                if (instantiatedObject is T newPooleable)
                {
                    newPooleable.OnCreatedForPool();//if it's bugged because of this!
                    newPooleable.OnDequeuePool();
                    return newPooleable;
                }
                return null;
            }
        }

        private void AssignItemToPool<T>(string poolName, T item) where T : MonoBehaviour, MyPoolable
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError("Pool doesn't exist");
                return;
            }

            item.OnAssignPool();
            _pools[poolName].Enqueue(item);
        }
        public void EnqueueItemToPool<T>(string poolName, T item) where T : MonoBehaviour, MyPoolable
        {
            if (!_pools.ContainsKey(poolName))
            {
                Debug.LogError("Pool doesn't exist");
                return;
            }
            item.OnEnqueuePool();
            _pools[poolName].Enqueue(item);

        }
    }
}
