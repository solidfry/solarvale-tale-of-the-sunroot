using UnityEngine;
#if UNITY_EDITOR
using Gaia.Internal;
using PWCommon5;
using UnityEditor;
#endif
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using ProceduralWorlds.WaterSystem;
using UnityEditor.UIElements;
using UnityEngine.UI;

namespace Gaia
{
    /// <summary>
    /// Editor for reource manager
    /// </summary>
    [CustomEditor(typeof(GaiaSessionManager))]
    public class GaiaSessionManagerEditor : PWEditor
    {
        //GUIStyle m_boxStyle;
        //GUIStyle m_wrapStyle;
        //GUIStyle m_descWrapStyle;
        private GUIStyle m_operationCreateWorldStyle;
        private GUIStyle m_operationFlattenTerrainStyle;
        private GUIStyle m_operationClearSpawnsStyle;
        private GUIStyle m_operationStampStyle;
        private GUIStyle m_operationStampUndoRedoStyle;
        private GUIStyle m_operationSpawnStyle;
        private GUIStyle m_operationRemoveNonBiomeResourcesStyle;
        private GUIStyle m_operationMaskMapExportStyle;
        private GUIStyle m_operationCheckboxStyle;
        private GUIStyle m_operationFoldOutStyle;
        private List<Texture2D> m_tempTextureList = new List<Texture2D>();
        private Vector2 m_scrollPosition = Vector2.zero;
        GaiaSessionManager m_manager;
        private EditorUtils m_editorUtils;


        //private int m_lastSessionID = -1;
        //private string m_lastPreviewImgName = "";
        //private bool m_showTooltips = true;

        private void Awake()
        {
#if GAIA_PRO_PRESENT
            WorldOriginEditor.m_sessionManagerExits = true;
#else
           Gaia2TopPanel.m_sessionManagerExits = true;
#endif
        }

        void OnEnable()
        {
            m_manager = (GaiaSessionManager)target;

            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            

            GaiaLighting.SetPostProcessingStatus(false);
        }

        private void OnDisable()
        {
            GaiaLighting.SetPostProcessingStatus(true);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < m_tempTextureList.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(m_tempTextureList[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            m_manager = (GaiaSessionManager)target;
            serializedObject.Update();

            SetupOperationHeaderColor(ref m_operationCreateWorldStyle, "3FC1C9ff", "297e83ff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationFlattenTerrainStyle, "C46564ff", "804241ff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationClearSpawnsStyle, "F0E999ff", "9d9864ff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationStampStyle, "B8C99Dff", "788367ff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationStampUndoRedoStyle, "d1a6a3ff", "896c6bff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationSpawnStyle, "EEB15Bff", "9c743bff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationRemoveNonBiomeResourcesStyle, "ba7fcdff", "7a5386ff", m_tempTextureList);
            SetupOperationHeaderColor(ref m_operationMaskMapExportStyle, "9e955bff", "635D39ff", m_tempTextureList);

           if (m_operationCheckboxStyle == null)
           {
                m_operationCheckboxStyle = new GUIStyle(GUI.skin.toggle);
                m_operationCheckboxStyle.fixedWidth = 15;
                m_operationCheckboxStyle.margin = new RectOffset(5,0,0,0);
                m_operationCheckboxStyle.padding = new RectOffset(0, 0, 0, 5);
           }

           if (m_operationFoldOutStyle == null)
           { 
            m_operationFoldOutStyle = new GUIStyle(EditorStyles.foldout);
                m_operationFoldOutStyle.margin = new RectOffset(0, 0, 0, 0);
           }

            m_editorUtils.Panel("Summary", DrawSummary, true);
            m_editorUtils.Panel("HeightmapBackups", DrawHeightmapBackups, true);
            m_editorUtils.Panel("Operations", DrawOperations, true);


        }

        public static void SetupOperationHeaderColor(ref GUIStyle style, string normalColor, string proColor, List<Texture2D> tempTextureList)
        {
            if (style == null || style.normal.background == null)
            {
                style = new GUIStyle();
                style.stretchWidth = true;
                style.margin = new RectOffset(5, 5, 0, 0);
                //m_operationHeaderStyle.overflow = new RectOffset(2, 2, 2, 2);

                // Setup colors for Unity Pro
                if (EditorGUIUtility.isProSkin)
                {
                    style.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML(proColor), tempTextureList);
                }
                // or Unity Personal
                else
                {
                    style.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML(normalColor), tempTextureList);
                }
            }
        }

