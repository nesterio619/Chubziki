using Core.Interfaces;
using Core.Utilities;
using UnityEngine;

namespace QuestsSystem.QuestsElements.Bowling
{
    public class PinObject : QuestElement, IPushable
    {
        [SerializeField] private Transform pin;

        private Rigidbody _bowlingPinRigidbody;

        private Vector3 _lastLocalPosition;
        private Quaternion _lastLocalRotation;
        
        public bool IsKnockedDown { get; private set; }

        public Rigidbody BowlingPinRigidbody
        {
            get => _bowlingPinRigidbody;
            private set => _bowlingPinRigidbody = value;
        }
  
        private void Awake()
        {
            _lastLocalPosition = pin.localPosition;

            _lastLocalRotation = pin.localRotation;
            
            _bowlingPinRigidbody = GetComponentInChildren<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if(IsKnockedDown || BowlingPinRigidbody.isKinematic)
                return;
            
            if (MathUtils.ObjectIsTilted(transform, 10, Axis.X | Axis.Z))
                InvokeAction();
        }

        public void InvokeAction()
        {
            if (IsKnockedDown) return;
            
            IsKnockedDown = true;
            OnQuestEventTriggered?.Invoke();
        }

        public override void ReturnToPool()
        {
            IsKnockedDown = false;

            pin.localPosition = _lastLocalPosition;
            pin.localRotation =  _lastLocalRotation;
            
            BowlingPinRigidbody.isKinematic = true;
            
            base.ReturnToPool();
        }

        public Rigidbody GetRigidbody()
        {
            return BowlingPinRigidbody;
        }

        public void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode)
        {
            BowlingPinRigidbody.AddForce(pushForce / BowlingPinRigidbody.mass, forceMode);

            InvokeAction();
        }

        public Vector3 GetVelocity()
        {
            return BowlingPinRigidbody.velocity;
        }
    }
}