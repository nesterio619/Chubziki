using UnityEngine;

namespace SimpleSeeThroughDemo
{
    public class SeeThroughControl : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] Transform seeThroughSphere;
        [SerializeField] Transform target;
        [SerializeField] float targetScaleSize = 3f;
        [SerializeField] float scaleSpeed = 2f;
        
        bool isTargetOccluded = false;    bool scaledUp = false;

        void Start()
        {
            if(cam == null) cam = Camera.main;
        }

        void CastToTarget()
        {
            Ray ray = new(cam.transform.position, target.position - cam.transform.position);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.transform == target) isTargetOccluded = false;
                else isTargetOccluded = true;
            }
        }

        void Update()
        {
            CastToTarget();
        }

        void FixedUpdate()
        {
            if(isTargetOccluded && !scaledUp)
            {
                LerpScale(Vector3.one * targetScaleSize);
            }

            if(!isTargetOccluded && scaledUp)
            {
                LerpScale(Vector3.zero);
            }
        }

        void LerpScale(Vector3 targetScale)
        {
            if(seeThroughSphere.localScale == Vector3.one * targetScaleSize) scaledUp = true;
            if(seeThroughSphere.localScale == Vector3.zero) scaledUp = false;

            if(Vector3.Distance(seeThroughSphere.localScale, targetScale) < 0.2f)
            {
                seeThroughSphere.localScale = targetScale;
                return;
            }
            else
            seeThroughSphere.localScale = Vector3.Lerp(seeThroughSphere.localScale, targetScale, 
                EasingLibrary.EaseOutQuad(Time.deltaTime * scaleSpeed));
        }

        void OnDrawGizmos()
        {
            Debug.DrawRay(cam.transform.position, target.position - cam.transform.position, Color.red);
        }
    }
}