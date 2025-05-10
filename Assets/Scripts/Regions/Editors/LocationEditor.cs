#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Regions.Editors
{
    [CustomEditor(typeof(Location), true)]
    public class LocationEditor : Editor
    {
        protected virtual string iconPath => "Assets/Textures/Icons/Editor/LocationIcon.png";
        
		protected Location _location;
        private Sector _parentSector;
        private Color _previousColor;

		// Foldout states
		private bool _showCustomSettings = true;
        private bool _showUnityEvents = false;

        private bool showLocation = false;

        
        protected virtual void OnEnable()
        {
            InitializeProperties();
            _parentSector = _location.GetComponentInParent<Sector>();
        }
        
        protected virtual void InitializeProperties()
        {
            _location = (Location)target;
		}

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _previousColor = _location.boundsColor;
            
            DrawCustomFields("actorPresets", "boundsColor", "boundsMaterial");
            DrawCustomSettings();
            DrawUnityEvents();
            
            EditorGUILayout.Space();
            
            _location.ShowLocation = EditorGUILayout.Toggle("Show Location", _location.ShowLocation);

            serializedObject.ApplyModifiedProperties();

            if (_previousColor != _location.boundsColor)
                _location.RefreshBoundsColor();
            
            if(GUI.changed)
                _location.RefreshEditorActors();
            SetCustomIcon();
        }

        private void SetLocationVisibility(bool visible)
        {
            _location.ToggleDisplayBounds(visible);
            EditorUtility.SetDirty(_location);
        }
        protected void SetCustomIcon()
        { 
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon != null) EditorGUIUtility.SetIconForObject(target, icon);
        }
        
        protected void DrawCustomFields(params string[] fields)
		{
            foreach (var field in fields)
            {
                var property = serializedObject.FindProperty(field);
                if (property == null)
                {
                    Debug.LogError($"Field with name {field} is not found. Change it's name to the appropriate class.");
                    continue;
                }
                EditorGUILayout.PropertyField(property, true);
            }
		}

		protected void DrawCustomSettings()
        {
            EditorGUILayout.Space();
            _showCustomSettings = EditorGUILayout.Foldout(_showCustomSettings, "Custom Location Settings", true);

            if (_showCustomSettings)
            {
                EditorGUILayout.Space();

                DrawAutomaticallyCalculatedBoundsButton();
            }
        }

		protected virtual void DrawAutomaticallyCalculatedBoundsButton()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Calculate Bounds from Renderers"))
            {
                Undo.RecordObject(_parentSector, "Calculate Bounds from Renderers");
                
                _location.CalculateBounds();
                
                if(!_parentSector.Locations.Contains(_location))
                    _parentSector.Locations.Add(_location);
                
                EditorSceneManager.MarkSceneDirty(_location.gameObject.scene);
                
                Debug.Log("Location bounds calculated");
            }
        }

        protected void DrawUnityEvents()
        {
            EditorGUILayout.Space();
            _showUnityEvents = EditorGUILayout.Foldout(_showUnityEvents, "Unity Events", true);

            if (_showUnityEvents)
            {
                EditorGUILayout.HelpBox("These events are triggered when the _location is entered or exited.", MessageType.Info);
            }
        }

        protected virtual void CalculateBoundsFromRenderers()
        {
            // Check if the location has children
            if (_location.transform.childCount == 0)
            {
                Debug.LogWarning("Location has no child objects to calculate bounds from.");
                return;
            }

            // Initialize a new Bounds with zero size
            Bounds combinedBounds = new Bounds(_location.transform.position, Vector3.zero);
            Renderer[] renderers = _location.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No renderers found in the location's children.");
                return;
            }

            // Iterate through each renderer to encapsulate their bounds
            foreach (Renderer renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            //UpdateBoundsTransformAndAdjustChildren(combinedBounds , BoundsTransform);

            Debug.Log("Bounds calculated, scale factor updated, and child objects adjusted.");
        }

    }
}
#endif
