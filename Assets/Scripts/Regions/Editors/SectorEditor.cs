#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Regions.Editors
{
    [CustomEditor(typeof(Sector))]
    [CanEditMultipleObjects]
    public class SectorEditor : LocationEditor
    {
        protected override string iconPath => "Assets/Textures/Icons/Editor/SectorIcon.png";
        
	    private Sector _sector;
	    private Region _parentRegion;
        
        private bool showSector = false;
        private bool showLocations = false;
	    
	    private bool _showLocationsList = true;
	    
	    protected virtual void OnEnable()
	    {
		    InitializeProperties();
		    _parentRegion = _sector.GetComponentInParent<Region>();
		    Undo.undoRedoPerformed += RefreshChildrenLocations;
	    }

	    private void OnDisable()
	    {
		    Undo.undoRedoPerformed -= RefreshChildrenLocations;
	    }

	    protected override void InitializeProperties()
	    {
		    base.InitializeProperties();
		    _sector = (Sector)target;
	    }
	    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLocationsList();
            
            
            DrawCustomSettings();
            DrawUnityEvents();
            RefreshChildrenLocations();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                _location.RefreshEditorActors();
            }

            var sector = (Sector)target;
            DrawVisibilityCheckbox(sector);
            SetCustomIcon();
        }
       
        private void DrawVisibilityCheckbox(Sector sector)
        {
            sector.ShowSector = EditorGUILayout.Toggle("Show Sector", sector.ShowSector);
    
            bool allLocationsVisible = sector.Locations.All(location => location.ShowLocation);
            bool anyLocationVisible = sector.Locations.Any(location => location.ShowLocation);
            bool locationsToggleState = allLocationsVisible ? true : anyLocationVisible ? false : false;
            bool newLocationsToggleState = EditorGUILayout.Toggle("Show Locations", locationsToggleState);
    
            if (newLocationsToggleState != locationsToggleState)
            {
                foreach (var location in sector.Locations)
                {
                    location.ShowLocation = newLocationsToggleState;
                }
            }
        }
        private void DrawLocationsList()
	    {
		    EditorGUILayout.Space();
		    
		    _showLocationsList = EditorGUILayout.Foldout(_showLocationsList, "Locations List", true);
		    
		    if (!_showLocationsList) return;
		    
		    EditorGUILayout.Space();
		    
		    EditorGUILayout.Space();
		    
		    if (_sector.Locations != null)
		    {
			    for (int i = 0; i < _sector.Locations.Count; i++)
			    {
				    EditorGUILayout.BeginHorizontal();
	            
				    _sector.Locations[i].name = EditorGUILayout.TextField("Location " + i, _sector.Locations[i].name);

				    if (GUILayout.Button("Remove", GUILayout.Width(60)))
				    {
					    Undo.RecordObject(_sector.Locations[i].gameObject, "Remove Location");
					    
					    Undo.DestroyObjectImmediate(_sector.Locations[i].gameObject);
					    break;
				    }
	            
				    EditorGUILayout.EndHorizontal();
			    }
		    }
		    
		    if (GUILayout.Button("Add Location"))
		    {
			    AddNewLocation();
		    }
		    
		    EditorGUILayout.Space();
	    }
	    
	    private void AddNewLocation()
	    {
		    GameObject newLocationObject = new GameObject("New Location " + (_sector.Locations == null ? 0 :_sector.Locations.Count + 1));
		    Undo.RegisterCreatedObjectUndo(newLocationObject, "Add New Location");
		    newLocationObject.transform.SetParent(_sector.transform);
		    Location newLocation = newLocationObject.AddComponent<Location>();

		    _sector.Locations ??= new List<Location>();
		    
		    _sector.Locations.Add(newLocation);
		    
		    newLocation.CalculateBounds();
	    }

	    private void RefreshChildrenLocations()
	    {
		    if (_sector.Locations == null)
			     return;
		    
		    _sector.Locations.Clear();
		    
		    var locations = _sector.GetComponentsInChildren<Location>(false);
		    foreach (var location in locations)
		    {
			    if(!(location is Sector) && !_sector.Locations.Contains(location))
				    _sector.Locations.Add(location);
		    }
	    }

	    protected override void CalculateBoundsFromRenderers()
        {
	        Undo.RecordObject(_parentRegion, "Calculate Bounds from Renderers");
	        
            // Cast the target as a Sector
            _sector.CalculateBounds();
            
            if(!_parentRegion.Sectors.Contains(_sector))
	            _parentRegion.Sectors.Add(_sector);
            
            EditorSceneManager.MarkSceneDirty(_sector.gameObject.scene);

            Debug.Log("Sector bounds calculated, scale factor updated, and child objects adjusted.");
        }

        protected override void DrawAutomaticallyCalculatedBoundsButton()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Calculate Bounds from Locations"))
            {
                CalculateBoundsFromRenderers();
            }
        }
    }
}
#endif


