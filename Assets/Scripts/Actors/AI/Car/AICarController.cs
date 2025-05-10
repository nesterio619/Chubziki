using Actors;
using Components.Car;
using System.Collections;
using UnityEngine;

public class AICarController : MonoBehaviour
{
    [SerializeField] private CarDriving carDriving;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float steeringSensitivity = 1f;
    [SerializeField] private CarActor carActor;
    //private Transform _targetTransform;
    public Vector3 TargetPosition;


    [SerializeField] private Transform[] rayForwardPosition;
    [SerializeField] private LayerMask environmentLayer;

    private float _lastSteeringInput = 0;
    private float _currentTooLongTimeTurning = 0;
    private float _tooLongTimeTurning = 3;

    private void FixedUpdate()
    {
        AICarControllerUpdate();
    }

    private void AICarControllerUpdate()
    {
        bool isStuck = CheckForwardObstacles();

        Vector3 targetPosition = TargetPosition;

        // Calculate the direction to the player
        Vector3 directionToPlayer = targetPosition - carActor.transform.position;
        directionToPlayer.y = 0; // Ignore vertical difference

        // Get the distance to the player
        float distanceToPlayer = directionToPlayer.magnitude;

        // Stop the car if within stopping distance
        if (distanceToPlayer <= stoppingDistance)
        {
            carDriving.OnVerticalInput(0);
            return;
        }

        // Calculate the required steering angle
        Vector3 forward = carActor.transform.forward;
        float angleToPlayer = Vector3.SignedAngle(forward, directionToPlayer, Vector3.up);

        // Determine steering input
        float steeringInput = Mathf.Clamp(angleToPlayer * steeringSensitivity, -1f, 1f);

        // Apply forward movement
        if(_currentTooLongTimeTurning >= _tooLongTimeTurning)
        {
            carDriving.Brake();
            return;
        }
        
        if (isStuck)
        {
            carDriving.OnHorizontalInput(-steeringInput);
            carDriving.OnVerticalInput(-1);

        }
        else
        {
            if (steeringInput == _lastSteeringInput)
            {
                _currentTooLongTimeTurning += Time.deltaTime;
                if (_currentTooLongTimeTurning >= _tooLongTimeTurning)
                    StartCoroutine(ResetTimer());
            }
            else
            {
                _currentTooLongTimeTurning = 0;
                _lastSteeringInput = steeringInput;
            }

            carDriving.OnHorizontalInput(steeringInput);
            carDriving.OnVerticalInput(1);
        }
    }

    private IEnumerator ResetTimer()
    {

        yield return new WaitForSeconds(1);
        _currentTooLongTimeTurning = 0;
    }

    public bool CheckForwardObstacles()
    {
        foreach (var item in rayForwardPosition)
        {
            if (Physics.Raycast(item.position, item.forward, 2f, environmentLayer))
                return true;

        }


        return false;
    }
}
