using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(DemoAPIController))]
    public class DemoAPIControllerEditor : Editor
    {
        private DemoAPIController m_controller;
        private GUIStyle m_boxStyle;

        /// <summary>
        /// Unity functions
        /// </summary>
        private void OnEnable()
        {
            m_controller = (DemoAPIController) target;
#if GAIA_PRO_PRESENT
            LoadTimeOfDay(m_controller);
            LoadWeather(m_controller);
            LoadSeason(m_controller);
            LoadWind(m_controller);
#endif
        }
        public override void OnInspectorGUI()
        {
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = GUI.skin.label.normal.textColor},
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

#if GAIA_PRO_PRESENT
            TimeOfDayGUI();
            WeatherGUI(true);
            SeasonGUI(true);
            WindGUI(true);
#else
            EditorGUILayout.HelpBox("The Current Demo API is only made for Gaia Pro. You can see API functions from 'GaiaControllerAPI' class.", MessageType.Info);
#endif
        }
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="controller"></param>
#if GAIA_PRO_PRESENT
        private void LoadTimeOfDay(DemoAPIController controller)
        {
            if (controller != null)
            {
                controller.m_timeOfDayData = GaiaAPI.GetTimeOfDaySettings();
            }
        }
        private void LoadWeather(DemoAPIController controller)
        {
            if (controller != null)
            {
                controller.m_weatherEnabled = GaiaAPI.GetWeatherEnabled();
                controller.m_instantWeatherTransition = GaiaAPI.GetInstantWeatherTransitionEffects();
            }
        }
        private void LoadSeason(DemoAPIController controller)
        {
            if (controller != null)
            {
                controller.m_seasonData = GaiaAPI.GetWeatherSeasonSettings();
            }
        }
        private void LoadWind(DemoAPIController controller)
        {
            if (controller != null)
            {
                controller.m_windData = GaiaAPI.GetWeatherWindSettings();
            }
        }
        /// <summary>
        /// GUI
        /// </summary>
        /// <param name="addSpace"></param>
        private void TimeOfDayGUI(bool addSpace = false)
        {
            if (addSpace)
            {
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Time Of Day Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            m_controller.m_timeOfDayData.m_todEnabled = EditorGUILayout.Toggle("Time Of Day Enabled", m_controller.m_timeOfDayData.m_todEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetTimeOfDayEnabled(m_controller.m_timeOfDayData.m_todEnabled);
            }
            if (m_controller.m_timeOfDayData.m_todEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                m_controller.m_timeOfDayData.m_todDayTimeScale = EditorGUILayout.Slider("Time Scale", m_controller.m_timeOfDayData.m_todDayTimeScale, 0f, 50f);
                if (EditorGUI.EndChangeCheck())
                {
                    GaiaAPI.SetTimeOfDayTimeScale(m_controller.m_timeOfDayData.m_todDayTimeScale);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.BeginChangeCheck();
            m_controller.m_timeOfDayData.m_todHour = EditorGUILayout.IntSlider("Hour", m_controller.m_timeOfDayData.m_todHour, 0, 23);
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetTimeOfDayHour(m_controller.m_timeOfDayData.m_todHour);
            }
            EditorGUI.BeginChangeCheck();
            m_controller.m_timeOfDayData.m_todMinutes = EditorGUILayout.Slider("Minute", m_controller.m_timeOfDayData.m_todMinutes, 0f, 59f);
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetTimeOfDayMinute(m_controller.m_timeOfDayData.m_todMinutes);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        private void WeatherGUI(bool addSpace = false)
        {
            if (addSpace)
            {
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Weather Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            m_controller.m_weatherEnabled = EditorGUILayout.Toggle("Enable Weather", m_controller.m_weatherEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetWeatherEnabled(m_controller.m_weatherEnabled);

            }
            if (m_controller.m_weatherEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                m_controller.m_instantWeatherTransition = EditorGUILayout.Toggle("Instant Weather Effects", m_controller.m_instantWeatherTransition);
                if (EditorGUI.EndChangeCheck())
                {
                    GaiaAPI.SetInstantWeatherTransitionEffects(m_controller.m_instantWeatherTransition);
                }

                if (!Application.isPlaying)
                {
                    GUI.enabled = false;
                }
                if (GaiaAPI.IsSnowing())
                {
                    GUI.enabled = false;
                }
                if (GaiaAPI.IsRaining())
                {
                    if (GUILayout.Button("Stop Rain"))
                    {
                        GaiaAPI.StopWeatherRain();
                        LoadWeather(m_controller);
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Rain"))
                    {
                        GaiaAPI.StartWeatherRain();
                    }
                }

                GUI.enabled = true;

                if (!Application.isPlaying)
                {
                    GUI.enabled = false;
                }
                if (GaiaAPI.IsRaining())
                {
                    GUI.enabled = false;
                }
                if (GaiaAPI.IsSnowing())
                {
                    if (GUILayout.Button("Stop Snow"))
                    {
                        GaiaAPI.StopWeatherSnow();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Snow"))
                    {
                        GaiaAPI.StartWeatherSnow();
                    }
                }

                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        private void SeasonGUI(bool addSpace = false)
        {
            if (addSpace)
            {
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Season Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            m_controller.m_seasonData.EnableSeasons = EditorGUILayout.Toggle("Enable Seasons", m_controller.m_seasonData.EnableSeasons);
            if (m_controller.m_seasonData.EnableSeasons)
            {
                EditorGUI.indentLevel++;
                m_controller.m_seasonData.Season = EditorGUILayout.Slider("Season", m_controller.m_seasonData.Season, 0f, 3.99f);
                EditorGUI.indentLevel++;
                if (m_controller.m_seasonData.Season < 1f)
                {
                    EditorGUILayout.LabelField(string.Format("{0:0}% Winter {1:0}% Spring", (1f - m_controller.m_seasonData.Season) * 100f, m_controller.m_seasonData.Season * 100f));
                }
                else if (m_controller.m_seasonData.Season < 2f)
                {
                    EditorGUILayout.LabelField(string.Format("{0:0}% Spring {1:0}% Summer", (2f - m_controller.m_seasonData.Season) * 100f, (m_controller.m_seasonData.Season - 1f) * 100f));
                }
                else if (m_controller.m_seasonData.Season < 3f)
                {
                    EditorGUILayout.LabelField(string.Format("{0:0}% Summer {1:0}% Autumn", (3f - m_controller.m_seasonData.Season) * 100f, (m_controller.m_seasonData.Season - 2f) * 100f));
                }
                else
                {
                    EditorGUILayout.LabelField(string.Format("{0:0}% Autumn {1:0}% Winter", (4f - m_controller.m_seasonData.Season) * 100f, (m_controller.m_seasonData.Season - 3f) * 100f));
                }
                EditorGUI.indentLevel--;
                m_controller.m_seasonData.m_seasonTransitionDuration = EditorGUILayout.FloatField("Season Duration", m_controller.m_seasonData.m_seasonTransitionDuration);
                m_controller.m_seasonData.SeasonWinterTint = EditorGUILayout.ColorField("Winter Tint", m_controller.m_seasonData.SeasonWinterTint);
                m_controller.m_seasonData.SeasonSpringTint = EditorGUILayout.ColorField("Spring Tint", m_controller.m_seasonData.SeasonSpringTint);
                m_controller.m_seasonData.SeasonSummerTint = EditorGUILayout.ColorField("Summer Tint", m_controller.m_seasonData.SeasonSummerTint);
                m_controller.m_seasonData.SeasonAutumnTint = EditorGUILayout.ColorField("Autumn Tint", m_controller.m_seasonData.SeasonAutumnTint);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetWeatherSeasonSettings(m_controller.m_seasonData);
            }
            EditorGUILayout.EndVertical();
        }
        private void WindGUI(bool addSpace = false)
        {
            if (addSpace)
            {
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Wind Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            m_controller.m_windData.WindDirection = EditorGUILayout.Slider("Wind Direction", m_controller.m_windData.WindDirection, 0f, 1f);
            if (m_controller.m_windData.WindDirection < 0.25f || m_controller.m_windData.WindDirection == 1)
            {
                EditorGUILayout.LabelField("N ", GUILayout.Width(30f));
            }
            else if (m_controller.m_windData.WindDirection > 0.25f && m_controller.m_windData.WindDirection < 0.5f)
            {
                EditorGUILayout.LabelField("E ", GUILayout.Width(30f));
            }
            else if (m_controller.m_windData.WindDirection > 0.5 && m_controller.m_windData.WindDirection < 0.75f)
            {
                EditorGUILayout.LabelField("S ", GUILayout.Width(30f));
            }
            else
            {
                EditorGUILayout.LabelField("W ", GUILayout.Width(30f));
            }
            EditorGUILayout.EndHorizontal();
            m_controller.m_windData.WindSpeed = EditorGUILayout.Slider("Wind Speed", m_controller.m_windData.WindSpeed, 0f, 5f);
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                GaiaAPI.SetWeatherWindSettings(m_controller.m_windData);
            }
            EditorGUILayout.EndVertical();
        }
#endif
    }
}