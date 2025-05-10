using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// This class server as an example of how to work with the MeshGeneration Classes in different scenarios, disable / enable the script repeatedly
/// 
/// Plese refer to the documentation and the examples below for more details
/// 
/// But in general the points to take away are:
/// 
/// 1. The meshes generated are not new instances but instead are referenced by the Generator
/// 2. To detach the mesh from the generator either use .Clone() to create a copy  or  use CombineMeshes.Combine() to merge it into a mesh refernce of your own
/// 3. The reason for all this is that Unity will semi-permanently store newly created Meshes in memory, so I want to give control to the user, instead of filling it up everytime you generate anything
/// 4. Its recommended to avoid continuously using  .Clone()  i.e. var something = new Mesh();   unless used for debugging purposes
/// 
/// </summary>
namespace SimpleMeshGenerator
{
    public class Demo : MonoBehaviour
    {
        private enum Example
        {
            SingleMesh_MemoryAdding,
            SingleMesh_MemoryAdding_Incorrect,
            SingleMesh_MemoryConstant,
            MultiMesh_MemoryAdding,
            MultiMesh_MemoryConstant,
            MultiMesh_MemoryConstant_Final,
        }

        [SerializeField] private Example _example = default;
        [SerializeField] private GameObject _dummyPrefab = default;

        private Mesh _shape1 = default;
        private Mesh _shape2 = default;
        private Mesh _shape3 = default;

        private Mesh _shapeBase = default;

        private int _counter = 0;

        private Mesh[] _shapes = new Mesh[7];
        private GameObject[] _spawns = new GameObject[7];


