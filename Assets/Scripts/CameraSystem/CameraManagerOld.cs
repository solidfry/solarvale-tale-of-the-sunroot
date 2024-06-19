using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CameraManagerOld : MonoBehaviour
{
    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;

    [Header("Hide all UI")]
    [SerializeField] private GameObject[] hideUI;
    [SerializeField] private GameObject[] takePhotoUI;

    //Required the inventory system to place photos taken to the album
    //[SerializeField] private GameObject albumSlots;

    private Texture2D screenCapture;
    private bool viewingPhoto;

    private bool isFirstPerson = false;

    public Camera mainCamera;
    public Transform firstPersonCameraPosition;
    public Transform thirdPersonCameraPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Debug.Log("First person camera found" + mainCamera.name);
        }
        else
        {
            Debug.Log("First person camera not found!");
        }
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        // ADD Check if player is in camera mode, if so then enable camera functions other wise remove the camera mode
        // Hide all unncessary UI
        // When player press E take the photo else remove the photo
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchToFirstPerson();

        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
            else
            {
                RemovePhoto();
            }
        }
    }

    IEnumerator CapturePhoto()
    {
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
        ShowPhoto();

        yield return new WaitForSeconds(1f); // Cooldown period
    }

    void ShowPhoto()
    {
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

        // Start other effects/animations
        photoFrame.SetActive(true);
    }

    void RemovePhoto()
    {
        for (int i = 0; i < takePhotoUI.Length; i++)
        {
            takePhotoUI[i].SetActive(false);
        }
        viewingPhoto = false;
        photoFrame.SetActive(false);
    }

    void SwitchToThirdPerson()
    {
        mainCamera.transform.position = thirdPersonCameraPosition.position;
        mainCamera.transform.rotation = thirdPersonCameraPosition.rotation;
        isFirstPerson = false;
    }

    void SwitchToFirstPerson()
    {
        mainCamera.transform.position = firstPersonCameraPosition.position;
        mainCamera.transform.rotation = firstPersonCameraPosition.rotation;
        isFirstPerson = true;
    }
}
