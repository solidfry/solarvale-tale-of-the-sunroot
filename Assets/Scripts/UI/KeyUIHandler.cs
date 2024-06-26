using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KeyUIHandler : MonoBehaviour
    {
        [SerializeField] KeyMappingData keyMappingData;
        [SerializeField] string key;
        [SerializeField] KeyType keyType;
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
            keyType = keyMapType.keyType;
            _image.sprite = keyMappingData.GetSprite(keyType);
        }

    
    }
}


