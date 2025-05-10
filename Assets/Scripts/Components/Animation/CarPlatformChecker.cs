using System.Collections.Generic;
using UnityEngine;

namespace Components.Animation
{
    public class CarPlatformChecker : MonoBehaviour
    {
        [SerializeField]
        private List<WheelCollider> _wheelColliders;

        private bool IsOnPlatform;

        private PlatformAttacher platformAttacher;



        [ContextMenu("Check")]
        public void Check()
        {
            foreach (var item in _wheelColliders)
            {
                item.GetGroundHit(out WheelHit wheelHit);
            }
        }

        void Update()
        {
            IsOnPlatform = false;

            foreach (var item in _wheelColliders)
            {
                item.GetGroundHit(out WheelHit wheelHit);


                IsOnPlatform = CheckPlarform(wheelHit);
            }

            if (platformAttacher != null && !IsOnPlatform)
            {
                // platformAttacher.RemoveFromPlatform(transform);
                platformAttacher = null;
            }

        }

        public bool CheckPlarform(WheelHit wheelHit)
        {


            if (wheelHit.collider != null && wheelHit.collider.TryGetComponent(out PlatformAttacher platform))
            {
                //platform.AttachToPlatform(transform);

                return true;
            }

            return false;
        }
    }
}