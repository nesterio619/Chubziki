using Core.ObjectPool;
using MelenitasDev.SoundsGood;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomButtonController : Button, IDisposable
{
    private PooledGameObject pooledGameObject;
    private RectTransform _buttonDimensions;
    private TMP_Text _text;
    private Sound _clickSound;
    private bool _isInitialized;

    protected override void Start()
    {
        base.Start();

        if (_isInitialized)
            return;

        Initialize();
    }

    //Default initialize
    private void Initialize()
    {
        _isInitialized = true;

        pooledGameObject = GetComponent<PooledGameObject>();
        _buttonDimensions = GetComponent<RectTransform>();
        _text = GetComponentInChildren<TMP_Text>();
        _clickSound = new Sound(SFX.button);
        _clickSound.SetPosition(Camera.main?.transform.position ?? Vector3.zero);
        onClick.AddListener(() => _clickSound.Play());
    }

    ///initialize base values.
    private void Initialize(Color buttonColor, string buttonText)
    {
        Initialize();
        image.color = buttonColor;
        _text.text = buttonText;
    }

    //Expanded initialize
    private void Initialize(Color buttonColor, string buttonText, Vector2 buttonSize, Action actions)
    {
        Initialize(buttonColor, buttonText);
        _buttonDimensions.sizeDelta = buttonSize;
        if (actions != null)
            onClick.AddListener(actions.Invoke);
    }

    public static CustomButtonController Create(ButtonMold buttonMold, Transform parent, PrefabPoolInfo _PrefabPoolInfo)
    {
        CustomButtonController customButtonController = ObjectPooler.TakePooledGameObject(_PrefabPoolInfo).GetComponent<CustomButtonController>();

        customButtonController.transform.SetParent(parent);
        customButtonController.transform.localScale = Vector3.one;

        customButtonController.Initialize(buttonMold.ButtonColor, buttonMold.Text, buttonMold.Size, buttonMold.OnClick);

        return customButtonController;
    }

    public void Dispose()
    {
        onClick.RemoveAllListeners();
        pooledGameObject.ReturnToPool();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onClick.RemoveAllListeners();
    }



    [Serializable]
    public struct ButtonMold
    {
        public Action OnClick;
        public string Text;
        public Color ButtonColor;
        public Vector2 Size;

        public ButtonMold(Action onClick, string text, Color color, Vector2 size)
        {
            OnClick = onClick;

            if (text == null)
                Text = "Default text";
            else
                Text = text;

            if (color == null)
                ButtonColor = Color.blue;
            else
                ButtonColor = color;

            if (size == null)
                Size = Vector2.zero;
            else
                Size = size;
        }
    }
}
