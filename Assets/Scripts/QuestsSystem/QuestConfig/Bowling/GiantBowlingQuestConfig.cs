using QuestsSystem.QuestLogic;
using QuestsSystem.QuestLogic.Bowling;
using UnityEngine;

namespace QuestsSystem.Base
{
    [CreateAssetMenu(menuName = "Quests/Create Giant Bowling quest", fileName = "Assets/Resources/Quests/NewBowlingQuest")]
    public class GiantBowlingQuestConfig : QuestConfig.QuestConfig
    {
        [field: SerializeField] public float TimeTotalForMission;
        [field: SerializeField] public float MinOffsetBowlingPin;
        [field: SerializeField] public float MaxOffsetBowlingPin;
        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new GiantBowlingQuest();
        }

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            (questLogic as GiantBowlingQuest).Initialize(this,TimeTotalForMission,MinOffsetBowlingPin,MaxOffsetBowlingPin);
        }
    }
}
