using System.Collections.Generic;
using UnityEngine;

namespace Upgrades.CarUpgrades
{
    [System.Serializable]
    public class ControlCarUpgrade : CarUpgradeBase
    {

        private Dictionary<string, CarUpgradesValue> _carUpgradesValues = new();

        public override Dictionary<string, CarUpgradesValue> CarUpgradesDictionary
        {
            get
            {
                if (_carUpgradesValues.Count == 0)
                {
                    _carUpgradesValues.Add("Steering angle", new CarUpgradesValue(26.5f, 0.05f));
                    _carUpgradesValues.Add("Steering angle speed", new CarUpgradesValue(24, 0.16f));
                    _carUpgradesValues.Add("SidewaysGrip", new CarUpgradesValue(0.6f, 0.05f));
                    _carUpgradesValues.Add("SidewaysStiffnes", new CarUpgradesValue(0.8f, 0.02f));
                }

                return _carUpgradesValues;
            }
        }

        public override float GetValue(int indexEnum)
        {
            ControlUpgrades upgrades = (ControlUpgrades)indexEnum;

            CarUpgradesValue value;

            switch (upgrades)
            {
                case ControlUpgrades.MaxSteeringAngle:
                    CarUpgradesDictionary.TryGetValue("Steering angle", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case ControlUpgrades.SteeringSpeed:
                    CarUpgradesDictionary.TryGetValue("Steering angle speed", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case ControlUpgrades.SidewaysGrip:
                    CarUpgradesDictionary.TryGetValue("SidewaysGrip", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case ControlUpgrades.SidewaysStiffnes:
                    CarUpgradesDictionary.TryGetValue("SidewaysStiffnes", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;
            }

            Debug.LogError("Not right index");

            return 0;

        }


        public enum ControlUpgrades
        {
            MaxSteeringAngle,
            SteeringSpeed,
            SidewaysGrip,
            SidewaysStiffnes
        }

    }
}