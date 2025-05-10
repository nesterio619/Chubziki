using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System;

namespace RSM
{
    public class ConditionView
    {
        public static implicit operator VisualElement(ConditionView conditionView)
       => conditionView.root;

        public StateTransition transition;
        public string conditionName;
        public VisualElement root;
        public PopupField<string> transitionDropdown;
        public Button removeButton;
        public Action refresh;
        public StateMachineEvents.GenericEvent<ConditionView> remove;

        SerializedObject so;
        public ConditionView(StateTransition transition, string conditionName, Action refresh, SerializedObject so, StateMachineEvents.GenericEvent<ConditionView> remove = null)
        {
            this.root = new VisualElement();
            this.refresh = refresh;
            this.remove = remove;
            this.transition = transition;
            this.conditionName = conditionName;

            //this.so = so;
            if (transition.from != null) this.so = new SerializedObject(transition.from);
            else if (transition.to != null) this.so = new SerializedObject(transition.to);
            string selectedTransition = CreateConditionDropdown();
            CreateDefaultInputs(selectedTransition);
            CreateInvertButton();
            CreateRemoveButton();

            root.style.flexGrow = 1;
            root.style.paddingRight = 5;
            root.style.flexDirection = FlexDirection.Row;
        }
        

        private void CreateInvertButton()
        {

            Button notLabel = new Button();
            notLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            notLabel.style.fontSize = 16;
            notLabel.style.width = 50;
            notLabel.style.SetBorderColour(Color.clear);
            notLabel.style.color = Color.white;

            notLabel.style.backgroundColor = Color.clear;
            if (transition.ConditionIsInverted(conditionName))
            {
                notLabel.text = "NOT";
                notLabel.tooltip = "click to un-invert condition";
                notLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            }
            else
            {
                notLabel.text = "When";
                notLabel.tooltip = "click to invert condition";
            }

            notLabel.clicked += () =>
            {
                so.Update();
                so.SetProperty(GetConditionPath("conditionsInverted"), !so.GetProperty(GetConditionPath("conditionsInverted")).boolValue);
                Refresh();
            };
            root.Insert(0, notLabel);
        }

        private List<string> GetDefaultConditions()
        {
            List<string> defaultConditionNames = new List<string>() { "Never" };

            if (transition != null)
            {
                if ((!transition.HasConditionWithName("Delay") && !transition.HasConditionWithName("Delay Between"))
                    || (conditionName == "Delay" || conditionName == "Delay Between"))
                {
                    defaultConditionNames.Add("Delay");
                    defaultConditionNames.Add("Delay Between");
                }
                if ((!transition.HasConditionWithName("Cooldown") && !transition.HasConditionWithName("Cooldown Between"))
                    || (conditionName == "Cooldown" || conditionName == "Cooldown Between"))
                {
                    defaultConditionNames.Add("Cooldown");
                    defaultConditionNames.Add("Cooldown Between");
                }
            }
            return defaultConditionNames;
        }

        private void CreateRemoveButton()
        {
            removeButton = new Button();
            removeButton.tooltip = "Delete condition";
            removeButton.clicked += () =>
            {
                remove?.Invoke(this);
            };
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RapidStateMachine/Editor/Icons/DeleteIcon.png");
            removeButton.style.SetIcon(icon, 25, 6);
            removeButton.style.unityBackgroundImageTintColor = Color.grey;

            root.Add(removeButton);
        }

        private string GetTransitionPath(string propertyName)
        {
            if (transition == null) return "";
            if (transition.from == null && transition.to == null) return "";

            if (transition.from != null && transition.from.stateTransitions.Contains(transition)) return GetToTransitionPath(propertyName);
            else if (transition.to != null) return GetAnyTransitionPath(propertyName);
            return "";
        }
        private string GetToTransitionPath(string propertyName)
        {
            int transitionIndex = transition.from.stateTransitions.IndexOf(transition);
            return $"stateTransitions.Array.data[{transitionIndex}].{propertyName}";
        }
        private string GetAnyTransitionPath(string propertyName)
            => $"anyTransition.{propertyName}";

        public string GetConditionPath(string propertyName)
        {
            int conditionIndex = transition.conditionNames.IndexOf(conditionName);
            return $"{GetTransitionPath(propertyName)}.Array.data[{conditionIndex}]";
        }