        private void DrawSummary(bool helpEnabled)
        {
            m_manager.m_session = (GaiaSession)m_editorUtils.ObjectField("SessionData", m_manager.m_session, typeof(GaiaSession), helpEnabled);
            m_editorUtils.InlineHelp("SessionData", helpEnabled);
            if (m_manager.m_session == null)
            {
                if (m_editorUtils.Button("CreateSessionButton"))
                {
                    m_manager.CreateSession();
                }
            }
            if (m_manager.m_session == null)
            {
                return;
            }
            string oldSessionName = m_manager.m_session.m_name;
            EditorGUI.BeginChangeCheck();
            m_manager.m_session.m_name = m_editorUtils.DelayedTextField("Name", m_manager.m_session.m_name, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (!GaiaUtils.HasDynamicLoadedTerrains() || EditorUtility.DisplayDialog("Allow Terrain unloading for name change?", "Changing the session name while using terrain loading in a scene requires all terrains to be unloaded, to then load them back in again under their new path. Do you allow Gaia to unload all terrains automatically for the renaming process?", "Continue", "Abort"))
                {
                    EditorUtility.SetDirty(m_manager.m_session);
                    AssetDatabase.SaveAssets();
                    //Get the old path
                    string oldSessionDataPath = GaiaDirectories.GetSessionSubFolderPath(m_manager.m_session);
                    //Rename the session asset
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(m_manager.m_session), m_manager.m_session.m_name + ".asset");
                    //rename the session data path as well
                    string newSessionDataPath = GaiaDirectories.GetSessionSubFolderPath(m_manager.m_session, false);
                    AssetDatabase.MoveAsset(oldSessionDataPath, newSessionDataPath);
                    //if we have terrain scenes stored in the Terrain Loader, we need to update the paths in there as well
                    TerrainLoaderManager.Instance.UnloadAll(true);
                    foreach (TerrainScene terrainScene in TerrainLoaderManager.TerrainScenes)
                    {
                        terrainScene.m_scenePath = newSessionDataPath + GaiaDirectories.TERRAIN_SCENES_DIRECTORY + terrainScene.m_scenePath.Substring(terrainScene.m_scenePath.LastIndexOf("/"));
                        if (!string.IsNullOrEmpty(terrainScene.m_impostorScenePath))
                            terrainScene.m_impostorScenePath = newSessionDataPath + GaiaDirectories.IMPOSTOR_SCENES_DIRECTORY + terrainScene.m_impostorScenePath.Substring(terrainScene.m_impostorScenePath.LastIndexOf("/"));
                        if (!string.IsNullOrEmpty(terrainScene.m_backupScenePath))
                            terrainScene.m_backupScenePath = newSessionDataPath + GaiaDirectories.BACKUP_SCENES_DIRECTORY + terrainScene.m_backupScenePath.Substring(terrainScene.m_backupScenePath.LastIndexOf("/"));
                        if (!string.IsNullOrEmpty(terrainScene.m_colliderScenePath))
                            terrainScene.m_colliderScenePath = newSessionDataPath + GaiaDirectories.COLLIDER_SCENES_DIRECTORY + terrainScene.m_colliderScenePath.Substring(terrainScene.m_colliderScenePath.LastIndexOf("/"));
                    }
                    TerrainLoaderManager.Instance.SaveStorageData();
                    AssetDatabase.DeleteAsset(oldSessionDataPath);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    m_manager.m_session.m_name = oldSessionName;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_editorUtils.GetContent("Description"), GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            m_manager.m_session.m_description = EditorGUILayout.TextArea(m_manager.m_session.m_description, GUILayout.MinHeight(100));
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("Description", helpEnabled);
            m_manager.m_session.m_previewImage = (Texture2D)m_editorUtils.ObjectField("PreviewImage", m_manager.m_session.m_previewImage, typeof(Texture2D), helpEnabled);
            GUILayout.BeginHorizontal();
            Rect rect = EditorGUILayout.GetControlRect();
            GUILayout.Space(rect.width - 20);
            if (GUILayout.Button("Generate Image"))
            {
                string textureFileName = GaiaDirectories.GetSessionSubFolderPath(m_manager.m_session) + Path.DirectorySeparatorChar + m_manager.m_session + "_Preview";
                var originalLODBias = QualitySettings.lodBias;
                QualitySettings.lodBias = 100;

#if HDPipeline
                //Switch off all active lights in the scene as they would interfere with the baking for this mode
                OrthographicBake.LightsOff();
                OrthographicBake.m_HDLODBiasOverride = 100;
                OrthographicBake.CreateBakeDirectionalLight(3, Color.white);
#endif
                OrthographicBake.BakeTerrain(Terrain.activeTerrain, 2048, 2048, Camera.main.cullingMask, textureFileName);
                //OrthographicBake.RemoveOrthoCam();
#if HDPipeline
                //Restore original lighting
                OrthographicBake.LightsOn();
                OrthographicBake.RemoveBakeDirectionalLight();
#endif
                QualitySettings.lodBias = originalLODBias;
                textureFileName += ".png";
                AssetDatabase.ImportAsset(textureFileName);
                var importer = AssetImporter.GetAtPath(textureFileName) as TextureImporter;
                if (importer != null)
                {
                    importer.sRGBTexture = true;
                    importer.alphaIsTransparency = false;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.mipmapEnabled = false;
                }
                AssetDatabase.ImportAsset(textureFileName);
                m_manager.m_session.m_previewImage = (Texture2D)AssetDatabase.LoadAssetAtPath(textureFileName, typeof(Texture2D));
            }
            GUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("PreviewImage", helpEnabled);
            m_editorUtils.LabelField("Created", new GUIContent(m_manager.m_session.m_dateCreated), helpEnabled);
            m_manager.m_session.m_isLocked = m_editorUtils.Toggle("Locked", m_manager.m_session.m_isLocked, helpEnabled);
            float maxSeaLevel = 2000f;
            if (Terrain.activeTerrain != null)
            {
                maxSeaLevel = Terrain.activeTerrain.terrainData.size.y + Terrain.activeTerrain.transform.position.y;
            }
            else
            {
                maxSeaLevel = m_manager.GetSeaLevel() + 500f;
            }

            float oldSeaLevel = m_manager.GetSeaLevel();
            float newSeaLEvel = oldSeaLevel;
            newSeaLEvel = m_editorUtils.Slider("SeaLevel", newSeaLEvel, 0, maxSeaLevel, helpEnabled);
            if (newSeaLEvel != oldSeaLevel)
            {
                //Do we have a water instance? If yes, update it & it will update the sea level in the session as well
                if (PWS_WaterSystem.Instance != null)
                {
                    PWS_WaterSystem.Instance.SeaLevel = newSeaLEvel;
                }
                else
                {
                    //no water instance yet, just update the sea level in the session
                    m_manager.SetSeaLevel(newSeaLEvel,false);
                    SceneView.RepaintAll();
                }
            }

            m_manager.m_session.m_spawnDensity = m_editorUtils.FloatField("SpawnDensity", Mathf.Max(0.01f, m_manager.m_session.m_spawnDensity), helpEnabled);
            GUILayout.BeginHorizontal();
            if (m_editorUtils.Button("DeleteAllOperations"))
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("PopupDeleteAllTitle"), m_editorUtils.GetTextValue("PopupDeleteAllMessage"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    foreach (GaiaOperation op in m_manager.m_session.m_operations)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(op.scriptableObjectAssetGUID))
                            {
                                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(op.scriptableObjectAssetGUID));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Error while deleting one of the operation data files: " + ex.Message + " Stack Trace:" + ex.StackTrace);
                        }
                    }

