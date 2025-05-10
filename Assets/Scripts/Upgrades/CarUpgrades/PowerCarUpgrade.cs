using System.Collections.Generic;
using UnityEngine;

namespace Upgrades.CarUpgrades
{
    [System.Serializable]
    public class PowerCarUpgrade : CarUpgradeBase
    {
        private Dictionary<string, CarUpgradesValue> _carUpgradesProperties = new();

        public override Dictionary<string, CarUpgradesValue> CarUpgradesDictionary
        {
            get
            {
                if (_carUpgradesProperties.Count == 0)
                {
                    _carUpgradesProperties.Add("Speed acceleration", new CarUpgradesValue(195, 10.5f));
                    _carUpgradesProperties.Add("Speed decceleration", new CarUpgradesValue(4.8f, 0.02f));
                    _carUpgradesProperties.Add("Brake force", new CarUpgradesValue(50, 45));
                }

                return _carUpgradesProperties;
            }
        }

        public override float GetValue(int indexEnum)
        {
            PowerUpgrades upgrades = (PowerUpgrades)indexEnum;

            CarUpgradesValue value;

            switch (upgrades)
            {
                case PowerUpgrades.SpeedAcceleration:
                    CarUpgradesDictionary.TryGetValue("Speed acceleration", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case PowerUpgrades.SpeedDecceleration:
                    CarUpgradesDictionary.TryGetValue("Speed decceleration", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case PowerUpgrades.BrakeForce:
                    _carUpgradesProperties.TryGetValue("Brake force", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;
            }

            Debug.LogError("Not right index");

            return 0;

        }


        public enum PowerUpgrades
        {
            SpeedAcceleration,
            SpeedDecceleration,
            BrakeForce
        }
    }
}