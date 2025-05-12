using Core;
using Core.Interfaces;
using System;
using UnityEngine;

namespace Components.RamProvider
{
    public abstract class RamProvider<ColliderHandler> : ColliderHandlerBase, IAttacker where ColliderHandler : ColliderHandlerBase
    {
        private const string Name_Of_Config_File = "/RamProviderConfig.json";

        [field: SerializeField, Range(0f, 15f)] public int DefaultDamage { get; protected set; }
        [SerializeField][Range(0f, 3f)] public float DefaultPushForce;

        public ColliderHandler CurrentColliderHandler;
        public int AttackDamage => DefaultDamage;
        public abstract int GetDamageAmount { get; }

        private static RamConfigData _configData;
        private static float _minExtraPushAngle;
        private static float _maxExtraPushAngle;

        private static RamConfigData GetConfigData
        {
            get
            {
                if (_configData == null)
                {
                    _configData = JSONParser.Load<RamConfigData>(Name_Of_Config_File);
                    ParsePushAngleRange();
                }

                return _configData;
            }
        }


        // Minimum force required to trigger a successful ram
        private static float minimumForceToRam => GetConfigData.MinimumForceToRam;

        // Modifier for how mass difference affects ram force
        private static float massDifferenceModifier => GetConfigData.MassDifferenceModifier;

        // Modifier for converting ram force into damage
        private static float forceDamageModifier => GetConfigData.ForceDamageModifier;

        // Used for scaling damage based on target's speed
        private static float speedDamageModifier => GetConfigData.SpeedDamageModifier;

        // Used to make the actors fly upwards
        private static string equipmentExtraPushAngleRange => GetConfigData.EquipmentExtraPushAngleRandomRange;

        private protected static float GetPowerOfHit(float OwnWeightA, float TargetWeightB, float OwnSpeedA, float TargetSpeedB)
        {
            return RamForceFormula(OwnWeightA, TargetWeightB, OwnSpeedA, TargetSpeedB) / minimumForceToRam - 1;
        }

        private protected static bool ShouldRam(float OwnWeightA, float TargetWeightB, float OwnSpeedA, float TargetSpeedB)
        {
            return RamForceFormula(OwnWeightA, TargetWeightB, OwnSpeedA, TargetSpeedB) > minimumForceToRam;
        }

        private protected static float RamForceFormula(float OwnWeightA, float TargetWeightB, float OwnSpeedA, float TargetSpeedB)
        {
            // Calculates the push force based on mass difference and combined speeds
            return Mathf.Abs(OwnWeightA - TargetWeightB) / massDifferenceModifier * (OwnSpeedA + TargetSpeedB);
        }


        private protected virtual Vector3 RamDirection(Vector3 pushedObjectPosition)
        {
            return (pushedObjectPosition - transform.position).normalized;
        }

        private static void ParsePushAngleRange()
        {
            var range = equipmentExtraPushAngleRange.Split('-');
            if (range.Length != 2 ||
                !float.TryParse(range[0], out _minExtraPushAngle) ||
                !float.TryParse(range[1], out _maxExtraPushAngle))
            {
                Debug.LogError("Invalid EquipmentExtraPushAngleRandomRange format. Expected 'min-max'.");
                _minExtraPushAngle = 0f;
                _maxExtraPushAngle = 0f;
            }
        }

        private protected static float GetRandomExtraPushAngle()
        {
            return UnityEngine.Random.Range(_minExtraPushAngle, _maxExtraPushAngle);
        }


        private protected static int MultiplyDamageByForce(int damage, float force)
        {
            // Scale the damage proportionally to the impact force
            // The modifier (Force_To_Damage_Modifier) ensures damage doesn't scale too high
            return Mathf.RoundToInt(damage * force / forceDamageModifier);
        }

        private protected static int MultiplyDamageBySpeed(int damage, float speed)
        {
            // Damage is calculated as a fraction of the target's speed multiplied by attacker's damage
            return Mathf.RoundToInt(speed / speedDamageModifier * damage);
        }

        [Serializable]
        private class RamConfigData
        {
            public float MinimumForceToRam;
            public float MassDifferenceModifier;
            public float ForceDamageModifier;
            public float SpeedDamageModifier;
            public string EquipmentExtraPushAngleRandomRange;
        }
    }
}