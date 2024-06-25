using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraSystem
{
    public class PhotographyManager : MonoBehaviour
    {
        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        public Canvas questUICanvas; // Reference to the Quest UI Canvas

        [SerializeField] private InputActionReference cameraOpenActionRef;

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
            }
            else
            {
                thirdPersonCamera.Priority = 1;
                firstPersonCamera.Priority = 0;
            }
        }

        void AdjustUIVisibility()
        {
            bool isFirstPersonActive = firstPersonCamera.Priority > thirdPersonCamera.Priority;

            // Show/hide UI elements based on camera priority.
            // TODO: Refactor. This script should not need a reference to this UI element.
            questUICanvas.gameObject.SetActive(!isFirstPersonActive); // Quest UI Canvas
        }
    }
}

