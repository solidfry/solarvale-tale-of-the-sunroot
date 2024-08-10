
using System;
using UnityEngine;

namespace UI.Onboarding
{
   
    [Serializable]
    public struct OnboardingNotificationRequest
    {
        public enum AnchorPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Centre,
            TopCentre,
            BottomCentre,
            LeftCentre,
            RightCentre
        }

        [field: SerializeField] public Vector2 Position { get; private set; }
        [field: SerializeField] public AnchorPosition Anchor { get; private set; }
        [field: SerializeField] public float Width { get; private set;  }
        [field: SerializeField] public float Height { get; private set;  }
        [field: SerializeField] [Tooltip("This controls the size of the object when it spawns and how it animates")] 
        public float SizeOffset { get; private set;  }
        [field: SerializeField, ColorUsage(true, true)] 
        public Color PulseColor { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        
        public static OnboardingNotificationRequest Create(Vector2 position, float width, float height, float sizeOffset, Color pulseColor, float duration = 1f, AnchorPosition anchor = AnchorPosition.Centre)
        {
            return new OnboardingNotificationRequest
            {
                Position = position,
                Width = width,
                Height = height,
                SizeOffset = sizeOffset,
                PulseColor = pulseColor,
                Duration = duration,
                Anchor = anchor
            };
        }
        
        
        
    }
}