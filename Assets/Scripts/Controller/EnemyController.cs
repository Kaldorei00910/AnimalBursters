using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : BaseController
{
    protected Transform ClosestTarget { get; private set; }
    public HealthSystem healthSystem;

    public EnemyInfo enemyInfo;

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        ClosestTarget = Data.PlayerTransform;
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnDeath += OnDeath;
        // effect
        healthSystem.OnDeath -= DeathEffect;
        healthSystem.OnDeath += DeathEffect;

    }
    protected virtual void FixedUpdate()
    {

    }
    protected float DistanceToTarget()
    {
        return Vector3.Distance(transform.position, ClosestTarget.position);
    }

    protected Vector3 DirectionToTarget()
    {
        return (ClosestTarget.position - transform.position).normalized;
    }

    private void OnDeath()
    {
        BattleSceneManager.Instance.EnemyKilled();
        ObjectPoolManager.Instance.ReturnObject(enemyInfo.objectName, Data.Pool_Enemy, this.gameObject);
    }
    protected virtual void DeathEffect()
    {
        ObjectPoolManager.Instance.GetObject(Data.Pool_Effect_EnemyDie, Data.Pool_Effect, transform.position);
    }
}
