using System;
using Cinemachine;
using Entities;
using Interaction;
using QuestSystem;
using QuestSystem.Conditions;

namespace Events
{
    public static class GlobalEvents
    {
        #region Game Management
        public static Action<bool> OnGamePausedEvent;
        public static Action<bool> OnPlayerControlsLockedEvent;
        public static Action<bool> OnLockCursorEvent;
        public static Action<string> OnSetPlayerControlMapEvent;
        public static Action<bool> OnSetCursorInputForLookEvent;
        public static Action<IInteractable> OnInteractableFound;
        #endregion

        #region UI Management
        public static Action<bool> OnSetHUDVisibilityEvent;
        public static Action<bool> OnSetCameraHUDVisibilityEvent;
        #endregion
        
        #region Player Management
        public static Action<bool> OnSetCanInteractEvent;
        #endregion

        #region Dialogue
        public static Action OnDialogueStartEvent;
        public static Action OnDialogueCompleteEvent;
        public static Action<string> OnDialogueStartWithNodeEvent;
        public static Action<string> OnDialogueCompleteWithNodeEvent;
        #endregion
        
        #region Quests
        public static Action<QuestData> OnQuestCompletedEvent;
        public static Action<QuestData> OnQuestCompletedLogUpdatedEvent;
        public static Action<QuestData> OnQuestAcquiredEvent;
        public static Action<QuestData> OnQuestAcquiredLogUpdatedEvent;
        public static Action<QuestConditionBase> OnQuestConditionUpdatedEvent;
        public static Action<EntityData> OnPhotographConditionUpdatedEvent;
        #endregion
        
        #region Camera Management
        public static Action<CinemachineVirtualCamera> OnSetPriorityOfCameraEvent;
        #endregion

      
    }
}