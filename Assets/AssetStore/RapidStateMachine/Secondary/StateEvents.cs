using UnityEngine;
using UnityEngine.Events;

namespace RSM
{
    public class StateEvents : MonoBehaviour
    {
        RSMState _rsmState;
        public UnityEvent enterEvent;
        public UnityEvent exitEvent;

        private void Awake()
        {
            if (_rsmState == null) _rsmState = GetComponent<RSMState>();
        }

        private void OnEnable()
        {
            _rsmState.enterEvent.AddListener(EnterEvent);
            _rsmState.exitEvent.AddListener(ExitEvent);
        }
        private void OnDisable()
        {
            _rsmState.enterEvent.RemoveListener(EnterEvent);
            _rsmState.exitEvent.RemoveListener(ExitEvent);
        }

        private void EnterEvent()
            => enterEvent?.Invoke();
        private void ExitEvent()
            => exitEvent?.Invoke();
    }
}