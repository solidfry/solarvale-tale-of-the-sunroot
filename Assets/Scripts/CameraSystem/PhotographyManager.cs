using System.Collections;
using Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Events;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CameraSystem
{
    public class PhotographyManager : MonoBehaviour
    {
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
        [SerializeField] LayerMask ignoreLayerMask;

        [SerializeField] private PhotographyHUDController photographyHUDController;
        
        [SerializeField] private bool IsInCameraMode;
        [FormerlySerializedAs("_viewingPhoto")] [SerializeField] private bool viewingPhoto;
        
        private Camera _mainCamera;
        private Texture2D _screenCapture;

        // Desired photo size
        private const int PhotoWidth = 800; 
        private const int PhotoHeight = 800; 
        
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnCameraModeChanged(CameraMode mode)
        {
            if (mode == CameraMode.Photography)
            {
                photographyHUDController.SetHUDVisibility(1, 0);
                IsInCameraMode = true;
            }
            else
            {
                photographyHUDController.SetHUDVisibility(0, 0);
                IsInCameraMode = false;
            }
        }

        private void OnEnable()
        {
            takePhotoActionRef.action.performed += OnTakePhoto;
            keepButton.onClick.AddListener(KeepPhoto);
            discardButton.onClick.AddListener(DiscardPhoto);

            photographyHUDController.OnEnable();

            GlobalEvents.OnChangeCameraModeEvent += OnCameraModeChanged;
        }

        private void OnDisable()
        {
            takePhotoActionRef.action.performed -= OnTakePhoto;
            keepButton.onClick.RemoveListener(KeepPhoto);
            discardButton.onClick.RemoveListener(DiscardPhoto);

            photographyHUDController.OnDisable();
            GlobalEvents.OnChangeCameraModeEvent -= OnCameraModeChanged;
        }

        private void OnTakePhoto(InputAction.CallbackContext context)
        {
            if (IsInCameraMode)
            {
                StartCoroutine(CapturePhoto());
                photographyHUDController.SetHUDVisibility(0, 0);
            }
        }
        
        private IEnumerator CapturePhoto()
        {
            yield return new WaitForEndOfFrame();

            // Calculate the center position of the screen
            int centerX = Screen.width / 2;
            int centerY = Screen.height / 2;

            // Calculate the region to read
            var regionToRead = new Rect(
                centerX - PhotoWidth / 2,
                centerY - PhotoHeight / 2,
                PhotoWidth,
                PhotoHeight
            );

            _screenCapture = new Texture2D(PhotoWidth, PhotoHeight, TextureFormat.RGBA32, false);
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

            photographyHUDController.ToggleCanvas(false);
            HandleOnShowPhotoGlobalEvents();
        }

        private void RemovePhoto()
        {
            viewingPhoto = false;
            photoFrame.SetActive(false);
            photoCanvas.gameObject.SetActive(false);
            photoDisplayArea.sprite = null;

            photographyHUDController.ToggleCanvas(true);
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

        private RaycastHit GetRayCastHit()
        {
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Create a ray from the center of the screen
            RaycastHit hit;
            bool hitSomething = Physics.BoxCast(ray.origin, Vector3.one * rayCastBoxSize, ray.direction, out hit, Quaternion.identity, rayCastDistance,  ~ignoreLayerMask);

            if (hitSomething)
            {
                return hit; // Return the hit if something was detected
            }
            else
            {
                return default; // Return a default RaycastHit if nothing was detected
            }
        }

        private void HandlePhotographyRayForQuests()
        {
            var hit = GetRayCastHit();
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
            GlobalEvents.OnPlayerChangeActionMapEvent.Invoke(false);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(true);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(true);
        }

        private static void HandleOnShowPhotoGlobalEvents()
        {
            GlobalEvents.OnLockCursorEvent?.Invoke(false);
            GlobalEvents.OnPlayerChangeActionMapEvent.Invoke(true);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(false);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(false);
        }

        private void OnDrawGizmos()
        {
            if (IsInCameraMode)
            {
                var hit = GetRayCastHit();

                Gizmos.color = Color.red;
                if (hit.collider != null)
                {
                    Gizmos.DrawRay(hit.point, hit.normal * rayCastDistance);
                    Gizmos.DrawCube(hit.point, Vector3.one * rayCastBoxSize);
                }
                else
                {
                    Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    Gizmos.DrawRay(ray.origin, ray.direction * rayCastDistance);
                }
            }
        }
    }
}
