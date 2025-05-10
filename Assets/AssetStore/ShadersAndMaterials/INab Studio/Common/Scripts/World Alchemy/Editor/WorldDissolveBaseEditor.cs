using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace INab.WorldAlchemy
{
	public abstract class WorldDissolveBaseEditor : Editor
	{
        public SerializedProperty activeMasks;
        public SerializedProperty type;
        public SerializedProperty materialsList;
        
        public SerializedProperty useAutoDetection;
        public SerializedProperty detectionLayerMask;
        public SerializedProperty copyEditorLists;
        public SerializedProperty controlMaterialsProperties;
        public SerializedProperty dissolveType;
        
        public SerializedProperty invert;
        public SerializedProperty guideTexture;
        public SerializedProperty guideTiling;
        public SerializedProperty guideStrength;
        public SerializedProperty useBackColor;
        public SerializedProperty backColor;
        
        public SerializedProperty burnHardness;
        public SerializedProperty burnOffset;
        public SerializedProperty emberOffset;
        public SerializedProperty emberSmoothness;
        public SerializedProperty emberWidth;
        public SerializedProperty emberColor;
        public SerializedProperty burnColor;
        
        public SerializedProperty useDithering;
        public SerializedProperty edgeColor;
        public SerializedProperty edgeWidth;
        public SerializedProperty edgeSmoothness;
        public SerializedProperty affectAlbedo;
        public SerializedProperty glareColor;
        public SerializedProperty glareGuideStrength;
        public SerializedProperty glareWidth;
        public SerializedProperty glareSmoothness;
        public SerializedProperty glareOffset;
        
        
        public SerializedProperty useDisplacement;
        public SerializedProperty displacementPerVertex;
        public SerializedProperty displacementSmoothness;
        public SerializedProperty displacementOffset;
        public SerializedProperty rotationAxis;
        public SerializedProperty rotationMin;
        public SerializedProperty rotationMax;
        public SerializedProperty randomPositionOffset;
        public SerializedProperty positionOffset;
        public SerializedProperty scale;
        public SerializedProperty normalOffset;
        
        public SerializedProperty displacementColor;


        protected WorldDissolveBase worldDissolve;

        // Flags to control delayed updates
        public bool waitForType = false;
        public bool waitForActiveMasks = false;
        public bool waitForUseDisplacement = false;

        public virtual void OnEnable()
        {
            activeMasks = serializedObject.FindProperty("activeMasks");
            type = serializedObject.FindProperty("type");
            materialsList = serializedObject.FindProperty("materialsList");

            useAutoDetection = serializedObject.FindProperty("useAutoDetection");
            detectionLayerMask = serializedObject.FindProperty("detectionLayerMask");
            copyEditorLists = serializedObject.FindProperty("copyEditorLists");
            controlMaterialsProperties = serializedObject.FindProperty("controlMaterialsProperties");
            dissolveType = serializedObject.FindProperty("dissolveType");

            invert = serializedObject.FindProperty("invert");
            backColor = serializedObject.FindProperty("backColor");
            useBackColor = serializedObject.FindProperty("useBackColor");

            guideTexture = serializedObject.FindProperty("guideTexture");
            guideTiling = serializedObject.FindProperty("guideTiling");
            guideStrength = serializedObject.FindProperty("guideStrength");
            burnHardness = serializedObject.FindProperty("burnHardness");
            burnOffset = serializedObject.FindProperty("burnOffset");
            emberOffset = serializedObject.FindProperty("emberOffset");
            emberSmoothness = serializedObject.FindProperty("emberSmoothness");
            emberWidth = serializedObject.FindProperty("emberWidth");
            emberColor = serializedObject.FindProperty("emberColor");
            burnColor = serializedObject.FindProperty("burnColor");

            useDithering = serializedObject.FindProperty("useDithering");
            edgeColor = serializedObject.FindProperty("edgeColor");
            edgeWidth = serializedObject.FindProperty("edgeWidth");
            edgeSmoothness = serializedObject.FindProperty("edgeSmoothness");
            affectAlbedo = serializedObject.FindProperty("affectAlbedo");
            glareColor = serializedObject.FindProperty("glareColor");
            glareGuideStrength = serializedObject.FindProperty("glareGuideStrength");
            glareWidth = serializedObject.FindProperty("glareWidth");
            glareSmoothness = serializedObject.FindProperty("glareSmoothness");
            glareOffset = serializedObject.FindProperty("glareOffset");


            useDisplacement = serializedObject.FindProperty("useDisplacement");
            displacementPerVertex = serializedObject.FindProperty("displacementPerVertex");
            displacementSmoothness = serializedObject.FindProperty("displacementSmoothness");
            displacementColor = serializedObject.FindProperty("displacementColor"); 
             displacementOffset = serializedObject.FindProperty("displacementOffset");

            rotationAxis = serializedObject.FindProperty("rotationAxis");
            rotationMin = serializedObject.FindProperty("rotationMin");
            rotationMax = serializedObject.FindProperty("rotationMax");
            randomPositionOffset = serializedObject.FindProperty("randomPositionOffset");
            positionOffset = serializedObject.FindProperty("positionOffset");
            scale = serializedObject.FindProperty("scale");
            normalOffset = serializedObject.FindProperty("normalOffset");

        }

        public override void OnInspectorGUI()
        {
            worldDissolve = (WorldDissolveBase)target;
            serializedObject.Update();

            // Custom inspector implementation
            Inspector();

            serializedObject.ApplyModifiedProperties();

            // Update materials based on property changes
            UpdateKeywords();
        }

        public abstract void Inspector();

        public void DrawMaterialsList()
        {
            EditorGUILayout.PropertyField(materialsList, true);

            // Test materialsDictionary draw
            //foreach (var kvp in worldDissolve.materialsDictionary)
            //{
            //    EditorGUILayout.LabelField(kvp.Key.name, kvp.Value.ToString());
            //}
        }

        public virtual void DrawSettings(bool globalDraw)
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (globalDraw)
                {
                    EditorGUILayout.PropertyField(controlMaterialsProperties);
                }
                else
                {
                    if(copyEditorLists.boolValue == false) EditorGUILayout.PropertyField(useAutoDetection);
                    if (useAutoDetection.boolValue)
                    {
                        EditorGUILayout.PropertyField(detectionLayerMask);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(copyEditorLists);
                    }
                    if (useAutoDetection.boolValue == false && copyEditorLists.boolValue == false) EditorGUILayout.HelpBox("Make sure you are adding materials via code in runtime.", MessageType.Warning);

                    EditorGUILayout.PropertyField(controlMaterialsProperties);
                }

                if (controlMaterialsProperties.boolValue)
                {

                    string shaderTypeName = "";
                    if (globalDraw)
                    {
                        shaderTypeName = "Global";
                    }
                    else
                    {
                        shaderTypeName = "World";
                    }

                    string alphaThresholdMessage = " Alpha Clipping Threshold must be set to a very small value, like 0.0001.";
                    string dissolveTypeName = "";
                    switch (dissolveType.intValue)
                    {
                        case 0:
                            dissolveTypeName = "Burn";
                            //alphaThresholdMessage = "";
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
            }

            EditorGUILayout.Space();
        }

        public virtual void DrawKeywords(int activeMasksMax)
        {
            EditorGUILayout.LabelField("Keywords", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                activeMasks.intValue = EditorGUILayout.IntSlider("Active Masks", activeMasks.intValue, 0, activeMasksMax);
                EditorGUILayout.PropertyField(type);
                EditorGUILayout.PropertyField(useDisplacement);
                if(dissolveType.intValue == (int)DissolveType.DisplacementOnly && useDisplacement.boolValue == false)
                {
                    EditorGUILayout.HelpBox("You need to turn on useDisplacement when using Displacement Only dissolve type.", MessageType.Warning);
                }
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

        public virtual void UpdateKeywords()
        {
            if(waitForType)
            {
                worldDissolve.ChangeType(worldDissolve.Type);
                waitForType = false;
            }

            if (waitForActiveMasks)
            {
                worldDissolve.ChangeActiveMasks(worldDissolve.ActiveMasks);
                waitForActiveMasks = false;
            }

            if (waitForUseDisplacement)
            {
                worldDissolve.ChangeUseDisplacement(worldDissolve.UseDisplacement);
                waitForUseDisplacement = false;
            }
        }

        public void DrawEdgeTypeProperties()
        {
            EditorGUILayout.LabelField("Edge Properties", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(invert);
                EditorGUILayout.PropertyField(dissolveType);

                EditorGUILayout.PropertyField(guideTexture);
                EditorGUILayout.PropertyField(guideTiling);
                EditorGUILayout.PropertyField(guideStrength);

                EditorGUILayout.PropertyField(burnHardness);
                EditorGUILayout.PropertyField(burnOffset);
                EditorGUILayout.PropertyField(burnColor);
                EditorGUILayout.PropertyField(emberOffset);
                EditorGUILayout.PropertyField(emberSmoothness);
                EditorGUILayout.PropertyField(emberWidth);
                EditorGUILayout.PropertyField(emberColor);
                EditorGUILayout.PropertyField(useBackColor);
                if(useBackColor.boolValue) EditorGUILayout.PropertyField(backColor);
            }
            EditorGUILayout.Space();
        }

        public void DrawSmoothTypeProperties()
        {
            EditorGUILayout.LabelField("Edge Properties", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(invert);
                EditorGUILayout.PropertyField(dissolveType);

                EditorGUILayout.PropertyField(guideTexture);
                EditorGUILayout.PropertyField(guideTiling);
                EditorGUILayout.PropertyField(guideStrength);

                EditorGUILayout.PropertyField(useDithering); 
                EditorGUILayout.PropertyField(edgeColor); 
                EditorGUILayout.PropertyField(edgeWidth);
                EditorGUILayout.PropertyField(edgeSmoothness);
                EditorGUILayout.PropertyField(affectAlbedo);
                EditorGUILayout.PropertyField(glareColor);
                EditorGUILayout.PropertyField(glareGuideStrength);
                EditorGUILayout.PropertyField(glareWidth);
                EditorGUILayout.PropertyField(glareSmoothness);
                EditorGUILayout.PropertyField(glareOffset);

                EditorGUILayout.PropertyField(useBackColor);
                if (useBackColor.boolValue) EditorGUILayout.PropertyField(backColor);
            }
            EditorGUILayout.Space();
        }

        public void DrawDisplacementOnlyTypeProperties()
        {
            EditorGUILayout.LabelField("Displacement Only Properties", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(invert);
                EditorGUILayout.PropertyField(displacementColor);
                EditorGUILayout.PropertyField(dissolveType);
            }
            EditorGUILayout.Space();
        }

        public void DrawDisplacementProperties()
        {
            EditorGUILayout.LabelField("Displacement Properties", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(displacementPerVertex);
                EditorGUILayout.PropertyField(displacementSmoothness);
                EditorGUILayout.PropertyField(displacementOffset);
                EditorGUILayout.PropertyField(rotationAxis);
                EditorGUILayout.PropertyField(rotationMin);
                EditorGUILayout.PropertyField(rotationMax);
                EditorGUILayout.PropertyField(randomPositionOffset);
                EditorGUILayout.PropertyField(positionOffset);
                EditorGUILayout.PropertyField(scale);
                EditorGUILayout.PropertyField(normalOffset);
            }
            EditorGUILayout.Space();
        }

    }
}