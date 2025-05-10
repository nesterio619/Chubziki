using Core.Utilities;
using UnityEngine;

namespace Regions.BoundsInEditor
{
    public class SectorBounds : BoundsSceneElement
    {

        public bool IsMovingInEditor;

        
        [ContextMenu("CreateFirstBounds")]
        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            MyMeshFilter.mesh = MeshUtilities.CreateOneBigCubeCollider(MyLocation.GetAllBounds(), transform);
#endif
        }
#if UNITY_EDITOR
        protected override void Update()
        {
            if (!IsMovingInEditor)
                base.Update();
        }
#endif
    }
}
