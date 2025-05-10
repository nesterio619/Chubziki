using System;
using Core;
using System.Collections.Generic;
using UnityEngine;
using NWH.WheelController3D;

namespace Components.Car.CarLogic
{
    [System.Serializable]
    public class WheelAnimator : IDisposable
    {
        [SerializeField]
        private List<PairControllerMesh> pairControllerMeshes = new();

        private bool _isInitialized = false;

        public void Initialize(MeshFilter[] wheelsModels, WheelController[] wheelControllers)
        {
            if(_isInitialized)
            {
                Debug.LogWarning("WheelAnimator already initialized");
                return;
            }

            if(wheelsModels.Length != wheelControllers.Length)
            {
                Debug.LogWarning("Not same amount of wheels!");
                return;
            }


            pairControllerMeshes.Clear();

            PairControllerMesh pair;

            for (int index = 0; index < wheelsModels.Length; index++)
            {
                pair = new PairControllerMesh();

                pair.WheelController = wheelControllers[index];
                pair.WheelMesh = wheelsModels[index].gameObject;

                pairControllerMeshes.Add(pair);
            }

            _isInitialized = true;

            Player.Instance.OnUpdateEvent += OnUpdate;
        }

        public void OnUpdate()
        {
            AnimateWheelMeshes();
        }

        private void AnimateWheelMeshes()
        {
            foreach (var item in pairControllerMeshes)
            {
                item.WheelMesh.transform.position = item.WheelController.WheelVisual.transform.position;
                item.WheelMesh.transform.rotation = item.WheelController.WheelVisual.transform.rotation;
            }
        }
        
        public void Dispose()
        {
            if (Player.Instance == null) return;
            
            Player.Instance.OnUpdateEvent -= OnUpdate;
            _isInitialized = false;
        }

        [System.Serializable]
        public class PairControllerMesh
        {
            public WheelController WheelController;

            public GameObject WheelMesh;
        }

       
    }
}