using Core.Utilities;
using Regions.BoundsInEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Regions
{
    public class Sector : Location
    {
        public List<Location> Locations;

        [HideInInspector] [SerializeField] internal UnityEvent onEnter = null;

        [HideInInspector] [SerializeField] internal UnityEvent onExit = null;

        public Region MyRegion { get; private set; }
        
        [SerializeField] public SectorNavMeshBaker navMeshBaker;
        
        
        [HideInInspector] [SerializeField] private bool showSector = false;

        public bool ShowSector 
        { 
            get { return showSector; } 
            set 
            { 
                showSector = value; 
                #if UNITY_EDITOR
                ToggleDisplayBounds(value); 
                #endif
            } 
        }


        protected override string MaterialPath => "Assets/Materials/EditorLocationBounds/SectorMaterial.mat";
        
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Sector already initialized");
                return;
            }

            isInitialized = true;

            SwitchGraphics(false);

            if (LocationBounds == null)
            {
                LocationBounds = BoundsSceneElement.Create<SectorBounds>(gameObject.name, this, boundsMaterial, transform);
                LocationBounds.CreateMeshBounds();
            }
            
            Bounds = MeshUtilities.TransformBounds(LocationBounds.MyMeshFilter.sharedMesh.bounds, transform);

            foreach (var location in Locations)
                location.Initialize(this);
        }

        public void SetRegion(Region region)
        {
            if (MyRegion == null)
                MyRegion = region;
        }

        protected internal override void Load()
        {
            if (IsLoaded)
                return;

            IsLoaded = true;

            foreach (var location in Locations)
                location.Load();
        }

        public override void Dispose()
        {
            onEnter.RemoveAllListeners();
            onExit.RemoveAllListeners();

            foreach (var location in Locations)
                location.Dispose();

            base.Dispose();
        }

        public override void Enter()
        {
            onEnter?.Invoke();
        }

        public override void Exit()
        {
            onExit?.Invoke();
        }

        public override List<Bounds> GetAllBounds()
        {
            List <Bounds> collectedLocationBounds = new List <Bounds>(base.GetAllBounds());

            if(Locations == null) return collectedLocationBounds;
            
            foreach (var item in Locations)
            {
                var mesh = item.GetComponentInChildren<LocationBounds>().GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                    collectedLocationBounds.Add(MeshUtilities.TransformBounds(mesh.bounds, item.transform));
            }

            return collectedLocationBounds;
        }

        public override void CalculateBounds(bool displayBounds = true)
        {

            if (Locations != null)
                foreach (Location location in Locations)
                    location.CalculateBounds(displayBounds);
#if UNITY_EDITOR
            AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
            if (LocationBounds == null)
                LocationBounds = BoundsSceneElement.Create<SectorBounds>(gameObject.name, this, boundsMaterial, transform);
            
            if (LocationBounds == null || environmentParent.childCount == 0) return;
            
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
            
            Locations ??= new List<Location>();
            
            CalculateBounds();
        }

#endif
    }
}