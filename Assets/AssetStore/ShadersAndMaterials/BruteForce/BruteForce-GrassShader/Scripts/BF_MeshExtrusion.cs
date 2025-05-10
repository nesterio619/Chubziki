  
using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class BF_MeshExtrusion : MonoBehaviour {

    [SerializeField] private MeshFilter meshFilter;
    
    public Mesh originalMesh;

    public float offsetValue = 1f;
    public Vector3 offsetVector = Vector3.zero;
    private float offsetValueMem = 1f;
    private Vector3 offsetVectorMem = Vector3.zero;
    public int numberOfStacks = 1;
    private int numberOfStacksMem = 1;
    private int[] oldTri;
    private Vector3[] oldVert;
    private Vector3[] oldNorm;
    private Vector2[] oldUV;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertexs = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> cols = new List<Color>();
    
    void Awake()
    {
        CheckValues();
        BuildGeometry();
    }

    private void OnEnable()
    {
        CheckValues();
    }

    private void Update()
    {
        if(offsetValueMem != offsetValue || numberOfStacks != numberOfStacksMem || offsetVectorMem != offsetVector)
        {
            ClearGeometry();
            BuildGeometry();
            offsetValueMem = offsetValue;
            offsetVectorMem = offsetVector;
            numberOfStacksMem = numberOfStacks;
        }
    }

    private void CheckValues()
    {
        offsetValueMem = offsetValue;
        offsetVectorMem = offsetVector;
        numberOfStacksMem = numberOfStacks;
        oldTri = originalMesh.triangles;
        oldVert = originalMesh.vertices;
        oldNorm = originalMesh.normals;
        oldUV = originalMesh.uv;
    }

    private void ClearGeometry()
    {
        triangles.Clear();
        triangles.TrimExcess();
        vertexs.Clear();
        vertexs.TrimExcess();
        uvs.Clear();
        uvs.TrimExcess();
        cols.Clear();
        cols.TrimExcess();
    }

    private void BuildGeometry()
    {
        if (meshFilter == null) return;
            
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int faces = Mathf.Min(numberOfStacks, 100);
        int subMeshCount = originalMesh.subMeshCount;
        mesh.subMeshCount = subMeshCount;

        List<List<int>> subMeshTriangles = new List<List<int>>(subMeshCount);
        for (int s = 0; s < subMeshCount; s++)
            subMeshTriangles.Add(new List<int>());

        for (int i = 0; i < faces; i++)
        {
            int triangleOffset = i * oldVert.Length;
            int indexNewV = 0;
        
            foreach (Vector3 v in oldVert)
            {
                vertexs.Add(v + (oldNorm[indexNewV]) * offsetValue * 0.01f * i + (offsetVectorMem * 0.01f * i));
                uvs.Add(oldUV[indexNewV]);
                cols.Add(new Color(1 * ((float)i / (faces - 1)), 1 * ((float)i / (faces - 1)), 1 * ((float)i / (faces - 1))));
                indexNewV++;
            }

            for (int s = 0; s < subMeshCount; s++)
            {
                int[] subTriangles = originalMesh.GetTriangles(s);
                foreach (int t in subTriangles)
                    subMeshTriangles[s].Add(t + triangleOffset);
            }
        }

        mesh.vertices = vertexs.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = cols.ToArray();
    
        for (int s = 0; s < subMeshCount; s++)
            mesh.SetTriangles(subMeshTriangles[s], s);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

}