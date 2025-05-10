using Components.Car;
using Core.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Upgrades.UpgradeLevelContainer;

namespace Actors.Molds
{

    [CreateAssetMenu(fileName = "CarMold", menuName = "Actors/Molds/CarMold")]
    public class CarMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }


        [Header("UPGRADES")]

        public int SpeedLevel;
        public int PowerLevel;
        public int ControlLevel;
        public int SurviveLevel;

        [Space(10)]
        [Header("EQUIPMENT\nEach array element is a socket.\nEmpty array element = empty socket.")]

        [Space(10)]
        [Tooltip("Revert changes made in play mode when entering edit mode")]
        public bool RevertOnEditorEnter = true;

        public CarEquipmentLoadout Equipment;
        private CarEquipmentLoadout _lastEquipment;

        public Action OnMoldChange;
        private void OnValidate()
        {
            OnMoldChange?.Invoke();

            if (RevertOnEditorEnter) 
            {
                if (_lastEquipment == null) _lastEquipment = new CarEquipmentLoadout();
                Equipment.CopyTo(_lastEquipment);
            }
        }

        public void ResetEquipment()
        {
            if (RevertOnEditorEnter)
                _lastEquipment.CopyTo(Equipment);
        }

        public UpgradeInfo[] GetUpgradesArray() => new UpgradeInfo[] { 
            new UpgradeInfo("SpeedCarUpgrade",SpeedLevel),
            new UpgradeInfo("PowerCarUpgrade",PowerLevel),
            new UpgradeInfo("ControlCarUpgrade",ControlLevel),
            new UpgradeInfo("SurviveCarUpgrade",SurviveLevel)
        };

        public void SaveValues(List<UpgradeInfo> upgrades)
        {
            SpeedLevel = upgrades[0].UpgradeIndex;
            PowerLevel = upgrades[1].UpgradeIndex;
            ControlLevel = upgrades[2].UpgradeIndex;
            SurviveLevel = upgrades[3].UpgradeIndex;
        }
    }
}