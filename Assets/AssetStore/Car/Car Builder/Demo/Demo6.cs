using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo6 : MonoBehaviour
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
            _carDataBlend = CarData.Blend(_carDataA, _carDataA, 0, _carDataBlend);
        }

        private void Update()
        {
            float progress = Mathf.PingPong(Time.time * 0.5f, 1);     

            Back.Data.Blend(_carDataA.BackData, _carDataB.BackData, progress, _carDataBlend.BackData);
            Nose.Data.Blend(_carDataA.NoseData, _carDataB.NoseData, 1 - progress, _carDataBlend.NoseData);
            Body.Data.Blend(_carDataA.BodyData, _carDataB.BodyData, 1 - progress, _carDataBlend.BodyData);
            
            // after Blending it could happen that Data doesn't produce legitimate results. Thus do a Sanity check / modify values where needed
            CarData.SanityCheckData(_carDataBlend);

            // Dont create a new car, keep updating the current one
            CarGenerator.Generate(_carDataBlend,null, false);
        }

    }

}
