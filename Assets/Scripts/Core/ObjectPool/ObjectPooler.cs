using Components.ProjectileSystem;
using Core.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.ObjectPool
{
    public static class ObjectPooler
    {
        private static readonly Dictionary<string, ObjectPool<GameObject>> ActiveObjectPools = new();
        private static readonly Dictionary<string, PrefabPoolInfo> ActiveObjectPoolsInfos = new();

        private static readonly Dictionary<string, Transform> PoolParents = new();
        private const string PooledObjectsParent = "PooledObjects";

        private static Transform GetPooledGameObjectsParent
        {
            get
            {
                if (pooledGameObjectsParent == null)
                {
                    pooledGameObjectsParent = GameObject.Find(PooledObjectsParent).transform;

                    if (pooledGameObjectsParent == null)
                    {
                        Debug.LogWarning("Cant find PooledObjectsParent");
                        return null;
                    }
                }

                return pooledGameObjectsParent;
            }
        }
        private static Transform pooledGameObjectsParent;


        public static bool PoolExist(string poolName) => ActiveObjectPoolsInfos.ContainsKey(poolName);
        private static void CreateParentForPool(string poolName)
        {
            if (PoolParents.ContainsKey(poolName))
                PoolParents[poolName] = new GameObject(poolName).transform;
            else
                PoolParents.Add(poolName, new GameObject(poolName).transform);

            if (!PoolParents.TryGetValue(poolName, out Transform createdParent))
            {
                Debug.LogError("Couldn't get a parent for pool");
                return;
            }

            createdParent.transform.SetParent(GetPooledGameObjectsParent, false);
            createdParent.transform.localScale = Vector3.one;
        }

        public static void CreatePool(PrefabPoolInfo _PrefabPoolInfo)
        {
            if (PoolExist(_PrefabPoolInfo.PoolName))
                return;

            if (!PoolParents.ContainsKey(_PrefabPoolInfo.PoolName))
                CreateParentForPool(_PrefabPoolInfo.PoolName);

            var pool = new ObjectPool<GameObject>
            (
                createFunc: () => CreatePooledObject(_PrefabPoolInfo),

                actionOnGet: obj => obj.SetActive(true),

                actionOnRelease: obj => obj.SetActive(false),

                actionOnDestroy: obj => UnityEngine.Object.Destroy(obj),

                defaultCapacity: _PrefabPoolInfo.DefaultAmount,

                maxSize: Mathf.CeilToInt(_PrefabPoolInfo.DefaultAmount * _PrefabPoolInfo.MaxAmountMultiplier)
            );

            ActiveObjectPoolsInfos.Add(_PrefabPoolInfo.PoolName, _PrefabPoolInfo);
            ActiveObjectPools.Add(_PrefabPoolInfo.PoolName, pool);
        }

        private static GameObject CreatePooledObject(PrefabPoolInfo poolInfo_PrefabPoolInfo)
        {
            if (!AssetUtils.TryLoadAsset(poolInfo_PrefabPoolInfo.ObjectPath, out GameObject loadedObjectAsset))
                return null;


            PoolParents.TryGetValue(poolInfo_PrefabPoolInfo.PoolName, out var parent);

            if (parent == null)
            {
                CreateParentForPool(poolInfo_PrefabPoolInfo.PoolName);
                if (!PoolParents.TryGetValue(poolInfo_PrefabPoolInfo.PoolName, out parent))
                    return null;
            }

            var instance = UnityEngine.Object.Instantiate(loadedObjectAsset, parent.transform, true);
            if (!instance.TryGetComponent(out PooledGameObject pooledObject))
            {
                Debug.LogError("Created object does not have the PooledGameObject component or it couldn't be found. Destroying the created GameObject");
                UnityEngine.Object.Destroy(instance);
            }

            pooledObject.poolName = poolInfo_PrefabPoolInfo.PoolName;
            instance.SetActive(false);

            instance.transform.localScale = Vector3.one;

            if (!instance.name.Contains("_Pooled"))
                instance.name += "_Pooled";

            return instance;
        }

        public static PooledGameObject TakePooledGameObject(PrefabPoolInfo poolInfo_PrefabPoolInfo, Transform transformToSet = null)
        {
            GameObject pooledGameObject = GetPooledGameObject(poolInfo_PrefabPoolInfo, transformToSet);
            return pooledGameObject == null ? null : pooledGameObject.GetComponent<PooledGameObject>();
        }

        public static Projectile TakePooledProjectile(PrefabPoolInfo poolInfo_PrefabPoolInfo, Transform transformToSet = null)
        {
            GameObject pooledGameObject = GetPooledGameObject(poolInfo_PrefabPoolInfo, transformToSet);
            return pooledGameObject == null ? null : pooledGameObject.GetComponent<Projectile>();
        }

        private static GameObject GetPooledGameObject(PrefabPoolInfo poolInfo_PrefabPoolInfo, Transform transformToSet = null)
        {
            if (poolInfo_PrefabPoolInfo == null)
            {
                Debug.LogWarning("Pool is null.");
                return null;
            }

            if (!PoolExist(poolInfo_PrefabPoolInfo.PoolName)) CreatePool(poolInfo_PrefabPoolInfo);

            if (!ActiveObjectPools.TryGetValue(poolInfo_PrefabPoolInfo.PoolName, out var pool))
            {
                //Theoretically impossible, but this validation is required to protect against possible further changes
                Debug.LogWarning($"No active pool found with name: {poolInfo_PrefabPoolInfo.PoolName}");
                return null;
            }

            var pooledGameObject = pool.Get();

            if (pooledGameObject == null)
            {
                Debug.LogWarning($"No pooled object available in pool: {poolInfo_PrefabPoolInfo.PoolName}");
                return null;
            }

            // Set the position, rotation, and scale based on transformToSet
            if (transformToSet != null)
            {
                pooledGameObject.transform.position = transformToSet.position;
                pooledGameObject.transform.rotation = transformToSet.rotation;
                pooledGameObject.transform.localScale = transformToSet.localScale;
            }

            if (!PoolParents.ContainsKey(poolInfo_PrefabPoolInfo.PoolName))
            {
                PoolParents.Add(poolInfo_PrefabPoolInfo.PoolName, new GameObject(poolInfo_PrefabPoolInfo.PoolName).transform);
                PoolParents.TryGetValue(poolInfo_PrefabPoolInfo.PoolName, out Transform createdParent);
                createdParent.transform.SetParent(pooledGameObjectsParent);
                createdParent.transform.localScale = Vector3.one;
            }

            PoolParents.TryGetValue(poolInfo_PrefabPoolInfo.PoolName, out Transform parent);

            pooledGameObject.transform.SetParent(parent.transform);
            pooledGameObject.transform.localScale = Vector3.one;

            if (!pooledGameObject.name.Contains("_Pooled"))
                pooledGameObject.name = pooledGameObject.name + "_Pooled";

            return pooledGameObject;
        }

        internal static void ReturnPooledObject(PooledGameObject pooledObjectToReturn)
        {
            if (pooledObjectToReturn == null)
                return;

            if (!ActiveObjectPools.TryGetValue(pooledObjectToReturn.poolName, out var pool))
            {
                Debug.LogWarning($"No existing pool {pooledObjectToReturn.gameObject.name} has been destroyed");
                UnityEngine.Object.Destroy(pooledObjectToReturn.gameObject);
                return;
            }

            PoolParents.TryGetValue(pooledObjectToReturn.poolName, out Transform parent);

            if(parent != null && !pooledObjectToReturn.IsBeingDestroyed)
                pooledObjectToReturn.transform.SetParent(parent.transform);

            pool.Release(pooledObjectToReturn.gameObject);
        }

        public static void Clear()
        {
            PoolParents.Clear();
            ActiveObjectPools.Clear();
            ActiveObjectPoolsInfos.Clear();
        }

    }
}
