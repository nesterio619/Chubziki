using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace RSM
{
    [System.Serializable]
    public class StateTransition
    {
        public StateMachine stateMachine;
        public RSMState from;
        public RSMState to;
        [FormerlySerializedAs("conditions")]
        [SerializeField] private List<StateCondition> _conditions;
        public List<string> conditionNames;
        public List<bool> conditionsInverted;
        public bool muted;
        public float timeLastPassed = 0;

        public float delay = 0;
        public float minDelay = 0;
        public float maxDelay = 0;

        public float cooldown = 0;
        public float minCooldown = 0;
        public float maxCooldown = 0;

        public StateTransition(RSMState from, RSMState to, List<StateCondition> conditions = null, string test = "")
        {
            this.from = from;
            this.to = to;
            _conditions = new List<StateCondition>();
            conditionNames = new List<string>();
            conditionsInverted = new List<bool>();
            if (conditions == null) return;
            
            foreach (StateCondition condition in conditions)
            {
                _conditions.Add(condition);
                conditionNames.Add(condition.conditionName);
            }
        }

        public bool ShouldTransition()
        {
            if (muted) return false;
            if (!HasConditions) return false;
            if (!DelayComplete) return false;
            if (!CooldownComplete) return false;
            
            int index = -1;
            List<StateCondition> triggers = new List<StateCondition>();
            foreach (string condition in conditionNames)
            {               
                index++;
                if (IsDefaultCondition(condition)) continue;
                StateCondition stateCondition = ConditionManager.GetCondition(condition, stateMachine);
                if (stateCondition == null)
                {
                    Debug.LogError($"{stateMachine.behaviour.GetType()} is missing a condition or trigger called {condition}");
                    return false;
                }
                bool conditionWasTrue = stateCondition.ConditionIsTrue(stateMachine);
                if(stateCondition.isTrigger) triggers.Add(stateCondition);

                if (conditionsInverted[index]) conditionWasTrue = !conditionWasTrue;
                if (conditionWasTrue) continue;
                return false;
            }

            foreach (StateCondition StateCondition in triggers)
            {
                StateCondition.SetValue(stateMachine, false);
            }

            ResetDelays();
            timeLastPassed = Time.time;
            return true;
        }

        
        public void ResetDelays()
        {
            if (!HasConditionWithName("Delay")) delay = 0;
            if (HasConditionWithName("Delay Between")) delay = UnityEngine.Random.Range(minDelay, maxDelay);
            else
            {
                minDelay = 0;
                maxDelay = 0;
            }
            if (!HasConditionWithName("Cooldown")) cooldown = 0;
            if (HasConditionWithName("Cooldown Between")) cooldown = UnityEngine.Random.Range(minCooldown, maxCooldown);
            else
            {
                minCooldown = 0;
                maxCooldown = 0;
            }
        }

        private bool HasConditions => ConditionManager.GetConditionCount(stateMachine) > 0;
        private int FindConditionIndex(params string[] conditionNamesToCheck) 
            => conditionNames.FindIndex(conditionNamesToCheck.Contains);
        private bool IsConditionComplete(int index, float time, float threshold) 
            => index == -1 || !conditionsInverted[index] == (time <= 0 || threshold >= time);
        private bool DelayComplete 
            => IsConditionComplete(FindConditionIndex("Delay", "Delay Between"), delay, stateMachine.currentState.inStateFor);
        private bool CooldownComplete 
            => IsConditionComplete(FindConditionIndex("Cooldown", "Cooldown Between"), cooldown, Time.time - timeLastPassed);

        private bool IsDefaultCondition(string conditionName) 
            => conditionName is "Delay"
                                or "Delay Between"
                                or "Cooldown"
                                or "Cooldown Between";

        public bool HasConditionWithName(string name)
        {
            return conditionNames.Any(condition => condition == name);
        }

        public bool ConditionIsInverted(string name)
        {
            return HasConditionWithName(name) && conditionsInverted[conditionNames.IndexOf(name)];
        }
        public void SetStateMachine(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            if (_conditions == null || _conditions.Count <= 0)
            {
                if (conditionsInverted.Count == conditionNames.Count) return;
                while (conditionsInverted.Count < conditionNames.Count)
                {
                    conditionsInverted.Add(false);
                }
            }
            else
            {
                conditionsInverted = new List<bool>();
                foreach (StateCondition condition in _conditions)
                {
                    conditionsInverted.Add(condition.invertCondition);
                    if (conditionNames.Contains(condition.conditionName)) continue;
                    conditionNames.Add(condition.conditionName);
                }
            }
            _conditions?.Clear();
        }

    }
}