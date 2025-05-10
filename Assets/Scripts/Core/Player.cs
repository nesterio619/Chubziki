using System;
using System.Linq;
using Actors;
using Actors.Constructors;
using Actors.Molds;
using Components.Camera;
using Core.Enums;
using Core.ObjectPool;
using Core.SaveSystem;
using Regions;
using Transition;
using UI.Canvas;
using UnityEngine;

namespace Core
{
    public sealed class Player : MonoBehaviour
    {
        public PlayerCarActor PlayerCarGameObject 
        {
            get
            {
                if (_playerCarGameObject == null)
                {
                    TryFindPlayerCar();
                }
                return _playerCarGameObject;
            }
            private set => _playerCarGameObject = value; 
        }
        
        public bool CarGameObjectIsNull => _playerCarGameObject == null;

        [HideInInspector] public Vector3 AutoTeleportationPositionOnMainScene;
        [HideInInspector] public Vector3 AutoTeleportationRotationOnMainScene;
        
        public delegate void OnUpdateDelegate();
        public event OnUpdateDelegate OnUpdateEvent;
        public event OnUpdateDelegate OnFixedUpdateEvent;
        
        [SerializeField] private CarMold carMold;
        
        private PlayerCarActor _playerCarGameObject;
        private static bool IsFirstSceneLoaded;

        public static Player Instance { get; private set; }

        private void Awake()
        {
            if (!IsFirstSceneLoaded)
            {
                SceneManager.LoadScene((int)UnityScenes.mainMenu, TransitionManager.LoadMode.None);
                IsFirstSceneLoaded = true;
                return;
            }
            
            SpawnCamera();

            if (!InitializeGameObject())
                return;

            TransitionManager.Initialize();

            SceneManager.OnBeforeNewSceneLoaded_ActionList.Add(RemoveAllRegions);
            
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(SpawnCamera);
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(LoadOnPlayerPosition);

            InputManager.InputManager.Initialize();
            VisibleActorsManager.StartUpdatingVisibleObjects();

            OnUpdateEvent += RegionCoordinator.FindCurrentPlayerLocation;
            OnUpdateEvent += RegionManager.UpdatePlayerPositionWithRepositionDelay;
        }

        public void ApplyLoadedProgress()
        {
            SaveManager.LoadProgress();

            if (CarGameObjectIsNull)
            {
                Debug.LogWarning("PlayerCarGameObject is null, delaying ApplyLoadedProgress until car is created.");
                return; 
            }

            var progress = SaveManager.Progress;
            var carActor = PlayerCarGameObject.GetComponent<CarActor>();
            if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Progress upgrades before applying: {JsonUtility.ToJson(progress.UpgradeData.UpgradeLevels)}");
            
            carActor.GetLevelUpgrades().Copy(progress.UpgradeData.UpgradeLevels.ToUpgradeLevelContainer());
            carActor.UpdateUpgrades();
            if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"CarActor upgrades after applying: {JsonUtility.ToJson(carActor.GetLevelUpgrades())}");
        }

        private void RemoveAllRegions()
        {
            RegionManager.Regions.Clear();
        }

        public void LoadOnPlayerPosition()
        {
            if (!CarGameObjectIsNull) 
                RegionManager.LoadLocationOnPosition(Instance.PlayerCarGameObject.transform.position);
        }

        public void RespawnPlayer()
        {
            if (Instance != null && !CarGameObjectIsNull)
            {
                SaveManager.Progress.PlayerTransformData.TryGetPlayerPose(out Pose pose);
                PlayerCarGameObject.Respawn(pose.position,pose.rotation);
            }
        }

        public void TryFindPlayerCar()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
                return;

            var carPosition = GameObject.Find("PlayerCarPosition");
            if (carPosition == null)
            {
                Debug.LogError("PlayerCarPosition not found in scene!");
                return;
            }

            PlayerCarGameObject = ActorConstructor.Instance.Load(carMold, carPosition.transform).GetComponent<PlayerCarActor>();
            ApplyLoadedProgress();
        }

        public void SpawnCamera()
        {
            CameraManager.SetCameraBySceneIndex(GameObject.Find("Cameras").transform);
        }

        bool InitializeGameObject()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                ObjectPooler.Clear();
                return false;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }

     
        private void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdateEvent?.Invoke();
        }
    }
}