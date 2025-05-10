using Core.ObjectPool;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifiableParticlesConfig ", menuName = "Particles/ModifiableParticlesConfig")]
public class DynamicParticlesConfig : ScriptableObject
{
    public PrefabPoolInfo ModifiableParticlesPool;

    public Gradient ParticlesChangingGradient;
}
