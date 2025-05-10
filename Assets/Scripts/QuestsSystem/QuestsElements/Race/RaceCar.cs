using Actors;
using System;
using System.Collections;
using QuestsSystem.QuestsElements;
using UnityEngine;

public class RaceCar : QuestElement
{
    private int _indexOfCurrentCheckPoint;

    private Vector3 _currentCheckPoint;

    [SerializeField] private float _changingCheckpointDistance = 2;
    [SerializeField] private AICarController aICarController;
    [SerializeField] private CarActor carActor;
    public Func<int, Vector3> GetCheckpointPosition;
    public Action OnCompleteRace;
    private bool delay = true;
    private int _currentCircle = 0;
    private int _amountOfCircles = -1;
    
    public void Initialize(int circlesAmout, float countDown)
    {
        _amountOfCircles = circlesAmout;
        _currentCircle = 0;
        _indexOfCurrentCheckPoint = 0;
        carActor.InitializeQuestCar();
        aICarController.TargetPosition = transform.position;

        StartCoroutine(StartCountdown(countDown));

    }

    IEnumerator StartCountdown(float countDown)
    {
        yield return new WaitForSeconds(countDown);
        _currentCheckPoint = GetCheckpointPosition(_indexOfCurrentCheckPoint);
        aICarController.TargetPosition = _currentCheckPoint;
    }

    IEnumerator DelayChanging()
    {
        yield return new WaitForSeconds(0.2f);

        delay = true;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, _currentCheckPoint) <= _changingCheckpointDistance && delay)
        {
            _indexOfCurrentCheckPoint++;

            if (_currentCheckPoint == GetCheckpointPosition(_indexOfCurrentCheckPoint))
            {
                _currentCircle++;
                if (_currentCircle >= _amountOfCircles)
                    OnCompleteRace?.Invoke();
                else
                    _indexOfCurrentCheckPoint = 0;
            }

            _currentCheckPoint = GetCheckpointPosition(_indexOfCurrentCheckPoint);
            aICarController.TargetPosition = _currentCheckPoint;
            delay = false;
            StartCoroutine(DelayChanging());
        }
    }

    public override void ReturnToPool()
    {
        questName = null;
        GetCheckpointPosition = null;
        carActor.ReturnToPool();
    }
}
