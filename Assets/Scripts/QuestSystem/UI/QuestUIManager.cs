using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestSystem.UI
{
    public class QuestUIManager : MonoBehaviour
    {
        [SerializeField] private QuestNode prefab;
        [SerializeField] QuestManager questManager;
        [SerializeField] List<QuestNode> questNodes;

        [Header("UI Elements")] 
        [SerializeField] RectTransform questUICanvas;
        [SerializeField] CanvasGroup questUICanvasGroup;
        [SerializeField] RectTransform questLogParent;
        [SerializeField] CanvasGroup headerCanvasGroup;

        [FormerlySerializedAs("notificationModal")]
        [FormerlySerializedAs("questNotificationModal")]
        [Header("Quest Notifications")]
        [SerializeField] private QuestModal questModal;
        [SerializeField] float notificationDuration = 3f;
        [SerializeField] float notificationFadeDuration = 2f;
        [SerializeField] float headerFadeDuration = 0.5f;
        private QuestModal _questInstance;
        
        Tween _fadeTween;
        
        private void Start()
        {
            InitialiseQuestLog();
            InitialiseQuestNotificationModal();
            // If the quest log is empty then hide the header canvas group
            ToggleHeader(0);
        }

        private void OnEnable()
        {
            GlobalEvents.OnQuestCompletedLogUpdatedEvent += CheckQuestsCompleted;
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent += UpdateQuestLog;
            GlobalEvents.OnSetHUDVisibilityEvent += ToggleQuestUI;
        }

        private void ToggleQuestUI(bool value)
        {
            if (questUICanvasGroup is null) return;
            if (_fadeTween != null) _fadeTween.Kill();
            _fadeTween = questUICanvasGroup.DOFade(value ? 1 : 0, 0.5f);
        }

        private void OnDisable()
        {
            GlobalEvents.OnQuestCompletedLogUpdatedEvent -= CheckQuestsCompleted;
            GlobalEvents.OnQuestAcquiredLogUpdatedEvent -= UpdateQuestLog;
            GlobalEvents.OnSetHUDVisibilityEvent -= ToggleQuestUI;
            DestroyQuestNotification();
        }

        private void DestroyQuestNotification()
        {
            if (_questInstance is null) return;
            Destroy(_questInstance.gameObject);
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
                // Update the quest node because it already exists
                QuestNode q = questNodes.Find(questNode => questNode.QuestData == questData);
                q.SetQuestData(questData);
                return;
            }
            
            ToggleHeader(headerFadeDuration);
            // Add the quest node because it doesn't exist
            AddQuestNodeElement(questData);
            ShowQuestNotification(questData);
        }
        
        private void InitialiseQuestNotificationModal()
        {
            if (questModal is null) return;
            _questInstance = Instantiate(questModal, questUICanvas);
        }

        private void ShowQuestNotification(QuestData questData)
        {
            if (_questInstance is null) return;
            _questInstance.SetData(questData);
            _questInstance.SetActive();
            
            PlayHideQuestNotification();
        }
        
        void PlayHideQuestNotification() => StartCoroutine(HideQuestNotification());
        
        IEnumerator HideQuestNotification()
        {
            yield return new WaitForSeconds(notificationDuration);
            _questInstance.CanvasGroup.DOFade(0, notificationFadeDuration).OnComplete(() => _questInstance.gameObject.SetActive(false));
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
                    //TODO: pooling would be a good idea at some point but this one is low risk
                    Destroy(q.gameObject);
                });
            }
        }
        
        // if there is no quest nodes then do not show the header canvas group.
        // We need a check here.
        
        bool IsQuestLogEmpty() => questNodes.Count == 0;
        
        void ToggleHeader(float duration = 0.5f)
        {
            if (headerCanvasGroup is null) return;
            headerCanvasGroup.DOFade(headerCanvasGroup.alpha > 0 && IsQuestLogEmpty() ? 0 : 1, duration);
        }
    }
}
