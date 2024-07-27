using Events;
using UnityEngine;

namespace UI.Onboarding
{
    public class OnboardingRequestSender : MonoBehaviour
    {
        [SerializeField]
        OnboardingNotificationRequest onboardingNotificationRequest;
        public void SendOnboardingRequest()
        {
            GlobalEvents.OnOnboardingRequestEvent?.Invoke(onboardingNotificationRequest);
            Debug.Log("Onboarding request sent");
        }
        
        public void SendOnboardingRequestWithParams(Vector2 position, float width, float height)
        {
            var req = new OnboardingNotificationRequest(position, width, height);
            
            GlobalEvents.OnOnboardingRequestEvent?.Invoke(req);
            Debug.Log($"Onboarding request sent with params: {position}, {width}, {height}");
        }
        
    }
}