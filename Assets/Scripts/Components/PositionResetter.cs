using System;
using Core.Utilities;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class PositionResetter : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;

    [SerializeField] private float maxDistance;
    
    [SerializeField] float positionResetTimeOffset = 5;
    
    [Space]
    [SerializeField] private float _jumpPower = 5f;
    
    [SerializeField] private int _jumpNum = 1;
    
    [SerializeField] private float _jumpDuration = 2f;
    
    [SerializeField] private Ease _jumpEase = Ease.OutBounce;

    private bool _isResetting;
    
    private Vector3 _savedPosition;
    
    private Sequence _movingBall;
    
    public void SavePosition() =>
        _savedPosition = gameObject.transform.localPosition;

    private void FixedUpdate()
    {
        if (!_isResetting && Vector3.Distance(transform.localPosition, _savedPosition) > maxDistance)
        {
            ResetPositionAfterTimer();
        }
    }

    public void ResetPositionAfterTimer()
    {
        if(isActiveAndEnabled == false || _isResetting)
            return;
        
        _isResetting = true;
        
        UtilitiesProvider.WaitAndRun(ResetPositionDoTweenJump, false, positionResetTimeOffset);
    }

    public void ResetPositionDoTweenJump()
    {
        _isResetting = false;

        if (gameObject == null || !gameObject.activeSelf) return;
        if(rigidbody == null || transform.parent == null || _movingBall.IsActive()) return;
        
        rigidbody.isKinematic = true;

        _movingBall = transform.DOJump(transform.parent.position, _jumpPower, _jumpNum, _jumpDuration)
            .SetEase(_jumpEase)
            .OnComplete(() =>
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = false;
            });
        
            
        if(rigidbody.interpolation != RigidbodyInterpolation.None)
            Physics.SyncTransforms();
    }
    
    public void ResetPositionNow()
    {
        _isResetting = false;
        
        if (this == null || gameObject == null || !isActiveAndEnabled)
            return;

        transform.SetLocalPositionAndRotation(_savedPosition, quaternion.identity);

        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            
            if(rigidbody.interpolation != RigidbodyInterpolation.None)
                Physics.SyncTransforms();
        }
    }

    private void OnDestroy() => _movingBall.Kill();
}
