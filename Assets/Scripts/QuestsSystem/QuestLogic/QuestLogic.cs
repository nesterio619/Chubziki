using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Core;
using Core.ObjectPool;
using Core.SaveSystem;
using Core.Utilities;
using Core.Extensions;
using MicroWorldNS;
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using QuestsSystem.QuestsElements;
using Regions;
using UI.Debug;
using UI.Popup;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public struct QuestElementAndTransform
    {
        public Transform ElementTransform; // Path in the scene hierarchy
        public PrefabPoolInfo PoolInfo_PrefabPoolInfo; // The ScriptableObject _PrefabPoolInfo
    }

    public abstract class QuestLogic : IDisposable
    {
        protected string QuestName { get; private set; }
        protected string QuestDescriptionText { get; private set; }
        
        protected Dictionary<string, (Transform, PrefabPoolInfo)> TransformsAndPools { get; private set; } =
            new Dictionary<string, (Transform, PrefabPoolInfo)>();

        [SerializeField] protected string questLocationPath;
        [SerializeField] protected Location questLocation;
        [SerializeField] protected ActorPresetWithPath[] actorSpawnPresets;
        public List<Actor> AllCreatedActors = new List<Actor>();
        public List<bool> ActorsSavePositions = new List<bool>();

        protected Transform startTransform = null;
        //=----=//

        protected List<Action> questLoadedCallbacks = new List<Action>();

        protected DebugCanvasCommand debugCanvasCommand;
        protected List<QuestElement> CreatedQuestElements = new List<QuestElement>();

        private Transform _wherePlayerAcceptedQuest;

        private bool _playerDiedDuringQuest = false;

        private PlayerCarActor _playerCarActor = Player.Instance == null ? null : Player.Instance.PlayerCarGameObject;

        protected bool questIsCompleted = false;
        
        protected bool questIsOver = false;

        protected bool _showDebugPopupOnStart;
        protected bool _showDebugPopupOnFinish;

        protected string _rewardUnlockName;
        protected int _rewardUpgradePoints;

        protected bool enableActorLogic = false;

        public virtual void  Initialize(QuestConfig.QuestConfig questConfig)
        {
            QuestName = questConfig.QuestName;
            QuestDescriptionText = questConfig.QuestDescription;
            
            questIsCompleted = false;
            questIsOver = false;
            _playerDiedDuringQuest = false;
            _showDebugPopupOnStart = questConfig.ShowDebugPopupOnStart;
            _showDebugPopupOnFinish = questConfig.ShowDebugPopupOnFinish;

            _rewardUnlockName = questConfig.UnlockName;
            _rewardUpgradePoints = questConfig.UpgradePoints;
            
            startTransform = string.IsNullOrEmpty(questConfig.StartPositionPath) ? null : GameObject.Find(questConfig.StartPositionPath).transform;
            
            //=-- Find quest elements transforms
            var transformsAndPoolsArray = TryGetQuestElementsAndTransforms(questConfig.QuestElementsAndTransformsPaths.ToArray());

            if (transformsAndPoolsArray != null && transformsAndPoolsArray.Length > 0)
            {
                TransformsAndPools.Clear();
                
                foreach (var poolInfoAndTransform in transformsAndPoolsArray)
                    if (!ObjectPooler.PoolExist(poolInfoAndTransform.PoolInfo_PrefabPoolInfo.PoolName))
                        ObjectPooler.CreatePool(poolInfoAndTransform.PoolInfo_PrefabPoolInfo);

                foreach (var questElementAndTransform in transformsAndPoolsArray)
                    TransformsAndPools[questElementAndTransform.ElementTransform.name] = (questElementAndTransform.ElementTransform, questElementAndTransform.PoolInfo_PrefabPoolInfo);
            }
            //=--
            
            //=-- Find actor transforms
            //=- Find location for actors
            if (!questConfig.QuestLocationPath.IsNullOrEmpty())
            {
                questLocationPath = questConfig.QuestLocationPath;
                questLocation = GetQuestLocation(questLocationPath);
            }

            var newActorSpawnPresets = questConfig.GetActorsToSpawn();
            if (newActorSpawnPresets != null && newActorSpawnPresets.Length > 0)
            {
                UnloadAllCreatedActors();

                actorSpawnPresets = newActorSpawnPresets;
            }
            //=--
        }

        protected Location GetQuestLocation(string locationPath)
        {
            var locationGameObject = GameObject.Find(locationPath);
            if (locationGameObject == null) return null;

            return locationGameObject.GetComponent<Location>();
        }

        private QuestElementAndTransform[] TryGetQuestElementsAndTransforms(QuestElementAndTransformPath[] elementAndTransformsPaths)
        {
            if (elementAndTransformsPaths == null || elementAndTransformsPaths.Length == 0)
                return null;
            
            var elements = new List<QuestElementAndTransform>();

            foreach (var item in elementAndTransformsPaths)
            {
                var transformOfElement = GameObject.Find(item.TransformPath).transform;
                if (transformOfElement != null)
                {
                    elements.Add(new QuestElementAndTransform
                    {
                        ElementTransform = transformOfElement,
                        PoolInfo_PrefabPoolInfo = item._PrefabPoolInfo
                    });
                }
            }
            return elements.ToArray();
        }

        #region Quest progression

        protected internal virtual void OnAccept()
        {
            questLoadedCallbacks.Add(InitializeQuestElements);

#if UNITY_EDITOR
            questLoadedCallbacks.Add(CreateDebugUI);
#endif

            if (startTransform == null)
                InvokeQuestLoadedCallbacks();
            else
                TeleportPlayerToStart(InvokeQuestLoadedCallbacks);
            
            _playerCarActor = Player.Instance.PlayerCarGameObject;
            _playerCarActor.OnNoHealthLeft += OnPlayerDeath;

            _wherePlayerAcceptedQuest = new GameObject("LastPlayerPosition").transform;
            _wherePlayerAcceptedQuest.ApplyPose(_playerCarActor.LastSafePose);

            if(!QuestsManager.AnyQuestIsActive())
                SaveManager.Progress.PlayerTransformData.SavePlayerPose(_playerCarActor.LastSafePose);

            if (_showDebugPopupOnStart)
                InfoPopup.Create($"Active quest: {QuestName}", displayTime: 2.5f);
        }

        private void OnPlayerDeath()
        {
            _playerDiedDuringQuest = true;

            OnComplete(false);

            Player.Instance.RespawnPlayer();
        }

        void InvokeQuestLoadedCallbacks()
        {
            foreach (var callback in questLoadedCallbacks)
                callback?.Invoke();

            questLoadedCallbacks.Clear();
        }

        public virtual void OnComplete(bool victory)
        {
            if (questIsOver) 
                return;

            var textToDisplay = victory ? "has been completed!" : "has been lost. Try again!";

            if (victory)
            {
                GiveReward(out string rewardText);
                textToDisplay += rewardText;
            }
                
            if(_showDebugPopupOnFinish)
                InfoPopup.Create($"{QuestName} {textToDisplay}", anchor: InfoPopup.PopupAnchor.TopLeft);
            QuestsManager.Instance.CompleteQuest(QuestName, victory);


            questIsCompleted = victory;
            questIsOver = true;

            Dispose();
        }

        private void GiveReward(out string rewardText)
        {
            rewardText = "";

            if(_rewardUpgradePoints > 0)
            {
                SaveManager.Progress.UpgradeData.AvailablePoints += _rewardUpgradePoints;
                rewardText += $"Upgrade points +{_rewardUpgradePoints}\n";
            }

            if (!string.IsNullOrEmpty(_rewardUnlockName))
            {
                SaveManager.Progress.Unlocks.UnlockEquipment(_rewardUnlockName);
                rewardText += $"Unlocked {_rewardUnlockName}!";
            }

            SaveManager.SaveProgress();
        }

        public virtual void Dispose()
        {
            if (debugCanvasCommand != null)
            {
                debugCanvasCommand.Dispose();
                debugCanvasCommand = null;
            }

            UnloadQuestElements();

            if(_wherePlayerAcceptedQuest!=null)
                UnityEngine.Object.Destroy(_wherePlayerAcceptedQuest.gameObject);

            QuestsManager.Instance.RemoveQuest(QuestName);

            _playerCarActor.OnNoHealthLeft -= OnPlayerDeath;

            _playerDiedDuringQuest = false;
            
            questIsCompleted = false;
            questIsOver = false;

            for (int i = 0; i < AllCreatedActors.Count; i++)
                if (ActorsSavePositions[i])
                    questLocation.SaveActorPose(AllCreatedActors[i].gameObject);
        }

        #endregion

        #region Quest elements

        private void InitializeQuestElements()
        {
            CreatedQuestElements.Clear();

            foreach (var transformAndPool in TransformsAndPools.Values)
            {
                var pooledObject = ObjectPooler.TakePooledGameObject(transformAndPool.Item2);

                var questElement = pooledObject as QuestElement;
                if (questElement == null)
                {
                    questElement = pooledObject.GetComponent<QuestElement>();

                    if (questElement == null)
                    {
                        Debug.LogError($"Couldn't find Quest Element component in {pooledObject.gameObject.name}");
                        return;
                    }
                }

                var elementType = questElement.GetType();
                if (QuestElementSetupActions.TryGetValue(elementType, out var setupAction))
                    setupAction(questElement, transformAndPool.Item1);

                CreatedQuestElements.Add(questElement);
            }
        }
        protected virtual void UnloadQuestElements()
        {
            foreach (var questElement in CreatedQuestElements)
                questElement.ReturnToPool();

            CreatedQuestElements.Clear();
        }

        protected virtual Dictionary<Type, Action<QuestElement, Transform>> QuestElementSetupActions => new Dictionary<Type, Action<QuestElement, Transform>>
        {
            {
                typeof(QuestElement), (questElement, transformToSet) =>
                {
                    questElement.transform.SetParent(transformToSet);
                    questElement.transform.SetPositionAndRotation(transformToSet.position, transformToSet.rotation);
                    questElement.SetQuestName(QuestName);;
                }
            }
        };

        #endregion

        #region Quest actors

        public virtual void SpawnAllActors()
        {
            if (questLocation == null)
                questLocation = GetQuestLocation(questLocationPath);

            AllCreatedActors.Clear();
            ActorsSavePositions.Clear();
            if (actorSpawnPresets != null && actorSpawnPresets.Length > 0)
                foreach (var actorSpawnPreset in actorSpawnPresets)
                {
                    if(actorSpawnPreset.Mold==null) continue;
                    CreateActors(questLocation, actorSpawnPreset);
                }
                    
        }

        protected void CreateActors(Location location, ActorPresetWithPath actorSpawnPreset)
        {
            if (location == null || !location.IsLoaded
                                || actorSpawnPreset.TransformPaths == null || actorSpawnPreset.TransformPaths.Count == 0)
                return;

            var questCompleted = QuestsManager.Instance.IsQuestCompleted(QuestName);
            var createdActors = location.CreateActors(actorSpawnPreset, true, enableActorLogic, questCompleted);
            if (createdActors == null || createdActors.Length == 0)
            {
                Debug.LogError("Couldn't create actors for quest!");
                return;
            }

            foreach (var createdActor in createdActors)
            {
                AllCreatedActors.Add(createdActor);
                ActorsSavePositions.Add(actorSpawnPreset.SavePosition);
            }
        }
        protected void UnloadAllCreatedActors()
        {
            if(AllCreatedActors == null || AllCreatedActors.Count == 0)
                return;

            questLocation.UnloadActors(AllCreatedActors.ToArray());
            AllCreatedActors.Clear();
            ActorsSavePositions.Clear();
        }

        public void ToggleActorLogic(bool enable)
        {
            return;

            if (enable) return;

            foreach (var actor in AllCreatedActors)
                if(actor != null) actor.ToggleLogic(false);
        }
        #endregion

        protected virtual void CreateDebugUI()
        {
            debugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"Active quest: {QuestName}");

            debugCanvasCommand.SetTextFontSize(50);

            debugCanvasCommand.Update();
        }

        private void TeleportPlayerToStart(Action afterTeleportAction)
        {
            TeleportUtilities.TeleportAnimation(Player.Instance.PlayerCarGameObject.transform, startTransform.position,
                startTransform.rotation, afterTeleportAction);
        }

        protected void TeleportPlayerToWhereQuestWasAccepted(Action afterTeleportAction = null)
        {
            if (_playerDiedDuringQuest) return;

            TeleportUtilities.TeleportAnimation(Player.Instance.PlayerCarGameObject.transform, _wherePlayerAcceptedQuest.position,
                _wherePlayerAcceptedQuest.rotation, afterTeleportAction);
        }
    }
}