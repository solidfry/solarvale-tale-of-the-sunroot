using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using Gaia.Internal;
using System;
using System.Linq;
using System.IO;
using ProceduralWorlds.Gaia.PackageSystem;

namespace Gaia.Pipeline
{
    [CustomEditor(typeof(UnityPipelineProfile))]
    public class UnityPipelineProfileEditor : PWEditor
    {
        private GUIStyle m_boxStyle;
        private EditorUtils m_editorUtils;
        private UnityPipelineProfile m_profile;
        private string m_version;
        private bool[] m_shaderMappingLibraryFoldouts;
        private GUIStyle m_matlibButtonStyle;

        private void OnEnable()
        {
            //Initialization
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            //Get Gaia Lighting Profile object
            m_profile = (UnityPipelineProfile)target;

            m_version = PWApp.CONF.Version;

            m_shaderMappingLibraryFoldouts = new bool[m_profile.m_ShaderMappingLibrary.Length];
        }
        public override void OnInspectorGUI()
        {           
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

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

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Profile Version: " + m_version);

            bool enableEditMode = System.IO.Directory.Exists(GaiaUtils.GetAssetPath("Dev Utilities"));
            if (enableEditMode)
            {
                m_profile.m_editorUpdates = EditorGUILayout.ToggleLeft("Use Procedural Worlds Editor Settings", m_profile.m_editorUpdates);
            }
            else
            {
                m_profile.m_editorUpdates = false;
            }

            if (m_profile.m_editorUpdates)
            {
                m_editorUtils.Panel("ProfileSettings", ProfileSettingsEnabled, false);
            }
            else
            {
                ProfileSettingsRelease();
            }

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profile, "Made changes");
                EditorUtility.SetDirty(m_profile);
            }
        }
        private void ProfileSettingsEnabled(bool helpEnabled)
        {
            GUILayout.BeginVertical("Pipeline Profile Settings", m_boxStyle);
            GUILayout.Space(20);
            EditorGUI.indentLevel++;
            DrawDefaultInspector();
            EditorGUI.indentLevel--;

            if (m_matlibButtonStyle == null)
            {
                m_matlibButtonStyle = new GUIStyle(GUI.skin.button);
                m_matlibButtonStyle.margin = new RectOffset(40, m_matlibButtonStyle.margin.right, m_matlibButtonStyle.margin.top, m_matlibButtonStyle.margin.bottom);
            }

            //Draw the Material library settings
            EditorGUILayout.LabelField("Shader Mapping");
            EditorGUI.indentLevel++;
            for (int i=0; i < m_profile.m_ShaderMappingLibrary.Length; i++)
            {
                ShaderMappingEntry entry = m_profile.m_ShaderMappingLibrary[i];
                m_shaderMappingLibraryFoldouts[i] = EditorGUILayout.Foldout(m_shaderMappingLibraryFoldouts[i], entry.m_builtInShaderName);
                if (m_shaderMappingLibraryFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    entry.m_builtInShaderName = EditorGUILayout.TextField("Builtin Shader", entry.m_builtInShaderName);
                    entry.m_URPReplacementShaderName = EditorGUILayout.TextField("URP Shader", entry.m_URPReplacementShaderName);
                    entry.m_HDRPReplacementShaderName = EditorGUILayout.TextField("HDRP Shader", entry.m_HDRPReplacementShaderName);
                }
                if (GUILayout.Button("Remove " + entry.m_builtInShaderName, m_matlibButtonStyle))
                {
                    if (EditorUtility.DisplayDialog("Delete Shader Mapping Entry", "Are you sure you want to delete the entire entry for '" + entry.m_builtInShaderName + "' ?", "OK", "Cancel"))
                    {
                        m_profile.m_ShaderMappingLibrary = GaiaUtils.RemoveArrayIndexAt(m_profile.m_ShaderMappingLibrary, i);
                        m_shaderMappingLibraryFoldouts = GaiaUtils.RemoveArrayIndexAt(m_shaderMappingLibraryFoldouts, i);
                    }

                }
                if (GUILayout.Button("Insert entry below", m_matlibButtonStyle))
                {
                    m_profile.m_ShaderMappingLibrary = GaiaUtils.InsertElementInArray(m_profile.m_ShaderMappingLibrary, new ShaderMappingEntry() { m_builtInShaderName = "New Entry" } ,i+1);
                    m_shaderMappingLibraryFoldouts = GaiaUtils.InsertElementInArray(m_shaderMappingLibraryFoldouts, true, i+1);

                }

            }
            if (GUILayout.Button("Add new Material Library entry"))
            {
                m_profile.m_ShaderMappingLibrary = GaiaUtils.AddElementToArray(m_profile.m_ShaderMappingLibrary, new ShaderMappingEntry() { m_builtInShaderName = "New Entry" });
                m_shaderMappingLibraryFoldouts = GaiaUtils.AddElementToArray(m_shaderMappingLibraryFoldouts, true);
            }
            EditorGUI.indentLevel--;

            GUILayout.EndVertical();
        }
        private void ProfileSettingsRelease()
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Installed Pipeline: " + m_profile.m_activePipelineInstalled);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("URP", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_profile.m_setUPPipelineProfile = EditorGUILayout.Toggle(new GUIContent("Set Pipeline Asset", "If enabled our pipeline asset will be set as the active pipeline asset."), m_profile.m_setUPPipelineProfile);
            if (!m_profile.m_setUPPipelineProfile)
            {
                EditorGUILayout.HelpBox("You have disabled 'Set Pipeline Asset', you will need to set your own pipeline asset in Project Settings/Graphics/Pipeline Asset.", MessageType.Warning);
            }
            m_profile.m_UPAutoConfigureTerrain = EditorGUILayout.Toggle(new GUIContent("Auto Config Terrain", "If enabled terrain will be configured to use unity default URP material."), m_profile.m_UPAutoConfigureTerrain);
            m_profile.m_UPAutoConfigureWater = EditorGUILayout.Toggle(new GUIContent("Auto Config Water", "If enabled water will be configured to use the correct pipeline shader"), m_profile.m_UPAutoConfigureWater);
            m_profile.m_UPAutoConfigureCamera = EditorGUILayout.Toggle(new GUIContent("Auto Config Camera", "If enabled camera will be configured to use the correct pipeline"), m_profile.m_UPAutoConfigureCamera);
            m_profile.m_UPAutoConfigureProbes = EditorGUILayout.Toggle(new GUIContent("Auto Config Probes", "If enabled probes will be configured to use the correct pipeline"), m_profile.m_UPAutoConfigureProbes);
            m_profile.m_UPAutoConfigureLighting = EditorGUILayout.Toggle(new GUIContent("Auto Config Lighting", "If enabled lighting will be configured to use the correct pipeline"), m_profile.m_UPAutoConfigureLighting);
            m_profile.m_UPAutoConfigureBiomePostFX = EditorGUILayout.Toggle(new GUIContent("Auto Config Post FX", "If enabled post FX will be configured to use the correct pipeline"), m_profile.m_UPAutoConfigureBiomePostFX);
            EditorGUI.indentLevel--;
            if (AutoDisabled(m_profile.m_UPAutoConfigureTerrain, m_profile.m_UPAutoConfigureWater,
                m_profile.m_UPAutoConfigureCamera, m_profile.m_UPAutoConfigureProbes,
                m_profile.m_UPAutoConfigureLighting, m_profile.m_UPAutoConfigureBiomePostFX))
            {
                EditorGUILayout.HelpBox("We highly recommend enabling all the 'Auto Config' for URP, Leaving one of these disabled could result in a shader going pink, terrain going invisable or lighting and post fx not setup to URP correctly.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("HDRP", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_profile.m_setHDPipelineProfile = EditorGUILayout.Toggle(new GUIContent("Set Pipeline Asset", "If enabled our pipeline asset will be set as the active pipeline asset."), m_profile.m_setHDPipelineProfile);
            if (!m_profile.m_setHDPipelineProfile)
            {
                EditorGUILayout.HelpBox("You have disabled 'Set Pipeline Asset', you will need to set your own pipeline asset in Project Settings/Graphics/Pipeline Asset.", MessageType.Warning);
            }
            m_profile.m_HDAutoConfigureTerrain = EditorGUILayout.Toggle(new GUIContent("Auto Config Terrain", "If enabled terrain will be configured to use unity default HDRP material."), m_profile.m_HDAutoConfigureTerrain);
            m_profile.m_HDAutoConfigureWater = EditorGUILayout.Toggle(new GUIContent("Auto Config Water", "If enabled water will be configured to use the correct pipeline shader"), m_profile.m_HDAutoConfigureWater);
            m_profile.m_HDAutoConfigureCamera = EditorGUILayout.Toggle(new GUIContent("Auto Config Camera", "If enabled camera will be configured to use the correct pipeline"), m_profile.m_HDAutoConfigureCamera);
            m_profile.m_HDAutoConfigureProbes = EditorGUILayout.Toggle(new GUIContent("Auto Config Probes", "If enabled probes will be configured to use the correct pipeline"), m_profile.m_HDAutoConfigureProbes);
            m_profile.m_HDAutoConfigureLighting = EditorGUILayout.Toggle(new GUIContent("Auto Config Lighting", "If enabled lighting will be configured to use the correct pipeline"), m_profile.m_HDAutoConfigureLighting);
            m_profile.m_HDAutoConfigureBiomePostFX = EditorGUILayout.Toggle(new GUIContent("Auto Config Post FX", "If enabled post FX will be configured to use the correct pipeline"), m_profile.m_HDAutoConfigureBiomePostFX);
            EditorGUI.indentLevel--;
            if (AutoDisabled(m_profile.m_HDAutoConfigureTerrain, m_profile.m_HDAutoConfigureWater,
                m_profile.m_HDAutoConfigureCamera, m_profile.m_HDAutoConfigureProbes,
                m_profile.m_HDAutoConfigureLighting, m_profile.m_HDAutoConfigureBiomePostFX))
            {
                EditorGUILayout.HelpBox("We highly recommend enabling all the 'Auto Config' for HDRP, Leaving one of these disabled could result in a shader going pink, terrain going invisable or lighting and post fx not setup to HDRP correctly.", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }
        /// <summary>
        /// Returns false if one of the bools is disabled
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="water"></param>
        /// <param name="camera"></param>
        /// <param name="probes"></param>
        /// <param name="lighting"></param>
        /// <param name="postFX"></param>
        /// <returns></returns>
        private bool AutoDisabled(bool terrain, bool water, bool camera, bool probes, bool lighting, bool postFX)
        {
            if (!terrain)
            {
                return true;
            }
            if (!water)
            {
                return true;
            }
            if (!camera)
            {
                return true;
            }
            if (!probes)
            {
                return true;
            }
            if (!lighting)
            {
                return true;
            }
            if (!postFX)
            {
                return true;
            }

            return false;
        }
    }
}