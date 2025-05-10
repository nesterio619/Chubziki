using System.Collections;
using System.Collections.Generic;
using Actors;
using Core;
using Core.Utilities;
using QuestsSystem.Base;
using UnityEngine;

public class FightQuestLogic : BattleQuestLogicBase
{
    private AIActor[] createdAIActors;
    private bool _allEnemiesAreDead;

    public override void SpawnAllActors()
    {
        if (!QuestsManager.Instance.IsQuestCompleted(QuestName))
            base.SpawnAllActors();
    }

    protected internal override void OnAccept()
    {
        if (!questIsCompleted)
        {
            var createdAI = new List<AIActor>();
            foreach (var createdActor in AllCreatedActors)
            {
                createdActor.ToggleLogic(true);

                if (createdActor is AIActor createdActorAI && !createdAI.Contains(createdActorAI))
                    createdAI.Add(createdActorAI);
            }

            createdAIActors = createdAI.ToArray();
            createdAI.Clear();

            Player.Instance.StartCoroutine(CountAliveEnemies());
        }

        base.OnAccept();
    }

    IEnumerator CountAliveEnemies()
    {
        yield return new WaitForSeconds(1f);
        
        while (!questIsCompleted && !_allEnemiesAreDead)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();

            _allEnemiesAreDead = true;
            foreach (var actorAI in createdAIActors)
                if (actorAI.IsAlive)
                {
                    _allEnemiesAreDead = false;
                    yield return null;
                }

            if (_allEnemiesAreDead)
            {
                FinishBattle();
                yield break;
            }
        }
    }

    public override void FinishBattle()
    {
        UtilitiesProvider.WaitAndRun(() =>
        {
            if (questIsCompleted)
                return;
        
            if(QuestsManager.Instance != null)
                OnComplete(_allEnemiesAreDead);
        }, true, 1f);
    }
}
