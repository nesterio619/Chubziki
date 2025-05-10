using UnityEngine;
using UnityEngine.InputSystem;

namespace Components.Car.CarLogic
{
    public class MouseCarControl : MonoBehaviour
    {
        public GameObject CarObject;

        public WheelTurning wheelTurning;

        public Vector3 mousePosition;

        public GameObject example;

        public Vector3 point;

        public Vector3 forward;

        public Vector3 side1;

        public Vector3 side2;

        public float pointMagnitude;

        public float forwardMagnitude;

        public LayerMask layer;

        public float angle;

        public float offsetBeforeTurningWheel;

        public float degreeForTurning;

        public float currentTurningValue;

        private void Start()
        {
            example = GameObject.Find("Cube");
        }

        private void Update()
        {
            Ray r = UnityEngine.Camera.main.ScreenPointToRay(Mouse.current.position.value);

            if (Physics.Raycast(r, out RaycastHit hit, layer))
            {
                point = hit.point;

                example.transform.position = hit.point;

                mousePosition = transform.position;

                point.y = 0;

                mousePosition.y = 0;

                forward = mousePosition + transform.forward * 10f;

                forward.y = 0;

                side1 = point - mousePosition;
                side2 = forward - mousePosition;

                pointMagnitude = side1.magnitude;
                forwardMagnitude = side2.magnitude;

                //Fingally find angle
                angle = Vector3.SignedAngle(side1, side2, Vector3.up);
            }

            if (angle > 0)
            {
                angle -= offsetBeforeTurningWheel;

                if (angle < 0)
                {
                    angle = 0;
                }
            }
            else
            {
                angle += offsetBeforeTurningWheel;

                if (angle > 0)
                {
                    angle = 0;
                }
            }
            currentTurningValue = -(angle / degreeForTurning);

            currentTurningValue = Mathf.Clamp(currentTurningValue, -1, 1);

            wheelTurning.TurningWheels(currentTurningValue);


        }
        /*
        public void MousePresser()
        {
            wheelTurning.TurningWheels(0);
        }*/

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, transform.forward * 10f);
            Gizmos.DrawRay(transform.position, (point - transform.position).normalized * 10f);

        }

    }
}