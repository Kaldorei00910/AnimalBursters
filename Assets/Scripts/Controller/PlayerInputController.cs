using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : BaseController
{
    // 계속 경고 뜨는데 해결이 안되어서 가림
#pragma warning disable CS0108 // 멤버가 상속된 멤버를 숨깁니다. new 키워드가 없습니다.
    private Camera camera;
#pragma warning restore CS0108 // 멤버가 상속된 멤버를 숨깁니다. new 키워드가 없습니다.
    public Joystick JoystickController;
    Vector3 moveInput;

    [SerializeField]
    private Collider[] colliderArr;
    [SerializeField]
    private Collider targetCol;
    [SerializeField]
    private RangedAttackSO rangedAttackSO;
    private ShootingController shootingController;

    private Vector3 targetDirection = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        shootingController = GetComponent<ShootingController>();
        camera = Camera.main;
        Data.PlayerTransform = transform;
        IsAttacking = true;
    }
    private void Start()
    {
        StartCoroutine(FindClosestTargetCoroutine(rangedAttackSO));

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector3>().normalized;
        //moveInput = new Vector3 (moveInput.x, 0f, moveInput.y);
        CallMoveEvent(moveInput);
        CallLookEvent(moveInput);
    }

    private void FixedUpdate()
    {
        if(Data.IsWheelyXControlling || Data.IsCinemachineWorking)
        {
            return;
        }

        if (JoystickController.isDragging)
        {
            CallMoveEvent(transform.TransformDirection(JoystickController.DirectionVec3.z * Vector3.forward).normalized * JoystickController.DirectionVec3.magnitude);
            Quaternion rotation = Quaternion.Euler(0, JoystickController.DirectionVec3.x, 0);
            CallLookEvent(rotation * transform.forward);

            //변경 전 이동, 회전 값
            //CallMoveEvent(JoystickController.DirectionVec3);
            //CallLookEvent(JoystickController.DirectionVec3);
        }
        else 
        {
            CallMoveEvent(moveInput);
        }
    }

    private IEnumerator FindClosestTargetCoroutine(RangedAttackSO rangedAttackSO)
    {
        while (true)
        {
            colliderArr = null;
            colliderArr = Physics.OverlapSphere(transform.position, rangedAttackSO.attack_range, rangedAttackSO.target);

            if (colliderArr.Length == 0 || (targetCol != null && !targetCol.gameObject.activeSelf))
            {
                targetCol = null;
            }

            float shortestDistanceSqr = rangedAttackSO.attack_range * rangedAttackSO.attack_range;

            foreach (Collider col in colliderArr)
            {
                float distanceSqr = (transform.position - col.transform.position).sqrMagnitude;

                if (distanceSqr < shortestDistanceSqr)
                {
                    shortestDistanceSqr = distanceSqr;
                    targetCol = col;
                }
            }
            shootingController.targetCol = targetCol;

            yield return Data.WaitForSeconds(rangedAttackSO.check_find_delay);
        }
    }
}
