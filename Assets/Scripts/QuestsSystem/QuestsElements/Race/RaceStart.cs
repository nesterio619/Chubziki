
namespace QuestsSystem.QuestsElements
{
    public class RaceStart : QuestElement
    {
        public void PlayerCrossStart()
        {
            OnQuestEventTriggered?.Invoke();
        }
    }
}

