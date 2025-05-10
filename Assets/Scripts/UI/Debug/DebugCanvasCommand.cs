using Core.ObjectPool;
using System;
using TMPro;
using UnityEngine;

namespace UI.Debug
{
    public class DebugCanvasCommand : CanvasCommand
    {
		private protected const string DebugCanvasPoolPath = "ScriptableObjects/ObjectPool/UI/DebugCanvasCommandPoolInfo";
		private protected Func<string> ValueGetter;
        private float _defaultFontSize;
        public TextMeshProUGUI Text { get; private set; }
        public DebugCanvasCommand(CanvasReceiver receiver, Func<string> valueGetter) : base(receiver)
        {
            ValueGetter = valueGetter;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            var prefabPoolInfo = (PrefabPoolInfo)Resources.Load(DebugCanvasPoolPath);

			PooledObjectReference = ObjectPooler.TakePooledGameObject(prefabPoolInfo);

            Text = PooledObjectReference.GetComponent<TextMeshProUGUI>();

            PooledObjectReference.transform.SetParent(Receiver.Canvas.transform, false);
            PooledObjectReference.transform.localScale = Vector3.one;
        }

        public override void Update() => Text.text = ValueGetter?.Invoke();

        public void SetTextFontSize(float updatedFontSize)
        {
            _defaultFontSize = Text.fontSize;
            Text.fontSize = updatedFontSize;
        }

        private void ResetTextSettings()
        {
            Text.text = "";
            Text.fontSize = _defaultFontSize > 0 ? _defaultFontSize : Text.fontSize;
        }
        public override void Dispose()
        {
            if(IsDisposed)
                return;
            
            if (PooledObjectReference != null)
            {
                ObjectPooler.ReturnPooledObject(PooledObjectReference);
                ResetTextSettings();
            }
            
            base.Dispose();
        }
    }
}


