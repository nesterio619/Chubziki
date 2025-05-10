using System.Collections;
using UnityEngine;

namespace Transition
{
    [CreateAssetMenu(fileName = "Fade", menuName = "Scene Transition/Fade")]
    public class FadeTransitionScriptableObject : AbstractSceneTransitionScriptableObject
    {
        [SerializeField]
        private Color _fadeColor;

        public override IEnumerator Enter(bool expectSceneLoad = true)
        {
            float time = 0;
            float duration = 1f / AnimationSpeedMultiplier;

            while (time < 1)
            {
                time += Time.deltaTime * AnimationSpeedMultiplier;
                float transitionValue = LerpCurve.Evaluate(time);
                AnimatedObject.color = new Color(0, 0, 0, transitionValue);
                if (transitionValue >= TransitionCompletionThreshold && expectSceneLoad)
                {
                    Regions.RegionManager.Regions.Clear();
                }
                yield return null;
            }
            OnEnterFinished();
        }

        public override IEnumerator Exit()
        {
            float time = 0;
            float duration = 1f / AnimationSpeedMultiplier;

            while (time < 1)
            {
                time += Time.deltaTime * AnimationSpeedMultiplier;
                float transitionValue = LerpCurve.Evaluate(time);
                AnimatedObject.color = new Color(0, 0, 0, 1 - transitionValue); 
                yield return null;
            }
        }
    }
}