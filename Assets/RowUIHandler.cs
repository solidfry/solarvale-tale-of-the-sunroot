using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RowUIHandler : MonoBehaviour
{
    EventSystem eventSystem;
    [SerializeField] Image image;
    [SerializeField] Selectable childSelectable;
    
    void Start()
    {
        eventSystem = EventSystem.current;
        
        if (image is null)
            image = GetComponent<Image>();

        if (childSelectable is null)
            childSelectable = GetComponentInChildren<Selectable>();
    }
    
    void Update()
    {
        if (IsChildSelected())
        {
           HighlightRow(); 
        } 
        else
        {
            RemoveHighlight();
        }
        
    }
    
    bool IsChildSelected()
    {
        if (childSelectable is null) return false;
        return eventSystem.currentSelectedGameObject == childSelectable.gameObject;
    }

    void HighlightRow()
    {
        image.CrossFadeAlpha(1f, 0.1f, false);
    }
    
    void RemoveHighlight()
    {
        image.CrossFadeAlpha(0.0f, 0.1f, false);
    }
}
