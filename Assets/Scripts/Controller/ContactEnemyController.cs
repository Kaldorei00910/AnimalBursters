using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ContactEnemyController : EnemyController
{
    private float followRange = float.MaxValue;
    [SerializeField] private LayerMask layerMaskTarget;
    private bool isCollidingWithTarget;

    private HealthSystem collidingTargetHealthSystem;
    private CharacterMovement collidingMovement;

    protected override void Start()
    {
        base.Start();

        healthSystem.OnDamage += OnDamage;
        healthSystem.OnDeath += DropItemOnDeath;
    }

    private void OnEnable()
    {
        collidingTargetHealthSystem = null;
        isCollidingWithTarget = false;
    }
    private void OnDamage()
    {
        //피격받았을 때 행동 정의
        
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isCollidingWithTarget)
        {
            ApplyHealthChange();
        }


        Vector3 direction = Vector3.zero;
        if (DistanceToTarget() < followRange)
        {
            direction = DirectionToTarget();
        }

        CallMoveEvent(direction);

        // LEE) 게임 진행하다보면 언젠가부터 Look rotation viewing vector is zero 로그가 찍히기 시작해서(렉심함) 임시방편으로 수정 함.
        // 원인은 아마 플레이어랑 멀어진 적들이 Look할 곳을 못찾아서 발생하는 것으로 추정
        // direction == Zero가 아닌 경우만 Rotate 실행하도록 함.
        //-------------------------원문(주석화)
        //Rotate(direction);
        //-------------------------수정
        if(direction != Vector3.zero)
        {
            Rotate(direction);
        }
        //-------------------------END
    }
    private void Rotate(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject receiver = other.gameObject;

        if (layerMaskTarget != (layerMaskTarget | (1 << other.gameObject.layer)))
        {
            return;
        }


        collidingTargetHealthSystem = receiver.GetComponent<HealthSystem>();
        if (collidingTargetHealthSystem != null)
        {
            isCollidingWithTarget = true;
        }

        collidingMovement = receiver.GetComponent<CharacterMovement>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        GameObject receiver = collision.gameObject;

        if (layerMaskTarget != (layerMaskTarget | (1 << collision.gameObject.layer)))
        {
            return;
        }


        collidingTargetHealthSystem = receiver.GetComponent<HealthSystem>();
        if (collidingTargetHealthSystem != null)
        {
            isCollidingWithTarget = true;
        }

        collidingMovement = receiver.GetComponent<CharacterMovement>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (layerMaskTarget != (layerMaskTarget | (1 << other.gameObject.layer)))
        {
            return;
        }

        isCollidingWithTarget = false;
    }

    private void OnCollisionExit(Collision other)
    {
        if (layerMaskTarget != (layerMaskTarget | (1 << other.gameObject.layer)))
        {
            return;
        }

        isCollidingWithTarget = false;
    }

    private void ApplyHealthChange()
    {
        AttackSO attackSO = stats.CurrentStat.attackSO;

        float finalDamage = Mathf.FloorToInt(Random.Range(attackSO.power * 0.8f, attackSO.power * 1.0f));

        bool hasBeenChanged = collidingTargetHealthSystem.ChangeHealth(-finalDamage);

        if(attackSO.is_on_knockback && collidingMovement != null)
        {
            collidingMovement.ApplyKnockback(transform, attackSO.knockback_power, attackSO.knockback_time);
        }
    }

    private void DropItemOnDeath()
    {
        int itemCount = Random.Range(1, 6);

        for (int i = 0; i < itemCount; i++)
        {
            GameObject droppedItem = ObjectPoolManager.Instance.GetObject(Data.Pool_Exp, Data.Pool_Item);
            droppedItem.transform.position = this.transform.position;
            Exp exp = droppedItem.GetComponent<Exp>();
            exp.OnDrop();
        }
    }
}
