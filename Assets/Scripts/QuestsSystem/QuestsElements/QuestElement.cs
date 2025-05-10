using System;
using Core.ObjectPool;
using UnityEngine;

namespace QuestsSystem.QuestsElements
{
    public abstract class QuestElement : PooledGameObject
    {
        protected string questName;
        
        public Action OnQuestEventTriggered;
        
        public virtual void SetQuestName(string questsName)
        {
            if (!string.IsNullOrEmpty(questName))
            {
                Debug.LogError("You are trying to reset quest element's 'questName' which is not supported");
                return;
            }
            
            questName = questsName;
        }

        public override void ReturnToPool()
        {
            questName = null;
            base.ReturnToPool();
        }
    }
}

