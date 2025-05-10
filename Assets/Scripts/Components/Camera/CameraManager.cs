using Core;
using Core.Utilities;
using UnityEngine;

namespace Components.Camera
{
    public static class CameraManager
    {
        private static UnityEngine.Camera mainCamera;
        private static string currentCameraPath;

        private static SceneConfig currentSceneConfig;

        public static void Initialize()
        {
            SceneConfig sceneConfig = SceneManager.CurrentSceneConfig;
            if (sceneConfig == null)
            {
                Debug.LogError("SceneConfig is null. Cannot initialize CameraManager.");
                return;
            }
            currentSceneConfig = sceneConfig;
        }

        public static void SetCameraBySceneIndex(Transform transform)
        {
            if (currentSceneConfig == null)
            {
                Debug.LogError("SceneConfig is not initialized. Call Initialize first.");
                return;
            }
            
            SetCurrentCamera(currentSceneConfig.CameraPath, transform);
        }
        
        public static void SetCurrentCamera(string cameraName, Transform transform, Transform target = null)
        {
            var cameraPath = GetCameraTypeForScene(cameraName);

            if (mainCamera != null && currentCameraPath == cameraPath)
            {
                Debug.LogWarning("Camera of the requested type is already active.");
                return;
            }

            DestroyAllChildCameras(transform);

            SpawnCameraByPath(cameraPath, transform);

            currentCameraPath = cameraPath;
        }
        
        public static bool IsBoundsInCameraView(Bounds bounds)
        {
            if (mainCamera == null)
                return false;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
        
        private static string GetCameraTypeForScene(string cameraName)
        {
            return currentSceneConfig != null ? currentSceneConfig.CameraPath : null;
        }
        
        private static void DestroyAllChildCameras(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<UnityEngine.Camera>() != null)
                    Object.Destroy(child.gameObject);
            }
            mainCamera = null;
        }

  
        private static void SpawnCameraByPath(string path, Transform transform) =>
            mainCamera = UtilitiesProvider.SearchComponentInObject<UnityEngine.Camera>(
                UtilitiesProvider.LoadAndInstantiate(path, transform.position, null, transform)
            );
    }
}