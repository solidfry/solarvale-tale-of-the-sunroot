using System.Collections;
using System.Collections.Generic;
using Gaia.Internal;
using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace Gaia
{
    [CustomEditor(typeof(FreeCamera))]
    public class FreeCameraEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private FreeCamera m_freeCamera;

        private void OnEnable()
        {
            m_freeCamera = (FreeCamera)target;

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
            m_editorUtils.Initialize();
            if (m_freeCamera == null)
            {
                m_freeCamera = (FreeCamera)target;
            }

            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_editorUtils.Heading("Settings");
            EditorGUI.indentLevel++;
            m_freeCamera.enableInputCapture = m_editorUtils.Toggle("EnableInputCapture", m_freeCamera.enableInputCapture, helpEnabled);
            m_freeCamera.lockAndHideCursor = m_editorUtils.Toggle("LockAndHideCursor", m_freeCamera.lockAndHideCursor, helpEnabled);
            m_freeCamera.holdRightMouseCapture = m_editorUtils.Toggle("HoldRightMouseCapture", m_freeCamera.holdRightMouseCapture, helpEnabled);
            m_freeCamera.lookSpeed = m_editorUtils.FloatField("LookSpeed", m_freeCamera.lookSpeed, helpEnabled);
            m_freeCamera.moveSpeed = m_editorUtils.FloatField("MoveSpeed", m_freeCamera.moveSpeed, helpEnabled);
            m_freeCamera.sprintSpeed = m_editorUtils.FloatField("SprintSpeed", m_freeCamera.sprintSpeed, helpEnabled);
            m_freeCamera.m_useScrollSpeedIncrease = m_editorUtils.Toggle("UseScrollSpeedIncrease", m_freeCamera.m_useScrollSpeedIncrease, helpEnabled);
            if (m_freeCamera.m_useScrollSpeedIncrease)
            {
                EditorGUI.indentLevel++;
                m_freeCamera.m_speedIncreaseValue = m_editorUtils.FloatField("SpeedIncreaseValue", m_freeCamera.m_speedIncreaseValue, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_freeCamera);
            }
        }
    }
}