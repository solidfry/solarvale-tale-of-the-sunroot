using System.Collections.Generic;
using Gaia.Pipeline.HDRP;
using Gaia.Pipeline.URP;
#if GAIA_PRO_PRESENT
using ProceduralWorlds.HDRPTOD;
#endif
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
#if UPPipeline
using UnityEngine.Rendering.Universal;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Gaia
{
    /// <summary>
    /// API calls that allows you to call some of Gaia features either in the Gaia Tools and Gaia Runtime systems.
    /// All these calls are static and can be called from this class without needing to reference any of the systems.
    /// To use it just call GaiaAPI anywhere in a .cs script and call the method you want and provide the required data the function needs
    /// </summary>
    public static class GaiaAPI
    {
        #region Gaia Time Of Day

#if GAIA_PRO_PRESENT
        /// <summary>
        /// Gets all the time of day settings
        /// </summary>
        /// <returns></returns>
        public static GaiaTimeOfDay GetTimeOfDaySettings()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                return GaiaGlobal.Instance.GaiaTimeOfDayValue;
            }
            else
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                GaiaTimeOfDay timeOfDay = new GaiaTimeOfDay();
                float time = HDRPTimeOfDayAPI.GetCurrentTime();
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float timeScale);
                timeOfDay.m_todDayTimeScale = timeScale;
                timeOfDay.m_todEnabled = autoUpdate;
                timeOfDay.m_todHour = (int)time;
                timeOfDay.m_todMinutes = (time / 24 * 60f);
                return timeOfDay;
#else
                return null;
#endif
            }
        }
        /// <summary>
        /// Sets all the time of day settings
        /// </summary>
        /// <param name="newGaiaTimeOfDaySettings"></param>
        public static void SetTimeOfDaySettings(GaiaTimeOfDay newGaiaTimeOfDaySettings)
        {
            if (newGaiaTimeOfDaySettings != null)
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    GaiaGlobal.Instance.GaiaTimeOfDayValue = newGaiaTimeOfDaySettings;
                    GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
                }
                else
                {
                    float time = newGaiaTimeOfDaySettings.m_todHour + (newGaiaTimeOfDaySettings.m_todMinutes / 60f);
                    SetTimeOfDaySettingsHDRP(time, newGaiaTimeOfDaySettings.m_todEnabled, newGaiaTimeOfDaySettings.m_todDayTimeScale);
                }
            }
            else
            {
                Debug.LogError("The time of day profile data you have provided is null. Please make sure it's not null.");
            }
        }

        /// <summary>
        /// Sets all the time of day settings in HDRP
        /// </summary>
        /// <param name="time"></param>
        /// <param name="enabled"></param>
        /// <param name="timeScale"></param>
        public static void SetTimeOfDaySettingsHDRP(float time, bool enabled, float timeScale)
        {
#if HDPipeline && UNITY_2021_2_OR_NEWER
            HDRPTimeOfDayAPI.SetCurrentTime(time);
            HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(enabled, timeScale);
#endif
        }
        /// <summary>
        /// Gets the time of day settings in HDRP
        /// </summary>
        /// <param name="time"></param>
        /// <param name="enabled"></param>
        /// <param name="timeScale"></param>
        public static void GetTimeOfDaySettingsHDRP(out float time, out bool enabled, out float timeScale)
        {
            time = 0f;
            enabled = false;
            timeScale = 1f;

#if HDPipeline && UNITY_2021_2_OR_NEWER
            time = HDRPTimeOfDayAPI.GetCurrentTime();
            HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out enabled, out timeScale);
#endif
        }
        /// <summary>
        /// Gets the current hour value in time of day
        /// </summary>
        /// <returns></returns>
        public static int GetTimeOfDayHour()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                return GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todHour;
            }

            return 0;
        }
        /// <summary>
        /// Sets the time of day hour
        /// </summary>
        /// <param name="newHour"></param>
        public static void SetTimeOfDayHour(int newHour)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todHour = newHour;
                GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
            }
        }
        /// <summary>
        /// Gets current minute value in time of day
        /// </summary>
        /// <returns></returns>
        public static float GetTimeOfDayMinute()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                return GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todMinutes;
            }

            return 0;
        }
        /// <summary>
        /// Sets the time of day minute
        /// </summary>
        /// <param name="newMinute"></param>
        public static void SetTimeOfDayMinute(float newMinute)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todMinutes = newMinute;
                GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
            }
        }
        /// <summary>
        /// Gets if time of day is enabled
        /// </summary>
        /// <returns></returns>
        public static bool GetTimeOfDayEnabled()
        {
            if (GaiaUtils.GetActivePipeline() != GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    return GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todEnabled;
                }
            }
            else
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float value);
                return autoUpdate;
#endif
            }

            return false;
        }
        /// <summary>
        /// Sets if time of day is enabled
        /// </summary>
        /// <param name="timeOfDayEnabled"></param>
        public static void SetTimeOfDayEnabled(bool timeOfDayEnabled)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todEnabled = timeOfDayEnabled;
                GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
            }
            else
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float timeScale);
                HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(timeOfDayEnabled, timeScale);
#endif
            }
        }
        /// <summary>
        /// Gets the time of day scale. How quick Day/Night lasts
        /// Higher values makes Day/Night go faster
        /// </summary>
        /// <returns></returns>
        public static float GetTimeOfDayTimeScale()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                return GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todDayTimeScale;
            }
            else
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float timeScale);
                return timeScale;
#else
                return 0f;
#endif
            }
        }
        /// <summary>
        /// Sets the time scale
        /// Higher values makes Day/Night go faster
        /// </summary>
        /// <param name="newTimeScale"></param>
        public static void SetTimeOfDayTimeScale(float newTimeScale)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.GaiaTimeOfDayValue.m_todDayTimeScale = newTimeScale;
                GaiaGlobal.Instance.UpdateGaiaTimeOfDay(false);
            }
            else
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float timeScale);
                HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(autoUpdate, newTimeScale);
#endif
            }
        }
#endif

#endregion
            #region Gaia Weather

            /// <summary>
            /// Gets the gaia wind settings
            /// </summary>
            /// <param name="windSpeed"></param>
            /// <param name="windDirection"></param>
            public static bool GetGaiaWindSettings(out float windSpeed, out float windDirection, out bool overrideWind)
        {
            windSpeed = 0.35f;
            windDirection = 0f;
            overrideWind = false;

            PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
            if (values != null)
            {
                overrideWind = values.m_gaiaWindSettingsOverride;
            }

#if GAIA_PRO_PRESENT
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                windSpeed = ProceduralWorldsGlobalWeather.Instance.WindSpeed;
                windDirection = ProceduralWorldsGlobalWeather.Instance.WindDirection;
                return true;
            }
            else
            {
                WindZone windZone = GaiaUtils.FindOOT<WindZone>();
                if (windZone != null)
                {
                    windSpeed = windZone.windMain;
                    windDirection = windZone.transform.eulerAngles.y / 360f;
                    return true;
                }
            }
#else
            WindZone windZone = GaiaUtils.FindOOT<WindZone>();
            if (windZone != null)
            {
                windSpeed = windZone.windMain;
                windDirection = windZone.transform.eulerAngles.y / 360f;
                return true;
            }
#endif
            return false;
        }
        /// <summary>
        /// Sets the gaia wind settings
        /// </summary>
        /// <param name="windSpeed"></param>
        /// <param name="windDirection"></param>
        public static void SetGaiaWindSettings(float windSpeed, float windDirection)
        {
#if GAIA_PRO_PRESENT
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                ProceduralWorldsGlobalWeather.Instance.WindSpeed = windSpeed;
                ProceduralWorldsGlobalWeather.Instance.WindDirection = windDirection;
            }
            else
            {
                WindZone windZone = GaiaUtils.FindOOT<WindZone>();
                if (windZone != null)
                {
                    windZone.windMain = windSpeed;
                    windZone.transform.eulerAngles = new Vector3(windZone.transform.eulerAngles.x, windDirection * 360f, windZone.transform.eulerAngles.z);
                }
            }
#else
            WindZone windZone = GaiaUtils.FindOOT<WindZone>();
            if (windZone != null)
            {
                windZone.windMain = windSpeed;
                windZone.transform.eulerAngles = new Vector3(windZone.transform.eulerAngles.x, windDirection * 360f, windZone.transform.eulerAngles.z);
            }
#endif
        }

#if GAIA_PRO_PRESENT
        /// <summary>
        /// Checks if weather is in the scene
        /// </summary>
        /// <returns></returns>
        public static bool GaiaWeatherInScene()
        {
            return ProceduralWorldsGlobalWeather.Instance;
        }
        /// <summary>
        /// Sets if the weather transition effect should be isntant or sue the fade duration value
        /// </summary>
        /// <param name="instantWeatherTransition"></param>
        public static void SetInstantWeatherTransitionEffects(bool instantWeatherTransition)
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                weather.m_instantStartStop = instantWeatherTransition;
            }
        }
        /// <summary>
        /// Sets if the weather transition effect should be isntant or sue the fade duration value
        /// </summary>
        /// <param name="instantWeatherTransition"></param>
        public static bool GetInstantWeatherTransitionEffects()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                return weather.m_instantStartStop;
            }

            return false;
        }
        /// <summary>
        /// Sets the weather effects being enabled
        /// </summary>
        /// <param name="newWeatherEnabled"></param>
        public static void SetWeatherEnabled(bool newWeatherEnabled)
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                weather.SetWeatherStatus(newWeatherEnabled);
            }
        }
        /// <summary>
        /// Returns a bool to see if weather effects are enabled or not
        /// </summary>
        /// <returns></returns>
        public static bool GetWeatherEnabled()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                if (weather.m_disableWeatherFX)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Start raining if it is not already raining
        /// </summary>
        public static void StartWeatherRain()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                if (!weather.m_disableWeatherFX)
                {
                    if (!weather.IsRaining)
                    {
                        weather.PlayRain();
                    }
                }
            }
        }
        /// <summary>
        /// Stops raining if it is raining
        /// </summary>
        public static void StopWeatherRain()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                if (weather.IsRaining)
                {
                    weather.StopRain();
                }
            }
        }
        /// <summary>
        /// Start snowing if it is not already snowing
        /// </summary>
        public static void StartWeatherSnow()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                if (!weather.m_disableWeatherFX)
                {
                    if (!weather.IsSnowing)
                    {
                        weather.PlaySnow();
                    }
                }
            }
        }
        /// <summary>
        /// Stops snowing if it is snowing
        /// </summary>
        public static void StopWeatherSnow()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                if (weather.IsSnowing)
                {
                    weather.StopSnow();
                }
            }
        }
        /// <summary>
        /// Returns the IsRaining bool so you can check to see if it is raining
        /// </summary>
        /// <returns></returns>
        public static bool IsRaining()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                return weather.IsRaining;
            }

            return false;
        }
        /// <summary>
        /// Returns the IsSnowing bool so you can check to see if it is snowing
        /// </summary>
        /// <returns></returns>
        public static bool IsSnowing()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                return weather.IsSnowing;
            }

            return false;
        }
        /// <summary>
        /// Gets the current season settings
        /// </summary>
        /// <returns></returns>
        public static PWSkySeason GetWeatherSeasonSettings()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                PWSkySeason season = new PWSkySeason
                {
                    EnableSeasons = weather.EnableSeasons,
                    Season = weather.Season,
                    SeasonAutumnTint = weather.SeasonAutumnTint,
                    SeasonSpringTint = weather.SeasonSpringTint,
                    SeasonSummerTint = weather.SeasonSummerTint,
                    SeasonWinterTint = weather.SeasonWinterTint
                };
                return season;
            }

            return null;
        }
        /// <summary>
        /// Sets the new season setting
        /// </summary>
        /// <param name="season"></param>
        public static void SetWeatherSeasonSettings(PWSkySeason season)
        {
            if (season != null)
            {
                if (GaiaWeatherInScene())
                {
                    ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                    weather.Season = season.Season;
                    weather.SeasonAutumnTint = season.SeasonAutumnTint;
                    weather.SeasonSpringTint = season.SeasonSpringTint;
                    weather.SeasonSummerTint = season.SeasonSummerTint;
                    weather.SeasonWinterTint = season.SeasonWinterTint;
                    weather.UpdateAllSystems(false);
                }
            }
            else
            {
                Debug.LogError("Season settings is null");
            }
        }
        /// <summary>
        /// Gets the current wind settings
        /// </summary>
        /// <returns></returns>
        public static PWSkyWind GetWeatherWindSettings()
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                PWSkyWind wind = new PWSkyWind
                {
                    WindDirection = weather.WindDirection,
                    WindSpeed = weather.WindSpeed
                };

                return wind;
            }

            return null;
        }
        /// <summary>
        /// Sets the new wind settings
        /// </summary>
        /// <param name="wind"></param>
        public static void SetWeatherWindSettings(PWSkyWind wind)
        {
            if (wind != null)
            {
                if (GaiaWeatherInScene())
                {
                    ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
                    weather.WindDirection = wind.WindDirection;
                    weather.WindSpeed = wind.WindSpeed;
                    weather.UpdateAllSystems(false);
                }
            }
        }
        /// <summary>
        /// Gets the weather fade duration value
        /// </summary>
        /// <returns></returns>
        public static float GetWeatherFadeDuration()
        {
            if (GaiaWeatherInScene())
            {
                return ProceduralWorldsGlobalWeather.Instance.m_weatherFadeDuration;
            }

            return 0f;
        }
        /// <summary>
        /// Sets the weather fade duration this value is ignored if instant weather effects is enabled
        /// </summary>
        /// <param name="newWeatherFadeDuration"></param>
        public static void SetWeatherFadeDuration(float newWeatherFadeDuration)
        {
            if (GaiaWeatherInScene())
            {
                ProceduralWorldsGlobalWeather.Instance.m_weatherFadeDuration = newWeatherFadeDuration;
            }
        }
        /// <summary>
        /// Gets the additional linear fog value that is used too add or remove fog in PW Sky
        /// </summary>
        /// <returns></returns>
        public static float GetAdditionalFogLinear()
        {
            ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                return weather.AdditionalFogDistanceLinear;
            }

            return 0f;
        }
        /// <summary>
        /// Sets the additional linear fog value that is used too add or remove fog in PW Sky
        /// </summary>
        /// <param name="value"></param>
        public static void SetAdditionalFogLinear(float value)
        {
            ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                weather.AdditionalFogDistanceLinear = value;
                weather.DoesAtmosphereNeedUpdate = true;
            }
        }
        /// <summary>
        /// Gets the additional exponential fog value that is used too add or remove fog in PW Sky
        /// </summary>
        /// <returns></returns>
        public static float GetAdditionalFogExponential()
        {
            ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                return weather.AdditionalFogDistanceExponential;
            }

            return 0f;
        }
        /// <summary>
        /// Sets the additional exponential fog value that is used too add or remove fog in PW Sky
        /// </summary>
        /// <param name="value"></param>
        public static void SetAdditionalFogExponential(float value)
        {
            ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                weather.AdditionalFogDistanceExponential = value;
                weather.DoesAtmosphereNeedUpdate = true;
            }
        }
        /// <summary>
        /// Sets the additional fog color that is added or removed from PW sky
        /// </summary>
        /// <param name="value"></param>
        public static void SetAdditionalFogColor(Color value)
        {
            ProceduralWorldsGlobalWeather weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                weather.AdditionalFogColor = value;
                weather.DoesAtmosphereNeedUpdate = true;
            }
        }
