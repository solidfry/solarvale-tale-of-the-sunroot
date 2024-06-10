using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MenuTabManager : MonoBehaviour
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

        private void OnValidate()
        {
            if (menuItems.Count == 0) return;
            foreach (var menuItem in menuItems)
            {
                if (menuItem.Button is null || menuItem.MenuPanel is null)
                {
                    Debug.LogError("Menu Item is missing a reference");
                    return;
                } 
                else if (menuItem.CanvasGroup == null)
                {
                    menuItem.SetCanvasGroup(menuItem.MenuPanel.GetComponent<CanvasGroup>());
                }
            }
        }

        private void Initialise()
        {
            foreach (var menuItem in menuItems)
                menuItem.Button.onClick.AddListener(() =>
                {
                    OpenMenu(menuItem.CanvasGroup);
                    _currentMenuIndex = menuItems.IndexOf(menuItem);
                });

            SetInitialTarget();
        }

        private void SetInitialTarget()
        {
            EventSystem.current.SetSelectedGameObject(menuItems[_currentMenuIndex].Button.gameObject);
        }

        void OpenMenu(CanvasGroup menuPanel)
        {
            menuPanel.gameObject.SetActive(true);
            FadeIn(menuPanel);
            foreach (var menuItem in menuItems)
                if (menuItem.CanvasGroup != menuPanel && menuItem.MenuPanel.activeInHierarchy)
                {
                    FadeOut(menuItem.CanvasGroup);
                }
        }

        void FadeOut(CanvasGroup canvasGroup) => canvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.gameObject.SetActive(false);
        }).SetUpdate(true);

        void FadeIn(CanvasGroup canvasGroup) =>
            canvasGroup.DOFade(1, fadeDuration).OnComplete(() => { canvasGroup.interactable = true; }).SetUpdate(true);
    }

    [Serializable]
    public record MenuItem
    {
        [field:SerializeField] public Button Button { get; private set; }
        [field:SerializeField] public GameObject MenuPanel { get; private set; }
        [field:SerializeField] public CanvasGroup CanvasGroup  { get; private set; }
        
        public void SetCanvasGroup(CanvasGroup canvasGroup) => CanvasGroup = canvasGroup;
    }
}