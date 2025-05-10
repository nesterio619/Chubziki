using QuestsSystem.QuestConfig;
using QuestsSystem.QuestLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Puzzles/Generic puzzle", fileName = "PuzzleQuestConfig")]
public class PuzzleQuestConfig : QuestConfig
{
    [Serializable]
    public struct ActorPresetWithEvent
    {
        public ActorMoldAndTransformPath Base;
        public bool CompletesQuest;
        public string EventName;

        public static implicit operator ActorPresetWithPath(ActorPresetWithEvent param)
        {
            return param.Base;
        }
    }

    [Serializable]
    public struct ActorEvent
    {
        public string ActorPath;
        public string[] EventNames;

        public ActorEvent(string actorPath, string[] eventNames)
        {
            ActorPath = actorPath;
            EventNames = eventNames;
        }
    }

    public List<ActorPresetWithEvent> ActorPresetsWithEvents = new List<ActorPresetWithEvent>();
    public List<ActorEvent> ActorEvents = new List<ActorEvent>();

    protected override QuestLogic GetQuestLogicType()
    {
        return new PuzzleQuestLogic();
    }

    protected override void InitializeQuestLogic(QuestLogic questLogic)
    {
        ((PuzzleQuestLogic)questLogic).Initialize(this);
    }

    public override ActorPresetWithPath[] GetActorsToSpawn()
    {
        var list = new List<ActorPresetWithPath>();

        foreach (var actor in ActorPresetsWithEvents)
            list.Add(actor);

        return list.ToArray();
    }

    public void ModifyOrAddActorEvents(string transformPath, List<string> eventsList)
    {
        if(eventsList.Count == 0) return;

        bool exists = false;
        for (int j = 0; j < ActorEvents.Count; j++)
        {
            var listElement = ActorEvents[j];
            if (listElement.ActorPath == transformPath)
            {
                listElement.EventNames = eventsList.ToArray();
                ActorEvents[j] = listElement;
                exists = true;
                break;
            }
        }

        if (!exists)
            ActorEvents.Add(new ActorEvent(transformPath, eventsList.ToArray()));
    }
}
