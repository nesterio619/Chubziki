#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Core.ObjectPool;

namespace ChubzikiUnityEditor.EditorWindows
{
    public class PoolCreatorWindow : EditorWindow
    {
        private Object _prefabForPool = null;

        private Object _checkPrefab = null;

        private string _poolPath = "";

        private string _poolName = "";

        private string _pathOfPooledObject = "";

        private int _defaultAmount = 1;

        private float _maxAmountMultiplier = 2;

        private string unnededPath = "Assets/Resources/";

        [MenuItem("Chubziki/PoolCreatorWindow")]
        public static void ShowWindow()
        {
            GetWindow<PoolCreatorWindow>("PoolCreatorWindow");
        }

        private void OnGUI()
        {
            GUILayout.Label("Pool creator");
            GUILayout.Label("Create pool in StarterPool folder");
            GUILayout.Space(10f);

            _prefabForPool = EditorGUILayout.ObjectField("Pooled object", _prefabForPool, typeof(GameObject), false);

            GUILayout.Space(5f);
            _poolName = EditorGUILayout.TextField("Pool name", _poolName);

            GUILayout.Space(5f);
            _poolPath = EditorGUILayout.TextField("Pool path", _poolPath);

            GUILayout.Space(5f);
            _pathOfPooledObject = EditorGUILayout.TextField("Path of pooled object", _pathOfPooledObject);

            GUILayout.Space(5f);
            _defaultAmount = EditorGUILayout.IntField("Default amount", _defaultAmount);

            GUILayout.Space(5f);
            _maxAmountMultiplier = EditorGUILayout.FloatField("Max amount multiplier", _maxAmountMultiplier);

            GUILayout.Space(15f);


            if (GUILayout.Button("Create pool"))
            {
                if (_poolName == "" || _pathOfPooledObject == "" || _defaultAmount <= 0 || _maxAmountMultiplier <= 0)
                {
                    ShowNotification(new GUIContent("Incorrect data in fields"));
                    return;
                }
                CreatePool();
            }

            if (GUI.changed)
            {
                if (_prefabForPool != _checkPrefab)
                {
                    _checkPrefab = _prefabForPool;
                    if (_prefabForPool)
                        SetDefault();
                }
            }

        }

        public void SetDefault()
        {
            _pathOfPooledObject = GetPath();

            _poolName = _prefabForPool.name + "Pool";
        }

        public void CreatePool()
        {
            string pathWhereCreatePool = _poolPath + "/" + _prefabForPool.name + "_PrefabPoolInfo.asset";
            PrefabPoolInfo _PrefabPoolInfo = CreateInstance<PrefabPoolInfo>();

            _PrefabPoolInfo.Initialize(_poolName, _pathOfPooledObject, _defaultAmount, _maxAmountMultiplier);

            AssetDatabase.CreateAsset(_PrefabPoolInfo, pathWhereCreatePool);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public string GetPath()
        {
            string path = AssetDatabase.GetAssetPath(_prefabForPool);

            path = path.Replace(unnededPath, "");

            path = path.Replace(".prefab", "");

            return path;
        }

    }
}
#endif