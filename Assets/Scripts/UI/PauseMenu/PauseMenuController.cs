using CameraSystem;
using Core;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.PauseMenu
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] MenuTabManager pauseMenuCanvas;
        

        [SerializeField]
        private bool isMenuShown = false;
    
        // Input action reference
        [SerializeField]
        private InputActionReference pauseMenuOpenAction, pauseMenuCloseAction;
        
        bool canShowMenu = true;
        
        void Awake()
        {
            pauseMenuCanvas = GetComponent<MenuTabManager>();
            // Initialize the Input Action

            if (pauseMenuOpenAction == null)
            {
                Debug.LogError("MainMenuOpen action not found in the InputActionAsset.");
                return;
            }

            pauseMenuOpenAction.action.performed += context => ToggleMenu();
            pauseMenuCloseAction.action.performed += context => ToggleMenu();

            // ToggleMenu();
        }

        private void OnEnable()
        {
            GlobalEvents.OnChangeCameraModeEvent += SetMenuAvailability;
            GlobalEvents.OnPauseMenuAvailabilityEvent += SetMenuAvailability;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnChangeCameraModeEvent -= SetMenuAvailability;
            GlobalEvents.OnPauseMenuAvailabilityEvent -= SetMenuAvailability;
        }

        private void SetMenuAvailability(CameraMode mode)
        {
            canShowMenu = mode == CameraMode.Exploration;
        }


        private void SetMenuAvailability(bool availability)
        {
            canShowMenu = availability;
        }

        public void ToggleMenu()
        {
            if (!canShowMenu) return;
            isMenuShown = !isMenuShown;
            // Toggle visibility of the main menu canvas

            // Log menu state and time scale for debugging
            Debug.Log($"Menu Open: {isMenuShown}");
            Debug.Log($"Time.timeScale: {Time.timeScale}");
            // Trigger global events based on menu state
            GlobalEvents.OnGamePausedEvent?.Invoke(isMenuShown); // Pause game logic if menu is shown
            // GlobalEvents.OnLockCursorEvent?.Invoke(!isMenuShown);
            GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(isMenuShown);
            GlobalEvents.OnGameStateChangeEvent?.Invoke(isMenuShown ? 
                GameState.PauseMenu : GameState.Exploration);

            if (!isMenuShown)
            {
                EventSystem.current.SetSelectedGameObject(null);
                pauseMenuCanvas.CloseAllMenus();
            }
            else
            {
                pauseMenuCanvas.OpenMenu(0);
            }
            
        }



    }
}






