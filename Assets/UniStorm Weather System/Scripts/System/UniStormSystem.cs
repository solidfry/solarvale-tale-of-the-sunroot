using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UniStorm.Effects;
using UniStorm.Utility;
using System.Linq;

namespace UniStorm
{
    public class UniStormSystem : MonoBehaviour
    {
        public UniversalRendererData urpForwardRendererData;

        public static UniStormSystem Instance = null;

        public CustomizeQualityEnum CustomizeQuality = CustomizeQualityEnum.No;
        public enum CustomizeQualityEnum
        {
            No, Yes
        }
        public int ConvergenceSpeed = 75;
        public int NearMarchSteps = 100;
        public int DistantMarchSteps = 10;
        public EnableFeature UpdateMarchStepsDuringRuntime = EnableFeature.Disabled;
        public bool ColorSpaceSuggestionDismissed = false;
        public int RendersPerFrame = 1;
        UniStormClouds m_UniStormClouds;

        //Events
        public UnityEvent OnHourChangeEvent;
        public UnityEvent OnDayChangeEvent;
        public UnityEvent OnMonthChangeEvent;
        public UnityEvent OnYearChangeEvent;
        public UnityEvent OnWeatherChangeEvent;
        public UnityEvent OnWeatherGenerationEvent;
        public UnityEvent OnLightningStrikePlayerEvent;
        public UnityEvent OnLightningStrikeObjectEvent;

        //Audio Mixer Volumes
        public float WeatherSoundsVolume = 1;
        public float AmbienceVolume = 1;
        public float MusicVolume = 1;

        //UI
        public Slider TimeSlider;
        public GameObject WeatherButtonGameObject;
        public GameObject TimeSliderGameObject;
        public Dropdown WeatherDropdown;
        public EnableFeature UseUniStormMenu = EnableFeature.Enabled;
        public KeyCode UniStormMenuKey = KeyCode.Escape;
        public GameObject UniStormCanvas;
        public bool m_MenuToggle = true;

        //Editor
        public int TabNumber = 0;
        public int TimeTabNumbers = 0;
        public int WeatherTabNumbers = 0;
        public int CelestialTabNumbers = 0;
        public bool TimeFoldout = true, DateFoldout = true, TimeSoundsFoldout = true, TimeMusicFoldout = true,
            SunFoldout = true, MoonFoldout = true, AtmosphereFoldout = true, FogFoldout = true,
            WeatherFoldout = true, LightningFoldout = true, CameraFoldout = true, SettingsFoldout = true, CloudsFoldout = true, PlatformFoldout = true;
        public UniStormProfile m_UniStormProfile;
        public string FilePath = "";
        public UniStormProfileTypeEnum UniStormProfileType;
        public enum UniStormProfileTypeEnum
        {
            Import,
            Export
        }
        public PlatformTypeEnum PlatformType = PlatformTypeEnum.Desktop;
        public enum PlatformTypeEnum
        {
            Desktop,
            VR,
            Mobile
        }

        //Camera & Player
        public Transform PlayerTransform;
        public Camera PlayerCamera;
        public bool m_PlayerFound = false;
        public EnableFeature GetPlayerAtRuntime = EnableFeature.Disabled;
        public EnableFeature UseRuntimeDelay = EnableFeature.Disabled;
        public GetPlayerMethodEnum GetPlayerMethod = GetPlayerMethodEnum.ByTag;
        public enum GetPlayerMethodEnum { ByTag, ByName };
        public string PlayerTag = "Player";
        public string PlayerName = "Player";
        public string CameraTag = "MainCamera";
        public string CameraName = "MainCamera";

        //Time
        public System.DateTime UniStormDate;
        public int StartingMinute = 0;
        public int StartingHour = 0;
        public int Minute = 1;
        public int Hour = 0;
        public int Day = 0;
        public int Month = 0;
        public int Year = 0;
        public int DayLength = 10;
        public int NightLength = 10;
        public int TimeOfDayUpdateSeconds = 0;
        public enum UseTimeOfDayUpdateSeconds { Yes, No };
        public UseTimeOfDayUpdateSeconds UseTimeOfDayUpdateControl = UseTimeOfDayUpdateSeconds.No;
        float TimeOfDayUpdateTimer;
        public float m_TimeFloat;
        public EnableFeature TimeFlow = EnableFeature.Enabled;
        public EnableFeature RealWorldTime = EnableFeature.Disabled;
        float m_roundingCorrection;
        float m_PreciseCurveTime;
        public bool m_HourUpdate = false;
        float m_TimeOfDaySoundsTimer = 0;
        int m_TimeOfDaySoundsSeconds = 10;
        public int TimeOfDaySoundsSecondsMin = 10;
        public int TimeOfDaySoundsSecondsMax = 30;
        public List<AudioClip> MorningSounds = new List<AudioClip>();
        public List<AudioClip> DaySounds = new List<AudioClip>();
        public List<AudioClip> EveningSounds = new List<AudioClip>();
        public List<AudioClip> NightSounds = new List<AudioClip>();
        public AudioSource TimeOfDayAudioSource;
        public List<AudioClip> MorningMusic = new List<AudioClip>();
        public List<AudioClip> DayMusic = new List<AudioClip>();
        public List<AudioClip> EveningMusic = new List<AudioClip>();
        public List<AudioClip> NightMusic = new List<AudioClip>();
        public AudioSource TimeOfDayMusicAudioSource;
        public int TimeOfDayMusicDelay = 1;
        float m_CurrentMusicClipLength = 0;
        float m_TimeOfDayMusicTimer = 0;
        public EnableFeature TimeOfDaySoundsDuringPrecipitationWeather = EnableFeature.Disabled;
        public EnableFeature TransitionMusicOnTimeOfDayChange = EnableFeature.Disabled;
        float m_CurrentClipLength = 0;
        public bool m_UpdateTimeOfDayMusic = false;
        public bool m_UpdateBiomeTimeOfDayMusic = false;
        public int MusicTransitionLength = 3;
        int m_LastHour;

        public CurrentTimeOfDayEnum CurrentTimeOfDay;
        public enum CurrentTimeOfDayEnum
        {
            Morning = 0, Day, Evening, Night
        }

        public WeatherGenerationMethodEnum WeatherGenerationMethod = WeatherGenerationMethodEnum.Daily;
        public List<WeatherType> WeatherForecast = new List<WeatherType>();
        public enum WeatherGenerationMethodEnum
        {
            Hourly = 0, Daily = 1
        }

        //General Enums
        public enum EnableFeature
        {
            Enabled = 0, Disabled = 1
        }

        //Weather
        public EnableFeature ForceLowClouds = EnableFeature.Disabled;
        public int LowCloudHeight = 225;
        public int CloudDomeTrisCountX = 48;
        public int CloudDomeTrisCountY = 32;
        public bool IgnoreConditions = false;
        public AnimationCurve CloudyFadeControl = AnimationCurve.Linear(0, 0.22f, 24, 0);
        public AnimationCurve PrecipitationGraph = AnimationCurve.Linear(1, 0, 13, 100);
        public List<WeatherType> NonPrecipiationWeatherTypes = new List<WeatherType>();
        public List<WeatherType> PrecipiationWeatherTypes = new List<WeatherType>();
        public List<WeatherType> AllWeatherTypes = new List<WeatherType>();
        public WeatherType CurrentWeatherType;
        public WeatherType NextWeatherType;
        public bool ByPassCoverageTransition = false;
        public int m_PrecipitationOdds = 50;
        float m_CurrentPrecipitationAmountFloat = 1;
        int m_CurrentPrecipitationAmountInt = 1;
        public static bool m_IsFading;
        public int TransitionSpeed = 5;
        public int HourToChangeWeather;
        float m_CloudFadeLevelStart = -0.05f;
        float m_CloudFadeLevelEnd = 0.22f;
        int m_GeneratedOdds;
        bool m_WeatherGenerated = false;
        Coroutine CloudCoroutine, FogCoroutine, WeatherEffectCoroutine, AdditionalWeatherEffectCoroutine, ParticleFadeCoroutine, StormyCloudsCoroutine, CloudTallnessCoroutine, AuroraCoroutine, FogLightFalloffCoroutine;
        Coroutine AdditionalParticleFadeCoroutine, SunCoroutine, MoonCoroutine, WindCoroutine, SoundInCoroutine, SoundOutCoroutine, MostlyCloudyCoroutine, SunAttenuationIntensityCoroutine, AtmosphericFogCoroutine;
        Coroutine ColorCoroutine, CloudHeightCoroutine, RainShaderCoroutine, SnowShaderCoroutine, SunColorCoroutine, CloudProfileCoroutine, CloudShadowIntensityCoroutine, MusicVolumeCoroutine, SunHeightCoroutine;
        public WindZone UniStormWindZone;
        public GameObject m_SoundTransform;
        public GameObject m_EffectsTransform;
        public Light m_LightningLight;
        public LightningSystem m_UniStormLightningSystem;
        public LightningStrike m_LightningStrikeSystem;
        public int LightningSecondsMin = 5;
        public int LightningSecondsMax = 10;
        public Color LightningColor = new Color(0.725f,0.698f,0.713f, 1);
        public Color LightningLightColor = new Color(195f / 255f, 213f / 255f, 226f / 255f, 1);
        int m_LightningSeconds;
        float m_LightningTimer;
        public List<AnimationCurve> LightningFlashPatterns = new List<AnimationCurve>();
        public List<AudioClip> ThunderSounds = new List<AudioClip>();
        public int LightningGroundStrikeOdds = 50;
        public GameObject LightningStrikeEffect;
        public GameObject LightningStrikeFire;
        public EnableFeature WeatherGeneration = EnableFeature.Enabled;
        public EnableFeature LightningStrikes = EnableFeature.Enabled;
        public EnableFeature CloudShadows = EnableFeature.Enabled;
        public EnableFeature LightningStrikesEmeraldAI = EnableFeature.Disabled;
        public string EmeraldAITag = "Respawn";
        public int EmeraldAIRagdollForce = 500;
        public int EmeraldAILightningDamage = 500;
        public UniStormCloudShadowsFeature.Settings m_CloudShadows;
        public float m_CurrentCloudHeight;
        public CloudShadowResolutionEnum CloudShadowResolution = CloudShadowResolutionEnum._256x256;
        public enum CloudShadowResolutionEnum { _256x256, _512x512, _1024x1024 }
        public int CloudSpeed = 8;
        public int CloudTurbulence = 8;
        public LayerMask DetectionLayerMask;
        public List<string> LightningFireTags = new List<string>();
        public float LightningLightIntensityMin = 1;
        public float LightningLightIntensityMax = 3;
        public float CurrentFogAmount;
        public int LightningGenerationDistance = 100;
        public int LightningDetectionDistance = 20;
        public int m_CloudSeed;
        public Color CurrentFogColor;
        public enum FogTypeEnum { UnistormFog, UnityFog };
        public FogTypeEnum FogType = FogTypeEnum.UnistormFog;
        public enum FogModeEnum { Exponential, ExponentialSquared };
        public FogModeEnum FogMode = FogModeEnum.Exponential;
        public UniStormAtmosphericFogFeature.Settings m_UniStormAtmosphericFog;
        public EnableFeature UseDithering = EnableFeature.Enabled;
        public EnableFeature UseRadialDistantFog = EnableFeature.Disabled;
        public float SnowAmount = 0;
        public float CurrentWindIntensity = 0;
        public float MostlyCloudyFadeValue = 0;
        public float StormyHorizonBrightness = 1.4f;
        WeatherType TempWeatherType;
        public AnimationCurve SunAttenuationCurve = AnimationCurve.Linear(0, 1, 24, 3);
        public AnimationCurve AmbientIntensityCurve = AnimationCurve.Linear(0, 0, 24, 1);
        public CurrentSeasonEnum CurrentSeason;
        public enum CurrentSeasonEnum
        {
            Spring = 1, Summer = 2, Fall = 3, Winter = 4
        }
        public CloudTypeEnum CloudType = CloudTypeEnum.Volumetric;
        public enum CloudTypeEnum
        {
            _2D = 0, Volumetric
        }
        public CloudQualityEnum CloudQuality = CloudQualityEnum.High;
        public enum CloudQualityEnum
        {
            Low = 0, Medium, High, Ultra
        }

        //Temperature
        public TemperatureTypeEnum TemperatureType = TemperatureTypeEnum.Fahrenheit;
        public enum TemperatureTypeEnum
        {
            Fahrenheit, Celsius
        }
        public AnimationCurve TemperatureCurve = AnimationCurve.Linear(1, -100, 13, 125);
        public AnimationCurve TemperatureFluctuation = AnimationCurve.Linear(0, -25, 24, 25);
        public int Temperature;
        public GameObject LightningStruckObject;
        public float FogLightFalloff = 9.7f;
        public float CameraFogHeight = 0.85f;
        int m_FreezingTemperature;

        //Celestial
        Renderer m_CloudDomeRenderer;
        Material m_CloudDomeMaterial;
        Material m_SkyBoxMaterial;
        Renderer m_StarsRenderer;
        Material m_StarsMaterial;
        public Light m_SunLight;
        Transform m_CelestialAxisTransform;
        public int SunRevolution = -90;
        public float SunIntensity = 1;
        public float SunAttenuationMultipler = 1;
        public float PrecipitationSunIntensity = 0.25f;
        public AnimationCurve SunIntensityCurve = AnimationCurve.Linear(0, 0, 24, 5);
        public AnimationCurve SunSize = AnimationCurve.Linear(0, 1, 24, 10);
        public AnimationCurve SunAtmosphericFogIntensity = AnimationCurve.Linear(0, 2, 24, 2);
        public AnimationCurve SunControlCurve = AnimationCurve.Linear(0, 1, 24, 1);
        public AnimationCurve MoonAtmosphericFogIntensity = AnimationCurve.Linear(0, 1, 24, 1);
        public AnimationCurve MoonObjectFade = AnimationCurve.Linear(0, 1, 24, 1);
        public float AtmosphericFogMultiplier = 1;
        public Light m_MoonLight;
        public int MoonPhaseIndex = 5;
        public float MoonBrightness = 0.7f;
        public Material m_MoonPhaseMaterial;
        Renderer m_MoonRenderer;
        Transform m_MoonTransform;
        Renderer m_SunRenderer;
        Transform m_SunTransform;
        public float MoonIntensity = 1;
        public float MoonPhaseIntensity = 1;
        public AnimationCurve MoonIntensityCurve = AnimationCurve.Linear(0, 0, 24, 5);
        public AnimationCurve MoonSize = AnimationCurve.Linear(0, 1, 24, 10);
        Vector3 m_MoonStartingSize;
        GameObject m_MoonParent;
        public AnimationCurve AtmosphereThickness = AnimationCurve.Linear(0, 1, 24, 3);
        public AnimationCurve EnvironmentReflections = AnimationCurve.Linear(0, 0, 24, 1);
        public float StarSpeed = 0.75f;
        public int SunAngle = 10;
        public int MoonAngle = -10;
        public EnableFeature SunShaftsEffect = EnableFeature.Enabled;
        public EnableFeature MoonShaftsEffect = EnableFeature.Enabled;
        UniStormSunShaftsFeature.Settings m_SunShafts;
        UniStormSunShaftsFeature.Settings m_MoonShafts;
        public GameObject SunObject;
        public Material SunObjectMaterial;
        public HemisphereEnum Hemisphere = HemisphereEnum.Northern;
        public enum HemisphereEnum
        {
            Northern = 0, Southern
        }
        public LightShadows SunShadowType = LightShadows.Soft;
        public LightShadows MoonShadowType = LightShadows.Soft;
        public LightShadows LightningShadowType = LightShadows.Soft;
        public UnityEngine.Rendering.LightShadowResolution SunShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        public UnityEngine.Rendering.LightShadowResolution MoonShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        public UnityEngine.Rendering.LightShadowResolution LightningShadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        public float SunShadowStrength = 0.75f;
        public float MoonShadowStrength = 0.75f;
        public float LightningShadowStrength = 0.75f;
        [System.Serializable]
        public class MoonPhaseClass
        {
            public Texture MoonPhaseTexture = null;
            public float MoonPhaseIntensity = 1;
        }
        public List<MoonPhaseClass> MoonPhaseList = new List<MoonPhaseClass>();
        public GameObject m_AuroraParent;
        public StarmapTypeEnum StarmapType = StarmapTypeEnum.LightConstellations;
        public enum StarmapTypeEnum { VeryStrongConstellations = 0, StrongConstellations, LightConstellations }
        public CloudRenderTypeEnum CloudRenderType = CloudRenderTypeEnum.Transparent;
        public enum CloudRenderTypeEnum
        {
            Transparent = 0, Opaque
        }

        //Light Shafts
        public AnimationCurve SunLightShaftIntensity = AnimationCurve.Linear(0, 1, 24, 1);
        public Gradient SunLightShaftsColor;
        public float SunLightShaftsBlurSize = 4.86f;
        public int SunLightShaftsBlurIterations = 2;
        public AnimationCurve MoonLightShaftIntensity = AnimationCurve.Linear(0, 1, 24, 1);
        public Gradient MoonLightShaftsColor;
        public float MoonLightShaftsBlurSize = 3f;
        public int MoonLightShaftsBlurIterations = 2;


        //Colors
        public Gradient SunColor;
        public Gradient StormySunColor;
        public Gradient MoonColor;
        public Gradient SkyColor;
        public Gradient AmbientSkyLightColor;
        public Gradient StormyAmbientSkyLightColor;
        public Gradient AmbientEquatorLightColor;
        public Gradient StormyAmbientEquatorLightColor;
        public Gradient AmbientGroundLightColor;
        public Gradient StormyAmbientGroundLightColor;
        public Gradient StarLightColor;
        public Gradient FogColor;
        public Gradient FogStormyColor;
        public Gradient CloudLightColor;
        public Gradient StormyCloudLightColor;
        public Gradient CloudBaseColor;
        public Gradient CloudStormyBaseColor;
        public Gradient SkyTintColor;
        [GradientUsage(true)]
        public Gradient SunSpotColor;
        public Gradient FogLightColor;
        public Gradient StormyFogLightColor;
        public Color MoonPhaseColor = Color.white;
        public Color MoonlightColor;

        //Internal
        float m_FadeValue;
        float m_ReceivedCloudValue;

        public Gradient DefaultCloudBaseColor;
        GradientColorKey[] CloudColorKeySwitcher;

        public Gradient DefaultFogBaseColor;
        GradientColorKey[] FogColorKeySwitcher;

        public Gradient DefaultCloudLightColor;
        GradientColorKey[] CloudLightColorKeySwitcher;

        public Gradient DefaultFogLightColor;
        GradientColorKey[] FogLightColorKeySwitcher;

        public Gradient DefaultAmbientSkyLightBaseColor;
        GradientColorKey[] AmbientSkyLightColorKeySwitcher;

        public Gradient DefaultAmbientEquatorLightBaseColor;
        GradientColorKey[] AmbientEquatorLightColorKeySwitcher;

        public Gradient DefaultAmbientGroundLightBaseColor;
        GradientColorKey[] AmbientGroundLightColorKeySwitcher;

        public Gradient DefaultSunLightBaseColor;
        GradientColorKey[] SunLightColorKeySwitcher;

        public List<ParticleSystem> ParticleSystemList = new List<ParticleSystem>();
        public List<ParticleSystem> WeatherEffectsList = new List<ParticleSystem>();
        public List<ParticleSystem> AdditionalParticleSystemList = new List<ParticleSystem>();
        public List<ParticleSystem> AdditionalWeatherEffectsList = new List<ParticleSystem>();
        public List<AudioSource> WeatherSoundsList = new List<AudioSource>();
        public ParticleSystem CurrentParticleSystem;
        public float m_ParticleAmount = 0;
        public ParticleSystem AdditionalCurrentParticleSystem;       
        public bool UniStormInitialized = false;
        public UnityEngine.Audio.AudioMixer UniStormAudioMixer;
        public bool UpgradedToCurrentVersion = false;
        public VRState VRStateData;
        float m_DetailStrength = 0.072f;

