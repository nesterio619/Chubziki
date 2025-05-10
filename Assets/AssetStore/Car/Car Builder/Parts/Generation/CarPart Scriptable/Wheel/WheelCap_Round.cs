using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    [CreateAssetMenu(fileName = "WheelCap_Round", menuName = "CarBuilder/PartGenerators/Wheel/WheelCap_Round", order = 1)]
    public class WheelCap_Round : WheelScriptable
    {
        private static Mesh _targetMesh;

        public override WheelMeshes Generate(float radius, float tireThickness, bool rightSide, int resolution)
        {
            if (_targetMesh == null)
            {
                _targetMesh = new Mesh();
            }
            else
            {
                _targetMesh.Clear();
            }

            var data = CarGenerator.ActiveDataSet.WheelData;
            var capRadius = radius * _wheelCapRadiusPercentage;
            var capThickness = tireThickness * _wheelCapWidthPercentage;
            _targetMesh = CombineMeshes.Combine(_targetMesh, CylinderGenerator.Generate(capRadius, capThickness, resolution, Wheels.WheelAcrossResolution, true, GeneralMeshGenerator.Axis.X));


            var result = new WheelMeshes();
            result.Tire = GenerateTire(radius, capRadius, tireThickness, resolution).OverrideUVs(data.ColorSettings.TireUV, 0);
            result.Cap = _targetMesh.OverrideUVs(data.ColorSettings.CapUV, 0);

            return result;
        }
    }
}
