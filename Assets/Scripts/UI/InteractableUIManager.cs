
using DG.Tweening;
using Events;
using Interaction;
using UI;
using UnityEngine;

public class InteractableUIManager : MonoBehaviour
{
    IInteractable currentInteractable;
    [SerializeField] KeyPromptUI keyPromptUI;
    [SerializeField] CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] private float keyAnimationTime = 0.5f;
    
    bool isAnimating;
    

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
    }
    
    private void OnDisable()
    {
        GlobalEvents.OnInteractableFound -= SetInteractable;
    }

    void SetInteractable(IInteractable interactable)
    {
        if (interactable == null)
        {
            FadeOut();
            currentInteractable = null;
            return;
        }
        currentInteractable = interactable;
        ConfigureInteractableUI();
        FadeIn();
    }

    private void FadeOut()
    {
        if (isAnimating) return;
        isAnimating = true;
        canvasGroup.DOFade(0f, fadeDuration ).OnComplete(() =>
        {
            isAnimating = false;
        } ).SetAutoKill(false);
    }
    
    private void FadeIn()
    {
        if (isAnimating) return;
        isAnimating = true;
        canvasGroup.DOFade(1f, fadeDuration ).OnComplete( ( ) => isAnimating = false ).SetAutoKill(false);
    }

    void ConfigureInteractableUI()
    {
        keyPromptUI.SetText(currentInteractable.InteractMessage);
    }
}
