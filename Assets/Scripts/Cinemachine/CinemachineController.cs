using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCamera;
    public GameObject Camera;
    CinemachineTrackedDolly dolly;
    private void Awake()
    {
        VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        dolly = VirtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        Data.IsCinemachineWorking = true;
    }

    private void FixedUpdate()
    {
        dolly.m_PathPosition += 0.005f;
        if(dolly.m_PathPosition > 1.5f)
        {
            this.gameObject.SetActive(false);
            Camera.SetActive(false);
            Data.IsCinemachineWorking = false;
        }


    }
}
