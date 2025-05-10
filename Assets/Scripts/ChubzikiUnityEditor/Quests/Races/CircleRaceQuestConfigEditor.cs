#if UNITY_EDITOR
using QuestsSystem.Base;
using UnityEditor;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(CircleRaceQuestConfig))]
    public class CircleRaceQuestConfigEditor : QuestConfigEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CirclesForRace"));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }


    }
}
#endif