using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace RSM
{
    [CustomEditor(typeof(GenericState))]
    public class GenericStateEditor : UnityEditor.Editor
    {
        private GenericState _state;
        private VisualElement root;
        private VisualTreeAsset tree;
        private SerializedObject so;

        public void OnEnable()
        {
            _state = (GenericState)target;
            if (_state != null) _state.stateMachine.ImportStates();
            Undo.undoRedoPerformed += Refresh;
            so = new SerializedObject(target);
            so.Update();

            root = new VisualElement();
            tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AssetStore/RapidStateMachine/Editor/GenericState/StateInspector.uxml");
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _state.stateMachine.ImportStates();
            if (_state == null) return root;

            root.Clear();
            tree.CloneTree(root);

            CreateBehaviourTracker();
            StateTitle();
            ToTransitions();
            AnyTransitions();
            if (Selection.activeGameObject.GetComponent<StateMachine>() != null) so?.Dispose();
            return root;
        }
        private void CreateBehaviourTracker()
        {
            root.Insert(1, new BehaviourTrackerView(_state));
        }

        private void StateTitle()
        {
            SetStateName();
            SetReturnButton();
        }
        private void SetStateName()
        {
            var stateName = root.Q<Label>("StateName");
            stateName.text = _state.name;
        }
        private void SetReturnButton()
        {
            var returnButton = root.Q<Button>("ReturnButton");
            returnButton.clicked += Return;
        }

        private void ToTransitions()
        {
            SetStateBehaviourIndicators();
            SetAddTransitionButton();
            CreateTransitionViews();
        }

        private List<StateView> stateViews;
        VisualElement anyTransitions;

        private void SetStateBehaviourIndicators()
        {
            var enterButton = root.Q<Button>("Enter");
            enterButton.style.backgroundColor = _state.HasEnterMethod() ? Color.green : Color.grey;
            enterButton.clicked += _state.OpenEnter;
            var tickButton = root.Q<Button>("Tick");
            tickButton.style.backgroundColor = _state.HasTickMethod() ? Color.green : Color.grey;
            tickButton.clicked += _state.OpenTick;
            var exitButton = root.Q<Button>("Exit");
            exitButton.style.backgroundColor = _state.HasExitMethod() ? Color.green : Color.grey;
            exitButton.clicked += _state.OpenExit;
        }


        private void Return()
            => Selection.activeGameObject = _state.transform.parent.gameObject;

        private void CreateTransitionViews()
        {
            VisualElement transitions = root.Q<VisualElement>("Transitions");
    
            foreach (StateTransition transition in _state.stateTransitions)
            {
                transitions.Insert(transitions.childCount - 1, new TransitionView(transition, Refresh, so));
            }
        }
        private void SetAddTransitionButton()
        {
            var addTransitionButton = root.Q<Button>("AddTransition");
            addTransitionButton.clicked += () =>
            {
                so.Update();
                EditorUtility.SetDirty(_state.stateMachine.gameObject);
                so.GetProperty("stateTransitions").arraySize += 1;
                so.ApplyModifiedProperties();
                _state.stateTransitions[so.GetProperty("stateTransitions").arraySize - 1] = new StateTransition(_state, null);
                so.Update();
                so.ApplyModifiedProperties();
                Refresh();
            };
        }

        private void AnyTransitions()
        {
            SetAddAnyTransitionButton();
            DrawAnyConditions();
            DrawExcluding();
            SetAddAnyConditionButton();
            SetAddExcludeButton();
        }
        private void SetAddAnyTransitionButton()
        {
            Button addAny = root.Q<Button>("AddAnyTransition");
            if (_state.transitionFromAny)
            {
                addAny.parent.Remove(addAny);
                return;
            }
            addAny.clicked += () =>
            {
                EditorUtility.SetDirty(_state.stateMachine.gameObject);
                // if (state.anyTransition.conditions == null) state.anyTransition.conditions = new List<StateCondition>();
                // if (state.anyTransition.to == null) state.anyTransition.to = state;
                //if (state.anyTransition.conditions.Count <= 0) state.anyTransition.conditions.Add(new StateCondition());
                so.Update();
                so.SetProperty("transitionFromAny", true);
                so.ApplyModifiedProperties();
                Refresh();
            };
        }

        private void DrawAnyConditions()
        {
            anyTransitions = root.Q<VisualElement>("AnyTransition");
            if (!_state.transitionFromAny) return;

            int count = 0;
            foreach (string anyConditionName in _state.anyTransition.conditionNames)
            {
                anyTransitions.Insert(count, new ConditionView(_state.anyTransition ,anyConditionName, Refresh, so, RemoveAnyCondition));
                count++;
            }
        }

        private void DrawExcluding()
        {
            if (_state.excluding == null) return;

            anyTransitions = root.Q<VisualElement>("AnyTransition");

            stateViews = new List<StateView>();
            List<VisualElement> excludingViews = new List<VisualElement>();
            excludingViews.Clear();

            foreach (RSMState state in _state.excluding)
            {
                VisualElement excludingElement = new VisualElement();
                excludingElement.style.flexDirection = FlexDirection.Row;
                StateView stateView = new StateView(this._state.stateMachine, state, RemoveExcluding);
                stateViews.Add(stateView);

                stateView.stateDropdown.formatSelectedValueCallback += (string input) =>
                {
                    so.Update();
                    EditorUtility.SetDirty(this._state.stateMachine.gameObject);
                    so.GetProperty("excluding").GetArrayElementAtIndex(stateViews.IndexOf(stateView)).objectReferenceValue
                    = this._state.stateMachine.states.Where(s => s.gameObject.name == input).SingleOrDefault();

                    stateView.selectedRsmState = this._state.stateMachine.states.Where(s => s.gameObject.name == input).SingleOrDefault();
                    so.ApplyModifiedProperties();
                    return input;
                };

                excludingElement.Add(stateView);
                excludingViews.Add(excludingElement);
            }


            foreach (VisualElement excludingElement in excludingViews)
            {
                anyTransitions.Insert(anyTransitions.childCount - 1, excludingElement);
            }
        }

        private void SetAddAnyConditionButton()
        {
            Button addCondition = root.Q<Button>("AddCondition");
            if (!_state.transitionFromAny)
            {
                addCondition.parent.Remove(addCondition);
                return;
            }
            addCondition.clicked += () =>
            {
                so.Update();
                EditorUtility.SetDirty(_state.stateMachine.gameObject);
                SerializedProperty names = so.FindProperty("anyTransition.conditionNames");
                SerializedProperty inversions = so.FindProperty("anyTransition.conditionsInverted");
                inversions.arraySize += 1;
                names.arraySize += 1;
                so.ApplyModifiedProperties();
                int lastIndex = names.arraySize - 1;
                names.GetArrayElementAtIndex(lastIndex).stringValue = "Never";
                inversions.GetArrayElementAtIndex(lastIndex).boolValue = false;
                so.ApplyModifiedProperties();
                Debug.Log($"count are {names.arraySize} and {inversions.arraySize}");
                Refresh();
            };
        }
        private void RemoveAnyCondition(ConditionView conditionView)
        {
            so.Update();
            EditorUtility.SetDirty(_state.stateMachine.gameObject);
            so.GetProperty("anyTransition.conditionNames").DeleteArrayElementAtIndex(_state.anyTransition.conditionNames.IndexOf(conditionView.conditionName));
            if (so.GetProperty("anyTransition.conditionNames").arraySize <= 0)
            {
                so.SetProperty("transitionFromAny", false);
                so.GetProperty("excluding").ClearArray();
            }
            so.ApplyModifiedProperties();
            Refresh();
        }
        private void SetAddExcludeButton()
        {
            Button addExclude = root.Q<Button>("AddExclude");
            if (!_state.transitionFromAny)
            {
                addExclude.parent.Remove(addExclude);
                return;
            }
            addExclude.clicked += () =>
            {
                so.Update();
                EditorUtility.SetDirty(_state.stateMachine.gameObject);
                so.FindProperty("excluding").arraySize += 1;
                so.ApplyModifiedProperties();
                Refresh();
            };
        }

        private void RemoveExcluding(StateView stateView)
        {
            so.Update();
            EditorUtility.SetDirty(this._state.stateMachine.gameObject);
            so.GetProperty("excluding").DeleteArrayElementAtIndex(_state.excluding.IndexOf(stateView.selectedRsmState));
            so.ApplyModifiedProperties();
            Refresh();
        }

        public void Refresh()
            => CreateInspectorGUI();
    }
}