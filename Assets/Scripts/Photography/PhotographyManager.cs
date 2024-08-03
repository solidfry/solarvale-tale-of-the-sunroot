using System.Collections;
using System.Collections.Generic;
using CameraSystem;
using Core;
using Entities;
using Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Photography
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

        [Header("Events")]
        [Space(10)]
        [SerializeField]  UnityEvent onCameraModeActivated;
        [SerializeField]  UnityEvent onCameraModeDeactivated;
        
            [Header("On Photo Taken Event Handling")]
        [SerializeField] UnityEvent onBeforePhotoTaken;
        [SerializeField] UnityEvent<EntityData[]> onPhotoTaken;

        [Header("RayCast Settings")]
        [SerializeField] private float rayCastDistance = 300f;
        [SerializeField] private float rayCastBoxSize = 0.5f;
        [SerializeField] LayerMask ignoreLayerMask;

        [SerializeField] private PhotographyHUDController photographyHUDController;
        
        [SerializeField] private bool IsInCameraMode;
        
        private Camera _mainCamera;
        private Texture2D _screenCapture;
        private Transform albumParent;
        
        List<EntityData> currentEntities = new();
        List<EntityData> previousEntities = new();

        private Collider[] collidersHit = new Collider[5];
        
        private RaycastHit[] results = new RaycastHit[5];

        
        private const int DesiredPhotoSize = 800; // Desired photo width and height


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
                onCameraModeActivated?.Invoke();
            }
            else
            {
                photographyHUDController.SetHUDVisibility(0, 0);
                IsInCameraMode = false;
                onCameraModeDeactivated?.Invoke();
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
                GlobalEvents.OnSetOnboardingVisibilityEvent?.Invoke(false);
            }
        }
        
        private IEnumerator CapturePhoto()
        {
            onBeforePhotoTaken?.Invoke();
            yield return new WaitForEndOfFrame();

            int screenMinDimensions = Mathf.Min(Screen.width, Screen.height);
            // Calculate the center position of the screen
            int photoSize = Mathf.Min(DesiredPhotoSize, screenMinDimensions);
            int centerX = Screen.width / 2;
            int centerY = Screen.height / 2;
            
            var floor = Mathf.FloorToInt(photoSize / 2f);
            
            // Calculate the region to read
            var regionToRead = new Rect(
                centerX - floor,
                centerY - floor,
                photoSize,
                photoSize
            );

            _screenCapture = new Texture2D(photoSize, photoSize, TextureFormat.RGBA32, false);
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
            photoFrame.SetActive(false);
            photoCanvas.gameObject.SetActive(false);
            photoDisplayArea.sprite = null;

            photographyHUDController.ToggleCanvas(true);
            photographyHUDController.SetHUDVisibility(true);
            HandleOnRemovePhotoGlobalEvents();
        }

        private void KeepPhoto()
        {
            if (_screenCapture != null)
            {
                if (currentEntities != null)
                {
                    PhotoData photoData = new PhotoData(_screenCapture, currentEntities);
                    GlobalEvents.OnPhotoKeptEvent?.Invoke(photoData);
                    currentEntities.Clear();
                }
                else
                {
                    PhotoData photoData = new PhotoData(_screenCapture);
                    GlobalEvents.OnPhotoKeptEvent?.Invoke(photoData);
                }

            }
            RemovePhoto();
        }

        private void DiscardPhoto()
        {
            RemovePhoto();
        }

        private Collider[] GetRayCastHit()
        {
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Create a ray from the center of the screen
            var size = Physics.BoxCastNonAlloc(ray.origin, Vector3.one * rayCastBoxSize, ray.direction, results, Quaternion.identity, rayCastDistance, ~ignoreLayerMask);
            if (results.Length > 0 && results[0].collider != null)
            {
                collidersHit = new Collider[5];
                for (int i = 0; i < size; i++)
                {
                    collidersHit[i] = results[i].collider;
                }
                
                // Debug.Log("Colliders hit: " + collidersHit.Length + " Size was " + size);
                return collidersHit;
            }
            else
            {
                return default; // Return a default RaycastHit if nothing was detected
            }
        }

        private void HandlePhotographyRayForQuests()
        {
            var hits = GetRayCastHit();
            if (hits != null && hits.Length > 0)
            {
                var entities = GetEntityDataFromRayCastHit(hits);
                onPhotoTaken?.Invoke(entities);
            }
            else
            {
                var entities = new[]
                {
                    EntityData.Empty
                };
                onPhotoTaken?.Invoke(entities);
            }
        }

        private EntityData[] GetEntityDataFromRayCastHit(Collider[] hits)
        {
            previousEntities = currentEntities;
            
            if (hits == null) return null;
            
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.TryGetComponent(out Entity entity))
                {
                    if (entity == null) continue;
                    currentEntities.Add(entity.GetEntityData);
                }
                
            }
            return currentEntities?.ToArray();
        }

        private static void HandleOnRemovePhotoGlobalEvents()
        {
            // GlobalEvents.OnLockCursorEvent?.Invoke(true);
            GlobalEvents.OnGameStateChangeEvent?.Invoke(GameState.Photography);
            GlobalEvents.OnPlayerChangeActionMapEvent.Invoke(false);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(true);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(true);
            GlobalEvents.OnSetOnboardingVisibilityEvent?.Invoke(true);
        }

        private static void HandleOnShowPhotoGlobalEvents()
        {
            // GlobalEvents.OnLockCursorEvent?.Invoke(false);
            GlobalEvents.OnGameStateChangeEvent?.Invoke(GameState.EditingPhoto);
            GlobalEvents.OnPlayerChangeActionMapEvent.Invoke(true);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(false);
            GlobalEvents.OnSetCanInteractEvent?.Invoke(false);
        }

        private void OnDrawGizmos()
        {
            if (!IsInCameraMode) return;
            
            var hit = GetRayCastHit();
                
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * rayCastDistance);
            
            if (hit == null) return;

            foreach (var collider in hit)
            {
                if (collider != null)
                {
                    Gizmos.DrawCube(collider.transform.position, Vector3.one * rayCastBoxSize);
                }
            }
        }
    }
}
