using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Onboarding
{
    public class OnboardingNotification : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] Image image;
        [SerializeField] float startOffset = 32;
        [SerializeField] private Color pulseColor;

        private Tween sizeTween;
        private Tween colorTween;
        private Tween fadeTween;
        private Tween sizeTweenShow;
        private Tween hideTween;

        private Sequence pulseSequence;
        private Sequence showSequence;

        private OnboardingNotificationRequest _request;

        public event Action OnNotificationComplete = delegate { };

        public void Initialise(OnboardingNotificationRequest req)
        {
            if (rectTransform == null || image == null) return;

            SetRequest(req);

            rectTransform.anchoredPosition = _request.Position;
            rectTransform.sizeDelta = new Vector2(_request.Width + startOffset, _request.Height + startOffset);

            fadeTween = image.DOFade(1, 0.5f).From(0).SetEase(Ease.Linear);
            sizeTweenShow = rectTransform.DOSizeDelta(new Vector2(_request.Width, _request.Height), 0.5f).SetEase(Ease.InOutCirc);
            hideTween = image.DOFade(0, 0.25f).SetEase(Ease.Linear);

            Show();
        }

        private void SetRequest(OnboardingNotificationRequest req)
        {
            _request = req;
        }

        private void Show()
        {
            if (rectTransform == null || image == null) return;

            showSequence = DOTween.Sequence();
            showSequence.Append(fadeTween).Append(sizeTweenShow).OnComplete(Pulse);
            showSequence.Play();
        }

        private void Pulse()
        {
            if (rectTransform == null || image == null) return;

            sizeTween = rectTransform.DOSizeDelta(new Vector2(_request.Width + startOffset, _request.Height + startOffset), _request.Duration).SetEase(Ease.InOutCirc);
            colorTween = image.DOColor(_request.PulseColor, _request.Duration).SetEase(Ease.Linear);

            pulseSequence = DOTween.Sequence();
            pulseSequence.Append(sizeTween).Append(colorTween).SetLoops(3, LoopType.Yoyo).OnComplete(Hide);
            pulseSequence.Play();
        }

        private void Hide()
        {
            if (image == null) return;

            hideTween.OnComplete(() => OnNotificationComplete()).Play();
        }

        public void Cleanup()
        {
            sizeTween?.Kill(false);
            colorTween?.Kill(false);
            fadeTween?.Kill(false);
            sizeTweenShow?.Kill(false);
            hideTween?.Kill(false);
            pulseSequence?.Kill();
            showSequence?.Kill();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}