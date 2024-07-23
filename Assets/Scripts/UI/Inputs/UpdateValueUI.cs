using System.Globalization;
using TMPro;
using UnityEngine;

namespace UI.Inputs
{
    public class UpdateValueUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        
        void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }
        
        public void UpdateValue(float value)
        {
            value *= 100;
            var clamp = Mathf.FloorToInt(value);
            _text.text = clamp.ToString(CultureInfo.CurrentCulture);
        }
    }
}
