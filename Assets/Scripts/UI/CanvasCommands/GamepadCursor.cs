using Core;
using Core.InputManager;
using Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using UI.Canvas;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI
{
    public enum GamepadCursorImage { Normal, Click, Grab }

    public static class GamepadCursor
    {
        private const string Prefab_Path = "Prefabs/UI/GamepadCursor";
        private const string Click_Sprite_Path = "CursorSprites/cursor_click";
        private const string Grab_Sprite_Path = "CursorSprites/cursor_grab";

        public static GameObject Instance { get; private set; }
        public static Vector3 CurrentCursorPosition => _rectTransform != null ? _rectTransform.anchoredPosition : default;

        private static float _cursorSpeed = 425;
        private static float _scrollSpeed = 0.7f; // scroll position is from 0 to 1
        private static Sprite _clickSprite;
        private static Sprite _grabSprite;
        private static Sprite _normalSprite;

        private static RectTransform _rectTransform;
        private static Image _cursorImage;
        private static GameInput _inputActions;
        private static ScrollRect _currentScrollRect;

        private static Vector2 _canvasDimensions;
        private static bool _shouldDisplay;
        private static bool _eventSubbed;

        private static List<GameObject> _lastHoveredElements = new();

        public static void DisplayCursor(bool display)
        {
            _shouldDisplay = display;

            OnInputDeviceChanged(InputManager.CurrentInputDevice);

            if (!_eventSubbed) InputManager.OnInputDeviceChanged += OnInputDeviceChanged;
        }

        public static void SetImage(GamepadCursorImage imageType)
        {
            if (_cursorImage == null) return;

            switch (imageType)
            {
                case GamepadCursorImage.Normal: _cursorImage.sprite = _normalSprite; break;
                case GamepadCursorImage.Click: _cursorImage.sprite = _clickSprite; break;
                case GamepadCursorImage.Grab: _cursorImage.sprite = _grabSprite; break;
            }
        }

        public static bool WasPressedThisFrame() => _inputActions != null ? _inputActions.GamepadCursorControls.Click.WasPressedThisFrame() : false;
        public static bool WasReleasedThisFrame() => _inputActions != null ? _inputActions.GamepadCursorControls.Click.WasReleasedThisFrame() : false;

        public static void SetCurrentScrollRect(ScrollRect scrollRect, bool opened)
        {
            if(_currentScrollRect != scrollRect && !opened) return;

            _currentScrollRect = opened ? scrollRect : null;
        }

        private static void Initialize()
        {
            AssetUtils.TryLoadAsset(Prefab_Path, out GameObject prefab);
            Instance = GameObject.Instantiate(prefab);
            Instance.transform.SetParent(CanvasManager.Instance.transform);
            Instance.transform.localPosition = Vector3.zero;

            _rectTransform = Instance.GetComponent<RectTransform>();
            _cursorImage = Instance.GetComponentInChildren<Image>();
            _canvasDimensions = CanvasManager.Instance.MainCanvas.sizeDelta;

            _normalSprite = _cursorImage.sprite;
            AssetUtils.TryLoadAsset(Click_Sprite_Path, out _clickSprite);
            AssetUtils.TryLoadAsset(Grab_Sprite_Path, out _grabSprite);

            _inputActions = new();
            _inputActions.Enable();

            Player.Instance.OnUpdateEvent += Update;
        }

        private static void OnInputDeviceChanged(InputDeviceType type)
        {
            if (type == InputDeviceType.Gamepad && _shouldDisplay)
            {
                if(Instance==null) Initialize();
                Instance.SetActive(true);
            }

            if(type == InputDeviceType.Keyboard || !_shouldDisplay)
            {
                if (Instance!=null) Instance.SetActive(false);
            }

            SetImage(GamepadCursorImage.Normal);
        }

        private static void Update()
        {
            if(!Instance.activeSelf) return;

            MoveCursor();

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
                { position = CurrentCursorPosition };

            List<GameObject> hoveredElements = RaycastUI(pointerData);

            InvokePointerEnter(hoveredElements, pointerData);
            InvokePointerExit(hoveredElements, pointerData);
            Scroll();
        }

        private static void MoveCursor()
        {
            var horizontal = _inputActions.GamepadCursorControls.MovementHorizontal.ReadValue<float>();
            var vertical = _inputActions.GamepadCursorControls.MovementVertical.ReadValue<float>();

            Instance.transform.position += Vector3.up * _cursorSpeed * vertical * Time.unscaledDeltaTime;
            Instance.transform.position += Vector3.right * _cursorSpeed * horizontal * Time.unscaledDeltaTime;

            _rectTransform.anchoredPosition = _rectTransform.anchoredPosition.Clamp(Vector2.zero, _canvasDimensions);
        }

        private static void InvokePointerEnter(List<GameObject> hoveredElements, PointerEventData pointerData)
        {
            foreach (var element in hoveredElements)
            {
                var parent = element.transform.parent.gameObject;
                if (WasPressedThisFrame())
                {
                    ExecuteEvents.Execute(parent, pointerData, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.Execute(element, pointerData, ExecuteEvents.pointerClickHandler);
                }

                if (_lastHoveredElements.Contains(element)) continue;

                ExecuteEvents.Execute(parent, pointerData, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(element, pointerData, ExecuteEvents.pointerEnterHandler);

                SetImage(GamepadCursorImage.Click);
            }
        }

        private static void InvokePointerExit(List<GameObject> hoveredElements, PointerEventData pointerData)
        {
            foreach (var element in _lastHoveredElements)
            {
                if (element == null) continue;
                if (hoveredElements.Contains(element)) continue;

                var parent = element.transform.parent.gameObject;
                ExecuteEvents.Execute(parent, pointerData, ExecuteEvents.pointerExitHandler);
                ExecuteEvents.Execute(element, pointerData, ExecuteEvents.pointerExitHandler);

                SetImage(GamepadCursorImage.Normal);
            }

            _lastHoveredElements.Clear();
            _lastHoveredElements = new(hoveredElements);
        }

        private static void Scroll()
        {
            if(_currentScrollRect ==  null) return;

            var scrollInput = _inputActions.GamepadCursorControls.Scroll.ReadValue<float>();

            _currentScrollRect.verticalNormalizedPosition += _scrollSpeed * scrollInput * Time.unscaledDeltaTime;
        }

        private static List<GameObject> RaycastUI(PointerEventData pointerData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            return results.Select(x => x.gameObject).ToList();
        }

        private static void Dispose()
        {
            if (_inputActions != null) _inputActions.Dispose();

            InputManager.OnInputDeviceChanged -= OnInputDeviceChanged;
            Player.Instance.OnUpdateEvent -= Update;
        }
    }
}
