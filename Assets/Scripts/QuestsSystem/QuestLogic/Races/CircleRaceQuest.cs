using System;
using System.Collections.Generic;
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestsElements;
using QuestsSystem.QuestsElements.Race;
using UI.Debug;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public class CircleRaceQuest : RaceQuest
    {
        private int _currentCircle = 0;
        private int _circlesToCompleteQuest = 3;
        private Dictionary<RoadCheckPoint, Vector3> _checkpointInitialLocalPositions = new Dictionary<RoadCheckPoint, Vector3>(); 

        protected override void OnPlayerReachFinish()
        {
            if (_currentRoadCheckPointIndex < _totalCheckpointCount) return;

            _currentRoadCheckPointIndex = 0;

            DirectionPoint.Instance.Show(GetPositionOfCheckpoint(1));
            
            foreach(var checkpoint in checkpoints)
                checkpoint.ResetCheckpoint();

            _currentCircle++;

            if (_currentCircle == _circlesToCompleteQuest)
            {
                OnComplete(true);
                return;
            }
            
            ResetCheckpoints();

            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }

        RoadCheckPoint _currentCheckpoint;


        protected override Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions
        {
            get
            {
                var baseActions = base.QuestElementSetupActions;

                baseActions[typeof(RoadCheckPoint)] = (questElement, transformToSet) =>
                {
                    _currentCheckpoint = (RoadCheckPoint)questElement;
                    _currentCheckpoint.transform.SetParent(transformToSet);
                    _currentCheckpoint.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    _currentCheckpoint.SetQuestName(QuestName);

                    _currentCheckpoint.OnQuestEventTriggered += OnPlayerCrossCurrentCheckpoint;

                    _checkpointInitialLocalPositions[_currentCheckpoint] = _currentCheckpoint.GraphicsToMoveOnCross.localPosition;

                    checkpoints.Add(_currentCheckpoint);

                    _totalCheckpointCount++;
                };

                return baseActions;
            }
        }
        
        private void ResetCheckpoints()
        {
            foreach (var questElement in CreatedQuestElements)
            {
                if (questElement is RoadCheckPoint checkpoint)
                {
                 
                    checkpoint.TryCancelAnimation(); 
                    checkpoint.GraphicsToMoveOnCross.localPosition = _checkpointInitialLocalPositions[checkpoint]; 
                    checkpoint.ToggleAllRenderers(true); 
                }
            }

         
            if (CreatedQuestElements.Count > 0 && CreatedQuestElements[0] is RoadCheckPoint firstCheckpoint)
            {
                _currentCheckpoint = firstCheckpoint;
            }
        }

        protected internal override void OnAccept()
        {
            base.OnAccept();

            questLoadedCallbacks.Add(() => DirectionPoint.Instance.Show(GetPositionOfCheckpoint(0)));
        }
        public override void Dispose()
        {
            _currentCircle = 0;
            // Move the player to a default position
            _checkpointInitialLocalPositions.Clear();
            TeleportPlayerToWhereQuestWasAccepted();

            base.Dispose();
        }

        protected override int GetAmountOfCircles()
        {
            return _circlesToCompleteQuest;
        }

        protected override void CreateDebugUI()
        {
            debugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"{QuestName}: Circles Count : {_currentCircle}");
            debugCanvasCommand.Update();
        }

        public void Initialize(QuestConfig.QuestConfig questConfig, int circlesToComplete)
        {
            base.Initialize(questConfig);
            _circlesToCompleteQuest = circlesToComplete;
        }
    }
}