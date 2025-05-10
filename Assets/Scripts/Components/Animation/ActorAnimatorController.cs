using System;
using UnityEngine;

namespace Components.Animation
{
    public abstract class ActorAnimatorController : MonoBehaviour
    {
        [SerializeField]
        private protected GameObject Actor;

        [SerializeField]
        private protected float GlobalAnimationSpeed = 1;

        [SerializeField]
        [Tooltip("0 = Idle, 1 = Disturb, 2 = Attack")]
        private DefaultAnimations StartAnimation;

        public DefaultAnimations CurrentAnimation { get; protected set; }

        [SerializeField]
        private bool PlayOnStart;

        private void Start()
        {
            if (PlayOnStart)
            {
                SetAnimationByID(StartAnimation);
            }
        }

        private protected abstract void SetIdle(float animationSpeed = 1f, float offsetAnimation = 0);

        private protected abstract void SetDisturb(float animationSpeed = 1f, float offsetAnimation = 0);

        private protected virtual void SetAttack(float animationSpeed = 1f, float offsetAnimation = 0) { }

        private protected virtual void SetAim(float animationSpeed = 1f, float offsetAnimation = 0) { }

        public virtual void SetAnimationByID(DefaultAnimations animationID, float animationSpeed = 1f, float offsetAnimation = 0)
        {
            switch (animationID)
            {
                case DefaultAnimations.Idle:
                    {
                        SetIdle(animationSpeed, offsetAnimation);
                        break;
                    }
                case DefaultAnimations.Attack:
                    {
                        SetAttack(animationSpeed, offsetAnimation);
                        break;
                    }
                case DefaultAnimations.Disturb:
                    {
                        SetDisturb(animationSpeed, offsetAnimation);
                        break;
                    }
                case DefaultAnimations.Aim:
                    {
                        SetAim(animationSpeed, offsetAnimation);
                        break;
                    }
            }

            CurrentAnimation = animationID;
        }

        public abstract void AddEventToAnimation(DefaultAnimations animationID, Action animationAction);

        public virtual void RemoveEventToAnimation(DefaultAnimations animationID, Action animationAction)
        {

        }

        public abstract void AddEventOnEndAnimation(DefaultAnimations animationID, Action animationAction);

        private void OnDisable()
        {
            StopAnimation();
        }

        private void OnEnable()
        {
            SetAnimationByID(CurrentAnimation);
        }

        public virtual void StopAnimation()
        {

        }

        public enum DefaultAnimations
        {
            Idle = 0,
            Disturb = 1,
            Attack = 2,
            Aim = 3,
        }

    }
}