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
        public PhotoManager PhotoManager => photoManager;
        public CollectionManager CollectionManager => collectionManager;

        readonly Observable<bool> _playerCanInteract = new(true);

        public void Awake()
        {
            if (hudManager is null)
                hudManager = GetComponentInChildren<HudManager>();
            
            if (photoManager is null)
                photoManager = GetComponentInChildren<PhotoManager>();
            
            if (collectionManager is null)
                collectionManager = GetComponentInChildren<CollectionManager>();
            
            // CollectionManager.SetGameManager(this);
        }
        

        private void OnEnable()
        {
            GlobalEvents.OnGamePausedEvent += PauseGame;
            GlobalEvents.OnLockCursorEvent += LockCursor;
            GlobalEvents.OnDialogueCompleteEvent += SetPlayerInteractionActive;
            GlobalEvents.OnDialogueStartEvent += SetPlayerInteractionInactive;
            
            _playerCanInteract.ValueChanged += _ => GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(!_);
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnGamePausedEvent -= PauseGame;
            GlobalEvents.OnLockCursorEvent -= LockCursor;
            GlobalEvents.OnDialogueCompleteEvent -= SetPlayerInteractionActive;
            GlobalEvents.OnDialogueStartEvent -= SetPlayerInteractionInactive;
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
            Debug.Log("PauseGame");
            IsPaused = pause;
            float _ = IsPaused ? Time.timeScale = 0 : Time.timeScale = 1;
            Debug.Log($"Time.timeScale: {Time.timeScale}");
        }
    
        void LockCursor(bool _lockCursor) => Cursor.lockState = _lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        
        public void SetPlayerCanInteract(bool canInteract) => _playerCanInteract.Value = canInteract;
        
    }
}
