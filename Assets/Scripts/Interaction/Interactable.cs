using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    ///<summary>
    /// This class is added to any GameObject that can be interacted with.
    /// </summary>
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] private UnityEvent interactEvent;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private bool isOneTimeUse = false;
        [SerializeField] private bool isDisabledAfterUse = false;
        [SerializeField] private string interactMessage = "Interact";
        [SerializeField] private bool showCannotInteractMessage = true;
        [SerializeField] private string cannotInteractMessage = "Cannot interact with this object.";

        public string Name => gameObject.name;
        public string InteractMessage => interactMessage;

        private void Awake()
        {
            // need to set the layer to interactable
            gameObject.layer = LayerMask.NameToLayer("Interaction");
        }

        public void Interact()
        {
            if (CheckIsInteractable()) return;
            
            Debug.Log("Interacting with " + gameObject.name);
            interactEvent?.Invoke();

            if (isOneTimeUse) isInteractable = false;

            if (isDisabledAfterUse) gameObject.SetActive(false);
        }

        private bool CheckIsInteractable()
        {
            if (!isInteractable)
            {
                if (showCannotInteractMessage)
                    Debug.Log(cannotInteractMessage);

                return true;
            }

            return false;
        }
    }
}