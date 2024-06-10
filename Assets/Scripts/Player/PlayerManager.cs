using System.Collections;
using System.Collections.Generic;
using Events;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] StarterAssetsInputs starterAssetsInputs;
    
    private void Awake()
    {
        if (playerInput is null)
            playerInput = GetComponentInChildren<PlayerInput>();
    }
    
    private void OnEnable()
    {
        GlobalEvents.OnPlayerControlsLockedEvent += TogglePlayerControls;
        GlobalEvents.OnSetCursorInputForLookEvent += SetCursorInputForLook;
    }

    private void SetCursorInputForLook(bool canLook)
    {
        if (starterAssetsInputs is null) return;
        starterAssetsInputs.cursorInputForLook = canLook;
    }

    private void OnDisable()
    {
        GlobalEvents.OnPlayerControlsLockedEvent -= TogglePlayerControls;
        GlobalEvents.OnSetCursorInputForLookEvent -= SetCursorInputForLook;
    }

    void TogglePlayerControls(bool isPaused)
    {   
        //Disable Player Input on Player Action Map
        SetPlayerControlMap(isPaused ? "UI" : "Player");
    }
    
    void SetPlayerControlMap(string actionMap)
    {
        playerInput.SwitchCurrentActionMap(actionMap);
    }
    
}
