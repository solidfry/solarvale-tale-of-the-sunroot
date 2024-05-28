using System;
using System.Collections;
using System.Collections.Generic;
#if GAIA_PRO_PRESENT
using ProceduralWorlds.HDRPTOD;
#endif
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.Vehicles.Car;
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
    /// Handy FPS and device capabilities class.
    /// </summary>
    public class PhotoMode : MonoBehaviour
    {
        public static PhotoMode Instance
        {
            get { return m_instance; }
        }

        [SerializeField] private static PhotoMode m_instance;

        public static List<PhotoModeUIHelper> CurrentRuntimeUIElements = new List<PhotoModeUIHelper>();

        #region Variables

        public PhotoModeProfile m_photoModeProfile;
        public List<PhotoModeProfile> m_photoModeProfiles = new List<PhotoModeProfile>();
        public List<PhotoModeImages> m_uiImages = new List<PhotoModeImages>();
        public PhotoModeUIHelper m_runtimeUIPrefab = null;
        private PhotoModeValues m_photoModeValues = new PhotoModeValues();
        private PhotoModeValues m_savedPhotoModeValues = new PhotoModeValues();
        private PhotoModeMinAndMaxValues m_minAndMaxValues = new PhotoModeMinAndMaxValues();
        //System bools
        private bool m_isSettingValues = false;
        private bool m_isUpdatingValues = false;
        private bool m_pwWeatherPresent = false;
        private bool m_hdrpTimeOfDay = false;
        private bool m_unitySkyboxPresent = false;
        private bool m_lastTerrainCullingValue = true;
        //Components
        public Camera m_targetCamera = null;
        private bool m_photoModeCameraInstantiated = false;
        private ScreenShotter m_screenShotter;
        private GaiaConstants.EnvironmentRenderer m_renderPipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;
        private int m_savedLightingProfileIndex;
        private Light m_mainSunLight;
        private Terrain m_activeTerrain;
        private SceneProfile m_sceneProfile;
        private Material m_unitySkybox;
        private UIConfiguration m_gaiaUI;

        #region New Photo Mode

        public Text m_selectedPanelText;
        public List<PhotoModePanel> m_panelButtons = new List<PhotoModePanel>();
        public PhotoModePanelTransformSettings m_transformSettings = new PhotoModePanelTransformSettings();
        public ScrollRect m_scrollRect;
        public GameObject m_reticule;
        public GameObject m_ruleOfThirds;
        public GameObject m_optionsPanel;

        #endregion
        #region FPS Variables

        public int FPS
        {
            get { return (int)m_currentFps; }
        }

        public Color m_30FPSColor = Color.red;
        public Color m_60FPSColor = Color.yellow;
        public Color m_120FPSColor = Color.green;
        public Color m_maxFPSColor = Color.white;

        //private const string m_cFormat = "FPS {0}, MS {1:0.00}";
        private const string m_cFormat = "FPS {0}";
        private const float m_cMeasurePeriod = 1f;
        private float m_currentFps;
        //private float m_currentMs;
        private float m_fpsAccumulator = 0;
        private float m_fpsNextPeriod = 0;

        #endregion
        #region System Metrics UX

        public Text m_fpsText;
        public Text m_StormVersionText;
        public Text m_OSText;
        public Text m_deviceText;
        public Text m_systemText;
        public Text m_gpuText;
        public Text m_gpuCapabilitiesText;
        public Text m_screenInfoText;

        private string m_stormVersion;
        private string m_OS;
        private string m_deviceName;
        private string m_deviceType;
        private string m_deviceModel;
        private string m_platform;
        private string m_processor;
        private string m_ram;
        private string m_gpu;
        private string m_gpuDevice;
        private string m_gpuSpec;
        private string m_gpuCapabilities;
        private string m_screenInfo;
        private string m_quality;

        #endregion
        #region Photo Mode Settings

        public Text m_screenshotText;
        private PhotoModeUIHelper m_photoModeHeader = null;
        private PhotoModeUIHelper m_screenshotResolution = null;
        private PhotoModeUIHelper m_screenshotImageFormat = null;
        private PhotoModeUIHelper m_loadSavedSettings = null;
        private PhotoModeUIHelper m_resetPhotoModeOnDisable = null;
        private PhotoModeUIHelper m_showFPS = null;
        private PhotoModeUIHelper m_showReticule = null;
        private PhotoModeUIHelper m_showRuleOfThirds = null;

        #endregion
        #region Unity Settings

        private PhotoModeUIHelper m_unityVolume = null;
        private PhotoModeUIHelper m_unityLODBias = null;
        private PhotoModeUIHelper m_unityVSync = null;
        private PhotoModeUIHelper m_unityTargetFPS = null;
        private PhotoModeUIHelper m_unityAA = null;
        private PhotoModeUIHelper m_unityShadowDistance = null;
        private PhotoModeUIHelper m_unityShadownResolution = null;
        private PhotoModeUIHelper m_unityShadowCascades = null;
        private PhotoModeUIHelper m_fieldOfView = null;
        private PhotoModeUIHelper m_cameraRoll = null;
        private PhotoModeUIHelper m_cullingDistance = null;
        private PhotoModeUIHelper m_unitySettingsGeneralHeader = null;
        private PhotoModeUIHelper m_unitySettingsShadowHeader = null;
        private PhotoModeUIHelper m_unitySettingsVSyncHeader = null;
        private PhotoModeUIHelper m_cameraSettingsGeneralHeader = null;

        #endregion
        #region Streaming Settings

        public RectTransform m_streamingSettingsArea = null;
#if GAIA_PRO_PRESENT
        private TerrainLoader m_terrainLoader = null;
        private PhotoModeUIHelper m_gaiaLoadRange = null;
        private PhotoModeUIHelper m_gaiaImpostorRange = null;
        private PhotoModeUIHelper m_streamingHeader = null;
#endif

        #endregion
        #region Weather Settings

        private PhotoModeUIHelper m_gaiaWindDirection = null;
        private PhotoModeUIHelper m_gaiaWindSpeed = null;
        private PhotoModeUIHelper m_gaiaWindHeader = null;
        private PhotoModeUIHelper m_gaiaWindSettingsOverride = null;
#if GAIA_PRO_PRESENT
        private ProceduralWorldsGlobalWeather m_weather = null;
        private PW_VFX_Atmosphere m_atmosphere = null;

        private PhotoModeUIHelper m_gaiaWeatherEnabled = null;
        private PhotoModeUIHelper m_gaiaWeatherToggleRain = null;
        private PhotoModeUIHelper m_gaiaWeatherToggleSnow = null;
#endif

        #endregion
        #region Gaia Lighting Settings

#if GAIA_PRO_PRESENT
        private PhotoModeUIHelper m_gaiaTime = null;
        private PhotoModeUIHelper m_gaiaTimeOfDayEnabled = null;
        private PhotoModeUIHelper m_gaiaTimeScale = null;
        private PhotoModeUIHelper m_gaiaAdditionalLinearFog = null;
        private PhotoModeUIHelper m_gaiaAdditionalExponentialFog = null;
        private PhotoModeUIHelper m_lightingWeatherHeader = null;
        private PhotoModeUIHelper m_lightingGeneralHeader = null;
#endif
        private PhotoModeUIHelper m_gaiaSunAngle = null;
        private PhotoModeUIHelper m_gaiaSunPitch = null;
        private PhotoModeUIHelper m_gaiaFogOverride = null;
        private PhotoModeUIHelper m_gaiaFogMode = null;
        private PhotoModeUIHelper m_gaiaFogColor = null;
        private PhotoModeUIHelper m_gaiaFogStart = null;
        private PhotoModeUIHelper m_gaiaFogEnd = null;
        private PhotoModeUIHelper m_gaiaFogDensity = null;
        private PhotoModeUIHelper m_gaiaSkyboxOverride = null;
        private PhotoModeUIHelper m_gaiaSkyboxRotation = null;
        private PhotoModeUIHelper m_gaiaSkyboxExposure = null;
        private PhotoModeUIHelper m_gaiaSkyboxTint = null;
        private PhotoModeUIHelper m_gaiaAmbientIntensity = null;
        private PhotoModeUIHelper m_gaiaSunOverride = null;
        private PhotoModeUIHelper m_gaiaSunIntensity = null;
        private PhotoModeUIHelper m_gaiaSunKelvin = null;
        private PhotoModeUIHelper m_gaiaSunColor = null;
        private PhotoModeUIHelper m_lightingFogHeader = null;
        private PhotoModeUIHelper m_lightingSkyboxHeader = null;
        private PhotoModeUIHelper m_lightingSunHeader = null;
        private PhotoModeUIHelper m_lightingAmbientHeader = null;
        private PhotoModeUIHelper m_ambientSkyColor = null;
        private PhotoModeUIHelper m_ambientEquatorColor = null;
        private PhotoModeUIHelper m_ambientGroundColor = null;

        //HDRP Time Of Day
#if GAIA_PRO_PRESENT
        private PhotoModeUIHelper m_hdrpGlobalShadowMultiplier = null;
        private PhotoModeUIHelper m_hdrpGlobalFogMultiplier = null;
        private PhotoModeUIHelper m_hdrpGlobalSunMultiplier = null;
#endif

        #endregion
        #region Water Settings

        private PhotoModeUIHelper m_gaiaWaterheader = null;
        private PhotoModeUIHelper m_gaiaUnderwaterheader = null;
        private PhotoModeUIHelper m_gaiaWaterReflectionsEnabled = null;
        private PhotoModeUIHelper m_gaiaWaterReflectionDistance = null;
        private PhotoModeUIHelper m_gaiaWaterReflectionResolution = null;
        private PhotoModeUIHelper m_gaiaWaterReflectionLODBias = null;
        private PhotoModeUIHelper m_gaiaUnderwaterFogColor = null;
        private PhotoModeUIHelper m_gaiaUnderwaterFogDistance = null;
        private PhotoModeUIHelper m_gaiaUnderwaterVolume = null;
        #endregion
        #region Post FX Settings

        private PhotoModeUIHelper m_gaiaPostFXExposure = null;
#if UNITY_POST_PROCESSING_STACK_V2 || UPPipeline || HDPipeline
        private PhotoModeUIHelper m_gaiaPostFXExposureHeader = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFHeader = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFEnabled = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFAutoFocus = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFAperture = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFFocalLength = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFFocusDistance = null;
#endif

#if UNITY_POST_PROCESSING_STACK_V2
        private PhotoModeUIHelper m_gaiaPostFXDOFKernelSizeBuiltIn = null;
        private PostProcessLayer m_postProcessingLayer = null;
        private UnityEngine.Rendering.PostProcessing.DepthOfField m_depthOfField;
#else
        private PhotoModeUIHelper m_noPostProcessingHeader = null;
#endif

#if UPPipeline
        private PhotoModeUIHelper m_gaiaPostFXDOFModeURP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFStartDistanceURP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFEndDistanceURP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFMaxRadiusURP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFHighQualityURP = null;
        private UnityEngine.Rendering.Universal.DepthOfField m_depthOfFieldURP;
        private UniversalAdditionalCameraData m_urpCameraData;
#endif

#if HDPipeline
        private PhotoModeUIHelper m_gaiaPostFXDOFModeHDRP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFNearStartDistanceHDRP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFFarStartDistanceHDRP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFNearEndDistanceHDRP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFFarEndDistanceHDRP = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFMaxRadius = null;
        private PhotoModeUIHelper m_gaiaPostFXDOFHighQualityHDRP = null;
        private UnityEngine.Rendering.HighDefinition.DepthOfField m_depthOfFieldHDRP;
        private HDAdditionalCameraData m_hdrpCameraData = null;
#endif

        #endregion
        #region Terrain Settings

        private PhotoModeUIHelper m_terrainDetailDensity = null;
        private PhotoModeUIHelper m_terrainDetailDistance = null;
        private PhotoModeUIHelper m_terrainHeightResolution = null;
        private PhotoModeUIHelper m_terrainTextureDistance = null;
        private PhotoModeUIHelper m_terrainDrawInstanced= null;
        private PhotoModeUIHelper m_terrainHeader = null;

        #endregion
        #region HDRP Density Volume

        #if HDPipeline
        private PhotoModeUIHelper m_overrideDensityVolume = null;
        private PhotoModeUIHelper m_densityVolumeAlbedoColor = null;
        private PhotoModeUIHelper m_densityVolumeFogDistance = null;
        private PhotoModeUIHelper m_densityVolumeEffectType = null;
        private PhotoModeUIHelper m_densityVolumeTilingResolution = null;
        private PhotoModeUIHelper m_densityVolumeHeader = null;
        #endif

        #endregion
        #region Controller Settings

        public bool m_freezePlayerController = true;
        public GameObject m_spawnedCamera;
        private Rigidbody m_rigidBodyController;
        private RigidbodyConstraints m_lastConstrants;
        private MonoBehaviour m_playerController;
        private MonoBehaviour m_thirdPersonBaseController;
        private MonoBehaviour m_carBaseController;
        private MonoBehaviour m_carAudioController;
        private GameObject m_lastPlayerController;
        private Camera m_lastPlayerCamera;

        #endregion
        #region Color Picker

        public ColorPickerReferenceMode m_colorPickerRefMode = ColorPickerReferenceMode.FogColor;
        public PhotoModeColorPicker m_colorPicker;
        [HideInInspector]
        public bool m_updateColorPickerRef = false;

        #endregion
        #region Grass System

#if FLORA_PRESENT
        private PhotoModeUIHelper m_grassSettingsHeader = null;
        private PhotoModeUIHelper m_globalGrassDensity = null;
        private PhotoModeUIHelper m_globalGrassDistance = null;
        private PhotoModeUIHelper m_globalCameraCellDistance = null;
        private PhotoModeUIHelper m_globalCameraCellSubdivision = null;
        private FloraGlobalManager m_detailManager;
#endif

        #endregion

        #endregion
        #region Unity Functions

        private void Start()
        {
            Instantiate();
        }
        private void Update()
        {
            ProcessUpdate();
            UpdateColorPicker();
        }
        private void OnDestroy()
        {
            ResetBackToDefault();
        }
        private void OnDisable()
        {
            if (m_gaiaUI == null)
            {
                return;
            }

            if (!m_gaiaUI.m_resetOnDisable)
            {
                return;
            }

            RevertPostProcessing();
        }

        #endregion
        #region Functions

        #region Generic Functions

        /// <summary>
        /// Cloese photo mode
        /// </summary>
        public void ClosePhotoMode()
        {
            if (UIConfiguration.Instance != null)
            {
                UIConfiguration.Instance.ExecuteShowPhotoMode();
            }
        }
        /// <summary>
        /// Gets panel profile data
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        public PhotoModePanel GetPanelProfile(string searchFor)
        {
            if (m_panelButtons.Count > 0)
            {
                foreach (PhotoModePanel panel in m_panelButtons)
                {
                    if (panel.m_shownTitle.Contains(searchFor))
                    {
                        return panel;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Called when the system is loaded to setup everything
        /// </summary>
        private void Instantiate()
        {
            m_instance = this;
#if FLORA_PRESENT
            m_detailManager = GaiaUtils.FindOOT<FloraGlobalManager>();
#endif
            if (UIConfiguration.Instance != null)
            {
                m_renderPipeline = UIConfiguration.RenderPipeline;
                if (UIConfiguration.Instance.m_textContent.activeInHierarchy)
                {
                    UIConfiguration.Instance.m_textContent.gameObject.SetActive(false);
                }
            }

            if (m_colorPicker != null)
            {
                m_colorPicker.gameObject.SetActive(false);
            }

            m_fpsNextPeriod = Time.realtimeSinceStartup + m_cMeasurePeriod;
            m_lastPlayerCamera = GaiaUtils.GetCamera();
            
            if (GaiaUtils.CheckIfSceneProfileExists(out m_sceneProfile))
            {
                m_lastTerrainCullingValue = m_sceneProfile.m_terrainCullingEnabled;
            }

            if (m_lastPlayerCamera != null)
            {
                if (!m_lastPlayerCamera.name.Contains("FlyCam"))
                {
                    m_targetCamera = LoadPhotoModeCamera();
                    m_photoModeCameraInstantiated = true;
                }
                else
                {
                    m_targetCamera = GaiaUtils.GetCamera();
                    m_photoModeCameraInstantiated = false;
                }
            }
            else
            {
                m_targetCamera = GaiaUtils.GetCamera();
                m_photoModeCameraInstantiated = false;
            }

            if (m_playerController == null)
            {
                m_playerController = GaiaUtils.GetPlayerControllerSystem();
            }

            if (m_targetCamera != null)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_postProcessingLayer == null)
                        {
                            m_postProcessingLayer = m_targetCamera.GetComponent<PostProcessLayer>();
                        }
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
#if UPPipeline
                        if (m_urpCameraData == null)
                        {
                            m_urpCameraData = m_targetCamera.GetComponent<UniversalAdditionalCameraData>();
                        }
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        if (m_hdrpCameraData == null)
                        {
                            m_hdrpCameraData = m_targetCamera.GetComponent<HDAdditionalCameraData>();
                        }
#endif
                        break;
                    }
                }
                AssignFloraCamera(m_targetCamera);
            }

#if GAIA_PRO_PRESENT
            if (m_terrainLoader == null)
            {
                if (m_targetCamera != null)
                {
                    m_terrainLoader = m_targetCamera.GetComponent<TerrainLoader>();
                }
                else
                {
                    m_terrainLoader = GaiaUtils.FindOOT<TerrainLoader>();
                }
            }
            m_weather = ProceduralWorldsGlobalWeather.Instance;
            m_atmosphere = PW_VFX_Atmosphere.Instance;
#endif
                    m_gaiaUI = UIConfiguration.Instance;

            m_activeTerrain = TerrainHelper.GetActiveTerrain();

            if (!VerifySystems())
            {
                return;
            }

            FreezePlayerController();
            SaveStartValues();
            SetupSystemMetrics();
            Setup();

            if (m_sceneProfile != null)
            {
                m_sceneProfile.m_terrainCullingEnabled = m_lastTerrainCullingValue;
            }
            PhotoModeButtonManager buttonManager = GaiaUtils.FindOOT<PhotoModeButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.Startup();
            }
        }
        /// <summary>
        /// Process the update thread
        /// </summary>
        private void ProcessUpdate()
        {
            // measure average frames per second
            if (m_photoModeValues.m_showFPS)
            {
                m_fpsAccumulator++;
                if (Time.realtimeSinceStartup > m_fpsNextPeriod)
                {
                    m_currentFps = m_fpsAccumulator / m_cMeasurePeriod;
                    //m_currentMs = 1000f / m_currentFps;
                    m_fpsAccumulator = 0f;
                    m_fpsNextPeriod = Time.realtimeSinceStartup + m_cMeasurePeriod;
                    if (m_fpsText != null)
                    {
                        if (m_currentFps < 30)
                        {
                            m_fpsText.color = m_30FPSColor;
                        }
                        else if (m_currentFps < 60)
                        {
                            m_fpsText.color = m_60FPSColor;
                        }
                        else if (m_currentFps < 120)
                        {
                            m_fpsText.color = m_120FPSColor;
                        }
                        else
                        {
                            m_fpsText.color = m_maxFPSColor;
                        }

                        m_fpsText.text = string.Format(m_cFormat, m_currentFps);
                    }
                }
            }

            //Update the UI
            UpdateUI();
            GaiaAPI.SaveImportantPhotoModeValues(m_photoModeValues, m_renderPipeline);
        }
        /// <summary>
        /// Assigns the camera for Flora Tiles
        /// </summary>
        /// <param name="camera"></param>
        private void AssignFloraCamera(Camera camera)
        {
#if FLORA_PRESENT
            if (m_detailManager != null)
            {

                FloraTerrainTile[] tiles = GaiaUtils.FindOOTs<FloraTerrainTile>();

                if (tiles.Length > 0)
                {
                    foreach (FloraTerrainTile tile in tiles)
                    {
                        tile.DetailCamera = camera;
                    }
                }
            }
#endif
            }
        /// <summary>
        /// Loads the photo mode camera
        /// </summary>
        /// <returns></returns>
        private Camera LoadPhotoModeCamera()
        {
            Camera camera = null;
            if (m_spawnedCamera != null)
            {
                camera = Instantiate(m_spawnedCamera).GetComponent<Camera>();
                GaiaAPI.SetRuntimePlayerAndCamera(camera.gameObject, camera, false);
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
                        GaiaAPI.CopyCameraSettings(m_lastPlayerCamera, camera);
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
                        GaiaAPI.CopyCameraSettingsURP(m_lastPlayerCamera, camera);
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        GaiaAPI.CopyCameraSettingsHDRP(m_lastPlayerCamera, camera);
#endif
                        break;
                    }
                }
            }

            return camera;
        }
        /// <summary>
        /// Removes photo mode camera from the scene
        /// </summary>
        private void RemovePhotoModeCamera()
        {
            if (m_targetCamera != null && m_photoModeCameraInstantiated)
            {
                DestroyImmediate(m_targetCamera.gameObject);
                if (m_lastPlayerController != null)
                {
                    if (m_lastPlayerCamera != null)
                    {
                        AssignFloraCamera(m_lastPlayerCamera);
                        GaiaAPI.SetRuntimePlayerAndCamera(m_lastPlayerController, m_lastPlayerCamera, true);
                    }
                }
                else
                {
                    if (m_lastPlayerCamera != null)
                    {
                        GaiaAPI.SetRuntimePlayerAndCamera(m_lastPlayerCamera.gameObject, m_lastPlayerCamera, true);
                    }
                }
            }
        }
        /// <summary>
        /// Freezes the player controller
        /// </summary>
        private void FreezePlayerController()
        {
            if (m_freezePlayerController)
            {
                m_lastPlayerController = GaiaUtils.GetPlayerGameObject();
                if (m_lastPlayerController != null)
                {
                    if (m_lastPlayerController.name.Contains("FlyCam"))
                    {
                        return;
                    }

                    m_rigidBodyController = m_lastPlayerController.GetComponent<Rigidbody>();
                    if (m_rigidBodyController == null)
                    {
                        m_rigidBodyController = GetComponentInChildren<Rigidbody>();
                    }

                    if (m_rigidBodyController != null)
                    {
                        m_lastConstrants = m_rigidBodyController.constraints;
                        m_rigidBodyController.constraints = RigidbodyConstraints.FreezeAll;
                    }
                }

                if (m_lastPlayerController.name.Contains("ThirdPersonController"))
                {
                    m_thirdPersonBaseController = m_lastPlayerController.GetComponent<ThirdPersonUserControl>();
                    if (m_thirdPersonBaseController == null)
                    {
                        m_thirdPersonBaseController = m_lastPlayerController.GetComponentInChildren<ThirdPersonUserControl>();
                    }

                    if (m_thirdPersonBaseController != null)
                    {
                        m_thirdPersonBaseController.enabled = false;
                    }
                }
                else if (m_lastPlayerController.name.Contains("Gaia Car"))
                {
                    m_carBaseController = m_lastPlayerController.GetComponent<CarUserControl>();
                    if (m_carBaseController == null)
                    {
                        m_carBaseController = m_lastPlayerController.GetComponentInChildren<CarUserControl>();
                    }

                    if (m_carBaseController != null)
                    {
                        m_carBaseController.enabled = false;
                    }

                    m_carAudioController = m_lastPlayerController.GetComponent<CarAudio>();
                    if (m_carAudioController == null)
                    {
                        m_carAudioController = m_lastPlayerController.GetComponentInChildren<CarAudio>();
                    }

                    if (m_carAudioController != null)
                    {
                        m_carAudioController.enabled = false;
                    }
                }

                if (m_playerController != null)
                {
                    m_playerController.enabled = false;
                }
            }

            m_lastPlayerCamera = GaiaUtils.GetCamera();
            if (m_lastPlayerCamera != null)
            {
                if (m_targetCamera != null)
                {
                    m_targetCamera.transform.SetPositionAndRotation(m_lastPlayerCamera.transform.position, m_lastPlayerCamera.transform.rotation);
                }
                m_lastPlayerCamera.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// Un-freezes the player controller
        /// </summary>
        private void UnFreezePlayerController()
        {
            if (m_freezePlayerController)
            {
                if (m_rigidBodyController != null)
                {
                    m_rigidBodyController.constraints = m_lastConstrants;
                }

                if (m_thirdPersonBaseController != null)
                {
                    m_thirdPersonBaseController.enabled = true;
                }

                if (m_playerController != null)
                {
                    m_playerController.enabled = true;
                }

                if (m_carBaseController != null)
                {
                    m_carBaseController.enabled = true;
                }

                if (m_carAudioController != null)
                {
                    m_carAudioController.enabled = true;
                }
            }

            if (m_lastPlayerCamera != null)
            {
                m_lastPlayerCamera.gameObject.SetActive(true);
            }
        }

#endregion
#region Setup Functions

        /// <summary>
        /// Setup function used to load settings and variables and creates the UI
        /// </summary>
        public void Setup()
        {
            if (!VerifySystems())
            {
                Debug.LogError("Verifying Photo Mode UI failed, maybe something is null, checked the photo mode prefab to make sure nothing has been assigned.");
                return;
            }

            PhotoModeUtils.m_runtimeUIPrefab = m_runtimeUIPrefab;
            PhotoMode.CurrentRuntimeUIElements.Clear();

            //Is present
#if GAIA_PRO_PRESENT
            m_weather = ProceduralWorldsGlobalWeather.Instance;
            m_pwWeatherPresent = m_weather != null;

#if HDPipeline && UNITY_2021_2_OR_NEWER
            m_hdrpTimeOfDay = HDRPTimeOfDay.Instance;
#endif
#endif

            if (m_screenShotter == null)
            {
               m_screenShotter = GaiaUtils.FindOOT<ScreenShotter>();
            }
            if (m_screenShotter != null)
            {
                if (m_screenshotText != null)
                {
                    m_screenshotText.text = string.Format("Screenshot {0}", m_screenShotter.m_screenShotKey);
                }
            }

            List<Transform> prefabs = new List<Transform>(3);
            if (m_runtimeUIPrefab != null)
            {
                prefabs.Add(m_runtimeUIPrefab.transform);
            }
            if (m_runtimeUIPrefab != null)
            {
                prefabs.Add(m_runtimeUIPrefab.transform);
            }
            if (m_runtimeUIPrefab != null)
            {
                prefabs.Add(m_runtimeUIPrefab.transform);
            }

            //Clear current
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_photoMode, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_camera, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_unity, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_terrain, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_lighting, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_water, prefabs);
            PhotoModeUtils.RemoveAllChildren(m_transformSettings.m_postFX, prefabs);

            //Create new runtime UI
            CreatePhotoModeSettingsUI();
            CreateUnitySettingsUI();
            CreateCameraSettingsUI();
            CreateWaterSettingsUI();
            CreatePostFXSettingsUI();
            CreateTerrainSettingsUI();
            CreateLightingSettingsUI();
            CreateGrassSettingsUI();
            CreateStreamingSettingsUI();

            //Load From Profile
            if (m_gaiaUI != null)
            {
                if (m_gaiaUI.m_loadFromLastSaved)
                {
                    if (m_sceneProfile != null)
                    {
                        if (m_photoModeProfile != null)
                        {
                            bool save = false;
                            if (m_photoModeProfile.m_everBeenSaved)
                            {
                                if (m_photoModeProfile.LastRenderPipeline == m_renderPipeline)
                                {
                                    PhotoModeValues values = GaiaAPI.LoadPhotoModeValues();
                                    if (values != null)
                                    {
                                        if (values.m_selectedGaiaLightingProfile == m_sceneProfile.m_selectedLightingProfileValuesIndex)
                                        {
                                            if (values.m_lastSceneName == SceneManager.GetActiveScene().name)
                                            {
                                                m_photoModeValues.Load(values);
                                                RefreshAllUI();
                                            }
                                            else
                                            {
                                                save = true;
                                            }
                                        }
                                        else
                                        {
                                            save = true;
                                        }
                                    }
                                }

                                if (save)
                                {
                                    GaiaAPI.SavePhotoModeValues(m_photoModeValues, m_renderPipeline);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets all the system metrics data
        /// </summary>
        public void SetupSystemMetrics()
        {
            //Grab information about the system
            try
            {
                m_stormVersion = "Unity v" + Application.unityVersion;
                m_deviceName = SystemInfo.deviceName;
                m_deviceType = SystemInfo.deviceType.ToString();
                m_OS = SystemInfo.operatingSystem;
                m_platform = Application.platform.ToString();
                m_processor = SystemInfo.processorType + " - " + SystemInfo.processorCount + " cores";
                m_gpu = SystemInfo.graphicsDeviceName;
                m_gpuDevice = SystemInfo.graphicsDeviceType + " - " + SystemInfo.graphicsDeviceVersion;
                m_gpuCapabilities = "";
                m_gpuCapabilities += "TA: " + SystemInfo.supports2DArrayTextures.ToString();
                m_gpuCapabilities += ", MT: " + SystemInfo.maxTextureSize.ToString();
                m_gpuCapabilities += ", NPOT: " + SystemInfo.npotSupport.ToString();
                m_gpuCapabilities += ", RTC: " + SystemInfo.supportedRenderTargetCount.ToString();
                m_gpuCapabilities += ", CT: " + SystemInfo.copyTextureSupport.ToString();

                int sm = SystemInfo.graphicsShaderLevel;
                if (sm >= 10 && sm <= 99)
                {
                    // getting first and second digits from sm
                    m_gpuSpec = "SM: " + (sm /= 10) + '.' + (sm / 10);
                }
                else
                {
                    m_gpuSpec = "SM: N/A";
                }

                int vram = SystemInfo.graphicsMemorySize;
                if (vram > 0)
                {
                    m_gpuSpec += ", VRAM: " + vram + " MB";
                }
                else
                {
                    m_gpuSpec += ", VRAM: " + vram + " N/A";
                }

                int ram = SystemInfo.systemMemorySize;
                if (ram > 0)
                {
                    m_ram = ram.ToString();
                }
                else
                {
                    m_ram = "N/A";
                }

                Resolution res = Screen.currentResolution;
#if UNITY_2022_2_OR_NEWER
                m_screenInfo = res.width + "x" + res.height + " @" + res.refreshRateRatio + " Hz [window size: " +
#else
                m_screenInfo = res.width + "x" + res.height + " @" + res.refreshRate + " Hz [window size: " +
#endif
                               Screen.width + "x" + Screen.height;
                float dpi = Screen.dpi;
                if (dpi > 0)
                {
                    m_screenInfo += ", DPI: " + dpi + "]";
                }
                else
                {
                    m_screenInfo += "]";
                }

                m_deviceModel = SystemInfo.deviceModel;
                m_quality = QualitySettings.GetQualityLevel().ToString();
            }
            catch (Exception ex)
            {
                Debug.Log("Problem getting system metrics : " + ex.Message);
            }

            //Update UX if it is there
            if (m_StormVersionText != null)
            {
                m_StormVersionText.text = PhotoModeUtils.UpdateWrap(m_stormVersion);
            }

            if (m_OSText != null)
            {
                m_OSText.text = PhotoModeUtils.UpdateWrap(m_OS);
            }

            if (m_deviceText != null)
            {
                m_deviceText.text = PhotoModeUtils.UpdateWrap(m_deviceName + ", " + m_platform + ", " + m_deviceType);
            }

            if (m_systemText != null)
            {
                m_systemText.text = PhotoModeUtils.UpdateWrap(m_deviceModel + ", " + m_processor + ", " + m_ram + " GB");
            }

            if (m_gpuText != null)
            {
                m_gpuText.text = PhotoModeUtils.UpdateWrap(m_gpu + ", " + m_gpuSpec + ", QUAL: " + m_quality);
            }

            if (m_gpuCapabilitiesText != null)
            {
                m_gpuCapabilitiesText.text = PhotoModeUtils.UpdateWrap(m_gpuDevice + ", " + m_gpuCapabilities);
            }

            if (m_screenInfoText != null)
            {
                m_screenInfoText.text = PhotoModeUtils.UpdateWrap(m_screenInfo);
            }
        }
        /// <summary>
        /// Updates the Runtime UI
        /// </summary>
        public void UpdateUI()
        {
            m_isUpdatingValues = true;

            if (m_sceneProfile == null)
            {
                return;
            }

            if (m_sceneProfile.m_selectedLightingProfileValuesIndex != m_savedLightingProfileIndex)
            {
                SaveStartValues();
            }

#if GAIA_PRO_PRESENT
            if (m_sceneProfile.m_lightSystemMode == GaiaConstants.GlobalSystemMode.Gaia)
            {
                bool skyIsSet =
                    m_sceneProfile.m_lightingProfiles[m_sceneProfile.m_selectedLightingProfileValuesIndex]
                        .m_profileType == GaiaConstants.GaiaLightingProfileType.ProceduralWorldsSky;
                if (skyIsSet != m_pwWeatherPresent && m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    m_pwWeatherPresent = skyIsSet;
                    m_weather = ProceduralWorldsGlobalWeather.Instance;
                    SetMetricsParent(false, m_transformSettings.m_photoMode);
                    Setup();
                }
                else
                {
                    if (skyIsSet != m_hdrpTimeOfDay)
                    {
                        m_hdrpTimeOfDay = skyIsSet;
                        SetMetricsParent(false, m_transformSettings.m_photoMode);
                        Setup();
                    }
                }
            }

            if (m_pwWeatherPresent)
            {
                GaiaTimeOfDay tod = m_sceneProfile.m_gaiaTimeOfDay;
                if (m_photoModeValues.m_gaiaTimeOfDayEnabled != tod.m_todEnabled)
                {
                    SetGaiaTimeOfDayEnabled(PhotoModeUtils.ConvertBoolToInt(tod.m_todEnabled));
                }
                if (m_photoModeValues.m_gaiaTimeScale != tod.m_todDayTimeScale)
                {
                    SetGaiaTimeScale(tod.m_todDayTimeScale);
                }
                if (tod.m_todEnabled)
                {
                    float t = tod.m_todHour + (tod.m_todMinutes / 60f);
                    if (m_photoModeValues.m_gaiaTime != t)
                    {
                        SetGaiaTime(t);
                    }
                }
            }
            else if (m_hdrpTimeOfDay)
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float autoUpdateValue);
                if (m_photoModeValues.m_gaiaTimeOfDayEnabled != autoUpdate)
                {
                    SetGaiaTimeOfDayEnabled(PhotoModeUtils.ConvertBoolToInt(autoUpdate));
                }
                if (m_photoModeValues.m_gaiaTimeScale != autoUpdateValue)
                {
                    SetGaiaTimeScale(autoUpdateValue);
                }
                SetGaiaTime(HDRPTimeOfDayAPI.GetCurrentTime());
                m_gaiaTime.SyncHDRPTimeOfDay();
#endif
            }

            if (m_weather != null)
            {
                if (m_photoModeValues.m_gaiaWeatherEnabled != (m_weather.EnableRain && m_weather.EnableSnow))
                {
                    SetGaiaWeatherEnabled(PhotoModeUtils.ConvertBoolToInt(m_weather.EnableRain || m_weather.EnableSnow));
                }

                if (m_photoModeValues.m_gaiaWeatherRain != m_weather.IsRaining)
                {
                    OnSetRain(m_weather.IsRaining, true);
                    m_photoModeValues.m_gaiaWeatherRain = m_weather.IsRaining;
                }

                if (m_photoModeValues.m_gaiaWeatherSnow != m_weather.IsSnowing)
                {
                    OnSetSnow(m_weather.IsSnowing, true);
                    m_photoModeValues.m_gaiaWeatherSnow = m_weather.IsSnowing;
                }

                if (m_photoModeValues.m_gaiaWindDirection != m_weather.WindDirection)
                {
                    SetGaiaWindDirection(m_weather.WindDirection);
                }

                if (m_photoModeValues.m_gaiaWindSpeed != m_weather.WindSpeed)
                {
                    SetGaiaWindSpeed(m_weather.WindSpeed);
                }
            }
#endif
            if (m_gaiaUI.m_loadFromLastSaved)
            {
                GaiaAPI.SavePhotoModeValues(m_photoModeValues, m_renderPipeline);
            }

            m_isUpdatingValues = false;
        }
        /// <summary>
        /// Sets the new min/max values for photo mode
        /// </summary>
        /// <param name="values"></param>
        public void SetMinMax(PhotoModeMinAndMaxValues values)
        {
            if (values == null)
            {
                return;
            }

            m_minAndMaxValues.SetNewMinMaxValues(values);
        }
        /// <summary>
        /// Process system metrics parenting
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transform"></param>
        private void SetMetricsParent(bool parent, RectTransform transform)
        {
            if (transform == null || m_transformSettings.m_photoMode == null)
            {
                return;
            }

            if (parent)
            {
                m_StormVersionText.transform.SetParent(m_transformSettings.m_photoMode);
                m_OSText.transform.SetParent(m_transformSettings.m_photoMode);
                m_deviceText.transform.SetParent(m_transformSettings.m_photoMode);
                m_systemText.transform.SetParent(m_transformSettings.m_photoMode);
                m_gpuText.transform.SetParent(m_transformSettings.m_photoMode);
                m_gpuCapabilitiesText.transform.SetParent(m_transformSettings.m_photoMode);
                m_screenInfoText.transform.SetParent(m_transformSettings.m_photoMode);
            }
            else
            {
                m_StormVersionText.transform.SetParent(null);
                m_OSText.transform.SetParent(null);
                m_deviceText.transform.SetParent(null);
                m_systemText.transform.SetParent(null);
                m_gpuText.transform.SetParent(null);
                m_gpuCapabilitiesText.transform.SetParent(null);
                m_screenInfoText.transform.SetParent(null);
            }
        }
        /// <summary>
        /// Creates photo mode UI
        /// </summary>
        private void CreatePhotoModeSettingsUI()
        {
            if (m_transformSettings.m_photoMode != null)
            {
                if (m_screenShotter != null && m_gaiaUI != null)
                {
                    if (m_screenShotter.m_useScreenSize)
                    {
                        m_photoModeValues.m_screenshotResolution = 0;
                    }
                    else
                    {
                        m_photoModeValues.m_screenshotResolution = (int) m_screenShotter.m_screenshotResolution;
                        m_photoModeValues.m_screenshotResolution++;
                    }

                    m_photoModeValues.m_screenshotImageFormat = (int)m_screenShotter.m_imageFormat;

                    GaiaAPI.GetPhotoModeSettings(out m_photoModeValues.m_loadSavedSettings, out m_photoModeValues.m_revertOnDisabled, out m_photoModeValues.m_showReticle, out m_photoModeValues.m_showRuleOfThirds, out m_gaiaUI.m_enablePhotoMode);

                    PhotoModeUtils.CreateTitleHeader(ref m_photoModeHeader, m_transformSettings.m_photoMode, "General Settings");
                    PhotoModeUtils.CreateDropdown(ref m_screenshotResolution, m_transformSettings.m_photoMode, "Screenshot Res.", m_photoModeValues.m_screenshotResolution, SetUnityScreenshotResolution, m_photoModeValues.GetScreenResolutionOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_screenshotImageFormat, m_transformSettings.m_photoMode, "Screenshot Format", m_photoModeValues.m_screenshotImageFormat, SetUnityScreenshotImageFormat, m_photoModeValues.GetScreenshotFormatOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_showFPS, m_transformSettings.m_photoMode, "Show FPS", m_photoModeValues.m_showFPS, SetPhotoModeShowFPS, m_photoModeValues.GetDefaultToggleOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_showReticule, m_transformSettings.m_photoMode, "Show Reticule", m_photoModeValues.m_showReticle, SetPhotoModeShowReticule, m_photoModeValues.GetDefaultToggleOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_showRuleOfThirds, m_transformSettings.m_photoMode, "Show Rule Of Thirds", m_photoModeValues.m_showRuleOfThirds, SetPhotoModeShowRuleOfThirds, m_photoModeValues.GetDefaultToggleOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_loadSavedSettings, m_transformSettings.m_photoMode, "Load Last Settings", m_photoModeValues.m_loadSavedSettings, SetPhotoModeLoadSettings, m_photoModeValues.GetDefaultToggleOptions(), true);
                    PhotoModeUtils.CreateDropdown(ref m_resetPhotoModeOnDisable, m_transformSettings.m_photoMode, "Reset When Closed", m_photoModeValues.m_revertOnDisabled, SetPhotoModeRevertOnDisabledSettings, m_photoModeValues.GetDefaultToggleOptions(), true);
                    //SetMetricsParent(true, m_transformSettings.m_photoMode);
                }
            }
        }
        /// <summary>
        /// Creates unity UI
        /// </summary>
        private void CreateUnitySettingsUI()
        {
            if (m_transformSettings.m_unity != null)
            {
                m_photoModeValues.m_vSync = QualitySettings.vSyncCount;
                m_photoModeValues.m_targetFPS = Application.targetFrameRate;
                m_photoModeValues.m_globalVolume = AudioListener.volume;
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
                        m_photoModeValues.m_lodBias = QualitySettings.lodBias;
                        m_photoModeValues.m_shadowDistance = QualitySettings.shadowDistance;
                        m_photoModeValues.m_shadowCascades = QualitySettings.shadowCascades;
                        m_photoModeValues.m_shadowResolution = (int)QualitySettings.shadowResolution;
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
#if UPPipeline
                        m_photoModeValues.m_lodBias = QualitySettings.lodBias;
                        m_photoModeValues.m_shadowDistance = GaiaAPI.GetURPShadowDistance();
                        m_photoModeValues.m_shadowCascades = GaiaAPI.GetURPShadowCasecade();
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        m_photoModeValues.m_lodBias = GaiaAPI.GetHDRPLODBias(m_targetCamera);
#endif
                        break;
                    }
                }

                PhotoModeUtils.CreateTitleHeader(ref m_unitySettingsGeneralHeader, m_transformSettings.m_unity, "General Settings");
                PhotoModeUtils.CreateSlider(ref m_unityVolume, m_transformSettings.m_unity, "Global Volume", m_photoModeValues.m_globalVolume, m_minAndMaxValues.m_globalVolume.x, m_minAndMaxValues.m_globalVolume.y, SetUnityVolume, SetUnityVolume, true);

                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
                        PhotoModeUtils.CreateSlider(ref m_unityLODBias, m_transformSettings.m_unity, "LOD Bias", m_photoModeValues.m_lodBias, m_minAndMaxValues.m_lodBias.x, m_minAndMaxValues.m_lodBias.y, SetUnityLODBias, SetUnityLODBias, true);
                        PhotoModeUtils.CreateTitleHeader(ref m_unitySettingsShadowHeader, m_transformSettings.m_unity, "Shadow Settings");
                        PhotoModeUtils.CreateSlider(ref m_unityShadowDistance, m_transformSettings.m_unity, "Shadow Distance", m_photoModeValues.m_shadowDistance, m_minAndMaxValues.m_shadowDistance.x, m_minAndMaxValues.m_shadowDistance.y, SetUnityShadowDistance, SetUnityShadowDistance, true);
                        PhotoModeUtils.CreateDropdown(ref m_unityShadowCascades, m_transformSettings.m_unity, "Shadow Cascades", m_photoModeValues.m_shadowCascades, SetUnityShadowCascades, m_photoModeValues.GetShadowCascadeOptions(), true);
                        PhotoModeUtils.CreateDropdown(ref m_unityShadownResolution, m_transformSettings.m_unity, "Shadow Resolution", m_photoModeValues.m_shadowResolution, SetUnityShadowResolution, m_photoModeValues.GetShadowQualityOptions(), true);
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
                        PhotoModeUtils.CreateSlider(ref m_unityLODBias, m_transformSettings.m_unity, "LOD Bias", m_photoModeValues.m_lodBias, m_minAndMaxValues.m_lodBias.x, m_minAndMaxValues.m_lodBias.y, SetUnityLODBias, SetUnityLODBias, true);
                        PhotoModeUtils.CreateTitleHeader(ref m_unitySettingsShadowHeader, m_transformSettings.m_unity, "Shadow Settings");
                        PhotoModeUtils.CreateSlider(ref m_unityShadowDistance, m_transformSettings.m_unity, "Shadow Distance", m_photoModeValues.m_shadowDistance, m_minAndMaxValues.m_shadowDistance.x, m_minAndMaxValues.m_shadowDistance.y, SetUnityShadowDistance, SetUnityShadowDistance, true);
                        PhotoModeUtils.CreateDropdown(ref m_unityShadowCascades, m_transformSettings.m_unity, "Shadow Cascades", m_photoModeValues.m_shadowCascades, SetUnityShadowCascades, m_photoModeValues.GetShadowCascadeOptions(), true);
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
                        PhotoModeUtils.CreateSlider(ref m_unityLODBias, m_transformSettings.m_unity, "LOD Bias", m_photoModeValues.m_lodBias, 0.001f, 10f, SetUnityLODBias, SetUnityLODBias, true);
                        PhotoModeUtils.CreateTitleHeader(ref m_unitySettingsShadowHeader, m_transformSettings.m_unity, "Shadow Settings");
                        PhotoModeUtils.CreateSlider(ref m_unityShadowDistance, m_transformSettings.m_unity, "Shadow Distance", m_photoModeValues.m_shadowDistance, 0, 5000, SetUnityShadowDistance, SetUnityShadowDistance, true);
                        PhotoModeUtils.CreateDropdown(ref m_unityShadowCascades, m_transformSettings.m_unity, "Shadow Cascades", m_photoModeValues.m_shadowCascades, SetUnityShadowCascades, m_photoModeValues.GetShadowCascadeOptions(), true);
                        break;
                    }
                }

                PhotoModeUtils.CreateTitleHeader(ref m_unitySettingsVSyncHeader, m_transformSettings.m_unity, "V-Sync Settings");
                PhotoModeUtils.CreateDropdown(ref m_unityVSync, m_transformSettings.m_unity, "V-Sync Count", m_photoModeValues.m_vSync, SetUnityVSync, m_photoModeValues.GetVsyncOptions(), true);
                PhotoModeUtils.CreateIntSlider(ref m_unityTargetFPS, m_transformSettings.m_unity, "Target FPS", m_photoModeValues.m_targetFPS, m_minAndMaxValues.m_targetFPS.x, m_minAndMaxValues.m_targetFPS.y, SetUnityTargetFPS, SetUnityTargetFPS, true);
                if (m_unityTargetFPS != null)
                {
                    m_unityTargetFPS.gameObject.SetActive(m_photoModeValues.m_vSync == 0);
                }
            }
        }
        /// <summary>
        /// Creates camera UI
        /// </summary>
        private void CreateCameraSettingsUI()
        {
            if (m_transformSettings.m_unity != null)
            {
                m_photoModeValues.m_vSync = QualitySettings.vSyncCount;
                m_photoModeValues.m_targetFPS = Application.targetFrameRate;
                m_photoModeValues.m_globalVolume = AudioListener.volume;
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_postProcessingLayer != null)
                        {
                            m_photoModeValues.m_antiAliasing = (int)m_postProcessingLayer.antialiasingMode;
                        }
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
#if UPPipeline
                        m_photoModeValues.m_antiAliasing = GaiaAPI.GetURPAntiAliasingMode();
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        m_photoModeValues.m_antiAliasing = GaiaAPI.GetHDRPAntiAliasingMode();
#endif
                        break;
                    }
                }

                PhotoModeUtils.CreateTitleHeader(ref m_cameraSettingsGeneralHeader, m_transformSettings.m_camera, "General Settings");
                PhotoModeUtils.CreateDropdown(ref m_unityAA, m_transformSettings.m_camera, "Anti-Alias Level", m_photoModeValues.m_antiAliasing, SetUnityAA, m_photoModeValues.GetAntiAliasingOptions(), true);
                if (m_sceneProfile != null)
                {
                    GaiaAPI.GetCullingSettings(out m_photoModeValues.m_gaiaCullinDistance);
                    PhotoModeUtils.CreateSlider(ref m_cullingDistance, m_transformSettings.m_camera, "Extra Culling Distance", m_photoModeValues.m_gaiaCullinDistance, m_minAndMaxValues.m_gaiaCullinDistance.x, m_minAndMaxValues.m_gaiaCullinDistance.y, SetUnityCullingDistance, SetUnityCullingDistance, true);
                }
                if (m_targetCamera != null)
                {
                    m_photoModeValues.m_fieldOfView = m_targetCamera.fieldOfView;
                    m_photoModeValues.m_farClipPlane = m_targetCamera.farClipPlane;
                    m_photoModeValues.m_cameraRoll = GaiaAPI.GetCameraRoll(m_targetCamera);
                    PhotoModeUtils.CreateSlider(ref m_fieldOfView, m_transformSettings.m_camera, "Field Of View", m_photoModeValues.m_fieldOfView, m_minAndMaxValues.m_fieldOfView.x, m_minAndMaxValues.m_fieldOfView.y, SetUnityFieldOfView, SetUnityFieldOfView, true);
                    PhotoModeUtils.CreateSlider(ref m_cameraRoll, m_transformSettings.m_camera, "Camera Roll", m_photoModeValues.m_cameraRoll, m_minAndMaxValues.m_cameraRoll.x, m_minAndMaxValues.m_cameraRoll.y, SetUnityCameraRoll, SetUnityCameraRoll, true);
                    GaiaAPI.SetCameraRoll(m_photoModeValues.m_cameraRoll, m_targetCamera);
                }

                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
                        if (m_unityAA != null)
                        {
#if UNITY_POST_PROCESSING_STACK_V2
                            m_unityAA.gameObject.SetActive(m_postProcessingLayer != null);
#endif
                        }
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
                        if (m_unityAA != null)
                        {
#if UPPipeline
                            m_unityAA.gameObject.SetActive(m_urpCameraData != null);
#endif
                        }
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
                        if (m_unityAA != null)
                        {
#if HDPipeline
                            m_unityAA.gameObject.SetActive(m_hdrpCameraData != null);
#endif
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Creates water UI
        /// </summary>
        private void CreateWaterSettingsUI()
        {
            if (m_transformSettings.m_water != null)
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                    m_photoModeValues.m_gaiaWaterReflectionEnabled = sceneProfile.m_enableReflections;
                    m_photoModeValues.m_gaiaWaterReflectionDistance = 0f;
                    m_photoModeValues.m_gaiaWaterReflectionResolution = GaiaAPI.GetWaterResolutionQuality();
                }

                PhotoModeUtils.CreateTitleHeader(ref m_gaiaWaterheader, m_transformSettings.m_water, "Ocean Settings");
                PhotoModeUtils.CreateDropdown(ref m_gaiaWaterReflectionsEnabled, m_transformSettings.m_water, "Reflections Enabled", m_photoModeValues.m_gaiaWaterReflectionEnabled, SetGaiaWaterReflectionEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                PhotoModeUtils.CreateDropdown(ref m_gaiaWaterReflectionResolution, m_transformSettings.m_water, "Reflection Resolution", m_photoModeValues.m_gaiaWaterReflectionResolution, SetGaiaWaterReflectionResolution, m_photoModeValues.GetWaterReflectionQualityOptions(), true);
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        m_photoModeValues.m_gaiaReflectionsLODBias = GaiaAPI.GetHDRPWaterLODBias();
#endif
                        PhotoModeUtils.CreateSlider(ref m_gaiaWaterReflectionLODBias, m_transformSettings.m_water, "Reflection LOD Bias", m_photoModeValues.m_gaiaReflectionsLODBias, m_minAndMaxValues.m_lodBias.x, m_minAndMaxValues.m_lodBias.y, SetGaiaWaterReflectionLODBias, SetGaiaWaterReflectionLODBias, true);
                        break;
                    }
                    default:
                    {
                        PhotoModeUtils.CreateSlider(ref m_gaiaWaterReflectionDistance, m_transformSettings.m_water, "Extra Reflection Distance", m_photoModeValues.m_gaiaWaterReflectionDistance, m_minAndMaxValues.m_gaiaWaterReflectionDistance.x, m_minAndMaxValues.m_gaiaWaterReflectionDistance.y, SetGaiaWaterReflectionDistance, SetGaiaWaterReflectionDistance, true);
                        break;
                    }
                }

                if (GaiaUnderwaterEffects.Instance != null)
                {
                    m_photoModeValues.m_gaiaUnderwaterFogColor = GaiaAPI.GetUnderwaterFogColor();
                    GaiaAPI.GetUnderwaterFogDensity(out m_photoModeValues.m_gaiaUnderwaterFogDensity, out m_photoModeValues.m_gaiaUnderwaterFogDistance);
                    m_photoModeValues.m_gaiaUnderwaterVolume = GaiaAPI.GetUnderwaterVolume();
                    PhotoModeUtils.CreateTitleHeader(ref m_gaiaUnderwaterheader, m_transformSettings.m_water, "Underwater Settings");
                    PhotoModeUtils.CreateColorField(ref m_gaiaUnderwaterFogColor, m_transformSettings.m_water, "Extra Underwater Fog Color", m_photoModeValues.m_gaiaUnderwaterFogColor, false, OpenColorPickerUnderwaterFogColor, true);
                    switch (m_renderPipeline)
                    {
                        case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            PhotoModeUtils.CreateSlider(ref m_gaiaUnderwaterFogDistance, m_transformSettings.m_water, "Underwater Fog Distance", m_photoModeValues.m_gaiaUnderwaterFogDistance, m_minAndMaxValues.m_gaiaUnderwaterFogDistance.x, m_minAndMaxValues.m_gaiaUnderwaterFogDistance.y, SetGaiaUnderwaterFogDistance, SetGaiaUnderwaterFogDistance, true);
                            break;
                        }
                        default:
                        {
                            switch (RenderSettings.fogMode)
                            {
                                case FogMode.Linear:
                                {
                                    PhotoModeUtils.CreateSlider(ref m_gaiaUnderwaterFogDistance, m_transformSettings.m_water, "Underwater Fog Distance", m_photoModeValues.m_gaiaUnderwaterFogDistance, m_minAndMaxValues.m_gaiaUnderwaterFogDistance.x, m_minAndMaxValues.m_gaiaUnderwaterFogDistance.y, SetGaiaUnderwaterFogDistance, SetGaiaUnderwaterFogDistance, true);
                                    break;
                                }
                                default:
                                {
                                    PhotoModeUtils.CreateSlider(ref m_gaiaUnderwaterFogDistance, m_transformSettings.m_water, "Underwater Fog Distance", m_photoModeValues.m_gaiaUnderwaterFogDensity, m_minAndMaxValues.m_gaiaUnderwaterFogDensity.x, m_minAndMaxValues.m_gaiaUnderwaterFogDensity.y, SetGaiaUnderwaterFogDistance, SetGaiaUnderwaterFogDistance, true);
                                    break;
                                }
                            }

                            break;
                        }
                    }
                    PhotoModeUtils.CreateSlider(ref m_gaiaUnderwaterVolume, m_transformSettings.m_water, "Underwater FX Volume", m_photoModeValues.m_gaiaUnderwaterVolume, 0f, 1f, SetGaiaUnderwaterVolume, SetGaiaUnderwaterVolume, true);
                }

                if (!m_photoModeValues.m_gaiaWaterReflectionEnabled)
                {
                    if (m_gaiaWaterReflectionLODBias != null)
                    {
                        m_gaiaWaterReflectionLODBias.gameObject.SetActive(false);
                    }
                    if (m_gaiaWaterReflectionDistance != null)
                    {
                        m_gaiaWaterReflectionDistance.gameObject.SetActive(false);
                    }
                    if (m_gaiaWaterReflectionResolution != null)
                    {
                        m_gaiaWaterReflectionResolution.gameObject.SetActive(false);
                    }
                }
            }
        }
        /// <summary>
        /// Creates post fx UI
        /// </summary>
        private void CreatePostFXSettingsUI()
        {
            if (m_transformSettings.m_postFX != null)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        bool exposureExists = GaiaAPI.GetPostFXExposure(out m_photoModeValues.m_postFXExposure);
                        m_depthOfField = GaiaAPI.GetDepthOfFieldSettings();
                        m_photoModeValues.m_autoDOFFocus = GaiaAPI.GetAutoFocusDepthOfField();

                        if (exposureExists && !m_pwWeatherPresent)
                        {
                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXExposureHeader, m_transformSettings.m_postFX, "Exposure Settings");
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXExposure, m_transformSettings.m_postFX, "Exposure", m_photoModeValues.m_postFXExposure, m_minAndMaxValues.m_postFXExposure.x, m_minAndMaxValues.m_postFXExposure.y, SetAutoExposure, SetAutoExposure, true);
                        }
                        else
                        {
                            if (m_gaiaPostFXExposure != null)
                            {
                                m_gaiaPostFXExposure.gameObject.SetActive(false);
                            }
                        }

                        if (m_depthOfField != null)
                        {
                            m_photoModeValues.m_dofActive = m_depthOfField.active;
                            m_photoModeValues.m_dofFocusDistance = m_depthOfField.focusDistance.value;
                            m_photoModeValues.m_dofAperture = m_depthOfField.aperture.value;
                            m_photoModeValues.m_dofFocalLength = m_depthOfField.focalLength.value;
                            m_photoModeValues.m_dofKernelSize = (int)m_depthOfField.kernelSize.value;

                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXDOFHeader, m_transformSettings.m_postFX, "Depth Of Field Settings");
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFEnabled, m_transformSettings.m_postFX, "Enable Depth Of Field", m_photoModeValues.m_dofActive, SetDOFEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFAutoFocus, m_transformSettings.m_postFX, "Auto Focus", m_photoModeValues.m_autoDOFFocus, SetDOFAutoFocusEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFKernelSizeBuiltIn, m_transformSettings.m_postFX, "Blur Size", (int)m_photoModeValues.m_dofKernelSize, SetDOFKernelSize, m_photoModeValues.GetKernelSizeOptions(), true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocusDistance, m_transformSettings.m_postFX, "Focus Distance", m_photoModeValues.m_dofFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistance.x, m_minAndMaxValues.m_postFXDOFFocusDistance.y, SetDOFFocusDistance, SetDOFFocusDistance, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFAperture, m_transformSettings.m_postFX, "Aperture", m_photoModeValues.m_dofAperture, m_minAndMaxValues.m_cameraAperture.x, m_minAndMaxValues.m_cameraAperture.y, SetDOFAperture, SetDOFAperture, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocalLength, m_transformSettings.m_postFX, "Focal Length", m_photoModeValues.m_dofFocalLength, m_minAndMaxValues.m_cameraFocalLength.x, m_minAndMaxValues.m_cameraFocalLength.y, SetDOFFocalLength, SetDOFFocalLength, true);

                            if (m_photoModeValues.m_autoDOFFocus)
                            {
                                if (m_gaiaPostFXDOFFocusDistance != null)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                                }
                            }
                            if (!m_photoModeValues.m_dofActive)
                            {
                                if (m_gaiaPostFXDOFAutoFocus != null)
                                {
                                    m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFFocusDistance != null)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFAperture != null)
                                {
                                    m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFFocalLength != null)
                                {
                                    m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFKernelSizeBuiltIn != null)
                                {
                                    m_gaiaPostFXDOFKernelSizeBuiltIn.gameObject.SetActive(false);
                                }
                            }
                        }
