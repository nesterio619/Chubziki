using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
/// 
/// UV0
/// 0 - 1: per quad
/// </summary>

namespace SimpleMeshGenerator
{
    // source: https://en.wikipedia.org/wiki/Tetrahedron
    public static class TetrahedronGenerator
    {
        private static Mesh _mesh = new Mesh();

        private const float _faceEdgeFaceAngle = 90 - 70.5288f;
        private const float _faceVertexEdgeAngle = 54.7356f;

        public static Mesh Generate(float height)
        {
            _mesh.Clear();

            var edgeLength = (height / Mathf.Tan(_faceVertexEdgeAngle * Mathf.Deg2Rad)) * 2;

            var radians = Mathf.PI * 0.5f;
            var pointBaseA = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            radians += (1f / 3) * (Mathf.PI * 2f);
            var pointBaseB = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            radians += (1f / 3) * (Mathf.PI * 2f);
            var pointBaseC = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            var pointTop = new Vector3(0, height, 0);


            var normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -60, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate(new Vector3[] { pointBaseA, pointTop, pointBaseB }, Vector2.zero, normal));

            normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -180, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate(new Vector3[] { pointBaseB, pointTop, pointBaseC }, Vector2.zero, normal));

            normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -300, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate(new Vector3[] { pointBaseC, pointTop, pointBaseA }, Vector2.zero, normal));

            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate(new Vector3[] { pointBaseA, pointBaseB, pointBaseC }, Vector2.zero, Vector3.down));

            return _mesh;
        }

        public static Mesh GenerateHollow(float height, float width)
        {
            _mesh.Clear();

            width = width / Mathf.Sin(30 * Mathf.Deg2Rad);
            var edgeLength = (height / Mathf.Tan(_faceVertexEdgeAngle * Mathf.Deg2Rad)) * 2;

            var radians = Mathf.PI * 0.5f;
            var pointBaseA = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            radians += (1f / 3) * (Mathf.PI * 2f);
            var pointBaseB = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            radians += (1f / 3) * (Mathf.PI * 2f);
            var pointBaseC = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized * edgeLength * 0.5f;

            var pointTop = new Vector3(0, height, 0);


            var normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -60, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate_Hollow(new Vector3[] { pointBaseA, pointTop, pointBaseB }, normal, width));

            normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -180, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate_Hollow(new Vector3[] { pointBaseB, pointTop, pointBaseC }, normal, width));

            normal = pointBaseA.normalized;
            MeshManipulation.Rotate(ref normal, new Vector3(-_faceEdgeFaceAngle, -300, 0));
            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate_Hollow(new Vector3[] { pointBaseC, pointTop, pointBaseA }, normal, width));

            CombineMeshes.Combine(_mesh, TriangleGenerator.Generate_Hollow(new Vector3[] { pointBaseA, pointBaseB, pointBaseC }, Vector3.down, width));

            return _mesh;
        }
    }
}
