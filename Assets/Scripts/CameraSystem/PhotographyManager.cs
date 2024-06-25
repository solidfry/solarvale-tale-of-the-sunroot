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
        [SerializeField] CameraManager cameraManager;
        
        [Header("Photo UI")]
        [SerializeField] private Image photoDisplayArea;
        [SerializeField] private GameObject photoFrame;
        [SerializeField] private Canvas photoCanvas; // Reference to the photo canvas
        [SerializeField] private Button keepButton; // Reference to the keep button
        [SerializeField] private Button discardButton; // Reference to the discard button

        private Texture2D _screenCapture;
        private bool _viewingPhoto;
        
        [SerializeField] private InputActionReference takePhotoActionRef;
        
        [Header("On Photo Taken Event Handling")]
        [Space(10)]
        public UnityEvent<EntityData> onPhotoTaken;
        
        [Header("RayCast Settings")]
        [SerializeField] float rayCastDistance = 300f;
        [SerializeField] float rayCastBoxSize = 0.5f;
        Camera _mainCamera;
        

        private void OnValidate()
        {
            if (cameraManager is null)
                cameraManager = GetComponent<CameraManager>();
        }

        void Awake()
        {
            _mainCamera = Camera.main;
            cameraManager = GetComponent<CameraManager>();
        }

        void OnEnable()
        {
            takePhotoActionRef.action.performed += OnTakePhoto;
            keepButton.onClick.AddListener(KeepPhoto);
            discardButton.onClick.AddListener(DiscardPhoto);
        }

        void OnDisable()
        {
            takePhotoActionRef.action.performed -= OnTakePhoto;
            keepButton.onClick.RemoveListener(KeepPhoto);
            discardButton.onClick.RemoveListener(DiscardPhoto);
        }

        void OnTakePhoto(InputAction.CallbackContext context)
        {
            Debug.Log("OnTakePhoto: Photo action performed");
            if (IsFirstPersonCameraHigherPriority())
            {
                StartCoroutine(CapturePhoto());
            }
            else
            {
                Debug.Log("OnTakePhoto: First person camera does not have higher priority");
            }
        }

        public bool IsFirstPersonCameraHigherPriority()
        {
            if (cameraManager is null)
            {
                Debug.LogError("IsFirstPersonCameraHigherPriority: Camera manager is null");
                cameraManager = GetComponent<CameraManager>();
            }
            return cameraManager.IsInCameraMode();
        }

        IEnumerator CapturePhoto()
        {
            Debug.Log("CapturePhoto: Starting photo capture coroutine");

            // Hide all UI elements
            _viewingPhoto = true;

            yield return new WaitForEndOfFrame();

            Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);

            // Use RGBA32 format for screen capture
            _screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            _screenCapture.ReadPixels(regionToRead, 0, 0, false);
            _screenCapture.Apply();

            // the ray should fire here which will then pass the data to the unity event to invoke
            HandlePhotographyRayForQuests();

            // Display the photo
            ShowPhoto();
        }

        void ShowPhoto()
        {
            if (_screenCapture == null) return;
            
            // Debug.Log("ShowPhoto: Displaying captured photo");
            Sprite photoSprite = Sprite.Create(_screenCapture, new Rect(0.0f, 0.0f, _screenCapture.width, _screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
            photoDisplayArea.sprite = photoSprite;

            // Show the photo frame
            photoFrame.SetActive(true);

            // Show the canvas
            photoCanvas.gameObject.SetActive(true);

            HandleOnShowPhotoGlobalEvents();
        }

        void RemovePhoto()
        {
            _viewingPhoto = false;

            // Hide the photo frame
            photoFrame.SetActive(false);

            // Hide the canvas
            photoCanvas.gameObject.SetActive(false);

            // Clear the photo display area
            photoDisplayArea.sprite = null;

            HandleOnRemovePhoto();
        }
        
        public void KeepPhoto()
        {
            // Implement logic to keep the photo, if needed
            Debug.Log("KeepPhoto: Photo kept");
            RemovePhoto();
        }

        public void DiscardPhoto()
        {
            Debug.Log("DiscardPhoto: Photo discarded");
            RemovePhoto();
        }
        
        RaycastHit GetRayCastHit(out Ray ray)
        {
            ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Physics.BoxCast(ray.origin, Vector3.one * rayCastBoxSize, ray.direction, out var hit, Quaternion.identity, rayCastDistance);
            return hit;
        }
        
        private void HandlePhotographyRayForQuests()
        {
            RaycastHit hit = GetRayCastHit(out Ray ray);
            EntityData e = GetEntityDataFromRayCastHit(hit);
            onPhotoTaken?.Invoke(e);
        }

        EntityData GetEntityDataFromRayCastHit(RaycastHit hit)
        {
            if (hit.collider is not null && hit.collider.TryGetComponent(out IEntity e))
            {
                return e?.GetEntityData;
            }
            return null;
        }
        
        private static void HandleOnRemovePhoto()
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
            if (IsFirstPersonCameraHigherPriority())
            {
                RaycastHit hit = GetRayCastHit(out Ray ray);
                
                Gizmos.color = Color.red; 
                // I want to draw a box cast gizmo here
                Gizmos.DrawRay(ray.origin,  ray.direction * rayCastDistance);  
                Gizmos.DrawCube(hit.point, Vector3.one * rayCastBoxSize);
            }
        }
    }
}






