#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
	[CreateAssetMenu(fileName = "MirrorData", menuName = "CarBuilder/DataContainers/Mirror", order = 1)]
	public class MirrorData : ScriptableObject
	{
		[SerializeField] private Vector2[] _path;
		[SerializeField] private Vector2 _attachmentPoint;
		[SerializeField] private Vector2 _attachmentPointDirection;

		public Vector2 AttachmentPoint { get { return _attachmentPoint; }}
		public Vector2 AttachmentPointDirection {get { return _attachmentPointDirection; }}

		public void GetPath(ref Vector2[] value)
		{
			if (value == null || value.Length != _path.Length)
			{
				value = (Vector2[])_path.Clone();
			}
			else
			{
				value.CopyFrom(_path);
			}
		}

		public void SetPath(Vector2[] path, Vector2 attachmentPoint, Vector2 attachmentPointDirection)
		{
			_path = path;
			_attachmentPoint = attachmentPoint;
			_attachmentPointDirection = attachmentPointDirection;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
		}
	}
}