#endif

        #endregion
        #region Gaia Lighting Setup

        /// <summary>
        /// Gets the current selected lighting profile settings
        /// </summary>
        /// <returns></returns>
        public static GaiaLightingProfileValues GetCurrentLightingProfileSettings()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                return sceneProfile.m_lightingProfiles[sceneProfile.m_selectedLightingProfileValuesIndex];
            }

            return null;
        }
        /// <summary>
        /// Gets the lighting profile settings by name
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public static GaiaLightingProfileValues GetLightingProfileByName(string profileName)
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                    for (int i = 0; i < sceneProfile.m_lightingProfiles.Count; i++)
                    {
                        GaiaLightingProfileValues values = sceneProfile.m_lightingProfiles[i];
                        if (values.m_typeOfLighting == profileName)
                        {
                            return sceneProfile.m_lightingProfiles[i];
                        }
                    }

                    Debug.LogWarning("The name you provided does not exist in the current lighting profiles. Please provide a valid name that exists in the current profiles.");
                }

                return null;
            }
            else
            {
                Debug.LogError("You did not provide a name");
                return null;
            }
        }
        /// <summary>
        /// Gets the lighting profile settings by index
        /// </summary>
        /// <param name="profileIndex"></param>
        /// <returns></returns>
        public static GaiaLightingProfileValues GetLightingProfileByIndex(int profileIndex)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                int count = sceneProfile.m_lightingProfiles.Count - 1;
                if (profileIndex < 0 || profileIndex > count)
                {
                    return sceneProfile.m_lightingProfiles[profileIndex];
                }

                Debug.LogError("The profile index you provided was either < 0 or > than the current profile index count. The profile index is ranged from 0 - " + count);
            }

            return null;
        }
        /// <summary>
        /// Updates the sun rotation on the Y axis
        /// </summary>
        /// <param name="value"></param>
        public static bool SetTimeOfDaySunRotation(float value)
        {
#if GAIA_PRO_PRESENT
            PW_VFX_Atmosphere atmosphere = PW_VFX_Atmosphere.Instance;
            if (atmosphere != null)
            {
                atmosphere.m_sunRotation = value;
                return true;
            }
#endif
            return false;
        }
        /// <summary>
        /// Updates the sun rotation on the Y axis
        /// </summary>
        /// <param name="value"></param>
        public static float GetTimeOfDaySunRotation()
        {
#if GAIA_PRO_PRESENT
            PW_VFX_Atmosphere atmosphere = PW_VFX_Atmosphere.Instance;
            if (atmosphere != null)
            {
                return atmosphere.m_sunRotation;
            }
#endif
            return 0f;
        }
        /// <summary>
        /// Sets the sun pitch on the X axis and the rotation on the Y axis
        /// </summary>
        /// <param name="sunPitch"></param>
        /// <param name="sunRotation"></param>
        /// <param name="sunLight"></param>
        public static void SetSunRotation(float sunPitch, float sunRotation, Light sunLight = null)
        {
            Vector3 rotation = new Vector3(sunPitch, sunRotation, 0f);
            if (sunLight == null)
            {
                Light sun = GaiaUtils.GetMainDirectionalLight(false);
                if (sun != null)
                {
                    sun.transform.eulerAngles = rotation;
                }
            }
            else
            {
                sunLight.transform.eulerAngles = rotation;
            }
        }
        /// <summary>
        /// Gets the sun pitch on the X axis and the rotation on the Y axis
        /// </summary>
        /// <param name="sunPitch"></param>
        /// <param name="sunRotation"></param>
        public static void GetSunRotation(out float sunPitch, out float sunRotation)
        {
            sunPitch = 0f;
            sunRotation = 0f;

            Light sun = GaiaUtils.GetMainDirectionalLight(false);
            if (sun != null)
            {
                sunPitch = sun.transform.eulerAngles.x;
                sunRotation = sun.transform.eulerAngles.y;
            }
        }
        /// <summary>
        /// Gets the sun pitch on the X axis and the rotation on the Y axis
        /// </summary>
        /// <param name="sunPitch"></param>
        /// <param name="sunRotation"></param>
        /// <param name="sun"></param>
        public static void GetSunRotation(out float sunPitch, out float sunRotation, Light sun)
        {
            sunPitch = 0f;
            sunRotation = 0f;

            if (sun != null)
            {
                sunPitch = sun.transform.eulerAngles.x;
                sunRotation = sun.transform.eulerAngles.y;
            }
        }
        /// <summary>
        /// Gets unity HDRI skybox settings
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static bool GetUnityHDRISkybox(out float exposure, out float rotation, out Color tint, out bool overrideSkybox)
        {
            exposure = 0f;
            rotation = 0f;
            tint = Color.white;
            overrideSkybox = false;

            Material skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                if (skybox.shader != null)
                {
                    if (skybox.shader.name == GaiaShaderID.m_unitySkyboxShaderHDRI || skybox.shader.name == GaiaShaderID.m_unitySkyboxShaderHDRIDefault)
                    {
                        exposure = skybox.GetFloat(GaiaShaderID.m_unitySkyboxExposure);
                        rotation = skybox.GetFloat(GaiaShaderID.m_unitySkyboxRotation);
                        tint = skybox.GetColor(GaiaShaderID.m_unitySkyboxTintHDRI);

                        PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
                        if (values != null)
                        {
                            overrideSkybox = values.m_fogOverride;
                        }
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Sets unity HDRI skybox settings
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static void SetUnityHDRISkybox(float exposure, float rotation, Color tint)
        {
            Material skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                if (skybox.shader != null)
                {
                    if (skybox.shader.name == GaiaShaderID.m_unitySkyboxShaderHDRI || skybox.shader.name == GaiaShaderID.m_unitySkyboxShaderHDRIDefault)
                    {
                        skybox.SetFloat(GaiaShaderID.m_unitySkyboxExposure, exposure);
                        skybox.SetFloat(GaiaShaderID.m_unitySkyboxRotation, rotation);
                        skybox.SetColor(GaiaShaderID.m_unitySkyboxTintHDRI, tint);
                    }
                }
            }
        }
        /// <summary>
        /// Gets unity sun intensity and color
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="sunColor"></param>
        public static void GetUnitySunSettings(out float intensity, out Color sunColor, out float kelvin, out bool overrideSun, Light sunLight = null)
        {
            intensity = 1f;
            kelvin = 6500f;
            sunColor = Color.white;
            if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
#if HDPipeline
                GetUnitySunSettingsHDRP(out intensity, out sunColor, out kelvin, sunLight);
#endif
            }
            else
            {
                if (sunLight == null)
                {
                    sunLight = GaiaUtils.GetMainDirectionalLight(false);
                    if (sunLight != null)
                    {
                        intensity = sunLight.intensity;
                        sunColor = sunLight.color;
                        kelvin = sunLight.colorTemperature;
                    }
                }
                else
                {
                    intensity = sunLight.intensity;
                    sunColor = sunLight.color;
                    kelvin = sunLight.colorTemperature;
                }
            }

            overrideSun = false;
            PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
            if (values != null)
            {
                overrideSun = values.m_fogOverride;
            }
        }
        /// <summary>
        /// Sets unity sun intensity and color
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="sunColor"></param>
        /// <param name="sunLight"></param>
        public static void SetUnitySunSettings(float intensity, Color sunColor, float kelvin, Light sunLight = null)
        {
            if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
#if HDPipeline
                SetUnitySunSettingsHDRP(intensity, kelvin, sunLight);
#endif
            }
            else
            {
                if (sunLight == null)
                {
                    sunLight = GaiaUtils.GetMainDirectionalLight(false);
                    if (sunLight != null)
                    {
                        sunLight.intensity = intensity;
                        sunLight.color = sunColor;
                        sunLight.colorTemperature = kelvin;
                    }
                }
                else
                {
                    sunLight.intensity = intensity;
                    sunLight.color = sunColor;
                    sunLight.colorTemperature = kelvin;
                }
            }
        }

        /// <summary>
        /// Gets ambient intensity
        /// </summary>
        /// <returns></returns>
        public static float GetAmbientIntensity()
        {
            if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
#if HDPipeline
                return GetHDRPAmbientIntensity();
#else
                return 1f;
#endif
            }
            else
            {
                return RenderSettings.ambientIntensity;
            }
        }
        /// <summary>
        /// Sets ambient intensity
        /// </summary>
        /// <returns></returns>
        public static void SetAmbientIntensity(float value)
        {
            if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
#if HDPipeline
                SetHDRPAmbientIntensity(value);
#endif
            }
            else
            {
                RenderSettings.ambientIntensity = value;
            }
        }
        /// <summary>
        /// Sets the ambient color for sky, equaotr and ground colors
        /// </summary>
        /// <param name="skyValue"></param>
        /// <param name="equatorValue"></param>
        /// <param name="groundValue"></param>
        public static void SetAmbientColor(Color skyValue, Color equatorValue, Color groundValue)
        {
            if (GaiaUtils.GetActivePipeline() != GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                RenderSettings.ambientSkyColor = skyValue * skyValue.a;
                RenderSettings.ambientEquatorColor = equatorValue * equatorValue.a;
                RenderSettings.ambientGroundColor = groundValue * groundValue.a;
            }
        }
        /// <summary>
        /// Gets the ambient colors
        /// </summary>
        /// <param name="skyValue"></param>
        /// <param name="equatorValue"></param>
        /// <param name="groundValue"></param>
        public static void GetAmbientColor(out Color skyValue, out Color equatorValue, out Color groundValue)
        {

            skyValue = RenderSettings.ambientSkyColor;
            //skyValue.a = GetHDRIntensityValue(skyValue);
            skyValue.a = 1f;
            equatorValue = RenderSettings.ambientEquatorColor;
            //equatorValue.a = GetHDRIntensityValue(equatorValue);
            equatorValue.a = 1f;
            groundValue = RenderSettings.ambientGroundColor;
            //groundValue.a = GetHDRIntensityValue(groundValue);
            groundValue.a = 1f;
        }

        public static float GetHDRIntensityValue(Color color)
        {
            return (color.r + color.g + color.b) / 3f;
        }
        /// <summary>
        /// Gets the built-in and Universal render pipeline fog settings
        /// </summary>
        /// <param name="fogMode"></param>
        /// <param name="fogColor"></param>
        /// <param name="fogDensity"></param>
        /// <param name="linearFogStart"></param>
        /// <param name="linearFogEnd"></param>
        public static void GetFogSettings(out FogMode fogMode, out Color fogColor, out float fogDensity, out float linearFogStart, out float linearFogEnd, out bool overrideFog)
        {
            fogMode = RenderSettings.fogMode;
            fogColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            linearFogStart = RenderSettings.fogStartDistance;
            linearFogEnd = RenderSettings.fogEndDistance;

            overrideFog = false;
            PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
            if (values != null)
            {
                overrideFog = values.m_fogOverride;
            }
        }
        /// <summary>
        /// Sets the built-in and Universal render pipeline fog settings
        /// </summary>
        /// <param name="fogMode"></param>
        /// <param name="fogColor"></param>
        /// <param name="fogDensity"></param>
        /// <param name="linearFogStart"></param>
        /// <param name="linearFogEnd"></param>
        public static void SetFogSettings(FogMode fogMode, Color fogColor, float fogDensity, float linearFogStart, float linearFogEnd)
        {
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = linearFogStart;
            RenderSettings.fogEndDistance = linearFogEnd;
        }

        #region URP

