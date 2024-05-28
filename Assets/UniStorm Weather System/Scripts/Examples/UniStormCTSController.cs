using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniStorm.Example
{
    public class UniStormCTSController : MonoBehaviour
    {
#if CTS_PRESENT
        public float MaxRainSmoothness = 0.8f;
        CTS.CTSWeatherManager m_CTSWeatherManager;

        //Get the CTS Weather Manager. If one is not found, create one.
        void Start()
        {
            if (FindObjectOfType<CTS.CTSWeatherManager>())
            {
                m_CTSWeatherManager = FindObjectOfType<CTS.CTSWeatherManager>();
            }
            else
            {
                GameObject TempCTSWeatherManager = new GameObject("CTS Weather Manager");
                TempCTSWeatherManager.AddComponent<CTS.CTSWeatherManager>();
                m_CTSWeatherManager = FindObjectOfType<CTS.CTSWeatherManager>();
            }

            InitializeCTS();
        }

        //Initializes CTS
        public void InitializeCTS()
        {
            if (Shader.GetGlobalFloat("_WetnessStrength") > 0)
            {
                m_CTSWeatherManager.RainPower = 1;
            }
            else if (Shader.GetGlobalFloat("_SnowStrength") > 0)
            {
                m_CTSWeatherManager.SnowPower = 1;
            }
            else
            {
                m_CTSWeatherManager.RainPower = 0f;
                m_CTSWeatherManager.SnowPower = 0f;
            }

            m_CTSWeatherManager.MaxRainSmoothness = MaxRainSmoothness;
            m_CTSWeatherManager.Season = ((float)UniStormSystem.Instance.UniStormDate.DayOfYear / 365) * 4;
        }

        void Update()
        {
            //Watches UniStorm's Global Weather Shader variables and applies the same value to CTS's shader values.
            //Both need to be watched at once in order for them to be properly controlled when transtioning between snow and rain.
            if (Shader.GetGlobalFloat("_WetnessStrength") > 0)
            {
                m_CTSWeatherManager.RainPower = Shader.GetGlobalFloat("_WetnessStrength");
            }
            if (Shader.GetGlobalFloat("_SnowStrength") > 0)
            {
                m_CTSWeatherManager.SnowPower = Shader.GetGlobalFloat("_SnowStrength");
            }

            //Uses UniStorm's date time as the seasonal progression for CTS
            m_CTSWeatherManager.Season = ((float)UniStormSystem.Instance.UniStormDate.DayOfYear / 365) * 4;
        }
#endif
    }
}