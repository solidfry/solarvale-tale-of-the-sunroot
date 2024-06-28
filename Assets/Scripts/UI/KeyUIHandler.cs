using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class KeyUIHandler : MonoBehaviour
    {
        [SerializeField] KeyMappingData keyMappingData;
        [SerializeField] string key;
        [SerializeField] KeyType keyType;
        [SerializeField] Image fallbackImage;
        Image _image;
        TMP_Text _text;
    
        private void Start() => Initialise();

        private void OnValidate() => Initialise();

        private void Initialise()
        {
            _image = GetComponent<Image>();
            _image.sprite = keyMappingData.GetSprite(keyType);
            _text = GetComponentInChildren<TMP_Text>();
            _text.text = key;
        }
        
        public void SetText(string k)
        {
            key = k;
            _text.text = key;
        }
        
        public void SetKeyType(string deviceName, string actionName)
        {
            var keyMapType = keyMappingData.GetSpriteByDeviceAndAction(deviceName, actionName);
            if (keyMapType is null) return;
            if (_image is null) return;
            keyType = keyMapType.keyType;
            _image.sprite ??= keyMappingData.GetSprite(keyType);
            if (keyMapType.spriteOverrideForGlyph != null)
            {
                fallbackImage.sprite = keyMapType.spriteOverrideForGlyph;
                fallbackImage.enabled = true;
                _text.gameObject.SetActive(false);
            } 
            else 
            {
                fallbackImage.enabled = false;
                SetText(keyMapType.shortName);
                _text.gameObject.SetActive(true);
            }
        }

    
    }
}


