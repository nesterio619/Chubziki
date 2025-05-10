using System;
using QuestsSystem.Base;
using UnityEngine;

namespace QuestsSystem.QuestsElements
{
    public class BrakingLine : QuestElement
    {
        public void PlayerTouchedBrakingLine()
        {
            OnQuestEventTriggered?.Invoke();
        }
    }
}
