using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace UniStorm.Utility
{
    [CustomEditor(typeof(UniStormSystem))]
    [System.Serializable]
    public class UniStormEditor : Editor
    {
        SerializedProperty URPForwardRenderer;
        //5.0
        SerializedProperty CustomizeQualityProp;
        SerializedProperty ConvergenceSpeedProp;
        SerializedProperty NearMarchStepsProp;
        SerializedProperty DistantMarchStepsProp;
        SerializedProperty UpdateMarchStepsDuringRuntimeProp;
        //SerializedProperty RendersPerFrameProp;

        //Audio Mixer
        SerializedProperty TimeOfDaySoundsVolumeProp;
        SerializedProperty TimeOfDayMusicVolumeProp;
        SerializedProperty WeatherSoundsVolumeProp;

        SerializedProperty TabNumberProp;
        SerializedProperty PlayerTransformProp;
        SerializedProperty PlayerCameraProp;
        SerializedProperty CameraFoldoutProp;
        SerializedProperty SettingsFoldoutProp;
        SerializedProperty PlatformFoldoutProp;
        SerializedProperty PlatformTypeProp;
        SerializedProperty UniStormProfileProp;

        //Camera & Player
        SerializedProperty GetPlayerAtRuntimeProp;
        SerializedProperty GetPlayerMethodProp;
        SerializedProperty UseRuntimeDelayProp;
        SerializedProperty PlayerTagProp;
        SerializedProperty CameraTagProp;
        SerializedProperty PlayerNameProp;
        SerializedProperty CameraNameProp;
        SerializedProperty UseUniStormMenuProp;
        SerializedProperty UniStormMenuKeyProp;

        //Time
        SerializedProperty HourProp;
        SerializedProperty MinuteProp;
        SerializedProperty TimeFlowProp;
        SerializedProperty RealWorldTimeProp;
        SerializedProperty DayLengthProp;
        SerializedProperty NightLengthProp;
        SerializedProperty TimeOfDaySoundsSecondsMinProp;
        SerializedProperty TimeOfDaySoundsSecondsMaxProp;
        SerializedProperty TimeFoldoutProp;
        SerializedProperty DateFoldoutProp;
        SerializedProperty TimeSoundsFoldoutProp;
        SerializedProperty TimeOfDayMusicDelayProp;
        SerializedProperty TimeMusicFoldoutProp;
        SerializedProperty TimeOfDaySoundsDuringPrecipitationWeatherProp;

        //Date
        SerializedProperty DayProp;
        SerializedProperty MonthProp;
        SerializedProperty YearProp;

        //Weather
        SerializedProperty PrecipitationGraphProp;
        SerializedProperty TemperatureCurveProp;
        SerializedProperty TemperatureFluctuationProp;
        SerializedProperty FogColorProp;
        SerializedProperty FogLightColorProp;
        SerializedProperty StormyFogLightColorProp;
        SerializedProperty FogStormyColorProp;
        SerializedProperty CloudLightColorProp;
        SerializedProperty StormyCloudLightColorProp;
        SerializedProperty CloudBaseColorProp;
        SerializedProperty CloudStormyBaseColorProp;
        SerializedProperty TransitionSpeedProp;
        SerializedProperty WeatherFoldoutProp;
        SerializedProperty FogFoldoutProp;
        SerializedProperty CloudsFoldoutProp;
        SerializedProperty LightningFoldoutProp;
        SerializedProperty LightningStrikeOddsProp;
        SerializedProperty LightningStrikeEffectProp;
        SerializedProperty LightningStrikeFireProp;
        SerializedProperty LightningSecondsMinProp;
        SerializedProperty LightningSecondsMaxProp;
        SerializedProperty LightningLightIntensityMinProp;
        SerializedProperty LightningLightIntensityMaxProp;
        SerializedProperty LightningGenerationDistanceProp;
        SerializedProperty LightningDetectionDistanceProp;
        SerializedProperty CloudFormationSpeedProp;
        SerializedProperty CloudSpeedProp;
        SerializedProperty CloudTurbulenceProp;
        SerializedProperty WeatherGenerationProp;
        SerializedProperty WeatherGenerationMethodProp;
        SerializedProperty CurrentWeatherTypeProp;
        SerializedProperty DetectionLayerMaskProp;
        SerializedProperty TemperatureTypeProp;
        SerializedProperty LightningStrikesProp;
        SerializedProperty CloudTypeProp;
        SerializedProperty CloudDomeTrisCountXProp;
        SerializedProperty CloudDomeTrisCountYProp;
        SerializedProperty ForceLowCloudsProp;
        SerializedProperty LowCloudHeightProp;
        SerializedProperty CloudQualityProp;
        SerializedProperty SunAttenuationCurveProp;
        SerializedProperty StormyHorizonBrightnessProp;
        SerializedProperty CloudShadowsProp;
        SerializedProperty CloudShadowResolutionProp;
        SerializedProperty CloudRenderTypeProp;
        SerializedProperty LightningColorProp;
        SerializedProperty LightningLightColorProp;
        SerializedProperty FogTypeProp;
        SerializedProperty FogModeProp;
        SerializedProperty CameraFogHeightProp;
        SerializedProperty UseRadialDistantFogProp;
        #if EMERALD_AI_PRESENT
        SerializedProperty LightningStrikesEmeraldAIProp;
        SerializedProperty EmeraldAIRagdollForceProp;
        SerializedProperty EmeraldAILightningDamageProp;
        SerializedProperty EmeraldAITagProp;
#endif

        //Celestial
        SerializedProperty SunColorProp;
        SerializedProperty SunSpotColorProp; 
        SerializedProperty CelestialUpdateSecondsProp;
        SerializedProperty CelestialUpdateEnabledProp;
        SerializedProperty StormySunColorProp;
        SerializedProperty MoonColorProp;
        SerializedProperty SkyTintColorProp;
        SerializedProperty SkyColorProp;
        SerializedProperty AmbientSkyLightColorProp;
        SerializedProperty StormyAmbientSkyLightColorProp;
        SerializedProperty AmbientEquatorLightColorProp;
        SerializedProperty StormyAmbientEquatorLightColorProp;
        SerializedProperty AmbientGroundLightColorProp;
        SerializedProperty StormyAmbientGroundLightColorProp;
        SerializedProperty StarLightColorProp;
        SerializedProperty SunRevolutionProp;
        SerializedProperty SunIntensityCurveProp;
        SerializedProperty SunSizeProp;
        SerializedProperty SunFoldoutProp;
        SerializedProperty MoonIntensityCurveProp;
        SerializedProperty MoonSizeProp;
        SerializedProperty MoonPhaseColorProp;
        SerializedProperty MoonFoldoutProp;
        SerializedProperty MoonPhaseIndexProp;
        SerializedProperty MoonBrightnessProp;
        SerializedProperty MoonlightColorProp;
        SerializedProperty AtmosphereFoldoutProp;
        SerializedProperty StarSpeedProp;
        SerializedProperty StarmapTypeProp;
        SerializedProperty SunAngleProp;
        SerializedProperty MoonAngleProp;
        SerializedProperty HemisphereProp;
        SerializedProperty AtmosphereThicknessProp;
        SerializedProperty SunShaftsEffectProp;
        SerializedProperty MoonShaftsEffectProp;
        SerializedProperty EnvironmentReflectionsProp;
        SerializedProperty AmbientIntensityProp;
        SerializedProperty SunShadowTypeProp;
        SerializedProperty MoonShadowTypeProp;
        SerializedProperty LightningShadowTypeProp;
        SerializedProperty SunShadowResolutionProp;
        SerializedProperty MoonShadowResolutionProp;
        SerializedProperty LightningShadowResolutionProp;
        SerializedProperty SunShadowStrengthProp;
        SerializedProperty MoonShadowStrengthProp;
        SerializedProperty LightningShadowStrengthProp;
        SerializedProperty UseDitheringProp;

        //Light Shafts       
        SerializedProperty SunLightShaftIntensityProp;
        SerializedProperty SunLightShaftsColorProp;
        SerializedProperty SunLightShaftsBlurSizeProp;
        SerializedProperty SunLightShaftsBlurIterationsProp;
        SerializedProperty MoonLightShaftIntensityProp;
        SerializedProperty MoonLightShaftsColorProp;
        SerializedProperty MoonLightShaftsBlurSizeProp;
        SerializedProperty MoonLightShaftsBlurIterationsProp;

        Texture TimeIcon;
        Texture WeatherIcon;
        Texture CelestialIcon;
        Texture CameraIcon;
        Texture HelpIcon;
        Texture SettingsIcon;

        //Reorderable lists
        ReorderableList AllWeatherTypesList;
        ReorderableList MoonPhaseList;
        ReorderableList LightningFlashPatternsList;
        ReorderableList ThunderSoundsList;
        ReorderableList MorningSoundsList;
        ReorderableList DaySoundsList;
        ReorderableList EveningSoundsList;
        ReorderableList NightSoundsList;
        ReorderableList MorningMusicList;
        ReorderableList DayMusicList;
        ReorderableList EveningMusicList;
        ReorderableList NightMusicList;
        ReorderableList LightningFireTagsList;

        public float secs = 2f;
        public float startVal = 0f;
        public float progress = 0f;
        public bool Importing = false;

        void OnDisable() { EditorApplication.update -= Update; }

        void UpdateColorProperties()
        {
            SunColorProp = serializedObject.FindProperty("SunColor");
            SunSpotColorProp = serializedObject.FindProperty("SunSpotColor"); 
            StormySunColorProp = serializedObject.FindProperty("StormySunColor");
            MoonColorProp = serializedObject.FindProperty("MoonColor");
            SkyTintColorProp = serializedObject.FindProperty("SkyTintColor");
            SkyColorProp = serializedObject.FindProperty("SkyColor");
            AmbientSkyLightColorProp = serializedObject.FindProperty("AmbientSkyLightColor");
            StormyAmbientSkyLightColorProp = serializedObject.FindProperty("StormyAmbientSkyLightColor");
            AmbientEquatorLightColorProp = serializedObject.FindProperty("AmbientEquatorLightColor");
            StormyAmbientEquatorLightColorProp = serializedObject.FindProperty("StormyAmbientEquatorLightColor");
            AmbientGroundLightColorProp = serializedObject.FindProperty("AmbientGroundLightColor");
            StormyAmbientGroundLightColorProp = serializedObject.FindProperty("StormyAmbientGroundLightColor");
            FogColorProp = serializedObject.FindProperty("FogColor");
            FogLightColorProp = serializedObject.FindProperty("FogLightColor");
            StormyFogLightColorProp = serializedObject.FindProperty("StormyFogLightColor"); 
            FogStormyColorProp = serializedObject.FindProperty("FogStormyColor");
            StarLightColorProp = serializedObject.FindProperty("StarLightColor");
            CloudLightColorProp = serializedObject.FindProperty("CloudLightColor");
            StormyCloudLightColorProp = serializedObject.FindProperty("StormyCloudLightColor"); 
            CloudBaseColorProp = serializedObject.FindProperty("CloudBaseColor");
            CloudStormyBaseColorProp = serializedObject.FindProperty("CloudStormyBaseColor");
        }

        void OnEnable()
        {
            EditorApplication.update += Update;
            UniStormSystem self = (UniStormSystem)target;

            URPForwardRenderer = serializedObject.FindProperty("urpForwardRendererData");

            //5.0
            CustomizeQualityProp = serializedObject.FindProperty("CustomizeQuality");
            ConvergenceSpeedProp = serializedObject.FindProperty("ConvergenceSpeed");
            NearMarchStepsProp = serializedObject.FindProperty("NearMarchSteps"); 
            DistantMarchStepsProp = serializedObject.FindProperty("DistantMarchSteps");
            UpdateMarchStepsDuringRuntimeProp = serializedObject.FindProperty("UpdateMarchStepsDuringRuntime");
            //RendersPerFrameProp = serializedObject.FindProperty("RendersPerFrame");

            //Audio Mixer
            TimeOfDaySoundsVolumeProp = serializedObject.FindProperty("AmbienceVolume");
            TimeOfDayMusicVolumeProp = serializedObject.FindProperty("MusicVolume");
            WeatherSoundsVolumeProp = serializedObject.FindProperty("WeatherSoundsVolume");

            //Editor
            TabNumberProp = serializedObject.FindProperty("TabNumber");
            PlayerTransformProp = serializedObject.FindProperty("PlayerTransform");
            PlayerCameraProp = serializedObject.FindProperty("PlayerCamera");
            CameraFoldoutProp = serializedObject.FindProperty("CameraFoldout");
            SettingsFoldoutProp = serializedObject.FindProperty("SettingsFoldout");
            PlatformFoldoutProp = serializedObject.FindProperty("PlatformFoldout");
            PlatformTypeProp = serializedObject.FindProperty("PlatformType");
            UniStormProfileProp = serializedObject.FindProperty("m_UniStormProfile");
            if (TimeIcon == null) TimeIcon = Resources.Load("TimeIcon") as Texture;
            if (WeatherIcon == null) WeatherIcon = Resources.Load("WeatherIcon") as Texture;
            if (CelestialIcon == null) CelestialIcon = Resources.Load("CelestialIcon") as Texture;
            if (CameraIcon == null) CameraIcon = Resources.Load("CameraIcon") as Texture;
            if (HelpIcon == null) HelpIcon = Resources.Load("HelpIcon") as Texture;
            if (SettingsIcon == null) SettingsIcon = Resources.Load("SettingsIcon") as Texture;

            //Camera & Player
            GetPlayerAtRuntimeProp = serializedObject.FindProperty("GetPlayerAtRuntime");
            GetPlayerMethodProp = serializedObject.FindProperty("GetPlayerMethod");
            UseRuntimeDelayProp = serializedObject.FindProperty("UseRuntimeDelay");
            PlayerTagProp = serializedObject.FindProperty("PlayerTag");
            CameraTagProp = serializedObject.FindProperty("CameraTag");
            PlayerNameProp = serializedObject.FindProperty("PlayerName");
            CameraNameProp = serializedObject.FindProperty("CameraName");
            UseUniStormMenuProp = serializedObject.FindProperty("UseUniStormMenu");
            UniStormMenuKeyProp = serializedObject.FindProperty("UniStormMenuKey");

            //Time
            HourProp = serializedObject.FindProperty("Hour");
            MinuteProp = serializedObject.FindProperty("Minute");
            TimeFlowProp = serializedObject.FindProperty("TimeFlow");
            RealWorldTimeProp = serializedObject.FindProperty("RealWorldTime");
            TimeOfDaySoundsSecondsMinProp = serializedObject.FindProperty("TimeOfDaySoundsSecondsMin");
            TimeOfDaySoundsSecondsMaxProp = serializedObject.FindProperty("TimeOfDaySoundsSecondsMax");
            TimeOfDayMusicDelayProp = serializedObject.FindProperty("TimeOfDayMusicDelay");
            TimeFoldoutProp = serializedObject.FindProperty("TimeFoldout");
            DateFoldoutProp = serializedObject.FindProperty("DateFoldout");
            TimeSoundsFoldoutProp = serializedObject.FindProperty("TimeSoundsFoldout");
            TimeMusicFoldoutProp = serializedObject.FindProperty("TimeMusicFoldout");
            TimeOfDaySoundsDuringPrecipitationWeatherProp = serializedObject.FindProperty("TimeOfDaySoundsDuringPrecipitationWeather");

            //Date
            DayProp = serializedObject.FindProperty("Day");
            MonthProp = serializedObject.FindProperty("Month");
            YearProp = serializedObject.FindProperty("Year");
            DayLengthProp = serializedObject.FindProperty("DayLength");
            NightLengthProp = serializedObject.FindProperty("NightLength");

            //Weather
            PrecipitationGraphProp = serializedObject.FindProperty("PrecipitationGraph");
            TemperatureCurveProp = serializedObject.FindProperty("TemperatureCurve");
            TemperatureFluctuationProp = serializedObject.FindProperty("TemperatureFluctuation");
            FogColorProp = serializedObject.FindProperty("FogColor");
            FogLightColorProp = serializedObject.FindProperty("FogLightColor");
            StormyFogLightColorProp = serializedObject.FindProperty("StormyFogLightColor"); 
            FogStormyColorProp = serializedObject.FindProperty("FogStormyColor");
            CloudLightColorProp = serializedObject.FindProperty("CloudLightColor");
            StormyCloudLightColorProp = serializedObject.FindProperty("StormyCloudLightColor"); 
            CloudBaseColorProp = serializedObject.FindProperty("CloudBaseColor");
            CloudStormyBaseColorProp = serializedObject.FindProperty("CloudStormyBaseColor");
            TransitionSpeedProp = serializedObject.FindProperty("TransitionSpeed");
            WeatherFoldoutProp = serializedObject.FindProperty("WeatherFoldout");
            FogFoldoutProp = serializedObject.FindProperty("FogFoldout");
            CloudsFoldoutProp = serializedObject.FindProperty("CloudsFoldout");
            LightningFoldoutProp = serializedObject.FindProperty("LightningFoldout");
            LightningStrikeOddsProp = serializedObject.FindProperty("LightningGroundStrikeOdds");
            LightningStrikeEffectProp = serializedObject.FindProperty("LightningStrikeEffect");
            LightningStrikeFireProp = serializedObject.FindProperty("LightningStrikeFire");
            LightningSecondsMinProp = serializedObject.FindProperty("LightningSecondsMin");
            LightningSecondsMaxProp = serializedObject.FindProperty("LightningSecondsMax");
            LightningLightIntensityMinProp = serializedObject.FindProperty("LightningLightIntensityMin");
            LightningLightIntensityMaxProp = serializedObject.FindProperty("LightningLightIntensityMax");
            LightningGenerationDistanceProp = serializedObject.FindProperty("LightningGenerationDistance");
            LightningDetectionDistanceProp = serializedObject.FindProperty("LightningDetectionDistance");
            CloudSpeedProp = serializedObject.FindProperty("CloudSpeed");
            CloudTurbulenceProp = serializedObject.FindProperty("CloudTurbulence");
            WeatherGenerationProp = serializedObject.FindProperty("WeatherGeneration");
            WeatherGenerationMethodProp = serializedObject.FindProperty("WeatherGenerationMethod");
            CurrentWeatherTypeProp = serializedObject.FindProperty("CurrentWeatherType");
            DetectionLayerMaskProp = serializedObject.FindProperty("DetectionLayerMask");
            TemperatureTypeProp = serializedObject.FindProperty("TemperatureType");
            LightningStrikesProp = serializedObject.FindProperty("LightningStrikes");
            CloudTypeProp = serializedObject.FindProperty("CloudType");
            CloudDomeTrisCountXProp = serializedObject.FindProperty("CloudDomeTrisCountX");
            CloudDomeTrisCountYProp = serializedObject.FindProperty("CloudDomeTrisCountY");
            ForceLowCloudsProp = serializedObject.FindProperty("ForceLowClouds");
            LowCloudHeightProp = serializedObject.FindProperty("LowCloudHeight");
            CloudQualityProp = serializedObject.FindProperty("CloudQuality");
            SunAttenuationCurveProp = serializedObject.FindProperty("SunAttenuationCurve");
            StormyHorizonBrightnessProp = serializedObject.FindProperty("StormyHorizonBrightness");
            CloudShadowsProp = serializedObject.FindProperty("CloudShadows");
            CloudShadowResolutionProp = serializedObject.FindProperty("CloudShadowResolution");
            CloudRenderTypeProp = serializedObject.FindProperty("CloudRenderType");
            LightningColorProp = serializedObject.FindProperty("LightningColor");
            LightningLightColorProp = serializedObject.FindProperty("LightningLightColor");
            FogTypeProp = serializedObject.FindProperty("FogType");
            FogModeProp = serializedObject.FindProperty("FogMode");
            CameraFogHeightProp = serializedObject.FindProperty("CameraFogHeight");
            UseRadialDistantFogProp = serializedObject.FindProperty("UseRadialDistantFog");
#if EMERALD_AI_PRESENT
            LightningStrikesEmeraldAIProp = serializedObject.FindProperty("LightningStrikesEmeraldAI");
            EmeraldAITagProp = serializedObject.FindProperty("EmeraldAITag");
            EmeraldAIRagdollForceProp = serializedObject.FindProperty("EmeraldAIRagdollForce");
            EmeraldAILightningDamageProp = serializedObject.FindProperty("EmeraldAILightningDamage");
#endif

            //Celestial
            CelestialUpdateSecondsProp = serializedObject.FindProperty("TimeOfDayUpdateSeconds");
            CelestialUpdateEnabledProp = serializedObject.FindProperty("UseTimeOfDayUpdateControl");
            SunColorProp = serializedObject.FindProperty("SunColor");
            SunSpotColorProp = serializedObject.FindProperty("SunSpotColor"); 
            StormySunColorProp = serializedObject.FindProperty("StormySunColor");
            MoonColorProp = serializedObject.FindProperty("MoonColor");
            SkyTintColorProp = serializedObject.FindProperty("SkyTintColor");
            SkyColorProp = serializedObject.FindProperty("SkyColor");
            AmbientSkyLightColorProp = serializedObject.FindProperty("AmbientSkyLightColor");
            StormyAmbientSkyLightColorProp = serializedObject.FindProperty("StormyAmbientSkyLightColor");
            AmbientEquatorLightColorProp = serializedObject.FindProperty("AmbientEquatorLightColor");
            StormyAmbientEquatorLightColorProp = serializedObject.FindProperty("StormyAmbientEquatorLightColor");
            AmbientGroundLightColorProp = serializedObject.FindProperty("AmbientGroundLightColor");
            StormyAmbientGroundLightColorProp = serializedObject.FindProperty("StormyAmbientGroundLightColor");
            StarLightColorProp = serializedObject.FindProperty("StarLightColor");
            SunRevolutionProp = serializedObject.FindProperty("SunRevolution");
            SunIntensityCurveProp = serializedObject.FindProperty("SunIntensityCurve");
            SunSizeProp = serializedObject.FindProperty("SunSize");
            SunFoldoutProp = serializedObject.FindProperty("SunFoldout");
            MoonIntensityCurveProp = serializedObject.FindProperty("MoonIntensityCurve");
            MoonSizeProp = serializedObject.FindProperty("MoonSize");
            MoonPhaseColorProp = serializedObject.FindProperty("MoonPhaseColor");
            MoonFoldoutProp = serializedObject.FindProperty("MoonFoldout");
            MoonPhaseIndexProp = serializedObject.FindProperty("MoonPhaseIndex");
            MoonBrightnessProp = serializedObject.FindProperty("MoonBrightness");
            AtmosphereFoldoutProp = serializedObject.FindProperty("AtmosphereFoldout");
            StarSpeedProp = serializedObject.FindProperty("StarSpeed");
            StarmapTypeProp = serializedObject.FindProperty("StarmapType");
            SunAngleProp = serializedObject.FindProperty("SunAngle");
            MoonAngleProp = serializedObject.FindProperty("MoonAngle");
            HemisphereProp = serializedObject.FindProperty("Hemisphere");
            AtmosphereThicknessProp = serializedObject.FindProperty("AtmosphereThickness");
            SunShaftsEffectProp = serializedObject.FindProperty("SunShaftsEffect");
            MoonShaftsEffectProp = serializedObject.FindProperty("MoonShaftsEffect");
            MoonlightColorProp = serializedObject.FindProperty("MoonlightColor");
            EnvironmentReflectionsProp = serializedObject.FindProperty("EnvironmentReflections");
            AmbientIntensityProp = serializedObject.FindProperty("AmbientIntensityCurve");
            SunShadowTypeProp = serializedObject.FindProperty("SunShadowType");
            MoonShadowTypeProp = serializedObject.FindProperty("MoonShadowType");
            LightningShadowTypeProp = serializedObject.FindProperty("LightningShadowType");
            SunShadowResolutionProp = serializedObject.FindProperty("SunShadowResolution");
            MoonShadowResolutionProp = serializedObject.FindProperty("MoonShadowResolution");
            LightningShadowResolutionProp = serializedObject.FindProperty("LightningShadowResolution");
            SunShadowStrengthProp = serializedObject.FindProperty("SunShadowStrength");
            MoonShadowStrengthProp = serializedObject.FindProperty("MoonShadowStrength");
            LightningShadowStrengthProp = serializedObject.FindProperty("LightningShadowStrength");
            UseDitheringProp = serializedObject.FindProperty("UseDithering");

            //Light Shafts       
            SunLightShaftIntensityProp = serializedObject.FindProperty("SunLightShaftIntensity");
            SunLightShaftsColorProp = serializedObject.FindProperty("SunLightShaftsColor");
            SunLightShaftsBlurSizeProp = serializedObject.FindProperty("SunLightShaftsBlurSize");
            SunLightShaftsBlurIterationsProp = serializedObject.FindProperty("SunLightShaftsBlurIterations");
            MoonLightShaftIntensityProp = serializedObject.FindProperty("MoonLightShaftIntensity");
            MoonLightShaftsColorProp = serializedObject.FindProperty("MoonLightShaftsColor");
            MoonLightShaftsBlurSizeProp = serializedObject.FindProperty("MoonLightShaftsBlurSize");
            MoonLightShaftsBlurIterationsProp = serializedObject.FindProperty("MoonLightShaftsBlurIterations");

            //Reorderable lists
            //Put our lists into reorderable lists because Unity allows these lists to be serialized
            //All weather types
            AllWeatherTypesList = new ReorderableList(serializedObject, serializedObject.FindProperty("AllWeatherTypes"), true, true, true, true);
            AllWeatherTypesList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "All Weather Types", EditorStyles.boldLabel);
            };
            AllWeatherTypesList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = AllWeatherTypesList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Moon Phases New
            MoonPhaseList = new ReorderableList(serializedObject, serializedObject.FindProperty("MoonPhaseList"), true, true, true, true);
            MoonPhaseList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 1;
                    var element = MoonPhaseList.serializedProperty.GetArrayElementAtIndex(index);

                //Give our MoonPhaseIntensity an initialized value of 1.
                if (element.FindPropertyRelative("MoonPhaseIntensity").floatValue == 0)
                    {
                        element.FindPropertyRelative("MoonPhaseIntensity").floatValue = 1;
                    }

                    EditorGUI.PropertyField(new Rect(rect.x + 200, rect.y, rect.width - 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("MoonPhaseIntensity"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("MoonPhaseTexture"), GUIContent.none);
                };

            MoonPhaseList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   Moon Phase Texture  " + "               Light Intensity", EditorStyles.boldLabel);
            };

            //Lightning patterns
            LightningFlashPatternsList = new ReorderableList(serializedObject, serializedObject.FindProperty("LightningFlashPatterns"), true, true, true, true);
            LightningFlashPatternsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Lightning Flash Patterns", EditorStyles.boldLabel);
            };
            LightningFlashPatternsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = LightningFlashPatternsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Thunder Sounds
            ThunderSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("ThunderSounds"), true, true, true, true);
            ThunderSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Thunder Sounds", EditorStyles.boldLabel);
            };
            ThunderSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = ThunderSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Morning Sounds
            MorningSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("MorningSounds"), true, true, true, true);
            MorningSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Morning Sounds", EditorStyles.boldLabel);
            };
            MorningSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = MorningSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Day Sounds
            DaySoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("DaySounds"), true, true, true, true);
            DaySoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Day Sounds", EditorStyles.boldLabel);
            };
            DaySoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = DaySoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Evening Sounds
            EveningSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("EveningSounds"), true, true, true, true);
            EveningSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Evening Sounds", EditorStyles.boldLabel);
            };
            EveningSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = EveningSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Night Sounds
            NightSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("NightSounds"), true, true, true, true);
            NightSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Night Sounds", EditorStyles.boldLabel);
            };
            NightSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = NightSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Morning Music
            MorningMusicList = new ReorderableList(serializedObject, serializedObject.FindProperty("MorningMusic"), true, true, true, true);
            MorningMusicList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Morning Music", EditorStyles.boldLabel);
            };
            MorningMusicList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = MorningMusicList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Day Music
            DayMusicList = new ReorderableList(serializedObject, serializedObject.FindProperty("DayMusic"), true, true, true, true);
            DayMusicList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Day Music", EditorStyles.boldLabel);
            };
            DayMusicList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = DayMusicList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Evening Music
            EveningMusicList = new ReorderableList(serializedObject, serializedObject.FindProperty("EveningMusic"), true, true, true, true);
            EveningMusicList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Evening Music", EditorStyles.boldLabel);
            };
            EveningMusicList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = EveningMusicList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Night Music
            NightMusicList = new ReorderableList(serializedObject, serializedObject.FindProperty("NightMusic"), true, true, true, true);
            NightMusicList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Night Music", EditorStyles.boldLabel);
            };
            NightMusicList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = NightMusicList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            //Lightning Fire Tags
            LightningFireTagsList = new ReorderableList(serializedObject, serializedObject.FindProperty("LightningFireTags"), true, true, true, true);
            LightningFireTagsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Lightning Fire Tags", EditorStyles.boldLabel);
            };
            LightningFireTagsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = LightningFireTagsList.serializedProperty.GetArrayElementAtIndex(index);

                    if (element.stringValue == "")
                    {
                        element.stringValue = "Untagged";
                    }

                    element.stringValue = EditorGUI.TagField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.stringValue);
                };
        }

        public override void OnInspectorGUI()
        {
            UniStormSystem self = (UniStormSystem)target;

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            myFoldoutStyle.fontStyle = FontStyle.Bold;
            myFoldoutStyle.fontSize = 12;
            myFoldoutStyle.active.textColor = Color.black;
            myFoldoutStyle.focused.textColor = Color.white;
            myFoldoutStyle.onHover.textColor = Color.black;
            myFoldoutStyle.normal.textColor = Color.white;
            myFoldoutStyle.onNormal.textColor = Color.black;
            myFoldoutStyle.onActive.textColor = Color.black;
            myFoldoutStyle.onFocused.textColor = Color.black;
            Color myStyleColor = Color.black;

            if (QualitySettings.activeColorSpace == ColorSpace.Gamma && !self.ColorSpaceSuggestionDismissed)
            {
                GUI.backgroundColor = new Color(1f, 1, 0.6f, 0.6f);
                EditorGUILayout.HelpBox("Linear Color Space is highly recommended in order to avoid the lighting from being washed out and to have improved lighting, visuals, and colors. If possible, please switch the Color Space to Linear.", MessageType.Warning);
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Dismiss"))
                {
                    self.ColorSpaceSuggestionDismissed = true;
                }
                if (GUILayout.Button("Switch to Linear Color Space"))
                {
                    if (EditorUtility.DisplayDialog("Switch to Linear Color Space", "Are you sure you want to switch to Linear Color Space? This will require some assets to be reimported.", "Yes", "Cancel"))
                    {
                        PlayerSettings.colorSpace = ColorSpace.Linear;
                    }
                }
                GUILayout.Space(15);
            }

            if (!self.UpgradedToCurrentVersion)
            {
                GUI.backgroundColor = new Color(1f, 1, 0.6f, 0.6f);
                EditorGUILayout.HelpBox("Please update this UniStorm system to be compatible with the current version by pressing the button below. This is required in order to properly use newly added features. Your custom settings" +
                    " will not be affected. However, it is recommended that users use the updated UniStorm prefab system going Window>UniStorm>Create UniStorm, if possible.", MessageType.Error);
                GUI.backgroundColor = Color.white;          
                if (GUILayout.Button("Update to Current Version"))
                {
                    Debug.Log("UniStorm System updated to the current version");
                    UniStormProfile m_UniStormProfile = Resources.Load("UniStorm Data") as UniStormProfile;
                    self.SunAtmosphericFogIntensity = new AnimationCurve(m_UniStormProfile.SunAtmosphericFogIntensity.keys);
                    self.SunControlCurve = new AnimationCurve(m_UniStormProfile.SunControlCurve.keys);
                    self.MoonAtmosphericFogIntensity = new AnimationCurve(m_UniStormProfile.MoonAtmosphericFogIntensity.keys);
                    self.MoonObjectFade = new AnimationCurve(m_UniStormProfile.MoonObjectFade.keys);
                    self.StormyCloudLightColor = UpdateGradient(m_UniStormProfile.StormyCloudLightColor, self.StormyCloudLightColor);
                    StormyCloudLightColorProp = serializedObject.FindProperty("StormyCloudLightColor").Copy();
                    self.FogLightColor = UpdateGradient(m_UniStormProfile.FogLightColor, self.FogLightColor);
                    FogLightColorProp = serializedObject.FindProperty("FogLightColor").Copy();
                    self.StormyFogLightColor = UpdateGradient(m_UniStormProfile.StormyFogLightColor, self.StormyFogLightColor);
                    StormyFogLightColorProp = serializedObject.FindProperty("StormyFogLightColor").Copy();
                    self.SunSpotColor = UpdateGradient(m_UniStormProfile.SunSpotColor, self.SunSpotColor);
                    SunSpotColorProp = serializedObject.FindProperty("SunSpotColor").Copy();

                    serializedObject.ApplyModifiedProperties();

                    self.UpgradedToCurrentVersion = true;
                }
                GUILayout.Space(15);
            }

            var HelpStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.UpperRight };

            GUIContent[] TabButtons = new GUIContent[5] {new GUIContent(" Player & \n Camera", CameraIcon), new GUIContent(" Time", TimeIcon), new GUIContent(" Weather", WeatherIcon),
            new GUIContent(" Celestial", CelestialIcon), new GUIContent(" Settings", SettingsIcon)};

            GUIStyle SelectionGridStyle = new GUIStyle(EditorStyles.miniButton);
            SelectionGridStyle.fixedHeight = 35;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

