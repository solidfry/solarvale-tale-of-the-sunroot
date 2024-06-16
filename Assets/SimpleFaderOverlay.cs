using System.Collections;
using UnityEngine;
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
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeDelayOnStart = 1f;
    [SerializeField] private bool fadeOnStart = false;
    [SerializeField] private FadeDirection fadeDirection = FadeDirection.In;
    [SerializeField] private bool autoFade = false;
    [SerializeField] private float timeBeforeExit = 1f;

    public UnityEvent OnComplete;

    private void Start() => FadeOnStart();

    private void FadeOnStart()
    {
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
        }
        else
        {
            yield return PerformFadeRoutine(FadeDirection.Out);
            yield return new WaitForSeconds(timeBeforeExit);
            yield return PerformFadeRoutine(FadeDirection.In);
        }

        OnComplete?.Invoke();
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
                FadeIn();
                break;
            case FadeDirection.Out:
                FadeOut();
                break;
            default:
                canvasGroup.alpha = direction == FadeDirection.In ? 1 : 0;
                break;
        }
    }

    public void FadeIn() => canvasGroup.DOFade(1, fadeInDuration);

    public void FadeOut() => canvasGroup.DOFade(0, fadeOutDuration);
}
