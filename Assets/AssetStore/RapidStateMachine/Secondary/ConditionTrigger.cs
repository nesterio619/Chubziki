using UnityEngine;
namespace RSM
{
    [ExecuteInEditMode]
    public class ConditionTrigger : MonoBehaviour
    {
        public string conditionName;
        public StateMachine stateMachine;
        public TransformTarget target;

        public void Trigger()
        {
            if (conditionName == "Never")
            {
                Debug.LogError("Tried to trigger a condition trigger without a condition set", gameObject);
                return;
            }
            ConditionManager.GetCondition(conditionName, stateMachine)?.SetValue(stateMachine, true);
        }

        private void OnEnable()
        {
            GetStateMachine();
        }

        public void GetStateMachine()
        {
            StateMachine newTarget;
            switch (target)
            {
                case TransformTarget.Parent:
                    newTarget = transform.parent.GetComponent<StateMachine>();
                    break;
                case TransformTarget.Self:
                    newTarget = transform.GetComponent<StateMachine>();
                    break;
                case TransformTarget.Child:
                    newTarget = transform.GetComponentInChildren<StateMachine>();
                    break;
                case TransformTarget.Manual:
                default:
                    return;
            }
            stateMachine = newTarget;
        }
    }
}