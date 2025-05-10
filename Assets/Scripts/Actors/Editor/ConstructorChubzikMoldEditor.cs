using Actors.Molds;
using Components.ProjectileSystem.AttackPattern;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConstructorChubzikMold), true)]
public class ConstructorChubzikMoldEditor : Editor
{
    private ConstructorChubzikMold constructorChubzikMold;

    private void OnEnable()
    {
        constructorChubzikMold = (ConstructorChubzikMold)target;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        /*serializedObject.Update();

        var property = serializedObject.FindProperty("ModelPrefabInfo");
        EditorGUILayout.PropertyField(property, true);


        EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackPatternPool"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("chubzikType"), true);

        if (constructorChubzikMold.AttackPatternPool != null)
        {
            var AttackPattern = Resources.Load(constructorChubzikMold.AttackPatternPool.ObjectPath);

            var pattern = (AttackPattern as GameObject).GetComponent<AttackPattern>();

            if (pattern is RangedAttackPattern)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StartAimingPercentFromMaxAimingDistance"), true);
            }
        }

        serializedObject.ApplyModifiedProperties();*/
    }

    protected void DrawCustomFields(params string[] fields)
    {
        foreach (var field in fields)
        {
            var property = serializedObject.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"Field with name {field} is not found. Change it's name to the appropriate class.");
                continue;
            }
            EditorGUILayout.PropertyField(property, true);
        }
    }
}
