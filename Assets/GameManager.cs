using System;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] HudManager hudManager;
    
    [SerializeField] bool IsPaused = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if (hudManager is null)
            hudManager = GetComponentInChildren<HudManager>();
    }

    private void OnEnable()
    {
        GlobalEvents.OnGamePausedEvent += PauseGame;
        GlobalEvents.OnLockCursorEvent += LockCursor;
        GlobalEvents.OnDialogueCompleteEvent += OnDialogueCompleteEvent;
        GlobalEvents.OnDialogueStartEvent += OnDialogueStartEvent;
    }

    public void OnDialogueStartEvent()
    {
        GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);
    }

    private void OnDialogueCompleteEvent()
    {
        GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(true);
    }

    private void OnDisable()
    {
        GlobalEvents.OnGamePausedEvent -= PauseGame;
        GlobalEvents.OnLockCursorEvent -= LockCursor;
        GlobalEvents.OnDialogueCompleteEvent -= OnDialogueCompleteEvent;
        GlobalEvents.OnDialogueStartEvent -= OnDialogueStartEvent;
    }
    
    void PauseGame(bool pause)
    {
        Debug.Log("PauseGame");
        IsPaused = pause;
        float _ = IsPaused ? Time.timeScale = 0 : Time.timeScale = 1;
    }
    
    void LockCursor(bool _lockCursor) => Cursor.lockState = _lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
}