#else
                        PhotoModeUtils.CreateTitleHeader(ref m_noPostProcessingHeader, m_transformSettings.m_postFX, "No post processing was found in your scene.");
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
#if UPPipeline
                        bool exposureExists = GaiaAPI.GetPostExposureURP(out m_photoModeValues.m_postFXExposure);
                        if (exposureExists)
                        {
                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXExposureHeader, m_transformSettings.m_postFX, "Exposure Settings");
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXExposure, m_transformSettings.m_postFX, "Exposure", m_photoModeValues.m_postFXExposure, -5f, 5f, SetAutoExposure, SetAutoExposure, true);
                        }
                        else
                        {
                            if (m_gaiaPostFXExposure != null)
                            {
                                m_gaiaPostFXExposure.gameObject.SetActive(false);
                            }
                        }

                        m_depthOfFieldURP = GaiaAPI.GetDepthOfFieldSettingsURP();
                        m_photoModeValues.m_autoDOFFocus = GaiaAPI.GetAutoFocusDepthOfField();
                        if (m_depthOfFieldURP != null)
                        {
                            m_photoModeValues.m_dofActive = m_depthOfFieldURP.active;
                            m_photoModeValues.m_dofFocusModeURP = (int)m_depthOfFieldURP.mode.value;
                            m_photoModeValues.m_dofFocusDistance = m_depthOfFieldURP.focusDistance.value;
                            m_photoModeValues.m_dofAperture = m_depthOfFieldURP.aperture.value;
                            m_photoModeValues.m_dofFocalLength = m_depthOfFieldURP.focalLength.value;
                            m_photoModeValues.m_dofStartBlurURP = m_depthOfFieldURP.gaussianStart.value;
                            m_photoModeValues.m_dofEndBlurURP = m_depthOfFieldURP.gaussianEnd.value;
                            m_photoModeValues.m_dofMaxRadiusBlur = m_depthOfFieldURP.gaussianMaxRadius.value;
                            m_photoModeValues.m_dofHighQualityURP = m_depthOfFieldURP.highQualitySampling.value;

                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXDOFHeader, m_transformSettings.m_postFX, "Depth Of Field Settings");
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFEnabled, m_transformSettings.m_postFX, "Enable Depth Of Field", m_photoModeValues.m_dofActive, SetDOFEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFModeURP, m_transformSettings.m_postFX, "Mode", m_photoModeValues.m_dofFocusModeURP, SetDOFModeURP, m_photoModeValues.GetDOFModeOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFHighQualityURP, m_transformSettings.m_postFX, "High Quality", m_photoModeValues.m_dofHighQualityURP, SetDOFHighQualityURP, m_photoModeValues.GetDefaultToggleOptions(), true);
                            //Bokeh
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFStartDistanceURP, m_transformSettings.m_postFX, "Start Blur Distance", m_photoModeValues.m_dofStartBlurURP, m_minAndMaxValues.m_postFXDOFGaussianBlurStartURP.x, m_minAndMaxValues.m_postFXDOFGaussianBlurStartURP.y, SetDOFStartBlurDistance, SetDOFStartBlurDistance, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFEndDistanceURP, m_transformSettings.m_postFX, "End Blur Distance", m_photoModeValues.m_dofEndBlurURP, m_minAndMaxValues.m_postFXDOFGaussianBlurEndURP.x, m_minAndMaxValues.m_postFXDOFGaussianBlurEndURP.y, SetDOFEndBlurDistance, SetDOFEndBlurDistance, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFMaxRadiusURP, m_transformSettings.m_postFX, "Max Radius", m_photoModeValues.m_dofMaxRadiusBlur, m_minAndMaxValues.m_postFXDOFGaussianBlurMaxRadiusURP.x, m_minAndMaxValues.m_postFXDOFGaussianBlurMaxRadiusURP.y, SetDOFMaxBlurRadius, SetDOFMaxBlurRadius, true);
                            //Gaussian
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFAutoFocus, m_transformSettings.m_postFX, "Auto Focus", m_photoModeValues.m_autoDOFFocus, SetDOFAutoFocusEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocusDistance, m_transformSettings.m_postFX, "Focus Distance", m_photoModeValues.m_dofFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistanceURP.x, m_minAndMaxValues.m_postFXDOFFocusDistanceURP.y, SetDOFFocusDistance, SetDOFFocusDistance, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFAperture, m_transformSettings.m_postFX, "Aperture", m_photoModeValues.m_dofAperture, m_minAndMaxValues.m_cameraAperture.x, m_minAndMaxValues.m_cameraAperture.y, SetDOFAperture, SetDOFAperture, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocalLength, m_transformSettings.m_postFX, "Focal Length", m_photoModeValues.m_dofFocalLength, m_minAndMaxValues.m_cameraFocalLength.x, m_minAndMaxValues.m_cameraFocalLength.y, SetDOFFocalLength, SetDOFFocalLength, true);
                            if (m_photoModeValues.m_autoDOFFocus)
                            {
                                if (m_gaiaPostFXDOFFocusDistance != null)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                                }
                            }

                            if (!m_photoModeValues.m_dofActive)
                            {
                                if (m_gaiaPostFXDOFModeURP != null)
                                {
                                    m_gaiaPostFXDOFModeURP.gameObject.SetActive(false);
                                }
                                //Bokeh
                                if (m_gaiaPostFXDOFAutoFocus != null)
                                {
                                    m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFFocusDistance != null)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFAperture != null)
                                {
                                    m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFFocalLength != null)
                                {
                                    m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                                }
                                //Gaussian
                                if (m_gaiaPostFXDOFStartDistanceURP != null)
                                {
                                    m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFEndDistanceURP != null)
                                {
                                    m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFMaxRadiusURP != null)
                                {
                                    m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                                }
                                if (m_gaiaPostFXDOFHighQualityURP != null)
                                {
                                    m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                SetURPUIModeSetup();
                            }
                        }
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        if (GaiaAPI.GetPostExposureHDRP(out m_photoModeValues.m_postFXExposure, out m_photoModeValues.m_postFXExposureMode))
                        {
                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXExposureHeader, m_transformSettings.m_postFX, "Exposure Settings");
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXExposure, m_transformSettings.m_postFX, "Exposure", m_photoModeValues.m_postFXExposure, m_minAndMaxValues.m_postFXExposureHDRP.x, m_minAndMaxValues.m_postFXExposureHDRP.y, SetAutoExposure, SetAutoExposure);
                        }
                        else
                        {
                            if (m_gaiaPostFXExposure != null)
                            {
                                m_gaiaPostFXExposure.gameObject.SetActive(false);
                            }
                        }

                        m_depthOfFieldHDRP = GaiaAPI.GetDepthOfFieldSettingsHDRP();
                        if (m_depthOfFieldHDRP != null)
                        {
                            m_photoModeValues.m_dofActive = m_depthOfFieldHDRP.active;
                            m_photoModeValues.m_dofFocusModeHDRP = (int)m_depthOfFieldHDRP.focusMode.value;
                            m_photoModeValues.m_dofQualityHDRP = m_depthOfFieldHDRP.quality.value;
                            m_photoModeValues.m_dofFocusDistance = m_depthOfFieldHDRP.focusDistance.value;
                            m_photoModeValues.m_dofNearBlurStart = m_depthOfFieldHDRP.nearFocusStart.value;
                            m_photoModeValues.m_dofNearBlurEnd = m_depthOfFieldHDRP.nearFocusEnd.value;
                            m_photoModeValues.m_dofFarBlurStart = m_depthOfFieldHDRP.farFocusStart.value;
                            m_photoModeValues.m_dofFarBlurEnd = m_depthOfFieldHDRP.farFocusEnd.value;
                            m_photoModeValues.m_autoDOFFocus = GaiaAPI.GetAutoFocusDepthOfField();

                            PhotoModeUtils.CreateTitleHeader(ref m_gaiaPostFXDOFHeader, m_transformSettings.m_postFX, "Depth Of Field Settings");
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFEnabled, m_transformSettings.m_postFX, "Enable Depth Of Field", m_photoModeValues.m_dofActive, SetDOFEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFModeHDRP, m_transformSettings.m_postFX, "Mode", m_photoModeValues.m_dofFocusModeHDRP, SetDOFModeHDRP, m_photoModeValues.GetDOFModeOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFHighQualityHDRP, m_transformSettings.m_postFX, "High Quality", m_photoModeValues.m_dofQualityHDRP, SetDOFQualityHDRP, m_photoModeValues.GetDOFQualityOptions(), true);
                            //Physical Camera
                            PhotoModeUtils.CreateDropdown(ref m_gaiaPostFXDOFAutoFocus, m_transformSettings.m_postFX, "Auto Focus", m_photoModeValues.m_autoDOFFocus, SetDOFAutoFocusEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocusDistance, m_transformSettings.m_postFX, "Focus Distance", m_photoModeValues.m_dofFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistanceHDRP.x, m_minAndMaxValues.m_postFXDOFFocusDistanceHDRP.y, SetDOFFocusDistance, SetDOFFocusDistance, true);
                            if (m_hdrpCameraData != null && m_targetCamera != null)
                            {
                                GaiaAPI.GetHDRPCameraSettings(out m_photoModeValues.m_cameraAperture, out m_photoModeValues.m_cameraFocalLength, m_targetCamera);
                                PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFAperture, m_transformSettings.m_postFX, "Aperture", m_photoModeValues.m_cameraAperture, m_minAndMaxValues.m_cameraAperture.x, m_minAndMaxValues.m_cameraAperture.y, SetDOFAperture, SetDOFAperture, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFocalLength, m_transformSettings.m_postFX, "Focal Length", m_photoModeValues.m_cameraFocalLength, m_minAndMaxValues.m_cameraFocalLength.x, m_minAndMaxValues.m_cameraFocalLength.y, SetDOFFocalLength, SetDOFFocalLength, true);
                            }

                            if (m_photoModeValues.m_autoDOFFocus)
                            {
                                if (m_gaiaPostFXDOFFocusDistance != null)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                                }
                            }
                            //Manual
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFNearStartDistanceHDRP, m_transformSettings.m_postFX, "Near Start Blur", m_photoModeValues.m_dofNearBlurStart, m_minAndMaxValues.m_postFXDOFNearBlurStart.x, m_minAndMaxValues.m_postFXDOFNearBlurStart.y, SetHDRPDOFNearStartBlur, SetHDRPDOFNearStartBlur, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFNearEndDistanceHDRP, m_transformSettings.m_postFX, "Near End Blur", m_photoModeValues.m_dofNearBlurEnd, m_minAndMaxValues.m_postFXDOFNearBlurEnd.x, m_minAndMaxValues.m_postFXDOFNearBlurEnd.y, SetHDRPDOFNearEndBlur, SetHDRPDOFNearEndBlur, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFarStartDistanceHDRP, m_transformSettings.m_postFX, "Far Start Blur", m_photoModeValues.m_dofFarBlurStart, m_minAndMaxValues.m_postFXDOFFarBlurStart.x, m_minAndMaxValues.m_postFXDOFFarBlurStart.y, SetHDRPDOFFarStartBlur, SetHDRPDOFFarStartBlur, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaPostFXDOFFarEndDistanceHDRP, m_transformSettings.m_postFX, "Far End Blur", m_photoModeValues.m_dofFarBlurEnd, m_minAndMaxValues.m_postFXDOFFarBlurEnd.x, m_minAndMaxValues.m_postFXDOFFarBlurEnd.y, SetHDRPDOFFarEndBlur, SetHDRPDOFFarEndBlur, true);

                            SetHDRPDOFMode();
                        }
