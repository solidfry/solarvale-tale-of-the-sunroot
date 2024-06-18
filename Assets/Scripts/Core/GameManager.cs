using Events;
using Gaia;
using UnityEngine;
using Utilities;

namespace Core
{
    public enum GameState
    {
        MainMenu,
        InGame,
        Paused,
        Dialogue,
        Cutscene
    }
    public class GameManager : SingletonPersistent<GameManager>
    {
        [SerializeField] HudManager hudManager;
    
        [SerializeField] bool IsPaused = false;

        readonly Observable<bool> _playerCanInteract = new(true);

        public override void Awake()
        {
            base.Awake();
         
            if (hudManager is null)
                hudManager = GetComponentInChildren<HudManager>();
        }
        

        private void OnEnable()
        {
            GlobalEvents.OnGamePausedEvent += PauseGame;
            GlobalEvents.OnLockCursorEvent += LockCursor;
            GlobalEvents.OnDialogueCompleteEvent += OnDialogueCompleteEvent;
            GlobalEvents.OnDialogueStartEvent += OnDialogueStartEvent;
            
            _playerCanInteract.ValueChanged += _ => GlobalEvents.OnPlayerControlsLockedEvent?.Invoke(!_);
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnGamePausedEvent -= PauseGame;
            GlobalEvents.OnLockCursorEvent -= LockCursor;
            GlobalEvents.OnDialogueCompleteEvent -= OnDialogueCompleteEvent;
            GlobalEvents.OnDialogueStartEvent -= OnDialogueStartEvent;
            
        }

        public void OnDialogueStartEvent()
        {
            _playerCanInteract.Value = false;
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(false);
        }

        private void OnDialogueCompleteEvent()
        {
            _playerCanInteract.Value = true;
            GlobalEvents.OnSetCursorInputForLookEvent?.Invoke(true);
        }

        
    
        void PauseGame(bool pause)
        {
            Debug.Log("PauseGame");
            IsPaused = pause;
            float _ = IsPaused ? Time.timeScale = 0 : Time.timeScale = 1;
        }
    
        void LockCursor(bool _lockCursor) => Cursor.lockState = _lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        
        public void SetPlayerCanInteract(bool canInteract) => _playerCanInteract.Value = canInteract;
    }
}
