using UnityEngine;
using SimpleMeshGenerator;

public class DoubleHelix : MonoBehaviour
{
    void OnEnable()
    {
		gameObject.GetComponent<MeshFilter>().mesh = DoubleHelixGenerator.Generate(10, 1000, 4, 1f).Clone();
    }
}
