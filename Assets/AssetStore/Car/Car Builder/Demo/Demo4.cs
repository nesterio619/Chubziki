using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo4 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;

        private GameObject _randomCar1;
        private GameObject _randomCar2;

        void Start()
        {
            var carData = _carSettings.GenerateData();
            var car = CarGenerator.Generate(carData);
            car.transform.position = Vector3.right * -6;

            carData = _carSettings.GenerateData();
            car = CarGenerator.Generate(carData);
            car.transform.position = Vector3.right * 6;

            RandomCars();

            InvokeRepeating(nameof(RandomCars), 2, 2);
        }

        private void RandomCars()
        {
            Destroy(_randomCar1);
            Destroy(_randomCar2);

            var carData = _carSettings.GenerateData();
            var car = CarGenerator.Generate(carData);
            car.transform.position = Vector3.right * -2;
            _randomCar1 = car.gameObject;

            carData = _carSettings.GenerateData();
            car = CarGenerator.Generate(carData);
            car.transform.position = Vector3.right * 2;
            _randomCar2 = car.gameObject;
        }
    }

}
