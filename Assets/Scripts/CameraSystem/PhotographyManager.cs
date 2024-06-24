using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraSystem
{
    public class PhotographyManager : MonoBehaviour
    {
        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        public InputActionAsset inputActionAsset;
        public Canvas questUICanvas; // Reference to the Quest UI Canvas

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

            // Show/hide UI elements based on camera priority
            questUICanvas.gameObject.SetActive(!isFirstPersonActive); // Quest UI Canvas
        }
    }
}

