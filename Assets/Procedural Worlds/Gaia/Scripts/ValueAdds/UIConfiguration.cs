#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.UI;

namespace Gaia
{
    [ExecuteAlways]
    public class UIConfiguration : MonoBehaviour
    {
        public static UIConfiguration Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static UIConfiguration m_instance;

        public static GaiaConstants.EnvironmentRenderer RenderPipeline
        {
            get { return m_renderPipeline; }
            set
            {
                m_renderPipeline = value;
            }
        }
        [SerializeField] private static GaiaConstants.EnvironmentRenderer m_renderPipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;

        #region Variables

        //Global
        public bool m_hideMouseCursor = false;
        public bool m_useTooltips = true;
        public GameObject m_tooltipManager;
        public bool m_resetOnDisable = true;
        public bool m_loadFromLastSaved = true;
        public bool m_showReticule = true;
        public bool m_showRuleOfThirds = true;

        public Color32 UITextColor
        {
            get { return m_uiTextColor; }
            set
            {
                m_uiTextColor = value;
                UpdateTextColor();
            }
        }
        [SerializeField] private Color32 m_uiTextColor = Color.white;
        public int TextSize
        {
            get { return m_textSize; }
            set
            {
                if (m_textSize != value)
                {
                    m_textSize = value;
                    UpdateTextSize();
                }
            }
        }
        [SerializeField] private int m_textSize = 20;

        public KeyCode m_uiToggleButton = KeyCode.U;
        public GameObject m_textContent;

        public bool m_usePhotoMode = true;
        public PhotoModeProfile m_currentPhotoModeProfile;
        public KeyCode m_enablePhotoMode = KeyCode.F11;
        public GameObject m_photoMode;
        public UIControllerSelection m_uiControllerSelection;
        public Text m_photoModeText;
        public Text m_locationManagerText;
        public Text[] m_allTexts;
        [Range(0, 6)]
        public int m_startingPanel = 0;

        //Puase
        public KeyCode m_showPauseMenu = KeyCode.Escape;
        public bool m_pauseGame = true;
        public bool m_useBackgroundBlur = true;
        public GameObject m_pauseMenu;
        //Blur
        public GameObject m_backgroundBlur;

#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile BuiltInBlurProfile
        {
            get
            {
                if (m_builtInBlurProfile == null)
                {
                    if (!string.IsNullOrEmpty(m_builtInBlurProfileGUID))
                    {
#if UNITY_EDITOR
                        m_builtInBlurProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(AssetDatabase.GUIDToAssetPath(m_builtInBlurProfileGUID));
#endif
                    }
                }

                return m_builtInBlurProfile;
            }
            set
            {
                if (m_builtInBlurProfile != value)
                {
                    m_builtInBlurProfile = value;
#if UNITY_EDITOR
                    m_builtInBlurProfileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_builtInBlurProfile));
#endif
                }
            }
        }
        [SerializeField] private PostProcessProfile m_builtInBlurProfile;
#endif
        public string m_builtInBlurProfileGUID;

#if UPPipeline
        public VolumeProfile URPBlurProfile
        {
            get
            {
                if (m_urpBlurProfile == null)
                {
                    if (!string.IsNullOrEmpty(m_urpBlurProfileGUID))
                    {
#if UNITY_EDITOR
                        m_urpBlurProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath(m_urpBlurProfileGUID));
#endif
                    }
                }

                return m_urpBlurProfile;
            }
            set
            {
                if (m_urpBlurProfile != value)
                {
                    m_urpBlurProfile = value;
#if UNITY_EDITOR
                    m_urpBlurProfileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_urpBlurProfile));
#endif
                }
            }
        }
        [SerializeField] private VolumeProfile m_urpBlurProfile;
#endif
        public string m_urpBlurProfileGUID;

#if HDPipeline
        public VolumeProfile HDRPBlurProfile
        {
            get
            {
                if (m_hdrpBlurProfile == null)
                {
                    if (!string.IsNullOrEmpty(m_hdrpBlurProfileGUID))
                    {
#if UNITY_EDITOR
                        m_hdrpBlurProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath(m_hdrpBlurProfileGUID));
#endif
                    }
                }

                return m_hdrpBlurProfile;
            }
            set
            {
                if (m_hdrpBlurProfile != value)
                {
                    m_hdrpBlurProfile = value;
#if UNITY_EDITOR
                    m_hdrpBlurProfileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_hdrpBlurProfile));
