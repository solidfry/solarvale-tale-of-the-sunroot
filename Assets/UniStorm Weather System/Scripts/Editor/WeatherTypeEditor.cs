using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UniStorm.Utility
{
    [CustomEditor(typeof(WeatherType))]
    [System.Serializable]
    public class WeatherTypeEditor : Editor
    {
        public Texture WeatherTypeIcon;
        public Texture PrecipitationIcon;
        public Texture NonPrecipitationIcon;
        Texture HelpIcon;
        private SerializedProperty FogColorProp;
        private SerializedProperty OverrideFogColorProp;
        private SerializedProperty OverrideCameraFogHeightProp;
        private SerializedProperty CloudColorProp;
        private SerializedProperty OverrideCloudColorProp;

        void OnEnable()
        {
            WeatherType self = (WeatherType)target;

            FogColorProp = serializedObject.FindProperty("FogColor");
            OverrideFogColorProp = serializedObject.FindProperty("OverrideFogColor");
            OverrideCameraFogHeightProp = serializedObject.FindProperty("OverrideCameraFogHeight");
            CloudColorProp = serializedObject.FindProperty("CloudColor");
            OverrideCloudColorProp = serializedObject.FindProperty("OverrideCloudColor");

            if (PrecipitationIcon == null && self.CustomizeWeatherIcon == WeatherType.Yes_No.No || PrecipitationIcon == null) PrecipitationIcon = Resources.Load("PrecipitationIcon") as Texture;
            if (NonPrecipitationIcon == null && self.CustomizeWeatherIcon == WeatherType.Yes_No.No || NonPrecipitationIcon == null)
                NonPrecipitationIcon = Resources.Load("NonPrecipitationIcon") as Texture;
            if (HelpIcon == null) HelpIcon = Resources.Load("HelpIcon") as Texture;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            WeatherType self = (WeatherType)target;

            GUIStyle TitleStyle = new GUIStyle(EditorStyles.toolbarButton);
            TitleStyle.fontStyle = FontStyle.Bold;
            TitleStyle.fontSize = 14;
            TitleStyle.alignment = TextAnchor.UpperCenter;
            TitleStyle.normal.textColor = Color.white;

            var HelpStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.UpperRight };

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUILayout.Width(90 * Screen.width / Screen.dpi));
            if (self.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/Creating-a-Custom-Weather-Type#creating-a-custom-weather-type");
                }

                EditorGUILayout.LabelField(self.WeatherTypeName, style, GUILayout.ExpandWidth(true));
                GUILayout.Space(2);
                EditorGUILayout.LabelField(new GUIContent(PrecipitationIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            }
            else if (self.PrecipitationWeatherType == WeatherType.Yes_No.No)
            {
                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/Creating-a-Custom-Weather-Type#creating-a-custom-weather-type");
                }

                EditorGUILayout.LabelField(self.WeatherTypeName, style, GUILayout.ExpandWidth(true));
                GUILayout.Space(2);
                EditorGUILayout.LabelField(new GUIContent(NonPrecipitationIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            }

            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Space();

            //Info
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            EditorGUILayout.LabelField("Info", TitleStyle);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            self.WeatherTypeName = EditorGUILayout.TextField("Weather Type Name", self.WeatherTypeName);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("The name of the weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.CustomizeWeatherIcon = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Customize Weather Icon", self.CustomizeWeatherIcon);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this Weather Type's weather icon can be customized.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            if (self.CustomizeWeatherIcon == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                self.WeatherIcon = (Texture)EditorGUILayout.ObjectField("Weather Icon", self.WeatherIcon, typeof(Texture), false);

                if (self.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                {
                    PrecipitationIcon = self.WeatherIcon;
                }
                else if (self.PrecipitationWeatherType == WeatherType.Yes_No.No)
                {
                    NonPrecipitationIcon = self.WeatherIcon;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);

            //Settings
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            EditorGUILayout.LabelField("Settings", TitleStyle);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            self.CloudProfileComponent = (CloudProfile)EditorGUILayout.ObjectField("Cloud Profile", self.CloudProfileComponent, typeof(CloudProfile), false);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls the cloud profile this weather type will use.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.PrecipitationWeatherType = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Precipitation Weather Type?", self.PrecipitationWeatherType);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether this weather type is a precipitation weather type or not. Precipitation weather types are weather types " +
                "such as rain, snow, sleet, hail, fog, etc. Precipitation weather types will also have the sun color changed, clouds darkened, and fog color " +
                "changed all according to the Stormy color settings within the UniStorm Editor. Note: A particle effect does not have to be used.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.SunIntensity = EditorGUILayout.Slider("Sun Intensity", self.SunIntensity, 0.0f, 1.5f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls UniStorm's Sun intensity for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.MoonIntensity = EditorGUILayout.Slider("Moon Intensity", self.MoonIntensity, 0.0f, 1.5f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls UniStorm's Moon intensity for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.CloudShadowIntensity = EditorGUILayout.Slider("Cloud Shadow Intensity", self.CloudShadowIntensity, 0.0f, 2f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls the intensity of UniStorm's cloud shadows for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.FogDensity = EditorGUILayout.Slider("Fog Desnity", self.FogDensity, 0.0f, 0.04f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls UniStorm's Fog Density.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.FogHeight = EditorGUILayout.Slider("Fog Height", self.FogHeight, 0.0f, 1.0f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls the height of UniStorm's fog on UniStorm's clouds. If the clouds look they've lost too much detail, this setting may be too high.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.FogLightFalloff = EditorGUILayout.Slider("Fog Light Intensity", self.FogLightFalloff, 0.0f, 1f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls how much the fog is affected by the sun and moon's light for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.WindIntensity = EditorGUILayout.Slider("Wind Intensity", self.WindIntensity, 0.0f, 4f);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls UniStorm's Wind Zone intensity.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.CloudHeight = EditorGUILayout.IntSlider("Cloud Height", self.CloudHeight, 150, 1100);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls the cloud height for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(self.PrecipitationWeatherType == WeatherType.Yes_No.Yes);
            self.CloudLevel = (WeatherType.CloudLevelEnum)EditorGUILayout.EnumPopup("Cloud Level", self.CloudLevel);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("The level of cloud cover that will be generated for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (self.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField("Precipitation weather types will automatically use the Cloudy cloud level.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }

            EditorGUI.BeginDisabledGroup(self.CloudLevel == WeatherType.CloudLevelEnum.Clear || self.CloudLevel == WeatherType.CloudLevelEnum.MostlyClear || self.CloudLevel == WeatherType.CloudLevelEnum.PartyCloudy);
            self.UseLightning = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Use Lightning", self.UseLightning);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this weather type will use lightning. The lightning settings can be " +
                "adjusted within the Lightning section of the Weather tab in the UniStorm Editor.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (self.PrecipitationWeatherType == WeatherType.Yes_No.No)
            {
                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField("Lightning can only be used with a Cloud Level of Mostly Cloudy or Cloudy.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space();
            self.ShaderControl = (WeatherType.ShaderControlEnum)EditorGUILayout.EnumPopup("Shader Control", self.ShaderControl);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls UniStorm's Global Weather Shader to allow dynamic snow and wetness on surfaces that use the UniStorm/Global Weather Shader.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.WaitForCloudLevel = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Wait for Cloud Level", self.WaitForCloudLevel);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this weather type will wait for the generated cloud level to be met before fading in particle and sound effects. For example, you don't want it to rain until a Mostly Cloud cloud cover has been met.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);

            //Effects
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            EditorGUILayout.LabelField("Effects", TitleStyle);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            self.UseWeatherEffect = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Use Weather Effect", self.UseWeatherEffect);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls the option to have a weather particle effect for this weather type.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            if (self.UseWeatherEffect == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(1);

                self.WeatherEffect = (ParticleSystem)EditorGUILayout.ObjectField("Weather Effect", self.WeatherEffect, typeof(ParticleSystem), false);
                EditorGUILayout.Space();
                self.ParticleEffectVector = EditorGUILayout.Vector3Field("Weather Effect Position", self.ParticleEffectVector);
                EditorGUILayout.Space();
                self.ParticleEffectAmount = EditorGUILayout.IntSlider("Weather Effect Intensity", self.ParticleEffectAmount, 5, 3000);
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                self.UseAdditionalWeatherEffect = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Use Additional Weather Effect", self.UseAdditionalWeatherEffect);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the option to have an additional weather particle effect for this weather type (wind, mist, etc).", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }

            if (self.UseAdditionalWeatherEffect == WeatherType.Yes_No.Yes && self.UseWeatherEffect == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(1);

                self.AdditionalWeatherEffect = (ParticleSystem)EditorGUILayout.ObjectField("Additional Weather Effect", self.AdditionalWeatherEffect, typeof(ParticleSystem), false);
                EditorGUILayout.Space();
                self.AdditionalParticleEffectVector = EditorGUILayout.Vector3Field("Weather Effect Position", self.AdditionalParticleEffectVector);
                EditorGUILayout.Space();
                self.AdditionalParticleEffectAmount = EditorGUILayout.IntSlider("Weather Effect Intensity", self.AdditionalParticleEffectAmount, 5, 3000);
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            self.UseWeatherSound = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Use Weather Sound", self.UseWeatherSound);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this weather type will use a weather sound.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            if (self.UseWeatherSound == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                self.WeatherSound = (AudioClip)EditorGUILayout.ObjectField("Weather Sound", self.WeatherSound, typeof(AudioClip), false);
                EditorGUILayout.Space();
                self.WeatherVolume = EditorGUILayout.Slider("Weather Sound Volume", self.WeatherVolume, 0.1f, 1.0f);
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            self.UseAuroras = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Use Auroras", self.UseAuroras);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this weater type will use Auroras (Northern/Southern Lights). Colors and intensities can be adjusted when this option is enabled.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
            EditorGUILayout.LabelField("Note: Weather Types using auroras will only have them visible at night.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            if (self.UseAuroras == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                self.AuroraIntensity = EditorGUILayout.Slider("Aurora Intenisty", self.AuroraIntensity, 0.1f, 1.0f);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the intensity of UniStorm's Auroras for this weather type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                self.AuroraInnerColor = EditorGUILayout.ColorField("Inner Color", self.AuroraInnerColor);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the inner color of UniStorm's Auroras for this weather type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                self.AuroraOuterColor = EditorGUILayout.ColorField("Outer Color", self.AuroraOuterColor);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the outer color of UniStorm's Auroras for this weather type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);

            //Conditions
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            EditorGUILayout.LabelField("Overrides", TitleStyle);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(OverrideCloudColorProp, new GUIContent("Overrride Cloud Color"));
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Allows this weather type to override the system's cloud color. This is useful for adding variation to weather types or special event or quest related weather types.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            if (self.OverrideCloudColor == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.PropertyField(CloudColorProp, new GUIContent("Cloud Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("A gradient that controls the cloud color for this weather type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(OverrideFogColorProp, new GUIContent("Overrride Fog Color"));
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Allows this weather type to override the system's fog color. This is useful for adding variation to weather types or special event or quest related weather types.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            if (self.OverrideFogColor == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.PropertyField(FogColorProp, new GUIContent("Fog Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("A gradient that controls the fog color for this weather type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(OverrideCameraFogHeightProp, new GUIContent("Overrride Skybox Fog Height"));
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Allows this weather type to override the system's Skybox Fog Height. This is useful for adding variation to weather types. Setting this value to 1 can fully mask the sky with fog resulting in very dense looking fog that blends with the landscape.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            if (self.OverrideCameraFogHeight == WeatherType.Yes_No.Yes)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();

                self.CameraFogHeight = EditorGUILayout.Slider("Skybox Fog Height", self.CameraFogHeight, 0.0f, 1.0f);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls how much UniStorm's Fog affects the skybox. The higher far clipping plane your camera uses, the higher this value will need to be set. This can also be used to fully mask the skybox with fog with a value of 1, if desired.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Conditions
            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            EditorGUILayout.LabelField("Conditions", TitleStyle);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            self.Season = (WeatherType.SeasonEnum)EditorGUILayout.EnumPopup("Seasonal Condition", self.Season);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls what season this weather type is allowed to be generated in. If you'd like your weather type to be available in all seasons," +
                " you can select the 'All' option.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.TemperatureType = (WeatherType.TemperatureTypeEnum)EditorGUILayout.EnumPopup("Temperature Condition", self.TemperatureType);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether this Weather Type happens above or below freezing such as for rain or for snow.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            self.SpecialWeatherType = (WeatherType.Yes_No)EditorGUILayout.EnumPopup("Special Weather Condition", self.SpecialWeatherType);
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField("Controls whether or not this weather type is special. A special weather condition stops a weather type from being generated with UniStorm's " +
                "weather generator and can only be called through custom events or programmatically. This can be useful for player events or quests.", EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RecordObject(self, "Undo");

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
#endif
            serializedObject.ApplyModifiedProperties();
        }
    }
}