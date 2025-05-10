using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[Serializable]
public class SocketGenerator
{
    private const float Socket_Diameter = 0.3f;

    [SerializeField] private Transform socketTransform;
    [SerializeField] private List<EquipmentSocket> sockets = new();
    [SerializeField] private List<Renderer> renderers = new();
    [SerializeField] private EquipmentSocket socketPrefab;
    [SerializeField] private EquipmentMold[] equipmentMolds;
    [SerializeField] private Vector3 defaultRotationEuler;

    public Transform SocketTransform => socketTransform;

    public SocketGenerator(Transform parentTransform, EquipmentSocket socketPrefab, Vector3 defaultRotationEuler)
    {
        this.socketPrefab = socketPrefab;

        socketTransform = new GameObject("Sockets").transform;
        socketTransform.SetParent(parentTransform);
        socketTransform.localPosition = Vector3.zero;
        this.defaultRotationEuler = defaultRotationEuler;
    }

    private Socket GetSocket(int index)
    {
        if (index >= sockets.Count)
        {
            var obj = GameObject.Instantiate(socketPrefab, socketTransform);
            obj.name = $"Socket_{index}";
            obj.defaultRotationEuler = defaultRotationEuler;

            sockets.Add(obj);
            renderers.Add(obj.GetComponentInChildren<MeshRenderer>());

#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                EditorUtility.SetDirty(socketTransform.gameObject);
#endif
        }

        sockets[index].EquipmentMoldsReference = equipmentMolds;
        return sockets[index];
    }

    private void DestroySockets(int amount)
    {
        var socketCount = sockets.Count;
        for (int i = socketCount - 1; i >= socketCount - amount; i--)
            GameObject.DestroyImmediate(sockets[i].gameObject);
        
        sockets.RemoveRange(socketCount - amount, amount);
        renderers.RemoveRange(socketCount - amount, amount);

#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            EditorUtility.SetDirty(socketTransform.gameObject);
#endif
    }

    public void ShowSockets(bool show)
    {
        foreach (var renderer in renderers)
            renderer.enabled = show;
    }

    public void GenerateSockets(Bounds bounds, EquipmentMold[] equipmentMolds)
    {
        this.equipmentMolds = equipmentMolds;
        var socketCount = equipmentMolds.Length;

        if (socketCount < sockets.Count)
        {
#if UNITY_EDITOR
            // В редакторе используем delayCall для отложенного уничтожения
            EditorApplication.delayCall += () => DestroySockets(sockets.Count - socketCount);
#else
            // В билде выполняем уничтожение сразу
            DestroySockets(sockets.Count - socketCount);
#endif
        }

        SocketPlane socketPlane = CalculateSocketPlane(bounds);

        int maxPerRow = Mathf.FloorToInt(socketPlane.Width / Socket_Diameter);
        int rows = Mathf.CeilToInt((float)socketCount / maxPerRow);

        float verticalSpacing = socketPlane.Height / (rows + 1);
        Vector3 verticalOffset = socketPlane.Axis2 * verticalSpacing;
        Vector3 verticalRowOffset = verticalOffset * (rows - 1) / 2;

        for (int row = 0, placed = 0; row < rows && placed < socketCount; row++)
        {
            int itemsInRow = Mathf.Min(maxPerRow, socketCount - placed);
            float horizontalSpacing = socketPlane.Width / (itemsInRow + 1);
            
            Vector3 horizontalOffset = socketPlane.Axis1 * horizontalSpacing;
            Vector3 rowCenterOffset = horizontalOffset * (itemsInRow - 1) / 2;

            for (int col = 0; col < itemsInRow; col++)
            {
                var verticalPosition = bounds.center - verticalRowOffset + verticalOffset * row;
                var horizontalPosition = -rowCenterOffset + horizontalOffset * col;
                
                GetSocket(placed++).transform.localPosition = verticalPosition + horizontalPosition;
            }
        }
    }

    public void LoadEquipment()
    {
        for (int i = 0; i < sockets.Count; i++)
        {
            if (equipmentMolds[i] == null) continue;

            var equipmentObject = EquipmentConstructor.Instance.Load(equipmentMolds[i], null);
            equipmentObject.transform.position = sockets[i].transform.position;
            equipmentObject.transform.rotation = sockets[i].transform.rotation;

            var placeableItem = equipmentObject.GetComponentInChildren<PlaceableItem>();
            sockets[i].OnItemEnteredPlaceableZone(placeableItem);
            sockets[i].PlaceItem(placeableItem);
        }
    }

    private static SocketPlane CalculateSocketPlane(Bounds bounds)
    {
        Vector3 size = bounds.size;

        SocketPlane plane = new();
        Vector2 planeSize = new();

        if (size.x == 0)
        {
            plane.Axis1 = Vector3.forward;
            plane.Axis2 = Vector3.up;
            planeSize = new Vector2(size.y, size.z);
        }
        else if (size.y == 0)
        {
            plane.Axis1 = Vector3.right;
            plane.Axis2 = Vector3.forward;
            planeSize = new Vector2(size.x, size.z);
        }
        else if (size.z == 0)
        {
            plane.Axis1 = Vector3.right;
            plane.Axis2 = Vector3.up;
            planeSize = new Vector2(size.x, size.y);
        }

        plane.Width = Mathf.Max(planeSize.x, planeSize.y);
        plane.Height = Mathf.Min(planeSize.x, planeSize.y);

        return plane;
    }

    private struct SocketPlane
    {
        public Vector3 Axis1;
        public Vector3 Axis2;

        public float Width;
        public float Height;
    }
}