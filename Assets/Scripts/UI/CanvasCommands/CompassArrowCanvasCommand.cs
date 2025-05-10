using Core;
using Core.ObjectPool;
using QuestsSystem.Base;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CompassArrowCanvasCommand : CanvasCommand
    {
        private const string Pool_Path = "ScriptableObjects/ObjectPool/UI/CompassArrow";
        private const float Min_Spring_Stiffness = 37f;  // Strength of the spring force
        private const float Max_Spring_Stiffness = 75f;  // Strength of the spring force
        private const float Min_Spring_Damping = 1f;     // Resistance to motion (higher = less wobble)
        private const float Max_Spring_Damping = 5f;     // Resistance to motion (higher = less wobble)

        public bool IsEnabled => _arrowImage != null && _arrowImage.gameObject.activeSelf;

        private readonly Transform _originTransform;

        private Transform _arrowTransform;
        private Image _arrowImage;

        private float currentVelocity = 0f;  
        private float currentAngle = 0f;

        public CompassArrowCanvasCommand(CanvasReceiver receiver, Transform originTransform) : base(receiver)
        {
            _originTransform = originTransform;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var prefabPoolInfo = (PrefabPoolInfo)Resources.Load(Pool_Path);

            PooledObjectReference = ObjectPooler.TakePooledGameObject(prefabPoolInfo);

            PooledObjectReference.transform.SetParent(Receiver.Canvas.transform, false);
            PooledObjectReference.transform.localScale = Vector3.one;
            PooledObjectReference.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            _arrowTransform = PooledObjectReference.transform;
            _arrowImage = _arrowTransform.GetComponentInChildren<Image>();
        }

        public override void Update()
        {
            float targetRotation = CalculateZRotation();
            float currentRotation = _arrowTransform.localRotation.eulerAngles.z;

            float angleDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
            float absoluteDifference = Mathf.Abs(angleDifference);

            // Adaptive Stiffness & Damping (higher force when far, higher damping when close)
            float stiffness = Mathf.Lerp(Min_Spring_Stiffness, Max_Spring_Stiffness, absoluteDifference / 30f);
            float damping = Mathf.Lerp(Max_Spring_Damping, Min_Spring_Damping, absoluteDifference / 30f);

            // Apply Hooke's Law: Force = -stiffness * displacement
            float force = -stiffness * angleDifference;

            // Apply damping: Opposes velocity
            float dampingForce = -damping * currentVelocity;

            // Compute acceleration (simplified physics: F = ma, assume m = 1)
            float acceleration = force + dampingForce;

            // Integrate velocity
            currentVelocity += acceleration * Time.deltaTime;

            // Integrate position (apply movement)
            currentAngle += currentVelocity * Time.deltaTime;

            // Apply final rotation
            _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
        }

        private float CalculateZRotation()
        {
            var directionPoint = DirectionPoint.Instance.transform;
            var direction = directionPoint.position - _originTransform.position;

            return Mathf.Atan2(-direction.x, direction.z) * Mathf.Rad2Deg;
        }

        public void EnableImage(bool enable)
        {
            _arrowImage.gameObject.SetActive(enable);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            if (PooledObjectReference != null)
                ObjectPooler.ReturnPooledObject(PooledObjectReference);

            base.Dispose();
        }
    }
}