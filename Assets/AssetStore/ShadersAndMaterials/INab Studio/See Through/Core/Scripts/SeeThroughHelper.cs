using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace INab.WorldAlchemy
{
    public class SeeThroughHelper : SeeThroughHelperBase
    {
        [Serializable]
        public class MaskObject : MaskObjectBase
        {
            private Mask mask;
            private SeeThroughDissolve seeThroughDissolve;

            /// <summary>
            /// Method to set the SeeThroughDissolve reference for this mask.
            /// </summary>
            /// <param name="seeThroughDissolve"></param>
            public void SetSeeThroughDissolve(SeeThroughDissolve seeThroughDissolve)
            {
                this.seeThroughDissolve = seeThroughDissolve;
            }


            protected override void AdjustMaskScale(float targetScale)
            {
                base.AdjustMaskScale(targetScale);
            
                if(mask == null)
                {
                    mask = maskTransform.GetComponent<Mask>();
                }

                if (mask)
                {
                    seeThroughDissolve.UpdateMaskScale(mask.ID);
                }
            }

        }

        [Header("See-Through Dissolve Settings")]
        [Tooltip("Reference to the main see-through dissolve effect handler.")]
        public SeeThroughDissolve seeThroughDissolve;

        [Header("Masks Configuration")]
        [Tooltip("List of mask objects to manage.")]
        public List<MaskObject> maskObjects = new List<MaskObject>();

        [Header("Custom Camera Update")]
        [Tooltip("Manually triggers updates to custom camera scripts, typically during LateUpdate. See documentation for integration details."),Space]
        public UnityEvent onCameraUpdate;

        [Tooltip("Forces an immediate update of mask parameters to mitigate potential lag effects.")]
        public bool forceUpdateMaskParameters = true;
        protected void Start()
        {
            SetCameraTransform(seeThroughDissolve.CameraTransform);

            foreach (var maskObject in maskObjects)
            {
                StartCall(maskObject);
                maskObject.SetSeeThroughDissolve(seeThroughDissolve);
            }
        }

        protected void LateUpdate()
        {
            onCameraUpdate.Invoke(); // Invoke custom camera updates.

            foreach (var maskObject in maskObjects)
            {
                LateUpdateCall(maskObject);
            }

            if (forceUpdateMaskParameters)
            {
                seeThroughDissolve.ForceUpdateMasksParameters();
            }
        }

        protected override void Update()
        {
            SetCameraTransform(seeThroughDissolve.CameraTransform);

            base.Update();

            foreach (var maskObject in maskObjects)
            {
                UpdateCall(maskObject);
            }
        }

    }
}