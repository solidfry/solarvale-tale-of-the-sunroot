using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Compiler;

namespace QuestSystem.UI
{
    public class QuestNode : MonoBehaviour
    {
        [field: SerializeField] public QuestData QuestData { get; private set; }
        [SerializeField] TMP_Text questTitle;
        [SerializeField] TMP_Text questDescription;
        [SerializeField] Image questStatusIcon;
        [SerializeField] Sprite questCompleteIcon, questIncompleteIcon;
        [SerializeField] bool isCompleted;
        [SerializeField] Color completedColor;
        [SerializeField] Color defaultColor;
    
        [Header("UI Elements")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] LayoutElement layoutElement;
        [SerializeField] LayoutGroup layoutGroup;
    
        [Header("Animation Settings")]
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] private float colorAnimationTime = 0.5f;
        [SerializeField] private float heightAnimationTime = 0.5f;
        [SerializeField] float delayBeforeFade = 3f;
        
        float combinedHeight;
    
        List<TMP_Text> allTextElements;
        List<Graphic> colorChangeElements;
        
        public Observable<bool> isSafeToDestroy = new(false);

        private void Start()
        {
            if (rectTransform == null || layoutElement == null || layoutGroup == null)
            {
                rectTransform = GetComponent<RectTransform>();
                layoutElement = GetComponent<LayoutElement>();
                layoutGroup = GetComponent<LayoutGroup>();
            }
        
            allTextElements = GetComponentsInChildren<TMP_Text>().ToList();
            CompileColorElements();

            // if (QuestData != null)
            // {
            //     SetQuestText(QuestData);
            // }
        
        }

        private void Update()
        {
            if (combinedHeight != 0) return;
            if (CalculateCombinedHeight() == 0) return;
            combinedHeight = CalculateCombinedHeight();
        }

        private void CompileColorElements()
        {
            colorChangeElements = new();
            colorChangeElements.Add(questStatusIcon);
            colorChangeElements.AddRange(allTextElements);
        }

        public void SetQuestData(QuestData data)
        {
            QuestData = data;
            SetQuestText(data);
        }
    
        void SetQuestText(QuestData data)
        {
            questTitle.text = data.Title;
            questDescription.text = data.Description;
            // if the quest has conditions we want to show them also
            if (data.HasQuestConditions())
            {
                Debug.Log(data.GetQuestConditions().Count);
                foreach (var condition in data.GetQuestConditions())
                {
                    // I want to get the description TMP Text and duplicate it and replace the text in the duplicate with the condition text
                    Instantiate(questDescription, questDescription.transform.parent).text = condition.Title;
                }
            }
        }
    
        [ContextMenu("SetIsCompleted")]
        public void SetIsCompleted(bool completed)
        {
            isCompleted = completed;
        
            SetQuestStatusIcon();
            AnimateOut();
        }

        private float CalculateCombinedHeight()
        {
            var text = GetComponentsInChildren<TMP_Text>();
            var sum = text.Sum( t => t.rectTransform.rect.height);
            return sum + layoutGroup.padding.vertical;
            // return questTitle.rectTransform.rect.height + questDescription.rectTransform.rect.height + layoutGroup.padding.vertical;
        }
    
        public void SetQuestStatusIcon()
        {
            questStatusIcon.sprite = isCompleted ? questCompleteIcon : questIncompleteIcon;
        }
    
        [ContextMenu("AnimateOut")]
        public void AnimateOut()
        { 
            if (!isCompleted) return;
        
            for  (int i = 0; i < colorChangeElements.Count; i++)
            {
                // if the element is not the last element then continue
                if (i != colorChangeElements.Count - 1)
                {
                    colorChangeElements[i].DOColor(completedColor, colorAnimationTime).SetAutoKill(true);
                    continue;
                } 
                colorChangeElements[i].DOColor(completedColor, colorAnimationTime).SetAutoKill(true)
                    .OnComplete(
                        () =>
                        {
                            canvasGroup.DOFade(0, fadeDuration)
                                .SetDelay(delayBeforeFade);
                        
                            layoutElement.DOPreferredSize( new Vector2( layoutElement.preferredWidth, 0 ), heightAnimationTime )
                                .SetDelay(delayBeforeFade)
                                .OnComplete(() =>
                                {
                                    isSafeToDestroy.Value = true;
                                    gameObject.SetActive(false);
                                })
                                .SetAutoKill(true);
                        });
            }
        }
    
        public void AnimateIn()
        {
            canvasGroup.DOFade(1, fadeDuration).From(0).SetAutoKill(true);
            layoutElement.DOPreferredSize(new Vector2( layoutElement.preferredWidth, combinedHeight ), heightAnimationTime ).From( new Vector2( layoutElement.preferredWidth, layoutElement.preferredHeight ) )
                .OnComplete( () => layoutElement.preferredHeight = combinedHeight ).SetAutoKill(true);
        }
    
        // To animate in properly, we have to wait for the UI to update the text elements height because on Start they are zero.
        public void AnimateInDelayed()
        {
            StartCoroutine(AnimateInDelayedCoroutine());
        }
    
        IEnumerator AnimateInDelayedCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            AnimateIn();
        }
    
        public void ToggleActive() => gameObject.SetActive(!gameObject.activeSelf);

        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }
}
