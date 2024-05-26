using System;
using TMPro;
using UnityEngine;

namespace TimeCycle.UI
{
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] TMP_Text timeText;

        private void Start()
        {
            if (timeText == null)
                timeText = GetComponentInChildren<TMP_Text>();
        }

        public void UpdateText( DateTime currentTime )
        {
            if (timeText == null)
                return;
            timeText.text = currentTime.ToString("hh:mm");
        }
    }
}