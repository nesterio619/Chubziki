using Actors.AI;
using Actors.AutoRepairShop;
using Actors.Molds;
using Components;
using Components.Mechanism;
using Regions;
using UnityEngine;

namespace Actors.Constructors
{
    public class ActorConstructor : ObjectConstructor<Actor>
    {
        private static ActorConstructor _instance = new();
        private static Location _currentLocation;

        public static ActorConstructor Instance
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

        public Actor Load(Mold actorMold, Transform parentToSet, Location location)
        {
            _currentLocation = location;

            var actor = Load(actorMold, parentToSet);

            _currentLocation = null;

            return actor;
        }

        public override Actor Load(Mold actorMold, Transform parentToSet)
        {
            Actor pooledObject = null;

            switch (actorMold)
            {
                case ScriptableRigidbodyMold rigidbodyMold:
                    pooledObject = LoadRigidbody(rigidbodyMold, parentToSet);
                    break;
                case ScriptableTrapMold trapMold:
                    pooledObject = LoadTrap(trapMold, parentToSet);
                    break;
                case ChubzikMold chubzikMold:
                    pooledObject = LoadChubzik(chubzikMold, parentToSet);
                    break;
                case TrainingDummyMold DummyMold:
					pooledObject = LoadDummy(DummyMold, parentToSet);
					break;
				case ScriptableWeaponMold weaponMold:
                    pooledObject = LoadWeapon(weaponMold, parentToSet);
                    break;
                case TowerMold turretMold:
                    pooledObject = LoadTower(turretMold, parentToSet);
                    break;
                case BridgeMold bridgeMold:
                    pooledObject = LoadBridge(bridgeMold, parentToSet);
                    break;
                case PressureButtonMold buttonMold:
                    pooledObject = LoadButton(buttonMold, parentToSet);
                    break;
                case CarMold carMold:
                    pooledObject = LoadCar(carMold, parentToSet);
                    break;
                case AutoRepairShopMold atoRepairShopMold:
                    pooledObject = LoadAutoRepairShop(atoRepairShopMold, parentToSet);
                    break;
                case RBLibraBridgeMold atoRepairShopMold:
                    pooledObject = LoadLibraBridge(atoRepairShopMold, parentToSet);
                    break;
                default:
                    Debug.LogError($"Unknown mold type: {actorMold.GetType().Name}");
                    break;
            }
            
            if (pooledObject != null)
            {
                pooledObject.SwitchGraphic(false); 
                pooledObject.ToggleLogic(false);  
            }
            return pooledObject;
        }

        public Actor LoadLibraBridge(RBLibraBridgeMold atoLibraBridgeMold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(atoLibraBridgeMold, parentToSet);
            var artor = pooledObject.GetComponent<Actor>();

            artor.LoadActor(atoLibraBridgeMold);

            return artor;
        }

        public Actor LoadButton(PressureButtonMold buttonMold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(buttonMold, parentToSet);
            var button = pooledObject.GetComponent<Actor>();

            button.LoadActor(buttonMold);

            return button;
        }
        
        private Actor LoadRigidbody(ScriptableRigidbodyMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            if (pooledObject.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.mass = mold.Weight;
                rigidbody.GetComponent<RigidbodyActor>().ToggleKinematic(mold.KinematicUntilFirstTouch);
            }

            var actor = pooledObject.GetComponent<Actor>();
            actor.LoadActor(mold);
            
            return actor;
        }

        private Actor LoadTrap(ScriptableTrapMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);

            var actor = pooledObject.GetComponent<Actor>();
            actor.LoadActor(mold);
            
            return actor;
        }

        private Actor LoadChubzik(ChubzikMold mold, Transform parentToSet)
        {
            return ChubzikConstructor.Instance.LoadChubzikActor(mold, parentToSet, _currentLocation);
        }

		private Actor LoadDummy(TrainingDummyMold mold, Transform parentToSet)
		{
			var pooledObject = TakeFromPool(mold, parentToSet);
            Actor dummy = pooledObject.GetComponent<Actor>();
            dummy.LoadActor(mold);

			return dummy;
		}

		private Actor LoadWeapon(ScriptableWeaponMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            
            var projectileLauncher = pooledObject.GetComponent<ProjectileLauncher>();
            
            projectileLauncher.LoadActor(mold);
            
            return projectileLauncher;
        }

        private Actor LoadTower(TowerMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            TowerActor projectileLauncher = pooledObject.GetComponent<TowerActor>();
            projectileLauncher.LoadActor(mold);

            return projectileLauncher;
        }

        private Actor LoadBridge(BridgeMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            var bridge = pooledObject.GetComponent<BridgeActor>();

            bridge.LoadActor(mold);

            return bridge;
        }

        private Actor LoadCar(Mold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            CarActor carActor = pooledObject.GetComponent<CarActor>();

            carActor.LoadActor(mold,parentToSet);

            return carActor;
        }

        private Actor LoadAutoRepairShop(Mold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold, parentToSet);
            var autoRepairShopMold = pooledObject.GetComponent<AutoRepairShopActor>();

            autoRepairShopMold.LoadActor(mold);

            return autoRepairShopMold;
        }
    }
}
