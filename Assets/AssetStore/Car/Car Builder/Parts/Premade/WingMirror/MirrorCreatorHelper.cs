using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
	public class MirrorCreatorHelper : MonoBehaviour
	{
		[SerializeField] private MirrorData _dataStorage = default;
		[Space]
		[SerializeField] private Transform _pathHolder = default;
		[SerializeField] private Transform _attachmentPoint = default;
		[SerializeField] private Transform _attachmentPointDirectionReference = default;
		[Space] [SerializeField] private float _gizmoSize = 0.02f;

		[ContextMenu("Save")]
		private void Save()
		{
			var path = new Vector2[_pathHolder.childCount];

			for (int i = 0; i < _pathHolder.childCount; i++)
			{
				path[i] = _pathHolder.GetChild(i).localPosition.ZY();
			}

			_dataStorage.SetPath(path, _attachmentPoint.localPosition.XY(), (_attachmentPointDirectionReference.localPosition - _attachmentPoint.localPosition).normalized.XY());
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			//left / right
			Gizmos.DrawLine(Vector3.forward + Vector3.up, Vector3.forward - Vector3.up);
			Gizmos.DrawLine(-Vector3.forward + Vector3.up, -Vector3.forward - Vector3.up);

			//top / bottom
			Gizmos.DrawLine(-Vector3.forward + Vector3.up, Vector3.forward + Vector3.up);
			Gizmos.DrawLine(-Vector3.forward - Vector3.up, Vector3.forward - Vector3.up);

			if (_pathHolder == null || _pathHolder.childCount < 2)
				return;

			Gizmos.color = Color.red;
			for (int i = 0; i < _pathHolder.childCount - 1; i++)
				Gizmos.DrawLine(_pathHolder.GetChild(i).position, _pathHolder.GetChild(i + 1).position);

			for (int i = 0; i < _pathHolder.childCount; i++)
				Gizmos.DrawWireSphere(_pathHolder.GetChild(i).position, _gizmoSize);

			Gizmos.DrawLine(_pathHolder.GetChild(0).position, _pathHolder.GetChild(_pathHolder.childCount - 1).position);

			Gizmos.color = Color.green;
			if (_attachmentPoint != null) Gizmos.DrawWireSphere(_attachmentPoint.position, _gizmoSize * 1.5f);

			Gizmos.color = Color.yellow;
			if (_attachmentPointDirectionReference != null)
			{
				Gizmos.DrawWireSphere(_attachmentPointDirectionReference.position, _gizmoSize * 1.5f);
				Gizmos.DrawLine(_attachmentPoint.position, _attachmentPointDirectionReference.position);
			}
		}
	}
}
