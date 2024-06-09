using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // Events
    public static Action OnGamePausedEvent;
    
    bool isPaused = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        OnGamePausedEvent += PauseGame;
    }

    private void OnDisable()
    {
        OnGamePausedEvent -= PauseGame;
    }
    
    void PauseGame()
    {
        Debug.Log("PauseGame");
        isPaused = !isPaused;
        float _ = isPaused ? Time.timeScale = 0 : Time.timeScale = 1;
        Debug.Log("Time.timeScale: " + Time.timeScale);
    }
}
