using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;

namespace RSM
{
    [CustomEditor(typeof(ConditionTrigger))]
    public class ConditionTriggerEditor : UnityEditor.Editor
    {
        private ConditionTrigger _conditionTrigger;
        private VisualElement _root;
        private SerializedObject _so;
        public void OnEnable()
        {
            _conditionTrigger = (ConditionTrigger)target;
            _so = new SerializedObject(_conditionTrigger);

            _root = new VisualElement();
            _root.style.SetPadding(5);
            _root.style.SetBorderWidth(1);
            _root.style.marginTop = 5;
            _root.style.SetBorderColour(new Color(0.75f, 0.75f, 0, 1));
            _root.style.SetBorderRadius(5);
            _root.style.backgroundColor = new Color(.141f, .141f, .141f, 1);
            Undo.undoRedoPerformed += Refresh;

        }

        public override VisualElement CreateInspectorGUI()
        {

            TargetSelector();
            ConditionSelector();
            return _root;
        }

        private void TargetSelector()
        {
            TransformTargetElement targetElement = new TransformTargetElement(_conditionTrigger, _so);
            _root.Add(targetElement);
        }

        private PopupField<string> _transitionDropdown;
        private bool _missingTrigger;
        private List<string> _triggerNames;
        private void ConditionSelector()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            _root.Add(row);
            List<string> _conditionNames = new List<string>() { "Never" };
            _triggerNames = new List<string>();
            
            if (_conditionTrigger.stateMachine == null) return;

            SerializedProperty conditionName = _so.GetProperty("conditionName");
            _triggerNames.AddRange(GetTriggers());
            _conditionNames.AddRange(_triggerNames);
            string selectedTransition = _conditionNames[0];
            _missingTrigger = false;
            if (!string.IsNullOrEmpty(conditionName.stringValue))
            {
                if (_conditionNames.Contains(_conditionTrigger.conditionName)) selectedTransition = _conditionTrigger.conditionName;
                else
                {
                    _conditionNames.Add(_conditionTrigger.conditionName);
                    selectedTransition = _conditionTrigger.conditionName;
                    _missingTrigger = true;
                }
            }
            
            
            _transitionDropdown = new PopupField<string>(_conditionNames, selectedTransition);
            _transitionDropdown.style.fontSize = 18;
            
            UpdateTextColour();

            _transitionDropdown.formatSelectedValueCallback += (string input) =>
            {
                _so.Update();
                conditionName.stringValue = input;
                _so.ApplyModifiedProperties();
                UpdateTextColour();
                return input;
            };

            Label whenLabel = new Label("When")
                              {
                                  style =
                                  { 
                                      unityTextAlign = TextAnchor.MiddleLeft,
                                      fontSize = 16,
                                      marginLeft = 5
                                  }
                              };
            row.Add(whenLabel);
            row.Add(_transitionDropdown);
        }

        
        private List<string> GetTriggers()
        {
            List<MemberInfo> members = _conditionTrigger.stateMachine.behaviour
                                                 .GetType()
                                                 .GetMembers()
                                                 .Where(m => m.GetCustomAttributes(typeof(Trigger), true).Any())
                                                 .ToList();
            List<string> methodNames = members.Select(m => m.Name).ToList();
            return methodNames;
        }

        private void UpdateTextColour()
        {
            _missingTrigger = !_triggerNames.Contains(_conditionTrigger.conditionName);
            VisualElement textElement = _transitionDropdown.Children().First().Children().First();
            if (_missingTrigger)
            {
                _transitionDropdown.tooltip
                    = $"{_conditionTrigger.conditionName} in {_conditionTrigger.stateMachine.behaviour.GetType()} but it could not be found";
                textElement.style.color = Color.red;
            }
            else
            {
                textElement.style.color = Color.white;
            }
        }
        private void Refresh()
        {
            _transitionDropdown.value = _conditionTrigger.conditionName;
        }
    }
}