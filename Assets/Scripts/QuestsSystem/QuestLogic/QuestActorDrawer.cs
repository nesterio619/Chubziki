#if UNITY_EDITOR
using UnityEngine;
using QuestsSystem.QuestConfig;
using Core.Utilities;
using UnityEditorInternal;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using ChubzikiUnityEditor.Quests;
using System;

namespace QuestsSystem
{
    [CustomPropertyDrawer(typeof(ActorSpawnInfo))]
    public class QuestActorDrawer : PropertyDrawer
    {
        private const string FoldoutKey = "ActorSpawnInfoFoldout";
        private bool isFoldout = true;

        public static Action OnGenerate;

        private List<ReorderableList> _reorderableLists = new();
        private QuestConfig.QuestConfig _questConfig;

        private float _lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_questConfig == null)
                _questConfig = (QuestConfig.QuestConfig)property.serializedObject.targetObject;
                
            property.serializedObject.Update();
            EditorGUI.BeginProperty(position, label, property);

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            var centerProp = property.FindPropertyRelative("CenterTransformPath");
            var radiusProp = property.FindPropertyRelative("Radius");
            var moldsProp = property.FindPropertyRelative("MoldCounts");

            if (GetPropertyIndex(property) >= _reorderableLists.Count)
                _reorderableLists.Add(CreateReorderableList(property.serializedObject, moldsProp));

            string foldoutName = QuestConfig.QuestConfig.GenerateSpawnerName((ActorSpawnInfo)property.boxedValue);
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            isFoldout = EditorGUI.Foldout(foldoutRect, isFoldout, foldoutName, true, GetFoldoutStyle(property));
            EditorPrefs.SetBool(foldoutKey, isFoldout);

            if (isFoldout)
            {
                EditorGUI.indentLevel++;
                
                Rect currentRect = new Rect(position.x, position.y + _lineHeight, position.width, EditorGUIUtility.singleLineHeight);

                UtilitiesProvider.TransformPathField("Center Transform", centerProp, null, currentRect);
                currentRect.y += _lineHeight;

                EditorGUI.PropertyField(currentRect, radiusProp);
                currentRect.y += _lineHeight * 1.5f;

                _reorderableLists[GetPropertyIndex(property)].DoList(currentRect);
                currentRect.y += _reorderableLists[GetPropertyIndex(property)].GetHeight();

                currentRect.y += EditorGUIUtility.standardVerticalSpacing*2;
                if(GUI.Button(currentRect,"Generate positions"))
                {
                    _questConfig.GenerateActorPositions(GetPropertyIndex(property));
                    OnGenerate?.Invoke();
                }
                    

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            bool isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            var moldsProp = property.FindPropertyRelative("MoldCounts");

            if (isFoldout)
            {
                height += EditorGUIUtility.singleLineHeight * 8f;

                if (_reorderableLists != null)
                {
                    var arraySize = moldsProp.arraySize == 0 ? 0.5f : moldsProp.arraySize;
                    height += EditorGUIUtility.singleLineHeight * 2.7f * arraySize;
                }
            }
            return height;
        }

        private ReorderableList CreateReorderableList(SerializedObject serializedObject, SerializedProperty listProperty)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return new ReorderableList(serializedObject, listProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Molds and counts"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
                    SerializedProperty moldProp = element.FindPropertyRelative("Mold");
                    SerializedProperty countProp = element.FindPropertyRelative("Count");

                    var moldRect = rect;
                    moldRect.height = lineHeight;
                    EditorGUI.PropertyField(moldRect, moldProp);
                    rect.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;

                    var countRect = rect;
                    countRect.height = lineHeight;
                    EditorGUI.PropertyField(countRect, countProp);
                    rect.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;
                },
                onRemoveCallback = list =>
                {
                    if (list.index >= 0)
                        listProperty.DeleteArrayElementAtIndex(list.index);
                },
                onAddCallback = list => listProperty.arraySize++,
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight * 2.7f
            };
        }

        private int GetPropertyIndex(SerializedProperty property)
        {
            Match match = Regex.Match(property.propertyPath, @"\[(\d+)\]");
            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
        }

        private GUIStyle GetFoldoutStyle(SerializedProperty property)
        {
            var foldoutStyle = new GUIStyle(EditorStyles.foldout);
            int index = GetPropertyIndex(property) % QuestConfigEditor.GizmoColors.Length;

            foldoutStyle.normal.textColor = QuestConfigEditor.GizmoColors[index];
            foldoutStyle.onNormal.textColor = QuestConfigEditor.GizmoColors[index];
            foldoutStyle.focused.textColor = QuestConfigEditor.GizmoColors[index];
            foldoutStyle.onFocused.textColor = QuestConfigEditor.GizmoColors[index];

            return foldoutStyle;
        }
    }
}
#endif
