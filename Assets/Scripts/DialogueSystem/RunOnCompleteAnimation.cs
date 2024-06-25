using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class RunOnCompleteAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animationController;
        [SerializeField] private string animationName;
        public UnityEvent onAnimationEndEvent;
        
        void Start()
        {
            if (animationController == null)
            {
                animationController = GetComponent<Animator>();
            }
            StartCoroutine(WaitForAnimationToEnd(animationName));
        }

        private IEnumerator WaitForAnimationToEnd(string anim = null)
        {
            if (anim == null)
            {
                Debug.Log("No animation name provided");
                yield break;
            }
            
            while (animationController.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
                   animationController.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f)
            {
                yield return null;
            }

            Debug.Log("Animation ended");
            onAnimationEndEvent?.Invoke();
        }
    }
}
