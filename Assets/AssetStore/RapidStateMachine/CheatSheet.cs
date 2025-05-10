using System;
using UnityEngine;
using RSM;
// These includes are required for StateBehaviours

namespace RSM
{
    public class CheatSheet : MonoBehaviour, IStateBehaviour
    {
        //STATE BEHAVIOUR METHODS
        //Replace "State" in "EnterState" to match the name of the State.
        [State] private void EnterState()
        {
            //your code here
        }
        //Replace "State" to match the name of the State.
        [State] private void State()
        {
            //your code here
        }
        //Replace "State" in "ExitState" to match the name of the State.
        [State] private void ExitState()
        {
            //your code here
        }

        //CONDITIONS
        //replace "YourCondition" with the name of your condition
        //replace "true" with a bool that determines your condition, e.g. a raycast that checks if the character is grounded

        [Condition] public bool YourCondition => true;
        
        //Expanded Condition
        //replace "YourExpandedCondition" with the name of your condition
        //replace "true" with a bool that determines your condition, e.g. a raycast that checks if the character is grounded
        
        [Condition]
        public bool YourExpandedCondition
        {
            get
            {
                //Additional logic goes here
                return true;
            }
        }
        
        //TRIGGERS
        //replace "YourTrigger" with the name of your trigger

        [Trigger] public bool YourTrigger;

        public void ManipulatingTriggers() //example method, normally you'd trigger Triggers via Update() or state behaviour methods.
        {
            //activates a trigger
            YourTrigger.Trigger();

            //deactivates a trigger
            YourTrigger.Cancel();
        }

        private RSMState _localState;
        private StateMachine _stateMachine;
        public void OnEnable()
        {
            _localState = GetComponent<RSMState>();
            _stateMachine = transform.parent.GetComponent<StateMachine>();
            _stateMachine.OnStateChange += MyMethod;
        }
        public void OnDisable()
        {
            _stateMachine.OnStateChange += MyMethod;
        }
        public void MyMethod(RSMState from, RSMState to)
        {
            if (to != _localState) return;
        }
    }
}