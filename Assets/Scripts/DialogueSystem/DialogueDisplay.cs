using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueDisplay : MonoBehaviour
{
    [SerializeField] private Animator introDolly;
    [SerializeField] private string animationName;
    public UnityEvent onDialogueStartEvent;
    void Start()
    {
        StartCoroutine(WaitForIntroDollyToEnd());
    }

    private IEnumerator WaitForIntroDollyToEnd()
    {
        while (introDolly.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               introDolly.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        onDialogueStartEvent?.Invoke();
    }
}
