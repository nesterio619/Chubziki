using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;

namespace RSM
{
    public class StateSummaryView
    {
        public static implicit operator VisualElement(StateSummaryView stateSummary)
        => stateSummary.root;

        public RSMState rsmState;
        private VisualElement root;
        private Action refresh;
        private SerializedObject so;
        private int index;

        public StateSummaryView(RSMState rsmState, Action refresh, SerializedObject so, int index)
        {
            root = new VisualElement();
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AssetStore/RapidStateMachine/Editor/StateMachine/StateSummaryInspector.uxml");
            tree.CloneTree(root);
            this.rsmState = rsmState;
            this.refresh = refresh;
            this.so = so;
            this.index = index;
            so.Update();

            SetStateName();
            SetRenameButton();
            SetStateTransitions();
            RightClickMenu();

            SetSummaryStateButton();
            SetCurrentBorder();
            SetDeleteButton();
        }
        private void RightClickMenu()
        {
            root.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Set to Current State", (x) =>
                {
                    EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                    rsmState.stateMachine.currentState = rsmState;
                    so.Update();
                    Refresh();
                });
                evt.menu.AppendAction("Move to top", (x) =>
                {
                    EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                    Undo.RegisterChildrenOrderUndo(rsmState.stateMachine.gameObject, "stateMachine");
                    rsmState.gameObject.transform.SetAsFirstSibling();
                    Refresh();
                });
                evt.menu.AppendAction("Move up", (x) =>
                {
                    EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                    Undo.RegisterChildrenOrderUndo(rsmState.stateMachine.gameObject, "stateMachine");
                    rsmState.gameObject.transform.SetSiblingIndex(index - 1);
                    Refresh();
                });
                evt.menu.AppendAction("Move down", (x) =>
                {
                    EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                    Undo.RegisterChildrenOrderUndo(rsmState.stateMachine.gameObject, "stateMachine");
                    rsmState.gameObject.transform.SetSiblingIndex(index + 1);
                    Refresh();
                });
                evt.menu.AppendAction("Move to bottom", (x) =>
                {
                    EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                    Undo.RegisterChildrenOrderUndo(rsmState.stateMachine.gameObject, "stateMachine");
                    rsmState.gameObject.transform.SetAsLastSibling();
                    Refresh();
                });

            }));
        }

        private Label stateName;
        private void SetStateName()
        {
            stateName = root.Q<Label>("StateName");
            stateName.text = rsmState.stateMachine.states[index].name;
        }

        private void SetCurrentBorder()
        {
            if (rsmState.stateMachine.currentState != rsmState.stateMachine.states[index]) return;
            ShowCurrentBorder();
        }

        public void UpdateCurrentBorder()
        {
            if (rsmState.stateMachine.currentState != rsmState.stateMachine.states[index]) HideCurrentBorder();
            else ShowCurrentBorder();
        }

        private void ShowCurrentBorder()
        {
            Button stateButton = root.Q<Button>("StateSummaryButton");
            stateButton.style.borderLeftColor = Color.green;
            stateButton.style.borderBottomColor = Color.green;
            stateButton.style.borderLeftWidth = 1;
            stateButton.style.borderBottomWidth = 4;
        }

        private void HideCurrentBorder()
        {
            Button stateButton = root.Q<Button>("StateSummaryButton");
            stateButton.style.borderLeftWidth = 0;
            stateButton.style.borderBottomWidth = 0;
        }
        private double timeLastPressed;
        private void SetSummaryStateButton()
        {
            var summaryStateButton = root.Q<Button>("StateSummaryButton");

            summaryStateButton.clicked += () =>
            {
                if (EditorApplication.timeSinceStartup - timeLastPressed < 0.4f)
                {
                    Selection.activeGameObject = ((RSMState)so.FindProperty("states").GetArrayElementAtIndex(index).objectReferenceValue).gameObject;
                }
                timeLastPressed = EditorApplication.timeSinceStartup;
            };
        }
        private void SetStateTransitions()
        {
            VisualElement stateSummary = root.Q<VisualElement>("StateSummaryButton");
            if (rsmState.GetType() == typeof(GenericState)) stateSummary.Insert(0, new BehaviourTrackerView((GenericState)rsmState));

            Button from = root.Q<Button>("From");
            Button to = root.Q<Button>("To");
            Button any = root.Q<Button>("Any");

            int fromCount = rsmState.stateTransitions.Count;
            int anyCount = rsmState.transitionFromAny ? 1 : 0;
            int toCount = 0;

            foreach (RSMState globalState in rsmState.stateMachine.states)
            {
                foreach (StateTransition transition in globalState.stateTransitions)
                {
                    if (transition.to == rsmState) toCount++;
                }
            }

            from.text = fromCount.ToString();
            to.text = toCount.ToString();
            any.text = anyCount.ToString();
        }

        private Button renameButton;
        private TextField replaceName;
        private void SetRenameButton()
        {
            renameButton = root.Q<Button>("Rename");
            VisualElement leftContent = root.Q<VisualElement>("Left");

            replaceName = leftContent.Q<TextField>("Replace");
            ShowRename(false);
            replaceName.value = rsmState.name;
            replaceName.RegisterCallback<KeyDownEvent>(e => CancelRename(e));
            replaceName.RegisterValueChangedCallback(input => CompleteRenaming(input, leftContent, replaceName));

            renameButton.clicked += () =>
            {
                ShowRename(true);
                replaceName.SelectAll();
                replaceName.Focus();
                stateName.text = "";
            };
        }
        private void SetDeleteButton()
        {
            var deleteButton = root.Q<Button>("Delete");
            deleteButton.clicked += () =>
            {
                DeleteStateEditor.Show(rsmState);
                Refresh();
            };
        }
        private void CancelRename(KeyDownEvent test)
        {
            if (test.keyCode != KeyCode.Escape && test.keyCode != KeyCode.Return) return;
            stateName.text = rsmState.name;
            ShowRename(false);
            renameButton.Focus();
        }
        private void CompleteRenaming(ChangeEvent<string> input, VisualElement leftContent, TextField replaceName)
        {
            Undo.RecordObject(rsmState.gameObject, "change state name");
            EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
            stateName.text = input.newValue;
            rsmState.gameObject.name = input.newValue;
            ShowRename(false);
            Refresh();
        }

        private void ShowRename(bool visible)
        {
            renameButton.visible = !visible;
            stateName.visible = !visible;
            replaceName.visible = visible;
            stateName.style.width = visible ? 1 : new StyleLength(StyleKeyword.Auto);
            replaceName.style.width = visible ? new StyleLength(StyleKeyword.Auto) : 1;
        }

        private void Refresh()
            => refresh?.Invoke();
    }
}