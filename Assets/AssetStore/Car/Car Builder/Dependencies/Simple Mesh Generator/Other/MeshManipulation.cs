using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>

namespace SimpleMeshGenerator
{
    public static class MeshManipulation
    {
        private static List<Vector3> verts = new List<Vector3>();
        private static List<Vector3> normals = new List<Vector3>();
        private static List<Color> colors = new List<Color>();

        private static List<Vector4> uvs = new List<Vector4>();
        private static List<Vector2> uvsB0 = new List<Vector2>();

        private static List<Vector2> uvsA1 = new List<Vector2>();
        private static List<Vector2> uvsB1 = new List<Vector2>();

        private static List<Vector2> uvsA2 = new List<Vector2>();
        private static List<Vector2> uvsB2 = new List<Vector2>();

        private static List<int> triangles = new List<int>();

        private static Vector4 uv = new Vector4();

        private static List<int> _samePosVertsIDs = new List<int>();

        public static List<int> TriangleGeneration(int resolutionWidth, int resolutionLength, bool flipOrientation)
        {
            triangles.Clear();

            for (int x = 0; x < resolutionWidth - 1; x++)
            {
                for (int y = 0; y < resolutionLength - 1; y++)
                {
                    int indexBase = x * resolutionLength + y;

                    if (flipOrientation)
                    {
                        triangles.Add(indexBase);
                        triangles.Add(indexBase + 1);
                        triangles.Add(indexBase + 1 + resolutionLength);

                        triangles.Add(indexBase);
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase + resolutionLength);
                    }
                    else
                    {
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase + 1);
                        triangles.Add(indexBase);

                        triangles.Add(indexBase + resolutionLength);
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase);
                    }
                }
            }

            return triangles;
        }

        public static List<int> TriangleGeneration_Method2(int resolutionWidth, int resolutionLength, bool flipOrientation)
        {
            triangles.Clear();

            for (int x = 0; x < resolutionWidth - 1; x += 2)
            {
                for (int y = 0; y < resolutionLength - 1; y += 2)
                {
                    int indexBase = x * resolutionLength + y;

                    if (flipOrientation)
                    {
                        triangles.Add(indexBase);
                        triangles.Add(indexBase + 1);
                        triangles.Add(indexBase + 1 + resolutionLength);

                        triangles.Add(indexBase);
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase + resolutionLength);
                    }
                    else
                    {
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase + 1);
                        triangles.Add(indexBase);

                        triangles.Add(indexBase + resolutionLength);
                        triangles.Add(indexBase + 1 + resolutionLength);
                        triangles.Add(indexBase);
                    }
                }
            }

            return triangles;
        }

        public static void ConvertToSoftNormals(ref Mesh mesh, float threshold = 0.01f)
        {
            mesh.GetVertices(verts);
            mesh.GetNormals(normals);

            for (int i = 0; i < verts.Count; i++)
            {
                _samePosVertsIDs.Clear();

                for (int j = 0; j < verts.Count; j++)
                {
                    if (i == j)
                        continue;

                    var vertA = verts[i];
                    var vertB = verts[j];

                    bool sameLocation = Vector3.Distance(vertA, vertB) < threshold;
                    if (sameLocation)
                    {
                        _samePosVertsIDs.Add(j);
                    }
                }

                if (_samePosVertsIDs.Count > 0)
                {
                    var combinedNormal = normals[i];
                    for (int j = 0; j < _samePosVertsIDs.Count; j++)
                    {
                        int id = _samePosVertsIDs[j];
                        combinedNormal += normals[id];
                    }

                    combinedNormal = combinedNormal.normalized;


                    normals[i] = combinedNormal;
                    for (int j = 0; j < _samePosVertsIDs.Count; j++)
                    {
                        int id = _samePosVertsIDs[j];
                        normals[id] = combinedNormal;
                    }
                }
            }

            mesh.SetNormals(normals);
        }

        public static void BoxUVChannel_XY(ref Mesh mesh, Vector2 min, Vector2 max, int targetChannel)
        {
            uvs.Clear();
            mesh.GetVertices(verts);

            for (int i = 0; i < verts.Count; i++)
            {
                var vert = verts[i];
                uv.x = Utility.GetPercentage(vert.x, min.x, max.x);
                uv.y = Utility.GetPercentage(vert.y, min.y, max.y);

                uvs.Add(uv);
            }

            mesh.SetUVs(targetChannel, uvs);
        }

        public static void SetWorldUVs_XY(ref Mesh mesh)
        {
            BoxUVChannel_XY(ref mesh, new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), 1);
        }

        public static Mesh OverrideNormals(this Mesh m, Vector3 normal)
        {
            normal = normal.normalized;
            normals.Clear();

            for (int i = 0; i < m.vertexCount; i++)
                normals.Add(normal);

            m.SetNormals(normals);

            return m;
        }

        public static Mesh OverrideUVs(this Mesh m, Vector2 uv, int channel)
        {
            uvs.Clear();

            for (int i = 0; i < m.vertexCount; i++)
                uvs.Add(uv);

            m.SetUVs(channel, uvs);

            return m;
        }

        public static Mesh OverrideUVsByScaling(this Mesh m, Vector2 uvScaler, int channel)
        {
            uvs.Clear();
            m.GetUVs(0, uvs);

            for (int i = 0; i < uvs.Count; i++)
                uvs[i] = uvs[i] * uvScaler;

            m.SetUVs(channel, uvs);

            return m;
        }

        public static Mesh BoxUVChannel_XZ(this Mesh m, Vector2 min, Vector2 max, int targetChannel)
        {
            float aspectRatio = Mathf.Abs(max.x - min.x) / Mathf.Abs(max.y - min.y);

            return BoxUVChannel_XZ(m, min, max, aspectRatio, targetChannel);
        }

        public static Mesh BoxUVChannel_XZ(this Mesh m, Vector2 min, Vector2 max, float aspectRatio, int targetChannel)
        {
            uvs.Clear();
            m.GetVertices(verts);

            for (int i = 0; i < verts.Count; i++)
            {
                var vert = verts[i];
                uv.x = Utility.GetPercentage(vert.x, min.x, max.x);
                uv.y = Utility.GetPercentage(vert.z, min.y, max.y);
                uv.z = aspectRatio;

                uvs.Add(uv);
            }

            m.SetUVs(targetChannel, uvs);
            return m;
        }

        public static Mesh SetOrigin(this Mesh m, Vector3 origin)
        {
            m.GetVertices(verts);
            for (int i = 0; i < verts.Count; i++)
                verts[i] = verts[i] - origin;

            m.SetVertices(verts);
            m.RecalculateBounds();

            return m;
        }

        public static Mesh AddPositionOffset(this Mesh m, Vector3 offset)
        {
            m.GetVertices(verts);
            for (int i = 0; i < verts.Count; i++)
                verts[i] = verts[i] + offset;

            m.SetVertices(verts);
            m.RecalculateBounds();

            return m;
        }


        public static void Rotate(ref Mesh mesh, Vector3 translate, Vector3 eulerAnglers, bool test = false)
        {
            var scale = Vector3.one;

            // Set a Quaternion from the specified Euler angles.
            Quaternion rotation = Quaternion.Euler(eulerAnglers);

            // Set the translation, rotation and scale parameters.
            Matrix4x4 m_TRS = Matrix4x4.TRS(translate, rotation, scale);
            Matrix4x4 m_R = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);


            Matrix4x4 m_TRS_TEST = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.right * (90 - 116.5651f)), scale);

            mesh.GetVertices(verts);
            mesh.GetNormals(normals);
            //var verts = mesh.vertices;
            // For each vertex...
            for (int i = 0; i < verts.Count; i++)
            {
                // Apply the matrix to the vertex.

                if (test)
                    verts[i] = (m_TRS * m_TRS_TEST).MultiplyPoint3x4(verts[i]);
                else
                    verts[i] = (m_TRS).MultiplyPoint3x4(verts[i]);
            }

            // For each vertex...
            for (int i = 0; i < normals.Count; i++)
            {
                // Apply the matrix to the vertex.
                normals[i] = m_R.MultiplyPoint3x4(normals[i]);
            }

            // Copy the transformed vertices back to the mesh.
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
        }

        public static void Rotate(ref Vector3 point, Vector3 eulerAnglers)
        {
            // Set a Quaternion from the specified Euler angles.
            Quaternion rotation = Quaternion.Euler(eulerAnglers);

            // Set the translation, rotation and scale parameters.
            // Matrix4x4 m_TRS = Matrix4x4.TRS(translate, rotation, scale);
            Matrix4x4 m_R = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

            point = m_R.MultiplyPoint3x4(point);
        }
    }
}
