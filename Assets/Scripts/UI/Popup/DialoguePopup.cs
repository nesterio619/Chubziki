using System;
using Core.ObjectPool;
using Core.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class DialoguePopup : PopupController, IPopupWithMainText
    {
        [field: SerializeField] public TextMeshProUGUI MainText { get; set; }
        [SerializeField] private Button YesButton, NoButton;
        
        private const string ScriptablePoolInfoPath = "ScriptableObjects/ObjectPool/UI/DialoguePopupPoolInfo";
        private static PrefabPoolInfo _popup_PrefabPoolInfo;
        
        public static void CreateWithoutTransform() => Create();
        
        public static void Create(string text = "Are you sure?", Action ConfirmAction = null, Action CancelAction = null, Vector2Int? overrideSize = null, Transform parent = null)
        {
            AssetUtils.TryLoadAsset(ScriptablePoolInfoPath, out _popup_PrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(_popup_PrefabPoolInfo, parent).GetComponent<DialoguePopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;
            
            popup.Initialize(text, ConfirmAction, CancelAction, overrideSize);
        }

        private void Initialize(string text, Action ConfirmAction, Action CancelAction = null, Vector2Int? overrideSize = null)
        {
            InitializeMainText(text);
            InitializeConfirmButton(ConfirmAction);
            InitializeCancelButton(CancelAction);
            if (overrideSize.HasValue)
            {
                GetComponent<RectTransform>().sizeDelta = overrideSize.Value;
            }

            GamepadCursor.DisplayCursor(true);
        }

        public void InitializeMainText(string text)
        {
            MainText.text = text;
        }

        private void InitializeConfirmButton(Action ConfirmAction)
        {
            YesButton.onClick.AddListener(() => OnClickConfirm(ConfirmAction));
        }

        private void InitializeCancelButton(Action CancelAction)
        {
            NoButton.onClick.AddListener(() => OnClickCancel(CancelAction));
        }

        private void OnClickConfirm(Action ConfirmAction)
        {
            ConfirmAction?.Invoke();
            OnCloseAction?.Invoke();
            ReturnToPool();
        }

        private void OnClickCancel(Action CancelAction)
        {
            CancelAction?.Invoke();
            OnCloseAction?.Invoke();
            ReturnToPool();
        }

        public override void ReturnToPool()
        {
            GamepadCursor.DisplayCursor(false);

            if (YesButton != null)
                YesButton.onClick.RemoveAllListeners();
            
            if(NoButton != null)
                NoButton.onClick.RemoveAllListeners();
            
            base.ReturnToPool();
        }
    }
}