#if UPPipeline
        public static float GetURPShadowDistance()
        {
            UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            if (asset != null)
            {
                return asset.shadowDistance;
            }

            return 0f;
        }
        public static void SetURPShadowDistance(float value)
        {
            UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            if (asset != null)
            {
                asset.shadowDistance = value;
            }
        }
        public static int GetURPShadowCasecade()
        {
            UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            if (asset != null)
            {
                return asset.shadowCascadeCount;
            }

            return 0;
        }
        public static void SetURPShadowCasecade(int value)
        {
            UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            if (asset != null)
            {
                if (value == 0)
                {
                    value = 1;
                }
                else if (value > 4)
                {
                    value = 4;
                }
                asset.shadowCascadeCount = value;
            }
        }
        public static int GetURPShadowResolution()
        {
            UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            if (asset != null)
            {
                return asset.mainLightShadowmapResolution;
            }

            return 1024;
        }
#endif

        #endregion
        #region HDRP
#if HDPipeline
        /// <summary>
        /// Gets HDRP Ambient lighting
        /// </summary>
        /// <returns></returns>
        public static float GetHDRPAmbientIntensity()
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out IndirectLightingController ambient))
                {
                    return ambient.indirectDiffuseLightingMultiplier.value;
                }
            }

            return 1f;
        }
        /// <summary>
        /// Gets HDRP dof mode
        /// </summary>
        /// <returns></returns>
        public static int GetHDRPDOFFocusMode()
        {
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.DepthOfField dof))
                {
                    return (int)dof.focusMode.value;
                }
            }

            return 0;
        }
        /// <summary>
        /// Sets HDRP dof mode
        /// </summary>
        /// <param name="value"></param>
        public static void SetHDRPDOFFocusMode(int value)
        {
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.DepthOfField dof))
                {
                    dof.focusMode.value = (DepthOfFieldMode)value;
                }
            }
        }
        /// <summary>
        /// Sets HDRP Ambient lighting
        /// </summary>
        /// <param name="value"></param>
        public static void SetHDRPAmbientIntensity(float value)
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out IndirectLightingController ambient))
                {
                    ambient.indirectDiffuseLightingMultiplier.value = value;
                }
            }
        }
        /// <summary>
        /// Gets unity sun intensity and color
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="sunColor"></param>
        public static void GetUnitySunSettingsHDRP(out float intensity, out Color sunColor, out float kelvin, Light sunLight = null)
        {
            intensity = 1f;
            sunColor = Color.white;
            kelvin = 6500f;

            if (sunLight == null)
            {
                sunLight = GaiaUtils.GetMainDirectionalLight(false);
                if (sunLight != null)
                {
                    HDAdditionalLightData data = GaiaHDRPRuntimeUtils.GetHDLightData(sunLight);
                    intensity = data.intensity;
                    sunColor = data.surfaceTint;
                    kelvin = sunLight.colorTemperature;
                }
            }
            else
            {
                HDAdditionalLightData data = sunLight.GetComponent<HDAdditionalLightData>();
                if (data == null)
                {
                    data = sunLight.gameObject.AddComponent<HDAdditionalLightData>();
                }

                intensity = data.intensity;
                sunColor = data.surfaceTint;
                kelvin = sunLight.colorTemperature;
            }
        }
        /// <summary>
        /// Sets unity sun intensity and color
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="sunColor"></param>
        /// <param name="sunLight"></param>
        public static void SetUnitySunSettingsHDRP(float intensity, float kelvin, Light sunLight = null)
        {
            if (sunLight == null)
            {
                sunLight = GaiaUtils.GetMainDirectionalLight(false);
                if (sunLight != null)
                {
                    HDAdditionalLightData data = sunLight.GetComponent<HDAdditionalLightData>();
                    if (data == null)
                    {
                        data = sunLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }

                    data.SetIntensity(intensity);
                    data.EnableColorTemperature(true);
                    sunLight.colorTemperature = kelvin;
                }
            }
            else
            {
                HDAdditionalLightData data = sunLight.GetComponent<HDAdditionalLightData>();
                if (data == null)
                {
                    data = sunLight.gameObject.AddComponent<HDAdditionalLightData>();
                }

                data.SetIntensity(intensity);
                data.EnableColorTemperature(true);
                sunLight.colorTemperature = kelvin;
            }
        }
#endif

        #endregion

        #endregion
        #region Gaia Water Setup

        /// <summary>
        /// Gets the current selected water profile settings
        /// </summary>
        /// <returns></returns>
        public static GaiaWaterProfileValues GetCurrentWaterProfileSettings()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                return sceneProfile.m_waterProfiles[sceneProfile.m_selectedWaterProfileValuesIndex];
            }

            return null;
        }
        /// <summary>
        /// Gets the water profile settings by name
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public static GaiaWaterProfileValues GetWaterProfileByName(string profileName)
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                    for (int i = 0; i < sceneProfile.m_lightingProfiles.Count; i++)
                    {
                        GaiaWaterProfileValues values = sceneProfile.m_waterProfiles[i];
                        if (values.m_typeOfWater == profileName)
                        {
                            return sceneProfile.m_waterProfiles[i];
                        }
                    }

                    Debug.LogWarning("The name you provided does not exist in the current water profiles. Please provide a valid name that exists in the current profiles.");
                }

                return null;
            }
            else
            {
                Debug.LogError("You did not provide a name");
                return null;
            }
        }
        /// <summary>
        /// Gets the lighting profile settings by index
        /// </summary>
        /// <param name="profileIndex"></param>
        /// <returns></returns>
        public static GaiaWaterProfileValues GetWaterProfileByIndex(int profileIndex)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                int count = sceneProfile.m_waterProfiles.Count - 1;
                if (profileIndex < 0 || profileIndex > count)
                {
                    return sceneProfile.m_waterProfiles[profileIndex];
                }

                Debug.LogError("The profile index you provided was either < 0 or > than the current profile index count. The profile index is ranged from 0 - " + count);
            }

            return null;
        }
        /// <summary>
        /// Sets the water reflections state
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetWaterReflections(bool enabled)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                sceneProfile.m_enableReflections = enabled;
                sceneProfile.m_reflectionSettingsData.m_enableReflections = enabled;
                if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GaiaPlanarReflectionsHDRP hdrpReflection = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();

                    if (hdrpReflection != null)
                    {
                        hdrpReflection.ReflectionsActive(enabled);
                        hdrpReflection.RequestRender = true;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the water reflection distance
        /// </summary>
        /// <param name="extraDistance"></param>
        public static void SetWaterReflectionExtraDistance(float extraDistance)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;

                //Single layer
                float savedCustomDistance = sceneProfile.m_savedWaterRenderDistance;
                if (extraDistance >= 0)
                {
                    savedCustomDistance += extraDistance;
                }
                else
                {
                    savedCustomDistance -= Mathf.Abs(extraDistance);
                    savedCustomDistance = Mathf.Clamp(savedCustomDistance, 0.01f, Mathf.Infinity);
                }

                sceneProfile.m_customRenderDistance = savedCustomDistance;
                //All layers
                float[] savedCustomDistances = new float[32];
                for (int i = 0; i < savedCustomDistances.Length; i++)
                {
                    savedCustomDistances[i] = sceneProfile.m_savedWaterRenderDistances[i];
                    if (savedCustomDistances[i] != 0f)
                    {
                        if (extraDistance >= 0)
                        {
                            savedCustomDistances[i] += extraDistance;
                        }
                        else
                        {
                            savedCustomDistances[i] -= Mathf.Abs(extraDistance);
                            savedCustomDistances[i] = Mathf.Clamp(savedCustomDistances[i], 0.01f, Mathf.Infinity);
                        }
                    }

                    sceneProfile.m_reflectionSettingsData.m_customRenderDistances[i] = savedCustomDistances[i];
                }
                sceneProfile.m_customRenderDistances = savedCustomDistances;
                sceneProfile.m_reflectionSettingsData.m_customRenderDistance = savedCustomDistance;
            }
        }
        /// <summary>
        /// Sets the water reflection distance
        /// </summary>
        /// <param name="extraDistance"></param>
        public static void SetWaterReflectionDistances(float[] distances, float distance)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;

                //Single layer
                float savedCustomDistance = distance;
                sceneProfile.m_customRenderDistance = distance;
                //All layers
                float[] savedCustomDistances = new float[32];
                for (int i = 0; i < savedCustomDistances.Length; i++)
                {
                    savedCustomDistances[i] = distances[i];
                    sceneProfile.m_reflectionSettingsData.m_customRenderDistances[i] = savedCustomDistances[i];
                }
                sceneProfile.m_customRenderDistances = savedCustomDistances;
                sceneProfile.m_reflectionSettingsData.m_customRenderDistance = savedCustomDistance;
            }
        }
        /// <summary>
        /// Gets the shadow resolution
        /// </summary>
        /// <returns></returns>
        public static int GetWaterResolutionQuality()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                switch (GaiaUtils.GetActivePipeline())
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        return GetHDRPWaterResolutionQuality();
#else
                        break;
#endif
                    }
                    default:
                    {
                        switch (sceneProfile.m_reflectionResolution)
                        {
                            case GaiaConstants.GaiaProWaterReflectionsQuality.Resolution256:
                                return 1;
                            case GaiaConstants.GaiaProWaterReflectionsQuality.Resolution512:
                                return 2;
                            case GaiaConstants.GaiaProWaterReflectionsQuality.Resolution1024:
                                return 3;
                            case GaiaConstants.GaiaProWaterReflectionsQuality.Resolution2048:
                                return 4;
                            case GaiaConstants.GaiaProWaterReflectionsQuality.Resolution4096:
                                return 5;
                            default:
                                return 0;
                        }
                    }
                }
            }

            return 0;
        }
        /// <summary>
        /// Sets the water reflection resolution
        /// </summary>
        /// <param name="value"></param>
        public static void SetWaterResolutionQuality(int value)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                if (value > 5)
                {
                    value = 5;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                switch (GaiaUtils.GetActivePipeline())
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        SetHDRPWaterResolutionQuality(value);
#endif
                        break;
                    }
                    default:
                    {
                        switch (value)
                        {
                            case 0:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution128;
                                break;
                            case 1:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution256;
                                break;
                            case 2:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution512;
                                break;
                            case 3:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution1024;
                                break;
                            case 4:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution2048;
                                break;
                            case 5:
                                sceneProfile.m_reflectionResolution = GaiaConstants.GaiaProWaterReflectionsQuality.Resolution4096;
                                break;
                        }
                        break;
                    }
                }

                sceneProfile.UpdateTextureResolution();
            }
        }
        /// <summary>
        /// Sets the water reflection resolution
        /// </summary>
        /// <param name="value"></param>
        public static void SetWaterResolutionQuality(GaiaConstants.GaiaProWaterReflectionsQuality value)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                sceneProfile.m_reflectionResolution = value;

                sceneProfile.UpdateTextureResolution();
            }
        }
        /// <summary>
        /// Gets the underwater fog color multiplier
        /// </summary>
        /// <returns></returns>
        public static Color GetUnderwaterFogColor()
        {
            GaiaUnderwaterEffects underwaterEffects = GaiaUnderwaterEffects.Instance;
            if (underwaterEffects != null)
            {
                return underwaterEffects.m_fogColorMultiplier;
            }

            return Color.black;
        }
        /// <summary>
        /// Sets the underwater fog color multiplier
        /// </summary>
        /// <param name="color"></param>
        public static void SetUnderwaterFogColor(Color color)
        {
            GaiaUnderwaterEffects underwaterEffects = GaiaUnderwaterEffects.Instance;
            if (underwaterEffects != null)
            {
                underwaterEffects.m_fogColorMultiplier = color;
            }
        }
        /// <summary>
        /// Gets the underwater fog color multiplier
        /// </summary>
        /// <returns></returns>
        public static void GetUnderwaterFogDensity(out float fogDensity, out float fogDistance)
        {
            fogDensity = 0.045f;
            fogDistance = 45f;
            GaiaUnderwaterEffects underwaterEffects = GaiaUnderwaterEffects.Instance;
            if (underwaterEffects != null)
            {
                fogDensity = underwaterEffects.m_fogDensity;
                if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    fogDistance = underwaterEffects.m_hdrpFogDistance;
                }
                else
                {
                    fogDistance = underwaterEffects.m_fogDistance;
                }
            }
        }
        /// <summary>
        /// Sets the underwater fog color multiplier
        /// </summary>
        /// <param name="color"></param>
        public static void SetUnderwaterFogDensity(float fogDensity, float fogDistance)
        {
            GaiaUnderwaterEffects underwaterEffects = GaiaUnderwaterEffects.Instance;
            if (underwaterEffects != null)
            {
                underwaterEffects.m_fogDensity = fogDensity;
                if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    underwaterEffects.m_hdrpFogDistance = fogDistance;
                }
                else
                {
                    underwaterEffects.m_fogDistance = fogDistance;
                }
            }
        }
        /// <summary>
        /// Gets the underwater volume
        /// </summary>
        /// <returns></returns>
        public static float GetUnderwaterVolume()
        {
            if (GaiaUnderwaterEffects.Instance != null)
            {
                return GaiaUnderwaterEffects.Instance.m_playbackVolume;
            }

            return 0f;
        }
        /// <summary>
        /// Sets the underwater volume
        /// </summary>
        /// <returns></returns>
        public static void SetUnderwaterVolume(float value)
        {
            if (GaiaUnderwaterEffects.Instance != null)
            {
                GaiaUnderwaterEffects.Instance.SetNewUnderwaterSoundFXVolume(value);
            }
        }

        #region HDRP

#if HDPipeline
        /// <summary>
        /// Gets the hdrp water reflections quality
        /// </summary>
        /// <param name="planarProbe"></param>
        /// <returns></returns>
        public static int GetHDRPWaterResolutionQuality(GaiaPlanarReflectionsHDRP planarProbe = null)
        {
            if (planarProbe == null)
            {
                planarProbe = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();
                if (planarProbe != null)
                {
                    PlanarReflectionProbe reflections = planarProbe.m_reflections;
                    if (reflections != null)
                    {
                        return (int) reflections.settingsRaw.resolutionScalable.@override;
                    }
                }
            }
            else
            {
                PlanarReflectionProbe reflections = planarProbe.m_reflections;
                if (reflections != null)
                {
                    return (int) reflections.settingsRaw.resolutionScalable.@override;
                }
            }

            return 0;
        }
        /// <summary>
        /// Sets the hdrp water reflections quality
        /// </summary>
        /// <param name="value"></param>
        /// <param name="planarProbe"></param>
        public static void SetHDRPWaterResolutionQuality(int value, GaiaPlanarReflectionsHDRP planarProbe = null)
        {
            //Fixes out of range and bounds it max to 1024 resolution
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 4)
            {
                value = 4;
            }

            if (planarProbe == null)
            {
                planarProbe = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();
                if (planarProbe != null)
                {
                    PlanarReflectionProbe reflections = planarProbe.m_reflections;
                    if (reflections != null)
                    {
                        switch (value)
                        {
                            case 0:
                            {
                                reflections.settingsRaw.resolutionScalable.useOverride = true;
                                reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution64;
                                break;
                            }
                            case 1:
                            {
                                reflections.settingsRaw.resolutionScalable.useOverride = true;
                                reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution128;
                                break;
                            }
                            case 2:
                            {
                                reflections.settingsRaw.resolutionScalable.useOverride = true;
                                reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution256;
                                break;
                            }
                            case 3:
                            {
                                reflections.settingsRaw.resolutionScalable.useOverride = true;
                                reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution512;
                                break;
                            }
                            case 4:
                            {
                                reflections.settingsRaw.resolutionScalable.useOverride = true;
                                reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution1024;
                                break;
                            }
                        }
                       
                        planarProbe.RequestReflectionRender();
                    }
                }
            }
            else
            {
                PlanarReflectionProbe reflections = planarProbe.m_reflections;
                if (reflections != null)
                {
                    switch (value)
                    {
                        case 0:
                        {
                            reflections.settingsRaw.resolutionScalable.useOverride = true;
                            reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution64;
                            break;
                        }
                        case 1:
                        {
                            reflections.settingsRaw.resolutionScalable.useOverride = true;
                            reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution128;
                            break;
                        }
                        case 2:
                        {
                            reflections.settingsRaw.resolutionScalable.useOverride = true;
                            reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution256;
                            break;
                        }
                        case 3:
                        {
                            reflections.settingsRaw.resolutionScalable.useOverride = true;
                            reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution512;
                            break;
                        }
                        case 4:
                        {
                            reflections.settingsRaw.resolutionScalable.useOverride = true;
                            reflections.settingsRaw.resolutionScalable.@override = PlanarReflectionAtlasResolution.Resolution1024;
                            break;
                        }
                    }
                   
                    planarProbe.RequestReflectionRender();
                }
            }
        }
        /// <summary>
        /// Gets LOD bias in HDRP
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camera"></param>
        public static float GetHDRPWaterLODBias(GaiaPlanarReflectionsHDRP planarProbe = null)
        {
            if (planarProbe == null)
            {
                planarProbe = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();
                if (planarProbe != null)
                {
                    PlanarReflectionProbe reflections = planarProbe.m_reflections;
                    if (reflections != null)
                    {
                        return reflections.frameSettings.lodBias;
                    }
                }
            }
            else
            {
                PlanarReflectionProbe reflections = planarProbe.m_reflections;
                if (reflections != null)
                {
                    return reflections.frameSettings.lodBias;
                }
            }

            return 1f;
        }
        /// <summary>
        /// Sets LOD bias in HDRP
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camera"></param>
        public static void SetHDRPWaterLODBias(float value, GaiaPlanarReflectionsHDRP planarProbe = null)
        {
            if (planarProbe == null)
            {
                planarProbe = GetGaiaOceanPlanarReflections();

                if (planarProbe != null)
                {
                    PlanarReflectionProbe reflections = planarProbe.m_reflections;
                    if (reflections != null)
                    {
                        reflections.settingsRaw.cameraSettings.customRenderingSettings = true;
                        reflections.frameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
                        reflections.frameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBiasMode] = true;
                        reflections.frameSettings.lodBias = value;
                        reflections.frameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBias] = true;
                        planarProbe.RequestReflectionRender();
                    }
                }
            }
            else
            {
                PlanarReflectionProbe reflections = planarProbe.m_reflections;
                if (reflections != null)
                {
                    reflections.settingsRaw.cameraSettings.customRenderingSettings = true;
                    reflections.frameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
                    reflections.frameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBiasMode] = true;
                    reflections.frameSettings.lodBias = value;
                    reflections.frameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBias] = true;
                    planarProbe.RequestReflectionRender();
                }
            }
        }
        /// <summary>
        /// Gets the Gaia HDRP Planar Reflection system from Gaia water system
        /// </summary>
        /// <returns></returns>
        public static GaiaPlanarReflectionsHDRP GetGaiaOceanPlanarReflections()
        {
            PWS_WaterSystem water = PWS_WaterSystem.Instance;
            if (water != null)
            {
                GaiaPlanarReflectionsHDRP hdrpPlanar = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();
                if (hdrpPlanar != null)
                {
                    return hdrpPlanar;
                }
            }
            else
            {
                GaiaPlanarReflectionsHDRP hdrpPlanar = GaiaUtils.FindOOT<GaiaPlanarReflectionsHDRP>();
                if (hdrpPlanar != null)
                {
                    return hdrpPlanar;
                }
            }

            return null;
        }
#endif

        #endregion

        #endregion
        #region Gaia Post Processing

