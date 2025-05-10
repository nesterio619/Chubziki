using UnityEngine;
using DG.Tweening;
using Core.ObjectPool;
using Core;

public class AnimatedPooledParticle : PooledGameObject
{
    [System.Serializable]
    public struct RotateConfig
    {
        public Transform Target;
        public Vector3 Angle;
    }

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateEnabled = true;
    [SerializeField] private float rotationSpeedMultiplier = 1f;
    [SerializeField] private RotateConfig[] rotations;


    [Header("Animation Settings")]
    [SerializeField] private Transform parentScale;
    [SerializeField] private AnimationType animationType;

    private float _scaleDuration;

    private void OnEnable()
    {
        if (rotateEnabled && Player.Instance != null)
            Player.Instance.OnUpdateEvent += HandleAutoRotation;
    }

    private void OnDisable()
    {
        Player.Instance.OnUpdateEvent -= HandleAutoRotation;
    }

    private void HandleAutoRotation()
    {
        if (!rotateEnabled) return;

        foreach (var config in rotations)
        {
            if (config.Target != null)
            {
                config.Target.Rotate(
                    config.Angle * (Time.deltaTime * rotationSpeedMultiplier),
                    Space.Self
                );
            }
        }
    }

    public void EndAnimation()
    {
        switch (animationType)
        {
            case AnimationType.Scale:
                ScaleToZero();
                break;
        }
    }

    public void SetDefaultState()
    {
        parentScale.localScale = Vector3.one;
    }

    public void SetDurationScaling(float scaleDuration)
    {
        _scaleDuration = scaleDuration;
    }

    public void ScaleToZero()
    {
        parentScale.DOScale(Vector3.zero, _scaleDuration);
    }


    public enum AnimationType
    {
        None,
        Scale
    }
}
