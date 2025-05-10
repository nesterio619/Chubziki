using System.Collections.Generic;
using Upgrades.CarUpgrades;

namespace Upgrades
{
    [System.Serializable]
    public class UpgradesList
    {
        public List<CarUpgradeBase> CarUpgradesList = new() { new SpeedCarUpgrade(), new PowerCarUpgrade(), new ControlCarUpgrade(), new SurviveCarUpgrade() };

        public void SetUpgradesFromUpgradesContatiner(UpgradeLevelContainer upgradeLevelContainer)
        {
            for (int i = 0; i < CarUpgradesList.Count; i++)
                CarUpgradesList[i].UpgradeIndex = upgradeLevelContainer.UpgradeLevels[i].UpgradeIndex;
        }

        public enum Upgrades
        {
            Speed,
            Power,
            Control,
            Survive
        }

        public struct UpgradesInfo
        {
            public List<CarUpgradeBase.CarUpgradesInfo> CarUpgradesList;
        }


    }
}