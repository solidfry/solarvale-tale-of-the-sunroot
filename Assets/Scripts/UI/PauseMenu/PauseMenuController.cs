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

    void ToggleMenu()
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

        // Adjust cursor lock state based on menu visibility
     

        // Resume game state if menu is closed
      //  if (isMenuShown)
      //  {
     //       Time.timeScale = 0f; // Pause game time
      //       // Unlock cursor for UI interaction
     //   }
     //  else
       // {
       //     Time.timeScale = 1f; // Resume game time
       //     GlobalEvents.OnLockCursorEvent?.Invoke(true); // Lock cursor back when menu closes
       // }
    }


    // Example function to be called elsewhere to update menu availability
  //  public void SetMenuAvailability(bool canShow)
  //  {
  //      canShowMenu = canShow;
  //  }

}






