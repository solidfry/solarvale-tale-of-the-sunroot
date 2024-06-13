using Events;
using UnityEngine;

namespace QuestSystem
{
    public enum QuestAction
    {
        Complete,
        Add
    }
    
    public class QuestUpdater : MonoBehaviour
    {
        [SerializeField] QuestData questData;
        [SerializeField] QuestAction questAction = QuestAction.Complete;
        
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
                default:    
                    Debug.LogWarning("Quest action not found: " + questAction);
                    break;
            }
        }
    }
}