using System.Collections.Generic;
using Upgrades;

namespace Core.SaveSystem
{
    [System.Serializable]
    public class UpgradeLevelContainerWrapper
    {
        public List<UpgradeLevelContainer.UpgradeInfo> UpgradeLevels;

        public UpgradeLevelContainerWrapper()
        {
            UpgradeLevels = new List<UpgradeLevelContainer.UpgradeInfo>();
            UpgradesList mainUpgradesList = new UpgradesList();
            foreach (var item in mainUpgradesList.CarUpgradesList)
                UpgradeLevels.Add(new UpgradeLevelContainer.UpgradeInfo(item.GetType().Name, 10));
        }

        public UpgradeLevelContainer ToUpgradeLevelContainer()
        {
            var container = new UpgradeLevelContainer();
            container.UpgradeLevels.Clear();
            foreach (var item in UpgradeLevels)
            {
                container.UpgradeLevels.Add(item);
            }
            return container;
        }

        public void Copy(UpgradeLevelContainer copiedContainer)
        {
            UnityEngine.Debug.Log($"Copying from container: {UnityEngine.JsonUtility.ToJson(copiedContainer)}");
            UpgradeLevels.Clear();
            if (copiedContainer == null || copiedContainer.UpgradeLevels == null)
            {
                UnityEngine.Debug.LogWarning("Copied container or its UpgradeLevels is null!");
                return;
            }
            foreach (var item in copiedContainer.UpgradeLevels)
            {
                UpgradeLevels.Add(item);
            }
            UnityEngine.Debug.Log($"Result after copy: {UnityEngine.JsonUtility.ToJson(this)}");
        }

        public void UpdateFromMainUpgradeContainer(UpgradesList mainUpgradesList)
        {
            UpgradeLevelContainer.UpgradeInfo upgradeInfo;
            for (int i = 0; i < mainUpgradesList.CarUpgradesList.Count; i++)
            {
                upgradeInfo.UpgradeName = UpgradeLevels[i].UpgradeName;
                upgradeInfo.UpgradeIndex = mainUpgradesList.CarUpgradesList[i].UpgradeIndex;
                UpgradeLevels[i] = upgradeInfo;
            }
        }
    }
}