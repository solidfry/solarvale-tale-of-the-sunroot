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
        [SerializeField] bool startAllMenusClosed = false;
        [SerializeField] 
        private List<MenuItem> menuItems;
        private int _currentMenuIndex;
        
        Tween _fadeInTween;
        Tween _fadeOutTween;

        private void OnEnable() => Initialise();

        private void OnValidate()
        {
            if (menuItems.Count == 0) return;
            foreach (var menuItem in menuItems)
            {
                if (menuItem.ButtonToActivatePanel is null || menuItem.MenuPanel is null)
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
                menuItem.ButtonToActivatePanel.onClick.AddListener(() =>
                {
                    OpenMenu(menuItem.CanvasGroup);
                    _currentMenuIndex = menuItems.IndexOf(menuItem);
                });
            
            if (startAllMenusClosed)
            {
                CloseAllMenus();
            }
            else
            {
                CloseAllMenus();
                OpenMenu(0);
            }
        }
        
        public void OpenMenu(int index)
        {
            if (index < 0 || index >= menuItems.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }
            
            OpenMenu(menuItems[index].CanvasGroup);
            _currentMenuIndex = index;
            SetInitialTarget();
        }

        public void CloseAllMenus()
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.CanvasGroup.alpha = 0;
                menuItem.CanvasGroup.interactable = false;
                menuItem.CanvasGroup.blocksRaycasts = false;
                menuItem.CanvasGroup.gameObject.SetActive(false);
            }
        }

        private void SetInitialTarget()
        {
            if (menuItems[_currentMenuIndex].InitialFocusObject is null)
            {
                EventSystem.current.SetSelectedGameObject(menuItems[_currentMenuIndex].ButtonToActivatePanel.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(menuItems[_currentMenuIndex].InitialFocusObject);
            }
        }

        void OpenMenu(CanvasGroup menuPanel)
        {
            menuPanel.gameObject.SetActive(true);
            menuPanel.alpha = 0;
            FadeIn(menuPanel);
            foreach (var menuItem in menuItems)
                if (menuItem.CanvasGroup != menuPanel)
                {
                    FadeOut(menuItem.CanvasGroup);
                }
        }

        void FadeOut(CanvasGroup canvasGroup)
        {
            // Debug.Log($"Fading out: {canvasGroup.gameObject.name}");
            if (_fadeOutTween != null && _fadeOutTween.IsActive())
                _fadeOutTween.Kill();
            _fadeOutTween = canvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.gameObject.SetActive(false);
                // Debug.Log("Fade out complete");
            }).SetUpdate(isIndependentUpdate: true);
        }

        void FadeIn(CanvasGroup canvasGroup)
        {
            Debug.Log($"Fading in: {canvasGroup.gameObject.name}");
            if (_fadeInTween != null && _fadeInTween.IsActive())
                _fadeInTween.Kill();
            _fadeInTween = canvasGroup.DOFade(1, fadeDuration).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                // Debug.Log("Fade in complete");
                SetInitialTarget();
                if (canvasGroup.alpha >= 1) return;
                canvasGroup.alpha = 1;
            }).SetUpdate(isIndependentUpdate: true);
        }
    }

    [Serializable]
    public record MenuItem
    {
        [field:SerializeField] public Button ButtonToActivatePanel { get; private set; }
        [field:SerializeField] public GameObject InitialFocusObject { get; private set; }
        [field:SerializeField] public GameObject MenuPanel { get; private set; }
        [field:SerializeField] public CanvasGroup CanvasGroup  { get; private set; }
        
        public void SetCanvasGroup(CanvasGroup canvasGroup) => CanvasGroup = canvasGroup;
    }
}