using TMPro;
using UnityEngine;
using Core.ObjectPool;
using Core.Utilities;
using Core;
using Actors;
using UI.Canvas;
using Upgrades;
using Michsky.MUIP;
using Core.SaveSystem;

namespace UI.Popup.Developer
{
    public class CarUpgradePopup : PopupController
    {
        public const int DEVELOPER_SCENE = 1;

        private UpgradeLevelContainer _playerUpgradeLevelContainer;

        private UpgradeLevelContainer _copyLevelContainer = new();

        [SerializeField] private ChangeFields[] changeFields;

        [SerializeField] private TextMeshProUGUI upgradePointsCounter;

        [SerializeField] private ButtonManager addPointsButton;

        [SerializeField] private ButtonManager closeButton;

        private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/UpgradeStatsCreationPopupPoolInfo";

        private int _availableUpgradePoints
        {
            get => SaveManager.Progress.UpgradeData.AvailablePoints;
            set
            {
                SaveManager.Progress.UpgradeData.AvailablePoints = value;
                SaveManager.SaveProgress();
            }
        }

        private int _availableUpgradePointsOnOpenPopup;

        private PlayerCarActor _playerCarActor;

        public static int LastScene = 0;

        private static bool isCreatedCarUpgradePopup = false;

        public static void CreateWithoutTransform() => Create();

        public static void Create(Transform parent = null)
        {
            if (isCreatedCarUpgradePopup)
                return;

            isCreatedCarUpgradePopup = true;

            if (parent == null)
                parent = CanvasManager.Instance.PopupCanvas;

            AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PrefabPoolInfo popup_PrefabPoolInfo);

            var popup = ObjectPooler.TakePooledGameObject(popup_PrefabPoolInfo, parent).GetComponent<CarUpgradePopup>();

            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == DEVELOPER_SCENE || LastScene == DEVELOPER_SCENE)
                popup.addPointsButton.gameObject.SetActive(true);
            else
                popup.addPointsButton.gameObject.SetActive(false);

            popup.SetUpPopup(Player.Instance.PlayerCarGameObject.GetComponent<CarActor>());

            popup.UpdatePointsCounter();

