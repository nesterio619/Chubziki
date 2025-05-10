using Actors.EditorActors;
using Core.Enums;
using Core.Utilities;
using UnityEngine;

namespace Regions.BoundsInEditor
{
    public abstract class BoundsSceneElement : EditorSceneElement
    {
        [SerializeField] protected Location MyLocation;

        [SerializeField] public MeshFilter MyMeshFilter;

        public MeshRenderer MyMeshRenderer;

        public bool IsVisibleBounds
        {
            get
            {
                if (MyMeshRenderer != null) 
                    return MyMeshRenderer.enabled;
            
                Debug.LogWarning("MyMeshRenderer is null so return false");
                return false;
            }
        }

#if UNITY_EDITOR

        protected override void Start()
        {
            base.Start();
            SwitchVisible(false);
        }

        public void SwitchVisible(bool value)
        {
            if (MyMeshRenderer == null)
                MyMeshRenderer = GetComponent<MeshRenderer>();

            if (MyMeshRenderer == null)
            {
                Debug.LogWarning(gameObject.name + " have no MeshRenderer");
                return;
            }

            MyMeshRenderer.enabled = value;
        }
#endif

        public abstract void CreateMeshBounds();

        public static T Create<T>(string name, Location location, Material material = null, Transform parent = null) where T : BoundsSceneElement
        {
            var createdBoundsSceneElement = new GameObject("BoundsMesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(T)).GetComponent<T>();

            createdBoundsSceneElement.transform.SetParent(parent);

            createdBoundsSceneElement.transform.localPosition = Vector3.zero;

            createdBoundsSceneElement.MyLocation = location;

            createdBoundsSceneElement.MyMeshFilter = createdBoundsSceneElement.GetComponent<MeshFilter>();

            if(material != null)
                createdBoundsSceneElement.GetComponent<Renderer>().material = material;

            createdBoundsSceneElement.gameObject.layer = UnityLayers.Bounds.GetIndex();

            return createdBoundsSceneElement;
        }

    }
}
