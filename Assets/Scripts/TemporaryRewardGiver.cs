using Core;
using Core.SaveSystem;
using System;
using UI.Popup;
using UnityEngine;

public class TemporaryRewardGiver : MonoBehaviour
{
    [SerializeField] private UnlockableEquipment EquipmentUnlock;
    [SerializeField] private int upgradePoints;  
    private string chestId;     
    public string ChestId => chestId;             

    public event Action OnGiveReward;
    private void Awake()
    {
        if (string.IsNullOrEmpty(chestId))
            chestId = "Chest_" + transform.position.ToString();

        if (SaveManager.Progress != null)
        {
            var chestStates = SaveManager.Progress.ChestStates;
            if (chestStates.TryGetValue(chestId, out bool isPickedUp) && isPickedUp)
            {
                gameObject.SetActive(false);
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Chest '{chestId}' already picked up. Deactivated on Awake.");
            }
        }
    }

    public void GiveReward()
    {
        if (Player.Instance != null && SaveManager.Progress != null)
        {
            var progress = SaveManager.Progress;

            progress.ChestStates[chestId] = true;
            if (SaveManager.EnableSaveLoadDebugLogs)  Debug.Log($"Chest '{chestId}' picked up. New total points: {progress.UpgradeData.AvailablePoints}");
       
            string rewardText = "";

            if (upgradePoints > 0)
            {
                progress.UpgradeData.AvailablePoints += upgradePoints;

                rewardText += $"Upgrade points +{upgradePoints}\n";
            }

            if (EquipmentUnlock != UnlockableEquipment.None)
            {
                progress.Unlocks.UnlockEquipment(EquipmentUnlock.ToString());
                rewardText += $"Unlocked {EquipmentUnlock.ToString()}!";
            }

            SaveManager.SaveProgress();

            InfoPopup.Create(rewardText);

            OnGiveReward?.Invoke();

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Player or ProgressService not initialized. Reward not added.");
        }
    }
}