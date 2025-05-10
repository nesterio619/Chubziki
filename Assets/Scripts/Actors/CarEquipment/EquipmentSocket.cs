using UnityEngine;
using Hypertonic.Modules.UltimateSockets.Sockets;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;

public class EquipmentSocket : Socket
{
    [SerializeField] public EquipmentMold[] EquipmentMoldsReference;
    [SerializeField] public Vector3 defaultRotationEuler;

    public override void OnItemEnteredPlaceableZone(PlaceableItem placeableItem)
    {
        if (placeableItem.ItemTag.Contains("Rotating"))
            DefaultItemPlacementConfig.PlacedRotation = Quaternion.identity;
        else
            DefaultItemPlacementConfig.PlacedRotation = Quaternion.Euler(defaultRotationEuler);
    }
}