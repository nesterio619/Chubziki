using UnityEngine;
using System.IO;

using UnityEditor;

namespace INab.WorldAlchemy
{
    [CustomEditor(typeof(Mask)),CanEditMultipleObjects]
    public class MaskEditor : Editor
    {
		private Mask mask;

		// Serialized properties
		private SerializedProperty type, maskSettings, colliderScale, radiusAdjust, angle, angleAdjust;
		private SerializedProperty startTransform, endTransform, usePreview, onlyEditorPreview;

		private void OnEnable()
		{
			mask = (Mask)target;

			// Finding serialized properties
			type = serializedObject.FindProperty("type");
			maskSettings = serializedObject.FindProperty("maskSettings");
			colliderScale = serializedObject.FindProperty("colliderScale");
            radiusAdjust = serializedObject.FindProperty("radiusAdjust");
            angle = serializedObject.FindProperty("angle");
			angleAdjust = serializedObject.FindProperty("angleAdjust");
			startTransform = serializedObject.FindProperty("startTransform");
			endTransform = serializedObject.FindProperty("endTransform");
			usePreview = serializedObject.FindProperty("usePreview");
			onlyEditorPreview = serializedObject.FindProperty("onlyEditorPreview");
		}

		private MaskSettings GetDefaultMaskSettings(Mask targetMask)
		{
            MaskSettings maskSettings = targetMask.maskSettings;
            if (maskSettings == null)
            {
                MonoScript monoScript = MonoScript.FromScriptableObject(this);
                string path = AssetDatabase.GetAssetPath(monoScript);
                string directory = Path.GetDirectoryName(path);

                maskSettings = AssetDatabase.LoadAssetAtPath<MaskSettings>(directory + "/Mask Settings.asset");
                targetMask.maskSettings = maskSettings;
            }

			return maskSettings;
        }

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var targetMask = (Mask)target;
			GetDefaultMaskSettings(targetMask);


            // Main settings section
            DrawMainSettings();

			Type currentType = (Type)type.enumValueIndex;

			// Drawing specific settings based on the type of the mask
			DrawSpecificSettings(currentType);

			// Auto detection settings
			DrawAutoDetection(currentType);

