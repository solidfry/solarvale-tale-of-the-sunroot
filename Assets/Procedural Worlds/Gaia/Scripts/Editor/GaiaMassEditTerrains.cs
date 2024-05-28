using Gaia.Internal;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
using UnityEditor;
using UnityEngine;

namespace Gaia
{

    public enum MassEditOperation { SetHeight, ApplySettings }

    public enum MassEditTarget { AllTerrains, ActiveTerrains, ManualList }

    /// <summary>
    /// Utility to mass edit multiple terrains - apply terrain settings to multiple or all terrains at the same time
    /// Helpful if your scene consists of 100s of terrains, and you would need to adjust the same setting on all of them.
    /// </summary>
    /// 

    class GaiaMassEditTerrains : EditorWindow, IPWEditor
    {

        private EditorUtils m_editorUtils;

        private MassEditTarget m_target;
        private MassEditOperation m_operation;
        private List<Terrain> m_manualList = new List<Terrain>();

        /// <summary>
        /// This is a "prototype" Terrain that can be referenced for the mass edit process. All settings of the prototype will be applied to the target terrains also if this feature is used.
        /// </summary>
        private Terrain m_prototypeTerrain;
        private Terrain m_TerrainCopy;

        private float m_targetHeight = 0f;

        private float[,] m_heights;
        float m_normalizedHeight = 0;

        private bool m_copyHMResolution = false;

        private GaiaSettings m_gaiaSettings;
        private GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }

        public bool PositionChecked { get; set; }

