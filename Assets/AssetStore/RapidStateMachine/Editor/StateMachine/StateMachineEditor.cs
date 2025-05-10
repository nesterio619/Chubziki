using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RSM
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private StateMachine stateMachine;
        private VisualElement root;
        private VisualTreeAsset tree;
        private SerializedObject so;
        public void OnEnable()
        {
            stateMachine = (StateMachine)target;
            stateMachine.AfterStateChange += OnStateChanged;
            if (stateMachine != null) stateMachine.ImportStates();
            so = new SerializedObject(target);
            Undo.undoRedoPerformed += Refresh;
            //Undo.RegisterChildrenOrderUndo(stateMachine.gameObject, "stateMachine");

            root = new VisualElement();
            tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AssetStore/RapidStateMachine/Editor/StateMachine/StateMachineInspector.uxml");
        }

        private void OnDisable()
        {
            stateMachine.AfterStateChange -= OnStateChanged;
            Undo.undoRedoPerformed -= Refresh;
        }

        private void OnStateChanged(RSMState from, RSMState to)
        {
            foreach (StateSummaryView summaryView in stateSummaryViews)
            {
                if (summaryView.rsmState == from) summaryView.UpdateCurrentBorder();
                if (summaryView.rsmState == to) summaryView.UpdateCurrentBorder();
            }
        }
        public override VisualElement CreateInspectorGUI()
        {
            stateMachine.ImportStates();

            root.Clear();
            tree.CloneTree(root);

            SetBehaviourName();
            SetImportButton();
            SetUpdateTypeToggle();

            SetAddStateButton();
            CreateStateSummaries();

            return root;
        }

        private void SetImportButton()
        {
            Button importButton = root.Q<Button>("Import");
            importButton.style.height = 23;

            importButton.clicked += () =>
            {
                stateMachine.ImportStates();
                so.Update();
                so.ApplyModifiedProperties();
                Refresh();
            };
        }
        private void SetBehaviourName()
        {
            IStateBehaviour behaviour = stateMachine.behaviour;
            var stateName = root.Q<Label>("BehaviourName");
            if (behaviour == null)
            {
                stateName.text = "Missing State Behaviour";
                stateName.tooltip = "Add a script which implements IStateBehaviour to the parent of this GameObject";
            }
            else stateName.text = behaviour.GetType().Name;
        }
        private void SetAddStateButton()
        {
            Button addStateButton = root.Q<Button>("AddState");
            if (stateMachine.behaviour == null)
            {
                addStateButton.parent.parent.parent.Remove(addStateButton.parent.parent);
                return;
            }
            addStateButton.clicked += () =>
            {
                EditorUtility.SetDirty(stateMachine.gameObject);
                string newStateName = GetNewStateName();
                if (newStateName == "") return;
                GameObject newState = new GameObject(newStateName);
                Undo.RegisterCreatedObjectUndo(newState, "Added new state");
                newState.AddComponent<GenericState>();
                newState.transform.parent = stateMachine.transform;
                SerializedProperty states = so.FindProperty("states");
                states.arraySize++;
                states.GetArrayElementAtIndex(states.arraySize - 1).objectReferenceValue = newState.GetComponent<GenericState>();
                so.ApplyModifiedProperties();
                Refresh();
            };
        }

        private void SetUpdateTypeToggle()
        {
            EnumField dropdown = root.Q <EnumField>("UpdateToggle");
            SerializedProperty updateType = so.FindProperty("updateType");
            dropdown.Init(stateMachine.GetUpdateType());
            dropdown.style.height = 23;
            dropdown.RegisterValueChangedCallback((evt) =>
            {
                so.Update();
                int oldType = updateType.enumValueIndex;
                if ((int)(UpdateType)evt.newValue == oldType) return;
                if (Application.isPlaying)
                {
                    Debug.LogError("Update type cannot be changed at runtime");
                    Refresh();
                    return;
                }
                updateType.enumValueIndex = (int)(UpdateType)evt.newValue;
                so.ApplyModifiedProperties();
                Refresh();
            });
        }
        private string GetNewStateName()
        {
            bool completedString = false;
            int loops = 1;
            string newStateName = "NewState";
            while (!completedString)
            {
                if (loops == 1)
                {
                    if (!HasStringWithName(newStateName)) return newStateName;
                }
                else
                {
                    if (!HasStringWithName($"{newStateName}{loops}")) return $"{newStateName}{loops}";
                }

                if (loops >= 99)
                {
                    Debug.LogError("Cannot add more than 99 states with the name NewState. Rename NewStates to allow for the creation of more.");
                    return "";
                }
                loops++;
            }
            return "";
        }
        private bool HasStringWithName(string name)
        {
            foreach (RSMState state in stateMachine.states)
            {
                if (state.name == name) return true;
            }
            return false;
        }

        List<StateSummaryView> stateSummaryViews;
        private void CreateStateSummaries()
        {
            SerializedProperty statesArray = so.FindProperty("states");
            VisualElement states = root.Q<VisualElement>("StateContainer");
            stateSummaryViews = new List<StateSummaryView>();

            for (int i = 0; i < statesArray.arraySize; i++)
            {
                StateSummaryView summaryView = new StateSummaryView((RSMState)statesArray.GetArrayElementAtIndex(i).objectReferenceValue, Refresh, so, i);
                states.Insert(states.childCount - 1, summaryView);
                stateSummaryViews.Add(summaryView);
            }
        }

        public void Refresh()
        {
            CreateInspectorGUI();
        }
    }
}