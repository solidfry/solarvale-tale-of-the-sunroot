using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DOTweenAnimationHandler : MonoBehaviour
{

    [SerializeField] List<DOTweenAnimation> animations;


    private void Awake()
    {
        animations = GetComponentsInChildren<DOTweenAnimation>().ToList();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable Ran");
        foreach (var tween in animations)
        {
            tween.DORewind();
            Debug.Log(tween + " tween is playing and " + tween.autoKill + " is the autoKill value");
            tween.DOPlay();
        }
    }
    
    private void OnDisable()
    {
        Debug.Log("OnDisable Ran");
        foreach (var tween in animations)
        {
            tween.DORewind();
        }
    }
}
