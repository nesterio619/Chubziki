using System.Collections.Generic;
using UnityEngine;

namespace Upgrades.CarUpgrades
{
    [System.Serializable]
    public class SurviveCarUpgrade : CarUpgradeBase
    {
        private Dictionary<string, CarUpgradesValue> _carUpgradesProperties = new();

        public override Dictionary<string, CarUpgradesValue> CarUpgradesDictionary
        {
            get
            {
                if (_carUpgradesProperties.Count == 0)
                {
                    _carUpgradesProperties.Add("Weight", new CarUpgradesValue(790, 9));
                    _carUpgradesProperties.Add("Health", new CarUpgradesValue(50, 5));
                }

                return _carUpgradesProperties;
            }
        }

        public override float GetValue(int indexEnum)
        {
            SurviveUpgrades upgrades = (SurviveUpgrades)indexEnum;

            CarUpgradesValue value;

            switch (upgrades)
            {
                case SurviveUpgrades.Weight:
                    CarUpgradesDictionary.TryGetValue("Weight", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;

                case SurviveUpgrades.Health:
                    CarUpgradesDictionary.TryGetValue("Health", out value);
                    return value.BaseValue + value.ValuePerUpgrade * UpgradeIndex;
            }

            Debug.LogError("Not right index");

            return 0;

        }

        public enum SurviveUpgrades
        {
            Weight,
            Health
        }

    }
}