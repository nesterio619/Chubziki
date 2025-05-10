using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RSM
{
    [System.Serializable]
    public class StateMachine : MonoBehaviour
    {
        public IStateBehaviour behaviour;
        public UpdateType updateType;
        public RSMState currentState;
        [SerializeField] public List<RSMState> states;
        public List<StateTransition> anyTransitions;
        [HideInInspector] public List<RSMState> excluding;

        public StateMachineEvents.StateChangeEvent OnStateChange;
        public StateMachineEvents.StateChangeEvent AfterStateChange;

        private int _id = -1;
        private static int _NextID = 0;
        private bool _registered;
        public bool GetRegistered() => _registered;

        public int GetID() => _id;
        

        public void UpdateStateMachine()
        {
            if (!_registered) return;
            CheckAnyTransitions();
            CheckCurrentStateTransitions();
            Tick();
        }

        public void CheckAnyTransitions()
        {
            foreach (StateTransition transition in anyTransitions)
            {
                if (transition.to == null) continue;
                if (transition.to.excluding.Contains(currentState)) continue;
                if (transition.to == currentState) continue;
                if (transition.ShouldTransition())
                {
                    MoveToState(transition);
                    return;
                }
            }
        }

        private void CheckCurrentStateTransitions()
        {
            if (currentState == null) return;
            foreach (StateTransition transition in currentState.stateTransitions)
            {
                if (transition.ShouldTransition())
                {
                    MoveToState(transition);
                    return;
                }
            }
        }

        public void Tick()
        {
            if (currentState != null) currentState.Tick();
            else Debug.LogError("state machine missing default state", gameObject);
        }

        public void MoveToState(RSMState newRsmState)
        {
            currentState?.OnExit(newRsmState);
            RSMState previousRsmState = currentState;
            OnStateChange?.Invoke(previousRsmState, newRsmState);
            currentState = newRsmState;
            newRsmState.OnEnter(previousRsmState);
            AfterStateChange?.Invoke(previousRsmState, newRsmState);
        }

        public void MoveToState(StateTransition transition)
        {
            RSMState previousRsmState = currentState;
            OnStateChange?.Invoke(previousRsmState, transition.to);
            currentState.OnExit(transition.to);
            transition.to.OnEnter(previousRsmState);
            currentState = transition.to;
            AfterStateChange?.Invoke(previousRsmState, transition.to);
        }

        [ContextMenu("Import state")]
        public void ImportStates()
        {
            behaviour = GetStateBehaviour();
            if (behaviour == null) return;

            states?.Clear();
            foreach (Transform child in transform)
            {
                RSMState rsmState = child.GetComponent<RSMState>();
                if (rsmState == null) continue;
                if (states == null) continue;
                if (!states.Contains(rsmState)) states.Add(rsmState);
            }

            anyTransitions = new List<StateTransition>();
            excluding ??= new List<RSMState>();
            states ??= new List<RSMState>();
            foreach (RSMState state in states)
            {
                state.SetStateMachine(this);
                if (state.transitionFromAny) anyTransitions.Add(state.anyTransition);
            }

            if (currentState != null) return;
            if (states.Count <= 0) return;
            MoveToState(states[0]);
        }

        private IStateBehaviour GetStateBehaviour()
        {
            if (transform.parent == null)
            {
                Debug.LogError($"State machine requires a parent gameobject. The gameobject called {gameObject} must be a child of another gameobject." +
                    $" For more information, see page 5 of the guide \"State Machine gameobject structure\"", gameObject);
                return null;
            }

            IStateBehaviour stateBehaviour = transform.parent.GetComponent<IStateBehaviour>();
            if (stateBehaviour != null) return stateBehaviour;
            Debug.LogError($"State machine parent requires a state behaviour script. Add a script which implements IStateBehaviour to the gameobject " +
                           $"called {transform.parent}. For more information, see page 5 of the guide \"State Machine gameobject structure\"", gameObject);
            return null;
        } 

        public void RemoveTransitionsTo(RSMState to)
        {
            foreach (RSMState state in states)
            {
                state.RemoveTransitionsTo(to);
            }
        }

        public UpdateType GetUpdateType() => updateType;

        private void OnEnable()
        {            
            if (_id <= -1)
            {
                _id = _NextID;
                _NextID++;
                behaviour = transform.parent.GetComponent<IStateBehaviour>();
                ImportStates();
            }
            
            if (_registered) return;
            _registered = true;
            ConditionManager.AddBehaviour(this);

            StateMachineManager.Add(this);
        }

        private void OnDisable()
        {
            if (!_registered) return;
            _registered = false;
            StateMachineManager.Remove(this);
        }

        public void OnManagerDestroyed()
        {
            _registered = false;
        }

        private void OnDestroy()
        {
            if (!_registered) return;
            StateMachineManager.Remove(this);
        }
    }
}