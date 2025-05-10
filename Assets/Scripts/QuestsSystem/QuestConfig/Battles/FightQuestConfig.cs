using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestLogic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Battles/Fights quest", fileName = "Assets/Resources/Quests/NewFightQuest")]
public class FightQuestConfig : QuestConfig
{
    protected override QuestLogic GetQuestLogicType()
    {
        return new FightQuestLogic();
    }
    
    protected override void InitializeQuestLogic(QuestLogic questLogic)
    {
        ((FightQuestLogic)questLogic).Initialize(this);
    }
}
