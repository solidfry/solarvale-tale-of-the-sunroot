using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

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
    }
    
    private void OnDisable()
    {
        currentDialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
        currentDialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    void OnDialogueStart()
    {
        GlobalEvents.OnPlayerControlsLockedEvent?.Invoke(true);
        GlobalEvents.OnLockCursorEvent?.Invoke(false);
        GlobalEvents.OnSetPlayerControlMapEvent?.Invoke("UI");
        SetFocusObject(initialFocusObject);
    }
    
    void SetFocusObject(GameObject focusObject)
    {
        if (focusObject is null) return;
        EventSystem.current.SetSelectedGameObject(focusObject);
        EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().Select();
        // Debug.Log(" SetFocusObject " + focusObject.name + " " + EventSystem.current.currentSelectedGameObject.name);
    }
    
    void OnDialogueComplete()
    {
        GlobalEvents.OnPlayerControlsLockedEvent?.Invoke(false);
        GlobalEvents.OnLockCursorEvent?.Invoke(true);
        GlobalEvents.OnSetPlayerControlMapEvent?.Invoke("Player");
        GlobalEvents.OnDialogueCompleteEvent?.Invoke();
        // Debug.Log("OnDialogueComplete");
    }
}
