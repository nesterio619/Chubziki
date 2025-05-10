using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace INab.WorldAlchemy
{
	public static class HandlesUtilities
	{
		public static void DrawArrow(Transform transform, float size)
		{
			Vector3 position = transform.position;
			Vector3 normalEndPosition = position + transform.up * size;
			Vector3 arrowStartPosition = position + transform.up * size * 0.73f;

			// Draw the line
			Handles.DrawLine(position, normalEndPosition);

			// Draw the arrowhead
			float arrowWide = size * 0.15f;

			// Forward
			Vector3 leftPointForward = arrowStartPosition + transform.forward * -arrowWide;
			Vector3 rightPointForward = arrowStartPosition + transform.forward * arrowWide;

			Handles.DrawLine(arrowStartPosition, leftPointForward);
			Handles.DrawLine(arrowStartPosition, rightPointForward);

			Handles.DrawLine(leftPointForward, normalEndPosition);
			Handles.DrawLine(rightPointForward, normalEndPosition);
		}

		public static void DrawPlane(Transform transform, float size)
		{
			float offset = 0;

			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;

			Vector3 right = rotation * Vector3.right;
			Vector3 up = rotation * Vector3.up;
			Vector3 forward = rotation * Vector3.forward;

			Vector3 p0 = position + transform.up * offset;
			Vector3 p1 = p0 + right * size / 2f;
			Vector3 p2 = p0 - right * size / 2f;
			Vector3 p3 = p0 + forward * size / 2f;
			Vector3 p4 = p0 - forward * size / 2f;

			Handles.DrawPolyLine(p1 + p3 - p0, p2 + p3 - p0, p2 + p4 - p0, p1 + p4 - p0, p1 + p3 - p0, p2 + p3 - p0, p2 + p4 - p0, p1 + p4 - p0);
		}

		public static void DrawBox(Transform transform)
		{
			// Calculate the transformation matrix based on the object's position, rotation, and scale
			Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 1, 1));

			// Set the transformation matrix for the Handles system
			Handles.matrix = cubeTransform;

			// Draw the wire cube using the Handles system
			Handles.DrawWireCube(Vector3.zero, Vector3.Max(transform.lossyScale, Vector3.zero));
		}

		public static void DrawSphere(Transform transform)
		{
			float radius = Mathf.Max(transform.lossyScale.x, 0);

			Vector3 spherePosition = transform.position;
			Quaternion sphereRotation = transform.rotation;

			Handles.DrawWireDisc(spherePosition, sphereRotation * Vector3.forward, radius);
			Handles.DrawWireDisc(spherePosition, sphereRotation * Vector3.up, radius);
			Handles.DrawWireDisc(spherePosition, sphereRotation * Vector3.right, radius);

			Matrix4x4 handlesMatrix = Handles.matrix;
			Handles.matrix = Matrix4x4.TRS(spherePosition, sphereRotation, Vector3.one);

			Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
			Handles.DrawWireDisc(Vector3.zero, Vector3.right, radius);
			Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);

			Handles.matrix = handlesMatrix;

		}

		public static void DrawEllipse(Transform transform)
		{
			int numSegments = 64;

			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;

			Vector3 right = rotation * Vector3.right * Mathf.Max(transform.lossyScale.x, 0); ;
			Vector3 up = rotation * Vector3.up * Mathf.Max(transform.lossyScale.y, 0); ;
			Vector3 forward = rotation * Vector3.forward * Mathf.Max(transform.lossyScale.z, 0); ;

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < numSegments; j++)
				{
					float angle1 = Mathf.Lerp(0, 2 * Mathf.PI, (float)j / numSegments);
					float angle2 = Mathf.Lerp(0, 2 * Mathf.PI, (float)(j + 1) / numSegments);

					Vector3 axis1 = Vector3.zero;
					Vector3 axis2 = Vector3.zero;

					switch (i)
					{
						case 0:
							axis1 = right * Mathf.Cos(angle1) + up * Mathf.Sin(angle1);
							axis2 = right * Mathf.Cos(angle2) + up * Mathf.Sin(angle2);
							break;
						case 1:
							axis1 = right * Mathf.Cos(angle1) + forward * Mathf.Sin(angle1);
							axis2 = right * Mathf.Cos(angle2) + forward * Mathf.Sin(angle2);
							break;
						case 2:
							axis1 = up * Mathf.Cos(angle1) + forward * Mathf.Sin(angle1);
							axis2 = up * Mathf.Cos(angle2) + forward * Mathf.Sin(angle2);
							break;
					}

					Handles.DrawLine(position + axis1, position + axis2);
				}
			}
		}

		public static void DrawSolidAngle(Transform transform, float angle)
		{
			angle /= 2;

			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;

			Vector3 right = rotation * Vector3.right;
			Vector3 up = rotation * Vector3.up;
			Vector3 forward = rotation * Vector3.forward;

			float length = Mathf.Max(transform.lossyScale.y, 0); // Mathf.Abs
			float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * length;

			Vector3 endPoint = position + up * length;
			Vector3 rightPoint = endPoint + right * radius;
			Vector3 leftPoint = endPoint - right * radius;
			Vector3 topPoint = endPoint + forward * radius;
			Vector3 bottomPoint = endPoint - forward * radius;

			Handles.DrawLine(position, endPoint);
			Handles.DrawLine(endPoint, rightPoint);
			Handles.DrawLine(endPoint, leftPoint);
			Handles.DrawLine(endPoint, topPoint);
			Handles.DrawLine(endPoint, bottomPoint);

			Handles.DrawLine(position, rightPoint);
			Handles.DrawLine(position, leftPoint);
			Handles.DrawLine(position, topPoint);
			Handles.DrawLine(position, bottomPoint);

			Handles.DrawWireDisc(endPoint, up, radius);
		}

		public static void DrawConeHandles(Transform startTransform, Transform endTransform)
		{
			EditorGUI.BeginChangeCheck();
			float topRadiusHandle = Handles.RadiusHandle(Quaternion.identity, startTransform.position, startTransform.lossyScale.x);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(startTransform, "Change Cone Top Radius");
				startTransform.localScale = new Vector3(topRadiusHandle, topRadiusHandle, topRadiusHandle);
			}

			EditorGUI.BeginChangeCheck();
			float bottomRadiusHandle = Handles.RadiusHandle(Quaternion.identity, endTransform.position, endTransform.lossyScale.x);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(endTransform, "Change Cone Bottom Radius");
				endTransform.localScale = new Vector3(bottomRadiusHandle, bottomRadiusHandle, bottomRadiusHandle);
			}

			// Draw lines between the top and bottom circles of the cone
			Handles.DrawLine(startTransform.position + startTransform.right * startTransform.lossyScale.x, endTransform.position + endTransform.right * endTransform.lossyScale.x);
			Handles.DrawLine(startTransform.position - startTransform.right * startTransform.lossyScale.x, endTransform.position - endTransform.right * endTransform.lossyScale.x);
			Handles.DrawLine(startTransform.position + startTransform.forward * startTransform.lossyScale.x, endTransform.position + endTransform.forward * endTransform.lossyScale.x);
			Handles.DrawLine(startTransform.position - startTransform.forward * startTransform.lossyScale.x, endTransform.position - endTransform.forward * endTransform.lossyScale.x);
		}

		public static void DrawConeLines(Transform startTransform, Transform endTransform)
		{
			// Draw lines between the top and bottom circles of the cone
			Handles.DrawLine(startTransform.position + startTransform.right * startTransform.lossyScale.x, endTransform.position + endTransform.right * endTransform.lossyScale.x);
			Handles.DrawLine(startTransform.position - startTransform.right * startTransform.lossyScale.x, endTransform.position - endTransform.right * endTransform.lossyScale.x);

			Handles.DrawLine(startTransform.position + startTransform.up * startTransform.lossyScale.x, endTransform.position + endTransform.up * endTransform.lossyScale.x);
			Handles.DrawLine(startTransform.position - startTransform.up * startTransform.lossyScale.x, endTransform.position - endTransform.up * endTransform.lossyScale.x);

			DrawSphere(startTransform);
			DrawSphere(endTransform);

			/*
			// Draw half-spheres at the start and end points
			DrawHalfSphere(startTransform.position, startTransform.rotation, startTransform.lossyScale.x,false);
			DrawHalfSphere(endTransform.position, endTransform.rotation, endTransform.lossyScale.x,true);

			if(independentStartEnd)
			{
				DrawHalfSphere(startTransform.position, startTransform.rotation, startTransform.lossyScale.x, true);
				DrawHalfSphere(endTransform.position, endTransform.rotation, endTransform.lossyScale.x, false);
			}*/
		}

		// Not used for now
		public static void DrawHalfSphere(Vector3 position, Quaternion rotation, float radius, bool invert)
		{
			Matrix4x4 handlesMatrix = Handles.matrix;
			Handles.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

			float upAngle = invert ? 180 : -180;
			float forwardAngle = invert ? -180 : 180;
			float rightAngle = 360;

			Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward * radius, upAngle, radius);
			Handles.DrawWireArc(Vector3.zero, Vector3.forward, Vector3.up * radius, forwardAngle, radius);
			Handles.DrawWireArc(Vector3.zero, Vector3.right, Vector3.forward * radius, rightAngle, radius);

			Handles.matrix = handlesMatrix;
		}

	}
}