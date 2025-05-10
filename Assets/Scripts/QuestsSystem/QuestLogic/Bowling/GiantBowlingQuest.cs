using System;
using System.Collections.Generic;
using Components.Camera;
using Core;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestsElements;
using UnityEngine;

namespace QuestsSystem.QuestLogic.Bowling
{
    public class GiantBowlingQuest : BowlingQuest
    {
        private BowlingBall _bowlingBall;
        private SmoothTopDownCameraMovement _smoothTopDownCameraMovement;
        private bool _hasTouchedBowlingBall = false;
        protected override Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions
        {
            get
            {
                // Get base class actions
                var baseActions = base.QuestElementSetupActions;
            
                // Add child-specific actions
                baseActions.Add(typeof(BowlingBall), (questElement, transformToSet) =>
                {
                    _bowlingBall = (BowlingBall)questElement;
                    
                    _bowlingBall.transform.SetParent(transformToSet, false);
                    _bowlingBall.transform.localScale = Vector3.one;
                    _bowlingBall.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    
                    _bowlingBall.BowlingBallRigidbody.isKinematic = false;
                    
                    _bowlingBall.OnQuestEventTriggered += OnPlayerTouchedBowlingBall;
                    
                    _bowlingBall.SetQuestName(QuestName);
                    
                    Physics.SyncTransforms();
                });
                return baseActions;
            }
        }

        public void Initialize(QuestConfig.QuestConfig questConfig ,float timeTotalForMission,float minOffsetBowlingPin,float maxOffsetBowlingPin) /// Нужно исправить
        {
            base.Initialize(questConfig, timeTotalForMission, minOffsetBowlingPin, maxOffsetBowlingPin);
            _smoothTopDownCameraMovement = Camera.main.GetComponentInParent<SmoothTopDownCameraMovement>();
            _hasTouchedBowlingBall = false;
        }
        
        protected override void UnloadQuestElements()
        {
            foreach (var questElement in CreatedQuestElements)
            {
                switch (questElement)
                {
                    case BowlingBall bowlingBall:
                        bowlingBall.OnQuestEventTriggered -= OnPlayerTouchedBowlingBall;
                        break;
                }
            }
            base.UnloadQuestElements();
        }
    
        public override void Dispose()
        {
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions();
            
            _smoothTopDownCameraMovement.SetTarget(Player.Instance.PlayerCarGameObject.transform);

            _hasTouchedBowlingBall = false;
            
            base.Dispose();
        }

        private void OnPlayerTouchedBowlingBall()
        {
            if (_hasTouchedBowlingBall) return;
            _hasTouchedBowlingBall = true;

            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);
            
            _smoothTopDownCameraMovement.SetTarget(_bowlingBall.transform);
            
            SetTimer();
        }

        protected override void OnPlayerTouchedBrakingLine()
        {
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);
        }
    }
}
