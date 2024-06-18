using System.ComponentModel;
using Events;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    public class DialogueHelper : MonoBehaviour
    {
        [SerializeField] string dialogueNode;
        [SerializeField] bool isOneTimeUse = false;
        [SerializeField] bool used = false;
    
        [Header("Events")]
        [Space(24)]
        [SerializeField, Description("Event called when the dialogue starts.")] 
        UnityEvent onDialogueStartEvent;
        [SerializeField, Description("Event called when the dialogue ends.")] 
        UnityEvent onDialogueCompleteEvent;

        private void OnEnable()
        {
            GlobalEvents.OnDialogueCompleteWithNodeEvent += OnDialogueComplete;
        }

        private void OnDisable()
        {
            GlobalEvents.OnDialogueCompleteWithNodeEvent -= OnDialogueComplete;
        }

        public void StartDialogue()
        {
            if (string.IsNullOrEmpty(dialogueNode))
            {
                Debug.LogWarning("Dialogue node is empty.");
                return;
            }
        
            if (isOneTimeUse && used) return;
        
            if (isOneTimeUse && !used) used = true;
        
            GlobalEvents.OnDialogueStartWithNodeEvent?.Invoke(dialogueNode);
            onDialogueStartEvent?.Invoke();
        }
    
        private void OnDialogueComplete(string node)
        {
            if (node != dialogueNode) return;
            onDialogueCompleteEvent?.Invoke();
        }
    }
}
