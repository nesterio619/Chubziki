using Core.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using static PuzzleQuestConfig;

namespace QuestsSystem.QuestLogic
{
    public class PuzzleQuestLogic : QuestLogic
    {
        private List<ActorPresetWithEvent> _actorPresetsWithEvents = new List<ActorPresetWithEvent>();

        private Action _completePuzzle;

        public override void Initialize(QuestConfig.QuestConfig questConfig)
        {
            base.Initialize(questConfig);

            var puzzleConfig = questConfig as PuzzleQuestConfig;
            _actorPresetsWithEvents = puzzleConfig.ActorPresetsWithEvents;

            _completePuzzle = () => OnComplete(true);
            enableActorLogic = true;
        }

        protected internal override void OnAccept()
        {
            base.OnAccept();

            foreach (var preset in _actorPresetsWithEvents)
            { 
                if(!preset.CompletesQuest) continue;

                var transform = UtilitiesProvider.GetTransformFromPath(preset.Base.TransformPath);
                if (transform == null) continue;

                var script = transform.GetComponentInChildren<MonoBehaviour>();
                if(script == null) continue;

                SubscribeToAnyEvent(script, preset.EventName, _completePuzzle);
            }
        }

        public override void OnComplete(bool victory)
        {
            base.OnComplete(victory);

            foreach (var preset in _actorPresetsWithEvents)
            {
                if (!preset.CompletesQuest) continue;

                var transform = UtilitiesProvider.GetTransformFromPath(preset.Base.TransformPath);
                if (transform == null) continue;

                var script = transform.GetComponentInChildren<MonoBehaviour>();
                if (script == null) continue;

                UnsubscribeFromAnyEvent(script, preset.EventName, _completePuzzle);
            }
        }

        private void SubscribeToAnyEvent(MonoBehaviour target, string eventName, Delegate handler)
        {
            if(!TryHandleCSharpEvent(subscribe: true, target, eventName, handler))
                TryHandleUnityEvent(subscribe: true, target, eventName, handler);
        }

        private void UnsubscribeFromAnyEvent(MonoBehaviour target, string eventName, Delegate handler)
        {
            if (!TryHandleCSharpEvent(subscribe: false, target, eventName, handler))
                TryHandleUnityEvent(subscribe: false, target, eventName, handler);
        }

        private bool TryHandleCSharpEvent(bool subscribe, MonoBehaviour target, string eventName, Delegate handler)
        {
            var type = target.GetType();
            var eventInfo = type.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (eventInfo == null) return false;
            if (!eventInfo.EventHandlerType.IsAssignableFrom(handler.GetType()))
            {
                Debug.LogError($"Handler type does not match C# event '{eventName}' type.");
                return false;
            }

            if (subscribe)
                eventInfo.AddEventHandler(target, handler);
            else
                eventInfo.RemoveEventHandler(target, handler);

            return true;
        }

        private bool TryHandleUnityEvent(bool subscribe, MonoBehaviour target, string eventName, Delegate handler)
        {
            var type = target.GetType();
            var field = type.GetField(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
            {
                var unityEvent = field.GetValue(target);
                if (unityEvent == null) return false;

                var listenerMethod = field.FieldType.GetMethod(subscribe ? "AddListener" : "RemoveListener");
                if (listenerMethod == null) return false;

                try
                {
                    listenerMethod.Invoke(unityEvent, new object[] { handler });
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to subscribe to UnityEvent '{eventName}': {e.Message}");
                }
            }
            return false;
        }
    }
}