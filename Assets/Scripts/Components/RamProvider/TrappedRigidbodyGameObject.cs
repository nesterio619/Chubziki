using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrappedRigidbodyGameObject
{
    public Rigidbody TrappedRigidbody;

    public List<Collider> ChildColliders = new();

    public TrappedRigidbodyGameObject(Rigidbody mainObject, Collider collider)
    {
        TrappedRigidbody = mainObject;

        ChildColliders.Add(collider);
    }

    public bool RemoveAndCheckOnLast(Collider collider)
    {
        ChildColliders.Remove(collider);

        return ChildColliders.Count == 0;
    }

}
