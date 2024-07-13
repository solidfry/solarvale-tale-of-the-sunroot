using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnboardingManager : MonoBehaviour
{
    [SerializeField] private GameObject boarderLowAnimationToCameraObject;
    [SerializeField] private GameObject boarderLowAnimationToTakePhotoObject;

    private bool _animationToCameraOn = false;
    private bool _animationToTakePhotoOn = false;


    private void Start()
    {
        boarderLowAnimationToCameraObject.SetActive(false);
        boarderLowAnimationToTakePhotoObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && _animationToCameraOn == true)
        {
            boarderLowAnimationToCameraObject.SetActive(false);
            _animationToCameraOn = false;
            _animationToTakePhotoOn = true;
        }
        if (_animationToTakePhotoOn)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                bool isActive = boarderLowAnimationToTakePhotoObject.activeSelf;
                boarderLowAnimationToTakePhotoObject.SetActive(!isActive);
            }
            if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(0))
            {
                boarderLowAnimationToTakePhotoObject.SetActive(false);
                _animationToTakePhotoOn = false;
            }
        }
    }

    public void AnimationToCamera()
    {
        boarderLowAnimationToCameraObject.SetActive(true);
        _animationToCameraOn = true;
    }
}
