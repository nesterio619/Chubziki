using System.Collections;
using UnityEngine;

namespace Transition
{
    [CreateAssetMenu(fileName = "Scale", menuName = "Scene Transition/Scale")]
    public class ScaleTransitionScriptableObject : AbstractSceneTransitionScriptableObject
    {
        public Sprite ScaleSprite;
        public Color ScaleColor;

        public override IEnumerator Enter(bool expectSceneLoad = true)
        {
            AnimatedObject.color = ScaleColor;
            AnimatedObject.sprite = ScaleSprite;

            float time = 0;
            float duration = 1f / AnimationSpeedMultiplier;

            while (time < 1)
            {
                time += Time.deltaTime * AnimationSpeedMultiplier;
                float transitionValue = LerpCurve.Evaluate(time);
                AnimatedObject.transform.localScale = Vector3.one * transitionValue;
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
                AnimatedObject.transform.localScale = Vector3.one * (1 - transitionValue); 
                yield return null;
            }
        }
    }
}