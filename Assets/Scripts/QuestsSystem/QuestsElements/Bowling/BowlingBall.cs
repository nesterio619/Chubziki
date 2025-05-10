using System;
using Core.Interfaces;
using QuestsSystem.Base;
using UnityEngine;

namespace QuestsSystem.QuestsElements
{
    public class BowlingBall : QuestElement, IPushable
    {
        private Rigidbody _bowlingBallRigidbody;
        
        public Rigidbody BowlingBallRigidbody
        {
            get => _bowlingBallRigidbody;
            private set => _bowlingBallRigidbody = value;
        }
        
        private void Awake()
        {
            BowlingBallRigidbody = GetComponent<Rigidbody>();
        }
        
        public override void ReturnToPool()
        {
            BowlingBallRigidbody.isKinematic = true;
            base.ReturnToPool();
        }

        public Rigidbody GetRigidbody()
        {
            return BowlingBallRigidbody;
        }

        public void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode)
        {
            BowlingBallRigidbody.AddForce(pushForce, forceMode);
            OnQuestEventTriggered?.Invoke();
        }

        public Vector3 GetVelocity()
        {
            return BowlingBallRigidbody.velocity;
        }
    }
}
