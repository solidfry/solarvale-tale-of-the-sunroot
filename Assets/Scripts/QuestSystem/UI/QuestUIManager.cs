using System.Collections.Generic;
using Events;
using UnityEngine;

namespace QuestSystem.UI
{
    public class QuestUIManager : MonoBehaviour
    {
        [SerializeField] private QuestNode prefab;
        [SerializeField] QuestManager questManager;
        [SerializeField] List<QuestNode> questNodes;

        [Header("UI Elements")] 
        [SerializeField]  RectTransform questLogParent;

        private void Start()
        {
            InitialiseQuestLog();
        }

        private void OnEnable()
        {
            GlobalEvents.OnQuestCompletedLogUpdatedEvent += CheckQuestsCompleted;
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent += UpdateQuestLog;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnQuestCompletedLogUpdatedEvent -= CheckQuestsCompleted;
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent -= UpdateQuestLog;
        }

        private void CheckQuestsCompleted(QuestData questData)
        {
            if (!questData.CheckQuestCompleted()) return;
            CompleteQuest(questData);
        }
        
        void InitialiseQuestLog()
        {
            foreach (var quest in questManager.GetQuestList())
            {
                AddQuestNodeElement(quest);
            }
        }
        
        public void UpdateQuestLog(QuestData questData)
        {
            if (questData is null) return;
            if (questNodes.Find(questNode => questNode.QuestData == questData))
            {
                QuestNode q = questNodes.Find(questNode => questNode.QuestData == questData);
                q.SetQuestData(questData);
                return;
            }
            
            AddQuestNodeElement(questData);
        }
        
        public void AddQuestNodeElement(QuestData questData)
        {
            QuestNode questNode = Instantiate(prefab, questLogParent);
            questNode.SetQuestData(questData);
            questNodes.Add(questNode);
            questNode.AnimateInDelayed();
        }
        
        void CompleteQuest(QuestData questData) => questNodes.Find(q => q.QuestData == questData).SetIsCompleted(true);
    }
}
