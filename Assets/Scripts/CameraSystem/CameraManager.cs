using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Events;

namespace CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Photo Taker")]
        [SerializeField] private Image photoDisplayArea;
        [SerializeField] private GameObject photoFrame;
        [SerializeField] private Canvas photoCanvas; // Reference to the photo canvas
        [SerializeField] private Button keepButton; // Reference to the keep button
        [SerializeField] private Button discardButton; // Reference to the discard button

        [Header("Hide all UI")]
        [SerializeField] private GameObject hideUIParent; // Parent GameObject to hide all UI elements

        private Texture2D screenCapture;
        private bool viewingPhoto;

        public InputActionAsset inputActionAsset; // Reference to the Input Action Asset
        private InputAction takePhotoAction;

        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        void Awake()
        {
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            takePhotoAction = playerActionMap.FindAction("TakePhoto");
        }

        void OnEnable()
        {
            takePhotoAction.Enable();
            takePhotoAction.performed += OnTakePhoto;
            keepButton.onClick.AddListener(KeepPhoto);
            discardButton.onClick.AddListener(DiscardPhoto);

        }

        void OnDisable()
        {
            takePhotoAction.performed -= OnTakePhoto;
            takePhotoAction.Disable();
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
            bool isFirstHigher = firstPersonCamera.Priority > thirdPersonCamera.Priority;
            Debug.Log($"IsFirstPersonCameraHigherPriority: {isFirstHigher}");
            return isFirstHigher;
        }

        IEnumerator CapturePhoto()
        {
            Debug.Log("CapturePhoto: Starting photo capture coroutine");

            // Hide all UI elements
            hideUIParent.SetActive(false);

            viewingPhoto = true;

            yield return new WaitForEndOfFrame();

            Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);

            // Use RGBA32 format for screen capture
            screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            screenCapture.ReadPixels(regionToRead, 0, 0, false);
            screenCapture.Apply();

            // Display the photo
            ShowPhoto();
        }

        void ShowPhoto()
        {
            if (screenCapture != null)
            {
                Debug.Log("ShowPhoto: Displaying captured photo");
                Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
                photoDisplayArea.sprite = photoSprite;

                // Show the photo frame
                photoFrame.SetActive(true);

                // Show the canvas
                photoCanvas.gameObject.SetActive(true);

                GlobalEvents.OnLockCursorEvent?.Invoke(false);
                GlobalEvents.OnPlayerControlsLockedEvent.Invoke(true);
                GlobalEvents.OnSetCursorInputForLookEvent.Invoke(false);
            }
            else
            {
                Debug.LogError("ShowPhoto: Screen capture texture is null");
            }
        }

        void RemovePhoto()
        {
            viewingPhoto = false;

            // Hide the photo frame
            photoFrame.SetActive(false);

            // Hide the canvas
            photoCanvas.gameObject.SetActive(false);

            // Clear the photo display area
            photoDisplayArea.sprite = null;

            // Show all UI elements again
            hideUIParent.SetActive(true);

            GlobalEvents.OnLockCursorEvent?.Invoke(true);
            GlobalEvents.OnPlayerControlsLockedEvent.Invoke(false);
            GlobalEvents.OnSetCursorInputForLookEvent.Invoke(true);
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
    }
}






