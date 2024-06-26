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
        [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
        [SerializeField] private CinemachineVirtualCamera firstPersonCamera;
        [SerializeField] private CinemachineVirtualCamera conversationCamera;

        private void Start()
        {
            AdjustUIVisibility();
        }

        private void OnEnable()
        {
            cameraOpenActionRef.action.performed += OnCameraOpen;
        }

        private void OnDisable()
        {
            cameraOpenActionRef.action.performed -= OnCameraOpen;
        }

        private void OnCameraOpen(InputAction.CallbackContext context)
        {
            SwitchCamera();
            AdjustUIVisibility();
        }

        private void SwitchCamera()
        {
            if (IsInCameraMode)
            {
                thirdPersonCamera.Priority = 1;
                firstPersonCamera.Priority = 0;
            }
            else
            {
                thirdPersonCamera.Priority = 0;
                firstPersonCamera.Priority = 1;
                GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);
            }
        }

        public bool IsInCameraMode => firstPersonCamera.Priority > thirdPersonCamera.Priority;

        private void AdjustUIVisibility()
        {
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(!IsInCameraMode);
            GlobalEvents.OnSetHUDVisibilityEvent?.Invoke(!IsInCameraMode);
            GlobalEvents.OnSetCameraHUDVisibilityEvent?.Invoke(IsInCameraMode);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(!IsInCameraMode);
        }
    }
}