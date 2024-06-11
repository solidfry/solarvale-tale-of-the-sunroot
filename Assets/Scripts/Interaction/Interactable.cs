using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    ///<summary>
    /// This class is added to any GameObject that can be interacted with.
    /// </summary>
    public class Interactable : MonoBehaviour, IInteract
    {
        [SerializeField] private UnityEvent interactEvent;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private bool isOneTimeUse = false;
        [SerializeField] private string interactMessage = "Interact";
        [SerializeField] private bool showCannotInteractMessage = true;
        [SerializeField] private string cannotInteractMessage = "Cannot interact with this object.";

        public string Name => gameObject.name;
        public string InteractMessage => interactMessage;

        public void Interact()
        {
            if (CheckIsInteractable()) return;
            
            Debug.Log("Interacting with " + gameObject.name);
            
            if (isOneTimeUse) isInteractable = false;
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