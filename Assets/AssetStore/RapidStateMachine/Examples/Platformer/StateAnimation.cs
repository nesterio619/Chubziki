using System.Collections.Generic;
using UnityEngine;

namespace RSM
{
    public class StateAnimation : MonoBehaviour
    {
        public List<Animator> animators;
        public string defaultAnimation;
        public List<TransitionAnimation> transitions;

        public void PlayAnimation(RSMState from, RSMState to)
        {
            if (to != _localRsmState) return;
            foreach (TransitionAnimation transition in transitions)
            {
                if (transition.from == from)
                {
                    if (defaultAnimation == "") Debug.LogError("State Transition Animation has no name");
                    SetAnimation(transition.animation);
                    return;
                }
            }
            if (defaultAnimation == "") Debug.LogError("State Animation has no name");
            SetAnimation(defaultAnimation);
        }

        public void SetAnimation(string animationName, float time = 0)
        {
            string formattedName = "Base Layer." + animationName;

            foreach (Animator animator in animators)
            {
                if (animator.HasState(0, Animator.StringToHash(formattedName)))
                {
                    animator.Play(formattedName, 0, time);
                }
            }
        }

        private RSMState _localRsmState;

        public void OnEnable()
        {
            transform.parent.GetComponent<StateMachine>().OnStateChange += PlayAnimation;
            _localRsmState = GetComponent<RSMState>();
        }
        public void OnDisable()
        {
            transform.parent.GetComponent<StateMachine>().OnStateChange -= PlayAnimation;
        }

        [System.Serializable]
        public struct TransitionAnimation
        {
            public string animation;
            public RSMState from;
        }
    }
}