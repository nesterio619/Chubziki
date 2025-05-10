using UnityEngine;
using UnityEditor;
using System;
using Debug = UnityEngine.Debug;

namespace RSM
{
    public class IDEManager : MonoBehaviour
    {
        private static int _Line;
        public static void OpenLine(UnityEngine.Object component, string content)
        {
#if UNITY_EDITOR
            Type type = component.GetType();
            string path = $"{type.Name}.cs";

            try
            {
                _Line = GetLineNumber(component, content);
            }
            catch
            {
                Debug.Log($"failed to find {content}");
                return;
            }
            foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (!assetPath.EndsWith(path)) continue;
                MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript));
                if (script == null) continue;
                AssetDatabase.OpenAsset(script, _Line);
                break;
            }
#endif
        }
        

        private static int GetLineNumber(UnityEngine.Object component, string content)
        {
#if UNITY_EDITOR
            Type type = component.GetType();
            string path = $"{type.Name}.cs";
            foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (!assetPath.EndsWith(path)) continue;
                
                MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript));

                if (script != null)
                {
                    string[] scriptText;
                    try
                    {
                        scriptText = System.IO.File.ReadAllLines(assetPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to read script file: {ex.Message}");
                        return -1;
                    }

                    for (int i = 0; i < scriptText.Length; i++)
                    {
                        if (scriptText[i].Contains($"bool {content}") || scriptText[i].Contains($"void {content}"))
                        {
                            return i + 1;
                        }
                    }

                    Debug.Log($"'{content}' not found in {component.GetType().Name}");
                    return -1; 
                }
            }
            return -1; 
#else
            Debug.LogWarning($"GetLineNumber is only available in the Unity Editor.");
            return -1;
#endif
        }
    }
}