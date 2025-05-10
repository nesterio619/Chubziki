using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace Core.InputManager
{
    public enum InputDeviceType { Keyboard, Gamepad }

    public static class InputManager
    {
        private static GameInput inputActions;

		private static InputAction horizontalAxis, verticalAxis, stopAlternativeUse, useStop, pauseCancel;
        public static InputAction leftMouseButton;

        public delegate void ButtonDelegate();

        public static event ButtonDelegate OnStopOrAlternativeUse;

        public static event ButtonDelegate OnUseStop;

        public static event ButtonDelegate OnPauseCancel;

		public delegate void AxisDelegate(float axis);

        public static event AxisDelegate OnHorizontalAxis;

        public static event AxisDelegate OnVerticalAxis;

        public static event Action<InputDeviceType> OnInputDeviceChanged;
        public static InputDeviceType CurrentInputDevice { get; private set; }

        private static float mouseSensitive = 1;
		private static bool isContolsByMouse;

		public static bool IsContolsByMouse
		{
			get => isContolsByMouse;
			set
			{
				isContolsByMouse = value;
				if (isContolsByMouse) ChangeControlsToMouse();
				else ChangeControlsToMain();
			}
		}
		internal static void Initialize()
        {
            inputActions = new();
            inputActions.Enable();
			IsContolsByMouse = false;

			Player.Instance.OnUpdateEvent += OnUpdate;

            horizontalAxis.performed += CheckDevice;
            verticalAxis.performed += CheckDevice;
            inputActions.MouseControls.MovementHorizontal.performed += CheckDevice;
            inputActions.MouseControls.MovementVertical.performed += CheckDevice;
            inputActions.GamepadCursorControls.MovementVertical.performed += CheckDevice;
        }

        private static void CheckDevice(InputAction.CallbackContext context)
        {
            InputDeviceType newDevice;
            string newDeviceName = context.action.activeControl.device.name;

            if (newDeviceName == "Keyboard" || newDeviceName == "Mouse")
                newDevice = InputDeviceType.Keyboard;
            else
                newDevice = InputDeviceType.Gamepad;

            if (newDevice == CurrentInputDevice) return;

            CurrentInputDevice = newDevice;
            OnInputDeviceChanged?.Invoke(newDevice);

            Cursor.visible = newDevice == InputDeviceType.Keyboard;
        }

        private static void OnUpdate()
        {
	        if(Player.Instance.CarGameObjectIsNull) return;

            VerticalAxis();
            HorizontalAxis();

			if (stopAlternativeUse.IsPressed())
				OnStopOrAlternativeUse?.Invoke();

			if (useStop.IsPressed())
				OnUseStop?.Invoke();

			if (pauseCancel.WasPressedThisFrame())
				OnPauseCancel?.Invoke();
		}
		private static void HorizontalAxis()
		{
			float horizontalInput = 0f;
			if (IsContolsByMouse)
			{
				var mouseDeltaX = Mouse.current.delta.x.ReadValue();
				if (Mathf.Abs(mouseDeltaX) > 0.1f)
				{
					// We scale down the delta to fit into range of -1 to 1
					horizontalInput = (2f / Mathf.PI) * Mathf.Atan(mouseDeltaX) * mouseSensitive;

					// Extra check for better safety
					horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
				}
			}
			else if(horizontalAxis.ReadValue<float>()!=0)
			{
				horizontalInput = horizontalAxis.ReadValue<float>();
			}
            OnHorizontalAxis?.Invoke(horizontalInput);
        }
		private static void VerticalAxis()
		{
			float verticalInput = 0f;

			if (verticalAxis.ReadValue<float>() != 0)
				verticalInput = verticalAxis.ReadValue<float>();

			verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
			OnVerticalAxis?.Invoke(verticalInput);
        }

		public static void ToggleInputActions(bool state)
		{
			if (state)
				inputActions.Enable();
			else
				inputActions.Disable();
		}
		
		private static void ChangeControlsToMain()
		{
			horizontalAxis = inputActions.MainControls.MovementHorizontal;
			verticalAxis = inputActions.MainControls.MovementVertical;
			stopAlternativeUse = inputActions.MainControls.StopAlternativeUse;
			useStop = inputActions.MainControls.UseStop;
			pauseCancel = inputActions.MainControls.PauseCancel;
            leftMouseButton = inputActions.MainControls.LeftMouseButton;
		}
		private static void ChangeControlsToMouse()
		{
			horizontalAxis = inputActions.MouseControls.MovementHorizontal;
			verticalAxis = inputActions.MouseControls.MovementVertical;
			stopAlternativeUse = inputActions.MouseControls.StopAlternativeUse;
			useStop = inputActions.MouseControls.UseStop;
			pauseCancel = inputActions.MouseControls.PauseCancel;
            leftMouseButton = inputActions.MainControls.LeftMouseButton;
        }

        public static void Dispose()
        {
            inputActions.Dispose();
            Player.Instance.OnUpdateEvent -= OnUpdate;
        }
	}
}