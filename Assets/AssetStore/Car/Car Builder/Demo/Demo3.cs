using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo3 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;

        private CarPartReferences _car;

        void Start()
        {
            var carData = _carSettings.GenerateData();
            _car = CarGenerator.Generate(carData);       
        }

        private void Update()
        {
            _car.TrunkBonnet.localRotation = Quaternion.Euler(new Vector3(Mathf.PingPong(Time.time * 18, 45), 0, 0));
            _car.Hood.localRotation = Quaternion.Euler(new Vector3(-Mathf.PingPong(Time.time * 18, 45), 0, 0));
        }

    }

}
