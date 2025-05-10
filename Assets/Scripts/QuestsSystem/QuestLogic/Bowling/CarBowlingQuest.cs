using Core;
using QuestsSystem.QuestConfig;

namespace QuestsSystem.QuestLogic.Bowling
{
    public class CarBowlingQuest : BowlingQuest
    {
        protected internal override void OnAccept()
        {
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true);

            base.OnAccept();
        }
        
        public override void Dispose()
        {
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions();
            base.Dispose();
        }
        
        // нужно исправить
        public void Initialize(QuestConfig.QuestConfig questConfig, float timeTotalForMission,float minOffsetBowlingPin,float maxOffsetBowlingPin)
        {
            base.Initialize(questConfig, timeTotalForMission,minOffsetBowlingPin, maxOffsetBowlingPin);
        }
        protected override void OnPlayerTouchedBrakingLine()
        {
            SetTimer();
        }
    }
}
