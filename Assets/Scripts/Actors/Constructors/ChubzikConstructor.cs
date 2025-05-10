using Actors;
using Actors.AI.Chubziks.Base;
using Actors.Constructors;
using Actors.Molds;
using Components.ProjectileSystem.AttackPattern;
using Core.Enums;
using Core.ObjectPool;
using Core.Utilities;
using Regions;
using UnityEngine;

public class ChubzikConstructor : ObjectConstructor<ChubzikActor>
{
    private const int Location_Searching_Depth = 4;

    private static ChubzikConstructor _instance = new();

    public static ChubzikConstructor Instance
    {
        get
        {
            return _instance;
        }

        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
        }

    }

    public override ChubzikActor Load(Mold moldType, Transform transform)
    {
        var chubzikLocation = Core.Utilities.UtilitiesProvider.GetComponentInParentWithDepth<Location>(transform, Location_Searching_Depth);

        return LoadChubzikActor(moldType, transform, chubzikLocation);
    }


    public ChubzikActor LoadChubzikActor(Mold mold, Transform parentToSet, Location location)
    {
        var chubzikMold = mold as ChubzikMold;

        var pooledObject = TakeFromPool(mold, parentToSet);
        ChubzikActor chubzik = pooledObject.GetComponent<ChubzikActor>();
        chubzik.SetLocation(location);

        var attackPattern = LoadAttackPattern(chubzikMold.WeaponPrefabPool.AttackPattern, UnityLayers.EnemyProjectile.GetIndex(), null, chubzik.transform, chubzik.GetAttackPoint());
        var chubzikModel = LoadChubzikModel(chubzikMold.ArmorMold.PrefabPoolInfoGetter, chubzikMold.WeaponPrefabPool.WeaponAnimatorController, chubzik.transform);

        chubzik.LoadActor(chubzikMold, chubzikModel, attackPattern);

        return chubzik;

    }


    public static AttackPattern LoadAttackPattern(PrefabPoolInfo attackPatternPoolInfo, LayerMask layerMask, Collider[] ignoredColliders, Transform parent = null, Transform firePoint = null)
    {
        var attackPattern = ObjectPooler.TakePooledGameObject(attackPatternPoolInfo).GetComponent<AttackPattern>();

        if (firePoint == null)
        {
            attackPattern.Initialize(parent, attackPattern.transform, ignoredColliders, layerMask);

        }
        else
        {
            attackPattern.Initialize(parent, firePoint, ignoredColliders, layerMask);
        }

        return attackPattern;
    }

    public static ChubzikModel LoadChubzikModel(PrefabPoolInfo modelPoolInfo, RuntimeAnimatorController animatorController, Transform parent)
    {
        var chubzikModel = ObjectPooler.TakePooledGameObject(modelPoolInfo, parent).GetComponent<ChubzikModel>();
        chubzikModel.ModelAnimator.runtimeAnimatorController = animatorController;


        return chubzikModel;
    }

    public static WeaponSettings CreateWeaponModel(PrefabPoolInfo weaponModel, Transform leftHand, Transform rightHand)
    {
        var weaponSettings = ObjectPooler.TakePooledGameObject(weaponModel).GetComponent<WeaponSettings>();

        if (weaponSettings.WhichHand == Hand.Right)
            weaponSettings.transform.SetParent(rightHand);
        else
            weaponSettings.transform.SetParent(leftHand);


        weaponSettings.transform.localPosition = weaponSettings.PositionInHand;
        weaponSettings.transform.localRotation = Quaternion.Euler(weaponSettings.RotationInHand);

        return weaponSettings;
    }
}
