using System;
using Core;
using Core.Utilities;
using Transition;
using UI.Popup;
using UnityEngine;

namespace UI.Canvas
{
    public class CanvasManager : MonoBehaviour
    {
        [field: SerializeField] public RectTransform DebugCanvas { get; private set; }
        [field: SerializeField] public RectTransform CarCanvas { get; private set; }
        [field: SerializeField] public RectTransform PopupCanvas { get; private set; } // TODO: rename to popup layer
        [field: SerializeField] public  RectTransform CanvasScreensLayer { get; private set; }
        [field: SerializeField] public GameObject LoadingText { get; private set; }

        private RectTransform _mainCanvas;
        public RectTransform MainCanvas => _mainCanvas ??= GetComponent<RectTransform>();

        public static CanvasManager Instance { get; private set; }

        private static bool firstLoad;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AbstractSceneTransitionScriptableObject.OnEnterCompleted  += LoadAllCanvasScreens;
            AbstractSceneTransitionScriptableObject.OnEnterCompleted  += CanvasScreen.ActivateCreatedCanvas;
        }

        private void Start()
        {
            if (!firstLoad) 
                LoadAllCanvasScreens();
        }

        private void LoadAllCanvasScreens()
        {
            firstLoad = true;
            SceneConfig sceneConfig = SceneManager.CurrentSceneConfig;
            CanvasScreen.LoadCanvasScreensForCurrentScene(sceneConfig, CanvasScreensLayer);
        }
    }

}

  