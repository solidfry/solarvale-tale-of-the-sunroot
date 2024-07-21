using UnityEngine;
using Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    private bool isMenuShown = false;
    private bool canShowMenu = true;

    // Serialize the Input Action Asset to ensure it remains assigned in the Inspector
    [SerializeField]
    private InputActionAsset inputActions;

    // Input action reference
    private InputAction mainMenuOpenAction;

    void Awake()
    {
        // Initialize the Input Action
        mainMenuOpenAction = inputActions.FindAction("Player/MainMenuOpen");

        if (mainMenuOpenAction == null)
        {
            Debug.LogError("MainMenuOpen action not found in the InputActionAsset.");
            return;
        }

        mainMenuOpenAction.performed += context =>
        {
            Debug.Log("MainMenuOpen action performed.");
            ToggleMenu();
        };
    }

    void OnEnable()
    {
        mainMenuOpenAction?.Enable();
    }

    void OnDisable()
    {
        mainMenuOpenAction?.Disable();
    }

    void ToggleMenu()
    {
        if (!canShowMenu) return;

        isMenuShown = !isMenuShown;

        // Toggle visibility of the main menu canvas
        mainMenuCanvas.SetActive(isMenuShown);

        // Log menu state for debugging
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






