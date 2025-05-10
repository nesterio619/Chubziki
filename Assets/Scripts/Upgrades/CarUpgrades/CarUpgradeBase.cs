using System.Collections.Generic;
using System;

namespace Upgrades.CarUpgrades
{
    [Serializable]
    public abstract class CarUpgradeBase
    {
        public int UpgradeIndex = 10;

        public abstract float GetValue(int indexEnum);

        public virtual Dictionary<string, CarUpgradesValue> CarUpgradesDictionary => new();

        public void CopyUpgrades(CarUpgradeBase carUpgrades)
        {
            if (carUpgrades.GetType() != GetType())
                return;

            foreach (var key in carUpgrades.CarUpgradesDictionary.Keys)
            {
                CarUpgradesDictionary[key] = carUpgrades.CarUpgradesDictionary[key];
            }
        }

        public struct CarUpgradesInfo
        {
            public Dictionary<string, CarUpgradesValue> dictInfo;
        }

        public struct CarUpgradeIndex
        {
            public UpgradeLevelContainer.UpgradeInfo[] UpgradeIndexes;

            public UpgradeLevelContainer.UpgradeInfo[] GetDefaultUpgrades() => new UpgradeLevelContainer.UpgradeInfo[] {
            new UpgradeLevelContainer.UpgradeInfo("SpeedCarUpgrade",10),
            new UpgradeLevelContainer.UpgradeInfo("PowerCarUpgrade",10),
            new UpgradeLevelContainer.UpgradeInfo("ControlCarUpgrade",10),
            new UpgradeLevelContainer.UpgradeInfo("SurviveCarUpgrade",10)
        };
        }

        public struct CarUpgradesValue
        {
            public float BaseValue;
            public float ValuePerUpgrade;

            public CarUpgradesValue(float baseValue, float valuePerUpgrade)
            {
                BaseValue = baseValue;
                ValuePerUpgrade = valuePerUpgrade;
            }
        }

    }
}