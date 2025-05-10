using Actors;
using Actors.Constructors;
using Actors.Molds;
using Core.InputManager;
using Core.ObjectPool;
using Core.Utilities;
using Michsky.MUIP;
using Regions;
using System.Collections.Generic;
using UI.Canvas;
using UI.Popup;
using UnityEngine;

public class ChubzikConstructorUI : PopupController
{
    private const string Scriptable_Pool_Info_Path = "ScriptableObjects/ObjectPool/UI/AISpawner_PrefabPoolInfo";

    public const string AI_MELEE_PREFAB_PATH = "ScriptableObjects/ActorMolds/Chubzik/ChubzikConstructor/AI/Melee";

    public const string AI_RANGED_PREFAB_PATH = "ScriptableObjects/ActorMolds/Chubzik/ChubzikConstructor/AI/Ranged";

    public const string MODELS_PATH = "ScriptableObjects/ActorMolds/Chubzik/ChubzikConstructor/Armor";

    public const string WEAPON_PATH = "ScriptableObjects/ActorMolds/Chubzik/ChubzikConstructor/ChubzikWeapon";

    public const string ALL_CHUBZIK_VARIATIONS_PATH = "Assets/Resources/ScriptableObjects/ActorMolds/Chubzik";

    private static bool IS_SPAWNED = false;

    private static Transform SPAWN_POINT;
    private static Location SPAWN_LOCATION;

    private List<ChubzikAIMold> _aiTypes = new();
    private List<ArmorMold> _chubzikArmor = new();
    private List<ChubzikWeaponMold> _chubzikWeapons = new();

    [SerializeField] private ConstructFields inputAttackType;

    [SerializeField] private List<ConstructFields> inputFields;

    [SerializeField] private CustomDropdown customDropdown;
    [SerializeField] private Transform chubzikOnCameraTransform;
    [SerializeField] private ButtonManager closeButton;
    [SerializeField] private ButtonManager createAIButton;
    [SerializeField] private ButtonManager createAllVariationsButton;

    private ChubzikModel _chubzikModel;
    private WeaponSettings _weaponModel;

    private ConstructFieldType _currentDropdownType;

    private AIActor _chubzikAIActor;

    private void Start()
    {
        closeButton.onClick.AddListener(ReturnToPool);
        createAIButton.onClick.AddListener(SpawnAI);

        FindAllPrefabs(MODELS_PATH, ref _chubzikArmor);

        inputAttackType.IncreaceButton.onClick.AddListener(() => ChangeAttackType(1));
        inputAttackType.DecreaceButton.onClick.AddListener(() => ChangeAttackType(-1));
        ChangeAttackType(0);

        ChangeWeaponRosterFromAI();

        for (int i = 0; i < inputFields.Count; i++)
        {
            int index = i;

            #region AddingListenersToButtons
            inputFields[i].IncreaceButton.onClick.AddListener(() => ChangeConstructField((ConstructFieldType)index, 1));
            inputFields[i].DecreaceButton.onClick.AddListener(() => ChangeConstructField((ConstructFieldType)index, -1));
            inputFields[i].FieldButton.onClick.AddListener(() => SetupDropdown((ConstructFieldType)index));

            if ((ConstructFieldType)index == ConstructFieldType.AI)
            {
                inputFields[i].IncreaceButton.onClick.AddListener(ChangeWeaponRosterFromAI);
                inputFields[i].DecreaceButton.onClick.AddListener(ChangeWeaponRosterFromAI);

            }
            #endregion

            ChangeConstructField((ConstructFieldType)index, 0);
        }

        createAllVariationsButton.onClick.AddListener(CreateAllVariations);
#if !UNITY_EDITOR
        createAllVariationsButton.gameObject.SetActive(false);
#endif

    }



    private void Update()
    {

        if (InputManager.leftMouseButton.WasPressedThisFrame() && !customDropdown.isPointerInDropdown)
            customDropdown.gameObject.SetActive(false);
    }

    private void ChangeAttackType(int changeNumber)
    {
        if (inputAttackType.CurrentIndex + changeNumber < 0 || inputAttackType.CurrentIndex + changeNumber > (int)AttackType.Ranged)
            return;


        inputAttackType.CurrentIndex += changeNumber;

        var attackType = (AttackType)inputAttackType.CurrentIndex;

        _aiTypes.Clear();

        switch (attackType)
        {
            case AttackType.Melee:
                {
                    inputAttackType.FieldButton.buttonText = "Melee";

                    foreach (var item in Resources.LoadAll<ChubzikAIMold>(AI_MELEE_PREFAB_PATH))
                        _aiTypes.Add(item);

                    break;
                }

            case AttackType.Ranged:
                {
                    inputAttackType.FieldButton.buttonText = "Ranged";

                    foreach (var item in Resources.LoadAll<ChubzikAIMold>(AI_RANGED_PREFAB_PATH))
                        _aiTypes.Add(item);

                    break;
                }
        }

        inputAttackType.FieldButton.UpdateUI();

        inputFields[(int)ConstructFieldType.AI].CurrentIndex = 0;
        inputFields[(int)ConstructFieldType.Weapon].CurrentIndex = 0;

        ChangeWeaponRosterFromAI();
        ChangeConstructField(ConstructFieldType.AI, 0);
    }

