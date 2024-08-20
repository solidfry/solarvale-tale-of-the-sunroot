using Core;
using DG.Tweening;
using Events;
using Interaction;
using UI.Prompts;
using UnityEngine;

namespace UI
{
    public class InteractableUIManager : MonoBehaviour
    {
        IInteractable _currentInteractable;
        [SerializeField] KeyPromptUI keyPromptUI;
        [SerializeField] CanvasGroup canvasGroup;
    
        [Header("Animation Settings")]
        [SerializeField] float fadeDuration = 0.5f;
        
        [SerializeField] string currentInteractableName;
    
        bool _isAnimating;
        bool _canInteract = true;
    

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;
        }

        private void OnEnable()
        {
            GlobalEvents.OnInteractableFound += SetInteractable;
            GlobalEvents.OnPlayerChangeActionMapEvent += OnPlayerControlsLocked;
            GlobalEvents.OnGameStateChangeEvent += OnGameStateChange;
        }

        private void OnGameStateChange(GameState state)
        {
            _canInteract = state == GameState.Exploration;
            // Debug.Log("Can interact: " + _canInteract);
            if (state != GameState.Exploration)
            {
               canvasGroup.alpha = 0;
            }
        }

        // private void Update()
        // {
        //     if (_currentInteractable == null) 
        //         currentInteractableName = "Null";
        //     else
        //         currentInteractableName = _currentInteractable?.Name;
        // }

        private void OnDisable()
        {
            GlobalEvents.OnInteractableFound -= SetInteractable;
            GlobalEvents.OnPlayerChangeActionMapEvent -= OnPlayerControlsLocked;
            GlobalEvents.OnGameStateChangeEvent -= OnGameStateChange;
        }

        void SetInteractable(IInteractable interactable)
        {
            if (!_canInteract) return;
            if (interactable == null)
            {
                FadeOut();
                _currentInteractable = null;
                ConfigureInteractableUI();
                return;
            }
            _currentInteractable = interactable;
            ConfigureInteractableUI();
            FadeIn();
        }

        private void FadeOut()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            canvasGroup.DOFade(0f, fadeDuration ).OnComplete(() =>
            {
                _isAnimating = false;
            } ).SetAutoKill(false);
        }
    
        private void FadeIn()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            canvasGroup.DOFade(1f, fadeDuration).OnComplete(() => _isAnimating = false ).SetAutoKill(false);
        }

        void ConfigureInteractableUI()
        {
            keyPromptUI.SetText(_currentInteractable?.InteractMessage);
        }
    
        private void OnPlayerControlsLocked(bool value)
        { 
            if (value)
            {
                FadeOut();
            }
            else
            {
                if (_currentInteractable != null)
                {
                    FadeIn();
                }
            }
        }
    }
}
