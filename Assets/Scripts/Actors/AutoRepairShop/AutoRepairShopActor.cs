using System.Collections;
using Core;
using Core.Enums;
using Core.Extensions;
using Core.SaveSystem;
using Core.Utilities;
using Regions;
using Transition;
using UnityEngine;

namespace Actors.AutoRepairShop
{
    public class AutoRepairShopActor : Actor
    {
        private const float Time_To_Leave = 0.4f;

        [SerializeField] private Transform carSpawnPoint;
        [SerializeField] private float timeToAllowControlCar;
        [SerializeField] private Collider sceneChangeTrigger;

        private static SceneConfig _sceneToReturnTo;

        public Transform CarSpawnPoint => carSpawnPoint;

        private void Start()
        {
            if(sceneChangeTrigger == null) return;

            sceneChangeTrigger.enabled = false;
            UtilitiesProvider.WaitAndRun(()=> sceneChangeTrigger.enabled = true,false,2.5f);
        }

        public void GarageEnter()
        {
            if (TransitionManager.IsTransitioning) return;
            if(_sceneToReturnTo != null) return;

            SaveManager.Progress.PlayerTransformData.SavePlayerPose(carSpawnPoint.GetPose());
            PauseMenuCanvasScreen.OnMainMenuLoad += SavePlayerEquipment;

            _sceneToReturnTo = SceneManager.CurrentSceneConfig;
            UI.Popup.Developer.CarUpgradePopup.LastScene = SceneManager.CurrentSceneConfig.SceneIndex;

            Player.Instance.AutoTeleportationPositionOnMainScene = carSpawnPoint.position;
            Player.Instance.AutoTeleportationRotationOnMainScene = carSpawnPoint.rotation.eulerAngles;
            PrepareScene((int)UnityScenes.mechanicWorkshop);
        }

        public void GarageExit()
        {
            SavePlayerEquipment();
            PauseMenuCanvasScreen.OnMainMenuLoad -= SavePlayerEquipment;

            PrepareScene(_sceneToReturnTo.SceneIndex);
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(SetCarPositionOnScene);
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(StartTimer);
            _sceneToReturnTo = null;
            sceneChangeTrigger.enabled = false;
        }
        
        private void SavePlayerEquipment()
        {
            var equipment = Player.Instance.PlayerCarGameObject.CarMold.Equipment;
            SaveManager.Progress.SaveEquipment(equipment);
        }

        private void SetCarPositionOnScene()
        {
            UtilitiesProvider.WaitAndRun(() =>
            {
                Player.Instance.PlayerCarGameObject.transform.position = Player.Instance.AutoTeleportationPositionOnMainScene;
                Player.Instance.PlayerCarGameObject.transform.rotation = Quaternion.Euler(Player.Instance.AutoTeleportationRotationOnMainScene);
                Physics.SyncTransforms();
            }, true, 0);
            
        }

        private void StartTimer()
        {
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);
            Player.Instance.StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            yield return new WaitForSeconds(timeToAllowControlCar);
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions();
        }

        public void PushCar()
        {
            if (Player.Instance.PlayerCarGameObject.HorizontalInput || Player.Instance.PlayerCarGameObject.VerticalInput) return;

            Player.Instance.PlayerCarGameObject.CarDriving.MoveForwardForSeconds(Time_To_Leave);
        }


        private void PrepareScene(int sceneIndex)
        {
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(Player.Instance.SpawnCamera);
            SceneManager.OnNewSceneLoaded_BeforeAnimation_ActionList.Add(Player.Instance.LoadOnPlayerPosition);
            SceneManager.LoadScene(sceneIndex, TransitionManager.LoadMode.Fade);
        }
    }
}

