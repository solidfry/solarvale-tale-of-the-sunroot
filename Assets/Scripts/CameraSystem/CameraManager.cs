using Cinemachine;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        public Canvas questUICanvas; // Reference to the Quest UI Canvas

        [SerializeField] private InputActionReference cameraOpenActionRef;


        private void Awake()
        {
            
        }

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
                GlobalEvents.OnSetHUDVisibilityEvent?.Invoke(false);
                GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);

            }
            else
            {
                thirdPersonCamera.Priority = 1;
                firstPersonCamera.Priority = 0;
                GlobalEvents.OnSetHUDVisibilityEvent?.Invoke(true);
                GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(true);
            }
        }
        
        public bool IsCameraOpen() => firstPersonCamera?.Priority > thirdPersonCamera?.Priority;

        void AdjustUIVisibility()
        {
            // Show/hide UI elements based on camera priority.
            // TODO: Refactor. This script should not need a reference to this UI element.
            questUICanvas.gameObject.SetActive(!IsCameraOpen()); // Quest UI Canvas
        }
    }
}

