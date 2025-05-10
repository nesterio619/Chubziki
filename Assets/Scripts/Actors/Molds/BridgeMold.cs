using Components.Mechanism;
using Core.Utilities;
using DG.Tweening;
using System;

using UnityEngine;
using static Components.Mechanism.BridgeActor;
using Core.ObjectPool;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "BridgeMold", menuName = "Actors/Molds/BridgeMold")]
    public class BridgeMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        [HideInInspector] public bool DisplayGizmos;
        public MoveType BridgeType;
        [Space(7)]
        public Vector3 Size;

        [Tooltip("Only for bridges with 2 parts")]
        public float ZSpacing;

        [Space(7)]
        [Tooltip("For Drawbridges - target X rotation\nFor Slidebridges - target Z offset")]
        public float TargetValue;

        public bool StartOpened;

        [Space(7)]
        public float MoveDuration;
        public Ease MoveEase;

        public Action<Vector3,float,float, bool> OnMoldChange;

        private void OnValidate()
        {
            OnMoldChange?.Invoke(Size, ZSpacing, TargetValue, DisplayGizmos);
        }

        public void ApplyPrefabValues()
        {
            AssetUtils.TryLoadAsset(PrefabPoolInfoGetter.ObjectPath, out GameObject prefab);
            var bridge = prefab.GetComponent<BridgeActor>();

            BridgeType = bridge.BridgeType;
            ZSpacing = bridge.ZSpacing;
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