            GamepadCursor.DisplayCursor(true);
        }

        public void AddPoints()
        {
            _availableUpgradePointsOnOpenPopup += 10;
            UpdatePointsCounter();
        }

        public void SetUpPopup(CarActor carActor)
        {
            _playerCarActor = carActor as PlayerCarActor;

            _availableUpgradePointsOnOpenPopup = _availableUpgradePoints;
            _playerUpgradeLevelContainer = _playerCarActor.GetLevelUpgrades();
            _copyLevelContainer.Copy(_playerUpgradeLevelContainer);

            for (int i = 0; i < changeFields.Length; i++)
            {
                int index = i;

                changeFields[i].DecreaseButton.onClick.AddListener(() => ChangeStatValue((UpgradesList.Upgrades)index, -1));
                changeFields[i].IncreaseButton.onClick.AddListener(() => ChangeStatValue((UpgradesList.Upgrades)index, 1));
                changeFields[i].InputField.onEndEdit.AddListener((inputString) => OnChangeInputSpeed((UpgradesList.Upgrades)index, inputString));

                changeFields[i].InputField.text = _copyLevelContainer.UpgradeLevels[index].UpgradeIndex.ToString();
            }

            addPointsButton.onClick.AddListener(AddPoints);
            closeButton.onClick.AddListener(ClosePopup);

            _playerCarActor.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);

        }


        public void ChangeStatValue(UpgradesList.Upgrades stat, int changeValue)
        {
            if (_availableUpgradePointsOnOpenPopup <= 0 && changeValue > 0)
                return;

            _availableUpgradePointsOnOpenPopup -= changeValue;

            TryChangeUpgradeValue(stat, _copyLevelContainer.UpgradeLevels[(int)stat].UpgradeIndex + changeValue);
        }

        private void OnChangeInputSpeed(UpgradesList.Upgrades stat, string inputText)
        {
            ChangeStatByEndEditing(stat, int.Parse(inputText));
        }

        private void TryChangeUpgradeValue(UpgradesList.Upgrades upgradeType, int changeValue)
        {
            if (changeValue <= 0)
                return;

            UpdatePointsCounter();

            UpgradeLevelContainer.UpgradeInfo upgradeInfo;

            //Setup ugrade struct
            upgradeInfo = _copyLevelContainer.UpgradeLevels[(int)upgradeType];

            //Set new value
            upgradeInfo.UpgradeIndex = changeValue;

            //Setting upgrades
            _copyLevelContainer.UpgradeLevels[(int)upgradeType] = upgradeInfo;

            //Updating input text
            changeFields[(int)upgradeType].InputField.text = _copyLevelContainer.UpgradeLevels[(int)upgradeType].UpgradeIndex.ToString();
        }

        private void UpdatePointsCounter() => upgradePointsCounter.text = _availableUpgradePointsOnOpenPopup.ToString();

        private void ChangeStatByEndEditing(UpgradesList.Upgrades upgradeType, int input)
        {
            if (input <= 0)
            {
                changeFields[(int)upgradeType].InputField.text = _copyLevelContainer.UpgradeLevels[(int)upgradeType].UpgradeIndex.ToString();
                return;
            }

            int changeValue = input - _copyLevelContainer.UpgradeLevels[(int)upgradeType].UpgradeIndex;

            if (changeValue > 0 && changeValue < _availableUpgradePointsOnOpenPopup)
            {
                _availableUpgradePointsOnOpenPopup -= changeValue;
                TryChangeUpgradeValue(upgradeType, input);
            }
            else if (changeValue < 0)
            {
                _availableUpgradePointsOnOpenPopup += Mathf.Abs(changeValue);
                TryChangeUpgradeValue(upgradeType, input);
            }

            changeFields[(int)upgradeType].InputField.text = _copyLevelContainer.UpgradeLevels[(int)upgradeType].UpgradeIndex.ToString();
        }


        public void UpdateUpgrades()
        {
            var carActor = Player.Instance.PlayerCarGameObject.GetComponent<CarActor>();

            _availableUpgradePoints = _availableUpgradePointsOnOpenPopup;
            _playerUpgradeLevelContainer.Copy(_copyLevelContainer);

            carActor.UpdateUpgrades();
            carActor.SaveCurrentUpgrades();
        }

        private bool CheckIsStatNotChanged()
        {
            for (int i = 0; i < _playerUpgradeLevelContainer.UpgradeLevels.Count; i++)
            {
                if (_playerUpgradeLevelContainer.UpgradeLevels[i].UpgradeIndex != _copyLevelContainer.UpgradeLevels[i].UpgradeIndex)
                    return false;
            }

            return true;
        }

        public void ClosePopup()
        {
            UnityEngine.Debug.Log("Close");

            if (CheckIsStatNotChanged())
            {
                ReturnToPool();
                return;
            }

            DialoguePopup.Create("Save changes?",
                () => { UpdateUpgrades(); ReturnToPool(); },
                () => { ReturnToPool(); },
                new Vector2Int(400, 200),
                transform);
        }

        public override void ReturnToPool()
        {
            GamepadCursor.DisplayCursor(false);

            foreach (var item in changeFields)
            {
                item.InputField.onEndEdit.RemoveAllListeners();
                item.InputField.onValueChanged.RemoveAllListeners();
                item.DecreaseButton.onClick.RemoveAllListeners();
                item.IncreaseButton.onClick.RemoveAllListeners();
            }

            addPointsButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();


            isCreatedCarUpgradePopup = false;

            base.ReturnToPool();
            _playerCarActor.MovementDirectionLimiter.SetMovementRestrictions(false, false, false, false);
        }

    }

    [System.Serializable]
    public class ChangeFields
    {
        public TMP_InputField InputField;

        public ButtonManager IncreaseButton;
        public ButtonManager DecreaseButton;
    }
}