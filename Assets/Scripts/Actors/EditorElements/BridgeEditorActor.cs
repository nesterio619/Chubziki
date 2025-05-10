using Actors.Molds;
using Regions;
using UnityEditor;
using UnityEngine;
using static Components.Mechanism.BridgeActor;

namespace Actors.EditorActors
{
    [ExecuteInEditMode]
    public class BridgeEditorActor : EditorActor
    {
        [SerializeField] private BridgeMold mold;
        [SerializeField] private Transform moveablePart;
        [SerializeField] private Transform mirroredPart;

        private float _targetValue;
        [SerializeField] private bool displayGizmos;

        public bool HasMirroredPart => mirroredPart != null;

        public void Initialize(Location location, BridgeMold mold)
        {
#if UNITY_EDITOR
            Initialize(location);
            this.mold = mold;
            this.mold.OnMoldChange += UpdateValues;

            moveablePart = transform.GetChild(0);
            if (transform.childCount > 1) mirroredPart = transform.GetChild(1);

            UpdateValues(this.mold.Size, mold.ZSpacing, mold.TargetValue, mold.DisplayGizmos);
#endif
        }
        
#if UNITY_EDITOR
        protected override void OnEnable()
        {
            base.OnEnable();

            if (this == null) return;
            if (mold == null) return;

            mold.OnMoldChange += UpdateValues;
            if (mirroredPart == null && transform.childCount > 1) mirroredPart = transform.GetChild(1);
            UpdateValues(mold.Size, mold.ZSpacing, mold.TargetValue, mold.DisplayGizmos);
        }
#endif
        
        private void OnDisable()
        {
            if (HasMirroredPart) mold.OnMoldChange -= UpdateValues;
        }

        public void UpdateValues(Vector3 size, float zSpacing, float targetValue, bool displayGizmos)
        {
#if UNITY_EDITOR
            _targetValue = targetValue;
            this.displayGizmos = displayGizmos;

            var child = moveablePart.GetChild(0);
            if (child.localScale != size)
            {
                child.localScale = size;
                child.localPosition = new Vector3(0, 0, size.z / 2);
                if (HasMirroredPart) MirrorMoveablePart();
            }
            else {
                if (!HasMirroredPart) return;
                moveablePart.localPosition = new Vector3(0, 0, -zSpacing);
                mirroredPart.localPosition = new Vector3(0, 0, zSpacing);
            }
#endif
        }

        public void MirrorMoveablePart()
        {
#if UNITY_EDITOR
            if(moveablePart == null || mirroredPart == null)
                return;
            
            var child = moveablePart.GetChild(0);
            var child1 = mirroredPart.GetChild(0);

            child1.localScale = child.localScale;
            child1.localPosition = child.localPosition;

            var bounds = GetBounds();
            var zSpacing = bounds.size.z + bounds.min.z;

            moveablePart.localPosition = new Vector3(0, 0, -zSpacing);
            mirroredPart.localPosition = new Vector3(0, 0, zSpacing);

            mold.ZSpacing = zSpacing;
            EditorUtility.SetDirty(mold);
#endif
        }

        private Bounds GetBounds()
        {
            var bounds = new Bounds();

            foreach (var renderer in moveablePart.GetComponentsInChildren<Renderer>())
            {
                var renderBounds = renderer.bounds;
                renderBounds.center -= moveablePart.position;

                bounds.Encapsulate(renderBounds);
            }

            return bounds;
        }

        private void OnDrawGizmos()
        {
            if(!displayGizmos) return;
            if (moveablePart == null) return;

            if (mold.BridgeType == MoveType.Draw)
            {
                DisplayRotation(moveablePart);
                DisplayRotation(mirroredPart);
            }
            else
            {
                DisplayOffset(moveablePart);
                DisplayOffset(mirroredPart);
            }

            void DisplayRotation(Transform transform)
            {
                if (transform == null) return;

                Quaternion targetRotation = Quaternion.Euler(new Vector3(_targetValue, 0, 0));
                Vector3 angledForward = transform.rotation * targetRotation * Vector3.forward;

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, transform.forward * 2);

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, angledForward * 2);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + transform.forward, transform.position + angledForward);
            }
            void DisplayOffset(Transform transform)
            {
                if (transform == null) return;

                Vector3 targetPosition = new Vector3(0, 0, _targetValue);
                var child = transform.GetChild(0);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(child.transform.position + transform.rotation * targetPosition, child.localScale);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(child.transform.position, child.transform.position + transform.rotation * targetPosition);
            }
        }
    }
}