        void Awake()
        {
            GameObject m_UniStormManager = new GameObject();
            m_UniStormManager.transform.SetParent(this.transform);
            m_UniStormManager.AddComponent<UniStormManager>();
            m_UniStormManager.name = "UniStorm Manager";
            Instance = this;
            m_UniStormClouds = FindObjectOfType<UniStormClouds>();
            InitializeCloudSettings();

            //When in the Unity Editor, check the state of VR, along with the StereoRenderingPath, and cache it within VRState so can be used during runtime for VR related features.
            var m_VRStateData = Resources.Load("VR State Data") as VRState;
            #if UNITY_EDITOR
                m_VRStateData.VREnabled = UnityEngine.XR.XRSettings.enabled; //UnityEditor.PlayerSettings.virtualRealitySupported;
            if (UnityEngine.XR.XRSettings.stereoRenderingMode == UnityEngine.XR.XRSettings.StereoRenderingMode.SinglePassInstanced) //(UnityEditor.PlayerSettings.stereoRenderingPath == UnityEditor.StereoRenderingPath.SinglePass)
                m_VRStateData.StereoRenderingMode = VRState.StereoRenderingModes.SinglePass;
            else if (UnityEngine.XR.XRSettings.stereoRenderingMode == UnityEngine.XR.XRSettings.StereoRenderingMode.MultiPass) //(UnityEditor.PlayerSettings.stereoRenderingPath == UnityEditor.StereoRenderingPath.MultiPass)
                m_VRStateData.StereoRenderingMode = VRState.StereoRenderingModes.MultiPass;
            #endif
            VRStateData = m_VRStateData;

            FindURPRendererData();
        }

        void Start()
        {
            if (GetPlayerAtRuntime == EnableFeature.Enabled)
            {
                //Make sure our PlayerTransform is null because we will be looking it up via Unity tag or by name.
                PlayerTransform = null;

                //If our player is being received at runtime, wait to intilialize UniStorm until the player has been found.
                if (UseRuntimeDelay == EnableFeature.Enabled)
                {
                    StartCoroutine(InitializeDelay());
                }
                //If our player is being received at runtime and UseRuntimeDelay is disabled, get our player immediately by tag.
                else if (UseRuntimeDelay == EnableFeature.Disabled)
                {
                    if (GetPlayerMethod == GetPlayerMethodEnum.ByTag)
                    {
                        PlayerTransform = GameObject.FindWithTag(PlayerTag).transform;
                        PlayerCamera = GameObject.FindWithTag(CameraTag).GetComponent<Camera>();
                    }
                    else if (GetPlayerMethod == GetPlayerMethodEnum.ByName)
                    {
                        PlayerTransform = GameObject.Find(PlayerName).transform;
                        PlayerCamera = GameObject.Find(CameraName).GetComponent<Camera>();
                    }
                    InitializeUniStorm();
                }
            }
            //If our player is not being received at runtime, initialize UniStorm immediately.
            else if (GetPlayerAtRuntime == EnableFeature.Disabled)
            {
                InitializeUniStorm();
            }
        }

        //Wait to intilialize UniStorm until the player has been found.
        IEnumerator InitializeDelay()
        {
            PlayerTransform = null;
            PlayerCamera = null;

            yield return new WaitWhile(() => GameObject.FindWithTag(PlayerTag) == null);     
            yield return new WaitWhile(() => GameObject.FindWithTag(CameraTag) == null);

            if (GetPlayerMethod == GetPlayerMethodEnum.ByTag)
            {
                PlayerTransform = GameObject.FindWithTag(PlayerTag).transform;
                PlayerCamera = GameObject.FindWithTag(CameraTag).GetComponent<Camera>();
            }
            else if (GetPlayerMethod == GetPlayerMethodEnum.ByName)
            {
                PlayerTransform = GameObject.Find(PlayerName).transform;
                PlayerCamera = GameObject.Find(CameraName).GetComponent<Camera>();
            }
            InitializeUniStorm();
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

        //Intilialize UniStorm
        void InitializeUniStorm()
        {
            StopCoroutine(InitializeDelay());

            if (PlayerTransform == null || PlayerCamera == null)
            {
                Debug.LogWarning("(UniStorm has been disabled) - No player/camera has been assigned on the Player Transform/Player Camera slot." +
                    "Please go to the Player & Camera tab and assign one.");
                GetComponent<UniStormSystem>().enabled = false;
            }
            else if (!PlayerTransform.gameObject.activeSelf || !PlayerCamera.gameObject.activeSelf)
            {
                Debug.LogWarning("(UniStorm has been disabled) - The player/camera game object is disabled on the Player Transform/Player Camera slot is disabled. " +
                    "Please go to the Player & Camera tab and ensure your player/camera is enabled.");
                GetComponent<UniStormSystem>().enabled = false;
            }

            //If our current weather type is not apart of the available weather type lists, assign it to the proper category.
            if (!AllWeatherTypes.Contains(CurrentWeatherType))
            {
                AllWeatherTypes.Add(CurrentWeatherType);
            }

            if (MusicVolume == 0)
            {
                MusicVolume = 0.001f;
            }
            if (AmbienceVolume == 0)
            {
                AmbienceVolume = 0.001f;
            }
            if (WeatherSoundsVolume == 0)
            {
                WeatherSoundsVolume = 0.001f;
            }

            UniStormAudioMixer = Resources.Load("UniStorm Audio Mixer") as UnityEngine.Audio.AudioMixer;
            UniStormAudioMixer.SetFloat("MusicVolume", Mathf.Log(MusicVolume) * 20);
            UniStormAudioMixer.SetFloat("AmbienceVolume", Mathf.Log(AmbienceVolume) * 20);
            UniStormAudioMixer.SetFloat("WeatherVolume", Mathf.Log(WeatherSoundsVolume) * 20);

            //Setup our sound holder
            m_SoundTransform = new GameObject();
            m_SoundTransform.name = "UniStorm Sounds";
            m_SoundTransform.transform.SetParent(PlayerTransform);
            m_SoundTransform.transform.localPosition = Vector3.zero;

            //Setup our particle effects holder
            m_EffectsTransform = new GameObject();
            m_EffectsTransform.name = "UniStorm Effects";
            m_EffectsTransform.AddComponent<UniStormStereoRenderer>();
            m_EffectsTransform.transform.SetParent(PlayerTransform);
            m_EffectsTransform.transform.localPosition = Vector3.zero;

            for (int i = 0; i < AllWeatherTypes.Count; i++)
            {
                if (AllWeatherTypes[i] != null)
                {
                    if (AllWeatherTypes[i].PrecipitationWeatherType == WeatherType.Yes_No.Yes && !PrecipiationWeatherTypes.Contains(AllWeatherTypes[i]))
                    {
                        PrecipiationWeatherTypes.Add(AllWeatherTypes[i]);
                    }
                    else if (AllWeatherTypes[i].PrecipitationWeatherType == WeatherType.Yes_No.No && !NonPrecipiationWeatherTypes.Contains(AllWeatherTypes[i]))
                    {
                        NonPrecipiationWeatherTypes.Add(AllWeatherTypes[i]);
                    }
                }
                else
                {
                    Debug.Log("A Weather Type from the All Weather Types list is missing. It will be excluded form the usable weather types.");
                }
            }

            //Sets up and checks all of our weather types
            for (int i = 0; i < AllWeatherTypes.Count; i++)
            {
                if (AllWeatherTypes[i] != null)
                {
                    //If our weather types have certain features enabled, but there are none detected, disable the feature.
                    if (AllWeatherTypes[i].UseWeatherSound == WeatherType.Yes_No.Yes && AllWeatherTypes[i].WeatherSound == null)
                    {
                        AllWeatherTypes[i].UseWeatherSound = WeatherType.Yes_No.No;
                    }

                    if (AllWeatherTypes[i].UseWeatherEffect == WeatherType.Yes_No.Yes && AllWeatherTypes[i].WeatherEffect == null)
                    {
                        AllWeatherTypes[i].UseWeatherEffect = WeatherType.Yes_No.No;
                    }

                    if (AllWeatherTypes[i].UseAdditionalWeatherEffect == WeatherType.Yes_No.Yes && AllWeatherTypes[i].AdditionalWeatherEffect == null)
                    {
                        AllWeatherTypes[i].UseAdditionalWeatherEffect = WeatherType.Yes_No.No;
                    }

                    //Add all of our weather effects to a list to be controlled when needed.
                    if (!ParticleSystemList.Contains(AllWeatherTypes[i].WeatherEffect) && AllWeatherTypes[i].WeatherEffect != null)
                    {
                        AllWeatherTypes[i].CreateWeatherEffect();
                        ParticleSystemList.Add(AllWeatherTypes[i].WeatherEffect);
                    }

                    //Add all of our additional weather effects to a list to be controlled when needed.
                    if (!AdditionalParticleSystemList.Contains(AllWeatherTypes[i].AdditionalWeatherEffect))
                    {
                        if (AllWeatherTypes[i].UseAdditionalWeatherEffect == WeatherType.Yes_No.Yes)
                        {
                            AllWeatherTypes[i].CreateAdditionalWeatherEffect();
                            AdditionalParticleSystemList.Add(AllWeatherTypes[i].AdditionalWeatherEffect);
                        }
                    }

                    //Create a weather sound for each weather type that has one.
                    if (AllWeatherTypes[i].UseWeatherSound == WeatherType.Yes_No.Yes && AllWeatherTypes[i].WeatherSound != null)
                    {
                        AllWeatherTypes[i].CreateWeatherSound();
                    }
                }
            }

            //Initialize the color switching keys. This allows gradient colors to be switched between stormy and regular.
            CloudColorKeySwitcher = new GradientColorKey[7];
            CloudColorKeySwitcher = CloudBaseColor.colorKeys;
            DefaultCloudBaseColor.colorKeys = new GradientColorKey[7];
            DefaultCloudBaseColor.colorKeys = CloudBaseColor.colorKeys;

            FogColorKeySwitcher = new GradientColorKey[7];
            FogColorKeySwitcher = FogColor.colorKeys;
            DefaultFogBaseColor.colorKeys = new GradientColorKey[7];
            DefaultFogBaseColor.colorKeys = FogColor.colorKeys;

            CloudLightColorKeySwitcher = new GradientColorKey[7];
            CloudLightColorKeySwitcher = CloudLightColor.colorKeys;
            DefaultCloudLightColor.colorKeys = new GradientColorKey[7];
            DefaultCloudLightColor.colorKeys = CloudLightColor.colorKeys;

            FogLightColorKeySwitcher = new GradientColorKey[7];
            FogLightColorKeySwitcher = FogLightColor.colorKeys;
            DefaultFogLightColor.colorKeys = new GradientColorKey[7];
            DefaultFogLightColor.colorKeys = FogLightColor.colorKeys;

            AmbientSkyLightColorKeySwitcher = new GradientColorKey[7];
            AmbientSkyLightColorKeySwitcher = AmbientSkyLightColor.colorKeys;
            DefaultAmbientSkyLightBaseColor.colorKeys = new GradientColorKey[7];
            DefaultAmbientSkyLightBaseColor.colorKeys = AmbientSkyLightColor.colorKeys;

            AmbientEquatorLightColorKeySwitcher = new GradientColorKey[7];
            AmbientEquatorLightColorKeySwitcher = AmbientEquatorLightColor.colorKeys;
            DefaultAmbientEquatorLightBaseColor.colorKeys = new GradientColorKey[7];
            DefaultAmbientEquatorLightBaseColor.colorKeys = AmbientEquatorLightColor.colorKeys;

            AmbientGroundLightColorKeySwitcher = new GradientColorKey[7];
            AmbientGroundLightColorKeySwitcher = AmbientGroundLightColor.colorKeys;
            DefaultAmbientGroundLightBaseColor.colorKeys = new GradientColorKey[7];
            DefaultAmbientGroundLightBaseColor.colorKeys = AmbientGroundLightColor.colorKeys;

            SunLightColorKeySwitcher = new GradientColorKey[6];
            SunLightColorKeySwitcher = SunColor.colorKeys;
            DefaultSunLightBaseColor.colorKeys = new GradientColorKey[6];
            DefaultSunLightBaseColor.colorKeys = SunColor.colorKeys;

            CalculatePrecipiation();
            CreateSun();
            CreateMoon();            

            //Intialize the other components and set the proper settings from within the editor
            GameObject TempAudioSource = new GameObject("UniStorm Time of Day Sounds");
            TempAudioSource.transform.SetParent(this.transform);
            TempAudioSource.transform.localPosition = Vector3.zero;
            TempAudioSource.AddComponent<AudioSource>();
            TimeOfDayAudioSource = TempAudioSource.GetComponent<AudioSource>();
            TimeOfDayAudioSource.outputAudioMixerGroup = UniStormAudioMixer.FindMatchingGroups("Master/Ambience")[0];
            m_TimeOfDaySoundsSeconds = Random.Range(TimeOfDaySoundsSecondsMin, TimeOfDaySoundsSecondsMax + 1);

            GameObject TempAudioSourceMusic = new GameObject("UniStorm Time of Day Music");
            TempAudioSourceMusic.transform.SetParent(this.transform);
            TempAudioSourceMusic.transform.localPosition = Vector3.zero;
            TempAudioSourceMusic.AddComponent<AudioSource>();
            TimeOfDayMusicAudioSource = TempAudioSourceMusic.GetComponent<AudioSource>();
            TimeOfDayMusicAudioSource.outputAudioMixerGroup = UniStormAudioMixer.FindMatchingGroups("Master/Music")[0];

            UniStormWindZone = GameObject.Find("UniStorm Windzone").GetComponent<WindZone>();
            m_StarsRenderer = GameObject.Find("UniStorm Stars").GetComponent<Renderer>();
            m_StarsMaterial = m_StarsRenderer.material;
            m_StarsMaterial.SetFloat("_StarSpeed", StarSpeed);
            m_StarsRenderer.gameObject.AddComponent<MeshFilter>();
            m_StarsRenderer.GetComponent<MeshFilter>().sharedMesh = ProceduralHemispherePolarUVs.hemisphere;
            if (StarmapType == StarmapTypeEnum.LightConstellations)
            {
                m_StarsMaterial.SetTexture("_Starmap", Resources.Load("Starmap (Light Constellations)") as Texture);
            }
            else if (StarmapType == StarmapTypeEnum.StrongConstellations)
            {
                m_StarsMaterial.SetTexture("_Starmap", Resources.Load("Starmap (Strong Constellations)") as Texture);
            }
            else if (StarmapType == StarmapTypeEnum.VeryStrongConstellations)
            {
                m_StarsMaterial.SetTexture("_Starmap", Resources.Load("Starmap (Very Strong Constellations)") as Texture);
            }

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                m_StarsMaterial.SetFloat("_LoY", -2200);
                m_StarsMaterial.SetFloat("_HiY", -60);
            }

            m_CloudDomeMaterial = m_UniStormClouds.skyMaterial;
            GameObject AuroraSystem = Resources.Load("UniStorm Auroras") as GameObject;
            m_AuroraParent = Instantiate(AuroraSystem, transform.position, Quaternion.identity);
            m_AuroraParent.transform.SetParent(m_UniStormClouds.transform);
            m_AuroraParent.transform.localPosition = Vector3.zero;
            m_AuroraParent.transform.localScale = Vector3.one * 0.001f;
            m_AuroraParent.name = "UniStorm Auroras";

            //Calculates our start time based off the user's input
            float StartingMinuteFloat = (int)Minute;
            if (RealWorldTime == EnableFeature.Disabled)
            {
                m_TimeFloat = (float)Hour / 24 + StartingMinuteFloat / 1440;
            }
            else if (RealWorldTime == EnableFeature.Enabled)
            {
                m_TimeFloat = (float)System.DateTime.Now.Hour / 24 + (float)System.DateTime.Now.Minute / 1440;
            }

            m_LastHour = Hour;
            m_SunLight.intensity = SunIntensityCurve.Evaluate((float)Hour) * SunIntensity;
            m_MoonLight.intensity = MoonIntensityCurve.Evaluate((float)Hour) * MoonIntensity * MoonPhaseIntensity;

            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyCloudy)
            {
                MostlyCloudyFadeValue = 1;
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Cloudy)
            {
                if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                {
                    MostlyCloudyFadeValue = 1;
                }
            }

            if (CloudRenderType == CloudRenderTypeEnum.Transparent)
            {
                if (Hour <= 5 || Hour >= 19)
                {
                    m_CloudDomeMaterial.SetFloat("_MaskMoon", 1);
                }
                if (Hour >= 6 && Hour < 19)
                {
                    m_CloudDomeMaterial.SetFloat("_MaskMoon", 0);
                }
            }
            else if (CloudRenderType == CloudRenderTypeEnum.Opaque)
            {
                m_CloudDomeMaterial.SetFloat("_MaskMoon", 1);
            }

            //Get randomized cloud amount 
            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.DontChange)
            {
                m_CloudDomeMaterial.SetFloat("_uCloudsCoverage", Random.Range(0.4f, 0.55f));
            }

            m_SkyBoxMaterial = (Material)Resources.Load("UniStorm Skybox") as Material;
            RenderSettings.skybox = m_SkyBoxMaterial;
            m_SkyBoxMaterial.SetFloat("_AtmosphereThickness", AtmosphereThickness.Evaluate((float)Hour));
            m_SkyBoxMaterial.SetColor("_NightSkyTint", SkyTintColor.Evaluate((float)Hour));
            RenderSettings.reflectionIntensity = EnvironmentReflections.Evaluate((float)Hour);

            Temperature = (int)TemperatureCurve.Evaluate(m_PreciseCurveTime) + (int)TemperatureFluctuation.Evaluate((float)StartingHour);

            if (TemperatureType == TemperatureTypeEnum.Fahrenheit)
            {
                m_FreezingTemperature = 32;
            }
            else if (TemperatureType == TemperatureTypeEnum.Celsius)
            {
                m_FreezingTemperature = 0;
            }

