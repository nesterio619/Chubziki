using Core.SaveSystem;
using System.Collections.Generic;

namespace Upgrades
{
    [System.Serializable]
    public class UpgradeLevelContainer
    {
        public List<UpgradeInfo> UpgradeLevels = new();

        public UpgradeLevelContainer()
        {
            UpgradesList mainUpgradesList = new();
            foreach (var item in mainUpgradesList.CarUpgradesList)
                UpgradeLevels.Add(new UpgradeInfo(item.GetType().Name, 10));
        }

        public void UpdateFromMainUpgradeContainer(UpgradesList mainUpgradesList)
        {
            UpgradeInfo upgradeInfo;
            for (int i = 0; i < mainUpgradesList.CarUpgradesList.Count; i++)
            {
                upgradeInfo.UpgradeName = UpgradeLevels[i].UpgradeName;
                upgradeInfo.UpgradeIndex = mainUpgradesList.CarUpgradesList[i].UpgradeIndex;
                UpgradeLevels[i] = upgradeInfo;
            }
        }

        public void Copy(UpgradeLevelContainer copiedContainer)
        {
            if (SaveManager.EnableSaveLoadDebugLogs)    UnityEngine.Debug.Log($"Copying from container: {UnityEngine.JsonUtility.ToJson(copiedContainer)}");
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
            if (SaveManager.EnableSaveLoadDebugLogs) UnityEngine.Debug.Log($"Result after copy: {UnityEngine.JsonUtility.ToJson(this)}");
        }

        [System.Serializable]
        public struct UpgradeInfo
        {
            public string UpgradeName;
            public int UpgradeIndex;

            public UpgradeInfo(string name, int index)
            {
                UpgradeName = name;
                UpgradeIndex = index;
            }
        }
    }
}