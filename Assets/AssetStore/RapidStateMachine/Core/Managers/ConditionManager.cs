using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RSM
{
    public class ConditionManager : RSM.MonoSingleton<ConditionManager>
    {
        private Dictionary<Type, Dictionary<string, StateCondition>> conditions;

        protected override void Initialize()
        {
            base.Initialize();
            conditions = new Dictionary<Type, Dictionary<string, StateCondition>>();
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public static StateCondition GetCondition(string conditionName, StateMachine stateMachine)
        {
            Type behaviourType = stateMachine.behaviour.GetType();
            if (Instance.conditions.TryGetValue(behaviourType, out Dictionary<string, StateCondition> conditionDict))
            {
                if (conditionDict.TryGetValue(conditionName, out StateCondition condition))
                {
                    return condition;
                }
            }
            else
            {
                conditionDict = new Dictionary<string, StateCondition>();
                Instance.conditions[behaviourType] = conditionDict;
            }

            return null;
        }

        public void AddCondition(string conditionName, StateMachine stateMachine, StateCondition condition)
        {
            Type behaviourType = stateMachine.behaviour.GetType();

            if (!conditions.TryGetValue(behaviourType, out var conditionDict))
            {
                conditionDict = new Dictionary<string, StateCondition>();
                conditions[behaviourType] = conditionDict;
            }

            conditionDict[conditionName] = condition;
        }

        public static void AddBehaviour(StateMachine stateMachine)
        {
            Type behaviourType = stateMachine.behaviour.GetType();

            if (Instance.conditions.ContainsKey(behaviourType)) return;
            Instance.conditions.Add(behaviourType, new Dictionary<string, StateCondition>());
            foreach (StateCondition condition in Instance.CreateConditions(stateMachine))
            {
                Instance.conditions[behaviourType].Add(condition.conditionName, condition);
            }
        }

        private List<StateCondition> CreateConditions(StateMachine stateMachine)
        {
            List<MemberInfo> members = stateMachine.behaviour.GetType().GetMembers()
                                                   .Where(m => (m.GetCustomAttributes(typeof(Condition), true).Any())
                                                               || m.GetCustomAttributes(typeof(Trigger), true).Any())
                                                   .ToList();

            return members.Select(member => new StateCondition(member)).ToList();
        }


        public static int GetConditionCount(StateMachine stateMachine)
        {
            Type behaviourType = stateMachine.behaviour.GetType();

            if (!Instance.conditions.ContainsKey(stateMachine.behaviour.GetType()))
            {
                Instance.conditions.Add(behaviourType, new Dictionary<string, StateCondition>());
                foreach (StateCondition condition in Instance.CreateConditions(stateMachine))
                {
                    Instance.conditions[behaviourType].Add(condition.conditionName, condition);
                }
            }            
            
            return Instance.conditions.TryGetValue(behaviourType,
                                                   out Dictionary<string, StateCondition> stateConditions)
                ? stateConditions.Count
                : 0;
        }

        public bool HasCondition(string conditionName, StateMachine stateMachine)
        {
            Type behaviourType = stateMachine.behaviour.GetType();
            return conditions.TryGetValue(behaviourType, out var conditionDict)
                   && conditionDict.ContainsKey(conditionName);
        }

        public static void Clear()
            => Instance.conditions.Clear();

        public static void Remove(StateMachine stateMachine)
            => Instance.conditions.Remove(stateMachine.behaviour.GetType());
    }
}