        private void CreateDefaultInputs(string selectedTransition)
        {
            FloatField leftField = new FloatField();
            leftField.isDelayed = true;
            FloatField rightField = new FloatField();
            rightField.isDelayed = true;

            leftField.style.minWidth = 35;
            rightField.style.minWidth = 30;
            leftField.style.paddingLeft = 10;
            leftField.style.paddingRight = 5;
            rightField.style.paddingRight = 5;
            rightField.style.paddingLeft = 5;

            switch (selectedTransition)
            {
                case "Delay":

                    leftField.tooltip = "Delay in seconds";
                    leftField.value = so.GetProperty(GetTransitionPath("delay")).floatValue;
                    leftField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("delay"), evt.newValue);
                        Refresh();
                    }
                    );
                    root.Add(leftField);
                    break;

                case "Delay Between":

                    leftField.tooltip = "Minumum delay in seconds";
                    leftField.value = transition.minDelay;
                    leftField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("minDelay"), evt.newValue);
                        bool updateMax = evt.newValue > transition.maxDelay;
                        if (updateMax) so.SetProperty(GetTransitionPath("maxDelay"), evt.newValue);
                        Refresh();
                    }
                    );
                    root.Add(leftField);

                    rightField.tooltip = "Maximum delay in seconds";
                    rightField.value = transition.maxDelay;
                    rightField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("maxDelay"), evt.newValue >= transition.minDelay ? evt.newValue : transition.minDelay);
                        Refresh();
                    }
                    );
                    root.Add(rightField);
                    break;

                case "Cooldown":

                    leftField.tooltip = "Cooldown in seconds";
                    leftField.value = transition.cooldown;
                    leftField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("cooldown"), evt.newValue);
                        Refresh();
                    }
                    );
                    root.Add(leftField);
                    break;

                case "Cooldown Between":

                    leftField.tooltip = "Minimum cooldown in seconds";
                    leftField.value = transition.minCooldown;
                    leftField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("minCooldown"), evt.newValue);
                        bool updateMax = evt.newValue > transition.maxCooldown;
                        if (updateMax) so.SetProperty(GetTransitionPath("maxCooldown"), evt.newValue);
                        Refresh();
                    }
                    );
                    root.Add(leftField);

                    rightField.tooltip = "Maximum cooldown in seconds";
                    rightField.value = transition.maxCooldown;
                    rightField.RegisterValueChangedCallback<float>(evt =>
                    {
                        so.SetProperty(GetTransitionPath("maxCooldown"), evt.newValue >= transition.minCooldown ? evt.newValue : transition.minCooldown);
                        Refresh();
                    }
                    );
                    root.Add(rightField);
                    break;
            }
        }

        private string CreateConditionDropdown()
        {
            List<string> conditionNames = GetDefaultConditions();

            GetConditions().ForEach(c => conditionNames.Add(c));
            string selectedTransition = conditionNames[0];
            bool nameMissing = false;
            if (conditionName != null)
            {               
                selectedTransition = conditionName;
                if (!conditionNames.Contains(conditionName))
                {
                    conditionNames.Add(selectedTransition);
                    nameMissing = true;
                }
            }

            transitionDropdown = new PopupField<string>(conditionNames, selectedTransition);
            transitionDropdown.style.fontSize = 18;

            if (nameMissing)
            {           
                VisualElement textElement = transitionDropdown.Children().First().Children().First();
                transitionDropdown.tooltip
                    = $"{transition.stateMachine.behaviour.GetType()} does not contain a trigger or condition called {conditionName}";
                textElement.style.color = Color.red;  // Change text color to red
            }
            

            
            transitionDropdown.RegisterValueChangedCallback<string>(evt =>
            {
                so.Update();
                so.SetProperty(GetConditionPath("conditionNames"), evt.newValue);
                transition.ResetDelays();
                Refresh();
            });

            transitionDropdown.style.flexGrow = 1;
            root.Add(transitionDropdown);
            return selectedTransition;
        }

        private List<string> GetConditions()
        {
            List<MemberInfo> members = transition.stateMachine.behaviour
                                                .GetType()
                                                .GetMembers()
                                                .Where(m => 
                                                            (m.GetCustomAttributes(typeof(Condition), true).Any()) ||
                                                            m.GetCustomAttributes(typeof(Trigger), true).Any())
                                                .ToList();
            List<string> methodNames = members.Select(m => m.Name).ToList();
            return methodNames;
        }

        private void Refresh()
            => refresh?.Invoke();
    }
}