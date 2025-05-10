using Core;
using Core.Utilities;
using QuestsSystem.Base;
using System.Collections;
using QuestsSystem.QuestConfig;
using UI.Debug;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public class SprintRaceQuest : RaceQuest
    {
        protected float _timeSpend = 0.0f;
        protected float _timeTotalForMission = 25f;
        protected Coroutine _timeCoroutine;

        protected internal override void OnAccept()
        {
            questLoadedCallbacks.Add(SetTimer);

            base.OnAccept();

            questLoadedCallbacks.Add(() => DirectionPoint.Instance.Show(GetPositionOfCheckpoint(0)));
        }

        protected void SetTimer()
        {
            _timeCoroutine = Player.Instance.StartCoroutine(Timer());
        }

        protected IEnumerator Timer()
        {
            while (true)
            {
                _timeSpend += Time.deltaTime;

                if (_timeSpend >= _timeTotalForMission)
                    OnComplete(false);

                if (debugCanvasCommand != null)
                    debugCanvasCommand.Update();

                yield return null;
            }
        }

        public override void Dispose()
        {
            if (_timeCoroutine != null)
            {
                Player.Instance.StopCoroutine(_timeCoroutine);
                _timeCoroutine = null; 
            }
            if (debugCanvasCommand != null)
            {
                debugCanvasCommand.Dispose();
                debugCanvasCommand = null;
            }

            TeleportPlayerToWhereQuestWasAccepted();

            _currentRoadCheckPointIndex = 0;
            _totalCheckpointCount = 0;
            _timeSpend = 0.0f;

            base.Dispose();
        }


        protected override void CreateDebugUI()
        {
            debugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"{QuestName} TimeSpend : {_timeTotalForMission:F0} / {_timeSpend:F0}");

            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }

        protected override void OnPlayerReachFinish()
        {
            if (_currentRoadCheckPointIndex < _totalCheckpointCount)
                return;
            OnComplete(true);
        }
        
        protected override void OnPlayerTouchFallTrigger()
        {
            OnComplete(false);
        }

        public void Initialize(QuestConfig.QuestConfig questConfig, float timeToComplete)
        {
            base.Initialize(questConfig);
            _timeTotalForMission = timeToComplete;
        }

    }

}
