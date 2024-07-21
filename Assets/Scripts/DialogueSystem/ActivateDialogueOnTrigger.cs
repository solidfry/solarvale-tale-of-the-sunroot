using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateDialogueOnTrigger : MonoBehaviour
{
    public UnityEvent onTriggerEvent;
    private bool _hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.CompareTag("Player"))
        {
            if (!_hasBeenTriggered)
            {
                onTriggerEvent?.Invoke();
                _hasBeenTriggered = true;
            }

        }
    }
}
