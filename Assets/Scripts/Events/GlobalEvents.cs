using System;
using Interaction;

namespace Events
{
    public static class GlobalEvents
    {
        public static Action<bool> OnGamePausedEvent;
        public static Action<bool> OnPlayerControlsLockedEvent;
        public static Action<bool> OnLockCursorEvent;
        public static Action<string> OnSetPlayerControlMapEvent;
        public static Action OnDialogueStartEvent;
        public static Action OnDialogueCompleteEvent;
        public static Action<bool> OnSetCursorInputForLookEvent;
        public static Action<IInteract> OnInteractableFound;
        public static Action OnInteractableUIEvent;

        
    }
}