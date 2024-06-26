using System;
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
        
        public void OnEnable()
        {
            _hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
            GlobalEvents.OnSetCameraHUDVisibilityEvent += SetHUDVisibility;
        }
        
        public void OnDisable()
        {
            GlobalEvents.OnSetCameraHUDVisibilityEvent -= SetHUDVisibility;
        }
        
        public void SetHUDVisibility(bool isVisible)
        {
            _hudCanvasGroup.DOFade(isVisible ? 1 : 0, fadeDuration);
        }
        
        public void SetHUDVisibility(float alpha, float duration = 0.5f)
        {
            _hudCanvasGroup.DOFade(alpha, duration);
        }
    }
}