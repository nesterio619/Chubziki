using AI;
using Components.Animation;
using Components.ProjectileSystem.AttackPattern;
using Core.Enums;
using Core.ObjectPool;
using Core.Utilities;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TurretEquipment : EquipmentActor
{
    [Space(10)]
    [SerializeField] private PrefabPoolInfo attackPoolPattern_PrefabPoolInfo;
    [Space(10)]
    [SerializeField] float recoilForce;
    [SerializeField] private GunRecoilTween recoilAnimation;
    [Space(10)]
    [SerializeField] private AimProvider.AimingUserData aimData;

    private RangedAttackPattern _rangeAttackPattern;

    private Action _stopAiming;
    private List<Collider> _ignoredColliders = new();

    private Rigidbody _carRigidbody;

    public void Initialize(Transform actorTransform, Rigidbody carRigidbody)
    {
        _carRigidbody = carRigidbody;

        Debug.Log(actorTransform);

        _ignoredColliders.AddRange(actorTransform.GetComponentsInChildren<Collider>());
        _ignoredColliders.Add(_equipmentCollider);


        _rangeAttackPattern = ObjectPooler.TakePooledGameObject(attackPoolPattern_PrefabPoolInfo).GetComponent<RangedAttackPattern>();
        _rangeAttackPattern.Initialize(transform, aimData.FirePoint, _ignoredColliders.ToArray(), UnityLayers.FriendlyProjectile.GetIndex());

        _rangeAttackPattern.OnShoot += recoilAnimation.PlayRecoil;
        _rangeAttackPattern.OnShoot += ApplyRecoilForce;

        ToggleLogic(true);
        ToggleRenderersEnabled(true);
    }

    public override void ToggleLogic(bool stateToSet)
    {
        if (_rangeAttackPattern == null) return;

        if (stateToSet)
            AimProvider.StartSearchAndAim(aimData, _rangeAttackPattern, RotateTurret, out _stopAiming);
        else
            _stopAiming?.Invoke();
    }

    private void RotateTurret(Quaternion targetRotation)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rangeAttackPattern.RotationSpeed);

        TryShoot(targetRotation);
    }

    private void TryShoot(Quaternion targetRotation)
    {
        if (Quaternion.Angle(targetRotation, Quaternion.LookRotation(aimData.FirePoint.forward)) < _rangeAttackPattern.MinimalAngleToShoot)
        {
            _rangeAttackPattern.SetShootLoop(true);
            _rangeAttackPattern.PerformAttack();
        }
        else
        {
            _rangeAttackPattern.SetShootLoop(false);
        }
    }

    public override void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem)
    {
        base.HandleRemovedFromSocket(socket, placeableItem);

        ToggleLogic(false);
    }

    private void ApplyRecoilForce()
    {
        var shootPoint = aimData.FirePoint;
        _carRigidbody.AddForceAtPosition(shootPoint.forward * -recoilForce, shootPoint.position, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (_rangeAttackPattern != null)
            Gizmos.DrawWireSphere(aimData.FirePoint.position, _rangeAttackPattern.FiringRadius);
    }

    private void OnDestroy()
    {
        if (_rangeAttackPattern == null) return;

        _rangeAttackPattern.OnShoot -= recoilAnimation.PlayRecoil;
        _rangeAttackPattern.OnShoot -= ApplyRecoilForce;

        _stopAiming?.Invoke();
    }
}