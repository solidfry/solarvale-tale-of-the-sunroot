using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using System;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<CinemachineVirtualCamera> cameras;

    public void OnEnable()
    {
        GlobalEvents.OnSetPriorityOfCameraEvent += SetCameraPriority;
    }

    private void SetCameraPriority(CinemachineVirtualCamera obj)
    {
        foreach (var cam in cameras)
        {
            if (obj != cam)
            {
                cam.Priority = 0;
            }
            else
            {
                cam.Priority = 1;
            }
        }
    }

    public void OnDisable()
    {
        GlobalEvents.OnSetPriorityOfCameraEvent -= SetCameraPriority;
    }
}
