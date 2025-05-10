#if UNITY_EDITOR
using QuestsSystem.Base;
using UnityEditor;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(GiantBowlingQuestConfig))]
    public class GiantBowlingQuestConfigEditor : QuestConfigEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TimeTotalForMission"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MinOffsetBowlingPin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxOffsetBowlingPin"));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif