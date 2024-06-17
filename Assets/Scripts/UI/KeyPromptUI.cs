using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class KeyPromptUI : MonoBehaviour
    {
        [SerializeField] private Image keyImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text keyPromptText;
        
        [SerializeField] private Color interactedColor = Color.green;
        Color originalColor;
        
        [SerializeField] InputActionReference inputAction;

        private void Awake()
        {
            originalColor = keyImage.color;
        }
        
        private void OnEnable()
        {
            if (inputAction == null) return;
            inputAction.action.performed += ctx => AnimateKeyImage(interactedColor);
        }

        private void OnDisable()
        {
            if (inputAction == null) return;
            inputAction.action.performed -= ctx => AnimateKeyImage(interactedColor);
        }

        public void SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }

        public void SetText(string key)
        {
            keyPromptText.text = key;
        }
        
        public void AnimateKeyImage(Color color, float duration = 0.5f)
        {
            keyImage.transform.DOScale( 1f, .01f);
            keyImage.DOColor(color, duration).OnComplete(() => keyImage.DOColor(originalColor, duration)).SetAutoKill(false); 
            keyImage.gameObject.transform.DOPunchScale( new Vector3(0.1f, 0.1f, 0.1f), duration, 1, 0.5f).OnComplete( (
                () => keyImage.transform.DOScale(1, 0.1f)))
                .SetAutoKill(false);
        }
    }
}
