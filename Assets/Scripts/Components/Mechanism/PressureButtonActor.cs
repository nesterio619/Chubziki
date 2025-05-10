using Core.Utilities;
using UnityEngine;
using Actors;
using Actors.Molds;
using System;
using UnityEngine.Events;

namespace Components.Mechanism
{

    [RequireComponent(typeof(ObjectInPlace))]
    public class PressureButtonActor : Actor
    {
        [SerializeField] private Transform anchor;
        [Space]
        [SerializeField] private SpringJoint springJoint;

        public ObjectInPlace ObjectInPlace;

        public Transform ActivatedActorTransform;

        public event Action OnActivate;

        private void Start()
        {
            ChangeConnectedAnchor();
        }

        protected void FixedUpdate()
        {
            ObjectInPlace.CheckDistance();
        }

        [ContextMenu("ChangeConnectedAnchor")]
        private void ChangeConnectedAnchor()
        {
            springJoint.connectedAnchor = anchor.position;
        }

        private void ActivateActor(bool activation)
        {
            if (activation) OnActivate?.Invoke();

            if (ActivatedActorTransform == null) return;

            var actor = ActivatedActorTransform.GetComponentInChildren<Actor>();

            if (actor != null)
                actor.ExternalActivation(activation);
            else
                Debug.LogWarning($"<b>{gameObject.name}</b> - {ActivatedActorTransform.name} is not an Actor!");
        }

        public void ResetSpringJoint()
        {
            springJoint.connectedAnchor = anchor.position;
        }

        public void Initialize(Transform activatedActor, UnityAction onTooClose, UnityAction onTooFar)
        {
            ActivatedActorTransform = activatedActor;

            ObjectInPlace.OnTooClose.RemoveAllListeners();
            ObjectInPlace.OnTooClose.AddListener(onTooClose);
            ObjectInPlace.OnTooClose.AddListener(() => ActivateActor(true));

            ObjectInPlace.OnTooFar.RemoveAllListeners();
            ObjectInPlace.OnTooFar.AddListener(onTooFar);
            ObjectInPlace.OnTooFar.AddListener(() => ActivateActor(false));
        }
    }
}
