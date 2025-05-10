using QuestsSystem.QuestLogic;
using UnityEngine;

namespace QuestsSystem.Base
{
    [CreateAssetMenu(menuName = "Quests/Create sprint quest", fileName = "Assets/Resources/Quests/NewSprintRaceQuest")]
    public class SprintRaceQuestConfig : QuestConfig.QuestConfig
    {
        public float TimeForSprint;

        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new SprintRaceQuest();
        }

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            ((SprintRaceQuest)questLogic).Initialize(this,TimeForSprint);
        }

        

    }
}