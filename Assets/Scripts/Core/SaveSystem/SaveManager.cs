using System;
using System.Collections.Generic;
using Core.Enums;
using Transition;
using UnityEngine;

namespace Core.SaveSystem
{
    public static class SaveManager
    {
        public const string LAST_SLOT_KEY = "last_save_slot";
        public const string LAST_SLOT_FILE = "last_slot.es3";

        public static event Action OnSaveCompleted;

        public static PlayerProgress Progress = new PlayerProgress();

        public static readonly bool EnableSaveLoadDebugLogs = false;

        public static void SaveProgress()
        {
            string saveName = GetCurrentSaveSlot();
            string saveFilePath = $"{saveName}.es3";

            ES3.Save("playerProgress", Progress, saveFilePath);
            ES3.Save(LAST_SLOT_KEY, saveName, LAST_SLOT_FILE);

            if (EnableSaveLoadDebugLogs) Debug.Log($"Save to{saveFilePath}");
            OnSaveCompleted?.Invoke();
        }

        public static bool LoadProgress()
        {
            string saveName = GetCurrentSaveSlot();
            string saveFilePath = $"{GetCurrentSaveSlot()}.es3";

            if (ES3.FileExists(saveFilePath))
            {
                try
                {
                    Progress = ES3.Load<PlayerProgress>("playerProgress", saveFilePath);
                    ES3.Save(LAST_SLOT_KEY, saveName, LAST_SLOT_FILE);
                    if (EnableSaveLoadDebugLogs) Debug.Log($"Progress load to: {saveFilePath}");
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error load save: {saveName}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"File doesn't exist: {saveFilePath}");
            }

            return false;
        }
        public static bool ContinueGame()
        {
            string lastSlot = GetCurrentSaveSlot();
            if (string.IsNullOrEmpty(lastSlot) || !ES3.FileExists($"{lastSlot}.es3"))
            {
                if (GetSaveFiles().Count == 0)
                {
                    if (EnableSaveLoadDebugLogs) Debug.Log("First launch: no conservation.");
                    return false;
                }
                Debug.LogWarning("There is no last saved cell or a file is absent!");
                return false;
            }

            SetCurrentSaveSlot(lastSlot);
            bool loaded = LoadProgress();
            if (loaded)
                return true;

            Debug.LogWarning($"The load of the last cell: {lastSlot}");
            return false;
        }
        public static string GetCurrentSaveSlot()
        {
            if (ES3.FileExists(LAST_SLOT_FILE) && ES3.KeyExists(LAST_SLOT_KEY, LAST_SLOT_FILE))
            {
                return ES3.Load<string>(LAST_SLOT_KEY, LAST_SLOT_FILE);
            }
            return "";
        }

        public static List<string> GetSaveFiles()
        {
            var allFiles = ES3.GetFiles();
            var saveFiles = new List<string>();

            foreach (var file in allFiles)
            {
                if (file.EndsWith(".es3", StringComparison.OrdinalIgnoreCase) && file != LAST_SLOT_FILE)
                {
                    saveFiles.Add(file);
                }
            }
            return saveFiles;
        }
        public static void SetCurrentSaveSlot(string saveSlotName)
        {
            if (string.IsNullOrEmpty(saveSlotName))
            {
                Debug.LogWarning("the name of the conservation slot cannot be empty");
                return;
            }

            ES3.Save(LAST_SLOT_KEY, saveSlotName, LAST_SLOT_FILE);
            if (EnableSaveLoadDebugLogs) Debug.Log($"the current conservation slot is changed to {saveSlotName}");
        }
        public static void ResetProgress()
        {
            Progress = new PlayerProgress();
        }
    }
}