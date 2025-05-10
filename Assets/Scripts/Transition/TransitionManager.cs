using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UI;
using Core.Utilities;

namespace Transition
{
    public static class TransitionManager
    {
        private static Dictionary<LoadMode, AbstractSceneTransitionScriptableObject> transitions = new();
        private static AbstractSceneTransitionScriptableObject currentTransition;

        private static Canvas _transitionCanvas;
        public static Image _transitionImage;

        public static bool IsTransitioning => _transitionCanvas.enabled;

        private const string TransitionCanvasPath = "Prefabs/SceneTransition";

        public enum LoadMode
        {
            None,
            Scale,
            Fade
        }

        public static void Initialize()
        {
            if (_transitionCanvas != null && _transitionImage != null) return;

            if (AssetUtils.TryLoadAsset(TransitionCanvasPath, out GameObject transitionCanvasPrefab))
            {
                GameObject instanceObject = UnityEngine.Object.Instantiate(transitionCanvasPrefab);
                _transitionCanvas = instanceObject.GetComponent<Canvas>();
                _transitionImage = instanceObject.GetComponentInChildren<Image>();

                UnityEngine.Object.DontDestroyOnLoad(instanceObject);

                _transitionCanvas.enabled = false;
                
                if (_transitionImage != null) _transitionImage.color = new Color(0, 0, 0, 0);
                
                if (transitions.Count == 0) InitializeTransitions();
            }
            else
            {
                Debug.LogError("Failed to load TransitionCanvas prefab.");
            }
        }

        private static void InitializeTransitions()
        {
            AbstractSceneTransitionScriptableObject transfer = null;

            if (AssetUtils.TryLoadAsset("ScriptableObjects/Transitions/Fade", out transfer) && !transitions.ContainsKey(LoadMode.Fade))
            {
                transitions.Add(LoadMode.Fade, transfer);
            }

            if (AssetUtils.TryLoadAsset("ScriptableObjects/Transitions/CircleScale", out transfer) && !transitions.ContainsKey(LoadMode.Scale))
            {
                transitions.Add(LoadMode.Scale, transfer);
            }
        }

        public static void StartTransition(LoadMode loadMode, Action afterFirstAnimation,
            List<Action> postTransitionActions, bool expectSceneLoad = true)
        {
            if (!transitions.TryGetValue(loadMode, out currentTransition) || currentTransition == null)
            {
                Debug.LogError("Can't find transition: " + loadMode);
                return;
            }

            if (_transitionCanvas == null || _transitionImage == null)
            {
                Debug.LogError("TransitionCanvas or TransitionImage is not initialized.");
                return;
            }
            _transitionImage.color = Color.black;
            _transitionCanvas.enabled = true;
            currentTransition.InitializeAnimatedObject(_transitionImage); 
            Player.Instance.StartCoroutine(ExecuteTransition(loadMode, afterFirstAnimation, postTransitionActions, expectSceneLoad));
        }

        private static IEnumerator ExecuteTransition(LoadMode loadMode, Action afterFirstAnimation,
            List<Action> postTransitionActions, bool expectSceneLoad)
        {
            if (currentTransition == null)
            {
                Debug.LogError("Current transition is not set.");
                yield break;
            }
            
            yield return Player.Instance.StartCoroutine(currentTransition.Enter(expectSceneLoad));

            afterFirstAnimation?.Invoke();

            if (expectSceneLoad)
            {
                AsyncOperation sceneLoadOperation = SceneManager.SceneLoadOperation;
                if (sceneLoadOperation != null)
                {
                    while (!sceneLoadOperation.isDone)
                    {
                        yield return null;
                    }
                }
                else
                {
                    Debug.LogWarning("SceneLoadOperation is null, but scene load was expected.");
                }
            }

            yield return new WaitForSeconds(0.5f);
            
            yield return Player.Instance.StartCoroutine(currentTransition.Exit());

            _transitionCanvas.enabled = false;

            yield return new WaitForEndOfFrame();
            if (postTransitionActions != null)
            {
                foreach (var action in postTransitionActions)
                {
                    action?.Invoke();
                }
            }
        }
    }
}