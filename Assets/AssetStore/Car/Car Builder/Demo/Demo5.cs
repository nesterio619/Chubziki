using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo5 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private CarSettings _carSettingsA = default;
        [SerializeField] private CarSettings _carSettingsB = default;

        private CarData _carDataA;
        private CarData _carDataB;
        private CarData _carDataBlend;

        void Start()
        {
            _carDataA = _carSettingsA.GenerateData();
            _carDataB = _carSettingsB.GenerateData();
        }

        private void Update()
        {
            float progress = Mathf.PingPong(Time.time * 0.5f, 1);

            _carDataBlend = CarData.Blend(_carDataA, _carDataB, progress, _carDataBlend);

            // after Blending it could happen that Data doesn't produce legitimate results. Thus do a Sanity check / modify values where needed
            CarData.SanityCheckData(_carDataBlend);

            // Dont create a new car, keep updating the current one
            CarGenerator.Generate(_carDataBlend,null, false);
        }

    }

}