            transform.position = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);

            GenerateWeather();
            CreateLightning();
            CreateUniStormFog();
            UpdateColors();
            CalculateMoonPhase();
            InitializeWeather(true);
            CalculateTimeOfDay();
            CalculateSeason();
            UpdateCelestialLightShafts();
            StartCoroutine(InitializeCloudShadows());           

            if (CurrentWeatherType.UseAuroras == WeatherType.Yes_No.Yes)
            {
                if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Night)
                {
                    m_AuroraParent.SetActive(true);
                    Shader.SetGlobalFloat("_LightIntensity", CurrentWeatherType.AuroraIntensity);
                    Shader.SetGlobalColor("_InnerColor", CurrentWeatherType.AuroraInnerColor);
                    Shader.SetGlobalColor("_OuterColor", CurrentWeatherType.AuroraOuterColor);
                }
            }
            else if (CurrentWeatherType.UseAuroras == WeatherType.Yes_No.No)
            {
                m_AuroraParent.SetActive(false);
            }

            if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Hourly)
            {
                WeatherForecast[Hour] = CurrentWeatherType;
            }

            //Only create our UniStorm UI if it is enabled
            if (UseUniStormMenu == UniStormSystem.EnableFeature.Enabled)
            {
                CreateUniStormMenu();
            }

            Material m_CloudsMaterial = m_UniStormClouds.skyMaterial;
            if (CustomizeQuality == CustomizeQualityEnum.Yes && CloudType == CloudTypeEnum.Volumetric)
            {
                if (CloudQuality == CloudQualityEnum.Ultra)
                {
                    m_CloudsMaterial.SetFloat("_UseHighConvergenceSpeed", 1);
                    m_CloudDomeMaterial.SetFloat("_DistantCloudUpdateSpeed", ConvergenceSpeed);
                    Shader.SetGlobalFloat("CLOUD_MARCH_STEPS", NearMarchSteps);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", DistantMarchSteps);
                }
                else
                {
                    m_CloudsMaterial.SetFloat("_UseHighConvergenceSpeed", 0);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", 10);
                }
            }
            else
            {
                if (CloudQuality == CloudQualityEnum.Ultra) //If CustomizeQuality is not used, apply the default Ultra settings.
                {
                    m_CloudsMaterial.SetFloat("_UseHighConvergenceSpeed", 1);
                    m_CloudDomeMaterial.SetFloat("_DistantCloudUpdateSpeed", 75);
                    Shader.SetGlobalFloat("CLOUD_MARCH_STEPS", 100);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", 10);
                }
                else
                {
                    m_CloudsMaterial.SetFloat("_UseHighConvergenceSpeed", 0);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", 10);
                }
            }

            //Enable Single Pass support for UniStorm's clouds, given that the VR settings are enabled.
            if (VRStateData.VREnabled && VRStateData.StereoRenderingMode == VRState.StereoRenderingModes.SinglePass)
                m_CloudsMaterial.SetFloat("_VRSinglePassEnabled", 1);
            else if (!VRStateData.VREnabled || VRStateData.VREnabled && VRStateData.StereoRenderingMode == VRState.StereoRenderingModes.MultiPass)
                m_CloudsMaterial.SetFloat("_VRSinglePassEnabled", 0);

            UniStormInitialized = true;
        }

        IEnumerator InitializeCloudShadows ()
        {
            //Cloud Shadows are not currently supported with URP so disable them
            GetCloudShadowsFeatureSettings().isEnabled = false;
            CloudShadows = EnableFeature.Disabled;
            yield break;

            /*
            if (CloudShadows == EnableFeature.Enabled)
            {
                if (GetCloudShadowsFeature() != null)
                {
                    m_CloudShadows = GetCloudShadowsFeatureSettings();
                }
                else
                {
                    m_CloudShadows = GetCloudShadowsFeatureSettings();
                    m_CloudShadows.isEnabled = true;
                }

                //Find UniStorm's cloud system and wait until the cloud shadows are generated before finishing initialization.
                yield return new WaitUntil(() => m_UniStormClouds.PublicCloudShadowTexture != null);
                m_CloudShadows.CloudShadowTexture = m_UniStormClouds.PublicCloudShadowTexture;
                m_CloudShadows.BottomThreshold = 0.5f;
                m_CloudShadows.TopThreshold = 1;
                m_CloudShadows.CloudTextureScale = 0.001f;
                m_CloudShadows.ShadowIntensity = CurrentWeatherType.CloudShadowIntensity;

                m_CloudShadows.m_CurrentCloudHeight = m_CurrentCloudHeight;
                m_CloudShadows.CloudSpeed = CloudSpeed;

                PlayerCamera.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                if (GetCloudShadowsFeature() != null)
                {
                    GetCloudShadowsFeatureSettings().isEnabled = false;
                }
            }
            */
        }

        internal UniStormCloudShadowsFeature.Settings GetCloudShadowsFeatureSettings()
        {
            UniStormCloudShadowsFeature uniStormCloudShadowsFeature = GetCloudShadowsFeature();
            if (uniStormCloudShadowsFeature != null)
            {
                return uniStormCloudShadowsFeature.settings;
            }

            return new UniStormCloudShadowsFeature.Settings();
        }

        private UniStormCloudShadowsFeature GetCloudShadowsFeature()
        {
            if (urpForwardRendererData != null)
            {
                UniStormCloudShadowsFeature uniStormCloudShadowsFeature = urpForwardRendererData.rendererFeatures.OfType<UniStormCloudShadowsFeature>().FirstOrDefault();
                return uniStormCloudShadowsFeature;
            }

            return null;
        }

        internal void EnableCloudShadows(bool enable)
        {
            GetCloudShadowsFeatureSettings().isEnabled = enable;
        }

        void InitializeCloudSettings()
        {
            Material m_CloudsMaterial = m_UniStormClouds.skyMaterial;
            m_UniStormClouds.performance = (UniStormClouds.CloudPerformance)CloudQuality;
            m_CloudsMaterial.SetFloat("_uCloudsMovementSpeed", (float)CloudSpeed);
            m_CloudsMaterial.SetFloat("_uCloudsTurbulenceSpeed", (float)CloudTurbulence);
            m_CloudsMaterial.SetColor("_uMoonColor", MoonlightColor);
            m_CloudsMaterial.SetColor("_uLightningColor", LightningLightColor);

            if (ForceLowClouds == EnableFeature.Enabled)
            {
                Shader.SetGlobalFloat("_uCloudNoiseScale", 1.8f);
            } 
            else
            {
                Shader.SetGlobalFloat("_uCloudNoiseScale", 0.7f);
            }

            if (CloudShadows == EnableFeature.Enabled)
            {
                m_UniStormClouds.CloudShadowsTypeRef = UniStormClouds.CloudShadowsType.Simulated;

                if (CloudShadowResolution == CloudShadowResolutionEnum._256x256)
                {
                    m_UniStormClouds.CloudShadowResolutionValue = 256;
                }
                else if (CloudShadowResolution == CloudShadowResolutionEnum._512x512)
                {
                    m_UniStormClouds.CloudShadowResolutionValue = 512;
                }
                else if (CloudShadowResolution == CloudShadowResolutionEnum._1024x1024)
                {
                    m_UniStormClouds.CloudShadowResolutionValue = 1024;
                }
            }

            if (CloudType == CloudTypeEnum.Volumetric)
            {
                m_UniStormClouds.cloudType = UniStormClouds.CloudType.Volumetric;

                CloudProfile m_CP = CurrentWeatherType.CloudProfileComponent;
                m_CloudsMaterial.SetFloat("_uCloudsBaseEdgeSoftness", m_CP.EdgeSoftness);
                m_CloudsMaterial.SetFloat("_uCloudsBottomSoftness", m_CP.BaseSoftness);
                m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", m_CP.DetailStrength);
                m_CloudsMaterial.SetFloat("_uCloudsDensity", m_CP.Density);
                m_CloudsMaterial.SetFloat("_uCloudsDetailScale", 1000f);

                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    m_CloudsMaterial.SetFloat("_uCloudsCoverageBias", m_CP.CoverageBias);
                    m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", m_CP.DetailStrength);
                }
                else
                {
                    m_CloudsMaterial.SetFloat("_uCloudsCoverageBias", 0.02f);
                    m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", m_CP.DetailStrength);
                }

                m_CloudsMaterial.SetFloat("_uCloudsBaseScale", 1.72f);
            }
            else if (CloudType == CloudTypeEnum._2D)
            {
                m_UniStormClouds.cloudType = UniStormClouds.CloudType.TwoD;
                m_CloudsMaterial.SetFloat("_uCloudsBaseEdgeSoftness", 0.05f);
                m_CloudsMaterial.SetFloat("_uCloudsBottomSoftness", 0.15f);
                m_CloudsMaterial.SetFloat("_uCloudsDetailStrength", 0.1f);
                m_CloudsMaterial.SetFloat("_uCloudsDensity", 1f);
                m_CloudsMaterial.SetFloat("_uCloudsBaseScale", 1.5f);
                m_CloudsMaterial.SetFloat("_uCloudsDetailScale", 700f);
            }
        }

        //Initialize our starting weather so it fades in instantly on start
        public void InitializeWeather(bool UseWeatherConditions)
        {
            if (CloudCoroutine != null) { StopCoroutine(CloudCoroutine); }
            if (FogCoroutine != null) { StopCoroutine(FogCoroutine); }
            if (WeatherEffectCoroutine != null) { StopCoroutine(WeatherEffectCoroutine); }
            if (AdditionalWeatherEffectCoroutine != null) { StopCoroutine(AdditionalWeatherEffectCoroutine); }
            if (ParticleFadeCoroutine != null) { StopCoroutine(ParticleFadeCoroutine); }
            if (AdditionalParticleFadeCoroutine != null) { StopCoroutine(AdditionalParticleFadeCoroutine); }
            if (SunCoroutine != null) { StopCoroutine(SunCoroutine); }
            if (MoonCoroutine != null) { StopCoroutine(MoonCoroutine); }
            if (SoundInCoroutine != null) { StopCoroutine(SoundInCoroutine); }
            if (SoundOutCoroutine != null) { StopCoroutine(SoundOutCoroutine); }
            if (ColorCoroutine != null) { StopCoroutine(ColorCoroutine); }
            if (SunColorCoroutine != null) { StopCoroutine(SunColorCoroutine); }
            if (CloudHeightCoroutine != null) { StopCoroutine(CloudHeightCoroutine); }
            if (WindCoroutine != null) { StopCoroutine(WindCoroutine); }
            if (RainShaderCoroutine != null) { StopCoroutine(RainShaderCoroutine); }
            if (SnowShaderCoroutine != null) { StopCoroutine(SnowShaderCoroutine); }
            if (StormyCloudsCoroutine != null) { StopCoroutine(StormyCloudsCoroutine); }
            if (CloudProfileCoroutine != null) { StopCoroutine(CloudProfileCoroutine); }
            if (CloudShadowIntensityCoroutine != null) { StopCoroutine(CloudShadowIntensityCoroutine); }
            if (SunAttenuationIntensityCoroutine != null) { StopCoroutine(SunAttenuationIntensityCoroutine); }
            if (AuroraCoroutine != null) { StopCoroutine(AuroraCoroutine); }

            //If our starting weather type's conditions are not met, keep rerolling weather until an appropriate one is found.
            TempWeatherType = CurrentWeatherType;

            if (UseWeatherConditions)
            {
                while (TempWeatherType.TemperatureType == WeatherType.TemperatureTypeEnum.AboveFreezing && Temperature <= m_FreezingTemperature
                    || TempWeatherType.Season != WeatherType.SeasonEnum.All && (int)TempWeatherType.Season != (int)CurrentSeason
                || TempWeatherType.TemperatureType == WeatherType.TemperatureTypeEnum.BelowFreezing && Temperature > m_FreezingTemperature)
                {
                    if (TempWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                    {
                        TempWeatherType = NonPrecipiationWeatherTypes[Random.Range(0, NonPrecipiationWeatherTypes.Count)];
                    }
                    else if (TempWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                    {
                        TempWeatherType = PrecipiationWeatherTypes[Random.Range(0, PrecipiationWeatherTypes.Count)];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            CurrentWeatherType = TempWeatherType;
            m_ReceivedCloudValue = GetCloudLevel(true);
            m_CloudDomeMaterial.SetFloat("_uCloudsCoverage", m_ReceivedCloudValue);
            RenderSettings.fogDensity = CurrentWeatherType.FogDensity;
            CurrentFogAmount = RenderSettings.fogDensity;
            UniStormWindZone.windMain = CurrentWeatherType.WindIntensity;
            CurrentWindIntensity = CurrentWeatherType.WindIntensity;
            SunIntensity = CurrentWeatherType.SunIntensity;
            MoonIntensity = CurrentWeatherType.MoonIntensity;
            if (ForceLowClouds == EnableFeature.Disabled)
            {
                m_CloudDomeMaterial.SetFloat("_uCloudsBottom", CurrentWeatherType.CloudHeight);
                m_CurrentCloudHeight = CurrentWeatherType.CloudHeight;
            }
            else
            {
                m_CloudDomeMaterial.SetFloat("_uCloudsBottom", LowCloudHeight);
                m_CurrentCloudHeight = LowCloudHeight;
            }            

            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Cloudy || CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyCloudy)
            {
                m_CloudDomeMaterial.SetFloat("_uCloudsHeight", 1000);
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyClear || CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Clear ||
                CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.PartyCloudy)
            {
                m_CloudDomeMaterial.SetFloat("_uCloudsHeight", 1000);
            }

            if (FogMode == FogModeEnum.Exponential)
            {
                RenderSettings.fogMode = UnityEngine.FogMode.Exponential;
            }
            else if (FogMode == FogModeEnum.ExponentialSquared)
            {
                RenderSettings.fogMode = UnityEngine.FogMode.ExponentialSquared;
            }

            if (FogType == FogTypeEnum.UnistormFog)
            {
                //Disable Unity's fog while UniStorm's fog is being used.
                RenderSettings.fog = false;

                if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                {
                    if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.No)
                    {
                        m_UniStormAtmosphericFog.BlendHeight = 0.0f;
                    }
                    else if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.Yes)
                    {
                        m_UniStormAtmosphericFog.BlendHeight = ((1 - CurrentWeatherType.CameraFogHeight) / 10);
                    }
                    m_CloudDomeMaterial.SetFloat("_FogBlendHeight", (1-CurrentWeatherType.FogHeight));
                    SunObjectMaterial.SetFloat("_OpaqueY", -600);
                    SunObjectMaterial.SetFloat("_TransparentY", -400);
                }
                else if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                {
                    if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.No)
                    {
                        m_UniStormAtmosphericFog.BlendHeight = ((1 - CameraFogHeight) / 10);
                    }
                    else if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.Yes)
                    {
                        m_UniStormAtmosphericFog.BlendHeight = ((1 - CurrentWeatherType.CameraFogHeight) / 10);
                    }
                    m_CloudDomeMaterial.SetFloat("_FogBlendHeight", (1-CurrentWeatherType.FogHeight));
                    SunObjectMaterial.SetFloat("_OpaqueY", -100);
                    SunObjectMaterial.SetFloat("_TransparentY", -70);
                }

                if (UseRadialDistantFog == EnableFeature.Enabled)
                {
                    m_UniStormAtmosphericFog.useRadialDistance = true;
                }

                FogLightFalloff = CurrentWeatherType.FogLightFalloff;
            }

            if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.Rain)
            {
                Shader.SetGlobalFloat("_WetnessStrength", 1);
                Shader.SetGlobalFloat("_SnowStrength", 0);
            }
            else if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.Snow)
            {
                Shader.SetGlobalFloat("_SnowStrength", 1);
                Shader.SetGlobalFloat("_WetnessStrength", 0);
            }
            else
            {
                Shader.SetGlobalFloat("_WetnessStrength", 0);
                Shader.SetGlobalFloat("_SnowStrength", 0);
            }

            for (int i = 0; i < WeatherEffectsList.Count; i++)
            {
                ParticleSystem.EmissionModule CurrentEmission = WeatherEffectsList[i].emission;
                CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
            }

            for (int i = 0; i < AdditionalWeatherEffectsList.Count; i++)
            {
                ParticleSystem.EmissionModule CurrentEmission = AdditionalWeatherEffectsList[i].emission;
                CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
            }

            //Initialize our weather type's particle effetcs
            if (CurrentWeatherType.UseWeatherEffect == WeatherType.Yes_No.Yes)
            {
                for (int i = 0; i < WeatherEffectsList.Count; i++)
                {
                    if (WeatherEffectsList[i].name == CurrentWeatherType.WeatherEffect.name + " (UniStorm)")
                    {
                        CurrentParticleSystem = WeatherEffectsList[i];
                        ParticleSystem.EmissionModule CurrentEmission = CurrentParticleSystem.emission;
                        CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve((float)CurrentWeatherType.ParticleEffectAmount);
                    }
                }

                CurrentParticleSystem.transform.localPosition = CurrentWeatherType.ParticleEffectVector;
            }

            //Initialize our weather type's additional particle effetcs
            if (CurrentWeatherType.UseAdditionalWeatherEffect == WeatherType.Yes_No.Yes)
            {
                for (int i = 0; i < AdditionalWeatherEffectsList.Count; i++)
                {
                    if (AdditionalWeatherEffectsList[i].name == CurrentWeatherType.AdditionalWeatherEffect.name + " (UniStorm)")
                    {
                        AdditionalCurrentParticleSystem = AdditionalWeatherEffectsList[i];
                        ParticleSystem.EmissionModule CurrentEmission = AdditionalCurrentParticleSystem.emission;
                        CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve((float)CurrentWeatherType.AdditionalParticleEffectAmount);
                    }
                }

                AdditionalCurrentParticleSystem.transform.localPosition = CurrentWeatherType.AdditionalParticleEffectVector;
            }

            //Instantly change all of our gradients to the stormy gradients
            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeStart", -0.2f);
                m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeEnd", 0.32f);
                m_CloudDomeMaterial.SetFloat("_uHorizonFadeStart", 0);

                if (FogType == FogTypeEnum.UnistormFog)
                {
                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", 0.22f);
                    m_CloudDomeMaterial.SetFloat("_uSunFadeEnd", 0.18f);
                }
                else if (FogType == FogTypeEnum.UnityFog)
                {
                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", 0);
                }

                m_CloudDomeMaterial.SetFloat("_uCloudAlpha", StormyHorizonBrightness);
                SunAttenuationMultipler = 0.35f;

                for (int i = 0; i < CloudBaseColor.colorKeys.Length; i++)
                {
                    if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.No)
                    {
                        CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CloudStormyBaseColor.colorKeys[i].color, 1);
                    }
                    else if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.Yes)
                    {
                        CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CurrentWeatherType.CloudColor.colorKeys[i].color, 1);
                    }
                }

                for (int i = 0; i < FogColor.colorKeys.Length; i++)
                {
                    if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.No)
                    {
                        FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, FogStormyColor.colorKeys[i].color, 1);
                    }
                    else if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.Yes)
                    {
                        FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, CurrentWeatherType.FogColor.colorKeys[i].color, 1);
                    }
                }
              
                for (int i = 0; i < CloudLightColor.colorKeys.Length; i++)
                {
                    CloudLightColorKeySwitcher[i].color = Color.Lerp(CloudLightColorKeySwitcher[i].color, StormyCloudLightColor.colorKeys[i].color, 1);
                }
                           
                for (int i = 0; i < FogLightColor.colorKeys.Length; i++)
                {
                    FogLightColorKeySwitcher[i].color = Color.Lerp(FogLightColorKeySwitcher[i].color, StormyFogLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < AmbientSkyLightColor.colorKeys.Length; i++)
                {
                    AmbientSkyLightColorKeySwitcher[i].color = Color.Lerp(AmbientSkyLightColorKeySwitcher[i].color, StormyAmbientSkyLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < AmbientEquatorLightColor.colorKeys.Length; i++)
                {
                    AmbientEquatorLightColorKeySwitcher[i].color = Color.Lerp(AmbientEquatorLightColorKeySwitcher[i].color, StormyAmbientEquatorLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < AmbientGroundLightColor.colorKeys.Length; i++)
                {
                    AmbientGroundLightColorKeySwitcher[i].color = Color.Lerp(AmbientGroundLightColorKeySwitcher[i].color, StormyAmbientGroundLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < SunColor.colorKeys.Length; i++)
                {
                    SunLightColorKeySwitcher[i].color = Color.Lerp(SunLightColorKeySwitcher[i].color, StormySunColor.colorKeys[i].color, 1);
                }

                FogLightColor.SetKeys(FogLightColorKeySwitcher, FogLightColor.alphaKeys);
                CloudLightColor.SetKeys(CloudLightColorKeySwitcher, CloudLightColor.alphaKeys);
                FogColor.SetKeys(FogColorKeySwitcher, FogColor.alphaKeys);
                CloudBaseColor.SetKeys(CloudColorKeySwitcher, CloudBaseColor.alphaKeys);
                AmbientSkyLightColor.SetKeys(AmbientSkyLightColorKeySwitcher, AmbientSkyLightColor.alphaKeys);
                AmbientEquatorLightColor.SetKeys(AmbientEquatorLightColorKeySwitcher, AmbientEquatorLightColor.alphaKeys);
                AmbientGroundLightColor.SetKeys(AmbientGroundLightColorKeySwitcher, AmbientGroundLightColor.alphaKeys);
                SunColor.SetKeys(SunLightColorKeySwitcher, SunColor.alphaKeys);
            }
            //Instantly change all of our gradients to the regular gradients
            else if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
            {
                m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeStart", 0);
                m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeEnd", 0);
                m_CloudDomeMaterial.SetFloat("_uHorizonFadeStart", m_CloudFadeLevelStart);
                m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", m_CloudFadeLevelEnd);
                m_CloudDomeMaterial.SetFloat("_uSunFadeEnd", 0.045f);
                m_CloudDomeMaterial.SetFloat("_uCloudAlpha", 1);

                //Reduce the sun's attenuation by 50% if Gamma Color Space is used to stop the clouds from being too bright.
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    SunAttenuationMultipler = 0.5f;
                }
                else
                {
                    SunAttenuationMultipler = 1;
                }              

                for (int i = 0; i < CloudBaseColor.colorKeys.Length; i++)
                {
                    if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.No)
                    {
                        CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, DefaultCloudBaseColor.colorKeys[i].color, 1);
                    }
                    else if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.Yes)
                    {
                        CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CurrentWeatherType.CloudColor.colorKeys[i].color, 1);
                    }
                }

                for (int i = 0; i < FogColor.colorKeys.Length; i++)
                {
                    if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.No)
                    {
                        FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, DefaultFogBaseColor.colorKeys[i].color, 1);
                    }
                    else if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.Yes)
                    {
                        FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, CurrentWeatherType.FogColor.colorKeys[i].color, 1);
                    }                       
                }

                for (int i = 0; i < AmbientSkyLightColor.colorKeys.Length; i++)
                {
                    AmbientSkyLightColorKeySwitcher[i].color = Color.Lerp(AmbientSkyLightColorKeySwitcher[i].color, DefaultAmbientSkyLightBaseColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < CloudLightColor.colorKeys.Length; i++)
                {
                    CloudLightColorKeySwitcher[i].color = Color.Lerp(CloudLightColorKeySwitcher[i].color, DefaultCloudLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < FogLightColor.colorKeys.Length; i++)
                {
                    FogLightColorKeySwitcher[i].color = Color.Lerp(FogLightColorKeySwitcher[i].color, DefaultFogLightColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < AmbientEquatorLightColor.colorKeys.Length; i++)
                {
                    AmbientEquatorLightColorKeySwitcher[i].color = Color.Lerp(AmbientEquatorLightColorKeySwitcher[i].color, DefaultAmbientEquatorLightBaseColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < AmbientGroundLightColor.colorKeys.Length; i++)
                {
                    AmbientGroundLightColorKeySwitcher[i].color = Color.Lerp(AmbientGroundLightColorKeySwitcher[i].color, DefaultAmbientGroundLightBaseColor.colorKeys[i].color, 1);
                }

                for (int i = 0; i < SunColor.colorKeys.Length; i++)
                {
                    SunLightColorKeySwitcher[i].color = Color.Lerp(SunLightColorKeySwitcher[i].color, DefaultSunLightBaseColor.colorKeys[i].color, 1);
                }

                FogLightColor.SetKeys(FogLightColorKeySwitcher, FogLightColor.alphaKeys);
                CloudLightColor.SetKeys(CloudLightColorKeySwitcher, CloudLightColor.alphaKeys);
                FogColor.SetKeys(FogColorKeySwitcher, FogColor.alphaKeys);
                CloudBaseColor.SetKeys(CloudColorKeySwitcher, CloudBaseColor.alphaKeys);
                AmbientSkyLightColor.SetKeys(AmbientSkyLightColorKeySwitcher, AmbientSkyLightColor.alphaKeys);
                AmbientEquatorLightColor.SetKeys(AmbientEquatorLightColorKeySwitcher, AmbientEquatorLightColor.alphaKeys);
                AmbientGroundLightColor.SetKeys(AmbientGroundLightColorKeySwitcher, AmbientGroundLightColor.alphaKeys);
                SunColor.SetKeys(SunLightColorKeySwitcher, SunColor.alphaKeys);
            }

            foreach (AudioSource A in WeatherSoundsList)
            {
                A.volume = 0;
            }

            if (CurrentWeatherType.UseWeatherSound == WeatherType.Yes_No.Yes)
            {
                foreach (AudioSource A in WeatherSoundsList)
                {
                    if (A.gameObject.name == CurrentWeatherType.WeatherTypeName + " (UniStorm)")
                    {
                        A.Play();
                        A.volume = CurrentWeatherType.WeatherVolume;
                    }
                }
            }
        }

        //If follow player is enabled, adjust the distant UniStorm components to the player's position
        void FollowPlayer()
        {
            m_MoonLight.transform.position = PlayerTransform.position;
            m_SunLight.transform.position = PlayerTransform.position;
        }

        //Calculate our precipitation odds based on the UniStorm date
        void CalculatePrecipiation()
        {
            CalculateMonths(); //Calculate our months
            GetDate(); //Calculate our UniStorm date

            //This algorithm uses an Animation curve to calculate the precipitation odds given the date from the Animation Curve
            m_roundingCorrection = UniStormDate.DayOfYear * 0.00273972602f;
            m_PreciseCurveTime = ((UniStormDate.DayOfYear / 28.07692307692308f)) + 1 - m_roundingCorrection;
            m_PreciseCurveTime = Mathf.Round(m_PreciseCurveTime * 10f) / 10f;

            m_CurrentPrecipitationAmountFloat = PrecipitationGraph.Evaluate(m_PreciseCurveTime);
            m_CurrentPrecipitationAmountInt = (int)Mathf.Round(m_CurrentPrecipitationAmountFloat);
            m_PrecipitationOdds = m_CurrentPrecipitationAmountInt;
        }

        void CreateUniStormFog ()
        {
            if (FogType == FogTypeEnum.UnistormFog)
            {
                m_UniStormAtmosphericFog = GetAtmosphericFogFeatureSettings();
                m_UniStormAtmosphericFog.SunSource = m_SunLight;
                m_UniStormAtmosphericFog.MoonSource = m_MoonLight;
                m_UniStormAtmosphericFog.MoonColor = MoonlightColor;
                m_CloudDomeMaterial.SetColor("_MoonColor", MoonlightColor);
                m_CloudDomeMaterial.SetFloat("_UseUniStormFog", 1);
                m_CloudFadeLevelStart = 0;
                m_CloudFadeLevelEnd = 0.18f;

                //Enable dithering on both UniStorm's clouds and Fog 
                if (UseDithering == EnableFeature.Enabled)
                {
                    m_UniStormAtmosphericFog.Dither = UniStormAtmosphericFogFeature.DitheringControl.Enabled;
                    m_UniStormAtmosphericFog.NoiseTexture = (Texture2D)Resources.Load("Clouds/baseNoise") as Texture2D;

                    m_CloudDomeMaterial.SetFloat("_EnableDithering", 1);
                    m_CloudDomeMaterial.SetTexture("_NoiseTex", (Texture2D)Resources.Load("Clouds/baseNoise") as Texture2D);
                }

                Invoke("CheckSinglePass", 0.05f);
            }
        }

        private UniStormAtmosphericFogFeature.Settings GetAtmosphericFogFeatureSettings()
        {
            if(urpForwardRendererData != null)
            {
                UniStormAtmosphericFogFeature uniStormAtmosphericFogFeature = urpForwardRendererData.rendererFeatures.OfType<UniStormAtmosphericFogFeature>().FirstOrDefault();
                if(uniStormAtmosphericFogFeature != null)
                {
                    return uniStormAtmosphericFogFeature.settings;
                }
            }
            return new UniStormAtmosphericFogFeature.Settings();
        }

        private UniStormSunShaftsFeature.Settings GetSunShaftsFeatureSettingsForSun()
        {
            if (urpForwardRendererData != null)
            {
                UniStormSunShaftsFeature uniStormSunShaftsFeature = urpForwardRendererData.rendererFeatures.OfType<UniStormSunShaftsFeature>().FirstOrDefault(x=>x.name.StartsWith("Sun", System.StringComparison.OrdinalIgnoreCase));
                if (uniStormSunShaftsFeature != null)
                {
                    return uniStormSunShaftsFeature.settings;
                }
            }
            return new UniStormSunShaftsFeature.Settings();
        }

        private UniStormSunShaftsFeature.Settings GetSunShaftsFeatureSettingsForMoon()
        {
            if (urpForwardRendererData != null)
            {
                UniStormSunShaftsFeature uniStormSunShaftsFeature = urpForwardRendererData.rendererFeatures.OfType<UniStormSunShaftsFeature>().FirstOrDefault(x => x.name.StartsWith("Moon", System.StringComparison.OrdinalIgnoreCase));
                if (uniStormSunShaftsFeature != null)
                {
                    return uniStormSunShaftsFeature.settings;
                }
            }
            return new UniStormSunShaftsFeature.Settings();
        }

        void CheckSinglePass()
        {
            ////Enable Single Pass support for UniStorm Fog, given that the VR settings are enabled.
            //if (VRStateData.VREnabled && VRStateData.StereoRenderingMode == VRState.StereoRenderingModes.SinglePass)
            //m_UniStormAtmosphericFog.fogMaterial.SetFloat("_VRSinglePassEnabled", 1);
        }

        //Create and positioned UniStorm's moon
        void CreateMoon()
        {
            m_MoonLight = GameObject.Find("UniStorm Moon").GetComponent<Light>();
            m_MoonLight.transform.localEulerAngles = new Vector3(-180, MoonAngle, 0);
            GameObject m_CreatedMoon = Instantiate((GameObject)Resources.Load("UniStorm Moon Object") as GameObject, transform.position, Quaternion.identity);
            m_CreatedMoon.name = "UniStorm Moon Object";
            m_MoonRenderer = GameObject.Find("UniStorm Moon Object").GetComponent<Renderer>();
            m_MoonTransform = m_MoonRenderer.transform;
            m_MoonPhaseMaterial = m_MoonRenderer.material;
            m_MoonPhaseMaterial.SetColor("_MoonColor", MoonPhaseColor);
            m_MoonTransform.parent = m_MoonLight.transform;

            m_MoonLight.shadowResolution = MoonShadowResolution;
            m_MoonLight.shadows = MoonShadowType;
            m_MoonLight.shadowStrength = MoonShadowStrength;

            if (PlayerCamera.farClipPlane < 2000)
            {
                m_MoonTransform.localPosition = new Vector3(0, 0, PlayerCamera.farClipPlane * -1);
                m_MoonTransform.localEulerAngles = new Vector3(270, 0, 0);
                float RecalculatedMoonSize = (PlayerCamera.farClipPlane) / 2000;
                m_MoonStartingSize = m_MoonTransform.localScale * RecalculatedMoonSize;
                m_MoonTransform.localScale = new Vector3(m_MoonTransform.localScale.x, m_MoonTransform.localScale.y, m_MoonTransform.localScale.z);
            }
            else
            {
                m_MoonTransform.localPosition = new Vector3(0, 0, -2000);
                m_MoonTransform.localEulerAngles = new Vector3(270, 0, 0);
                m_MoonStartingSize = m_MoonTransform.localScale;
                m_MoonTransform.localScale = new Vector3(m_MoonTransform.localScale.x, m_MoonTransform.localScale.y, m_MoonTransform.localScale.z);
            }

            //if (MoonShaftsEffect == EnableFeature.Enabled)
            //{
                CreateMoonShafts();
            //}
        }

        //Sets up UniStorm's sun
        void CreateSun()
        {
            m_SunLight = GameObject.Find("UniStorm Sun").GetComponent<Light>();
            m_SunLight.transform.localEulerAngles = new Vector3(0, SunAngle, 0);
            m_CelestialAxisTransform = GameObject.Find("Celestial Axis").transform;
            RenderSettings.sun = m_SunLight;
            m_SkyBoxMaterial = RenderSettings.skybox;
            m_SunLight.shadowResolution = SunShadowResolution;
            m_SunLight.shadows = SunShadowType;
            m_SunLight.shadowStrength = SunShadowStrength;

            SunObject = Instantiate((GameObject)Resources.Load("UniStorm Sun Object") as GameObject, transform.position, Quaternion.identity);
            SunObject.name = "UniStorm Sun Object";
            SunObjectMaterial = SunObject.GetComponent<Renderer>().material;
            m_SunRenderer = GameObject.Find("UniStorm Sun Object").GetComponent<Renderer>();
            m_SunTransform = m_SunRenderer.transform;
            m_SunTransform.parent = m_SunLight.transform;

            if (PlayerCamera.farClipPlane < 2000)
            {
                m_SunTransform.localPosition = new Vector3(0, 0, PlayerCamera.farClipPlane * -1);
                m_SunTransform.localEulerAngles = new Vector3(270, 0, 0);
                float RecalculatedSunSize = (PlayerCamera.farClipPlane) / 2000;
            }
            else
            {
                m_SunTransform.localPosition = new Vector3(0, 0, -2000);
                m_SunTransform.localEulerAngles = new Vector3(270, 0, 0);
            }

            //if (SunShaftsEffect == EnableFeature.Enabled)
            //{
                CreatSunShafts();
            //}
        }

        void CreatSunShafts()
        {
            m_SunShafts = GetSunShaftsFeatureSettingsForSun();
            m_SunShafts.isEnabled = SunShaftsEffect == EnableFeature.Enabled;
            m_SunShafts.useDepthTexture = true;
            m_SunShafts.maxRadius = 0.5f;
            m_SunShafts.sunShaftBlurRadius = 4.86f;
            m_SunShafts.radialBlurIterations = 2;
            m_SunShafts.sunShaftIntensity = 1;
            Color SunColor;
            ColorUtility.TryParseHtmlString("#C8A763", out SunColor);
            m_SunShafts.sunColor = SunColor;
            Color ThresholdColor;
            ColorUtility.TryParseHtmlString("#8E897B", out ThresholdColor);
            m_SunShafts.sunThreshold = ThresholdColor;
            m_SunShafts.celestialName = "UniStorm Sun";
        }

        void CreateMoonShafts()
        {
            m_MoonShafts = GetSunShaftsFeatureSettingsForMoon();
            m_MoonShafts.isEnabled = MoonShaftsEffect == EnableFeature.Enabled;
            m_MoonShafts.useDepthTexture = true;
            m_MoonShafts.maxRadius = 0.3f;
            m_MoonShafts.sunShaftBlurRadius = 3.32f;
            m_MoonShafts.radialBlurIterations = 3;
            m_MoonShafts.sunShaftIntensity = 1;
            GameObject MoonTransform = new GameObject("Moon Transform");
            MoonTransform.transform.SetParent(m_MoonLight.transform);
            MoonTransform.transform.localPosition = new Vector3(0,0,-20000);
            Color SunColor;
            ColorUtility.TryParseHtmlString("#515252FF", out SunColor);
            m_MoonShafts.sunColor = SunColor;
            Color ThresholdColor;
            ColorUtility.TryParseHtmlString("#222222FF", out ThresholdColor);
            m_MoonShafts.sunThreshold = ThresholdColor;
            m_MoonShafts.celestialName = "UniStorm Moon";
        }

        //Create, setup, and assign all needed lightning components
        void CreateLightning()
        {
            GameObject CreatedLightningSystem = new GameObject("UniStorm Lightning System");
            CreatedLightningSystem.AddComponent<LightningSystem>();
            m_UniStormLightningSystem = CreatedLightningSystem.GetComponent<LightningSystem>();
            m_UniStormLightningSystem.transform.SetParent(this.transform);

            for (int i = 0; i < ThunderSounds.Count; i++)
            {
                m_UniStormLightningSystem.ThunderSounds.Add(ThunderSounds[i]);
            }

            GameObject CreatedLightningLight = new GameObject("UniStorm Lightning Light");
            CreatedLightningLight.AddComponent<Light>();
            m_LightningLight = CreatedLightningLight.GetComponent<Light>();
            m_LightningLight.type = LightType.Directional;
            m_LightningLight.transform.SetParent(this.transform);
            m_LightningLight.transform.localPosition = Vector3.zero;
            m_LightningLight.intensity = 0;
            m_LightningLight.shadowResolution = LightningShadowResolution;
            m_LightningLight.shadows = LightningShadowType;
            m_LightningLight.shadowStrength = LightningShadowStrength;
            m_LightningLight.color = LightningLightColor;
            m_UniStormLightningSystem.LightningLightSource = m_LightningLight;
            m_UniStormLightningSystem.PlayerTransform = PlayerTransform;
            m_UniStormLightningSystem.LightningGenerationDistance = LightningGenerationDistance;
            m_LightningSeconds = Random.Range(LightningSecondsMin, LightningSecondsMax);
            m_UniStormLightningSystem.LightningLightIntensityMin = LightningLightIntensityMin;
            m_UniStormLightningSystem.LightningLightIntensityMax = LightningLightIntensityMax;
        }

        //A public function for UniStorm's UI Menu to change the weather with a dropdown
        public void ChangeWeatherUI()
        {
            CurrentWeatherType = AllWeatherTypes[WeatherDropdown.value];
            TransitionWeather();
        }

        //If enabled, create our UniStorm UI and Canvas.
        void CreateUniStormMenu()
        {
            //Resource load UI here
            UniStormCanvas = Instantiate((GameObject)Resources.Load("UniStorm Canvas") as GameObject, transform.position, Quaternion.identity);
            UniStormCanvas.name = "UniStorm Canvas";

            TimeSlider = GameObject.Find("Time Slider").GetComponent<Slider>();
            TimeSliderGameObject = TimeSlider.gameObject;
            TimeSlider.onValueChanged.AddListener(delegate { CalculateTimeSlider(); }); //Create an event to control UniStorm's time with a slider
            OnHourChangeEvent.AddListener(delegate { UpdateTimeSlider(); }); 
            TimeSlider.maxValue = 0.995f;

            WeatherButtonGameObject = GameObject.Find("Change Weather Button");

            WeatherDropdown = GameObject.Find("Weather Dropdown").GetComponent<Dropdown>();
            GameObject.Find("Change Weather Button").GetComponent<Button>().onClick.AddListener(delegate { ChangeWeatherUI(); });

            List<string> m_DropOptions = new List<string> { };

            for (int i = 0; i < AllWeatherTypes.Count; i++)
            {
                m_DropOptions.Add(AllWeatherTypes[i].WeatherTypeName);
            }

            WeatherDropdown.AddOptions(m_DropOptions);
            TimeSlider.value = m_TimeFloat;

            WeatherDropdown.value = AllWeatherTypes.IndexOf(CurrentWeatherType);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject m_EventSystem = new GameObject();
                m_EventSystem.name = "EventSystem";
                m_EventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                m_EventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            m_MenuToggle = false;
            ToggleUniStormMenu();
        }

        //Gets a custom DateTime using UniStorm's current date
        public System.DateTime GetDate()
        {
            if (RealWorldTime == EnableFeature.Disabled)
            {
                UniStormDate = new System.DateTime(Year, Month, Day, Hour, Minute, 0);
            }
            else if (RealWorldTime == EnableFeature.Enabled)
            {
                UniStormDate = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, Hour, Minute, 0);
                Year = UniStormDate.Year;
                Month = UniStormDate.Month;
                Day = UniStormDate.Day;
            }

            return UniStormDate;
        }

        //Move our sun according to the time of day
        public void MoveSun()
        {
            if (UseTimeOfDayUpdateControl == UseTimeOfDayUpdateSeconds.Yes)
            {
                TimeOfDayUpdateTimer += Time.deltaTime;
                if (TimeOfDayUpdateTimer >= TimeOfDayUpdateSeconds)
                {
                    m_CelestialAxisTransform.eulerAngles = new Vector3(m_TimeFloat * 360 - 100, SunRevolution, 180);

                    if (CloudShadows == EnableFeature.Enabled)
                    {
                        m_CloudShadows.ShadowDirection = m_SunLight.transform.forward;
                    }

                    TimeOfDayUpdateTimer = 0;
                }
            }
            else if (UseTimeOfDayUpdateControl == UseTimeOfDayUpdateSeconds.No)
            {
                m_CelestialAxisTransform.eulerAngles = new Vector3(m_TimeFloat * 360 - 100, SunRevolution, 180);

                if (CloudShadows == EnableFeature.Enabled)
                {
                    m_CloudShadows.ShadowDirection = m_SunLight.transform.forward;
                }
            }
        }

        void UpdateCelestialLightShafts ()
        {
            if (SunShaftsEffect == EnableFeature.Enabled)
            {
                if (m_SunLight.intensity <= 0)
                {
                    m_SunShafts.isEnabled = false;
                }
                else
                {
                    m_SunShafts.isEnabled = true;
                }
            }

            if (MoonShaftsEffect == EnableFeature.Enabled)
            {
                if (m_MoonLight.intensity <= 0)
                {
                    m_MoonShafts.isEnabled = false;
                }
                else
                {
                    m_MoonShafts.isEnabled = true;
                }
            }
        }

        public void ToggleUniStormMenu()
        {
            WeatherButtonGameObject.SetActive(m_MenuToggle);
            TimeSliderGameObject.SetActive(m_MenuToggle);
            WeatherDropdown.gameObject.SetActive(m_MenuToggle);
            m_MenuToggle = !m_MenuToggle;
        }

        void Update()
        {
            //Only run UniStorm if it has been initialized.
            if (UniStormInitialized)
            {
                //Only allow runtime editing of Customize Quality settings if enabled.
                if (CustomizeQuality == CustomizeQualityEnum.Yes && UpdateMarchStepsDuringRuntime == EnableFeature.Enabled && CloudQuality == CloudQualityEnum.Ultra)
                {
                    m_CloudDomeMaterial.SetFloat("_DistantCloudUpdateSpeed", ConvergenceSpeed);
                    Shader.SetGlobalFloat("CLOUD_MARCH_STEPS", NearMarchSteps);
                    Shader.SetGlobalFloat("DISTANT_CLOUD_MARCH_STEPS", DistantMarchSteps);
                    //m_UniStormClouds.numRendersPerFrame = RendersPerFrame;
                }

                if (UseUniStormMenu == EnableFeature.Enabled)
                {
                    //Some versions of Unity cannot have the Canvas disabled without causing issues with dropdown menus.
                    //So, disable the button and slider gameobjects then move the dropdown menu up 300 units so it is no longer visible. 
                    //Revese everything when the menu is enabled again.
                    if (Input.GetKeyDown(UniStormMenuKey))
                    {
                        ToggleUniStormMenu();
                    }
                }

                //Only calculate our time if TimeFlow is enabled
                if (TimeFlow == UniStormSystem.EnableFeature.Enabled)
                {
                    if (RealWorldTime == UniStormSystem.EnableFeature.Disabled)
                    {
                        if (Hour > 6 && Hour <= 18)
                        {
                            m_TimeFloat = m_TimeFloat + Time.deltaTime / DayLength / 120;
                        }

                        if (Hour > 18 || Hour <= 6)
                        {
                            m_TimeFloat = m_TimeFloat + Time.deltaTime / NightLength / 120;
                        }
                    }
                    else if (RealWorldTime == UniStormSystem.EnableFeature.Enabled)
                    {
                        m_TimeFloat = (float)System.DateTime.Now.Hour / 24 + (float)System.DateTime.Now.Minute / 1440;
                    }

                    if (m_TimeFloat >= 1.0f)
                    {
                        m_TimeFloat = 0;
                        CalculateDays();
                    }
                }

                //Calculate our time
                float m_HourFloat = m_TimeFloat * 24;
                Hour = (int)m_HourFloat;
                float m_MinuteFloat = m_HourFloat * 60;
                Minute = (int)m_MinuteFloat % 60;

                //Update all hourly related settings
                if (m_LastHour != Hour)
                {
                    m_LastHour = Hour;
                    HourlyUpdate();
                }

                MoveSun();
                UpdateColors();
                PlayTimeOfDaySound();
                PlayTimeOfDayMusic();
                CalculateTimeOfDay();

                //Generate our lightning, if the randomized lightning seconds have been met
                if (CurrentWeatherType.UseLightning == WeatherType.Yes_No.Yes)
                {
                    if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyCloudy || CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Cloudy)
                    {
                        m_LightningTimer += Time.deltaTime;

                        //Only create a lightning strike if the clouds have fully faded in
                        if (m_LightningTimer >= m_LightningSeconds && m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.5f)
                        {
                            m_UniStormLightningSystem.LightningCurve = LightningFlashPatterns[Random.Range(0, LightningFlashPatterns.Count)];
                            m_UniStormLightningSystem.GenerateLightning();
                            m_LightningSeconds = Random.Range(LightningSecondsMin, LightningSecondsMax);
                            m_LightningTimer = 0;
                        }
                    }
                }

                //Update our FollowPlayer function
                FollowPlayer();
            }
            else if (GetPlayerAtRuntime == UniStormSystem.EnableFeature.Enabled && !UniStormInitialized)
            {
                //Continue to look for our player until it's found. Once it is, UniStorm can be initialized.
                try
                {
                    PlayerTransform = GameObject.FindWithTag(PlayerTag).transform;
                    m_PlayerFound = true;
                }
                catch
                {
                    m_PlayerFound = false;
                }

            }
        }

        //Generate and return a random cloud intensity based on the current weather type cloud level
        float GetCloudLevel(bool InstantFade)
        {
            Random.InitState(System.DateTime.Now.Millisecond); //Initialize Random.Range with a random seed 
            float GeneratedCloudLevel = 0;

            if (MostlyCloudyCoroutine != null) { StopCoroutine(MostlyCloudyCoroutine); }
            if (CloudTallnessCoroutine != null) { StopCoroutine(CloudTallnessCoroutine); }
            
            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Clear)
            {
                GeneratedCloudLevel = 0.36f;
                if (!InstantFade)
                {
                    MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(10 * TransitionSpeed, 1, true));
                    CloudTallnessCoroutine = StartCoroutine(CloudTallnessSequence(5 * TransitionSpeed, 800));
                }
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyClear)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    GeneratedCloudLevel = Random.Range(0.35f, 0.39f);
                else
                    GeneratedCloudLevel = Random.Range(0.41f, 0.44f);

                if (!InstantFade)
                {
                    MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(10 * TransitionSpeed, 1, true));
                    CloudTallnessCoroutine = StartCoroutine(CloudTallnessSequence(5 * TransitionSpeed, 800));
                }
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.PartyCloudy)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    GeneratedCloudLevel = Random.Range(0.43f, 0.47f);
                else
                    GeneratedCloudLevel = Random.Range(0.45f, 0.48f);

                if (!InstantFade)
                {
                    MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(10 * TransitionSpeed, 1, true));
                    CloudTallnessCoroutine = StartCoroutine(CloudTallnessSequence(5 * TransitionSpeed, 1000));
                }
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.MostlyCloudy)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    GeneratedCloudLevel = Random.Range(0.5f, 0.55f);
                else
                    GeneratedCloudLevel = Random.Range(0.49f, 0.52f);

                if (!InstantFade)
                {
                    MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(10 * TransitionSpeed, 1, false));
                    CloudTallnessCoroutine = StartCoroutine(CloudTallnessSequence(5 * TransitionSpeed, 1000));
                }
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Cloudy)
            {
                if (!InstantFade)
                {
                    CloudTallnessCoroutine = StartCoroutine(CloudTallnessSequence(5 * TransitionSpeed, 1000));
                }

                if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                {
                    if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                        GeneratedCloudLevel = 0.6f;
                    else
                        GeneratedCloudLevel = Random.Range(0.53f, 0.55f);

                    if (!InstantFade)
                    {
                        MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(10 * TransitionSpeed, 1, false));
                    }
                }
                else if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                {
                    if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                        GeneratedCloudLevel = 0.6f;
                    else
                        GeneratedCloudLevel = Random.Range(0.53f, 0.55f);

                    if (!InstantFade)
                    {
                        MostlyCloudyCoroutine = StartCoroutine(MostlyCloudyAdjustment(5 * TransitionSpeed, 1, true));
                    }
                }
            }
            else if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.DontChange)
            {
                GeneratedCloudLevel = m_CloudDomeMaterial.GetFloat("_uCloudsCoverage");
            }

            float RoundedCloudLevel = (float)Mathf.Round(GeneratedCloudLevel * 1000f) / 1000f;
            return RoundedCloudLevel;
        }

        //Used for controlling UniStorm's time slider
        public void CalculateTimeSlider()
        {
            m_TimeFloat = TimeSlider.value;
            TimeOfDayUpdateTimer = TimeOfDayUpdateSeconds;
        }

        public void UpdateTimeSlider()
        {
            TimeSlider.value = m_TimeFloat;
        }

        //Calculate all of our hourly updates
        void HourlyUpdate()
        {
            OnHourChangeEvent.Invoke();

            MoveSun();

            if (CloudRenderType == CloudRenderTypeEnum.Transparent)
            {
                if (Hour <= 5 || Hour >= 19)
                {
                    m_CloudDomeMaterial.SetFloat("_MaskMoon", 1);
                }
                if (Hour >= 6 && Hour < 19)
                {
                    m_CloudDomeMaterial.SetFloat("_MaskMoon", 0);
                }
            }

            UpdateCelestialLightShafts();

            Temperature = (int)TemperatureCurve.Evaluate(m_PreciseCurveTime) + (int)TemperatureFluctuation.Evaluate((float)Hour);

            if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Hourly)
            {
                if (Hour < 23)
                {
                    CurrentWeatherType = WeatherForecast[Hour];
                    NextWeatherType = WeatherForecast[Hour + 1];
                }
                else
                {
                    CurrentWeatherType = WeatherForecast[Hour];
                    NextWeatherType = WeatherForecast[0];
                }
            }

            CheckWeather();

            //If the hour is equal to 12, update our moon phase.
            if (Hour == 12)
            {
                MoonPhaseIndex++;
                CalculateMoonPhase();
            }

            if (CurrentWeatherType.UseAuroras == WeatherType.Yes_No.Yes)
            {
                if (Hour >= 20 && Hour <= 23 || Hour >= 0 && Hour <= 5)
                {
                    if (AuroraCoroutine != null) { StopCoroutine(AuroraCoroutine); }
                    AuroraCoroutine = StartCoroutine(AuroraShaderFadeSequence(3, CurrentWeatherType.AuroraIntensity, CurrentWeatherType.AuroraInnerColor, CurrentWeatherType.AuroraOuterColor));
                }
                else if (Hour >= 6 && Hour <= 7)
                {
                    if (AuroraCoroutine != null) { StopCoroutine(AuroraCoroutine); }
                    AuroraCoroutine = StartCoroutine(AuroraShaderFadeSequence(3, 0, CurrentWeatherType.AuroraInnerColor, CurrentWeatherType.AuroraOuterColor));
                }
            }
        }

        void CalculateTimeOfDay()
        {
            if (Hour >= 6 && Hour <= 7)
            {
                if (CurrentTimeOfDay != CurrentTimeOfDayEnum.Morning)
                {
                    m_UpdateTimeOfDayMusic = true;
                }
                CurrentTimeOfDay = CurrentTimeOfDayEnum.Morning;               
            }
            else if (Hour >= 8 && Hour <= 16)
            {
                if (CurrentTimeOfDay != CurrentTimeOfDayEnum.Day)
                {
                    m_UpdateTimeOfDayMusic = true;
                }
                CurrentTimeOfDay = CurrentTimeOfDayEnum.Day;
            }
            else if (Hour >= 17 && Hour <= 18)
            {
                if (CurrentTimeOfDay != CurrentTimeOfDayEnum.Evening)
                {
                    m_UpdateTimeOfDayMusic = true;
                }
                CurrentTimeOfDay = CurrentTimeOfDayEnum.Evening;
            }
            else if (Hour >= 19 && Hour <= 23 || Hour >= 0 && Hour <= 5)
            {
                if (CurrentTimeOfDay != CurrentTimeOfDayEnum.Night)
                {
                    m_UpdateTimeOfDayMusic = true;
                }
                CurrentTimeOfDay = CurrentTimeOfDayEnum.Night;
            }
        }

        //Calculate our seasons based on either the Norhtern or Southern Hemisphere
        public void CalculateSeason()
        {
            if (Month == 3 && Day >= 20 || Month == 4 || Month == 5 || Month == 6 && Day <= 20)
            {
                if (Hemisphere == HemisphereEnum.Northern)
                {
                    CurrentSeason = CurrentSeasonEnum.Spring;
                }
                else if (Hemisphere == HemisphereEnum.Southern)
                {
                    CurrentSeason = CurrentSeasonEnum.Fall;
                }
            }
            else if (Month == 6 && Day >= 21 || Month == 7 || Month == 8 || Month == 9 && Day <= 21)
            {
                if (Hemisphere == HemisphereEnum.Northern)
                {
                    CurrentSeason = CurrentSeasonEnum.Summer;
                }
                else if (Hemisphere == HemisphereEnum.Southern)
                {
                    CurrentSeason = CurrentSeasonEnum.Winter;
                }
            }
            else if (Month == 9 && Day >= 22 || Month == 10 || Month == 11 || Month == 12 && Day <= 20)
            {
                if (Hemisphere == HemisphereEnum.Northern)
                {
                    CurrentSeason = CurrentSeasonEnum.Fall;
                }
                else if (Hemisphere == HemisphereEnum.Southern)
                {
                    CurrentSeason = CurrentSeasonEnum.Spring;
                }
            }
            else if (Month == 12 && Day >= 21 || Month == 1 || Month == 2 || Month == 3 && Day <= 19)
            {
                if (Hemisphere == HemisphereEnum.Northern)
                {
                    CurrentSeason = CurrentSeasonEnum.Winter;
                }
                else if (Hemisphere == HemisphereEnum.Southern)
                {
                    CurrentSeason = CurrentSeasonEnum.Summer;
                }
            }
        }

        //Calculates our time of day sounds according to the hour and randomized seconds set by the user.
        void PlayTimeOfDaySound()
        {
            m_TimeOfDaySoundsTimer += Time.deltaTime;

            if (m_TimeOfDaySoundsTimer >= m_TimeOfDaySoundsSeconds + m_CurrentClipLength)
            {
                if (CurrentWeatherType != null && CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes &&
                    TimeOfDaySoundsDuringPrecipitationWeather == UniStormSystem.EnableFeature.Enabled ||
                    CurrentWeatherType != null && CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No &&
                    TimeOfDaySoundsDuringPrecipitationWeather == UniStormSystem.EnableFeature.Disabled)
                {
                    if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Morning)
                    {
                        //Morning Sounds
                        if (MorningSounds.Count != 0)
                        {
                            TimeOfDayAudioSource.clip = MorningSounds[Random.Range(0, MorningSounds.Count)];
                            if (TimeOfDayAudioSource.clip != null)
                            {
                                TimeOfDayAudioSource.Play();
                                m_CurrentClipLength = TimeOfDayAudioSource.clip.length;
                            }
                        }
                    }
                    else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Day)
                    {
                        //Day Sounds
                        if (DaySounds.Count != 0)
                        {
                            TimeOfDayAudioSource.clip = DaySounds[Random.Range(0, DaySounds.Count)];
                            if (TimeOfDayAudioSource.clip != null)
                            {
                                TimeOfDayAudioSource.Play();
                                m_CurrentClipLength = TimeOfDayAudioSource.clip.length;
                            }
                        }
                    }
                    else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Evening)
                    {
                        //Evening Sounds
                        if (EveningSounds.Count != 0)
                        {
                            TimeOfDayAudioSource.clip = EveningSounds[Random.Range(0, EveningSounds.Count)];
                            if (TimeOfDayAudioSource.clip != null)
                            {
                                TimeOfDayAudioSource.Play();
                                m_CurrentClipLength = TimeOfDayAudioSource.clip.length;
                            }
                        }
                    }
                    else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Night)
                    {
                        //Night Sounds
                        if (NightSounds.Count != 0)
                        {
                            TimeOfDayAudioSource.clip = NightSounds[Random.Range(0, NightSounds.Count)];
                            if (TimeOfDayAudioSource.clip != null)
                            {
                                TimeOfDayAudioSource.Play();
                                m_CurrentClipLength = TimeOfDayAudioSource.clip.length;
                            }
                        }
                    }

                    m_TimeOfDaySoundsTimer = 0;
                }
            }
        }

        //Calculates our time of day sounds according to the hour and randomized seconds set by the user.
        void PlayTimeOfDayMusic()
        {
            m_TimeOfDayMusicTimer += Time.deltaTime;

            if (m_TimeOfDayMusicTimer >= m_CurrentMusicClipLength + TimeOfDayMusicDelay || m_UpdateTimeOfDayMusic && TransitionMusicOnTimeOfDayChange == EnableFeature.Enabled || m_UpdateBiomeTimeOfDayMusic)
            {
                if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Morning)
                {
                    //Morning Music
                    if (MorningMusic.Count != 0)
                    {
                        if (MusicVolumeCoroutine != null) { StopCoroutine(MusicVolumeCoroutine); }
                        AudioClip RandomMorningSound = MorningMusic[Random.Range(0, MorningMusic.Count)];
                        if (RandomMorningSound != null)
                        {
                            MusicVolumeCoroutine = StartCoroutine(MusicFadeSequence(MusicTransitionLength, RandomMorningSound));
                            m_CurrentMusicClipLength = RandomMorningSound.length;
                        }
                    }
                }
                else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Day)
                {
                    //Day Music
                    if (DayMusic.Count != 0)
                    {
                        if (MusicVolumeCoroutine != null) { StopCoroutine(MusicVolumeCoroutine); }
                        AudioClip RandomDaySound = DayMusic[Random.Range(0, DayMusic.Count)];
                        if (RandomDaySound != null)
                        {
                            MusicVolumeCoroutine = StartCoroutine(MusicFadeSequence(MusicTransitionLength, RandomDaySound));
                            m_CurrentMusicClipLength = RandomDaySound.length;
                        }
                    }
                }
                else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Evening)
                {
                    //Evening Music
                    if (EveningMusic.Count != 0)
                    {
                        if (MusicVolumeCoroutine != null) { StopCoroutine(MusicVolumeCoroutine); }
                        AudioClip RandomEveningSound = EveningMusic[Random.Range(0, EveningMusic.Count)];
                        if (RandomEveningSound != null)
                        {
                            MusicVolumeCoroutine = StartCoroutine(MusicFadeSequence(MusicTransitionLength, RandomEveningSound));
                            m_CurrentMusicClipLength = RandomEveningSound.length;
                        }
                    }
                }
                else if (CurrentTimeOfDay == CurrentTimeOfDayEnum.Night)
                {
                    //Night Music
                    if (NightMusic.Count != 0)
                    {
                        if (MusicVolumeCoroutine != null) { StopCoroutine(MusicVolumeCoroutine); }
                        AudioClip RandomNightSound = NightMusic[Random.Range(0, NightMusic.Count)];
                        if (RandomNightSound != null)
                        {
                            MusicVolumeCoroutine = StartCoroutine(MusicFadeSequence(MusicTransitionLength, RandomNightSound));
                            m_CurrentMusicClipLength = RandomNightSound.length;
                        }
                    }
                }

                m_TimeOfDayMusicTimer = 0;
                m_UpdateTimeOfDayMusic = false;
                m_UpdateBiomeTimeOfDayMusic = false;
            }
        }

        //Check our generated weather to see if it's time to update the weather.
        //If it is, slowly transition the weather according to the current weather type scriptable object
        void CheckWeather()
        {
            if (m_WeatherGenerated && WeatherGeneration == EnableFeature.Enabled)
            {
                if (Hour == HourToChangeWeather || WeatherGenerationMethod == WeatherGenerationMethodEnum.Hourly)
                {
                    if (CurrentWeatherType != NextWeatherType)
                    {
                        if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Daily)
                        {
                            CurrentWeatherType = NextWeatherType;
                        }

                        TransitionWeather();
                    }

                    if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Daily)
                    {
                        GenerateWeather();
                    }
                }
            }
        }

        /// <summary>
        /// Changes UniStorm's weather according to the Weather parameter.
        /// </summary>
        public void ChangeWeather (WeatherType Weather)
        {
            CurrentWeatherType = Weather;
            TransitionWeather();
        }

        void TransitionWeather()
        {
            OnWeatherChangeEvent.Invoke(); //Invoke our weather change event
            if (CloudCoroutine != null) { StopCoroutine(CloudCoroutine); }
            if (FogCoroutine != null) { StopCoroutine(FogCoroutine); }
            if (WeatherEffectCoroutine != null) { StopCoroutine(WeatherEffectCoroutine); }
            if (AdditionalWeatherEffectCoroutine != null) { StopCoroutine(AdditionalWeatherEffectCoroutine); }
            if (ParticleFadeCoroutine != null) { StopCoroutine(ParticleFadeCoroutine); }
            if (AdditionalParticleFadeCoroutine != null) { StopCoroutine(AdditionalParticleFadeCoroutine); }
            if (SunCoroutine != null) { StopCoroutine(SunCoroutine); }
            if (MoonCoroutine != null) { StopCoroutine(MoonCoroutine); }
            if (SoundInCoroutine != null) { StopCoroutine(SoundInCoroutine); }
            if (SoundOutCoroutine != null) { StopCoroutine(SoundOutCoroutine); }
            if (ColorCoroutine != null) { StopCoroutine(ColorCoroutine); }
            if (SunColorCoroutine != null) { StopCoroutine(SunColorCoroutine); }
            if (CloudHeightCoroutine != null) { StopCoroutine(CloudHeightCoroutine); }
            if (WindCoroutine != null) { StopCoroutine(WindCoroutine); }
            if (RainShaderCoroutine != null) { StopCoroutine(RainShaderCoroutine); }
            if (SnowShaderCoroutine != null) { StopCoroutine(SnowShaderCoroutine); }
            if (StormyCloudsCoroutine != null) { StopCoroutine(StormyCloudsCoroutine); }
            if (CloudProfileCoroutine != null) { StopCoroutine(CloudProfileCoroutine); }
            if (CloudShadowIntensityCoroutine != null) { StopCoroutine(CloudShadowIntensityCoroutine); }
            if (SunAttenuationIntensityCoroutine != null) { StopCoroutine(SunAttenuationIntensityCoroutine); }
            if (AuroraCoroutine != null) { StopCoroutine(AuroraCoroutine); }
            if (AtmosphericFogCoroutine != null) { StopCoroutine(AtmosphericFogCoroutine); }
            if (FogLightFalloffCoroutine != null) { StopCoroutine(FogLightFalloffCoroutine); }
            if (SunHeightCoroutine != null) { StopCoroutine(SunHeightCoroutine); }

            //Reset our time of day sounds timer so it doesn't play right after a weather change
            m_TimeOfDaySoundsTimer = 0;

            //Get randomized cloud amount based on cloud level from weather type.
            if (CurrentWeatherType.CloudLevel != WeatherType.CloudLevelEnum.DontChange)
            {
                m_ReceivedCloudValue = GetCloudLevel(false);
            }

            //Clouds
            if (m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") < m_ReceivedCloudValue)
            {
                CloudCoroutine = StartCoroutine(CloudFadeSequence(10 * TransitionSpeed, m_ReceivedCloudValue, false));
            }
            else
            {
                CloudCoroutine = StartCoroutine(CloudFadeSequence(10 * TransitionSpeed, m_ReceivedCloudValue, true));
            }

            if (ForceLowClouds == EnableFeature.Disabled)
            {
                CloudHeightCoroutine = StartCoroutine(CloudHeightSequence(10 * TransitionSpeed, CurrentWeatherType.CloudHeight));
            }

            if (CloudType == CloudTypeEnum.Volumetric && CurrentWeatherType.CloudLevel != WeatherType.CloudLevelEnum.DontChange)
            {
                CloudProfile m_CP = CurrentWeatherType.CloudProfileComponent;

                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    CloudProfileCoroutine = StartCoroutine(CloudProfileSequence(10 * TransitionSpeed, m_CP.EdgeSoftness, m_CP.BaseSoftness, m_CP.DetailStrength, m_CP.Density, m_CP.CoverageBias, m_CP.DetailScale));
                else
                    CloudProfileCoroutine = StartCoroutine(CloudProfileSequence(10 * TransitionSpeed, m_CP.EdgeSoftness, m_CP.BaseSoftness, m_DetailStrength, m_CP.Density, 0.02f, m_CP.DetailScale));
            }

            //Cloud Shadows
            if (CurrentWeatherType.CloudLevel != WeatherType.CloudLevelEnum.DontChange && CloudShadows == EnableFeature.Enabled)
            {
                CloudShadowIntensityCoroutine = StartCoroutine(CloudShadowIntensitySequence(10 * TransitionSpeed, CurrentWeatherType.CloudShadowIntensity));
            }

            //Wind
            if (CurrentWindIntensity < CurrentWeatherType.WindIntensity)
            {
                WindCoroutine = StartCoroutine(WindFadeSequence(10 * TransitionSpeed, CurrentWeatherType.WindIntensity, false));
            }
            else
            {
                WindCoroutine = StartCoroutine(WindFadeSequence(10 * TransitionSpeed, CurrentWeatherType.WindIntensity, true));
            } 
            
            if (FogType == FogTypeEnum.UnistormFog)
            {
                FogLightFalloffCoroutine = StartCoroutine(FogLightFalloffSequence(10 * TransitionSpeed, CurrentWeatherType.FogLightFalloff));
            }

            //Fog
            if (RenderSettings.fogDensity < CurrentWeatherType.FogDensity)
            {
                FogCoroutine = StartCoroutine(FogFadeSequence(5 * TransitionSpeed, CurrentWeatherType.FogDensity, false));
            }
            else
            {
                FogCoroutine = StartCoroutine(FogFadeSequence(5 * TransitionSpeed, CurrentWeatherType.FogDensity, true));
            }

            //Auroras
            if (CurrentWeatherType.UseAuroras == WeatherType.Yes_No.Yes)
            {
                if (Hour >= 20 && Hour <= 23 || Hour >= 0 && Hour <= 5)
                {
                    AuroraCoroutine = StartCoroutine(AuroraShaderFadeSequence(5 * TransitionSpeed, CurrentWeatherType.AuroraIntensity, CurrentWeatherType.AuroraInnerColor, CurrentWeatherType.AuroraOuterColor));
                }
            }
            else
            {
                AuroraCoroutine = StartCoroutine(AuroraShaderFadeSequence(5 * TransitionSpeed, 0, CurrentWeatherType.AuroraInnerColor, CurrentWeatherType.AuroraOuterColor));
            }

            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                SunCoroutine = StartCoroutine(SunFadeSequence(10 * TransitionSpeed, CurrentWeatherType.SunIntensity, false));
                MoonCoroutine = StartCoroutine(MoonFadeSequence(10 * TransitionSpeed, CurrentWeatherType.MoonIntensity, false));
                ColorCoroutine = StartCoroutine(ColorFadeSequence(10 * TransitionSpeed, 1, CurrentWeatherType.FogColor, CurrentWeatherType.CloudColor));
                SunColorCoroutine = StartCoroutine(SunColorFadeSequence(10 * TransitionSpeed, 1));
                StormyCloudsCoroutine = StartCoroutine(StormyCloudsSequence(10 * TransitionSpeed, false));
                SunAttenuationIntensityCoroutine = StartCoroutine(SunAttenuationIntensitySequence(10 * TransitionSpeed, 0.35f));
                SunHeightCoroutine = StartCoroutine(SunHeightSequence(10 * TransitionSpeed, -600, -400));

                if (FogType == FogTypeEnum.UnistormFog)
                {
                    if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.No)
                    {
                        AtmosphericFogCoroutine = StartCoroutine(AtmosphericFogFadeSequence(10 * TransitionSpeed, 0.0f, (1 - CurrentWeatherType.FogHeight)));
                    }
                    else if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.Yes)
                    {
                        AtmosphericFogCoroutine = StartCoroutine(AtmosphericFogFadeSequence(10 * TransitionSpeed, ((1 - CurrentWeatherType.CameraFogHeight) / 10), (1 - CurrentWeatherType.FogHeight)));
                    }
                }

                if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.Rain)
                {
                    RainShaderCoroutine = StartCoroutine(RainShaderFadeSequence(20 * TransitionSpeed, 1, false));
                    SnowShaderCoroutine = StartCoroutine(SnowShaderFadeSequence(20 * TransitionSpeed, 0, true));
                }
                else if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.Snow)
                {
                    SnowShaderCoroutine = StartCoroutine(SnowShaderFadeSequence(20 * TransitionSpeed, 1, false));
                    RainShaderCoroutine = StartCoroutine(RainShaderFadeSequence(20 * TransitionSpeed, 0, true));
                }
                else if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.None)
                {
                    SnowShaderCoroutine = StartCoroutine(SnowShaderFadeSequence(20 * TransitionSpeed, 0, true));
                    RainShaderCoroutine = StartCoroutine(RainShaderFadeSequence(20 * TransitionSpeed, 0, true));
                }
            }
            else if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
            {
                SunCoroutine = StartCoroutine(SunFadeSequence(10 * TransitionSpeed, CurrentWeatherType.SunIntensity, false));
                MoonCoroutine = StartCoroutine(MoonFadeSequence(10 * TransitionSpeed, CurrentWeatherType.MoonIntensity, false));
                ColorCoroutine = StartCoroutine(ColorFadeSequence(30 * TransitionSpeed, 1, CurrentWeatherType.FogColor, CurrentWeatherType.CloudColor));
                SunColorCoroutine = StartCoroutine(SunColorFadeSequence(10 * TransitionSpeed, 1));
                StormyCloudsCoroutine = StartCoroutine(StormyCloudsSequence(10 * TransitionSpeed, true));

                //Reduce the sun's attenuation by 50% if Gamma Color Space is used to stop the clouds from being too bright.
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    SunAttenuationIntensityCoroutine = StartCoroutine(SunAttenuationIntensitySequence(10 * TransitionSpeed, 0.5f));
                }
                else
                {
                    SunAttenuationIntensityCoroutine = StartCoroutine(SunAttenuationIntensitySequence(10 * TransitionSpeed, 1));
                }

                SunHeightCoroutine = StartCoroutine(SunHeightSequence(10 * TransitionSpeed, -100, -70));

                if (FogType == FogTypeEnum.UnistormFog)
                {
                    if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.No)
                    {
                        AtmosphericFogCoroutine = StartCoroutine(AtmosphericFogFadeSequence(10 * TransitionSpeed, ((1 - CameraFogHeight) / 10), (1 - CurrentWeatherType.FogHeight)));
                    }
                    else if (CurrentWeatherType.OverrideCameraFogHeight == WeatherType.Yes_No.Yes)
                    {
                        AtmosphericFogCoroutine = StartCoroutine(AtmosphericFogFadeSequence(10 * TransitionSpeed, ((1 - CurrentWeatherType.CameraFogHeight) / 10), (1 - CurrentWeatherType.FogHeight)));
                    }

                }

                if (CurrentWeatherType.ShaderControl == WeatherType.ShaderControlEnum.None)
                {
                    SnowShaderCoroutine = StartCoroutine(SnowShaderFadeSequence(20 * TransitionSpeed, 0, true));
                    RainShaderCoroutine = StartCoroutine(RainShaderFadeSequence(20 * TransitionSpeed, 0, true));
                }
            }

            if (CurrentWeatherType.UseWeatherEffect == WeatherType.Yes_No.Yes)
            {
                ParticleSystem m_PreviousWeatherEffect = CurrentParticleSystem;

                for (int i = 0; i < WeatherEffectsList.Count; i++)
                {
                    if (WeatherEffectsList[i].name == CurrentWeatherType.WeatherEffect.name + " (UniStorm)")
                    {
                        CurrentParticleSystem = WeatherEffectsList[i];
                        CurrentParticleSystem.transform.localPosition = CurrentWeatherType.ParticleEffectVector;
                    }
                }

                if (CurrentParticleSystem.emission.rateOverTime.constant < CurrentWeatherType.ParticleEffectAmount)
                {
                    WeatherEffectCoroutine = StartCoroutine(ParticleFadeSequence(10 * TransitionSpeed, CurrentWeatherType.ParticleEffectAmount, null, false));
                }
                else
                {
                    if (m_PreviousWeatherEffect != CurrentParticleSystem)
                    {
                        ParticleFadeCoroutine = StartCoroutine(ParticleFadeSequence(5 * TransitionSpeed, 0, CurrentParticleSystem, true));
                    }
                    else
                    {
                        ParticleFadeCoroutine = StartCoroutine(ParticleFadeSequence(5 * TransitionSpeed, CurrentWeatherType.ParticleEffectAmount, CurrentParticleSystem, true));
                    }
                }
            }

            if (CurrentWeatherType.UseAdditionalWeatherEffect == WeatherType.Yes_No.Yes)
            {
                for (int i = 0; i < AdditionalWeatherEffectsList.Count; i++)
                {
                    if (AdditionalWeatherEffectsList[i].name == CurrentWeatherType.AdditionalWeatherEffect.name + " (UniStorm)")
                    {
                        AdditionalCurrentParticleSystem = AdditionalWeatherEffectsList[i];
                        AdditionalCurrentParticleSystem.transform.localPosition = CurrentWeatherType.AdditionalParticleEffectVector;
                    }
                }

                if (AdditionalCurrentParticleSystem.emission.rateOverTime.constant < CurrentWeatherType.AdditionalParticleEffectAmount)
                {
                    AdditionalWeatherEffectCoroutine = StartCoroutine(AdditionalParticleFadeSequence(10 * TransitionSpeed, CurrentWeatherType.AdditionalParticleEffectAmount, null, false));
                }
                else
                {
                    AdditionalParticleFadeCoroutine = StartCoroutine(AdditionalParticleFadeSequence(5 * TransitionSpeed, 0, AdditionalCurrentParticleSystem, true));
                }
            }

            if (CurrentWeatherType.UseWeatherSound == WeatherType.Yes_No.Yes)
            {
                foreach (AudioSource A in WeatherSoundsList)
                {
                    if (A.gameObject.name == CurrentWeatherType.WeatherTypeName + " (UniStorm)")
                    {
                        A.Play();
                        SoundInCoroutine = StartCoroutine(SoundFadeSequence(10 * TransitionSpeed, CurrentWeatherType.WeatherVolume, A, false));
                    }
                }
            }

            if (CurrentWeatherType.UseWeatherEffect == WeatherType.Yes_No.No)
            {
                CurrentParticleSystem = null;

                if (CurrentWeatherType.UseAdditionalWeatherEffect == WeatherType.Yes_No.No)
                {
                    AdditionalCurrentParticleSystem = null;
                }
            }

            foreach (ParticleSystem P in WeatherEffectsList)
            {
                if (P != CurrentParticleSystem && P.emission.rateOverTime.constant > 0 ||
                    CurrentWeatherType.UseWeatherEffect == WeatherType.Yes_No.No && P.emission.rateOverTime.constant > 0)
                {
                    ParticleFadeCoroutine = StartCoroutine(ParticleFadeSequence(5 * TransitionSpeed, 0, P, true));
                }
            }

            foreach (ParticleSystem P in AdditionalWeatherEffectsList)
            {
                if (P != AdditionalCurrentParticleSystem && P.emission.rateOverTime.constant > 0 ||
                    CurrentWeatherType.UseAdditionalWeatherEffect == WeatherType.Yes_No.No && P.emission.rateOverTime.constant > 0)
                {
                    AdditionalParticleFadeCoroutine = StartCoroutine(AdditionalParticleFadeSequence(5 * TransitionSpeed, 0, P, true));
                }
            }

            foreach (AudioSource A in WeatherSoundsList)
            {
                if (A.gameObject.name != CurrentWeatherType.WeatherTypeName + " (UniStorm)" && A.volume > 0 || CurrentWeatherType.UseWeatherSound == WeatherType.Yes_No.No && A.volume > 0)
                {
                    SoundOutCoroutine = StartCoroutine(SoundFadeSequence(5 * TransitionSpeed, 0, A, true));
                }
            }
        }

        //Calculates our moon phases. This is updated daily at exactly 12:00.
        void CalculateMoonPhase()
        {
            if (MoonPhaseList.Count > 0)
            {
                if (MoonPhaseIndex == MoonPhaseList.Count)
                {
                    MoonPhaseIndex = 0;
                }
                m_MoonPhaseMaterial.SetTexture("_MainTex", MoonPhaseList[MoonPhaseIndex].MoonPhaseTexture);
                m_MoonRenderer.material = m_MoonPhaseMaterial;
                m_MoonPhaseMaterial.SetFloat("_MoonBrightness", MoonBrightness);
                MoonPhaseIntensity = MoonPhaseList[MoonPhaseIndex].MoonPhaseIntensity;
                m_MoonPhaseMaterial.SetColor("_MoonColor", MoonPhaseColor);
            }
        }

        //Continuously update our colors based on the time of day
        void UpdateColors()
        {
            m_SunLight.color = SunColor.Evaluate(m_TimeFloat);
            m_MoonLight.color = MoonColor.Evaluate(m_TimeFloat);
            m_StarsMaterial.color = StarLightColor.Evaluate(m_TimeFloat);
            m_StarsRenderer.transform.position = PlayerTransform.position;
            m_StarsMaterial.SetVector("_uWorldSpaceCameraPos", PlayerCamera.transform.position);
            m_SkyBoxMaterial.SetColor("_SkyTint", SkyColor.Evaluate(m_TimeFloat));
            m_SkyBoxMaterial.SetFloat("_AtmosphereThickness", AtmosphereThickness.Evaluate(m_TimeFloat * 24));
            m_SkyBoxMaterial.SetColor("_NightSkyTint", SkyTintColor.Evaluate(m_TimeFloat));
            m_CloudDomeMaterial.SetColor("_uCloudsAmbientColorTop", CloudLightColor.Evaluate(m_TimeFloat));
            m_CloudDomeMaterial.SetColor("_uCloudsAmbientColorBottom", CloudBaseColor.Evaluate(m_TimeFloat));
            m_CloudDomeMaterial.SetColor("_uSunColor", SunColor.Evaluate(m_TimeFloat));
            Color currentFogColor = FogColor.Evaluate(m_TimeFloat);
            m_CloudDomeMaterial.SetVector("_uFogColor", new Vector4(currentFogColor.r, currentFogColor.g, currentFogColor.b, CurrentFogAmount));
            m_CloudDomeMaterial.SetFloat("_uAttenuation", SunAttenuationCurve.Evaluate(m_TimeFloat * 24) * SunAttenuationMultipler);
            RenderSettings.ambientIntensity = AmbientIntensityCurve.Evaluate(m_TimeFloat * 24);
            RenderSettings.ambientSkyColor = AmbientSkyLightColor.Evaluate(m_TimeFloat);
            RenderSettings.ambientEquatorColor = AmbientEquatorLightColor.Evaluate(m_TimeFloat);
            RenderSettings.ambientGroundColor = AmbientGroundLightColor.Evaluate(m_TimeFloat);
            RenderSettings.reflectionIntensity = EnvironmentReflections.Evaluate(m_TimeFloat * 24);

            SunObjectMaterial.SetVector("_uWorldSpaceCameraPos", PlayerCamera.transform.position);
            SunObjectMaterial.SetColor("_SunColor", SunSpotColor.Evaluate(m_TimeFloat));
            SunObject.transform.localScale = Vector3.one * SunSize.Evaluate(m_TimeFloat * 24) * 3;
            m_SunLight.intensity = SunIntensityCurve.Evaluate(m_TimeFloat * 24) * SunIntensity;
            m_MoonLight.intensity = MoonIntensityCurve.Evaluate(m_TimeFloat * 24) * MoonIntensity * MoonPhaseIntensity;
            m_MoonTransform.localScale = MoonSize.Evaluate(m_TimeFloat * 24) * m_MoonStartingSize;

            m_MoonPhaseMaterial.SetFloat("_MoonBrightness", MoonObjectFade.Evaluate(m_TimeFloat * 24) * MoonBrightness);

            if (SunShaftsEffect == EnableFeature.Disabled)
                m_SunShafts.isEnabled = false;

            if (MoonShaftsEffect == EnableFeature.Disabled)
                m_MoonShafts.isEnabled = false;

            if (SunShaftsEffect == EnableFeature.Enabled && m_SunLight.intensity > 0)
            {
                if(!m_SunShafts.isEnabled)
                    m_SunShafts.isEnabled = true;
                if (m_MoonShafts.isEnabled)
                    m_MoonShafts.isEnabled = false;
                m_SunShafts.sunShaftIntensity = SunLightShaftIntensity.Evaluate(m_TimeFloat * 24);
                m_SunShafts.radialBlurIterations = SunLightShaftsBlurIterations;
                m_SunShafts.sunShaftBlurRadius = SunLightShaftsBlurSize;
                m_SunShafts.sunColor = SunLightShaftsColor.Evaluate(m_TimeFloat);
            }
            else if (MoonShaftsEffect == EnableFeature.Enabled && m_MoonLight.intensity > 0)
            {
                if(!m_MoonShafts.isEnabled)
                    m_MoonShafts.isEnabled = true;
                if (m_SunShafts.isEnabled)
                    m_SunShafts.isEnabled = false;
                m_MoonShafts.sunShaftIntensity = MoonLightShaftIntensity.Evaluate(m_TimeFloat * 24);
                m_MoonShafts.radialBlurIterations = MoonLightShaftsBlurIterations;
                m_MoonShafts.sunShaftBlurRadius = MoonLightShaftsBlurSize;
                m_MoonShafts.sunColor = MoonLightShaftsColor.Evaluate(m_TimeFloat);
            }

            if (FogType == FogTypeEnum.UnityFog)
            {
                CurrentFogColor = FogColor.Evaluate(m_TimeFloat);
                RenderSettings.fogColor = CurrentFogColor;
                m_CloudDomeMaterial.SetFloat("_UseUniStormFog", 0);
            }
            else if (FogType == FogTypeEnum.UnistormFog)
            {
                CurrentFogColor = FogColor.Evaluate(m_TimeFloat);
                m_UniStormAtmosphericFog.BottomColor = CurrentFogColor;
                m_UniStormAtmosphericFog.SunColor = FogLightColor.Evaluate(m_TimeFloat);
                m_UniStormAtmosphericFog.SunControl = SunControlCurve.Evaluate(m_TimeFloat * 24);
                m_UniStormAtmosphericFog.MoonControl = m_MoonLight.intensity;
                m_UniStormAtmosphericFog.SunIntensity = SunAtmosphericFogIntensity.Evaluate(m_TimeFloat * 24) * FogLightFalloff;
                m_UniStormAtmosphericFog.MoonIntensity = MoonAtmosphericFogIntensity.Evaluate(m_TimeFloat * 24) * FogLightFalloff;
                m_CloudDomeMaterial.SetColor("_FogColor", CurrentFogColor);
                m_CloudDomeMaterial.SetColor("_SunColor", FogLightColor.Evaluate(m_TimeFloat));
                m_CloudDomeMaterial.SetFloat("_SunControl", SunControlCurve.Evaluate(m_TimeFloat * 24));
                m_CloudDomeMaterial.SetFloat("_MoonControl", m_MoonLight.intensity);
                m_CloudDomeMaterial.SetVector("_SunVector", m_SunLight.transform.rotation * -Vector3.forward);
                m_CloudDomeMaterial.SetVector("_MoonVector", m_MoonLight.transform.rotation * -Vector3.forward);
                m_CloudDomeMaterial.SetFloat("_SunIntensity", SunAtmosphericFogIntensity.Evaluate(m_TimeFloat * 24) * FogLightFalloff);
                m_CloudDomeMaterial.SetFloat("_MoonIntensity", MoonAtmosphericFogIntensity.Evaluate(m_TimeFloat * 24) * FogLightFalloff);
            }
        }

        //Calculates our days and updates our Animation curves.
        void CalculateDays()
        {
            CalculatePrecipiation();
            TemperatureCurve.Evaluate(m_PreciseCurveTime);

            Day++; //Add a day to our Day variable
            CalculateMonths(); //Calculate our months
            CalculateSeason(); //Calculate our seasons
            OnDayChangeEvent.Invoke(); //Invoke our day events
            GetDate(); //Calculate the DateTime

            //Clears our hourly forecast and generates a new one for the current day
            if (WeatherGenerationMethod == UniStormSystem.WeatherGenerationMethodEnum.Hourly)
            {
                WeatherForecast.Clear();
                GenerateWeather();
            }
        }

        //Calculates our months for an accurate calendar that also calculates leap year
        void CalculateMonths()
        {
            //Calculates our days into months
            if (Day >= 32 && Month == 1 || Day >= 32 && Month == 3 || Day >= 32 && Month == 5 || Day >= 32 && Month == 7
                || Day >= 32 && Month == 8 || Day >= 32 && Month == 10 || Day >= 32 && Month == 12)
            {
                Day = Day % 32;
                Day += 1;
                Month += 1;
                OnMonthChangeEvent.Invoke(); //Invoke our Month events
            }

            if (Day >= 31 && Month == 4 || Day >= 31 && Month == 6 || Day >= 31 && Month == 9 || Day >= 31 && Month == 11)
            {
                Day = Day % 31;
                Day += 1;
                Month += 1;
                OnMonthChangeEvent.Invoke(); //Invoke our Month events
            }

            //Calculates Leap Year
            if (Day >= 30 && Month == 2 && (Year % 4 == 0 && Year % 100 != 0) || (Year % 400 == 0))
            {
                Day = Day % 30;
                Day += 1;
                Month += 1;
                OnMonthChangeEvent.Invoke(); //Invoke our Month events
            }

            else if (Day >= 29 && Month == 2 && Year % 4 != 0)
            {
                Day = Day % 29;
                Day += 1;
                Month += 1;
                OnMonthChangeEvent.Invoke(); //Invoke our Month events
            }

            //Calculates our months into years
            if (Month > 12)
            {
                Month = Month % 13;
                Year += 1;
                Month += 1;
                OnYearChangeEvent.Invoke(); //Invoke our Year events

                //Reset our m_roundingCorrection variable to 0
                m_roundingCorrection = 0;
            }
        }

        //Generate our weather according to the precipitation odds for the current time of year.
        //Check the weather type's conditions, if they are not met, reroll weather within the same category.
        public void GenerateWeather()
        {
            if (WeatherGeneration == EnableFeature.Enabled)
            {
                if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Daily)
                {
                    m_GeneratedOdds = UnityEngine.Random.Range(1, 101);
                    HourToChangeWeather = UnityEngine.Random.Range(0, 23);

                    if (HourToChangeWeather == Hour)
                    {
                        HourToChangeWeather = Hour - 1;
                    }

                    CheckGeneratedWeather();
                }
                else if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Hourly)
                {
                    for (int i = 0; i < 24; i++)
                    {
                        m_GeneratedOdds = UnityEngine.Random.Range(1, 101);
                        CheckGeneratedWeather();
                    }
                }
            }
        }

        //Check our generated weather for seasonal and temperature conditions. 
        //Reroll the weather if they are not met until an appropriate weather type in the same category is found.
        public void CheckGeneratedWeather()
        {
            CalculatePrecipiation();

            if (m_GeneratedOdds <= m_PrecipitationOdds && PrecipiationWeatherTypes.Count != 0)
            {
                TempWeatherType = PrecipiationWeatherTypes[Random.Range(0, PrecipiationWeatherTypes.Count)];
            }
            else if (m_GeneratedOdds > m_PrecipitationOdds && NonPrecipiationWeatherTypes.Count != 0)
            {
                TempWeatherType = NonPrecipiationWeatherTypes[Random.Range(0, NonPrecipiationWeatherTypes.Count)];
            }

            if (!IgnoreConditions)
            {
                while (TempWeatherType.TemperatureType == WeatherType.TemperatureTypeEnum.AboveFreezing && Temperature <= m_FreezingTemperature
                    || TempWeatherType.Season != WeatherType.SeasonEnum.All && (int)TempWeatherType.Season != (int)CurrentSeason
                || TempWeatherType.TemperatureType == WeatherType.TemperatureTypeEnum.BelowFreezing && Temperature > m_FreezingTemperature
                || TempWeatherType.SpecialWeatherType == WeatherType.Yes_No.Yes)
                {
                    if (TempWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                    {
                        TempWeatherType = NonPrecipiationWeatherTypes[Random.Range(0, NonPrecipiationWeatherTypes.Count)];
                    }
                    else if (TempWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                    {
                        TempWeatherType = PrecipiationWeatherTypes[Random.Range(0, PrecipiationWeatherTypes.Count)];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Daily)
            {
                NextWeatherType = TempWeatherType;
                OnWeatherGenerationEvent.Invoke();
            }
            else if (WeatherGenerationMethod == WeatherGenerationMethodEnum.Hourly)
            {
                WeatherForecast.Add(TempWeatherType);
                OnWeatherGenerationEvent.Invoke();
            }
            m_WeatherGenerated = true;
        }

        IEnumerator SunColorFadeSequence(float TransitionTime, float MaxValue)
        {
            float LerpValue = 0;
            float t = 0;

            while (LerpValue < MaxValue)
            {
                if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                {
                    if (FogType == FogTypeEnum.UnistormFog)
                    {
                        yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.45f);
                    }
                    else if (FogType == FogTypeEnum.UnityFog)
                    {
                        yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
                    }

                    t += Time.deltaTime / TransitionTime * 0.01f;
                    LerpValue += Time.deltaTime / TransitionTime;

                    for (int i = 0; i < SunColor.colorKeys.Length; i++)
                    {
                        SunLightColorKeySwitcher[i].color = Color.Lerp(SunLightColorKeySwitcher[i].color, StormySunColor.colorKeys[i].color, t);
                    }

                    SunColor.SetKeys(SunLightColorKeySwitcher, SunColor.alphaKeys);
                }
                else
                {
                    t += Time.deltaTime / TransitionTime * 0.01f;
                    LerpValue += Time.deltaTime / TransitionTime;

                    for (int i = 0; i < SunColor.colorKeys.Length; i++)
                    {
                        SunLightColorKeySwitcher[i].color = Color.Lerp(SunLightColorKeySwitcher[i].color, DefaultSunLightBaseColor.colorKeys[i].color, t);
                    }

                    SunColor.SetKeys(SunLightColorKeySwitcher, SunColor.alphaKeys);
                }

                yield return null;
            }
        }

        IEnumerator ColorFadeSequence(float TransitionTime, float MaxValue, Gradient FogGradientColor, Gradient CloudGradientColor)
        {
            float LerpValue = 0;
            float t = 0;

            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes && CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.Cloudy)
            {
                yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.48f);
            }

            while (LerpValue < MaxValue)
            {
                if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
                {
                    t += Time.deltaTime / TransitionTime * 0.01f;
                    LerpValue += Time.deltaTime / TransitionTime;

                    for (int i = 0; i < CloudBaseColor.colorKeys.Length; i++)
                    {
                        if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.No)
                        {
                            CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CloudStormyBaseColor.colorKeys[i].color, t);
                        }
                        else if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.Yes)
                        {
                            CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CloudGradientColor.colorKeys[i].color, t);
                        }                           
                    }

                    for (int i = 0; i < FogColor.colorKeys.Length; i++)
                    {
                        if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.No)
                        {
                            FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, FogStormyColor.colorKeys[i].color, t);
                        }
                        else if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.Yes)
                        {
                            FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, FogGradientColor.colorKeys[i].color, t);
                        }                           
                    }

                    for (int i = 0; i < AmbientSkyLightColor.colorKeys.Length; i++)
                    {
                        AmbientSkyLightColorKeySwitcher[i].color = Color.Lerp(AmbientSkyLightColorKeySwitcher[i].color, StormyAmbientSkyLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < CloudLightColor.colorKeys.Length; i++)
                    {
                        CloudLightColorKeySwitcher[i].color = Color.Lerp(CloudLightColorKeySwitcher[i].color, StormyCloudLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < FogLightColor.colorKeys.Length; i++)
                    {
                        FogLightColorKeySwitcher[i].color = Color.Lerp(FogLightColorKeySwitcher[i].color, StormyFogLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < AmbientEquatorLightColor.colorKeys.Length; i++)
                    {
                        AmbientEquatorLightColorKeySwitcher[i].color = Color.Lerp(AmbientEquatorLightColorKeySwitcher[i].color, StormyAmbientEquatorLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < AmbientGroundLightColor.colorKeys.Length; i++)
                    {
                        AmbientGroundLightColorKeySwitcher[i].color = Color.Lerp(AmbientGroundLightColorKeySwitcher[i].color, StormyAmbientGroundLightColor.colorKeys[i].color, t);
                    }

                    FogLightColor.SetKeys(FogLightColorKeySwitcher, FogLightColor.alphaKeys);
                    CloudLightColor.SetKeys(CloudLightColorKeySwitcher, CloudLightColor.alphaKeys);
                    FogColor.SetKeys(FogColorKeySwitcher, FogColor.alphaKeys);
                    CloudBaseColor.SetKeys(CloudColorKeySwitcher, CloudBaseColor.alphaKeys);
                    AmbientSkyLightColor.SetKeys(AmbientSkyLightColorKeySwitcher, AmbientSkyLightColor.alphaKeys);
                    AmbientEquatorLightColor.SetKeys(AmbientEquatorLightColorKeySwitcher, AmbientEquatorLightColor.alphaKeys);
                    AmbientGroundLightColor.SetKeys(AmbientGroundLightColorKeySwitcher, AmbientGroundLightColor.alphaKeys);
                }
                else if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.No)
                {
                    t += Time.deltaTime / TransitionTime * 0.01f;
                    LerpValue += Time.deltaTime / TransitionTime;

                    for (int i = 0; i < CloudBaseColor.colorKeys.Length; i++)
                    {
                        if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.No)
                        {
                            CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, DefaultCloudBaseColor.colorKeys[i].color, t);
                        }
                        else if (CurrentWeatherType.OverrideCloudColor == WeatherType.Yes_No.Yes)
                        {
                            CloudColorKeySwitcher[i].color = Color.Lerp(CloudColorKeySwitcher[i].color, CloudGradientColor.colorKeys[i].color, t);
                        }
                    }

                    for (int i = 0; i < FogColor.colorKeys.Length; i++)
                    {
                        if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.No)
                        {
                            FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, DefaultFogBaseColor.colorKeys[i].color, t);
                        }
                        else if (CurrentWeatherType.OverrideFogColor == WeatherType.Yes_No.Yes)
                        {
                            FogColorKeySwitcher[i].color = Color.Lerp(FogColorKeySwitcher[i].color, FogGradientColor.colorKeys[i].color, t);
                        }                      
                    }

                    for (int i = 0; i < AmbientSkyLightColor.colorKeys.Length; i++)
                    {
                        AmbientSkyLightColorKeySwitcher[i].color = Color.Lerp(AmbientSkyLightColorKeySwitcher[i].color, DefaultAmbientSkyLightBaseColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < CloudLightColor.colorKeys.Length; i++)
                    {
                        CloudLightColorKeySwitcher[i].color = Color.Lerp(CloudLightColorKeySwitcher[i].color, DefaultCloudLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < FogLightColor.colorKeys.Length; i++)
                    {
                        FogLightColorKeySwitcher[i].color = Color.Lerp(FogLightColorKeySwitcher[i].color, DefaultFogLightColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < AmbientEquatorLightColor.colorKeys.Length; i++)
                    {
                        AmbientEquatorLightColorKeySwitcher[i].color = Color.Lerp(AmbientEquatorLightColorKeySwitcher[i].color, DefaultAmbientEquatorLightBaseColor.colorKeys[i].color, t);
                    }

                    for (int i = 0; i < AmbientGroundLightColor.colorKeys.Length; i++)
                    {
                        AmbientGroundLightColorKeySwitcher[i].color = Color.Lerp(AmbientGroundLightColorKeySwitcher[i].color, DefaultAmbientGroundLightBaseColor.colorKeys[i].color, t);
                    }

                    FogLightColor.SetKeys(FogLightColorKeySwitcher, FogLightColor.alphaKeys);
                    CloudLightColor.SetKeys(CloudLightColorKeySwitcher, CloudLightColor.alphaKeys);
                    FogColor.SetKeys(FogColorKeySwitcher, FogColor.alphaKeys);
                    CloudBaseColor.SetKeys(CloudColorKeySwitcher, CloudBaseColor.alphaKeys);
                    AmbientSkyLightColor.SetKeys(AmbientSkyLightColorKeySwitcher, AmbientSkyLightColor.alphaKeys);
                    AmbientEquatorLightColor.SetKeys(AmbientEquatorLightColorKeySwitcher, AmbientEquatorLightColor.alphaKeys);
                    AmbientGroundLightColor.SetKeys(AmbientGroundLightColorKeySwitcher, AmbientGroundLightColor.alphaKeys);
                }

                yield return null;
            }
        }

        IEnumerator CloudFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            float CurrentValue = m_CloudDomeMaterial.GetFloat("_uCloudsCoverage");
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsCoverage", LerpValue);

                yield return null;
            }
        }

        IEnumerator StormyCloudsSequence(float TransitionTime, bool FadeOut)
        {
            float HorizonColorFadeStart = m_CloudDomeMaterial.GetFloat("_uHorizonColorFadeStart");
            float HorizonColorFadeEnd = m_CloudDomeMaterial.GetFloat("_uHorizonColorFadeEnd");
            float LerpValueColorStart = HorizonColorFadeStart;
            float LerpValueColorEnd = HorizonColorFadeEnd;

            float HorizonFadeStart = m_CloudDomeMaterial.GetFloat("_uHorizonFadeStart");
            float HorizonFadeEnd = m_CloudDomeMaterial.GetFloat("_uHorizonFadeEnd");
            float HorizonSunFadeEnd = m_CloudDomeMaterial.GetFloat("_uSunFadeEnd");
            float LerpValueStart = HorizonFadeStart;
            float LerpValueEnd = HorizonFadeEnd;
            float LerpSunValueEnd = HorizonSunFadeEnd;

            float HorizonBrightness = m_CloudDomeMaterial.GetFloat("_uCloudAlpha");
            float LerpValueHorizonBrightness = HorizonBrightness;

            float t = 0;

            if (!FadeOut)
            {
                yield return new WaitUntil(() => MostlyCloudyFadeValue <= 0);

                while (LerpValueColorEnd < 0.32)
                {
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue-0.05f);                   

                    t += Time.deltaTime * 1.5f;
                    LerpValueColorStart = Mathf.Lerp(HorizonColorFadeStart, -0.2f, t * 15 / TransitionTime);
                    LerpValueColorEnd = Mathf.Lerp(HorizonColorFadeEnd, 0.32f, t * 1.5f / TransitionTime);
                    m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeStart", LerpValueColorStart);
                    m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeEnd", LerpValueColorEnd);

                    if (FogType == FogTypeEnum.UnistormFog)
                    {
                        LerpValueStart = Mathf.Lerp(HorizonFadeStart, 0, t * 4 / TransitionTime);
                        LerpValueEnd = Mathf.Lerp(HorizonFadeEnd, 0.22f, t * 4 / TransitionTime);
                        LerpSunValueEnd = Mathf.Lerp(HorizonSunFadeEnd, 0.18f, t * 4 / TransitionTime);
                    }
                    else if (FogType == FogTypeEnum.UnityFog)
                    {
                        LerpValueStart = Mathf.Lerp(HorizonFadeStart, 0, t * 4 / TransitionTime);
                        LerpValueEnd = Mathf.Lerp(HorizonFadeEnd, 0.015f, t * 4 / TransitionTime);
                        LerpSunValueEnd = Mathf.Lerp(HorizonSunFadeEnd, 0.18f, t * 4 / TransitionTime);
                    }

                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeStart", LerpValueStart);
                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", LerpValueEnd);
                    m_CloudDomeMaterial.SetFloat("_uSunFadeEnd", LerpSunValueEnd);

                    LerpValueHorizonBrightness = Mathf.Lerp(HorizonBrightness, StormyHorizonBrightness, t * 5 / TransitionTime);
                    m_CloudDomeMaterial.SetFloat("_uCloudAlpha", LerpValueHorizonBrightness);

                    if (LerpValueColorEnd >= 0.32f)
                    {
                        break;
                    }

                    yield return null;
                }
            }
            else if (FadeOut)
            {
                yield return new WaitUntil(() => MostlyCloudyFadeValue <= 0);

                while (LerpValueEnd > m_CloudFadeLevelEnd)
                {                   
                    //Make lowest value the control
                    t += Time.deltaTime;
                    LerpValueColorStart = Mathf.Lerp(HorizonColorFadeStart, 0, t * 2f / TransitionTime);
                    LerpValueColorEnd = Mathf.Lerp(HorizonColorFadeEnd, 0, t * 5f / TransitionTime);
                    m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeStart", LerpValueColorStart);
                    m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeEnd", LerpValueColorEnd);

                    LerpValueStart = Mathf.Lerp(HorizonFadeStart, m_CloudFadeLevelStart, t * 10f / TransitionTime);
                    LerpValueEnd = Mathf.Lerp(HorizonFadeEnd, m_CloudFadeLevelEnd, t * 1f / TransitionTime);
                    LerpSunValueEnd = Mathf.Lerp(HorizonSunFadeEnd, 0.045f, t * 1f / TransitionTime);

                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeStart", LerpValueStart);
                    m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", LerpValueEnd);
                    m_CloudDomeMaterial.SetFloat("_uSunFadeEnd", LerpSunValueEnd);

                    LerpValueHorizonBrightness = Mathf.Lerp(HorizonBrightness, 1, t * 1f / TransitionTime);
                    m_CloudDomeMaterial.SetFloat("_uCloudAlpha", LerpValueHorizonBrightness);

                    yield return null;
                }
            }
        }

        IEnumerator FogFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.6f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.5f);
            }

            float CurrentValue = RenderSettings.fogDensity;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                RenderSettings.fogDensity = LerpValue;

                yield return null;
            }
        }

        IEnumerator WindFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            float CurrentValue = UniStormWindZone.windMain;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                UniStormWindZone.windMain = LerpValue;

                yield return null;
            }
        }

        IEnumerator SunFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.DontChange)
            {
                yield break;
            }

            if (SunIntensity > CurrentWeatherType.SunIntensity)
            {
                FadeOut = true;
            }

            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (FogType == FogTypeEnum.UnistormFog)
                {
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.45f);
                }
                else if (FogType == FogTypeEnum.UnityFog)
                {
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
                }
            }

            float CurrentValue = SunIntensity;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                SunIntensity = LerpValue;

                yield return null;
            }
        }

        IEnumerator MoonFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            if (CurrentWeatherType.CloudLevel == WeatherType.CloudLevelEnum.DontChange)
            {
                yield break;
            }

            if (MoonIntensity > CurrentWeatherType.MoonIntensity)
            {
                FadeOut = true;
            }

            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (FogType == FogTypeEnum.UnistormFog)
                {
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.45f);
                }
                else if (FogType == FogTypeEnum.UnityFog)
                {
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
                }
            }

            float CurrentValue = MoonIntensity;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                MoonIntensity = LerpValue;

                yield return null;
            }
        }

        IEnumerator MostlyCloudyAdjustment(float TransitionTime, float MaxValue, bool FadeOut)
        {
            yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uHorizonColorFadeStart") >= 0);

            float CurrentValue = MostlyCloudyFadeValue;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > 0 && FadeOut) || (LerpValue < 1 && !FadeOut))
            {
                t += Time.deltaTime;
                if (!FadeOut)
                {
                    LerpValue = Mathf.Lerp(CurrentValue, 1, t / TransitionTime);
                }
                else if (FadeOut)
                {
                    LerpValue = Mathf.Lerp(CurrentValue, 0, t / TransitionTime);
                }

                MostlyCloudyFadeValue = LerpValue;

                yield return null;
            }
        }

        IEnumerator CloudHeightSequence(float TransitionTime, float MaxValue)
        {
            float CurrentValue = m_CloudDomeMaterial.GetFloat("_uCloudsBottom");
            m_CurrentCloudHeight = CurrentValue;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                m_CurrentCloudHeight = LerpValue;
                m_CloudDomeMaterial.SetFloat("_uCloudsBottom", LerpValue);

                yield return null;
            }
        }

        IEnumerator CloudTallnessSequence(float TransitionTime, float MaxValue)
        {
            if (UniStormInitialized && ForceLowClouds == EnableFeature.Disabled)
            {
                float CurrentValue = m_CloudDomeMaterial.GetFloat("_uCloudsHeight");
                float LerpValue = CurrentValue;
                float t = 0;

                while ((t / TransitionTime) < 1)
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    m_CloudDomeMaterial.SetFloat("_uCloudsHeight", LerpValue);

                    yield return null;
                }
            }
        }

        IEnumerator ParticleFadeSequence(float TransitionTime, float MaxValue, ParticleSystem EffectToFade, bool FadeOut)
        {
            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.59f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
            }
            else if (CurrentWeatherType.WaitForCloudLevel == WeatherType.Yes_No.Yes)
            {
                yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= (m_ReceivedCloudValue-0.01f));
            }

            if (EffectToFade == null)
            {
                float CurrentValue = CurrentParticleSystem.emission.rateOverTime.constant;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    ParticleSystem.EmissionModule CurrentEmission = CurrentParticleSystem.emission;
                    CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(LerpValue);

                    yield return null;
                }
            }
            else
            {
                float CurrentValue = EffectToFade.emission.rateOverTime.constant;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    ParticleSystem.EmissionModule CurrentEmission = EffectToFade.emission;
                    CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(LerpValue);
                    m_ParticleAmount = LerpValue;

                    yield return null;
                }
            }
        }

        IEnumerator AdditionalParticleFadeSequence(float TransitionTime, float MaxValue, ParticleSystem AdditionalEffectToFade, bool FadeOut)
        {
            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.59f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
            }
            else if (CurrentWeatherType.WaitForCloudLevel == WeatherType.Yes_No.Yes)
            {
                yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= (m_ReceivedCloudValue - 0.01f));
            }

            if (AdditionalEffectToFade == null)
            {
                float CurrentValue = AdditionalCurrentParticleSystem.emission.rateOverTime.constant;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    ParticleSystem.EmissionModule CurrentEmission = AdditionalCurrentParticleSystem.emission;
                    CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(LerpValue);

                    yield return null;
                }
            }
            else
            {
                float CurrentValue = AdditionalEffectToFade.emission.rateOverTime.constant;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    ParticleSystem.EmissionModule CurrentEmission = AdditionalEffectToFade.emission;
                    CurrentEmission.rateOverTime = new ParticleSystem.MinMaxCurve(LerpValue);

                    yield return null;
                }
            }
        }

        IEnumerator SoundFadeSequence(float TransitionTime, float MaxValue, AudioSource SourceToFade, bool FadeOut)
        {
            if (CurrentWeatherType.PrecipitationWeatherType == WeatherType.Yes_No.Yes)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.59f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
            }
            else if (CurrentWeatherType.WaitForCloudLevel == WeatherType.Yes_No.Yes)
            {
                yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= (m_ReceivedCloudValue - 0.01f));
            }

            float CurrentValue = SourceToFade.volume;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                SourceToFade.volume = LerpValue;

                yield return null;
            }
        }

        IEnumerator RainShaderFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            if (!FadeOut)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.59f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);
            }
            else
            {
                yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") <= m_ReceivedCloudValue + 0.01f);
            }

            float CurrentValue = Shader.GetGlobalFloat("_WetnessStrength");
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, (t / TransitionTime));
                Shader.SetGlobalFloat("_WetnessStrength", LerpValue);

                yield return null;
            }
        }

        IEnumerator SnowShaderFadeSequence(float TransitionTime, float MaxValue, bool FadeOut)
        {
            if (!FadeOut)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= 0.59f);
                else
                    yield return new WaitUntil(() => m_CloudDomeMaterial.GetFloat("_uCloudsCoverage") >= m_ReceivedCloudValue);

                TransitionTime = TransitionTime * 2;
            }

            float CurrentValue = Shader.GetGlobalFloat("_SnowStrength");
            float LerpValue = CurrentValue;
            float t = 0;

            while ((LerpValue > MaxValue && FadeOut) || (LerpValue < MaxValue && !FadeOut))
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, (t / TransitionTime));
                Shader.SetGlobalFloat("_SnowStrength", LerpValue);
                yield return null;
            }
        }

        IEnumerator AuroraShaderFadeSequence(float TransitionTime, float MaxValue, Color InnerColor, Color OuterColor)
        {
            float CurrentLightIntensity = Shader.GetGlobalFloat("_LightIntensity");
            float LerpLightIntensity = CurrentLightIntensity;

            Color CurrentInnerColor = Shader.GetGlobalColor("_InnerColor");
            Color LerpInnerColor = CurrentInnerColor;

            Color CurrentOuterColor = Shader.GetGlobalColor("_OuterColor");
            Color LerpOuterColor = CurrentOuterColor;

            float t = 0;

            if (CurrentLightIntensity <= 0 && CurrentWeatherType.UseAuroras == WeatherType.Yes_No.Yes)
            {
                m_AuroraParent.SetActive(true);
            }

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                LerpLightIntensity = Mathf.Lerp(CurrentLightIntensity, MaxValue, t / TransitionTime);
                Shader.SetGlobalFloat("_LightIntensity", LerpLightIntensity);

                if (CurrentWeatherType.UseAuroras == WeatherType.Yes_No.Yes)
                {
                    LerpInnerColor = Color.Lerp(CurrentInnerColor, InnerColor, t / TransitionTime);
                    Shader.SetGlobalColor("_InnerColor", LerpInnerColor);

                    LerpOuterColor = Color.Lerp(CurrentOuterColor, OuterColor, t / TransitionTime);
                    Shader.SetGlobalColor("_OuterColor", LerpOuterColor);
                }

                yield return null;
            }

            if (LerpLightIntensity <= 0)
            {
                m_AuroraParent.SetActive(false);
            }
        }

        IEnumerator CloudProfileSequence (float TransitionTime, float MaxEdgeSoftness, float MaxBaseSoftness, float MaxDetailStrength, float MaxDensity, float MaxCoverageBias, float MaxDetailScale)
        {
            float EdgeSoftnessValue = m_CloudDomeMaterial.GetFloat("_uCloudsBaseEdgeSoftness");
            float LerpedEdgeSoftnessValue = EdgeSoftnessValue;

            float BaseSoftnessValue = m_CloudDomeMaterial.GetFloat("_uCloudsBottomSoftness");
            float LerpedBaseSoftnessValue = BaseSoftnessValue;

            float DetailStrengthValue = m_CloudDomeMaterial.GetFloat("_uCloudsDetailStrength");
            float LerpedDetailStrengthValue = DetailStrengthValue;

            float DensityValue = m_CloudDomeMaterial.GetFloat("_uCloudsDensity");
            float LerpedDensityValue = DensityValue;

            float CoverageBiasValue = m_CloudDomeMaterial.GetFloat("_uCloudsCoverageBias");
            float LerpedCoverageBiasValue = CoverageBiasValue;

            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;

                //Edge Softness
                LerpedEdgeSoftnessValue = Mathf.Lerp(EdgeSoftnessValue, MaxEdgeSoftness, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsBaseEdgeSoftness", LerpedEdgeSoftnessValue);

                //Base Softness
                LerpedBaseSoftnessValue = Mathf.Lerp(BaseSoftnessValue, MaxBaseSoftness, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsBottomSoftness", LerpedBaseSoftnessValue);

                //Detail Strength
                LerpedDetailStrengthValue = Mathf.Lerp(DetailStrengthValue, MaxDetailStrength, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsDetailStrength", LerpedDetailStrengthValue);

                //Density
                LerpedDensityValue = Mathf.Lerp(DensityValue, MaxDensity, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsDensity", LerpedDensityValue);

                //Coverage Bias
                LerpedCoverageBiasValue = Mathf.Lerp(CoverageBiasValue, MaxCoverageBias, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_uCloudsCoverageBias", LerpedCoverageBiasValue);

                yield return null;
            }
        }

        IEnumerator CloudShadowIntensitySequence(float TransitionTime, float MaxValue)
        {
            if (UniStormInitialized)
            {
                float CurrentValue = m_CloudShadows.ShadowIntensity;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((t / TransitionTime) < 1)
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    m_CloudShadows.ShadowIntensity = LerpValue;

                    yield return null;
                }
            }
        }

        IEnumerator SunAttenuationIntensitySequence(float TransitionTime, float MaxValue)
        {
            if (UniStormInitialized)
            {
                float CurrentValue = SunAttenuationMultipler;
                float LerpValue = CurrentValue;
                float t = 0;

                while ((t / TransitionTime) < 1)
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                    SunAttenuationMultipler = LerpValue;

                    yield return null;
                }
            }
        }

        IEnumerator MusicFadeSequence(float TransitionTime, AudioClip NewMusicClip)
        {
            if (UniStormInitialized)
            {
                float CurrentValue = TimeOfDayMusicAudioSource.volume;
                float LerpValue = CurrentValue;
                float t = 0;

                //Fade out for transition, only if the AudioSource has a clip
                if (TimeOfDayMusicAudioSource.clip != null)
                {                   
                    while ((t / TransitionTime) < 1)
                    {
                        t += Time.deltaTime;
                        LerpValue = Mathf.Lerp(CurrentValue, 0, t / TransitionTime);
                        TimeOfDayMusicAudioSource.volume = LerpValue;

                        yield return null;
                    }
                }
                else
                {
                    TimeOfDayMusicAudioSource.volume = 0;
                }

                //Assign new music clip
                TimeOfDayMusicAudioSource.clip = NewMusicClip;               
                TimeOfDayMusicAudioSource.Play();

                //Reset values to fade in from 0
                CurrentValue = TimeOfDayMusicAudioSource.volume;
                LerpValue = CurrentValue;
                t = 0;

                //Fade back in with new clip
                while ((t / TransitionTime) < 1)
                {
                    t += Time.deltaTime;
                    LerpValue = Mathf.Lerp(CurrentValue, MusicVolume, t / TransitionTime);
                    TimeOfDayMusicAudioSource.volume = LerpValue;

                    yield return null;
                }

                m_TimeOfDayMusicTimer = 0;
            }
        }

        IEnumerator AtmosphericFogFadeSequence(float TransitionTime, float ShaderMaxValue, float CloudMaxValue)
        {
            float ShaderCurrentValue = m_UniStormAtmosphericFog.BlendHeight;
            float ShaderLerpValue = ShaderCurrentValue;
            float CloudsCurrentValue = m_CloudDomeMaterial.GetFloat("_FogBlendHeight");
            float CloudsLerpValue = CloudsCurrentValue;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                ShaderLerpValue = Mathf.Lerp(ShaderCurrentValue, ShaderMaxValue, t / TransitionTime);
                m_UniStormAtmosphericFog.BlendHeight = ShaderLerpValue;

                CloudsLerpValue = Mathf.Lerp(CloudsCurrentValue, CloudMaxValue, t / TransitionTime);
                m_CloudDomeMaterial.SetFloat("_FogBlendHeight", CloudsLerpValue);

                yield return null;
            }
        }

        IEnumerator FogLightFalloffSequence(float TransitionTime, float MaxValue)
        {
            float CurrentValue = FogLightFalloff;
            float LerpValue = CurrentValue;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                LerpValue = Mathf.Lerp(CurrentValue, MaxValue, t / TransitionTime);
                FogLightFalloff = LerpValue;

                yield return null;
            }
        }

        IEnumerator SunHeightSequence(float TransitionTime, float OpaqueValue, float TransparentValue)
        {
            float CurrentOpaqueValue = SunObjectMaterial.GetFloat("_OpaqueY");
            float LerpOpaqueValue = CurrentOpaqueValue;
            float CurrentTransparentValue = SunObjectMaterial.GetFloat("_TransparentY");
            float LerpTransparentValue = CurrentTransparentValue;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime * 0.85f;
                LerpOpaqueValue = Mathf.Lerp(CurrentOpaqueValue, OpaqueValue, t / TransitionTime);
                SunObjectMaterial.SetFloat("_OpaqueY", LerpOpaqueValue);

                LerpTransparentValue = Mathf.Lerp(CurrentTransparentValue, TransparentValue, t / TransitionTime);
                SunObjectMaterial.SetFloat("_TransparentY", LerpTransparentValue);

                yield return null;
            }
        }

        void OnApplicationQuit()
        {
            //Reset our weather shader when the scene is stopped so the shader values remain unchanged in the editor.
            Shader.SetGlobalFloat("_WetnessStrength", 0);
            Shader.SetGlobalFloat("_SnowStrength", 0);
            Shader.SetGlobalFloat("_LightIntensity", 0);

            m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeStart", 0);
            m_CloudDomeMaterial.SetFloat("_uHorizonColorFadeEnd", 0);
            m_CloudDomeMaterial.SetFloat("_uHorizonFadeStart", m_CloudFadeLevelStart);
            m_CloudDomeMaterial.SetFloat("_uHorizonFadeEnd", 0.18f);
            m_CloudDomeMaterial.SetFloat("_uSunFadeEnd", 0.045f);
            m_CloudDomeMaterial.SetFloat("_uCloudAlpha", 1);
            m_CloudDomeMaterial.SetFloat("_FogBlendHeight", 0.3f);

            if (CloudShadows == EnableFeature.Enabled)
            {
                m_CloudShadows.ShadowIntensity = 0;
            }
        }

        private void FindURPRendererData()
        {
            if (urpForwardRendererData != null) return;

            var pipeline = ((UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset);
            System.Reflection.FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            urpForwardRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0] as UniversalRendererData;
        }
    }
}