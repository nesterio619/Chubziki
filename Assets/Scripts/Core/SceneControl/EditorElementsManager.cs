using Actors.Constructors;
using QuestsSystem.Base;
using Regions;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Core
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class EditorElementsManager
    {
        static EditorElementsManager()
        {
            EditorSceneManager.sceneOpened += SceneOpenedCallback;
            EditorSceneManager.sceneSaving += BeforeSceneSave;
            EditorSceneManager.sceneSaved += AfterSceneSave;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (!SessionState.GetBool("FirstInitDone", false))
                EditorApplication.update += FirstSceneLoaded;
        }

        private static void FirstSceneLoaded()
        {
            CreateEditorElements(EditorSceneManager.GetActiveScene());

            EditorApplication.update -= FirstSceneLoaded;
            SessionState.SetBool("FirstInitDone", true);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                DestroyEditorElements();

            if(state == PlayModeStateChange.EnteredEditMode)
                CreateEditorElements(EditorSceneManager.GetActiveScene());
        }
        private static void BeforeSceneSave(Scene scene, string path) => DestroyEditorElements();
        private static void AfterSceneSave(Scene scene) => CreateEditorElements(scene);
        private static void SceneOpenedCallback(Scene scene, OpenSceneMode mode) => CreateEditorElements(scene);

        private static void CreateEditorElements(Scene scene)
        {
            if (Application.isPlaying) return;

            RefreshAllLocations(scene);
            QuestsManager.CreateAllQuestEditorElements();

            EditorActorConstructor.TryHideAllEditorActors();
        }
        private static void DestroyEditorElements()
        {
            EditorActorConstructor.Instance.DestroyAllEditorActors();
            QuestsManager.DestroyAllQuestEditorElements();
        }

        private static void RefreshAllLocations(Scene scene)
        {
            var locations = GameObject.FindObjectsOfType<Location>();
            
            foreach (var location in locations)
                location.ForceRefreshEditorActors();
        }
    }
#endif
}