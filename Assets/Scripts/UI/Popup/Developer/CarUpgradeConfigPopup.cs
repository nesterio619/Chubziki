using Michsky.MUIP;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Utilities;
using Core.ObjectPool;
using UI.Canvas;
using Core;
using Actors;
using Upgrades;
using Upgrades.CarUpgrades;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.Popup.Developer
{
    public class CarUpgradeConfigPopup : PopupController
    {
        private UpgradesList _mainUpgradeController;

        [SerializeField]
        private CustomInputField inputFieldPrefab;

        [SerializeField]
        private TMP_Text textPrefab;

        #region Objects On Scene

        [SerializeField] private RectTransform placeForNames;
        [SerializeField] private RectTransform placeForInputBaseValue;
        [SerializeField] private RectTransform placeForInputUpgradePerValue;
        [SerializeField] private RectTransform placeForTotalValue;

        [SerializeField] private CustomDropdown upgradeDropdown;

        [SerializeField] private CustomInputField simulatedUpgradeLevel;

        [SerializeField] private ButtonManager resetConfigButton;
        [SerializeField] private ButtonManager saveConfigButton;
        [SerializeField] private ButtonManager closeButton;

        #endregion

        #region Stored Fields
        private List<TMP_Text> _textNames = new();
        private List<CustomInputField> _inputBaseValues = new();
        private List<CustomInputField> _inputUpgradePerValues = new();
        private List<TMP_Text> _textTotalValue = new();
        #endregion

        private int _currentUpgradeIndex = 0;

        private CarUpgradeBase _currentUpgrade;

        private UpgradesList _resetUpgradeList;

        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/StatsCreationPopupReworkPoolInfo";
        private static PrefabPoolInfo _popup_PrefabPoolInfo;

        private static bool _isCreated = false;

        public static void CreateWithoutTransform() => Create();

        public static void Create(Transform parent = null)
        {
            if(_isCreated)
                return;
            
            if (parent == null)
                parent = CanvasManager.Instance.PopupCanvas;

            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out _popup_PrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(_popup_PrefabPoolInfo, parent).GetComponent<CarUpgradeConfigPopup>();

            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;

            popup._mainUpgradeController = Player.Instance.PlayerCarGameObject.GetComponent<CarActor>().GetCarUpgrades();

            popup._resetUpgradeList = new();

            for (int i = 0; i < popup._resetUpgradeList.CarUpgradesList.Count; i++)
                popup._resetUpgradeList.CarUpgradesList[i].CopyUpgrades(popup._mainUpgradeController.CarUpgradesList[i]);

            if (popup != null)
                popup.Initialize();
        }

        private void Initialize()
        {
            _isCreated = true;
            
            while (upgradeDropdown.items.Count != 0)
            {
                upgradeDropdown.items.RemoveAt(0);
            }

            resetConfigButton.onClick.AddListener(ResetToLastNumbers);
            saveConfigButton.onClick.AddListener(SaveMainUpgradesController);
            closeButton.onClick.AddListener(ReturnToPool);


            upgradeDropdown.CreateNewItem("Speed");
            upgradeDropdown.CreateNewItem("Power");
            upgradeDropdown.CreateNewItem("Control");
            upgradeDropdown.CreateNewItem("Survive", true);

            upgradeDropdown.onValueChanged.AddListener(SwitchUpgrade);

            SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[(int)UpgradesList.Upgrades.Speed]);

            simulatedUpgradeLevel.inputText.onValueChanged.AddListener(OnUpgradeIndexChanged);

        }

        public void SwitchUpgrade(int dropdownValue)
        {
            _currentUpgradeIndex = dropdownValue;

            switch (dropdownValue)
            {
                case 0:
                    SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[(int)UpgradesList.Upgrades.Speed]);
                    break;
                case 1:
                    SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[(int)UpgradesList.Upgrades.Power]);
                    break;
                case 2:
                    SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[(int)UpgradesList.Upgrades.Control]);
                    break;
                case 3:
                    SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[(int)UpgradesList.Upgrades.Survive]);
                    break;
            }
        }

        public void SetCurrentUpgrade(CarUpgradeBase mainUpgrade)
        {
            _currentUpgrade = mainUpgrade;

            for (int i = 0; i < _textNames.Count; i++)
            {


                Destroy(_textNames[i].gameObject);
                _inputBaseValues[i].inputText.onValueChanged.RemoveAllListeners();
                Destroy(_inputBaseValues[i].gameObject);
                _inputUpgradePerValues[i].inputText.onValueChanged.RemoveAllListeners();
                Destroy(_inputUpgradePerValues[i].gameObject);
                Destroy(_textTotalValue[i].gameObject);
            }

            _textNames.Clear();
            _inputBaseValues.Clear();
            _inputUpgradePerValues.Clear();
            _textTotalValue.Clear();

            int index = 0;

            foreach (var item in _currentUpgrade.CarUpgradesDictionary)
            {
                _textNames.Add(Instantiate(textPrefab, placeForNames));
                _textNames[index].text = item.Key;

                _inputBaseValues.Add(Instantiate(inputFieldPrefab, placeForInputBaseValue));
                _inputBaseValues[index].inputText.text = item.Value.BaseValue.ToString();
                _inputBaseValues[index].inputText.onValueChanged.AddListener((inputText) => UpdateValues());
                _inputBaseValues[index].UpdateStateInstant();

                _inputUpgradePerValues.Add(Instantiate(inputFieldPrefab, placeForInputUpgradePerValue));
                _inputUpgradePerValues[index].inputText.text = item.Value.ValuePerUpgrade.ToString();
                _inputUpgradePerValues[index].inputText.onValueChanged.AddListener((inputText) => UpdateValues());
                _inputUpgradePerValues[index].UpdateStateInstant();

                _textTotalValue.Add(Instantiate(textPrefab, placeForTotalValue));

                index++;
            }

            simulatedUpgradeLevel.inputText.text = _currentUpgrade.UpgradeIndex.ToString();
            simulatedUpgradeLevel.UpdateStateInstant();
            OnUpgradeIndexChanged(_currentUpgrade.UpgradeIndex.ToString());
        }

        public void UpdateValues()
        {
            var childStats = _currentUpgrade;

            CarUpgradeBase.CarUpgradesValue value;

            _currentUpgrade.UpgradeIndex = int.Parse(simulatedUpgradeLevel.inputText.text);

            List<string> keys = new();

            foreach (var carProperty in _currentUpgrade.CarUpgradesDictionary)
            {
                keys.Add(carProperty.Key);
            }

            for (int index = 0; index < keys.Count; index++)
            {

                value.BaseValue = float.Parse(_inputBaseValues[index].inputText.text);
                value.ValuePerUpgrade = float.Parse(_inputUpgradePerValues[index].inputText.text);

                _currentUpgrade.CarUpgradesDictionary[keys[index]] = value;

                _textTotalValue[index].text =
                    (_currentUpgrade.CarUpgradesDictionary[keys[index]].BaseValue + _currentUpgrade.UpgradeIndex * _currentUpgrade.CarUpgradesDictionary[keys[index]].ValuePerUpgrade).ToString();
            }

        }

        public void SaveMainUpgradesController()
        {
            UpdateValues();

            var carActor = Player.Instance.PlayerCarGameObject.GetComponent<CarActor>();
            carActor.UpdateUpgrades();
            carActor.SaveCurrentUpgrades();
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public void ResetToLastNumbers()
        {
            _mainUpgradeController.CarUpgradesList[_currentUpgradeIndex].CopyUpgrades(_resetUpgradeList.CarUpgradesList[_currentUpgradeIndex]);
            SetCurrentUpgrade(_mainUpgradeController.CarUpgradesList[_currentUpgradeIndex]);
        }

        public void OnUpgradeIndexChanged(string upgradeStringIndex)
        {
            if (upgradeStringIndex == "")
            {
                UnityEngine.Debug.LogWarning("No index");
                return;
            }

            int upgradeIndex = int.Parse(upgradeStringIndex);

            List<string> keys = new();

            foreach (var carProperty in _currentUpgrade.CarUpgradesDictionary)
            {
                keys.Add(carProperty.Key);
            }

            for (int index = 0; index < keys.Count; index++)
            {
                _textTotalValue[index].text =
                    (_currentUpgrade.CarUpgradesDictionary[keys[index]].BaseValue + upgradeIndex * _currentUpgrade.CarUpgradesDictionary[keys[index]].ValuePerUpgrade).ToString();
            }
        }

        public override void ReturnToPool()
        {
            if (resetConfigButton != null)
                resetConfigButton.onClick.RemoveAllListeners();
            
            if (saveConfigButton != null)
                saveConfigButton.onClick.RemoveAllListeners();

            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            if (upgradeDropdown != null)
                upgradeDropdown.onValueChanged.RemoveAllListeners();

            _isCreated = false;
        }

    }
}