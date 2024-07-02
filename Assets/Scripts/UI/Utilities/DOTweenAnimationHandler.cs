using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace UI.Utilities
{
    public class DoTweenAnimationHandler : MonoBehaviour
    {

        [SerializeField] bool setAutoKill = true;
        [SerializeField] List<DOTweenAnimation> animations;
        
        Coroutine _playInReverseCoroutine;
        Coroutine _playCoroutine;
        
        private void Awake()
        {
            animations = GetComponentsInChildren<DOTweenAnimation>().ToList();
        }

        private void OnEnable()
        {
            PlayAll();
        }

        private void PlayAll()
        {
            foreach (var tween in animations)
            {
                tween.DORewind();
                tween.autoKill = setAutoKill;
                // Debug.Log(tween + " tween is playing and " + tween.autoKill + " is the autoKill value");
                tween.DOPlay();
            }
            
        }

        private void OnDisable()
        {
            foreach (var tween in animations)
            {
                tween.DORewind();
            }
        }
        
        public List<DOTweenAnimation> GetAnimations() => animations;

    
    }
}
