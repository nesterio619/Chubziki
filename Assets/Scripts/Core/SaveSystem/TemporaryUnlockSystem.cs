using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    public enum UnlockableEquipment { None, Turret, SawBlade, RammingBar }

    [Serializable]
    public class TemporaryUnlockSystem
    {
        [SerializeField] private List<string> unlockedEquipment;

        public TemporaryUnlockSystem()
        {
            unlockedEquipment = new List<string>();
        }

        public void UnlockEquipment(string unlockName)
        {
            if (string.IsNullOrEmpty(unlockName)) return;
            if (unlockName == UnlockableEquipment.None.ToString()) return;

            if (!unlockedEquipment.Contains(unlockName))
                unlockedEquipment.Add(unlockName);
        }

        public bool IsEquipmentUnlocked(UnlockableEquipment unlock)
            => unlockedEquipment.Contains(unlock.ToString());

    }
}