#endif
                }
            }
        }
        [SerializeField] private VolumeProfile m_hdrpBlurProfile;
#endif
        public string m_hdrpBlurProfileGUID;

        private FreeCamera m_freeCamera;
        private GameObject m_photoModeSystem;
        private bool m_photoModeEnabled = false;
#endregion
        #region UI Text Setup
        /// <summary>
        /// Starting function setup
        /// </summary>
        private void Awake()
        {
            m_instance = this;
            m_renderPipeline = GaiaUtils.GetActivePipeline();
            if (!Application.isPlaying)
            {
                return;
            }
            ExecuteUIRefresh();
            SetupBlur(RenderPipeline);
            if (Application.isPlaying)
            {
                ProcessShowTooltips();
            }
            if (m_currentPhotoModeProfile != null)
            {
                GaiaAPI.SetPhotoModeSettings(m_currentPhotoModeProfile.Profile.m_loadSavedSettings, m_currentPhotoModeProfile.Profile.m_revertOnDisabled, m_currentPhotoModeProfile.Profile.m_showReticle, m_currentPhotoModeProfile.Profile.m_showRuleOfThirds, m_enablePhotoMode);
            }

            if (!m_hideMouseCursor)
            {
                GaiaAPI.SetCursorState(true);
            }
            else
            {
                GaiaAPI.SetCursorState(false);
            }
        }
        //Checks every frame
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Input.GetKeyDown(m_uiToggleButton))
            {
                if (m_textContent != null)
                {
                    if (m_textContent.activeInHierarchy)
                    {
                        m_textContent.gameObject.SetActive(false);
                    }
                    else
                    {
                        m_textContent.gameObject.SetActive(true);
                    }
                }
            }
            if (Input.GetKeyDown(m_showPauseMenu))
            {
                if (m_pauseMenu != null)
                {
                    if (m_pauseMenu.activeInHierarchy)
                    {
                        ReturnToGame();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }

            if (m_usePhotoMode)
            {
                if (Input.GetKeyDown(m_enablePhotoMode))
                {
                    ExecuteShowPhotoMode();
                }
            }
        }
        /// <summary>
        /// Processes on enable
        /// </summary>
        private void OnEnable()
        {
            m_instance = this;
            m_renderPipeline = GaiaUtils.GetActivePipeline();
        }
        /// <summary>
        /// Updates the text color
        /// </summary>
        private void UpdateTextColor()
        {
            if (m_allTexts.Length > 0)
            {
                foreach (Text text in m_allTexts)
                {
                    text.color = UITextColor;
                }
            }
        }
        /// <summary>
        /// Updates text size
        /// </summary>
        private void UpdateTextSize()
        {
            if (m_allTexts.Length > 0)
            {
                foreach (Text text in m_allTexts)
                {
                    text.fontSize = TextSize;
                }
            }
        }
        /// <summary>
        /// Sets the free camera component if it's null and present in the scene
        /// </summary>
        /// <param name="camera"></param>
        public void SetFreeCameraComponent(Camera camera)
        {
            if (camera != null)
            {
                if (m_freeCamera == null)
                {
                    m_freeCamera = camera.GetComponent<FreeCamera>();
                }
            }
        }
        /// <summary>
        /// When called the UI Text contents will be refreshed
        /// </summary>
        public void ExecuteUIRefresh()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                if (m_uiControllerSelection != null)
                {
                    m_uiControllerSelection.RefreshUI(sceneProfile.m_controllerType);
                }
            }

            m_allTexts = this.GetComponentsInChildren<Text>();

            if (m_textContent == null)
            {
                Transform[] transforms = this.GetComponentsInChildren<Transform>();
                if (transforms.Length > 0)
                {
                    foreach (Transform transform1 in transforms)
                    {
                        if (transform1.name.Contains("Text Layout"))
                        {
                            m_textContent = transform1.gameObject;
                            break;
                        }
                    }
                }
            }
            if (m_photoModeText != null)
            {
                m_photoModeText.text = "";
                m_photoModeText.text = string.Format("Photo Mode: {0} key to enable photo mode", m_enablePhotoMode);

                m_photoModeText.gameObject.SetActive(m_usePhotoMode);
            }
            if (m_locationManagerText != null)
            {
#if UNITY_EDITOR
                m_locationManagerText.text = "";
                string locationProfilePath = GaiaUtils.GetAssetPath("Location Profile.asset");
                if (!string.IsNullOrEmpty(locationProfilePath))
                {
                    LocationSystemScriptableObject locationProfile = AssetDatabase.LoadAssetAtPath<LocationSystemScriptableObject>(locationProfilePath);
                    if (locationProfile != null)
                    {
                        string text = "";
                        text += "Open the Location Manager from the Advanced Tab in the Gaia Manager to bookmark interesting locations: ";
                        text += string.Format("{0} + {1} (new bookmark), {0} + {2} / {3} (cycle bookmarks)", locationProfile.m_mainKey.ToString(), locationProfile.m_addBookmarkKey.ToString(), locationProfile.m_prevBookmark.ToString(), locationProfile.m_nextBookmark.ToString());

                        m_locationManagerText.text = text;
                    }
                }
#endif
            }

            if (m_freeCamera == null)
            {
                m_freeCamera = GaiaUtils.FindOOT<FreeCamera>();
            }
        }
        /// <summary>
        /// When called it will show or remove photo mode
        /// </summary>
        public void ExecuteShowPhotoMode()
        {
            if (m_photoModeEnabled)
            {
                if (m_photoModeSystem != null)
                {
                    GaiaAPI.RemovePhotoMode(false);
                }
                if (m_textContent != null && m_textContent.gameObject != null)
                {
                    m_textContent.gameObject.SetActive(true);
                }

                m_photoModeEnabled = false;
            }
            else
            {
                if (m_photoModeSystem == null)
                {
                    if (m_photoMode != null)
                    {
#if !UNITY_2020_2_OR_NEWER
                        if (RenderPipeline != GaiaConstants.EnvironmentRenderer.BuiltIn)
                        {
                            Debug.Log("Photo Mode in SRP is only supported in 2020.2 and above, please use Unity Engine Version 2020.2.0 or higher.");
                        }
                        else
                        {
                            m_photoModeSystem = GaiaAPI.InstantiatePhotoMode(m_photoMode, true);
                        }
#else
                        m_photoModeSystem = GaiaAPI.InstantiatePhotoMode(m_photoMode, true);
#endif
                    }
                    else
                    {
                        Debug.LogError("Photo Mode prefab is null");
                    }
                }

                m_photoModeEnabled = true;
            }
        }
