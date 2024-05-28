using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniStorm.Example
{
    public class UniStormUIDisplay : MonoBehaviour
    {

        public Text UniStormTime;
        public Text UniStormTemperature;
        public RawImage UniStormWeatherIcon;

        void Update()
        {
            UniStormTime.text = UniStormSystem.Instance.Hour.ToString() + ":" + UniStormSystem.Instance.Minute.ToString("00");
            UniStormTemperature.text = UniStormSystem.Instance.Temperature.ToString() + "°";
            UniStormWeatherIcon.texture = UniStormSystem.Instance.CurrentWeatherType.WeatherIcon;
        }
    }
}