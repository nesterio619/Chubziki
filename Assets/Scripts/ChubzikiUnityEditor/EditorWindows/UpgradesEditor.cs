#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using Upgrades;
using Upgrades.CarUpgrades;

namespace ChubzikiUnityEditor.EditorWindows
{
    public class UpgradesEditor : EditorWindow
    {

        [FormerlySerializedAs("_upgradeContainer")] public UpgradesList UpgradeList = new();

        private bool _showSpeed = false;
        private bool _showPower = false;
        private bool _showControl = false;
        private bool _showSurvive = false;

        [MenuItem("Chubziki/UpgradesEditor")]
        public static void ShowWindow() =>
            GetWindow<UpgradesEditor>("UpgradesEditor");

        private void OnEnable()
        {
            UpgradeList = UpgradeConfigs.LoadCarUpgrades(UpgradeConfigs.PLAYER_CAR_UPGRADES_CONFIG_NAME);
        }

        private void OnGUI()
        {
            _showSpeed = EditorGUILayout.Foldout(_showSpeed, "Speed");
            if (_showSpeed)
                ShowUpgrades(UpgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Speed], "Speed");


            _showPower = EditorGUILayout.Foldout(_showPower, "Power");
            if (_showPower)
                ShowUpgrades(UpgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Power], "Power");


            _showControl = EditorGUILayout.Foldout(_showControl, "Control");
            if (_showControl)
                ShowUpgrades(UpgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Control], "Control");


            _showSurvive = EditorGUILayout.Foldout(_showSurvive, "Survive");
            if (_showSurvive)
                ShowUpgrades(UpgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Survive], "Survive");

            if (GUILayout.Button("Save changes"))
            {
                UpgradeConfigs.SaveCarUpgrades(UpgradeConfigs.PLAYER_CAR_UPGRADES_CONFIG_NAME, UpgradeList);

                Debug.Log("Upgrade changes saved.");

                AssetDatabase.Refresh();
            }

        }

        private void ShowUpgrades(CarUpgradeBase carUpgradeBase, string nameOfUpgrade)
        {
            GUILayout.Label(nameOfUpgrade);

            CarUpgradeBase.CarUpgradesValue value;

            List<string> keys = new();

            foreach (var carProperty in carUpgradeBase.CarUpgradesDictionary)
            {
                keys.Add(carProperty.Key);
            }

            foreach (var item in keys)
            {
                EditorGUILayout.LabelField(item);

                value.BaseValue = EditorGUILayout.FloatField("Base Value", carUpgradeBase.CarUpgradesDictionary[item].BaseValue);
                value.ValuePerUpgrade = EditorGUILayout.FloatField("Value Per Update", carUpgradeBase.CarUpgradesDictionary[item].ValuePerUpgrade);

                carUpgradeBase.CarUpgradesDictionary[item] = value;

                GUILayout.Space(5f);
            }

            GUILayout.Space(5f);
        }


    }
}
#endif