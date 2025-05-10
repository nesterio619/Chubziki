using System.Collections.Generic;
using UnityEngine;

namespace RSM
{
    public class StateMachineManager : RSM.MonoSingleton<StateMachineManager>
    {
        private Dictionary<int, StateMachine> _stateMachines;
        private Dictionary<int, StateMachine> _fixedStateMachines;

        private List<StateMachine> _updateRemove = new List<StateMachine>();
        private List<StateMachine> _updateAdd = new List<StateMachine>();

        private List<StateMachine> _fixedRemove = new List<StateMachine>();
        private List<StateMachine> _fixedAdd = new List<StateMachine>();

        protected override void Initialize()
        {
            base.Initialize();
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            _stateMachines = new Dictionary<int, StateMachine>();
            _fixedStateMachines = new Dictionary<int, StateMachine>();
            _updateAdd = new List<StateMachine>();
            _updateRemove = new List<StateMachine>();
            _fixedAdd = new List<StateMachine>();
            _fixedRemove = new List<StateMachine>();
        }

        public static void Add(StateMachine stateMachine, UpdateType? updateType = null)
        {
            updateType ??= stateMachine.GetUpdateType();
            List<StateMachine> targetList = updateType switch
            {
                UpdateType.FixedUpdate => Instance._fixedAdd,
                _ => Instance._updateAdd
            };
            targetList.Add(stateMachine);
        }

        public static void Remove(StateMachine stateMachine, UpdateType? updateType = null)
        {
            updateType ??= stateMachine.GetUpdateType();
            List<StateMachine> targetList = updateType switch
            {
                UpdateType.FixedUpdate => Instance._fixedRemove,
                _ => Instance._updateRemove
            };
            targetList.Add(stateMachine);
        }

        private void ProcessUpdateAdd()
        {
            foreach (StateMachine stateMachine in _updateAdd)
            {
                AddLocal(stateMachine);
            }

            _updateAdd.Clear();
        }

        private void ProcessUpdateRemove()
        {
            foreach (StateMachine stateMachine in _updateRemove)
            {
                RemoveLocal(stateMachine);
            }

            _updateRemove.Clear();
        }

        private void ProcessFixedAdd()
        {
            foreach (StateMachine stateMachine in _fixedAdd)
            {
                AddLocal(stateMachine);
            }

            _fixedAdd.Clear();
        }

        private void ProcessFixedRemove()
        {
            foreach (StateMachine stateMachine in _fixedRemove)
            {
                RemoveLocal(stateMachine);
            }

            _fixedRemove.Clear();
        }

        public static void AddLocal(StateMachine stateMachine)
        {
            Dictionary<int, StateMachine> targetDictionary = stateMachine.GetUpdateType() switch
            {
                UpdateType.FixedUpdate => Instance._fixedStateMachines,
                _ => Instance._stateMachines
            };
            int ID = stateMachine.GetID();
            targetDictionary.TryAdd(ID, stateMachine);
        }

        public static void RemoveLocal(StateMachine stateMachine)
        {
            Dictionary<int, StateMachine> targetDictionary = stateMachine.GetUpdateType() switch
            {
                UpdateType.FixedUpdate => Instance._fixedStateMachines,
                _ => Instance._stateMachines
            };
            targetDictionary.Remove(stateMachine.GetID());
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;
            Clear();
        }

        public static void Clear()
        {
            foreach (StateMachine stateMachine in Instance._stateMachines.Values)
            {
                stateMachine.OnManagerDestroyed();
            }

            foreach (StateMachine stateMachine in Instance._fixedStateMachines.Values)
            {
                stateMachine.OnManagerDestroyed();
            }

            Instance._stateMachines.Clear();
            Instance._fixedStateMachines.Clear();
        }

        private void Update()
        {
            ProcessUpdateRemove();
            ProcessUpdateAdd();
            foreach (StateMachine stateMachine in _stateMachines.Values)
            {
                stateMachine.UpdateStateMachine();
            }
        }

        private void FixedUpdate()
        {
            ProcessFixedRemove();
            ProcessFixedAdd();
            foreach (StateMachine stateMachine in _fixedStateMachines.Values)
            {
                stateMachine.UpdateStateMachine();
            }
        }
    }
}