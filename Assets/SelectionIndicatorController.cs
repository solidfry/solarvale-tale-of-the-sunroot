using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class SelectedItemController : MonoBehaviour
{
    [SerializeField] RectTransform selectionIndicator;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Transform itemsList;
    
    CanvasGroup selectionIndicatorCanvasGroup;

    [SerializeField] RectTransform currentTarget;

    [FormerlySerializedAs("animationSpeed")] [SerializeField] private float interpolationValue = 0.5f;
    
    private void Start()
    {
        eventSystem = EventSystem.current;
        selectionIndicatorCanvasGroup = selectionIndicator.GetComponent<CanvasGroup>();
        selectionIndicatorCanvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (CanLerp()) return;
        UpdateSelectorAlpha();
        UpdateTransform();
    }

    private void UpdateSelectorAlpha()
    {
        if (selectionIndicatorCanvasGroup.alpha < 1)
        {
            selectionIndicatorCanvasGroup.alpha = Lerp(selectionIndicatorCanvasGroup.alpha, 1, interpolationValue);
        }
    }

    private bool CanLerp()
    {
        SetTarget();
        if (currentTarget == null) return true;
        return false;
    }

    private void SetTarget()
    {
        if (currentTarget == eventSystem.currentSelectedGameObject?.transform || eventSystem.currentSelectedGameObject?.transform.parent != itemsList ) return;
        if (eventSystem?.currentSelectedGameObject == null) return;
        currentTarget = eventSystem?.currentSelectedGameObject.GetComponent<RectTransform>();
        Debug.Log("Setting new target" + currentTarget.name);
    }

    private void UpdateTransform()
    {
        selectionIndicator.position = 
            new Vector2(
                Lerp(selectionIndicator.position.x, 
                    currentTarget.position.x, 
                    interpolationValue), 
                selectionIndicator.position.y);
        selectionIndicator.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal,  
            Lerp(selectionIndicator.rect.width, 
                currentTarget.rect.width, 
                interpolationValue));
    }

    private float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t * Time.deltaTime);
}
