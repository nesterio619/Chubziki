#if UNITY_EDITOR
using Core.SaveSystem;
using Core.Utilities;
using QuestsSystem;
using QuestsSystem.QuestConfig;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(QuestConfig),true)]
    public class QuestConfigEditor : Editor
    {
        protected QuestConfig _config;
        
        protected ReorderableList _questElementsList;
        protected SerializedProperty _questNameProp;
        protected SerializedProperty _questDescriptionProp;
        protected SerializedProperty _startPositionPathProp;
        protected SerializedProperty _questElementsProp;
        protected SerializedProperty _showQuestElementsProp;
        protected SerializedProperty _questLocationPathProperty;
        protected SerializedProperty _showDebugPopupOnFinishProp;
        protected SerializedProperty _showDebugPopupOnStartProp;
        protected SerializedProperty _acceptOnLocationEnterProperty;
        protected SerializedProperty _spawnInfoProp;
        protected SerializedProperty _actorsProp;
        protected SerializedProperty _failOnLocationExitProperty;
        protected SerializedProperty _rewardKey;
        protected SerializedProperty _rewardAddAmount;
        protected SerializedProperty _canBeRestarted;
        protected ReorderableList _actorsList;

        protected readonly Color WarningColor = new Color(1f, 0.3f, 0.3f);
        public static readonly Color[] GizmoColors = new Color[] { Color.yellow, Color.cyan, Color.red, Color.green, Color.magenta, Color.blue };
        protected bool _showActorGroupRayGizmo;
        protected int selectedKeyIndex = 0;

        protected static readonly string[] unlockNames = new string[] {
            UnlockableEquipment.None.ToString(),
            UnlockableEquipment.Turret.ToString(),
            UnlockableEquipment.SawBlade.ToString(),
            UnlockableEquipment.RammingBar.ToString(),
        };

        protected virtual void OnEnable()
        {
            _config = target as QuestConfig;
            if (_config == null) return;
            
            _questNameProp = serializedObject.FindProperty("QuestName");
            _questDescriptionProp = serializedObject.FindProperty("QuestDescription");
            _showQuestElementsProp = serializedObject.FindProperty("ShowQuestElements");
            _startPositionPathProp = serializedObject.FindProperty("StartPositionPath");
            _questElementsProp = serializedObject.FindProperty("<QuestElementsAndTransformsPaths>k__BackingField");
            _questLocationPathProperty = serializedObject.FindProperty("QuestLocationPath");
            _showDebugPopupOnStartProp = serializedObject.FindProperty("ShowDebugPopupOnStart");
            _showDebugPopupOnFinishProp = serializedObject.FindProperty("ShowDebugPopupOnFinish");
            _acceptOnLocationEnterProperty = serializedObject.FindProperty("AcceptQuestOnLocationEnter");
            _failOnLocationExitProperty = serializedObject.FindProperty("FailQuestOnLocationExit");
            _spawnInfoProp = serializedObject.FindProperty("<ActorSpawnInfo>k__BackingField");
            _actorsProp = serializedObject.FindProperty("IndividualQuestActors");
            _rewardKey = serializedObject.FindProperty("UnlockName");
            _rewardAddAmount = serializedObject.FindProperty("UpgradePoints");
            _canBeRestarted = serializedObject.FindProperty("CanBeRestarted");
            _questElementsList = CreateReorderableList(_questElementsProp, "Quest Elements And Transforms");
            _actorsList = CreateReorderableList(_actorsProp, "Individual actors");

            SceneView.duringSceneGui += DrawSpawnerGizmos;
            QuestActorDrawer.OnGenerate += ShowActorGroupRayGizmo;

            selectedKeyIndex = Array.IndexOf(unlockNames, _rewardKey.stringValue);
            if(selectedKeyIndex == -1) selectedKeyIndex = 0;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSpawnerGizmos;
            QuestActorDrawer.OnGenerate -= ShowActorGroupRayGizmo;
        }

        private void ShowActorGroupRayGizmo() => _showActorGroupRayGizmo = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quest Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_questNameProp);
            EditorGUILayout.PropertyField(_questDescriptionProp);
            EditorGUILayout.PropertyField(_canBeRestarted);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_showQuestElementsProp);
            EditorGUILayout.PropertyField(_showDebugPopupOnStartProp);
            EditorGUILayout.PropertyField(_showDebugPopupOnFinishProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reward", EditorStyles.boldLabel);
            selectedKeyIndex = EditorGUILayout.Popup("Unlock Equipment", selectedKeyIndex, unlockNames);
            if(selectedKeyIndex != -1) _rewardKey.stringValue = unlockNames[selectedKeyIndex];
            EditorGUILayout.PropertyField(_rewardAddAmount);

            EditorGUILayout.Space();
            DrawTransformPathField("Start Position", _startPositionPathProp, new Rect());
            _questElementsList.DoLayoutList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quest Actors", EditorStyles.boldLabel);

            DrawTransformPathField("'Location' of quest", _questLocationPathProperty, new Rect());

            EditorGUILayout.PropertyField(_acceptOnLocationEnterProperty);
            EditorGUILayout.PropertyField(_failOnLocationExitProperty);

            EditorGUILayout.Space();
            _actorsList.DoLayoutList();

            EditorGUILayout.Space();
            var previousShowGizmo = _showActorGroupRayGizmo;
            _showActorGroupRayGizmo = EditorGUILayout.Toggle(new GUIContent("Show Actor Group Ray Gizmo"), _showActorGroupRayGizmo);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.PropertyField(_spawnInfoProp,new GUIContent("Actor groups"), true);

            if (GUI.changed)
            {
                _config.CreateEditorElements();
                if(previousShowGizmo!=_showActorGroupRayGizmo) SceneView.RepaintAll();
            }
        }

        protected ReorderableList CreateReorderableList(SerializedProperty property, string header)
        {
            return new ReorderableList(serializedObject, property, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, header),
                drawElementCallback = (rect, index, isActive, isFocused) => DrawListElement(property, rect, index),
                elementHeightCallback = _ => GetListElementHeight(),
                onAddCallback = list => ModifyArraySize(property, 1),
                onRemoveCallback = list => RemoveElement(property, list)
            };
        }

        protected virtual float GetListElementHeight() => EditorGUIUtility.singleLineHeight * 3.5f + 10f;

        protected virtual void DrawListElement(SerializedProperty property, Rect rect, int index)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            SerializedProperty transformPathProp = element.FindPropertyRelative("TransformPath");
            SerializedProperty objectProp = element.FindPropertyRelative("_PrefabPoolInfo") ?? element.FindPropertyRelative("Mold");
            
            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 4f;
            Rect transformRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            DrawTransformPathField("Element Transform", transformPathProp, transformRect);
            
            Rect objectRect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            DrawPropertyField("Pool Info", objectProp, objectRect);

            SerializedProperty saveProp = element.FindPropertyRelative("SavePosition");
            if(saveProp != null )
            {
                Rect saveRect = new Rect(rect.x, rect.y + (lineHeight + padding)*2, rect.width, lineHeight);
                DrawPropertyField("Save Position", saveProp, saveRect);
            }
        }

        protected void DrawTransformPathField(string label, SerializedProperty property, Rect rect)
        {
            if (string.IsNullOrEmpty(property.stringValue)) GUI.color = WarningColor;
            UtilitiesProvider.TransformPathField(label, property, _config, rect);
            GUI.color = Color.white;
        }

        protected void DrawPropertyField(string label, SerializedProperty property, Rect rect)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null) GUI.color = WarningColor;
            EditorGUI.PropertyField(rect, property, new GUIContent(label));
            GUI.color = Color.white;
        }

        private void ModifyArraySize(SerializedProperty property, int delta)
        {
            property.arraySize += delta;
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveElement(SerializedProperty property, ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to remove this element?", "Yes", "No"))
            {
                if(list == _questElementsList)
                    _config.DestroyEditorElementAtPath(_config.QuestElementsAndTransformsPaths[list.index].TransformPath);

                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawSpawnerGizmos(SceneView sceneView)
        {
            for(int i = 0; i < _config.ActorSpawnInfo.Count; i++)
            {
                var info = _config.ActorSpawnInfo[i];
                var center = UtilitiesProvider.GetTransformFromPath(info.CenterTransformPath);

                if (center == null) continue;

                var color = GizmoColors[i % GizmoColors.Length];

                Handles.color = color;
                Handles.DrawSolidDisc(center.position, center.up, 0.1f);
                Handles.DrawWireDisc(center.position, center.up, info.Radius);

                var textStyle = new GUIStyle();
                textStyle.alignment = TextAnchor.MiddleCenter;
                textStyle.normal.textColor = color;
                Handles.Label(center.position+Vector3.up, QuestConfig.GenerateSpawnerName(info), textStyle);
            }

            if (!_showActorGroupRayGizmo) return;

            for (int i = 0; i < _config.ActorsToSpawn.Count; i++)
            {
                var element = _config.ActorsToSpawn[i];

                var colorIndex = _config.ActorSpawnInfo.FindIndex(x=>x.CenterTransformPath==element.ParentTransformPath);
                Handles.color = GizmoColors[colorIndex % GizmoColors.Length];

                foreach (var path in element.TransformPaths)
                {
                    var transform = UtilitiesProvider.GetTransformFromPath(path);
                    if (transform == null) continue;

                    Handles.DrawLine(transform.position, transform.position + transform.up * 10);
                }
            }
                
        }
    }
}
#endif