using System.ComponentModel;
using Events;
using ExternPropertyAttributes;
using QuestSystem.Conditions;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestSystem
{
    public enum QuestAction
    {
        Complete,
        Add,
        UpdateCondition
    }
    
    public class QuestUpdater : MonoBehaviour
    {
        [FormerlySerializedAs("QuestAction")] public QuestAction questAction = QuestAction.Complete;
        [HideInInspector] public QuestData questData;
        [Header("Quest Condition")]
        [Tooltip("The condition to update. This will only update the condition below if the Quest Action is set to Update.")]
        [HideInInspector] public QuestConditionBase questCondition;
        
        public void UpdateQuest()
        {
            switch (questAction)
            {
                case QuestAction.Complete:
                    GlobalEvents.OnQuestCompletedEvent?.Invoke(questData);
                    break;
                case QuestAction.Add:
                    GlobalEvents.OnQuestAcquiredEvent?.Invoke(questData);
                    break;
                case QuestAction.UpdateCondition:
                    GlobalEvents.OnQuestConditionUpdatedEvent?.Invoke(questCondition);
                    break;
                
                default:    
                    Debug.LogWarning("Quest action not found: " + questAction);
                    break;
            }
        }
    }
}