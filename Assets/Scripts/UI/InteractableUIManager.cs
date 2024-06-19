using DG.Tweening;
using Events;
using Interaction;
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
        [SerializeField] private float keyAnimationTime = 0.5f;
    
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
            GlobalEvents.OnPlayerControlsLockedEvent += OnPlayerControlsLocked;
        }

   

        private void OnDisable()
        {
            GlobalEvents.OnInteractableFound -= SetInteractable;
            GlobalEvents.OnPlayerControlsLockedEvent -= OnPlayerControlsLocked;
        }

        void SetInteractable(IInteractable interactable)
        {
            if (!_canInteract) return;
            if (interactable == null)
            {
                FadeOut();
                _currentInteractable = null;
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
            canvasGroup.DOFade(1f, fadeDuration ).OnComplete( ( ) => _isAnimating = false ).SetAutoKill(false);
        }

        void ConfigureInteractableUI()
        {
            keyPromptUI.SetText(_currentInteractable.InteractMessage);
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
