using Actors.Molds;
using Core.ObjectPool;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentMold", menuName = "Actors/Molds/EquipmentMold")]
[Serializable]
public class EquipmentMold : Mold
{
    public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

    [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

    public Vector3 PositionOffset;

    public Vector3 RotationOffset;

    public float PushForce;

    public int Damage;

    public float RigidbodyMass;
}
