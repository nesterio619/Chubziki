#if UNITY_EDITOR
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using UnityEditor;
using UnityEngine;

namespace ChubzikiUnityEditor.EditorWindows
{
    public class FastCopyQuestWindow : EditorWindow
    {
        // Start is called before the first frame update
        private Object _SOConfigToCopy = null;

        private Object _SOConfigFromCopy = null;

        [MenuItem("Chubziki/FastCopyQuestConfig")]
        public static void ShowWindow()
        {
            GetWindow<FastCopyQuestWindow>("FastCopyQuestConfig");
        }

        private void OnGUI()
        {
            GUILayout.Label("From copy config");
            _SOConfigFromCopy = EditorGUILayout.ObjectField("Pooled object", _SOConfigFromCopy, typeof(QuestConfig), false);

            GUILayout.Label("To copy config");
            _SOConfigToCopy = EditorGUILayout.ObjectField("Pooled object", _SOConfigToCopy, typeof(QuestConfig), false);

            GUILayout.Space(10f);

            if (GUILayout.Button("Copy config"))
            {
                if (_SOConfigToCopy == null || _SOConfigFromCopy == null)
                {
                    ShowNotification(new GUIContent("Incorrect data in fields"));
                    return;
                }
                CopyConfig();
            }
        }

        private void CopyConfig()
        {

            foreach (var item in (_SOConfigFromCopy as QuestConfig).QuestElementsAndTransformsPaths)
            {
                (_SOConfigToCopy as QuestConfig).QuestElementsAndTransformsPaths.Add(item);
            }
        }

    }
}
#endif