    private void ChangeWeaponRosterFromAI()
    {
        if (_chubzikAIActor != null)
        {
            _chubzikAIActor.ReturnToPool();
        }

        _chubzikAIActor = ObjectPooler.TakePooledGameObject(_aiTypes[inputFields[(int)ConstructFieldType.AI].CurrentIndex].PrefabPoolInfoGetter, null).GetComponent<AIActor>();

        ChangeWeapons();
    }

    private void ChangeWeapons()
    {
        _chubzikWeapons.Clear();

        if (_chubzikAIActor is SniperChubzikFSM)
        {
            foreach (var item in Resources.LoadAll<ChubzikRangedWeaponMold>(WEAPON_PATH))
            {
                _chubzikWeapons.Add(item);
            }

        }
        else
        {
            foreach (var item in Resources.LoadAll<ChubzikMeleeWeaponMold>(WEAPON_PATH))
            {
                _chubzikWeapons.Add(item);
            }
        }

        ChangeConstructField(ConstructFieldType.Weapon, 0);

    }

    public void CreateAllVariations()
    {
#if UNITY_EDITOR
        DialoguePopup.Create("Are you sure? This might take a while�", AllVersionsCreations.CreateAllVariations, parent: transform);
#endif
    }

    public override void ReturnToPool()
    {
        if (_chubzikModel != null)
            _chubzikModel.ReturnToPool();

        if (_weaponModel != null)
            _weaponModel.ReturnToPool();

        if (_chubzikAIActor != null)
            _chubzikAIActor.ReturnToPool();

        _chubzikModel = null;
        _weaponModel = null;
        _chubzikAIActor = null;

        base.ReturnToPool();

        IS_SPAWNED = false;

    }

    private void SpawnChubzikOnCamera()
    {

        if (_chubzikModel != null)
        {
            _chubzikModel.ReturnToPool();
            _chubzikModel = null;
        }
        if (_weaponModel != null)
        {
            _weaponModel.ReturnToPool();
            _weaponModel = null;
        }


        _chubzikModel = ObjectPooler.TakePooledGameObject(_chubzikArmor[inputFields[(int)ConstructFieldType.Armor].CurrentIndex].PrefabPoolInfoGetter, chubzikOnCameraTransform).GetComponent<ChubzikModel>();
        _chubzikModel.ModelAnimator.runtimeAnimatorController = _chubzikWeapons[inputFields[(int)ConstructFieldType.Weapon].CurrentIndex].WeaponAnimatorController;


        var weaponMold = _chubzikWeapons[inputFields[(int)ConstructFieldType.Weapon].CurrentIndex];

        if (weaponMold.PrefabPoolInfoGetter != null)
        {
            _weaponModel = ObjectPooler.TakePooledGameObject(weaponMold.PrefabPoolInfoGetter, transform).GetComponent<WeaponSettings>();


            if (_weaponModel.WhichHand == Hand.Right)
                _weaponModel.transform.SetParent(_chubzikModel.RightHand);
            else
                _weaponModel.transform.SetParent(_chubzikModel.LeftHand);

            _weaponModel.transform.localPosition = _weaponModel.PositionInHand;
            _weaponModel.transform.localRotation = Quaternion.Euler(_weaponModel.RotationInHand);

        }
    }

    public void SetupDropdown(ConstructFieldType type)
    {
        int index = (int)type;

        _currentDropdownType = type;

        customDropdown.gameObject.SetActive(true);

        customDropdown.GetComponent<RectTransform>().position = inputFields[index].FieldButton.GetComponent<RectTransform>().position;

        customDropdown.onValueChanged.RemoveAllListeners();

        while (customDropdown.items.Count != 0)
        {
            customDropdown.items.RemoveAt(0);
        }

        for (int i = 0; i < GetCount(type); i++)
        {
            customDropdown.CreateNewItem(GetName(type, i));
        }

        customDropdown.SetupDropdown();

        customDropdown.onValueChanged.AddListener(DropdownChange);
    }

    public void DropdownChange(int index)
    {
        inputFields[(int)_currentDropdownType].CurrentIndex = index;

        ChangeConstructField(_currentDropdownType, 0);

        customDropdown.gameObject.SetActive(false);

    }

