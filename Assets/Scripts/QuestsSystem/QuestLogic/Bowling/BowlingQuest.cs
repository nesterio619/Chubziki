using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestsElements;
using QuestsSystem.QuestsElements.Bowling;
using UI.Debug;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuestsSystem.QuestLogic
{
    public abstract class BowlingQuest : QuestLogic
    {
        protected override Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions
        {
            get
            {
                // Get base class actions
                var baseActions = base.QuestElementSetupActions;

                // Add child-specific actions
                baseActions.Add(typeof(BrakingLine), (questElement, transformToSet) =>
                {
                    var brakingLine = (BrakingLine)questElement;

                    brakingLine.transform.SetParent(transformToSet, false);
                    brakingLine.transform.localScale = Vector3.one;
                    brakingLine.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    brakingLine.SetQuestName(QuestName);

                    brakingLine.OnQuestEventTriggered += OnPlayerTouchedBrakingLine;
                });
                
                baseActions.Add(typeof(PinObject), (questElement, transformToSet) =>
                {
                    pinsParentTransform = transformToSet.parent;

                    BowlingPinRandomPosition();

                     var pinObject = (PinObject)questElement;
                    
                    pinObject.transform.SetParent(transformToSet, false);
                    pinObject.transform.localScale = Vector3.one;
                    pinObject.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    Physics.SyncTransforms();
                    pinObject.SetQuestName(QuestName);
                    pinObject.BowlingPinRigidbody.isKinematic = false;

                    pinObject.OnQuestEventTriggered += KnockDownPin;
                    totalPinCount++;

                    if(_firstPin==null) _firstPin = pinObject;
                    
                    Physics.SyncTransforms();
                });
                
                baseActions.Add(typeof(BowlingAlley), (questElement, transformToSet) =>
                {
                    var bowlingAlley = (BowlingAlley)questElement;
                    
                    bowlingAlley.transform.SetParent(transformToSet, false);
                    bowlingAlley.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    bowlingAlley.SetQuestName(QuestName);
                });
                
                baseActions.Add(typeof(DirectionArrow), (questElement, transformToSet) =>
                {
                    RotateDirectionArrow(transformToSet);

                    var directionArrow = (DirectionArrow)questElement;
                    
                    directionArrow.transform.SetParent(transformToSet, false);
                    directionArrowParentTransform = transformToSet;
                    directionArrow.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    directionArrow.SetQuestName(QuestName);
                });
                return baseActions;
            }
        }

        private void RotateDirectionArrow(Transform transformToSet)
        {
            defaultArrowRotation = transformToSet.rotation.eulerAngles;
            Vector3 direction = pinsParentTransform.GetChild(0).position - transformToSet.position;
            float targetAngleY = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            transformToSet.localRotation *= Quaternion.Euler(0, -targetAngleY, 0);
        }

        private void BowlingPinRandomPosition()
        {
            pinsParentTransform.localPosition = Vector3.zero;
            Vector3 randomOffset = new Vector3(Random.Range(minOffsetBowlingPin, maxOffsetBowlingPin), 0, Random.Range(minOffsetBowlingPin, maxOffsetBowlingPin));
            pinsParentTransform.position += randomOffset;
        }

        protected int totalPinCount = 0;
        protected int currentKnockedDownPins = 0;
        protected float timeTotalForMission = 0f;
        protected float timeSpend = 0f;
        protected Transform pinsParentTransform;
        protected float minOffsetBowlingPin;
        protected float maxOffsetBowlingPin;
        protected Vector3 defaultArrowRotation;
        protected Transform directionArrowParentTransform;
        protected bool isDisposing = false;
        
        private Coroutine _timeCoroutine;

        private PinObject _firstPin;

        protected virtual void KnockDownPin()
        {
            currentKnockedDownPins++;
            timeSpend = 0;
            
            if(debugCanvasCommand != null)
                debugCanvasCommand.Update();
        }

        protected void SetTimer()
        {
            if (_timeCoroutine == null)
                _timeCoroutine = Player.Instance.StartCoroutine(Timer());
        }

        protected IEnumerator Timer()
        {
            while (true)
            {
                timeSpend += Time.deltaTime;

                if (timeSpend <= timeTotalForMission && currentKnockedDownPins == totalPinCount)
                {
                    yield return new WaitForSeconds(3f);
                    OnComplete(true);
                    _timeCoroutine = null; 
                   yield break;
                }

                if (timeSpend >= timeTotalForMission)
                {
                    OnComplete(false);
                    _timeCoroutine = null; 
                   yield break;
                }
                   
                
                if (debugCanvasCommand != null)
                    debugCanvasCommand.Update();

                yield return null;
            }
        }
        
        protected override void CreateDebugUI()
        {
            debugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => 
                $"{QuestName}:{Environment.NewLine}Pin Count : {currentKnockedDownPins}/{totalPinCount},{Environment.NewLine}Timer :{timeSpend:F0}/{timeTotalForMission:F0}");

            debugCanvasCommand.Update();
        }
        
        protected override void UnloadQuestElements()
        {
            foreach (var questElement in CreatedQuestElements)
            {
                switch (questElement)
                {
                    case BrakingLine raceStart:
                        raceStart.OnQuestEventTriggered -= OnPlayerTouchedBrakingLine;
                        break;
                    case PinObject pinObject:
                        pinObject.OnQuestEventTriggered -= KnockDownPin;
                        break;
                }
            }

            base.UnloadQuestElements();
        }
        public override void OnComplete(bool success)
        {
            Debug.Log($"BowlingQuest {QuestName} completed with success: {success}");
        
            if (debugCanvasCommand != null)
            {
                debugCanvasCommand.Dispose(); 
                debugCanvasCommand = null;
            }

            Dispose();
            TeleportPlayerToWhereQuestWasAccepted();
            base.OnComplete(success);
            QuestsManager.Instance.CompleteQuest(QuestName, success);
            DirectionPoint.Instance.Hide();
        }
        public override void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;
            directionArrowParentTransform.transform.rotation = Quaternion.Euler(defaultArrowRotation);
            
            currentKnockedDownPins = 0;
            totalPinCount = 0; 
            timeSpend = 0;
            
            base.Dispose();
            isDisposing = false;
        }

        protected void Initialize(QuestConfig.QuestConfig questConfig,float timeTotalForMission,float minOffsetBowlingPin,float maxOffsetBowlingPin)
        {
            base.Initialize(questConfig);
            this.timeTotalForMission = timeTotalForMission;
            this.minOffsetBowlingPin = minOffsetBowlingPin;
            this.maxOffsetBowlingPin = maxOffsetBowlingPin;
        }

        protected abstract void OnPlayerTouchedBrakingLine();

        protected internal override void OnAccept()
        {
            base.OnAccept();

            questLoadedCallbacks.Add(() => DirectionPoint.Instance.Show(_firstPin.transform.position));
        }
    }
}
