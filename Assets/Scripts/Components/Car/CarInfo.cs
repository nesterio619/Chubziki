using System;
using UI.Debug;
using UnityEngine;
using System.Collections.Generic;
using Upgrades;
using Upgrades.CarUpgrades;
using Actors;
using PassiveEffects;

namespace Components.Car
{
    [Serializable]
    public class CarInfo
    {
        private readonly UpgradesList _startingStats;
        private readonly CarDriving _carDriving;

        private DebugCanvasCommand _speedDebugCanvasCommand;
        private DebugCanvasCommand _healthDebugCanvasCommand;
        private List<DebugCanvasCommand> _debugCanvasCommands;
        public event Action<int> OnChangeHealth = delegate { };
        public event Action OnNoHealthLeft = delegate { };

        private int _maxHealth;

        private int _currentHealth;
        
        public int MaxHealth
        {
            get => _maxHealth;
            private set
            {
                _maxHealth = value;
                if (_currentHealth > _maxHealth)
                {
                    _currentHealth = _maxHealth;
                }
            }
        }

        public int CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            }
        }

        public int MaxSpeed => (int)_startingStats.CarUpgradesList[(int)UpgradesList.Upgrades.Survive].GetValue((int)SurviveCarUpgrade.SurviveUpgrades.Health);

        public float CurrentSpeed => _carDriving.CarSpeed;

        public int Acceleration => (int)_startingStats.CarUpgradesList[(int)UpgradesList.Upgrades.Power].GetValue((int)PowerCarUpgrade.PowerUpgrades.SpeedAcceleration);

        public int SteerAbility => (int)_startingStats.CarUpgradesList[(int)UpgradesList.Upgrades.Control].GetValue((int)ControlCarUpgrade.ControlUpgrades.MaxSteeringAngle);

        public CarInfo(UpgradesList baseStats, CarDriving carDriving, CarActor carActor)
        {
            _startingStats = baseStats;
            
            _carDriving = carDriving;

            ResetHealth();
            if (carActor is PlayerCarActor)
                CreateAndInitializeDebugCommands();
            
            
        }

        public void ResetHealth()
        {
            //SURVIVE
            MaxHealth = (int)_startingStats.CarUpgradesList[(int)UpgradesList.Upgrades.Survive].GetValue((int)SurviveCarUpgrade.SurviveUpgrades.Health);
            CurrentHealth = MaxHealth;

            _carDriving.ChangeStateDamageCar(false);
        }

        public void ChangeHealthBy(int changeAmount)
        {
            CurrentHealth += changeAmount;
            
            OnChangeHealth.Invoke(changeAmount);

            if (CurrentHealth <= 0)
                OnNoHealthLeft?.Invoke();
        }


        public override string ToString()
        {
            return $"CurrentHealth: {CurrentHealth}, " +
                   $"MaxSpeed: {MaxSpeed}, Power: {Acceleration}, " +
                   $"SteerAbility: {SteerAbility}, " +
                   $"CurrentSpeed: {CurrentSpeed}";
        }

        private void CreateAndInitializeDebugCommands()
        {
            _healthDebugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"Current Health : {CurrentHealth}");
            _speedDebugCanvasCommand = new DebugCanvasCommand(DebugCanvasReceiver.Instance, () => $"Current Speed : {_carDriving.GetCarSpeedInKm()} KM/h");

            _debugCanvasCommands = new List<DebugCanvasCommand> { _healthDebugCanvasCommand, _speedDebugCanvasCommand };
        }
        public void UpdateDebugCommands()
        {
            foreach (var command in _debugCanvasCommands)
            {
                command.Update();
            }
        }

        public void UpdateCarUpgrades()
        {
            ResetHealth();
        }

    }
}