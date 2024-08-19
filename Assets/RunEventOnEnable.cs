using UnityEngine;
using UnityEngine.Events;

public class RunEventOnEnable : MonoBehaviour
{
    [SerializeField] UnityEvent onEnableEvent;
    
    void OnEnable()
    {
        onEnableEvent?.Invoke();
    }

    
}
