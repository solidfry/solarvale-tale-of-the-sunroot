using Events;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] PlayerInput playerInput;
        [SerializeField] StarterAssetsInputs starterAssetsInputs;

        private string _currentControlsScheme;
        Observable<string> _currentControlSchemeObservable;
        
        private void Awake()
        {
            if (playerInput is null)
                playerInput = GetComponentInChildren<PlayerInput>();
            
            _currentControlSchemeObservable = new Observable<string>(playerInput.currentControlScheme);
            _currentControlSchemeObservable.ValueChanged += OnControlsChanged;
        }

        private void Update()
        {
            if (_currentControlsScheme == playerInput.currentControlScheme) return;
            _currentControlsScheme = playerInput.currentControlScheme;
            _currentControlSchemeObservable?.Invoke();
        }

        private void OnEnable()
        {
            GlobalEvents.OnPlayerChangeActionMapEvent += TogglePlayerActionMap;
            GlobalEvents.OnSetCursorInputForLookEvent += SetCursorInputForLook;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnPlayerChangeActionMapEvent -= TogglePlayerActionMap;
            GlobalEvents.OnSetCursorInputForLookEvent -= SetCursorInputForLook; 
            _currentControlSchemeObservable.ValueChanged -= OnControlsChanged;
        }

        private void OnControlsChanged(string inputs)
        {
            GlobalEvents.OnControlSchemeChangedEvent?.Invoke(playerInput.currentControlScheme);
        }

        private void SetCursorInputForLook(bool canLook)
        {
            if (starterAssetsInputs is null) return;
            starterAssetsInputs.cursorInputForLook = canLook;
        }
        
        public void SetPlayerControlsLocked(bool isLocked)
        {
            switch (isLocked)
            {
                case true:
                    playerInput.ActivateInput();
                    break;
                default:
                    playerInput.DeactivateInput();
                    break;
            }
        }

        public void TogglePlayerActionMap(bool isPaused) => SetPlayerControlMap(isPaused ? "UI" : "Player");

        void SetPlayerControlMap(string actionMap)
        {
            playerInput.SwitchCurrentActionMap(actionMap);
        }
    
    }
}
