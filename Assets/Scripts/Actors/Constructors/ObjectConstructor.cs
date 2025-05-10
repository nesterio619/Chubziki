using Actors.Molds;
using Core.ObjectPool;
using Core.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Constructors
{
    public abstract class ObjectConstructor<ReturnT>
    {
        public abstract ReturnT Load(Mold moldType, Transform transform);

        protected GameObject TakeFromPool(Mold moldType, Transform transformPosition)
        {
            var instance = ObjectPooler.TakePooledGameObject(moldType.PrefabPoolInfoGetter);

            SetConstructedObjectTransform(instance.transform, transformPosition);
            
            return instance.gameObject;
        }

        protected GameObject InstantiateFromPool(PrefabPoolInfo poolInfo, Transform transformPosition)
        {
#if UNITY_EDITOR
            AssetUtils.TryLoadAsset(poolInfo.ObjectPath, out GameObject prefab);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
            GameObject instance = null;
            Debug.LogError("PrefabUtility.InstantiatePrefab is only supported in the Unity Editor.");
#endif
            SetConstructedObjectTransform(instance.transform, transformPosition);

            return instance;
        }

        private void SetConstructedObjectTransform(Transform instance, Transform transformPosition)
        {
            if (transformPosition == null)
                return;

            instance.transform.SetParent(transformPosition, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

    }
}