using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Utilities;
using QuestsSystem.Base;
using QuestsSystem.QuestsElements;
using QuestsSystem.QuestsElements.Race;
using UI.Debug;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public abstract class RaceQuest : QuestLogic
    {
        #region Overrides

        protected override Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions
        {
            get
            {
                // Get base class actions
                var baseActions = base.QuestElementSetupActions;

                // Add child-specific actions
                baseActions.Add(typeof(RaceStart), (questElement, transformToSet) =>
                {
                    var raceStart = (RaceStart)questElement;

                    raceStart.transform.SetParent(transformToSet);
                    raceStart.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    raceStart.SetQuestName(QuestName);

                    raceStart.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    raceStart.OnQuestEventTriggered += OnPlayerCrossStart;
                });

                // Add child-specific actions
                baseActions.Add(typeof(RaceFinish), (questElement, transformToSet) =>
                {
                    var raceFinish = (RaceFinish)questElement;

                    raceFinish.transform.SetParent(transformToSet);
                    raceFinish.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    raceFinish.SetQuestName(QuestName);

                    raceFinish.OnQuestEventTriggered += OnPlayerReachFinish;

                    finish = raceFinish;
                });

                baseActions.Add(typeof(FallTrigger), (questElement, transformToSet) =>
                {
                    var fallTrigger = (FallTrigger)questElement;

                    fallTrigger.transform.SetParent(transformToSet);
                    fallTrigger.transform.localScale = Vector3.one;
                    fallTrigger.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    fallTrigger.SetQuestName(QuestName);

                    fallTrigger.OnQuestEventTriggered += OnPlayerTouchFallTrigger;
                });

                baseActions.Add(typeof(RoadCheckPoint), (questElement, transformToSet) =>
                {
                    var checkpoint = (RoadCheckPoint)questElement;
                    checkpoint.transform.SetParent(transformToSet);
                    checkpoint.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    checkpoint.SetQuestName(QuestName);

                    checkpoint.OnQuestEventTriggered += OnPlayerCrossCurrentCheckpoint;

                    checkpoints.Add(checkpoint);
                    _totalCheckpointCount++;
                });

                baseActions.Add(typeof(RaceCar), (questElement, transformToSet) =>
                {
                    var raceCar = (RaceCar)questElement;

                    raceCar.transform.SetParent(transformToSet);
                    raceCar.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    raceCar.SetQuestName(QuestName);
                    raceCar.GetCheckpointPosition += GetPositionOfCheckpoint;
                    raceCar.OnCompleteRace += OnRaceCarCompletedRace;

                    raceCar.Initialize(GetAmountOfCircles(), timeToCountdown);
                    Physics.SyncTransforms();

                });

                return baseActions;
            }
        }

        protected float currentTimeCountdown = 0;
        protected float timeToCountdown = 5f;
        protected int _totalCheckpointCount = 0;
        protected int _currentRoadCheckPointIndex = 0;

        protected List<RoadCheckPoint> checkpoints = new List<RoadCheckPoint>();
        protected RaceFinish finish;

        protected override void UnloadQuestElements()
        {
            foreach (var questElement in CreatedQuestElements)
            {
                switch (questElement)
                {
                    case RaceStart raceStart:
                        raceStart.OnQuestEventTriggered -= OnPlayerCrossStart;
                        break;

                    case RaceFinish raceFinish:
                        raceFinish.OnQuestEventTriggered -= OnPlayerReachFinish;
                        break;

                    case FallTrigger fallTrigger:
                        fallTrigger.OnQuestEventTriggered -= OnPlayerTouchFallTrigger;
                        break;

                    case RoadCheckPoint roadCheckPoint:
                        roadCheckPoint.OnQuestEventTriggered -= OnPlayerCrossCurrentCheckpoint;
                        break;
                    case RaceCar raceCar:
                        raceCar.GetCheckpointPosition -= GetPositionOfCheckpoint;
                        raceCar.OnCompleteRace -= OnRaceCarCompletedRace;
                        break;
                }
            }

            base.UnloadQuestElements();
        }

        protected internal override void OnAccept()
        {
            questLoadedCallbacks.Add(() => Player.Instance.StartCoroutine(TimerPlayer()));
            base.OnAccept();
        }

        #endregion
        protected DebugCanvasCommand debugCountdownCanvasCommand;

        protected IEnumerator TimerPlayer()
        {
            Debug.Log("Start timer");

            currentTimeCountdown = timeToCountdown;

            debugCountdownCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"{QuestName}: Circles Count : {(int)currentTimeCountdown}");

            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);

            while (currentTimeCountdown > 0)
            {
                currentTimeCountdown -= Time.deltaTime;

                debugCountdownCanvasCommand.Update();

                yield return null;
            }

            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(false, false, false, false);
            debugCountdownCanvasCommand.Dispose();

            Debug.Log("Stop timer");

        }

        protected virtual void OnPlayerCrossStart() { }

        protected virtual void OnPlayerReachFinish() { }

        protected virtual void OnPlayerTouchFallTrigger()
        {
            OnComplete(false);
        }

        protected virtual void OnRaceCarCompletedRace()
        {
            OnComplete(false);
        }

        public override void OnComplete(bool victory)
        {
            base.OnComplete(victory);

            DirectionPoint.Instance.Hide();
        }

        protected virtual int GetAmountOfCircles()
        {
            return -1;
        }

        protected virtual void OnPlayerCrossCurrentCheckpoint()
        {
            _currentRoadCheckPointIndex++;

            DirectionPoint.Instance.Show(GetPositionOfCheckpoint(_currentRoadCheckPointIndex));
        }

        public virtual Vector3 GetPositionOfCheckpoint(int indexOfCheckPoint)
        {
            if (indexOfCheckPoint >= _totalCheckpointCount || indexOfCheckPoint < 0) return finish.transform.position;

            return checkpoints[indexOfCheckPoint].transform.position;
        }
    }
}

