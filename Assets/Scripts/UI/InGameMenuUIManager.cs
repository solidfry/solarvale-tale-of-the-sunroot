using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenuUIManager : MonoBehaviour
    {
        
        [Header("Animation")]
        [SerializeField] 
        private float fadeDuration = 0.5f;
        
        [Header("Menu Items")]
        [SerializeField] 
        private List<MenuItem> menuItems;
        private int _currentMenuIndex;
        
        private void Start()
        {
            Initialise();
            // Open the first menu by default
            OpenMenu(menuItems[_currentMenuIndex].CanvasGroup);
        }

        private void Initialise()
        {
            foreach (var menuItem in menuItems)
                menuItem.Button.onClick.AddListener(() =>
                {
                    OpenMenu(menuItem.CanvasGroup);
                    _currentMenuIndex = menuItems.IndexOf(menuItem);
                });
        }

        void OpenMenu(CanvasGroup menuPanel)
        {
            menuPanel.gameObject.SetActive(true);
            FadeIn(menuPanel);
            foreach (var menuItem in menuItems)
                if (menuItem.CanvasGroup != menuPanel)
                {
                    menuItem.MenuPanel.SetActive(false);
                    FadeOut(menuItem.CanvasGroup);
                }
        }
        
        void FadeOut(CanvasGroup canvasGroup) => canvasGroup.DOFade(0, fadeDuration);
        void FadeIn(CanvasGroup canvasGroup) => canvasGroup.DOFade(1, fadeDuration);
    }

    [Serializable]
    public record MenuItem
    {
        [field:SerializeField] public Button Button { get; private set; }
        [field:SerializeField] public GameObject MenuPanel { get; private set; }
        [field:SerializeField] public CanvasGroup CanvasGroup  { get; private set; }
    }
}