using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class UIOverlayController : MonoBehaviour
    {
        CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ToggleFade(float duration = 0)
        {
            if (_canvasGroup is not null) _canvasGroup.DOFade(_canvasGroup.alpha == 0 ? 1 : 0, duration).SetUpdate(true);

            SetInteractable();
        }

        private void SetInteractable()
        {
            if (_canvasGroup.alpha == 0)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
            else
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }
        
    }
}
