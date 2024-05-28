using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniStorm.Utility;
using UniStorm.Effects;

namespace UniStorm
{
    public class UniStormManager : MonoBehaviour
    {
        public static UniStormManager Instance = null;

        void Start()
        {
            Instance = this;
        }

        /// <summary>
        /// Sets UniStorm's Time
        /// </summary>
        public void SetTime(int Hour, int Minute)
        {
            UniStormSystem.Instance.m_TimeFloat = (float)Hour / 24 + (float)Minute / 1440;
        }

        /// <summary>
        /// Sets UniStorm's Date
        /// </summary>
        public void SetDate(int Month, int Day, int Year)
        {
            UniStormSystem.Instance.UniStormDate = new System.DateTime(Year, Month, Day);
            UniStormSystem.Instance.Month = Month;
            UniStormSystem.Instance.Day = Day;
            UniStormSystem.Instance.Year = Year;
        }

        /// <summary>
        /// Sets UniStorm's Date
        /// </summary>
        public System.DateTime GetDate()
        {
            return UniStormSystem.Instance.UniStormDate.Date;
        }

        /// <summary>
        /// Changes UniStorm's weather, regardless of conditions, with the transition speed to the weather type parameter. 
        /// </summary>
        public void ChangeWeatherWithTransition(WeatherType weatherType)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                if (UniStormSystem.Instance.AllWeatherTypes.Contains(weatherType))
                {
                    UniStormSystem.Instance.ChangeWeather(weatherType);
                }
                else
                {
                    Debug.LogError("The weather type was not found in UniStorm Editor's All Weather Types list. The weather type must be added to the UniStorm Editor's All Weather Type list before it can be used.");
                }
            }
        }

        /// <summary>
        /// Changes UniStorm's weather instantly, regardless of conditions, to the weather type parameter. 
        /// </summary>
        public void ChangeWeatherInstantly(WeatherType weatherType)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                if (UniStormSystem.Instance.AllWeatherTypes.Contains(weatherType))
                {
                    UniStormSystem.Instance.CurrentWeatherType = weatherType;
                    UniStormSystem.Instance.InitializeWeather(false);
                }
                else
                {
                    Debug.LogError("The weather type was not found in UniStorm Editor's All Weather Types list. The weather type must be added to the UniStorm Editor's All Weather Type list before it can be used.");
                }
            }
        }

        /// <summary>
        /// Generates a random weather type, regardless of conditions, from UniStorm's All Weather Type list 
        /// </summary>
        public void RandomWeather()
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                int RandomWeatherType = Random.Range(0, UniStormSystem.Instance.AllWeatherTypes.Count);
                UniStormSystem.Instance.ChangeWeather(UniStormSystem.Instance.AllWeatherTypes[RandomWeatherType]);
            }
        }

        /// <summary>
        /// Regenerates UniStorm's forecast using the precipitation odds and weather type conditions, but does not change the weather. 
        /// If you are using the Daily weather generation type, a new forecast weather will be generated along with a new forecast hour.
        /// If you are using the Hourly weather generation type, a new forecast weather will be generated for all 24 hours.
        /// </summary>
        public void RegenerateForecast()
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.WeatherForecast.Clear();
                UniStormSystem.Instance.GenerateWeather();
            }
        }

        /// <summary>
        /// Disables or enables all UniStorm weather sounds depeding on the ActiveState bool, but does not affect their current volume.
        /// </summary>
        public void ChangeWeatherSoundsState(bool ActiveState)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.m_SoundTransform.SetActive(ActiveState);
            }
        }

        /// <summary>
        /// Disables or enables all UniStorm particle effects depeding on the ActiveState bool, , but does not affect their emission amount.
        /// </summary>
        public void ChangeWeatherEffectsState(bool ActiveState)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.m_EffectsTransform.SetActive(ActiveState);
            }
        }

        /// <summary>
        /// Change the player transform and player camera sources, if they need to be changed or updated.
        /// UniStorm's weather type components will be moved to the new Player Transform's transform. 
        /// Your previous Camera Component will be disabled as the clouds would be rendered twice.
        /// This function will alway enabled the CameraSource's Camera Component.
        /// </summary>
        public void ChangeCameraSource(Transform PlayerTransform, Camera CameraSource)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.PlayerCamera.enabled = false;

                if (//UniStormSystem.Instance.PlayerCamera.GetComponent<ScreenSpaceCloudShadows>() != null &&
                    UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                {
                    UniStormSystem.Instance.EnableCloudShadows(false); //PlayerCamera.GetComponent<ScreenSpaceCloudShadows>().enabled = false;
                }

                UniStormSystem.Instance.PlayerTransform = PlayerTransform;
                UniStormSystem.Instance.m_EffectsTransform.transform.SetParent(PlayerTransform);
                UniStormSystem.Instance.m_EffectsTransform.transform.localPosition = Vector3.zero;
                UniStormSystem.Instance.m_SoundTransform.transform.SetParent(PlayerTransform);
                UniStormSystem.Instance.m_SoundTransform.transform.localPosition = Vector3.zero;
                UniStormSystem.Instance.PlayerCamera = CameraSource;
                UniStormSystem.Instance.m_UniStormLightningSystem.PlayerTransform = PlayerTransform;

                //Look to see if the Render Clouds CommandBuffer exists on the newly assignned camera source, if it doesn't add one as this is needed to properly render the clouds.
                UnityEngine.Rendering.CommandBuffer[] CommandBuffers = CameraSource.GetCommandBuffers(UnityEngine.Rendering.CameraEvent.AfterSkybox);

                if (CommandBuffers.Length > 0)
                {
                    for (int i = 0; i < CommandBuffers.Length; i++)
                    {
                        if (CommandBuffers[i].name != "Render Clouds")
                        {
                            UnityEngine.Rendering.CommandBuffer cloudsCommBuff = FindObjectOfType<UniStormClouds>().cloudsCommBuff;
                            CameraSource.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterSkybox, cloudsCommBuff);
                            cloudsCommBuff.name = "Render Clouds";
                        }
                    }
                }
                else if (CommandBuffers.Length == 0)
                {
                    UnityEngine.Rendering.CommandBuffer cloudsCommBuff = FindObjectOfType<UniStormClouds>().cloudsCommBuff;
                    CameraSource.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterSkybox, cloudsCommBuff);
                    cloudsCommBuff.name = "Render Clouds";
                }

                if (//UniStormSystem.Instance.PlayerCamera.GetComponent<ScreenSpaceCloudShadows>() == null &&
                    UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                {
                    UniStormCloudShadowsFeature.Settings m_CloudShadows = UniStormSystem.Instance.GetCloudShadowsFeatureSettings(); //ScreenSpaceCloudShadows PlayerCamera.gameObject.AddComponent<ScreenSpaceCloudShadows>();

                    m_CloudShadows.CloudShadowTexture = FindObjectOfType<UniStormClouds>().PublicCloudShadowTexture;
                    m_CloudShadows.BottomThreshold = 0.5f;
                    m_CloudShadows.TopThreshold = 1;
                    m_CloudShadows.CloudTextureScale = 0.001f;
                    m_CloudShadows.ShadowIntensity = UniStormSystem.Instance.CurrentWeatherType.CloudShadowIntensity;
                }
                else
                {
                    if (UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                    {
                        UniStormSystem.Instance.EnableCloudShadows(true); //PlayerCamera.GetComponent<ScreenSpaceCloudShadows>().enabled = true;
                    }
                }

                UniStormSystem.Instance.PlayerCamera.enabled = true;
            }
        }

        /// <summary>
        /// Gets the forecasted weather type's name
        /// </summary>
        public string GetWeatherForecastName()
        {
            return UniStormSystem.Instance.NextWeatherType.WeatherTypeName;
        }

        /// <summary>
        /// Gets the hour that the forecasted weather will change
        /// </summary>
        public int GetWeatherForecastHour()
        {
            return UniStormSystem.Instance.HourToChangeWeather;
        }

        /// <summary>
        /// Set UniStorm's Music volume using a value from 0 (Fully muted) to 1 (Full volume).
        /// </summary>
        public void SetMusicVolume(float Volume)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                if (Volume <= 0)
                {
                    Volume = 0.001f;
                }
                else if (Volume > 1)
                {
                    Volume = 1;
                }
                UniStormSystem.Instance.UniStormAudioMixer.SetFloat("MusicVolume", Mathf.Log(Volume) * 20);
            }
        }

        /// <summary>
        /// Set UniStorm's Ambience volume using a value from 0 (Fully muted) to 1 (Full volume).
        /// </summary>
        public void SetAmbienceVolume(float Volume)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                if (Volume <= 0)
                {
                    Volume = 0.001f;
                }
                else if (Volume > 1)
                {
                    Volume = 1;
                }
                UniStormSystem.Instance.UniStormAudioMixer.SetFloat("AmbienceVolume", Mathf.Log(Volume) * 20);
            }
        }

        /// <summary>
        /// Set UniStorm's Weather volume using a value from 0 (Fully muted) to 1 (Full volume).
        /// </summary>
        public void SetWeatherVolume(float Volume)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                if (Volume <= 0)
                {
                    Volume = 0.001f;
                }
                else if (Volume > 1)
                {
                    Volume = 1;
                }
                UniStormSystem.Instance.UniStormAudioMixer.SetFloat("WeatherVolume", Mathf.Log(Volume) * 20);
            }
        }

        /// <summary>
        /// Sets the length, in minutes, of UniStorm's days
        /// </summary>
        public void SetDayLength(int MinuteLength)
        {
            UniStormSystem.Instance.DayLength = MinuteLength;
        }

        /// <summary>
        /// Sets the length, in minutes, of UniStorm's night
        /// </summary>
        public void SetNightLength(int MinuteLength)
        {
            UniStormSystem.Instance.NightLength = MinuteLength;
        }

        /// <summary>
        /// Changes UniStorm's moon phase color. The updated color will be applied at noon when UniStorm's moon is updated.
        /// </summary>
        public void ChangeMoonPhaseColor(Color MoonPhaseColor)
        {
            UniStormSystem.Instance.MoonPhaseColor = MoonPhaseColor;
        }

        /// <summary>
        /// Updates UniStorm's Cloud Quality to be Low, Medium, High, or Ultra
        /// </summary>
        public void UpdateCloudQuality(UniStormSystem.CloudQualityEnum CloudQuality)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.CloudQuality = CloudQuality;
                UniStormClouds m_UniStormClouds = FindObjectOfType<UniStormClouds>();
                Material m_CloudsMaterial = FindObjectOfType<UniStormClouds>().skyMaterial;

                if (UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                {
                    m_UniStormClouds.SetCloudDetails((UniStormClouds.CloudPerformance)CloudQuality, (UniStormClouds.CloudType)UniStormSystem.Instance.CloudType, UniStormClouds.CloudShadowsType.Simulated);
                }
                else if (UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Disabled)
                {
                    m_UniStormClouds.SetCloudDetails((UniStormClouds.CloudPerformance)CloudQuality, (UniStormClouds.CloudType)UniStormSystem.Instance.CloudType, UniStormClouds.CloudShadowsType.Off);
                }

                if (CloudQuality == UniStormSystem.CloudQualityEnum.Ultra)
                {                    
                    m_CloudsMaterial.SetFloat("_DistantCloudUpdateSpeed", 75);
                    Shader.SetGlobalFloat("CLOUD_MARCH_STEPS", 100);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", 10);
                }
            }
        }

        /// <summary>
        /// Updates UniStorm's Cloud Type to be either Volumetric or 2D
        /// </summary>
        public void UpdateCloudType(UniStormSystem.CloudTypeEnum CloudType)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.CloudType = CloudType;
                UniStormClouds m_UniStormClouds = FindObjectOfType<UniStormClouds>();
                Material m_CloudsMaterial = FindObjectOfType<UniStormClouds>().skyMaterial;

                if (UniStormSystem.Instance.CloudType == UniStormSystem.CloudTypeEnum._2D)
                {
                    m_CloudsMaterial.SetFloat("_uCloudsBaseEdgeSoftness", 0.05f);
                    m_CloudsMaterial.SetFloat("_uCloudsBottomSoftness", 0.15f);
                    m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", 0.1f);
                    m_CloudsMaterial.SetFloat("_uCloudsDensity", 1f);
                    m_CloudsMaterial.SetFloat("_uCloudsBaseScale", 1.5f);
                    m_CloudsMaterial.SetFloat("_uCloudsDetailScale", 700f);
                }
                else if (UniStormSystem.Instance.CloudType == UniStormSystem.CloudTypeEnum.Volumetric)
                {
                    CloudProfile m_CP = UniStormSystem.Instance.CurrentWeatherType.CloudProfileComponent;
                    m_CloudsMaterial.SetFloat("_uCloudsBaseEdgeSoftness", m_CP.EdgeSoftness);
                    m_CloudsMaterial.SetFloat("_uCloudsBottomSoftness", m_CP.BaseSoftness);
                    m_CloudsMaterial.SetFloat("_uCloudsDensity", m_CP.Density);
                    m_CloudsMaterial.SetFloat("_uCloudsBaseScale", 1.72f);
                    m_CloudsMaterial.SetFloat("_uCloudsDetailScale", 1000f);

                    if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    {
                        m_CloudsMaterial.SetFloat("_uCloudsCoverageBias", m_CP.CoverageBias);
                        m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", m_CP.DetailStrength);
                    }
                    else
                    {
                        m_CloudsMaterial.SetFloat("_uCloudsCoverageBias", 0.02f);
                        m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", 0.072f);
                    }
                }

                if (UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                {
                    m_UniStormClouds.SetCloudDetails((UniStormClouds.CloudPerformance)UniStormSystem.Instance.CloudQuality, 
                        (UniStormClouds.CloudType)UniStormSystem.Instance.CloudType, UniStormClouds.CloudShadowsType.Simulated);
                }
                else if (UniStormSystem.Instance.CloudShadows == UniStormSystem.EnableFeature.Disabled)
                {
                    m_UniStormClouds.SetCloudDetails((UniStormClouds.CloudPerformance)UniStormSystem.Instance.CloudQuality, 
                        (UniStormClouds.CloudType)UniStormSystem.Instance.CloudType, UniStormClouds.CloudShadowsType.Off);
                }
            }
        }

        /// <summary>
        /// Gets the current dominating precipitation type. This can only be Rain or Snow and is based off of a Weather Type's Shader Control Type.
        /// </summary>
        public string GetCurrentPrecipitationType()
        {
            if (Shader.GetGlobalFloat("_WetnessStrength") > Shader.GetGlobalFloat("_SnowStrength"))
            {
                return "Rain";
            }
            else if (Shader.GetGlobalFloat("_SnowStrength") > Shader.GetGlobalFloat("_WetnessStrength"))
            {
                return "Snow";
            }
            else
            {
                return "None";
            }
        }

        /// <summary>
        /// Controls whether or not the Sun Shafts effect is enabled or disabled using a bool, if it is set to be used within the UniStorm Editor.
        /// </summary>
        public void SetSunShaftsState(bool CurrentState)
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                //UniStormSystem.Instance.PlayerCamera.GetComponent<UniStorm.Effects.UniStormSunShafts>().enabled = CurrentState;
                UniStormSystem.Instance.SunShaftsEffect = CurrentState ? UniStormSystem.EnableFeature.Enabled : UniStormSystem.EnableFeature.Disabled;
                UniStormSystem.Instance.MoonShaftsEffect = CurrentState ? UniStormSystem.EnableFeature.Enabled : UniStormSystem.EnableFeature.Disabled;
            }
        }

        /// <summary>
        /// Updates the sun's settings
        /// </summary>
        public void UpdateSunlightSettings()
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                UniStormSystem.Instance.m_SunLight.shadowResolution = UniStormSystem.Instance.SunShadowResolution;
                UniStormSystem.Instance.m_SunLight.shadows = UniStormSystem.Instance.SunShadowType;
            }
        }
    }
}