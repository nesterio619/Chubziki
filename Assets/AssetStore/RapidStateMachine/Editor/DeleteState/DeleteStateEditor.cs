using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace RSM
{
    public class DeleteStateEditor : EditorWindow
    {
        private static DeleteStateEditor current;
        private VisualElement root;
        private VisualTreeAsset tree;
        private SerializedObject so;

        public void OnEnable()
        {
            root = new VisualElement();
            tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/RapidStateMachine/Editor/DeleteState/DeleteStateWindow.uxml");
            StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RapidStateMachine/Editor/StateStyle.uss");

            tree.CloneTree(root);
            root.styleSheets.Add(style);
        }
        public static void Show(RSMState rsmState)
        {
            if (current == null) current = new DeleteStateEditor();
            EditorWindow wnd = GetWindow<DeleteStateEditor>();
            wnd.titleContent = new GUIContent("Delete State?");
            wnd.minSize = new Vector2(350, 200);
            wnd.maxSize = new Vector2(350, 200);

            current.SetHeader(rsmState);
            current.SetDeleteBoth(rsmState);
            current.SetOnlyDeleteState(rsmState);
            current.SetCancel();

            wnd.rootVisualElement.Add(current.root);
            wnd.ShowModal();
        }

        private void SetHeader(RSMState rsmState)
        {
            Label header = root.Q<Label>("Header");
            header.text = $"Delete \"{rsmState.name}\"?";
        }

        private void SetDeleteBoth(RSMState rsmState)
        {
            Button deleteBoth = root.Q<Button>("DeleteBoth");
            so = new SerializedObject(rsmState.stateMachine.gameObject);
            deleteBoth.clicked += () =>
            {
                EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                StateMachine stateMachine = rsmState.stateMachine;

                stateMachine.RemoveTransitionsTo(rsmState);
                so.Update();
                stateMachine.states.Remove(rsmState);
                Undo.DestroyObjectImmediate(rsmState.gameObject);
                this.Close();
            };
        }

        private void SetOnlyDeleteState(RSMState rsmState)
        {
            Button onlyDeleteState = root.Q<Button>("OnlyDeleteState");
            onlyDeleteState.clicked += () =>
            {
                EditorUtility.SetDirty(rsmState.stateMachine.gameObject);
                StateMachine stateMachine = rsmState.stateMachine;

                stateMachine.states.Remove(rsmState);
                Undo.DestroyObjectImmediate(rsmState.gameObject);
                this.Close();
            };
        }

        private void SetCancel()
        {
            Button cancel = root.Q<Button>("Cancel");
            cancel.clicked += () =>
            {
                this.Close();
            };
        }
    }
}