#endif
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Creates terrain UI
        /// </summary>
        private void CreateTerrainSettingsUI()
        {
            if (m_transformSettings.m_terrain != null)
            {
                m_activeTerrain = TerrainHelper.GetActiveTerrain();
                if (m_activeTerrain != null)
                {
                    m_photoModeValues.m_terrainDetailDensity = m_activeTerrain.detailObjectDensity;
                    m_photoModeValues.m_terrainDetailDistance = m_activeTerrain.detailObjectDistance;
                    m_photoModeValues.m_terrainPixelError = m_activeTerrain.heightmapPixelError;
                    m_photoModeValues.m_terrainBasemapDistance = m_activeTerrain.basemapDistance;
                    m_photoModeValues.m_drawInstanced = m_activeTerrain.drawInstanced;

                    PhotoModeUtils.CreateTitleHeader(ref m_terrainHeader, m_transformSettings.m_terrain, "General Settings");
                    switch (m_renderPipeline)
                    {
                        case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            PhotoModeUtils.CreateDropdown(ref m_terrainDrawInstanced, m_transformSettings.m_terrain, "Draw Instanced", m_photoModeValues.m_drawInstanced, SetTerrainDrawInstanced, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateSlider(ref m_terrainHeightResolution, m_transformSettings.m_terrain, "Pixel Error", m_photoModeValues.m_terrainPixelError, m_minAndMaxValues.m_terrainPixelError.x, m_minAndMaxValues.m_terrainPixelError.y, SetTerrainHeightResolution, SetTerrainHeightResolution, true);
                            PhotoModeUtils.CreateSlider(ref m_terrainTextureDistance, m_transformSettings.m_terrain, "Base Map Distance", m_photoModeValues.m_terrainBasemapDistance, m_minAndMaxValues.m_terrainBasemapDistance.x, m_minAndMaxValues.m_terrainBasemapDistance.y, SetTerrainTextureDistance, SetTerrainTextureDistance, true);
                            break;
                        }
                        default:
                        {
                            PhotoModeUtils.CreateDropdown(ref m_terrainDrawInstanced, m_transformSettings.m_terrain, "Draw Instanced", m_photoModeValues.m_drawInstanced, SetTerrainDrawInstanced, m_photoModeValues.GetDefaultToggleOptions(), true);
#if FLORA_PRESENT
                            if (m_detailManager == null)
                            {
#endif
                                PhotoModeUtils.CreateSlider(ref m_terrainDetailDensity, m_transformSettings.m_terrain, "Detail Density", m_photoModeValues.m_terrainDetailDensity, m_minAndMaxValues.m_terrainDetailDensity.x, m_minAndMaxValues.m_terrainDetailDensity.y, SetTerrainDetailDensity, SetTerrainDetailDensity, true);
                                PhotoModeUtils.CreateSlider(ref m_terrainDetailDistance, m_transformSettings.m_terrain, "Detail Distance", m_photoModeValues.m_terrainDetailDistance, m_minAndMaxValues.m_terrainDetailDistance.x, m_minAndMaxValues.m_terrainDetailDistance.y, SetTerrainDetailDistance, SetTerrainDetailDistance, true);
#if FLORA_PRESENT
                            }
#endif
                            PhotoModeUtils.CreateSlider(ref m_terrainHeightResolution, m_transformSettings.m_terrain, "Pixel Error", m_photoModeValues.m_terrainPixelError, m_minAndMaxValues.m_terrainPixelError.x, m_minAndMaxValues.m_terrainPixelError.y, SetTerrainHeightResolution, SetTerrainHeightResolution, true);
                            PhotoModeUtils.CreateSlider(ref m_terrainTextureDistance, m_transformSettings.m_terrain, "Base Map Distance", m_photoModeValues.m_terrainBasemapDistance, m_minAndMaxValues.m_terrainBasemapDistance.x, m_minAndMaxValues.m_terrainBasemapDistance.y, SetTerrainTextureDistance, SetTerrainTextureDistance, true);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Creates lighting UI
        /// </summary>
        private void CreateLightingSettingsUI()
        {
            if (m_transformSettings.m_lighting != null)
            {
                if (GaiaUtils.CheckIfSceneProfileExists())
                {
                    m_sceneProfile = GaiaGlobal.Instance.SceneProfile;

#if GAIA_PRO_PRESENT
                    if (!m_hdrpTimeOfDay)
                    {
                        GaiaTimeOfDay tod = m_sceneProfile.m_gaiaTimeOfDay;
                        m_photoModeValues.m_gaiaTimeOfDayEnabled = tod.m_todEnabled;
                        m_photoModeValues.m_gaiaTime = tod.m_todHour + (tod.m_todMinutes / 60f);
                        m_photoModeValues.m_gaiaTimeScale = tod.m_todDayTimeScale;
                    }
                    else
                    {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                        HDRPTimeOfDay hdrpTimeOfDay = HDRPTimeOfDayAPI.GetTimeOfDay();
                        m_photoModeValues.m_gaiaTimeOfDayEnabled = hdrpTimeOfDay.m_enableTimeOfDaySystem;
                        m_photoModeValues.m_gaiaTime = hdrpTimeOfDay.TimeOfDay;
                        m_photoModeValues.m_gaiaTimeScale = hdrpTimeOfDay.m_timeOfDayMultiplier;
                        m_photoModeValues.m_sunRotation = hdrpTimeOfDay.DirectionY;

                        m_photoModeValues.m_globalLightIntensityMultiplier = HDRPTimeOfDayAPI.GetGlobalSunMultiplier();
                        m_photoModeValues.m_globalFogDensityMultiplier = HDRPTimeOfDayAPI.GetGlobalFogMultiplier();
                        m_photoModeValues.m_globalShadowDistanceMultiplier = HDRPTimeOfDayAPI.GetGlobalShadowMultiplier();
#endif
                    }
#endif
                }

#if GAIA_PRO_PRESENT
                if (m_atmosphere != null)
                {
                    m_photoModeValues.m_sunRotation = m_atmosphere == null ? 0 : m_atmosphere.m_sunRotation;
                    m_photoModeValues.m_gaiaAdditionalLinearFog = m_atmosphere.AdditionalFogDistanceLinear;
                    m_photoModeValues.m_gaiaAdditionalExponentialFog = m_atmosphere.AdditionalFogDistanceExponential;
                }
                else
                {
                    if (m_mainSunLight != null && !m_hdrpTimeOfDay)
                    {
                        m_photoModeValues.m_sunRotation = m_mainSunLight.transform.eulerAngles.y;
                        m_photoModeValues.m_sunPitch = m_mainSunLight.transform.eulerAngles.x;
                    }
                }
#else
                if (m_mainSunLight != null)
                {
                    m_photoModeValues.m_sunRotation = m_mainSunLight.transform.eulerAngles.y;
                    m_photoModeValues.m_sunPitch = m_mainSunLight.transform.eulerAngles.x;
                }
#endif
                GaiaAPI.GetFogSettings(out m_photoModeValues.m_fogMode, out m_photoModeValues.m_fogColor, out m_photoModeValues.m_fogDensity, out m_photoModeValues.m_fogStart, out m_photoModeValues.m_fogEnd, out m_photoModeValues.m_fogOverride);
                m_photoModeValues.m_ambientIntensity = GaiaAPI.GetAmbientIntensity();

                if (m_pwWeatherPresent && m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
#if GAIA_PRO_PRESENT
                    m_photoModeValues.m_fogColor = ProceduralWorldsGlobalWeather.Instance.AdditionalFogColor;
                    PhotoModeUtils.CreateTitleHeader(ref m_lightingGeneralHeader, m_transformSettings.m_lighting, "Time Of Day Settings");
                    PhotoModeUtils.CreateSlider(ref m_gaiaTime, m_transformSettings.m_lighting, "Current Time", m_photoModeValues.m_gaiaTime, m_minAndMaxValues.m_gaiaTime.x, m_minAndMaxValues.m_gaiaTime.y, SetGaiaTime, SetGaiaTime, true);
                    PhotoModeUtils.CreateSlider(ref m_gaiaSunAngle, m_transformSettings.m_lighting, "Sun Rotation", m_photoModeValues.m_sunRotation, m_minAndMaxValues.m_sunRotation.x, m_minAndMaxValues.m_sunRotation.y, SetGaiaSunAngle, SetGaiaSunAngle, true);
                    PhotoModeUtils.CreateDropdown(ref m_gaiaTimeOfDayEnabled, m_transformSettings.m_lighting, "Time of Day Enabled", m_photoModeValues.m_gaiaTimeOfDayEnabled, SetGaiaTimeOfDayEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                    PhotoModeUtils.CreateSlider(ref m_gaiaTimeScale, m_transformSettings.m_lighting, "Time Scale", m_photoModeValues.m_gaiaTimeScale, m_minAndMaxValues.m_gaiaTimeScale.x, m_minAndMaxValues.m_gaiaTimeScale.y, SetGaiaTimeScale, SetGaiaTimeScale, true);

                    PhotoModeUtils.CreateTitleHeader(ref m_lightingFogHeader, m_transformSettings.m_lighting, "Fog Settings");
                    PhotoModeUtils.CreateColorField(ref m_gaiaFogColor, m_transformSettings.m_lighting, "Extra Fog Color", m_photoModeValues.m_fogColor, false, OpenColorPickerFog, true);
                    PhotoModeUtils.CreateSlider(ref m_gaiaAdditionalLinearFog, m_transformSettings.m_lighting, "Extra Fog Distance", m_photoModeValues.m_gaiaAdditionalLinearFog, m_minAndMaxValues.m_gaiaAdditionalLinearFog.x, m_minAndMaxValues.m_gaiaAdditionalLinearFog.y, SetAdditionalLinearFog, SetAdditionalLinearFog, true);
                    PhotoModeUtils.CreateSlider(ref m_gaiaAdditionalExponentialFog, m_transformSettings.m_lighting, "Extra Fog Distance", m_photoModeValues.m_gaiaAdditionalExponentialFog, m_minAndMaxValues.m_gaiaAdditionalExponentialFog.x, m_minAndMaxValues.m_gaiaAdditionalExponentialFog.y, SetAdditionalExponentialFog, SetAdditionalExponentialFog, true);
                    if (RenderSettings.fogMode == FogMode.Linear)
                    {
                        if (m_gaiaAdditionalLinearFog != null)
                        {
                            m_gaiaAdditionalLinearFog.gameObject.SetActive(true);
                        }
                        if (m_gaiaAdditionalExponentialFog != null)
                        {
                            m_gaiaAdditionalExponentialFog.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (m_gaiaAdditionalLinearFog != null)
                        {
                            m_gaiaAdditionalLinearFog.gameObject.SetActive(false);
                        }
                        if (m_gaiaAdditionalExponentialFog != null)
                        {
                            m_gaiaAdditionalExponentialFog.gameObject.SetActive(true);
                        }
                    }

                    if (!m_photoModeValues.m_gaiaTimeOfDayEnabled)
                    {
                        if (m_gaiaTimeScale != null)
                        {
                            m_gaiaTimeScale.gameObject.SetActive(false);
                        }
                    }

                    CreateWeatherSettingsUI();
#endif
                }
                else
                {
                    switch (m_renderPipeline)
                    {
                        case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
#if HDPipeline
                            if (m_hdrpTimeOfDay)
                            {
#if HDPipeline && UNITY_2021_2_OR_NEWER && GAIA_PRO_PRESENT
                                PhotoModeUtils.CreateTitleHeader(ref m_lightingGeneralHeader, m_transformSettings.m_lighting, "Time Of Day Settings");
                                PhotoModeUtils.CreateSlider(ref m_gaiaTime, m_transformSettings.m_lighting, "Current Time", m_photoModeValues.m_gaiaTime, m_minAndMaxValues.m_gaiaTime.x, m_minAndMaxValues.m_gaiaTime.y, SetGaiaTime, SetGaiaTime, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSunAngle, m_transformSettings.m_lighting, "Sun Rotation", m_photoModeValues.m_sunRotation, m_minAndMaxValues.m_sunRotation.x, m_minAndMaxValues.m_sunRotation.y, SetGaiaSunAngle, SetGaiaSunAngle, true);
                                PhotoModeUtils.CreateDropdown(ref m_gaiaTimeOfDayEnabled, m_transformSettings.m_lighting, "Time of Day Enabled", m_photoModeValues.m_gaiaTimeOfDayEnabled, SetGaiaTimeOfDayEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaTimeScale, m_transformSettings.m_lighting, "Time Scale", m_photoModeValues.m_gaiaTimeScale, m_minAndMaxValues.m_gaiaTimeScale.x, m_minAndMaxValues.m_gaiaTimeScale.y, SetGaiaTimeScale, SetGaiaTimeScale, true);

                                PhotoModeUtils.CreateTitleHeader(ref m_lightingGeneralHeader, m_transformSettings.m_lighting, "Global Settings");
                                PhotoModeUtils.CreateSlider(ref m_hdrpGlobalSunMultiplier, m_transformSettings.m_lighting, "Light Intensity Multiplier", m_photoModeValues.m_globalLightIntensityMultiplier, 0.001f, 5f, SetGlobalSunIntensity, SetGlobalSunIntensity, true);
                                PhotoModeUtils.CreateSlider(ref m_hdrpGlobalFogMultiplier, m_transformSettings.m_lighting, "Fog Density Multiplier", m_photoModeValues.m_globalFogDensityMultiplier, 0.001f, 5f, SetGlobalFogDensity, SetGlobalFogDensity, true);
                                PhotoModeUtils.CreateSlider(ref m_hdrpGlobalShadowMultiplier, m_transformSettings.m_lighting, "Shadow Distance Multiplier", m_photoModeValues.m_globalShadowDistanceMultiplier, 0.001f, 5f, SetGlobalShadowDistance, SetGlobalShadowDistance, true);

                                if (!m_photoModeValues.m_gaiaTimeOfDayEnabled)
                                {
                                    if (m_gaiaTimeScale != null)
                                    {
                                        m_gaiaTimeScale.gameObject.SetActive(false);
                                    }
                                }
#endif
                            }
                            else
                            {
                                m_unitySkyboxPresent = GaiaAPI.GetUnityHDRISkyboxHDRP(out m_photoModeValues.m_skyboxRotation, out m_photoModeValues.m_skyboxExposure);
                                GaiaAPI.GetUnityFogHDRP(out m_photoModeValues.m_fogEnd, out m_photoModeValues.m_fogColor);
                                GaiaAPI.GetHDRPDensityVolume(out m_photoModeValues.m_densityVolumeAlbedoColor, out m_photoModeValues.m_densityVolumeFogDistance, out m_photoModeValues.m_densityVolumeEffectType, out m_photoModeValues.m_densityVolumeTilingResolution, out m_photoModeValues.m_overrideDensityVolume);

                                PhotoModeUtils.CreateTitleHeader(ref m_lightingAmbientHeader, m_transformSettings.m_lighting, "Ambient Settings");
                                PhotoModeUtils.CreateSlider(ref m_gaiaAmbientIntensity, m_transformSettings.m_lighting, "Ambient Intensity", m_photoModeValues.m_ambientIntensity, m_minAndMaxValues.m_ambientIntensity.x, m_minAndMaxValues.m_ambientIntensity.y, SetAmbientIntensity, SetAmbientIntensity, true);
                                if (m_mainSunLight)
                                {
                                    GaiaAPI.GetUnitySunSettings(out m_photoModeValues.m_sunIntensity, out m_photoModeValues.m_sunColor, out m_photoModeValues.m_sunKelvinValue, out m_photoModeValues.m_sunOverride, m_mainSunLight);
                                    PhotoModeUtils.CreateTitleHeader(ref m_lightingSunHeader, m_transformSettings.m_lighting, "Sun Settings");
                                    PhotoModeUtils.CreateDropdown(ref m_gaiaSunOverride, m_transformSettings.m_lighting, "Override Sun", m_photoModeValues.m_sunOverride, SetGaiaSunOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSunAngle, m_transformSettings.m_lighting, "Sun Rotation", m_photoModeValues.m_sunRotation, m_minAndMaxValues.m_sunRotation.x, m_minAndMaxValues.m_sunRotation.y, SetGaiaSunAngle, SetGaiaSunAngle, true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSunPitch, m_transformSettings.m_lighting, "Sun Pitch", m_photoModeValues.m_sunPitch, m_minAndMaxValues.m_sunPitch.x, m_minAndMaxValues.m_sunPitch.y, SetGaiaSunPitch, SetGaiaSunPitch, true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSunIntensity, m_transformSettings.m_lighting, "Sun Intensity", m_photoModeValues.m_sunIntensity, m_minAndMaxValues.m_sunIntensityHDRP.x, m_minAndMaxValues.m_sunIntensityHDRP.y, SetGaiaSunIntensity, SetGaiaSunIntensity, true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSunKelvin, m_transformSettings.m_lighting, "Sun Color (Kelvin)", m_photoModeValues.m_sunKelvinValue, m_minAndMaxValues.m_sunKelvinValue.x, m_minAndMaxValues.m_sunKelvinValue.y, SetGaiaSunKelvin, SetGaiaSunKelvin, true);
                                }

                                //Fog
                                PhotoModeUtils.CreateTitleHeader(ref m_lightingFogHeader, m_transformSettings.m_lighting, "Fog Settings");
                                PhotoModeUtils.CreateDropdown(ref m_gaiaFogOverride, m_transformSettings.m_lighting, "Override Fog", m_photoModeValues.m_fogOverride, SetGaiaFogOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                                PhotoModeUtils.CreateColorField(ref m_gaiaFogColor, m_transformSettings.m_lighting, "Fog Color", m_photoModeValues.m_fogColor, false, OpenColorPickerFog, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaFogEnd, m_transformSettings.m_lighting, "Fog Distance", m_photoModeValues.m_fogEnd, m_minAndMaxValues.m_fogEndHDRP.x, m_minAndMaxValues.m_fogEndHDRP.y, SetGaiaFogEnd, SetGaiaFogEnd, true);

                                //Sky
                                if (m_unitySkyboxPresent)
                                {
                                    PhotoModeUtils.CreateTitleHeader(ref m_lightingSkyboxHeader, m_transformSettings.m_lighting, "Skybox Settings");
                                    PhotoModeUtils.CreateDropdown(ref m_gaiaSkyboxOverride, m_transformSettings.m_lighting, "Override Skybox", m_photoModeValues.m_skyboxOverride, SetGaiaSkyboxOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSkyboxRotation, m_transformSettings.m_lighting, "Skybox Rotation", m_photoModeValues.m_skyboxRotation, m_minAndMaxValues.m_skyboxRotation.x, m_minAndMaxValues.m_skyboxRotation.y, SetGaiaSkyboxRotation, SetGaiaSkyboxRotation, true);
                                    PhotoModeUtils.CreateSlider(ref m_gaiaSkyboxExposure, m_transformSettings.m_lighting, "Skybox Exposure", m_photoModeValues.m_skyboxExposure, m_minAndMaxValues.m_skyboxExposureHDRP.x, m_minAndMaxValues.m_skyboxExposureHDRP.y, SetGaiaSkyboxExposure, SetGaiaSkyboxExposure, true);
                                }

                                //Density Volume
                                PhotoModeUtils.CreateTitleHeader(ref m_densityVolumeHeader, m_transformSettings.m_lighting, "Density Volume Settings");
                                PhotoModeUtils.CreateDropdown(ref m_overrideDensityVolume, m_transformSettings.m_lighting, "Override Density Volume", m_photoModeValues.m_overrideDensityVolume, SetOverrideDensityVolume, m_photoModeValues.GetDefaultToggleOptions(), true);
                                PhotoModeUtils.CreateDropdown(ref m_densityVolumeEffectType, m_transformSettings.m_lighting, "Effect Type", m_photoModeValues.m_densityVolumeEffectType, SetDensityVolumeEffectType, m_photoModeValues.GetDensityVolumeEffectTypeOptions(), true);
                                PhotoModeUtils.CreateDropdown(ref m_densityVolumeTilingResolution, m_transformSettings.m_lighting, "Tiling Resolution", m_photoModeValues.m_densityVolumeTilingResolution, SetDensityVolumeTilingResolution, m_photoModeValues.GetDensityVolumeTilingResolutionOptions(), true);
                                PhotoModeUtils.CreateColorField(ref m_densityVolumeAlbedoColor, m_transformSettings.m_lighting, "Density Albedo Color", m_photoModeValues.m_densityVolumeAlbedoColor, false, OpenColorPickerDensityAlbedo, true);
                                PhotoModeUtils.CreateSlider(ref m_densityVolumeFogDistance, m_transformSettings.m_lighting, "Fog Distance", m_photoModeValues.m_densityVolumeFogDistance, 0.01f, 1500f, SetDensityVolumeFogDistance, SetDensityVolumeFogDistance, true);

                                if (!m_photoModeValues.m_overrideDensityVolume)
                                {
                                    if (m_densityVolumeAlbedoColor != null)
                                    {
                                        m_densityVolumeAlbedoColor.gameObject.SetActive(false);
                                    }
                                    if (m_densityVolumeFogDistance != null)
                                    {
                                        m_densityVolumeFogDistance.gameObject.SetActive(false);
                                    }
                                    if (m_densityVolumeEffectType != null)
                                    {
                                        m_densityVolumeEffectType.gameObject.SetActive(false);
                                    }
                                    if (m_densityVolumeTilingResolution != null)
                                    {
                                        m_densityVolumeTilingResolution.gameObject.SetActive(false);
                                    }
                                }
                            }
#endif
                                    break;
                        }
                        default:
                        {
                            if (m_unitySkybox == null)
                            {
                                m_unitySkybox = RenderSettings.skybox;
                            }

                            m_unitySkyboxPresent = GaiaAPI.GetUnityHDRISkybox(out m_photoModeValues.m_skyboxExposure, out m_photoModeValues.m_skyboxRotation, out m_photoModeValues.m_skyboxTint, out m_photoModeValues.m_skyboxOverride);
                            if (m_photoModeValues.m_skyboxRotation < 0f)
                            {
                                m_photoModeValues.m_skyboxRotation = Mathf.Abs(m_photoModeValues.m_skyboxRotation);
                            }

                            GaiaAPI.GetAmbientColor(out m_photoModeValues.m_ambientSkyColor, out m_photoModeValues.m_ambientEquatorColor, out m_photoModeValues.m_ambientGroundColor);
                            PhotoModeUtils.CreateTitleHeader(ref m_lightingAmbientHeader, m_transformSettings.m_lighting, "Ambient Settings");
                            if (RenderSettings.ambientMode == AmbientMode.Skybox)
                            {
                                PhotoModeUtils.CreateSlider(ref m_gaiaAmbientIntensity, m_transformSettings.m_lighting, "Ambient Intensity", m_photoModeValues.m_ambientIntensity, m_minAndMaxValues.m_ambientIntensity.x, m_minAndMaxValues.m_ambientIntensity.y, SetAmbientIntensity, SetAmbientIntensity, true);
                            }
                            else if (RenderSettings.ambientMode == AmbientMode.Trilight)
                            {
                                PhotoModeUtils.CreateColorField(ref m_ambientSkyColor, m_transformSettings.m_lighting, "Sky Ambient Color", m_photoModeValues.m_ambientSkyColor, true, OpenColorPickerAmbientSkyColor, true);
                                PhotoModeUtils.CreateColorField(ref m_ambientEquatorColor, m_transformSettings.m_lighting, "Equator Ambient Color", m_photoModeValues.m_ambientEquatorColor, true, OpenColorPickerAmbientEquatorColor, true);
                                PhotoModeUtils.CreateColorField(ref m_ambientGroundColor, m_transformSettings.m_lighting, "Ground Ambient Color", m_photoModeValues.m_ambientGroundColor, true, OpenColorPickerAmbientGroundColor, true);
                            }
                            else
                            {
                                PhotoModeUtils.CreateColorField(ref m_ambientSkyColor, m_transformSettings.m_lighting, "Sky Ambient Color", m_photoModeValues.m_ambientSkyColor, true, OpenColorPickerAmbientSkyColor, true);
                            }

                            if (m_mainSunLight)
                            {
                                GaiaAPI.GetUnitySunSettings(out m_photoModeValues.m_sunIntensity, out m_photoModeValues.m_sunColor, out m_photoModeValues.m_sunKelvinValue, out m_photoModeValues.m_sunOverride, m_mainSunLight);
                                PhotoModeUtils.CreateTitleHeader(ref m_lightingSunHeader, m_transformSettings.m_lighting, "Sun Settings");
                                PhotoModeUtils.CreateDropdown(ref m_gaiaSunOverride, m_transformSettings.m_lighting, "Override Sun", m_photoModeValues.m_sunOverride, SetGaiaSunOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSunAngle, m_transformSettings.m_lighting, "Sun Rotation", m_photoModeValues.m_sunRotation, m_minAndMaxValues.m_sunRotation.x, m_minAndMaxValues.m_sunRotation.y, SetGaiaSunAngle, SetGaiaSunAngle, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSunPitch, m_transformSettings.m_lighting, "Sun Pitch", m_photoModeValues.m_sunPitch, m_minAndMaxValues.m_sunPitch.x, m_minAndMaxValues.m_sunPitch.y, SetGaiaSunPitch, SetGaiaSunPitch, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSunIntensity, m_transformSettings.m_lighting, "Sun Intensity", m_photoModeValues.m_sunIntensity, m_minAndMaxValues.m_sunIntensity.x, m_minAndMaxValues.m_sunIntensity.y, SetGaiaSunIntensity, SetGaiaSunIntensity, true);
                                PhotoModeUtils.CreateColorField(ref m_gaiaSunColor, m_transformSettings.m_lighting, "Sun Color", m_photoModeValues.m_sunColor,false, OpenColorPickerSunColor, true);
                            }

                            PhotoModeUtils.CreateTitleHeader(ref m_lightingFogHeader, m_transformSettings.m_lighting, "Fog Settings");
                            PhotoModeUtils.CreateDropdown(ref m_gaiaFogOverride, m_transformSettings.m_lighting, "Override Fog", m_photoModeValues.m_fogOverride, SetGaiaFogOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                            PhotoModeUtils.CreateDropdown(ref m_gaiaFogMode, m_transformSettings.m_lighting, "Fog Mode", (int)m_photoModeValues.m_fogMode, SetGaiaFogMode, m_photoModeValues.GetFogModeOptions(), true);
                            PhotoModeUtils.CreateColorField(ref m_gaiaFogColor, m_transformSettings.m_lighting, "Fog Color", m_photoModeValues.m_fogColor,false, OpenColorPickerFog, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaFogStart, m_transformSettings.m_lighting, "Fog Start", m_photoModeValues.m_fogStart, m_minAndMaxValues.m_fogStart.x, m_minAndMaxValues.m_fogStart.y, SetGaiaFogStart, SetGaiaFogStart, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaFogEnd, m_transformSettings.m_lighting, "Fog End", m_photoModeValues.m_fogEnd, m_minAndMaxValues.m_fogEnd.x, m_minAndMaxValues.m_fogEnd.y, SetGaiaFogEnd, SetGaiaFogEnd, true);
                            PhotoModeUtils.CreateSlider(ref m_gaiaFogDensity, m_transformSettings.m_lighting, "Fog Density", m_photoModeValues.m_fogDensity, m_minAndMaxValues.m_fogDensity.x, m_minAndMaxValues.m_fogDensity.y, SetGaiaFogDensity, SetGaiaFogDensity, true);
                            if (RenderSettings.fogMode == FogMode.Linear)
                            {
                                if (m_gaiaFogStart != null)
                                {
                                    m_gaiaFogStart.gameObject.SetActive(true);
                                }
                                if (m_gaiaFogEnd != null)
                                {
                                    m_gaiaFogEnd.gameObject.SetActive(true);
                                }
                                if (m_gaiaFogDensity != null)
                                {
                                    m_gaiaFogDensity.gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (m_gaiaFogStart != null)
                                {
                                    m_gaiaFogStart.gameObject.SetActive(false);
                                }
                                if (m_gaiaFogEnd != null)
                                {
                                    m_gaiaFogEnd.gameObject.SetActive(false);
                                }
                                if (m_gaiaFogDensity != null)
                                {
                                    m_gaiaFogDensity.gameObject.SetActive(true);
                                }
                            }

                            if (m_unitySkyboxPresent)
                            {
                                PhotoModeUtils.CreateTitleHeader(ref m_lightingSkyboxHeader, m_transformSettings.m_lighting, "Skybox Settings");
                                PhotoModeUtils.CreateDropdown(ref m_gaiaSkyboxOverride, m_transformSettings.m_lighting, "Override Skybox", m_photoModeValues.m_skyboxOverride, SetGaiaSkyboxOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSkyboxRotation, m_transformSettings.m_lighting, "Skybox Rotation", m_photoModeValues.m_skyboxRotation, m_minAndMaxValues.m_skyboxRotation.x, m_minAndMaxValues.m_skyboxRotation.y, SetGaiaSkyboxRotation, SetGaiaSkyboxRotation, true);
                                PhotoModeUtils.CreateSlider(ref m_gaiaSkyboxExposure, m_transformSettings.m_lighting, "Skybox Exposure", m_photoModeValues.m_skyboxExposure, m_minAndMaxValues.m_skyboxExposure.x, m_minAndMaxValues.m_skyboxExposure.y, SetGaiaSkyboxExposure, SetGaiaSkyboxExposure, true);
                                PhotoModeUtils.CreateColorField(ref m_gaiaSkyboxTint, m_transformSettings.m_lighting, "Skybox Tint Color", m_photoModeValues.m_skyboxTint, false, OpenColorPickerSkyboxTint, true);
                            }
                            break;
                        }
                    }

                    if (GaiaAPI.GetGaiaWindSettings(out m_photoModeValues.m_gaiaWindSpeed, out m_photoModeValues.m_gaiaWindDirection, out m_photoModeValues.m_gaiaWindSettingsOverride))
                    {
                        PhotoModeUtils.CreateTitleHeader(ref m_gaiaWindHeader, m_transformSettings.m_lighting, "Wind Settings");
                        PhotoModeUtils.CreateDropdown(ref m_gaiaWindSettingsOverride, m_transformSettings.m_lighting, "Override Wind", m_photoModeValues.m_gaiaWindSettingsOverride, SetWindOverride, m_photoModeValues.GetDefaultToggleOptions(), true);
                        PhotoModeUtils.CreateSlider(ref m_gaiaWindDirection, m_transformSettings.m_lighting, "Wind Direction", m_photoModeValues.m_gaiaWindDirection, m_minAndMaxValues.m_gaiaWindDirection.x, m_minAndMaxValues.m_gaiaWindDirection.y, SetGaiaWindDirection, SetGaiaWindDirection, true);
                        PhotoModeUtils.CreateSlider(ref m_gaiaWindSpeed, m_transformSettings.m_lighting, "Wind Speed", m_photoModeValues.m_gaiaWindSpeed, m_minAndMaxValues.m_gaiaWindSpeed.x, m_minAndMaxValues.m_gaiaWindSpeed.y, SetGaiaWindSpeed, SetGaiaWindSpeed, true);
                    }

                    if (!m_photoModeValues.m_fogOverride)
                    {
                        if (m_gaiaFogMode != null)
                        {
                            m_gaiaFogMode.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogColor != null)
                        {
                            m_gaiaFogColor.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogStart != null)
                        {
                            m_gaiaFogStart.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogEnd != null)
                        {
                            m_gaiaFogEnd.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogDensity != null)
                        {
                            m_gaiaFogDensity.gameObject.SetActive(false);
                        }
                    }

                    if (!m_photoModeValues.m_skyboxOverride)
                    {
                        if (m_gaiaSkyboxRotation != null)
                        {
                            m_gaiaSkyboxRotation.gameObject.SetActive(false);
                        }

                        if (m_gaiaSkyboxExposure != null)
                        {
                            m_gaiaSkyboxExposure.gameObject.SetActive(false);
                        }

                        if (m_gaiaSkyboxTint != null)
                        {
                            m_gaiaSkyboxTint.gameObject.SetActive(false);
                        }
                    }

                    if (!m_photoModeValues.m_sunOverride)
                    {
                        if (m_gaiaSunAngle != null && !m_hdrpTimeOfDay)
                        {
                            m_gaiaSunAngle.gameObject.SetActive(false);
                        }

                        if (m_gaiaSunPitch != null)
                        {
                            m_gaiaSunPitch.gameObject.SetActive(false);
                        }

                        if (m_gaiaSunIntensity != null)
                        {
                            m_gaiaSunIntensity.gameObject.SetActive(false);
                        }

                        if (m_gaiaSunKelvin != null)
                        {
                            m_gaiaSunKelvin.gameObject.SetActive(false);
                        }

                        if (m_gaiaSunColor != null)
                        {
                            m_gaiaSunColor.gameObject.SetActive(false);
                        }
                    }

                    if (!m_photoModeValues.m_gaiaWindSettingsOverride)
                    {
                        if (m_gaiaWindSpeed != null)
                        {
                            m_gaiaWindSpeed.gameObject.SetActive(false);
                        }

                        if (m_gaiaWindDirection != null)
                        {
                            m_gaiaWindDirection.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Creates streaming UI
        /// </summary>
        private void CreateStreamingSettingsUI()
        {
#if GAIA_PRO_PRESENT
            if (m_streamingSettingsArea != null && GaiaUtils.HasDynamicLoadedTerrains())
            {
                if (m_terrainLoader != null)
                {
                    Vector3 size = m_terrainLoader.m_loadingBoundsRegular.size;
                    m_photoModeValues.m_gaiaLoadRange = Mathf.Max(size.x, size.z) * 0.5f;
                    size = m_terrainLoader.m_loadingBoundsImpostor.size;
                    m_photoModeValues.m_gaiaImpostorRange = Mathf.Max(size.x, size.z) * 0.5f;
                }

                PhotoModeUtils.CreateTitleHeader(ref m_streamingHeader, m_transformSettings.m_terrain, "Gaia Streaming Settings");
                PhotoModeUtils.CreateSlider(ref m_gaiaLoadRange, m_streamingSettingsArea, "Terrain Range", m_photoModeValues.m_gaiaLoadRange, m_minAndMaxValues.m_gaiaLoadRange.x, m_minAndMaxValues.m_gaiaLoadRange.y, SetGaiaLoadRange, SetGaiaLoadRange, true);
                PhotoModeUtils.CreateSlider(ref m_gaiaImpostorRange, m_streamingSettingsArea, "Impostor Range", m_photoModeValues.m_gaiaImpostorRange, m_minAndMaxValues.m_gaiaImpostorRange.x, m_minAndMaxValues.m_gaiaImpostorRange.y, SetGaiaImpostorRange, SetGaiaImpostorRange, true);
            }
#endif
        }
        /// <summary>
        /// Creates weather UI
        /// </summary>
        private void CreateWeatherSettingsUI()
        {
#if GAIA_PRO_PRESENT
            if (m_transformSettings.m_lighting != null && m_pwWeatherPresent)
            {
                if (m_weather != null)
                {
                    m_photoModeValues.m_gaiaWeatherEnabled = m_weather.EnableRain || m_weather.EnableSnow;
                    m_photoModeValues.m_gaiaWindDirection = m_weather.WindDirection;
                    m_photoModeValues.m_gaiaWindSpeed = m_weather.WindSpeed;
                }

                PhotoModeUtils.CreateTitleHeader(ref m_lightingWeatherHeader, m_transformSettings.m_lighting, "Weather Settings");
                PhotoModeUtils.CreateDropdown(ref m_gaiaWeatherEnabled, m_transformSettings.m_lighting, "Weather Enabled", m_photoModeValues.m_gaiaWeatherEnabled, SetGaiaWeatherEnabled, m_photoModeValues.GetDefaultToggleOptions(), true);
                PhotoModeUtils.CreateButton(ref m_gaiaWeatherToggleRain, m_transformSettings.m_lighting, "Rain", "Start Rain", OnToggleRainClicked, true);
                PhotoModeUtils.CreateButton(ref m_gaiaWeatherToggleSnow, m_transformSettings.m_lighting, "Snow", "Start Snow", OnToggleSnowClicked, true);
                if (GaiaAPI.GetGaiaWindSettings(out m_photoModeValues.m_gaiaWindSpeed, out m_photoModeValues.m_gaiaWindDirection, out m_savedPhotoModeValues.m_gaiaWindSettingsOverride))
                {
                    PhotoModeUtils.CreateTitleHeader(ref m_gaiaWindHeader, m_transformSettings.m_lighting, "Wind Settings");
                    PhotoModeUtils.CreateSlider(ref m_gaiaWindDirection, m_transformSettings.m_lighting, "Wind Direction", m_photoModeValues.m_gaiaWindDirection, m_minAndMaxValues.m_gaiaWindDirection.x, m_minAndMaxValues.m_gaiaWindDirection.y, SetGaiaWindDirection, SetGaiaWindDirection, true);
                    PhotoModeUtils.CreateSlider(ref m_gaiaWindSpeed, m_transformSettings.m_lighting, "Wind Speed", m_photoModeValues.m_gaiaWindSpeed, m_minAndMaxValues.m_gaiaWindSpeed.x, m_minAndMaxValues.m_gaiaWindSpeed.y, SetGaiaWindSpeed, SetGaiaWindSpeed, true);
                }

                if (m_photoModeValues.m_gaiaWeatherEnabled)
                {
                    if (m_gaiaWeatherToggleRain != null)
                    {
                        m_gaiaWeatherToggleRain.gameObject.SetActive(true);
                    }

                    if (m_gaiaWeatherToggleSnow != null)
                    {
                        m_gaiaWeatherToggleSnow.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (m_gaiaWeatherToggleRain != null)
                    {
                        m_gaiaWeatherToggleRain.gameObject.SetActive(false);
                    }

                    if (m_gaiaWeatherToggleSnow != null)
                    {
                        m_gaiaWeatherToggleSnow.gameObject.SetActive(false);
                    }
                }
            }
#endif
        }
        /// <summary>
        /// Creates grass UI
        /// </summary>
        private void CreateGrassSettingsUI()
        {
#if FLORA_PRESENT
            if (m_detailManager != null)
            {
                m_photoModeValues.m_globalGrassDensity = m_detailManager.Settings.ObjectGlobalDensityModifier;
                m_photoModeValues.m_globalGrassDistance = m_detailManager.Settings.ObjectGlobalDistanceModifier;
                m_photoModeValues.m_cameraCellDistance = m_detailManager.Settings.TerrainTileGlobalDistanceModifier;
                m_photoModeValues.m_cameraCellSubdivision = m_detailManager.Settings.CameraCellGlobalSubdivisionModifier;

                PhotoModeUtils.CreateTitleHeader(ref m_grassSettingsHeader, m_transformSettings.m_terrain, "Grass Settings");
                PhotoModeUtils.CreateSlider(ref m_globalGrassDensity, m_transformSettings.m_terrain, "Global Grass Density", m_photoModeValues.m_globalGrassDensity, m_minAndMaxValues.m_globalGrassDensity.x, m_minAndMaxValues.m_globalGrassDensity.y, SetGlobalGrassDensity, SetGlobalGrassDensity, true);
                PhotoModeUtils.CreateSlider(ref m_globalGrassDistance, m_transformSettings.m_terrain, "Global Grass Distance", m_photoModeValues.m_globalGrassDistance, m_minAndMaxValues.m_globalGrassDistance.x, m_minAndMaxValues.m_globalGrassDistance.y, SetGlobalGrassDistance, SetGlobalGrassDistance, true);
                PhotoModeUtils.CreateSlider(ref m_globalCameraCellDistance, m_transformSettings.m_terrain, "Global Camera Cell Distance", m_photoModeValues.m_cameraCellDistance, m_minAndMaxValues.m_cameraCellDistance.x, m_minAndMaxValues.m_cameraCellDistance.y, SetGlobalCameraCellDistance, SetGlobalCameraCellDistance, true);
                PhotoModeUtils.CreateIntSlider(ref m_globalCameraCellSubdivision, m_transformSettings.m_terrain, "Global Camera Cell Subdivision", m_photoModeValues.m_cameraCellSubdivision, m_minAndMaxValues.m_cameraCellSubdivision.x, m_minAndMaxValues.m_cameraCellSubdivision.y, SetGlobalCameraCellSubdivision, SetGlobalCameraCellSubdivision, true);
            }
#endif
        }
        /// <summary>
        /// Checks to see if something is missing in the system
        /// </summary>
        /// <returns></returns>
        private bool VerifySystems()
        {
            if (m_photoModeProfile == null || m_photoModeValues == null)
            {
                Debug.LogError("Photo Mode Profile is missing, check that one has been assigned");
                return false;
            }
            if (m_transformSettings.m_unity == null)
            {
                Debug.LogError("Unity Settings Area rect is missing");
                return false;
            }
#if GAIA_PRO_PRESENT
            if (m_streamingSettingsArea == null)
            {
                Debug.LogError("Streaming Settings Area rect is missing");
                return false;
            }
#endif
            if (m_transformSettings.m_lighting == null)
            {
                Debug.LogError("Gaia Lighting Settings Area rect is missing");
                return false;
            }
            if (m_transformSettings.m_water == null)
            {
                Debug.LogError("Water Settings Area rect is missing");
                return false;
            }
            if (m_transformSettings.m_postFX == null)
            {
                Debug.LogError("Post FX Settings Area rect is missing");
                return false;
            }

            if (m_fpsText == null)
            {
                Debug.LogError("FPS UI text is missing");
                return false;
            }
            if (m_StormVersionText == null)
            {
                Debug.LogError("Version UI text is missing");
                return false;
            }
            if (m_OSText == null)
            {
                Debug.LogError("OS UI text is missing");
                return false;
            }
            if (m_deviceText == null)
            {
                Debug.LogError("Device UI text is missing");
                return false;
            }
            if (m_systemText == null)
            {
                Debug.LogError("System UI text is missing");
                return false;
            }
            if (m_gpuText == null)
            {
                Debug.LogError("GPU UI text is missing");
                return false;
            }
            if (m_gpuCapabilitiesText == null)
            {
                Debug.LogError("GPU Capabilities UI text is missing");
                return false;
            }
            if (m_screenInfoText == null)
            {
                Debug.LogError("Screen Info UI text is missing");
                return false;
            }
            if (m_screenshotText == null)
            {
                //Debug.LogError("Screenshot UI text is missing");
                //return false;
            }

            if (m_runtimeUIPrefab == null)
            {
                Debug.LogError("UI Runtime Prefab is missing");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Resets the settings back to default
        /// This is normally called OnDisable
        /// </summary>
        private void ResetBackToDefault()
        {
            RemovePhotoModeCamera();
            UnFreezePlayerController();

            if (m_gaiaUI == null || !m_gaiaUI.m_resetOnDisable || !Application.isPlaying)
            {
                return;
            }

            //Pipeline Specific
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetAutoFocusDepthOfField(true);
                    GaiaAPI.SetUnityHDRISkyboxHDRP(m_savedPhotoModeValues.m_skyboxRotation, m_savedPhotoModeValues.m_skyboxExposure);
                    GaiaAPI.SetUnityFogHDRP(m_savedPhotoModeValues.m_fogEnd, m_savedPhotoModeValues.m_fogColor);
                    GaiaAPI.SetHDRPCameraSettings(m_savedPhotoModeValues.m_cameraAperture, m_savedPhotoModeValues.m_cameraFocalLength, m_targetCamera);
                    GaiaAPI.SetHDRPShadowDistance(m_savedPhotoModeValues.m_shadowDistance);
                    GaiaAPI.SetHDRPShadowCascades(m_savedPhotoModeValues.m_shadowCascades);
                    GaiaAPI.SetHDRPLODBias(m_savedPhotoModeValues.m_lodBias, m_targetCamera);
                    GaiaAPI.SetHDRPWaterLODBias(m_savedPhotoModeValues.m_gaiaReflectionsLODBias);
                    GaiaAPI.SetHDRPDensityVolume(m_savedPhotoModeValues);
                    GaiaAPI.SetHDRPAntiAliasingMode(m_savedPhotoModeValues.m_antiAliasing);
                    GaiaAPI.SetHDRPAmbientIntensity(m_savedPhotoModeValues.m_ambientIntensity);
                    GaiaAPI.SetHDRPDOFFocusMode(m_savedPhotoModeValues.m_savedDofFocusMode);
#endif
#if HDPipeline && UNITY_2021_2_OR_NEWER && GAIA_PRO_PRESENT
                    HDRPTimeOfDayAPI.SetCurrentTime(m_savedPhotoModeValues.m_gaiaTime, false);
                    HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(m_savedPhotoModeValues.m_gaiaTimeOfDayEnabled, m_savedPhotoModeValues.m_gaiaTimeScale);
                    HDRPTimeOfDayAPI.SetGlobalSunMultiplier(m_savedPhotoModeValues.m_globalLightIntensityMultiplier);
                    HDRPTimeOfDayAPI.SetGlobalFogMultiplier(m_savedPhotoModeValues.m_globalFogDensityMultiplier);
                    HDRPTimeOfDayAPI.SetGlobalShadowMultiplier(m_savedPhotoModeValues.m_globalShadowDistanceMultiplier);
#endif
                        break;
                }
                default:
                {
                    if (m_pwWeatherPresent)
                    {
#if GAIA_PRO_PRESENT
                        //Time/Weather etc
                        GaiaAPI.SetAdditionalFogLinear(0f);
                        GaiaAPI.SetAdditionalFogExponential(0f);
                        GaiaAPI.SetAdditionalFogColor(new Color(0f, 0f, 0f));
                        GaiaTimeOfDay tod = new GaiaTimeOfDay
                        {
                            m_todEnabled = m_savedPhotoModeValues.m_gaiaTimeOfDayEnabled,
                            m_todDayTimeScale = m_savedPhotoModeValues.m_gaiaTimeScale,
                            m_todHour = (int)m_savedPhotoModeValues.m_gaiaTime,
                            m_todMinutes = ((m_savedPhotoModeValues.m_gaiaTime % 1f) * 60)
                        };
                        GaiaAPI.SetTimeOfDaySettings(tod);
                        GaiaAPI.SetWeatherEnabled(m_savedPhotoModeValues.m_gaiaWeatherEnabled);
                        GaiaAPI.SetTimeOfDaySunRotation(m_savedPhotoModeValues.m_sunRotation);
#endif
                    }
                    else
                    {
                        //Sky
                        GaiaAPI.SetUnityHDRISkybox(m_savedPhotoModeValues.m_skyboxExposure, m_savedPhotoModeValues.m_skyboxRotation, m_savedPhotoModeValues.m_skyboxTint);
                        //Fog
                        GaiaAPI.SetFogSettings(m_savedPhotoModeValues.m_fogMode, m_savedPhotoModeValues.m_fogColor, m_savedPhotoModeValues.m_fogDensity, m_savedPhotoModeValues.m_fogStart, m_savedPhotoModeValues.m_fogEnd);
                    }
                    //Quality Settings/Shadows
                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                    {
#if UPPipeline
                        GaiaAPI.SetURPAntiAliasingMode(m_savedPhotoModeValues.m_antiAliasing);
                        GaiaAPI.SetURPShadowDistance(m_savedPhotoModeValues.m_shadowDistance);
                        GaiaAPI.SetURPShadowCasecade(m_savedPhotoModeValues.m_shadowCascades);
#endif
                    }
                    else
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_postProcessingLayer != null)
                        {
                            m_postProcessingLayer.antialiasingMode = (PostProcessLayer.Antialiasing) m_photoModeValues.m_antiAliasing;
                        }
#endif
                        QualitySettings.shadowDistance = m_savedPhotoModeValues.m_shadowDistance;
                        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)m_savedPhotoModeValues.m_shadowResolution;
                        QualitySettings.shadowCascades = m_savedPhotoModeValues.m_shadowCascades;
                    }

                    //Ambient
                    GaiaAPI.SetAmbientColor(m_savedPhotoModeValues.m_ambientSkyColor, m_savedPhotoModeValues.m_ambientEquatorColor, m_savedPhotoModeValues.m_ambientGroundColor);
                    RenderSettings.ambientIntensity = m_savedPhotoModeValues.m_ambientIntensity;
                    QualitySettings.lodBias = m_savedPhotoModeValues.m_lodBias;
                    break;
                }
            }
            //Volume
            AudioListener.volume = m_savedPhotoModeValues.m_globalVolume;
            //Resets post fx
            RevertPostProcessing();
            //Reset camera culling
            if (m_sceneProfile != null)
            {
                if (m_sceneProfile.CullingProfile != null)
                {
                    m_sceneProfile.CullingProfile.m_additionalCullingDistance = 0f;
                    if (m_targetCamera != null)
                    {
                        m_targetCamera.farClipPlane = m_savedPhotoModeValues.m_farClipPlane;
                    }

                    GaiaAPI.RefreshCameraCulling();
                }
            }
            //Reset Terrain
            if (m_activeTerrain != null)
            {
                GaiaAPI.SetTerrainDrawInstanced(m_savedPhotoModeValues.m_drawInstanced);
                GaiaAPI.SetTerrainDetails(m_savedPhotoModeValues.m_terrainDetailDensity, m_savedPhotoModeValues.m_terrainDetailDistance);
                GaiaAPI.SetTerrainPixelErrorAndBaseMapTexture(m_savedPhotoModeValues.m_terrainPixelError, m_savedPhotoModeValues.m_terrainBasemapDistance);
            }
            //Camera
            if (m_targetCamera != null)
            {
                m_targetCamera.fieldOfView = m_savedPhotoModeValues.m_fieldOfView;
                GaiaAPI.SetCameraRoll(m_savedPhotoModeValues.m_cameraRoll, m_targetCamera);
#if HDPipeline
                GaiaAPI.SetHDRPCameraSettings(m_savedPhotoModeValues.m_cameraAperture, m_savedPhotoModeValues.m_cameraFocalLength, m_targetCamera);
#endif
            }
            //Water
            if (m_sceneProfile != null)
            {
                m_sceneProfile.m_extraWaterRenderDistance = m_savedPhotoModeValues.m_gaiaWaterReflectionDistance;
                m_sceneProfile.m_reflectionResolution = (GaiaConstants.GaiaProWaterReflectionsQuality)m_savedPhotoModeValues.m_gaiaWaterReflectionResolution;
                GaiaAPI.SetWaterReflectionExtraDistance(m_savedPhotoModeValues.m_gaiaWaterReflectionDistance);
                GaiaAPI.SetWaterReflections(m_savedPhotoModeValues.m_gaiaWaterReflectionEnabled);
            }
            //Wind
            GaiaAPI.SetGaiaWindSettings(m_savedPhotoModeValues.m_gaiaWindSpeed, m_savedPhotoModeValues.m_gaiaWindDirection);

            if (GaiaUnderwaterEffects.Instance != null)
            {
                GaiaAPI.SetUnderwaterFogColor(m_savedPhotoModeValues.m_gaiaUnderwaterFogColor);
                GaiaAPI.SetUnderwaterFogDensity(m_savedPhotoModeValues.m_gaiaUnderwaterFogDensity, m_savedPhotoModeValues.m_gaiaUnderwaterFogDistance);
                GaiaAPI.SetUnderwaterVolume(m_savedPhotoModeValues.m_gaiaUnderwaterVolume);
            }

            //Streaming
#if GAIA_PRO_PRESENT
            if (m_terrainLoader)
            {
                m_terrainLoader.m_loadingBoundsRegular.size = new Vector3Double(m_savedPhotoModeValues.m_gaiaLoadRange * 2f, m_terrainLoader.m_loadingBoundsRegular.size.y, m_savedPhotoModeValues.m_gaiaLoadRange * 2f);
                m_terrainLoader.m_loadingBoundsImpostor.size = new Vector3Double(m_savedPhotoModeValues.m_gaiaImpostorRange * 2f, m_terrainLoader.m_loadingBoundsImpostor.size.y, m_savedPhotoModeValues.m_gaiaImpostorRange * 2f);
            }
#endif

            //Quality Settings
            QualitySettings.vSyncCount = m_savedPhotoModeValues.m_vSync;
            Application.targetFrameRate = m_savedPhotoModeValues.m_targetFPS;
            //Sun
            if (!m_hdrpTimeOfDay)
            {
                GaiaAPI.SetSunRotation(m_savedPhotoModeValues.m_sunPitch, m_savedPhotoModeValues.m_sunRotation, m_mainSunLight);
            }

            GaiaAPI.SetUnitySunSettings(m_savedPhotoModeValues.m_sunIntensity, m_savedPhotoModeValues.m_sunColor, m_savedPhotoModeValues.m_sunKelvinValue, m_mainSunLight);
        }
        /// <summary>
        /// Reverts post processing back to it's default state
        /// </summary>
        private void RevertPostProcessing()
        {
            //Resets post fx
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_depthOfField != null)
                    {
                        GaiaAPI.SetDepthOfFieldSettings(m_savedPhotoModeValues);
                    }

                    GaiaAPI.SetPostFXExposure(m_savedPhotoModeValues.m_postFXExposure);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_depthOfFieldURP != null)
                    {
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_savedPhotoModeValues);
                    }

                    GaiaAPI.SetPostExposureURP(m_savedPhotoModeValues.m_postFXExposure);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (GaiaAPI.DepthOfFieldPresentHDRP())
                    {
                        GaiaAPI.SetDepthOfFieldSettingsHDRP(m_savedPhotoModeValues);
                    }

                    GaiaAPI.SetPostExposureHDRP(m_savedPhotoModeValues.m_postFXExposure, (ExposureMode)m_savedPhotoModeValues.m_postFXExposureMode);
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Stores all the saved settings
        /// </summary>
        private void SaveStartValues()
        {
            if (GaiaGlobal.Instance == null)
            {
                return;
            }

            m_savedLightingProfileIndex = m_sceneProfile.m_selectedLightingProfileValuesIndex;
            m_photoModeValues = new PhotoModeValues();
            //Get Sun
            m_mainSunLight = GaiaUtils.GetMainDirectionalLight(false);
            //Pipeline Specific
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetAutoFocusDepthOfField(false);
                    GaiaAPI.GetUnityFogHDRP(out m_savedPhotoModeValues.m_fogEnd, out m_savedPhotoModeValues.m_fogColor);
                    GaiaAPI.GetUnityHDRISkyboxHDRP(out m_savedPhotoModeValues.m_skyboxRotation, out m_savedPhotoModeValues.m_skyboxExposure);
                    GaiaAPI.GetHDRPCameraSettings(out m_savedPhotoModeValues.m_cameraAperture, out m_savedPhotoModeValues.m_cameraFocalLength, m_targetCamera);
                    GaiaAPI.GetHDRPDensityVolume(out m_savedPhotoModeValues.m_densityVolumeAlbedoColor, out m_savedPhotoModeValues.m_densityVolumeFogDistance, out m_savedPhotoModeValues.m_densityVolumeEffectType, out m_savedPhotoModeValues.m_densityVolumeTilingResolution, out m_savedPhotoModeValues.m_overrideDensityVolume);
                    m_savedPhotoModeValues.m_shadowDistance = GaiaAPI.GetHDRPShadowDistance();
                    m_savedPhotoModeValues.m_shadowCascades = GaiaAPI.GetHDRPShadowCascades();
                    m_savedPhotoModeValues.m_lodBias = GaiaAPI.GetHDRPLODBias(m_targetCamera);
                    m_savedPhotoModeValues.m_gaiaReflectionsLODBias = GaiaAPI.GetHDRPWaterLODBias();
                    m_savedPhotoModeValues.m_antiAliasing = GaiaAPI.GetHDRPAntiAliasingMode();
                    m_savedPhotoModeValues.m_ambientIntensity = GaiaAPI.GetHDRPAmbientIntensity();
                    GaiaAPI.GetSunRotation(out m_savedPhotoModeValues.m_sunPitch, out m_savedPhotoModeValues.m_sunRotation, m_mainSunLight);
                    m_savedPhotoModeValues.m_savedDofFocusMode = GaiaAPI.GetHDRPDOFFocusMode();
#endif
#if HDPipeline && UNITY_2021_2_OR_NEWER && GAIA_PRO_PRESENT
                    m_savedPhotoModeValues.m_gaiaTime = HDRPTimeOfDayAPI.GetCurrentTime();
                    HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float autoUpdateValue);
                    m_savedPhotoModeValues.m_gaiaTimeOfDayEnabled = autoUpdate;
                    m_savedPhotoModeValues.m_gaiaTimeScale = autoUpdateValue;
                    m_savedPhotoModeValues.m_globalLightIntensityMultiplier = HDRPTimeOfDayAPI.GetGlobalSunMultiplier();
                    m_savedPhotoModeValues.m_globalFogDensityMultiplier = HDRPTimeOfDayAPI.GetGlobalFogMultiplier();
                    m_savedPhotoModeValues.m_globalShadowDistanceMultiplier = HDRPTimeOfDayAPI.GetGlobalShadowMultiplier();
#endif
                    break;
                }
                default:
                {
                    //Saved Skybox
                    if (m_pwWeatherPresent)
                    {
#if GAIA_PRO_PRESENT
                        GaiaTimeOfDay tod = GaiaGlobal.Instance.GaiaTimeOfDayValue;
                        m_savedPhotoModeValues.m_gaiaTimeOfDayEnabled = tod.m_todEnabled;
                        m_savedPhotoModeValues.m_gaiaTimeScale = tod.m_todDayTimeScale;
                        m_savedPhotoModeValues.m_gaiaTime = (int) m_savedPhotoModeValues.m_gaiaTime;
                        m_savedPhotoModeValues.m_gaiaTime += ((m_savedPhotoModeValues.m_gaiaTime % 1f) * 60);

                        m_savedPhotoModeValues.m_gaiaWeatherEnabled = m_weather.m_disableWeatherFX;
                        if (m_atmosphere != null)
                        {
                            m_savedPhotoModeValues.m_sunRotation = m_atmosphere.m_sunRotation;
                        }
#endif
                    }
                    else
                    {
                        GaiaAPI.GetSunRotation(out m_savedPhotoModeValues.m_sunPitch, out m_savedPhotoModeValues.m_sunRotation, m_mainSunLight);
                    }

                    GaiaAPI.GetUnityHDRISkybox(out m_savedPhotoModeValues.m_skyboxExposure, out m_savedPhotoModeValues.m_skyboxRotation, out m_savedPhotoModeValues.m_skyboxTint, out m_savedPhotoModeValues.m_skyboxOverride);
                    //Save fog settings
                    GaiaAPI.GetFogSettings(out m_savedPhotoModeValues.m_fogMode, out m_savedPhotoModeValues.m_fogColor, out m_savedPhotoModeValues.m_fogDensity, out m_savedPhotoModeValues.m_fogStart, out m_savedPhotoModeValues.m_fogEnd, out m_savedPhotoModeValues.m_fogOverride);
                    //Quality
                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                    {
#if UPPipeline
                        m_savedPhotoModeValues.m_antiAliasing = GaiaAPI.GetURPAntiAliasingMode();
                        m_savedPhotoModeValues.m_shadowDistance = GaiaAPI.GetURPShadowDistance();
                        m_savedPhotoModeValues.m_shadowCascades = GaiaAPI.GetURPShadowCasecade();
#endif
                    }
                    else
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        if (m_postProcessingLayer != null)
                        {
                            m_savedPhotoModeValues.m_antiAliasing = (int)m_postProcessingLayer.antialiasingMode;
                        }
#endif
                        m_savedPhotoModeValues.m_shadowDistance = QualitySettings.shadowDistance;
                        m_savedPhotoModeValues.m_shadowResolution = (int)QualitySettings.shadowResolution;
                        m_savedPhotoModeValues.m_shadowCascades = QualitySettings.shadowCascades;
                    }

                    m_savedPhotoModeValues.m_lodBias = QualitySettings.lodBias;
                    //Ambient
                    m_savedPhotoModeValues.m_ambientIntensity = RenderSettings.ambientIntensity;
                    GaiaAPI.GetAmbientColor(out m_savedPhotoModeValues.m_ambientSkyColor, out m_savedPhotoModeValues.m_ambientEquatorColor, out m_savedPhotoModeValues.m_ambientGroundColor);
                    break;
                }
            }

            //Save Terrain Settings
            if (m_activeTerrain != null)
            {
                m_savedPhotoModeValues.m_drawInstanced = m_activeTerrain.drawInstanced;
                m_savedPhotoModeValues.m_terrainDetailDensity = m_activeTerrain.detailObjectDensity;
                m_savedPhotoModeValues.m_terrainDetailDistance = m_activeTerrain.detailObjectDistance;
                m_savedPhotoModeValues.m_terrainPixelError = m_activeTerrain.heightmapPixelError;
                m_savedPhotoModeValues.m_terrainBasemapDistance = m_activeTerrain.basemapDistance;
            }
            //Volume
            m_savedPhotoModeValues.m_globalVolume = AudioListener.volume;
            //Sun
            GaiaAPI.GetUnitySunSettings(out m_savedPhotoModeValues.m_sunIntensity, out m_savedPhotoModeValues.m_sunColor, out m_savedPhotoModeValues.m_sunKelvinValue, out m_savedPhotoModeValues.m_sunOverride, m_mainSunLight);
            //Camera
            if (m_targetCamera != null)
            {
                m_savedPhotoModeValues.m_fieldOfView = m_targetCamera.fieldOfView;
                m_savedPhotoModeValues.m_farClipPlane = m_targetCamera.farClipPlane;
#if HDPipeline
                GaiaAPI.GetHDRPCameraSettings(out m_savedPhotoModeValues.m_cameraAperture, out m_savedPhotoModeValues.m_cameraFocalLength, m_targetCamera);
#endif
            }
            //Water
            if (m_sceneProfile != null)
            {
                m_savedPhotoModeValues.m_gaiaWaterReflectionEnabled = m_sceneProfile.m_enableReflections;
                m_savedPhotoModeValues.m_gaiaWaterReflectionDistance = m_sceneProfile.m_extraWaterRenderDistance;
                m_savedPhotoModeValues.m_gaiaWaterReflectionResolution = (int)m_sceneProfile.m_reflectionResolution;
            }
            //Wind
            GaiaAPI.GetGaiaWindSettings(out m_savedPhotoModeValues.m_gaiaWindSpeed, out m_savedPhotoModeValues.m_gaiaWindDirection, out m_savedPhotoModeValues.m_gaiaWindSettingsOverride);

            if (GaiaUnderwaterEffects.Instance != null)
            {
                m_savedPhotoModeValues.m_gaiaUnderwaterFogColor = GaiaAPI.GetUnderwaterFogColor();
                GaiaAPI.GetUnderwaterFogDensity(out m_savedPhotoModeValues.m_gaiaUnderwaterFogDensity, out m_savedPhotoModeValues.m_gaiaUnderwaterFogDistance);
                m_savedPhotoModeValues.m_gaiaUnderwaterVolume = GaiaAPI.GetUnderwaterVolume();
            }
            //Quality Settings
            m_savedPhotoModeValues.m_vSync = QualitySettings.vSyncCount;
            m_savedPhotoModeValues.m_targetFPS = Application.targetFrameRate;

            //Terrain loading
#if GAIA_PRO_PRESENT
            if (m_terrainLoader != null)
            {
                Vector3 size = m_terrainLoader.m_loadingBoundsRegular.size;
                m_savedPhotoModeValues.m_gaiaLoadRange = Mathf.Max(size.x, size.z) * 0.5f;
                size = m_terrainLoader.m_loadingBoundsImpostor.size;
                m_savedPhotoModeValues.m_gaiaImpostorRange = Mathf.Max(size.x, size.z) * 0.5f;
            }
#endif

            //Post FX
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_depthOfField != null)
                    {
                        m_savedPhotoModeValues.m_dofActive = m_depthOfField.active;
                        m_savedPhotoModeValues.m_dofAperture = m_depthOfField.aperture.value;
                        m_savedPhotoModeValues.m_dofFocalLength = m_depthOfField.focalLength.value;
                        m_savedPhotoModeValues.m_dofFocusDistance = m_depthOfField.focusDistance.value;
                        m_savedPhotoModeValues.m_dofKernelSize = (int)m_depthOfField.kernelSize.value;
                    }
                    GaiaAPI.GetPostFXExposure(out m_savedPhotoModeValues.m_postFXExposure);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_depthOfFieldURP == null)
                    {
                        m_depthOfFieldURP = GaiaAPI.GetDepthOfFieldSettingsURP();
                    }

                    m_savedPhotoModeValues.m_dofActive = m_depthOfFieldURP.active;
                    m_savedPhotoModeValues.m_dofFocusModeURP = (int)m_depthOfFieldURP.mode.value;
                    m_savedPhotoModeValues.m_dofFocusDistance = m_depthOfFieldURP.focusDistance.value;
                    m_savedPhotoModeValues.m_dofAperture = m_depthOfFieldURP.aperture.value;
                    m_savedPhotoModeValues.m_dofFocalLength = m_depthOfFieldURP.focalLength.value;
                    m_savedPhotoModeValues.m_dofStartBlurURP = m_depthOfFieldURP.gaussianStart.value;
                    m_savedPhotoModeValues.m_dofEndBlurURP = m_depthOfFieldURP.gaussianEnd.value;
                    m_savedPhotoModeValues.m_dofMaxRadiusBlur = m_depthOfFieldURP.gaussianMaxRadius.value;
                    m_savedPhotoModeValues.m_dofHighQualityURP = m_depthOfFieldURP.highQualitySampling.value;

                    GaiaAPI.GetPostExposureURP(out m_savedPhotoModeValues.m_postFXExposure);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    m_depthOfFieldHDRP = GaiaAPI.GetDepthOfFieldSettingsHDRP();
                    if (m_depthOfFieldHDRP != null)
                    {
                        m_savedPhotoModeValues.m_dofActive = m_depthOfFieldHDRP.active;
                        m_savedPhotoModeValues.m_dofFocusModeHDRP = (int)m_depthOfFieldHDRP.focusMode.value;
                        m_savedPhotoModeValues.m_dofQualityHDRP = m_depthOfFieldHDRP.quality.value;
                        m_savedPhotoModeValues.m_dofFocusDistance = m_depthOfFieldHDRP.focusDistance.value;
                        m_savedPhotoModeValues.m_dofNearBlurStart = m_depthOfFieldHDRP.nearFocusStart.value;
                        m_savedPhotoModeValues.m_dofNearBlurEnd = m_depthOfFieldHDRP.nearFocusEnd.value;
                        m_savedPhotoModeValues.m_dofFarBlurStart = m_depthOfFieldHDRP.farFocusStart.value;
                        m_savedPhotoModeValues.m_dofFarBlurEnd = m_depthOfFieldHDRP.farFocusEnd.value;
                    }

                    GaiaAPI.GetPostExposureHDRP(out m_savedPhotoModeValues.m_postFXExposure, out m_savedPhotoModeValues.m_postFXExposureMode);
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Keeps focus distance synced with auto focus this helps when you switch mode
        /// </summary>
        private void SyncAutoFocus()
        {
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_depthOfField != null)
                    {
                        m_photoModeValues.m_dofFocusDistance = m_depthOfField.focusDistance.value;
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, m_photoModeValues.m_dofFocusDistance);
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_depthOfFieldURP != null)
                    {
                        m_photoModeValues.m_dofFocusDistance = m_depthOfFieldURP.focusDistance.value;
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, m_photoModeValues.m_dofFocusDistance);
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_depthOfFieldHDRP != null)
                    {
                        m_photoModeValues.m_dofFocusDistance = m_depthOfFieldHDRP.focusDistance.value;
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, m_photoModeValues.m_dofFocusDistance);
                        }
                    }
#endif
                    break;
                }
            }
        }