#if UNITY_POST_PROCESSING_STACK_V2
        /// <summary>
        /// Gets unity post processign v2 depth of field
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Rendering.PostProcessing.DepthOfField GetDepthOfFieldSettings()
        {
            PostProcessProfile profile = GetGlobalProcessingProfile();
            if (profile != null)
            {
                if (profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.DepthOfField dof))
                {
                    return dof;
                }
            }

            return null;
        }
        /// <summary>
        /// Sets unity post processign v2 depth of field
        /// </summary>
        /// <param name="dof"></param>
        public static void SetDepthOfFieldSettings(PhotoModeValues photoModeProfile)
        {
            if (photoModeProfile == null)
            {
                return;
            }

            PostProcessVolume[] volumes = GaiaUtils.FindOOTs<PostProcessVolume>();
            PostProcessProfile profile = null;
            if (volumes.Length > 0)
            {
                foreach (PostProcessVolume volume in volumes)
                {
                    if (volume.name.Contains("Global Post Processing") || volume.name.Contains("Post FX"))
                    {
                        profile = volume.sharedProfile;
                        break;
                    }
                }
            }

            if (profile != null)
            {
                if (profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.DepthOfField currentDof))
                {
                    currentDof.active = photoModeProfile.m_dofActive;
                    currentDof.aperture.value = photoModeProfile.m_dofAperture;
                    currentDof.focalLength.value = photoModeProfile.m_dofFocalLength;
                    currentDof.focusDistance.value = photoModeProfile.m_dofFocusDistance;
                    currentDof.kernelSize.value = (KernelSize)photoModeProfile.m_dofKernelSize;
                }
            }
        }
        /// <summary>
        /// Copies the depth of field settings from source to dest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyDepthOfFieldProperties(UnityEngine.Rendering.PostProcessing.DepthOfField source, UnityEngine.Rendering.PostProcessing.DepthOfField dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            dest.active = source.active;
            dest.aperture.overrideState = source.aperture.overrideState;
            dest.aperture.value = source.aperture.value;
            dest.focalLength.overrideState = source.focalLength.overrideState;
            dest.focalLength.value = source.focalLength.value;
            dest.focusDistance.overrideState = source.focusDistance.overrideState;
            dest.focusDistance.value = source.focusDistance.value;
            dest.kernelSize.overrideState = source.kernelSize.overrideState;
            dest.kernelSize.value = source.kernelSize.value;
        }
        /// <summary>
        /// Gets exposure value
        /// returns false if auto exposure was not found in your global post processing volume
        /// </summary>
        /// <param name="exposureValue"></param>
        /// <returns></returns>
        public static bool GetPostFXExposure(out float exposureValue)
        {
            exposureValue = 0f;
            PostProcessProfile profile = GetGlobalProcessingProfile();
            if (profile != null)
            {
                if (profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.AutoExposure exposure))
                {
                    exposureValue = exposure.keyValue.value;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets exposure value
        /// returns false if auto exposure was not found in your global post processing volume
        /// </summary>
        /// <param name="exposureValue"></param>
        /// <returns></returns>
        public static void SetPostFXExposure(float exposureValue)
        {
            PostProcessProfile profile = GetGlobalProcessingProfile();
            if (profile != null)
            {
                if (profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.AutoExposure exposure))
                {
                    exposure.keyValue.value = exposureValue;
                }
            }
        }
        /// <summary>
        /// Gets the global post processing profile from the global post fx volume in your scene
        /// </summary>
        /// <returns></returns>
        public static PostProcessProfile GetGlobalProcessingProfile()
        {
            PostProcessVolume[] volumes = GaiaUtils.FindOOTs<PostProcessVolume>();
            PostProcessProfile profile = null;
            if (volumes.Length > 0)
            {
                foreach (PostProcessVolume volume in volumes)
                {
                    if (volume.name.Contains("Global Post Processing") || volume.name.Contains("Post FX"))
                    {
                        profile = volume.sharedProfile;
                        break;
                    }
                }

                if (profile == null)
                {
                    foreach (PostProcessVolume volume in volumes)
                    {
                        if (volume.isGlobal)
                        {
                            profile = volume.sharedProfile;
                            break;
                        }
                    }
                }
            }

            return profile;
        }
#endif

        #region URP

#if UPPipeline
        /// <summary>
        /// Gets URP antialiasing mode
        /// </summary>
        /// <returns></returns>
        public static int GetURPAntiAliasingMode()
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (data == null)
                {
                    data = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }

                return (int)data.antialiasing;
            }
            return 0;
        }
        /// <summary>
        /// Sets antialaising mode
        /// </summary>
        /// <param name="value"></param>
        public static void SetURPAntiAliasingMode(int value)
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (data == null)
                {
                    data = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }

                data.antialiasing = (AntialiasingMode)value;
            }
        }
        /// <summary>
        /// Gets unity post processign v2 depth of field
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Rendering.Universal.DepthOfField GetDepthOfFieldSettingsURP()
        {
            VolumeProfile profile = GetGlobalProcessingProfileURP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.Universal.DepthOfField dof))
                {
                    return dof;
                }
            }

            return null;
        }
        /// <summary>
        /// Sets unity post processign v2 depth of field
        /// </summary>
        /// <param name="dof"></param>
        public static void SetDepthOfFieldSettingsURP(PhotoModeValues photoModeProfile)
        {
            if (photoModeProfile == null)
            {
                return;
            }

            VolumeProfile profile = GetGlobalProcessingProfileURP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.Universal.DepthOfField currentDof))
                {
                    currentDof.active = photoModeProfile.m_dofActive;
                    currentDof.aperture.value = photoModeProfile.m_dofAperture;
                    currentDof.focalLength.value = photoModeProfile.m_dofFocalLength;
                    currentDof.focusDistance.value = photoModeProfile.m_dofFocusDistance;
                    currentDof.gaussianEnd.value = Mathf.Clamp(photoModeProfile.m_dofEndBlurURP, photoModeProfile.m_dofStartBlurURP + 0.05f, Mathf.Infinity);
                    currentDof.gaussianMaxRadius.value = photoModeProfile.m_dofMaxRadiusBlur;
                    currentDof.gaussianStart.value = photoModeProfile.m_dofStartBlurURP;
                    currentDof.highQualitySampling.value = photoModeProfile.m_dofHighQualityURP;
                    currentDof.mode.value = (UnityEngine.Rendering.Universal.DepthOfFieldMode)photoModeProfile.m_dofFocusModeURP;            
                }
            }
        }
        /// <summary>
        /// Copies the depth of field settings from source to dest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyDepthOfFieldPropertiesURP(UnityEngine.Rendering.Universal.DepthOfField source, UnityEngine.Rendering.Universal.DepthOfField dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            dest.active = source.active;
            dest.aperture.overrideState = source.aperture.overrideState;
            dest.aperture.value = source.aperture.value;
            dest.focalLength.overrideState = source.focalLength.overrideState;
            dest.focalLength.value = source.focalLength.value;
            dest.focusDistance.overrideState = source.focusDistance.overrideState;
            dest.focusDistance.value = source.focusDistance.value;
        }
        /// <summary>
        /// Gets the global post processing profile from the global post fx volume in your scene
        /// </summary>
        /// <returns></returns>
        public static VolumeProfile GetGlobalProcessingProfileURP()
        {
            Volume[] volumes = GaiaUtils.FindOOTs<Volume>();
            VolumeProfile profile = null;
            if (volumes.Length > 0)
            {
                foreach (Volume volume in volumes)
                {
                    if (volume.name.Contains("Global Post Processing") || volume.name.Contains("Post FX"))
                    {
                        profile = volume.sharedProfile;
                        break;
                    }
                }

                if (profile == null)
                {
                    foreach (Volume volume in volumes)
                    {
                        if (volume.isGlobal)
                        {
                            profile = volume.sharedProfile;
                            break;
                        }
                    }
                }
            }

            return profile;
        }
        /// <summary>
        /// Gets the post exposure
        /// </summary>
        /// <returns></returns>
        public static bool GetPostExposureURP(out float value)
        {
            value = 0f;
            VolumeProfile profile = GetGlobalProcessingProfileURP();
            if (profile != null)
            {
                if (profile.TryGet(out ColorAdjustments colorAdjustments))
                {
                    value = colorAdjustments.postExposure.value;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Sets the post exposure
        /// </summary>
        /// <param name="value"></param>
        public static void SetPostExposureURP(float value)
        {
            VolumeProfile profile = GetGlobalProcessingProfileURP();
            if (profile != null)
            {
                if (profile.TryGet(out ColorAdjustments colorAdjustments))
                {
                    colorAdjustments.postExposure.value = value;
                }
            }
        }
#endif

        #endregion
        #region HDRP

#if HDPipeline
        /// <summary>
        /// Gets unity post processign v2 depth of field
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Rendering.HighDefinition.DepthOfField GetDepthOfFieldSettingsHDRP()
        {
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.DepthOfField dof))
                {
                    return dof;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets unity post processign v2 depth of field
        /// </summary>
        /// <returns></returns>
        public static bool DepthOfFieldPresentHDRP()
        {
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.DepthOfField dof))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Sets HDRP post processing depth of field
        /// </summary>
        /// <param name="dof"></param>
        public static void SetDepthOfFieldSettingsHDRP(PhotoModeValues photoModeProfile)
        {
            if (photoModeProfile == null)
            {
                return;
            }

            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.DepthOfField currentDof))
                {
                    currentDof.SetAllOverridesTo(true);
                    currentDof.active = photoModeProfile.m_dofActive;
                    currentDof.nearFocusStart.value = photoModeProfile.m_dofNearBlurStart;
                    currentDof.nearFocusEnd.value = photoModeProfile.m_dofNearBlurEnd;
                    currentDof.farFocusStart.value = photoModeProfile.m_dofFarBlurStart;
                    currentDof.farFocusEnd.value = photoModeProfile.m_dofFarBlurEnd;
                    currentDof.quality.value = photoModeProfile.m_dofQualityHDRP;
                    currentDof.focusDistance.value = photoModeProfile.m_dofFocusDistance;
                    currentDof.focusMode.value = (DepthOfFieldMode)photoModeProfile.m_dofFocusModeHDRP;
                }
            }
        }
        /// <summary>
        /// Copies the depth of field settings from source to dest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyDepthOfFieldPropertiesHDRP(UnityEngine.Rendering.HighDefinition.DepthOfField source, UnityEngine.Rendering.HighDefinition.DepthOfField dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            dest.active = source.active;
            dest.nearFocusStart.value = source.nearFocusStart.value;
            dest.nearFocusEnd.value = source.nearFocusEnd.value;
            dest.farFocusStart.value = source.farFocusStart.value;
            dest.farFocusEnd.value = source.farFocusEnd.value;
            dest.quality.value = source.quality.value;
            dest.focusDistance.value = source.focusDistance.value;
            dest.nearSampleCount = source.nearSampleCount;
            dest.nearMaxBlur = source.nearMaxBlur;
            dest.farSampleCount = source.farSampleCount;
            dest.farMaxBlur = source.farMaxBlur;
            dest.focusMode.value = source.focusMode.value;
        }
        /// <summary>
        /// Gets URP antialiasing mode
        /// </summary>
        /// <returns></returns>
        public static int GetHDRPAntiAliasingMode()
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera != null)
            {
                HDAdditionalCameraData data = camera.GetComponent<HDAdditionalCameraData>();
                if (data == null)
                {
                    data = camera.gameObject.AddComponent<HDAdditionalCameraData>();
                }

                return (int)data.antialiasing;
            }
            return 0;
        }
        /// <summary>
        /// Sets antialaising mode
        /// </summary>
        /// <param name="value"></param>
        public static void SetHDRPAntiAliasingMode(int value)
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera != null)
            {
                HDAdditionalCameraData data = camera.GetComponent<HDAdditionalCameraData>();
                if (data == null)
                {
                    data = camera.gameObject.AddComponent<HDAdditionalCameraData>();
                }

                data.antialiasing = (HDAdditionalCameraData.AntialiasingMode)value;
            }
        }
        /// <summary>
        /// Gets the global post processing profile from the global post fx volume in your scene
        /// </summary>
        /// <returns></returns>
        public static VolumeProfile GetGlobalProcessingProfileHDRP()
        {
            Volume[] volumes = GaiaUtils.FindOOTs<Volume>();
            VolumeProfile profile = null;
            if (volumes.Length > 0)
            {
                foreach (Volume volume in volumes)
                {
                    if (volume.name.Contains("Global Post Processing") || volume.name.Contains("Post Processing") || volume.name.Contains("Post FX"))
                    {
                        if (!volume.name.Contains("Underwater"))
                        {
                            profile = volume.sharedProfile;
                            break;
                        }
                    }
                }

                if (profile == null)
                {
                    foreach (Volume volume in volumes)
                    {
                        if (volume.isGlobal)
                        {
                            profile = volume.sharedProfile;
                            break;
                        }
                    }
                }
            }

            return profile;
        }
        /// <summary>
        /// Gets the global post processing profile from the global post fx volume in your scene
        /// </summary>
        /// <returns></returns>
        public static VolumeProfile GetGlobalLightingProfileHDRP()
        {
            Volume[] volumes = GaiaUtils.FindOOTs<Volume>();
            VolumeProfile profile = null;
            if (volumes.Length > 0)
            {
                foreach (Volume volume in volumes)
                {
                    if (volume.name.Contains("HD Environment Volume"))
                    {
                        profile = volume.sharedProfile;
                        break;
                    }
                }

                if (profile == null)
                {
                    foreach (Volume volume in volumes)
                    {
                        if (volume.isGlobal)
                        {
                            profile = volume.sharedProfile;
                            break;
                        }
                    }
                }
            }

            return profile;
        }
        /// <summary>
        /// Gets the post exposure
        /// </summary>
        /// <returns></returns>
        public static bool GetPostExposureHDRP(out float value, out int mode, ExposureMode overrideMode = ExposureMode.Fixed)
        {
            value = 0f;
            mode = 0;
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.Exposure exposure))
                {
                    value = 15f;
                    if (exposure.fixedExposure.value >= 0)
                    {
                        value -= exposure.fixedExposure.value;
                    }
                    else
                    {
                        value += Mathf.Abs(exposure.fixedExposure.value);
                    }
                    mode = (int)exposure.mode.value;
                    exposure.mode.value = overrideMode;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Sets the post exposure
        /// </summary>
        /// <param name="value"></param>
        public static void SetPostExposureHDRP(float value, ExposureMode mode = ExposureMode.Automatic)
        {
            VolumeProfile profile = GetGlobalProcessingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.Exposure exposure))
                {
                    exposure.mode.value = mode;
                    float newValue = 15f;
                    if (value >= 0)
                    {
                        newValue -= value;
                    }
                    else
                    {
                        newValue += Mathf.Abs(value);
                    }
                    exposure.fixedExposure.value = newValue;
                }
            }
        }
        public static float GetHDRPShadowDistance()
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out HDShadowSettings shadows))
                {
                    return shadows.maxShadowDistance.value;
                }
            }

            return 0f;
        }
        public static void SetHDRPShadowDistance(float value)
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out HDShadowSettings shadows))
                {
                    shadows.maxShadowDistance.value = value;
                }
            }
        }
        public static int GetHDRPShadowCascades()
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out HDShadowSettings shadows))
                {
                    return shadows.cascadeShadowSplitCount.value;
                }
            }

            return 0;
        }
        public static void SetHDRPShadowCascades(int value)
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out HDShadowSettings shadows))
                {
                    shadows.cascadeShadowSplitCount.value = value;
                }
            }
        }
        /// <summary>
        /// Gets unity HDRI skybox settings for HDRP
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static bool GetUnityHDRISkyboxHDRP(out float rotation, out float exposure)
        {
            rotation = 0f;
            exposure = 13f;
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.HDRISky profileSky))
                {
                    rotation = profileSky.rotation.value;
                    exposure = profileSky.exposure.value;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets unity HDRI skybox settings for HDRP
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static void SetUnityHDRISkyboxHDRP(float rotation, float exposure)
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.HDRISky profileSky))
                {
                    profileSky.rotation.value = rotation;
                    profileSky.exposure.value = exposure;
                }
            }
        }
        /// <summary>
        /// Gets unity HDRI skybox settings for HDRP
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static bool GetUnityFogHDRP(out float fogDistance, out Color fogColor)
        {
            fogDistance = 1000f;
            fogColor = Color.white;
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.Fog profileFog))
                {
                    fogDistance = profileFog.meanFreePath.value;
                    fogColor = profileFog.albedo.value;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets unity HDRI skybox settings for HDRP
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="rotation"></param>
        /// <param name="tint"></param>
        public static void SetUnityFogHDRP(float fogDistance, Color fogColor)
        {
            VolumeProfile profile = GetGlobalLightingProfileHDRP();
            if (profile != null)
            {
                if (profile.TryGet(out UnityEngine.Rendering.HighDefinition.Fog profileFog))
                {
                    profileFog.meanFreePath.value = fogDistance;
                    profileFog.albedo.value = fogColor;
                }
            }
        }
        /// <summary>
        /// Gets the base density volume settings
        /// </summary>
        /// <param name="albedoColor"></param>
        /// <param name="fogDistance"></param>
        /// <param name="effectType"></param>
        /// <param name="tilingResolution"></param>
        public static void GetHDRPDensityVolume(out Color albedoColor, out float fogDistance, out int effectType, out int tilingResolution, out bool overrideDensity)
        {
            albedoColor = Color.white;
            fogDistance = 250f;
            effectType = 1;
            tilingResolution = 3;
            overrideDensity = false;

            HDRPDensityVolumeController controller = HDRPDensityVolumeController.Instance;
            if (controller != null)
            {
                albedoColor = controller.DensityVolumeProfile.m_singleScatteringAlbedo;
                fogDistance = controller.DensityVolumeProfile.m_fogDistance;
                effectType = (int)controller.DensityVolumeProfile.m_effectType;
                tilingResolution = (int)controller.DensityVolumeProfile.m_resolution;

                PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
                if (values != null)
                {
                    overrideDensity = values.m_overrideDensityVolume;
                }
            }
        }
        /// <summary>
        /// Sets the base density volume settings
        /// </summary>
        /// <param name="photoModeProfile"></param>
        public static void SetHDRPDensityVolume(PhotoModeValues photoModeProfile)
        {
            if (photoModeProfile == null)
            {
                return;
            }

            HDRPDensityVolumeController controller = HDRPDensityVolumeController.Instance;
            if (controller != null)
            {
                controller.DensityVolumeProfile.m_singleScatteringAlbedo = photoModeProfile.m_densityVolumeAlbedoColor;
                controller.DensityVolumeProfile.m_fogDistance = photoModeProfile.m_densityVolumeFogDistance;
                controller.DensityVolumeProfile.m_effectType = (DensityVolumeEffectType)photoModeProfile.m_densityVolumeEffectType;
                controller.DensityVolumeProfile.m_resolution = (DensityVolumeResolution)photoModeProfile.m_densityVolumeTilingResolution;
                controller.ApplyChanges();
            }
        }
        /// <summary>
        /// Gets HDRP camera settings
        /// </summary>
        /// <param name="aperture"></param>
        /// <param name="focalLength"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool GetHDRPCameraSettings(out float aperture, out float focalLength, Camera camera = null)
        {
            aperture = 16f;
            focalLength = 50f;
            if (camera == null)
            {
                camera = Camera.main;
                if (camera == null)
                {
                    Camera[] cameras = Camera.allCameras;
                    if (cameras.Length > 0)
                    {
                        camera = cameras[0];
                    }

                    if (camera != null)
                    {
#if UNITY_2022_2_OR_NEWER
                        aperture = camera.aperture;
                        focalLength = camera.focalLength;
                        return true;
#else
                        HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                        aperture = data.physicalParameters.aperture;
                        focalLength = camera.focalLength;
                        return true;
#endif
                    }
                }
            }
            else
            {
#if UNITY_2022_2_OR_NEWER
                aperture = camera.aperture;
                focalLength = camera.focalLength;
                return true;
#else
                HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                aperture = data.physicalParameters.aperture;
                focalLength = camera.focalLength;
                return true;
#endif
            }

            return false;
        }
        /// <summary>
        /// Sets HDRP camera settings
        /// </summary>
        /// <param name="aperture"></param>
        /// <param name="focalLength"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool SetHDRPCameraSettings(float aperture, float focalLength, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
                if (camera == null)
                {
                    Camera[] cameras = Camera.allCameras;
                    if (cameras.Length > 0)
                    {
                        camera = cameras[0];
                    }

                    if (camera != null)
                    {
#if UNITY_2022_2_OR_NEWER
                        camera.aperture = aperture;
                        camera.focalLength = focalLength;
                        return true;
#else
                        HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                        data.physicalParameters.aperture = aperture;
                        camera.focalLength = focalLength;
                        return true;
#endif
                    }
                }
            }
            else
            {
#if UNITY_2022_2_OR_NEWER
                camera.aperture = aperture;
                camera.focalLength = focalLength;
                return true;
#else
                HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                data.physicalParameters.aperture = aperture;
                camera.focalLength = focalLength;
                return true;
#endif
            }

            return false;
        }
