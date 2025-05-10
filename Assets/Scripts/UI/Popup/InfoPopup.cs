using System;
using System.Collections.Generic;
using Core;
using Core.ObjectPool;
using Core.Utilities;
using TMPro;
using UI.Canvas;
using UnityEngine;

namespace UI.Popup
{
    public class InfoPopup : PopupController, IPopupWithMainText
    {
        [field: SerializeField] public TextMeshProUGUI MainText { get; private set; }
        [field: SerializeField] public CustomButtonController CloseButton { get; private set; }

        private const string ScriptablePoolInfoPath = "ScriptableObjects/ObjectPool/UI/InfoPopupPoolInfo";
        private static PrefabPoolInfo popup_PrefabPoolInfo;
        
        #region Anchors
        public enum PopupAnchor
        {
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            TopLeft,
            Center
        }

        private struct AnchorData
        {
            public Vector2 Pivot { get; }
            public Vector2 AnchorMin { get; }
            public Vector2 AnchorMax { get; }
            public Vector2 AnchoredPosition { get; }

            public AnchorData(Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
            {
                Pivot = pivot;
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                AnchoredPosition = anchoredPosition;
            }
        }

        private static readonly Dictionary<PopupAnchor, AnchorData> AnchorDictionary = new Dictionary<PopupAnchor, AnchorData>
        {
            { PopupAnchor.Top, new AnchorData(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -1)) },
            { PopupAnchor.TopRight, new AnchorData(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-1, -1)) },
            { PopupAnchor.Right, new AnchorData(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-1, 0)) },
            { PopupAnchor.BottomRight, new AnchorData(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-1, 1)) },
            { PopupAnchor.Bottom, new AnchorData(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 1)) },
            { PopupAnchor.BottomLeft, new AnchorData(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1, 1)) },
            { PopupAnchor.Left, new AnchorData(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1, 0)) },
            { PopupAnchor.TopLeft, new AnchorData(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1, -1)) },
            { PopupAnchor.Center, new AnchorData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero) }
        };
        #endregion

        private void Start()
        {
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(ReturnToPool);
        }

        public static InfoPopup Create(string text, Action onClose = null, Vector2Int? overrideSize = null, PopupAnchor anchor = PopupAnchor.TopRight, float displayTime = 6, float offset = 50)
        {
            if (CanvasManager.Instance == null)
                return null;
            
            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(ScriptablePoolInfoPath, out popup_PrefabPoolInfo);
            var popup = ObjectPooler.TakePooledGameObject(popup_PrefabPoolInfo, parent).GetComponent<InfoPopup>();
            
            var popupRect = popup.transform as RectTransform;
            if (popupRect == null)
            {
                popup.ReturnToPool();
                return null;
            }
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;
            
            popup.Initialize(text, onClose, overrideSize, anchor, displayTime, offset);
            return popup;
        }

        private void Initialize(string text, Action onClose = null, Vector2Int? overrideSize = null, PopupAnchor anchor = PopupAnchor.Center, float displayTime = 6, float offset = 50)
        {
            _isDisposed = false;
            OnCloseAction = onClose;
            InitializeRectTransform(overrideSize, anchor, offset);
            InitializeMainText(text);
            InitializeCloseButton();
            StartCloseTimer(displayTime);
        }

        private void InitializeRectTransform(Vector2Int? overrideSize, PopupAnchor anchor, float offset)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = overrideSize ?? DefaultPopupSize;
            
            if (AnchorDictionary.TryGetValue(anchor, out AnchorData config))
            {
                rectTransform.pivot = config.Pivot;
                rectTransform.anchorMin = config.AnchorMin;
                rectTransform.anchorMax = config.AnchorMax;
                rectTransform.anchoredPosition = config.AnchoredPosition * offset;
            }        
        }

        public void InitializeMainText(string text)
        {
            MainText.text = text;
        }

        public void InitializeCloseButton()
        {
            CloseButton.onClick.AddListener(ReturnToPool);
        }

        private void StartCloseTimer(float displayTime)
        {
            UtilitiesProvider.WaitAndRun(() =>
            {
                if(this != null && !_isDisposed)
                    ReturnToPool();
            }, false, displayTime);
        }

        protected bool _isDisposed;
        public override void ReturnToPool()
        {
            if(_isDisposed)
                return;

            _isDisposed = true;
            
            // Callback for when we close popup
            if (OnCloseAction != null)
            {
                OnCloseAction.Invoke();
                OnCloseAction = null;
            }

            if(CloseButton != null)
                CloseButton.onClick.RemoveAllListeners();
            
            base.ReturnToPool();
        }
    }
}
