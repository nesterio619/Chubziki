using System.Collections.Generic;
using Actors;
using MicroWorldNS;
using UnityEngine;

public class QuestZoneActor : TriggerHandler // Этот код где то используется?
{
    [SerializeField] private List<GameObject> questGameObjects;
    
    private List<Actor> _actors = new List<Actor>();
    
    public void ToggleVisibility(bool state)
    {
        foreach (Actor obj in _actors)
        {
            obj.SwitchGraphic(state);
        }
    }
    
    public void ReturnObjectInPlace()
    {
        foreach (Actor obj in _actors)
        {
            obj.gameObject.transform.localPosition = Vector3.zero;
            
            var objRigidbody = obj.GetComponentInChildren<Rigidbody>();
            
            if (objRigidbody == null) continue;
            
            objRigidbody.velocity = Vector3.zero;
            objRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void GetActorsInZone()
    {
        foreach (var obj in questGameObjects)
        {
            var actor = obj.GetComponentInChildren<Actor>();
            if (actor != null && !_actors.Contains(actor))
            {
                _actors.Add(actor);
            }
        }
    }
}