using System.Collections;
using DG.Tweening;
using Entities;
using Events;
using Progression.UI;
using UnityEngine;

namespace Progression
{
    public class DiscoveryNotificationManager : MonoBehaviour
    {
        [SerializeField] private RectTransform discoveryNotificationUICanvas;
        [SerializeField] private DiscoveryModal discoveryModalPrefab;
        [SerializeField] float notificationDuration = 3f;
        [SerializeField] float notificationFadeDuration = 2f;
        private DiscoveryModal _discoveryModalInstance;

        private void Awake()
        {
            InitialiseDiscoveryNotificationModal();
        }
    
        private void OnEnable()
        {
            GlobalEvents.OnNewEntityDiscovered += ShowDiscoveryNotification;
        }
    
        private void OnDisable()
        {
            GlobalEvents.OnNewEntityDiscovered -= ShowDiscoveryNotification;
        }

        private void InitialiseDiscoveryNotificationModal()
        {
            if (discoveryModalPrefab is null) return;
            _discoveryModalInstance = Instantiate(discoveryModalPrefab, discoveryNotificationUICanvas);
        }

        private void ShowDiscoveryNotification(EntityData questData)
        {
            if (_discoveryModalInstance is null) return;
            _discoveryModalInstance.SetData(questData);
            _discoveryModalInstance.SetActive();
            
            PlayHideDiscoveryNotification();
        }
        
        void PlayHideDiscoveryNotification() => StartCoroutine(HideDiscoveryNotification());
        
        IEnumerator HideDiscoveryNotification()
        {
            yield return new WaitForSeconds(notificationDuration);
            _discoveryModalInstance.CanvasGroup.DOFade(0, notificationFadeDuration).OnComplete(() => _discoveryModalInstance.gameObject.SetActive(false));
        }
    }
}
