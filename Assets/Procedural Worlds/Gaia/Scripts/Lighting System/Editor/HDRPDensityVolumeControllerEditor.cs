#if HDPipeline
using Gaia.Internal;
using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace Gaia
{
    [CustomEditor(typeof(HDRPDensityVolumeController))]
    public class HDRPDensityVolumeControllerEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private HDRPDensityVolumeController m_profile;

        private void OnEnable()
        {
            //Get Gaia Lighting Profile object
            m_profile = (HDRPDensityVolumeController)target;

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            m_editorUtils.Panel("GlobalSettings", GlobalSettingsPanel, false, true, true);
        }
        private void GlobalSettingsPanel(bool helpEnabled)
        {
            if (m_profile != null)
            {
                EditorGUI.BeginChangeCheck();
                m_profile.DensityVolumeProfile.m_singleScatteringAlbedo = m_editorUtils.ColorField("SingleScatteringAlbedo", m_profile.DensityVolumeProfile.m_singleScatteringAlbedo, helpEnabled);
                m_profile.DensityVolumeProfile.m_fogDistance = m_editorUtils.FloatField("FogDistance", m_profile.DensityVolumeProfile.m_fogDistance, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("VolumeSettings");
                m_profile.DensityVolumeProfile.m_effectType = (DensityVolumeEffectType)m_editorUtils.EnumPopup("SizeEffectType", m_profile.DensityVolumeProfile.m_effectType, helpEnabled);
                if (m_profile.DensityVolumeProfile.m_effectType == DensityVolumeEffectType.Custom)
                {
                    EditorGUI.indentLevel++;
                    m_profile.DensityVolumeProfile.m_size = m_editorUtils.Vector3Field("Size", m_profile.DensityVolumeProfile.m_size, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                m_profile.DensityVolumeProfile.m_blendDistance = m_editorUtils.FloatField("BlendDistance", m_profile.DensityVolumeProfile.m_blendDistance, helpEnabled);
                m_profile.DensityVolumeProfile.m_invertBlend = m_editorUtils.Toggle("InvertBlend", m_profile.DensityVolumeProfile.m_invertBlend, helpEnabled);
                m_profile.DensityVolumeProfile.m_distanceFadeStart = m_editorUtils.FloatField("DistanceFadeStart", m_profile.DensityVolumeProfile.m_distanceFadeStart, helpEnabled);
                m_profile.DensityVolumeProfile.m_distanceFadeEnd = m_editorUtils.FloatField("DistanceFadeStart", m_profile.DensityVolumeProfile.m_distanceFadeEnd, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("DensityMaskTexture");
                m_profile.DensityVolumeProfile.m_texture = (Texture3D)m_editorUtils.ObjectField("Texture", m_profile.DensityVolumeProfile.m_texture, typeof(Texture3D), helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.DensityVolumeProfile.m_scrollSpeed = m_editorUtils.Vector3Field("ScrollSpeed", m_profile.DensityVolumeProfile.m_scrollSpeed, helpEnabled);
                m_profile.DensityVolumeProfile.m_resolution = (DensityVolumeResolution)m_editorUtils.EnumPopup("TilingResolution", m_profile.DensityVolumeProfile.m_resolution, helpEnabled);
                if (m_profile.DensityVolumeProfile.m_resolution == DensityVolumeResolution.Custom)
                {
                    EditorGUI.indentLevel++;
                    m_profile.DensityVolumeProfile.m_tiling = m_editorUtils.Vector3Field("Tiling", m_profile.DensityVolumeProfile.m_tiling, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    m_profile.ApplyChanges();
                    EditorUtility.SetDirty(m_profile);
                }
            }
        }
    }
}
#endif