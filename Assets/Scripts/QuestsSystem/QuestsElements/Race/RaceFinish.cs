namespace QuestsSystem.QuestsElements
{
    public class RaceFinish : QuestElement
    {
        public void PlayerReachFinish()
        {
            OnQuestEventTriggered?.Invoke();
        }
    }

}
