
using DG.Tweening;
using Events;
using Interaction;
using UI;
using UnityEngine;

public class InteractableUIManager : MonoBehaviour
{
    IInteract currentInteractable;
    [SerializeField] KeyPromptUI keyPromptUI;
    [SerializeField] CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] private Color interactedColor;
    [SerializeField] private float keyAnimationTime = 0.5f;
    

    private void Awake()
    {
        keyPromptUI.SetActive(false);
    }

    private void OnEnable()
    {
        GlobalEvents.OnInteractableFound += SetInteractable;
        GlobalEvents.OnInteractableUIEvent += Interacted;
    }

    private void Interacted()
    {
        if (currentInteractable == null) return;
        keyPromptUI.AnimateKeyImage(interactedColor, keyAnimationTime);
    }

    private void OnDisable()
    {
        GlobalEvents.OnInteractableFound -= SetInteractable;
        GlobalEvents.OnInteractableUIEvent -= Interacted;
    }

    void SetInteractable(IInteract interactable)
    {
        if (interactable == null)
        {
            FadeOut();
            currentInteractable = null;
            return;
        }
        currentInteractable = interactable;
        ConfigureInteractableUI();
    }

    private void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeDuration ).OnComplete( () => keyPromptUI.SetActive( false ) ).SetAutoKill(false);
    }
    
    private void FadeIn()
    {
        keyPromptUI.SetActive( true );
        canvasGroup.DOFade(1f, fadeDuration ).SetAutoKill(false);
    }

    void ConfigureInteractableUI()
    {
        if (currentInteractable == null)
        {
            FadeOut();
            return;
        }
        
        FadeIn();
        keyPromptUI.SetText(currentInteractable.InteractMessage);
        
        // Debug.Log( $"Interactable: {currentInteractable.Name} - {currentInteractable.InteractMessage}");
    }
}
