#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Regions.Editors
{
    public class SceneExceptionFinderEditorWindow : EditorWindow
    {
        private static List<Object> _faultyObjects;
        private static int _index;

        private static int _count;

        private Button _nextButton;
        private Button _previousButton;
        private Label _countLabel;

        private static SceneExceptionFinderEditorWindow _instance;

        public static void Show(List<Object> faultyObjects)
        {
            _faultyObjects = faultyObjects;
            _index = -1;
            _count = faultyObjects.Count;

            _instance = GetWindow<SceneExceptionFinderEditorWindow>();

            var size = new Vector2(280,130);
            _instance.minSize = size;
            _instance.maxSize = size;

            _instance.titleContent = new GUIContent("SceneBaker Reference fixer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var horizontalContainerTop = new VisualElement();
            horizontalContainerTop.style.flexDirection = FlexDirection.Row;
            horizontalContainerTop.style.alignSelf = Align.Center;
            root.Add(horizontalContainerTop);

            var iconImage = new Image();
            var icon = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
            iconImage.style.backgroundImage = new StyleBackground(icon);
            iconImage.style.width = 32;
            iconImage.style.height = 32;
            horizontalContainerTop.Add(iconImage);

            Label bigLabel = new Label("Empty references found!");
            bigLabel.style.fontSize = 20;
            bigLabel.style.alignSelf = Align.Center;
            horizontalContainerTop.Add(bigLabel);

            var spaceTop = new VisualElement();
            spaceTop.style.height = 10;
            root.Add(spaceTop);

            Label smallLabel = new Label("Please fix them before entering play mode.");
            smallLabel.style.alignSelf = Align.Center;
            root.Add(smallLabel);

            var spaceBot = new VisualElement();
            spaceBot.style.height = 20;
            root.Add(spaceBot);

            _countLabel = new Label(_count + " objects to fix");
            _countLabel.style.alignSelf = Align.Center;
            root.Add(_countLabel);

            var horizontalContainerBot = new VisualElement();
            horizontalContainerBot.style.flexDirection = FlexDirection.Row;
            horizontalContainerBot.style.alignSelf = Align.Center;
            root.Add(horizontalContainerBot);

            _previousButton = new Button(Previous);
            _previousButton.text = "Previous";
            _previousButton.SetEnabled(false);
            horizontalContainerBot.Add(_previousButton);

            _nextButton = new Button(Next);
            _nextButton.text = "Start";
            horizontalContainerBot.Add(_nextButton);
        }

        private void Next()
        {
            _index++; 
            
            if (_index == _count)
            {
                _instance.Close();
                return;
            }

            if (_index > 0) _previousButton.SetEnabled(true);

            ShowObject();
        }

        private void Previous()
        {
            _index--;

            if (_index == 0) _previousButton.SetEnabled(false);

            ShowObject();
        }

        private void ShowObject()
        {
            var faultyObject = _faultyObjects[_index];

            EditorGUIUtility.PingObject(faultyObject);
            Selection.activeObject = faultyObject;

            _nextButton.text = _index == _count - 1 ? "Close window" : "Next";
            _countLabel.text = $"{_index + 1}/{_count} - {faultyObject.name}";
        }
    }
}
#endif