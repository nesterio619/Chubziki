using Actors.EditorActors;
using Actors.Molds;
using Regions;
using UnityEngine;

namespace Actors.EditorElements
{
    public class RBLibraBridgeEditorActor : EditorActor
    {
        [SerializeField] private RBLibraBridgeMold mold;
        
        [SerializeField] private Transform moveablePart;
        
        private bool HasMoveablePart => moveablePart != null;
        
        public void Initialize(Location location, RBLibraBridgeMold mold)
        {
#if UNITY_EDITOR
            Initialize(location);
            this.mold = mold;
            this.mold.OnMoldChange += UpdateValues;

            moveablePart = transform.GetChild(0);

            UpdateValues(this.mold.Size);
#endif
        }
        
        public void UpdateValues(Vector3 size)
        {
#if UNITY_EDITOR
            var child = moveablePart.GetChild(0);
            if (child.localScale != size)
                child.localScale = size;
#endif
        }
    }
}
