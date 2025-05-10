#if UNITY_EDITOR
using Actors.Molds;
using Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(PuzzleQuestConfig), true)]
    public class PuzzleQuestConfigEditor : QuestConfigEditor
    {
        private SerializedProperty _actorListProp;
        private PuzzleQuestConfig _puzzleConfig;

        protected override void OnEnable()
        {
            base.OnEnable();

            _puzzleConfig = _config as PuzzleQuestConfig;

            _actorListProp = serializedObject.FindProperty("ActorPresetsWithEvents");
            _actorsList = CreateReorderableList(_actorListProp, "Individual actors");
        }

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
            if (selectedKeyIndex != -1) _rewardKey.stringValue = unlockNames[selectedKeyIndex];
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

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                _puzzleConfig.CreateEditorElements();
                RefreshActorEvents();
            }
        }

        protected override float GetListElementHeight() => EditorGUIUtility.singleLineHeight * 4.8f + 10f;

        protected override void DrawListElement(SerializedProperty property, Rect rect, int index)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            SerializedProperty baseProp = element.FindPropertyRelative("Base");
            SerializedProperty transformPathProp = baseProp.FindPropertyRelative("TransformPath");
            SerializedProperty objectProp = baseProp.FindPropertyRelative("_PrefabPoolInfo") ?? baseProp.FindPropertyRelative("Mold");

            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 4f;
            Rect transformRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            DrawTransformPathField("Element Transform", transformPathProp, transformRect);

            Rect objectRect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
            DrawPropertyField("Pool Info", objectProp, objectRect);

            SerializedProperty saveProp = baseProp.FindPropertyRelative("SavePosition");
            if (saveProp != null)
            {
                Rect saveRect = new Rect(rect.x, rect.y + (lineHeight + padding) * 2, rect.width, lineHeight);
                DrawPropertyField("Save Position", saveProp, saveRect);
            }

            var path = transformPathProp.stringValue;
            DrawEventPopup(element, rect, path);
        }

        private void DrawEventPopup(SerializedProperty element, Rect rect, string transformPath)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 4f;

            SerializedProperty toggleProp = element.FindPropertyRelative("CompletesQuest");
            Rect toggleRect = new Rect(rect.x, rect.y + (lineHeight + padding) * 3, rect.width, lineHeight);
            EditorGUI.PropertyField(toggleRect, toggleProp, new GUIContent("Completes Quest", "If enabled, lets you choose the event that completes the quest."));

            if (!toggleProp.boolValue) return;

            Rect popupRect = new Rect(rect.x + rect.width / 2, rect.y + (lineHeight + padding) * 3, rect.width / 2, lineHeight);

            string errorString = null;
            if (string.IsNullOrEmpty(transformPath))
                errorString = "Transform cannot be null";

            if (!_puzzleConfig.ActorEvents.Exists(x => x.ActorPath == transformPath))
                errorString = "No events found";

            if (errorString != null)
            {
                GUI.color = WarningColor;
                EditorGUI.LabelField(popupRect, new GUIContent(errorString));
                GUI.color = Color.white;
                return;
            }

            SerializedProperty eventProp = element.FindPropertyRelative("EventName");

            var actorEvents = _puzzleConfig.ActorEvents.First(x => x.ActorPath == transformPath).EventNames;
            var eventIndex = Array.IndexOf(actorEvents, eventProp.stringValue);
            eventIndex = eventIndex == -1 ? 0 : eventIndex;

            var newIndex = EditorGUI.Popup(popupRect, eventIndex, actorEvents);

            eventProp.stringValue = actorEvents[newIndex];
        }

        private void RefreshActorEvents()
        {
            for (int i = 0; i < _actorListProp.arraySize; i++)
            {
                var element = _actorListProp.GetArrayElementAtIndex(i);
                var baseProp = element.FindPropertyRelative("Base");

                var toggleProp = element.FindPropertyRelative("CompletesQuest");
                var transformPathProp = baseProp.FindPropertyRelative("TransformPath");
                var moldProp = baseProp.FindPropertyRelative("Mold");
                var transformPath = transformPathProp.stringValue;

                if (string.IsNullOrEmpty(transformPath)) continue;

                if (!toggleProp.boolValue)
                {
                    _puzzleConfig.ActorEvents.RemoveAll(x => x.ActorPath == transformPath);
                    continue;
                }

                var transform = UtilitiesProvider.GetTransformFromPath(transformPath);
                if(transform == null) continue;

                _puzzleConfig.ModifyOrAddActorEvents(transformPath, GatherEvents(transform, moldProp));
            }
        }

        private List<string> GatherEvents(Transform transform, SerializedProperty moldProp)
        {
            var objScript = transform.GetComponentInChildren<MonoBehaviour>();
            var list = new List<string>();
            if (objScript != null)
                list.AddRange(GetAllEvents(objScript));

            if (moldProp.objectReferenceValue != null)
            {
                var poolInfo = ((Mold)moldProp.objectReferenceValue).PrefabPoolInfoGetter;
                if (AssetUtils.TryLoadAsset(poolInfo.ObjectPath, out GameObject prefab))
                {
                    var moldScript = prefab.GetComponent<MonoBehaviour>();
                    if (moldScript != null)
                        list.AddRange(GetAllEvents(moldScript));
                }
            }
            return list;
        }

        private List<string> GetAllEvents(object target)
        {
            var events = target.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var unityEvents = target.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(UnityEventBase).IsAssignableFrom(f.FieldType))
                .ToArray();

            var list = new List<string>(events.Select(e => e.Name));
            list.AddRange(unityEvents.Select(e => e.Name));

            return list;
        }
    }
}
#endif