using Regions;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IMoving
    {
        public void AddVisibleObject() => VisibleActorsManager.AddActingObject(this);

        public Bounds GetBounds();

        public Transform GetLocationParent();

        public Transform GetSelfTransform();

        public Actors.Actor GetActor();

        public bool IsOutOfSector { get;}

        public bool IsVisible { get; set; }
        
        public bool IsSectorLoaded { get; }

        public void UnloadIfOutOfBounds();

        public void SetSector(Sector sector);
    }
}