using System;
using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] GameObject menu;
        [SerializeField] bool showMenu = false;
        [SerializeField] bool canShowMenu = true;
        [SerializeField] InputActionReference menuOpenAction;
        [SerializeField] InputActionReference menuCloseAction;
        
        [SerializeField] List<UIType> uiCanvases;

        GameObject menuInstance;
        
        // I want the UIs to register with the HudManager
        public void RegisterUI(GameObject uiPrefab, InterfaceType interfaceType)
        {
            var uiType = new UIType
            {
                uiPrefab = uiPrefab,
                interfaceType = interfaceType
            };
            uiCanvases.Add(uiType);
        }
    
        private void Awake()
        {
            menuInstance = Instantiate(menu, transform.position, Quaternion.identity);
            menuInstance.SetActive(false);
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
            menuInstance.SetActive(showMenu);
            // Debug.Log("show menu: " + showMenu);
            GlobalEvents.OnGamePausedEvent?.Invoke(showMenu);
            GlobalEvents.OnPlayerControlsLockedEvent?.Invoke(showMenu);
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            Destroy(menuInstance);
        }
    
        public void SetCanShowMenu(bool canShow)
        {
            canShowMenu = canShow;
        }
    }
    
    [Serializable]
    public class UIType
    {
        [SerializeField] public GameObject uiPrefab;
        [SerializeField] public InterfaceType interfaceType;
    }
    
    public enum InterfaceType
    {
        HUD,
        Menu,
        Dialogue,
        Quest,
        Interaction,
    }
}
