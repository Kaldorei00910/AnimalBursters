using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : AnimationController
{
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsHit = Animator.StringToHash("IsHit");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private readonly float magnituteThreshold = 0.5f;//해당 값 이상일 때에만 애니메이션 작동

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        controller.OnAttackEvent += Attacking;
        controller.OnMoveEvent += Move;
        healthSystem.OnDamage += Hit;
        healthSystem.OnInvincibilityEnd += InvincibilityEnd;
    }

    private void Move(Vector3 obj)
    {
        animator.SetBool(IsWalking, obj.magnitude > magnituteThreshold);
    }

    private void Attacking(AttackSO obj)
    {
        //animator.SetTrigger(Attack);
    }

    private void Hit()
    {
        //animator.SetBool(IsHit, true);
        animator.SetTrigger(IsHit);
    }

    private void InvincibilityEnd()
    {
        //animator.SetBool(IsHit, false);
    }
}
