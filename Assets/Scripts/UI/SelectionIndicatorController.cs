using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class SelectedItemController : MonoBehaviour
    {
        [SerializeField] private RectTransform selectionIndicator;
        [SerializeField] private Transform itemsList;
    
        private CanvasGroup _selectionIndicatorCanvasGroup;
        private RectTransform _currentTarget;
        private EventSystem _eventSystem;

        [SerializeField] private float interpolationValue = 0.5f;

        private void Start()
        {
            _eventSystem = EventSystem.current;
            _selectionIndicatorCanvasGroup = selectionIndicator.GetComponent<CanvasGroup>();
            _selectionIndicatorCanvasGroup.alpha = 0;
        }

        private void Update()
        {
            if (!CanLerp()) return;
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
            return _currentTarget != null;
        }

        private void SetTarget()
        {
            if (_eventSystem.currentSelectedGameObject == null) return;

            RectTransform newTarget = _eventSystem.currentSelectedGameObject.transform as RectTransform;
            if (newTarget == null || newTarget == _currentTarget || newTarget.transform.parent != itemsList) return;

            _currentTarget = newTarget;
            // Debug.Log("Setting new target: " + _currentTarget.name);
        }

        private void UpdateTransform()
        {
            if (_currentTarget == null) return;
            
            selectionIndicator.position = new Vector2(
                Lerp(selectionIndicator.position.x, _currentTarget.position.x, interpolationValue * Time.unscaledDeltaTime), 
                selectionIndicator.position.y);
            
            selectionIndicator.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,  
                Lerp(selectionIndicator.rect.width, _currentTarget.rect.width, interpolationValue * Time.unscaledDeltaTime));
        }

        private float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);
    }
}
