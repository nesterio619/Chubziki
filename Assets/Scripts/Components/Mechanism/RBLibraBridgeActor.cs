using Actors;
using Actors.Molds;
using UnityEngine;

namespace Components.Mechanism
{
    public class RBLibraBridgeActor : Actor
    {
        [SerializeField, HideInInspector] private Transform moveablePart;
        
        private void OnValidate()
        {
            if (moveablePart == null) 
                moveablePart = transform.GetChild(0);

            moveablePart.localPosition = new Vector3(0, 0, 0);
        }
        
        public override void LoadActor(Mold actorMold)
        {
            base.LoadActor(actorMold);

            var bridgeMold = actorMold as RBLibraBridgeMold;
            
            var child = moveablePart.GetChild(0);
            child.localScale = bridgeMold.Size;

            var bounds = GetBounds(transform.GetChild(1));
            var supportHeight = bounds.size.y;
            child.localPosition = new Vector3(0, supportHeight, 0);
        }
        
        private Bounds GetBounds(Transform transform)
        {
            var bounds = new Bounds();

            foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
            {
                var renderBounds = renderer.bounds;
                renderBounds.center -= transform.position;

                bounds.Encapsulate(renderBounds);
            }

            return bounds;
        }
    }
}