    public static void CreateWithoutTransform() => Create();

    public static void Create(Transform parent = null)
    {
        if (IS_SPAWNED)
            return;

        IS_SPAWNED = true;

        if (parent == null)
            parent = CanvasManager.Instance.PopupCanvas;

        AssetUtils.TryLoadAsset(Scriptable_Pool_Info_Path, out PrefabPoolInfo popup_PrefabPoolInfo);

        var popup = ObjectPooler.TakePooledGameObject(popup_PrefabPoolInfo, parent).GetComponent<ChubzikConstructorUI>();

        var popupRect = popup.transform as RectTransform;

        if (popupRect == null)
        {
            popup.ReturnToPool();
            return;
        }
        popupRect.SetParent(parent, false);
        popupRect.anchoredPosition = Vector2.zero;

        if (popup._aiTypes.Count > 0)
        {
            popup.SpawnChubzikOnCamera();
            popup.ChangeWeaponRosterFromAI();
        }
    }

    public static void SetLocation(Location location)
    {
        SPAWN_LOCATION = location;
    }

    public static void SetSpawnPoint(Transform transform)
    {
        SPAWN_POINT = transform;
    }

    public void ChangeConstructField(ConstructFieldType type, int changeNumber)
    {
        int index = (int)type;

        if (inputFields[index].CurrentIndex + changeNumber < 0 || inputFields[index].CurrentIndex + changeNumber >= GetCount(type))
            return;

        inputFields[index].CurrentIndex += changeNumber;
        inputFields[index].FieldButton.buttonText = GetName(type, inputFields[index].CurrentIndex);
        inputFields[index].FieldButton.UpdateUI();

        SpawnChubzikOnCamera();
    }

    private int GetCount(ConstructFieldType type)
    {
        switch (type)
        {
            case ConstructFieldType.AI:
                return _aiTypes.Count;
            case ConstructFieldType.Armor:
                return _chubzikArmor.Count;
            case ConstructFieldType.Weapon:
                return _chubzikWeapons.Count;
        }

        return 0;
    }

    private string GetName(ConstructFieldType type, int index)
    {
        switch (type)
        {
            case ConstructFieldType.AI:
                return _aiTypes[index].name;
            case ConstructFieldType.Armor:
                return _chubzikArmor[index].name;
            case ConstructFieldType.Weapon:
                return _chubzikWeapons[index].name;
        }

        return "";
    }

    public void FindAllPrefabs<Type>(string path, ref List<Type> list) where Type : Object
    {
        foreach (var item in Resources.LoadAll<Type>(path))
        {
            list.Add(item);
        }
    }

    public void SpawnAI()
    {
        var chubzikMold = CreateChubzikMold(_chubzikAIActor);

        Actor chubzikActor = ActorConstructor.Instance.Load(chubzikMold, SPAWN_POINT, SPAWN_LOCATION);

        InfoPopup.Create("Chubzik has been spawned", anchor: InfoPopup.PopupAnchor.TopLeft, displayTime: 2.5f);

        chubzikActor.ToggleLogic(true);
        chubzikActor.SwitchGraphic(true);
    }

    private ChubzikMold CreateChubzikMold(AIActor aIActor)
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

        constructorChubzikMold.ChubzikAIMold = _aiTypes[inputFields[(int)ConstructFieldType.AI].CurrentIndex];
        constructorChubzikMold.ArmorMold = _chubzikArmor[inputFields[(int)ConstructFieldType.Armor].CurrentIndex];
        constructorChubzikMold.WeaponPrefabPool = _chubzikWeapons[inputFields[(int)ConstructFieldType.Weapon].CurrentIndex];

        if (constructorChubzikMold is ChubzikSniperMold)
        {
            (constructorChubzikMold as ChubzikSniperMold).StartAimingPercentFromMaxAimingDistance = (_chubzikWeapons[inputFields[(int)ConstructFieldType.Weapon].CurrentIndex] as ChubzikRangedWeaponMold).StartAimingPercentFromMaxAimingDistance;
            (constructorChubzikMold as ChubzikSniperMold).TooCloseistancePercent = (_chubzikWeapons[inputFields[(int)ConstructFieldType.Weapon].CurrentIndex] as ChubzikRangedWeaponMold).TooCloseistancePercent;
        }

        return constructorChubzikMold;
    }

    [System.Serializable]
    public class ConstructFields
    {
        public int CurrentIndex;

        public ButtonManager FieldButton;

        public ButtonManager DecreaceButton;

        public ButtonManager IncreaceButton;
    }

    public enum ConstructFieldType
    {
        AI,
        Armor,
        Weapon
    }

    public enum AttackType
    {
        Melee,
        Ranged
    }

}
