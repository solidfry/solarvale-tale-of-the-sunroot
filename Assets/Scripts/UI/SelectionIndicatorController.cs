using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class SelectedItemController : MonoBehaviour
    {
        [SerializeField] RectTransform selectionIndicator;
        [SerializeField] EventSystem eventSystem;
        [SerializeField] Transform itemsList;
    
        CanvasGroup _selectionIndicatorCanvasGroup;

        [SerializeField] RectTransform currentTarget;

        [SerializeField] private float interpolationValue = 0.5f;
    
        private void Start()
        {
            eventSystem = EventSystem.current;
            _selectionIndicatorCanvasGroup = selectionIndicator.GetComponent<CanvasGroup>();
            _selectionIndicatorCanvasGroup.alpha = 0;
        }

        private void Update()
        {
            if (CanLerp()) return;
            UpdateSelectorAlpha();
            UpdateTransform();
        }
        
        private void UpdateSelectorAlpha()
        {
            if (_selectionIndicatorCanvasGroup.alpha < 1)
            {
                _selectionIndicatorCanvasGroup.alpha = Lerp(_selectionIndicatorCanvasGroup.alpha, 1, interpolationValue);
            }
        }

        private bool CanLerp()
        {
            SetTarget();
            return currentTarget == null;
        }

        private void SetTarget()
        {
            if (eventSystem.currentSelectedGameObject is null) return;
            if (eventSystem.currentSelectedGameObject.transform == currentTarget || eventSystem.currentSelectedGameObject.transform.parent != itemsList) return;
            if (eventSystem?.currentSelectedGameObject == null) return;
            currentTarget = eventSystem?.currentSelectedGameObject.transform as RectTransform;
            // Debug.Log("Setting new target" + currentTarget.name);
        }

        private void UpdateTransform()
        {
            
            selectionIndicator.position = 
                new Vector2(
                    Lerp(selectionIndicator.position.x, 
                        currentTarget.position.x, 
                        interpolationValue * Time.unscaledDeltaTime), 
                    selectionIndicator.position.y);
            
            selectionIndicator.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal,  
                Lerp(selectionIndicator.rect.width, 
                    currentTarget.rect.width, 
                    interpolationValue * Time.unscaledDeltaTime));
        }

        private float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);
    }
}
