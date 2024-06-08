using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RowUIHandler : MonoBehaviour
{
    EventSystem eventSystem;
    [SerializeField] Image image;

    void Start()
    {
        eventSystem = EventSystem.current;
        
        if (image == null)
            image = GetComponent<Image>();
    }
    
    void Update()
    {
        // if (IsChildSelected())
        //     HighlightRow();
        // else 
        //     return;
    }
    
    // bool IsChildSelected() => eventSystem.currentSelectedGameObject.transform.parent == this.gameObject ? true : false;

    void HighlightRow()
    {
        image.color = Color.green;
    }
}