#endif

        #endregion

        /// <summary>
        /// Sets Auto Dof enabled or disabled 
        /// </summary>
        /// <param name="enabled"></param>
        public static bool GetAutoFocusDepthOfField()
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera == null)
            {
                return false;
            }

            AutoDepthOfField autoDOF = camera.GetComponent<AutoDepthOfField>();
            return autoDOF;
        }
        /// <summary>
        /// Sets Auto Dof enabled or disabled 
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetAutoFocusDepthOfField(bool enabled)
        {
            Camera camera = GaiaUtils.GetCamera();
            if (camera == null)
            {
                return;
            }

            AutoDepthOfField autoDOF = camera.GetComponent<AutoDepthOfField>();
            if (autoDOF != null)
            {
                if (enabled)
                {
                    autoDOF.m_disableSystem = false;
                }
                else
                {
                    autoDOF.m_disableSystem = true;
                }
            }
        }

#endregion
#region Runtime

        /// <summary>
        /// Gets the culling settings
        /// Gaia additional culling is for our custom culling system for layers
        /// Camera far clip plane is unity base viewing distance
        /// </summary>
        /// <param name="gaiaAdditionalCullingDistance"></param>
        /// <param name="cameraFarClipPlane"></param>
        /// <param name="camera"></param>
        public static void GetCullingSettings(out float gaiaAdditionalCullingDistance, out float cameraFarClipPlane, Camera camera = null)
        {
            gaiaAdditionalCullingDistance = 0f;
            cameraFarClipPlane = 2000f;

            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                GaiaSceneCullingProfile cullingProfile = sceneProfile.CullingProfile;
                if (cullingProfile != null)
                {
                    gaiaAdditionalCullingDistance = cullingProfile.m_additionalCullingDistance;
                }
            }

            if (camera != null)
            {
                cameraFarClipPlane = camera.farClipPlane;
            }
            else
            {
                camera = GaiaUtils.GetCamera();
                if (camera != null)
                {
                    cameraFarClipPlane = camera.farClipPlane;
                }
            }
        }
        /// <summary>
        /// Gets the culling settings
        /// Gaia additional culling is for our custom culling system for layers
        /// Camera far clip plane is unity base viewing distance
        /// </summary>
        /// <param name="gaiaAdditionalCullingDistance"></param>
        /// <param name="cameraFarClipPlane"></param>
        /// <param name="camera"></param>
        public static void GetCullingSettings(out float gaiaAdditionalCullingDistance)
        {
            gaiaAdditionalCullingDistance = 0f;

            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                GaiaSceneCullingProfile cullingProfile = sceneProfile.CullingProfile;
                if (cullingProfile != null)
                {
                    gaiaAdditionalCullingDistance = cullingProfile.m_additionalCullingDistance;
                }
            }
        }
        /// <summary>
        /// Sets the culling settings
        /// Gaia additional culling is for our custom culling system for layers
        /// Camera far clip plane is unity base viewing distance
        /// </summary>
        /// <param name="gaiaAdditionalCullingDistance"></param>
        /// <param name="cameraFarClipPlane"></param>
        /// <param name="camera"></param>
        public static void SetCullingSettings(float gaiaAdditionalCullingDistance, float cameraFarClipPlane, Camera camera = null)
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                GaiaSceneCullingProfile cullingProfile = sceneProfile.CullingProfile;
                if (cullingProfile != null)
                {
                    cullingProfile.m_additionalCullingDistance = gaiaAdditionalCullingDistance;
                    RefreshCameraCulling();
                }
            }

            if (camera != null)
            {
                camera.farClipPlane = cameraFarClipPlane;
                camera.farClipPlane = Mathf.Clamp(camera.farClipPlane, camera.nearClipPlane + 0.1f, float.PositiveInfinity);
            }
            else
            {
                camera = GaiaUtils.GetCamera();
                if (camera != null)
                {
                    camera.farClipPlane = cameraFarClipPlane;
                    camera.farClipPlane = Mathf.Clamp(camera.farClipPlane, camera.nearClipPlane + 0.1f, float.PositiveInfinity);
                }
            }
        }
        /// <summary>
        /// Sets unity terrain draw instanced
        /// </summary>
        /// <param name="detailDensity"></param>
        /// <param name="detailDistance"></param>
        public static void SetTerrainDrawInstanced(bool drawInstanced)
        {
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.drawInstanced = drawInstanced;
                }
            }
        }
        /// <summary>
        /// Sets unity terrain detail density and distance
        /// </summary>
        /// <param name="detailDensity"></param>
        /// <param name="detailDistance"></param>
        public static void SetTerrainDetails(float detailDensity, float detailDistance)
        {
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.detailObjectDensity = detailDensity;
                    terrain.detailObjectDistance = detailDistance;
                }
            }
        }
        /// <summary>
        /// Sets unity terrain pixel heightmap error and basemap render distance
        /// </summary>
        /// <param name="heightResolution"></param>
        /// <param name="textureDistance"></param>
        public static void SetTerrainPixelErrorAndBaseMapTexture(float heightResolution, float textureDistance)
        {
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.heightmapPixelError = heightResolution;
                    terrain.basemapDistance = textureDistance;
                }
            }
        }
        /// <summary>
        /// Gets the camera roll (z rotation)
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static float GetCameraRoll(Camera camera = null)
        {
            if (camera == null)
            {
                camera = GaiaUtils.GetCamera();
                if (camera != null)
                {
                    return camera.transform.eulerAngles.z;
                }
            }
            else
            {
                return camera.transform.eulerAngles.z;
            }

            return 0f;
        }
        /// <summary>
        /// Sets the camera roll (z rotation)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camera"></param>
        public static void SetCameraRoll(float value, Camera camera = null)
        {
            if (camera == null)
            {
                camera = GaiaUtils.GetCamera();
                if (camera != null)
                {
                    camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y, value);
                }

                FreeCamera freeCamera = GaiaUtils.FindOOT<FreeCamera>();
                if (freeCamera != null)
                {
                    freeCamera.RefreshCameraRoll(value);
                }
            }
            else
            {
                camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y, value);
                FreeCamera freeCamera = GaiaUtils.FindOOT<FreeCamera>();
                if (freeCamera != null)
                {
                    freeCamera.RefreshCameraRoll(value);
                }
            }
        }
        /// <summary>
        /// Gets the photomode settings
        /// </summary>
        /// <param name="loadSavedSettings"></param>
        /// <param name="revertOnDisabled"></param>
        public static void GetPhotoModeSettings(out bool loadSavedSettings, out bool revertOnDisabled, out bool showreticule, out bool showRuleOfThirds, out KeyCode showPhotoMode)
        {
            loadSavedSettings = true;
            revertOnDisabled = true;
            showreticule = false;
            showRuleOfThirds = false;
            showPhotoMode = KeyCode.F11;

            UIConfiguration ui = UIConfiguration.Instance;
            if (ui != null)
            {
                loadSavedSettings = ui.m_loadFromLastSaved;
                revertOnDisabled = ui.m_resetOnDisable;
                showPhotoMode = ui.m_enablePhotoMode;
                showreticule = ui.m_showReticule;
                showRuleOfThirds = ui.m_showRuleOfThirds;
            }
        }
        /// <summary>
        /// Sets photo mode settings
        /// </summary>
        /// <param name="loadSavedSettings"></param>
        /// <param name="revertOnDisabled"></param>
        /// <param name="showPhotoMode"></param>
        public static void SetPhotoModeSettings(bool loadSavedSettings, bool revertOnDisabled, bool showreticule, bool showRuleOfThirds, KeyCode showPhotoMode)
        {
            UIConfiguration ui = UIConfiguration.Instance;
            if (ui != null)
            {
                ui.m_loadFromLastSaved = loadSavedSettings;
                ui.m_resetOnDisable = revertOnDisabled;
                ui.m_enablePhotoMode = showPhotoMode;
                ui.m_showReticule = showreticule;
                ui.m_showRuleOfThirds = showRuleOfThirds;
            }
        }

        public static void SetShowOrHidePhotoModeReticule(bool value)
        {
            PhotoMode photoMode = PhotoMode.Instance;
            if (photoMode != null)
            {
                if (photoMode.m_reticule != null)
                {
                    photoMode.m_reticule.SetActive(value);
                }
            }
        }
        public static void SetShowOrHidePhotoModeRuleOfThirds(bool value)
        {
            PhotoMode photoMode = PhotoMode.Instance;
            if (photoMode != null)
            {
                if (photoMode.m_ruleOfThirds != null)
                {
                    photoMode.m_ruleOfThirds.SetActive(value);
                }
            }
        }
        /// <summary>
        /// Saves the photo mode values this is really useful to keep the changes you made in photo mode
        /// These will be removed when you exit play mode
        /// </summary>
        /// <param name="values"></param>
        public static void SavePhotoModeValues(PhotoModeValues values, GaiaConstants.EnvironmentRenderer currentRenderPipeline)
        {
            if (values == null)
            {
                return;
            }

            if (PhotoMode.Instance != null)
            {
                PhotoMode photoMode = PhotoMode.Instance;
                if (photoMode.m_photoModeProfile != null)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    values.Save(photoMode.m_photoModeProfile.Profile);
                    values.m_lastSceneName = sceneName;
                    photoMode.m_photoModeProfile.m_everBeenSaved = true;
                    photoMode.m_photoModeProfile.LastRenderPipeline = currentRenderPipeline;
                    photoMode.m_photoModeProfile.Profile.m_lastSceneName = sceneName;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(photoMode.m_photoModeProfile);
#endif
                }
            }
        }
        /// <summary>
        /// Saves the photo mode values this is really useful to keep the changes you made in photo mode
        /// These will be removed when you exit play mode
        /// </summary>
        /// <param name="values"></param>
        public static void SaveImportantPhotoModeValues(PhotoModeValues values, GaiaConstants.EnvironmentRenderer currentRenderPipeline)
        {
            if (values == null)
            {
                return;
            }

            if (PhotoMode.Instance != null)
            {
                PhotoMode photoMode = PhotoMode.Instance;
                if (photoMode.m_photoModeProfile != null)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    photoMode.m_photoModeProfile.Profile.m_screenshotResolution = values.m_screenshotResolution;
                    photoMode.m_photoModeProfile.Profile.m_screenshotImageFormat = values.m_screenshotImageFormat;
                    photoMode.m_photoModeProfile.Profile.m_showFPS = values.m_showFPS;
                    photoMode.m_photoModeProfile.Profile.m_showReticle = values.m_showReticle;
                    photoMode.m_photoModeProfile.Profile.m_showRuleOfThirds = values.m_showRuleOfThirds;
                    photoMode.m_photoModeProfile.Profile.m_loadSavedSettings = values.m_loadSavedSettings;
                    photoMode.m_photoModeProfile.Profile.m_revertOnDisabled = values.m_revertOnDisabled;
                    photoMode.m_photoModeProfile.LastRenderPipeline = currentRenderPipeline;
                    photoMode.m_photoModeProfile.Profile.m_lastSceneName = sceneName;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(photoMode.m_photoModeProfile);
#endif
                }
            }
        }
        /// <summary>
        /// Loads the photo mode values these values are only kept until you exit play mode
        /// But these can be loaded as many time and allows you to pick up where you left off
        /// </summary>
        /// <returns></returns>
        public static PhotoModeValues LoadPhotoModeValues()
        {
            if (PhotoMode.Instance != null)
            {
                PhotoMode photoMode = PhotoMode.Instance;
                if (photoMode.m_photoModeProfile != null)
                {
                    return photoMode.m_photoModeProfile.Profile;
                }
            }

            return null;
        } 
        /// <summary>
        /// Sets up the player and camera with Gaia runtime systems
        /// </summary>
        /// <param name="player"></param>
        /// <param name="camera"></param>
        public static void SetRuntimePlayerAndCamera(GameObject player, Camera camera, bool closePhotoMode)
        {
            if (player == null)
            {
                Debug.LogError("Player object provided was null, please make sure the object you are passing in is in the scene before calling SetRuntimePlayerAndCamera");
                return;
            }

            if (camera == null)
            {
                Debug.LogError("Camera object provided was null, please make sure the object you are passing in is in the scene before calling SetRuntimePlayerAndCamera");
                return;
            }

            GaiaGlobal.FinalizePlayerObjectRuntime(player, false);            
            GaiaGlobal.FinalizeCameraObjectRuntime(camera, closePhotoMode);
        }
        /// <summary>
        /// Sets up the player with Gaia runtime systems
        /// </summary>
        /// <param name="player"></param>
        /// <param name="camera"></param>
        public static void SetRuntimePlayer(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("Player object provided was null, please make sure the object you are passing in is in the scene before calling SetRuntimePlayerAndCamera");
                return;
            }

            GaiaGlobal.FinalizePlayerObjectRuntime(player, false);
        }
        /// <summary>
        /// Sets up the camera with Gaia runtime systems
        /// </summary>
        /// <param name="player"></param>
        /// <param name="camera"></param>
        public static void SetRuntimeCamera(Camera camera, bool closePhotoMode)
        {
            if (camera == null)
            {
                Debug.LogError("Camera object provided was null, please make sure the object you are passing in is in the scene before calling SetRuntimePlayerAndCamera");
                return;
            }

            GaiaGlobal.FinalizeCameraObjectRuntime(camera, closePhotoMode);
        }
        /// <summary>
        /// Instantiates photo mode system and sets the mouse cursor and screen lock state
        /// </summary>
        /// <param name="photoModePrefab"></param>
        /// <param name="setCursorActive"></param>
        /// <param name="resetOnDisabled"></param>
        /// <returns></returns>
        public static GameObject InstantiatePhotoMode(GameObject photoModePrefab, bool setCursorActive)
        {
            if (photoModePrefab == null)
            {
                return null;
            }

            GameObject photoModeObject = GameObject.Instantiate(photoModePrefab);
            if (UIConfiguration.Instance != null)
            {
                if (UIConfiguration.Instance.m_hideMouseCursor)
                {
                    SetCursorState(setCursorActive);
                }
            }
            else
            {
                SetCursorState(setCursorActive);
            }

            return photoModeObject;
        }
        /// <summary>
        /// Destroy the photo mode gameobject and sets the mouse cursor and screen lock state
        /// </summary>
        /// <param name="setCursorActive"></param>
        public static void RemovePhotoMode(bool setCursorActive)
        {
            if (PhotoMode.Instance != null)
            {
                GameObject.DestroyImmediate(PhotoMode.Instance.gameObject);
            }

            if (UIConfiguration.Instance != null)
            {
                if (UIConfiguration.Instance.m_hideMouseCursor)
                {
                    SetCursorState(setCursorActive);
                }
            }
            else
            {
                SetCursorState(setCursorActive);
            }
        }
        /// <summary>
        /// Shows or hides the cursor
        /// </summary>
        /// <param name="enabled"></param>
        public static bool SetCursorState(bool enabled)
        {
            if (enabled)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (PhotoMode.Instance == null)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            return enabled;
        }
        /// <summary>
        /// Refreshes the camera culling
        /// Applies the right refresh automatically based on if the application is running
        /// </summary>
        public static void RefreshCameraCulling()
        {
            if (Application.isPlaying)
            {
                GaiaScenePlayer.UpdateCullingDistances();
            }
            else
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    SceneProfile profile = GaiaGlobal.Instance.SceneProfile;
                    if (profile.CullingProfile != null)
                    {
                        GaiaScenePlayer.ApplySceneSetup(profile.CullingProfile.m_applyToEditorCamera);
                    }
                }
            }
        }
        /// <summary>
        /// Copies URP camera settings
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyCameraSettings(Camera source, Camera dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            dest.fieldOfView = source.fieldOfView;
            dest.farClipPlane = source.farClipPlane;
            dest.allowMSAA = source.allowMSAA;
            dest.allowHDR = source.allowHDR;

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessLayer sourceProcesslayer = source.GetComponent<PostProcessLayer>();
            if (sourceProcesslayer != null)
            {
                PostProcessLayer destProcesslayer = dest.GetComponent<PostProcessLayer>();
                if (destProcesslayer == null)
                {
                    destProcesslayer = dest.gameObject.AddComponent<PostProcessLayer>();
                }

                destProcesslayer.volumeTrigger = destProcesslayer.transform;
                destProcesslayer.volumeLayer = sourceProcesslayer.volumeLayer;
                destProcesslayer.antialiasingMode = sourceProcesslayer.antialiasingMode;
                destProcesslayer.fog = sourceProcesslayer.fog;
                destProcesslayer.stopNaNPropagation = sourceProcesslayer.stopNaNPropagation;
                destProcesslayer.temporalAntialiasing.jitterSpread = sourceProcesslayer.temporalAntialiasing.jitterSpread;
                destProcesslayer.temporalAntialiasing.stationaryBlending = sourceProcesslayer.temporalAntialiasing.stationaryBlending;
                destProcesslayer.temporalAntialiasing.motionBlending = sourceProcesslayer.temporalAntialiasing.motionBlending;
                destProcesslayer.temporalAntialiasing.sharpness = sourceProcesslayer.temporalAntialiasing.sharpness;
                destProcesslayer.subpixelMorphologicalAntialiasing.quality = sourceProcesslayer.subpixelMorphologicalAntialiasing.quality;
            }
#endif
        }

