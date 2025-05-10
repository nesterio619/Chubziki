using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackConfig ", menuName = "Weapon/MeleeAttackConfig")]
public class MeleeAttackConfig : ScriptableObject
{
    public int Damage;
    public float AttackCooldown;
    public float AttackRange;
    public float DistanceToAttack;
    
    public Vector3 AttackSize = new Vector3(1, 1, 1);

}
