// TerrainPaintingProfileEditor.cs
// Custom editor for TerrainPaintingProfile
// TerraForge 2.0.0

using System;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    [CustomEditor(typeof(TerraForgeTerrainPainter))]
    public class TerraForgeTerraForgeTerrainPainterEditor : UnityEditor.Editor
    {
        // Script and Banner
        TerraForgeTerrainPainter script;
        private Texture2D banner;
        private TerrainData currentTerrainData;

        // Textures
        private Texture2D outlineTexture_0;
        private Texture2D outlineTexture_1;
        private Texture2D outlineTexture_2;
        private Texture2D outlineTexture_3;

        // Serialized Properties
        private SerializedProperty layerSettings;
        private SerializedProperty autoRepaint;
        private SerializedProperty resolution;
        private SerializedProperty colorMapResolution;
        private SerializedProperty terrains;

        // Reorderable Lists
        private Dictionary<TerraForgeTerrainPainterLayerSettings, ReorderableList> m_modifierList = new Dictionary<TerraForgeTerrainPainterLayerSettings, ReorderableList>();
        private ReorderableList curModList;
        ReorderableList m_LayerList;

        // State and Control Variables
        private bool hasMissingTerrains;
        private bool requiresRepaint;
        private bool editLayerSettings;
        private int selectedModifierIndex;
        private int m_layerPickerWindowID = -1;
        private int selectedLayerID
        {
            get { return SessionState.GetInt("PTP_SELECTED_LAYER", -1); }
            set { SessionState.SetInt("PTP_SELECTED_LAYER", value); }
        }

        // Editor and Animation
        private UnityEditor.Editor layerEditor;
        private AnimBool editLayerSettingsAnim;

        // Render Textures
        private RenderTexture[] heatmaps;

        // UI Variables
        private TerrainLayer m_PickedLayer;
        private Texture2D m_PickedTexture;
        private Texture2D m_layerTexture;

        // Constants
        private const int kElementHeight = 40;
        private const int kElementObjectFieldHeight = 16;
        private const int kElementPadding = 2;
        private const int kElementObjectFieldWidth = 140;
        private const int kElementToggleWidth = 20;
        private const int kElementThumbSize = 40;

        // Icon Prefix
        private string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";
        
        private void OnEnable()
        {
            // Cast the target to TerraForgeTerrainPainter
            script = (TerraForgeTerrainPainter)target;
            
            // Load a banner texture from the specified path
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeTerrainPainterEditor_Banner.png");
            
            // If terrains are assigned to the script, recalculate their bounds
            if (script.terrains != null)
                script.RecalculateBounds();
            
            // Find serialized properties based on their names
            terrains = serializedObject.FindProperty("terrains");
            autoRepaint = serializedObject.FindProperty("autoRepaint");
            layerSettings = serializedObject.FindProperty("layerSettings");
            resolution = serializedObject.FindProperty("splatmapResolution");
            colorMapResolution = serializedObject.FindProperty("colorMapResolution");
            
            // Refresh modifiers in the TerraForgeTerrainPainterModifierEditor
            TerraForgeTerrainPainterModifierEditor.RefreshModifiers();
            
            // Create outline textures with specific colors and dimensions
            outlineTexture_0 = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
            outlineTexture_1 = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            outlineTexture_3 = CreateOutlineTexture(10, 10, new Color(0.25f, 0.25f, 0.25f), new Color(0.18f, 0.18f, 0.18f));
            
            // Refresh the layer list and modifier lists in the editor
            RefreshLayerList();
            RefreshModifierLists();
            
            // Initialize an animation boolean for editing layer settings
            editLayerSettingsAnim = new AnimBool(editLayerSettings);
            editLayerSettingsAnim.valueChanged.AddListener(this.Repaint);
            editLayerSettingsAnim.speed = 4f;
            
            // Check for missing terrains in the script's terrains array
            if (script.terrains != null)
                hasMissingTerrains = TerraForgeTerrainPainterUtilities.HasMissingTerrain(script.terrains);
            
            if (script.layerSettings != null && selectedLayerID > 0)
            {
                if (script.layerSettings.ElementAtOrDefault(selectedLayerID).modifierStack.Count() != 0)
                {
                    if (terrains.arraySize > 0)
                    {
                        var terrainProperty = terrains.GetArrayElementAtIndex(0);
                        var terrainComponent = terrainProperty.objectReferenceValue as Terrain;
                        if (script.layerSettings.ElementAtOrDefault(selectedLayerID).modifierStack[0].terrainData != terrainComponent.terrainData)
                        {
                            if (terrainComponent != null)
                            {
                                UpdateTerrainData(terrainComponent.terrainData);
                            }
                        }
                        
                    }
                }
            }
        }

        private void RefreshLayerList()
        {
            // Reset the layer list to null
            m_LayerList = null;
            
            // If the layer list is null, create a new ReorderableList for layer settings
            if (m_LayerList == null)
            {
                // Initialize a new ReorderableList for layer settings
                m_LayerList = new ReorderableList(script.layerSettings, typeof(TerraForgeTerrainPainterLayerSettings), true,
                    false, false, false);
                
                // Set element height for each element in the list
                m_LayerList.elementHeight = kElementHeight;
                
                // Set callback methods for drawing elements, selecting elements, drawing backgrounds, and reordering elements
                m_LayerList.drawElementCallback = DrawLayerElement;
                m_LayerList.onSelectCallback = OnSelectLayerElement;
                m_LayerList.drawElementBackgroundCallback = DrawLayerBackground;
                m_LayerList.onReorderCallbackWithDetails = OnReorderLayerElement;
                
                // Set header and footer heights to 0 to remove default spacing
                m_LayerList.headerHeight = 0f;
                m_LayerList.footerHeight = 0f;
                
                // Set the selected index of the layer list
                m_LayerList.index = selectedLayerID;
            }
            
            // Disable default background for the layer list
            m_LayerList.showDefaultBackground = false;
        }


        private void RefreshModifierLists()
        {
            m_modifierList.Clear();
            foreach (TerraForgeTerrainPainterLayerSettings s in script.layerSettings)
            {
                ReorderableList layerModifiers = new ReorderableList(s.modifierStack, typeof(TerraForgeTerrainPainterModifier));
                layerModifiers.draggable = true;
                layerModifiers.elementHeight = 25;
                layerModifiers.drawHeaderCallback = DrawModifierHeader;
                layerModifiers.displayAdd = true;
                layerModifiers.displayRemove = true;
                layerModifiers.drawElementCallback = DrawModifierElement;
                layerModifiers.onSelectCallback = OnSelectModifier;
                layerModifiers.drawElementBackgroundCallback = DrawModifierBackground;
                layerModifiers.onRemoveCallback = OnRemoveModifier;
                layerModifiers.onReorderCallbackWithDetails = OnReorderModifier;
                layerModifiers.onAddDropdownCallback = OnAddModifierDropDown;
                m_modifierList.Add(s, layerModifiers);
            }
        }
        
        public override void OnInspectorGUI()
        {
            requiresRepaint = false;

            if (banner != null)
            {
                float aspectRatio = (float)banner.width / banner.height;
                float bannerWidth = EditorGUIUtility.currentViewWidth - 20;
                float bannerHeight = bannerWidth / aspectRatio;

                GUILayout.Label(banner, GUILayout.Width(bannerWidth), GUILayout.Height(bannerHeight));
            }
            else
            {
                EditorGUILayout.HelpBox("Banner image not found!", MessageType.Warning);
            }
            
            if(hasMissingTerrains) EditorGUILayout.HelpBox("One or more terrains are missing", MessageType.Error);
            
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();

            DrawLayers();
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (requiresRepaint)
            {
                script.RepaintAll();
            }

            DrawSettings();
        }

        private void DrawSettings()
        {
            if (terrains.arraySize == 0 || hasMissingTerrains)
            {
                terrains.isExpanded = true;
            }
            GUILayout.Space(15);
            GUILayout.BeginVertical(GetOutlineStyle());

            GUILayout.BeginVertical(GetOutlineStyle_D_O());

            using (new EditorGUILayout.VerticalScope("Box"))
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.PropertyField(terrains, new GUIContent("Terrains (" + terrains.arraySize + ")"));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Automatic Assignment of Terrains"))
                {
                    if (terrains != null)
                    {
                        if(terrains.arraySize > 0) requiresRepaint = true;
                    
                        script.AssignActiveTerrains();
                        //script.SetTargetTerrains(Terrain.activeTerrains);

                        hasMissingTerrains = false;

                        if (terrains.arraySize > 0)
                        {
                            var terrainProperty = terrains.GetArrayElementAtIndex(0);
                            var terrainComponent = terrainProperty.objectReferenceValue as Terrain;
                            if (terrainComponent != null)
                            {
                                UpdateTerrainData(terrainComponent.terrainData);
                            }
                        }
                    }

                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("Refresh Modifiers"))
                {
                    if (terrains != null)
                    {
                        if (terrains.arraySize > 0)
                        {
                            var terrainProperty = terrains.GetArrayElementAtIndex(0);
                            var terrainComponent = terrainProperty.objectReferenceValue as Terrain;
                            if (terrainComponent != null)
                            {
                                UpdateTerrainData(terrainComponent.terrainData);
                            }
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(5);
            GUILayout.BeginVertical(GetOutlineStyle_D_O());
            
            if (terrains.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Assign terrains to paint on first", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.PropertyField(resolution);
                EditorGUILayout.PropertyField(colorMapResolution);
                if (EditorGUI.EndChangeCheck())
                {
                    requiresRepaint = true;
                }

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Recalculate bounds", "If the terrain size has changed, the bounds must be recalculated. The white box must encapsulate all terrains")))
                    {
                        script.RecalculateBounds();
                        requiresRepaint = true;
                        EditorUtility.SetDirty(target);
                    }
            }
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void DrawLayers()
        {
            GUILayout.BeginVertical(GetOutlineStyle());
            GUILayout.Space(5);
            GUILayout.BeginVertical(GetOutlineStyle_D_O());
            EditorGUILayout.LabelField("Terrain Layers", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Directly layout the list without scroll view
            m_LayerList.DoLayoutList();
            GUILayout.Space(5);
            // Control buttons
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginDisabledGroup(m_LayerList.index < 0 || m_LayerList.count == 0);
                {
                    editLayerSettings = GUILayout.Toggle(editLayerSettings, new GUIContent("  Edit layer", EditorGUIUtility.IconContent(iconPrefix + "editicon.sml").image), EditorStyles.toolbarButton, GUILayout.MaxWidth(90f));
                }
                EditorGUI.EndDisabledGroup();

                
                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(layerSettings.arraySize >= 32); //Maximum realistic number of terrain layers
                var newIcon = EditorGUIUtility.IconContent(iconPrefix + "DefaultAsset Icon").image;
                
                if (GUILayout.Button(new GUIContent("",
                    EditorGUIUtility.IconContent(iconPrefix + "Toolbar Plus More")
                        .image, "Add terrain layer from project"), EditorStyles.toolbarButton))
                {
                    m_layerPickerWindowID = EditorGUIUtility.GetControlID(FocusType.Passive) + 200; 
                    EditorGUIUtility.ShowObjectPicker<TerrainLayer>(null, false, "", m_layerPickerWindowID);
                }

                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(m_LayerList.index < 0 || m_LayerList.count == 0);
                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image,
                    "Remove selected layer"), EditorStyles.toolbarButton))
                {
                    if (!EditorUtility.DisplayDialog("Terrain Painter",
                        "Removing a layer cannot be undone, settings will be lost",
                        "Ok","Cancel")) return;
                    
                    RemoveLayerElement(m_LayerList.index);
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();
            
            if (script.layerSettings.ElementAtOrDefault(selectedLayerID) != null)
            {
                editLayerSettingsAnim.target = editLayerSettings;
                //TODO: Fix error about mismatching layout
                if (EditorGUILayout.BeginFadeGroup(editLayerSettingsAnim.faded))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Layer settings", EditorStyles.boldLabel);
                    UnityEditor.Editor.CreateCachedEditor(script.layerSettings.ElementAtOrDefault(selectedLayerID).layer, typeof(TerrainLayerInspector), ref layerEditor);
                    layerEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndFadeGroup();
            }

            if (m_LayerList.count == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("All existing terrain layers and painting will be cleared when adding the first layer!", MessageType.Warning);
            }
            
            ObjectPickerActions();
            
            EditorGUILayout.Space();

            GUILayout.BeginVertical(GetOutlineStyle_D_O());
            EditorGUILayout.Space(3);
            DrawLayerModifierStack();
            GUILayout.EndVertical();
            
            EditorGUILayout.Space();

            GUILayout.BeginVertical(GetOutlineStyle_D_O());

            EditorGUI.BeginChangeCheck();
                
            EditorGUILayout.PropertyField(autoRepaint);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                script.SetAutoRepaint(autoRepaint.boolValue);
            }   
            
            if(GUILayout.Button(new GUIContent(" Apply Texture Layers ", "Trigger a complete repaint operation. Typically needed if the terrain was modified in some way, yet not changes are made in the Terrain Painter component")))
            {
                requiresRepaint = true;
            }
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }
        
        private void ObjectPickerActions()
        {
            // Add existing layer
            if (Event.current.commandName == "ObjectSelectorClosed" &&
                EditorGUIUtility.GetObjectPickerControlID() == m_layerPickerWindowID)
            {
                m_PickedLayer = (TerrainLayer) EditorGUIUtility.GetObjectPickerObject();
                m_layerPickerWindowID = -1;

                if (m_PickedLayer)
                {

                    var exists = false;

                    foreach (TerraForgeTerrainPainterLayerSettings s in script.layerSettings)
                    {
                        if (s.layer == m_PickedLayer) exists = true;
                    }

                    if (exists)
                    {
                        EditorUtility.DisplayDialog("Terrain Painter", "Terrain layer already exists", "Ok");
                        return;
                    }
                }
                
                script.CreateSettingsForLayer(m_PickedLayer);
                RefreshLayerList();
                RefreshModifierLists();
                
                //Auto-select new layer
                m_LayerList.index = 0;
                m_LayerList.onSelectCallback.Invoke(m_LayerList);
                
                EditorUtility.SetDirty(target);

                requiresRepaint = true;
            }
        }
        
        private void DrawLayerBackground(Rect rect, int index, bool isactive, bool selected)
        {
            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;

            GUI.color = index % 2 == 0
                ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f)
                : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

            if (m_LayerList.index == index) GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;

            //Selection outline
            if (m_LayerList.index == index)
            {
                Rect outline = rect;
                EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);

                rect.x += 1;
                rect.y += 1;
                rect.width -= 2;
                rect.height -= 2;
            }

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;
        }

        void DrawLayerElement(Rect rect, int index, bool selected, bool focused)
        {
            rect.y = rect.y + kElementPadding;
            var rectButton = new Rect((rect.x + kElementPadding), rect.y + (kElementHeight / 4), kElementToggleWidth,
                kElementToggleWidth);
            var rectImage = new Rect((rectButton.x + kElementToggleWidth) + 5f, rect.y, kElementThumbSize, kElementThumbSize);
            var rectObject = new Rect((rectImage.x + kElementThumbSize + 10), rect.y + (kElementHeight / 4),
                kElementObjectFieldWidth, kElementObjectFieldHeight);
            
            if (script.layerSettings.Count > 0 && script.layerSettings.ElementAtOrDefault(index) != null)
            {
                if (index < layerSettings.arraySize-1)
                {
                    EditorGUI.Toggle(rectButton, new GUIContent(EditorGUIUtility.IconContent(script.layerSettings[index].enabled ? iconPrefix + "scenevis_visible_hover" : iconPrefix + "scenevis_hidden_hover").image), script.layerSettings[index].enabled, GUIStyle.none);
                    if (rectButton.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                        Event.current.button == 0)
                    {
                        script.layerSettings[index].enabled = !script.layerSettings[index].enabled;

                        requiresRepaint = true;
                    }
                }
                else
                {
                    //Base layer is always enabled
                    script.layerSettings[index].enabled = true;
                }
                
                Texture2D icon = null;
                if (script.layerSettings[index].layer != null)
                {
                    icon = AssetPreview.GetAssetPreview(script.layerSettings[index].layer.diffuseTexture);
                }
                GUI.Box(rectImage, icon);
                
                EditorGUI.BeginChangeCheck();
                script.layerSettings[index].layer = EditorGUI.ObjectField(rectObject, script.layerSettings[index].layer, typeof(TerrainLayer), false) as TerrainLayer;
                if (EditorGUI.EndChangeCheck())
                {
                    OnChangeLayer(script.layerSettings[index].layer, index);
                }
            }
        }

        void OnSelectLayerElement(ReorderableList list)
        {
            selectedLayerID = list.index;
            
            TerraForgeTerrainPainterLayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);

            SelectModifier(curModList, 0);
            
            //Refresh for current layer
            
        }

        void OnChangeLayer(TerrainLayer terrainLayer, int index)
        {
            requiresRepaint = true;

            script.SetTerrainLayers();
            RefreshLayerList();
        }

        void DrawLayerModifierStack()
        {
            if (layerSettings.arraySize == 0) return;
            
            if (selectedLayerID == layerSettings.arraySize -1)
            {
                EditorGUILayout.HelpBox("The base layer has no adjustable parameters, it fills the entire terrain." + (layerSettings.arraySize == 1 ? " \n\nAdd an additional terrain layer" : ""), MessageType.Info);
                return;
            }

            if (script.layerSettings.ElementAtOrDefault(selectedLayerID) == null)
            {
                EditorGUILayout.HelpBox("Select a layer to change its spawn rules", MessageType.Info);
                return;
            }
            
            TerraForgeTerrainPainterLayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);
            
            //Draw all modifierStack for the current layer
            using (new EditorGUI.DisabledGroupScope(settings.enabled == false))
            {
                if (curModList != null)
                {
                    curModList.DoLayoutList();
                    
                    if(curModList.index < 0 && curModList.count > 0) EditorGUILayout.HelpBox("Select a modifier from the stack to edit its settings", MessageType.Info);
                    if(curModList.count == 0) EditorGUILayout.HelpBox("Add a modifier to create painting rules", MessageType.Info);
                    
                    DrawModifierSettings(curModList.index);
                }
            }
            
        }

        void OnReorderLayerElement(ReorderableList list, int oldIndex, int newIndex)
        {
            script.SetTerrainLayers();
            RefreshLayerList();
            
            requiresRepaint = true;
        }
        
        void RemoveLayerElement(int index)
        {
            if (script.layerSettings.ElementAtOrDefault(index) == null)
            {
                return;
            }

            script.layerSettings.RemoveAt(index);

            script.SetTerrainLayers();
            RefreshLayerList();
            
            EditorUtility.SetDirty(target);

            requiresRepaint = true;
        }

        private void SelectModifier(ReorderableList list, int index)
        {
            selectedModifierIndex = index;
            list.index = index;
            list.onSelectCallback.Invoke(curModList);
        }
        private void OnRemoveModifier(ReorderableList list)
        {
            if (!EditorUtility.DisplayDialog("Remove modifier", "This operation cannot be undone, settings will be lost",
                "Ok", "Cancel")) return;

            TerraForgeTerrainPainterLayerSettings layer = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            
            layer.modifierStack.RemoveAt(list.index);
            RefreshModifierLists();
            
            EditorUtility.SetDirty(target);

            requiresRepaint = true;
        }

        private void DrawModifierHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Modifiers");
        }

        private void OnAddModifierDropDown(Rect buttonrect, ReorderableList list)
        {
            List<TerraForgeTerrainPainterModifier> currentModifierList = script.layerSettings.ElementAtOrDefault(selectedLayerID).modifierStack;
            
            GenericMenu menu = new GenericMenu();

            foreach (string item in TerraForgeTerrainPainterModifierEditor.ModifierNames)
            {
                menu.AddItem(new GUIContent(item), false, () => AddModifier(currentModifierList, list, item));
            }
                        
            menu.ShowAsContext();
        }
        
        private void AddModifier(List<TerraForgeTerrainPainterModifier> currentModifierList, ReorderableList list, string typeName)
        {
            Type type = TerraForgeTerrainPainterModifierEditor.GetType(typeName);
            TerraForgeTerrainPainterModifier m = (TerraForgeTerrainPainterModifier)CreateInstance(type);

            switch (typeName)
            {
                case "Terra Forge Terrain Painter Height":
                    m.label = "Height";
                    break;
                case "Terra Forge Terrain Painter Slope":
                    m.label = "Slope";
                    break;
                case "Terra Forge Terrain Painter Noise":
                    m.label = "Noise";
                    break;
            }

            // Assuming the terrains array contains Terrain components and you want the first one
            if (terrains.arraySize > 0)
            {
                var terrainProperty = terrains.GetArrayElementAtIndex(0);
                var terrainComponent = terrainProperty.objectReferenceValue as Terrain;
                if (terrainComponent != null)
                {
                    m.terrainData = terrainComponent.terrainData;
                }
            }

            // Save the created ScriptableObject as an asset
            string folderPath = "Assets/TerraForge 2/TerraForge Systems/Terrain Painter/Created Modifiers/";

            int index = 0;

            string assetName = $"Terra_Forge_Terrain_Painter_{m.label}_{index}.asset";
            string assetPath = folderPath + assetName;

            while (AssetDatabase.LoadAssetAtPath<TerraForgeTerrainPainterModifier>(assetPath) != null)
            {
                assetName = $"Terra_Forge_Terrain_Painter_{m.label}_{index}.asset";
                assetPath = folderPath + assetName;
                index++;
            }
            AssetDatabase.CreateAsset(m, assetPath);
            AssetDatabase.SaveAssets();

            currentModifierList.Insert(0, m);

            RefreshModifierLists();

            TerraForgeTerrainPainterLayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);

            // Auto select new modifier
            SelectModifier(curModList, 0);

            requiresRepaint = true;

            EditorUtility.SetDirty(target);
        }

        private void UpdateTerrainData(TerrainData newTerrainData)
        {
            foreach (var layer in script.layerSettings)
            {
                for (int i = 0; i < layer.modifierStack.Count; i++)
                {
                    var modifier = layer.modifierStack[i];
                    if (modifier != null)
                    {
                        if (modifier.terrainData != null && modifier.terrainData != newTerrainData)
                        {
                            // Create a copy of the modifier
                            TerraForgeTerrainPainterModifier newModifier = Instantiate(modifier);

                            // Save the new modifier as an asset

                            string folderPath = "Assets/TerraForge 2/TerraForge Systems/Terrain Painter/Created Modifiers/";

                            int index = 0;

                            string assetName = $"Terra_Forge_Terrain_Painter_{modifier.label}_{index}.asset";
                            string assetPath = folderPath + assetName;

                            while (AssetDatabase.LoadAssetAtPath<TerraForgeTerrainPainterModifier>(assetPath) != null)
                            {
                                assetName = $"Terra_Forge_Terrain_Painter_{modifier.label}_{index}.asset";
                                assetPath = folderPath + assetName;
                                index++;
                            }

                            AssetDatabase.CreateAsset(newModifier, assetPath);

                            // Update the terrainData of the new modifier
                            newModifier.terrainData = newTerrainData;
                            
                            // Replace the old modifier with the new one in the list
                            layer.modifierStack[i] = newModifier;

                            // Mark the new modifier as dirty
                            EditorUtility.SetDirty(newModifier);
                        }
                        else
                        {
                            // Update the terrainData of the existing modifier
                            modifier.terrainData = newTerrainData;
                            EditorUtility.SetDirty(modifier);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }



        private void RestoreModifiers()
        {
            foreach (var layer in script.layerSettings)
            {
                for (int i = 0; i < layer.modifierStack.Count; i++)
                {
                    // Ensure the reference is valid
                    if (layer.modifierStack[i] == null)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(layer.modifierStack[i].GetInstanceID().ToString());
                        layer.modifierStack[i] = AssetDatabase.LoadAssetAtPath<TerraForgeTerrainPainterModifier>(assetPath);
                    }
                }
            }
        }

        void OnSelectModifier(ReorderableList list)
        {
            selectedModifierIndex = list.index;
        }

        private void DrawModifierBackground(Rect rect, int index, bool isactive, bool isfocused)
        {
            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;

            GUI.color = index % 2 == 0
                ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f)
                : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

            
            //Selection outline (note: can't rely on isfocused. Focus and selection aren't the same thing)
            if (index == selectedModifierIndex)
            {
                GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;
                Rect outline = rect;
                EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);

                rect.x += 1;
                rect.y += 1;
                rect.width -= 2;
                rect.height -= 2;
            }
            

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;
        }

        private void DrawModifierElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            //Get modifierStack for current layer
            List<TerraForgeTerrainPainterModifier> currentModifierList = script.layerSettings.ElementAtOrDefault(m_LayerList.index).modifierStack;
            
            TerraForgeTerrainPainterModifier m = currentModifierList[index];
            
            rect.y = rect.y;
            var rectButton = new Rect(10 + (rect.x + kElementPadding), rect.y + kElementPadding, kElementToggleWidth,
                kElementToggleWidth);
            var labelRect = new Rect(rect.x + rectButton.x - 30, rect.y+kElementPadding, 80, 17);
            var blendModeRect = new Rect((labelRect.x + 90), rect.y+ kElementPadding, 80, 27);
            var opacityRect = new Rect(blendModeRect.x + blendModeRect.width + kElementPadding + 10, rect.y+ kElementPadding, 0f, 17);
            opacityRect.width = EditorGUIUtility.currentViewWidth - opacityRect.x - 50f;
            
            m.label = EditorGUI.TextField(labelRect, m.label);

            EditorGUI.Toggle(rectButton, new GUIContent(EditorGUIUtility.IconContent(m.enabled ? iconPrefix +  "scenevis_visible_hover" : iconPrefix + "scenevis_hidden_hover").image, "Toggle visibility"), m.enabled, GUIStyle.none);
            if (rectButton.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
            {
                m.enabled = !m.enabled;

                requiresRepaint = true;
            }
            
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            m.blendMode =  (TerraForgeTerrainPainterModifier.BlendMode) EditorGUI.Popup(blendModeRect, (int)m.blendMode, Enum.GetNames((typeof(TerraForgeTerrainPainterModifier.BlendMode))));
            m.opacity = EditorGUI.Slider(opacityRect, m.opacity, 1f, 100f);

            if (EditorGUI.EndChangeCheck())
            {
                requiresRepaint = true;
            }
        }

        private void DrawModifierSettings(int index)
        {
            EditorGUILayout.Space(5);
            // None selected
            if (index < 0) return;
            
            serializedObject.Update();
            
            SerializedProperty settingsElement = serializedObject.FindProperty("layerSettings").GetArrayElementAtIndex(m_LayerList.index);
            SerializedProperty modifiersElement = settingsElement.FindPropertyRelative("modifierStack");
            
            if (index >= modifiersElement.arraySize) return;
            
            if (modifiersElement.arraySize > 0)
            {
                SerializedProperty modifierProp = modifiersElement.GetArrayElementAtIndex(index);
                
                if (modifierProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Modifier is not assigned.", MessageType.Warning);
                    return;
                }
                
                var editor = UnityEditor.Editor.CreateEditor(modifierProp.objectReferenceValue);
                
                EditorGUI.BeginChangeCheck();

                editor.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    requiresRepaint = true;
                }
            }
        }
        
        void OnReorderModifier(ReorderableList list, int oldIndex, int newIndex)
        {
            RefreshModifierLists();

            requiresRepaint = true;
        }

        private GUIStyle GetOutlineStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_0;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D CreateOutlineTexture(int width, int height, Color borderColor, Color fillColor)
        {
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        pix[y * width + x] = borderColor;
                    }
                    else
                    {
                        pix[y * width + x] = fillColor;
                    }
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private GUIStyle GetOutlineStyle_D_O()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_2;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }
    }
}