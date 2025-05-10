using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;

namespace LinearAlgebra {
	public static class Line_Line_Intersection
	{
		public static KeyValuePair<bool, Vector3> IsIntersecting3D
		(
			Vector3 lineAStart, Vector3 lineAEnd,
			Vector3 lineBStart, Vector3 lineBEnd,
			Vector3 axis
		)
		{
			bool isIntersecting = false;

			
			Vector3 mask = new Vector3(1 - axis.x, 1 - axis.y, 1 - axis.z);


			//3d -> 2d
			Vector2 l1_start = lineAStart.ConvertToVec2(mask);// new Vector2(mask[0] != 0 ? lineAStart[0] : lineAStart[1], lineAStartEnd.y);
			Vector2 l1_end = lineAEnd.ConvertToVec2(mask);

			Vector2 l2_start = lineBStart.ConvertToVec2(mask);
			Vector2 l2_end = lineBEnd.ConvertToVec2(mask);

			//Direction of the lines
			Vector2 l1_dir = (l1_end - l1_start).normalized;
			Vector2 l2_dir = (l2_end - l2_start).normalized;

			//If we know the direction we can get the normal vector to each line
			Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
			Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


			//Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
			//The normal vector is the A, B
			float A = l1_normal.x;
			float B = l1_normal.y;

			float C = l2_normal.x;
			float D = l2_normal.y;

			//To get k we just use one point on the line
			float k1 = (A * l1_start.x) + (B * l1_start.y);
			float k2 = (C * l2_start.x) + (D * l2_start.y);


			//Step 2: are the lines parallel? -> no solutions
			if (IsParallel(l1_normal, l2_normal))
			{
				Debug.Log("The lines are parallel so no solutions!");

				return new KeyValuePair<bool, Vector3>(false, Vector3.zero);
			}


			//Step 3: are the lines the same line? -> infinite amount of solutions
			//Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
			if (IsOrthogonal(l1_start - l2_start, l1_normal))
			{
				Debug.Log("Same line so infinite amount of solutions!");

				//Return false anyway
				return new KeyValuePair<bool, Vector3>(false, Vector3.zero);
			}


			//Step 4: calculate the intersection point -> one solution
			float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
			float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

			Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


			//Step 5: but we have line segments so we have to check if the intersection point is within the segment
			if (IsBetween(l1_start, l1_end, intersectPoint) && IsBetween(l2_start, l2_end, intersectPoint))
			{
				Debug.Log("We have an intersection point!");

				isIntersecting = true;



				Vector3 final = LineFromPoint(intersectPoint, lineAStart, (lineAEnd - lineAStart).normalized, mask);

				//Vector3 final = new Vector3(lineAStart.x, intersectPoint.x, intersectPoint.y);

				return new KeyValuePair<bool, Vector3>(isIntersecting, final);
			}

			return new KeyValuePair<bool, Vector3>(isIntersecting, Vector3.zero);

		}



		private static Vector3 LineFromPoint(Vector2 endpoint, Vector3 basePos, Vector3 dir, Vector3 originalMask)
		{
			if (originalMask[0] == 0)
			{
				var target = endpoint.x - basePos.y;
				var length = target / dir.y;

				return basePos + dir * length;
			}
			else
			{
				var target = endpoint.x - basePos.x;
				var length = target / dir.x;

				return basePos + dir * length;
			}

		}




        //Check if the lines are interesecting in 2d space
        public static KeyValuePair<bool, Vector2> IsIntersecting2D(Line lineA, Line lineB)
        {
            return IsIntersecting2D(
                new Vector4(lineA.Start.x, lineA.Start.y, lineA.End.x, lineA.End.y),
                new Vector4(lineB.Start.x, lineB.Start.y, lineB.End.x, lineB.End.y));
        }


        public static KeyValuePair<bool, Vector2> IsIntersecting2D(Vector4 lineAStartEnd, Vector4 lineBStartEnd)
		{
			bool isIntersecting = false;

			//3d -> 2d
			Vector2 l1_start = new Vector2(lineAStartEnd.x, lineAStartEnd.y);
			Vector2 l1_end = new Vector2(lineAStartEnd.z, lineAStartEnd.w);

			Vector2 l2_start = new Vector2(lineBStartEnd.x, lineBStartEnd.y);
			Vector2 l2_end = new Vector2(lineBStartEnd.z, lineBStartEnd.w);

			//Direction of the lines
			Vector2 l1_dir = (l1_end - l1_start).normalized;
			Vector2 l2_dir = (l2_end - l2_start).normalized;

			//If we know the direction we can get the normal vector to each line
			Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
			Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


			//Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
			//The normal vector is the A, B
			float A = l1_normal.x;
			float B = l1_normal.y;

			float C = l2_normal.x;
			float D = l2_normal.y;

			//To get k we just use one point on the line
			float k1 = (A * l1_start.x) + (B * l1_start.y);
			float k2 = (C * l2_start.x) + (D * l2_start.y);


			//Step 2: are the lines parallel? -> no solutions
			if (IsParallel(l1_normal, l2_normal))
			{
				//Debug.Log("The lines are parallel so no solutions!");

				return new KeyValuePair<bool, Vector2>(false, Vector2.zero);
			}


			//Step 3: are the lines the same line? -> infinite amount of solutions
			//Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
			if (IsOrthogonal(l1_start - l2_start, l1_normal))
			{
				Debug.Log("Same line so infinite amount of solutions!");

				//Return false anyway
				return new KeyValuePair<bool, Vector2>(false, Vector2.zero);
			}


			//Step 4: calculate the intersection point -> one solution
			float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
			float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

			Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


			//Step 5: but we have line segments so we have to check if the intersection point is within the segment
			if (IsBetween(l1_start, l1_end, intersectPoint) && IsBetween(l2_start, l2_end, intersectPoint))
			{
				//Debug.Log("We have an intersection point!");

				isIntersecting = true;
				return new KeyValuePair<bool, Vector2>(isIntersecting, intersectPoint);
			}

			return new KeyValuePair<bool, Vector2>(isIntersecting, Vector2.zero);

		}

		//Are 2 vectors parallel?
		private static bool IsParallel(Vector2 v1, Vector2 v2)
		{
			//2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
			if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
			{
				return true;
			}

			return false;
		}

		//Are 2 vectors orthogonal?
		private static bool IsOrthogonal(Vector2 v1, Vector2 v2)
		{
			//2 vectors are orthogonal is the dot product is 0
			//We have to check if close to 0 because of floating numbers
			if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
			{
				return true;
			}

			return false;
		}

		//Is a point c between 2 other points a and b?
		private static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
		{
			bool isBetween = false;

			//Entire line segment
			Vector2 ab = b - a;
			//The intersection and the first point
			Vector2 ac = c - a;

			//Need to check 2 things: 
			//1. If the vectors are pointing in the same direction = if the dot product is positive
			//2. If the length of the vector between the intersection and the first point is smaller than the entire line
			if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
			{
				isBetween = true;
			}

			return isBetween;
		}

        public static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
        {
            Vector2 AP = P - A;       //Vector from A to P   
            Vector2 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.sqrMagnitude;//.LengthSquared();     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;
            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        public struct Line
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2 Dir;

            public Vector2 Center()
            {
               return Vector2.Lerp(Start, End, 0.5f);
            }
        }
    }
}
