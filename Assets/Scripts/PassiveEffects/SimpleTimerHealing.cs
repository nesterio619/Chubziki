using System.Collections;
using Components.Car;
using Core;
using UnityEngine;

namespace PassiveEffects
{
    public class SimpleTimerHealing : IHealthEffect
    {
        private bool _shouldResetTimer = false;
    
        #region Timer settings
        public int DefaultHealthChangeRate { get; } = 3;
        public int DefaultHealthChangeAmount { get; } = 1;
    
        private const float _waitBeforeHeal = 12f;
        #endregion
    
        private bool CarHealthIsFull => _carInfo != null && _carInfo.CurrentHealth == _carInfo.MaxHealth;

        private readonly CarInfo _carInfo;
        private Coroutine _currentLogic = null;

        private float _timeLeftBeforeHeal = _waitBeforeHeal;

        private bool _isActive = false;

        public SimpleTimerHealing(CarInfo carInfo)
        {
            _carInfo = carInfo;
        }

        public void ToggleActive(bool stateToSet)
        {
            if(_isActive == stateToSet)
                return;

            _isActive = stateToSet;

            if (stateToSet == true)
                _currentLogic = Player.Instance.StartCoroutine(MainLogic());
            else if (_currentLogic != null)
            {
                if(Player.Instance != null)
                    Player.Instance.StopCoroutine(_currentLogic);
                
                _currentLogic = null;
            }

        }

        IEnumerator MainLogic()
        {
            while (_isActive && _carInfo != null)
            {
                yield return WaitBeforeHealAfterDamage();
                yield return ApplyPassiveEffect();
            }
        }

        IEnumerator WaitBeforeHealAfterDamage()
        {
            if (CarHealthIsFull)
                yield break;

            if (_shouldResetTimer)
            {
                _timeLeftBeforeHeal = _waitBeforeHeal;
                _shouldResetTimer = false;
            }

            while (_timeLeftBeforeHeal > 0)
            {
                _timeLeftBeforeHeal -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
    
        public IEnumerator ApplyPassiveEffect()
        {
            yield return new WaitForSeconds(DefaultHealthChangeRate);
            
            if(_shouldResetTimer)
                yield break;

            if (CarHealthIsFull)
                _shouldResetTimer = true;
            else
                ChangeHealthBy(DefaultHealthChangeAmount);
        }

        public void TryResetTimer(int changeAmount)
        {
            if(changeAmount >= 0)
                return;
            
            _shouldResetTimer = true;
        }

        public void ChangeHealthBy(int healthChange)
        {
            _carInfo?.ChangeHealthBy(healthChange);
        }
    }
}
