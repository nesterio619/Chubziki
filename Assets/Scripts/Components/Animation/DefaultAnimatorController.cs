using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components.Animation
{
    public class DefaultAnimatorController : ActorAnimatorController
    {

        [SerializeField] private Animator animator;

        public event Action AttackAnimationTrigger;

        private int _idleAnimationStateHash = Animator.StringToHash("Idle");
        private int _runAnimationStateHash = Animator.StringToHash("Run");
        private int _attackAnimationStateHash = Animator.StringToHash("Attack");
        private int _aimAnimationStateHash = Animator.StringToHash("Aim");

        private Dictionary<DefaultAnimations, Action> _animationEndEvents = new Dictionary<DefaultAnimations, Action>();
        private Dictionary<DefaultAnimations,Action> _animationStartEvents = new Dictionary<DefaultAnimations,Action>();

        public override void SetAnimationByID(DefaultAnimations animationID, float animationSpeed = 1, float offsetAnimation = 0)
        {
            if(animator == null)
                return;
            
            animator.enabled = true;

            var animationState = animator.GetCurrentAnimatorStateInfo(0);
            if(animationState.loop)
                InvokeEvents(_animationEndEvents, CurrentAnimation);

            InvokeEvents(_animationStartEvents, animationID);

            base.SetAnimationByID(animationID, animationSpeed, offsetAnimation);

            StartCoroutine(WaitForAnimationFinish(CurrentAnimation));
        }

        private IEnumerator WaitForAnimationFinish(DefaultAnimations animationID)
        {
            // Wait until the end of the frame to ensure the Animator has updated its state
            yield return new WaitForEndOfFrame(); 
            
            var animationState = animator.GetCurrentAnimatorStateInfo(0);
            if(animationState.loop) yield break;
 
            yield return new WaitForSeconds(animationState.length * GlobalAnimationSpeed);
            InvokeEvents(_animationEndEvents, animationID);
        }

        public override void AddEventOnEndAnimation(DefaultAnimations animationID, Action animationAction)
        {
            if(_animationEndEvents.ContainsKey(animationID))
            {
                _animationEndEvents.TryGetValue(animationID, out var animationEvent);
                animationEvent += animationAction;
                _animationEndEvents[animationID] = animationEvent;
                return;
            }

            _animationEndEvents.Add(animationID, animationAction);
        }

        public override void RemoveEventToAnimation(DefaultAnimations animationID, Action animationAction)
        {
            if (_animationEndEvents.ContainsKey(animationID))
            {
                _animationEndEvents.TryGetValue(animationID, out var animationEvent);
                animationEvent -= animationAction;
                _animationEndEvents[animationID] = animationEvent;
                return;
            }

            _animationEndEvents.Add(animationID, animationAction);
        }

        public override void AddEventToAnimation(DefaultAnimations animationID, Action animationAction)
        {
            _animationStartEvents.Add(animationID, animationAction);
        }

        private protected override void SetDisturb(float animationSpeed = 1, float offsetAnimation = 0)
        {
            animator.Play(_runAnimationStateHash);
        }

        private protected override void SetIdle(float animationSpeed = 1, float offsetAnimation = 0)
        {
            animator.Play(_idleAnimationStateHash);
        }

        private protected override void SetAttack(float animationSpeed = 1, float offsetAnimation = 0)
        {
            animator.Play(_attackAnimationStateHash);
        }

        private protected override void SetAim(float animationSpeed = 1, float offsetAnimation = 0)
        {
            animator.Play(_aimAnimationStateHash);
        }

        public override void StopAnimation()
        {
            animator.enabled = false;
        }

        public void InvokeAttackTrigger()
        {
            AttackAnimationTrigger?.Invoke();
        }

        private static void InvokeEvents(Dictionary<DefaultAnimations, Action> eventsDictionary, DefaultAnimations animationID)
        {
            var events = eventsDictionary.Where(x => x.Key == animationID);
            foreach (var keyValuePair in events)
                keyValuePair.Value.Invoke();
        }
    }
}