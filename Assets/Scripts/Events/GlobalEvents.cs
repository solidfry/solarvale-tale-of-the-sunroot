using System;
using Interaction;
using QuestSystem;

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
        public static Action<IInteractable> OnInteractableFound;
        public static Action<QuestData> OnQuestCompletedEvent;
        public static Action<QuestData> OnQuestCompletedLogUpdatedEvent;
        public static Action<QuestData> OnQuestAcquiredEvent;
        public static Action<QuestData> OnQuestAcquiredLogUpdatedEvent;
    }
}