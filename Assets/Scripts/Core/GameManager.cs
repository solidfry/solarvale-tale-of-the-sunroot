using System;
using Events;
using Photography;
using Progression;
using UI;
using UnityEngine;
using Utilities;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] HudManager hudManager;
        [SerializeField] PhotoManager photoManager;
        [SerializeField] CollectionManager collectionManager;
    
        [SerializeField] bool IsPaused = false;
        [SerializeField] GameState currentGameState = GameState.Exploration;
        [SerializeField] CursorManager cursorManager;
        public PhotoManager PhotoManager => photoManager;
        public CollectionManager CollectionManager => collectionManager;

        readonly Observable<bool> _playerCanInteract = new(true);
        Observable<GameState> _currentGameState;

        public void Awake()
        {
            if (hudManager is null)
                hudManager = GetComponentInChildren<HudManager>();
            
            if (photoManager is null)
                photoManager = GetComponentInChildren<PhotoManager>();
            
            if (collectionManager is null)
                collectionManager = GetComponentInChildren<CollectionManager>();
            
            InitialiseGameState();
            InitialiseCursorManager();
        }

        private void InitialiseGameState()
        {
            currentGameState = GameState.Cutscene;
            _currentGameState = new Observable<GameState>(currentGameState);
        }

        private void InitialiseCursorManager()
        {
            cursorManager = new CursorManager();
            _currentGameState.ValueChanged += cursorManager.OnGameStateChange;
        }

        private void OnEnable()
        {
            GlobalEvents.OnGamePausedEvent += PauseGame;
            GlobalEvents.OnDialogueCompleteEvent += SetPlayerInteractionActive;
            GlobalEvents.OnDialogueStartEvent += SetPlayerInteractionInactive;
            GlobalEvents.OnGameStateChangeEvent += SetGameState;
            
            _playerCanInteract.ValueChanged += _ => GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(!_);
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnGamePausedEvent -= PauseGame;
            GlobalEvents.OnDialogueCompleteEvent -= SetPlayerInteractionActive;
            GlobalEvents.OnDialogueStartEvent -= SetPlayerInteractionInactive;
            GlobalEvents.OnGameStateChangeEvent += SetGameState;

            _currentGameState.ValueChanged -= cursorManager.OnGameStateChange;

            SetTimeScale(1);
        }
        
        public void SetPlayerInteractionInactive()
        {
            _playerCanInteract.Value = false;
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);
        }

        private void SetPlayerInteractionActive()
        {
            _playerCanInteract.Value = true;
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(true);
        }
        
        void PauseGame(bool pause)
        {
            IsPaused = pause;
            float _ = IsPaused ? SetTimeScale(0) : SetTimeScale(1);
        }
        
        float SetTimeScale(float timeScale) => Time.timeScale = timeScale;
        
        public void SetPlayerCanInteract(bool canInteract) => _playerCanInteract.Value = canInteract;
        
        void SetGameState(GameState state)
        {
            currentGameState = state;
            _currentGameState.Value = currentGameState;
        }
        
    }
    
    [Serializable]
    class CursorManager
    {
        [SerializeField] bool isCursorVisible;
        public void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Exploration:
                    SetCursorVisible(false);
                    break;
                case GameState.Photography:
                    SetCursorVisible(false);
                    break;
                case GameState.EditingPhoto:
                    SetCursorVisible(true);
                    break;
                case GameState.Dialogue:
                    SetCursorVisible(true);
                    break;
                case GameState.Menu:
                    SetCursorVisible(true);
                    break;
                case GameState.PauseMenu:
                    SetCursorVisible(true);
                    break;
                case GameState.Cutscene:
                    SetCursorVisible(false);
                    break;
                default:
                    SetCursorVisible(true);
                    break;
            }
        }
        
        void SetCursorVisible(bool showCursor)
        {
            Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showCursor;
            
            isCursorVisible = showCursor;
        }
        
    }
}
