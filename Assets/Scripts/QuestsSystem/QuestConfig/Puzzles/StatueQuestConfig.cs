using QuestsSystem.QuestLogic;
using UnityEngine;

namespace QuestsSystem.QuestConfig
{
    [CreateAssetMenu(menuName = "Quests/Puzzles/Statue quest", fileName = "StatueQuestConfig")]
    public class StatueQuestConfig : PuzzleQuestConfig
    {
        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new StatueQuest();
        }

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            ((StatueQuest)questLogic).Initialize(this);
        }
    }
}