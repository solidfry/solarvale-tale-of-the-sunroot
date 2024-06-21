using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PhotographyManager : MonoBehaviour
{
    public CinemachineVirtualCamera thirdPersonCamera;
    public CinemachineVirtualCamera firstPersonCamera;
    public InputActionAsset inputActionAsset; 


    private InputAction cameraOpenAction;

    void Awake()
    {
        var playerActionMap = inputActionAsset.FindActionMap("Player");
        cameraOpenAction = playerActionMap.FindAction("CameraOpen");
    }

    void OnEnable()
    {
        cameraOpenAction.Enable();
        cameraOpenAction.performed += OnCameraOpen;
    }

    void OnDisable()
    {
        cameraOpenAction.performed -= OnCameraOpen;
        cameraOpenAction.Disable();
    }

    void OnCameraOpen(InputAction.CallbackContext context)
    {
        SwitchCamera();
    }

    void SwitchCamera()
    {
        if (thirdPersonCamera.Priority > firstPersonCamera.Priority)
        {
            thirdPersonCamera.Priority = 0;
            firstPersonCamera.Priority = 1;
        }
        else
        {
            thirdPersonCamera.Priority = 1;
            firstPersonCamera.Priority = 0;
        }
    }


}
