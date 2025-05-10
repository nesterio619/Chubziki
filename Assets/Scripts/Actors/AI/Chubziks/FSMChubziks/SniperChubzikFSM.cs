using Actors;
using Actors.AI.Chubziks.Base;
using Actors.Molds;
using Components.Animation;
using Components.Particles;
using Components.ProjectileSystem.AttackPattern;
using Core;
using Core.Interfaces;
using Core.ObjectPool;
using Regions;
using RSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperChubzikFSM : ChubzikActor, IStateBehaviour
{
    private const float StopReturnInLocationDistance = 2;

    #region Conditions
    [Condition] public bool CanAttack => _currentRangeAttackPattern.CanAttack;
    [Condition] public bool IsAttacking => _isAttackingAnimation;
    [Condition] public bool IsShootingDistance => Vector3.Distance(currentTarget.position, transform.position) < _distanceOfAttack * _startAimingPercentFromMaxAimingDistance;
    [Condition] public bool IsTooFarDistance => Vector3.Distance(currentTarget.position, transform.position) > _distanceOfAttack;
    [Condition] public bool IsRetreatDistance => Vector3.Distance(currentTarget.position, transform.position) < _distanceOfAttack * tooCloseToShootPercent;
    [Condition] public bool ReadyToAttack => _isReadyToShoot;
    [Condition] public bool IsNavMeshAgentActive => navMeshAgent.enabled;
    [Condition] public bool IsStunned => !_isStanding;
    [Condition] public bool IsLogicActive => _isLogicActive;
    [Condition] public bool IsPlayerInSameLocation => assignedLocation.Bounds.Contains(currentTarget.position) || isStoppedReturningToOwnLocation;
    [Condition] public bool StopReturnInLocation => returnToSector && Vector3.Distance(transform.parent.position, transform.position) < StopReturnInLocationDistance;
    #endregion

    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask obstaclesLayer;
    [SerializeField] private AI.AimProvider.AimingUserData aimData;

    private float _startAimingPercentFromMaxAimingDistance;
    private float tooCloseToShootPercent;
    private float _distanceOfAttack;
    private bool _isReadyToShoot;
    private RangedAttackPattern _currentRangeAttackPattern;
    private Coroutine _isAimingCoroutine;

    private Vector2 _aimingDelay = new Vector2(1, 2);

    private Vector3 _currentRetreatPosition = new Vector3(0, -999, 0);

    private float _rotatingSpeed = 1;

    public bool VisualShootingRanges = false;

    private void Awake()
    {
        navMeshAgent.enabled = false;
        _defaultSpeed = navMeshAgent.speed;
    }

    private void RestartNavMesh()
    {
        navMeshAgent.enabled = false;
        navMeshAgent.enabled = true;
    }

    public override void LoadActor(ChubzikMold actorMold, ChubzikModel chubzikModel, AttackPattern attackPattern)
    {
        if (actorMold is not ChubzikSniperMold)
        {
            Debug.Log("Wrong mold for chubzik melee");
            return;
        }

        var sniperConstructorChubzik = actorMold as ChubzikSniperMold;

        currentTarget = Player.Instance.PlayerCarGameObject.transform;

        _startAimingPercentFromMaxAimingDistance = sniperConstructorChubzik.StartAimingPercentFromMaxAimingDistance;
        tooCloseToShootPercent = sniperConstructorChubzik.TooCloseistancePercent;
        _currentRangeAttackPattern = attackPattern as RangedAttackPattern;
        _distanceOfAttack = _currentRangeAttackPattern.FiringRadius;

        base.LoadActor(actorMold, chubzikModel, attackPattern);

        _rotatingSpeed = _currentRangeAttackPattern.RotationSpeed;
    }

    public override void ToggleLogic(bool stateToSet)
    {
        if (IsVisible)
            return;

        base.ToggleLogic(stateToSet);

        RestartNavMesh();

        _isLogicActive = stateToSet;
    }

    public override Transform GetAttackPoint()
    {
        return attackPoint;
    }

    protected override void ChubzikPerformingAttack()
    {
        _currentRangeAttackPattern.PerformAttack();
    }

    protected override void ReturnToPoolAttackPattern()
    {
        _stopAimAndSearch?.Invoke();

        if (_currentRangeAttackPattern != null)
        {
            _currentRangeAttackPattern.ReturnToPool();
            _currentRangeAttackPattern = null;
        }
    }

    /// <summary>
    /// States
    /// </summary>

    #region States

    #region Active
    [State]
    private void Active()
    {

    }
    #endregion

    #region Idle
    [State]
    private void EnterIdle()
    {
        actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Idle);
    }

    [State]
    private void Idle()
    {
        if (IsShootingDistance)
            isStoppedReturningToOwnLocation = true;

        RotatingToTarget();
    }

    [State]
    private void ExitIdle()
    {
        isStoppedReturningToOwnLocation = false;
    }
    #endregion

    #region Move

    [State]
    private void EnterMove()
    {
        actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Disturb);
        navMeshAgent.isStopped = false;
    }

    [State]
    private void Move()
    {
        if (outsideSectorCoroutine == null && !returnToSector && !IsInsideOwnLocation && !isStoppedReturningToOwnLocation)
            outsideSectorCoroutine = StartCoroutine(ReturnToSectorDelay());

        if (IsInsideOwnLocation && outsideSectorCoroutine != null)
        {
            StopCoroutine(outsideSectorCoroutine);
            outsideSectorCoroutine = null;
        }

        Vector3 destination = currentTarget.position;
        if (returnToSector)
        {
            if (IsInsideOwnLocation && IsPlayerInSameLocation)
                returnToSector = false;
            else
                destination = transform.parent.position;
        }

        navMeshAgent.SetDestination(destination);
    }

    [State]
    private void ExitMove()
    {
        _currentRetreatPosition.y = -999;
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();

        if (IsInsideOwnLocation) returnToSector = false;
    }

    private IEnumerator ReturnToSectorDelay()
    {
        yield return new WaitForSeconds(outsideSectorTimeLimit);

        returnToSector = true;
        outsideSectorCoroutine = null;
    }

    #endregion

    #region Attack

    [State]
    private void EnterAttack()
    {
        _isAttackingAnimation = true;

        actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Attack);
    }

    #endregion

    #region Stun
    [State]
    private void EnterStunned()
    {
        if (_currentHealth > 0)
            currentStunParticle = PooledParticle.TryToLoadAndPlay(particleStunPool, null);

        _isAttackingAnimation = false;
        navMeshAgent.enabled = false;
        actorAnimationController.StopAnimation();
    }

    [State]
    private void Stunned()
    {
        if (currentStunParticle == null)
            return;

        currentStunParticle.transform.position = actorRigidbody.position + stunParticlesPoisitionOffset;
    }

    [State]
    private void ExitStunned()
    {
        navMeshAgent.enabled = true;

        if (currentStunParticle != null)
            currentStunParticle.StopParticle();
        currentStunParticle = null;
    }
    #endregion

    #region Aiming

    [State]
    private void EnterAiming()
    {
        if (_isAimingCoroutine == null)
            _isAimingCoroutine = StartCoroutine(AimingTimer());
    }

    private bool _isChangingAimingPosition = false;
    private bool _stopAimingLogic = false;
    private bool _isAimingAnimation = false;
    private bool _isMovingWhileAiming = false;
    private bool _isObstacleOnTrajectory;
    private System.Action _stopAimAndSearch;

    [State]
    private void Aiming()
    {

        _isObstacleOnTrajectory = IsObstaclesOnTrajectoryOfShoot(transform.position, currentTarget.position);

        if (!_isObstacleOnTrajectory)
        {
            _isChangingAimingPosition = false;
            navMeshAgent.isStopped = true;

            if (!_isAimingAnimation)
            {
                _isAimingAnimation = true;
                actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Aim);
            }

            if (_isAimingCoroutine == null && !_stopAimingLogic)
                _isAimingCoroutine = StartCoroutine(AimingTimer());

            if (_stopAimAndSearch == null)
                AimRotating();
        }
        else
        {
            _stopAimAndSearch?.Invoke();
            _stopAimAndSearch = null;

            _isAimingAnimation = false;
        }

        if (_stopAimingLogic)
            return;

        if (!_isChangingAimingPosition && _isObstacleOnTrajectory)
        {
            _currentRetreatPosition = CheckRandomPositions();

            if (_isAimingCoroutine != null)
                StopCoroutine(_isAimingCoroutine);

            _isAimingCoroutine = null;

            if (_currentRetreatPosition == transform.position)
            {
                _currentRetreatPosition = GetRandomReachablePosition();
                return;
            }

            _isChangingAimingPosition = true;
        }

        if (_isChangingAimingPosition)
        {
            if (!_isMovingWhileAiming)
            {
                _isAimingAnimation = true;
                actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Disturb);
            }

            navMeshAgent.SetDestination(_currentRetreatPosition);

            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_currentRetreatPosition.x, _currentRetreatPosition.z)) < 0.5f)
            {
                _isChangingAimingPosition = false;
                StartCoroutine(StopAimingLogic());
            }

            navMeshAgent.isStopped = false;
            if (_isAimingCoroutine != null)
                StopCoroutine(_isAimingCoroutine);
        }
        else
        {
            _isAimingAnimation = false;
        }
    }

    [State]
    private void ExitAiming()
    {
        if (_isAimingCoroutine != null)
            StopCoroutine(_isAimingCoroutine);

        _stopAimAndSearch?.Invoke();
        _stopAimAndSearch = null;

        _isAimingCoroutine = null;
        _isChangingAimingPosition = false;

        isStoppedReturningToOwnLocation = true;

    }

    private bool CanReachTarget(Vector3 targetPosition)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        if (navMeshAgent.CalculatePath(targetPosition, path))
        {
            return path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private IEnumerator AimingTimer()
    {
        _isReadyToShoot = false;

        yield return new WaitForSeconds(Random.Range(_aimingDelay.x, _aimingDelay.y));

        _isReadyToShoot = true;

    }

    private IEnumerator StopAimingLogic()
    {
        _stopAimingLogic = true;

        yield return new WaitForSeconds(Random.Range(_aimingDelay.x, _aimingDelay.y));

        _stopAimingLogic = false;
    }

    #endregion

    #region Retreat

    [State]
    private void EnterRetreat()
    {
        actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Disturb);
        navMeshAgent.isStopped = false;
        FindRetreatPosition();
    }

    [State]
    private void Retreat()
    {
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_currentRetreatPosition.x, _currentRetreatPosition.z)) < 0.5f)
        {
            FindRetreatPosition();
        }
        else
        {
            navMeshAgent.SetDestination(_currentRetreatPosition);
        }
    }

    [State]
    private void ExitRetreat()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }
    #endregion

    #endregion

    #region ChubzikGetAttacked

    protected override void StopReturnToOwnLocation()
    {
        if (outsideSectorCoroutine != null)
            StopCoroutine(outsideSectorCoroutine);
        outsideSectorCoroutine = null;

        isStoppedReturningToOwnLocation = true;
        returnToSector = false;
    }


    #endregion

    private bool IsObstaclesOnTrajectoryOfShoot(Vector3 myPosition, Vector3 targetPosition)
    {
        float yOffset = 1;
        targetPosition.y += yOffset;
        myPosition.y += yOffset;

        Vector3 direction = targetPosition - myPosition;
        direction.Normalize();

        float distance = Vector3.Distance(targetPosition, myPosition);

        return Physics.Raycast(myPosition, direction, distance, obstaclesLayer);
    }

    #region Rotating

    private void RotatingToTarget()
    {
        Vector3 dir = currentTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        RotateChubzik(lookRotation);
    }

    private void AimRotating()
    {
        AI.AimProvider.StartSearchAndAim(aimData, _currentRangeAttackPattern, RotateChubzik, out _stopAimAndSearch);
    }

    private void RotateChubzik(Quaternion quaternion)
    {
        var lookDirection = quaternion;

        attackPoint.rotation = lookDirection;
        attackPoint.localRotation = Quaternion.Euler(attackPoint.localRotation.eulerAngles.x, 0, attackPoint.localRotation.eulerAngles.z);

        lookDirection.x = 0;
        lookDirection.z = 0;

        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, _rotatingSpeed);
    }

    #endregion 

    #region SearchingPosition

    private void FindRetreatPosition()
    {
        List<DiagonalDirections> diagonals = new List<DiagonalDirections> { DiagonalDirections.UpLeft, DiagonalDirections.UpRight, DiagonalDirections.DownRight, DiagonalDirections.DownLeft };

        diagonals.Remove(GetDiagonalDirectionOfTarget(currentTarget.position));

        do
        {
            _currentRetreatPosition = GetRandomDiagonalPosition(transform.position, _distanceOfAttack * tooCloseToShootPercent, diagonals[Random.Range(0, diagonals.Count)]);

        } while (!navMeshAgent.SetDestination(_currentRetreatPosition));
    }

    private DiagonalDirections GetDiagonalDirectionOfTarget(Vector3 targetPosition)
    {
        Vector3 agentPosition = transform.position;

        if (targetPosition.x > agentPosition.x && targetPosition.z > agentPosition.x)
        {
            return DiagonalDirections.UpRight;
        }
        else if (targetPosition.x > agentPosition.x && targetPosition.z < agentPosition.x)
        {
            return DiagonalDirections.DownRight;
        }
        else if (targetPosition.x < agentPosition.x && targetPosition.z < agentPosition.x)
        {
            return DiagonalDirections.DownLeft;
        }
        else
        {
            return DiagonalDirections.UpLeft;
        }

    }

    private Vector3 GetRandomDiagonalPosition(Vector3 myPosition, float distance, DiagonalDirections diagonalDirections)
    {
        Vector3 retreatPosition = myPosition;

        switch (diagonalDirections)
        {
            case DiagonalDirections.UpLeft:
                retreatPosition.x = Random.Range(myPosition.x, -distance + myPosition.x);
                retreatPosition.z = Random.Range(myPosition.z, distance + myPosition.z);

                break;
            case DiagonalDirections.UpRight:
                retreatPosition.x = Random.Range(myPosition.x, distance + myPosition.x);
                retreatPosition.z = Random.Range(myPosition.z, distance + myPosition.z);

                break;
            case DiagonalDirections.DownRight:
                retreatPosition.x = Random.Range(myPosition.x, distance + myPosition.x);
                retreatPosition.z = Random.Range(myPosition.z, -distance + myPosition.z);

                break;
            case DiagonalDirections.DownLeft:
                retreatPosition.x = Random.Range(myPosition.x, -distance + myPosition.x);
                retreatPosition.z = Random.Range(myPosition.z, -distance + myPosition.z);

                break;
            default:
                Debug.LogWarning("Wrong direction");

                break;
        }

        return retreatPosition;
    }

    private Vector3 GetRandomReachablePosition()
    {
        Vector3 randomPosition = Vector3.zero;

        do
        {
            randomPosition = GetRandomDiagonalPosition(transform.position, _distanceOfAttack * tooCloseToShootPercent, (DiagonalDirections)Random.Range(0, 3));

        } while (!CanReachTarget(randomPosition));

        return randomPosition;
    }

    private Vector3 CheckRandomPositions()
    {
        float yOffset = 5;

        for (int diagonal = 0; diagonal < 4; diagonal++)
        {
            for (int index = 0; index < 4; index++)
            {
                Vector3 randomPosition = GetRandomDiagonalPosition(transform.position, _distanceOfAttack * tooCloseToShootPercent, (DiagonalDirections)diagonal);
                randomPosition.y += yOffset;

                if (!Physics.Raycast(randomPosition, Vector3.down, out RaycastHit raycastHit, 10, groundLayer))
                    continue;

                randomPosition = raycastHit.point;

                if (!IsObstaclesOnTrajectoryOfShoot(randomPosition, currentTarget.position) && CanReachTarget(randomPosition))
                {
                    return randomPosition;
                }
            }
        }

        return transform.position;

    }

    #endregion

    public enum DiagonalDirections
    {
        UpLeft,
        UpRight,
        DownRight,
        DownLeft

    }

    private void OnDrawGizmos()
    {
        if (!VisualShootingRanges)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _distanceOfAttack);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _distanceOfAttack * _startAimingPercentFromMaxAimingDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _distanceOfAttack * tooCloseToShootPercent);

        Gizmos.DrawRay(attackPoint.position, transform.forward);
    }

}
