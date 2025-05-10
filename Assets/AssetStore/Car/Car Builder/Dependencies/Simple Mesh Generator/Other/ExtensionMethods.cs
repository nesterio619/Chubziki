using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>

namespace SimpleMeshGenerator
{
    public static class ExtensionMethods
    {
        public static Vector2 ConvertToVec2(this Vector3 v, Vector3 mask)
        {
            Vector2 converted = Vector2.zero;

            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                if (mask[i] != 0)
                {
                    converted[index] = v[i];
                    index++;
                }
            }

            return converted;
        }

        public static Vector3 ConvertToVec3(this Vector2 v)
        {
            Vector3 converted = new Vector3(v.x, v.y, 0);

            return converted;
        }


        public static Vector2 XY(this Vector3 v)
        {
            Vector2 converted = Vector2.zero;

            converted.x = v.x;
            converted.y = v.y;

            return converted;
        }

        public static Vector2 XZ(this Vector3 v)
        {
            Vector2 converted = Vector2.zero;

            converted.x = v.x;
            converted.y = v.z;

            return converted;
        }

        public static Vector2 ZY(this Vector3 v)
        {
            Vector2 converted = Vector2.zero;

            converted.x = v.z;
            converted.y = v.y;

            return converted;
        }


        public static Vector2 ToAxis(this Vector3 v, GeneralMeshGenerator.Axis2D axis)
        {
            switch (axis)
            {
                case GeneralMeshGenerator.Axis2D.XY:
                    return v.XY();
                case GeneralMeshGenerator.Axis2D.XZ:
                    return v.XZ();
                case GeneralMeshGenerator.Axis2D.ZY:
                    return v.ZY();
            }
            return v;
        }

        public static Vector3 ToAxisInverse(this Vector2 v, GeneralMeshGenerator.Axis2D axis, float missingAxisValue)
        {
            switch (axis)
            {
                case GeneralMeshGenerator.Axis2D.XY:
                    return new Vector3(v.x, v.y, missingAxisValue);
                case GeneralMeshGenerator.Axis2D.XZ:
                    return new Vector3(v.x, missingAxisValue, v.y);
                case GeneralMeshGenerator.Axis2D.ZY:
                    return new Vector3(missingAxisValue, v.y, v.x);
            }
            return v;
        }

        public static float GetRandom(this Vector2 v)
        {
            return Random.Range(v.x, v.y);
        }

        public static int GetRandom(this int[] i)
        {
            return i[Random.Range(0, i.Length)];
        }

        public static bool IsEven(this int i)
        {
            return i % 2 == 0;
        }

        public static T GetValueAt<T>(this List<T> list, int index)
        {
            if (list.IsNullOrEmpty())
            {
                Debug.LogError("trying to get an element out of an empty Array");
                return default(T);
            }
            else
            {
                if (index >= 0)
                {
                    return list[index % list.Count];
                }
                else
                {
                    return list[list.Count + index];
                }
            }
        }

        public static T GetValueAt<T>(this T[] array, int index)
        {
            if (array.IsNullOrEmpty())
            {
                Debug.LogError("trying to get an element out of an empty Array");
                return default(T);
            }
            else
            {
                if (index >= 0)
                {
                    return array[index % array.Length];
                }
                else
                {
                    return array[array.Length + index];
                }
            }
        }


        public static Vector3[] ConvertToVec3(this Vector2[] v)
        {
            var converted = new Vector3[v.Length];

            for (int i = 0; i < converted.Length; i++)
            {
                converted[i] = new Vector3(v[i].x, v[i].y, 0);
            }

            return converted;
        }

        public static Vector3[] FlipX(this Vector3[] v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                var pos = v[i];
                pos.x = -pos.x;
                v[i] = pos;
            }

