using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Events;

namespace CameraSystem
{
    [Serializable]
    public class PhotographyHUDController
    {
        [SerializeField] Canvas hudCanvas;
        CanvasGroup _hudCanvasGroup;
        
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] float fadeDelay = 1f;
        
        public Tween FadeTween { get; private set; } = null;
        
        public void OnEnable()
        {
            _hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
            SetHUDVisibility(0, 0);
            GlobalEvents.OnSetCameraHUDVisibilityEvent += SetHUDVisibility;
        }
        
        public void OnDisable()
        {
            GlobalEvents.OnSetCameraHUDVisibilityEvent -= SetHUDVisibility;
        }
        
        public void SetHUDVisibility(bool isVisible)
        {
            FadeTween = _hudCanvasGroup.DOFade(isVisible ? 1 : 0, fadeDuration).SetDelay(isVisible ? fadeDelay : 0);
        }
        
        public void SetHUDVisibility(float alpha, float duration = 0.5f, float delay = 0f)
        {
            FadeTween = _hudCanvasGroup.DOFade(alpha, duration).SetDelay(delay);
        }
        
        public void ToggleCanvas(bool value)
        {
            hudCanvas.enabled = value;
        }
        
    }
}