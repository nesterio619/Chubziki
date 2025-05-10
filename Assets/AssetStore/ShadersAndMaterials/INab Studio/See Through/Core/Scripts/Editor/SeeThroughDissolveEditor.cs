using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace INab.WorldAlchemy
{
    [CustomEditor(typeof(SeeThroughDissolve)), CanEditMultipleObjects]
    public class SeeThroughDissolveEditor : WorldDissolveBaseEditor
    {
        private SerializedProperty masksList;
        private SerializedProperty cameraTransform;
        private SerializedProperty useScreenStableRadius;
        private SerializedProperty screenRadiusMultiplier;
        private SerializedProperty useDepthOffset;
        private SerializedProperty depthOffset; 
        //private SerializedProperty stabilizeDistanceLook; 
        private SerializedProperty offsetSmoothness;
            
        private bool waitUseStableScreenRadius = false;

        SeeThroughDissolve targetSD;

        public override void OnEnable()
        {
            base.OnEnable();

            // Additional properties
            masksList = serializedObject.FindProperty("masksList");
            cameraTransform = serializedObject.FindProperty("cameraTransform");
            useScreenStableRadius = serializedObject.FindProperty("useScreenStableRadius");
            screenRadiusMultiplier = serializedObject.FindProperty("screenRadiusMultiplier");
            useDepthOffset = serializedObject.FindProperty("useDepthOffset");
            depthOffset = serializedObject.FindProperty("depthOffset");
            //stabilizeDistanceLook = serializedObject.FindProperty("stabilizeDistanceLook");
            offsetSmoothness = serializedObject.FindProperty("offsetSmoothness");
        }

        public override void Inspector()
        {
            targetSD = ((SeeThroughDissolve)worldDissolve);

            DrawGeneral();
            DrawKeywords(SeeThroughDissolve.MaxMasksSeeThrough);


            if (targetSD.UseScreenStableRadius != useScreenStableRadius.boolValue)
            {
                waitUseStableScreenRadius = true;
            }

            DrawSettings(false);


            if (worldDissolve.ControlMaterialsProperties)
            {
                switch (worldDissolve.DissolveType)
                {
                    case DissolveType.Burn:
                        DrawEdgeTypeProperties();
                        break;
                    case DissolveType.Smooth:
                        DrawSmoothTypeProperties();
                        break;
                    case DissolveType.DisplacementOnly:
                        EditorGUILayout.HelpBox("Displacement is not supported with see through.", MessageType.Error);
                        //DrawDisplacementOnlyTypeProperties();
                        break;
                }

                if (worldDissolve.UseDisplacement)
                {
                    DrawDisplacementProperties();
                }
            }

            //EditorUtilities.DrawLinks(EditorUtilities.WorldDissolveLink, EditorUtilities.WorldDissolveDocs);

        }

        public override void DrawKeywords(int activeMasksMax)
        {
            EditorGUILayout.LabelField("Keywords", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                activeMasks.intValue = EditorGUILayout.IntSlider("Active Masks", activeMasks.intValue, 0, activeMasksMax);
            }

            if (((int)worldDissolve.Type) != type.enumValueIndex)
            {
                waitForType = true;
            }

            if (worldDissolve.ActiveMasks != activeMasks.intValue)
            {
                waitForActiveMasks = true;
            }

            if (worldDissolve.UseDisplacement != useDisplacement.boolValue)
            {
                waitForUseDisplacement = true;
            }

            EditorGUILayout.Space();
        }

        public override void DrawSettings(bool globalDraw)
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                    EditorGUILayout.PropertyField(detectionLayerMask);
                    EditorGUILayout.PropertyField(controlMaterialsProperties);

                string shaderTypeName = "See Through";

                string alphaThresholdMessage = " Alpha Clipping Threshold must be set to a very small value, like 0.0001.";
                string dissolveTypeName = "";
                switch (dissolveType.intValue)
                {
                    case 0:
                        dissolveTypeName = "Burn";
                        break;
                    case 1:
                        dissolveTypeName = "Smooth";
                        if (!useDithering.boolValue)
                        {
                            EditorGUILayout.HelpBox("If useDithering is off, material surface type must be Transparent.", MessageType.Info);
                        }
                        break;
                    case 2:
                        dissolveTypeName = "Displacement Only";
                        break;
                }

                EditorGUILayout.HelpBox("All materials need to use " + shaderTypeName + " Dissolve " + dissolveTypeName + " shader.", MessageType.Info);
                EditorGUILayout.HelpBox("Alpha Clipping needs to be on." + alphaThresholdMessage, MessageType.Info);
            }

            EditorGUILayout.Space();
        }

        private void DrawGeneral()
        {
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Refresh"))
                {
                    worldDissolve.RefreshWorldDissolve();
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(cameraTransform);
                EditorGUILayout.HelpBox("See through works properly only in play mode.", MessageType.Info);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(useScreenStableRadius);
                if (useScreenStableRadius.boolValue)
                {
                    EditorGUILayout.PropertyField(screenRadiusMultiplier);
                EditorGUILayout.Space();
                }
                EditorGUILayout.PropertyField(useDepthOffset);
                if (useDepthOffset.boolValue)
                {
                    EditorGUILayout.PropertyField(depthOffset);
                    EditorGUILayout.PropertyField(offsetSmoothness);
                }
                EditorGUILayout.Space(); 
                    //EditorGUILayout.PropertyField(stabilizeDistanceLook);
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(masksList);
                EditorGUI.indentLevel--;


                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                DrawMaterialsList();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

            }

            EditorGUILayout.Space();

        }

        public override void UpdateKeywords()
        {
            base.UpdateKeywords();

            if (waitUseStableScreenRadius)
            {
                var targetWD = ((SeeThroughDissolve)worldDissolve);
                targetWD.ChangeUseScreenStableRadius(targetWD.UseScreenStableRadius);
                waitUseStableScreenRadius = false;
            }

        }

    }
}