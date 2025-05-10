using Components.Car.CarLogic;
using Core.Utilities;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.InputManager
{
    public class MouseCarControlSwipe : MonoBehaviour
    {
        private WheelTurning _wheelTurning;

        private bool _isMovingMouse;

        public void Initialize(WheelTurning wheelTurning)
        {
            _wheelTurning = wheelTurning;
        }

        public void OnMouseMoving(Mouse mouse)
        {
            float steerAngle = 0;

            if (mouse.delta.right.value > 0)
            {
                steerAngle = (2f / Mathf.PI) * Mathf.Atan(mouse.delta.right.value);

                _wheelTurning.TurningWheels(steerAngle);

                _isMovingMouse = true;
            }
            else if (mouse.delta.left.value > 0)
            {
                steerAngle = (2f / Mathf.PI) * Mathf.Atan(mouse.delta.left.value);

                _wheelTurning.TurningWheels(-steerAngle);

                _isMovingMouse = true;
            }
            else if (_isMovingMouse)
            {
                _wheelTurning.TurningWheels(0);

                _isMovingMouse = false;
            }
        }

    }
}