#endregion
#region Set Photo Mode Functions

        public void SetUnityScreenshotResolution(int f)
        {
            if (m_isSettingValues || m_screenshotResolution == null)
            {
                return;
            }
            m_photoModeValues.m_screenshotResolution = f;
            m_isSettingValues = true;

            if (!m_isUpdatingValues)
            {
                switch (m_photoModeValues.m_screenshotResolution)
                {
                    case 0:
                    {
                        m_screenShotter.m_useScreenSize = true;
                        break;
                    }
                    case 1:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution640X480;
                        break;
                    }
                    case 2:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution800X600;
                        break;
                    }
                    case 3:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution1280X720;
                        break;
                    }
                    case 4:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution1366X768;
                        break;
                    }
                    case 5:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution1600X900;
                        break;
                    }
                    case 6:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution1920X1080;
                        break;
                    }
                    case 7:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution2560X1440;
                        break;
                    }
                    case 8:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution3840X2160;
                        break;
                    }
                    case 9:
                    {
                        m_screenShotter.m_useScreenSize = false;
                        m_screenShotter.m_screenshotResolution = GaiaConstants.ScreenshotResolution.Resolution7680X4320;
                        break;
                    }
                }
            }

            m_screenshotResolution.SetDropdownValue(m_photoModeValues.m_screenshotResolution);
            if (m_screenShotter != null)
            {
                if (m_photoModeValues.m_screenshotResolution != 0)
                {
                    m_screenShotter.UpdateScreenshotResolution(
                        (GaiaConstants.ScreenshotResolution) m_photoModeValues.m_screenshotResolution);
                }
            }

            m_isSettingValues = false;
        }
        public void SetUnityScreenshotImageFormat(int f)
        {
            if (m_isSettingValues || m_screenshotImageFormat == null)
            {
                return;
            }
            m_photoModeValues.m_screenshotImageFormat = f;
            m_isSettingValues = true;

            if (!m_isUpdatingValues)
            {
                m_screenShotter.m_imageFormat = (GaiaConstants.ImageFileType)f;
            }

            m_screenshotImageFormat.SetDropdownValue(m_photoModeValues.m_screenshotImageFormat);
            m_isSettingValues = false;
        }
        public void SetPhotoModeLoadSettings(int value)
        {
            if (m_isSettingValues || m_loadSavedSettings == null)
            {
                return;
            }


            switch (value)
            {
                case 0:
                {
                    m_photoModeValues.m_loadSavedSettings = false;
                    break;
                }
                case 1:
                {
                    m_photoModeValues.m_loadSavedSettings = true;
                    break;
                }
            }

            if (!m_isUpdatingValues)
            {
                if (m_gaiaUI != null)
                {
                    GaiaAPI.SetPhotoModeSettings(m_photoModeValues.m_loadSavedSettings, m_photoModeValues.m_revertOnDisabled,m_photoModeValues.m_showReticle, m_photoModeValues.m_showRuleOfThirds, m_gaiaUI.m_enablePhotoMode);
                }
            }

            m_isSettingValues = true;
            m_loadSavedSettings.SetDropdownValue(value);

            m_isSettingValues = false;
        }
        public void SetPhotoModeRevertOnDisabledSettings(int value)
        {
            if (m_isSettingValues || m_resetPhotoModeOnDisable == null)
            {
                return;
            }

            m_photoModeValues.m_revertOnDisabled = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues)
            {
                if (m_gaiaUI != null)
                {
                    GaiaAPI.SetPhotoModeSettings(m_photoModeValues.m_loadSavedSettings, m_photoModeValues.m_revertOnDisabled,m_photoModeValues.m_showReticle, m_photoModeValues.m_showRuleOfThirds, m_gaiaUI.m_enablePhotoMode);
                }
            }

            m_isSettingValues = true;
            m_resetPhotoModeOnDisable.SetDropdownValue(value);

            m_isSettingValues = false;
        }

        public void SetPhotoModeShowFPS(int value)
        {
            if (m_isSettingValues || m_showFPS == null)
            {
                return;
            }

            m_photoModeValues.m_showFPS = PhotoModeUtils.ConvertIntToBool(value);
            if (m_photoModeValues.m_showFPS)
            {
                m_fpsAccumulator = 0f;
                m_fpsNextPeriod = Time.realtimeSinceStartup + m_cMeasurePeriod;
                m_fpsText.enabled = true;
            }
            else
            {
                m_fpsText.enabled = false;
            }
          
            m_isSettingValues = true;
            m_showFPS.SetDropdownValue(value);

            m_isSettingValues = false;
        }

        public void SetPhotoModeShowReticule(int value)
        {
            if (m_isSettingValues || m_showReticule == null)
            {
                return;
            }

            m_photoModeValues.m_showReticle = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues)
            {
                if (m_gaiaUI != null)
                {
                    GaiaAPI.SetShowOrHidePhotoModeReticule(m_photoModeValues.m_showReticle);
                }
            }

            m_isSettingValues = true;
            m_showReticule.SetDropdownValue(value);

            m_isSettingValues = false;
        }
        public void SetPhotoModeShowRuleOfThirds(int value)
        {
            if (m_isSettingValues || m_showRuleOfThirds == null)
            {
                return;
            }

            m_photoModeValues.m_showRuleOfThirds = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues)
            {
                if (m_gaiaUI != null)
                {
                    GaiaAPI.SetShowOrHidePhotoModeRuleOfThirds(m_photoModeValues.m_showRuleOfThirds);
                }
            }

            m_isSettingValues = true;
            m_showRuleOfThirds.SetDropdownValue(value);

            m_isSettingValues = false;
        }

