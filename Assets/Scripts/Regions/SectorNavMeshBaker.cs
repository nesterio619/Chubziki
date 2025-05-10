using Core.Enums;
using Regions;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class SectorNavMeshBaker : MonoBehaviour
{
    [SerializeField] private Sector sector;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private string regionName;

    public void Initialize(string regionName)
    {
        sector = GetComponentInParent<Sector>();
        sector.navMeshBaker = this;

        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.layerMask = ~(int)UnityLayers.Bounds;

        this.regionName = regionName;
    }
    private void OnEnable()
    {
        if(sector == null) return;

        Debug.Log("Not working bounds for navmesh");
        return;

        var bounds = sector.GetUnmodifiedBounds();
        navMeshSurface.size = bounds.size;
        navMeshSurface.center = bounds.center;
    }
    public void BakeNavMesh()
    {
        OnEnable();

        navMeshSurface.BuildNavMesh();
#if UNITY_EDITOR
        string navmeshDataFolder = "Assets/NavMeshData";
        string sceneName = SceneManager.GetActiveScene().name;

        if (!AssetDatabase.IsValidFolder($"{navmeshDataFolder}/{sceneName}"))
            AssetDatabase.CreateFolder(navmeshDataFolder, sceneName);

        if (!AssetDatabase.IsValidFolder($"{navmeshDataFolder}/{sceneName}/{regionName}"))
            AssetDatabase.CreateFolder($"{navmeshDataFolder}/{sceneName}", regionName);

        string savePath = $"{navmeshDataFolder}/{sceneName}/{regionName}/{transform.parent.name}_NavMeshData.asset";

        AssetDatabase.CreateAsset(navMeshSurface.navMeshData, savePath);
        AssetDatabase.SaveAssets();

        // update gizmos in scene
        navMeshSurface.enabled = false;
        navMeshSurface.enabled = true;
#endif
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SectorNavMeshBaker))]
public class SectorNavMeshSaverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var baker = (SectorNavMeshBaker)target;

        if(GUILayout.Button("Bake Sector NavMesh"))
            baker.BakeNavMesh();
    }
}
#endif