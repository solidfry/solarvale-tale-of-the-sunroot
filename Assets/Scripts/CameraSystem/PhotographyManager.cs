using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PhotographyManager : MonoBehaviour
{
    public CinemachineVirtualCamera thirdPersonCamera;
    public CinemachineVirtualCamera firstPersonCamera;
    public InputActionAsset inputActionAsset; // Reference to the Input Action Asset


    private InputAction cameraOpenAction;

    void Awake()
    {
        // Find the action map and the specific action
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
        // Check the current priority and switch
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
