using Core.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace Components.Animation
{
    public class CurveAnimatorController : ActorAnimatorController
    {

        [SerializeField]
        private float currentOffsetAnimation = 0;

        [SerializeField]
        [FormerlySerializedAs("WalkCurve")]
        private AxisCurveMovement walkCurve;

        [SerializeField]
        [FormerlySerializedAs("AttackCurve")]
        private AxisCurveMovement attackCurve;

        [SerializeField]
        [FormerlySerializedAs("IdleCurve")]
        private AxisCurveMovement idleCurve;

        [SerializeField]
        private AxisCurveMovement aimCurve;

        private float _currentTime = 0;

        private float _totalTime = 0;

        private Coroutine _currentAnimationCoroutine;


        private protected override void SetDisturb(float animationSpeed = 1, float offsetAnimation = 0)
        {
            ChangeAnimation(walkCurve, offsetAnimation);
        }

        private protected override void SetIdle(float animationSpeed = 1, float offsetAnimation = 0)
        {
            ChangeAnimation(idleCurve, offsetAnimation);
        }

        private protected override void SetAttack(float animationSpeed = 1f, float offsetAnimation = 0)
        {
            ChangeAnimation(attackCurve, offsetAnimation);
        }

        private protected override void SetAim(float animationSpeed = 1f, float offsetAnimation = 0)
        {
            ChangeAnimation(aimCurve, offsetAnimation);
        }

        public override void SetAnimationByID(DefaultAnimations animationID, float animationSpeed = 1f, float offsetAnimation = 0)
        {
            base.SetAnimationByID(animationID, animationSpeed, offsetAnimation);

            _currentTime = 0;

            GlobalAnimationSpeed = animationSpeed;

        }

        private void ChangeAnimation(AxisCurveMovement axis, float offsetAnimation)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_currentAnimationCoroutine != null)
                StopCoroutine(_currentAnimationCoroutine);

            currentOffsetAnimation = offsetAnimation;

            _totalTime = axis.XCurve.keys[axis.XCurve.length - 1].time;

            _currentAnimationCoroutine = StartCoroutine(PlayAnimation(axis));
        }

        private IEnumerator PlayAnimation(AxisCurveMovement axis)
        {
            axis.CurveEvent.Reset();

            bool canMove = !axis.XCurve.IsConstantZero() 
                && !axis.YCurve.IsConstantZero() 
                && !axis.ZCurve.IsConstantZero();

            while (gameObject.activeSelf)
            {
                if(canMove)
                {
                    var pos = Actor.transform.localPosition;

                    pos.x = axis.XCurve.Evaluate(_currentTime) * axis.Intensity;

                    pos.y = axis.YCurve.Evaluate(_currentTime) * axis.Intensity;

                    pos.z = axis.ZCurve.Evaluate(_currentTime) * axis.Intensity;

                    Actor.transform.localPosition = pos;
                }

                Actor.transform.localRotation = Quaternion.Euler(
                    axis.XRotationCurve.Evaluate(_currentTime) * axis.Intensity,
                    axis.YRotationCurve.Evaluate(_currentTime) * axis.Intensity,
                    axis.ZRotationCurve.Evaluate(_currentTime) * axis.Intensity);

                _currentTime += Time.deltaTime * GlobalAnimationSpeed * axis.Speed;

                if (!axis.CurveEvent.IsEventPlayed &&
                    axis.CurveEvent.AnimationPercent / 100f <= _currentTime / _totalTime && /*Check when animation reach CurveEvent.AnimationPercent */
                    axis.CurveEvent.AnimationPercent / 100f >= currentOffsetAnimation / _totalTime) /* Check if animation offset bigger CurveEvent.AnimationPercent*/
                {
                    axis.CurveEvent?.InvokeEvent();
                }

                if (_currentTime >= _totalTime)
                {
                    _currentTime = 0 + currentOffsetAnimation;
                    axis.CurveEvent.Reset();
                }

                yield return null;
            }

            axis.AnimationEndEvent?.Invoke();
        }

        public override void StopAnimation()
        {
            if (_currentAnimationCoroutine != null)
                StopCoroutine(_currentAnimationCoroutine);

        }

        public override void AddEventToAnimation(DefaultAnimations animationID, Action animationAction)
        {
            switch (animationID)
            {
                case DefaultAnimations.Idle:
                    idleCurve.CurveEvent.OnAnimation.AddListener( () => animationAction?.Invoke() );
                    break;
                case DefaultAnimations.Disturb:
                    walkCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
                case DefaultAnimations.Attack:
                    attackCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
                case DefaultAnimations.Aim:
                    aimCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
            }

            return;
        }

        public override void AddEventOnEndAnimation(DefaultAnimations animationID, Action animationAction)
        {
            switch (animationID)
            {
                case DefaultAnimations.Idle:
                    idleCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
                case DefaultAnimations.Disturb:
                    walkCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
                case DefaultAnimations.Attack:
                    attackCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
                case DefaultAnimations.Aim:
                    aimCurve.CurveEvent.OnAnimation.AddListener(() => animationAction?.Invoke());
                    break;
            }

            return;
        }

        [System.Serializable]
        public class AxisCurveMovement
        {
            [Range(0f, 3f)]
            public float Intensity = 1;

            [Range(0f, 3f)]
            public float Speed = 1;

            public AnimationCurve XCurve;

            public AnimationCurve YCurve;

            public AnimationCurve ZCurve;

            [Space(10)]
            public AnimationCurve XRotationCurve;

            public AnimationCurve YRotationCurve;

            public AnimationCurve ZRotationCurve;

            [Space(10)]
            public AnimationEvent CurveEvent;

            public UnityEvent AnimationEndEvent;
        }

        [System.Serializable]
        public class AnimationEvent
        {
            [Range(0, 100f)]
            public float AnimationPercent;

            public UnityEvent OnAnimation;

            [HideInInspector] public bool IsEventPlayed;

            [HideInInspector] public bool IsOpenEditor;

            public void Reset()
            {
                IsEventPlayed = false;
            }

            public void InvokeEvent()
            {
                IsEventPlayed = true;

                OnAnimation?.Invoke();
            }

        }

    }
}