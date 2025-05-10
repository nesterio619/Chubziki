#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(RangedAttackConfig))]
public class RangedAttackConfigEditor : Editor
{
    SerializedProperty projectileType;
    SerializedProperty shootParticlesPoolInfo;
    SerializedProperty firingRadius;
    SerializedProperty minimalAngleToShoot;
    SerializedProperty rotationSpeed;
    SerializedProperty minSpread;
    SerializedProperty maxSpread;
    SerializedProperty spreadCurve;
    SerializedProperty projectilesPerShoot;
    SerializedProperty shootLooping;
    SerializedProperty totalTimeBetweenShots;
    SerializedProperty isBurstFire;
    SerializedProperty shotsPerBurst;
    SerializedProperty pauseBetweenBursts;
    SerializedProperty ParticlesOnShootPool;

    void OnEnable()
    {
        projectileType = serializedObject.FindProperty("projectileType");
        shootParticlesPoolInfo = serializedObject.FindProperty("shootParticlesPoolInfo");
        firingRadius = serializedObject.FindProperty("FiringRadius");
        minimalAngleToShoot = serializedObject.FindProperty("MinimalAngleToShoot");
        rotationSpeed = serializedObject.FindProperty("RotationSpeed");
        minSpread = serializedObject.FindProperty("MinSpread");
        maxSpread = serializedObject.FindProperty("MaxSpread");
        spreadCurve = serializedObject.FindProperty("SpreadCurve");
        projectilesPerShoot = serializedObject.FindProperty("projectilesPerShoot");
        shootLooping = serializedObject.FindProperty("shootLooping");
        totalTimeBetweenShots = serializedObject.FindProperty("totalTimeBetweenShots");
        isBurstFire = serializedObject.FindProperty("isBurstFire");
        shotsPerBurst = serializedObject.FindProperty("shotsPerBurst");
        pauseBetweenBursts = serializedObject.FindProperty("pauseBetweenBursts");
        ParticlesOnShootPool = serializedObject.FindProperty("ParticlesOnShootPool");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(8);
        EditorGUILayout.PropertyField(projectileType);

        EditorGUILayout.PropertyField(ParticlesOnShootPool);
        EditorGUILayout.PropertyField(shootParticlesPoolInfo);

        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(firingRadius);
        EditorGUILayout.PropertyField(minimalAngleToShoot);
        EditorGUILayout.Slider(rotationSpeed, 0.01f, 3f);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Spread Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minSpread);
        EditorGUILayout.PropertyField(maxSpread);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(spreadCurve);
        EditorGUILayout.LabelField(" ", "X axis:  0 is 0, 1 is FiringRadius");
        EditorGUILayout.LabelField(" ", "Y axis:  0 is MinSpread, 1 is MaxSpread");

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shooting Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(projectilesPerShoot);
        EditorGUILayout.PropertyField(shootLooping);
        EditorGUILayout.PropertyField(totalTimeBetweenShots);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Burst Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isBurstFire);
        if (isBurstFire.boolValue)
        {
            EditorGUILayout.PropertyField(shotsPerBurst);
            EditorGUILayout.PropertyField(pauseBetweenBursts);
        }

        serializedObject.ApplyModifiedProperties();
    }

}
#endif