#region HDRP

#if HDPipeline
        /// <summary>
        /// Copies URP camera settings
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyCameraSettingsHDRP(Camera source, Camera dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

#if HDPipeline
            HDAdditionalCameraData dataSource = GaiaHDRPRuntimeUtils.GetHDCameraData(source);
            HDAdditionalCameraData dataDest = GaiaHDRPRuntimeUtils.GetHDCameraData(dest);

            //Anti-Aliasing
            dataDest.antialiasing = dataSource.antialiasing;
            dataDest.taaAntiFlicker = dataSource.taaAntiFlicker;
            dataDest.taaAntiHistoryRinging = dataSource.taaAntiHistoryRinging;
            dataDest.taaHistorySharpening = dataSource.taaHistorySharpening;
            dataDest.taaMotionVectorRejection = dataSource.taaMotionVectorRejection;
            dataDest.taaSharpenStrength = dataSource.taaSharpenStrength;
            dataDest.TAAQuality = dataSource.TAAQuality;
            //General
            dataDest.dithering = dataSource.dithering;
            dest.allowMSAA = source.allowMSAA;
            dest.allowHDR = source.allowHDR;
            //Physical
#if UNITY_2022_2_OR_NEWER
            dest.anamorphism = source.anamorphism;
            dest.aperture = source.aperture;
            dest.barrelClipping = source.barrelClipping;
            dest.bladeCount = source.bladeCount;
            dest.curvature = source.curvature;
            dest.iso = source.iso;
            dest.shutterSpeed = source.shutterSpeed;
#else
            dataDest.physicalParameters.anamorphism = dataSource.physicalParameters.anamorphism;
            dataDest.physicalParameters.aperture = dataSource.physicalParameters.aperture;
            dataDest.physicalParameters.barrelClipping = dataSource.physicalParameters.barrelClipping;
            dataDest.physicalParameters.bladeCount = dataSource.physicalParameters.bladeCount;
            dataDest.physicalParameters.curvature = dataSource.physicalParameters.curvature;
            dataDest.physicalParameters.iso = dataSource.physicalParameters.iso;
            dataDest.physicalParameters.shutterSpeed = dataSource.physicalParameters.shutterSpeed;
#endif
            dest.focalLength = source.focalLength;
#endif
        }
        /// <summary>
        /// Sets LOD bias in HDRP
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camera"></param>
        public static float GetHDRPLODBias(Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
                Camera[] cameras = Camera.allCameras;
                if (cameras.Length > 0)
                {
                    camera = cameras[0];
                }

                if (camera != null)
                {
                    HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                    if (data != null)
                    {
                        return data.renderingPathCustomFrameSettings.lodBias;
                    }
                }
            }
            else
            {
                HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                if (data != null)
                {
                    return data.renderingPathCustomFrameSettings.lodBias;
                }
            }

            return 1f;
        }
        /// <summary>
        /// Sets LOD bias in HDRP
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camera"></param>
        public static void SetHDRPLODBias(float value, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
                Camera[] cameras = Camera.allCameras;
                if (cameras.Length > 0)
                {
                    camera = cameras[0];
                }

                if (camera != null)
                {
                    HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                    if (data != null)
                    {
                        data.customRenderingSettings = true;
                        data.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
                        data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBiasMode] = true;
                        data.renderingPathCustomFrameSettings.lodBias = value;
                        data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBias] = true;
                    }
                }
            }
            else
            {
                HDAdditionalCameraData data = GaiaHDRPRuntimeUtils.GetHDCameraData(camera);
                if (data != null)
                {
                    data.customRenderingSettings = true;
                    data.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
                    data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBiasMode] = true;
                    data.renderingPathCustomFrameSettings.lodBias = value;
                    data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint) FrameSettingsField.LODBias] = true;
                }
            }
        }
