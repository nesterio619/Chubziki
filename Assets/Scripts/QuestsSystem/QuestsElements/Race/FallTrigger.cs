namespace QuestsSystem.QuestsElements
{
    public class FallTrigger : QuestElement
    {
        public void PlayerTouchFallTrigger()
        {
            OnQuestEventTriggered?.Invoke();
        }
    }
}