#endregion
#region Set Unity Functions

        public void SetUnityVolume(float f)
        {
            if (m_isSettingValues || m_unityVolume == null)
            {
                return;
            }
            m_photoModeValues.m_globalVolume = f;
            if (!m_isUpdatingValues)
            {
                AudioListener.volume = f;
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_unityVolume, f);
            m_isSettingValues = false;
        }
        public void SetUnityVolume(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityVolume, m_minAndMaxValues.m_0To1, m_unityVolume);
        }
        public void SetUnityFieldOfView(float f)
        {
            if (m_isSettingValues || m_fieldOfView == null)
            {
                return;
            }
            m_photoModeValues.m_fieldOfView = f;
            if (!m_isUpdatingValues)
            {
                if (m_targetCamera != null)
                {
                    m_targetCamera.fieldOfView = f;
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_fieldOfView, f);
            m_isSettingValues = false;
        }
        public void SetUnityFieldOfView(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityFieldOfView, m_minAndMaxValues.m_fieldOfView, m_fieldOfView);
        }
        public void SetUnityCameraRoll(float f)
        {
            if (m_isSettingValues || m_cameraRoll == null)
            {
                return;
            }
            m_photoModeValues.m_cameraRoll = f;
            if (!m_isUpdatingValues)
            {
                GaiaAPI.SetCameraRoll(f, m_targetCamera);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_cameraRoll, f);
            m_isSettingValues = false;
        }
        public void SetUnityCameraRoll(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityCameraRoll, m_minAndMaxValues.m_cameraRoll, m_cameraRoll);
        }
        public void SetUnityCullingDistance(float f)
        {
            if (m_isSettingValues || m_cullingDistance == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaCullinDistance = f;
            if (!m_isUpdatingValues)
            {
                if (f > 0)
                {
                    GaiaAPI.SetCullingSettings(f, m_photoModeValues.m_farClipPlane + f);
                }
                else
                {
                    GaiaAPI.SetCullingSettings(f, m_photoModeValues.m_farClipPlane - Mathf.Abs(f));
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_cullingDistance, f);
            m_isSettingValues = false;
        }
        public void SetUnityCullingDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityCullingDistance, m_minAndMaxValues.m_gaiaCullinDistance, m_cullingDistance);
        }
        public void SetUnityLODBias(float f)
        {
            if (m_isSettingValues || m_unityLODBias == null)
            {
                return;
            }
            m_photoModeValues.m_lodBias = f;
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetHDRPLODBias(f, m_targetCamera);
#endif
                    break;
                }
                default:
                {
                    QualitySettings.lodBias = f;
                    break;
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_unityLODBias, f);
            m_isSettingValues = false;
        }
        public void SetUnityLODBias(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityLODBias, m_minAndMaxValues.m_lodBias, m_unityLODBias);
        }
        public void SetUnityAA(int f)
        {
            if (m_isSettingValues || m_unityAA == null)
            {
                return;
            }

            m_photoModeValues.m_antiAliasing = f;

            m_isSettingValues = true;
            m_unityAA.SetDropdownValue(m_photoModeValues.m_antiAliasing);
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_postProcessingLayer != null)
                    {
                        m_postProcessingLayer.antialiasingMode = (PostProcessLayer.Antialiasing) m_photoModeValues.m_antiAliasing;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    GaiaAPI.SetURPAntiAliasingMode(m_photoModeValues.m_antiAliasing);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetHDRPAntiAliasingMode(m_photoModeValues.m_antiAliasing);
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetUnityVSync(int f)
        {
            if (m_isSettingValues || m_unityVSync == null)
            {
                return;
            }

            m_photoModeValues.m_vSync = f;
            if (!m_isUpdatingValues)
            {
                QualitySettings.vSyncCount = m_photoModeValues.m_vSync;
            }

            m_isSettingValues = true;
            m_unityVSync.SetDropdownValue(m_photoModeValues.m_vSync);
            m_isSettingValues = false;
            if (m_unityTargetFPS != null)
            {
                if (m_photoModeValues.m_vSync == 0)
                {
                    m_unityTargetFPS.gameObject.SetActive(true);
                    Application.targetFrameRate = m_photoModeValues.m_targetFPS;
                }
                else
                {
                    m_unityTargetFPS.gameObject.SetActive(false);
                    Application.targetFrameRate = -1;
                }
            }
        }
        public void SetUnityTargetFPS(float f)
        {
            if (m_isSettingValues || m_unityTargetFPS == null)
            {
                return;
            }
            m_photoModeValues.m_targetFPS = (int)f;
            if (!m_isUpdatingValues)
            {
                if (m_photoModeValues.m_vSync == 0)
                {
                    Application.targetFrameRate = m_photoModeValues.m_targetFPS;
                }
                else
                {
                    Application.targetFrameRate = -1;
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_unityTargetFPS, f);
            m_isSettingValues = false;
        }
        public void SetUnityTargetFPS(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityTargetFPS, m_minAndMaxValues.m_targetFPS, m_unityTargetFPS);
        }
        public void SetUnityShadowDistance(float f)
        {
            if (m_isSettingValues || m_unityShadowDistance == null)
            {
                return;
            }
            m_photoModeValues.m_shadowDistance = f;

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    if (!m_isUpdatingValues)
                    {
                        QualitySettings.shadowDistance = m_photoModeValues.m_shadowDistance;
                    }
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    GaiaAPI.SetURPShadowDistance(f);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetHDRPShadowDistance(f);
#endif
                    break;
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_unityShadowDistance, f);
            m_isSettingValues = false;
        }
        public void SetUnityShadowDistance(string val)
        {
            PhotoModeUtils.GetAndSetFloatValue(val, SetUnityShadowDistance, m_minAndMaxValues.m_shadowDistance, m_unityShadowDistance);
        }
        public void SetUnityShadowResolution(int f)
        {
            if (m_isSettingValues || m_unityShadownResolution == null)
            {
                return;
            }

            m_photoModeValues.m_shadowResolution = f;

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    if (!m_isUpdatingValues)
                    {
                        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)m_photoModeValues.m_shadowResolution;
                    }
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    break;
                }
            }

            m_isSettingValues = true;
            m_unityShadownResolution.SetDropdownValue(m_photoModeValues.m_shadowResolution);
            m_isSettingValues = false;
        }
        public void SetUnityShadowCascades(int f)
        {
            if (m_isSettingValues || m_unityShadowCascades == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_shadowCascades = f;
            m_unityShadowCascades.SetDropdownValue(f);

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    switch (f)
                    {
                        case 1:
                            f = 2;
                            break;
                        case 2:
                            f = 4;
                            break;
                    }
                    if (!m_isUpdatingValues)
                    {
                        QualitySettings.shadowCascades = m_photoModeValues.m_shadowCascades;
                    }
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    switch (f)
                    {
                        case 0:
                        {
                            f = 1;
                            break;
                        }
                        case 1:
                        {
                            f = 2;
                            break;
                        }
                        case 2:
                        {
                            f = 3;
                            break;
                        }
                        case 3:
                        {
                            f = 4;
                            break;
                        }
                    }
                    GaiaAPI.SetURPShadowCasecade(f);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    switch (f)
                    {
                        case 0:
                        {
                            f = 1;
                            break;
                        }
                        case 1:
                        {
                            f = 2;
                            break;
                        }
                        case 2:
                        {
                            f = 3;
                            break;
                        }
                        case 3:
                        {
                            f = 4;
                            break;
                        }
                    }
                    GaiaAPI.SetHDRPShadowCascades(f);
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }

#endregion
#region Set Gaia Streaming Functions

#if GAIA_PRO_PRESENT
        public void SetGaiaLoadRange(float f)
        {
            if (m_isSettingValues || m_gaiaLoadRange == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaLoadRange = f;
            if (!m_isUpdatingValues && m_terrainLoader != null)
            {
                m_terrainLoader.m_loadingBoundsRegular.size = new Vector3Double(f * 2f, m_terrainLoader.m_loadingBoundsRegular.size.y, f * 2f);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaLoadRange, f);
            m_isSettingValues = false;
        }
        public void SetGaiaLoadRange(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaLoadRange, m_minAndMaxValues.m_gaiaLoadRange, m_gaiaLoadRange);
        }
        public void SetGaiaImpostorRange(float f)
        {
            if (m_isSettingValues || m_gaiaImpostorRange == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaImpostorRange = f;
            if (!m_isUpdatingValues && m_terrainLoader != null)
            {
                m_terrainLoader.m_loadingBoundsImpostor.size = new Vector3Double(f * 2f, m_terrainLoader.m_loadingBoundsImpostor.size.y, f * 2f);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaImpostorRange, f);
            m_isSettingValues = false;
        }
        public void SetGaiaImpostorRange(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaImpostorRange, m_minAndMaxValues.m_gaiaImpostorRange, m_gaiaImpostorRange);
        }
#endif

#endregion
#region Set Weather Functions

        public void SetGaiaWindDirection(float f)
        {
            if (m_isSettingValues || m_gaiaWindDirection == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWindDirection = f;
            if (!m_isUpdatingValues)
            {
                GaiaAPI.SetGaiaWindSettings(m_photoModeValues.m_gaiaWindSpeed, f);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaWindDirection, f);
            if (f < 0.0625f || f > 0.9375f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (N)");
            }
            else if (f < 0.1875f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (NE)");
            }
            else if (f < 0.3125f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (E)");
            }
            else if (f < 0.4375f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (SE)");
            }
            else if (f < 0.5625f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (S)");
            }
            else if (f < 0.6875f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (SW)");
            }
            else if (f < 0.8125f)
            {
                m_gaiaWindDirection.SetLabel("Wind Direction (W)");
            }
            else if (f < 0.9375f)
            {
                m_gaiaWindDirection.SetLabel("NW", 1);
            }

            m_isSettingValues = false;
        }
        public void SetGaiaWindDirection(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaWindDirection, m_minAndMaxValues.m_gaiaWindDirection, m_gaiaWindDirection);
        }
        public void SetGaiaWindSpeed(float f)
        {
            if (m_isSettingValues || m_gaiaWindSpeed == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWindSpeed = f;
            if (!m_isUpdatingValues)
            {
                GaiaAPI.SetGaiaWindSettings(f, m_photoModeValues.m_gaiaWindDirection);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaWindSpeed, f);
            m_isSettingValues = false;
        }
        public void SetGaiaWindSpeed(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaWindSpeed, m_minAndMaxValues.m_gaiaWindSpeed, m_gaiaWindSpeed);
        }
        public void SetWindOverride(int value)
        {
            if (m_isSettingValues || m_gaiaWindSettingsOverride == null)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);

            m_photoModeValues.m_gaiaWindSettingsOverride = boolValue;
            if (!m_isUpdatingValues)
            {
                if (m_photoModeValues.m_gaiaWindSettingsOverride)
                {
                    GaiaAPI.SetGaiaWindSettings(m_photoModeValues.m_gaiaWindSpeed, m_photoModeValues.m_gaiaWindDirection);
                }
                else
                {
                    GaiaAPI.SetGaiaWindSettings(m_savedPhotoModeValues.m_gaiaWindSpeed, m_savedPhotoModeValues.m_gaiaWindDirection);
                }
            }

            m_isSettingValues = true;
            m_gaiaWindSettingsOverride.SetDropdownValue(value);

            if (m_gaiaWindSpeed != null)
            {
                m_gaiaWindSpeed.gameObject.SetActive(m_photoModeValues.m_gaiaWindSettingsOverride);
            }

            if (m_gaiaWindDirection != null)
            {
                m_gaiaWindDirection.gameObject.SetActive(m_photoModeValues.m_gaiaWindSettingsOverride);
            }

            m_isSettingValues = false;
        }
