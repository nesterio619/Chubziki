using System;
using System.Collections;
using System.Collections.Generic;
using RSM;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TransformTargetElement : VisualElement
{
    public EnumField dropdown;
    public ConditionTrigger target;
    private VisualElement _row;
    
    SerializedObject so;
    public TransformTargetElement(ConditionTrigger trigger, SerializedObject so)
    {
        this.so = so;
        target = trigger;
        style.flexGrow = 1;
        style.paddingRight = 5;
        style.flexDirection = FlexDirection.Row;
        Undo.undoRedoPerformed += Refresh;

        AddDropdown();
        AddTransformField();
    }
    
    private void AddDropdown()
    {
        dropdown = new EnumField();
        dropdown.style.fontSize = 18;
        SerializedProperty targetType = so.FindProperty("target");
        dropdown.Init(target.target);

        dropdown.RegisterValueChangedCallback((evt) =>
        {
            so.Update();
            int oldType = targetType.enumValueIndex;
            if ((int)(TransformTarget)evt.newValue == oldType) return;
            targetType.enumValueIndex = (int)(TransformTarget)evt.newValue;
            so.ApplyModifiedProperties();
            Refresh();
        });
        Add(dropdown);
    }

    private ObjectField _transformField;
    private void AddTransformField()
    {
        _transformField = new ObjectField
                            {
                                style =
                                {
                                    marginLeft = 10
                                },
                                allowSceneObjects = true,
                                objectType = typeof(Transform),
                                
                            };
        Label transformLabel = _transformField.Q<Label>();
        transformLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        
        if (target.stateMachine != null) _transformField.value = target.stateMachine.transform;

        Add(_transformField);
    }

    private Label _errorLabel;
    private void ErrorTooltip()
    {        
        Label transformLabel = _transformField.Q<Label>();
        transformLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        if (target.stateMachine == null)
        {
            transformLabel.style.color = Color.red;
            _transformField.tooltip = $"Could not find a state machine in {target.target}. Please add one or change your target";
        }
        else
        {
            transformLabel.style.color = Color.white;
            _transformField.tooltip = null;

        }
    }
    void Refresh()
    {
        target.GetStateMachine();
        dropdown.Init(target.target);
        _transformField.value = target.stateMachine != null ? target.stateMachine.transform : null;
        ErrorTooltip();
    }
}