        // disable / enable the script repeatedly to see the decribed scenarios
        void OnEnable()
        {
            var pos = new Vector3(_counter * 3, 0, 0);
            var heightOffSet = new Vector3(0, 4.5f, 0);

            switch (_example)
            {
                case Example.SingleMesh_MemoryAdding:
                    // add NEW combined shape each time, a cylinder with 2 rings.

                    var meshFilter = Instantiate(_dummyPrefab, pos, Quaternion.identity).GetComponent<MeshFilter>();
                    meshFilter.mesh = new Mesh();
                    CombineMeshes.Combine(meshFilter.mesh, CapsuleGenerator.Generate(0.5f, 3, 6));
                    CombineMeshes.Combine(meshFilter.mesh, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, 0.8f, 0)));
                    CombineMeshes.Combine(meshFilter.mesh, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, -0.8f, 0)));

                    break;

                case Example.SingleMesh_MemoryAdding_Incorrect:

                    // add NEW combined shape each time, a cylinder with 1 ring. Because the "topRing" refers to the same internal mesh that "bottomRing" does, thus the top and bottom ring refer to the same mesh.
                    // this will cause the 2 rings to be identical

                    var meshFilter2 = Instantiate(_dummyPrefab, pos, Quaternion.identity).GetComponent<MeshFilter>();
                    meshFilter2.mesh = new Mesh();

                    var middle = CapsuleGenerator.Generate(0.5f, 3, 6);
                    var topRing = TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, 0.8f, 0));
                    var bottomRing = TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, -0.8f, 0));

                    CombineMeshes.Combine(meshFilter2.mesh, middle);
                    CombineMeshes.Combine(meshFilter2.mesh, topRing);
                    CombineMeshes.Combine(meshFilter2.mesh, bottomRing);

                    break;

                case Example.SingleMesh_MemoryConstant:

                    // create the same shape as before, but re-use a local variable, this results in NO Extra mesh allocations in Memory eventhough we "re-create" the mesh everytime

                    var meshFilter3 = Instantiate(_dummyPrefab, pos, Quaternion.identity).GetComponent<MeshFilter>();

                    if (_shape1 == null) _shape1 = new Mesh();
                    else _shape1.Clear();

                    CombineMeshes.Combine(_shape1, CapsuleGenerator.Generate(0.5f, 3, 6));
                    CombineMeshes.Combine(_shape1, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, 0.8f, 0)));
                    CombineMeshes.Combine(_shape1, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, -0.8f, 0)));

                    meshFilter3.mesh = _shape1;

                    break;

                case Example.MultiMesh_MemoryAdding:

                    // create the same shape as before, similar to "SingleMesh_MemoryAdding_Incorrect", but this time we .Clone() the elements.
                    // this will result in a visually correct shape, as this time the "top" and "bottom" are their own "New Mesh()" instead of linked

                    //however everytime "meshFilter4" is filled with meshes, we will ad 4 meshes to memory, " meshFilter4.mesh = new Mesh()"  and then 3 more times by cloning each section
                    //.Clone() in this example gives the correct visual result but not an optimal memory use, lets go to our final example 

                    var meshFilter4 = Instantiate(_dummyPrefab, pos, Quaternion.identity).GetComponent<MeshFilter>();

                    meshFilter4.mesh = new Mesh();

                    var middle4 = CapsuleGenerator.Generate(0.5f, 3, 6).Clone();
                    var topRing4 = TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, 0.8f, 0)).Clone();
                    var bottomRing4 = TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, -0.8f, 0)).Clone();

                    CombineMeshes.Combine(meshFilter4.mesh, middle4);
                    CombineMeshes.Combine(meshFilter4.mesh, topRing4);
                    CombineMeshes.Combine(meshFilter4.mesh, bottomRing4);


                    //this second object reuses the mesh, thus avoiding extra Meshes being added to Memory
                    var meshFilter5 = Instantiate(_dummyPrefab, pos + heightOffSet, Quaternion.identity).GetComponent<MeshFilter>();
                    meshFilter5.mesh = meshFilter4.mesh;

                    break;

                case Example.MultiMesh_MemoryConstant:

                    // create the same shape as before, but this time we .Clone() a core element as an example of storing either a simple or complex combined mesh you would want to re-use
                    // the 2 object have differing meshes, both stored / reused locally causing no extra Memory uses as can be seen in the profiler  

                    if (_shapeBase == null)
                        _shapeBase = CapsuleGenerator.Generate(0.5f, 3, 24).Clone();
       
                    if (_shape2 == null)
                    {
                        _shape2 = new Mesh();
                        CombineMeshes.Combine(_shape2, _shapeBase);
                        CombineMeshes.Combine(_shape2, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, 0.8f, 0)));
                        CombineMeshes.Combine(_shape2, TorusGenerator.Generate(1f, 0.3f).AddPositionOffset(new Vector3(0, -0.8f, 0)));
                    }

                    var meshFilter6 = Instantiate(_dummyPrefab, pos, Quaternion.identity).GetComponent<MeshFilter>();
                    meshFilter6.mesh = _shape2;

    
                    if (_shape3 == null)
                    {
                        _shape3 = new Mesh();
                        CombineMeshes.Combine(_shape3, _shapeBase);
                        CombineMeshes.Combine(_shape3, RectangleGenerator.GenerateHollow(new Vector3(2f, 0.4f, 2f), 0.2f).AddPositionOffset(new Vector3(0, 0.8f, 0)));
                        CombineMeshes.Combine(_shape3, RectangleGenerator.GenerateHollow(new Vector3(2f, 0.4f, 2f), 0.2f).AddPositionOffset(new Vector3(0, -0.8f, 0)));
                    }

                    var meshFilter7 = Instantiate(_dummyPrefab, pos + heightOffSet, Quaternion.identity).GetComponent<MeshFilter>();
                    meshFilter7.mesh = _shape3;

                    break;

                case Example.MultiMesh_MemoryConstant_Final:
                    // example of multiple generated creatures being handled by reusing their Mesh reference, after creating the "max" amount, you will see no further meshes being allocated to memory in the profiler

                    var headRadius = 0.5f;

                    // base Head shape to re-use
                    if (_shapeBase == null)
                    {
                        _shapeBase = SphereGenerator.Generate(headRadius, 36).Clone(); // first / only time initialising the "_shapeBase", lets use .Clone() to disconnect the reference from the Generator
                        CombineMeshes.Combine(_shapeBase, SphereGenerator.Generate(0.2f, 18).AddPositionOffset(new Vector3(0.3f, 0.2f, -0.2f)));
                        CombineMeshes.Combine(_shapeBase, SphereGenerator.Generate(0.2f, 18).AddPositionOffset(new Vector3(-0.3f, 0.2f, -0.2f)));
                    }

                    // pretend we are creating creatures throughout the experience, setup the mesh references when Null 
                    var index = _counter % _shapes.Length;

                    if (_spawns[index] != null)
                        Destroy(_spawns[index]);

                    if (_shapes[index] == null)
                        _shapes[index] = new Mesh();

                    _spawns[index] = Instantiate(_dummyPrefab, new Vector3(index * 3, 0, 0), Quaternion.identity);
                    var filter = _spawns[index].GetComponent<MeshFilter>();
                    _shapes[index].Clear();


                    // add the head
                    CombineMeshes.Combine(_shapes[index], _shapeBase);

                    // randomised character hair
                    var hairLength = Random.Range(0.75f, 1.25f);

                    var hair = CapsuleGenerator.Generate(0.05f, hairLength).SetOrigin(new Vector3(0, -hairLength * 0.5f, 0));
                    MeshManipulation.Rotate(ref hair, Vector3.zero, new Vector3(0, 0, -45));
                    CombineMeshes.Combine(_shapes[index], hair);

                    bool evenNumber = _counter % 2 == 0;
                    if (evenNumber)
                    {
                        MeshManipulation.Rotate(ref hair, Vector3.zero, new Vector3(0, 0, 45));
                        CombineMeshes.Combine(_shapes[index], hair);
                    }

                    MeshManipulation.Rotate(ref hair, Vector3.zero, new Vector3(0, 0, evenNumber ? 45 : 90));
                    CombineMeshes.Combine(_shapes[index], hair);


                    filter.mesh = _shapes[index];

                    break;
            }


            _counter++;
        }
    }
}
