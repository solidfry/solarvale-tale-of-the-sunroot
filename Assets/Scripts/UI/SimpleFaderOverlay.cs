using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
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
    
        Sequence fadeInSequence;
        Sequence fadeOutSequence;
        Tween fadeInTween;
        Tween fadeOutTween;

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            image ??= GetComponent<Image>();
        
            CheckKeyComponentsEnabled();
        
            if (canvasGroup is null || image is null) return;

            CreateTweenSequences();

        }

        private void CreateTweenSequences()
        {
            if (autoFade || fadeDirection == FadeDirection.In)
            {
                CreateFadeIn();
            }

            if (autoFade || fadeDirection == FadeDirection.Out)
            {
                CreateFadeOut();
            }
        }

        private void CreateFadeOut()
        {
            fadeOutTween = canvasGroup.DOFade(0, fadeOutDuration).SetEase(Ease.Linear).SetAutoKill(false);
            fadeOutSequence = DOTween.Sequence().Append(fadeOutTween).Pause().SetAutoKill(false).OnComplete(() =>
            {
                FadeOutOnComplete?.Invoke();
            }).SetDelay(fadeDelayOnStart);
        }

        private void CreateFadeIn()
        {
            fadeInTween = canvasGroup.DOFade(1, fadeInDuration).SetEase(Ease.Linear).SetAutoKill(false);
            fadeInSequence = DOTween.Sequence().Append(fadeInTween).Pause().SetAutoKill(false).OnComplete(() =>
            {
                FadeInOnComplete?.Invoke();
            }).SetDelay(fadeDelayOnStart);
        }

        private void OnEnable() => FadeOnStart();

        private void OnDisable()
        {
            fadeInSequence?.Kill();
            fadeOutSequence?.Kill();
            fadeInTween?.Kill();
            fadeOutTween?.Kill();
        }

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

            if (fadeDirection is FadeDirection.In)
            {
                Debug.Log("Fade In Started");
                yield return PerformFadeRoutine(FadeDirection.In);
                Debug.Log("Fade In Complete");
                yield return new WaitForSeconds(timeBeforeExit);
                Debug.Log("Fade Out Started");
                yield return PerformFadeRoutine(FadeDirection.Out);
                Debug.Log("Fade Out Complete");
                yield return new WaitForSeconds(fadeOutDuration);
            }
            else
            {
                Debug.Log("Fade Out Started");
                yield return PerformFadeRoutine(FadeDirection.Out);
                Debug.Log("Fade Out Complete");
                yield return new WaitForSeconds(timeBeforeExit);
                Debug.Log("Fade In Started");
                yield return PerformFadeRoutine(FadeDirection.In);
                Debug.Log("Fade In Complete");
                yield return new WaitForSeconds(fadeInDuration);
            }

            FadeInAndOutOnComplete?.Invoke();
        }

        private IEnumerator PerformFadeRoutine(FadeDirection direction)
        {
            PerformFade(direction);
            Debug.Log("Fade Direction: " + direction + " for " + (direction == FadeDirection.In ? fadeInDuration : fadeOutDuration) + " seconds");
            yield return new WaitForSeconds(direction == FadeDirection.In ? fadeInDuration : fadeOutDuration);
        }

        private void PerformFade(FadeDirection direction)
        {
            switch (direction)
            {
                case FadeDirection.In:
                    // CheckKeyComponentsEnabled();
                    // Debug.Log("Fade In");
                    canvasGroup.alpha = 0;
                    FadeIn();
                    break;
                case FadeDirection.Out:
                    // Debug.Log("Fade Out");
                    // CheckKeyComponentsEnabled();
                    canvasGroup.alpha = 1;
                    FadeOut();
                    break;
                default:
                    break;
            }
        }

        private void CheckKeyComponentsEnabled()
        {
            if (image is null || canvasGroup is null) return;
            if (!image.enabled) image.enabled = true;
            if (!canvasGroup.enabled) canvasGroup.enabled = true;
        }

        void FadeIn()
        {
            if (canvasGroup is null) return;
            if (fadeInSequence is null) return;
            fadeInSequence.Restart();
            fadeInSequence.PlayForward();
        }

        void FadeOut()
        {
            if (canvasGroup is null) return;
            if (fadeOutSequence is null) return;
            fadeOutSequence.Restart();
            fadeOutSequence.PlayForward();
        }
    }
}