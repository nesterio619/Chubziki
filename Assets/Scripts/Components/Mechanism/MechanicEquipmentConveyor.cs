using Core;
using Core.SaveSystem;
using DG.Tweening;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using UI;
using UnityEngine;

public class MechanicEquipmentConveyor : MonoBehaviour
{
    [SerializeField] private ObjectGrabber objectGrabber;
    [Space(7)]
    [SerializeField] private GameObject barrierSpawner;
    [SerializeField] private GameObject sawSpawner;
    [SerializeField] private GameObject turretSpawner;

    private CarVisuals _playerCarVisuals;

    private void Start()
    {
        _playerCarVisuals = Player.Instance.PlayerCarGameObject.CarVisuals;

        if (!SaveManager.Progress.Unlocks.IsEquipmentUnlocked(UnlockableEquipment.RammingBar))
            barrierSpawner.SetActive(false);

        if (!SaveManager.Progress.Unlocks.IsEquipmentUnlocked(UnlockableEquipment.SawBlade))
            sawSpawner.SetActive(false);

        if (!SaveManager.Progress.Unlocks.IsEquipmentUnlocked(UnlockableEquipment.Turret))
            turretSpawner.SetActive(false);
    }

    private void OnEnable() => objectGrabber.OnRelease += ReturnEquipment;
    private void OnDisable() => objectGrabber.OnRelease -= ReturnEquipment;

    private void ReturnEquipment(Transform transform)
    {
        var item = transform.GetComponentInChildren<PlaceableItem>();
        if (item != null && !item.CanBePlaced)
        {
            item.SetPreventPlacement(true);
            transform.DOMove(this.transform.position, 0.3f)
                .onComplete = () => Destroy(transform.gameObject);
        }
            
    }

    public void ShowCarSockets(bool show)
    {
        _playerCarVisuals.ShowSockets(show);

        GamepadCursor.DisplayCursor(show);
        Player.Instance.PlayerCarGameObject.MovementDirectionLimiter.SetMovementRestrictions(restrictLeft: show, restrictRight: show);
    }

    public void UnlockAllEquipment()
    {
        barrierSpawner.SetActive(true);
        sawSpawner.SetActive(true);
        turretSpawner.SetActive(true);

        SaveManager.Progress.Unlocks.UnlockEquipment(UnlockableEquipment.Turret.ToString());
        SaveManager.Progress.Unlocks.UnlockEquipment(UnlockableEquipment.RammingBar.ToString());
        SaveManager.Progress.Unlocks.UnlockEquipment(UnlockableEquipment.SawBlade.ToString());
    }
}