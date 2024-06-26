using System.Collections;
using Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Events;
using UnityEngine.Events;

namespace CameraSystem
{
    public class PhotographyManager : MonoBehaviour
    {
        [SerializeField] private CameraManager cameraManager;

        [Header("Photo UI")]
        [SerializeField] private Image photoDisplayArea;
        [SerializeField] private GameObject photoFrame;
        [SerializeField] private Canvas photoCanvas;
        [SerializeField] private Button keepButton;
        [SerializeField] private Button discardButton;

        [SerializeField] private InputActionReference takePhotoActionRef;
        
        [Header("On Photo Taken Event Handling")]
        [Space(10)]
        public UnityEvent<EntityData> onPhotoTaken;
        
        [Header("RayCast Settings")]
        [SerializeField] private float rayCastDistance = 300f;
        [SerializeField] private float rayCastBoxSize = 0.5f;

        [SerializeField] private PhotographyHUDController photographyHUDController;

        private Camera _mainCamera;
        private Texture2D _screenCapture;
        private bool _viewingPhoto;

        private void OnValidate()
        {
            cameraManager ??= GetComponent<CameraManager>();
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            cameraManager ??= GetComponent<CameraManager>();
        }

        private void OnEnable()
        {
            takePhotoActionRef.action.performed += OnTakePhoto;
            keepButton.onClick.AddListener(KeepPhoto);
            discardButton.onClick.AddListener(DiscardPhoto);

            photographyHUDController.OnEnable();
        }

        private void OnDisable()
        {
            takePhotoActionRef.action.performed -= OnTakePhoto;
            keepButton.onClick.RemoveListener(KeepPhoto);
            discardButton.onClick.RemoveListener(DiscardPhoto);

            photographyHUDController.OnDisable();
        }

        private void OnTakePhoto(InputAction.CallbackContext context)
        {
            if (IsInCameraMode)
            {
                StartCoroutine(CapturePhoto());
                photographyHUDController.SetHUDVisibility(0,0);
            }
        }

        private bool IsInCameraMode => cameraManager != null && cameraManager.IsInCameraMode;

        private IEnumerator CapturePhoto()
        {
            yield return new WaitForEndOfFrame();

            var regionToRead = new Rect(0, 0, Screen.width, Screen.height);
            _screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            _screenCapture.ReadPixels(regionToRead, 0, 0, false);
            _screenCapture.Apply();

            HandlePhotographyRayForQuests();
            ShowPhoto();
        }

        private void ShowPhoto()
        {
            if (_screenCapture == null) return;

            var photoSprite = Sprite.Create(_screenCapture, new Rect(0.0f, 0.0f, _screenCapture.width, _screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
            photoDisplayArea.sprite = photoSprite;

            photoFrame.SetActive(true);
            photoCanvas.gameObject.SetActive(true);

            HandleOnShowPhotoGlobalEvents();
        }

        private void RemovePhoto()
        {
            _viewingPhoto = false;
            photoFrame.SetActive(false);
            photoCanvas.gameObject.SetActive(false);
            photoDisplayArea.sprite = null;
            
            photographyHUDController.SetHUDVisibility(true);
            HandleOnRemovePhotoGlobalEvents();
        }

        public void KeepPhoto()
        {
            RemovePhoto();
        }

        public void DiscardPhoto()
        {
            RemovePhoto();
        }

        private RaycastHit GetRayCastHit(out Ray ray)
        {
            ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Physics.BoxCast(ray.origin, Vector3.one * rayCastBoxSize, ray.direction, out var hit, Quaternion.identity, rayCastDistance);
            return hit;
        }

        private void HandlePhotographyRayForQuests()
        {
            var hit = GetRayCastHit(out _);
            var entityData = GetEntityDataFromRayCastHit(hit);
            onPhotoTaken?.Invoke(entityData);
        }

        private EntityData GetEntityDataFromRayCastHit(RaycastHit hit)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out IEntity entity))
            {
                return entity?.GetEntityData;
            }
            return null;
        }

        private static void HandleOnRemovePhotoGlobalEvents()
        {
            GlobalEvents.OnLockCursorEvent?.Invoke(true);
            GlobalEvents.OnPlayerControlsLockedEvent.Invoke(false);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(true);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(true);
        }

        private static void HandleOnShowPhotoGlobalEvents()
        {
            GlobalEvents.OnLockCursorEvent?.Invoke(false);
            GlobalEvents.OnPlayerControlsLockedEvent.Invoke(true);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(false);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(false);
        }

        private void OnDrawGizmos()
        {
            if (IsInCameraMode)
            {
                var hit = GetRayCastHit(out var ray);
                
                Gizmos.color = Color.red;
                Gizmos.DrawRay(ray.origin, ray.direction * rayCastDistance);
                Gizmos.DrawCube(hit.point, Vector3.one * rayCastBoxSize);
            }
        }
    }
}
