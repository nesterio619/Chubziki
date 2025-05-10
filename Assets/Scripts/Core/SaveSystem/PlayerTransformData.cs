using Core.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    [Serializable]
    public class PlayerTransformData
    {
        [SerializeField] private static List<ScenePose> _scenePoses;

        public PlayerTransformData()
        {
            _scenePoses = new List<ScenePose>();
        }

        public void SavePlayerPose(Pose pose)
        {
            if (SaveManager.EnableSaveLoadDebugLogs) Debug.Log("Saving PlayerPose: " + pose.ToString());
            
            int currentSceneIndex = SceneManager.CurrentSceneConfig.SceneIndex;
            int listIndex = _scenePoses.FindIndex(x => x.SceneIndex == currentSceneIndex);

            var scenePose = new ScenePose(currentSceneIndex, pose);

            if (listIndex != -1)
            {
                _scenePoses[listIndex] = scenePose;
            }
            else
            {
                _scenePoses.Add(scenePose);
            }

            SaveManager.SaveProgress();
        }

        public bool TryGetPlayerPose(out Pose pose)
        {
            int currentSceneIndex = SceneManager.CurrentSceneConfig.SceneIndex;
            int listIndex = _scenePoses.FindIndex(x => x.SceneIndex == currentSceneIndex);

            if (listIndex != -1)
            {
                pose = _scenePoses[listIndex];
                return true;
            }
            else
            {
                var playerTransform = Player.Instance.PlayerCarGameObject.transform;
                SavePlayerPose(playerTransform.GetPose());

                pose = Pose.identity;

                return false;
            }
        }

        [Serializable]
        public struct ScenePose
        {
            public int SceneIndex;
            public Vector3 Position;
            public Quaternion Rotation;

            public ScenePose(int sceneIndex, Pose pose)
            {
                SceneIndex = sceneIndex;
                Position = pose.position;
                Rotation = pose.rotation;
            }

            public static implicit operator Pose(ScenePose scenePose)
                => new Pose(scenePose.Position, scenePose.Rotation);
        }
    }
}