#if UNITY_2019_3_OR_NEWER
            TabNumberProp.intValue = GUILayout.SelectionGrid(TabNumberProp.intValue, TabButtons, 5, SelectionGridStyle, GUILayout.Height(32), GUILayout.Width(80 * Screen.width / Screen.dpi));
#else
            TabNumberProp.intValue = GUILayout.SelectionGrid(TabNumberProp.intValue, TabButtons, 5, GUILayout.Height(32), GUILayout.Width(EditorGUIUtility.currentViewWidth - 45));
#endif

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            //Camera Settings
            if (TabNumberProp.intValue == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(80 * Screen.width / Screen.dpi));

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/The-UniStorm-Editor#camera--player-settings-tab");
                }

                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(new GUIContent(CameraIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUILayout.LabelField("Camera & Player Settings", style, GUILayout.ExpandWidth(true));

                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                EditorGUILayout.LabelField("The Camera & Player Settings allow you to control what objects UniStorm uses for its player and camera.", EditorStyles.helpBox);

                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you need help, you can hover over each field with your mouse for a tooltip description. " +
                    "Documentation for this section can be found by pressing the ? in the upper right hand corner.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //Camera Settings
                if (!CameraFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                CameraFoldoutProp.boolValue = Foldout(CameraFoldoutProp.boolValue, "Camera & Player Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (CameraFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(GetPlayerAtRuntimeProp,
                            new GUIContent("Get Player at Runtime", "Controls whether or not UniStorm will get your player at runtime. " +
                            "This is useful if your player is instantiated or created at runtime."));
                    EditorGUILayout.Space();

                    if (self.GetPlayerAtRuntime == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.PropertyField(GetPlayerMethodProp,
                            new GUIContent("Get Player Method", "Controls whether your player and camera are found by tag or by name."));
                        EditorGUILayout.Space();

                        if (self.GetPlayerMethod == UniStormSystem.GetPlayerMethodEnum.ByTag)
                        {
                            CustomTagField(new Rect(), new GUIContent("Player Tag", "The tag of your player."), PlayerTagProp, "Player Tag");
                            CustomTagField(new Rect(), new GUIContent("Camera Tag", "The tag of your player's camera."), CameraTagProp, "Camera Tag");
                            EditorGUILayout.Space();
                        }
                        else if (self.GetPlayerMethod == UniStormSystem.GetPlayerMethodEnum.ByName)
                        {
                            EditorGUILayout.PropertyField(PlayerNameProp, new GUIContent("Player Name", "The name of your player."));
                            EditorGUILayout.PropertyField(CameraNameProp, new GUIContent("Camera Name", "The name of your player's camera."));
                            EditorGUILayout.Space();
                        }

                        EditorGUILayout.PropertyField(UseRuntimeDelayProp,
                            new GUIContent("Use Runtime Delay", "Controls whether or not UniStorm will wait to initialize until the runtime player has been created and found. " +
                            "Note: Upon initialization, there will be a brief generation process that will take about 0.2 seconds."));
                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    if (self.GetPlayerAtRuntime == UniStormSystem.EnableFeature.Disabled)
                    {
                        CustomObjectField(new Rect(), new GUIContent(), PlayerTransformProp,
                        new GUIContent("Player Transform", "The parent transform your player uses."), typeof(Transform), true);
                        CustomObjectField(new Rect(), new GUIContent(), PlayerCameraProp,
                        new GUIContent("Player Camera", "The main camera your player uses."), typeof(Camera), true);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(UseUniStormMenuProp,
                        new GUIContent("Use UniStorm Menu", "Controls whether UniStorm's UI menu is usable during runtime. This menu allows the ability to control UniStorm's " +
                        "time and weather via a slider and drop down UI."));
                    EditorGUILayout.Space();

                    if (self.UseUniStormMenu == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.PropertyField(UniStormMenuKeyProp,
                        new GUIContent("UniStorm Menu Key", "Controls which key will enable UniStorm's menu."));
                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            //Time Settings
            if (TabNumberProp.intValue == 1)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(80 * Screen.width / Screen.dpi));

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/The-UniStorm-Editor#unistorm-time-management-tab");
                }

                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(new GUIContent(TimeIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUILayout.LabelField("Time Management", style, GUILayout.ExpandWidth(true));

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (self.Minute < 10)
                {
                    EditorGUILayout.LabelField("Time: " + self.Hour + ":" + 0 + self.Minute + "  Date: " + self.Month + "/" + self.Day + "/" + self.Year +
                        "  Day of the Week: " + self.UniStormDate.DayOfWeek.ToString(), EditorStyles.helpBox);
                }
                else
                {
                    EditorGUILayout.LabelField("Time: " + self.Hour + ":" + self.Minute + "  Date: " + self.Month + "/" + self.Day + "/" + self.Year +
                        "  Day of the Week: " + self.UniStormDate.DayOfWeek.ToString(), EditorStyles.helpBox);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    self.CalculateSeason();
                }
#endif
                EditorGUILayout.LabelField("Current Season: " + self.CurrentSeason, EditorStyles.helpBox);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                EditorGUILayout.LabelField("The Time Management section allow you to control various time related settings such as starting time, starting date, and time flow.", EditorStyles.helpBox);

                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you need help, you can hover over each field with your mouse for a tooltip description. " +
                    "Documentation for this section can be found by pressing the ? in the upper right hand corner.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //Time Settings
                if (!TimeFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                TimeFoldoutProp.boolValue = Foldout(TimeFoldoutProp.boolValue, "Time Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (TimeFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    if (self.RealWorldTime == UniStormSystem.EnableFeature.Disabled)
                    {
                        CustomIntSlider(new Rect(), new GUIContent(), HourProp,
                        new GUIContent("Hour", "The starting hour UniStorm will start with."), 0, 23);
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), MinuteProp,
                            new GUIContent("Minute", "The starting minute UniStorm will start with."), 0, 59);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(TimeFlowProp,
                            new GUIContent("Time Flow", "Controls whether UniStorm's time is currently flowing."));
                        EditorGUILayout.Space();
                    }
                    else if (self.RealWorldTime == UniStormSystem.EnableFeature.Enabled)
                    {
                        GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                        EditorGUILayout.LabelField("The time settings cannot be set when using the Real-world Time setting.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(RealWorldTimeProp,
                        new GUIContent("Use Real-world Time", "Controls whether UniStorm's time will use real-world time. " +
                        "Note: This will overwrite your starting time on start and your UniStorm Date will use the real-world date."));
                    EditorGUILayout.Space();

                    if (self.RealWorldTime == UniStormSystem.EnableFeature.Disabled)
                    {
                        CustomIntSlider(new Rect(), new GUIContent(), DayLengthProp,
                            new GUIContent("Day Length", "Controls how long, in minutes, UniStorm's Days are."), 1, 500);
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), NightLengthProp,
                            new GUIContent("Night Length", "Controls how long, in minutes, UniStorm's Nights are."), 1, 500);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }
                EditorGUILayout.EndVertical();

                //Date Settings
                if (!DateFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                DateFoldoutProp.boolValue = Foldout(DateFoldoutProp.boolValue, "Date Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (DateFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    if (self.RealWorldTime == UniStormSystem.EnableFeature.Disabled)
                    {
                        CustomIntSlider(new Rect(), new GUIContent(), MonthProp,
                        new GUIContent("Month", "The starting month UniStorm will start with."), 1, 12);
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), DayProp,
                            new GUIContent("Day", "The starting day UniStorm will start with."), 1, 31);
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), YearProp,
                            new GUIContent("Year", "The starting year UniStorm will start with."), -2000, 3000);
                        EditorGUILayout.Space();
                    }
                    else if (self.RealWorldTime == UniStormSystem.EnableFeature.Enabled)
                    {
                        GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                        EditorGUILayout.LabelField("The date settings cannot be set when using the Real-world Time setting.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();


                //Time Sounds Settings
                if (!TimeSoundsFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                TimeSoundsFoldoutProp.boolValue = Foldout(TimeSoundsFoldoutProp.boolValue, "Time of Day Sounds Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (TimeSoundsFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(TimeOfDaySoundsDuringPrecipitationWeatherProp,
                        new GUIContent("Play During Precipitation", "Controls whether UniStorm's Time of Day sounds will play during precipitation weather types."));
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), TimeOfDaySoundsSecondsMinProp,
                        new GUIContent("Time of Day Sounds Min", "The minimum seconds needed for a time of day sound to trigger."), 0, self.TimeOfDaySoundsSecondsMax);
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), TimeOfDaySoundsSecondsMaxProp,
                        new GUIContent("Time of Day Sounds Max", "The maximum seconds needed for a time of day sound to trigger."), self.TimeOfDaySoundsSecondsMin, 60);
                    EditorGUILayout.Space();

                    CustomFloatSlider(new Rect(), new GUIContent(), TimeOfDaySoundsVolumeProp,
                        new GUIContent("Time of Day Sounds Volume", "The volume of the time of day sounds. This value alters UniStorm's Ambience Audio Mixer volume."), 0f, 1f);
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible sounds that UniStorm will play during the Morning."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    MorningSoundsList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible sounds that UniStorm will play during the Day."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    DaySoundsList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible sounds that UniStorm will play during the Evening."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    EveningSoundsList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible sounds that UniStorm will play during the Night."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    NightSoundsList.DoLayoutList();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

                //Time Music Settings
                if (!TimeMusicFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                TimeMusicFoldoutProp.boolValue = Foldout(TimeMusicFoldoutProp.boolValue, "Time of Day Music Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (TimeMusicFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    CustomIntSlider(new Rect(), new GUIContent(), TimeOfDayMusicDelayProp,
                        new GUIContent("Time of Day Music Delay", "The delay, in seconds, between each time of day music clip."), 0, 30);
                    EditorGUILayout.Space();

                    CustomFloatSlider(new Rect(), new GUIContent(), TimeOfDayMusicVolumeProp,
                        new GUIContent("Time of Day Music Volume", "The volume of the time of day music. This value alters UniStorm's Music Audio Mixer volume."), 0f, 1f);
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible music clips that UniStorm will play during the Morning."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    MorningMusicList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible music clips that UniStorm will play during the Day."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    DayMusicList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible music clips that UniStorm will play during the Evening."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    EveningMusicList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible music clips that UniStorm will play during the Night."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    NightMusicList.DoLayoutList();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

            }

            //Weather Settings
            if (TabNumberProp.intValue == 2)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(80 * Screen.width / Screen.dpi));

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/The-UniStorm-Editor#weather-management-tab");
                }

                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(new GUIContent(WeatherIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUILayout.LabelField("Weather Management", style, GUILayout.ExpandWidth(true));

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Temperature: " + self.Temperature, EditorStyles.helpBox);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (self.CurrentWeatherType != null)
                {
                    EditorGUILayout.LabelField("Current Weather: " + self.CurrentWeatherType.WeatherTypeName, EditorStyles.helpBox);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (self.NextWeatherType != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (self.WeatherGenerationMethod == UniStormSystem.WeatherGenerationMethodEnum.Daily)
                    {
                        EditorGUILayout.LabelField("Forecast: " + self.NextWeatherType.WeatherTypeName + " at " + self.HourToChangeWeather + ":00", EditorStyles.helpBox);
                    }
                    else if (self.WeatherGenerationMethod == UniStormSystem.WeatherGenerationMethodEnum.Hourly && self.Hour < 23)
                    {
                        EditorGUILayout.LabelField("Forecast: " + self.NextWeatherType.WeatherTypeName + " at " + (self.Hour + 1) + ":00", EditorStyles.helpBox);
                    }
                    else if (self.WeatherGenerationMethod == UniStormSystem.WeatherGenerationMethodEnum.Hourly && self.Hour == 23)
                    {
                        EditorGUILayout.LabelField("Forecast: " + "?", EditorStyles.helpBox);
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }

                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                EditorGUILayout.LabelField("The Weather Management section allow you to adjust various settings related to weather. These include the Weather Types that UniStorm can use, " +
                    "fog and cloud colors, and lightning settings.", EditorStyles.helpBox);

                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you need help, you can hover over each field with your mouse for a tooltip description. " +
                    "Documentation for this section can be found by pressing the ? in the upper right hand corner.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //Clouds Settings
                if (!CloudsFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();             

                CloudsFoldoutProp.boolValue = Foldout(CloudsFoldoutProp.boolValue, "Cloud Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (CloudsFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(CloudShadowsProp,
                       new GUIContent("Cloud Shadows", "Controls whether UniStorm's clouds will use screen space cloud shadows."));
                    EditorGUILayout.Space();

                    if (self.CloudShadows == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(5);

                        EditorGUILayout.PropertyField(CloudShadowResolutionProp,
                       new GUIContent("Cloud Shadow Resolution", "Controls the resolution of the screen space cloud shadows."));
                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    CustomIntSlider(new Rect(), new GUIContent(), CloudSpeedProp,
                       new GUIContent("Cloud Speed", "Controls how fast UniStorm's procedural clouds move across the sky."), 0, 50);
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), CloudTurbulenceProp,
                       new GUIContent("Cloud Turbulence", "Controls how fast the noise of UniStorm's procedural clouds moves."), 0, 25);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(CloudQualityProp,
                        new GUIContent("Cloud Quality", "Controls the quality of clouds UniStorm will use when rendering its clouds."));

                    if (self.CloudQuality == UniStormSystem.CloudQualityEnum.Ultra)
                    {
                        EditorGUILayout.Space();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(5);

                        EditorGUILayout.PropertyField(CustomizeQualityProp,
                       new GUIContent("Customize Quality", "Allows users to fine tune the internal cloud settings and qualities. (Limited to only Ultra Cloud Quality)"));

                        if (self.CustomizeQuality == UniStormSystem.CustomizeQualityEnum.Yes)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(5);

                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(UpdateMarchStepsDuringRuntimeProp,
                            new GUIContent("Update During Runtime", "Controls if the Customize Quality settings are updated during runtime so the changes made here can be visible in real-time. Enabling this is only recommended for fidning the desired settings then this feature should be disabled."));
                            EditorGUILayout.Space();

                            CustomIntSlider(new Rect(), new GUIContent(), ConvergenceSpeedProp,
                            new GUIContent("Convergence Speed", "Controls how quickly the clouds are updated. The higher the value, the quicker they are updated, however, more artifacts will be visible. To counteract this, you can increase the Distant Samples, which will affect performance."), 1, 150);
                            EditorGUILayout.Space();

                            GUI.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.75f);
                            EditorGUILayout.LabelField("Note: Modifying the samples past the defaults can greatly affect performance so use wisely.", EditorStyles.helpBox);
                            GUI.backgroundColor = Color.white;

                            if (GUILayout.Button("Reset Quality Settings to Default"))
                            {
                                //self.RendersPerFrame = 1;
                                self.NearMarchSteps = 100;
                                self.DistantMarchSteps = 10;
                                self.ConvergenceSpeed = 75;
                                self.UpdateMarchStepsDuringRuntime = UniStormSystem.EnableFeature.Disabled;
                            }

                            EditorGUILayout.Space();
                            CustomIntSlider(new Rect(), new GUIContent(), NearMarchStepsProp,
                            new GUIContent("Near Samples", "Controls the quality of the clouds nearest to the player by making them higher quality and reducing artifacts."), 100, 200);

                            CustomIntSlider(new Rect(), new GUIContent(), DistantMarchStepsProp,
                            new GUIContent("Distant Samples", "Controls the quality of the clouds in the distance by making them higher quality and reducing artifacts."), 1, 100);

                            /*
                            EditorGUILayout.Space();
                            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                            if (self.RendersPerFrame == 1)
                                EditorGUILayout.LabelField("The clouds will render 1 time per frame. (Default - Most Performant)", EditorStyles.helpBox);
                            else if (self.RendersPerFrame == 2)
                                EditorGUILayout.LabelField("The clouds will render 2 times per frame. (Balanced)", EditorStyles.helpBox);
                            else if (self.RendersPerFrame == 3)
                                EditorGUILayout.LabelField("The clouds will render 3 times per frame. (Best Visuals - Less Performant)", EditorStyles.helpBox);
                            GUI.backgroundColor = Color.white;

                            CustomIntSlider(new Rect(), new GUIContent(), RendersPerFrameProp,
                            new GUIContent("Renders Per Frame", "Controls how many times the clouds are rendered per frame. By Default, this is 1. UniStorm's clouds are then updated over the cource of 16 frames. " +
                            "This settings reduces this update time by half for each number increase allowing the clouds to update quicker and be more responsive, at the cost of performance."), 1, 3);
                            EditorGUILayout.Space();
                            */

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(CloudRenderTypeProp,
                       new GUIContent("Cloud Render Type", "Controls whether UniStorm's will clouds render as more opaque or transparent. Some Image Effects can make the sun more " +
                       "visible behind clouds. When this happens, it's best to use the Opaque render type to correct this."));
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(CloudTypeProp,
                       new GUIContent("Cloud Type", "Controls the type of clouds UniStorm will use when rendering its clouds."));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    if (self.CloudType == UniStormSystem.CloudTypeEnum.Volumetric)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                        EditorGUILayout.LabelField("Volumetric - Clouds will be procedural and rendered as 3D height based volumetric clouds (Better Visuals).", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                    }
                    else if (self.CloudType == UniStormSystem.CloudTypeEnum._2D)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                        EditorGUILayout.LabelField("2D - Clouds will be procedural and rendered as 2D flat clouds (Better Performing).", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), CloudDomeTrisCountXProp,
                        new GUIContent("Cloud Dome Tris Count X", "Controls the amount of triangles on UniStorm's Cloud Dome's X axis. Higher value results in less mesh artifacts, but higher overall triangle count for UniStorm's Cloud Dome."), 38, 200);
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), CloudDomeTrisCountYProp,
                        new GUIContent("Cloud Dome Tris Count Y", "Controls the amount of triangles on UniStorm's Cloud Dome's Y axis. Higher value results in less mesh artifacts, but higher overall triangle count for UniStorm's Cloud Dome."), 16, 50);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(ForceLowCloudsProp,
                       new GUIContent("Force Low Clouds", "Forces all Weather Types to ignore their cloud height and render low clouds. These can visually look and perform better, but this depends on preference."));
                    EditorGUILayout.Space();

                    if (self.ForceLowClouds == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();
                        CustomIntSlider(new Rect(), new GUIContent(), LowCloudHeightProp,
                            new GUIContent("Low Cloud Height", "Controls the low cloud height."), 200, 800);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(CloudBaseColorProp,
                        new GUIContent("Cloud Color", "A gradient that controls the base color of UniStorm's clouds during non-precipitation Weather Types  (Unless being driven by the Cloud Color Override on Weather Types). " +
                        "Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(CloudStormyBaseColorProp,
                        new GUIContent("Stormy Cloud Color", "A gradient that controls the base color of UniStorm's clouds during precipitation Weather Types. " +
                        "Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(CloudLightColorProp,
                        new GUIContent("Cloud Light Color", "A gradient that controls the light color of UniStorm's clouds when the clouds are receiving light during non-precipitation weather types. " +
                        "Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(StormyCloudLightColorProp,
                        new GUIContent("Stormy Cloud Light Color", "A gradient that controls the light color of UniStorm's clouds when the clouds are receiving light during precipitation weather types. " +
                        "Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                //Fog Settings
                if (!FogFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                FogFoldoutProp.boolValue = Foldout(FogFoldoutProp.boolValue, "Fog Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (FogFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(FogTypeProp,
                        new GUIContent("Fog Type", "Controls the type of fog UniStorm will use."));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();

                    if (self.FogType == UniStormSystem.FogTypeEnum.UnityFog)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                        EditorGUILayout.LabelField("Unity Fog - Uses Unity's built-in fog. This option doesn't allow the clouds to match with the horizon color, but is slightly less intensive. It is recommened to use the UniStorm Fog Type.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                    }
                    else if (self.FogType == UniStormSystem.FogTypeEnum.UnistormFog)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                        EditorGUILayout.LabelField("UniStorm Fog - Uses UniStorm's custom Atmospheric Fog that uses the sunlight's and moonlight's color and blends it perfectly with the horizon and UniStorm's clouds.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.Space();

                        CustomFloatSlider(new Rect(), new GUIContent(), CameraFogHeightProp,
                        new GUIContent("Skybox Fog Height", "Controls how much UniStorm's Fog affects the skybox. The higher far clipping plane your camera uses, the higher this value will need to be set. " +
                        "Note: This setting has an option to be overridden within each Weather Type for futher customization, if desired."), 0, 1);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(UseRadialDistantFogProp,
                         new GUIContent("Use Radial Distance", "Allows UniStorm's Fog to be applied consistently around the player in the distance mainly near the outer edges of the camera."));
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(UseDitheringProp,
                         new GUIContent("Use Dithering", "Applies Dithering to UniStorm's fog, clouds, and sky to greatly reduce banding."));
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    if (self.FogType == UniStormSystem.FogTypeEnum.UnityFog)
                    {
                        CustomFloatSlider(new Rect(), new GUIContent(), StormyHorizonBrightnessProp,
                            new GUIContent("Stormy Horizon Brightness", "Controls the brightness of UniStom's horizon during precipiation weather types. " +
                            "This can be useful to allow better blending with the terrain and the clouds."), 1, 2.25f);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(FogModeProp,
                        new GUIContent("Fog Mode", "Controls the mode of fog UniStorm will use."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(FogColorProp,
                        new GUIContent("Fog Color", "A gradient that controls the fog color during non-precipitation Weather Types (Unless being driven by the Fog Color Override on Weather Types). Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();                   

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(FogStormyColorProp,
                        new GUIContent("Stormy Fog Color", "A gradient that controls the fog color during precipitation Weather Types (Unless being driven by the Fog Color Override on Weather Types). Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(FogLightColorProp,
                        new GUIContent("Fog Light Color", "A gradient that controls the color of the fog light during non-precipitation Weather Types. Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(StormyFogLightColorProp,
                        new GUIContent("Stormy Fog Light Color", "A gradient that controls the color of the fog light during precipitation Weather Types. Each element is a transition into the next time of day."));
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                //Weather Settings
                if (!WeatherFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                WeatherFoldoutProp.boolValue = Foldout(WeatherFoldoutProp.boolValue, "Weather Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (WeatherFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(WeatherGenerationProp,
                        new GUIContent("Weather Generation", "Controls whether UniStorm's weather generation is active or disabled. If disabled, UniStorm's weather will remain constant " +
                        "unless changed through code or the UI."));

                    if (self.WeatherGeneration == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(10);

                        EditorGUILayout.PropertyField(WeatherGenerationMethodProp,
                            new GUIContent("Weather Generation Type", "Controls whether UniStorm's weather is generated per hour or per day. When using Daily weather generation, a generated hour " +
                            "for the weather change is also generated."));
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(PrecipitationGraphProp,
                            new GUIContent("Precipiation Odds", "Controls the odds of UniStorm's precipitation throughout the year. X represents the calendar month and Y represents the odds."));
                        EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    CustomIntSlider(new Rect(), new GUIContent(), TransitionSpeedProp,
                        new GUIContent("Transition Speed", "Controls how fast UniStorm will transition Weather Types."), 1, 20);
                    EditorGUILayout.Space();

                    CustomFloatSlider(new Rect(), new GUIContent(), WeatherSoundsVolumeProp,
                            new GUIContent("Weather Sounds Volume", "The global volume for UniStorm's weather sounds. Each weather effect will still have its own volume. " +
                            "This settings simply controls the global volume of all weather sounds and thunder. This value alters UniStorm's Weather Audio Mixer volume."), 0f, 1f);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(TemperatureTypeProp,
                        new GUIContent("Temperature Type", "Controls whether UniStorm uses the Fahrenheit or Celsius temperature type. Fahrenheit requires a temperature of 32 degrees or colder for snow" +
                        " and Celsius requires a temperature of 0 degrees or colder for snow."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(TemperatureCurveProp,
                        new GUIContent("Temperature Curve", "Controls UniStorm's temperature throughout the year. X represents the calendar month and Y represents the temperature."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(TemperatureFluctuationProp,
                        new GUIContent("Temperature Fluctuation", "Controls UniStorm's temperature fluctuation throughout the day. X represents the hour and Y represents the temperature fluctuation."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    CustomObjectField(new Rect(), new GUIContent(), CurrentWeatherTypeProp,
                        new GUIContent("Starting Weather Type", "The starting weather type UniStorm will start with."), typeof(WeatherType), true);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.Box(new GUIContent("What's This?", "A list of all possible Weather Types that UniStorm will use when generating weather. " +
                        "To create a new Weather Type, right click in the project tab and go to Create>UniStorm>New Weather Type."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    AllWeatherTypesList.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                //Lightning Settings
                if (!LightningFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                LightningFoldoutProp.boolValue = Foldout(LightningFoldoutProp.boolValue, "Lightning Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (LightningFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(LightningLightColorProp,
                            new GUIContent("Lightning Light Color", "Controls the color of UniStorm's lightning light color."));
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(LightningShadowTypeProp,
                        new GUIContent("Lightning Shadow Type", "Controls the type of shadow UniStorm's lightning will use."));

                    if (self.LightningShadowType == LightShadows.None)
                    {
                        EditorGUILayout.Space();
                        GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                        EditorGUILayout.LabelField("The lightning's shadows are disabled so its shadow settings are disabled.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        GUILayout.Space(-10);
                    }

                    EditorGUI.BeginDisabledGroup(self.LightningShadowType == LightShadows.None);
                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), LightningShadowStrengthProp,
                            new GUIContent("Lightning Shadow Strength", "Controls the strength of the lightning's shadow"), 0.1f, 1);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(LightningShadowResolutionProp,
                        new GUIContent("Lightning Shadow Resoultion", "Controls the resolution of the lightning's shadow."));
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    CustomIntSlider(new Rect(), new GUIContent(), LightningSecondsMinProp,
                    new GUIContent("Min Lightning Seconds", "Controls the minimum seconds for lightning to be generated."), 2, self.LightningSecondsMax);

                    CustomIntSlider(new Rect(), new GUIContent(), LightningSecondsMaxProp,
                    new GUIContent("Max Lightning Seconds", "Controls the maximum seconds for lightning to be generated."), self.LightningSecondsMin + 1, 60);

                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), LightningLightIntensityMinProp,
                    new GUIContent("Min Lightning Intensity", "Controls the minimum light intensity for the lightning to be generated."), 1, self.LightningLightIntensityMax);

                    CustomFloatSlider(new Rect(), new GUIContent(), LightningLightIntensityMaxProp,
                    new GUIContent("Max Lightning Intensity", "Controls the maximum light intensity for the lightning to be generated."), 1, 4);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(LightningStrikesProp,
                        new GUIContent("Lightning Bolts & Strikes", "Controls whether or not lightning bolts will be created during lightning weather types."));
                    EditorGUILayout.Space();

                    if (self.LightningStrikes == UniStormSystem.EnableFeature.Enabled)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(LightningColorProp,
                            new GUIContent("Lightning Bolt Color", "Controls the color of UniStorm's generated lightning bolts."));
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), LightningGenerationDistanceProp,
                        new GUIContent("Generation Distance", "Controls the maximum distance lightning can be generated around the player."), 50, 1000);
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), LightningDetectionDistanceProp,
                        new GUIContent("Detection Distance", "Controls the distance of UniStorm's lightning strike collider. The larger the radius, the more likely lightning will strike " +
                        "objects instead of the ground."), 5, 60);
                        EditorGUILayout.Space();

                        EditorGUILayout.Space();
                        EditorGUI.BeginChangeCheck();
                        var layersSelection = EditorGUILayout.MaskField(new GUIContent("Lightning Strike Layers", "Controls what layers UniStorm's procedural lightning can strike."), LayerMaskToField(DetectionLayerMaskProp.intValue), InternalEditorUtility.layers);
                        EditorGUILayout.Space();
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(self, "Layers changed");
                            DetectionLayerMaskProp.intValue = FieldToLayerMask(layersSelection);
                        }

#if EMERALD_AI_PRESENT
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(LightningStrikesEmeraldAIProp,
                            new GUIContent("Lightning Strikes Emerald AI", "When enabled, lightning can strike and kill Emerald AI agents. The amount of damage and ragdoll force can also be customized."));

                        if (self.LightningStrikesEmeraldAI == UniStormSystem.EnableFeature.Enabled)
                        {
                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            EditorGUILayout.BeginVertical();

                            CustomTagField(new Rect(), new GUIContent("Emerald AI Tag", "The tag of your Emerald AI agents."), EmeraldAITagProp, "Emerald AI Tag");
                            EditorGUILayout.Space();

                            CustomIntSlider(new Rect(), new GUIContent(), EmeraldAILightningDamageProp,
                        new GUIContent("Lightning Damage", "Controls the amount of damage the lightning will cause struck Emerald AI agents."), 1, 1000);
                            EditorGUILayout.Space();

                            CustomIntSlider(new Rect(), new GUIContent(), EmeraldAIRagdollForceProp,
                        new GUIContent("Ragdoll Force", "Controls the amount of ragdoll force the lightning will cause struck Emerald AI agents when killed, given that they have Ragdolls enabled."), 100, 6000);
                            EditorGUILayout.Space();

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
#endif

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        GUILayout.Box(new GUIContent("What's This?", "A list of tags that will create a fire particle effect when struck by lightning."),
                            EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                        LightningFireTagsList.DoLayoutList();
                        EditorGUILayout.Space();

                        CustomIntSlider(new Rect(), new GUIContent(), LightningStrikeOddsProp,
                            new GUIContent("Ground Strike Odds", "Controls the odds in which UniStorm's lightning will strike the ground or other objects of the appropriate tag."), 0, 100);

                        EditorGUILayout.Space();
                        CustomObjectField(new Rect(), new GUIContent(), LightningStrikeEffectProp,
                            new GUIContent("Lightning Strike Effect", "The effect that plays when lightning strikes the ground."), typeof(GameObject), true);

                        EditorGUILayout.Space();
                        CustomObjectField(new Rect(), new GUIContent(), LightningStrikeFireProp,
                            new GUIContent("Lightning Strike Fire", "The fire effect that plays when lightning strikes an object of the appropriate tag."), typeof(GameObject), true);                     

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible lightning flash patterns that UniStorm will use during lightning Weather Types."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    LightningFlashPatternsList.DoLayoutList();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of possible thunder sounds that UniStorm will play during lightning Weather Types."),
                        EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    ThunderSoundsList.DoLayoutList();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                GUI.backgroundColor = Color.white;
            }

            //Celestial Settings
            if (TabNumberProp.intValue == 3)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(80 * Screen.width / Screen.dpi));

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/The-UniStorm-Editor#celestial-settings-tab");
                }

                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(new GUIContent(CelestialIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUILayout.LabelField("Celestial Settings", style, GUILayout.ExpandWidth(true));

                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                EditorGUILayout.LabelField("The Celestial Settings allow you to control various celestial settings and colors for UniStorm's sun, moon, stars, and atmosphere.", EditorStyles.helpBox);

                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you need help, you can hover over each field with your mouse for a tooltip description. " +
                    "Documentation for this section can be found by pressing the ? in the upper right hand corner.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //Sun Settings
                if (!SunFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                SunFoldoutProp.boolValue = Foldout(SunFoldoutProp.boolValue, "Sun Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (SunFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();                  
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunSpotColorProp,
                        new GUIContent("Sun Object Color", "An HDR gradient that controls UniStorm's sun object color. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunShadowTypeProp,
                        new GUIContent("Sun Shadow Type", "Controls the type of shadow UniStorm's sun will use."));

                    if (self.SunShadowType == LightShadows.None)
                    {
                        EditorGUILayout.Space();
                        GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                        EditorGUILayout.LabelField("The sun's shadows are disabled so its shadow settings are disabled.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        GUILayout.Space(-10);
                    }

                    EditorGUI.BeginDisabledGroup(self.SunShadowType == LightShadows.None);
                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), SunShadowStrengthProp,
                            new GUIContent("Sun Shadow Strength", "Controls the strength of the sun's shadow"), 0.1f, 1);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunShadowResolutionProp,
                        new GUIContent("Sun Shadow Resoultion", "Controls the resolution of the sun's shadow."));
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunShaftsEffectProp,
                        new GUIContent("Sun Shafts Effect", "Controls whether or not UniStorm will automatically create and use a Sun Shafts Image Effect for the sun."));

                    if (self.SunShaftsEffect == UniStormSystem.EnableFeature.Enabled)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.PropertyField(SunLightShaftIntensityProp,
                            new GUIContent("Sun Shafts Intensity", "Controls the light shafts for UniStorm's sun for each time of day. X represents the hour and Y represents the intensity."));
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(SunLightShaftsColorProp,
                            new GUIContent("Sun Shafts Color", "A gadient that controls the color of the light shafts for UniStorm's sun. Each element is a transition into the next time of day."));
                        EditorGUILayout.Space();
                        CustomFloatSlider(new Rect(), new GUIContent(), SunLightShaftsBlurSizeProp,
                            new GUIContent("Sun Shafts Blur Size", "Controls the size of the light shafts' blurring for UniStorm's sun."), 0.1f, 10);
                        EditorGUILayout.Space();
                        CustomIntSlider(new Rect(), new GUIContent(), SunLightShaftsBlurIterationsProp,
                            new GUIContent("Sun Shafts Blur Iterations", "Controls the amount of iterations used for the blurring of the sun shafts for UniStorm's sun."), 1, 3);

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunColorProp,
                        new GUIContent("Sun Color", "A gradient that controls UniStorm's sun color during non-precipitation weather types. Each element is a transition into the next time of day."));
                    EditorGUILayout.PropertyField(StormySunColorProp,
                        new GUIContent("Stormy Sun Color", "A gradient that controls UniStorm's sun color during precipitation weather types. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(CelestialUpdateEnabledProp,
                        new GUIContent("Use Celestial Light Update", "Controls whether or not UniStorm's sun and moon are updated based on second intervals rather than continuously. This allows for cleaner shadows at the cost of a brief visible update to celestial light sources every update interval. (Recommended for days lengths of 25 or greater)"));

                    if (self.UseTimeOfDayUpdateControl == UniStormSystem.UseTimeOfDayUpdateSeconds.Yes)
                    {
                        EditorGUILayout.Space();
                        CustomIntSlider(new Rect(), new GUIContent(), CelestialUpdateSecondsProp,
                                new GUIContent("Celestial Update Seconds", "Controls, in seconds, how often both the sun and moon's positions are updated. This allows for cleaner shadows at the cost of a brief visible update to celestial shadows every update interval. (Recommended for days lengths of 25 or greater)"), 0, 15);
                    }                   

                    EditorGUILayout.Space();
                    CustomIntSlider(new Rect(), new GUIContent(), SunAngleProp,
                            new GUIContent("Sun Tilt Angle", "Controls the tilt angle of the Sun."), -45, 45);

                    EditorGUILayout.Space();
                    CustomIntSlider(new Rect(), new GUIContent(), SunRevolutionProp,
                        new GUIContent("Sun Revolution", "Controls the direction in which UniStorm's sun sets and rise."), -180, 180);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(SunIntensityCurveProp,
                        new GUIContent("Sun Intensity Curve", "Controls the intensity of UniStorm's sun. X represents the hour and Y represents the intensity."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SunAttenuationCurveProp,
                        new GUIContent("Sun Attenuation Curve", "Controls the intensity of the clouds' sun light attenuation (How much the clouds are affected by the light of the sun). " +
                        "X represents the hour and Y represents the intensity."));
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(SunSizeProp,
                        new GUIContent("Sun Size Curve", "Controls the size of UniStorm's sun. X represents the hour and Y represents the size."));
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                //Moon Settings
                if (!MoonFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                MoonFoldoutProp.boolValue = Foldout(MoonFoldoutProp.boolValue, "Moon Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (MoonFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonShadowTypeProp,
                        new GUIContent("Moon Shadow Type", "Controls the type of shadow UniStorm's moon will use."));

                    if (self.MoonShadowType == LightShadows.None)
                    {
                        EditorGUILayout.Space();
                        GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                        EditorGUILayout.LabelField("The moon's shadows are disabled so its shadow settings are disabled.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        GUILayout.Space(-10);
                    }

                    EditorGUI.BeginDisabledGroup(self.MoonShadowType == LightShadows.None);
                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), MoonShadowStrengthProp,
                            new GUIContent("Moon Shadow Strength", "Controls the strength of the moon's shadow"), 0.1f, 1);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonShadowResolutionProp,
                        new GUIContent("Moon Shadow Resoultion", "Controls the resolution of the moon's shadow."));
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonShaftsEffectProp,
                        new GUIContent("Moon Shafts Effect", "Controls whether or not UniStorm will automatically create and use a Moon Shafts Image Effect for the moon."));

                    if (self.MoonShaftsEffect == UniStormSystem.EnableFeature.Enabled)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.PropertyField(MoonLightShaftIntensityProp,
                            new GUIContent("Moon Shafts Intensity", "Controls the light shafts for UniStorm's moon for each time of day. X represents the hour and Y represents the intensity."));
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(MoonLightShaftsColorProp,
                            new GUIContent("Moon Shafts Color", "A gadient that controls the color of the light shafts for UniStorm's moon. Each element is a transition into the next time of day."));
                        EditorGUILayout.Space();
                        CustomFloatSlider(new Rect(), new GUIContent(), MoonLightShaftsBlurSizeProp,
                            new GUIContent("Moon Shafts Blur Size", "Controls the size of the light shafts' blurring for UniStorm's moon."), 0.1f, 10);
                        EditorGUILayout.Space();
                        CustomIntSlider(new Rect(), new GUIContent(), MoonLightShaftsBlurIterationsProp,
                            new GUIContent("Moon Shafts Blur Iterations", "Controls the amount of iterations used for the blurring of the moon shafts for UniStorm's moon."), 1, 3);

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonColorProp,
                        new GUIContent("Moonlight Color", "A gradient that controls UniStorm's moon light color."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonlightColorProp,
                        new GUIContent("Moonlight Attenuation Color", "The color of the light that hits the clouds at night."));

                    EditorGUILayout.Space();
                    CustomIntSlider(new Rect(), new GUIContent(), MoonAngleProp,
                            new GUIContent("Moon Tilt Angle", "Controls the tilt angle of the Moon."), -45, 45);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(MoonIntensityCurveProp,
                        new GUIContent("Moon Intensity Curve", "Controls the intensity of UniStorm's moon. X represents the hour and Y represents the intensity."));
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(MoonSizeProp,
                        new GUIContent("Moon Size Curve", "Controls the size of UniStorm's moon. X represents the hour and Y represents the size."));
                    EditorGUILayout.Space();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.Box(new GUIContent("What's This?", "A list of moon phase textures that UniStorm will use when creating UniStorm's moon. " +
                        "Each texture applied to the list will be used as a moon phase and be applied in order of the current moon phase. Each moon phase " +
                        "has an individual light intensity to allow each moon phase to give off different amounts of light."),
                       EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    MoonPhaseList.DoLayoutList();
                    EditorGUILayout.Space();

                    GUIStyle TitleStyle = new GUIStyle();
                    TitleStyle.fontStyle = FontStyle.Bold;
                    TitleStyle.fontSize = 14;
                    TitleStyle.alignment = TextAnchor.MiddleCenter;


                    if (MoonPhaseIndexProp.intValue == self.MoonPhaseList.Count && self.MoonPhaseList.Count > 0)
                    {
                        MoonPhaseIndexProp.intValue = self.MoonPhaseList.Count - 1;
                    }

                    if (self.MoonPhaseList.Count > 0)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Current Moon Phase", "Displays all moon phases by adjusting the slider. " +
                        "The Current Moon Phase also controls the moon phase UniStorm will start with."), TitleStyle);
                        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                        EditorGUILayout.BeginVertical("Box");
                        GUI.backgroundColor = Color.white;
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField(new GUIContent(self.MoonPhaseList[MoonPhaseIndexProp.intValue].MoonPhaseTexture), style, GUILayout.Height(50));
                        GUILayout.Space(5);
                        EditorGUILayout.EndVertical();
                    }

                    if (self.MoonPhaseList.Count - 1 != 0 && self.MoonPhaseList.Count > 0)
                    {
                        EditorGUILayout.Space();
                        CustomIntSliderNoTooltipTest(new Rect(), new GUIContent(), MoonPhaseIndexProp, "", 0, self.MoonPhaseList.Count - 1);
                    }

                    if (self.MoonPhaseList.Count == 0)
                    {
                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                        EditorGUILayout.HelpBox("There are currently no moon phases. To creat one, " +
                            "press the + sign on the list above and assign a texture to the newly created slot.", MessageType.Warning, true);
                        GUI.backgroundColor = Color.white;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(MoonPhaseColorProp,
                        new GUIContent("Moon Phase Color", "A gradient that controls the color of UniStorm's moon phases."));

                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), MoonBrightnessProp,
                        new GUIContent("Moon Phase Brightness", "Controls the brightness of all moon phase textures."), 0.25f, 1.0f);

                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                //Atmosphere Settings
                if (!AtmosphereFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                AtmosphereFoldoutProp.boolValue = Foldout(AtmosphereFoldoutProp.boolValue, "Atmosphere Settings", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (AtmosphereFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(HemisphereProp,
                        new GUIContent("Hemisphere", "Controls whether UniStorm's seasons are calculated in either the Northern or Southern Hemisphere."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AmbientIntensityProp,
                        new GUIContent("Ambient Intensity", "Controls the Ambient Intensity for each time of day. X represents the hour and Y represents the intensity."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(EnvironmentReflectionsProp,
                        new GUIContent("Environment Reflections", "Controls the Environment Reflections for each time of day. X represents the hour and Y represents the intensity."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AtmosphereThicknessProp,
                        new GUIContent("Atmosphere Thickness", "Controls the thickness of UniStorm's atmosphere for each time of day. This is mainly used for sunrises and sunsets. " +
                        "The higher the value, the darker and more red the sunrises and sunsets will be."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AmbientSkyLightColorProp,
                        new GUIContent("Ambient Sky Color", "A gradient that controls the Ambient Sky Color during non-precipitation weather types. Each element is a transition into the next time of day."));
                    EditorGUILayout.PropertyField(StormyAmbientSkyLightColorProp,
                        new GUIContent("Stormy Ambient Sky Color", "A gradient that controls the Ambient Sky Color during precipitation weather types. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AmbientEquatorLightColorProp,
                        new GUIContent("Ambient Equator Color", "A gradient that controls the Ambient Equator Color during non-precipitation weather types. Each element is a transition into the next time of day."));
                    EditorGUILayout.PropertyField(StormyAmbientEquatorLightColorProp,
                        new GUIContent("Stormy Ambient Equator Color", "A gradient that controls the Ambient Equator Color during precipitation weather types. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AmbientGroundLightColorProp,
                        new GUIContent("Ambient Ground Color", "A gradient that controls the Ambient Ground Color during non-precipitation weather types. Each element is a transition into the next time of day."));
                    EditorGUILayout.PropertyField(StormyAmbientGroundLightColorProp,
                        new GUIContent("Stormy Ambient Ground Color", "A gradient that controls the Ambient Ground Color during precipitation weather types. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SkyColorProp,
                        new GUIContent("Sky Color", "A gradient that controls the overall sky color of UniStorm's Skybox. " +
                        "Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(SkyTintColorProp,
                        new GUIContent("Sky Tint Color", "A gradient that controls the tint color of UniStorm's Skybox. " +
                        "Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(StarmapTypeProp,
                        new GUIContent("Constellation Type", "Controls the strength of the stars' constellations."));

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(StarLightColorProp,
                        new GUIContent("Starlight Color", "A gradient that controls the color and transparency of UniStorm's stars. Each element is a transition into the next time of day."));

                    EditorGUILayout.Space();
                    CustomFloatSlider(new Rect(), new GUIContent(), StarSpeedProp,
                        new GUIContent("Star Speed", "Controls how fast the stars will move in the sky at night."), 0, 3.0f);

                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
            }


            //Settings
            if (TabNumberProp.intValue == 4)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(80 * Screen.width / Screen.dpi));

                if (GUILayout.Button(new GUIContent(HelpIcon), HelpStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22.5f)))
                {
                    Application.OpenURL("https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/The-UniStorm-Editor#settings-tab");
                }

                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(new GUIContent(SettingsIcon), style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUILayout.LabelField("Settings", style, GUILayout.ExpandWidth(true));

                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1);
                EditorGUILayout.LabelField("The Settings allow you to control various system related settings. The Platform Controller allows the system to automatically apply optimal " +
                    "settings for either the Mobile or Desktop platforms making. The UniStorm Profile Manager is used to import or export UniStorm profiles to overwrite or " +
                    "export your current settings.", EditorStyles.helpBox);

                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you need help, you can hover over each field with your mouse for a tooltip description. " +
                    "Documentation for this section can be found by pressing the ? in the upper right hand corner.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                //Platform
                if (!PlatformFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                PlatformFoldoutProp.boolValue = Foldout(PlatformFoldoutProp.boolValue, "Platform Controller", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (PlatformFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.Space();
                    GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                    EditorGUILayout.LabelField("This setting allows UniStorm to automatically set the optimal settings for the Mobile or Desktop platforms. " +
                        "However, be aware that this will overwrite the quality settings you have set to related to the clouds, as well as some other settings within the UniStorm Editor (excluding color settings and weather types).", EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Note: You are free to modify these values after they have been applied.", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.PropertyField(PlatformTypeProp,
                        new GUIContent("Platform Type"));
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Apply Platform Settings"))
                    {
                        if (EditorUtility.DisplayDialog("Platform Controller", "Are you sure you want to apply the selected platform? This process cannot be undone.", "Yes", "Cancel"))
                        {
                            if (self.PlatformType == UniStormSystem.PlatformTypeEnum.Desktop)
                            {
                                self.LightningStrikes = UniStormSystem.EnableFeature.Enabled;
                                self.SunShadowType = LightShadows.Soft;
                                self.SunShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                                self.MoonShadowType = LightShadows.Soft;
                                self.MoonShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                                self.LightningShadowType = LightShadows.Soft;
                                self.LightningShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                                self.CloudQuality = UniStormSystem.CloudQualityEnum.High;
                                self.CloudType = UniStormSystem.CloudTypeEnum.Volumetric;
                                self.SunShaftsEffect = UniStormSystem.EnableFeature.Enabled;
                                self.MoonShaftsEffect = UniStormSystem.EnableFeature.Enabled;
                                self.CloudShadows = UniStormSystem.EnableFeature.Enabled;
                                self.CloudShadowResolution = UniStormSystem.CloudShadowResolutionEnum._512x512;
                                self.UseTimeOfDayUpdateControl = UniStormSystem.UseTimeOfDayUpdateSeconds.No;
                                self.CloudDomeTrisCountX = 48;
                                self.CloudDomeTrisCountY = 32;
                            }
                            else if (self.PlatformType == UniStormSystem.PlatformTypeEnum.VR)
                            {
                                self.LightningStrikes = UniStormSystem.EnableFeature.Disabled;
                                self.SunShadowType = LightShadows.Soft;
                                self.SunShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                                self.MoonShadowType = LightShadows.Soft;
                                self.MoonShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                                self.LightningShadowType = LightShadows.None;
                                self.LightningShadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
                                self.CloudQuality = UniStormSystem.CloudQualityEnum.Medium;
                                self.CloudType = UniStormSystem.CloudTypeEnum.Volumetric;
                                self.SunShaftsEffect = UniStormSystem.EnableFeature.Disabled;
                                self.MoonShaftsEffect = UniStormSystem.EnableFeature.Disabled;
                                self.CloudShadows = UniStormSystem.EnableFeature.Disabled;
                                self.CloudShadowResolution = UniStormSystem.CloudShadowResolutionEnum._256x256;
                                self.UseTimeOfDayUpdateControl = UniStormSystem.UseTimeOfDayUpdateSeconds.No;
                                self.CloudDomeTrisCountX = 48;
                                self.CloudDomeTrisCountY = 32;
                            }
                            else if (self.PlatformType == UniStormSystem.PlatformTypeEnum.Mobile)
                            {
                                self.LightningStrikes = UniStormSystem.EnableFeature.Disabled;
                                self.SunShadowType = LightShadows.None;
                                self.MoonShadowType = LightShadows.None;
                                self.LightningShadowType = LightShadows.None;
                                self.CloudQuality = UniStormSystem.CloudQualityEnum.Low;
                                self.CloudType = UniStormSystem.CloudTypeEnum._2D;
                                self.SunShaftsEffect = UniStormSystem.EnableFeature.Disabled;
                                self.MoonShaftsEffect = UniStormSystem.EnableFeature.Disabled;
                                self.CloudShadows = UniStormSystem.EnableFeature.Disabled;
                                self.CloudShadowResolution = UniStormSystem.CloudShadowResolutionEnum._256x256;
                                self.UseTimeOfDayUpdateControl = UniStormSystem.UseTimeOfDayUpdateSeconds.No;
                                self.CloudDomeTrisCountX = 44;
                                self.CloudDomeTrisCountY = 16;
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

                //Settings
                if (!SettingsFoldoutProp.boolValue)
                {
                    GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                }

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();

                SettingsFoldoutProp.boolValue = Foldout(SettingsFoldoutProp.boolValue, "Profile Management", true, myFoldoutStyle);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (SettingsFoldoutProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10);

                    CustomObjectProfileField(new Rect(), new GUIContent(), UniStormProfileProp, new GUIContent("UniStorm Profile", "Allows the importing of UniStorm settings from a profile object."), typeof(UniStormProfile), true);

                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);
                    if (GUILayout.Button("Import Profile"))
                    {
                        if (self.m_UniStormProfile != null)
                        {
                            if (EditorUtility.DisplayDialog("UniStorm Profile Importer", "Are you sure you'd like to import settings from the UniStorm Profile " + "''" +
                                self.m_UniStormProfile.name + "''" + "? This process cannot be undone.", "Yes", "Cancel"))
                            {

                                UniStormProfile Temp_UniStormProfile = self.m_UniStormProfile;

                                self.SunColor = UpdateGradient(Temp_UniStormProfile.SunColor, self.SunColor);
                                SunColorProp = serializedObject.FindProperty("SunColor").Copy();

                                self.StormySunColor = UpdateGradient(Temp_UniStormProfile.StormySunColor, self.StormySunColor);
                                StormySunColorProp = serializedObject.FindProperty("StormySunColor").Copy();

                                self.MoonColor = UpdateGradient(Temp_UniStormProfile.MoonColor, self.MoonColor);
                                MoonColorProp = serializedObject.FindProperty("MoonColor").Copy();

                                self.SkyTintColor = UpdateGradient(Temp_UniStormProfile.SkyTintColor, self.SkyTintColor);
                                SkyTintColorProp = serializedObject.FindProperty("SkyTintColor").Copy();

                                self.SkyColor = UpdateGradient(Temp_UniStormProfile.SkyColor, self.SkyColor);
                                SkyColorProp = serializedObject.FindProperty("SkyColor").Copy();

                                self.AmbientSkyLightColor = UpdateGradient(Temp_UniStormProfile.AmbientSkyLightColor, self.AmbientSkyLightColor);
                                AmbientSkyLightColorProp = serializedObject.FindProperty("AmbientSkyLightColor").Copy();

                                self.StormyAmbientSkyLightColor = UpdateGradient(Temp_UniStormProfile.StormyAmbientSkyLightColor, self.StormyAmbientSkyLightColor);
                                StormyAmbientSkyLightColorProp = serializedObject.FindProperty("StormyAmbientSkyLightColor").Copy();

                                self.AmbientEquatorLightColor = UpdateGradient(Temp_UniStormProfile.AmbientEquatorLightColor, self.AmbientEquatorLightColor);
                                AmbientEquatorLightColorProp = serializedObject.FindProperty("AmbientEquatorLightColor").Copy();

                                self.StormyAmbientEquatorLightColor = UpdateGradient(Temp_UniStormProfile.StormyAmbientEquatorLightColor, self.StormyAmbientEquatorLightColor);
                                StormyAmbientEquatorLightColorProp = serializedObject.FindProperty("StormyAmbientEquatorLightColor").Copy();

                                self.AmbientGroundLightColor = UpdateGradient(Temp_UniStormProfile.AmbientGroundLightColor, self.AmbientGroundLightColor);
                                AmbientGroundLightColorProp = serializedObject.FindProperty("AmbientGroundLightColor").Copy();

                                self.StormyAmbientGroundLightColor = UpdateGradient(Temp_UniStormProfile.StormyAmbientGroundLightColor, self.StormyAmbientGroundLightColor);
                                StormyAmbientGroundLightColorProp = serializedObject.FindProperty("StormyAmbientGroundLightColor").Copy();

                                self.StarLightColor = UpdateGradient(Temp_UniStormProfile.StarLightColor, self.StarLightColor);
                                StarLightColorProp = serializedObject.FindProperty("StarLightColor").Copy();

                                self.FogColor = UpdateGradient(Temp_UniStormProfile.FogColor, self.FogColor);
                                FogColorProp = serializedObject.FindProperty("FogColor").Copy();

                                self.FogStormyColor = UpdateGradient(Temp_UniStormProfile.FogStormyColor, self.FogStormyColor);
                                FogStormyColorProp = serializedObject.FindProperty("FogStormyColor").Copy();

                                self.CloudLightColor = UpdateGradient(Temp_UniStormProfile.CloudLightColor, self.CloudLightColor);
                                CloudLightColorProp = serializedObject.FindProperty("CloudLightColor").Copy();

                                self.StormyCloudLightColor = UpdateGradient(Temp_UniStormProfile.StormyCloudLightColor, self.StormyCloudLightColor);
                                StormyCloudLightColorProp = serializedObject.FindProperty("StormyCloudLightColor").Copy();

                                self.FogLightColor = UpdateGradient(Temp_UniStormProfile.FogLightColor, self.FogLightColor);
                                FogLightColorProp = serializedObject.FindProperty("FogLightColor").Copy();

                                self.StormyFogLightColor = UpdateGradient(Temp_UniStormProfile.StormyFogLightColor, self.StormyFogLightColor);
                                StormyFogLightColorProp = serializedObject.FindProperty("StormyFogLightColor").Copy();

                                self.SunSpotColor = UpdateGradient(Temp_UniStormProfile.SunSpotColor, self.SunSpotColor);
                                SunSpotColorProp = serializedObject.FindProperty("SunSpotColor").Copy();

                                self.CloudBaseColor = UpdateGradient(Temp_UniStormProfile.CloudBaseColor, self.CloudBaseColor);
                                CloudBaseColorProp = serializedObject.FindProperty("CloudBaseColor").Copy();

                                self.CloudStormyBaseColor = UpdateGradient(Temp_UniStormProfile.CloudStormyBaseColor, self.CloudStormyBaseColor);
                                CloudStormyBaseColorProp = serializedObject.FindProperty("CloudStormyBaseColor").Copy();

                                self.SunIntensityCurve = new AnimationCurve(Temp_UniStormProfile.SunIntensityCurve.keys);
                                SunIntensityCurveProp = serializedObject.FindProperty("SunIntensityCurve").Copy();

                                self.MoonIntensityCurve = new AnimationCurve(Temp_UniStormProfile.MoonIntensityCurve.keys);
                                MoonIntensityCurveProp = serializedObject.FindProperty("MoonIntensityCurve").Copy();

                                self.AtmosphereThickness = new AnimationCurve(Temp_UniStormProfile.AtmosphereThickness.keys);
                                AtmosphereThicknessProp = serializedObject.FindProperty("AtmosphereThickness").Copy();

                                self.SunAttenuationCurve = new AnimationCurve(Temp_UniStormProfile.SunAttenuationCurve.keys);
                                SunAttenuationCurveProp = serializedObject.FindProperty("SunAttenuationCurve").Copy();

                                self.EnvironmentReflections = new AnimationCurve(Temp_UniStormProfile.EnvironmentReflections.keys);
                                EnvironmentReflectionsProp = serializedObject.FindProperty("EnvironmentReflections").Copy();

                                self.AmbientIntensityCurve = new AnimationCurve(Temp_UniStormProfile.AmbientIntensityCurve.keys);
                                AmbientIntensityProp = serializedObject.FindProperty("AmbientIntensityCurve").Copy();

                                self.SunAtmosphericFogIntensity = new AnimationCurve(Temp_UniStormProfile.SunAtmosphericFogIntensity.keys);

                                self.SunControlCurve = new AnimationCurve(Temp_UniStormProfile.SunControlCurve.keys);

                                self.MoonAtmosphericFogIntensity = new AnimationCurve(Temp_UniStormProfile.MoonAtmosphericFogIntensity.keys);

                                self.MoonObjectFade = new AnimationCurve(Temp_UniStormProfile.MoonObjectFade.keys);

                                self.FogType = (UniStormSystem.FogTypeEnum)Temp_UniStormProfile.FogType;
                                FogTypeProp = serializedObject.FindProperty("FogType").Copy();

                                self.FogMode = (UniStormSystem.FogModeEnum)Temp_UniStormProfile.FogMode;
                                FogModeProp = serializedObject.FindProperty("FogMode").Copy();

                                serializedObject.ApplyModifiedProperties();
                                Importing = true;
                                startVal = (float)EditorApplication.timeSinceStartup;
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("UniStorm Profile Importer - Error", "There was no UniStorm Profile found in the UniStorm Profile slot. Please apply one and try again.", "Okay"))
                            {

                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Export Profile"))
                    {
                        self.FilePath = EditorUtility.SaveFilePanelInProject("Save as UniStorm Profile", "New UniStorm Profile", "asset", "Please enter a file name to save the file to");

                        if (self.FilePath != string.Empty)
                        {
                            UniStormProfile Temp_UniStormProfile = ScriptableObject.CreateInstance<UniStormProfile>();
                            UniStormSystem Temp_UniStormSystem = self;

                            Temp_UniStormProfile.SkyTintColor = UpdateGradient(self.SkyTintColor, Temp_UniStormProfile.SkyTintColor);
                            Temp_UniStormProfile.SkyColor = UpdateGradient(self.SkyColor, Temp_UniStormProfile.SkyColor);
                            Temp_UniStormProfile.SunColor = UpdateGradient(self.SunColor, Temp_UniStormProfile.SunColor);
                            Temp_UniStormProfile.StormySunColor = UpdateGradient(self.StormySunColor, Temp_UniStormProfile.StormySunColor);
                            Temp_UniStormProfile.MoonColor = UpdateGradient(self.MoonColor, Temp_UniStormProfile.MoonColor);
                            Temp_UniStormProfile.AmbientSkyLightColor = UpdateGradient(self.AmbientSkyLightColor, Temp_UniStormProfile.AmbientSkyLightColor);
                            Temp_UniStormProfile.StormyAmbientSkyLightColor = UpdateGradient(self.StormyAmbientSkyLightColor, Temp_UniStormProfile.StormyAmbientSkyLightColor);
                            Temp_UniStormProfile.AmbientEquatorLightColor = UpdateGradient(self.AmbientEquatorLightColor, Temp_UniStormProfile.AmbientEquatorLightColor);
                            Temp_UniStormProfile.StormyAmbientEquatorLightColor = UpdateGradient(self.StormyAmbientEquatorLightColor, Temp_UniStormProfile.StormyAmbientEquatorLightColor);
                            Temp_UniStormProfile.AmbientGroundLightColor = UpdateGradient(self.AmbientGroundLightColor, Temp_UniStormProfile.AmbientGroundLightColor);
                            Temp_UniStormProfile.StormyAmbientGroundLightColor = UpdateGradient(self.StormyAmbientGroundLightColor, Temp_UniStormProfile.StormyAmbientGroundLightColor);
                            Temp_UniStormProfile.StarLightColor = UpdateGradient(self.StarLightColor, Temp_UniStormProfile.StarLightColor);
                            Temp_UniStormProfile.FogColor = UpdateGradient(self.FogColor, Temp_UniStormProfile.FogColor);
                            Temp_UniStormProfile.FogStormyColor = UpdateGradient(self.FogStormyColor, Temp_UniStormProfile.FogStormyColor);
                            Temp_UniStormProfile.CloudLightColor = UpdateGradient(self.CloudLightColor, Temp_UniStormProfile.CloudLightColor);
                            Temp_UniStormProfile.StormyCloudLightColor = UpdateGradient(self.StormyCloudLightColor, Temp_UniStormProfile.StormyCloudLightColor); 
                            Temp_UniStormProfile.FogLightColor = UpdateGradient(self.FogLightColor, Temp_UniStormProfile.FogLightColor); 
                            Temp_UniStormProfile.StormyFogLightColor = UpdateGradient(self.StormyFogLightColor, Temp_UniStormProfile.StormyFogLightColor); 
                            Temp_UniStormProfile.SunSpotColor = UpdateGradient(self.SunSpotColor, Temp_UniStormProfile.SunSpotColor); 
                            Temp_UniStormProfile.CloudBaseColor = UpdateGradient(self.CloudBaseColor, Temp_UniStormProfile.CloudBaseColor);
                            Temp_UniStormProfile.CloudStormyBaseColor = UpdateGradient(self.CloudStormyBaseColor, Temp_UniStormProfile.CloudStormyBaseColor);
                            Temp_UniStormProfile.SunIntensityCurve = new AnimationCurve(Temp_UniStormSystem.SunIntensityCurve.keys);
                            Temp_UniStormProfile.MoonIntensityCurve = new AnimationCurve(Temp_UniStormSystem.MoonIntensityCurve.keys);
                            Temp_UniStormProfile.AtmosphereThickness = new AnimationCurve(Temp_UniStormSystem.AtmosphereThickness.keys);
                            Temp_UniStormProfile.SunAttenuationCurve = new AnimationCurve(Temp_UniStormSystem.SunAttenuationCurve.keys);
                            Temp_UniStormProfile.EnvironmentReflections = new AnimationCurve(Temp_UniStormSystem.EnvironmentReflections.keys);
                            Temp_UniStormProfile.AmbientIntensityCurve = new AnimationCurve(Temp_UniStormSystem.AmbientIntensityCurve.keys);
                            Temp_UniStormProfile.SunAtmosphericFogIntensity = new AnimationCurve(Temp_UniStormSystem.SunAtmosphericFogIntensity.keys);
                            Temp_UniStormProfile.SunControlCurve = new AnimationCurve(Temp_UniStormSystem.SunControlCurve.keys);
                            Temp_UniStormProfile.MoonAtmosphericFogIntensity = new AnimationCurve(Temp_UniStormSystem.MoonAtmosphericFogIntensity.keys);
                            Temp_UniStormProfile.MoonObjectFade = new AnimationCurve(Temp_UniStormSystem.MoonObjectFade.keys);
                            Temp_UniStormProfile.FogType = (UniStormProfile.FogTypeEnum)Temp_UniStormSystem.FogType;
                            Temp_UniStormProfile.FogMode = (UniStormProfile.FogModeEnum)Temp_UniStormSystem.FogMode;

                            AssetDatabase.CreateAsset(Temp_UniStormProfile, self.FilePath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(URPForwardRenderer);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
#endif

            serializedObject.ApplyModifiedProperties();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (progress < secs && Importing)
                {
                    EditorUtility.DisplayProgressBar("UniStorm Profile Importer", "Importing UniStorm Profile settings...", progress / secs);

                    if (Importing && progress / secs > 0.99f)
                    {
                        if (EditorUtility.DisplayDialog("UniStorm Profile Importer", "The UniStorm Profile settings have been successfully imported.", "Okay"))
                        {
                            Importing = false;
                            EditorUtility.ClearProgressBar();
                            Selection.activeObject = null;
                        }
                        else
                        {
                            Importing = false;
                            EditorUtility.ClearProgressBar();
                            Selection.activeObject = null;
                        }
                    }
                }
                else if (Importing && progress >= secs)
                {
                    EditorUtility.ClearProgressBar();
                }

                progress = (float)(EditorApplication.timeSinceStartup - startVal);
            }
#endif
        }

        Gradient UpdateGradient(Gradient Reference, Gradient GradientToUpdate)
        {
            GradientToUpdate = new Gradient();
            GradientColorKey[] m_ColorKey;
            GradientAlphaKey[] m_AlphaKey;
            m_ColorKey = new GradientColorKey[Reference.colorKeys.Length];
            m_ColorKey = Reference.colorKeys;
            m_AlphaKey = new GradientAlphaKey[Reference.alphaKeys.Length];
            m_AlphaKey = Reference.alphaKeys;
            GradientToUpdate.SetKeys(m_ColorKey, m_AlphaKey);
            return GradientToUpdate;
        }

        //Custom functions for handling serialized properties.
        void CustomIntSlider(Rect position, GUIContent label, SerializedProperty property, GUIContent NameAndTip, int MinValue, int MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntSlider(NameAndTip, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        void CustomIntSliderNoTooltip(Rect position, GUIContent label, SerializedProperty property, string Name, int MinValue, int MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntSlider(Name, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        void CustomIntSliderNoTooltipTest(Rect position, GUIContent label, SerializedProperty property, string Name, int MinValue, int MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntSlider(Name, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        void CustomFloatSlider(Rect position, GUIContent label, SerializedProperty property, GUIContent NameAndTip, float MinValue, float MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Slider(NameAndTip, property.floatValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newValue;

            EditorGUI.EndProperty();
        }

        public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style)
        {
            Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, style);
            return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
        }

        public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style)
        {
            return Foldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
        }

        void CustomObjectField(Rect position, GUIContent label, SerializedProperty property, GUIContent NameAndTip, Type typeOfObject, bool IsEssential)
        {
            if (IsEssential && property.objectReferenceValue == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("This field cannot be left blank", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.ObjectField(NameAndTip, property.objectReferenceValue, typeOfObject, true);

            if (EditorGUI.EndChangeCheck())
                property.objectReferenceValue = newValue;

            EditorGUI.EndProperty();
        }

        void CustomObjectProfileField(Rect position, GUIContent label, SerializedProperty property, GUIContent NameAndTip, Type typeOfObject, bool IsEssential)
        {
            if (IsEssential && property.objectReferenceValue == null)
            {
                GUI.backgroundColor = new Color(1f, 1, 0.5f, 0.5f);
                EditorGUILayout.LabelField("If you are importing a UniStorm Profile, this field cannot be left unassigned.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.ObjectField(NameAndTip, property.objectReferenceValue, typeOfObject, true);

            if (EditorGUI.EndChangeCheck())
                property.objectReferenceValue = newValue;

            EditorGUI.EndProperty();
        }

        private LayerMask FieldToLayerMask(int field)
        {
            LayerMask mask = 0;
            var layers = InternalEditorUtility.layers;
            for (int c = 0; c < layers.Length; c++)
            {
                if ((field & (1 << c)) != 0)
                {
                    mask |= 1 << LayerMask.NameToLayer(layers[c]);
                }
            }
            return mask;
        }

        private int LayerMaskToField(LayerMask mask)
        {
            int field = 0;
            var layers = InternalEditorUtility.layers;
            for (int c = 0; c < layers.Length; c++)
            {
                if ((mask & (1 << LayerMask.NameToLayer(layers[c]))) != 0)
                {
                    field |= 1 << c;
                }
            }
            return field;
        }

        void CustomTagField(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.TagField(label, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = newValue;

            EditorGUI.EndProperty();
        }
    }
}