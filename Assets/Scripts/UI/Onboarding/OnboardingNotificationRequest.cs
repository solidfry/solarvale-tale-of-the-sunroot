
using System;
using UnityEngine;

namespace UI.Onboarding
{
    [Serializable]
    public struct OnboardingNotificationRequest
    {
        public Vector2 Position { get; }
        public float Width { get; }
        public float Height { get; }
        
        public OnboardingNotificationRequest(Vector2 position, float width, float height)
        {
            Position = position;
            Width = width;
            Height = height;
        }
    }
}