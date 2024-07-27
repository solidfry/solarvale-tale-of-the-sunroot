using System.Collections.Generic;
using Events;
using UnityEngine;

namespace UI.Onboarding
{
    public class OnboardingNotificationManager : MonoBehaviour
    {
        [SerializeField] Canvas onboardingCanvas;
        [SerializeField] OnboardingNotification onboardingNotificationPrefab;
        readonly Queue<OnboardingNotificationRequest> _onboardingQueue = new();
        
        OnboardingNotification _currentNotification;
        
        private void OnEnable()
        {
            GlobalEvents.OnOnboardingRequestEvent += OnOnboardingRequest;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnOnboardingRequestEvent -= OnOnboardingRequest;
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
            _currentNotification.Initialise(notification);
            _currentNotification.OnNotificationComplete += OnNotificationComplete;
        }

        private void OnNotificationComplete()
        {
            _currentNotification.OnNotificationComplete -= OnNotificationComplete;
            Destroy(_currentNotification.gameObject);
            _onboardingQueue.Dequeue();
            if (_onboardingQueue.Count > 0)
            {
                ShowOnboardingNotification();
            }
        }
    }
}


