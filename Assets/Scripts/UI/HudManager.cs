using CameraSystem;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class HudManager : MonoBehaviour
    {
        [Header("Menu Settings")]
        [SerializeField] GameObject menu;
        [SerializeField] bool isMenuShown = false;
        [SerializeField] bool canShowMenu = true;
        [SerializeField] InputActionReference menuOpenAction;
        [SerializeField] InputActionReference menuCloseAction;
        
        [Header("Player HUD Settings")]
        [SerializeField] GameObject playerHUD;
        [SerializeField] float playerHUDFadeDuration = 0.5f;
        [SerializeField] bool isHUDShown = true;
        
        GameObject _menuInstance;
        GameObject _playerHUDInstance;
        CanvasGroup _playerHUDCanvasGroup;
        
        Tween _fadeTween;
        
        private void Awake() => Initialise();

        private void Initialise()
        {
            if (menu is null || playerHUD is null) return;
            _menuInstance = Instantiate(menu, transform.position, Quaternion.identity, transform);
            _playerHUDInstance = Instantiate(playerHUD, transform.position, Quaternion.identity, transform);
            
            _playerHUDCanvasGroup = _playerHUDInstance.GetComponent<CanvasGroup>();
            _menuInstance.SetActive(false);
        }
        
        private void OnCameraModeChanged(CameraMode mode)
        {
            var check = mode == CameraMode.Exploration;
            SetHUDVisibility(check);
        }

        private void OnEnable()
        {
            GlobalEvents.OnChangeCameraModeEvent += OnCameraModeChanged;
            menuOpenAction.action.performed += _ => OnMenu();
            menuCloseAction.action.performed += _ => OnMenu();
        }

        private void OnDisable()
        {
            GlobalEvents.OnChangeCameraModeEvent -= OnCameraModeChanged;
        }
        
        private void SetHUDVisibility(bool value)
        {
            // Debug.Log("Toggle HUD: " + value);
            //
            isHUDShown = value;
            
            if (_playerHUDCanvasGroup is not null) 
            {
                if (_fadeTween != null) _fadeTween.Kill();
                _fadeTween = _playerHUDCanvasGroup.DOFade(value ? 1 : 0, playerHUDFadeDuration).OnComplete(() => _playerHUDInstance.SetActive(value));
            }
            else 
                _playerHUDInstance.SetActive(value);
        }

        void OnMenu()
        {
            if (!canShowMenu) return;
            isMenuShown = !isMenuShown;
            _menuInstance.SetActive(isMenuShown);
            // Debug.Log("show menu: " + showMenu);
            GlobalEvents.OnGamePausedEvent?.Invoke(isMenuShown);
            GlobalEvents.OnPlayerChangeActionMapEvent?.Invoke(isMenuShown);
            Cursor.lockState = isMenuShown ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            Destroy(_menuInstance);
            Destroy(_playerHUDInstance);
        }
        
    }
}