                    m_manager.m_session.m_operations.Clear();
                }
            }

            if (m_editorUtils.Button("PlaySession"))
            {
                GaiaLighting.SetDefaultAmbientLight(GaiaUtils.GetGaiaSettings().m_gaiaLightingProfile);
                GaiaSessionManager.PlaySession();
            }
            GUILayout.EndHorizontal();
        }


        private void DrawHeightmapBackups(bool helpEnabled)
        {
            if (m_manager == null || m_manager.m_session == null)
            {
                GUILayout.Label("No Session assigned yet.");
                return;
            }
            string path = GaiaDirectories.GetBackupHeightmapsPath(false, m_manager.m_session);
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
                {
                    string completePath = path + "/" + subDirectoryInfo.Name;
                    GUILayout.BeginHorizontal();
                     GUILayout.Label(subDirectoryInfo.Name);
                    GUILayout.FlexibleSpace();
                    float buttonwidth = 60;

                    if (m_editorUtils.Button("Restore", GUILayout.Width(buttonwidth)))
                    {
                        if (EditorUtility.DisplayDialog("Restore Backup?", "Do you really want to restore this heightmap backup to your current terrains? This will restore the heights that were saved into the backup files at the point when the backup was taken. PLEASE NOTE: This can hugely impact your current terrains and has the potential to undo a lot of your work on the scene, please make sure you are restoring the correct backup. If there are any doubts, please make a backup copy of your entire project before proceeding.", "Restore Backup", "Cancel"))
                        {
                            m_manager.RestoreBackup(completePath);
                        }
                    }
                    if (m_editorUtils.Button("Overwrite", GUILayout.Width(buttonwidth)))
                    {
                        if (subDirectoryInfo.Name == GaiaDirectories.BACKUP_SPECIALSTAMPER_DIRECTORY.Substring(1))
                        {
                            if (EditorUtility.DisplayDialog("Overwrite Stamper Restore Point?", "WARNING: You are trying to overwrite the 'official' heightmap backup created by the Gaia Stamper tool. This is the backup used to undo Terrain Modifier Stamps that were applied during spawning. \r\n\r\n Overwriting this backup means that the current state of the terrains will become the new 'restore point' to undo Terrain Modifier Stamps instead.  If you are not 100% sure that this is what you want, you should NOT overwrite this backup at all.", "Overwrite Stamper Backup", "Cancel"))
                            {
                                m_manager.CreateBackup(completePath);
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Overwrite Backup?", "Do you want to overwrite this backup with the current terrain heightmap data? This will save the current heights of the terrains over the existing backup files. The old backup will be lost and will be updated with the current state of the terrains instead.", "Overwrite Backup", "Cancel"))
                            {
                                m_manager.CreateBackup(completePath);
                            }
                        }
                    }
                    if (m_editorUtils.Button("DeleteBackup", GUILayout.Width(buttonwidth)))
                    {
                        if (subDirectoryInfo.Name == GaiaDirectories.BACKUP_SPECIALSTAMPER_DIRECTORY.Substring(1))
                        {
                            if (EditorUtility.DisplayDialog("Delete Stamper Backup?", "WARNING: You are trying to delete the 'official' Stamper Backup made by the Gaia Stamper tool. \r\n\r\n This is the backup used to undo Terrain Modifier Stamps that were applied during spawning. If you delete this backup and spawn another Terrain Modifier Stamp again, it will take the current heightmaps of the terrains in your scene as a new restore / undo point instead. Please do not proceed unless you are 100% sure you want to do this.", "Delete Stamper Backup", "Cancel"))
                            {
                                FileUtil.DeleteFileOrDirectory(completePath);
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Delete Backup?", "Do you want to delete this backup completely? You will not be able to restore from this backup point anymore.", "Delete Backup", "Cancel"))
                            {
                                FileUtil.DeleteFileOrDirectory(completePath);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }


            if(m_editorUtils.Button("AddNewBackup"))
            {
                if (EditorUtility.DisplayDialog("Add new heightmap backup?", "Do you want to create a new backup of the heightmap data of all terrains? This will save the current heights of the terrains into a files, so you can restore the heightmaps at a later point again.", "Create Backup", "Cancel"))
                {
                    //Get the path again, but this time with creation flag in case it does not exist yet
                    path = GaiaDirectories.GetBackupHeightmapsPath(true, m_manager.m_session);
                    path += "/" + GaiaSessionManager.GetNewHeightmapBackupFolderName();
                    m_manager.CreateBackup(path, true);
                }
            }
        }


        private void DrawOperations(bool helpEnabled)
        {
            if (m_manager.m_session == null)
            {
                GUILayout.Label("No Session assigned yet.");
                return;
            }
            if (m_manager.m_session.m_operations.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                bool oldSelectAll = m_manager.m_selectAllOperations;
                m_manager.m_selectAllOperations = m_editorUtils.Toggle(m_manager.m_selectAllOperations, m_editorUtils.GetContent("SelectAllToolTip"));

                if (m_manager.m_selectAllOperations != oldSelectAll)
                {
                    foreach (GaiaOperation op in m_manager.m_session.m_operations)
                    {
                        op.m_isActive = m_manager.m_selectAllOperations;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.FlexibleSpace();
                m_editorUtils.Text("NoOperationsYet");
                GUILayout.FlexibleSpace();
                GUILayout.Space(50);
                GUILayout.EndHorizontal();
            }
            //Extra indent needed to draw the foldouts in the correct position
            EditorGUI.indentLevel++;
            bool currentGUIState = GUI.enabled;
            
            for (int i=0;i<m_manager.m_session.m_operations.Count;i++)
            {
                GaiaOperation op = m_manager.m_session.m_operations[i];
                GUIStyle headerStyle = m_operationCreateWorldStyle;

                switch (op.m_operationType)
                {
                    case GaiaOperation.OperationType.CreateWorld:
                        headerStyle = m_operationCreateWorldStyle;
                        break;
                    case GaiaOperation.OperationType.ClearSpawns:
                        headerStyle = m_operationClearSpawnsStyle;
                        break;
                    case GaiaOperation.OperationType.FlattenTerrain:
                        headerStyle = m_operationFlattenTerrainStyle;
                        break;
                    case GaiaOperation.OperationType.RemoveNonBiomeResources:
                        headerStyle = m_operationRemoveNonBiomeResourcesStyle;
                        break;
                    case GaiaOperation.OperationType.Spawn:
                        headerStyle = m_operationSpawnStyle;
                        break;
                    case GaiaOperation.OperationType.Stamp:
                        headerStyle = m_operationStampStyle;
                        break;
                    case GaiaOperation.OperationType.StampUndo:
                        headerStyle = m_operationStampUndoRedoStyle;
                        break;
                    case GaiaOperation.OperationType.StampRedo:
                        headerStyle = m_operationStampUndoRedoStyle;
                        break;
                    case GaiaOperation.OperationType.MaskMapExport:
                        headerStyle = m_operationMaskMapExportStyle;
                        break;
                }
                GUI.enabled = op.m_isActive; 
                GUILayout.BeginHorizontal(headerStyle);
                GUI.enabled = currentGUIState;
                op.m_isActive = GUILayout.Toggle(op.m_isActive, "", m_operationCheckboxStyle);
                GUI.enabled = op.m_isActive; 
                op.m_isFoldedOut = m_editorUtils.Foldout(op.m_isFoldedOut, new GUIContent((i+1).ToString() + " " + op.m_description.ToString()), true, m_operationFoldOutStyle);
                GUILayout.EndHorizontal();
                GUI.enabled = currentGUIState;

                if (op.m_isFoldedOut)
                {
                    DrawOperationFields(op, m_editorUtils, m_manager, helpEnabled, i);
                }
                GUILayout.Space(2);
            }
            EditorGUI.indentLevel--;
        }


        /// <summary>
        /// Draws the data fields for each operation 
        /// </summary>
        /// <param name="op"></param>
        public static void DrawOperationFields(GaiaOperation op, EditorUtils editorUtils, GaiaSessionManager sessionManager, bool helpEnabled, int currentIndex)
        {
            //shared default fields first
            //op.m_isActive = m_editorUtils.Toggle("Active", op.m_isActive, helpEnabled);
            bool currentGUIState = GUI.enabled;
            GUI.enabled = op.m_isActive;
            op.m_description = editorUtils.TextField("Description", op.m_description, helpEnabled);
            editorUtils.LabelField("DateTime", new GUIContent(op.m_operationDateTime), helpEnabled);
            EditorGUI.indentLevel++;
            op.m_terrainsFoldedOut = editorUtils.Foldout(op.m_terrainsFoldedOut, "AffectedTerrains", helpEnabled);

            if (op.m_terrainsFoldedOut)
            {
                foreach (string name in op.m_affectedTerrainNames)
                {
                    EditorGUILayout.LabelField(name);
                }
            }
            EditorGUI.indentLevel--;

            //type specific fields, switch by op type to draw additional fields suitable for the op type

            switch (op.m_operationType)
            {
                case GaiaOperation.OperationType.CreateWorld:
                    editorUtils.LabelField("xTiles", new GUIContent(op.WorldCreationSettings.m_xTiles.ToString()), helpEnabled);
                    editorUtils.LabelField("zTiles", new GUIContent(op.WorldCreationSettings.m_zTiles.ToString()), helpEnabled);
                    editorUtils.LabelField("TileSize", new GUIContent(op.WorldCreationSettings.m_tileSize.ToString()), helpEnabled);
                    break;
                case GaiaOperation.OperationType.Spawn:
                    editorUtils.LabelField("NumberOfSpawners", new GUIContent(op.SpawnOperationSettings.m_spawnerSettingsList.Count.ToString()), helpEnabled);
                    float size = (float)Mathd.Max(op.SpawnOperationSettings.m_spawnArea.size.x, op.SpawnOperationSettings.m_spawnArea.size.z);
                    editorUtils.LabelField("SpawnSize", new GUIContent(size.ToString()), helpEnabled);
                    break;
            }
            GUI.enabled = currentGUIState;
            //Button controls
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (editorUtils.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog(editorUtils.GetTextValue("PopupDeleteTitle"), editorUtils.GetTextValue("PopupDeleteText"), editorUtils.GetTextValue("OK"), editorUtils.GetTextValue("Cancel")))
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(op.scriptableObjectAssetGUID))
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(op.scriptableObjectAssetGUID));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error while deleting one of the operation data files: " + ex.Message + " Stack Trace:" + ex.StackTrace);
                    }

                    sessionManager.RemoveOperation(currentIndex);
                    EditorGUIUtility.ExitGUI();
                }
            }
            GUI.enabled = op.m_isActive;
            if (editorUtils.Button("Play"))
            {
                if (EditorUtility.DisplayDialog(editorUtils.GetTextValue("PopupPlayTitle"), editorUtils.GetTextValue("PopupPlayText"), editorUtils.GetTextValue("OK"), editorUtils.GetTextValue("Cancel")))
                {
                    GaiaSessionManager.ExecuteOperation(op);
                    //Destroy all temporary tools used while executing
                    //not if it is a spawn operation since that is asynchronous
                    if (op.m_operationType != GaiaOperation.OperationType.Spawn)
                    {
                        GaiaSessionManager.DestroyTempSessionTools();
                    }
                }
            }
            GUI.enabled = currentGUIState;
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.Space(20);
            if (editorUtils.Button("ViewData"))
            {
                switch (op.m_operationType)
                {
                    case GaiaOperation.OperationType.CreateWorld:
                        Selection.activeObject = op.WorldCreationSettings;
                        break;
                    case GaiaOperation.OperationType.Stamp:
                        Selection.activeObject = op.StamperSettings;
                        break;
                    case GaiaOperation.OperationType.Spawn:
                        Selection.activeObject = op.SpawnOperationSettings;
                        break;
                    case GaiaOperation.OperationType.FlattenTerrain:
                        Selection.activeObject = op.FlattenOperationSettings;
                        break;
                    case GaiaOperation.OperationType.StampUndo:
                        Selection.activeObject = op.UndoRedoOperationSettings;
                        break;
                    case GaiaOperation.OperationType.StampRedo:
                        Selection.activeObject = op.UndoRedoOperationSettings;
                        break;
                    case GaiaOperation.OperationType.ClearSpawns:
                        Selection.activeObject = op.ClearOperationSettings;
                        break;
                    case GaiaOperation.OperationType.RemoveNonBiomeResources:
                        Selection.activeObject = op.RemoveNonBiomeResourcesSettings;
                        break;
                    case GaiaOperation.OperationType.MaskMapExport:
                        Selection.activeObject = op.ExportMaskMapOperationSettings;
                        break;
                }

                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            switch (op.m_operationType)
            {
                case GaiaOperation.OperationType.Stamp:
                    if (editorUtils.Button("PreviewInStamper"))
                    {
                        Stamper stamper = GaiaSessionManager.GetOrCreateSessionStamper();
                        stamper.LoadSettings(op.StamperSettings);
#if GAIA_PRO_PRESENT
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            //We got placeholders, activate terrain loading
                            stamper.m_loadTerrainMode = LoadMode.EditorSelected;
                        }
#endif
                        Selection.activeObject = stamper.gameObject;
                    }

                    break;
                case GaiaOperation.OperationType.Spawn:
                    if (editorUtils.Button("PreviewInSpawner"))
                    {
                        BiomeController bmc = null;
                        List<Spawner> spawnerList = null;
                        Selection.activeObject = GaiaSessionManager.GetOrCreateSessionSpawners(op.SpawnOperationSettings, ref bmc, ref spawnerList);
                    }

                    break;
                case GaiaOperation.OperationType.MaskMapExport:
#if GAIA_PRO_PRESENT
                    if (editorUtils.Button("PreviewInExport"))
                    {
                        MaskMapExport mme = null;
                        Selection.activeObject = GaiaSessionManager.GetOrCreateMaskMapExporter(op.ExportMaskMapOperationSettings.m_maskMapExportSettings, ref mme);
                    }
#endif
                    break;
            }
           
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a progress bar
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        void ProgressBar(string label, float value)
        {
            // Get a rect for the progress bar using the same margins as a textfield:
            Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(rect, value, label);
            EditorGUILayout.Space();
        }

#region OLD LABEL CODE

        ///// <summary>
        ///// Get a content label - look the tooltip up if possible
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //GUIContent GetLabel(string name)
        //{
        //    string tooltip = "";
        //    if (m_showTooltips && m_tooltips.TryGetValue(name, out tooltip))
        //    {
        //        return new GUIContent(name, tooltip);
        //    }
        //    else
        //    {
        //        return new GUIContent(name);
        //    }
        //}

        ///// <summary>
        ///// The tooltips
        ///// </summary>
        //static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        //{
        //    { "Sea Level", "The sea level the session will be rendered at. Changing this will also change the resource files when it is played." },
        //    { "Locked", "When activated then this stamp is locked and no further changes can be made." },
        //    { "Delete", "Delete the step." },
        //    { "Apply", "Apply the step to the relevant object, but don't execute it. Great for seeing how something was configured." },
        //    { "Play", "Apply the step and play it in the scene." },

        //    { "Flatten Terrain", "Flatten all terrains." },
        //    { "Smooth Terrain", "Smooth all terrains." },
        //    { "Clear Trees", "Clear trees on all terrains and reset all tree spawners." },
        //    { "Clear Details", "Clear details on all terrains." },

        //    { "Terrain Helper", "Show the terrain helper controls." },
        //    { "Focus Scene View", "Focus the scene view on the terrain during session Playback." },
        //    { "Play Session", "Play the session from end to end." },
        //    { "Export Resources", "Export the embedded session resources to the Assest\\Gaia Sessions\\SessionName directory." },
        //    { "Session", "The way this spawner runs. Design time : At design time only. Runtime Interval : At run time on a timed interval. Runtime Triggered Interval : At run time on a timed interval, and only when the tagged game object is closer than the trigger range from the center of the spawner." },
        //};
#endregion

    }
}