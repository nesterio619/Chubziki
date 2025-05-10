using Core.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Camera
{
    public static class GameObjectViewAsRenderTexture
    {
        private const string Prefab_Path = "Prefabs/RenderTextureCamera";

        private static Transform _cameraInstanceTransform;
        private static UnityEngine.Camera _cameraInstance;

        public static UnityEngine.Camera CameraInstance
        {
            get
            {
                if (_cameraInstance == null)
                {
                    AssetUtils.TryLoadAsset(Prefab_Path, out GameObject cameraPrefab);
                    var cameraObject = GameObject.Instantiate(cameraPrefab);
                    _cameraInstance = cameraObject.GetComponent<UnityEngine.Camera>();

                    _cameraInstanceTransform = CameraInstance.transform;
                }

                return _cameraInstance;
            }
        }

        public static void RenderToRawImage(RawImage image, Transform cameraParentTransform, Vector2 textureSize, int cullingMask = 0, Color backgroundColor = default)
        {
            CameraInstance.targetTexture.width = (int)textureSize.x;
            CameraInstance.targetTexture.height = (int)textureSize.y;
            image.texture = CameraInstance.targetTexture;
            
            if (cullingMask != 0) CameraInstance.cullingMask = cullingMask;
            if (backgroundColor != default) CameraInstance.backgroundColor = backgroundColor;

            _cameraInstanceTransform.SetParent(cameraParentTransform);
            _cameraInstanceTransform.localPosition = Vector3.zero;
            _cameraInstanceTransform.localRotation = Quaternion.identity;
        }

        public static void EnableCamera(bool enable)
        {
            CameraInstance.gameObject.SetActive(enable);
        }
    }
}