#endregion
#region UI Events

        public void ProcessShowTooltips()
        {
            TooltipManager manager = TooltipManager.Instance;
            if (m_useTooltips)
            {
                if (manager == null)
                {
                    manager = CreateTooltipManager();
                    manager.SetInstance();
                }
            }
            else
            {
                if (manager != null)
                {
                    TooltipManager.Instance.gameObject.SetActive(m_useTooltips);
                }
            }
        }
        /// <summary>
        /// Gets or creates the tooltips manager
        /// </summary>
        /// <returns></returns>
        public TooltipManager CreateTooltipManager()
        {
            TooltipManager manager = TooltipManager.Instance;
            if (manager == null)
            {
                if (m_tooltipManager != null)
                {
                    Instantiate(m_tooltipManager);
                    manager = TooltipManager.Instance;
                }
            }

            return manager;
        }
        /// <summary>
        /// Quits the application
        /// </summary>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
        /// <summary>
        /// Hides the game and returns back to the game
        /// </summary>
        public void ReturnToGame()
        {
            if (m_pauseMenu != null)
            {
                m_pauseMenu.SetActive(false);
                if (m_pauseGame)
                {
                    Time.timeScale = 1f;
                    AudioListener.pause = false;
                    if (m_freeCamera != null)
                    {
                        m_freeCamera.enableInputCapture = true;
                    }
                }

                if (m_backgroundBlur != null)
                {
                    if (m_useBackgroundBlur)
                    {
                        m_backgroundBlur.SetActive(false);
                    }
                }

                if (m_hideMouseCursor)
                {
                    GaiaAPI.SetCursorState(false);
                }
            }
        }
        /// <summary>
        /// Pauses the game
        /// </summary>
        public void Pause()
        {
            if (m_pauseMenu != null)
            {
                m_pauseMenu.SetActive(true);
                if (m_pauseGame)
                {
                    Time.timeScale = 0f;
                    AudioListener.pause = true;
                    if (m_freeCamera != null)
                    {
                        m_freeCamera.enableInputCapture = false;
                    }

                    if (m_backgroundBlur != null)
                    {
                        if (m_useBackgroundBlur)
                        {
                            m_backgroundBlur.SetActive(true);
                        }
                        else
                        {
                            m_backgroundBlur.SetActive(false);
                        }
                    }
                }

                if (m_hideMouseCursor)
                {
                    GaiaAPI.SetCursorState(true);
                }
            }
        }
        /// <summary>
        /// Sets up the blur
        /// </summary>
        /// <param name="renderPipeline"></param>
        private void SetupBlur(GaiaConstants.EnvironmentRenderer renderPipeline)
        {
            if (m_backgroundBlur == null)
            {
                return;
            }

            switch (renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    PostProcessVolume volume = GetPostProcessVolumeBuiltIn(m_backgroundBlur, "TransparentFX");
                    RemovePostFXVolume(m_backgroundBlur, GaiaConstants.EnvironmentRenderer.Universal);
                    volume.sharedProfile = BuiltInBlurProfile;
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    Volume volume = GetPostProcessVolumeSRP(m_backgroundBlur, "Default");
                    RemovePostFXVolume(m_backgroundBlur, GaiaConstants.EnvironmentRenderer.BuiltIn);
                    volume.sharedProfile = URPBlurProfile;
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    Volume volume = GetPostProcessVolumeSRP(m_backgroundBlur, "Default");
                    RemovePostFXVolume(m_backgroundBlur, GaiaConstants.EnvironmentRenderer.BuiltIn);
                    volume.sharedProfile = HDRPBlurProfile;
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Gets the built in post processing volume
        /// Adds it is fit's null
        /// </summary>
        /// <param name="blurPostObject"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessVolume GetPostProcessVolumeBuiltIn(GameObject blurPostObject, string layer)
        {
            PostProcessVolume volume = blurPostObject.GetComponent<PostProcessVolume>();
            if (volume == null)
            {
                volume = blurPostObject.AddComponent<PostProcessVolume>();
            }

            volume.isGlobal = true;
            volume.priority = 99;
            blurPostObject.layer = LayerMask.NameToLayer(layer);

            return volume;
        }
#endif
        /// <summary>
        /// Gets the srp post processing volume
        /// Adds it is fit's null
        /// </summary>
        /// <param name="blurPostObject"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
#if UPPipeline || HDPipeline
        private Volume GetPostProcessVolumeSRP(GameObject blurPostObject, string layer)
        {
            Volume volume = blurPostObject.GetComponent<Volume>();
            if (volume == null)
            {
                volume = blurPostObject.AddComponent<Volume>();
            }

            volume.isGlobal = true;
            volume.priority = 99;
            blurPostObject.layer = LayerMask.NameToLayer(layer);

            return volume;
        }
#endif
        /// <summary>
        /// Removes Volume
        /// </summary>
        /// <param name="blurPostObject"></param>
        /// <param name="renderPipeline"></param>
        private void RemovePostFXVolume(GameObject blurPostObject, GaiaConstants.EnvironmentRenderer renderPipeline)
        {
            if (blurPostObject == null)
            {
                return;
            }

            switch (renderPipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    PostProcessVolume volume = blurPostObject.GetComponent<PostProcessVolume>();
                    if (volume != null)
                    {
                        DestroyImmediate(volume);
                    }
#endif
                    break;
                }
                default:
                {
#if UPPipeline || HDPipeline
                    Volume volume = blurPostObject.GetComponent<Volume>();
                    if (volume != null)
                    {
                        DestroyImmediate(volume);
                    }
#endif
                    break;
                }
            }
        }

#endregion
    }
}