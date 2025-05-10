using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class RidgeCreatorHelper : MonoBehaviour
    {
        [SerializeField] private RidgeData _dataStorage = default;
        [Space]
        [SerializeField] private Transform _pathHolder = default;
        [SerializeField] private float _gizmoSize = 0.02f;

        [ContextMenu("Save")]
        private void Save()
        {
            var path = new Vector2[_pathHolder.childCount];

            for (int i = 0; i < _pathHolder.childCount; i++)
            {
                path[i] = _pathHolder.GetChild(i).localPosition.ZY();
            }

            _dataStorage.SetPath(path);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            //left / right
            Gizmos.DrawLine(Vector3.up + Vector3.forward, Vector3.forward);
            Gizmos.DrawLine(Vector3.up, Vector3.zero);

            //top / bottom
            Gizmos.DrawLine(Vector3.up, Vector3.forward + Vector3.up);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);

            if (_pathHolder == null || _pathHolder.childCount < 2)
                return;

            Gizmos.color = Color.red;
            for (int i = 0; i < _pathHolder.childCount - 1; i++)
                Gizmos.DrawLine(_pathHolder.GetChild(i).position, _pathHolder.GetChild(i + 1).position);

            for (int i = 0; i < _pathHolder.childCount; i++)
                Gizmos.DrawWireSphere(_pathHolder.GetChild(i).position, _gizmoSize);
        }
    }
}
