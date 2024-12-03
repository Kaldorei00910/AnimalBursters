using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    private BaseController controller;
    private Rigidbody movementrigidBody;
    private CharacterStatHandler characterStatHandler;

    private Vector3 movementDirection = Vector3.zero;
    private Vector3 knockback = Vector3.zero;
    private float knockbackDuration = 0.0f;

    private void Awake()
    {
        controller = GetComponent<BaseController>();
        movementrigidBody = GetComponent<Rigidbody>();
        characterStatHandler = GetComponent<CharacterStatHandler>();
    }

    private void Start()
    {
        controller.OnMoveEvent += Move;
        controller.OnLookEvent += Look;
    }

    private void Move(Vector3 direction)
    {
        movementDirection = direction;
    }

    private void Look(Vector3 direction)
    {
        if (movementDirection != Vector3.zero || Data.IsWheelyXControlling)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void FixedUpdate()
    {
        ApplyMovement(movementDirection);
        if (knockbackDuration > 0.0f)
        {
            knockbackDuration -= Time.fixedDeltaTime;
        }
    }

    public void ApplyKnockback(Transform other, float power, float duration)
    {
        knockbackDuration = duration;
        knockback = -(other.position - transform.position).normalized * power;
    }

    private void ApplyMovement(Vector3 direction)
    {
        direction = direction * characterStatHandler.CurrentStat.attackSO.speed;

        Vector3 currentVelocity = movementrigidBody.velocity;
        direction.y = Mathf.Min(currentVelocity.y, 0.05f);

        if (knockbackDuration > 0.0f) 
        {
            direction += knockback;
        }
        movementrigidBody.velocity = direction;
        if (transform.position.y < -1f)
            GoStartPos();
    }
    public void GoStartPos()
    {
        if(TryGetComponent(out PlayerInputController _)){
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            transform.position = Vector3.zero;
        }
    }
}
