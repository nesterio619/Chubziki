using UnityEngine;

namespace SimpleSeeThroughDemo
{
    public class ScaleSeeThrough : MonoBehaviour
    {
        [SerializeField] float speed = 50f;
        bool isScalingUp = false;
        bool isScalingDown = false;
        bool isScaling = false;

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) && !isScaling && !isScalingUp && !isScalingDown)
            { 
                isScaling = true;
            }
            if(isScaling)
            {
                isScaling = false;
                if(transform.localScale.x <= 0.1f) isScalingUp = true;
                else isScalingDown = true;
            }
        }

        void FixedUpdate()
        {
            if(isScalingUp) LerpScale(Vector3.one*3);
            if(isScalingDown) LerpScale(Vector3.zero);
        }

        void LerpScale(Vector3 targetScale)
        {
            if(Vector3.Distance(transform.localScale, targetScale) < 0.1f)
            {
                transform.localScale = targetScale;
                isScaling = false;
                isScalingUp = false;
                isScalingDown = false;
                return;
            }
            else
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, EasingLibrary.EaseOutQuad(Time.deltaTime * speed));
        }
    }
}