#if GAIA_PRO_PRESENT
        public void SetGaiaWeatherEnabled(int value)
        {
            if (m_isSettingValues || m_gaiaWeatherEnabled == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaWeatherEnabled = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues && m_weather != null)
            {
                m_weather.SetWeatherStatus(m_photoModeValues.m_gaiaWeatherEnabled);
            }

            m_isSettingValues = true;
            m_gaiaWeatherEnabled.SetDropdownValue(value);

            m_isSettingValues = false;
            if (m_gaiaWeatherToggleRain != null)
            {
                m_gaiaWeatherToggleRain.gameObject.SetActive(m_photoModeValues.m_gaiaWeatherEnabled);
            }

            if (m_gaiaWeatherToggleSnow != null)
            {
                m_gaiaWeatherToggleSnow.gameObject.SetActive(m_photoModeValues.m_gaiaWeatherEnabled);
            }
        }
        public void OnToggleRainClicked()
        {
            OnSetRain(!m_photoModeValues.m_gaiaWeatherRain);
        }
        public void OnSetRain(bool on)
        {
            if (m_gaiaWeatherToggleRain == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWeatherRain = on;
            if (m_weather != null)
            {
                if (on)
                {
                    m_weather.PlayRain();
                    if (m_gaiaWeatherToggleSnow != null)
                    {
                        m_gaiaWeatherToggleSnow.SetButtonInactive(false);
                    }
                }
                else
                {
                    m_weather.StopRain();
                    if (m_gaiaWeatherToggleSnow != null)
                    {
                        m_gaiaWeatherToggleSnow.SetButtonInactive(true);
                    }
                }
            }
            m_gaiaWeatherToggleRain.SetButtonLabel(on ? "Stop Rain" : "Start Rain");
        }
        public void OnSetRain(bool on, bool setValueOnly)
        {
            if (m_gaiaWeatherToggleRain == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWeatherRain = on;
            if (m_weather != null)
            {
                if (!setValueOnly)
                {
                    if (on)
                    {
                        m_weather.PlayRain();
                    }
                    else
                    {
                        m_weather.StopRain();
                    }
                }
            }

            if (on)
            {
                if (m_gaiaWeatherToggleSnow != null)
                {
                    m_gaiaWeatherToggleSnow.SetButtonInactive(false);
                }
            }
            else
            {
                if (m_gaiaWeatherToggleSnow != null)
                {
                    m_gaiaWeatherToggleSnow.SetButtonInactive(true);
                }
            }
            m_gaiaWeatherToggleRain.SetButtonLabel(on ? "Stop Rain" : "Start Rain");
        }
        public void OnToggleSnowClicked()
        {
            OnSetSnow(!m_photoModeValues.m_gaiaWeatherSnow);
        }
        public void OnSetSnow(bool on)
        {
            if (m_gaiaWeatherToggleSnow == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWeatherSnow = on;
            if (m_weather != null)
            {
                if (on)
                {
                    m_weather.PlaySnow();
                    if (m_gaiaWeatherToggleRain != null)
                    {
                        m_gaiaWeatherToggleRain.SetButtonInactive(false);
                    }
                }
                else
                {
                    m_weather.StopSnow();
                    if (m_gaiaWeatherToggleRain != null)
                    {
                        m_gaiaWeatherToggleRain.SetButtonInactive(true);
                    }
                }
            }
            m_gaiaWeatherToggleSnow.SetButtonLabel(on ? "Stop Snow" : "Start Snow");
        }
        public void OnSetSnow(bool on, bool setValueOnly)
        {
            if (m_gaiaWeatherToggleSnow == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaWeatherSnow = on;
            if (m_weather != null)
            {
                if (!setValueOnly)
                {
                    if (on)
                    {
                        m_weather.PlaySnow();
                    }
                    else
                    {
                        m_weather.StopSnow();
                    }
                }
            }

            if (on)
            {
                if (m_gaiaWeatherToggleRain != null)
                {
                    m_gaiaWeatherToggleRain.SetButtonInactive(false);
                }
            }
            else
            {
                if (m_gaiaWeatherToggleRain != null)
                {
                    m_gaiaWeatherToggleRain.SetButtonInactive(true);
                }
            }
            m_gaiaWeatherToggleSnow.SetButtonLabel(on ? "Stop Snow" : "Start Snow");
        }
        public void SetAdditionalLinearFog(float f)
        {
            if (m_isSettingValues || m_gaiaAdditionalLinearFog == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaAdditionalLinearFog = f;
            PhotoModeUtils.SetSliderValue(m_gaiaAdditionalLinearFog, f);
            GaiaAPI.SetAdditionalFogLinear(f);

            m_isSettingValues = false;
        }
        public void SetAdditionalLinearFog(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetAdditionalLinearFog, m_minAndMaxValues.m_gaiaAdditionalLinearFog, m_gaiaAdditionalLinearFog);
        }
        public void SetAdditionalExponentialFog(float f)
        {
            if (m_isSettingValues || m_gaiaAdditionalExponentialFog == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaAdditionalExponentialFog = f;
            PhotoModeUtils.SetSliderValue(m_gaiaAdditionalExponentialFog, f);
            GaiaAPI.SetAdditionalFogExponential(f);

            m_isSettingValues = false;
        }
        public void SetAdditionalExponentialFog(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetAdditionalExponentialFog, m_minAndMaxValues.m_gaiaAdditionalExponentialFog, m_gaiaAdditionalExponentialFog);
        }
#endif

#endregion
#region Set Gaia Lighting

#if GAIA_PRO_PRESENT
        public void SetGlobalSunIntensity(float f)
        {
#if HDPipeline && UNITY_2021_2_OR_NEWER
            if (m_isSettingValues || m_hdrpGlobalSunMultiplier == null)
            {
                return;
            }
            m_photoModeValues.m_globalLightIntensityMultiplier = f;
            if (!m_isUpdatingValues)
            {
                HDRPTimeOfDayAPI.SetGlobalSunMultiplier(f);
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_hdrpGlobalSunMultiplier, f);
            m_isSettingValues = false;
#endif
        }
        public void SetGlobalSunIntensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalSunIntensity, new Vector2(0f, 5f), m_hdrpGlobalSunMultiplier);
        }
        public void SetGlobalFogDensity(float f)
        {
#if HDPipeline && UNITY_2021_2_OR_NEWER
            if (m_isSettingValues || m_hdrpGlobalFogMultiplier == null)
            {
                return;
            }
            m_photoModeValues.m_globalLightIntensityMultiplier = f;
            if (!m_isUpdatingValues)
            {
                HDRPTimeOfDayAPI.SetGlobalFogMultiplier(f);
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_hdrpGlobalFogMultiplier, f);
            m_isSettingValues = false;
#endif
        }
        public void SetGlobalFogDensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalFogDensity, new Vector2(0f, 5f), m_hdrpGlobalFogMultiplier);
        }
        public void SetGlobalShadowDistance(float f)
        {
#if HDPipeline && UNITY_2021_2_OR_NEWER
            if (m_isSettingValues || m_hdrpGlobalShadowMultiplier == null)
            {
                return;
            }
            m_photoModeValues.m_globalLightIntensityMultiplier = f;
            if (!m_isUpdatingValues)
            {
                HDRPTimeOfDayAPI.SetGlobalShadowMultiplier(f);
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_hdrpGlobalShadowMultiplier, f);
            m_isSettingValues = false;
#endif
        }
        public void SetGlobalShadowDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalShadowDistance, new Vector2(0f, 5f), m_hdrpGlobalShadowMultiplier);
        }
        public void SetGaiaTime(float f)
        {
            if (m_isSettingValues || m_gaiaTime == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaTime = f;
            if (!m_isUpdatingValues)
            {
                if (m_hdrpTimeOfDay)
                {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                    HDRPTimeOfDayAPI.SetCurrentTime(f, false);
#endif
                }

                if (GaiaGlobal.Instance != null && GaiaGlobal.Instance.SceneProfile != null)
                {
                    GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todHour = (int)f;
                    GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todMinutes = ((f % 1f) * 60);
                }
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaTime, f);
            m_isSettingValues = false;
        }
        public void SetGaiaTime(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaTime, m_minAndMaxValues.m_gaiaTime, m_gaiaTime);
        }
        public void SetGaiaTimeScale(float f)
        {
            if (m_isSettingValues || m_gaiaTimeScale == null)
            {
                return;
            }

            m_photoModeValues.m_gaiaTimeScale = f;
            if (!m_isUpdatingValues)
            {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float autoUpdateValue);
                HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(autoUpdate, f);
#endif
                if (GaiaGlobal.Instance != null && GaiaGlobal.Instance.SceneProfile != null)
                {
                    GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todDayTimeScale = f;
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaTimeScale, f);
            m_isSettingValues = false;
        }
        public void SetGaiaTimeScale(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaTimeScale, m_minAndMaxValues.m_gaiaTimeScale, m_gaiaTimeScale);
        }
        public void SetGaiaTimeOfDayEnabled(int value)
        {
            if (m_isSettingValues || m_gaiaTimeOfDayEnabled == null)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);

            m_photoModeValues.m_gaiaTimeOfDayEnabled = boolValue;
            if (!m_isUpdatingValues)
            {
                if (m_hdrpTimeOfDay)
                {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                    HDRPTimeOfDayAPI.GetAutoUpdateMultiplier(out bool autoUpdate, out float autoUpdateValue);
                    HDRPTimeOfDayAPI.SetAutoUpdateMultiplier(boolValue, autoUpdateValue);
#endif
                    if (m_gaiaTimeScale != null)
                    {
                        m_gaiaTimeScale.gameObject.SetActive(m_hdrpTimeOfDay && m_photoModeValues.m_gaiaTimeOfDayEnabled);
                    }
                }
                else
                {
                    if (m_gaiaTimeScale != null)
                    {
                        m_gaiaTimeScale.gameObject.SetActive(m_pwWeatherPresent && m_photoModeValues.m_gaiaTimeOfDayEnabled);
                    }
                }

                if (GaiaGlobal.Instance != null && GaiaGlobal.Instance.SceneProfile != null)
                {
                    GaiaGlobal.Instance.SceneProfile.m_gaiaTimeOfDay.m_todEnabled = boolValue;
                }
            }

            m_isSettingValues = true;
            m_gaiaTimeOfDayEnabled.SetDropdownValue(value);
            m_isSettingValues = false;
        }
#endif
        public void SetGaiaSunAngle(float f)
        {
            if (m_isSettingValues || m_gaiaSunAngle == null)
            {
                return;
            }

            m_photoModeValues.m_sunRotation = f;

#if GAIA_PRO_PRESENT
            if (!m_isUpdatingValues)
            {
                if (!m_hdrpTimeOfDay)
                {
                    if (!GaiaAPI.SetTimeOfDaySunRotation(f))
                    {
                        if (m_mainSunLight != null)
                        {
                            m_mainSunLight.transform.localEulerAngles =
                                new Vector3(m_photoModeValues.m_sunPitch, f, 0f);
                        }
                    }
                }
                else
                {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                    HDRPTimeOfDayAPI.SetDirection(f);
#endif
                }
            }
#else
            if (m_mainSunLight != null)
            {
                m_mainSunLight.transform.localEulerAngles = new Vector3(m_photoModeValues.m_sunPitch, f, 0f);
            }
#endif
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaSunAngle, f);
            if (f < 22.5f || f > 337.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (N)", 0);
            }
            else if (f < 67.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (NE)", 0);
            }
            else if (f < 112.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (E)", 0);
            }
            else if (f < 157.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (SE)", 0);
            }
            else if (f < 202.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (S)", 0);
            }
            else if (f < 247.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (SW)", 0);
            }
            else if (f < 292.5f)
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (W)", 0);
            }
            else
            {
                m_gaiaSunAngle.SetLabel("Sun Rotation (NW)", 0);
            }

            m_isSettingValues = false;
        }
        public void SetGaiaSunAngle(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSunAngle, m_minAndMaxValues.m_sunRotation, m_gaiaSunAngle);
        }
        public void SetGaiaSunPitch(float f)
        {
            if (m_isSettingValues || m_gaiaSunPitch == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_sunPitch = f;

            if (m_mainSunLight != null)
            {
                m_mainSunLight.transform.localEulerAngles = new Vector3(f, m_photoModeValues.m_sunRotation, 0f);
            }

            PhotoModeUtils.SetSliderValue(m_gaiaSunPitch, f);
            m_isSettingValues = false;
        }
        public void SetGaiaSunPitch(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSunPitch, m_minAndMaxValues.m_sunPitch, m_gaiaSunPitch);
        }
        public void SetGaiaSunOverride(int value)
        {
            if (m_isSettingValues || m_gaiaSunOverride == null)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);

            m_photoModeValues.m_sunOverride = boolValue;
            if (!m_isUpdatingValues)
            {
                if (m_photoModeValues.m_sunOverride)
                {
                    GaiaAPI.SetUnitySunSettings(m_photoModeValues.m_sunIntensity, m_photoModeValues.m_sunColor, m_photoModeValues.m_sunKelvinValue, m_mainSunLight);
                    GaiaAPI.SetSunRotation(m_photoModeValues.m_sunPitch, m_photoModeValues.m_sunRotation, m_mainSunLight);
                }
                else
                {
                    GaiaAPI.SetUnitySunSettings(m_savedPhotoModeValues.m_sunIntensity, m_savedPhotoModeValues.m_sunColor, m_savedPhotoModeValues.m_sunKelvinValue, m_mainSunLight);
                    GaiaAPI.SetSunRotation(m_savedPhotoModeValues.m_sunPitch, m_savedPhotoModeValues.m_sunRotation, m_mainSunLight);
                }
            }

            m_isSettingValues = true;
            m_gaiaSunOverride.SetDropdownValue(value);

            if (m_gaiaSunAngle != null)
            {
                m_gaiaSunAngle.gameObject.SetActive(m_photoModeValues.m_sunOverride);
            }

            if (m_gaiaSunPitch != null)
            {
                m_gaiaSunPitch.gameObject.SetActive(m_photoModeValues.m_sunOverride);
            }

            if (m_gaiaSunIntensity != null)
            {
                m_gaiaSunIntensity.gameObject.SetActive(m_photoModeValues.m_sunOverride);
            }

            if (m_gaiaSunKelvin != null)
            {
                m_gaiaSunKelvin.gameObject.SetActive(m_photoModeValues.m_sunOverride);
            }

            if (m_gaiaSunColor != null)
            {
                m_gaiaSunColor.gameObject.SetActive(m_photoModeValues.m_sunOverride);
            }

            m_isSettingValues = false;
        }
        public void SetGaiaSunIntensity(float f)
        {
            if (m_isSettingValues || m_gaiaSunIntensity == null)
            {
                return;
            }

            m_photoModeValues.m_sunIntensity = f;
            if (!m_isUpdatingValues)
            {
                GaiaAPI.SetUnitySunSettings(m_photoModeValues.m_sunIntensity, m_photoModeValues.m_sunColor, m_photoModeValues.m_sunKelvinValue, m_mainSunLight);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaSunIntensity, f);
            m_isSettingValues = false;
        }
        public void SetGaiaSunIntensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSunIntensity, m_minAndMaxValues.m_sunIntensity, m_gaiaSunIntensity);
        }
        public void SetGaiaSunKelvin(float f)
        {
            if (m_isSettingValues || m_gaiaSunKelvin == null)
            {
                return;
            }

            m_photoModeValues.m_sunKelvinValue = f;
            if (!m_isUpdatingValues)
            {
                GaiaAPI.SetUnitySunSettings(m_photoModeValues.m_sunIntensity, m_photoModeValues.m_sunColor, m_photoModeValues.m_sunKelvinValue, m_mainSunLight);
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaSunKelvin, f);
            m_isSettingValues = false;
        }
        public void SetGaiaSunKelvin(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSunKelvin, m_minAndMaxValues.m_sunKelvinValue, m_gaiaSunKelvin);
        }
        public void SetAmbientIntensity(float f)
        {
            if (m_isSettingValues || m_gaiaAmbientIntensity == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_ambientIntensity = f;
            GaiaAPI.SetAmbientIntensity(f);
            PhotoModeUtils.SetSliderValue(m_gaiaAmbientIntensity, f);
            m_isSettingValues = false;
        }
        public void SetAmbientIntensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetAmbientIntensity, m_minAndMaxValues.m_ambientIntensity, m_gaiaAmbientIntensity);
        }
        public void SetGaiaFogOverride(int value)
        {
            if (m_isSettingValues || m_gaiaFogOverride == null)
            {
                return;
            }

            m_photoModeValues.m_fogOverride = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues)
            {
                UpdateFogSettings();
            }

            m_isSettingValues = true;
            m_gaiaFogOverride.SetDropdownValue(value);

            if (m_gaiaFogMode != null)
            {
                m_gaiaFogMode.gameObject.SetActive(m_photoModeValues.m_fogOverride);
            }

            if (m_gaiaFogColor != null)
            {
                m_gaiaFogColor.gameObject.SetActive(m_photoModeValues.m_fogOverride);
            }

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    if (m_gaiaFogEnd != null)
                    {
                        m_gaiaFogEnd.gameObject.SetActive(m_photoModeValues.m_fogOverride);
                    }
                    break;
                }
                default:
                {
                    if (RenderSettings.fogMode == FogMode.Linear)
                    {
                        if (m_gaiaFogStart != null)
                        {
                            m_gaiaFogStart.gameObject.SetActive(m_photoModeValues.m_fogOverride);
                        }
                        if (m_gaiaFogEnd != null)
                        {
                            m_gaiaFogEnd.gameObject.SetActive(m_photoModeValues.m_fogOverride);
                        }
                        if (m_gaiaFogDensity != null)
                        {
                            m_gaiaFogDensity.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (m_gaiaFogStart != null)
                        {
                            m_gaiaFogStart.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogEnd != null)
                        {
                            m_gaiaFogEnd.gameObject.SetActive(false);
                        }
                        if (m_gaiaFogDensity != null)
                        {
                            m_gaiaFogDensity.gameObject.SetActive(m_photoModeValues.m_fogOverride);
                        }
                    }
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetGaiaFogMode(int f)
        {
            if (m_isSettingValues || m_gaiaFogMode == null)
            {
                return;
            }

            switch (f)
            {
                case 0:
                {
                    m_photoModeValues.m_fogMode = FogMode.Linear;
                    break;
                }
                case 1:
                {
                    m_photoModeValues.m_fogMode = FogMode.Exponential;
                    break;
                }
                case 2:
                {
                    m_photoModeValues.m_fogMode = FogMode.ExponentialSquared;
                    break;
                }
            }

            if (!m_isUpdatingValues)
            {
                UpdateFogSettings();
            }

            if (RenderSettings.fogMode == FogMode.Linear)
            {
                if (m_gaiaFogStart != null)
                {
                    m_gaiaFogStart.gameObject.SetActive(true);
                }
                if (m_gaiaFogEnd != null)
                {
                    m_gaiaFogEnd.gameObject.SetActive(true);
                }
                if (m_gaiaFogDensity != null)
                {
                    m_gaiaFogDensity.gameObject.SetActive(false);
                }
            }
            else
            {
                if (m_gaiaFogStart != null)
                {
                    m_gaiaFogStart.gameObject.SetActive(false);
                }
                if (m_gaiaFogEnd != null)
                {
                    m_gaiaFogEnd.gameObject.SetActive(false);
                }
                if (m_gaiaFogDensity != null)
                {
                    m_gaiaFogDensity.gameObject.SetActive(true);
                }
            }

            m_isSettingValues = true;
            m_gaiaFogMode.SetDropdownValue(f);
            m_isSettingValues = false;
        }
        public void SetGaiaFogStart(float f)
        {
            if (m_isSettingValues || m_gaiaFogStart == null)
            {
                return;
            }

            m_photoModeValues.m_fogStart = f;
            if (!m_isUpdatingValues)
            {
                UpdateFogSettings();
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaFogStart, f);
            m_isSettingValues = false;
        }
        public void SetGaiaFogStart(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaFogStart, m_minAndMaxValues.m_fogStart, m_gaiaFogStart);
        }
        public void SetGaiaFogDensity(float f)
        {
            if (m_isSettingValues || m_gaiaFogDensity == null)
            {
                return;
            }

            m_photoModeValues.m_fogDensity = f;
            if (!m_isUpdatingValues)
            {
                UpdateFogSettings();
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaFogDensity, f);
            m_isSettingValues = false;
        }
        public void SetGaiaFogDensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaFogDensity, m_minAndMaxValues.m_fogDensity, m_gaiaFogDensity);
        }
        public void SetGaiaFogEnd(float f)
        {
            if (m_isSettingValues || m_gaiaFogEnd == null)
            {
                return;
            }

            m_photoModeValues.m_fogEnd = f;
            if (!m_isUpdatingValues)
            {
                UpdateFogSettings();
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaFogEnd, f);
            m_isSettingValues = false;
        }
        public void SetGaiaFogEnd(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaFogEnd, m_minAndMaxValues.m_fogEnd, m_gaiaFogEnd);
        }
        public void SetGaiaSkyboxOverride(int value)
        {
            if (m_isSettingValues || m_gaiaSkyboxOverride == null)
            {
                return;
            }

            m_photoModeValues.m_skyboxOverride = PhotoModeUtils.ConvertIntToBool(value);
            if (!m_isUpdatingValues)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            if (m_photoModeValues.m_skyboxOverride)
                            {
                                UpdateHDRPSkyValues(m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxExposure);
                            }
                            else
                            {
                                UpdateHDRPSkyValues(m_savedPhotoModeValues.m_skyboxRotation, m_savedPhotoModeValues.m_skyboxExposure);
                            }
                            break;
                        }
                    default:
                        {
                            if (m_photoModeValues.m_skyboxOverride)
                            {
                                GaiaAPI.SetUnityHDRISkybox(m_photoModeValues.m_skyboxExposure, m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxTint);
                            }
                            else
                            {
                                GaiaAPI.SetUnityHDRISkybox(m_savedPhotoModeValues.m_skyboxExposure, m_savedPhotoModeValues.m_skyboxRotation, m_savedPhotoModeValues.m_skyboxTint);
                            }
                            break;
                        }
                }
            }

            m_isSettingValues = true;
            m_gaiaSkyboxOverride.SetDropdownValue(value);

            if (m_gaiaSkyboxRotation != null)
            {
                m_gaiaSkyboxRotation.gameObject.SetActive(m_photoModeValues.m_skyboxOverride);
            }

            if (m_gaiaSkyboxExposure != null)
            {
                m_gaiaSkyboxExposure.gameObject.SetActive(m_photoModeValues.m_skyboxOverride);
            }

            if (m_gaiaSkyboxTint != null)
            {
                m_gaiaSkyboxTint.gameObject.SetActive(m_photoModeValues.m_skyboxOverride);
            }

            m_isSettingValues = false;
        }
        public void SetGaiaSkyboxRotation(float f)
        {
            if (m_isSettingValues || m_gaiaSkyboxRotation == null)
            {
                return;
            }

            m_photoModeValues.m_skyboxRotation = f;
            if (!m_isUpdatingValues)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            UpdateHDRPSkyValues(m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxExposure);
                            break;
                        }
                    default:
                        {
                            GaiaAPI.SetUnityHDRISkybox(m_photoModeValues.m_skyboxExposure, m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxTint);
                            break;
                        }
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaSkyboxRotation, f);
            m_isSettingValues = false;
        }
        public void SetGaiaSkyboxRotation(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSkyboxRotation, m_minAndMaxValues.m_skyboxRotation, m_gaiaSkyboxRotation);
        }
        public void SetGaiaSkyboxExposure(float f)
        {
            if (m_isSettingValues || m_gaiaSkyboxExposure == null)
            {
                return;
            }

            m_photoModeValues.m_skyboxExposure = f;
            if (!m_isUpdatingValues)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            UpdateHDRPSkyValues(m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxExposure);
                            break;
                        }
                    default:
                        {
                            GaiaAPI.SetUnityHDRISkybox(m_photoModeValues.m_skyboxExposure, m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxTint);
                            break;
                        }
                }
            }

            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_gaiaSkyboxExposure, f);
            m_isSettingValues = false;
        }
        public void SetGaiaSkyboxExposure(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaSkyboxExposure, m_minAndMaxValues.m_skyboxExposure, m_gaiaSkyboxExposure);
        }
        public void UpdateSkyboxTint()
        {
            GaiaAPI.SetUnityHDRISkybox(m_photoModeValues.m_skyboxExposure, m_photoModeValues.m_skyboxRotation, m_photoModeValues.m_skyboxTint);
            if (m_gaiaSkyboxTint != null)
            {
                m_gaiaSkyboxTint.SetColorPreviewImage(m_photoModeValues.m_skyboxTint);
            }
        }
        public void UpdateSunColor()
        {
            GaiaAPI.SetUnitySunSettings(m_photoModeValues.m_sunIntensity, m_photoModeValues.m_sunColor, m_photoModeValues.m_sunKelvinValue, m_mainSunLight);
            if (m_gaiaSunColor != null)
            {
                m_gaiaSunColor.SetColorPreviewImage(m_photoModeValues.m_sunColor);
            }
        }
        public void UpdateAmbientSkyColor()
        {
            Color ambientColor = m_photoModeValues.m_ambientSkyColor;
            ambientColor.a = 1f;
            GaiaAPI.SetAmbientColor(ambientColor, m_photoModeValues.m_ambientEquatorColor, m_photoModeValues.m_ambientGroundColor);
            if (m_ambientSkyColor != null)
            {
                m_ambientSkyColor.SetColorPreviewImage(ambientColor, true);
            }
        }
        public void UpdateAmbientEquaotrColor()
        {
            Color ambientColor = m_photoModeValues.m_ambientEquatorColor;
            ambientColor.a = 1f;
            GaiaAPI.SetAmbientColor(m_photoModeValues.m_ambientSkyColor, ambientColor, m_photoModeValues.m_ambientGroundColor);
            if (m_ambientEquatorColor != null)
            {
                m_ambientEquatorColor.SetColorPreviewImage(ambientColor, true);
            }
        }
        public void UpdateAmbientGroundColor()
        {
            Color ambientColor = m_photoModeValues.m_ambientGroundColor;
            ambientColor.a = 1f;
            GaiaAPI.SetAmbientColor(m_photoModeValues.m_ambientSkyColor, m_photoModeValues.m_ambientEquatorColor, ambientColor);
            if (m_ambientGroundColor != null)
            {
                m_ambientGroundColor.SetColorPreviewImage(ambientColor, true);
            }
        }

