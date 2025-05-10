using System;
using Components.Mechanism;
using Core.ObjectPool;
using Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "LibraBridgeMold", menuName = "Actors/Molds/LibraBridgeMold")]

    public class RBLibraBridgeMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        public Vector3 Size;
        
        public Action<Vector3> OnMoldChange;

        private void OnValidate()
        {
            OnMoldChange?.Invoke(Size);
        }
        
        public void ApplyPrefabValues()
        {
            AssetUtils.TryLoadAsset(PrefabPoolInfoGetter.ObjectPath, out GameObject prefab);
            var bridge = prefab.GetComponent<RBLibraBridgeActor>();
            
            Size = bridge.transform.GetChild(0).GetChild(0).localScale;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            OnValidate();
        }
        
        [ContextMenu("Clear event list")]
        public void ClearEvents()
        {
            OnMoldChange = null;
        }
    }
}
