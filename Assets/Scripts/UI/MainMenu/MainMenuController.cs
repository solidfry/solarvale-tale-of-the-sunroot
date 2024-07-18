using UnityEngine;
using Events;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    private bool isMenuShown = false;
    private bool canShowMenu = true;

    // References to sliders in the scene
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundsVolumeSlider;

    void Start()
    {
        // Assign references to sliders from the scene
        masterVolumeSlider = GameObject.Find("MasterVolumeSlider").GetComponent<Slider>();
        musicVolumeSlider = GameObject.Find("MusicVolumeSlider").GetComponent<Slider>();
        soundsVolumeSlider = GameObject.Find("SoundsVolumeSlider").GetComponent<Slider>();
    }

    void Update()
    {
        // Open/close menu when 'M' key is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        if (!canShowMenu) return;

        isMenuShown = !isMenuShown;

        // Toggle visibility of the main menu canvas
        mainMenuCanvas.SetActive(isMenuShown);

        // Example: Log menu state for debugging
        Debug.Log("Menu Open: " + isMenuShown);

        // Trigger global events based on menu state
        GlobalEvents.OnGamePausedEvent?.Invoke(isMenuShown); // Pause game logic if menu is shown
        GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(isMenuShown); // Change player input mapping if menu is shown

        // Adjust cursor lock state based on menu visibility
        Cursor.lockState = isMenuShown ? CursorLockMode.None : CursorLockMode.Locked;

        // Resume game state if menu is closed
        if (!isMenuShown)
        {
            Time.timeScale = 1f; // Resume game time
            GlobalEvents.OnLockCursorEvent?.Invoke(true); // Lock cursor back when menu closes
        }
        else
        {
            Time.timeScale = 0f; // Pause game time
            GlobalEvents.OnLockCursorEvent?.Invoke(false); // Unlock cursor for UI interaction
        }

        // Ensure UI elements are interactable
        SetUIInteractivity(!isMenuShown);
    }

    void SetUIInteractivity(bool interactable)
    {
        // Set interactability of sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.interactable = interactable;
        if (musicVolumeSlider != null)
            musicVolumeSlider.interactable = interactable;
        if (soundsVolumeSlider != null)
            soundsVolumeSlider.interactable = interactable;
    }

    // Example function to be called elsewhere to update menu availability
    public void SetMenuAvailability(bool canShow)
    {
        canShowMenu = canShow;
    }

    private void OnDestroy()
    {
        // Clean up any instantiated menu instance or related objects
        if (mainMenuCanvas != null)
        {
            Destroy(mainMenuCanvas);
        }
    }
}






