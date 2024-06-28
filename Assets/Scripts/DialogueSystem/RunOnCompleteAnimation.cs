using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    public class RunOnCompleteAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animationController;
        [SerializeField] private string animationName;
        public UnityEvent onAnimationStartEvent;
        public UnityEvent onAnimationEndEvent;
        
        bool isPlaying = false;
        
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
            if (!isPlaying)
            {
                onAnimationStartEvent?.Invoke();
                isPlaying = true;
                Debug.Log(isPlaying + " is playing");
            }
            
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
            isPlaying = false;
            Debug.Log(anim + " is playing: " + isPlaying);
        }
    }
}
