using UnityEngine;
using System.Reflection;

namespace RSM
{
    [System.Serializable]
    public class StateCondition
    {
        public string conditionName;
        public MemberInfo memberInfo;
        public bool invertCondition = false;
        public bool isTrigger = false;

        public StateCondition(MemberInfo member)
        {
            memberInfo = member;
            conditionName = member.Name;

            isTrigger = memberInfo switch
            {
                FieldInfo fieldInfo => true,
                _ => false
            };
        }

        private bool IsDefaultCondition(string conditionName) 
            => conditionName is "Delay"
            or "Delay Between"
            or "Cooldown"
            or "Cooldown Between";

        public bool ConditionIsTrue(StateMachine stateMachine)
        {              
            if (IsDefaultCondition(conditionName)) return true;

            if (memberInfo == null)
            {
                Debug.LogWarning(
                    $"{stateMachine.behaviour.ToString()}, {stateMachine.currentState.name}, {conditionName} condition has no method so returns false",
                    stateMachine.gameObject);
                return false;
            }
            bool value = memberInfo switch
            {
                FieldInfo fieldInfo => (bool)fieldInfo.GetValue(stateMachine.behaviour),
                PropertyInfo propertyInfo => (bool)propertyInfo.GetValue(stateMachine.behaviour),
                _ => false
            };

            return invertCondition ? !value : value;
        }

        public void SetValue(StateMachine stateMachine,bool newValue)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(stateMachine.behaviour, newValue);
                    break;
                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue(stateMachine.behaviour, newValue);
                    break;
                default:
                    Debug.LogWarning("Cannot set value: Member is neither a field nor a writable property.");
                    break;
            }
        }
    }
}