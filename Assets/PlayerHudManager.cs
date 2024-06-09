using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [FormerlySerializedAs("isPaused")] [SerializeField] bool showMenu = false;

    GameObject menuInstance;
    
    private void Awake()
    {
        menuInstance = Instantiate(menu, transform.position, Quaternion.identity );
        menuInstance.SetActive(false);
    }
    

    void OnMenu()
    {
        showMenu = !showMenu;
        GameManager.OnGamePausedEvent?.Invoke();
        menuInstance.SetActive(showMenu);
    }

    private void OnDestroy()
    {
        Destroy(menuInstance);
    }
}
