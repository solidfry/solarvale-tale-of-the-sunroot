using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDisplay : MonoBehaviour
{
    [SerializeField] private GameObject dialogueSystem;
    [SerializeField] private Animator introDolly;
    [SerializeField] private string animationName;
    void Start()
    {
        dialogueSystem.SetActive(false);
        StartCoroutine(WaitForIntroDollyToEnd());
    }

    private IEnumerator WaitForIntroDollyToEnd()
    {
        while (introDolly.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               introDolly.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        dialogueSystem.SetActive(true);
    }
}
