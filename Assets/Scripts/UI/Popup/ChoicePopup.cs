
using Core.ObjectPool;
using System.Collections.Generic;
using Core.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Popup
{
    public class ChoicePopup : PopupController, IPopupWithMainText
    {
        [SerializeField] private Transform ButtonsContainerTransform;
        [field: SerializeField] public CustomButtonController CloseButton { get; private set; }
        [field: SerializeField] public TextMeshProUGUI MainText { get; set; }

        [SerializeField] private PrefabPoolInfo button_PrefabPoolInfo;

        private List<CustomButtonController> _myButtons = new();

        private const string ScriptablePoolInfoPath = "ScriptableObjects/ObjectPool/UI/ChoicePopupPoolInfo";
        private static PrefabPoolInfo _popup_PrefabPoolInfo;
        
        public void InitializeMainText(string text) => MainText.text = text;

        public static void Create(string text, CustomButtonController.ButtonMold[] buttonMolds, Vector2Int? overrideSize = null, Transform parent = null)
        {
            AssetUtils.TryLoadAsset(ScriptablePoolInfoPath, out _popup_PrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(_popup_PrefabPoolInfo, parent).GetComponent<ChoicePopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;

            popup.Initialize(text, buttonMolds, overrideSize);
        }

        private void Initialize(string text, CustomButtonController.ButtonMold[] buttonMolds, Vector2Int? overrideSize = null)
        {
            for (int i = 0; i < buttonMolds.Length; i++)
            {
                var button = CustomButtonController.Create(buttonMolds[i], ButtonsContainerTransform, button_PrefabPoolInfo);
                button.onClick.AddListener(ReturnToPool);
                _myButtons.Add(button);
            }

            InitializeRectTransform(overrideSize);
            InitializeMainText(text);
            InitializeCloseButton();

            GamepadCursor.DisplayCursor(true);
        }
        
        private void InitializeRectTransform(Vector2Int? overrideSize)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = overrideSize ?? DefaultPopupSize;
        }
        
        public void InitializeCloseButton() => CloseButton.onClick.AddListener(ReturnToPool);
        
        public override void ReturnToPool()
        {
            GamepadCursor.DisplayCursor(false);

            if (_myButtons != null)
            {
                foreach (var button in _myButtons)
                    button.Dispose();
                
                _myButtons.Clear();
            }

            if (CloseButton != null)
                CloseButton.onClick.RemoveAllListeners();

            base.ReturnToPool();
        }
    }
}

