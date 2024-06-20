using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
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
        if (IsThirdPersonCameraHigherPriority())
        {
            StartCoroutine(CapturePhoto());
        }
    }

    IEnumerator CapturePhoto()
    {
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
    }

    void ShowPhoto()
    {
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

        // Show the photo frame
        photoFrame.SetActive(true);
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
    }

    public bool IsThirdPersonCameraHigherPriority()
    {
        return thirdPersonCamera.Priority > firstPersonCamera.Priority;
    }
}
