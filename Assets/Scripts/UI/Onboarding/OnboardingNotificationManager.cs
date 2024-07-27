using System.Collections.Generic;
using Events;
using UnityEngine;

namespace UI.Onboarding
{
    public class OnboardingNotificationManager : MonoBehaviour
    {
        [SerializeField] Canvas onboardingCanvas;
        [SerializeField] RectTransform onboardingNotificationPrefab;
        Queue<OnboardingNotificationRequest> onboardingQueue = new();
        
        
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
            onboardingQueue.Enqueue(req);
            if (onboardingQueue.Count == 1)
            {
                ShowOnboardingNotification();
            }
        }

        private void ShowOnboardingNotification()
        {
            var notification = onboardingQueue.Peek();
            var onboardingNotification = Instantiate(onboardingNotificationPrefab, onboardingCanvas.transform);
            onboardingNotification.anchoredPosition = notification.Position;
            onboardingNotification.sizeDelta = new Vector2(notification.Width, notification.Height);
        }
    }
}


