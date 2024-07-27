using Events;
using UnityEngine;

namespace UI.Onboarding
{
    public class OnboardingRequestSender : MonoBehaviour
    {
        [Header("Onboarding Request will only be sent once")]
        bool onboardingRequestSent = false;
        [SerializeField] OnboardingNotificationRequest onboardingNotificationRequest;
        
        
        [ContextMenu("Send Onboarding Request")]
        public void SendOnboardingRequest()
        {
            if (onboardingRequestSent) return;
            
            GlobalEvents.OnOnboardingRequestEvent?.Invoke(onboardingNotificationRequest);
            Debug.Log("Onboarding request sent");
            
            onboardingRequestSent = true;
        }
        
        public void SendOnboardingRequestWithParams(Vector2 position, float width, float height, float sizeOffset, Color pulseColor, float duration = 1f)
        {
            if (onboardingRequestSent) return;
            
            var req = OnboardingNotificationRequest.Create(position, width, height, sizeOffset, pulseColor, duration);
            
            GlobalEvents.OnOnboardingRequestEvent?.Invoke(req);
            Debug.Log($"Onboarding request sent with params: {position}, {width}, {height} and {sizeOffset}");
            
            onboardingRequestSent = true;
        }
        
    }
}