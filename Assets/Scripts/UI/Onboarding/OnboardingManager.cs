using Events;
using UnityEngine;

namespace UI.Onboarding
{
    public class OnboardingManager : MonoBehaviour
    {
        [SerializeField] private GameObject boarderLowAnimationToCameraObject;
        [SerializeField] private GameObject boarderLowAnimationToTakePhotoObject;
        CanvasGroup _hudCanvas;
        private bool _animationToCameraOn = false;
        private bool _animationToTakePhotoOn = false;

        
        public void OnEnable()
        {
            GlobalEvents.OnSetOnboardingVisibilityEvent += SetHUDVisibility;
        }
        
        public void OnDisable()
        {
            GlobalEvents.OnSetOnboardingVisibilityEvent -= SetHUDVisibility;
        }

        private void SetHUDVisibility(bool value)
        {
            _hudCanvas.alpha = value ? 1 : 0;
        }
        
        private void Start()
        {
            if (_hudCanvas is null)
                _hudCanvas = GetComponent<CanvasGroup>();
            
            _hudCanvas.alpha = 0;
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
            _hudCanvas.alpha = 1;
            _animationToCameraOn = true;
        }
    }
}
