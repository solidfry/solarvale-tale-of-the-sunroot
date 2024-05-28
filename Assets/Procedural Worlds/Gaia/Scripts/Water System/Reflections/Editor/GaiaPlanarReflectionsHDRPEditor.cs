using Gaia.Internal;
using PWCommon5;
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(GaiaPlanarReflectionsHDRP))]
    public class GaiaPlanarReflectionsHDRPEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private GaiaPlanarReflectionsHDRP m_reflection;

        private void OnEnable()
        {
            m_reflection = (GaiaPlanarReflectionsHDRP)target;

            m_reflection.EditorBuildCullingGroup();
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
        public override void OnInspectorGUI()
        {
            if (m_reflection == null)
            {
                m_reflection = (GaiaPlanarReflectionsHDRP)target;
            }

            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!
            m_editorUtils.Panel("GlobalSettings", GlobalSettingsPanel, false, true, true);
        }

        private void GlobalSettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_reflection.ReflectionRenderMode = (HDRPWaterReflectionMode)m_editorUtils.EnumPopup("ReflectionRenderMode", m_reflection.ReflectionRenderMode, helpEnabled);
            switch (m_reflection.ReflectionRenderMode)
            {
                case HDRPWaterReflectionMode.PlanarReflection:
                {
                    //m_reflection.DisableCompletely = EditorGUILayout.Toggle(new GUIContent("Disable Completely", "If enabled the planar reflection system and probe will be disabled."), m_reflection.DisableCompletely);
                    m_reflection.DisableCompletely = m_editorUtils.Toggle("DisableCompletely", m_reflection.DisableCompletely, helpEnabled);
                    if (m_reflection.DisableCompletely)
                    {
                        GUI.enabled = false;
                    }
                    m_reflection.m_renderEveryFrame = m_editorUtils.Toggle("RenderEveryFrame", m_reflection.m_renderEveryFrame, helpEnabled);
                    AdvancedWaterUtilsEditor.DrawFrustumCheckEdtior(m_editorUtils, m_reflection, helpEnabled, ref m_reflection.m_frustumData, m_reflection.EditorBuildCullingGroup);
                    m_reflection.m_positionCheckData.m_usePositionCheck = m_editorUtils.Toggle("UsePositionCheck", m_reflection.m_positionCheckData.m_usePositionCheck, helpEnabled);
                    if (m_reflection.m_positionCheckData.m_usePositionCheck)
                    {
                        EditorGUI.indentLevel++;
                        m_reflection.m_mainCamera = (Camera)m_editorUtils.ObjectField("MainCamera", m_reflection.m_mainCamera, typeof(Camera), true, helpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    m_reflection.m_reflectionIntenisty = m_editorUtils.Slider("ReflectionIntenisty", m_reflection.m_reflectionIntenisty, 0f, 10f, helpEnabled);

                    GUI.enabled = true;

                    if (!m_reflection.m_renderEveryFrame)
                    {
                        if (m_editorUtils.Button("RequestReflectionRender"))
                        {
                            m_reflection.RequestReflectionRender();
                        }
                    }
                    break;
                }
                case HDRPWaterReflectionMode.ScreenSpaceReflection:
                {
                    GUI.enabled = true;
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_reflection);
                m_reflection.UpdateReflectionIntensity(m_reflection.m_reflectionIntenisty);
            }
        }
    }
}