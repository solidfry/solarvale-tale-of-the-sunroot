using TMPro;
using UI.Utilities;
using UnityEngine;

namespace QuestSystem.UI
{
    [RequireComponent(typeof(DoTweenAnimationHandler))]
    public class QuestNotificationModal : MonoBehaviour
    {
        [SerializeField] TMP_Text questName;
        [SerializeField] QuestData questData;
        [SerializeField] DoTweenAnimationHandler doTweenAnimationHandler;
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
    
        private void Awake()
        {
            if (doTweenAnimationHandler is null) doTweenAnimationHandler = GetComponent<DoTweenAnimationHandler>();
            if (CanvasGroup is null) CanvasGroup = GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }

        private void Start()
        {
            SetQuestData(questData);
        }
    
        private void OnDisable()
        {
            ClearQuestName();
        }

        public void SetQuestName()
        {
            questName.text = questData.Title;
        }
    
        public void ClearQuestName()
        {
            questName.text = "";
        }
    
        public void SetQuestData(QuestData data)
        {
            if (data is null) return;
            questData = data;
            SetQuestName();
        }
    
        public void SetActive()
        {
            gameObject.SetActive(true);
        }
    
    
    
    }
}