        void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            titleContent = m_editorUtils.GetContent("WindowTitle");
        }

        void OnGUI()
        {
            m_editorUtils.Initialize();
            m_editorUtils.Panel("MassEditTerrains", DrawMainPanel, true);
        }

        private void DrawMainPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical();
            {
                m_target = (MassEditTarget)m_editorUtils.EnumPopup("Target", m_target, helpEnabled);

                if (m_target == MassEditTarget.ManualList)
                {

                    int deleteIndex = -99;

                    if (m_manualList.Count == 0)
                    {
                        m_manualList.Add(null);
                    }

                    for (int i = 0; i < m_manualList.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            m_manualList[i] = (Terrain)EditorGUILayout.ObjectField(m_manualList[i], typeof(Terrain),true);
                            if (m_manualList.Count > 1)
                            {
                                if (m_editorUtils.Button("Delete"))
                                {
                                    deleteIndex = i;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (deleteIndex != -99)
                    {
                        m_manualList.RemoveAt(deleteIndex);
                    }

                    if (m_editorUtils.Button("AddTerrain"))
                    {
                        m_manualList.Add(null);
                    }

                }

                m_operation = (MassEditOperation)m_editorUtils.EnumPopup("Operation", m_operation, helpEnabled);

                switch (m_operation)
                {
                    case MassEditOperation.SetHeight:
                        m_editorUtils.Panel("SetHeightPanel", DrawSetHeight, true);
                        break;
                    case MassEditOperation.ApplySettings:
                        m_editorUtils.Panel("ApplySettingsPanel", DrawApplySettings, true);
                        break;
                }

                Color oldBGColor = GUI.backgroundColor;

                GUI.backgroundColor = GaiaSettings.GetActionButtonColor();

                Action<Terrain> action = null;

                switch (m_operation)
                {
                    case MassEditOperation.SetHeight:
                        action = SetHeight;
                        break;
                    case MassEditOperation.ApplySettings:
                        action = ApplySettings;
                        break;
                }


                if (m_editorUtils.Button("Start"))
                {
                    if (EditorUtility.DisplayDialog("Mass Editing Terrains", "You are about to automatically change mutiple terrains in your scene. This can irreversibly change your scene, so please make sure you made a backup of your project before proceeding.", "Ok", "Cancel"))
                    {
                        string progressTitle = "Processing Terrains";
                        string progressMessage = "Please Wait...";

                        if (m_prototypeTerrain != null)
                        {
                            //create a copy of the terrain - the original prototype terrain might unload during processing, and we need to keep the settings alive
                            m_TerrainCopy = Instantiate<Terrain>(m_prototypeTerrain);
                        }


                        switch (m_target)
                        {
                            case MassEditTarget.AllTerrains:
                                if (GaiaUtils.HasDynamicLoadedTerrains())
                                {
                                    GaiaUtils.CallFunctionOnDynamicLoadedTerrains(action, true, null, progressMessage);
                                }
                                else
                                {
                                    try
                                    {
                                        for (int i = 0; i < Terrain.activeTerrains.Length; i++)
                                        {
                                            ProgressBar.Show(ProgressBarPriority.MultiTerrainAction, progressTitle, progressMessage, i, Terrain.activeTerrains.Length, true);
                                            Terrain t = Terrain.activeTerrains[i];
                                            action(t);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Error while processing terrains: {ex.Message}, Stack Trace: {ex.StackTrace}");
                                    }
                                    finally
                                    {
                                        ProgressBar.Clear(ProgressBarPriority.MultiTerrainAction);
                                    }
                                }
                                break;
                            case MassEditTarget.ActiveTerrains:
                                try
                                {
                                    for (int i = 0; i < Terrain.activeTerrains.Length; i++)
                                    {
                                        ProgressBar.Show(ProgressBarPriority.MultiTerrainAction, progressTitle, progressMessage, i, Terrain.activeTerrains.Length, true);
                                        Terrain t = Terrain.activeTerrains[i];
                                        if (t.gameObject.activeInHierarchy && t.enabled)
                                        {
                                            action(t);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error while processing terrains: {ex.Message}, Stack Trace: {ex.StackTrace}");
                                }
                                finally
                                {
                                    ProgressBar.Clear(ProgressBarPriority.MultiTerrainAction);
                                }
                                break;
                            case MassEditTarget.ManualList:
                                try
                                {
                                    for (int i = 0; i < Terrain.activeTerrains.Length; i++)
                                    {
                                        ProgressBar.Show(ProgressBarPriority.MultiTerrainAction, progressTitle, progressMessage, i, Terrain.activeTerrains.Length, true);
                                        Terrain t = Terrain.activeTerrains[i];
                                        if (m_manualList.Contains(t))
                                        {
                                            action(t);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error while processing terrains: {ex.Message}, Stack Trace: {ex.StackTrace}");
                                }
                                finally
                                {
                                    ProgressBar.Clear(ProgressBarPriority.MultiTerrainAction);
                                }
                                break;
                        }

                        GameObject.DestroyImmediate(m_TerrainCopy.gameObject);

                    }
                }
            }
            EditorGUILayout.EndVertical();

        }

        private void ApplySettings(Terrain t)
        {
            //Basic Terrain
            t.groupingID = m_TerrainCopy.groupingID;
            t.allowAutoConnect = m_TerrainCopy.allowAutoConnect;
            t.drawHeightmap = m_TerrainCopy.drawHeightmap;
            t.drawInstanced = m_TerrainCopy.drawInstanced;
            t.heightmapPixelError = m_TerrainCopy.heightmapPixelError;
            t.basemapDistance = m_TerrainCopy.basemapDistance;
            t.shadowCastingMode = m_TerrainCopy.shadowCastingMode;
            t.reflectionProbeUsage = m_TerrainCopy.reflectionProbeUsage;
            t.materialTemplate = m_TerrainCopy.materialTemplate;
            //Tree and Detail Objects
            t.drawTreesAndFoliage = m_TerrainCopy.drawTreesAndFoliage;
            t.bakeLightProbesForTrees = m_TerrainCopy.bakeLightProbesForTrees;
            t.preserveTreePrototypeLayers = m_TerrainCopy.preserveTreePrototypeLayers;
            t.detailObjectDistance = m_TerrainCopy.detailObjectDistance;
            t.detailObjectDensity = m_TerrainCopy.detailObjectDensity;
            t.treeDistance = m_TerrainCopy.treeDistance;
            t.treeBillboardDistance = m_TerrainCopy.treeBillboardDistance;
            t.treeCrossFadeLength = m_TerrainCopy.treeCrossFadeLength;
            t.treeMaximumFullLODCount = m_TerrainCopy.treeMaximumFullLODCount;
            //Wind Settings for Grass
            t.terrainData.wavingGrassSpeed = m_TerrainCopy.terrainData.wavingGrassSpeed;
            t.terrainData.wavingGrassStrength = m_TerrainCopy.terrainData.wavingGrassStrength;
            t.terrainData.wavingGrassAmount = m_TerrainCopy.terrainData.wavingGrassAmount;
            t.terrainData.wavingGrassTint = m_TerrainCopy.terrainData.wavingGrassTint;
            ////Mesh Resolution
            t.terrainData.size = m_TerrainCopy.terrainData.size;
            t.terrainData.SetDetailResolution(m_TerrainCopy.terrainData.detailResolution, m_TerrainCopy.terrainData.detailResolutionPerPatch);
            //Holes Settings
            t.terrainData.enableHolesTextureCompression = m_TerrainCopy.terrainData.enableHolesTextureCompression;
            ////Texture Resolutions
            if (m_copyHMResolution)
            {
                t.terrainData.heightmapResolution = m_TerrainCopy.terrainData.heightmapResolution;
            }
            t.terrainData.alphamapResolution = m_TerrainCopy.terrainData.alphamapResolution;
            t.terrainData.baseMapResolution = m_TerrainCopy.terrainData.baseMapResolution;

        }

        private void SetHeight(Terrain t)
        {
            if (t != null && t.terrainData != null)
            {
                //only re-do the heights array if terrain resolution changed since last time
                if (m_heights == null || m_heights.Length != t.terrainData.heightmapResolution)
                {
                    m_normalizedHeight = Mathf.InverseLerp(0, t.terrainData.size.y, m_targetHeight);
                    m_heights = new float[t.terrainData.heightmapResolution, t.terrainData.heightmapResolution];
                    for (int x = 0; x < t.terrainData.heightmapResolution; x++)
                    {
                        for (int y = 0; y < t.terrainData.heightmapResolution; y++)
                        {
                            m_heights[x, y] = m_normalizedHeight;
                        }
                    }
                }
                t.terrainData.SetHeights(0, 0, m_heights);
            }
        }

        private void DrawApplySettings(bool helpEnabled)
        {
            m_prototypeTerrain = (Terrain)m_editorUtils.ObjectField("SourceTerrain", m_prototypeTerrain, typeof(Terrain), true, helpEnabled);
            m_copyHMResolution = m_editorUtils.Toggle("CopyHMRes", m_copyHMResolution, helpEnabled);
            if (m_copyHMResolution)
            {
                EditorGUILayout.HelpBox("Warning: Copying the heightmap resolution value will reset the heights on the target terrain!", MessageType.Warning);
            }
        }

        private void DrawSetHeight(bool helpEnabled)
        {
            m_targetHeight = m_editorUtils.FloatField("TargetHeight", m_targetHeight, helpEnabled);
        }
    }
}