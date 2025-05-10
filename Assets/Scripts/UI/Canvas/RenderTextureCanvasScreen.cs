using Actors.AutoRepairShop;
using Components.Camera;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas
{
    public class RenderTextureCanvasScreen : CanvasScreen
    {
        [SerializeField] private RawImage renderTextureImage;
        [SerializeField] private Transform cameraTransform;

        private void Start()
        {
            Initialize();
        }
        public override void Initialize()
        {
            if (IsInitialized) return;

            IsInitialized = true;

            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(Dispose);

            TrySwitchActiveScreen(this);

            GameObjectViewAsRenderTexture.RenderToRawImage(renderTextureImage, cameraTransform, renderTextureImage.rectTransform.sizeDelta);
        }

        public void EnableRenderTextureUI(bool enabled)
        {
            renderTextureImage.gameObject.SetActive(enabled);
            GameObjectViewAsRenderTexture.EnableCamera(enabled);
        }

        protected override void AddListenersToCanvasScreenButtons()
        {
            
        }

        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            
        }
    }
}