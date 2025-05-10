using System;
using QuestsSystem.Base;
using UnityEngine;

namespace QuestsSystem.QuestsElements
{
    public class DirectionArrow : QuestElement
    {
        public event Action OnPlayerDirectionArrow;
        
        public void PlayerTouchedDirectionArrow()
        {
            OnPlayerDirectionArrow?.Invoke();
        }
    }
}
