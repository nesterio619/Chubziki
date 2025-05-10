// TerraForgeTerrainPainterModifierEditor.cs
// Provides utility functions for managing and retrieving information about terrain modifiers.
// TerraForge 2.0.0

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    /// Provides utility functions for managing and retrieving information about terrain modifiers.
    /// </summary>
    public class TerraForgeTerrainPainterModifierEditor
    {
        /// <summary>
        /// An array of all available modifier types.
        /// </summary>
        public static Type[] ModifierTypes;
        
        /// <summary>
        /// An array of all available modifier names.
        /// </summary>
        public static string[] ModifierNames;

        /// <summary>
        /// An array of GUIContent representing blend modes for use in the editor.
        /// </summary>
        public static GUIContent[] blendModesList;
        
        /// <summary>
        /// Retrieves a type by its name.
        /// </summary>
        /// <param name="name">The name of the type to retrieve.</param>
        /// <returns>The Type corresponding to the specified name, or null if not found.</returns>
        public static Type GetType(string name)
        {
            for (int i = 0; i < ModifierNames.Length; i++)
            {
                if (ModifierNames[i] == name)
                {
                    return ModifierTypes[i];
                }
            }

            return null;
        }
        
        /// <summary>
        /// Refreshes the list of available modifiers and blend modes.
        /// </summary>
        public static void RefreshModifiers()
        {
            if (ModifierTypes != null) return;
            
            string[] enums = Enum.GetNames(typeof(TerraForgeTerrainPainterModifier.BlendMode));
            blendModesList = new GUIContent[enums.Length];

            for (int i = 0; i < enums.Length; i++)
            {
                blendModesList[i] = new GUIContent(enums[i]);
            }
            
            if (ModifierTypes == null)
            {
                List<Type> exts = new List<Type>();
                List<string> names = new List<string>();
                
                var allTypes = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsAbstract) continue;

                        if (type.IsSubclassOf(typeof(TerraForgeTerrainPainterModifier)))
                            allTypes.Add(type);
                    }
                }

                foreach (Type t in allTypes)
                {
                    exts.Add(t);
                    
                    // Insert blank space in between camel case strings
                    string name = Regex.Replace(Regex.Replace(t.Name, "([a-z])([A-Z])", "$1 $2", RegexOptions.Compiled),
                        "([A-Z])([A-Z][a-z])", "$1 $2", RegexOptions.Compiled);
                    
                    names.Add(name);
                }
                
                ModifierTypes = exts.ToArray();
                ModifierNames = names.ToArray();
            }
        }
    }
}
