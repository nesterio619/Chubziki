using Actors.AutoRepairShop;
using Core;
using Core.InputManager;
using Michsky.MUIP;
using Regions;
using System;
using Transition;
using UI;
using UI.Canvas;
using UI.Popup;
using UnityEngine;
using SceneManager = Core.SceneManager;

public class PauseMenuCanvasScreen : CanvasScreen
{
    [SerializeField] private GameObject pauseMenuContainer;
    
    [SerializeField] private ButtonManager resumeButtonManager;
    [SerializeField] private ButtonManager settingButtonManager;
    [SerializeField] private ButtonManager mainMenuButtonManager;
    [SerializeField] private ButtonManager showWelcomeMessageButtonManager;

    public static event Action OnMainMenuLoad;

    private const int Main_Menu_Scene_Index = 0;

    private static Transform _mechanicRespawnPoint;

    private void OnEnable()
    {
        InputManager.OnPauseCancel += Initialize;
        AddListenersToCanvasScreenButtons();
    }

    private void OnDisable()
    {
        InputManager.OnPauseCancel -= Initialize;
        RemoveListenersFromCanvasScreenButtons();
    }


    public override void Initialize()
    {
        if (pauseMenuContainer.activeSelf)
        {
            pauseMenuContainer.SetActive(false);
            GamepadCursor.DisplayCursor(false);
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
            pauseMenuContainer.SetActive(true);
            GamepadCursor.DisplayCursor(true);
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenuContainer.SetActive(false);
        GamepadCursor.DisplayCursor(false);
    }


    public void OpenSettingMenu(){ }


    public void LoadMainMenu()
    {
        OnMainMenuLoad?.Invoke();
        OnMainMenuLoad = null;

        ResumeGame();
        Time.timeScale = 1.0f;
        StartLoadScene();
    }
    
    public void ShowWelcomePopup()
    {
        WelcomePopup.Show();
        ResumeGame();
    }

    protected override void  AddListenersToCanvasScreenButtons()
    {
       resumeButtonManager.onClick.AddListener(ResumeGame);
       settingButtonManager.onClick.AddListener(OpenSettingMenu);
       mainMenuButtonManager.onClick.AddListener(LoadMainMenu);
       showWelcomeMessageButtonManager.onClick.AddListener(ShowWelcomePopup);
    }
    protected override void  RemoveListenersFromCanvasScreenButtons()
    {
       resumeButtonManager.onClick.RemoveAllListeners();
       settingButtonManager.onClick.RemoveAllListeners();
       mainMenuButtonManager.onClick.RemoveAllListeners();
       showWelcomeMessageButtonManager.onClick.RemoveAllListeners();
    }
    private void StartLoadScene() => 
        SceneManager.LoadScene(Main_Menu_Scene_Index, TransitionManager.LoadMode.Fade);

    public void DebugRespawnOnMechanic()
    {
        if(_mechanicRespawnPoint == null)
        {
            var mechanic = FindObjectOfType<AutoRepairShopActor>(true);
            if(mechanic == null) return;

            _mechanicRespawnPoint = mechanic.CarSpawnPoint;
        }

        Player.Instance.PlayerCarGameObject.Respawn(_mechanicRespawnPoint.position, _mechanicRespawnPoint.rotation);
        ResumeGame();
    }
}