using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;

    [Header("Hide all UI")]
    [SerializeField] private GameObject[] hideUI;
    [SerializeField] private GameObject[] takePhotoUI;

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
    }

    void OnDisable()
    {
        takePhotoAction.performed -= OnTakePhoto;
        takePhotoAction.Disable();
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

        // Hide UI elements
        foreach (var ui in hideUI)
        {
            ui.SetActive(false);
        }

        viewingPhoto = true;

        yield return new WaitForEndOfFrame();

        Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);

        // Use RGBA32 format for screen capture
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply();

        // Display the photo
        ShowPhoto();

        yield return new WaitForSeconds(1f); // Cooldown period

        // Remove the photo and reset UI elements
        RemovePhoto();

        // Show UI elements again
        foreach (var ui in hideUI)
        {
            ui.SetActive(true);
        }
    }

    void ShowPhoto()
    {
        if (screenCapture != null)
        {
            Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
            photoDisplayArea.sprite = photoSprite;

            // Show the photo frame
            photoFrame.SetActive(true);
        }
        else
        {
            Debug.LogError("Screen capture texture is null");
        }
    }

    void RemovePhoto()
    {
        // Hide take photo UI elements
        foreach (var ui in takePhotoUI)
        {
            ui.SetActive(false);
        }

        viewingPhoto = false;

        // Hide the photo frame
        photoFrame.SetActive(false);

        // Clear the photo display area
        photoDisplayArea.sprite = null;
    }
}

