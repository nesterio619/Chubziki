using System;
using Core.ObjectPool;
using Core.SaveSystem;
using Core.Utilities;
using TMPro;
using UI.Canvas;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class WelcomePopup : InfoPopup
    {
        [Header("Welcome Specific")]
        [SerializeField] private Image image;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform imageContainer;
        [SerializeField] private Sprite imageSprite;

        private static WelcomePopup instance;

        private const int PADDING = 15;
        private const int HEADER_SIZE = 40;

        private const string ScriptablePoolInfoPath = "ScriptableObjects/ObjectPool/UI/WelcomePopupPoolInfo";
        private static PrefabPoolInfo imagePopup_PrefabPoolInfo;

        public static WelcomePopup ShowIfFirstTime()
        {
            if (!SaveManager.Progress.FirstLaunch)
                return null;

            SaveManager.Progress.FirstLaunch = false;
            SaveManager.SaveProgress();

            return Show();
        }

        public static WelcomePopup Show()
        {
            if (instance != null)
                return instance;

            var parent = CanvasManager.Instance.PopupCanvas;
            AssetUtils.TryLoadAsset(ScriptablePoolInfoPath, out imagePopup_PrefabPoolInfo);
            var popupGO = ObjectPooler.TakePooledGameObject(imagePopup_PrefabPoolInfo, parent);
            instance = popupGO.GetComponent<WelcomePopup>();

            if (instance == null)
            {
                return null;
            }

            var popupRect = instance.transform as RectTransform;
            popupRect.SetParent(parent, false);
            popupRect.anchoredPosition = Vector2.zero;

            instance.InitializePopup();
            return instance;
        }

        private void InitializePopup()
        {
            _isDisposed = false;

            string text = ComposeText(IsGamepadConnected());
            InitializeMainText(text);
            InitializeCloseButton();

            LoadImage();
            AdjustLayout();

            OnCloseAction = () => instance = null;
        }

        private void LoadImage()
        {
            if (imageSprite == null)
            {
                image.enabled = false;
                return;
            }

            if (imageSprite != null)
            {
                image.sprite = imageSprite;
                image.enabled = true;
            }
            else
            {
                image.enabled = false;
            }
        }

        private void AdjustLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);

            if (image.sprite != null)
            {
                float imageHeight = contentRoot.rect.height;
                float imageWidth = image.sprite.rect.width * (imageHeight / image.sprite.rect.height);
                imageContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight);
                imageContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageWidth);

                var root = GetComponent<RectTransform>();
                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageWidth + contentRoot.rect.width + PADDING * 2);
                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight + PADDING * 2);

                image.preserveAspect = true;
            }
        }

        private bool IsGamepadConnected()
        {
            foreach (string name in Input.GetJoystickNames())
            {
                if (!string.IsNullOrEmpty(name)) return true;
            }
            return false;
        }

        private string ComposeText(bool isGamepad)
        {
            string header = $"<size={HEADER_SIZE}>Welcome</size>\n\n";
            string plot = $"<size={HEADER_SIZE}>Plot:</size>\n" +
                $"You are a mercenary sent to help a corporation settle down on some island by completing missions.\n" +
                $"Help them defeat all 'Chubziks' by wiping out their huge coordinated armies, solving physical puzzles, winning races and other quests…\n\n";
            string task = $"<size={HEADER_SIZE}>Your task:</size>\n" +
                $"Try to find equipment and upgrade points scattered around the map by exploring the open-world or get them by completing quests. Customize your car at mechanic-shop to kill all chubziks on map.\n\n";
            string controls = $"<size={HEADER_SIZE}>Controls:</size>\n" +
                $"Driving is the single most important skill here, besides building a cool vehicle.\n" +
                $"We hope you will enjoy your time at Chubzia!\n\n" +
                ComposeControlText(isGamepad);

            return $"{header}This is an open world sandbox, so feel free to experiment and have fun. You define your own adventure!\n\n{plot}{task}{controls}";
        }

        private string ComposeControlText(bool gamepad)
        {
            return gamepad
                ? $"Forward - RT 🎮\nBack - LT 🎮\nSteering - Right Stick 🎮\nDrift - B / Circle 🔘"
                : $"Forward - W ↑\nBack - S ↓\nSteering - A / D ← →\nDrift - Space / Ctrl / Shift";
        }

        public override void ReturnToPool()
        {
            if (_isDisposed) return;
            base.ReturnToPool();
            instance = null;
        }
    }
}
