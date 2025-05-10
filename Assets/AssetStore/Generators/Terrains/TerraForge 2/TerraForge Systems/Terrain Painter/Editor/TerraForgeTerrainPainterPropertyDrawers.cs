// TerraForgeTerrainPainterPropertyDrawers.cs
// Contains custom property drawers for Unity Editor.
// TerraForge 2.0.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// Contains custom property drawers for Unity Editor.
    /// </summary>
    public class TerraForgeTerrainPainterPropertyDrawers
    {
        /// <summary>
        /// Custom property drawer for <see cref="TerraForgeTerrainPainterAttributes.ResolutionDropdown"/> attribute.
        /// </summary>
        [CustomPropertyDrawer(typeof(TerraForgeTerrainPainterAttributes.ResolutionDropdown))]
        public class ResolutionDropdownAttributeDrawer : PropertyDrawer
        {
            private GUIContent[] options;

            /// <summary>
            /// Creates options for the dropdown based on the specified resolution range.
            /// </summary>
            /// <param name="minRes">Minimum resolution.</param>
            /// <param name="maxRes">Maximum resolution.</param>
            private void CreateOptions(int minRes, int maxRes)
            {
                List<GUIContent> contents = new List<GUIContent>();

                int max = minRes;

                while (max <= maxRes)
                {
                    contents.Add(new GUIContent(max + "x" + max));
                    max *= 2;
                }

                options = contents.ToArray();
            }

            /// <summary>
            /// Converts resolution value to its corresponding index in the dropdown options.
            /// </summary>
            /// <param name="resolution">Resolution value to convert.</param>
            /// <returns>Index in the dropdown options.</returns>
            private int ResToIndex(int resolution)
            {
                int index = 0;
                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i].text.Contains(resolution.ToString())) index = i;
                }

                return index;
            }

            /// <summary>
            /// Converts dropdown index to resolution value.
            /// </summary>
            /// <param name="index">Index in the dropdown options.</param>
            /// <returns>Resolution value.</returns>
            private int IndexToRes(int index)
            {
                string resString = options[index].text;

                return int.Parse(resString.Substring(0, resString.IndexOf("x")));
            }

            /// <summary>
            /// Draws the GUI for the property field using the ResolutionDropdown attribute.
            /// </summary>
            /// <param name="position">Position of the property field.</param>
            /// <param name="property">Serialized property being drawn.</param>
            /// <param name="label">Label of the property field.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                int index = 0;

                TerraForgeTerrainPainterAttributes.ResolutionDropdown range = attribute as TerraForgeTerrainPainterAttributes.ResolutionDropdown;

                CreateOptions(range.min, range.max);

                index = ResToIndex(property.intValue);

                EditorGUI.BeginProperty(position, label, property);
                position.width = EditorGUIUtility.labelWidth + 100f;
                index = EditorGUI.Popup(position, label, index, options);
                EditorGUI.EndProperty();

                property.intValue = IndexToRes(index);

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Custom property drawer for <see cref="TerraForgeTerrainPainterAttributes.ChannelPicker"/> attribute.
        /// </summary>
        [CustomPropertyDrawer(typeof(TerraForgeTerrainPainterAttributes.ChannelPicker))]
        public class ChannelPickerAttributeDrawer : PropertyDrawer
        {
            /// <summary>
            /// Draws the GUI for the property field using the ChannelPicker attribute.
            /// </summary>
            /// <param name="position">Position of the property field.</param>
            /// <param name="property">Serialized property being drawn.</param>
            /// <param name="label">Label of the property field.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                Rect rect = position;
                rect.width = 150f;
                rect.x = position.width - 75f;

                property.intValue = GUI.Toolbar(rect, property.intValue, new GUIContent[] { new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A") });

                EditorGUI.EndProperty();
            }
        }

        /// <summary>
        /// Custom property drawer for <see cref="TerraForgeTerrainPainterAttributes.MinMaxSlider"/> attribute.
        /// </summary>
        [CustomPropertyDrawer(typeof(TerraForgeTerrainPainterAttributes.MinMaxSlider))]
        public class SliderDrawer : PropertyDrawer
        {
            /// <summary>
            /// Draws the GUI for the property field using the MinMaxSlider attribute.
            /// </summary>
            /// <param name="position">Position of the property field.</param>
            /// <param name="property">Serialized property being drawn.</param>
            /// <param name="label">Label of the property field.</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (property.propertyType != SerializedPropertyType.Vector2) return;

                TerraForgeTerrainPainterAttributes.MinMaxSlider range = attribute as TerraForgeTerrainPainterAttributes.MinMaxSlider;

                EditorGUI.BeginProperty(position, label, property);

                var sliderRect = new Rect(position.x, position.y, 200, position.height);

                float minVal = property.vector2Value.x;
                float maxVal = property.vector2Value.y;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

                    minVal = EditorGUILayout.FloatField(minVal, GUILayout.Width(40f));
                    EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, range.min, range.max);
                    maxVal = EditorGUILayout.FloatField(maxVal, GUILayout.Width(40f));
                }

                property.vector2Value = new Vector2(minVal, maxVal);

                EditorGUI.EndProperty();
            }
        }
    }
}
