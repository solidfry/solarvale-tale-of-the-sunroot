using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private List<QuestData> questList;

        private void OnEnable()
        {
            GlobalEvents.OnQuestCompletedEvent += CompleteQuest;
            GlobalEvents.OnQuestAcquiredEvent += AddQuest;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnQuestCompletedEvent -= CompleteQuest;
            GlobalEvents.OnQuestAcquiredEvent -= AddQuest;
        }
        
        void AddQuest(QuestData questData)
        {
            if (questList.Contains(questData))
            {
                return;
            }
            questList.Add(questData);
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent?.Invoke(questData);
        }

        public void CompleteQuest(QuestData questData)
        {
            QuestData quest = questList.Find(q => q == questData);
            // Debug.Log("Attempting to complete quest: " + quest.Title);
            if (quest != null)
            {
                quest.CompleteQuest();
                GlobalEvents.OnQuestCompletedLogUpdatedEvent?.Invoke(quest);
            }
            else
            {
                foreach (var q in questList)
                {
                    Debug.LogWarning(" - " + q.Title);
                }
            }
        }
        
        public List<QuestData> GetQuestList() => questList;
    }

}