using System.Collections.Generic;
using Core;
using UnityEngine;
using Core.Utilities;
using Regions.BoundsInEditor;


namespace Regions
{
    public class Region : Sector
    {
        public List <Sector> Sectors;
        [HideInInspector] [SerializeField] private bool showRegion = false;

        public bool ShowRegion
        { 
            get => showRegion;
            set
            {
                showRegion = value;
#if UNITY_EDITOR
                ToggleDisplayBounds(value); 
#endif
            }
        }

        private void AddToRegionManager() => RegionManager.AddToRegions(this);

        protected override string MaterialPath => "Assets/Materials/EditorLocationBounds/RegionMaterial.mat";

        private void Awake()
        {
            if (Player.Instance != null)
            {
                Initialize();
                SceneManager.OnSceneChangeTriggered_BeforeAnimation_Event += Initialize;
            }
        }
        
        private new void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Location already initialized");
                return;
            }

            isInitialized = true;

            AddToRegionManager();
            
            if (LocationBounds == null)
            {
                LocationBounds = BoundsSceneElement.Create<SectorBounds>(gameObject.name, this, boundsMaterial, transform);
                LocationBounds.CreateMeshBounds();
            }

            Bounds = MeshUtilities.TransformBounds(LocationBounds.MyMeshFilter.sharedMesh.bounds, transform);

            //gameObject.SetActive(false);

            foreach (var sector in Sectors)
            {
                sector.SetRegion(this);
                sector.Initialize();
            }
        }
        

        public override void SwitchGraphics(bool stateToSet)
        {
            base.SwitchGraphics(stateToSet);
        }

        public override void Enter()
        {
            onEnter?.Invoke();
        }

        public override void Exit()
        {
            foreach (var sector in Sectors)
                sector.Dispose();
            
            onExit?.Invoke();
        }

        public override List<Bounds> GetAllBounds()
        {
            List <Bounds> collectedLocationBounds = new();
            
            if (Sectors == null) return collectedLocationBounds;
            
            foreach (var item in Sectors)
                collectedLocationBounds.Add(item.Bounds);

            return collectedLocationBounds;
        }

        public override void CalculateBounds(bool displayBounds = true)
        {
            if (Sectors != null)
                foreach (Sector sector in Sectors)
                    sector.CalculateBounds(displayBounds);

#if UNITY_EDITOR
            AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
            if (LocationBounds == null)
                LocationBounds = BoundsSceneElement.Create<RegionBounds>(gameObject.name, this, boundsMaterial, transform);
            if (LocationBounds == null) return;
            
            LocationBounds.CreateMeshBounds();

            Bounds = MeshUtilities.TransformBounds(LocationBounds.MyMeshFilter.sharedMesh.bounds, transform);
#if UNITY_EDITOR
            ToggleDisplayBounds(displayBounds);
#endif
        }
        
#if UNITY_EDITOR
        
        protected override void ReloadForEditor()
        {
            CreateEnvironmentParent();
            
            Sectors ??= new List <Sector>();
            
            CalculateBounds();
        }
#endif
    }
}