#region HDRP

#if HDPipeline
        public void SetOverrideDensityVolume(int value)
        {
            if (m_isSettingValues || m_overrideDensityVolume == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_overrideDensityVolume = PhotoModeUtils.ConvertIntToBool(value);
            m_overrideDensityVolume.SetDropdownValue(value);

            if (m_densityVolumeAlbedoColor != null)
            {
                m_densityVolumeAlbedoColor.gameObject.SetActive(m_photoModeValues.m_overrideDensityVolume);
            }
            if (m_densityVolumeFogDistance != null)
            {
                m_densityVolumeFogDistance.gameObject.SetActive(m_photoModeValues.m_overrideDensityVolume);
            }
            if (m_densityVolumeEffectType != null)
            {
                m_densityVolumeEffectType.gameObject.SetActive(m_photoModeValues.m_overrideDensityVolume);
            }
            if (m_densityVolumeTilingResolution != null)
            {
                m_densityVolumeTilingResolution.gameObject.SetActive(m_photoModeValues.m_overrideDensityVolume);
            }

            if (m_photoModeValues.m_overrideDensityVolume)
            {
                GaiaAPI.SetHDRPDensityVolume(m_photoModeValues);
            }
            else
            {
                GaiaAPI.SetHDRPDensityVolume(m_savedPhotoModeValues);
            }

            m_isSettingValues = false;
        }
        public void SetDensityVolumeEffectType(int f)
        {
            if (m_isSettingValues || m_densityVolumeEffectType == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_densityVolumeEffectType = f;
            m_densityVolumeEffectType.SetDropdownValue(f);

            GaiaAPI.SetHDRPDensityVolume(m_photoModeValues);

            m_isSettingValues = false;
        }
        public void SetDensityVolumeTilingResolution(int f)
        {
            if (m_isSettingValues || m_densityVolumeTilingResolution == null)
            {
                return;
            }

            m_isSettingValues = true;
            m_photoModeValues.m_densityVolumeTilingResolution = f;
            m_densityVolumeTilingResolution.SetDropdownValue(f);

            GaiaAPI.SetHDRPDensityVolume(m_photoModeValues);

            m_isSettingValues = false;
        }
        public void SetDensityVolumeFogDistance(float f)
        {
            if (m_isSettingValues || m_densityVolumeFogDistance == null)
            {
                return;
            }

            m_photoModeValues.m_densityVolumeFogDistance = f;
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_densityVolumeFogDistance, f);
            GaiaAPI.SetHDRPDensityVolume(m_photoModeValues);
            m_isSettingValues = false;
        }
        public void SetDensityVolumeFogDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetDensityVolumeFogDistance, m_minAndMaxValues.m_densityVolumeFogDistance, m_densityVolumeFogDistance);
        }

        public void UpdateDensityVolumeAlbedoColor()
        {
            GaiaAPI.SetHDRPDensityVolume(m_photoModeValues);
            if (m_densityVolumeAlbedoColor != null)
            {
                m_densityVolumeAlbedoColor.SetColorPreviewImage(m_photoModeValues.m_densityVolumeAlbedoColor);
            }
        }
#endif

#endregion

        private void UpdateFogSettings()
        {
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    if (m_photoModeValues.m_fogOverride)
                    {
                        UpdateHDRPFogValues(m_photoModeValues.m_fogEnd, m_photoModeValues.m_fogColor);
                    }
                    else
                    {
                        UpdateHDRPFogValues(m_savedPhotoModeValues.m_fogEnd, m_savedPhotoModeValues.m_fogColor);
                    }
                    break;
                }
                default:
                {
                    if (m_pwWeatherPresent)
                    {
#if GAIA_PRO_PRESENT
                        GaiaAPI.SetAdditionalFogColor(m_photoModeValues.m_fogColor);
                        if (m_gaiaFogColor != null)
                        {
                            m_gaiaFogColor.SetColorPreviewImage(m_photoModeValues.m_fogColor);
                        }
#endif
                    }
                    else
                    {
                        if (m_photoModeValues.m_fogOverride)
                        {
                            GaiaAPI.SetFogSettings(m_photoModeValues.m_fogMode, m_photoModeValues.m_fogColor, m_photoModeValues.m_fogDensity, m_photoModeValues.m_fogStart, m_photoModeValues.m_fogEnd);
                            if (m_gaiaFogColor != null)
                            {
                                m_gaiaFogColor.SetColorPreviewImage(m_photoModeValues.m_fogColor);
                            }
                        }
                        else
                        {
                            GaiaAPI.SetFogSettings(m_savedPhotoModeValues.m_fogMode, m_savedPhotoModeValues.m_fogColor, m_savedPhotoModeValues.m_fogDensity, m_savedPhotoModeValues.m_fogStart, m_savedPhotoModeValues.m_fogEnd);
                            if (m_gaiaFogColor != null)
                            {
                                m_gaiaFogColor.SetColorPreviewImage(m_photoModeValues.m_fogColor);
                            }
                        }
                    }
                    break;
                }
            }
            if (m_gaiaFogColor != null)
            {
                m_gaiaFogColor.SetColorPreviewImage(m_photoModeValues.m_fogColor);
            }
        }
        private void UpdateHDRPSkyValues(float rotation, float exposure)
        {
#if HDPipeline
            GaiaAPI.SetUnityHDRISkyboxHDRP(rotation, exposure);
#endif
        }
        private void UpdateHDRPFogValues(float fogDistance, Color fogColor)
        {
#if HDPipeline
            GaiaAPI.SetUnityFogHDRP(fogDistance, fogColor);
#endif
        }

#endregion
#region Set Gaia Water

        public void SetGaiaWaterReflectionEnabled(int value)
        {
            if (m_isSettingValues || m_gaiaWaterReflectionsEnabled == null)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);
            m_photoModeValues.m_gaiaWaterReflectionEnabled = boolValue;
            m_gaiaWaterReflectionsEnabled.SetDropdownValue(value);
            GaiaAPI.SetWaterReflections(boolValue);
            if (m_gaiaWaterReflectionDistance != null)
            {
                m_gaiaWaterReflectionDistance.gameObject.SetActive(boolValue);
            }
            if (m_gaiaWaterReflectionResolution != null)
            {
                m_gaiaWaterReflectionResolution.gameObject.SetActive(boolValue);
            }
            if (m_gaiaWaterReflectionLODBias != null)
            {
                m_gaiaWaterReflectionLODBias.gameObject.SetActive(boolValue);
            }
            m_isSettingValues = false;
        }
        public void SetGaiaWaterReflectionDistance(float f)
        {
            if (m_isSettingValues || m_gaiaWaterReflectionDistance == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaWaterReflectionDistance = f;
            PhotoModeUtils.SetSliderValue(m_gaiaWaterReflectionDistance, f);
            GaiaAPI.SetWaterReflectionExtraDistance(f);
            m_isSettingValues = false;
        }
        public void SetGaiaWaterReflectionDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaWaterReflectionDistance, m_minAndMaxValues.m_gaiaWaterReflectionDistance, m_gaiaWaterReflectionDistance);
        }
        public void SetGaiaWaterReflectionResolution(int f)
        {
            if (m_isSettingValues || m_gaiaWaterReflectionResolution == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaWaterReflectionResolution = f;
            m_gaiaWaterReflectionResolution.SetDropdownValue(f);
            GaiaAPI.SetWaterResolutionQuality(f);
            m_isSettingValues = false;
        }
        public void SetGaiaWaterReflectionLODBias(float f)
        {
#if HDPipeline
            if (m_isSettingValues || m_gaiaWaterReflectionLODBias == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaReflectionsLODBias = f;
            PhotoModeUtils.SetSliderValue(m_gaiaWaterReflectionLODBias, f);
            GaiaAPI.SetHDRPWaterLODBias(f);
#endif
            m_isSettingValues = false;
        }
        public void SetGaiaWaterReflectionLODBias(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaWaterReflectionLODBias, m_minAndMaxValues.m_gaiaReflectionsLODBias, m_gaiaWaterReflectionLODBias);
        }
        public void SetGaiaUnderwaterFogDistance(float f)
        {
            if (m_isSettingValues || m_gaiaUnderwaterFogDistance == null)
            {
                return;
            }

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    m_photoModeValues.m_gaiaUnderwaterFogDistance = f;
                    break;
                }
                default:
                {
                    switch (RenderSettings.fogMode)
                    {
                        case FogMode.Linear:
                        {
                            m_photoModeValues.m_gaiaUnderwaterFogDistance = f;
                            break;
                        }
                        default:
                        {
                            m_photoModeValues.m_gaiaUnderwaterFogDensity = f;
                            break;
                        }
                    }
                    break;
                }
            }

            PhotoModeUtils.SetSliderValue(m_gaiaUnderwaterFogDistance, f);
            GaiaAPI.SetUnderwaterFogDensity(m_photoModeValues.m_gaiaUnderwaterFogDensity, m_photoModeValues.m_gaiaUnderwaterFogDistance);
            m_isSettingValues = false;
        }
        public void SetGaiaUnderwaterFogDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }

            if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaUnderwaterFogDistance, m_minAndMaxValues.m_gaiaUnderwaterFogDistance, m_gaiaUnderwaterFogDistance);
            }
            else
            {
                switch (RenderSettings.fogMode)
                {
                    case FogMode.Linear:
                    {
                        PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaUnderwaterFogDistance, m_minAndMaxValues.m_gaiaUnderwaterFogDistance, m_gaiaUnderwaterFogDistance);
                        break;
                    }
                    default:
                    {
                        PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaUnderwaterFogDistance, m_minAndMaxValues.m_gaiaUnderwaterFogDensity, m_gaiaUnderwaterFogDistance);
                        break;
                    }
                }
            }
        }
        public void SetGaiaUnderwaterVolume(float f)
        {
            if (m_isSettingValues || m_gaiaUnderwaterVolume == null)
            {
                return;
            }
            m_photoModeValues.m_gaiaUnderwaterVolume = f;
            PhotoModeUtils.SetSliderValue(m_gaiaUnderwaterVolume, f);
            GaiaAPI.SetUnderwaterVolume(f);
            m_isSettingValues = false;
        }
        public void SetGaiaUnderwaterVolume(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGaiaUnderwaterVolume, m_minAndMaxValues.m_0To1, m_gaiaUnderwaterVolume);
        }
        public void UpdateUnderwaterFogColor()
        {
            GaiaAPI.SetUnderwaterFogColor(m_photoModeValues.m_gaiaUnderwaterFogColor);
            if (m_gaiaUnderwaterFogColor != null)
            {
                m_gaiaUnderwaterFogColor.SetColorPreviewImage(m_photoModeValues.m_gaiaUnderwaterFogColor);
            }
        }

#endregion
#region Set Gaia Post FX

        public void SetDOFEnabled(int value)
        {
            if (m_isSettingValues)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_gaiaPostFXDOFEnabled == null)
                    {
                        return;
                    }

                    if (m_depthOfField != null)
                    {
                        m_gaiaPostFXDOFEnabled.SetDropdownValue(value);
                        m_photoModeValues.m_dofActive = boolValue;
                        GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);

                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(boolValue);
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(boolValue);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(boolValue);
                        }
                        if (m_gaiaPostFXDOFKernelSizeBuiltIn != null)
                        {
                            m_gaiaPostFXDOFKernelSizeBuiltIn.gameObject.SetActive(boolValue);
                        }
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            if (m_photoModeValues.m_autoDOFFocus && boolValue)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                            else
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(boolValue);
                            }
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_gaiaPostFXDOFEnabled == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldURP != null)
                    {
                        m_gaiaPostFXDOFEnabled.SetDropdownValue(value);
                        m_photoModeValues.m_dofActive = boolValue;
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);

                        if (!m_photoModeValues.m_dofActive)
                        {
                            if (m_gaiaPostFXDOFModeURP != null)
                            {
                                m_gaiaPostFXDOFModeURP.gameObject.SetActive(false);
                            }
                            //Bokeh
                            if (m_gaiaPostFXDOFAutoFocus != null)
                            {
                                m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFFocusDistance != null)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFAperture != null)
                            {
                                m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFFocalLength != null)
                            {
                                m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                            }
                            //Gaussian
                            if (m_gaiaPostFXDOFStartDistanceURP != null)
                            {
                                m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFEndDistanceURP != null)
                            {
                                m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFMaxRadiusURP != null)
                            {
                                m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                            }
                            if (m_gaiaPostFXDOFHighQualityURP != null)
                            {
                                m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            SetURPUIModeSetup();
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_gaiaPostFXDOFEnabled == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldHDRP != null)
                    {
                        m_gaiaPostFXDOFEnabled.SetDropdownValue(value);
                        m_photoModeValues.m_dofActive = boolValue;
                        GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);

                        SetHDRPDOFMode();
                    }
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetDOFAutoFocusEnabled(int value)
        {
            if (m_isSettingValues)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);

            SyncAutoFocus();
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_gaiaPostFXDOFAutoFocus == null)
                    {
                        return;
                    }
                    if (m_depthOfField != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.SetDropdownValue(value);
                        m_photoModeValues.m_autoDOFFocus = boolValue;
                        GaiaAPI.SetAutoFocusDepthOfField(boolValue);
                        GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);

                        if (m_gaiaPostFXDOFFocusDistance != null && m_photoModeValues.m_dofActive)
                        {
                            if (boolValue)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                            else
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                            }
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_gaiaPostFXDOFAutoFocus == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldURP != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.SetDropdownValue(value);
                        m_photoModeValues.m_autoDOFFocus = boolValue;
                        GaiaAPI.SetAutoFocusDepthOfField(boolValue);
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);

                        if (m_gaiaPostFXDOFFocusDistance != null && m_photoModeValues.m_dofActive)
                        {
                            if (boolValue)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                            else
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                            }
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_gaiaPostFXDOFAutoFocus == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldHDRP != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.SetDropdownValue(value);
                        m_photoModeValues.m_autoDOFFocus = boolValue;
                        GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);

                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            if (boolValue && m_photoModeValues.m_dofActive)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                            else
                            {
                                if (m_photoModeValues.m_dofActive)
                                {
                                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                                }
                            }
                        }
                    }
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetDOFAperture(float f)
        {
            if (m_isSettingValues)
            {
                return;
            }
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_gaiaPostFXDOFAperture == null)
                    {
                        return;
                    }
                    if (m_photoModeValues != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFAperture, f);
                        m_photoModeValues.m_dofAperture = f;
                        GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_gaiaPostFXDOFAperture == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldURP != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFAperture, f);
                        m_photoModeValues.m_dofAperture = f;
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_gaiaPostFXDOFAperture == null)
                    {
                        return;
                    }
                    PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFAperture, f);
                    m_photoModeValues.m_cameraAperture = f;
                    GaiaAPI.SetHDRPCameraSettings(f, m_photoModeValues.m_cameraFocalLength, m_targetCamera);
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetDOFAperture(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
#if UNITY_POST_PROCESSING_STACK_V2 || UPPipeline || HDPipeline
            PhotoModeUtils.GetAndSetFloatValue(val, SetDOFAperture, m_minAndMaxValues.m_cameraAperture, m_gaiaPostFXDOFAperture);
#endif
        }
        public void SetDOFFocalLength(float f)
        {
            if (m_isSettingValues)
            {
                return;
            }

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_gaiaPostFXDOFFocalLength == null)
                    {
                        return;
                    }
                    if (m_depthOfField != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocalLength, f);
                        m_photoModeValues.m_dofFocalLength = f;
                        GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_gaiaPostFXDOFFocalLength == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldURP != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocalLength, f);
                        m_photoModeValues.m_dofFocalLength = f;
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_gaiaPostFXDOFFocalLength == null)
                    {
                        return;
                    }
                    PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocalLength, f);
                    m_photoModeValues.m_cameraFocalLength = f;
                    GaiaAPI.SetHDRPCameraSettings(m_photoModeValues.m_cameraAperture, f, m_targetCamera);
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetDOFFocalLength(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
#if UNITY_POST_PROCESSING_STACK_V2 || UPPipeline || HDPipeline
            PhotoModeUtils.GetAndSetFloatValue(val, SetDOFFocalLength, m_minAndMaxValues.m_cameraFocalLength, m_gaiaPostFXDOFFocalLength);
#endif
        }
        public void SetDOFFocusDistance(float f)
        {
            if (m_isSettingValues)
            {
                return;
            }

            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_gaiaPostFXDOFFocusDistance == null)
                    {
                        return;
                    }
                    if (m_depthOfField != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, f);
                        m_photoModeValues.m_dofFocusDistance = f;
                        GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_gaiaPostFXDOFFocusDistance == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldURP != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, f);
                        m_photoModeValues.m_dofFocusDistance = f;
                        GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_gaiaPostFXDOFFocusDistance == null)
                    {
                        return;
                    }
                    if (m_depthOfFieldHDRP != null)
                    {
                        PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFocusDistance, f);
                        m_photoModeValues.m_dofFocusDistance = f;
                        GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
                    }
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetDOFFocusDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }

#if UNITY_POST_PROCESSING_STACK_V2 || UPPipeline || HDPipeline
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetDOFFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistance, m_gaiaPostFXDOFFocusDistance);
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetDOFFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistanceURP, m_gaiaPostFXDOFFocusDistance);
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetDOFFocusDistance, m_minAndMaxValues.m_postFXDOFFocusDistanceHDRP, m_gaiaPostFXDOFFocusDistance);
                    break;
                }
            }
#endif
        }
        public void SetDOFKernelSize(int f)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (m_isSettingValues || m_gaiaPostFXDOFKernelSizeBuiltIn == null)
            {
                return;
            }

            if (m_depthOfField != null)
            {
                m_gaiaPostFXDOFKernelSizeBuiltIn.SetDropdownValue(f);
                m_photoModeValues.m_dofKernelSize = f;
                GaiaAPI.SetDepthOfFieldSettings(m_photoModeValues);
            }
#endif
            m_isSettingValues = false;
        }
        public void SetAutoExposure(float f)
        {
            if (m_isSettingValues || m_gaiaPostFXExposure == null)
            {
                return;
            }

            m_photoModeValues.m_postFXExposure = f;
            PhotoModeUtils.SetSliderValue(m_gaiaPostFXExposure, f);
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    GaiaAPI.SetPostFXExposure(f);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    GaiaAPI.SetPostExposureURP(f);
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    GaiaAPI.SetPostExposureHDRP(f, ExposureMode.Fixed);
#endif
                    break;
                }
            }

            m_isSettingValues = false;
        }
        public void SetAutoExposure(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            
            switch (m_renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetAutoExposure, m_minAndMaxValues.m_postFXExposure, m_gaiaPostFXExposure);
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetAutoExposure, m_minAndMaxValues.m_postFXExposureURP, m_gaiaPostFXExposure);
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    PhotoModeUtils.GetAndSetFloatValue(val, SetAutoExposure, m_minAndMaxValues.m_postFXExposureHDRP, m_gaiaPostFXExposure);
                    break;
                }
            }
        }

#region URP

        private void SetURPUIModeSetup()
        {
#if UPPipeline
            switch (m_photoModeValues.m_dofFocusModeURP)
            {
                case 0:
                {
                    if (m_gaiaPostFXDOFModeURP != null)
                    {
                        m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                    }
                    //Bokeh
                    if (m_gaiaPostFXDOFAutoFocus != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFFocusDistance != null)
                    {
                        m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFAperture != null)
                    {
                        m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFFocalLength != null)
                    {
                        m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                    }
                    //Gaussian
                    if (m_gaiaPostFXDOFStartDistanceURP != null)
                    {
                        m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFEndDistanceURP != null)
                    {
                        m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFMaxRadiusURP != null)
                    {
                        m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFHighQualityURP != null)
                    {
                        m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                    }
                    break;
                }
                case 1:
                {
                    if (m_gaiaPostFXDOFModeURP != null)
                    {
                        m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                    }
                    //Bokeh
                    if (m_gaiaPostFXDOFAutoFocus != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFFocusDistance != null)
                    {
                        m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFAperture != null)
                    {
                        m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFFocalLength != null)
                    {
                        m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                    }
                    //Gaussian
                    if (m_gaiaPostFXDOFStartDistanceURP != null)
                    {
                        m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(true);
                    }
                    if (m_gaiaPostFXDOFEndDistanceURP != null)
                    {
                        m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(true);
                    }
                    if (m_gaiaPostFXDOFMaxRadiusURP != null)
                    {
                        m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(true);
                    }
                    if (m_gaiaPostFXDOFHighQualityURP != null)
                    {
                        m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(true);
                    }
                    break;
                }
                case 2:
                {
                    if (m_gaiaPostFXDOFModeURP != null)
                    {
                        m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                    }
                    //Bokeh
                    if (m_gaiaPostFXDOFAutoFocus != null)
                    {
                        m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(true);
                    }
                    if (m_photoModeValues.m_autoDOFFocus)
                    {
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                        }
                    }
                    if (m_gaiaPostFXDOFAperture != null)
                    {
                        m_gaiaPostFXDOFAperture.gameObject.SetActive(true);
                    }
                    if (m_gaiaPostFXDOFFocalLength != null)
                    {
                        m_gaiaPostFXDOFFocalLength.gameObject.SetActive(true);
                    }
                    //Gaussian
                    if (m_gaiaPostFXDOFStartDistanceURP != null)
                    {
                        m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFEndDistanceURP != null)
                    {
                        m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFMaxRadiusURP != null)
                    {
                        m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                    }
                    if (m_gaiaPostFXDOFHighQualityURP != null)
                    {
                        m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                    }
                    break;
                }
            }
#endif
        }
        public void SetDOFModeURP(int f)
        {
#if UPPipeline
            if (m_isSettingValues || m_gaiaPostFXDOFModeURP == null)
            {
                return;
            }

            if (m_depthOfFieldURP != null)
            {
                m_gaiaPostFXDOFModeURP.SetDropdownValue(f);
                m_photoModeValues.m_dofFocusModeURP = f;
                GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);

                switch (m_photoModeValues.m_dofFocusModeURP)
                {
                    case 0:
                    {
                        if (m_gaiaPostFXDOFModeURP != null)
                        {
                            m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                        }
                        //Bokeh
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                        }
                        //Gaussian
                        if (m_gaiaPostFXDOFStartDistanceURP != null)
                        {
                            m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFEndDistanceURP != null)
                        {
                            m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFMaxRadiusURP != null)
                        {
                            m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFHighQualityURP != null)
                        {
                            m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                        }
                        break;
                    }
                    case 1:
                    {
                        if (m_gaiaPostFXDOFModeURP != null)
                        {
                            m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                        }
                        //Bokeh
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                        }
                        //Gaussian
                        if (m_gaiaPostFXDOFStartDistanceURP != null)
                        {
                            m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFEndDistanceURP != null)
                        {
                            m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFMaxRadiusURP != null)
                        {
                            m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFHighQualityURP != null)
                        {
                            m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(true);
                        }
                        break;
                    }
                    case 2:
                    {
                        if (m_gaiaPostFXDOFModeURP != null)
                        {
                            m_gaiaPostFXDOFModeURP.gameObject.SetActive(true);
                        }
                        //Bokeh
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(true);
                        }
                        if (m_photoModeValues.m_autoDOFFocus)
                        {
                            if (m_gaiaPostFXDOFFocusDistance != null)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (m_gaiaPostFXDOFFocusDistance != null)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                            }
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(true);
                        }
                        //Gaussian
                        if (m_gaiaPostFXDOFStartDistanceURP != null)
                        {
                            m_gaiaPostFXDOFStartDistanceURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFEndDistanceURP != null)
                        {
                            m_gaiaPostFXDOFEndDistanceURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFMaxRadiusURP != null)
                        {
                            m_gaiaPostFXDOFMaxRadiusURP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFHighQualityURP != null)
                        {
                            m_gaiaPostFXDOFHighQualityURP.gameObject.SetActive(false);
                        }
                        break;
                    }
                }
            }
#endif
            m_isSettingValues = false;
        }
        public void SetDOFHighQualityURP(int value)
        {
#if UPPipeline
            if (m_isSettingValues || m_gaiaPostFXDOFHighQualityURP == null)
            {
                return;
            }

            bool boolValue = PhotoModeUtils.ConvertIntToBool(value);

            if (m_depthOfFieldURP != null)
            {
                m_gaiaPostFXDOFHighQualityURP.SetDropdownValue(value);
                m_photoModeValues.m_dofHighQualityURP = boolValue;
                GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
            }
#endif

            m_isSettingValues = false;
        }
        public void SetDOFStartBlurDistance(float f)
        {
#if UPPipeline
            if (m_isSettingValues || m_gaiaPostFXDOFStartDistanceURP == null)
            {
                return;
            }

            if (m_depthOfFieldURP != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFStartDistanceURP, f);
                m_photoModeValues.m_dofStartBlurURP = f;
                GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
            }
#endif

            m_isSettingValues = false;
        }
        public void SetDOFStartBlurDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
#if UPPipeline
            PhotoModeUtils.GetAndSetFloatValue(val, SetDOFStartBlurDistance, m_minAndMaxValues.m_postFXDOFGaussianBlurStartURP, m_gaiaPostFXDOFStartDistanceURP);
#endif
        }
        public void SetDOFEndBlurDistance(float f)
        {
#if UPPipeline
            if (m_isSettingValues || m_gaiaPostFXDOFEndDistanceURP == null)
            {
                return;
            }

            if (m_depthOfFieldURP != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFEndDistanceURP, f);
                m_photoModeValues.m_dofEndBlurURP = f;
                GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
            }
#endif
            m_isSettingValues = false;
        }
        public void SetDOFEndBlurDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
#if UPPipeline
            PhotoModeUtils.GetAndSetFloatValue(val, SetDOFEndBlurDistance, m_minAndMaxValues.m_postFXDOFGaussianBlurEndURP, m_gaiaPostFXDOFEndDistanceURP);
#endif
        }
        public void SetDOFMaxBlurRadius(float f)
        {
#if UPPipeline
            if (m_isSettingValues || m_gaiaPostFXDOFMaxRadiusURP == null)
            {
                return;
            }

            if (m_depthOfFieldURP != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFMaxRadiusURP, f);
                m_photoModeValues.m_dofMaxRadiusBlur = f;
                GaiaAPI.SetDepthOfFieldSettingsURP(m_photoModeValues);
            }
#endif

            m_isSettingValues = false;
        }
        public void SetDOFMaxBlurRadius(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
#if UPPipeline
            PhotoModeUtils.GetAndSetFloatValue(val, SetDOFMaxBlurRadius, m_minAndMaxValues.m_postFXDOFGaussianBlurMaxRadiusURP, m_gaiaPostFXDOFMaxRadiusURP);
#endif
        }

#endregion
#region HDRP