            return v;
        }

        public static Vector3[] FlipXClone(this Vector3[] v)
        {
            var converted = new Vector3[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                var pos = v[i];
                pos.x = -pos.x;
                converted[i] = pos;
            }

            return converted;
        }

        public static List<Vector3> FlipXClone(this List<Vector3> v)
        {
            var converted = new List<Vector3>();

            for (int i = 0; i < v.Count; i++)
            {
                var pos = v[i];
                pos.x = -pos.x;
                converted.Add(pos);
            }

            return converted;
        }

        public static Vector3[][] FlipXClone(this Vector3[][] v)
        {
            var converted = new Vector3[v.Length][];

            for (int x = 0; x < v.Length; x++)
            {
                converted[x] = v[v.Length - 1 - x];
            }

            return converted;
        }

        public static Vector3 FlipX(this Vector3 v)
        {
            v.x = -v.x;
            return v;
        }

        public static Vector3 FlipXClone(this Vector3 v)
        {
            return new Vector3(-v.x, v.y, v.z);
        }

        public static Vector3 ReplaceXClone(this Vector3 v, float value)
        {
            return new Vector3(value, v.y, v.z);
        }

        public static Vector3 ReplaceYClone(this Vector3 v, float value)
        {
            return new Vector3(v.x, value, v.z);
        }

        public static Vector3 ReplaceZClone(this Vector3 v, float value)
        {
            return new Vector3(v.x, v.y, value);
        }

        public static Vector3[] ReplaceZClone(this Vector3[] v, float value)
        {
            var converted = new Vector3[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                var pos = v[i];
                pos.z = value;
                converted[i] = pos;
            }

            return converted;
        }

        public static void Reverse(this Vector3[] v)
        {
            var halfwayPoint = Mathf.FloorToInt(v.Length * 0.5f + 0.001f);

            var c = Vector3.zero;
            for (int i = 0; i < halfwayPoint; i++)
            {
                c = v[i];

                v[i] = v[v.Length - 1 - i];
                v[v.Length - 1 - i] = c;
            }
        }

        public static Vector3[] ReversedClone(this Vector3[] v)
        {
            var converted = new Vector3[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                converted[i] = v[v.Length - 1 - i];
            }

            return converted;
        }

        public static Vector3 Last(this Vector3[] v)
        {
            return v[v.Length - 1];
        }

        public static Vector3 Last(this List<Vector3> v)
        {
            return v[v.Count - 1];
        }

        public static Vector2 Last(this List<Vector2> v)
        {
            return v[v.Count - 1];
        }

        public static List<Vector3> Clone(this List<Vector3> v)
        {
            var converted = new List<Vector3>();
            converted.AddRange(v);

            return converted;
        }



        public static Vector3[] GetRange(this Vector3[] v, int range)
        {
            var converted = new Vector3[range];

            for (int i = 0; i < range; i++)
            {
                converted[i] = v[i];
            }

            return converted;
        }

        public static Vector3[] Positions(this Transform[] transforms)
        {
            var points = new Vector3[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                points[i] = transforms[i].position;
            }

            return points;
        }

        public static void Offset(this int[] v, int offset)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] += offset;
            }
        }

        public static void Offset(this List<int> v, int offset)
        {
            var length = v.Count;
            for (int i = 0; i < length; i++)
            {
                v[i] += offset;
            }
        }

        public static void Offset(this Vector3[] v, Vector3 offset)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] += offset;
            }
        }

        public static void Offset(this List<Vector3> v, Vector3 offset)
        {
            for (int i = 0; i < v.Count; i++)
            {
                v[i] += offset;
            }
        }


	    public static void CopyFrom(this Vector2[] v, Vector2[] value)
	    {
		    int length = Mathf.Min(v.Length, value.Length);
		    for (int i = 0; i < length; i++)
		    {
			    v[i] = value[i];
		    }
	    }
	    public static void CopyFrom(this Vector2[] v, List<Vector2> value)
	    {
		    int length = Mathf.Min(v.Length, value.Count);

		    for (int i = 0; i < length; i++)
		    {
			    v[i] = value[i];
		    }
	    }

		public static void CopyFrom(this Vector3[] v, Vector3[] value)
        {
            int length = Mathf.Min(v.Length, value.Length);
            for (int i = 0; i < length; i++)
            {
                v[i] = value[i];
            }
        }
        public static void CopyFrom(this Vector3[] v, List<Vector3> value)
        {
            int length = Mathf.Min(v.Length, value.Count);

            for (int i = 0; i < length; i++)
            {
                v[i] = value[i];
            }
        }

        public static float Min(this Vector2 v)
        {
            return Mathf.Min(v.x, v.y);
        }

        public static float Max(this Vector2 v)
        {
            return Mathf.Max(v.x, v.y);
        }

        public static float Min(this Vector3 v)
        {
            return Mathf.Min(v.x, v.y, v.z);
        }

        public static float Max(this Vector3 v)
        {
            return Mathf.Max(v.x, v.y, v.z);
        }


        public static Vector3 GetPosition(this Transform t, bool world)
        {
            if (world) return t.position;
            else return t.localPosition;
        }


        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static T GetRandomElement<T>(this T[] array)
        {
            if (array.IsNullOrEmpty())
            {
                Debug.LogError("trying to get an element out of an empty Array");
                return default(T);
            }
            else
            {
                return array[Random.Range(0, array.Length)];
            }
        }

        public static void SwapWith<T>(this T[] a, T[] b)
        {
            var c = a;
            a = b;
            b = c;
        }

        public static void SwapWith<T>(this List<T> a, List<T> b)
        {
            var c = a;
            a = b;
            b = c;
        }


        public static void SetParentReset(this Transform t, Transform targetParent)
        {
            t.SetParent(targetParent, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void SetParent(this Transform t, Transform targetParent, Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            t.SetParent(targetParent);
            t.localPosition = localPos;
            t.localRotation = localRot;
            t.localScale = localScale;
        }





    }
}
