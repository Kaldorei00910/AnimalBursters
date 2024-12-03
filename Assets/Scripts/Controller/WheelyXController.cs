using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class WheelyXController : MonoBehaviour
{
    Vector3 moveDirection = Vector3.zero;
    public double RightWheelSpeed = 0;
    public double LeftWheelSpeed = 0;

    public float rotationFactor = 1f; // 회전 정도 조절
    public float moveSpeedFactor = 1f; // 이동 속도 조절
    public float Speedthreshold = 1f; // 최소 굴림 속도, 해당 굴림 속도를 넘어야 플레이어가 이동함
    private BluetoothService _service;

    private BaseController controller;

    private CharacterStatHandler characterStatHandler;
    private void Start()
    {
        controller = GetComponent<BaseController>();
        _service = BluetoothService.Instance;
        characterStatHandler = GetComponent<CharacterStatHandler>();
        if (Data.IsWheelyXControlling)
        {
            _service.OnLeftWheelyxChanged += OnLeftWheelValueChanged;
            _service.OnRightWheelyxChanged += OnRightWheelValueChanged;
        }
        ReplaceFactors();
        CommandMethods.OnPlayerDataChanged -= ReplaceFactors;
        CommandMethods.OnPlayerDataChanged += ReplaceFactors;
    }

    public void ReplaceFactors()
    {
        RangedAttackSO rangedAttackSO = characterStatHandler.CurrentStat.attackSO as RangedAttackSO; // 전역으로 빼도 될 것 같아 보임

        rotationFactor = rangedAttackSO.rotation_factor;
        Speedthreshold = rangedAttackSO.speed_threshold;
    }
    private void OnDestroy()
    {
        if (Data.IsWheelyXControlling && _service != null)
        {
            _service.OnLeftWheelyxChanged -= OnLeftWheelValueChanged;
            _service.OnRightWheelyxChanged -= OnRightWheelValueChanged;
        }
    }

    private void OnRightWheelValueChanged(object sender, EventArgs e)
    {
        RightWheelSpeed = _service.RightWheelyx.RealSpeed;
        WheelchairMovement((float)LeftWheelSpeed, (float)RightWheelSpeed);
    }

    private void OnLeftWheelValueChanged(object sender, EventArgs e)
    {
        LeftWheelSpeed = _service.LeftWheelyx.RealSpeed;
        WheelchairMovement((float)LeftWheelSpeed, (float)RightWheelSpeed);
    }

    private void Update()
    {
        if (Data.IsWheelyXControlling)
        {
            WheelchairMovement((float)LeftWheelSpeed, (float)RightWheelSpeed);
        }
    }

    public void WheelchairMovement(float rightWheelSpeed, float leftWheelSpeed)
    {
        if (Data.IsCinemachineWorking)
        {
            return;
        }
        Vector3 lookDirection = Vector3.zero;
        Vector3 worldMoveDirection = Vector3.zero;
        // 평균 속도를 이용하여 전진 속도 계산
        float forwardSpeed = (leftWheelSpeed + rightWheelSpeed) / 2f;

        // 바퀴 속도 차이에 따른 회전 속도 계산
        float turnSpeed = (rightWheelSpeed - leftWheelSpeed) * rotationFactor;

        // 이동 벡터 계산
        moveDirection.z = forwardSpeed;

        if (Mathf.Abs(forwardSpeed) < Speedthreshold)//속도가 임계값보다 느릴 때
        {
            moveDirection.z = Mathf.Lerp(0, 1, forwardSpeed / Speedthreshold);
            //0~1사이의 벡터로 속도의 비만큼 선형적으로 변화하도록 함
            worldMoveDirection = transform.TransformDirection(moveDirection);
        }
        else // 속도가 임계값 이상일 때
        {
            worldMoveDirection = transform.TransformDirection(moveDirection).normalized;
            // normalize하여 크기가 1인 벡터로 고정
            //해당 값에 속도(상수)를 곱하기에 고정 속도로 이동하게 됨
        }

        // CallMoveEvent로 이동 벡터 전달
        controller.CallMoveEvent(worldMoveDirection);

        // 회전 처리: 회전 속도가 있을 때만 CallLookEvent 호출
        if (Mathf.Abs(turnSpeed) > Mathf.Epsilon)
        {
            // 휠체어가 회전할 방향 계산
            float rotationAngle = turnSpeed * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
            lookDirection = rotation * transform.forward;

            // CallLookEvent로 회전 방향 전달
            controller.CallLookEvent(lookDirection);
        }
    }
}
