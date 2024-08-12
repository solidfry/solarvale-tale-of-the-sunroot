using Events;
using UnityEngine;

public class HUDVisibilityRequestSender : MonoBehaviour
{
    [field: SerializeField] public bool SetVisible { get; private set; } = true;
    // Start is called before the first frame update

    public void RequestHUDVisibility() => GlobalEvents.OnActivateHUDVisibilityEvent?.Invoke(SetVisible);
}