#endif

#endregion
#region URP

        /// <summary>
        /// Copies URP camera settings
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyCameraSettingsURP(Camera source, Camera dest)
        {
#if UPPipeline
            if (source == null || dest == null)
            {
                return;
            }

            UniversalAdditionalCameraData dataSource = GaiaURPRuntimeUtils.GetUPCameraData(source);
            UniversalAdditionalCameraData dataDest = GaiaURPRuntimeUtils.GetUPCameraData(dest);

            dataDest.antialiasing = dataSource.antialiasing;
            dataDest.renderPostProcessing = dataSource.renderPostProcessing;
            dataDest.dithering = dataSource.dithering;
            dest.allowMSAA = source.allowMSAA;
            dest.allowHDR = source.allowHDR;
#endif
        }

#endregion

#endregion
#region PW Grass System

        /*public static List<GrassRenderingProfileData> GetGrassDetailData()
        {
            List<GrassRenderingProfileData> data = new List<GrassRenderingProfileData>();
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                foreach (Terrain terrain in terrains)
                {
                    DetailTerrainTile tile = terrain.GetComponent<DetailTerrainTile>();
                    if (tile != null)
                    {
                        data.Add(new GrassRenderingProfileData
                        {
                            m_terrainName = terrain.name,
                            m_tileData = tile,
                            m_additionalFarDistance = 0f,
                            m_additionalNearDistance = 0f
                        });
                    }
                }
            }

            return data;
        }
        public static void SetGrassRenderDistance(List<GrassRenderingProfileData> grassData, float nearDistance, float farDistance)
        {
            if (grassData.Count > 0)
            {
                for (int i = 0; i < grassData.Count; i++)
                {
                    GrassRenderingProfileData data = grassData[i];
                    if (data != null && data.m_distancesSaved)
                    {
                        if (data.m_tileData.detailObjectsList.Count > 0)
                        {
                            if (data.m_tileData.detailObjectsList.Count == data.m_savedData.Count)
                            {
                                for (int j = 0; j < data.m_tileData.detailObjectsList.Count; j++)
                                {
                                    DetailScriptableObject scriptableObjectData = data.m_tileData.detailObjectsList[j].detailScriptableObject;
                                    if (scriptableObjectData != null)
                                    {
                                        //Start Fade
                                        float newNearValue = data.m_savedData[j].m_nearFade;
                                        newNearValue += nearDistance;
                                        scriptableObjectData.startFadeDistance = newNearValue;
                                        //End Fade
                                        float newFarValue = data.m_savedData[j].m_farFade;
                                        newFarValue += farDistance;
                                        scriptableObjectData.endFadeDistance = newFarValue;
                                    }
                                }
                            }
                        }

                        data.m_tileData.CleanUp();
                        data.m_tileData.Refresh();
                    }
                }
            }
        }*/

#endregion
#region Gaia Internal

        /// <summary>
        /// Gets the current sea level from the the water system.
        /// If it can't find the water system it will get it from the session manager
        /// </summary>
        /// <returns></returns>
        public static float GetSeaLevel()
        {
            PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
            if (waterSystem != null)
            {
                return waterSystem.SeaLevel;
            }
            else
            {
                GaiaSessionManager manager = GaiaSessionManager.GetSessionManager(false, false);
                if (manager != null)
                {
                    return manager.GetSeaLevel();
                }
            }

            return 0f;
        }
        /// <summary>
        /// Sets the current sea level in the the water system.
        /// If it can't find the water system it will set it in the session manager
        /// </summary>
        /// <param name="newSeaLevel"></param>
        public static void SetSeaLevel(float newSeaLevel)
        {
            if (Application.isPlaying)
            {
                PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
                if (waterSystem != null)
                {
                    waterSystem.SeaLevel = newSeaLevel;
                }
            }
            else
            {
                GaiaSessionManager manager = GaiaSessionManager.GetSessionManager(false, false);
                if (manager != null)
                {
                    manager.SetSeaLevel(newSeaLevel);
                }
            }
        }
        /// <summary>
        /// Creates a session entry for world / terrain generation in the session and (optionally) executes it right away. 
        /// </summary>
        /// <param name="worldCreationSettings">The world creation settings for the new world.</param>
        /// <param name="executeNow">Controls if the creation should be excuted right away as well.</param>
        public static void CreateGaiaWorld(WorldCreationSettings worldCreationSettings, bool executeNow = true)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("CreateGaiaWorld can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.CreateOrUpdateWorld(worldCreationSettings, executeNow);
        }
        /// <summary>
        /// Clears all the actual terrains from the world. The world map will remain intact
        /// </summary>
        /// <param name="executeNow">If this operation should be executed right after being added to the session.</param>
        public static void ClearGaiaWorld(bool executeNow)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("ClearGaiaWorld can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.ClearWorld(executeNow);
        }
        /// <summary>
        /// Creates a clear spawns operation in the session and optionally executes it right away
        /// </summary>
        /// <param name="clearOperationSettings">The settings that define what and where will be cleared.</param>
        /// <param name="spawnerSettings">Optional spawner settings, required only if the clearing should only delete the resources contained within these spawner settings.</param>
        /// <param name="executeNow">If this operation should be executed right after being added to the session.</param>
        /// <param name="spawner">A reference to a spawner to execute the deletion from.</param>
        public static void ClearGaiaSpawns(ClearOperationSettings clearOperationSettings, SpawnerSettings spawnerSettings = null, bool executeNow = true, Spawner spawner = null)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("ClearGaiaSpawns can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.ClearSpawns(clearOperationSettings, spawnerSettings, executeNow, spawner);
        }
        /// <summary>
        /// Creates a spawning operation in the session with a list of spawners in the given bounds area, can optionally be executed right away
        /// </summary>
        /// <param name="spawnOperationSettings">The settings for this spawning operation</param>
        /// <param name="executeNow">Whether this operation should be executed right after storing it in the session</param>
        /// <param name="spawnerList">Optional list of spawners that should execute the spawning, those need to match the spanwer settings provided in the SpawnOperationSettings.</param>
        public static void GaiaSpawn(SpawnOperationSettings spawnOperationSettings, bool executeNow = true, List<Spawner> spawnerList = null)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("GaiaSpawn can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.Spawn(spawnOperationSettings, executeNow, spawnerList);
        }
        /// <summary>
        /// Adds a stamping operation to the session and optionally executes it right away.
        /// </summary>
        /// <param name="stamperSettings">The Stamper settings representing the stamping operation that should be added to the session</param>
        /// <param name="executeNow">Whether the stamping operation should be executed right away or not</param>
        /// <param name="stamper">A stamper which should perform the stamping if the operation is executed right away.</param>
        /// <param name="massStamp">Whether this is a "mass stamping" for world generation - turns off Undo recording in this case.</param>
        public static void GaiaStamp(StamperSettings stamperSettings, bool executeNow = true, Stamper stamper = null, bool massStamp = false)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("GaiaStamp can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.Stamp(stamperSettings, executeNow, stamper, massStamp);
        }
#if GAIA_PRO_PRESENT
        /// <summary>
        /// Creates a mask map export operation in the setting which can be executed right away.
        /// </summary>
        /// <param name="exportMaskMapOperationSettings">An export settings object for the mask map export.</param>
        /// <param name="executeNow">If the export should be executed directly as well.</param>
        /// <param name="maskMapExport">A mask map exporter to execute this operation with.</param>
        public static Texture2D ExportGaiaMaskMap(ExportMaskMapOperationSettings exportMaskMapOperationSettings, bool executeNow, MaskMapExport maskMapExport)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("ExportGaiaMaskMap can only be called in the editor when the application is not playing");
                return null;
            }
            GaiaSessionManager.ExportMaskMap(exportMaskMapOperationSettings, executeNow, maskMapExport);
#if UNITY_EDITOR
            Texture2D createdMask = AssetDatabase.LoadAssetAtPath<Texture2D>(GaiaUtils.GetAssetPath(exportMaskMapOperationSettings.m_maskMapExportSettings.m_exportFileName));
            return createdMask;
#else
            return null;
#endif
        }
#endif
        /// <summary>
        /// Creates a flatten terrain operation in the session, can optionally be executed right away
        /// </summary>
        /// <param name="terrainNames">A list of terrain names that the flattening should be applied to. Leave null for "All terrains".</param>
        /// <param name="executeNow"></param>
        public static void GaiaFlattenTerrain(List<string> terrainNames, bool executeNow)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("GaiaFlattenTerrain can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.FlattenTerrain(terrainNames, executeNow);
        }
        /// <summary>
        /// Loads the given session into the manager in the scene
        /// </summary>
        /// <param name="session">The session to load.</param>
        public static void LoadSession(GaiaSession session = null)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("LoadSession can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.LoadSession(session);
        }
        /// <summary>
        /// Plays back the given Session, or the current one if session parameter is left empty
        /// <param name="session">The session to play back.</param>
        /// <paramref name="regenerateOnly"/>Whether the session should be played back in "Regeneration Mode" - In this case only terrains that are flagged for outstanding changes in the session will be affected by the session playback.
        /// </summary>
        public static void PlaySession(GaiaSession session = null, bool regenerateOnly = false)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("PlaySession can only be called in the editor when the application is not playing");
                return;
            }
            GaiaSessionManager.PlaySession(session, regenerateOnly);
        }

#endregion
    }
}