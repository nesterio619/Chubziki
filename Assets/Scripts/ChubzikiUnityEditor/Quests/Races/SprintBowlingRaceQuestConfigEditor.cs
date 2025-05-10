#if UNITY_EDITOR
using UnityEditor;

namespace ChubzikiUnityEditor.Quests
{
    [CustomEditor(typeof(SprintBowlingRaceQuestConfig))]
    public class SprintBowlingRaceQuestConfigEditor : QuestConfigEditor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif