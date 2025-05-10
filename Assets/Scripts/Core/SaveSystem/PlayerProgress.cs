using Components.Car;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    [Serializable]
    public class PlayerProgress
    {
        //public CarSettings AppearanceData;
        public VehicleUpgradeData UpgradeData;
        public PlayerTransformData PlayerTransformData;
        [SerializeField] private long[][] equipmentReferences;

        public Dictionary<string, bool> QuestCompletion;
        public Dictionary<string, bool> ChestStates;
        [SerializeField] private Dictionary<string, Dictionary<string, Pose>> locationObjectPathAndPose;

        public bool FirstLaunch;

        public TemporaryUnlockSystem Unlocks;

        public PlayerProgress()
        {
            QuestCompletion = new Dictionary<string, bool>();
            ChestStates = new Dictionary<string, bool>();
            locationObjectPathAndPose = new Dictionary<string, Dictionary<string, Pose>>();
            UpgradeData = new VehicleUpgradeData();
            PlayerTransformData = new PlayerTransformData();
            Unlocks = new TemporaryUnlockSystem();

            FirstLaunch = true;
        }

        #region Equipment methods
        public void SaveEquipment(CarEquipmentLoadout equipment)
        {
            equipmentReferences = equipment.GetRefIDs();
            SaveManager.SaveProgress();
        }

        public void LoadEquipmentInto(CarEquipmentLoadout equipment)
        {
            if(equipmentReferences != null)
                equipment.SetEquipmentFromRefIDs(equipmentReferences);
        }
        #endregion

        #region objectPathAndPose methods
        
        public bool TryGetLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            if (!locationObjectPathAndPose.ContainsKey(location))
            {
                dictionary = null;
                return false;
            }
            
            dictionary = locationObjectPathAndPose[location];
            return true;
        }

        public void AddLocationObjectPoses(string location, out Dictionary<string, Pose> dictionary)
        {
            locationObjectPathAndPose.Add(location, new());
            dictionary = locationObjectPathAndPose[location];
        }
        #endregion
    }
}