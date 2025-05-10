using Components.Particles;
using System.Collections.Generic;
using UnityEngine;

public class DynamicParticles : PooledParticle
{
    [SerializeField] private Transform scaleParent;
    [SerializeField] protected List<ParticleSystem> _allParticles;

    private Gradient particlesChangeGradient;

    public static DynamicParticles TryToLoadAndPlay(DynamicParticlesConfig config)
    {
        var pooledParticle = Initialize(config.ModifiableParticlesPool) as DynamicParticles;

        pooledParticle.SetDefaultScale();

        pooledParticle.particlesChangeGradient = config.ParticlesChangingGradient;

        return pooledParticle;
    }

    public void SetDefaultScale()
    {
        scaleParent.localScale = Vector3.one;
    }

    public void ChangeParticlesColor(float colorGradientEvaluation)
    {
        foreach (var particle in _allParticles)
        {
            particle.startColor = particlesChangeGradient.Evaluate(colorGradientEvaluation); ;
        }
    }


}