#if HDPipeline
        public void SetDOFModeHDRP(int f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFModeHDRP == null)
            {
                return;
            }

            if (m_depthOfFieldHDRP != null)
            {
                m_gaiaPostFXDOFModeHDRP.SetDropdownValue(f);
                m_photoModeValues.m_dofFocusModeHDRP = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);

                SetHDRPDOFMode();
            }
            m_isSettingValues = false;
        }
        public void SetDOFQualityHDRP(int f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFHighQualityHDRP == null)
            {
                return;
            }

            if (m_depthOfFieldHDRP != null)
            {
                m_gaiaPostFXDOFHighQualityHDRP.SetDropdownValue(f);
                m_photoModeValues.m_dofQualityHDRP = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
            }

            m_isSettingValues = false;
        }
        public void SetHDRPDOFNearStartBlur(float f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFNearStartDistanceHDRP == null)
            {
                return;
            }

            if (m_photoModeValues != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFNearStartDistanceHDRP, f);
                m_photoModeValues.m_dofNearBlurStart = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
            }
            m_isSettingValues = false;
        }
        public void SetHDRPDOFNearStartBlur(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetHDRPDOFNearStartBlur, m_minAndMaxValues.m_postFXDOFNearBlurStart, m_gaiaPostFXDOFNearStartDistanceHDRP);
        }
        public void SetHDRPDOFNearEndBlur(float f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFNearEndDistanceHDRP == null)
            {
                return;
            }

            if (m_depthOfFieldHDRP != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFNearEndDistanceHDRP, f);
                m_photoModeValues.m_dofNearBlurEnd = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
            }
            m_isSettingValues = false;
        }
        public void SetHDRPDOFNearEndBlur(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetHDRPDOFNearEndBlur, m_minAndMaxValues.m_postFXDOFNearBlurEnd, m_gaiaPostFXDOFNearEndDistanceHDRP);
        }
        public void SetHDRPDOFFarStartBlur(float f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFFarStartDistanceHDRP == null)
            {
                return;
            }

            if (m_photoModeValues != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFarStartDistanceHDRP, f);
                m_photoModeValues.m_dofFarBlurStart = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
            }
            m_isSettingValues = false;
        }
        public void SetHDRPDOFFarStartBlur(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetHDRPDOFFarStartBlur, m_minAndMaxValues.m_postFXDOFFarBlurStart, m_gaiaPostFXDOFFarStartDistanceHDRP);
        }
        public void SetHDRPDOFFarEndBlur(float f)
        {
            if (m_isSettingValues || m_gaiaPostFXDOFFarEndDistanceHDRP == null)
            {
                return;
            }

            if (m_depthOfFieldHDRP != null)
            {
                PhotoModeUtils.SetSliderValue(m_gaiaPostFXDOFFarEndDistanceHDRP, f);
                m_photoModeValues.m_dofFarBlurEnd = f;
                GaiaAPI.SetDepthOfFieldSettingsHDRP(m_photoModeValues);
            }
            m_isSettingValues = false;
        }
        public void SetHDRPDOFFarEndBlur(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetHDRPDOFFarEndBlur, m_minAndMaxValues.m_postFXDOFFarBlurEnd, m_gaiaPostFXDOFFarEndDistanceHDRP);
        }
        private void SetHDRPDOFMode()
        {
            if (!m_photoModeValues.m_dofActive)
            {
                if (m_gaiaPostFXDOFModeHDRP != null)
                {
                    m_gaiaPostFXDOFModeHDRP.gameObject.SetActive(false);
                }
                //UsePhysicalCamera
                if (m_gaiaPostFXDOFAutoFocus != null)
                {
                    m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFFocusDistance != null)
                {
                    m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFAperture != null)
                {
                    m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFFocalLength != null)
                {
                    m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                }
                //Manual
                if (m_gaiaPostFXDOFNearStartDistanceHDRP != null)
                {
                    m_gaiaPostFXDOFNearStartDistanceHDRP.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFNearEndDistanceHDRP != null)
                {
                    m_gaiaPostFXDOFNearEndDistanceHDRP.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFFarStartDistanceHDRP != null)
                {
                    m_gaiaPostFXDOFFarStartDistanceHDRP.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFFarEndDistanceHDRP != null)
                {
                    m_gaiaPostFXDOFFarEndDistanceHDRP.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFMaxRadius != null)
                {
                    m_gaiaPostFXDOFMaxRadius.gameObject.SetActive(false);
                }
                if (m_gaiaPostFXDOFHighQualityHDRP != null)
                {
                    m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(false);
                }
            }
            else
            {
                switch (m_photoModeValues.m_dofFocusModeHDRP)
                {
                    case 0:
                    {
                        //UsePhysicalCamera
                        if (m_gaiaPostFXDOFModeHDRP != null)
                        {
                            m_gaiaPostFXDOFModeHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFHighQualityHDRP != null)
                        {
                            m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(false);
                        }
                        //Manual
                        if (m_gaiaPostFXDOFNearStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearStartDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFNearEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearEndDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFarStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarStartDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFarEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarEndDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFMaxRadius != null)
                        {
                            m_gaiaPostFXDOFMaxRadius.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFHighQualityHDRP != null)
                        {
                            m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(false);
                        }
                        break;
                    }
                    case 1:
                    {
                        //UsePhysicalCamera
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(true);
                        }
                        if (m_photoModeValues.m_autoDOFFocus)
                        {
                            if (m_gaiaPostFXDOFFocusDistance != null)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (m_gaiaPostFXDOFFocusDistance != null)
                            {
                                m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(true);
                            }
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFHighQualityHDRP != null)
                        {
                            m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFModeHDRP != null)
                        {
                            m_gaiaPostFXDOFModeHDRP.gameObject.SetActive(true);
                        }
                        //Manual
                        if (m_gaiaPostFXDOFNearStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearStartDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFNearEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearEndDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFarStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarStartDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFarEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarEndDistanceHDRP.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFMaxRadius != null)
                        {
                            m_gaiaPostFXDOFMaxRadius.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFHighQualityHDRP != null)
                        {
                            m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(false);
                        }
                        break;
                    }
                    case 2:
                    {
                        //UsePhysicalCamera
                        if (m_gaiaPostFXDOFAutoFocus != null)
                        {
                            m_gaiaPostFXDOFAutoFocus.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocusDistance != null)
                        {
                            m_gaiaPostFXDOFFocusDistance.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFAperture != null)
                        {
                            m_gaiaPostFXDOFAperture.gameObject.SetActive(false);
                        }
                        if (m_gaiaPostFXDOFFocalLength != null)
                        {
                            m_gaiaPostFXDOFFocalLength.gameObject.SetActive(false);
                        }
                        //Manual
                        if (m_gaiaPostFXDOFNearStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearStartDistanceHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFNearEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFNearEndDistanceHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFFarStartDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarStartDistanceHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFFarEndDistanceHDRP != null)
                        {
                            m_gaiaPostFXDOFFarEndDistanceHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFMaxRadius != null)
                        {
                            m_gaiaPostFXDOFMaxRadius.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFHighQualityHDRP != null)
                        {
                            m_gaiaPostFXDOFHighQualityHDRP.gameObject.SetActive(true);
                        }
                        if (m_gaiaPostFXDOFModeHDRP != null)
                        {
                            m_gaiaPostFXDOFModeHDRP.gameObject.SetActive(true);
                        }
                        break;
                    }
                }
            }
        }

#endif

#endregion

#endregion
#region Set Grass System

#if FLORA_PRESENT
        public void SetGlobalGrassDensity(float f)
        {
            if (m_isSettingValues || m_globalGrassDensity == null)
            {
                return;
            }
            m_photoModeValues.m_globalGrassDensity = f;
            if (m_detailManager != null)
            {
                m_detailManager.Settings.ObjectGlobalDensityModifier = f;
                m_detailManager.SetGlobals();
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_globalGrassDensity, f);
            m_isSettingValues = false;
        }
        public void SetGlobalGrassDensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalGrassDensity, m_minAndMaxValues.m_globalGrassDensity, m_globalGrassDensity);
        }
        public void SetGlobalGrassDistance(float f)
        {
            if (m_isSettingValues || m_globalGrassDistance == null)
            {
                return;
            }
            m_photoModeValues.m_globalGrassDistance = f;
            if (m_detailManager != null)
            {
                m_detailManager.Settings.ObjectGlobalDistanceModifier = f;
                m_detailManager.SetGlobals();
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_globalGrassDistance, f);
            m_isSettingValues = false;
        }
        public void SetGlobalGrassDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalGrassDistance, m_minAndMaxValues.m_globalGrassDistance, m_globalGrassDistance);
        }
        public void SetGlobalCameraCellDistance(float f)
        {
            if (m_isSettingValues || m_globalCameraCellDistance == null)
            {
                return;
            }
            m_photoModeValues.m_cameraCellDistance = f;
            if (m_detailManager != null)
            {
                m_detailManager.Settings.TerrainTileGlobalDistanceModifier = f;
                m_detailManager.SetGlobals();
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_globalCameraCellDistance, f);
            m_isSettingValues = false;
        }
        public void SetGlobalCameraCellDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalCameraCellDistance, m_minAndMaxValues.m_cameraCellDistance, m_globalCameraCellDistance);
        }
        public void SetGlobalCameraCellSubdivision(float f)
        {
            if (m_isSettingValues || m_globalCameraCellSubdivision == null)
            {
                return;
            }
            m_photoModeValues.m_cameraCellSubdivision = (int)f;
            if (m_detailManager != null)
            {
                m_detailManager.Settings.CameraCellGlobalSubdivisionModifier = (int)f;
                m_detailManager.SetGlobals();
            }
            m_isSettingValues = true;
            PhotoModeUtils.SetSliderValue(m_globalCameraCellSubdivision, f);
            m_isSettingValues = false;
        }
        public void SetGlobalCameraCellSubdivision(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetGlobalCameraCellSubdivision, m_minAndMaxValues.m_cameraCellSubdivision, m_globalCameraCellSubdivision);
        }
#endif

#endregion
#region Terrain Functions

        public void SetTerrainDetailDensity(float f)
        {
            if (m_isSettingValues || m_terrainDetailDensity == null)
            {
                return;
            }

            m_photoModeValues.m_terrainDetailDensity = f;
            PhotoModeUtils.SetSliderValue(m_terrainDetailDensity, f);
            GaiaAPI.SetTerrainDetails(f, m_photoModeValues.m_terrainDetailDistance);

            m_isSettingValues = false;
        }
        public void SetTerrainDetailDensity(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetTerrainDetailDensity, m_minAndMaxValues.m_terrainDetailDensity, m_terrainDetailDensity);
        }
        public void SetTerrainDetailDistance(float f)
        {
            if (m_isSettingValues || m_terrainDetailDistance == null)
            {
                return;
            }

            m_photoModeValues.m_terrainDetailDistance = f;
            PhotoModeUtils.SetSliderValue(m_terrainDetailDistance, f);
            GaiaAPI.SetTerrainDetails(m_photoModeValues.m_terrainDetailDensity, f);

            m_isSettingValues = false;
        }
        public void SetTerrainDetailDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetTerrainDetailDistance, m_minAndMaxValues.m_terrainDetailDistance, m_terrainDetailDistance);
        }
        public void SetTerrainHeightResolution(float f)
        {
            if (m_isSettingValues || m_terrainHeightResolution == null)
            {
                return;
            }

            m_photoModeValues.m_terrainPixelError = f;
            PhotoModeUtils.SetSliderValue(m_terrainHeightResolution, f);
            GaiaAPI.SetTerrainPixelErrorAndBaseMapTexture(f, m_photoModeValues.m_terrainBasemapDistance);

            m_isSettingValues = false;
        }
        public void SetTerrainHeightResolution(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetTerrainHeightResolution, m_minAndMaxValues.m_terrainPixelError, m_terrainHeightResolution);
        }
        public void SetTerrainTextureDistance(float f)
        {
            if (m_isSettingValues || m_terrainTextureDistance == null)
            {
                return;
            }

            m_photoModeValues.m_terrainBasemapDistance = f;
            PhotoModeUtils.SetSliderValue(m_terrainTextureDistance, f);
            GaiaAPI.SetTerrainPixelErrorAndBaseMapTexture(m_photoModeValues.m_terrainPixelError, f);

            m_isSettingValues = false;
        }
        public void SetTerrainTextureDistance(string val)
        {
            if (m_isSettingValues)
            {
                return;
            }
            PhotoModeUtils.GetAndSetFloatValue(val, SetTerrainTextureDistance, m_minAndMaxValues.m_terrainBasemapDistance,m_terrainTextureDistance);
        }
        public void SetTerrainDrawInstanced(int value)
        {
            if (m_isSettingValues || m_terrainDrawInstanced == null)
            {
                return;
            }

            m_photoModeValues.m_drawInstanced = PhotoModeUtils.ConvertIntToBool(value);
            m_terrainDrawInstanced.SetDropdownValue(value);
            GaiaAPI.SetTerrainDrawInstanced(m_photoModeValues.m_drawInstanced);

            m_isSettingValues = false;
        }

#endregion
#region UI Helper Functions

        /// <summary>
        /// Refreshes all the runtime photo mode UI
        /// </summary>
        private void RefreshAllUI()
        {
            SetAllInputFieldsToTrue();
            if (m_transformSettings.m_photoMode != null)
            {
                SetUnityScreenshotResolution(m_photoModeValues.m_screenshotResolution);
                SetUnityScreenshotImageFormat(m_photoModeValues.m_screenshotImageFormat);
                SetPhotoModeLoadSettings(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_loadSavedSettings));
                SetPhotoModeRevertOnDisabledSettings(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_revertOnDisabled));
                SetPhotoModeShowFPS(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_showFPS));
                SetPhotoModeShowReticule(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_showReticle));
                SetPhotoModeShowRuleOfThirds(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_showRuleOfThirds));
            }
            if (m_transformSettings.m_unity != null)
            {
                SetUnityVolume(m_photoModeValues.m_globalVolume);
                SetUnityLODBias(m_photoModeValues.m_lodBias);
                SetUnityShadowResolution(m_photoModeValues.m_shadowResolution);
                SetUnityVSync(m_photoModeValues.m_vSync);
                SetUnityTargetFPS(m_photoModeValues.m_targetFPS);
                SetUnityShadowDistance(m_photoModeValues.m_shadowDistance);
                SetUnityShadowCascades(m_photoModeValues.m_shadowCascades);
            }
            if (m_transformSettings.m_camera != null)
            {
                SetUnityAA(m_photoModeValues.m_antiAliasing);
                SetUnityFieldOfView(m_photoModeValues.m_fieldOfView);
                SetUnityCameraRoll(m_photoModeValues.m_cameraRoll);
                SetUnityCullingDistance(m_photoModeValues.m_gaiaCullinDistance);
            }
            if (m_transformSettings.m_water != null)
            {
                SetGaiaWaterReflectionEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_gaiaWaterReflectionEnabled));
                SetGaiaWaterReflectionDistance(m_photoModeValues.m_gaiaWaterReflectionDistance);
                SetGaiaWaterReflectionResolution(m_photoModeValues.m_gaiaWaterReflectionResolution);
                SetGaiaWaterReflectionLODBias(m_photoModeValues.m_gaiaReflectionsLODBias);
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
                        SetGaiaUnderwaterFogDistance(m_photoModeValues.m_gaiaUnderwaterFogDistance);
                        break;
                    }
                    default:
                    {
                        switch (RenderSettings.fogMode)
                        {
                            case FogMode.Linear:
                            {
                                SetGaiaUnderwaterFogDistance(m_photoModeValues.m_gaiaUnderwaterFogDistance);
                                break;
                            }
                            default:
                            {
                                SetGaiaUnderwaterFogDistance(m_photoModeValues.m_gaiaUnderwaterFogDensity);
                                break;
                            }
                        }
                        break;
                    }
                }
                UpdateUnderwaterFogColor();

            }
            if (m_transformSettings.m_postFX != null)
            {
                switch (m_renderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        SetDOFEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_dofActive));
                        SetDOFAutoFocusEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_autoDOFFocus));
                        SetDOFFocusDistance(m_photoModeValues.m_dofFocusDistance);
                        SetDOFAperture(m_photoModeValues.m_dofAperture);
                        SetDOFFocalLength(m_photoModeValues.m_dofFocalLength);
                        SetDOFKernelSize((int)m_photoModeValues.m_dofKernelSize);
                        SetAutoExposure(m_photoModeValues.m_postFXExposure);
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.Universal:
                    {
#if UPPipeline
                        SetDOFEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_dofActive));
                        SetDOFAutoFocusEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_autoDOFFocus));
                        SetDOFModeURP(m_photoModeValues.m_dofFocusModeURP);
                        SetDOFFocusDistance(m_photoModeValues.m_dofFocusDistance);
                        SetDOFAperture(m_photoModeValues.m_dofAperture);
                        SetDOFFocalLength(m_photoModeValues.m_dofFocalLength);
                        SetDOFStartBlurDistance(m_photoModeValues.m_dofStartBlurURP);
                        SetDOFEndBlurDistance(m_photoModeValues.m_dofEndBlurURP);
                        SetDOFMaxBlurRadius(m_photoModeValues.m_dofMaxRadiusBlur);
                        SetDOFHighQualityURP(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_dofHighQualityURP));
                        SetAutoExposure(m_photoModeValues.m_postFXExposure);
#endif
                        break;
                    }
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    {
#if HDPipeline
                        if (m_depthOfFieldHDRP != null)
                        {
                            SetDOFEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_dofActive));
                            SetDOFAutoFocusEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_autoDOFFocus));
                            SetDOFModeHDRP(m_photoModeValues.m_dofFocusModeHDRP);
                            SetDOFQualityHDRP(m_photoModeValues.m_dofQualityHDRP);
                            SetDOFFocusDistance(m_photoModeValues.m_dofFocusDistance);
                            SetDOFAperture(m_photoModeValues.m_cameraAperture);
                            SetDOFFocalLength(m_photoModeValues.m_cameraFocalLength);
                            SetHDRPDOFNearStartBlur(m_photoModeValues.m_dofNearBlurStart);
                            SetHDRPDOFNearEndBlur(m_photoModeValues.m_dofNearBlurEnd);
                            SetHDRPDOFFarStartBlur(m_photoModeValues.m_dofFarBlurStart);
                            SetHDRPDOFFarEndBlur(m_photoModeValues.m_dofFarBlurEnd);
                            SetAutoExposure(m_photoModeValues.m_postFXExposure);
                        }
#endif
                        break;
                    }
                }
            }
            if (m_transformSettings.m_terrain != null)
            {
                if (m_activeTerrain != null)
                {
                    SetTerrainDrawInstanced(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_drawInstanced));
                    SetTerrainDetailDensity(m_photoModeValues.m_terrainDetailDensity);
                    SetTerrainDetailDistance(m_photoModeValues.m_terrainDetailDistance);
                    SetTerrainHeightResolution(m_photoModeValues.m_terrainPixelError);
                    SetTerrainTextureDistance(m_photoModeValues.m_terrainBasemapDistance);

#if FLORA_PRESENT
                    if (m_detailManager != null)
                    {
                        //SetGlobalGrassDensity(m_photoModeValues.m_globalGrassDensity);
                        //SetGlobalGrassDistance(m_photoModeValues.m_globalGrassDistance);
                        //SetGlobalCameraCellDistance(m_photoModeValues.m_cameraCellDistance);
                        //SetGlobalCameraCellSubdivision(m_photoModeValues.m_cameraCellSubdivision);
                    }
#endif
                }
            }
            if (m_transformSettings.m_lighting != null)
            {
                if (m_pwWeatherPresent)
                {
#if GAIA_PRO_PRESENT
                    SetAdditionalLinearFog(m_photoModeValues.m_gaiaAdditionalLinearFog);
                    SetAdditionalExponentialFog(m_photoModeValues.m_gaiaAdditionalExponentialFog);
                    SetGaiaSunAngle(m_photoModeValues.m_sunRotation);
                    SetGaiaTime(m_photoModeValues.m_gaiaTime);
                    SetNewColorPickerRefs(m_photoModeValues.m_fogColor, m_gaiaFogColor.m_colorPreviewButton, false);
                    SetGaiaTimeOfDayEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_gaiaTimeOfDayEnabled));
                    SetGaiaTimeScale(m_photoModeValues.m_gaiaTimeScale);
#endif
                }
                else
                {
                    if (m_mainSunLight != null)
                    {
                        SetGaiaSunAngle(m_photoModeValues.m_sunRotation);
                        SetGaiaSunPitch(m_photoModeValues.m_sunPitch);
                        SetGaiaSunIntensity(m_photoModeValues.m_sunIntensity);
                        UpdateSunColor();
                        if (!m_hdrpTimeOfDay)
                        {
                            SetGaiaSunOverride(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_sunOverride));
                        }
                    }
#if GAIA_PRO_PRESENT
                    if (m_hdrpTimeOfDay)
                    {
                        SetGaiaTime(m_photoModeValues.m_gaiaTime);
                        SetGaiaTimeOfDayEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_gaiaTimeOfDayEnabled));
                        SetGaiaTimeScale(m_photoModeValues.m_gaiaTimeScale);
                    }
#endif
                    UpdateAmbientSkyColor();
                    UpdateAmbientEquaotrColor();
                    UpdateAmbientGroundColor();
                    SetAmbientIntensity(m_photoModeValues.m_ambientIntensity);
                    if (m_unitySkyboxPresent)
                    {
                        switch (m_renderPipeline)
                        {
                            case GaiaConstants.EnvironmentRenderer.HighDefinition:
                            {
                                SetGaiaSkyboxRotation(m_photoModeValues.m_skyboxRotation);
                                SetGaiaSkyboxExposure(m_photoModeValues.m_skyboxExposure);
                                SetGaiaFogEnd(m_photoModeValues.m_fogEnd);
                                SetNewColorPickerRefs(m_photoModeValues.m_fogColor, m_gaiaFogColor.m_colorPreviewButton, false);
                                break;
                            }
                            default:
                            {
                                SetGaiaSkyboxRotation(m_photoModeValues.m_skyboxRotation);
                                SetGaiaSkyboxExposure(m_photoModeValues.m_skyboxExposure);
                                UpdateSkyboxTint();
                                break;
                            }
                        }
                        SetGaiaSkyboxOverride(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_skyboxOverride));
                    }

                    SetGaiaFogMode((int)m_photoModeValues.m_fogMode);
                    if (m_gaiaFogColor != null)
                    {
                        SetNewColorPickerRefs(m_photoModeValues.m_fogColor, m_gaiaFogColor.m_colorPreviewButton, false);
                    }

                    SetGaiaFogStart(m_photoModeValues.m_fogStart);
                    SetGaiaFogEnd(m_photoModeValues.m_fogEnd);
                    SetGaiaFogDensity(m_photoModeValues.m_fogDensity);
                    SetGaiaFogOverride(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_fogOverride));
#if HDPipeline
                    switch (m_renderPipeline)
                    {
                        case GaiaConstants.EnvironmentRenderer.HighDefinition:
                        {
                            SetDensityVolumeEffectType(m_photoModeValues.m_densityVolumeEffectType);
                            SetDensityVolumeTilingResolution(m_photoModeValues.m_densityVolumeEffectType);
                            SetDensityVolumeFogDistance(m_photoModeValues.m_densityVolumeFogDistance);
                            UpdateDensityVolumeAlbedoColor();
                            break;
                        }
                    }
                    SetOverrideDensityVolume(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_overrideDensityVolume));
#endif
                }
                UpdateFogSettings();
            }

            SetGaiaWindDirection(m_photoModeValues.m_gaiaWindDirection);
            SetGaiaWindSpeed(m_photoModeValues.m_gaiaWindSpeed);
            SetWindOverride(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_gaiaWindSettingsOverride));
#if GAIA_PRO_PRESENT
            if (m_streamingSettingsArea != null)
            {
                SetGaiaLoadRange(m_photoModeValues.m_gaiaLoadRange);
                SetGaiaImpostorRange(m_photoModeValues.m_gaiaImpostorRange);
            }
            if (m_transformSettings.m_lighting != null)
            {
                if (m_weather != null)
                {
                    SetGaiaWeatherEnabled(PhotoModeUtils.ConvertBoolToInt(m_photoModeValues.m_gaiaWeatherEnabled));
                }
            }
#endif
        }
        /// <summary>
        /// Function used to reset all input fields so that the string value can be updated
        /// </summary>
        private void SetAllInputFieldsToTrue()
        {
            if (PhotoMode.CurrentRuntimeUIElements.Count > 0)
            {
                foreach (PhotoModeUIHelper runtimeUI in CurrentRuntimeUIElements)
                {
                    PhotoModeUtils.SetIsUsingSliderValue(runtimeUI);
                }
            }
        }

#endregion
#region Color Picker Functions

        public void OpenColorPickerFog()
        {
            OpenColorPicker(m_gaiaFogColor, m_photoModeValues.m_fogColor, ColorPickerReferenceMode.FogColor, UpdateFogSettings);
        }
        public void OpenColorPickerSunColor()
        {
            OpenColorPicker(m_gaiaSunColor, m_photoModeValues.m_sunColor, ColorPickerReferenceMode.SunColor, UpdateSunColor);
        }
        public void OpenColorPickerSkyboxTint()
        {
            OpenColorPicker(m_gaiaSkyboxTint, m_photoModeValues.m_skyboxTint, ColorPickerReferenceMode.SkyboxTintColor, UpdateSkyboxTint);
        }
        public void OpenColorPickerDensityAlbedo()
        {
#if HDPipeline
            OpenColorPicker(m_densityVolumeAlbedoColor, m_photoModeValues.m_densityVolumeAlbedoColor, ColorPickerReferenceMode.DensityAlbedoColor, UpdateDensityVolumeAlbedoColor);
#endif
        }
        public void OpenColorPickerUnderwaterFogColor()
        {
            OpenColorPicker(m_gaiaUnderwaterFogColor, m_photoModeValues.m_gaiaUnderwaterFogColor, ColorPickerReferenceMode.UnderwaterFogColor, UpdateUnderwaterFogColor);
        }
        public void OpenColorPickerAmbientSkyColor()
        {
            OpenColorPicker(m_ambientSkyColor, m_photoModeValues.m_ambientSkyColor, ColorPickerReferenceMode.AmbientSkyColor, UpdateAmbientSkyColor, true);
        }
        public void OpenColorPickerAmbientEquatorColor()
        {
            OpenColorPicker(m_ambientEquatorColor, m_photoModeValues.m_ambientEquatorColor, ColorPickerReferenceMode.AmbientEquatorColor, UpdateAmbientEquaotrColor, true);
        }
        public void OpenColorPickerAmbientGroundColor()
        {
            OpenColorPicker(m_ambientGroundColor, m_photoModeValues.m_ambientGroundColor, ColorPickerReferenceMode.AmbientGroundColor, UpdateAmbientGroundColor, true);
        }
        public void SetFogColor()
        {
            UpdateFogSettings();
        }
        public void SetNewColorPickerRefs(Color color, Button currentColorImage, bool colorPickerEnabled, bool applyHDR = false)
        {
            m_colorPicker.RefColor(ref color, ref currentColorImage, applyHDR);
            m_updateColorPickerRef = colorPickerEnabled;
        }
        private void OpenColorPicker(PhotoModeUIHelper runtimeUI, Color colorValue, ColorPickerReferenceMode mode, UnityAction onChanged, bool hdr = false)
        {
            if (runtimeUI != null && m_colorPicker != null)
            {
                runtimeUI.SetColorPreviewImage(colorValue, hdr);
                SetColorPickerVisable(true);
                m_colorPicker.SetColorValue(colorValue, hdr);
                SetNewColorPickerRefs(colorValue, runtimeUI.m_colorPreviewButton, true);
                m_colorPicker.SetLastColorValue(colorValue);
                m_colorPicker.SetCurrentFocusedName(runtimeUI.name);
                m_colorPicker.Refresh();
                m_colorPickerRefMode = mode;
                m_colorPicker.UpdateOnChangedMethod(onChanged);
            }
        }
        /// <summary>
        /// Shows or hides the color picker
        /// </summary>
        /// <param name="value"></param>
        private void SetColorPickerVisable(bool value)
        {
            if (m_colorPicker != null)
            {
                if (value)
                {
                    m_colorPicker.gameObject.SetActive(true);
                }
                else
                {
                    m_colorPicker.gameObject.SetActive(false);
                }

            }
        }
        /// <summary>
        /// Processes color picker updates
        /// </summary>
        public void UpdateColorPicker()
        {
            if (m_updateColorPickerRef)
            {
                switch (m_colorPickerRefMode)
                {
                    case ColorPickerReferenceMode.FogColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_fogColor, ref m_gaiaFogColor.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.SunColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_sunColor, ref m_gaiaSunColor.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.SkyboxTintColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_skyboxTint, ref m_gaiaSkyboxTint.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.DensityAlbedoColor:
                    {
#if HDPipeline
                        m_colorPicker.RefColor(ref m_photoModeValues.m_densityVolumeAlbedoColor, ref m_densityVolumeAlbedoColor.m_colorPreviewButton, true);
#endif
                        break;
                    }
                    case ColorPickerReferenceMode.UnderwaterFogColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_gaiaUnderwaterFogColor, ref m_gaiaUnderwaterFogColor.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.AmbientSkyColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_ambientSkyColor, ref m_ambientSkyColor.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.AmbientEquatorColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_ambientEquatorColor, ref m_ambientEquatorColor.m_colorPreviewButton, true);
                        break;
                    }
                    case ColorPickerReferenceMode.AmbientGroundColor:
                    {
                        m_colorPicker.RefColor(ref m_photoModeValues.m_ambientGroundColor, ref m_ambientGroundColor.m_colorPreviewButton, true);
                        break;
                    }
                }
            }
        }

#endregion

#endregion
    }
}