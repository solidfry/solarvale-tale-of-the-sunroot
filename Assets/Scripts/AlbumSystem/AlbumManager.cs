using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlbumManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUserInterface;
    [SerializeField] private GameObject submitButton;

    private bool _isPaused;
    public AlbumSlots[] albumSlot;
    public static int lupineAmount;

    public static bool turnInventoryOn;
    public static bool submitButtonOn;

    [Header("Materials")]
    [SerializeField] private GameObject buttonMaterialsTab;

    [Header("Album")]
    [SerializeField] private GameObject buttonAlbumTab;

    [Header("Audio")]
    [SerializeField] private AudioSource audioBagOpen;

    [Header("HideUI")]
    [SerializeField] private GameObject[] hideUI;
    [SerializeField] private GameObject inGameMenuUI;
    private void Start()
    {
        inventoryUserInterface.SetActive(false);
        submitButton.SetActive(false);
        buttonMaterialsTab.SetActive(true);
        buttonAlbumTab.SetActive(false);
    }

    private void Update()
    {
        //Check if Ui is enabled 
        if(inGameMenuUI.activeSelf == true)
        {
            for (int i = 0; i < hideUI.Length; i++)
            {
                hideUI[i].SetActive(!hideUI[i].activeSelf);
            }
            audioBagOpen.Play();
            if (turnInventoryOn)
            {
                submitButton.SetActive(true);
            }
            inventoryUserInterface.SetActive(!inventoryUserInterface.activeSelf);

            //Freeze screen
            _isPaused = !_isPaused; // Toggle the isPaused flag

            Time.timeScale = _isPaused ? 0f : 1f; // If isPaused is true, set timeScale to 0, otherwise set it to 1
            Debug.Log(_isPaused ? "Game paused" : "Game resumed");
        }
    }

    public int AddItem(string nameOfInteract, int quantity, Sprite itemSprite, string itemDescription)
    {
        Debug.Log("item Name = " + nameOfInteract + "quantity = " + quantity);

        for (int i = 0; i < albumSlot.Length; i++)
        {
            Debug.Log(albumSlot[i].isFull);
            if (albumSlot[i].isFull == false && albumSlot[i].nameOfInteract == nameOfInteract || albumSlot[i].quantity == 0)
            {
                int leftOverItems = albumSlot[i].AddItem(nameOfInteract, quantity, itemSprite, itemDescription);
                if (leftOverItems > 0)

                    leftOverItems = AddItem(nameOfInteract, leftOverItems, itemSprite, itemDescription);
                return leftOverItems;
            }
        }
        return quantity;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < albumSlot.Length; i++)
        {
            albumSlot[i].selectedShader.SetActive(false);
            albumSlot[i].thisItemSelected = false;
        }
    }

    public void RemoveFirstItem()
    {
        if (albumSlot.Length > 0)
        {
            // Create a new array with length one less than the current array
            AlbumSlots[] newAlbumSlot = new AlbumSlots[albumSlot.Length - 1];

            // Shift elements to remove the first item
            for (int i = 0; i < newAlbumSlot.Length; i++)
            {
                newAlbumSlot[i] = albumSlot[i + 1];
            }

            // Set the last element to null to remove it
            albumSlot = newAlbumSlot;
        }
    }

    public void ButtonMaterial()
    {
        buttonMaterialsTab.SetActive(true);
        buttonAlbumTab.SetActive(false);
    }

    public void ButtonAlbum()
    {
        buttonMaterialsTab.SetActive(false);
        buttonAlbumTab.SetActive(true);
    }
}
