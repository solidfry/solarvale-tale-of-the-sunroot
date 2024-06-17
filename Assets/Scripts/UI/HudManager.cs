using Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class HudManager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] bool showMenu = false;
    [SerializeField] bool canShowMenu = true;
    [SerializeField] InputActionReference menuOpenAction;
    [SerializeField] InputActionReference menuCloseAction;

    GameObject menuInstance;
    
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

    private void OnDisable()
    {
        menuOpenAction.action.performed -= _ => OnMenu();
        menuCloseAction.action.performed -= _ => OnMenu();
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
