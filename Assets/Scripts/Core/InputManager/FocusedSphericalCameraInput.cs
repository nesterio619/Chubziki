using Actors.AutoRepairShop;
using DG.Tweening;
using UnityEngine;

namespace Core.InputManager
{
    public class FocusedSphericalCameraInput : MonoBehaviour
    {
        private const float Camera_Reset_Time = 0.5f;
        private const Ease Camera_Reset_Ease = Ease.OutCirc;

        [SerializeField] private Transform defaultTarget;

        private GameInput _inputActions;

        private FocusedSphericalCameraController _cameraController;

        private void Start()
        {
            _inputActions = new();
            _inputActions.Enable();

            _cameraController = GetComponentInChildren<FocusedSphericalCameraController>();
            _cameraController.SetTarget(defaultTarget);
        }

        private void Update()
        {
            ReadCameraInput();
        }

        private void ReadCameraInput()
        {
            float horizontal = 0;
            float vertical = 0;

            if (_inputActions.CameraControls.DragButton.IsPressed())
            {
                horizontal = _inputActions.CameraControls.DragHorizontal.ReadValue<float>();
                vertical = _inputActions.CameraControls.DragVertical.ReadValue<float>();
            }
            else
            {
                horizontal = _inputActions.CameraControls.MovementHorizontal.ReadValue<float>();
                vertical = _inputActions.CameraControls.MovementVertical.ReadValue<float>();
            }

            var zoom = _inputActions.CameraControls.CameraZoom.ReadValue<float>();

            _cameraController.HandleCameraMovement(horizontal, vertical);
            _cameraController.HandleCameraZoom(zoom);
        }

        public void FocusCameraOnPlayer()
        {
            if (_cameraController == null) return;
            var target = Player.Instance.PlayerCarGameObject.transform;

            _cameraController.SetTarget(target);
        }
        public void SetCameraTarget(Transform transform)
        {
            if (_cameraController == null) return;

            _cameraController.SetTarget(transform);
        }
        public void SetCameraRadius(float radius)
        {
            if (_cameraController == null) return;

            _cameraController.SetRadius(radius);
        }
    }
}