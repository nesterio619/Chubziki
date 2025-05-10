using Core;
using Core.Enums;
using Core.SaveSystem;
using System;
using Transition;
using UI.Canvas.SaveSystemUI;
using UI.Popup;
using UnityEngine;

namespace UI.Canvas
{
    public class MainMenuCanvasScreen : CanvasScreen
    {
        [SerializeField] private CustomButtonController playDevSceneButton;
        
        [SerializeField] private CustomButtonController playGameButton;
        [SerializeField] private CustomButtonController saveQuizButton;
        [SerializeField] private CustomButtonController clearQuizButton;
        [SerializeField] private CustomButtonController settingsButton;
        [SerializeField] private CustomButtonController exitButton;
       
        
        private void Start() => Initialize();

        private void OnDestroy() => Dispose();

        public override void Initialize()
        {
            if(IsInitialized)return;
            
            AddListenersToCanvasScreenButtons();
            
#if UNITY_EDITOR == FALSE
            if(playDevSceneButton != null)
                playDevSceneButton.gameObject.SetActive(false);
#endif
            
            IsInitialized = true;

            TrySwitchActiveScreen(this);
            GamepadCursor.DisplayCursor(true);
        }
        private void ContinueGame(Action callback)
        {
            if (!SaveManager.ContinueGame())
                SaveSelectorManager.Instance.ShowBodySaveSelectorFirstStart(callback);

            else callback?.Invoke();
        }
        private void LoadDevelopersScene()
        {
            SceneManager.LoadScene((int)UnityScenes.testScene, TransitionManager.LoadMode.Fade);
            GamepadCursor.DisplayCursor(false);
        }

        private void LoadFirstRegion()
        {
            SceneManager.LoadScene((int)UnityScenes.testRegion, TransitionManager.LoadMode.Fade);
            SceneManager.OnNewSceneLoaded_AnimationFinished_ActionList.Add(ShowWelcomePopup);
            GamepadCursor.DisplayCursor(false);
        }

        private void ShowWelcomePopup() 
            => WelcomePopup.ShowIfFirstTime();
        
        public void CreateExitPopup() 
            => DialoguePopup.Create("Do you really want to exit game?", Exit,parent: transform);

        private void Exit() 
            => Application.Quit();


        protected override void AddListenersToCanvasScreenButtons()
        {
            if(playDevSceneButton != null)
                playDevSceneButton.onClick.AddListener(() => ContinueGame(callback: LoadDevelopersScene));

            if (playGameButton != null)
                playGameButton.onClick.AddListener(()=> ContinueGame(callback: LoadFirstRegion));
            
            if(saveQuizButton != null)
                saveQuizButton.onClick.AddListener(QuizCanvasManager.SaveSurveyToFile);
            
            if(clearQuizButton != null)
                clearQuizButton.onClick.AddListener(QuizCanvasManager.ClearSurveyData);
            
            if(settingsButton != null)
                settingsButton.onClick.AddListener(() =>TrySwitchActiveScreenByType<MainSettingsCanvasScreen>());
            
            if(exitButton != null)
                exitButton.onClick.AddListener(Exit);
        }

        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            if(playDevSceneButton != null)
                playDevSceneButton.onClick.RemoveAllListeners();
            
            if(playGameButton != null)
                playGameButton.onClick.RemoveAllListeners();
            
            
            if(saveQuizButton != null)
                saveQuizButton.onClick.RemoveAllListeners();
            
            if(clearQuizButton != null)
                clearQuizButton.onClick.RemoveAllListeners();
            
            if(settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();
            
            if(exitButton != null)
                exitButton.onClick.RemoveAllListeners();
        }
    }
}