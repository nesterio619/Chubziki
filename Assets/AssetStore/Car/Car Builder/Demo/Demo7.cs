using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo7 : MonoBehaviour
    {
        [SerializeField] [TextArea(30, 50)] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;
        [SerializeField] private CarPrefabSaver _prefabSaver = default;

        private bool _safeMode = true;
        private bool _tweaking = true;

        [Header("Save Car")]
        [SerializeField] private bool _save;

        private void Update()
        {
            // Dont create a "new" mesh, keep updating the current one
            var carData = _carSettings.GenerateData( _safeMode, _tweaking);
            var car = CarGenerator.Generate(carData,null, false);

            if(_save)
            {
                _save = false;
                _prefabSaver.SaveCar(car.GetComponent<TemporaryCarInitializer>());
            }
        }

    }

}
