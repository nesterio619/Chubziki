using System.Collections.Generic;
using System.Linq;
using Core.Utilities;
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using Regions;
using Regions.Editors;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement; 
#endif

namespace Core.SceneControl
{
    [ExecuteAlways]
    public class SceneExceptionFinder : MonoBehaviour
    {
        [SerializeField] private bool checkLocations;
        [SerializeField] private bool checkQuests;
        [Space]
        [SerializeField] private string questFolderPath = "Assets/Resources/Quests";
    
        public static SceneExceptionFinder Instance { get; private set; }
    
        private void Awake()
        {
            if (Application.isPlaying)
                Destroy(this);

#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
#endif

            if (Instance != null && Instance != this)
                DestroyImmediate(this);
            else
                Instance = this;
        }
    
        public bool CheckForExceptions()
        {
            if (!ConsoleLogHelper.LogIsStarted)
                ConsoleLogHelper.TryStartNewLog();
            
            List<Object> faultyObjects = new List<Object>();
            faultyObjects.AddRange(CheckLocations());
            faultyObjects.AddRange(CheckQuests());

            if (!SceneManager.IsChangingScene && faultyObjects.Count > 0)
            {
#if UNITY_EDITOR
                SceneExceptionFinderEditorWindow.Show(faultyObjects);
#endif
                ConsoleLogHelper.Log("\nReference fixer opened.");

                return true;
            }

            return false;
        }
    
        private List<Object> CheckLocations()
        {
            List<Object> faultyLocations = new();
            if (!checkLocations) return faultyLocations;

            ConsoleLogHelper.Log("\nChecking locations...");

            foreach (var region in FindObjectsOfType<Region>())
            foreach (var sector in region.Sectors)
            foreach (var location in sector.Locations)
                if (location.HasEmptyReferences())
                {
                    faultyLocations.Add(location.gameObject);
                    ConsoleLogHelper.Log($" - [{location.name}] has empty references;");
                }

            ConsoleLogHelper.Log($"{faultyLocations.Count} locations are faulty.");

            return faultyLocations;
        }
    
        private List<Object> CheckQuests()
        {
            var faultyQuests = new List<Object>();
            if (!checkQuests) return faultyQuests;

            ConsoleLogHelper.Log($"\nChecking quests in {questFolderPath}...");

            var quests = Resources.LoadAll<QuestConfig>(questFolderPath.Replace("Assets/Resources/", ""));

            if(quests.Length == 0)
            { 
                ConsoleLogHelper.Log("Quests not found.");
                return faultyQuests;
            }

            foreach (var quest in quests)
            {
                var startTransform = UtilitiesProvider.GetTransformFromPath(quest.StartPositionPath);
                if (startTransform == null)
                {
                    ConsoleLogHelper.Log($" - [{quest.name}] has invalid StartPositionPath;");

                    TryFindInvalidTransformPath(quest.StartPositionPath, out string newPath);

                    quest.StartPositionPath = newPath;
                    if (newPath == null) faultyQuests.Add(quest);
                }

                var list = quest.QuestElementsAndTransformsPaths;
                for (int i = 0; i< list.Count; i++)
                {
                    if (list[i]._PrefabPoolInfo == null)
                    {
                        faultyQuests.Add(quest);
                        ConsoleLogHelper.Log($" - [{quest.name}] at index {i} PrefabPoolInfo is null;");
                    }

                    var transform = UtilitiesProvider.GetTransformFromPath(list[i].TransformPath);
                    if (transform != null) continue;

                    ConsoleLogHelper.Log($" - [{quest.name}] at index {i} has invalid TransformPath;");

                    TryFindInvalidTransformPath(list[i].TransformPath, out string newPath);

                    var newValues = list[i];
                    newValues.TransformPath = newPath;
                    list[i] = newValues;

                    if(newPath==null) faultyQuests.Add(quest);
                }
            }

            var result = faultyQuests.Distinct().ToList();
            ConsoleLogHelper.Log($"{result.Count} quests are faulty.");

            return result;
        }
        
        private static void TryFindInvalidTransformPath(string path, out string newPath) 
        {
            var splitPath = path.Split('/');
            var transformName = splitPath[^1];

            var gameObject = GameObject.Find(transformName);

            newPath = gameObject != null ? UtilitiesProvider.GetGameObjectPath(gameObject) : null;
            
            if (newPath != null)
            {
                ConsoleLogHelper.LogIndent(1,$"- Found this transform on a different path:");
                ConsoleLogHelper.LogIndent(2,$"Old path: {path}");
                ConsoleLogHelper.LogIndent(2,$"New path: {newPath}");
            }
        }
    }
}