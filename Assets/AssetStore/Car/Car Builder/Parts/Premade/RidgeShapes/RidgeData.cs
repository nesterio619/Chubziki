#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    [CreateAssetMenu(fileName = "RidgeData", menuName = "CarBuilder/DataContainers/Ridge", order = 1)]
    public class RidgeData : ScriptableObject
    {
        [SerializeField] private Vector2[] _path;

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

		public void SetPath(Vector2[] path)
        {
            _path = path;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}
