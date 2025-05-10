using Core;
using Core.SaveSystem;
using Michsky.MUIP;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI.Canvas.SaveSystemUI
{
    public class SaveSelectorManager : CanvasScreen
    {
        [SerializeField] private ButtonManager defaultSaveButton; 
        [SerializeField] private RectTransform spawnSaveLotTransform; 
        [SerializeField] private GameObject saveLotPanel; 
        [SerializeField] private TMP_InputField saveNameInput; 
        [SerializeField] private ButtonManager saveInputReaderButton; 
        [SerializeField] private GameObject prefabLot; 
        [SerializeField] private GameObject bodySaveSelector;
        [SerializeField] private GameObject deleteLotPanelPrefab;
        [SerializeField] private ButtonManager closeSelectPopupButton;
        [SerializeField] private ButtonManager closeCreatePopupButton;
        [SerializeField] private Button closeTrigger;

        private Dictionary<string, GameObject> _saveLots = new Dictionary<string, GameObject>(); 
 
        private static SaveSelectorManager _instance;
    
        private GameObject _deleteLotPanel;

        private Action _saveChosenCallback;

        public static SaveSelectorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveSelectorManager>();
                    if (_instance == null)
                    {
                        UnityEngine.Debug.LogError("SaveSelectorManager not found in the scene make sure it is present");
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); 
                return;
            }

            Instance = this; 
        }

        private void Start() => Initialize();

   
        public override void Initialize()
        {
            AddListenersToCanvasScreenButtons();
            UpdateSaveLots(); 
        }

        public void ShowBodySaveSelectorFirstStart(Action callback)
        {
            if (GetSaveFiles().Count == 0)
                ShowSaveLotPanel(true);
            else
                ShowBodySaveSelector(true);

            _saveChosenCallback = callback;
        }
        public void ShowBodySaveSelector(bool active)
        {
            if (bodySaveSelector != null)
            {
                bodySaveSelector.SetActive(active);
                closeTrigger.gameObject.SetActive(active);
            }
              
            else UnityEngine.Debug.LogWarning("Body SaveSelector was not initialized");

            if (!active)
            {   
                Destroy(_deleteLotPanel);
                saveLotPanel.SetActive(active);
            }

            _saveChosenCallback = null;
        }
        private void ShowSaveLotPanel(bool show)
        {
            saveLotPanel.SetActive(show);
            closeTrigger.gameObject.SetActive(show);

            _saveChosenCallback = null;
        }

        private void UpdateSaveLots()
        {
       
            foreach (var saveLot in _saveLots.Values)
            {
                Destroy(saveLot);
            }
            _saveLots.Clear();

        
            var saveFiles = GetSaveFiles();

       
            foreach (var saveFile in saveFiles)
            {
                CreateSaveLot(Path.GetFileNameWithoutExtension(saveFile));
            }
        }

        private List<string> GetSaveFiles()
        {
            var allFiles = ES3.GetFiles();
            var saveFiles = new List<string>();

            foreach (var file in allFiles)
            {
                if (file.EndsWith(".es3", StringComparison.OrdinalIgnoreCase) && file != SaveManager.LAST_SLOT_FILE)
                {
                    saveFiles.Add(file);
                }
            }

            return saveFiles;
        }
        private void ReadNameAndCreateLot()
        {
            string nameText = saveNameInput.text.Trim();
            if (string.IsNullOrEmpty(nameText))
            {
                UnityEngine.Debug.LogWarning("The name of the conservation cannot be empty!");
                return;
            }
            CreateNewSave(nameText);
            CreateSaveLot(nameText); 
            saveNameInput.text = "";
        
        }

        private void CreateSaveLot(string saveName)
        {
            saveName = Path.GetFileNameWithoutExtension(saveName);

            if (_saveLots.ContainsKey(saveName))
            {
                if (SaveManager.EnableSaveLoadDebugLogs) UnityEngine.Debug.LogWarning($"The slot '{saveName}' already exists!");
                return;
            }
        
        
            GameObject saveLot = Instantiate(prefabLot, spawnSaveLotTransform);
            SaveLotButtonContainer buttons = saveLot.GetComponent<SaveLotButtonContainer>();

            if (buttons != null)
            {
                InitializeSaveButton(buttons.SaveLotButton,saveName);
                InitializeDeleteButton(buttons.DeleteLotButton,saveName);
            }
            else UnityEngine.Debug.LogError("Buttons = null");

            _saveChosenCallback?.Invoke();
            _saveChosenCallback = null;

            _saveLots.Add(saveName, saveLot);
            ShowBodySaveSelector(false);
        }
        private void LoadSave(string saveName)
        {
            SaveManager.SetCurrentSaveSlot(saveName);
        
            bool loaded = SaveManager.LoadProgress();

            if (loaded)
            {
               if (SaveManager.EnableSaveLoadDebugLogs)UnityEngine.Debug.Log($"Save load: {saveName}");
                Player.Instance.ApplyLoadedProgress();
                SaveManager.SaveProgress();

                _saveChosenCallback?.Invoke();
                _saveChosenCallback = null;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Safe loading error: {saveName}");
            }
        }


        private void CreateNewSave(string saveName)
        {
            SaveManager.SetCurrentSaveSlot(saveName);
            SaveManager.ResetProgress();
            SaveManager.SaveProgress();
            if (SaveManager.EnableSaveLoadDebugLogs) UnityEngine.Debug.Log($" Create new save {saveName}");
        }
        private void CreateDeletePanel(string saveName)
        {
            _deleteLotPanel = Instantiate(deleteLotPanelPrefab, transform);
            DeleteLotButtonsContainer buttons = _deleteLotPanel.GetComponent<DeleteLotButtonsContainer>();

            if (buttons != null)
            {
                buttons.ConfirmButton.onClick.AddListener(() =>
                {
                    DeleteSave(saveName);
                    Destroy(_deleteLotPanel);
                });

                buttons.CanelButton.onClick.AddListener(() => Destroy(_deleteLotPanel));
            }
            else
            {
                UnityEngine.Debug.LogError("Buttons = null");
            }
        }

        private void InitializeSaveButton( ButtonManager buttonManager, string saveName)
        {
            buttonManager.buttonText = saveName;
            buttonManager.normalText.text = saveName;
            buttonManager.highlightedText.text = saveName;
            buttonManager.disabledText.text = saveName;
      
            buttonManager.onClick.AddListener(() => LoadSave(saveName));
            buttonManager.onClick.AddListener(() => ShowBodySaveSelector(false));
        }

        private void InitializeDeleteButton(ButtonManager buttonManager, string saveName)
        {
            buttonManager.onClick.AddListener(() => CreateDeletePanel(saveName));
        }
        protected override void AddListenersToCanvasScreenButtons()
        {
            defaultSaveButton.onClick.AddListener(() => ShowSaveLotPanel(true)); 
            saveInputReaderButton.onClick.AddListener(ReadNameAndCreateLot); 
            closeSelectPopupButton.onClick.AddListener(()=> ShowBodySaveSelector(false));
            closeTrigger.onClick.AddListener(() => ShowBodySaveSelector(false));
            closeCreatePopupButton.onClick.AddListener(() => ShowSaveLotPanel(false));
        }

        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            defaultSaveButton.onClick.RemoveAllListeners(); 
            saveInputReaderButton.onClick.RemoveAllListeners(); 
            closeSelectPopupButton.onClick.RemoveAllListeners();
            closeCreatePopupButton.onClick.RemoveAllListeners();
            closeTrigger.onClick.RemoveAllListeners();
        }
        private void DeleteSave(string saveName)
        {
            if (_saveLots.ContainsKey(saveName))
            {
                string saveFilePath = $"{saveName}.es3";
                if (ES3.FileExists(saveFilePath))
                {
                    ES3.DeleteFile(saveFilePath);
                    if (SaveManager.EnableSaveLoadDebugLogs)  UnityEngine.Debug.Log($"Save deleted: {saveName}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Save file not found: {saveName}");
                }
            
                Destroy(_saveLots[saveName]);
                _saveLots.Remove(saveName);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Save lot not found: {saveName}");
            }
        }
        private void OnDestroy()
        {
            RemoveListenersFromCanvasScreenButtons();
        }
    }
}

