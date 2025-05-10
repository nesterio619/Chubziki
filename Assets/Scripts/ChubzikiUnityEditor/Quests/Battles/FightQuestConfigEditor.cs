#if UNITY_EDITOR
using UnityEditor;

namespace ChubzikiUnityEditor.Quests.Battles
{
    [CustomEditor(typeof(FightQuestConfig))]
    public class FightQuestConfigEditor : QuestConfigEditor
    {
    }
}
#endif
