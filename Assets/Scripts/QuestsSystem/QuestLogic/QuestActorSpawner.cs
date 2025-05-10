using Core.Enums;
using QuestsSystem.QuestConfig;
using System.Collections.Generic;
using UnityEngine;
using static Core.Extensions.TransformExtensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuestsSystem
{
    public static class QuestActorSpawner
    {
        private const float Ray_Origin_Height = 20;
        private const float Ray_Length = 40;
        private const int Maximum_Attempts = 50;

        public static List<ActorPresetWithPath> GenerateSpawnPositions(Transform center, ActorSpawnInfo spawnInfo)
        {
            var presets = new List<ActorPresetWithPath>();

            #if UNITY_EDITOR
            Debug.Log("Generating positions...");

            center.ClearChildren();

            foreach (var element in spawnInfo.MoldCounts)
            {
                var preset = new ActorPresetWithPath(spawnInfo.CenterTransformPath, element.Mold);
                int attempts = 0;

                for (int i = 0; i < element.Count;)
                {
                    attempts++;
                    if (attempts > Maximum_Attempts)
                    {
                        Debug.LogError("Too many raycast attempts. Make sure the area inside the circle has ground.");
                        return presets;
                    }

                    var raycastOrigin = GetRandomPosition(center.position,spawnInfo.Radius);

                    if(!GroundRaycast(raycastOrigin, out RaycastHit hit)) continue;

                    var actorTransform = CreateTransform(center, hit.point);
                    actorTransform.name = $"{element.Mold.name.Replace("Mold", "Transform")}.{i}";

                    preset.TransformPaths.Add($"{spawnInfo.CenterTransformPath}/{actorTransform.name}");

                    attempts = 0;
                    i++;
                }

                presets.Add(preset);
            }

            EditorUtility.SetDirty(center);

            Debug.Log("Positions generated successfully.");
            #endif

            return presets;
        }

        private static bool GroundRaycast(Vector3 origin, out RaycastHit hit)
        {
            var success = Physics.Raycast(origin, -Vector3.up, 
                out hit, Ray_Length, (int)UnityLayers.Environment);

            if (!success) return false;
            if (hit.transform.parent.name != "Ground") return false;

            return true;
        }

        private static Vector3 GetRandomPosition(Vector3 center, float radius)
        {
            var randomPos = Random.insideUnitCircle * radius;
            return center + new Vector3(randomPos.x, Ray_Origin_Height, randomPos.y);
        }

        private static Transform CreateTransform(Transform parent, Vector3 position)
        {
            var actorTransform = new GameObject().transform;

            actorTransform.SetParent(parent);
            actorTransform.position = position;

            return actorTransform;
        }
    }
}
