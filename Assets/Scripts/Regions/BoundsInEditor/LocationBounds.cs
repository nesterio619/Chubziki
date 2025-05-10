using Core.Utilities;
using UnityEngine;

namespace Regions.BoundsInEditor
{
    public class LocationBounds : BoundsSceneElement
    {
        public bool IsMovingInEditor;
        public Vector3 ScaleBounds = Vector3.one;
        public Vector3 OffsetPosition = Vector3.zero;


        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            MyMeshFilter.mesh = MeshUtilities.CreateOneBigCubeCollider(
                MyLocation.GetAllBounds(), 
                ScaleBounds, 
                OffsetPosition, 
                transform
            );
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("CreateFirstBounds")]
        private void CreateFirstBoundsContextMenu()
        {
            CreateMeshBounds();
        }

        protected override void Update()
        {
            if (!IsMovingInEditor)
                base.Update();
        }
#endif
    }
}