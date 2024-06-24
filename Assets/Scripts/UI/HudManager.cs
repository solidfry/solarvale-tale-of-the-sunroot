using System;
using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] GameObject menu;
        [SerializeField] bool showMenu = false;
        [SerializeField] bool canShowMenu = true;
        [SerializeField] InputActionReference menuOpenAction;
        [SerializeField] InputActionReference menuCloseAction;

        GameObject _menuInstance;
    
        private void Awake()
        {
            _menuInstance = Instantiate(menu, transform.position, Quaternion.identity);
            _menuInstance.SetActive(false);
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
        }
        
    }
}
