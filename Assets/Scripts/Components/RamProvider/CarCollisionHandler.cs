using Actors;
using Components.Car;
using Components.RamProvider;
using System;
using UnityEngine;

public class CarCollisionHandler : CollisionHandler
{
    [SerializeField] private CarActor carActor;
    [SerializeField] private CarDriving carDriving;

    [SerializeField] private float minimalSpeedForDamage = 20f;

    [SerializeField]
    [Range(0, 1)]
    private float damageFromSpeed = 0.1f;

    [SerializeField][Tooltip("If the angle of impact is greater than this, it does not cause damage.")]
    private float permissibleAngle = 20;

    //If the car flips over, this variable represents angle 
    private const float angleOfCarTurnedOver = 110;

#if UNITY_EDITOR
    [SerializeField] private bool activeDebugLog = false;
#endif
    
    [field: SerializeField]
    public RigidbodyRamProvider BodyCarRamProvider { private set; get; }

    public Action<Collision> OnHittingEnvironment;

    private void Start() =>
        Core.Utilities.UtilitiesProvider.ForceAddListener(ref OnEnter, OnHitEnvironment);

    public void OnHitEnvironment(Collision environmentCollider)
    {
        if(Mathf.Abs(carDriving.CarSpeed) < minimalSpeedForDamage
           || Mathf.Abs(transform.rotation.x) >= angleOfCarTurnedOver)
            return;

        float damageAngle = FindHitAngle(environmentCollider);
#if UNITY_EDITOR
        if (activeDebugLog)
            Debug.Log("Angle of damage: " + damageAngle);
#endif
        if (damageAngle > permissibleAngle) 
            return;
        
        var damage = Mathf.RoundToInt((Mathf.Abs(carDriving.CarSpeed) - minimalSpeedForDamage) * damageFromSpeed);
        OnHittingEnvironment?.Invoke(environmentCollider);
        carActor.ChangeHealthBy(-damage);
    }

    private float FindHitAngle(Collision environmentCollider)
    {
        Vector3 carPosition = transform.position;
        carPosition += transform.up;

        Vector3 direction = environmentCollider.contacts[0].point - carPosition;
        direction.Normalize();

        return Quaternion.LookRotation(direction).eulerAngles.x; 
    }
}
