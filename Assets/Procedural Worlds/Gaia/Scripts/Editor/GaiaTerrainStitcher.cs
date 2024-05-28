using Gaia.Internal;
using PWCommon5;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Gaia
{
    public class TerrainStitchInfo 
    {
        public string m_terrainDataGUID;
        public bool m_north;
        public bool m_south;
        public bool m_west;
        public bool m_east;
    }


    public class GaiaTerrainStitcher : EditorWindow, IPWEditor
    {

        private EditorUtils m_editorUtils;
        bool m_targetAllTerrains = true;
        static bool m_stitchAllDirections = true;
        static bool m_stitchDirectionNorth = true;
        static bool m_stitchDirectionSouth = true;
        static bool m_stitchDirectionWest = true;
        static bool m_stitchDirectionEast = true;

        static List<TerrainStitchInfo> m_stitchedTerrains = new List<TerrainStitchInfo>();

        static Terrain m_targetTerrain;
        public static int m_extraSeamSize = 1;
        static float m_maxDifference = 1.0f;
        private GaiaSettings m_settings;

        public bool PositionChecked { get => true; set => PositionChecked = value; }

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

            m_editorUtils.Panel("TerrainStitcher", DrawTerrainStitcher, true);
        }

        private void DrawTerrainStitcher(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical();
            m_targetAllTerrains = m_editorUtils.Toggle("AllTerrains", m_targetAllTerrains, helpEnabled);
            bool currentGUIState = GUI.enabled;
            if (m_targetAllTerrains)
            {
                GUI.enabled = false;
            }
            m_targetTerrain = (Terrain)m_editorUtils.ObjectField("TargetTerrain", m_targetTerrain, typeof(Terrain), true, helpEnabled);
            GUI.enabled = currentGUIState;
            m_stitchAllDirections = m_editorUtils.Toggle("StitchAllDirections", m_stitchAllDirections, helpEnabled);
            if (!m_stitchAllDirections)
            {
                m_stitchDirectionNorth = m_editorUtils.Toggle("StitchDirectionNorth", m_stitchDirectionNorth, helpEnabled);
                m_stitchDirectionSouth = m_editorUtils.Toggle("StitchDirectionSouth", m_stitchDirectionSouth, helpEnabled);
                m_stitchDirectionWest = m_editorUtils.Toggle("StitchDirectionWest", m_stitchDirectionWest, helpEnabled);
                m_stitchDirectionEast = m_editorUtils.Toggle("StitchDirectionEast", m_stitchDirectionEast, helpEnabled);
            }
            m_maxDifference = m_editorUtils.Slider("MaxDifference", m_maxDifference * 100, 0, 100, helpEnabled) / 100f;
            m_extraSeamSize = m_editorUtils.IntSlider("ExtraSeamSize", m_extraSeamSize, 0, 200, helpEnabled);
            
            if (m_settings == null)
            {
                m_settings = GaiaUtils.GetGaiaSettings();
            }
            GUI.backgroundColor = m_settings.GetActionButtonColor();
            if (m_editorUtils.Button("StartStitching"))
            {
                if (!m_targetAllTerrains && m_targetTerrain == null)
                {
                    EditorUtility.DisplayDialog("No terrain selected", "There is currently no terrain selected to work with. You need to either check 'Stitch all terrains' or select a single target terrain to use with this tool.", "OK");
                    EditorGUIUtility.ExitGUI();
                    return;
                }

                if (EditorUtility.DisplayDialog("Stitch terrain(s)?", "This tool will align heightmaps between the terrains in your scene. This will permanently change the terrain heights in your scene, so please create a backup of your project if there is any risk of losing work.", "Continue", "Cancel"))
                {
                    m_stitchedTerrains.Clear();
                    if (m_targetAllTerrains)
                    {
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            GaiaUtils.CallFunctionOnDynamicLoadedTerrains(StitchTerrain, false);
                        }
                        else
                        {
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                StitchTerrain(t);
                            }
                        }
                    }
                    else
                    {
                        if (m_targetTerrain != null)
                        {
                            StitchTerrain(m_targetTerrain);
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

        }

        public static void StitchTerrain(Terrain terrain)
        {
            TerrainStitchInfo stitchInfo = GetOrCreateStitchInfo(terrain);
            Terrain neighborTerrain = null;
            TerrainStitchInfo neighborStitchInfo = null;

            //Before we begin, we need to load in the potential neighbor scenes for stitching
            TerrainScene neighborSceneNorth = null;
            TerrainScene neighborSceneSouth = null;
            TerrainScene neighborSceneEast = null;
            TerrainScene neighborSceneWest = null;
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                TerrainScene ts = TerrainLoaderManager.Instance.GetTerrainSceneAtPosition(terrain.transform.position + terrain.terrainData.size * 0.5f);
                neighborSceneNorth = TerrainLoaderManager.Instance.TryGetNeighbor(ts, StitchDirection.North);
                neighborSceneSouth = TerrainLoaderManager.Instance.TryGetNeighbor(ts, StitchDirection.South);
                neighborSceneEast = TerrainLoaderManager.Instance.TryGetNeighbor(ts, StitchDirection.East);
                neighborSceneWest = TerrainLoaderManager.Instance.TryGetNeighbor(ts, StitchDirection.West);
            }


            if ((m_stitchAllDirections || m_stitchDirectionNorth) && stitchInfo.m_north ==false)
            {
                if (neighborSceneNorth != null)
                {
                    if (neighborSceneNorth.m_regularLoadState != LoadState.Loaded)
                    {
                        neighborSceneNorth.AddRegularReference(TerrainLoaderManager.Instance.gameObject);
                    }
                }
                neighborTerrain = TerrainHelper.GetTerrainNeighbor(terrain, StitchDirection.North);
                if (neighborTerrain != null)
                {
                    neighborStitchInfo = GetOrCreateStitchInfo(neighborTerrain);
                    if (neighborStitchInfo.m_south == false)
                    {
                        TerrainHelper.StitchTerrainHeightmaps(terrain, neighborTerrain, m_extraSeamSize, m_maxDifference);
                    }
                    stitchInfo.m_north = true;
                    neighborStitchInfo.m_south = true;        
                }
                if (neighborSceneNorth != null)
                {
                    neighborSceneNorth.RemoveAllReferences();
                }
            }
            if ((m_stitchAllDirections || m_stitchDirectionSouth) && stitchInfo.m_south == false)
            {
                if (neighborSceneSouth != null)
                {
                    if (neighborSceneSouth.m_regularLoadState != LoadState.Loaded)
                    {
                        neighborSceneSouth.AddRegularReference(TerrainLoaderManager.Instance.gameObject);
                    }
                }
                neighborTerrain = TerrainHelper.GetTerrainNeighbor(terrain, StitchDirection.South);
                if (neighborTerrain != null)
                {
                    neighborStitchInfo = GetOrCreateStitchInfo(neighborTerrain);
                    if (neighborStitchInfo.m_north == false)
                    {
                        TerrainHelper.StitchTerrainHeightmaps(terrain, neighborTerrain, m_extraSeamSize, m_maxDifference);
                    }
                    stitchInfo.m_south = true;
                    neighborStitchInfo.m_north = true;
                }
                if (neighborSceneSouth != null)
                {
                    neighborSceneSouth.RemoveAllReferences();
                }
            }
            if ((m_stitchAllDirections || m_stitchDirectionWest) && stitchInfo.m_west == false)
            {
                if (neighborSceneWest != null)
                {
                    if (neighborSceneWest.m_regularLoadState != LoadState.Loaded)
                    {
                        neighborSceneWest.AddRegularReference(TerrainLoaderManager.Instance.gameObject);
                    }
                }
                neighborTerrain = TerrainHelper.GetTerrainNeighbor(terrain, StitchDirection.West);
                if (neighborTerrain != null)
                {
                    neighborStitchInfo = GetOrCreateStitchInfo(neighborTerrain);
                    if (neighborStitchInfo.m_east == false)
                    {
                        TerrainHelper.StitchTerrainHeightmaps(terrain, neighborTerrain, m_extraSeamSize, m_maxDifference);
                    }
                    stitchInfo.m_west = true;
                    neighborStitchInfo.m_east= true;
                }
                if (neighborSceneWest != null)
                {
                    neighborSceneWest.RemoveAllReferences();
                }
            }
            if ((m_stitchAllDirections || m_stitchDirectionEast) && stitchInfo.m_east == false)
            {
                if (neighborSceneEast != null)
                {
                    if (neighborSceneEast.m_regularLoadState != LoadState.Loaded)
                    {
                        neighborSceneEast.AddRegularReference(TerrainLoaderManager.Instance.gameObject);
                    }
                }
                neighborTerrain = TerrainHelper.GetTerrainNeighbor(terrain, StitchDirection.East);
                if (neighborTerrain != null)
                {
                    neighborStitchInfo = GetOrCreateStitchInfo(neighborTerrain);
                    if (neighborStitchInfo.m_west== false)
                    {
                        TerrainHelper.StitchTerrainHeightmaps(terrain, neighborTerrain, m_extraSeamSize, m_maxDifference);
                    }
                    stitchInfo.m_east = true;
                    neighborStitchInfo.m_west = true;
                }
                if (neighborSceneEast != null)
                {
                    neighborSceneEast.RemoveAllReferences();
                }
            }
        }

        private static TerrainStitchInfo GetOrCreateStitchInfo(Terrain terrain)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(terrain.terrainData));
            if (!String.IsNullOrEmpty(guid))
            {
                TerrainStitchInfo stitchInfo = m_stitchedTerrains.Find(x => x.m_terrainDataGUID == guid);

                if (stitchInfo == null)
                {
                    stitchInfo = new TerrainStitchInfo() { m_terrainDataGUID = guid };
                    m_stitchedTerrains.Add(stitchInfo);
                }

                return stitchInfo;
            }
            else
            {
                Debug.LogError($"Error while stitching terrains, could not find terrain data GUID for terrain {terrain.name}");
                return null;
            }
        }
    }
}