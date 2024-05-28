using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    public class DemoAPIController : MonoBehaviour
    {
        //Time Of Day
        public GaiaTimeOfDay m_timeOfDayData;
        //Weather
        public bool m_weatherEnabled = false;
        public bool m_instantWeatherTransition = false;
        //Season
        public PWSkySeason m_seasonData;
        //Wind
        public PWSkyWind m_windData;
    }
}