using UnityEngine;

public class ReadmeLods : MonoBehaviour
{
    [TextArea(3, 10)]
    public string description = "Add your description here.";
# if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position, description);
    }
    #endif
}
