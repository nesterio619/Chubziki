using System;
using UnityEngine;
using Actors.Constructors;
using Regions;

namespace Actors.EditorActors
{
    [ExecuteInEditMode]
    public class EditorActor : EditorSceneElement
    {
        [SerializeField] private Location actorLocation;

#if UNITY_EDITOR

        protected virtual void OnEnable()
        {
            AdingInListOfElements();
        }

        protected new void Start()
        {
            base.Start();
        }

        public virtual void Initialize(Location location)
        {
            actorLocation = location;
        }

        private void AdingInListOfElements()
        {
            EditorActorConstructor.Instance.AddExistingElement(actorLocation, gameObject);
        }

#endif
    }
}