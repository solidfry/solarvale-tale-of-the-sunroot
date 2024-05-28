using System;
using System.Collections.Generic;
using UnityEngine;
using PWCommon5;
using Gaia.Internal;
using UnityEditor;
using UnityEngine.UI;

namespace Gaia
{
    [CustomEditor(typeof(PhotoMode))]
    public class PhotoModeEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private PhotoMode m_photoMode;
        private GUIStyle m_boxStyle;

        #region MyRegion

        /// <summary>
        /// Setup on enable
        /// </summary>
        private void OnEnable()
        {
            m_photoMode = (PhotoMode)target;
            if (m_photoMode != null)
            {
                m_photoMode.SetupSystemMetrics();
                RefreshProfilesArray();
            }

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }
        /// <summary>
        /// Setup on destroy
        /// </summary>
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }
        /// <summary>
        /// Process every frame in the inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (m_photoMode == null)
            {
                m_photoMode = (PhotoMode)target;
            }

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

            m_editorUtils.Initialize();

            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
        }

        #endregion
        #region UI

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            AutoIndent("GlobalSetup", GlobalSettings, helpEnabled);
            AutoIndent("PlayerControllerSetup", ControllerSettings, helpEnabled);
            AutoIndent("GUISettings", UISettings, helpEnabled, false);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_photoMode);
            }
            if (m_editorUtils.Button("CopySystemMetricsToClipboard"))
            {
                CopySystemMetricsToClipboard(m_photoMode);
            }
        }
        private void GlobalSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            m_photoMode.m_photoModeProfile = (PhotoModeProfile)m_editorUtils.ObjectField("PhotoModeProfile", m_photoMode.m_photoModeProfile, typeof(PhotoModeProfile), false);
            if (m_editorUtils.Button("Reset", GUILayout.MaxWidth(55f)))
            {
                if (m_photoMode.m_photoModeProfile != null)
                {
                    m_photoMode.m_photoModeProfile.Reset();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Found Profiles that can be loaded " + m_photoMode.m_photoModeProfiles.Count);
            if (m_editorUtils.Button("Refresh", GUILayout.MaxWidth(135f)))
            {
                RefreshProfilesArray();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            m_photoMode.m_runtimeUIPrefab = (PhotoModeUIHelper)m_editorUtils.ObjectField("UIHelper", m_photoMode.m_runtimeUIPrefab, typeof(PhotoModeUIHelper), false, helpEnabled);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Frame Rate Counter Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_photoMode.m_30FPSColor = m_editorUtils.ColorField("30FPSOrLess", m_photoMode.m_30FPSColor, helpEnabled);
            m_photoMode.m_60FPSColor = m_editorUtils.ColorField("60FPSOrLess", m_photoMode.m_60FPSColor, helpEnabled);
            m_photoMode.m_120FPSColor = m_editorUtils.ColorField("120FPSOrLess", m_photoMode.m_120FPSColor, helpEnabled);
            m_photoMode.m_maxFPSColor = m_editorUtils.ColorField("120FPSAndHigher", m_photoMode.m_maxFPSColor, helpEnabled);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Picker Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_photoMode.m_colorPicker = (PhotoModeColorPicker)m_editorUtils.ObjectField("ColorPicker", m_photoMode.m_colorPicker, typeof(PhotoModeColorPicker), true, helpEnabled);
            EditorGUI.indentLevel--;
        }
        private void ControllerSettings(bool helpEnabled)
        {
            m_photoMode.m_freezePlayerController = m_editorUtils.Toggle("FreezeController", m_photoMode.m_freezePlayerController, helpEnabled);
            if (m_photoMode.m_freezePlayerController)
            {
                EditorGUI.indentLevel++;
                m_photoMode.m_spawnedCamera = (GameObject)m_editorUtils.ObjectField("SpawnedCamera", m_photoMode.m_spawnedCamera, typeof(GameObject), false, helpEnabled);
                EditorGUI.indentLevel--;
            }
        }
        private void UISettings(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Image Setup");
            if (m_photoMode.m_uiImages.Count > 0)
            {
                for (int i = 0; i < m_photoMode.m_uiImages.Count; i++)
                {
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    PhotoModeImages imageData = m_photoMode.m_uiImages[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("[ " + i + "]", GUILayout.MaxWidth(40f));
                    imageData.m_name = EditorGUILayout.TextField(imageData.m_name);
                    imageData.m_image = (Sprite)EditorGUILayout.ObjectField(imageData.m_image, typeof(Sprite), false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    imageData.m_imageWidthAndHeight = EditorGUILayout.Vector2Field("Width And Height", imageData.m_imageWidthAndHeight);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        m_photoMode.m_uiImages.RemoveAt(i);
                        EditorGUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                    if (string.IsNullOrEmpty(imageData.m_name))
                    {
                        if (imageData.m_image != null)
                        {
                            imageData.m_name = imageData.m_image.name;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (GUILayout.Button("Add New UI Image"))
            {
                m_photoMode.m_uiImages.Add(new PhotoModeImages());
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Panel Buttons Setup");
            if (m_photoMode.m_panelButtons.Count > 0)
            {
                for (int i = 0; i < m_photoMode.m_panelButtons.Count; i++)
                {
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.BeginHorizontal();
                    m_photoMode.m_panelButtons[i].m_shownTitle = EditorGUILayout.TextField("Panel Title Name", m_photoMode.m_panelButtons[i].m_shownTitle);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                    {
                        m_photoMode.m_panelButtons.RemoveAt(i);
                        EditorGUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                    m_photoMode.m_panelButtons[i].m_button = (Button)EditorGUILayout.ObjectField("Button", m_photoMode.m_panelButtons[i].m_button, typeof(Button), true);
                    EditorGUILayout.EndVertical();
                }
            }
            if (GUILayout.Button("Add Panel Button"))
            {
                m_photoMode.m_panelButtons.Add(new PhotoModePanel());
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Metrics Setup");
            m_photoMode.m_fpsText = (Text)m_editorUtils.ObjectField("FPSText", m_photoMode.m_fpsText, typeof(Text), true, helpEnabled);
            m_photoMode.m_StormVersionText = (Text)m_editorUtils.ObjectField("StormVersionText", m_photoMode.m_StormVersionText, typeof(Text), true, helpEnabled);
            m_photoMode.m_OSText = (Text)m_editorUtils.ObjectField("OSText", m_photoMode.m_OSText, typeof(Text), true, helpEnabled);
            m_photoMode.m_deviceText = (Text)m_editorUtils.ObjectField("DeviceText", m_photoMode.m_deviceText, typeof(Text), true, helpEnabled);
            m_photoMode.m_systemText = (Text)m_editorUtils.ObjectField("SystemText", m_photoMode.m_systemText, typeof(Text), true, helpEnabled);
            m_photoMode.m_gpuText = (Text)m_editorUtils.ObjectField("GPUText", m_photoMode.m_gpuText, typeof(Text), true, helpEnabled);
            m_photoMode.m_gpuCapabilitiesText = (Text)m_editorUtils.ObjectField("GPUCapabilitiesText", m_photoMode.m_gpuCapabilitiesText, typeof(Text), true, helpEnabled);
            m_photoMode.m_screenInfoText = (Text)m_editorUtils.ObjectField("ScreenInfoText", m_photoMode.m_screenInfoText, typeof(Text), true, helpEnabled);
            m_photoMode.m_screenshotText = (Text)m_editorUtils.ObjectField("ScreenshotText", m_photoMode.m_screenshotText, typeof(Text), true, helpEnabled);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField(m_editorUtils.GetTextValue("ContainerRects"), EditorStyles.boldLabel);
            m_photoMode.m_transformSettings.m_photoMode = (RectTransform)m_editorUtils.ObjectField("PhotoModeSettingsArea", m_photoMode.m_transformSettings.m_photoMode, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_camera = (RectTransform)m_editorUtils.ObjectField("CameraSettingsArea", m_photoMode.m_transformSettings.m_camera, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_unity = (RectTransform)m_editorUtils.ObjectField("UnitySettingsArea", m_photoMode.m_transformSettings.m_unity, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_streamingSettingsArea = (RectTransform)m_editorUtils.ObjectField("StreamingSettingsArea", m_photoMode.m_streamingSettingsArea, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_lighting = (RectTransform)m_editorUtils.ObjectField("GaiaLightingSettingsArea", m_photoMode.m_transformSettings.m_lighting, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_water = (RectTransform)m_editorUtils.ObjectField("WaterSettingsArea", m_photoMode.m_transformSettings.m_water, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_postFX = (RectTransform)m_editorUtils.ObjectField("PostFXSettingsArea", m_photoMode.m_transformSettings.m_postFX, typeof(RectTransform), true, helpEnabled);
            m_photoMode.m_transformSettings.m_terrain = (RectTransform)m_editorUtils.ObjectField("TerrainSettingsArea", m_photoMode.m_transformSettings.m_terrain, typeof(RectTransform), true, helpEnabled);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField(m_editorUtils.GetTextValue("UIObjects"), EditorStyles.boldLabel);
            m_photoMode.m_scrollRect = (ScrollRect)m_editorUtils.ObjectField("ScrollRect", m_photoMode.m_scrollRect, typeof(ScrollRect), true, helpEnabled);
            m_photoMode.m_reticule = (GameObject)m_editorUtils.ObjectField("Reticule", m_photoMode.m_reticule, typeof(GameObject), true, helpEnabled);
            m_photoMode.m_ruleOfThirds = (GameObject)m_editorUtils.ObjectField("RuleOfThirds", m_photoMode.m_ruleOfThirds, typeof(GameObject), true, helpEnabled);
            m_photoMode.m_optionsPanel = (GameObject)m_editorUtils.ObjectField("OptionsPanel", m_photoMode.m_optionsPanel, typeof(GameObject), true, helpEnabled);
            EditorGUILayout.EndVertical();
        }

        #endregion
        #region UI Helper

        private void AutoIndent(string headerText, Action<bool> function, bool help, bool space = true)
        {
            m_editorUtils.Heading(headerText);
            EditorGUI.indentLevel++;
            function.Invoke(help);
            EditorGUI.indentLevel--;
            if (space)
            {
                EditorGUILayout.Space();
            }
        }
        private void CopySystemMetricsToClipboard(PhotoMode photoMode)
        {
            if (photoMode == null)
            {
                return;
            }

            string clipboard = "";

            //Version
            clipboard += photoMode.m_StormVersionText.text;
            clipboard = AddNewLine(clipboard);
            //OS
            clipboard += photoMode.m_OSText.text;
            clipboard = AddNewLine(clipboard);
            //Hardware Devide Info
            clipboard += photoMode.m_deviceText.text;
            clipboard = AddNewLine(clipboard);
            //Screen Resolutione ETC...
            clipboard += photoMode.m_screenInfoText.text;
            clipboard = AddNewLine(clipboard);
            //System Info
            clipboard += photoMode.m_systemText.text;
            clipboard = AddNewLine(clipboard);
            //GPU
            clipboard += photoMode.m_gpuText.text;
            clipboard = AddNewLine(clipboard);
            //GPU Capabilities
            clipboard += photoMode.m_gpuCapabilitiesText.text;
            GUIUtility.systemCopyBuffer = clipboard;
        }
        private string AddNewLine(string context)
        {
            return context += Environment.NewLine;
        }
        private void RefreshProfilesArray()
        {
            if (m_photoMode == null)
            {
                return;
            }

            if (m_photoMode.m_photoModeProfiles == null)
            {
                m_photoMode.m_photoModeProfiles = new List<PhotoModeProfile>();
            }

            m_photoMode.m_photoModeProfiles.Clear();
            PhotoModeProfile[] array = Resources.FindObjectsOfTypeAll<PhotoModeProfile>();
            if (array.Length > 0)
            {
                foreach (PhotoModeProfile profile in array)
                {
                    m_photoMode.m_photoModeProfiles.Add(profile);
                }
            }

            if (m_photoMode.m_photoModeProfile == null)
            {
                if (m_photoMode.m_photoModeProfiles.Count > 0)
                {
                    m_photoMode.m_photoModeProfile = m_photoMode.m_photoModeProfiles[0];
                    EditorUtility.SetDirty(m_photoMode);
                }
            }
        }

        #endregion
        #region Public Static

        /// <summary>
        /// Creates a new photo mode profile
        /// </summary>
        [MenuItem("Assets/Create/Procedural Worlds/Gaia/Photo Mode Profile")]
        public static void CreatePhotoModeProfile()
        {
            PhotoModeProfile asset = ScriptableObject.CreateInstance<PhotoModeProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/Photo Mode Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        #endregion
    }
}