using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;



namespace Transition
{
    public abstract class AbstractSceneTransitionScriptableObject : ScriptableObject
    {
        public AnimationCurve LerpCurve;
        public float AnimationSpeedMultiplier = 0.25f;
        public Image AnimatedObject;
        
        public static event Action OnEnterCompleted;

        public abstract IEnumerator Enter(bool expectSceneLoad = true);
        public abstract IEnumerator Exit();

        public void InitializeAnimatedObject(Image image)
        {
            AnimatedObject = image;
        }

        protected float TransitionCompletionThreshold = 0.98f;
        
        protected void OnEnterFinished() => 
            OnEnterCompleted?.Invoke();
    }
}