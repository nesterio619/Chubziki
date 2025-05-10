using System.Collections.Generic;
using UnityEngine;

namespace Upgrades.CarUpgrades
{
    [System.Serializable]
    public class SpeedCarUpgrade : CarUpgradeBase
    {

        private Dictionary<string, CarUpgradesValue> _carUpgradesProperties = new();

        public override Dictionary<string, CarUpgradesValue> CarUpgradesDictionary
        {
            get
            {
                if (_carUpgradesProperties.Count == 0)
                {
                    _carUpgradesProperties.Add("Max forward speed", new CarUpgradesValue(60, 2.5f));
                    _carUpgradesProperties.Add("Max reverse speed", new CarUpgradesValue(20, 1.5f));
                    _carUpgradesProperties.Add("ForwardGrip", new CarUpgradesValue(0.5f,0.05f));
                }

                return _carUpgradesProperties;
            }
        }

        public override float GetValue(int indexEnum)
        {
            SpeedUpgrades upgrades = (SpeedUpgrades)indexEnum;

            CarUpgradesValue value;

            switch (upgrades)
            {
                case SpeedUpgrades.MaxForwardSpeed:
                    CarUpgradesDictionary.TryGetValue("Max forward speed", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case SpeedUpgrades.MaxReverseSpeed:
                    CarUpgradesDictionary.TryGetValue("Max reverse speed", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case SpeedUpgrades.ForwardGrip:
                    CarUpgradesDictionary.TryGetValue("ForwardGrip", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;
            }

            Debug.LogError("Not right index");

            return 0;

        }

        public enum SpeedUpgrades
        {
            MaxForwardSpeed,
            MaxReverseSpeed,
            ForwardGrip
        }
    }
}