			// Preview settings
			DrawPreview(currentType);

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawMainSettings()
		{
			EditorGUILayout.LabelField("Main Settings", EditorStyles.boldLabel);
			using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.PropertyField(type);
                

                EditorGUILayout.PropertyField(maskSettings);
				EditorGUILayout.PropertyField(usePreview);
				if (usePreview.boolValue)
				{
					EditorGUILayout.PropertyField(onlyEditorPreview);
				}

				if(mask.SeeThroughMask)
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.IntField("Mask ID", mask.ID);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.HelpBox("This mask is used with the See Through Dissolve, thus it has special collider that detects objects between camera and the mask.", MessageType.Info);
                    
					if (mask.CameraTransform == null)
                    {
                        EditorGUILayout.HelpBox("Camera Transform is missing. Please assign it in the See Rhrough Dissolve script.", MessageType.Warning);
                    }
                }
			}
		}

		private void DrawSpecificSettings(Type currentType)
		{
			EditorGUILayout.Space();
			switch (currentType)
			{
				case Type.SolidAngle:
					DrawSolidAngleSettings();
					break;
				case Type.RoundCone:
					DrawRoundConeSettings();
					break;
			}
		}

		private void DrawSolidAngleSettings()
		{
			EditorGUILayout.LabelField("Solid Angle Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.PropertyField(angle);
			}
			EditorGUILayout.HelpBox("Y-axis scale determines the length of the solid angle cone.", MessageType.Info);
		}

		private void DrawRoundConeSettings()
		{
			EditorGUILayout.LabelField("Round Cone Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.PropertyField(startTransform);
				EditorGUILayout.PropertyField(endTransform);
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Start and End radiuses are defined by the x-axis scale of each transform.", MessageType.Info);

				DrawTransformButtons();
			}
		}

		private void DrawTransformButtons()
		{
			if (mask.startTransform == null || mask.endTransform == null)
			{
				DrawAddTransformsButton();
			}
			else
			{
				DrawDestroyTransformsButton();
			}
		}

		private void DrawAddTransformsButton()
		{
			EditorGUILayout.HelpBox("Transforms are missing.", MessageType.Warning);
			if (GUILayout.Button("Add Transforms"))
			{
				mask.AddRoundConeTransforms();
			}
		}

		private void DrawDestroyTransformsButton()
		{
			if (GUILayout.Button("Destroy Transforms"))
			{
				mask.DestroyRoundConeTransforms();
			}
		}


		private void DrawAutoDetection(Type currentType)
		{
			if (!mask.UseAutoDetection)
			{
				if(mask.HasMaskCollider)
                {
					if (GUILayout.Button("Destroy Collider"))
					{
						mask.DestroyCollider();
					}
				}
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Auto Detection Settings", EditorStyles.boldLabel);
			//if (currentType == Type.RoundCone)
			//{
			//	EditorGUILayout.HelpBox("Round cone does not support auto detection.", MessageType.Error);
			//	return;
			//}

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				DrawColliderManagementButtons();
				EditorGUILayout.PropertyField(colliderScale);
				if (currentType == Type.SolidAngle) EditorGUILayout.PropertyField(angleAdjust);
				if (currentType == Type.RoundCone) EditorGUILayout.PropertyField(radiusAdjust);
            }
		}

		private void DrawColliderManagementButtons()
		{
			if (mask.HasMaskCollider)
			{
				DrawColliderUpdateButtons();
			}
			else
			{
				EditorGUILayout.HelpBox("Collider is required for auto detection.", MessageType.Warning);
				if (GUILayout.Button("Add Collider"))
				{
					mask.UpdateCollider();
				}
			}
		}

		private void DrawColliderUpdateButtons()
		{
			if (GUILayout.Button("Update Collider"))
			{
				mask.UpdateCollider();
			}
			if (GUILayout.Button("Destroy Collider"))
			{
				mask.DestroyCollider();
			}
		}

		private void DrawPreview(Type currentType)
		{
			if (!usePreview.boolValue)
			{
				if(mask.HasMaskPreview)
                {
					if (GUILayout.Button("Destroy Preview"))
					{
						mask.DestroyPreview();
					}
				}
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mask Preview Settings", EditorStyles.boldLabel);
			if (currentType == Type.RoundCone)
			{
				EditorGUILayout.HelpBox("Round cone does not support preview.", MessageType.Error);
				return;
			}

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				DrawPreviewManagementButtons();
				if (currentType == Type.SolidAngle) EditorGUILayout.PropertyField(angleAdjust);
			}
		}

		private void DrawPreviewManagementButtons()
		{
			if (mask.HasMaskPreview)
			{
				if (GUILayout.Button("Update Preview"))
				{
					mask.UpdatePreview();
				}
				if (GUILayout.Button("Destroy Preview"))
				{
					mask.DestroyPreview();
				}
			}
			else
			{
				if (GUILayout.Button("Add Preview"))
				{
					mask.UpdatePreview();
				}
			}
		}

		private void OnSceneGUI()
		{
			foreach (var target in Selection.gameObjects)
			{
				Mask targetMask = target?.GetComponent<Mask>();
				if (targetMask == null) continue;

				DrawMaskGizmos(targetMask);
			}
		}

		private void DrawMaskGizmos(Mask targetMask)
		{
			MaskSettings settings = EnsureMaskSettings(targetMask);
			Transform targetTransform = targetMask.transform;
			Handles.color = settings.Color;

			MaskSettings maskSettings = GetDefaultMaskSettings(targetMask);

            // Drawing gizmos based on mask type
            switch (targetMask.Type)
			{
				case Type.Plane:
					HandlesUtilities.DrawArrow(targetTransform, maskSettings.NormalSize);
					HandlesUtilities.DrawPlane(targetTransform, maskSettings.PlaneSize);
					break;
				case Type.Box:
					HandlesUtilities.DrawBox(targetTransform);
					break;
				case Type.Sphere:
					HandlesUtilities.DrawSphere(targetTransform);
					break;
				case Type.Ellipse:
					HandlesUtilities.DrawEllipse(targetTransform);
					break;
				case Type.SolidAngle:
					HandlesUtilities.DrawSolidAngle(targetTransform, mask.angle);
					break;
				case Type.RoundCone:
					if (targetMask.startTransform != null && targetMask.endTransform != null)
					{
						HandlesUtilities.DrawConeLines(targetMask.startTransform, targetMask.endTransform);
					}
					break;
			}
		}

		private MaskSettings EnsureMaskSettings(Mask targetMask)
		{
			if (targetMask.maskSettings == null)
			{
				MonoScript monoScript = MonoScript.FromScriptableObject(this);
				string path = AssetDatabase.GetAssetPath(monoScript);
				string directory = Path.GetDirectoryName(path);
				return AssetDatabase.LoadAssetAtPath<MaskSettings>(directory + "/Mask Settings.asset");
			}
			return targetMask.maskSettings;
		}

	}
}