using UnityEngine;
using Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuCanvas;

    [SerializeField]
    private bool isMenuShown = false;
    
 

    // Input action reference
    [SerializeField]
    private InputActionReference pauseMenuOpenAction, pauseMenuCloseAction;


    void Awake()
    {
        // Initialize the Input Action

        if (pauseMenuOpenAction == null)
        {
            Debug.LogError("MainMenuOpen action not found in the InputActionAsset.");
            return;
        }

        pauseMenuOpenAction.action.performed += context => ToggleMenu();
        pauseMenuCloseAction.action.performed += context => ToggleMenu();

        pauseMenuCanvas.SetActive(false);
    }

    public void ToggleMenu()
    {


        isMenuShown = !isMenuShown;

        // Toggle visibility of the main menu canvas
        pauseMenuCanvas.SetActive(isMenuShown);

        // Log menu state and time scale for debugging
        Debug.Log($"Menu Open: {isMenuShown}");
        Debug.Log($"Time.timeScale: {Time.timeScale}");

        // Trigger global events based on menu state
        GlobalEvents.OnGamePausedEvent?.Invoke(isMenuShown); // Pause game logic if menu is shown
        GlobalEvents.OnLockCursorEvent?.Invoke(!isMenuShown);
        GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(isMenuShown); // Change player input mapping if menu is shown

    }



}






