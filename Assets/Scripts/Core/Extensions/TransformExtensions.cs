using UnityEngine;

namespace Core.Extensions
{
    public static class TransformExtensions
    {
        public static void ClearChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(transform.GetChild(i).gameObject);

                #if UNITY_EDITOR
                else GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
                #endif
            }

        }

        public static void ApplyPose(this Transform transform, Pose pose)
            => transform.SetPositionAndRotation(pose.position, pose.rotation);

        public static Pose GetPose(this Transform transform)
            => new Pose(transform.position, transform.rotation);
    }
}
