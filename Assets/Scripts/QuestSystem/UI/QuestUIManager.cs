using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] RectTransform questUICanvas;
        [SerializeField] RectTransform questLogParent;

        [Header("Quest Notifications")]
        [SerializeField] private QuestNotificationModal questNotificationModal;
        [SerializeField] float notificationDuration = 3f;
        [SerializeField] float notificationFadeDuration = 2f;
        private QuestNotificationModal _questNotificationInstance;
        private void Start()
        {
            InitialiseQuestLog();
            InitialiseQuestNotificationModal();
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
            DestroyQuestNotification();
        }

        private void DestroyQuestNotification()
        {
            if (_questNotificationInstance is null) return;
            // _questNotificationInstance.IntroAnimationCompleted -= PlayHideQuestNotification;
            Destroy(_questNotificationInstance.gameObject);
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
        
        private void InitialiseQuestNotificationModal()
        {
            _questNotificationInstance = Instantiate(questNotificationModal, questUICanvas);
        }
        
        public void UpdateQuestLog(QuestData questData)
        {
            if (questData is null) return;
            if (questNodes.Find(questNode => questNode.QuestData == questData))
            {
                // Update the quest node because it already exists
                QuestNode q = questNodes.Find(questNode => questNode.QuestData == questData);
                q.SetQuestData(questData);
                return;
            }
            
            // Add the quest node because it doesn't exist
            AddQuestNodeElement(questData);
            ShowQuestNotification(questData);
        }

        private void ShowQuestNotification(QuestData questData)
        {
            _questNotificationInstance.SetQuestData(questData);
            _questNotificationInstance.SetActive();
            
            PlayHideQuestNotification();
        }
        
        void PlayHideQuestNotification() => StartCoroutine(HideQuestNotification());
        
        IEnumerator HideQuestNotification()
        {
            yield return new WaitForSeconds(notificationDuration);
            _questNotificationInstance.CanvasGroup.DOFade(0, notificationFadeDuration).OnComplete(() => _questNotificationInstance.gameObject.SetActive(false));
        }

        public void AddQuestNodeElement(QuestData questData)
        {
            QuestNode questNode = Instantiate(prefab, questLogParent);
            questNode.SetQuestData(questData);
            questNodes.Add(questNode);
            questNode.AnimateInDelayed();
        }
        
        void CompleteQuest(QuestData questData)
        {
            QuestNode q = questNodes.Find(q => q.QuestData == questData);
            q.isSafeToDestroy.ValueChanged += RemoveQuestNodeElement;
            q.SetIsCompleted(true);
        }
        
        void RemoveQuestNodeElement(bool isSafeToDestroy)
        {
            if (isSafeToDestroy)
            {
                questNodes.FindAll(q => q.isSafeToDestroy.Value).ForEach(q =>
                {
                    questNodes.Remove(q);
                    Destroy(q.gameObject);
                });
            }
        }
    }
}
