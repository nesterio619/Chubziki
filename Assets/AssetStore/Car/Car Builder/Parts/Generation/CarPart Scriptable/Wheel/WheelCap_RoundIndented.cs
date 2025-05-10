using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    [CreateAssetMenu(fileName = "WheelCap_RoundIndented", menuName = "CarBuilder/PartGenerators/Wheel/WheelCap_RoundIndented", order = 1)]
    public class WheelCap_RoundIndented : WheelScriptable
    {
        private static Mesh _targetMesh;

        private Vector3[] _pointsOuter = new Vector3[100];
        private Vector3[] _pointsInner = new Vector3[100];


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
            var rimSize = 0.1f * capRadius;// _rimPercentage * capRadius;
            var outwardsOffset = Vector3.left * tireThickness * 0.5f * Utility.BoolToFlipValue(rightSide);
            var capThickness = tireThickness * _wheelCapWidthPercentage;
            var rotationOffset = (1f / resolution) * 0.5f * Mathf.PI * 2f;

            // rim edge
            var points = CircleGenerator.GetPositions(capRadius, Vector3.up, Vector3.forward, resolution, rotationOffset);
            var pointsCount = points.Count;
            if (points.Count != _pointsOuter.Length)
            {
                _pointsOuter = new Vector3[pointsCount];
                _pointsInner = new Vector3[pointsCount];
            }

            _pointsOuter.CopyFrom(points);
            _pointsOuter.Offset(outwardsOffset);

            _pointsInner.CopyFrom(_pointsOuter);

            DebugUtility.DrawPoints(_pointsOuter, Vector3.right, new Color(0, 0, 1, 0.5f), 10);


            for (int i = 0; i < _pointsInner.Length; i++)
            {
                var dir = -_pointsInner[i];
                dir.x = 0;
                dir = dir.normalized;

                _pointsInner[i] += rimSize * dir;
            }

            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateBridgeHardEdged(_pointsOuter, _pointsInner, false, !rightSide, !rightSide).OverrideUVs(data.ColorSettings.CapUV, 0));


            //inner cap base
            _pointsOuter.SwapWith(_pointsInner);
            var capOuterSize = 0.35f * capRadius;
            var capOffset = Vector3.right * tireThickness * 0.3f * Utility.BoolToFlipValue(rightSide);
            for (int i = 0; i < _pointsInner.Length; i++)
            {
                var dir = -_pointsInner[i];
                dir.x = 0;
                dir = dir.normalized;

                _pointsInner[i] += capOuterSize * dir;
                _pointsInner[i] += capOffset;
            }

           
            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateBridgeHardEdged(_pointsOuter, _pointsInner, false, !rightSide, !rightSide).OverrideUVs(data.ColorSettings.CapUV, 0));


            //cap side
            _pointsOuter.SwapWith(_pointsInner);
            var capTopOutwards = 0.15f * capRadius;
            capOffset = Vector3.left * tireThickness * 0.1f * Utility.BoolToFlipValue(rightSide);
            for (int i = 0; i < _pointsInner.Length; i++)
            {
                var dir = -_pointsInner[i];
                dir.x = 0;
                dir = dir.normalized;

                _pointsInner[i] += capTopOutwards * dir;
                _pointsInner[i] += capOffset;
            }

            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateBridgeHardEdged(_pointsOuter, _pointsInner, false, !rightSide, !rightSide).OverrideUVs(data.ColorSettings.CapCenterUV, 0));


            //cap
            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateFan(_pointsInner, _pointsInner[0], data.ColorSettings.CapCenterUV, Vector3.left * Utility.BoolToFlipValue(rightSide), false, rightSide));


            var result = new WheelMeshes();
            result.Tire = GenerateTire(radius, capRadius, tireThickness, resolution).OverrideUVs(data.ColorSettings.TireUV, 0);
            result.Cap = _targetMesh;

            return result;
        }
    }
}
