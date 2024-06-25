using Cinemachine;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Input Action to Open Camera Mode")]
        [SerializeField] private InputActionReference cameraOpenActionRef;
        
        [Header("Cameras")]
        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        

        void OnEnable()
        {
            cameraOpenActionRef.action.performed += OnCameraOpen;
        }

        void OnDisable()
        {
            cameraOpenActionRef.action.performed -= OnCameraOpen;
        }

        void OnCameraOpen(InputAction.CallbackContext context)
        {
            SwitchCamera();
            AdjustUIVisibility();
        }

        void SwitchCamera()
        {
            if (thirdPersonCamera.Priority > firstPersonCamera.Priority)
            {
                thirdPersonCamera.Priority = 0;
                firstPersonCamera.Priority = 1;
                GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);

            }
            else
            {
                thirdPersonCamera.Priority = 1;
                firstPersonCamera.Priority = 0;
            }
        }
        
        public bool IsInCameraMode() => firstPersonCamera?.Priority > thirdPersonCamera?.Priority;

        void AdjustUIVisibility()
        {
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(!IsInCameraMode());
            GlobalEvents.OnSetHUDVisibilityEvent?.Invoke(!IsInCameraMode());
        }
    }
}

