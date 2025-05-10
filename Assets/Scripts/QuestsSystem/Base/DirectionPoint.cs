using Core;
using UI;
using UnityEngine;

namespace QuestsSystem.Base
{
    public class DirectionPoint : MonoBehaviour
    {
        public static DirectionPoint Instance { get; private set; }

        private CompassArrowCanvasCommand _compassCommand;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            gameObject.SetActive(false);
        }

        public void Show(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);

            if (_compassCommand == null)
            {
                var playerTransform = Player.Instance.PlayerCarGameObject.transform;
                _compassCommand = new CompassArrowCanvasCommand(CarCanvasReceiver.Instance, playerTransform);
            }
            else _compassCommand.EnableImage(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            transform.position = Vector3.zero;

            _compassCommand?.EnableImage(false);
        }

        private void FixedUpdate()
        {
            if(_compassCommand!=null && _compassCommand.IsEnabled) 
                _compassCommand.Update();
        }

        private void OnDestroy()
        {
            _compassCommand?.Dispose();
        }
    }
}