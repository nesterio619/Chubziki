using UnityEngine;

namespace Actors.EditorActors
{
    [ExecuteInEditMode]
    public class EditorSceneElement : MonoBehaviour
    {
#if UNITY_EDITOR
        private Vector3 initialPosition;

        protected virtual void Start()
        {
            // Saving start position of object while creating or loading
            initialPosition = transform.localPosition;
        }

        protected virtual void Update()
        {
            // If object move, keep him back on last position
            if (transform.localPosition != initialPosition)
            {
                transform.localPosition = initialPosition;
            }
        }

#endif
    }
}