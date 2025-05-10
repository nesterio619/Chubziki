using System.Collections.Generic;
using Core;
using Transition;
using UnityEngine;

namespace UI.Canvas
{
    public abstract class CanvasScreen : MonoBehaviour, System.IDisposable
    {
        #region Fields
        [SerializeField] protected UnityEngine.Canvas CanvasReference;
        protected bool IsInitialized;
        private static List<CanvasScreen> existingCanvasScreens = new List<CanvasScreen>();
        #endregion

        #region Properties
        public static CanvasScreen ActiveCanvasScreen { get; set; }
        #endregion
        
        

        #region Initialization and Lifecycle
        public static void LoadCanvasScreensForCurrentScene(SceneConfig sceneConfig, RectTransform canvasScreenLayer)
        {
            List<string> paths = LoadCanvasScreensForScene(sceneConfig);
            DestroyCreatedCanvas();
            foreach (var pathToCanvasScreen in paths)
            {
                var prefab = Resources.Load<GameObject>(pathToCanvasScreen);
                if (prefab == null) UnityEngine.Debug.LogError($"Failed to load prefab");
                if (canvasScreenLayer == null) UnityEngine.Debug.LogError("Failed to find a valid parent layer.");
                CreateAndConfigureCanvasScreen(canvasScreenLayer, prefab);
            }
        }

        public virtual void Dispose()
        {
            RemoveListenersFromCanvasScreenButtons();
        }

        private static void DestroyCreatedCanvas()
        {
            foreach (var canvas in existingCanvasScreens)
            {
                Destroy(canvas.gameObject);
            }
            existingCanvasScreens.Clear();
        }

        public abstract void Initialize();
        #endregion

        #region Screen Management
        public static bool TrySwitchActiveScreen(CanvasScreen screenToSet)
        {
            if (screenToSet == null)
            {
                UnityEngine.Debug.LogError("Screen == null");
                return false;
            }
            if (screenToSet == ActiveCanvasScreen)
            {
                UnityEngine.Debug.LogError("Same screen " + ActiveCanvasScreen.gameObject.name);
                return false;
            }
            SwitchActiveScreen(screenToSet);
            return true;
        }

        public static bool TrySwitchActiveScreenByType<T>() where T : CanvasScreen
        {
            foreach (var canvasScreen in existingCanvasScreens)
            {
                if (canvasScreen is T)
                {
                    SwitchActiveScreen(canvasScreen);
                    return true;
                }
            }
            UnityEngine.Debug.LogError("Not found Canvas Screen");
            return false;
        }

        public static void ActivateCreatedCanvas()
        {
            foreach (var canvas in existingCanvasScreens)
            {
                canvas.gameObject.SetActive(true);
            }
        }

        private static void SwitchActiveScreen(CanvasScreen screenToSet)
        {
            CanvasScreen oldScreen = ActiveCanvasScreen;
            if (oldScreen != null)
            {
                oldScreen.SwitchActive(false);
            }
            //screenToSet.AddListenersToCanvasScreenButtons();
            screenToSet.SwitchActive(true);
            ActiveCanvasScreen = screenToSet;
        }

        private void SwitchActive(bool enable)
        {
            if (CanvasReference != null)
            {
                CanvasReference.enabled = enable;
            }
        }
        #endregion

        #region Canvas Creation and Configuration
        private static void CreateAndConfigureCanvasScreen(RectTransform canvasScreenLayer, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, canvasScreenLayer);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            if (SceneManager.CurrentLoadMode == TransitionManager.LoadMode.Fade)
            {
                instance.SetActive(false);
            }
            existingCanvasScreens.Add(instance.GetComponent<CanvasScreen>());
        }

        private static List<string> LoadCanvasScreensForScene(SceneConfig sceneConfig)
        {
            List<string> prefabPaths = new List<string>();
            if (sceneConfig != null)
            {
                foreach (var path in sceneConfig.PrefabCanvasScreensPath)
                {
                    prefabPaths.Add(path);
                }
            }
            else UnityEngine.Debug.LogError("Error loads prefabs paths");
            return prefabPaths;
        }
        #endregion

        #region Other Abstract Contract
        protected abstract void AddListenersToCanvasScreenButtons();
        protected abstract void RemoveListenersFromCanvasScreenButtons();
        #endregion
    }
}