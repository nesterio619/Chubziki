using QuestsSystem.QuestLogic;
using UnityEngine;

namespace QuestsSystem.QuestConfig
{
    [CreateAssetMenu(menuName = "Quests/Puzzles/Bridge and ball quest", fileName = "BridgeAndBallQuestConfig")]
    public class BridgeAndBallQuestConfig : PuzzleQuestConfig
    {
        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new BridgeAndBallQuest();
        }

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            ((BridgeAndBallQuest)questLogic).Initialize(this);
        }
    }
}