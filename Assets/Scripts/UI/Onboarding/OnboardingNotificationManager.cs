using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Onboarding
{
    public class OnboardingNotificationManager : MonoBehaviour
    {
        [SerializeField] Canvas onboardingCanvas;
        [SerializeField] OnboardingNotification onboardingNotificationPrefab;
        readonly Queue<OnboardingNotificationRequest> _onboardingQueue = new();
        
        [FormerlySerializedAs("_onboardingNotifications")] [SerializeField] List<OnboardingNotification> onboardingNotifications = new();
        
        OnboardingNotification _currentNotification;
        
        private void OnEnable()
        {
            GlobalEvents.OnOnboardingRequestEvent += OnOnboardingRequest;
            GlobalEvents.OnOnboardingInterruptEvent += InterruptOnboarding;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnOnboardingRequestEvent -= OnOnboardingRequest;
            GlobalEvents.OnOnboardingInterruptEvent -= InterruptOnboarding;
        }

        private void OnOnboardingRequest(OnboardingNotificationRequest req)
        {
            _onboardingQueue.Enqueue(req);
            if (_onboardingQueue.Count == 1)
            {
                ShowOnboardingNotification();
            }
        }

        private void ShowOnboardingNotification()
        {
            var notification = _onboardingQueue.Peek();
            _currentNotification = Instantiate(onboardingNotificationPrefab, onboardingCanvas.transform);
            SetAnchorPosition(notification.Anchor, _currentNotification);
            onboardingNotifications.Add(_currentNotification);
            _currentNotification.Initialise(notification);
            _currentNotification.OnNotificationComplete += OnNotificationComplete;
        }

        private void OnNotificationComplete()
        {
            _currentNotification.OnNotificationComplete -= OnNotificationComplete;
            // _currentNotification.Cleanup();
            _onboardingQueue.Dequeue();
            Destroy(_currentNotification.gameObject);
            onboardingNotifications.Remove(_currentNotification);
            
            _currentNotification = null;
            if (_onboardingQueue.Count > 0)
            {
                ShowOnboardingNotification();
            }
            
        }
        
        private void InterruptOnboarding()
        {
            // if (_currentNotification != null && _onboardingQueue.Count > 1)
            if (_currentNotification != null && _onboardingQueue.Count > 0)
                OnNotificationComplete();
        }
        
        private void SetAnchorPosition(OnboardingNotificationRequest.AnchorPosition anchor, OnboardingNotification notification)
        {
            var rectTransform = notification.GetComponent<RectTransform>();
            
            switch (anchor)
            {
                case OnboardingNotificationRequest.AnchorPosition.Centre:
                    rectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.TopLeft:
                    rectTransform.anchoredPosition = new Vector2(0, 1);
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.TopRight:
                    rectTransform.anchoredPosition = new Vector2(1, 1);
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.BottomLeft:
                    rectTransform.anchoredPosition = new Vector2(0, 0);
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.BottomRight:
                    rectTransform.anchoredPosition = new Vector2(1, 0);
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.TopCentre:
                    rectTransform.anchoredPosition = new Vector2(0.5f, 1);
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.BottomCentre:
                    rectTransform.anchoredPosition = new Vector2(0.5f, 0);
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.LeftCentre:
                    rectTransform.anchoredPosition = new Vector2(0, 0.5f);
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    break;
                case OnboardingNotificationRequest.AnchorPosition.RightCentre:
                    rectTransform.anchoredPosition = new Vector2(1, 0.5f);
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    break;

            }
        }
    }
}


