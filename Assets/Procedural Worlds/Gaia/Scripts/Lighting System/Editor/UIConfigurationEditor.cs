using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Gaia
{
    [CustomEditor(typeof(UIConfiguration))]
    public class UIConfigurationEditor : Editor
    {
        private UIConfiguration m_ui;

        public override void OnInspectorGUI()
        {
            if (m_ui == null)
            {
                m_ui = (UIConfiguration) target;
            }

            if (m_ui != null)
            {
                UIConfiguration.RenderPipeline = GaiaUtils.GetActivePipeline();
            }

            if (m_ui.m_photoMode == null)
            {
                m_ui.m_photoMode = AssetDatabase.LoadAssetAtPath<GameObject>(GaiaUtils.GetAssetPath("PhotoModeUI.prefab"));
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("General UI Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_ui.m_hideMouseCursor = EditorGUILayout.Toggle(new GUIContent("Hide Mouse Cursor", "If enabled the mouse cursor will be hidden and lock to the screen"), m_ui.m_hideMouseCursor);
            m_ui.m_uiToggleButton = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("UI Toggle Key", "When this key is pressed it will hide or show the UI text."), m_ui.m_uiToggleButton);
            m_ui.m_uiControllerSelection = (UIControllerSelection)EditorGUILayout.ObjectField("UI Controller Selection", m_ui.m_uiControllerSelection, typeof(UIControllerSelection), true);
            m_ui.m_textContent = (GameObject)EditorGUILayout.ObjectField("UI Text", m_ui.m_textContent, typeof(GameObject), true);
            m_ui.m_locationManagerText = (Text)EditorGUILayout.ObjectField("Location Manager Text", m_ui.m_locationManagerText, typeof(Text), true);
            m_ui.UITextColor = EditorGUILayout.ColorField(new GUIContent("UI Text Color", "Sets the text color."), m_ui.UITextColor);
            m_ui.TextSize = EditorGUILayout.IntField("Text Size", m_ui.TextSize);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tooltips Settings", EditorStyles.boldLabel);
            m_ui.m_useTooltips = EditorGUILayout.Toggle("Use Tooltips", m_ui.m_useTooltips);
            if (m_ui.m_useTooltips)
            {
                EditorGUI.indentLevel++;
                m_ui.m_tooltipManager = (GameObject)EditorGUILayout.ObjectField("Tooltips Manager", m_ui.m_tooltipManager, typeof(GameObject), false);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Photo Mode Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_ui.m_usePhotoMode = EditorGUILayout.Toggle("Use Photo Mode", m_ui.m_usePhotoMode);
            if (m_ui.m_usePhotoMode)
            {
                m_ui.m_enablePhotoMode = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Photo Mode Toggle Key", "When this key is pressed it will hide or show photo mode"), m_ui.m_enablePhotoMode);
                m_ui.m_currentPhotoModeProfile = (PhotoModeProfile)EditorGUILayout.ObjectField("Photo Mode Profile", m_ui.m_currentPhotoModeProfile, typeof(PhotoModeProfile), false);
                m_ui.m_loadFromLastSaved = EditorGUILayout.Toggle(new GUIContent("Load Last Saved", "When enabled photo mode will try load up the last saved settings this is stored in the scene profile. When you exit play mode these saved settings are removed"), m_ui.m_loadFromLastSaved);
                m_ui.m_resetOnDisable = EditorGUILayout.Toggle(new GUIContent("Reset On Disable", "When enabled if you exit photo mode the settings before you enter play mode will be applied"), m_ui.m_resetOnDisable);
                m_ui.m_photoMode = (GameObject)EditorGUILayout.ObjectField("Photo Mode Prefab", m_ui.m_photoMode, typeof(GameObject), false);
                m_ui.m_photoModeText = (Text)EditorGUILayout.ObjectField("Photo Mode Text", m_ui.m_photoModeText, typeof(Text), true);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pause Menu Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_ui.m_showPauseMenu = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Show Pause Menu", "When this key is pressed it will hide or show the pause menu."), m_ui.m_showPauseMenu);
            m_ui.m_pauseGame = EditorGUILayout.Toggle(new GUIContent("Pause Game", "If enabled when paused timeScale will be set to 0 and then back to 1 when you un-pause"), m_ui.m_pauseGame);
            m_ui.m_pauseMenu = (GameObject)EditorGUILayout.ObjectField("Puase Menu", m_ui.m_pauseMenu, typeof(GameObject), true);
            m_ui.m_useBackgroundBlur = EditorGUILayout.Toggle(new GUIContent("Use Background Blur", "If enabled the depth of field blur will be applied when the game is paused"), m_ui.m_useBackgroundBlur);
            if (m_ui.m_useBackgroundBlur)
            {
                EditorGUI.indentLevel++;
                m_ui.m_backgroundBlur = (GameObject)EditorGUILayout.ObjectField("Background Blur", m_ui.m_backgroundBlur, typeof(GameObject), true);
                switch (UIConfiguration.RenderPipeline)
                {
                    case GaiaConstants.EnvironmentRenderer.BuiltIn:
#if UNITY_POST_PROCESSING_STACK_V2
                        m_ui.BuiltInBlurProfile = (PostProcessProfile)EditorGUILayout.ObjectField("Blur Post Processing Profile", m_ui.BuiltInBlurProfile, typeof(PostProcessProfile), true);
#endif
                        break;
                    case GaiaConstants.EnvironmentRenderer.Universal:
#if UPPipeline
                        m_ui.URPBlurProfile = (VolumeProfile)EditorGUILayout.ObjectField("Blur Post Processing Profile", m_ui.URPBlurProfile, typeof(VolumeProfile), true);
#endif
                        break;
                    case GaiaConstants.EnvironmentRenderer.HighDefinition:
#if HDPipeline
                        m_ui.HDRPBlurProfile = (VolumeProfile)EditorGUILayout.ObjectField("Blur Post Processing Profile", m_ui.HDRPBlurProfile, typeof(VolumeProfile), true);
#endif
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                m_ui.ExecuteUIRefresh();
                EditorUtility.SetDirty(m_ui);
            }
        }
    }
}