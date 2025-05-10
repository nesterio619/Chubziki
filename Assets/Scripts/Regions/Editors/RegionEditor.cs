using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Regions.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Region))]
    [CanEditMultipleObjects]
    public class RegionEditor : LocationEditor
    {
        protected override string iconPath => "Assets/Textures/Icons/Editor/RegionIcon.png";
        
        private bool _showUtilitiesOptions = true;
        private bool _showSectorsList = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            Undo.undoRedoPerformed += RefreshChildrenSectors;
        }
        
        private void OnDisable() => Undo.undoRedoPerformed -= RefreshChildrenSectors;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSectorsList();
            DrawCustomSettings();
            DrawUnityEvents();
            RefreshChildrenSectors();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                _location.RefreshEditorActors();
            }

            Region region = (Region)target;
            
            DrawVisibilityCheckbox(region);
            DrawOverlapCheckButton(region);
            
            SetCustomIcon();
        }
        
        
        private void DrawSectorsList()
        {
            Region region = (Region)target;
            
            _showSectorsList = EditorGUILayout.Foldout(_showSectorsList, "Sectors List", true);
            
            if (!_showSectorsList) return;
            
            EditorGUILayout.Space();

            if (region.Sectors != null)
            {
                for (int i = 0; i < region.Sectors.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    region.Sectors[i].name = EditorGUILayout.TextField("Sector " + i, region.Sectors[i].name);

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.DestroyObjectImmediate(region.Sectors[i].gameObject);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("Add Sector"))
            {
                AddNewSector();
            }
            
            EditorGUILayout.Space();
        }

        private void AddNewSector()
        {
            Region region = (Region)target;

            GameObject newSector = new GameObject("New Sector " + (region.Sectors == null ? 0 : region.Sectors.Count + 1));
            Undo.RegisterCreatedObjectUndo(newSector, "Create new sector");
            newSector.transform.SetParent(region.transform);
            Sector sector = newSector.AddComponent<Sector>();
            
            region.Sectors ??= new List<Sector>();
            
            region.Sectors.Add(sector);

            sector.CalculateBounds();

            AddSectorNavMesh(sector.transform);
        }

        private void AddSectorNavMesh(Transform parent)
        {
            Region region = (Region)target;

            var navMeshGameObject = new GameObject("NavMeshSurface");
            navMeshGameObject.transform.SetParent(parent.transform);
            navMeshGameObject.transform.localPosition = Vector3.zero;

            var navmeshBaker = navMeshGameObject.AddComponent<SectorNavMeshBaker>();
            navMeshGameObject.AddComponent<NavMeshSurface>();
            navmeshBaker.Initialize(region.name);
        }

        private void RefreshChildrenSectors()
        {
            Region region = (Region)target;
            
            if (region.Sectors == null)
                return;
            
            region.Sectors.Clear();
            
            var sectors = region.GetComponentsInChildren<Sector>(false);
            foreach (var sector in sectors)
            {
                if(!(sector is Region) && !region.Sectors.Contains(sector))
                    region.Sectors.Add(sector);
            }
        }

        private void DrawVisibilityCheckbox(Region region)
        {
            EditorGUILayout.Space();
            region.ShowRegion = EditorGUILayout.Toggle("Show Region", region.ShowRegion);
    
            bool allSectorsVisible = region.Sectors.All(sector => sector.ShowSector);
            bool anySectorVisible = region.Sectors.Any(sector => sector.ShowSector);
            bool sectorsToggleState = allSectorsVisible ? true : anySectorVisible ? false : false;
            bool newSectorsToggleState = EditorGUILayout.Toggle("Show Sectors", sectorsToggleState);
    
            if (newSectorsToggleState != sectorsToggleState)
            {
                foreach (var sector in region.Sectors)
                {
                    sector.ShowSector = newSectorsToggleState;
                }
            }
    
            bool allLocationsVisible = region.Sectors.SelectMany(sector => sector.Locations).All(location => location.ShowLocation);
            bool anyLocationVisible = region.Sectors.SelectMany(sector => sector.Locations).Any(location => location.ShowLocation);
            bool locationsToggleState = allLocationsVisible ? true : anyLocationVisible ? false : false;
            bool newLocationsToggleState = EditorGUILayout.Toggle("Show Locations", locationsToggleState);
    
            if (newLocationsToggleState != locationsToggleState)
            {
                foreach (var sector in region.Sectors)
                {
                    foreach (var location in sector.Locations)
                    {
                        location.ShowLocation = newLocationsToggleState;
                    }
                }
            }
        }

        private void DrawOverlapCheckButton(Region region)
        {
            EditorGUILayout.Space();

            _showUtilitiesOptions = EditorGUILayout.Foldout(_showUtilitiesOptions, "Utilities", true);

            if (_showUtilitiesOptions)
            {
                if (GUILayout.Button("Check for Overlapping Sectors"))
                {
                    CheckForOverlappingSectors(region);
                }

                if (GUILayout.Button("Check for Overlapping Locations"))
                {
                    CheckForOverlappingLocations(region);
                }
            }
        }

        private void CheckForOverlappingSectors(Region region)
        {
            bool overlapDetected = false;

            for (int i = 0; i < region.Sectors.Count; i++)
            {
                for (int j = i + 1; j < region.Sectors.Count; j++)
                {
                    region.Sectors[i].CalculateBounds();
                    region.Sectors[j].CalculateBounds();
                    if (region.Sectors[i].Bounds.Intersects(region.Sectors[j].Bounds))
                    {
                        Debug.LogWarning($"Sectors {region.Sectors[i].name} and {region.Sectors[j].name} are overlapping.");
                        overlapDetected = true;
                    }
                }
            }

            if (!overlapDetected)
            {
                Debug.Log("No overlapping sectors found.");
            }
        }

        private void CheckForOverlappingLocations(Region region)
        {
            bool overlapDetected = false;

            List<Location> allLocations = new List<Location>();
            foreach (var sector in region.Sectors)
            {
                allLocations.AddRange(sector.Locations);
            }

            for (int i = 0; i < allLocations.Count; i++)
            {
                for (int j = i + 1; j < allLocations.Count; j++)
                {
                    allLocations[i].CalculateBounds();
                    allLocations[j].CalculateBounds();
                    if (allLocations[i].Bounds.Intersects(allLocations[j].Bounds))
                    {
                        Debug.LogWarning($"Locations {allLocations[i].name} and {allLocations[j].name} are overlapping.");
                        overlapDetected = true;
                    }
                }
            }

            if (!overlapDetected)
            {
                Debug.Log("No overlapping locations found.");
            }
        }

        protected override void CalculateBoundsFromRenderers()
        {
            // Cast the target as a Region
            Region region = (Region)target;

            // Initialize a new Bounds with zero size
            Bounds combinedBounds = new Bounds(region.transform.position, Vector3.zero);
            
            if (region.Sectors.Count == 0)
            {
                Debug.LogWarning("No Location components found in the Sector.");
                return;
            }

            region.CalculateBounds();
            

            EditorSceneManager.MarkSceneDirty(region.gameObject.scene);

            
            Debug.Log("All locations has been calculated.");
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
#endif
}