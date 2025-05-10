using Core;
using Core.InputManager;
using Michsky.MUIP;
using UI.Canvas.SaveSystemUI;
using UnityEngine;

namespace UI.Canvas
{
    public class MainSettingsCanvasScreen : CanvasScreen
    {
        [SerializeField] private CustomButtonController backButton;
        [SerializeField] private CustomButtonController  switchProfileButton;
       

        [Header("UI Elements")]
        [field: SerializeField] private CustomDropdown qualityDropdown;
        [field: SerializeField] private CustomDropdown resolutionDropdown;
        [field: SerializeField] private CustomToggle fullscreenToggle;
		[field: SerializeField] private CustomToggle mouseControlsToggle;
		private Resolution[] _resolutions;

        void Start()
        {
            Initialize();
        }
        
        #region  Initialization 
        public override void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;
            
            InitializeQualitySettings();
            InitializeResolutionSettings();
            InitializeFullscreenSettings();
            InitializeMouseControlsSettings();

            SceneManager.OnBeforeNewSceneLoaded_ActionList.Add(Dispose);

            AddListenersToCanvasScreenButtons();
        }

        protected override void AddListenersToCanvasScreenButtons()
        {
            backButton.onClick.AddListener(() =>  TrySwitchActiveScreenByType<MainMenuCanvasScreen>());
            switchProfileButton.onClick.AddListener(() => SaveSelectorManager.Instance.ShowBodySaveSelector(true));
            qualityDropdown.onValueChanged.AddListener(SetQuality);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            fullscreenToggle.toggleObject.onValueChanged.AddListener(SetFullscreen);
            mouseControlsToggle.toggleObject.onValueChanged.AddListener(SetMouseControls);
        }


        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            backButton.onClick.RemoveAllListeners();
            switchProfileButton.onClick.RemoveAllListeners();
            qualityDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            fullscreenToggle.toggleObject.onValueChanged.RemoveAllListeners();
            mouseControlsToggle.toggleObject.onValueChanged.RemoveAllListeners();
        }
        private void InitializeQualitySettings()
        {
            foreach (var option in qualityDropdown.items)
                qualityDropdown.RemoveItem(option.itemName, false);

            foreach (var option in QualitySettings.names)
                qualityDropdown.CreateNewItem(option, false);

            qualityDropdown.SetDropdownIndex(QualitySettings.GetQualityLevel());

            qualityDropdown.SetupDropdown();
        }

        private void InitializeResolutionSettings()
        {
            foreach (var option in resolutionDropdown.items)
                resolutionDropdown.RemoveItem(option.itemName, false);

            _resolutions = Screen.resolutions;

            int currentResolutionIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                string option = _resolutions[i].width + " x " + _resolutions[i].height;
                resolutionDropdown.CreateNewItem(option,false);

                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.SetDropdownIndex(currentResolutionIndex);
            resolutionDropdown.SetupDropdown();
        }

        private void InitializeFullscreenSettings() 
        {
            fullscreenToggle.toggleObject.isOn = Screen.fullScreen;
            fullscreenToggle.UpdateState();
        }
        private void InitializeMouseControlsSettings()
        {
            mouseControlsToggle.toggleObject.isOn = InputManager.IsContolsByMouse;
            mouseControlsToggle.UpdateState();
        } 
		#endregion
		public void SetQuality(int qualityIndex) => QualitySettings.SetQualityLevel(qualityIndex);
        public void SetFullscreen(bool isFullscreen) => Screen.fullScreen = isFullscreen;
        public void SetMouseControls(bool isControlsByMouse) => InputManager.IsContolsByMouse = isControlsByMouse;
		public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = _resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
        public void SaveSettings()
        {
            PlayerPrefs.SetInt("qualityIndex", qualityDropdown.selectedItemIndex);
            PlayerPrefs.SetInt("resolutionIndex", resolutionDropdown.selectedItemIndex);
            PlayerPrefs.SetInt("isFullscreen", fullscreenToggle.toggleObject.isOn ? 1 : 0);
			PlayerPrefs.SetInt("isMouseControls", mouseControlsToggle.toggleObject.isOn ? 1 : 0);
			PlayerPrefs.Save();
        }
        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey("qualityIndex"))
            {
                qualityDropdown.SetDropdownIndex(PlayerPrefs.GetInt("qualityIndex"));
                SetQuality(qualityDropdown.selectedItemIndex);
            }

            if (PlayerPrefs.HasKey("resolutionIndex"))
            {
                resolutionDropdown.SetDropdownIndex(PlayerPrefs.GetInt("resolutionIndex"));
                SetResolution(resolutionDropdown.selectedItemIndex);
            }

            if (PlayerPrefs.HasKey("isFullscreen"))
            {
                fullscreenToggle.toggleObject.isOn = PlayerPrefs.GetInt("isFullscreen") == 1;
                fullscreenToggle.UpdateState();
                SetFullscreen(fullscreenToggle.toggleObject.isOn);
            }

			if (PlayerPrefs.HasKey("isMouseControls"))
			{
				mouseControlsToggle.toggleObject.isOn = PlayerPrefs.GetInt("isMouseControls") == 1;
                mouseControlsToggle.UpdateState();
				SetMouseControls(fullscreenToggle.toggleObject.isOn);
			}
		}
 
    }
}