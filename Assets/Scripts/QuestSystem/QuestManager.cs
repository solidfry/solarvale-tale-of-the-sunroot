using System.Collections.Generic;
using System.Linq;
using Entities;
using Events;
using QuestSystem.Conditions;
using UnityEngine;

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
            GlobalEvents.OnPhotographConditionUpdatedEvent += UpdatePhotographyQuests;
        }
        

        private void OnDisable()
        {
            GlobalEvents.OnQuestCompletedEvent -= CompleteQuest;
            GlobalEvents.OnQuestAcquiredEvent -= AddQuest;
            GlobalEvents.OnQuestConditionUpdatedEvent -= UpdateQuestConditions;
            GlobalEvents.OnPhotographConditionUpdatedEvent -= UpdatePhotographyQuests;
        }
        
        void AddQuest(QuestData questData)
        {
            if (questList.Contains(questData) || completedQuests.Contains(questData))
            {
                return;
            }
            
            questList.Add(questData);
            questData.InitialiseQuest();
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent?.Invoke(questData);
        }
        
        private void UpdatePhotographyQuests(EntityData entity = null)
        {
            var tempList = questList
                .FindAll(x => x.GetQuestConditions()
                    .Any(y => y is QuestConditionPhotograph))
                .ToList();

            if (tempList.Count == 0)
            {
                Debug.Log("No Photography Quests so returning early");
                return;
            }
            
            foreach (var photoQuest in tempList)
            {
                foreach (var condition in photoQuest.GetQuestConditions())
                {
                    if (condition is not QuestConditionPhotograph photoCondition) continue;
                    if (photoCondition.GetEntityData() != entity && photoCondition.GetEntityData() != null && photoCondition.GetEntityType() != EntityType.None) continue;
                    
                    photoCondition.UpdateCondition();
                    
                    if (!photoCondition.IsConditionComplete()) continue;
                    
                    GlobalEvents.OnQuestConditionUpdatedEvent?.Invoke(photoCondition);
                }
            }
        }
        
        private void UpdateQuestConditions(QuestConditionBase condition)
        {
            var tempList = questList.ToList();
            
            foreach (var quest in tempList)
            {
                foreach (var questCondition in quest.GetQuestConditions())
                {
                    if (questCondition != condition || condition.IsConditionComplete()) continue;
                    
                    condition.UpdateCondition();
                        
                    if (!quest.IsAllQuestConditionsComplete()) continue;
                    GlobalEvents.OnQuestCompletedEvent?.Invoke(quest);
                }
            }
            tempList.Clear();
        }

        void CompleteQuest(QuestData questData)
        {
            QuestData quest = questList.Find(q => q == questData);
            if (quest is null) return;
            quest.CompleteQuest();
            questList.Remove(quest);
            completedQuests.Add(quest);
            
            AddNextQuest(quest);
            
            GlobalEvents.OnQuestCompletedLogUpdatedEvent?.Invoke(quest);
        }

        void AddNextQuest(QuestData quest)
        {
            if (quest.GetNextQuest() is null) return;
            AddQuest(quest.GetNextQuest());
        }

        public List<QuestData> GetQuestList() => questList;
        public List<QuestData> GetCompletedQuests() => completedQuests;
    }

}