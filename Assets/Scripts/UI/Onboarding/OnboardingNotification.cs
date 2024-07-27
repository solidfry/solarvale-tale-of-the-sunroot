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
    
        [SerializeField] float pulseDuration = 1f;
    
        OnboardingNotificationRequest _request;
    
        public event Action OnNotificationComplete = delegate {  }; 

        public void Initialise(OnboardingNotificationRequest req)
        {
            SetRequest(req);
            Show();
        }
    
        void SetRequest(OnboardingNotificationRequest req)
        {
            _request = req;
        }
    
        void Show()
        {
            rectTransform.anchoredPosition = _request.Position;
            rectTransform.sizeDelta = new Vector2(_request.Width + startOffset, _request.Height + startOffset);
            image.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
            rectTransform.DOSizeDelta(
                    new Vector2(_request.Width, _request.Height), 
                    1f)
                .SetEase(Ease.InOutCirc)
                .OnComplete(Pulse);
        }
    
        void Pulse()
        {
            Sequence pulseSequence = DOTween.Sequence();
            pulseSequence
                .Append(
                    rectTransform.DOSizeDelta(
                        new Vector2(_request.Width + startOffset, _request.Height + startOffset), 
                        pulseDuration)
                )
                .SetEase(Ease.InOutCirc)
                .Append(
                    image.DOColor(_request.PulseColor, pulseDuration)).SetEase(Ease.Linear).SetLoops(6, LoopType.Yoyo).OnComplete(Hide);
        }
    
        void Hide()
        {
            image.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                OnNotificationComplete();
            });
        }
    
    }
}
