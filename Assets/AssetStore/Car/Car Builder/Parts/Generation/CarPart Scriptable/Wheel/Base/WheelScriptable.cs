using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class WheelScriptable : CarPartScriptable
    {
        [Header("Cap Size")]
        [SerializeField] protected float wheelCapRadiusPercent = 0.7f;
        [SerializeField] protected float wheelCapWidthPercent = 1f;

        private static Mesh _tireMesh;

        protected static float _wheelCapRadiusPercentage;
        protected static float _wheelCapWidthPercentage;

        public override void Initialize()
        {
            _wheelCapRadiusPercentage = wheelCapRadiusPercent;
            _wheelCapWidthPercentage = wheelCapWidthPercent;
        }

        public virtual WheelMeshes Generate(float radius, float tireThickness, bool rightSide, int resolution)
        {
            return new WheelMeshes();
        }


        protected Mesh GenerateTire(float outerRadius, float innerRadius, float tireThickness, int resolution)
        {
            if (_tireMesh == null)
            {
                _tireMesh = new Mesh();
            }
            else
            {
                _tireMesh.Clear();
            }

            _tireMesh = CombineMeshes.Combine(_tireMesh, CylinderGenerator.Generate_Hollow(outerRadius, tireThickness, outerRadius - innerRadius, resolution, Wheels.WheelAcrossResolution, GeneralMeshGenerator.Axis.X));

            return _tireMesh;
        }

        public struct WheelMeshes
        {
            public Mesh Tire;
            public Mesh Cap;
        }
    }
}
