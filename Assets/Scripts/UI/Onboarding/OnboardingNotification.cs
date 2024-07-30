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
        
        Sequence pulseSequence;
    
    
        OnboardingNotificationRequest _request;
    
        public event Action OnNotificationComplete = delegate {  }; 

        public void Initialise(OnboardingNotificationRequest req)
        {
            pulseSequence = DOTween.Sequence();

            pulseSequence.Append(pulseSequence
                .Append(
                    rectTransform.DOSizeDelta(
                        new Vector2(_request.Width + startOffset, _request.Height + startOffset),
                        _request.Duration).SetEase(Ease.InOutCirc)
                )
                .Append(
                    image.DOColor(_request.PulseColor, _request.Duration).SetEase(Ease.Linear)
                )
                .SetLoops(3, LoopType.Yoyo)
                .OnComplete(Hide));
            
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
            image.DOFade(1, .5f).From(0).SetEase(Ease.Linear);
            rectTransform.DOSizeDelta(
                    new Vector2(_request.Width, _request.Height), 
                    .5f)
                .SetEase(Ease.InOutCirc)
                .OnComplete(Pulse);
        }
    
        void Pulse()
        {
            pulseSequence.Play();
        }
    
        void Hide()
        {
            image.DOFade(0, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                // pulseSequence.Kill();
                OnNotificationComplete();
            });
        }

        private void OnDestroy()
        {
            if (pulseSequence.IsActive())
                pulseSequence.Kill();
        }
    }
}
