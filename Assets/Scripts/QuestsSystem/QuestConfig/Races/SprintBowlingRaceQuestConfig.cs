using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestLogic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create bowling quest", fileName = "Assets/Resources/Quests/NewBowlingSprintRaceQuest")]
public class SprintBowlingRaceQuestConfig : QuestConfig
{
    public float TimeForSprint;

    protected override QuestLogic GetQuestLogicType()
    {
        return new SprintBowlingRaceQuest();
    }

    protected override void InitializeQuestLogic(QuestLogic questLogic)
    {
        (questLogic as SprintBowlingRaceQuest).Initialize(this);
    }

}
