using Actors.Molds;
using Core;
using Core.SaveSystem;
using Upgrades.CarUpgrades;

namespace Upgrades
{
    public static class UpgradeConfigs
    {
        public const string PLAYER_CAR_UPGRADES_CONFIG_NAME = "CarUpgradesConfig.json";
        public const string UPGRADES_CONFIG_NAME = "PlayerUpgradesLevel.json";

        public static void SaveCarUpgrades(string nameOfJSONFile, UpgradesList data)
        {
            UpgradesList.UpgradesInfo containerInfo;
            containerInfo.CarUpgradesList = new();

            foreach (var parent in data.CarUpgradesList)
            {
                CarUpgradeBase.CarUpgradesInfo parentInfo;
                parentInfo.dictInfo = new();

                foreach (var itemDictionary in parent.CarUpgradesDictionary)
                    parentInfo.dictInfo.Add(itemDictionary.Key, itemDictionary.Value);

                containerInfo.CarUpgradesList.Add(parentInfo);
            }

            JSONParser.Save(nameOfJSONFile, containerInfo);
        }

        public static UpgradesList LoadCarUpgrades(string nameOfJSONFile)
        {
            var containerInfo = JSONParser.Load<UpgradesList.UpgradesInfo>(nameOfJSONFile);

            if (containerInfo.CarUpgradesList == null)
                return new UpgradesList();

            UpgradesList list = new UpgradesList();
            int index = 0;

            foreach (var parent in containerInfo.CarUpgradesList)
            {
                foreach (var itemDictionary in parent.dictInfo)
                    list.CarUpgradesList[index].CarUpgradesDictionary[itemDictionary.Key] = itemDictionary.Value;

                index++;
            }

            return list;
        }

        public static void SaveCarUpgradesIndexes(string nameOfJSONFile, UpgradesList data)
        {
            CarUpgradeBase.CarUpgradeIndex carUpgradeIndex = new();
            carUpgradeIndex.UpgradeIndexes = carUpgradeIndex.GetDefaultUpgrades();

            for (int i = 0; i < data.CarUpgradesList.Count; i++)
            {
                carUpgradeIndex.UpgradeIndexes[i].UpgradeIndex = data.CarUpgradesList[i].UpgradeIndex;
            }

            JSONParser.Save(nameOfJSONFile, carUpgradeIndex);
        }

        public static UpgradeLevelContainer LoadCarUpgradesIndexes(string nameOfJSONFile)
        {
            var carUpgradeIndex = JSONParser.Load<CarUpgradeBase.CarUpgradeIndex>(nameOfJSONFile);

            UpgradeLevelContainer upgradesContainer = new UpgradeLevelContainer();
            upgradesContainer.UpgradeLevels.Clear();

            foreach (var parent in carUpgradeIndex.UpgradeIndexes)
            {
                upgradesContainer.UpgradeLevels.Add(parent);
            }

            return upgradesContainer;
        }

        public static UpgradeLevelContainer LoadUpgrades(CarMold carMold)
        {
            UpgradeLevelContainer upgradesContainer;

            if (Player.Instance != null && SaveManager.Progress != null)
            {
                var progressUpgrades = SaveManager.Progress.UpgradeData.UpgradeLevels;

                upgradesContainer = progressUpgrades.ToUpgradeLevelContainer();
            }
            else
            {
                var upgradeLevels = carMold.GetUpgradesArray();
                upgradesContainer = new UpgradeLevelContainer();
                upgradesContainer.UpgradeLevels.Clear();
                foreach (var parent in upgradeLevels)
                {
                    upgradesContainer.UpgradeLevels.Add(parent);
                }
            }

            return upgradesContainer;
        }
    }
}