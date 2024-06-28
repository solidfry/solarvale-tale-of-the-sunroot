using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class HudManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] GameObject menu;
        [SerializeField] GameObject playerHUD;
        
        [Header("Menu Settings")]
        [SerializeField] bool showMenu = false;
        [SerializeField] bool canShowMenu = true;
        [SerializeField] InputActionReference menuOpenAction;
        [SerializeField] InputActionReference menuCloseAction;
        
        GameObject _menuInstance;
        GameObject _playerHUDInstance;
    
        private void Start()
        {
            if (menu == null || playerHUD == null) return;
            _menuInstance = Instantiate(menu, transform.position, Quaternion.identity, transform);
            _playerHUDInstance = Instantiate(playerHUD, transform.position, Quaternion.identity, transform);
            _menuInstance.SetActive(false);
            // _playerHUDInstance.SetActive(false);
        }

        private void OnEnable()
        {
            menuOpenAction.action.performed += _ => OnMenu();
            menuCloseAction.action.performed += _ => OnMenu();
        }

        void OnMenu()
        {
            if (!canShowMenu) return;
            showMenu = !showMenu;
            _menuInstance.SetActive(showMenu);
            // Debug.Log("show menu: " + showMenu);
            GlobalEvents.OnGamePausedEvent?.Invoke(showMenu);
            GlobalEvents.OnPlayerControlsLockedEvent?.Invoke(showMenu);
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            Destroy(_menuInstance);
            Destroy(_playerHUDInstance);
        }
        
    }
}
