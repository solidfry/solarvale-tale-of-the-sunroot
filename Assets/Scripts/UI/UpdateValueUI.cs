using System.Globalization;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UpdateValueUI : MonoBehaviour
    {
        TMP_Text _text;
        
        void Start()
        {
            _text = GetComponent<TMP_Text>();
        }
        
        public void UpdateValue(float value)
        {
            _text.text = value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
