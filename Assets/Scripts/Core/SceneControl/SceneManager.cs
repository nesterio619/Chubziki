using Core.Utilities;
using System;
using System.Collections.Generic;
using Components.Camera;
using Regions;
using Transition;
using UI.Canvas;
using UI.Debug;
using Core.SaveSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core
{
    [ExecuteAlways]
    public class SceneManager : MonoBehaviour
    {
        #region Public Events and Properties
        public static event Action OnTransitionEnd;
        public static event Action OnSceneChangeTriggered_BeforeAnimation_Event;
        public static bool IsChangingScene { get; private set; }
        public static AsyncOperation SceneLoadOperation { get; private set; }
        public static SceneConfig CurrentSceneConfig 
        {
            get
            {
                if (_currentSceneConfig == null)
                {
                    _currentSceneConfig = LoadSceneConfig(0);
                }
                return _currentSceneConfig;
            }
            private set => _currentSceneConfig = value; 
        }
        public static TransitionManager.LoadMode CurrentLoadMode;
        #endregion

        #region Private Fields
        private static CanvasManager canvasManager;
        private static bool isInitialized = false;
        private static SceneConfig _currentSceneConfig;
        public static readonly List<Action> OnBeforeNewSceneLoaded_ActionList = new();
        public static readonly List<Action> OnNewSceneLoaded_BeforeAnimation_ActionList = new();
        public static readonly List<Action> OnNewSceneLoaded_AnimationFinished_ActionList = new();
        #endregion

        #region Unity Lifecycle Methods
#if UNITY_EDITOR
        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        #endregion

        #region Editor Play Mode Handling
#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            IsChangingScene = state is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode;
        }
#endif
        #endregion

        #region Scene Management Methods
        public static void LoadScene(int sceneIndex, TransitionManager.LoadMode loadMode, List<Action> postSceneLoadActions = null)
        {
            CurrentLoadMode = loadMode;
            CurrentSceneConfig = LoadSceneConfig(sceneIndex);
            InitializeCameraAndCanvas();
            if (SceneLoadOperation != null) return;

            SceneLoadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
            SceneLoadOperation.allowSceneActivation = false;

            OnSceneChangeTriggered_BeforeAnimation_Event?.Invoke();

            if (loadMode == TransitionManager.LoadMode.None)
            {
                RegionManager.Regions.Clear();
                SceneLoadOperation.allowSceneActivation = true;
                SceneLoadOperation = null; 
                postSceneLoadActions?.ForEach(action => action?.Invoke());
                OnTransitionEnd?.Invoke();
                return;
            }
            if (postSceneLoadActions == null)
            {
                postSceneLoadActions = new List<Action>();
            }
            postSceneLoadActions.Add(ExecuteAfterEndTransitionAnimation);

            ExecuteBeforeTransitionAnimation();

            TransitionManager.StartTransition(
                loadMode,
                ExecuteAfterLoadingScene,
                postSceneLoadActions);
        }

        private static void ExecuteAfterEndTransitionAnimation()
        {
            SceneLoadOperation = null;
            OnTransitionEnd?.Invoke();
            foreach (var item in OnNewSceneLoaded_AnimationFinished_ActionList)
            {
                item?.Invoke();
            }
            OnNewSceneLoaded_AnimationFinished_ActionList.Clear();
        }

        private static void ExecuteBeforeTransitionAnimation()
        {
            foreach (var item in OnBeforeNewSceneLoaded_ActionList)
            {
                item?.Invoke();
            }
            OnBeforeNewSceneLoaded_ActionList.Clear();

            CanvasManager.Instance.LoadingText.SetActive(true);
            SaveManager.SaveProgress();
        }

        private static void ExecuteAfterLoadingScene()
        {
            SceneLoadOperation.allowSceneActivation = true;

            DebugCanvasReceiver.Instance.Dispose();

            UtilitiesProvider.WaitAndRun(() =>
            {
                foreach (var item in OnNewSceneLoaded_BeforeAnimation_ActionList)
                {
                    item?.Invoke();
                }
                OnNewSceneLoaded_BeforeAnimation_ActionList.Clear();

                CanvasManager.Instance.LoadingText.SetActive(false);
            }, true);
        }

        private static void InitializeCameraAndCanvas()
        {
            if (isInitialized) return;
            canvasManager = FindObjectOfType<CanvasManager>();

            if (canvasManager == null) Debug.LogError("CanvasManager not found in the scene. Please add it to the scene.");

            OnSceneChangeTriggered_BeforeAnimation_Event += CameraManager.Initialize;
            isInitialized = true;
        }
        #endregion

        #region Scene Configuration Loading
        private static SceneConfig LoadSceneConfig(int sceneIndex)
        {
            SceneConfigsContainer sceneConfigs = JSONParser.Load<SceneConfigsContainer>("SceneConfig.json");

            if (sceneConfigs != null && sceneConfigs.SceneConfig != null)
            {
                foreach (var config in sceneConfigs.SceneConfig)
                {
                    if (config.SceneIndex == sceneIndex)
                    {
                        return config;
                    }
                }
            }
            Debug.LogWarning($"Scene with index {sceneIndex} not found.");
            return null;
        }
        #endregion
    }
}

[System.Serializable]
public class SceneConfigsContainer
{ 
    public SceneConfig[] SceneConfig;
}

[System.Serializable]
public class SceneConfig
{
    public string SceneName;
    public int SceneIndex;
    public List<string> PrefabCanvasScreensPath;
    public string CameraPath;
}