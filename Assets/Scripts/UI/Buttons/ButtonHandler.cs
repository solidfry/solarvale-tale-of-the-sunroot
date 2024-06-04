using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    [Serializable]
    public class ButtonHandler : MonoBehaviour
    {
        [Header("Styling Options")]
        [SerializeField] bool useButtonColor;
        [SerializeField] bool useBackgroundColor;
        
        [Header("UI Variants")]
        [SerializeField] Variant variant = Variant.None;
        [SerializeField] private UIImageVariantsData detailLevels;
        [SerializeField] private UIImageVariantsData detailLevelWithBackground;
        
        [Header("UI Components")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        
        Image _buttonImage;
        Color _currentColor;
        Color _nextColor;

        private void OnValidate()
        { 
            Initialise();
            _currentColor = button.colors.normalColor;
            SetTextColor();
        }

        void Start() => Initialise();

        private void Initialise()
        {
            if (text is null || button is null) return;
            text = GetComponentInChildren<TMP_Text>();
            button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            button.image = _buttonImage;
            
            SetButtonImage();
        }

        void Update()
        {
            if (!useButtonColor) return;
            if (text is null || button is null) return;
        
            SetNextColor();
            SetCurrentColor();
        }

        private void SetCurrentColor()
        {
            _currentColor = _nextColor;
            _currentColor.a = _nextColor.a;
            SetTextColor();
        }

        private void SetTextColor()
        {
            text.color = _currentColor;
        }

        private void SetNextColor()
        {
            _nextColor = button.targetGraphic.canvasRenderer.GetColor();
            _nextColor.a = button.targetGraphic.canvasRenderer.GetAlpha();
        }
        
        private void SetButtonImage()
        {
            if (detailLevels is null) return;
            if (useBackgroundColor)
            {
                button.image.sprite = detailLevelWithBackground.GetSprite(variant);
                return;
            }
            
            button.image.sprite = detailLevels.GetSprite(variant);
        }
    }
}
