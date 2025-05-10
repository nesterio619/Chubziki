using QuestsSystem.QuestLogic;
using UnityEngine;

namespace QuestsSystem.Base
{
    [CreateAssetMenu(menuName = "Quests/Create circle race quest", fileName = "Assets/Resources/Quests/NewCircleRaceQuest")]
    public class CircleRaceQuestConfig : QuestConfig.QuestConfig
    {
        [field: SerializeField] public int CirclesForRace;

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            (questLogic as CircleRaceQuest).Initialize(this,CirclesForRace);
        }

        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new CircleRaceQuest();
        }
    }
}