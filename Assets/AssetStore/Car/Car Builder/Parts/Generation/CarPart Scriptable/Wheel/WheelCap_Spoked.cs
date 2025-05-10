using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    [CreateAssetMenu(fileName = "WheelCap_Spoked", menuName = "CarBuilder/PartGenerators/Wheel/WheelCap_Spoked", order = 1)]
    public class WheelCap_Spoked : WheelScriptable
    {
        [Header("Cap Specifics")]
        [SerializeField] private float capInwardsPercent = 0.3f;
        [SerializeField] private float capOuterPercent = 0.7f;
        [SerializeField] private float wheelSpokePercent = 0.9f;

        private static float _capInwardsPercentage;
        private static float _wheelOuterPercentage;
        private static float _spokePercentage;

        private static Mesh _targetMesh;

        public override void Initialize()
        {
            base.Initialize();

            _capInwardsPercentage = capInwardsPercent;
            _wheelOuterPercentage = capOuterPercent;
            _spokePercentage = wheelSpokePercent;
        }


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
            var centerRadius = capRadius * (1 - _wheelOuterPercentage);
            var outerCapRadius = capRadius * _wheelOuterPercentage;
            var capThickness = tireThickness * _wheelCapWidthPercentage;

            CombineMeshes.Combine(_targetMesh, CylinderGenerator.Generate_Hollow(centerRadius, capThickness * (1 - _capInwardsPercentage), centerRadius, resolution, Wheels.WheelAcrossResolution, GeneralMeshGenerator.Axis.X));

            var spokeRadius = outerCapRadius * _spokePercentage;
            var outerRingRadius = outerCapRadius - spokeRadius;

            CombineMeshes.Combine(_targetMesh, CylinderGenerator.Generate_Hollow(capRadius, capThickness, outerRingRadius, resolution, Wheels.WheelAcrossResolution, GeneralMeshGenerator.Axis.X));

            //Spokes
            for (int i = 0; i < resolution; i += 2)
            {
                var offset = new Vector3(capThickness * (rightSide ? 0.5f : -0.5f), 0, 0);

                float progress = (float)i / resolution;
                float radians = Mathf.PI * 2 * progress;
                var inner = offset + GetCirclePos(radians) * centerRadius;
                var outer = offset + GetCirclePos(radians) * (capRadius - outerRingRadius);

                float progress2 = (float)(i + 1) / resolution;
                float radians2 = Mathf.PI * 2 * progress2;
                var inner2 = offset + GetCirclePos(radians2) * centerRadius;
                var outer2 = offset + GetCirclePos(radians2) * (capRadius - outerRingRadius);

                inner.x *= (1 - _capInwardsPercentage);
                inner2.x *= (1 - _capInwardsPercentage);

                var positions = new Vector3[4];
                positions[0] = inner;
                positions[1] = inner2;
                positions[2] = outer2;
                positions[3] = outer;

                if (rightSide == false)
                    positions.Reverse();

                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(positions, Vector2Int.one, rightSide ? Vector3.right : Vector3.left));
            }


            var result = new WheelMeshes();
            result.Tire = GenerateTire(radius, capRadius, tireThickness, resolution).OverrideUVs(data.ColorSettings.TireUV, 0);
            result.Cap = _targetMesh.OverrideUVs(data.ColorSettings.CapUV, 0);

            return result;


            Vector3 GetCirclePos(float radians)
            {
                return new Vector3(0, Mathf.Sin(radians), Mathf.Cos(radians)).normalized;
            }
        }
    }
}
