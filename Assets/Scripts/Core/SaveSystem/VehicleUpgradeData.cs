using System;
using Upgrades;

namespace Core.SaveSystem
{
    [Serializable]
    public class VehicleUpgradeData
    {
        public UpgradeLevelContainerWrapper UpgradeLevels;
        public int AvailablePoints; 

        public VehicleUpgradeData()
        {
            UpgradeLevels = new UpgradeLevelContainerWrapper();
            AvailablePoints = 0;
        }


        public void ApplyUpgrades(UpgradesList upgradesList)
        {
            UpgradeLevels.UpdateFromMainUpgradeContainer(upgradesList);
        }
    }
}