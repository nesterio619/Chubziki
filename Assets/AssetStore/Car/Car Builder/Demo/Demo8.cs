using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo8 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;

        [Header("Settings")]
        [SerializeField] [Range(0.4f, 1f)] private float _speed = 0.6f;
        [SerializeField] [Range(0.1f, 0.4f)] private float _turnSpeed = 0.2f;

        private CarPartReferences _car;

        private void Start()
        {
            var carData = _carSettings.GenerateData();
            _car = CarGenerator.Generate(carData);


        }

        //simple Car steering example
        private void Update()
        {
            var forwardInput = Input.GetAxis("Vertical");
            var sidewaysInput = Input.GetAxis("Horizontal");

            var movementAmount = forwardInput * _speed * Time.deltaTime;
            var rotationAmount = sidewaysInput * _turnSpeed * forwardInput;
            var wheelRotationAmount = sidewaysInput * _turnSpeed * Mathf.Abs(forwardInput) * 100;

            _car.transform.Rotate(Vector3.up * rotationAmount, Space.Self);
            _car.transform.position += _car.transform.forward * movementAmount;

            var allWheels = new Transform[] { _car.WheelsBackLeft, _car.WheelsBackRight, _car.WheelsFrontLeft, _car.WheelsFrontRight };
            var frontWheels = new Transform[] { _car.WheelsFrontLeft, _car.WheelsFrontRight };

            var wheelCircumference = CircleCircumference(_car.WheelRadius);
            var wheelRotation = (movementAmount / wheelCircumference) * 360;

            for (int i = 0; i < frontWheels.Length; i++)
            {
                frontWheels[i].transform.parent.localRotation = Quaternion.Euler(Vector3.up * wheelRotationAmount);
            }

            for (int i = 0; i < allWheels.Length; i++)
            {
                allWheels[i].transform.Rotate(Vector3.right * wheelRotation, Space.Self);
            }  
        }

        private float CircleCircumference(float radius)
        {
            return Mathf.PI * radius * 2;
        }

    }

}
