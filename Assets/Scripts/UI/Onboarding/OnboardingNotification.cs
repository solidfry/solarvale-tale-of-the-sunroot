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
        private Tween fade;
        private Tween sizeTweenShow;
        private Tween hide;

        OnboardingNotificationRequest _request;

        public event Action OnNotificationComplete = delegate { };

        public void Initialise(OnboardingNotificationRequest req)
        {
            SetRequest(req);
            
            
            rectTransform.anchoredPosition = _request.Position;
            
            rectTransform.sizeDelta = 
                new Vector2(_request.Width + startOffset, _request.Height + startOffset);
            
            
            
            fade = image.DOFade(1, .5f).From(0).SetEase(Ease.Linear);

            sizeTweenShow = rectTransform.DOSizeDelta(
                new Vector2(_request.Width, _request.Height), .5f);


            

            hide = image.DOFade(0, 0.25f).SetEase(Ease.Linear);
            
            Show();
        }

        void SetRequest(OnboardingNotificationRequest req)
        {
            _request = req;
        }

        void Show()
        {
            var showSequence = DOTween.Sequence();
            showSequence.Append(fade).Append(sizeTweenShow).SetEase(Ease.InOutCirc).OnComplete(Pulse);

            Debug.Log("Showing notification");
            showSequence.Play();
        }

        void Pulse()
        {
            var pulseSequence = DOTween.Sequence();

            sizeTween = rectTransform.DOSizeDelta(
                    new Vector2(_request.Width + startOffset, _request.Height + startOffset), _request.Duration)
                .SetEase(Ease.InOutCirc);
            
            colorTween = image.DOColor(_request.PulseColor, _request.Duration).SetEase(Ease.Linear);

            pulseSequence
                .Append(sizeTween)
                .Append(colorTween).OnComplete(Hide).Play().SetLoops(3, LoopType.Yoyo);
            Debug.Log("Pulsing notification");
        }

        void Hide()
        {
            Debug.Log("Hiding notification");
            hide.OnComplete(() =>
            {
                OnNotificationComplete();
            }).Play();
        }

        private void OnDestroy()
        {
            sizeTween?.Kill();
            colorTween?.Kill();
            fade?.Kill();
            sizeTweenShow?.Kill();
            hide?.Kill();
        }
    }
}
