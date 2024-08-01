using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public enum FadeDirection
{
    In,
    Out,
    None
}

public class SimpleFaderOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 6f;
    [SerializeField] private float fadeDelayOnStart = 1f;
    [SerializeField] private bool fadeOnStart = false;
    [SerializeField] private FadeDirection fadeDirection = FadeDirection.In;
    [SerializeField] private bool autoFade = false;
    [SerializeField] private float timeBeforeExit = 1f;

    public UnityEvent FadeInAndOutOnComplete;
    public UnityEvent FadeInOnComplete;
    public UnityEvent FadeOutOnComplete;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (image == null) image = GetComponent<Image>();
    }
    
    private void Start() => FadeOnStart();


    private void FadeOnStart()
    {
        CheckKeyComponentsEnabled();
        
        if (!fadeOnStart)
        {
            fadeDirection = FadeDirection.None;
            return;
        }

        if (autoFade)
        {
            StartCoroutine(FadeInAndOutRoutine());
        }
        else
        {
            PerformFade(fadeDirection);
        }
    }

    private IEnumerator FadeInAndOutRoutine()
    {
        yield return new WaitForSeconds(fadeDelayOnStart);

        if (fadeDirection == FadeDirection.In)
        {
            yield return PerformFadeRoutine(FadeDirection.In);
            yield return new WaitForSeconds(timeBeforeExit);
            yield return PerformFadeRoutine(FadeDirection.Out);
            yield return new WaitForSeconds(fadeOutDuration);
        }
        else
        {
            yield return PerformFadeRoutine(FadeDirection.Out);
            yield return new WaitForSeconds(timeBeforeExit);
            yield return PerformFadeRoutine(FadeDirection.In);
            yield return new WaitForSeconds(fadeInDuration);
        }

        FadeInAndOutOnComplete?.Invoke();
    }

    private IEnumerator PerformFadeRoutine(FadeDirection direction)
    {
        PerformFade(direction);
        yield return new WaitForSeconds(direction == FadeDirection.In ? fadeInDuration : fadeOutDuration);
    }

    private void PerformFade(FadeDirection direction)
    {
        switch (direction)
        {
            case FadeDirection.In:
                // CheckKeyComponentsEnabled();
                FadeIn();
                break;
            case FadeDirection.Out:
                // CheckKeyComponentsEnabled();
                FadeOut();
                break;
            default:
                canvasGroup.alpha = 0;
                break;
        }
    }

    private void CheckKeyComponentsEnabled()
    {
        if (!image.enabled) image.enabled = true;
        if (!canvasGroup.enabled) canvasGroup.enabled = true;
    }

    void FadeIn()
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, fadeInDuration).SetDelay(fadeDelayOnStart).SetEase(Ease.Linear)
            .OnComplete(() => FadeInOnComplete?.Invoke()).SetAutoKill(true).Delay();
    }

    void FadeOut()
    {
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, fadeOutDuration).SetEase(Ease.Linear).SetDelay(fadeDelayOnStart)
            .OnComplete(() => FadeOutOnComplete?.Invoke()).SetAutoKill(true).Delay();
        Debug.Log("Fading out for " + fadeOutDuration + " seconds");
    }
}
