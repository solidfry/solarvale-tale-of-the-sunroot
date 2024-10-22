using Core;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;

namespace DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] DialogueRunner currentDialogueRunner;
        [SerializeField] GameObject initialFocusObject;
    
        private void Awake()
        {
            if (currentDialogueRunner is null)
                currentDialogueRunner = FindObjectOfType<DialogueRunner>();
        }

        private void OnEnable()
        {
            currentDialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            currentDialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        
            GlobalEvents.OnDialogueStartWithNodeEvent += StartDialogue;
        }

        private void OnDisable()
        {
            currentDialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            currentDialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        
            GlobalEvents.OnDialogueStartWithNodeEvent -= StartDialogue;
        }

        void OnDialogueStart()
        {
            GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(true);
            // GlobalEvents.OnLockCursorEvent?.Invoke(false);
            GlobalEvents.OnSetPlayerControlMapEvent?.Invoke("UI");
            GlobalEvents.OnPauseMenuAvailabilityEvent?.Invoke(false);
            GlobalEvents.OnGameStateChangeEvent?.Invoke(GameState.Dialogue);
            SetFocusObject(initialFocusObject);
        }
    
        void SetFocusObject(GameObject focusObject)
        {
            if (focusObject is null) return;
            EventSystem.current.SetSelectedGameObject(focusObject);
            EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().Select();
            // Debug.Log(" SetFocusObject " + focusObject.name + " " + EventSystem.current.currentSelectedGameObject.name);
        }
        
        void ClearFocusObject()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    
        void OnDialogueComplete()
        {
            GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(false);
            // GlobalEvents.OnLockCursorEvent?.Invoke(true);
            GlobalEvents.OnSetPlayerControlMapEvent?.Invoke("Player");
            GlobalEvents.OnDialogueCompleteEvent?.Invoke();
            GlobalEvents.OnPauseMenuAvailabilityEvent?.Invoke(true);
            GlobalEvents.OnDialogueCompleteWithNodeEvent?.Invoke(currentDialogueRunner.startNode);
            GlobalEvents.OnGameStateChangeEvent?.Invoke(GameState.Exploration);
            ClearFocusObject();
        }
    
        private void StartDialogue(string node)
        {
            currentDialogueRunner.startNode = node;
            currentDialogueRunner.StartDialogue(node);
        }

        // private void Update()
        // {
        //     if (!currentDialogueRunner.IsDialogueRunning) return;
        //     if (Cursor.lockState != CursorLockMode.None)
        //     {
        //         Cursor.lockState = CursorLockMode.None;
        //         Cursor.visible = true;
        //         
        //         if (EventSystem.current.currentSelectedGameObject is null)
        //         {
        //             var selectable = gameObject.GetComponentInChildren<Selectable>();
        //             SetFocusObject(selectable.gameObject);
        //             selectable.Select();
        //         }
        //     }
        // }
    }
}
