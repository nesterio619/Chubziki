using Components.ProjectileSystem;
using Core.ObjectPool;
using UnityEngine;

[CreateAssetMenu(fileName = "RangeAttackConfig ", menuName = "Weapon/RangeAttackConfig")]
public class RangedAttackConfig : ScriptableObject
{
	[Space(8)]
	public ProjectileMold projectileType;
    public PrefabPoolInfo shootParticlesPoolInfo;

    [Space(8)]

    public float FiringRadius;
    
    [Tooltip("This is a imaginary cone within which shooting is possible, created from the aiming center and this angle.")]
    public float MinimalAngleToShoot;
    [Range(0.01f,3f)] public float RotationSpeed;

    [Space(8)]
    public Vector2 MinSpread;
    public Vector2 MaxSpread;
    public AnimationCurve SpreadCurve;

    [Space(8)]

    public int projectilesPerShoot = 1;

	public bool shootLooping;

	public Vector2 totalTimeBetweenShots;
	
	[Header("Burst")]
	public bool isBurstFire;

    public int shotsPerBurst;

    public float pauseBetweenBursts;

    public PrefabPoolInfo ParticlesOnShootPool;
}
