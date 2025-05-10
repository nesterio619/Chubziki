using System;
using Actors.Molds;
using Core.ObjectPool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Actors
{
    public class Actor : PooledGameObject
    {
        public bool LogicIsEnabled { get; private set; }
        [SerializeField] protected UnityEvent OnEnableLogic = null;
        [SerializeField] protected UnityEvent OnDisableLogic = null;
        [SerializeField] protected Renderer[] Renderers;

        public UnityEvent OnExternalActivation;
        public UnityEvent OnExternalDeactivation;
        
        public Action OnDispose;

        public virtual void ToggleLogic(bool stateToSet)
        {
            LogicIsEnabled = stateToSet;
            
            if(stateToSet == true)
                OnEnableLogic?.Invoke();
            else
                OnDisableLogic?.Invoke();
        }

        protected void ToggleRenderersEnabled(bool stateToSet)
        {
            if (Renderers == null)
                return;

            foreach (var rendererComponent in Renderers)
                rendererComponent.enabled = stateToSet;
        }

        public virtual void LoadActor(Mold actorMold)
        {

        }

        public virtual void ExternalActivation(bool value)
        {

            if (value)
            {
                OnExternalActivation?.Invoke();
            }
            else
                OnExternalDeactivation?.Invoke();
        }
        
        public override void ReturnToPool() // Unload actor into basic assets
        {
            ToggleRenderersEnabled(false);
            ToggleLogic(false);
            OnDispose?.Invoke();
            OnDispose = null;

            base.ReturnToPool();
        }

        public  void SwitchGraphic(bool value)
        {
            ToggleRenderersEnabled(value);
        }

    }

}
