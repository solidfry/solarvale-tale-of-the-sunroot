using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private List<QuestData> questList;
        [SerializeField] private List<QuestData> completedQuests;

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
            if (quest == null) return;
            quest.CompleteQuest();
            questList.Remove(quest);
            completedQuests.Add(quest);
            GlobalEvents.OnQuestCompletedLogUpdatedEvent?.Invoke(quest);
        }
        
        public List<QuestData> GetQuestList() => questList;
        public List<QuestData> GetCompletedQuests() => completedQuests;
    }

}