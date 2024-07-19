using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Debug = UnityEngine.Debug;

namespace CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CameraMode currentCameraMode;
        
        [SerializeField] private CinemachineBrain brain;
        
        [Header("Input Action to Open Camera Mode")]
        [SerializeField] private InputActionReference cameraOpenActionRef;
        
        [SerializeField] List<CameraType> cameras;

        Observable<CameraMode> CurrentCameraMode { get; set; }
        
        private void Awake()
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
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
        
        private void UpdateCamera(CameraMode mode) =>
            cameras.ForEach(c =>
            {
                c.SetPriority(c.Mode == mode ? 1 : 0);
                c.GameObject.SetActive(c.Mode == mode);
            });

        private void Start()
        {
            if (brain is not null)
            {
                brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
            }
            AdjustUIVisibility();
        }

        private void OnCameraActivated(ICinemachineCamera fromCam, ICinemachineCamera toCam)
        {
            // GlobalEvents.OnHidePlayerModelEvent?.Invoke(CurrentCameraMode.Value == CameraMode.Photography);
            if (CurrentCameraMode.Value == CameraMode.Photography)
                StartCoroutine(DelayedHidePlayerModel());
            else GlobalEvents.OnHidePlayerModelEvent?.Invoke(false);
        }
        
        IEnumerator DelayedHidePlayerModel()
        {
            yield return new WaitForSeconds(brain.ActiveBlend.Duration - 0.2f);
            GlobalEvents.OnHidePlayerModelEvent?.Invoke(CurrentCameraMode.Value == CameraMode.Photography);
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