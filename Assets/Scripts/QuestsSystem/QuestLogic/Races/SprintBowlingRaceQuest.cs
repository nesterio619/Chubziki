using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Utilities;
using QuestsSystem.Base;
using QuestsSystem.QuestsElements;
using QuestsSystem.QuestsElements.Bowling;
using UI.Debug;
using Unity.Mathematics;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public class SprintBowlingRaceQuest : RaceQuest
    {
        protected override Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions
        {
            get
            {
                // Get base class actions
                var baseActions = base.QuestElementSetupActions;
            
                // Add child-specific actions
                baseActions.Add(typeof(PinObject), (questElement, transformToSet) =>
                {
                    var pinObject = (PinObject)questElement;
                    
                    pinObject.transform.SetParent(transformToSet, false);
                    pinObject.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    pinObject.SetQuestName(QuestName);
                    pinObject.BowlingPinRigidbody.isKinematic = false;
                    Physics.SyncTransforms();

                    pinObject.OnQuestEventTriggered += KnockDownPin;

                    _pins.Add(pinObject);
                });

                return baseActions;
            }
        }

        private int totalPinCount => _pins.Count;

        private int _currentKnockedDownPins;

        private float _timeOnMission;

        //private float _timeTotalForMission = 25f;

        private Coroutine _timeCoroutine;

        private List<PinObject> _pins = new List<PinObject>();

        protected internal override void OnAccept()
        {
            questLoadedCallbacks.Add(SetTimer);

            base.OnAccept();

            questLoadedCallbacks.Add(() => DirectionPoint.Instance.Show(_pins[0].transform.position));
        }

        private void SetTimer()
        {
            _timeCoroutine = Player.Instance.StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            while(!questIsCompleted)
            {
                _timeOnMission += Time.deltaTime;

                if(debugCanvasCommand != null)
                    debugCanvasCommand.Update();

                yield return null;
            }
        }

        private void KnockDownPin()
        {
            _currentKnockedDownPins++;

            var pin = GetFirstStandingPin();
            if (pin == null)
                DirectionPoint.Instance.Show(finish.transform.position);
            else
                DirectionPoint.Instance.Show(pin.transform.position);

            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }

        public override void Dispose()
        {
            Player.Instance.StopCoroutine(_timeCoroutine);

            // Move the player to a default position
            TeleportPlayerToWhereQuestWasAccepted();
            //Reset Circles Count back 
            _currentKnockedDownPins = 0;
            _pins.Clear();
            base.Dispose();
        }


        protected override void CreateDebugUI()
        {
            debugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => 
                $"{QuestName}:{Environment.NewLine}Pin Count : {_currentKnockedDownPins}/{totalPinCount},{Environment.NewLine}Timer :{_timeOnMission:F0}");

            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }

        protected override void UnloadQuestElements()
        {
            foreach (var questElement in CreatedQuestElements)
            {
                switch (questElement)
                {
                    case PinObject pinObject:
                        pinObject.OnQuestEventTriggered -= KnockDownPin;
                        break;
                }
            }

            base.UnloadQuestElements();
        }

        protected override void OnPlayerReachFinish()
        {
            if (_currentKnockedDownPins >= totalPinCount)
            {
                OnComplete(true);
                return;
            }

            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }
        
        protected override void OnPlayerTouchFallTrigger()
        {
            OnComplete(false);
        }

        private PinObject GetFirstStandingPin()
        {
            foreach(var pin in _pins)
                if(!pin.IsKnockedDown)
                    return pin;

            return null;
        }
    }
}