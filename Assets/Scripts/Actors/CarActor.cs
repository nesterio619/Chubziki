using System;
using Actors.Molds;
using Components.Car;
using Core.Interfaces;
using Core.SaveSystem;
using PassiveEffects;
using Regions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Upgrades;


namespace Actors
{
    public class CarActor : Actor, IDamageable, IMoving
    {
        [field: SerializeField] internal CarDriving CarDriving { get; private set; }
        [FormerlySerializedAs("upgradesContainer")]
        [Space(10)]
        [field: SerializeField] private UpgradesList _upgradesList;

        private UpgradeLevelContainer _upgradeLevelContainer;

        [field: SerializeField] public CarMold CarMold { get; private set; }

        [Space(10)]
        [field: SerializeField] private protected Rigidbody carControllerRigidBody;

        [field: SerializeField] public CarEquipmentManager EquipmentManager { get; protected set; }

        [FormerlySerializedAs("carUpgradesManager")] [field: SerializeField] protected CarInfo carInfo;
        
        public IHealthEffect PassiveHealingModule { get; private set; }
        
        public event Action OnNoHealthLeft = delegate { };

        public Transform ActorPresetTransform { get; protected set; }

        public int MaxHealth => carInfo.MaxHealth;
        public int CurrentHealth => carInfo.CurrentHealth;

        private bool _isVisible = false;
        public bool IsVisible { get => _isVisible; set { _isVisible = value; } }
        public bool IsOutOfSector { get => !_actorSector.IsInsideBounds(transform.position); }
        public bool IsSectorLoaded { get => _actorSector.IsLoaded; }

        private Sector _actorSector;
        private Transform _locationParent;
        public CarVisuals CarVisuals { get; protected set; }

        [SerializeField]
        private Collider carCollider;

        public void LoadActor(Mold actorMold, Transform parentTransform)
        {
            LoadActor(actorMold);

            Initialize(actorMold as CarMold);

            _locationParent = transform.parent;

            ActorPresetTransform = parentTransform;
        }

        public void InitializeQuestCar()
        {
            if(CarMold == null)
            {
                Debug.Log("No CarMold");
                return;
            }

            Initialize(CarMold);
        }

        private protected virtual void Initialize(CarMold carMold)
        {
            //=-- UPGRADES --=//
            _upgradesList = UpgradeConfigs.LoadCarUpgrades(UpgradeConfigs.PLAYER_CAR_UPGRADES_CONFIG_NAME);
            _upgradeLevelContainer = GetUpgradeLevelContainer(carMold);
            _upgradesList.SetUpgradesFromUpgradesContatiner(_upgradeLevelContainer);

            //=-- CAR INFO --=//
            carInfo = new CarInfo(_upgradesList, CarDriving, this);
            carInfo.OnNoHealthLeft += ()=>OnNoHealthLeft.Invoke();
            
            //=-- CAR DRIVER --=//
            carControllerRigidBody.centerOfMass = Vector3.zero;
            CarDriving.Initialize(carControllerRigidBody, _upgradesList, this);
            carInfo.OnChangeHealth += changeAmount => CarDriving.ChangeStateDamageCar(CurrentHealth <= MaxHealth / 4f); // TEMPORARY CODE

            //=-- EQUIPMENT --=//
            EquipmentManager.Initialize(CarDriving, transform, carControllerRigidBody, carMold);
            
            //=-- HEALING --=//
            PassiveHealingModule = new SimpleTimerHealing(carInfo);
            PassiveHealingModule.ToggleActive(true);
            carInfo.OnChangeHealth += ((SimpleTimerHealing)PassiveHealingModule).TryResetTimer;

            //=-- CAR MODEL --=//
            CarVisuals = GetComponentInChildren<CarVisuals>();
            if (CarVisuals!=null) CarVisuals.LoadAllEquipment();

            //=-- AIR CONTROL --=//
            CarDriving.InitializeAirControl(carControllerRigidBody, CarVisuals.CarData.BodyData.TotalWidth);

            Physics.SyncTransforms();
        }

        public void UpdateUpgrades()
        {
            _upgradesList.SetUpgradesFromUpgradesContatiner(_upgradeLevelContainer);

            carInfo.UpdateCarUpgrades();
            CarDriving.UpdateUpgrades(_upgradesList);
        }

        public void ChangeHealthBy(int changeAmount)
        {
            carInfo.ChangeHealthBy(changeAmount);
            
            OnHealthChanged(changeAmount);
        }
        protected virtual void OnHealthChanged(int changeAmount) {}

        public void ResetHealth() => carInfo.ResetHealth();

        private Vector3 _autoMass = new Vector3(-0.1f, 0.74f, 0.16f);

        public void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode)
        {
            carControllerRigidBody.centerOfMass = _autoMass;

            carControllerRigidBody.AddForce(pushForce, forceMode);

            Core.Utilities.UtilitiesProvider.WaitAndRun(() => carControllerRigidBody.centerOfMass = Vector3.zero, true, 0.1f);

        }

        public Rigidbody GetRigidbody()
        {
            return carControllerRigidBody;
        }

        public Vector3 GetVelocity() => carControllerRigidBody.velocity;

        public void SaveCurrentUpgrades()
        {
            var progress = SaveManager.Progress;
            progress.UpgradeData.UpgradeLevels.Copy(_upgradeLevelContainer);
            SaveManager.SaveProgress();
        }

        public UpgradesList GetCarUpgrades()
        {
            return _upgradesList;
        }

        public UpgradeLevelContainer GetLevelUpgrades()
        {
            return _upgradeLevelContainer;
        }

        protected virtual UpgradeLevelContainer GetUpgradeLevelContainer(CarMold carMold)
        {
            return UpgradeConfigs.LoadUpgrades(carMold);
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();
            
            PassiveHealingModule.ToggleActive(false);

            CarDriving.Dispose();
            CarDriving.UpdateUpgrades(_upgradesList);

            EquipmentManager.Dispose();

            VisibleActorsManager.RemoveActingObject(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(carControllerRigidBody.centerOfMass, 0.1f);
        }

        #if UNITY_EDITOR
        private void OnEnable()
        {
            if(CarMold.RevertOnEditorEnter)
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private void OnDisable()
        {
            if (CarMold.RevertOnEditorEnter)
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                CarMold.ResetEquipment();
        }
        #endif

        #region IMovingVariablesImplementation
        public Actor GetActor()
        {
            return this;
        }

        public Bounds GetBounds()
        {
            return carCollider.bounds;
        }

        public Transform GetLocationParent()
        {
            return _locationParent;
        }

        public Transform GetSelfTransform()
        {
            return transform;
        }

        public void UnloadIfOutOfBounds() => ReturnToPool();
        public void SetSector(Sector sector) => _actorSector = sector;

        #endregion


    }
}

