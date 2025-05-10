using Core.Utilities;
using UnityEngine;

namespace Regions.BoundsInEditor
{
    public class RegionBounds : BoundsSceneElement
    {
        public bool IsMovingInEditor;

        public override void CreateMeshBounds()
        {
#if UNITY_EDITOR
            MyMeshFilter.mesh = MeshUtilities.CreateCombinedMeshFromBounds(MyLocation.GetAllBounds(), transform);
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