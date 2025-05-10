using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestsSystem.QuestsElements.Race
{
    public class RoadCheckPoint : QuestElement
    {
        [Header("Hide animation on cross")]
        [SerializeField] private Transform graphicsToMoveOnCross;
        [FormerlySerializedAs("textToMoveOnCross")] [SerializeField] private Transform textCheckPoint;
        [Space]
        [SerializeField] [Range(0.1f, 10)] private float hideAnimationLengthInSeconds = 1f;
        [SerializeField] private Vector3 hiddenGraphicsPosition;
        [SerializeField] private Renderer[] renderers;

        private Vector3 _defaultGraphicsPosition;
        private TweenerCore<Vector3, Vector3, VectorOptions> _tweener;

        private float _xOffset = 90;
        private float _zOffset = 180;

        public override void SetQuestName(string questsName)
        {
            base.SetQuestName(questsName);
            RotateInCameraDirection();
        }

        private void Awake()
        {
            _defaultGraphicsPosition = graphicsToMoveOnCross.localPosition; 
        }

        private bool _isCrossed;

        public void PlayerCrossCheckPoint()
        {
            // Prevent multiple trigger enter calls
            if(_isCrossed) return;
            _isCrossed = true;

            OnQuestEventTriggered?.Invoke();
            DoHideAnimation();
        }

        private void DoHideAnimation()
        {
            if (_tweener != null)
                return;

            _tweener = graphicsToMoveOnCross.DOLocalMove(hiddenGraphicsPosition, hideAnimationLengthInSeconds);
            _tweener.onComplete = () => _tweener = null;
            _tweener.Play();
        }

        public void RotateInCameraDirection()
        {
            Camera mainCamera = Camera.main;
            
            textCheckPoint.rotation = Quaternion.Euler(_xOffset, 0, _zOffset - mainCamera.transform.rotation.eulerAngles.y);
        }

        public void ToggleAllRenderers(bool stateToSet)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = stateToSet;
            }
        }

        public void TryCancelAnimation() 
        {
            if (_tweener != null)
            {
                _tweener.Kill();
                _tweener = null;
            }
            if (graphicsToMoveOnCross.localPosition != _defaultGraphicsPosition)
                graphicsToMoveOnCross.SetLocalPositionAndRotation(_defaultGraphicsPosition, quaternion.identity);
        }

public Transform GraphicsToMoveOnCross => graphicsToMoveOnCross;

        public override void ReturnToPool()
        {
            TryCancelAnimation();
            ToggleAllRenderers(true);

            _isCrossed = false;

            base.ReturnToPool();
        }

        public void ResetCheckpoint()
        {
            _isCrossed = false;

            // reset visuals?
        }
    }
}