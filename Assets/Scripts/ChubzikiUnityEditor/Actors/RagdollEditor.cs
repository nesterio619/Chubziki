#if UNITY_EDITOR

using System.Linq;
using Components;
using Core.Enums;
using Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace ChubzikiUnityEditor.Actors
{
    [CustomEditor(typeof(RagdollComponent))]
    [CanEditMultipleObjects]
    public class RagdollEditor : Editor
    {
        private const UnityLayers _ragdollLayer = UnityLayers.Ragdoll;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RagdollComponent ragdollComponent = (RagdollComponent)target;

            EditorGUILayout.Space();

            DisplayRagdollStatus(ragdollComponent);
            
            DisplayFindRagdollPartsButton(ragdollComponent);
        
        }

        private void DisplayFindRagdollPartsButton(RagdollComponent ragdollComponent)
        {

            EditorGUI.BeginDisabledGroup(ragdollComponent.RagdollParsSet());

            if (GUILayout.Button("Find Ragdoll Parts", GUILayout.Width(300)))
            {
                foreach (var t in targets)
                {
                    RagdollComponent component = (RagdollComponent)t;
                    FindAndSetRagdollParts(component);
                    EditorUtility.SetDirty(component);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DisplayRagdollStatus(RagdollComponent ragdollComponent)
        {
            bool ragdollPartsSet = ragdollComponent.RagdollParsSet();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Ragdoll Parts Set", GUILayout.Width(150));

            EditorGUI.BeginDisabledGroup(true);

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = ragdollPartsSet ? Color.green : Color.red;

            EditorGUILayout.Toggle(ragdollPartsSet);

            GUI.backgroundColor = previousColor;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }


        private void FindAndSetRagdollParts(RagdollComponent ragdollComponent)
        {
            var parts = ragdollComponent.GetComponentsInChildren<Transform>()
                .Where(child => child.gameObject.GetObjectLayer() == _ragdollLayer)
                .Select(child => new RagdollPart(child.GetComponent<Rigidbody>(), child.GetComponent<Collider>()))
                .Where(part => part.Rigidbody != null && part.Collider != null)
                .ToArray();

            if (parts.Length > 0)
            {
                Debug.Log($"Successfully set ragdoll parts for RagdollComponent");
            }

            ragdollComponent.SetRagdollParts(parts);
        }
    }
}

#endif
