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
        
        [SerializeField] List<OnboardingNotification> _onboardingNotifications = new();
        
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
            _onboardingNotifications.Add(_currentNotification);
            _currentNotification.Initialise(notification);
            _currentNotification.OnNotificationComplete += OnNotificationComplete;
        }

        private void OnNotificationComplete()
        {
            _currentNotification.OnNotificationComplete -= OnNotificationComplete;
            _onboardingQueue.Dequeue();
            Destroy(_currentNotification.gameObject);
            _onboardingNotifications.Remove(_currentNotification);
            
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
    }
}


