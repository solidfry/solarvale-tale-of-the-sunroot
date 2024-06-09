using TMPro;
using UnityEngine;

namespace UI
{
    public class UpdateValueUI : MonoBehaviour
    {
        TMP_Text text;
        
        void Start()
        {
            text = GetComponent<TMP_Text>();
        }
        
        public void UpdateValue(float value)
        {
            text.text = value.ToString();
        }
    }
}
