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
    [System.Serializable]
    public enum GaiaResourceHelperOperation { CopyResources, RemoveResources, TreesToGameObjects }

    /// <summary>
    /// Utility to copy or remove resources / prototypes from terrains in the scene
    /// </summary>

    class GaiaResourceHelper : EditorWindow, IPWEditor
    {

        private EditorUtils m_editorUtils;
        bool m_targetAllTerrains = true;
        public Enum m_operation = GaiaResourceHelperOperation.CopyResources;
        Terrain m_sourceTerrain;
        private bool m_layersSelected = true;
        private bool m_terrainTreesSelected = true;
        private bool m_terrainDetailsSelected = true;
        private bool m_removeTreesAfterConversion = false;
        Terrain m_targetTerrain;
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

            m_editorUtils.Panel("ResourceHelper", DrawOperation, true);

         
        }

        private void DrawOperation(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical();
            m_operation = m_editorUtils.EnumPopup("Operation", m_operation, helpEnabled);

            switch (m_operation)
            {
                case GaiaResourceHelperOperation.CopyResources:
                    DrawCopyOperation(helpEnabled);
                    break;
                case GaiaResourceHelperOperation.RemoveResources:
                    DrawRemoveOperation(helpEnabled);
                    break;
                case GaiaResourceHelperOperation.TreesToGameObjects:
                    DrawTreesToGameObjectsOperation(helpEnabled);
                    break;
            }

            EditorGUILayout.EndVertical();

        }

        private void DrawRemoveOperation(bool helpEnabled)
        {
            m_targetAllTerrains = m_editorUtils.Toggle("DeleteAll", m_targetAllTerrains, helpEnabled);
            GUI.enabled = !m_targetAllTerrains;
            m_targetTerrain = (Terrain)m_editorUtils.ObjectField("TargetTerrain", m_targetTerrain, typeof(Terrain), true, helpEnabled);
            GUI.enabled = true;
            m_layersSelected = m_editorUtils.Toggle("TerrainLayers", m_layersSelected, helpEnabled);
            m_terrainTreesSelected = m_editorUtils.Toggle("TerrainTrees", m_terrainTreesSelected, helpEnabled);
            m_terrainDetailsSelected = m_editorUtils.Toggle("TerrainDetails", m_terrainDetailsSelected, helpEnabled);
            Color normalBGColor = GUI.backgroundColor;
            if (m_settings == null)
            {
                m_settings = GaiaUtils.GetGaiaSettings();
            }

            GUI.backgroundColor = m_settings.GetActionButtonColor();
            if (m_editorUtils.Button("StartRemoval"))
            {
                if (EditorUtility.DisplayDialog("Remove resources from target terrain(s)?", "This will remove the selected resources from to the source terrain to the given target terrains. This can heavily impact your scene, so please make a backup if you are not sure about this.", "Continue", "Cancel"))
                {
                    if (m_targetAllTerrains)
                    {
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            GaiaUtils.CallFunctionOnDynamicLoadedTerrains(RemoveResourcesFromTerrain, false);
                        }
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            RemoveResourcesFromTerrain(t);
                        }
                        
                    }
                    else
                    {
                        RemoveResourcesFromTerrain(m_targetTerrain);
                    }
                }
            }
            GUI.backgroundColor = normalBGColor;

        }

        private void RemoveResourcesFromTerrain(Terrain t)
        {
            if (m_layersSelected)
            {
                t.terrainData.terrainLayers = new TerrainLayer[0];
            }
            if (m_terrainTreesSelected)
            {
                t.terrainData.SetTreeInstances(new TreeInstance[0], false);
                t.terrainData.treePrototypes = new TreePrototype[0];
            }
            if (m_terrainDetailsSelected)
            {

                int[,] details = new int[t.terrainData.detailWidth, t.terrainData.detailHeight];

                for (int dtlIdx = 0; dtlIdx < t.terrainData.detailPrototypes.Length; dtlIdx++)
                {
                    t.terrainData.SetDetailLayer(0, 0, dtlIdx, details);
                }
               
                t.terrainData.detailPrototypes = new DetailPrototype[0];

#if FLORA_PRESENT && UNITY_EDITOR
                FloraTerrainTile dtt = t.GetComponent<FloraTerrainTile>();
                if (dtt != null)
                {
                    DestroyImmediate(dtt);
                }
#endif
            }
            t.Flush();
        }

        private void DrawCopyOperation(bool helpEnabled)
        {
            m_sourceTerrain = (Terrain)m_editorUtils.ObjectField("SourceTerrain", m_sourceTerrain, typeof(Terrain), true, helpEnabled);
            m_layersSelected = m_editorUtils.Toggle("TerrainLayers", m_layersSelected, helpEnabled);
            m_terrainTreesSelected = m_editorUtils.Toggle("TerrainTrees", m_terrainTreesSelected, helpEnabled);
            m_terrainDetailsSelected = m_editorUtils.Toggle("TerrainDetails", m_terrainDetailsSelected, helpEnabled);
            m_targetAllTerrains = m_editorUtils.Toggle("CopyAll", m_targetAllTerrains, helpEnabled);
            GUI.enabled = !m_targetAllTerrains;
            m_targetTerrain = (Terrain)m_editorUtils.ObjectField("TargetTerrain", m_targetTerrain, typeof(Terrain), true, helpEnabled);
            GUI.enabled = true;
            Color normalBGColor = GUI.backgroundColor;
            if (m_settings == null)
            {
                m_settings = GaiaUtils.GetGaiaSettings();
            }

            GUI.backgroundColor = m_settings.GetActionButtonColor();
            if (m_editorUtils.Button("StartCopy"))
            {
                if (EditorUtility.DisplayDialog("Copy terrain resources to target?", "This will copy the selected resources from to the source terrain to the given target terrains. This can heavily impact your scene, so please make a backup if you are not sure about this.", "Continue", "Cancel"))
                {
                    if (m_targetAllTerrains)
                    {
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            GaiaUtils.CallFunctionOnDynamicLoadedTerrains(CopyResourcesToTerrain, false);
                        }
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            CopyResourcesToTerrain(t);
                        }
                    }
                    else
                    {
                        CopyResourcesToTerrain(m_targetTerrain);
                    }
                }
            }
            GUI.backgroundColor = normalBGColor;
        }

        private void CopyResourcesToTerrain(Terrain t)
        {
            if (m_layersSelected)
            {
                t.terrainData.terrainLayers = m_sourceTerrain.terrainData.terrainLayers;
            }
            if (m_terrainTreesSelected)
            {
                t.terrainData.treePrototypes = m_sourceTerrain.terrainData.treePrototypes;
            }
            if (m_terrainDetailsSelected)
            {
                t.terrainData.detailPrototypes = m_sourceTerrain.terrainData.detailPrototypes;
            }
            t.Flush();
        }

        private void DrawTreesToGameObjectsOperation(bool helpEnabled)
        {
            m_targetAllTerrains = m_editorUtils.Toggle("ConvertTreesForAllTerrains", m_targetAllTerrains, helpEnabled);
            GUI.enabled = !m_targetAllTerrains;
            m_targetTerrain = (Terrain)m_editorUtils.ObjectField("TargetTerrainConvertTrees", m_targetTerrain, typeof(Terrain), true, helpEnabled);
            GUI.enabled = true;
            m_removeTreesAfterConversion = m_editorUtils.Toggle("RemoveTreesAfterConversion", m_removeTreesAfterConversion, helpEnabled);
            Color normalBGColor = GUI.backgroundColor;
            if (m_settings == null)
            {
                m_settings = GaiaUtils.GetGaiaSettings();
            }

            GUI.backgroundColor = m_settings.GetActionButtonColor();
            if (m_editorUtils.Button("StartTreeConversion"))
            {
                string message = "This will create one independent Game Object for each terrain tree on the selected terrain(s). Your terrain should look the same after this operation, but all terrain trees will then be rendered as independent Game Objects instead.\r\n\r\n";
                if (m_removeTreesAfterConversion)
                {
                    message += "You selected to remove the original trees from the terrain, please note that those will be deleted permanently so that only the Game Object trees remain. If you intend to keep the original terrain trees, please uncheck this option before proceeding.";
                }
                else
                {
                    message += "You selected to keep the original trees from the terrain, please note that each tree will be displayed twice afterwards - once as Game Object and once as Terrain Tree. If you are happy with the conversion results you can then consider removing the terrain trees at some point so that only the Game Object trees remain.";

                }

                if (EditorUtility.DisplayDialog("Convert Trees to Game Objects?", message, "Continue", "Cancel"))
                {
                    if (m_targetAllTerrains)
                    {
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            GaiaUtils.CallFunctionOnDynamicLoadedTerrains(ConvertTreesOnTerrain, true);
                        }
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            ConvertTreesOnTerrain(t);
                        }
                    }
                    else
                    {
                        if (m_targetTerrain == null)
                        {
                            EditorUtility.DisplayDialog("No Terrain Selected!", "You currently have no terrain selected to perform the conversion on. Please select a terrain or activate the 'Convert on all terrains' option.", "OK");
                            return;
                        }
                        else
                        {
                            ConvertTreesOnTerrain(m_targetTerrain);
                        }
                    }

                    if (m_removeTreesAfterConversion)
                    {
                        if (m_targetAllTerrains)
                        {
                            if (GaiaUtils.HasDynamicLoadedTerrains())
                            {
                                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(RemoveTreesFromTerrain, true);
                            }
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                RemoveTreesFromTerrain(t);
                            }
                        }
                        else
                        {
                             RemoveTreesFromTerrain(m_targetTerrain);
                        }
                    }


                }
            }
        }

        private void ConvertTreesOnTerrain(Terrain t)
        {
            Transform treeTransform = GaiaUtils.ConvertTreesToGameObjects(t, t.transform);
        }

        private void RemoveTreesFromTerrain(Terrain t)
        {
            t.terrainData.SetTreeInstances(new TreeInstance[0], false);
            t.terrainData.treePrototypes = new TreePrototype[0];
            t.Flush();
        }
    }
}