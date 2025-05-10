using System.Collections;
using System.Collections.Generic;
using QuestsSystem.QuestLogic;
using QuestsSystem.QuestLogic.Bowling;
using UnityEngine;

namespace QuestsSystem.Base
{
    [CreateAssetMenu(menuName = "Quests/Create Car Bowling quest", fileName = "Assets/Resources/Quests/NewCarBowlingQuest")]
    public class CarBowlingQuestConfig : QuestConfig.QuestConfig
    {
        [field: SerializeField] public float TimeTotalForMission;
        [field: SerializeField] public float MinOffsetBowlingPin;
        [field: SerializeField] public float MaxOffsetBowlingPin;

        protected override QuestLogic.QuestLogic GetQuestLogicType()
        {
            return new CarBowlingQuest();
        }

        protected override void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            (questLogic as CarBowlingQuest).Initialize(this,TimeTotalForMission,MinOffsetBowlingPin,MaxOffsetBowlingPin);
        }
    }
}
