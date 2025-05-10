using Actors;
using Actors.Molds;
using DG.Tweening;
using System;
using UnityEngine;
using QuestsSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Components.Mechanism
{
    public class BridgeActor : Actor, ISetupForCompletedQuest
    {
        public enum MoveType { Draw, Slide }

        [field: SerializeField] public MoveType BridgeType { get; private set; }

        [SerializeField, HideInInspector] private float zSpacing;
        [SerializeField, HideInInspector] private Transform moveablePart;
        [SerializeField, HideInInspector] private Transform mirroredPart;

        private float _targetValue;
        private bool _startOpened;
        private float _moveDuration;
        private Ease _moveEase;

        private bool _isOpened;
        private Tweener _movingTween;
        private Func<Transform, Tweener> _movePart;

        public bool HasMirroredPart => mirroredPart != null;
        public float ZSpacing => zSpacing;

        public event Action OnFinishMoving;


        private void OnValidate()
        {
            if (moveablePart == null) moveablePart = transform.GetChild(0);

            if (mirroredPart == null) return;

            moveablePart.localPosition = new Vector3(0, 0, -zSpacing);
            mirroredPart.localPosition = new Vector3(0, 0, zSpacing);
        }

        public override void LoadActor(Mold actorMold)
        {
            base.LoadActor(actorMold);

            var bridgeMold = actorMold as BridgeMold;

            BridgeType = bridgeMold.BridgeType;
            _movePart = BridgeType == MoveType.Draw ? RotatePart : SlidePart;

            _targetValue = bridgeMold.TargetValue;
            _startOpened = bridgeMold.StartOpened;
            _moveDuration = bridgeMold.MoveDuration;
            _moveEase = bridgeMold.MoveEase;
            _isOpened = false;

            var child = moveablePart.GetChild(0);
            child.localScale = bridgeMold.Size;
            child.localPosition = new Vector3(0, 0, bridgeMold.Size.z / 2);

#if UNITY_EDITOR
            if (HasMirroredPart) MirrorMoveablePart();
#endif

            if (_startOpened) ToggleStateWithoutTween();
        }

        private void ToggleStateWithoutTween()
        {
            var duration = _moveDuration;
            _moveDuration = 0;
            ToggleState();
            _moveDuration = duration;
        }

        public void ToggleState()
        {
            if (_movingTween.IsActive()) return;

            _isOpened = !_isOpened;

            _movingTween = _movePart(moveablePart);
            _movingTween.onComplete = () => OnFinishMoving?.Invoke();

            _movePart(mirroredPart);
        }

        private Tweener RotatePart(Transform part)
        {
            if (part == null) return null;

            var targetRotation = new Vector3(_isOpened ? _targetValue : -_targetValue, 0, 0);
            return part.DOLocalRotate(part.localEulerAngles + targetRotation, _moveDuration).SetEase(_moveEase);
        }

        private Tweener SlidePart(Transform part)
        {
            if (part == null) return null;

            var targetPosition = new Vector3(0, 0, _isOpened ? _targetValue : -_targetValue);
            return part.DOMove(part.position + part.rotation * targetPosition, _moveDuration).SetEase(_moveEase);
        }

#if UNITY_EDITOR
        public void MirrorMoveablePart()
        {
            var bounds = GetBounds();

            try
            {
                if (HasMirroredPart)
                    DestroyImmediate(mirroredPart.gameObject);

                mirroredPart = Instantiate(moveablePart, transform);
            }
            catch (Exception e)
            {
                Debug.LogError(gameObject.name + " - Press the mirror button only inside the prefab!\nDetails: " + e.Message);
                return;
            }

            mirroredPart.name = "MoveablePartMirrored";
            mirroredPart.localRotation = Quaternion.Euler(0, 180, 0);

            zSpacing = bounds.size.z + bounds.min.z;

            OnValidate();
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
#endif

        public override void ReturnToPool()
        {
            if (_isOpened) ToggleStateWithoutTween();

            base.ReturnToPool();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            if (moveablePart != null) Gizmos.DrawWireSphere(moveablePart.position, 1);
            if (mirroredPart != null) Gizmos.DrawWireSphere(mirroredPart.position, 1);
        }

        public void SetupForCompletedQuest()
        {
            if (_startOpened && _isOpened)
            {
                _movingTween?.Complete();
                ToggleStateWithoutTween();
            }

            else if (!_startOpened && !_isOpened)
                ToggleStateWithoutTween();
        }
    }
}