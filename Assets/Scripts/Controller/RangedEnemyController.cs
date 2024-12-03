using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyController : EnemyController
{
    private float followRange = float.MaxValue;
    [SerializeField] private float shootRange = 10f;
    private int layerMaskLevel;
    private int layerMaskTarget;
    RaycastHit hit;

    private ShootingController shootingController;

    protected override void Start()
    {
        base.Start();
        layerMaskLevel = LayerMask.NameToLayer("Level");
        layerMaskTarget = stats.CurrentStat.attackSO.target;
        shootingController = GetComponent<ShootingController>();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        float distanceToTarget = DistanceToTarget();
        Vector3 directionToTarget = DirectionToTarget();

        UpdateEnemyState(distanceToTarget, directionToTarget);
        Rotate(directionToTarget);
    }

    private void UpdateEnemyState(float distance, Vector3 direction)
    {
        IsAttacking = false; 

        if (distance <= followRange)
        {
            CheckIfNear(distance, direction);
        }
        else
        {
            CallMoveEvent(Vector3.zero);
        }
    }
    private void CheckIfNear(float distance, Vector3 direction)
    {
        if (distance <= shootRange)
        {
            CallMoveEvent(Vector3.zero);
            TryShootAtTarget(direction);
        }
        else
        {
            CallMoveEvent(direction); 
        }
    }

    private void TryShootAtTarget(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, shootRange, GetLayerMaskForRaycast()))
        {
            Physics.Raycast(transform.position, direction, out hit);
        }

        if (IsTargetHit(hit))
        {
            shootingController.targetCol = hit.collider;
            PerformAttackAction(direction);
        }
        else
        {
            CallMoveEvent(direction);
        }
    }


    private int GetLayerMaskForRaycast()
    {
        return (1 << layerMaskLevel) | layerMaskTarget;
    }

    private bool IsTargetHit(RaycastHit hit)
    {
        return hit.collider != null && layerMaskTarget == (layerMaskTarget | (1 << hit.collider.gameObject.layer));
    }

    private void PerformAttackAction(Vector3 direction)
    {
        CallLookEvent(direction);
        CallMoveEvent(Vector3.zero);
        IsAttacking = true;
    }

    private void Rotate(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction).normalized;
    }

}
