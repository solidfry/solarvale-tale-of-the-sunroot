using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ToggleUIStateHandler : MonoBehaviour
    {
    
        Toggle _toggle;
        TMP_Text _text;
    
        [SerializeField] string toggleTextOff;
        [SerializeField] string toggleTextOn;
    
        // Start is called before the first frame update
        void Start()
        {
            _toggle = GetComponent<Toggle>();
            _text = GetComponentInChildren<TMP_Text>();
            _toggle.onValueChanged.AddListener(OnToggle);
        }
    
        void OnToggle(bool value) => _text.text = value ? toggleTextOn : toggleTextOff;
    }
}
