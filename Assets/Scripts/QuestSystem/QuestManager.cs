using System.Collections.Generic;
using System.Linq;
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
            GlobalEvents.OnQuestConditionUpdatedEvent += UpdateQuestConditions;
        }

        private void OnDisable()
        {
            GlobalEvents.OnQuestCompletedEvent -= CompleteQuest;
            GlobalEvents.OnQuestAcquiredEvent -= AddQuest;
            GlobalEvents.OnQuestConditionUpdatedEvent -= UpdateQuestConditions;
        }
        
        void AddQuest(QuestData questData)
        {
            if (questList.Contains(questData) || completedQuests.Contains(questData))
            {
                return;
            }
            
            questList.Add(questData);
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent?.Invoke(questData);
        }
        
        private void UpdateQuestConditions(QuestConditionBase condition)
        {
            var tempList = questList.ToList();
            foreach (var quest in tempList)
            {
                foreach (var questCondition in quest.GetQuestConditions())
                {
                    if (questCondition == condition && !condition.IsConditionComplete())
                    {
                        condition.UpdateCondition();
                        if (quest.IsAllQuestConditionsComplete())
                        {
                            GlobalEvents.OnQuestCompletedEvent?.Invoke(quest);
                        }
                    }
                }
            }
            tempList.Clear();
        }

        public void CompleteQuest(QuestData questData)
        {
            QuestData quest = questList.Find(q => q == questData);
            if (quest is null) return;
            quest.CompleteQuest();
            questList.Remove(quest);
            completedQuests.Add(quest);
            
            AddNextQuest(quest);
            
            GlobalEvents.OnQuestCompletedLogUpdatedEvent?.Invoke(quest);
        }

        private void AddNextQuest(QuestData quest)
        {
            if (quest.GetNextQuest() is null) return;
            AddQuest(quest.GetNextQuest());
        }

        public List<QuestData> GetQuestList() => questList;
        public List<QuestData> GetCompletedQuests() => completedQuests;
    }

}