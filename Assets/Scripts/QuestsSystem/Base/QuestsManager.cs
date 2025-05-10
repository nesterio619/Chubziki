using System.Collections.Generic;
using Core;
using QuestsSystem.QuestLogic;
using UI.Canvas;
using UI.Popup;
using UnityEngine;
using Core.Utilities;
using Core.SaveSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuestsSystem.Base
{
    public class QuestsManager : MonoBehaviour
    {
        private const string Configs_Path = "Quests";

        public static QuestsManager Instance { get; private set; }

        [SerializeField] private List<QuestConfig.QuestConfig> activeQuests = new List<QuestConfig.QuestConfig>();
        public List<QuestConfig.QuestConfig> ActiveQuests => activeQuests;

        private bool _isChoosingQuest;

        private static QuestConfig.QuestConfig[] allQuests;
        public static QuestConfig.QuestConfig[] AllQuests
        {
            get
            {
                if (allQuests == null) allQuests = Resources.LoadAll<QuestConfig.QuestConfig>(Configs_Path);

                return allQuests;
            }
        }

        /// <summary>
        /// Size of buttons need put in CellSize of Choice popup
        /// </summary>
        private float buttonWidth =300;
        private float buttonHeight = 150;



        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There are two instances of quest manager! This is wrong");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            foreach (var quest in AllQuests)
                quest.SetupLocationEvents();
        }

        #region Add or remove quests

        private void AddQuest(string questName)
        {
            if (TeleportUtilities.IsTeleportingWithAnimation) return;

            var quest = QuestConfig.QuestConfig.GetConfig(questName);

            if (quest == null || activeQuests.Contains(quest)) return;
            
            if (!quest.CanBeRestarted && IsQuestCompleted(questName))
            {
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Quest '{questName}' already completed, skipping activation.");
                return;
            }

            ///
            /// Wrong systax and not working code below
            ///


            if (quest is CircleRaceQuest)
            {
                foreach (var activeQuest in activeQuests)
                {
                    if (activeQuest is RaceQuest)
                        return;
                }
            }

            var questLogic = quest.QuestLogic;
            questLogic?.OnAccept();

            activeQuests.Add(quest);
        }

        public void RemoveQuest(string questName)
        {
            if(!IsQuestActive(questName))
                return;
            
            var quest = QuestConfig.QuestConfig.GetConfig(questName);

            if (quest == null || !activeQuests.Contains(quest)) return;
            
            if(quest.QuestLogic is BattleQuestLogicBase fightQuestLogic)
                fightQuestLogic.FinishBattle();
            else
                quest.QuestLogic.OnComplete(false);

            activeQuests.Remove(quest);
        }

        #endregion
        public void CompleteQuest(string questName, bool isCompleted)
        {
            if (Instance == null)
            {
                Debug.LogError("QuestsManager instance is not initialized!");
                return;
            }

            if (Player.Instance == null || SaveManager.Progress == null)
            {
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.LogError("Player instance or ProgressService is not initialized! Cannot complete quest.");
                return;
            }

            var questCompletion = SaveManager.Progress.QuestCompletion;

            if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Attempting to complete quest: {questName} (Completed: {isCompleted})");
            if (isCompleted)
            {
                if (questCompletion.ContainsKey(questName))
                {
                    questCompletion[questName] = true;
                    if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Quest '{questName}' marked as completed.");
                }
                else
                {
                    questCompletion.Add(questName, true);
                    if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Quest '{questName}' added and marked as completed.");
                }
        
                SaveManager.SaveProgress();
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Quest progress saved.");
            }
            else
            {
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log($"Quest '{questName}' not completed. Not saving.");
            }
        }

        public bool IsQuestCompleted(string questName)
        {
            if (Player.Instance == null || SaveManager.Progress == null)
            {
                if (SaveManager.EnableSaveLoadDebugLogs)Debug.LogWarning("Player instance or ProgressService not initialized. Assuming quest not completed.");
                return false;
            }

            return SaveManager.Progress.QuestCompletion.ContainsKey(questName) && 
                   SaveManager.Progress.QuestCompletion[questName];
        }

        public void DebugChooseAnyQuest()
        {
            CustomButtonController.ButtonMold[] buttonMolds =
            {
                new(
                    () => AddQuest("DefaultCircleRaceQuest"),
                    "Start Circle Race Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("DefaultSprintRaceQuest"),
                    "Start Sprint Race Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("DefaultSprintRaceQuest2"),
                    "Start Sprint Race Quest 2",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),

                new(
                    () => AddQuest("BowlingSprintRaceQuest"),
                    "Start Bowling Race Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("GiantBowlingQuest"),
                    "Start Giant Bowling Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("CarBowlingQuest"),
                    "Start Car Bowling Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("ObstacleSprintRaceQuest"),
                    "Start Obstacle Sprint Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("TestRegionSprintQuest"),
                    "Start Test Region Sprint Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("TestRegionCircleQuest"),
                    "Start Test Region Circle Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("TestRegionGiantBowlingQuest"),
                    "Start Test Region Giant Bowling Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),
                new(
                    () => AddQuest("TestRegionCarBowlingQuest"),
                    "Start Test Region Car Bowling Quest",
                    Color.blue,
                    new Vector2(buttonWidth, buttonHeight)
                ),

            };
            ChoicePopup.Create("Chose Quest To begin", buttonMolds, new Vector2Int(1480, 850), CanvasManager.Instance.PopupCanvas);
        }

        public void DebugChooseQuest(string questName)
        {
            if(_isChoosingQuest || IsQuestActive(questName))
                return;
            
            _isChoosingQuest = true;
            Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(true, true, true, true);
            
            
            DialoguePopup.Create($"Do you want to start '{questName}'?", 
                () => // Confirm action
                {
                    if(Instance == null)
                        return;
                    
                    _isChoosingQuest = false;
                    Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions();
                    
                    AddQuest(questName);
                }, 
                ()=> // Cancel action
                {
                    if(Instance == null)
                        return;
                    
                    _isChoosingQuest = false;
                    Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions();
                }, 
                new Vector2Int(550, 300), CanvasManager.Instance.PopupCanvas);
        }

        public void StartQuestImmediately(string questName)
        {
            AddQuest(questName);
        }

        public static bool AnyQuestIsActive() => Instance.activeQuests.Count > 0;

        public bool IsQuestActive(string questName)
        {
            foreach (QuestConfig.QuestConfig questConfig in activeQuests)
            {
                if (questConfig.QuestName.ToString() == questName.ToString())
                {
                    return true;
                }
            }

            //If doesn't exist
            return false;
        }
#if UNITY_EDITOR
        private static bool _showQuestElements = false;

        [MenuItem("Chubziki/Quests/Show all QuestElements", false, 1)]
        public static void ToggleAllQuestElements()
        {
            _showQuestElements = !_showQuestElements;

            foreach (var quest in AllQuests)
                quest.SetQuestElementsVisibility(_showQuestElements);
        }

        [MenuItem("Chubziki/Quests/Show all QuestElements", true)]
        public static bool ToggleAllQuestElementsValidate()
        {
            Menu.SetChecked("Chubziki/Quests/Show all QuestElements", _showQuestElements);
            return true;
        }

        [MenuItem("Chubziki/Quests/Open quest folder", false, 3)]
        public static void OpenQuestFolder()
        {
            if (AllQuests.Length > 0)
            {
                EditorGUIUtility.PingObject(AllQuests[0]);
            }
            else
            {
                string folderPath = "Assets/Resources/" + Configs_Path; 

                Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                if (folder != null)
                {
                    Selection.activeObject = folder;
                    EditorGUIUtility.PingObject(folder);
                }
            }
        }

        public static void CreateAllQuestEditorElements()
        {
            foreach(var quest in AllQuests)
                quest.CreateEditorElements();
        }
        public static void DestroyAllQuestEditorElements()
        {
            foreach (var quest in AllQuests)
                quest.DestroyAllEditorElements();
        }
#endif
    }
}