using System;
using UnityEngine;
using System.Reflection;

namespace RSM
{
    public class GenericState : RSMState
    {
        private MethodInfo _enter;
        private MethodInfo _tick;
        private MethodInfo _exit;

        public override void OnEnter(RSMState from)
        {
            _enter?.Invoke(stateMachine.behaviour, null);
            base.OnEnter(from);
        }

        public override void Tick()
        {
            _tick?.Invoke(stateMachine.behaviour, null);
            base.Tick();
        }

        public override void OnExit(RSMState to)
        {
            _exit?.Invoke(stateMachine.behaviour, null);
            base.OnExit(to);
        }

        public override void SetStateMachine(StateMachine stateMachine)
        {
            base.SetStateMachine(stateMachine);

            MonoBehaviour mono = (MonoBehaviour)stateMachine.behaviour;

            Type behaviour = mono.GetType();
            _enter = behaviour.GetMethod($"Enter{gameObject.name}", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            _tick = behaviour.GetMethod(gameObject.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            _exit = behaviour.GetMethod($"Exit{gameObject.name}", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        public bool HasEnterMethod() => _enter != null;
        public bool HasTickMethod() => _tick != null;
        public bool HasExitMethod() => _exit != null;

        public void OpenEnter()
        {
            MonoBehaviour mono = (MonoBehaviour)stateMachine.behaviour;
            IDEManager.OpenLine(mono, _enter.Name);
        }
        public void OpenTick()
        {
            MonoBehaviour mono = (MonoBehaviour)stateMachine.behaviour;
            IDEManager.OpenLine(mono, _tick.Name);
        }
        public void OpenExit()
        {
            MonoBehaviour mono = (MonoBehaviour)stateMachine.behaviour;
            IDEManager.OpenLine(mono, _exit.Name);
        }
    }
}