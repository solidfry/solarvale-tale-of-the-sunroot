using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CameraMode currentCameraMode;
        
        [Header("Input Action to Open Camera Mode")]
        [SerializeField] private InputActionReference cameraOpenActionRef;
        
        [SerializeField] List<CameraType> cameras;

        Observable<CameraMode> CurrentCameraMode { get; set; }
        
        private void Awake()
        {
            CurrentCameraMode = new Observable<CameraMode>(CameraMode.Exploration);
            CurrentCameraMode.ValueChanged += SendCameraMode;
        }

        private void SendCameraMode(CameraMode mode) => GlobalEvents.OnChangeCameraModeEvent?.Invoke(mode);

        private void SetCameraMode(CameraMode mode)
        {
            CurrentCameraMode.Value = mode;
            currentCameraMode = CurrentCameraMode;
            UpdateCamera(CurrentCameraMode.Value);
        }
        
        private void UpdateCamera(CameraMode mode) => cameras.ForEach(c => c.SetPriority(c.Mode == mode ? 1 : 0));

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
            SetCameraMode(IsInCameraMode ? CameraMode.Exploration : CameraMode.Photography);
            if (IsInCameraMode) 
                GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);
        }

        public bool IsInCameraMode => CurrentCameraMode == CameraMode.Photography;

        private void AdjustUIVisibility()
        {
            Debug.Log(IsInCameraMode ? CameraMode.Photography : CameraMode.Exploration);
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(!IsInCameraMode);
            GlobalEvents.OnSetHUDVisibilityEvent?.Invoke(!IsInCameraMode);
            GlobalEvents.OnSetCameraHUDVisibilityEvent?.Invoke(IsInCameraMode);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(!IsInCameraMode);
        }
        
        
    }
}