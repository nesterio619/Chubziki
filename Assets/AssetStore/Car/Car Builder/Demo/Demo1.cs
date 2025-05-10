using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo1 : MonoBehaviour
    {
        [SerializeField] [TextArea] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;

        [Space]
        [SerializeField] private Transform _carHolder = default;

        void Start()
        {
            var car = CarGenerator.Generate(_carSettings.GenerateData());
            car.transform.SetParent(_carHolder, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        void Update()
        {
            _carHolder.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.5f, Mathf.PingPong(Time.time * 0.33f, 1));
        }
    }

}
