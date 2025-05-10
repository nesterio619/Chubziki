#if UNITY_EDITOR
using Actors;
using Actors.Molds;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class AllVersionsCreations
{
    private static List<ChubzikAIMold> _aiTypes = new();
    private static List<ArmorMold> _chubzikArmor = new();
    private static List<ChubzikWeaponMold> _chubzikWeapons = new();

    //private static int _attackType = 0;
    private static int _aiTypesIndex = 0;
    private static int _chubzikArmorIndex = 0;
    private static int _chubzikWeaponsIndex = 0;

    private static string _attackTypePath;

    private static AIActor _chubzikAIActor;

    [ExecuteInEditMode]
    [MenuItem("Chubziki/Create all variations")]
    public static void CreateAllVariations()
    {
        string allVariationsPath = ChubzikConstructorUI.ALL_CHUBZIK_VARIATIONS_PATH;

        _aiTypesIndex = 0;
        _chubzikArmorIndex = 0;
        _chubzikWeaponsIndex = 0;

        _aiTypes.Clear();
        _chubzikArmor.Clear();
        _chubzikWeapons.Clear();

        Directory.CreateDirectory(allVariationsPath);

        foreach (var item in Resources.LoadAll<ArmorMold>(ChubzikConstructorUI.MODELS_PATH))
        {
            _chubzikArmor.Add(item);
        }

        ChangeAttackType(0);


        for (int attackTypeIndex = 0; attackTypeIndex <= (int)ChubzikConstructorUI.AttackType.Ranged; attackTypeIndex++)
        {
            ChangeAttackType(attackTypeIndex);
            for (int aiTypeIndex = 0; aiTypeIndex < _aiTypes.Count; aiTypeIndex++)
            {
                ChangeAIActorType(aiTypeIndex);
                _aiTypesIndex = aiTypeIndex;
                ChangeWeapons();


                for (int weaponIndex = 0; weaponIndex < _chubzikWeapons.Count; weaponIndex++)
                {
                    _chubzikWeaponsIndex = weaponIndex;


                    for (int chubzikArmor = 0; chubzikArmor < _chubzikArmor.Count; chubzikArmor++)
                    {
                        _chubzikArmorIndex = chubzikArmor;

                        string aiTypeName = GetName(ChubzikConstructorUI.ConstructFieldType.AI, aiTypeIndex);
                        string weaponName = GetName(ChubzikConstructorUI.ConstructFieldType.Weapon, weaponIndex);
                        string armorName = GetName(ChubzikConstructorUI.ConstructFieldType.Armor, chubzikArmor);

                        string chubzikPath = ChubzikConstructorUI.ALL_CHUBZIK_VARIATIONS_PATH + "/" + _attackTypePath + "/" + aiTypeName + "/" + weaponName + "/" + armorName;


                        if (File.Exists(chubzikPath + $"/ChubzikNumber( {aiTypeName} {weaponName} {armorName}).asset"))
                            continue;

                        ChubzikMold chubzikMold = CreateChubzikMold(_chubzikAIActor);
                        Directory.CreateDirectory(chubzikPath);
                        AssetDatabase.CreateAsset(chubzikMold, chubzikPath + $"/ChubzikNumber( {aiTypeName} {weaponName} {armorName}).asset");
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        AssetDatabase.Refresh();
    }


    private static void ChangeAttackType(int attackType)
    {
        _aiTypes.Clear();

        switch ((ChubzikConstructorUI.AttackType)attackType)
        {
            case ChubzikConstructorUI.AttackType.Melee:
                {

                    foreach (var item in Resources.LoadAll<ChubzikAIMold>(ChubzikConstructorUI.AI_MELEE_PREFAB_PATH))
                        _aiTypes.Add(item);

                    _attackTypePath = "/Melee";

                    break;
                }

            case ChubzikConstructorUI.AttackType.Ranged:
                {

                    foreach (var item in Resources.LoadAll<ChubzikAIMold>(ChubzikConstructorUI.AI_RANGED_PREFAB_PATH))
                        _aiTypes.Add(item);

                    _attackTypePath = "/Ranged";
 
                    break;
                }
        }
    }

    private static string GetName(ChubzikConstructorUI.ConstructFieldType type, int index)
    {
        switch (type)
        {
            case ChubzikConstructorUI.ConstructFieldType.AI:
                return _aiTypes[index].name;
            case ChubzikConstructorUI.ConstructFieldType.Armor:
                return _chubzikArmor[index].name;
            case ChubzikConstructorUI.ConstructFieldType.Weapon:
                return _chubzikWeapons[index].name;
        }

        return "";
    }

    private static ChubzikMold CreateChubzikMold(AIActor aIActor)
    {
        ChubzikMold constructorChubzikMold;

        if (aIActor is SniperChubzikFSM)
        {
            constructorChubzikMold = ScriptableObject.CreateInstance<ChubzikSniperMold>();
        }
        else
        {
            constructorChubzikMold = ScriptableObject.CreateInstance<ChubzikMeleeMold>();
        }

        constructorChubzikMold.ChubzikAIMold = _aiTypes[_aiTypesIndex];
        constructorChubzikMold.ArmorMold = _chubzikArmor[_chubzikArmorIndex];
        constructorChubzikMold.WeaponPrefabPool = _chubzikWeapons[_chubzikWeaponsIndex];

        if (constructorChubzikMold is ChubzikSniperMold)
        {
            (constructorChubzikMold as ChubzikSniperMold).StartAimingPercentFromMaxAimingDistance = (_chubzikWeapons[_chubzikWeaponsIndex] as ChubzikRangedWeaponMold).StartAimingPercentFromMaxAimingDistance;
            (constructorChubzikMold as ChubzikSniperMold).TooCloseistancePercent = (_chubzikWeapons[_chubzikWeaponsIndex] as ChubzikRangedWeaponMold).TooCloseistancePercent;
        }

        return constructorChubzikMold;
    }

    private static void ChangeWeapons()
    {
        _chubzikWeapons.Clear();

        if (_chubzikAIActor is SniperChubzikFSM)
        {
            foreach (var item in Resources.LoadAll<ChubzikRangedWeaponMold>(ChubzikConstructorUI.WEAPON_PATH))
            {
                _chubzikWeapons.Add(item);
            }

        }
        else
        {
            foreach (var item in Resources.LoadAll<ChubzikMeleeWeaponMold>(ChubzikConstructorUI.WEAPON_PATH))
            {
                _chubzikWeapons.Add(item);
            }
        }

    }

    private static void ChangeAIActorType(int actorType)
    {
        _chubzikAIActor = Resources.Load<AIActor>(_aiTypes[actorType].PrefabPoolInfoGetter.ObjectPath);


    }

}
#endif
