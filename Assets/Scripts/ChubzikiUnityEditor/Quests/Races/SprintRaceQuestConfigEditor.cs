#if UNITY_EDITOR
using QuestsSystem.Base;
using UnityEditor;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(SprintRaceQuestConfig))]
    public class SprintRaceQuestConfigEditor : QuestConfigEditor
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TimeForSprint"));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif