using System;
using QuestsSystem.Base;

namespace QuestsSystem.QuestsElements
{
    public class BowlingAlley : QuestElement
    {
        public event Action OnPlayerBowlingAlley;
        
        public void PlayerTouchedBowlingAlley()
        {
            OnPlayerBowlingAlley?.Invoke();
        }
    }
}
