using Gaia.Internal;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gaia
{

    public enum TerrainConversionSourceType { Scene, TerrainLoading }
    public class SelectableTerrain {
        public bool m_selected;
        public string m_name;
        public bool m_hasImpostor;
    }

    public class TerrainConverterEditorWindow : EditorWindow, IPWEditor
    {
        private EditorUtils m_editorUtils;

        [SerializeField]
        private ExportTerrainSettings settings;

        [SerializeField]
        public TerrainConversionSourceType m_sourceType;

        private List<ExportTerrainSettings> m_allPresets = new List<ExportTerrainSettings>();

        private GaiaSessionManager m_sessionManager;
        private ExportTerrainLODSettings m_currentLODSettings;
        private int m_currentLODLevel;
        private List<ExportTerrainLODSettings> m_currentLODSettingsList;
        private bool m_showTerrainState = true;
        private bool m_showConversionResultState;
        private IEnumerator m_updateCoroutine;
        private bool m_conversionRunning;
        private bool m_allTerrainsSelected;
        private Vector2 m_mainScrollPosition;
        private Vector2 m_terrainListScrollPosition;
        private DateTime m_timeSinceLastEditorUpdate;

        private List<SelectableTerrain> m_allTerrainsList;
        public List<SelectableTerrain> AllTerrainsList
        {
            get
            {
                if (m_allTerrainsList == null)
                {
                    RefreshTerrainList();
                }
                return m_allTerrainsList;
            }
        }

        
        /// <summary>
        /// The current spawner settings
        /// </summary>
        public ExportTerrainSettings m_settings
        {
            get
            {
                if (settings == null)
                {
                    if (SessionManager != null && SessionManager.m_lastUsedTerrainExportSettings != null)
                    {
                        settings = Instantiate(SessionManager.m_lastUsedTerrainExportSettings);
                        settings.name = settings.name.Replace("(Clone)", "").Trim();
                    }
                    else
                    {
                        settings = ScriptableObject.CreateInstance<ExportTerrainSettings>();
                        settings.name = "Terrain Export Settings " + System.DateTime.Now.ToShortDateString();
                        //SetImpostorPreset();
                    }
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        private GaiaSettings m_gaiaSettings;
        private string m_SaveAndLoadMessage;
        private MessageType m_SaveAndLoadMessageType;
        private GUIStyle m_boldStyle;

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


       

        private GaiaSessionManager SessionManager
        {
            get
            {
                if (m_sessionManager == null)
                {
                    m_sessionManager = GaiaSessionManager.GetSessionManager(false);
                }
                return m_sessionManager;
            }
        }

        public bool PositionChecked { get; set; }

        void OnEnable()
        {
            m_conversionRunning = false;
            RefreshTerrainList();
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            titleContent = m_editorUtils.GetContent("WindowTitle");

            m_allPresets = GaiaUtils.GetOrCreateUserFiles().m_exportTerrainSettings;

            if (string.IsNullOrEmpty(m_settings.m_exportPath))
            {
                m_settings.m_exportPath = GaiaDirectories.GetExportDirectory() + GaiaDirectories.TERRAIN_MESH_EXPORT_DIRECTORY;
            }
        }



        #region Coroutine
        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            EditorApplication.update += EditorUpdate;
        }

        //Stop editor updates
        public void StopEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
        }

        /// <summary>
        /// This is executed only in the editor - using it to simulate co-routine execution and update execution
        /// </summary>
        void EditorUpdate()
        {
            if (m_updateCoroutine == null)
            {
                StopEditorUpdates();
            }
            else
            {
                if ((DateTime.Now - m_timeSinceLastEditorUpdate).TotalMilliseconds > GaiaSettings.m_terrainConverterProcessingThreshold)
                {
                    m_timeSinceLastEditorUpdate = DateTime.Now;
                    m_updateCoroutine.MoveNext();
                }
            }
        }
        #endregion

        #region UI 

        public static void OpenWithPreset(string presetName)
        {
            TerrainConverterEditorWindow exportTerrainWindow = EditorWindow.GetWindow<TerrainConverterEditorWindow>();
            exportTerrainWindow.FindAndSetPreset(presetName);
            exportTerrainWindow.m_settings.m_customSettingsFoldedOut = false;
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                exportTerrainWindow.m_sourceType = TerrainConversionSourceType.TerrainLoading;
            }
            exportTerrainWindow.ToggleAllTerrainsInList(true);
        }

        public void OnGUI()
        {
            m_editorUtils.Initialize();

            if (m_boldStyle == null)
            {
                m_boldStyle = new GUIStyle(GUI.skin.label);
                m_boldStyle.fontStyle = FontStyle.Bold;
            }

            m_mainScrollPosition = GUILayout.BeginScrollView(m_mainScrollPosition);
            m_editorUtils.Panel("Source", SourcePanel, true);
            m_editorUtils.Panel("ConversionSettings", ConversionSettingsPanel, true);
            m_editorUtils.Panel("Debug", DebugPanel, true);
            DrawOperationButtons();
            EditorGUILayout.Space(20);
            GUILayout.EndScrollView();

        }

        private void DebugPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_showTerrainState = m_editorUtils.Toggle(m_showTerrainState, "ShowTerrains", helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_showTerrainState)
                {
                    ExportTerrainUtility.ShowTerrains(ExportTerrainUtility.GetAllTerrains());
                }
                else
                {
                    HideTerrains(ExportTerrainUtility.GetAllTerrains());
                }


            }


            EditorGUI.BeginChangeCheck();
            m_showConversionResultState = m_editorUtils.Toggle(m_showConversionResultState, "ShowConversionResult", helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_showConversionResultState)
                {
                    ShowMeshTerrains(GetAllMeshTerrains());
                }
                else
                {
                    HideMeshTerrains(GetAllMeshTerrains());
                }
            }
        }

        private void DrawOperationButtons()
        {
            
            Color normalBGColor = GUI.backgroundColor;
            float buttonWidth = (EditorGUIUtility.currentViewWidth-10f) / 2f;
            bool currentGUIState = GUI.enabled;


          


            bool exportEnabled = true;

            //Turn off buttons while conversion is running
            if (m_conversionRunning)
            {
                GUI.enabled = false;
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("RestoreBackupScenes"))
            {

                if (GaiaUtils.HasDynamicLoadedTerrains() && TerrainLoaderManager.ColliderOnlyLoadingActive)
                {
                    if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RestoreRegularLoadingTitle"), m_editorUtils.GetTextValue("RestoreRegularLoadingText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                    {
                        // do an unload with the old setting
                        TerrainLoaderManager.Instance.UnloadAll(true);
                        //then change the actual flag in storage
                        TerrainLoaderManager.Instance.TerrainSceneStorage.m_colliderOnlyLoading = false;
                        //now do a refresh under the new setting
                        TerrainLoaderManager.Instance.RefreshSceneViewLoadingRange();
                    }
                    else
                    {
                        EditorGUIUtility.ExitGUI();
                        return;
                    }
                }

                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RestoreBackupTitle"), m_editorUtils.GetTextValue("RestoreBackupText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    ExportTerrainUtility.RestoreBackup();
                }
            }

            //Do not offer button with no terrains selected
            if (AllTerrainsList.FindAll(x => x.m_selected).Count <= 0)
            {
                exportEnabled = false;
            }

            if (!GaiaUtils.HasDynamicLoadedTerrains() && m_settings.m_createImpostorScenes)
            {
                exportEnabled = false;
            }


            GUI.enabled = exportEnabled;
            GUI.backgroundColor = GaiaSettings.GetActionButtonColor();

            if (m_editorUtils.Button("ExportButton"))
            {
                StartConversion();
            }
            GUI.backgroundColor = normalBGColor;
            GUI.enabled = currentGUIState;

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void DrawPresetDropDown(bool helpEnabled)
        {
            GUILayout.BeginHorizontal();
            m_editorUtils.Label("ExportPreset", GUILayout.Width(EditorGUIUtility.labelWidth -3));

            int oldPresetIndex = m_settings.m_presetIndex;
            string[] allPresetNames = m_allPresets.Select(x => x.name).Append("Custom").ToArray();
            int[] allPresetIDs = m_allPresets.Select(x => x.name).Append("Custom").Select((x, i) => i).ToArray();

            //if nothing selected yet, initialize with first entry
            if (m_settings.m_presetIndex == -99)
            {
                m_settings.m_presetIndex = 0;
                m_settings.m_lastUsedPresetName = allPresetNames[0];
            }

            //If the name of the preset has changed, this means the order of presets might have changed or the preset itself was changed
            //treat the stored settings as custom setting then
            if (m_settings.m_presetIndex > allPresetNames.Length - 1 || m_settings.m_lastUsedPresetName != allPresetNames[m_settings.m_presetIndex])
            {
                m_settings.m_presetIndex = allPresetIDs.Length - 1;
                m_settings.m_lastUsedPresetName = allPresetNames[m_settings.m_presetIndex];
            }

            m_settings.m_presetIndex = EditorGUILayout.IntPopup(m_settings.m_presetIndex, allPresetNames, allPresetIDs);


            if (m_editorUtils.Button("SavePreset"))
            {
                string dialogPath = GaiaDirectories.GetUserSettingsDirectory();
                string filename = "Terrain Converter Settings";
                string saveFilePath = EditorUtility.SaveFilePanel("Save settings as..", dialogPath, filename, "asset");

                bool saveConditionsMet = true;

                //Do we have a path to begin with?
                if (saveFilePath == null || saveFilePath == "")
                {
                    //Silently abort in this case, the user has pressed "Abort" in the File Open Dialog
                    saveConditionsMet = false;
                }

                //Look for the Assets Directory
                if (!saveFilePath.Contains("Assets") && saveConditionsMet)
                {
                    m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveNoAssetDirectory").text;
                    m_SaveAndLoadMessageType = MessageType.Error;
                    saveConditionsMet = false;
                }

                if (saveConditionsMet)
                {
                    saveFilePath = GaiaDirectories.GetPathStartingAtAssetsFolder(saveFilePath);

                    AssetDatabase.CreateAsset(m_settings, saveFilePath);
                    AssetDatabase.ImportAsset(saveFilePath);

                    //Check if save was successful
                    ExportTerrainSettings settingsToLoad = (ExportTerrainSettings)AssetDatabase.LoadAssetAtPath(saveFilePath, typeof(ExportTerrainSettings));
                    if (settingsToLoad != null)
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveSuccessful").text;
                        m_SaveAndLoadMessageType = MessageType.Info;
                        EditorGUIUtility.PingObject(settingsToLoad);

                        //Add the saved file to the user file collection so it shows up in the Gaia Manager
                        UserFiles userFiles = GaiaUtils.GetOrCreateUserFiles();
                        if (userFiles.m_autoAddNewFiles)
                        {
                            if (!userFiles.m_exportTerrainSettings.Contains(settingsToLoad))
                            {
                                userFiles.m_exportTerrainSettings.Add(settingsToLoad);
                            }
                            userFiles.PruneNonExisting();
                        }
                        EditorUtility.SetDirty(userFiles);
                        AssetDatabase.SaveAssets();

                        //dissociate the current settings from the file we just saved, otherwise the user will continue editing the file afterwards
                        //We do this by just instantiating the file we just loaded
                        m_settings = Instantiate(settingsToLoad);
                    }
                    else
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveFailed").text;
                        m_SaveAndLoadMessageType = MessageType.Error;
                    }
                }
            }


            if (m_editorUtils.Button("LoadPreset"))
            {
                string path = GaiaDirectories.GetUserSettingsDirectory();
                path = path.Remove(path.LastIndexOf(Path.AltDirectorySeparatorChar));

                string openFilePath = EditorUtility.OpenFilePanel("Load Export settings..", path, "asset");

                bool loadConditionsMet = true;

                //Do we have a path to begin with?
                if (openFilePath == null || openFilePath == "")
                {
                    //Silently abort in this case, the user has pressed "Abort" in the File Open Dialog
                    loadConditionsMet = false;
                }

                //Look for the Assets Directory
                if (!openFilePath.Contains("Assets") && loadConditionsMet)
                {
                    m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadNoAssetDirectory").text;
                    m_SaveAndLoadMessageType = MessageType.Error;
                    loadConditionsMet = false;
                }
                if (loadConditionsMet)
                {
                    openFilePath = GaiaDirectories.GetPathStartingAtAssetsFolder(openFilePath);
                    ExportTerrainSettings settingsToLoad = (ExportTerrainSettings)AssetDatabase.LoadAssetAtPath(openFilePath, typeof(ExportTerrainSettings));

                    if (settingsToLoad != null)
                    {
                        //always switch to Custom setting after loading
                        LoadSettings(settingsToLoad, allPresetIDs.Length - 1);
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadSuccessful").text;
                        m_SaveAndLoadMessageType = MessageType.Info;
                    }
                    else
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("LoadFailed").text;
                        m_SaveAndLoadMessageType = MessageType.Error;
                    }
                }
            }

            GUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("ExportPreset", helpEnabled);
            
            if (!string.IsNullOrEmpty(m_SaveAndLoadMessage))
            {
                EditorGUILayout.HelpBox(m_SaveAndLoadMessage, m_SaveAndLoadMessageType);
            }

            if (oldPresetIndex != m_settings.m_presetIndex)
            {
                if (m_settings.m_presetIndex < allPresetNames.Length - 1)
                {
                    SwitchPresetIndex();
                    m_settings.m_lastUsedPresetName = allPresetNames[m_settings.m_presetIndex];
                }
                else
                {
                    //user switched to custom setting => fold out the settings
                    m_settings.m_customSettingsFoldedOut = true;
                }
            }

            //If we are using a preset, check if the user changed one of the settings so can switch away from the preset to custom mode
            if (m_settings.m_presetIndex < allPresetNames.Length - 1)
            {
                if (!m_settings.CompareTo(m_allPresets[m_settings.m_presetIndex]))
                {
                    m_settings.m_presetIndex = allPresetNames.Length - 1;
                }

            }

        }

        private void ConversionSettingsPanel(bool helpEnabled)
        {
            DrawPresetDropDown(helpEnabled);

            m_settings.m_convertSourceTerrainsAction = (ConversionAction)m_editorUtils.EnumPopup("ConversionAction", m_settings.m_convertSourceTerrainsAction, helpEnabled);

            m_settings.m_sourceTerrainTreatment = (SourceTerrainTreatment)m_editorUtils.EnumPopup("SourceTerrainTreatment", m_settings.m_sourceTerrainTreatment);
            m_editorUtils.InlineHelp("SourceTerrainTreatment", helpEnabled);
            if (m_settings.m_sourceTerrainTreatment == SourceTerrainTreatment.StoreInBackupScenes && !GaiaUtils.HasDynamicLoadedTerrains())
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("BackupScenesTerrainLoadingOnlyWarning"), MessageType.Warning);
            }

            switch (m_settings.m_convertSourceTerrainsAction)
            {
                case ConversionAction.OBJFileExport:
                    DrawOBJConversionSettings(helpEnabled);
                    break;
                case ConversionAction.ColliderOnly:
                    DrawColliderOnlyConversionSettings(helpEnabled);
                    break;
                case ConversionAction.MeshTerrain:
                    DrawMeshTerrainConversionSettings(helpEnabled);
                    break;
            }

        }

        private void DrawMeshTerrainConversionSettings(bool helpEnabled)
        {
            DrawTerrainColliderSettings(helpEnabled);
            m_settings.m_copyGaiaGameObjects = m_editorUtils.Toggle("CopyGaiaGameObjects", m_settings.m_copyGaiaGameObjects, helpEnabled);
            m_settings.m_convertTreesToGameObjects = m_editorUtils.Toggle("ConvertTreesToGameObjects", m_settings.m_convertTreesToGameObjects, helpEnabled);
            m_settings.m_createImpostorScenes = m_editorUtils.Toggle("UseAsImpostor", m_settings.m_createImpostorScenes, helpEnabled);
            if (m_settings.m_createImpostorScenes)
            {
                if (!GaiaUtils.HasDynamicLoadedTerrains())
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ImpostorNoTerrainLoading"), MessageType.Warning);
                }

                if (m_settings.m_sourceTerrainTreatment != SourceTerrainTreatment.Nothing)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ImpostorWrongSourceTerrainTreatment"), MessageType.Warning);
                }


                double regularRange = TerrainLoaderManager.Instance.GetLoadingRange();
                EditorGUI.BeginChangeCheck();
                regularRange = m_editorUtils.DelayedDoubleField("RegularLoadRange", regularRange, helpEnabled);
                m_settings.m_impostorRange = m_editorUtils.DelayedDoubleField("ImpostorLoadRange", m_settings.m_impostorRange, helpEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    TerrainLoaderManager.Instance.SetLoadingRange(regularRange, m_settings.m_impostorRange);
                }
            }
            //m_editorUtils.Heading("LODSettingsSourceTerrainConversion");
            int LODLevel = 0;

            foreach (ExportTerrainLODSettings LODSettings in m_settings.m_exportTerrainLODSettingsSourceTerrains)
            {
                m_currentLODSettings = LODSettings;
                m_currentLODLevel = LODLevel;
                m_currentLODSettingsList = m_settings.m_exportTerrainLODSettingsSourceTerrains;
                m_editorUtils.Panel(new GUIContent("LOD Level " + LODLevel.ToString() + " Settings"), DrawLODLevel, false);
                LODLevel++;
            }
            GUILayout.BeginHorizontal();
            if (m_settings.m_exportTerrainLODSettingsSourceTerrains.Count > 1)
            {
                if (m_editorUtils.Button("RemoveLODLevel"))
                {
                    m_settings.m_exportTerrainLODSettingsSourceTerrains.RemoveAt(m_settings.m_exportTerrainLODSettingsSourceTerrains.Count() - 1);
                }
            }
            if (m_editorUtils.Button("AddLODLevel"))
            {
                ExportTerrainLODSettings newSettings = new ExportTerrainLODSettings();
                newSettings.namePrefix = "LOD" + (m_settings.m_exportTerrainLODSettingsSourceTerrains.Count - 1).ToString() + "_";
                newSettings.m_LODGroupScreenRelativeTransitionHeight = Mathf.Max(0.2f * (3 - m_currentLODLevel), 0f);
                m_settings.m_exportTerrainLODSettingsSourceTerrains.Add(newSettings);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawColliderOnlyConversionSettings(bool helpEnabled)
        {
            DrawTerrainColliderSettings(helpEnabled);

            m_settings.m_colliderExportAddTreeColliders = m_editorUtils.Toggle("AddTreeColliders", m_settings.m_colliderExportAddTreeColliders, helpEnabled);
            if (m_settings.m_colliderExportAddTreeColliders)
            {
                EditorGUI.indentLevel++;
                m_settings.m_colliderTreeReplacement = (Mesh)m_editorUtils.ObjectField("TreeColliderReplacement", m_settings.m_colliderTreeReplacement, typeof(Mesh), false, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_settings.m_colliderExportAddGameObjectColliders = m_editorUtils.Toggle("AddGameObjectColliders", m_settings.m_colliderExportAddGameObjectColliders, helpEnabled);
            m_settings.m_colliderExportBakeCombinedCollisionMesh = m_editorUtils.Toggle("CombineColliderMeshes", m_settings.m_colliderExportBakeCombinedCollisionMesh, helpEnabled);
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                m_settings.m_colliderExportCreateColliderScenes = m_editorUtils.Toggle("CreateColliderScenes", m_settings.m_colliderExportCreateColliderScenes, helpEnabled);
            }
            m_settings.m_colliderExportCreateServerScene = m_editorUtils.Toggle("CreateColliderServerScene", m_settings.m_colliderExportCreateServerScene, helpEnabled);
        }

        private void DrawTerrainColliderSettings(bool helpEnabled)
        {
            m_settings.m_terrainColliderType = (TerrainColliderType)m_editorUtils.EnumPopup("TerrainColliderType", m_settings.m_terrainColliderType, helpEnabled);

            if (m_settings.m_terrainColliderType != TerrainColliderType.None)
            {
                EditorGUI.indentLevel++;
                if (m_settings.m_terrainColliderType == TerrainColliderType.MeshCollider)
                {
                    m_settings.m_colliderExportResolution = (SaveResolution)m_editorUtils.EnumPopup("ColliderExportResolution", m_settings.m_colliderExportResolution, helpEnabled);
#if GAIA_MESH_PRESENT
                    DrawMeshSimplificationSettings(ref m_settings.m_colliderSimplifyQuality, ref m_settings.m_customSimplificationSettingsFoldedOut, m_settings.m_colliderSimplificationOptions, helpEnabled);
#endif
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawOBJConversionSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            m_settings.m_exportPath = m_editorUtils.TextField("ExportDirectory", m_settings.m_exportPath);

            if (m_editorUtils.Button("ExportDirectoryOpen", GUILayout.Width(80)))
            {
                string path = EditorUtility.SaveFolderPanel(m_editorUtils.GetTextValue("ExportDirectoryWindowTitle"), m_settings.m_exportPath, "");
                if (path.Contains(Application.dataPath))
                {
                    m_settings.m_exportPath = GaiaDirectories.GetPathStartingAtAssetsFolder(path);
                }
                else
                {
                    m_settings.m_exportPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            //Path needs to be below the "Assets" directory, otherwise Asset Database functionality will not work
            //need to lock GUI and inform user if they chose a directory outside the "Assets" folder
            if (m_settings.m_exportPath == null || !m_settings.m_exportPath.StartsWith("Assets"))
            {
                ExportTerrainUtility.m_workingExportPath = GaiaDirectories.GetTempExportPath();
                ExportTerrainUtility.m_copyToPath = m_settings.m_exportPath;
                EditorGUILayout.HelpBox(String.Format(m_editorUtils.GetTextValue("ExportDirectoryNotValid"), ExportTerrainUtility.m_workingExportPath), MessageType.Warning);
            }
            else
            {
                ExportTerrainUtility.m_workingExportPath = m_settings.m_exportPath;
                ExportTerrainUtility.m_copyToPath = "";
            }

            m_editorUtils.InlineHelp("ExportDirectory", helpEnabled);
            m_settings.m_saveFormat = (SaveFormat)m_editorUtils.EnumPopup("ExportFormat", m_settings.m_saveFormat, helpEnabled);
            m_settings.m_terrainExportMask = (Texture2D)m_editorUtils.ObjectField("ExportMask", m_settings.m_terrainExportMask, typeof(Texture2D), false, GUILayout.Height(16f));
            if (m_settings.m_terrainExportMask != null)
            {
                EditorGUI.indentLevel++;
                m_settings.m_terrainExportMaskChannel = (GaiaConstants.ImageChannel)m_editorUtils.EnumPopup("ExportMaskChannel", m_settings.m_terrainExportMaskChannel, helpEnabled);
                m_settings.m_terrainExportInvertMask = m_editorUtils.Toggle("ExportMaskInvertChannel", m_settings.m_terrainExportInvertMask, helpEnabled);
                EditorGUI.indentLevel--;
            }

        }

        private void DrawLODLevel(bool helpEnabled)
        {
            m_currentLODSettings.namePrefix = "LOD" + m_currentLODLevel.ToString() + "_";
            //Mesh Resolution only displayed for LOD level 0 - following levels will inherit the mesh from the foregoing LOD
            if (m_currentLODLevel == 0)
            {
                m_currentLODSettings.m_saveResolution = (SaveResolution)m_editorUtils.EnumPopup("Resolution", m_currentLODSettings.m_saveResolution, helpEnabled);
            }
#if GAIA_MESH_PRESENT
            DrawMeshSimplificationSettings(ref m_currentLODSettings.m_simplifyQuality, ref m_currentLODSettings.m_customSimplifySettingsFoldedOut, m_currentLODSettings.m_simplificationOptions, helpEnabled);
#endif
            if (m_currentLODSettingsList.Count > 1)
            {

                Color regularBGColor = GUI.backgroundColor;

                bool LODRangeMismatch = false;

                if (m_currentLODLevel - 1 >= 0)
                {
                    if (m_currentLODSettings.m_LODGroupScreenRelativeTransitionHeight > m_currentLODSettingsList[m_currentLODLevel - 1].m_LODGroupScreenRelativeTransitionHeight)
                    {
                        LODRangeMismatch = true;
                    }
                }


                if (LODRangeMismatch)
                {
                    GUI.backgroundColor = Color.red;
                }

                m_currentLODSettings.m_LODGroupScreenRelativeTransitionHeight = m_editorUtils.Slider("LODGroupRange", m_currentLODSettings.m_LODGroupScreenRelativeTransitionHeight * 100f, 100f, 0f, helpEnabled) / 100f;

                GUI.backgroundColor = regularBGColor;

                if (LODRangeMismatch)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("LODRangeMismatch"), MessageType.Warning);
                }
            }



            GUILayout.BeginHorizontal();
            {
                LODSettingsMode oldMode = m_currentLODSettings.m_LODSettingsMode;
                m_currentLODSettings.m_LODSettingsMode = (LODSettingsMode)m_editorUtils.EnumPopup("LODSettingsMode", m_currentLODSettings.m_LODSettingsMode);

                if (!m_currentLODSettings.m_settingsFoldedOut)
                {
                    if (m_editorUtils.Button("PlusButtonCustomLODSettings", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        m_currentLODSettings.m_settingsFoldedOut = true;
                    }
                }
                else
                {
                    if (m_editorUtils.Button("MinusButtonCustomLODSettings", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        m_currentLODSettings.m_settingsFoldedOut = false;
                    }
                }

                if (oldMode != m_currentLODSettings.m_LODSettingsMode)
                {
                    if (oldMode != LODSettingsMode.Custom && m_currentLODSettings.m_LODSettingsMode == LODSettingsMode.Custom)
                    {
                        m_currentLODSettings.m_settingsFoldedOut = true;
                    }

                    switch (m_currentLODSettings.m_LODSettingsMode)
                    {
                        case LODSettingsMode.Impostor:
                            ExportTerrainSettings.SetLODToImpostorMode(m_currentLODSettings, m_currentLODLevel);
                            break;
                        case LODSettingsMode.LowPoly:
                            ExportTerrainSettings.SetLODToLowPolyMode(m_currentLODSettings, m_currentLODLevel);
                            break;
                        case LODSettingsMode.Custom:
                            break;
                    }

                }
            }

            //If we are using a preset, check if the user changed one of the settings so can switch away from the preset to custom mode
            if (m_currentLODSettings.m_LODSettingsMode != LODSettingsMode.Custom)
            {
                switch (m_currentLODSettings.m_LODSettingsMode)
                {
                    case LODSettingsMode.Impostor:
                        if (
                        m_currentLODSettings.m_normalEdgeMode != NormalEdgeMode.Smooth ||
                        m_currentLODSettings.m_exportTextures != true ||
                        m_currentLODSettings.m_textureExportMethod != TextureExportMethod.OrthographicBake ||
                        m_currentLODSettings.m_bakeLighting != BakeLighting.NeutralLighting ||
                        m_currentLODSettings.m_captureBaseMapTextures != false ||
                        m_currentLODSettings.m_bakeVertexColors != false ||
                        m_currentLODSettings.m_addAlphaChannel != AddAlphaChannel.None ||
                        m_currentLODSettings.m_exportNormalMaps != true ||
                        m_currentLODSettings.m_exportSplatmaps != false ||
                        m_currentLODSettings.m_createMaterials != true ||
                        m_currentLODSettings.m_materialShader != ExportedTerrainShader.Standard)
                        {
                            m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                        }
                        switch (m_currentLODLevel)
                        {
                            case 0:
                                if (m_currentLODSettings.m_textureExportResolution != TextureExportResolution.x2048)
                                    m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                                break;
                            case 1:
                                if (m_currentLODSettings.m_textureExportResolution != TextureExportResolution.x1024)
                                    m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                                break;
                            case 2:
                                if (m_currentLODSettings.m_textureExportResolution != TextureExportResolution.x512)
                                    m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                                break;
                            default:
                                if (m_currentLODSettings.m_textureExportResolution != TextureExportResolution.x256)
                                    m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                                break;
                        }

                        break;
                    case LODSettingsMode.LowPoly:
                        if (
                        m_currentLODSettings.m_normalEdgeMode != NormalEdgeMode.Sharp ||
                        m_currentLODSettings.m_exportTextures != true ||
                        m_currentLODSettings.m_textureExportMethod != TextureExportMethod.BaseMapExport ||
                        m_currentLODSettings.m_bakeVertexColors != true ||
                        m_currentLODSettings.m_VertexColorSmoothing != 3 ||
                        m_currentLODSettings.m_addAlphaChannel != AddAlphaChannel.None ||
                        m_currentLODSettings.m_exportNormalMaps != false ||
                        m_currentLODSettings.m_exportSplatmaps != false ||
                        m_currentLODSettings.m_createMaterials != true ||
                        m_currentLODSettings.m_materialShader != ExportedTerrainShader.VertexColor
                            )
                            m_currentLODSettings.m_LODSettingsMode = LODSettingsMode.Custom;
                        break;
                }

            }
            GUILayout.EndHorizontal();

            m_editorUtils.InlineHelp("LODSettingsMode", helpEnabled);

            if (m_currentLODSettings.m_settingsFoldedOut)
            {

                m_currentLODSettings.m_normalEdgeMode = (NormalEdgeMode)m_editorUtils.EnumPopup("NormalEdgeMode", m_currentLODSettings.m_normalEdgeMode, helpEnabled);

                m_currentLODSettings.m_exportTextures = m_editorUtils.Toggle("ExportTextures", m_currentLODSettings.m_exportTextures, helpEnabled);

                if (m_currentLODSettings.m_exportTextures)
                {
                    EditorGUI.indentLevel++;
                    m_currentLODSettings.m_textureExportMethod = (TextureExportMethod)m_editorUtils.EnumPopup("TextureExportMethod", m_currentLODSettings.m_textureExportMethod, helpEnabled);
                    m_currentLODSettings.m_addAlphaChannel = (AddAlphaChannel)m_editorUtils.EnumPopup("AddAlphaChannel", m_currentLODSettings.m_addAlphaChannel, helpEnabled);
                    if (m_currentLODSettings.m_textureExportMethod == TextureExportMethod.OrthographicBake)
                    {
                        m_currentLODSettings.m_textureExportResolution = (TextureExportResolution)m_editorUtils.EnumPopup("TextureResolution", m_currentLODSettings.m_textureExportResolution, helpEnabled);
                        m_currentLODSettings.m_bakeLayerMask = GaiaEditorUtils.LayerMaskField(m_editorUtils.GetContent("BakeMask"), m_currentLODSettings.m_bakeLayerMask);
                        m_editorUtils.InlineHelp("BakeMask", helpEnabled);
                        m_currentLODSettings.m_bakeLighting = (BakeLighting)m_editorUtils.EnumPopup("BakeLighting", m_currentLODSettings.m_bakeLighting, helpEnabled);
                        m_currentLODSettings.m_captureBaseMapTextures = m_editorUtils.Toggle("CaptureBasemapTextures", m_currentLODSettings.m_captureBaseMapTextures, helpEnabled);
                    }
                    m_currentLODSettings.m_bakeVertexColors = m_editorUtils.Toggle("BakeVertexColors", m_currentLODSettings.m_bakeVertexColors, helpEnabled);
                    if (m_currentLODSettings.m_bakeVertexColors)
                    {
                        EditorGUI.indentLevel++;
                        m_currentLODSettings.m_VertexColorSmoothing = m_editorUtils.IntSlider("VertexColorSmoothing", m_currentLODSettings.m_VertexColorSmoothing, 0, 10, helpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;

                }
                m_currentLODSettings.m_exportNormalMaps = m_editorUtils.Toggle("ExportNormalMaps", m_currentLODSettings.m_exportNormalMaps, helpEnabled);
                m_currentLODSettings.m_exportSplatmaps = m_editorUtils.Toggle("ExportSplatmaps", m_currentLODSettings.m_exportSplatmaps, helpEnabled);
                m_currentLODSettings.m_createMaterials = m_editorUtils.Toggle("CreateMaterials", m_currentLODSettings.m_createMaterials, helpEnabled);
                if (m_currentLODSettings.m_createMaterials)
                {
                    EditorGUI.indentLevel++;
                    m_currentLODSettings.m_materialShader = (ExportedTerrainShader)m_editorUtils.EnumPopup("MaterialShader", m_currentLODSettings.m_materialShader, helpEnabled);
                    if (m_currentLODSettings.m_materialShader == ExportedTerrainShader.VertexColor && !m_currentLODSettings.m_bakeVertexColors)
                    {
                        EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("VertexColorWithoutBaking"), MessageType.Warning);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void SourcePanel(bool helpEnabled)
        {
            bool hasDynamicLoading = GaiaUtils.HasDynamicLoadedTerrains();

            EditorGUI.BeginChangeCheck();
            m_sourceType = (TerrainConversionSourceType)m_editorUtils.EnumPopup("SourceType", m_sourceType, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshTerrainList();
            }

            if (m_sourceType == TerrainConversionSourceType.TerrainLoading && !hasDynamicLoading)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("SourceNoTerrainLoading"), MessageType.Warning);
            }

           
            GUILayout.BeginHorizontal();
            {
                if (AllTerrainsList.Count == AllTerrainsList.FindAll(x => x.m_selected == true).Count)
                {
                    m_allTerrainsSelected = true;
                }
                else
                {
                    m_allTerrainsSelected = false;
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(3);
                m_editorUtils.LeftCheckbox("NoLabelAllTerrainsCheckbox", ref m_allTerrainsSelected, GUILayout.Width(10));
                if (EditorGUI.EndChangeCheck())
                {
                    ToggleAllTerrainsInList(m_allTerrainsSelected);
                }
                m_editorUtils.Label("Terrains", m_boldStyle, GUILayout.Width(250));
                if (m_sourceType == TerrainConversionSourceType.TerrainLoading && hasDynamicLoading)
                {
                    m_editorUtils.Label("HasImpostor", m_boldStyle, GUILayout.Width(100));
                }
            }
            GUILayout.EndHorizontal();

            m_terrainListScrollPosition = GUILayout.BeginScrollView(m_terrainListScrollPosition);

            foreach (SelectableTerrain t in AllTerrainsList)
            {
                GUILayout.BeginHorizontal();
                {
                    m_editorUtils.LeftCheckbox("NoLabel", ref t.m_selected, GUILayout.Width(10));
                    m_editorUtils.Label(new GUIContent(t.m_name, "Name of the terrain"), GUILayout.Width(250));
                    if (m_sourceType == TerrainConversionSourceType.TerrainLoading && hasDynamicLoading)
                    {
                        if (t.m_hasImpostor)
                        {
                            m_editorUtils.Label("HasImpostorTrue", GUILayout.Width(100));
                        }
                        else
                        {
                            m_editorUtils.Label("HasImpostorFalse", GUILayout.Width(100));
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

        }

        public void ToggleAllTerrainsInList(bool selected)
        {
            RefreshTerrainList();
            foreach (SelectableTerrain selectableTerrain in AllTerrainsList)
            {
                selectableTerrain.m_selected = selected;
            }
        }

#if GAIA_MESH_PRESENT
        private void DrawMeshSimplificationSettings(ref float simplifyQuality, ref bool customSettingsFoldedOut, UnityMeshSimplifierGaia.SimplificationOptions simplificationOptions, bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            {
                simplifyQuality = m_editorUtils.Slider("SimplifyQuality", simplifyQuality * 100f, 0, 100) / 100f;
                if (!customSettingsFoldedOut)
                {
                    if (m_editorUtils.Button("PlusButtonCustomSimplifySettings", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        customSettingsFoldedOut = true;
                    }
                }
                else
                {
                    if (m_editorUtils.Button("MinusButtonCustomSimplifySettings", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        customSettingsFoldedOut = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("SimplifyQuality", helpEnabled);
            if (customSettingsFoldedOut)
            {
                bool currentGUIState = GUI.enabled;
                if (simplifyQuality >= 1)
                {
                    GUI.enabled = false;
                }

                EditorGUI.indentLevel++;
                simplificationOptions.PreserveBorderEdges = m_editorUtils.Toggle("SimplifyPreserveBorderEdges", simplificationOptions.PreserveBorderEdges, helpEnabled);
                simplificationOptions.PreserveUVSeamEdges = m_editorUtils.Toggle("SimplifyPreserveUVSeamEdges", simplificationOptions.PreserveUVSeamEdges, helpEnabled);
                simplificationOptions.PreserveUVFoldoverEdges = m_editorUtils.Toggle("SimplifyPreserveUVFoldoverEdges", simplificationOptions.PreserveUVFoldoverEdges, helpEnabled);
                simplificationOptions.PreserveSurfaceCurvature = m_editorUtils.Toggle("SimplifyPreserveSurfaceCurvature", simplificationOptions.PreserveSurfaceCurvature, helpEnabled);
                simplificationOptions.EnableSmartLink = m_editorUtils.Toggle("SimplifyEnableSmartLink", simplificationOptions.EnableSmartLink, helpEnabled);
                if (simplificationOptions.EnableSmartLink)
                {
                    EditorGUI.indentLevel++;
                    simplificationOptions.VertexLinkDistance = m_editorUtils.FloatField("SimplifyVertexLinkDistance", (float)simplificationOptions.VertexLinkDistance, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                simplificationOptions.MaxIterationCount = m_editorUtils.IntField("SimplifyMaxInterationCount", simplificationOptions.MaxIterationCount, helpEnabled);
                simplificationOptions.Agressiveness = m_editorUtils.DoubleField("SimplifyAggressiveness", simplificationOptions.Agressiveness, helpEnabled);
                simplificationOptions.ManualUVComponentCount = m_editorUtils.Toggle("SimplifyManualUVComponentCount", simplificationOptions.ManualUVComponentCount, helpEnabled);
                if (simplificationOptions.ManualUVComponentCount)
                {
                    EditorGUI.indentLevel++;
                    simplificationOptions.UVComponentCount = m_editorUtils.IntSlider("SimplifyManualUVComponentCountValue", 0, 4, simplificationOptions.UVComponentCount, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
                GUI.enabled = currentGUIState;
            }
        }
#endif

        #endregion

        #region TerrainHelpers

        public void RefreshTerrainList()
        {
            var allTerrains = Resources.FindObjectsOfTypeAll<Terrain>().ToList();
            var tempList = new List<SelectableTerrain>();
            if (m_sourceType == TerrainConversionSourceType.Scene)
            {
                foreach (Terrain t in allTerrains)
                {
                    tempList.Add(new SelectableTerrain() { m_selected = false, m_name = t.name });
                }
            }
            else
            {
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    foreach (TerrainScene t in TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes)
                    {
                        tempList.Add(new SelectableTerrain() { m_selected = false, m_name = t.GetTerrainName(), m_hasImpostor = !string.IsNullOrEmpty(t.m_impostorScenePath) });
                    }
                }
            }
            if (m_allTerrainsList == null)
            {
                m_allTerrainsList = new List<SelectableTerrain>();
            }
            //Copy over the previously selected terrains (if any)

            foreach (SelectableTerrain t in m_allTerrainsList)
            {
                SelectableTerrain matchingEntry = tempList.Find(x => x.m_name == t.m_name);
                if (matchingEntry != null)
                {
                    matchingEntry.m_selected = t.m_selected;
                }
            }

            m_allTerrainsList = tempList;

        }

        private void HideTerrains(List<Terrain> terrains)
        {
            foreach (Terrain t in terrains)
            {
                t.gameObject.SetActive(false);
            }
        }

        private void HideMeshTerrains(List<GameObject> meshTerrainGOs)
        {
            foreach (GameObject go in meshTerrainGOs)
            {
                go.SetActive(false);
            }
        }

        private void ShowMeshTerrains(List<GameObject> meshTerrainGOs)
        {
            foreach (GameObject go in meshTerrainGOs)
            {
                go.SetActive(true);
            }
        }

        /// <summary>
        /// Gets all Mesh terrain game objects in the scene
        /// </summary>
        /// <returns>List of all mesh terrain game objects</returns>
        private List<GameObject> GetAllMeshTerrains()
        {
            List<GameObject> returnList = new List<GameObject>();
            string searchstring = GaiaConstants.MeshTerrainName;
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name.StartsWith(searchstring))
                {
                    returnList.Add(go);
                }
            }
            return returnList;
        }
        #endregion

        #region Saving / Loading

        /// <summary>
        /// Looks for a keyword in the existing export presets, then applies the first preset with that keyword that is found.
        /// </summary>
        /// <param name="searchString"></param>
        public void FindAndSetPreset(string searchString)
        {
            //Find the export collider preset in the user settings
            var allPresets = GaiaUtils.GetOrCreateUserFiles().m_exportTerrainSettings;
            int foundPresetIndex = -99;
            for (int i = 0; i < allPresets.Count; i++)
            {
                if (allPresets[i].name.Contains(searchString))
                {
                    foundPresetIndex = i;
                    break;
                }
            }
            if (foundPresetIndex != -99)
            {
                m_settings.m_presetIndex = foundPresetIndex;
                m_settings.m_lastUsedPresetName = allPresets[foundPresetIndex].name;
                SwitchPresetIndex();
            }
        }

        public void SwitchPresetIndex()
        {
            //Remember the old foldout state & export path when switching
            bool oldfoldOutState = m_settings.m_customSettingsFoldedOut;
            string oldPath = m_settings.m_exportPath;
            LoadSettings(m_allPresets[m_settings.m_presetIndex], m_settings.m_presetIndex);
            m_settings.m_customSettingsFoldedOut = oldfoldOutState;
            m_settings.m_exportPath = oldPath;
        }


        private void LoadSettings(ExportTerrainSettings settingsToLoad, int selectedIndex)
        {
            m_settings = Instantiate(settingsToLoad);
            //Remove the "Clone" in the name
            m_settings.name = m_settings.name.Replace("(Clone)", "");
            m_settings.m_lastUsedPresetName = m_settings.name;
            //override the selected Index with what is currently selected - there can be any index stored in the old data which might not be up to date anymore.
            m_settings.m_presetIndex = selectedIndex;
            //If "Backup Scenes" are used in a non terrain loading scenario, fall back to "Deactivate" instead. Backup scenes are only supported for terrain loading!
            if (m_settings.m_sourceTerrainTreatment == SourceTerrainTreatment.StoreInBackupScenes && !GaiaUtils.HasDynamicLoadedTerrains())
            {
                m_settings.m_sourceTerrainTreatment = SourceTerrainTreatment.Deactivate;
            }

        }
        #endregion

        #region Processing Terrains

        private void StartConversion()
        {
            if (string.IsNullOrEmpty(m_settings.m_exportPath))
            {
                m_settings.m_exportPath = GaiaDirectories.GetExportDirectory() + GaiaDirectories.TERRAIN_MESH_EXPORT_DIRECTORY;
            }

            //Ask for permission to switch back to regular loading if the collider only mode is active
            if (GaiaUtils.HasDynamicLoadedTerrains() && TerrainLoaderManager.ColliderOnlyLoadingActive)
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RestoreRegularLoadingTitle"), m_editorUtils.GetTextValue("RestoreRegularLoadingText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                {
                    // do an unload with the old setting
                    TerrainLoaderManager.Instance.UnloadAll(true);
                    //then change the actual flag in storage
                    TerrainLoaderManager.Instance.TerrainSceneStorage.m_colliderOnlyLoading = false;
                    //now do a refresh under the new setting
                    TerrainLoaderManager.Instance.RefreshSceneViewLoadingRange();
                }
                else
                {
                    EditorGUIUtility.ExitGUI();
                    return;
                }
            }

            //Remove results of previous exports
            //Need to make sure to only remove things for terrains that we are targeting!
            List<string> allSelectedTerrainNames = AllTerrainsList.Where(x => x.m_selected == true).Select(x => x.m_name).ToList();
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                //Are we trying to create new impostors? If yes, it should be sufficient to delete the impostor scenes right away, no need to load in any of the scenes.
                if (m_settings.m_createImpostorScenes)
                {
                    TerrainSceneStorage terrainSceneStorage = TerrainLoaderManager.Instance.TerrainSceneStorage;
                    TerrainLoaderManager.Instance.UnloadAllImpostors(true);
                    foreach (TerrainScene terrainScene in terrainSceneStorage.m_terrainScenes.FindAll(x => AllTerrainsList.Exists(z => z.m_name == x.GetTerrainName() && z.m_selected == true)))
                    {
                        AssetDatabase.DeleteAsset(terrainScene.m_impostorScenePath);
                        terrainScene.m_impostorScenePath = "";
                    }
                    TerrainLoaderManager.Instance.SaveStorageData();
                }
                else
                {
                    if (m_settings.m_convertSourceTerrainsAction == ConversionAction.MeshTerrain || m_settings.m_convertSourceTerrainsAction == ConversionAction.ColliderOnly)
                    {
                        if (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Where(x => AllTerrainsList.Exists(z => z.m_name == x.GetTerrainName() && z.m_selected == true) && !String.IsNullOrEmpty(x.m_backupScenePath)).Count() > 0)
                        {
                            if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("RestoreBackupExportTitle"), m_editorUtils.GetTextValue("RestoreBackupExportText"), m_editorUtils.GetTextValue("Continue"), m_editorUtils.GetTextValue("Cancel")))
                            {
                                //If we want to restore a backup of 100s of scenes, we need to do this in a coroutine
                                m_updateCoroutine = RestoreBackup(allSelectedTerrainNames);
                                StartEditorUpdates();
                                //exit here, we will pick up with the actual conversion after the RemoveMeshTerrains coroutine is finished;
                                return;
                            }
                            else
                            {
                                EditorGUIUtility.ExitGUI();
                                return;
                            }
                        }
                        else
                        {
                            //If we want to remove all mesh terrains from 100s of scenes, we need to do this in a coroutine
                            m_updateCoroutine = RemoveMeshTerrains(allSelectedTerrainNames);
                            StartEditorUpdates();
                            //exit here, we will pick up with the actual conversion after the RemoveMeshTerrains coroutine is finished;
                            return;

                            //Action<Terrain> act = (t) => RemoveMeshTerrainFromTerrainScene(t);
                            //GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true, allSelectedTerrainNames, "Removing old Mesh Terrains...");

                            //TerrainLoaderManager.Instance.UnloadAllImpostors(true);
                            //In general we want to remove all impostor references - we are doing a new export which might not utilize impostors
                            //But for collider scene exports it can be valuable to keep the existing impostor scenes, so that the user can switch back and forth
                            //between collider loading & regular with impostors.
                            //if (!(m_settings.m_convertSourceTerrains && m_settings.m_convertSourceTerrainsAction == ConversionAction.ColliderOnly))
                            //{
                            //    foreach (TerrainScene ts in TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes)
                            //    {
                            //        ts.m_impostorScenePath = "";
                            //    }
                            //    TerrainLoaderManager.Instance.SaveStorageData();
                            //}

                        }
                    }
                }
            }
            else
            {
                if (m_settings.m_convertSourceTerrainsAction == ConversionAction.MeshTerrain)
                {
                    List<GameObject> allMeshGameObjects = GetAllMeshTerrains();
                    if (allMeshGameObjects != null && allMeshGameObjects.Count > 0)
                    {
                        for (int i = allMeshGameObjects.Count - 1; i >= 0; i--)
                        {
                            if (allSelectedTerrainNames.Exists(x => allMeshGameObjects[i].name.Contains(x)))
                            {
                                DestroyImmediate(allMeshGameObjects[i]);
                            }
                        }
                    }
                }
            }


            m_updateCoroutine = TerrainsCoroutine(m_settings, m_allTerrainsList.FindAll(x=>x.m_selected == true).Select(x=>x.m_name).ToList());
            StartEditorUpdates();
        }

        private IEnumerator RestoreBackup(List<string> allTerrainNames)
        {
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                List<TerrainScene> allBackupScenes = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Where(x => !String.IsNullOrEmpty(x.m_backupScenePath) && allTerrainNames.Contains(x.GetTerrainName())).ToList();
                List<TerrainScene> allRemainingScenes = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Where(x => String.IsNullOrEmpty(x.m_backupScenePath) && allTerrainNames.Contains(x.GetTerrainName())).ToList();
                TerrainLoaderManager.Instance.UnloadAll(true);
                int maxSceneCount = allBackupScenes.Count + allRemainingScenes.Count;
                int current = 0;
                foreach (TerrainScene ts in allBackupScenes)
                {
                    try
                    {
                        ProgressBar.Show(ProgressBarPriority.TerrainMeshExport, "Restoring Backups", "Restoring Source Terrains...", current, maxSceneCount, true, false);
                        Scene targetScene = EditorSceneManager.OpenScene(ts.m_scenePath, OpenSceneMode.Additive);
                        Scene BackupScene = EditorSceneManager.OpenScene(ts.m_backupScenePath, OpenSceneMode.Additive);
                        GameObject[] rootGOs = targetScene.GetRootGameObjects();
                        for (int i = rootGOs.Count() - 1; i >= 0; i--)
                        {
                            UnityEngine.Object.DestroyImmediate(rootGOs[i]);
                        }
                        foreach (GameObject go in BackupScene.GetRootGameObjects())
                        {
                            EditorSceneManager.MoveGameObjectToScene(go, targetScene);
                        }
                        EditorSceneManager.SaveScene(targetScene);
                        EditorSceneManager.CloseScene(BackupScene, true);
                        EditorSceneManager.CloseScene(targetScene, true);
                        ts.m_backupScenePath = "";
                        ts.m_impostorScenePath = "";
                        current++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error while restoring Terrain Scenes backup: " + ex.Message + ", Stack Trace: " + ex.StackTrace);
                    }
                }
                foreach (TerrainScene ts in allRemainingScenes)
                {
                    try
                    {
                        ProgressBar.Show(ProgressBarPriority.TerrainMeshExport, "Restoring Backups", "Restoring Source Terrains...", current, maxSceneCount, true, false);
                        Scene targetScene = EditorSceneManager.OpenScene(ts.m_scenePath, OpenSceneMode.Additive);
                        GameObject[] rootGOs = targetScene.GetRootGameObjects();
                        for (int i = rootGOs.Count() - 1; i >= 0; i--)
                        {
                            if (rootGOs[i].name == GaiaConstants.SourceTerrainBackupObject)
                            {
                                Transform child = rootGOs[i].transform.GetChild(0);
                                if (child != null)
                                {
                                    child.parent = null;
                                }
                            }
                            //Make sure we do not delete the original terrains
                            //Those could still be in the scene if the user chose to do "nothing" with the source terrains.
                            if (!rootGOs[i].name.StartsWith("Terrain"))
                            {
                                UnityEngine.Object.DestroyImmediate(rootGOs[i]);
                            }
                        }
                        EditorSceneManager.SaveScene(targetScene);
                        EditorSceneManager.CloseScene(targetScene, true);
                        ts.m_backupScenePath = "";
                        ts.m_impostorScenePath = "";
                        current++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error while restoring Terrain Scenes backup: " + ex.Message + ", Stack Trace: " + ex.StackTrace);
                    }
                    yield return null;
                }
                ProgressBar.Clear(ProgressBarPriority.TerrainMeshExport);
                TerrainLoaderManager.Instance.SaveStorageData();
                TerrainLoaderManager.Instance.UnloadAll(true);
                TerrainLoaderManager.Instance.RefreshSceneViewLoadingRange();
            }
            else
            {
                ExportTerrainUtility.ShowTerrains(ExportTerrainUtility.GetAllTerrains());
                GameObject exportObject = GaiaUtils.GetTerrainExportObject();
                if (exportObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(exportObject);
                }
            }
            //all done, continue with the actual conversion coroutine
            m_updateCoroutine = TerrainsCoroutine(m_settings, m_allTerrainsList.FindAll(x => x.m_selected == true).Select(x => x.m_name).ToList());
            yield return null;
        }

        private IEnumerator RemoveMeshTerrains(List<string> terrainNames)
        {
            foreach (string terrainName in terrainNames)
            {
                try
                {
                    m_conversionRunning = true;
                    //Scene originalScene = SessionManager.gameObject.scene;
                    //int numberOfLODs = m_settings.m_exportTerrainLODSettingsSourceTerrains.Count;

                    TerrainScene terrainScene = null;

                    Terrain targetTerrain = null;
                    if (Terrain.activeTerrains.Length > 0)
                    {
                        targetTerrain = Terrain.activeTerrains.Where(x => x.name == terrainName).FirstOrDefault();
                        if (GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            terrainScene = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Find(x => x.GetTerrainName() == terrainName);
                        }
                    }

                    //Terrain not found? Try to load it from Terrain loading
                    if (targetTerrain == null && GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        terrainScene = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Find(x => x.GetTerrainName() == terrainName);
                        if (terrainScene != null)
                        {
                            if (terrainScene.m_regularLoadState != LoadState.Loaded)
                            {
                                terrainScene.AddRegularReference(SessionManager.gameObject);
                            }
                            Scene scene = EditorSceneManager.GetSceneByPath(terrainScene.m_scenePath);
                            foreach (GameObject go in scene.GetRootGameObjects())
                            {
                                //go can be null if just deleted before this call
                                if (go == null)
                                {
                                    continue;
                                }
                                targetTerrain = go.GetComponent<Terrain>();
                                if (targetTerrain != null)
                                {
                                    break;
                                }
                                if (go.name == GaiaConstants.SourceTerrainBackupObject)
                                {
                                    Transform firstChild = go.transform.GetChild(0);
                                    if (firstChild != null)
                                    {
                                        targetTerrain = firstChild.GetComponent<Terrain>();
                                        if (targetTerrain != null)
                                        {
                                            targetTerrain.gameObject.SetActive(true);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (targetTerrain == null)
                    {
                        Debug.LogWarning($"Terrain {targetTerrain} is selected for conversion, but could neither be found in the scene nor loaded via terrain loading. This terrain will be skipped in the conversion.");
                        continue;
                    }

                    RemoveMeshTerrainFromTerrainScene(targetTerrain);

                    if (terrainScene != null)
                    {
                        Scene scene = EditorSceneManager.GetSceneByPath(terrainScene.m_scenePath);
                        if (scene != null)
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                        }
                        terrainScene.RemoveRegularReference(SessionManager.gameObject);
                    }

                    EditorUtility.ClearProgressBar();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error during Mesh Removal from Loaded Scenes: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                }
                finally
                {
                    ProgressBar.Clear(ProgressBarPriority.TerrainMeshExport);
                }
                m_timeSinceLastEditorUpdate = DateTime.Now;
                yield return null;
            }
            //all done, continue with the actual conversion coroutine
            m_updateCoroutine = TerrainsCoroutine(m_settings, m_allTerrainsList.FindAll(x => x.m_selected == true).Select(x => x.m_name).ToList());
        }

        private void RemoveMeshTerrainFromTerrainScene(Terrain terrain)
        {
            //We look for two things to remove: single meshes and Mesh LOD groups (created when the user selects multiple LOD levels for the export)
            string searchString1 = GaiaConstants.MeshTerrainName;
            string searchString2 = GaiaConstants.MeshTerrainLODGroupPrefix;

            GameObject[] rootGOs = terrain.gameObject.scene.GetRootGameObjects();
            for (int i = rootGOs.Count() - 1; i >= 0; i--)
            {
                if (rootGOs[i].name == GaiaConstants.SourceTerrainBackupObject)
                {
                    rootGOs[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                if (rootGOs[i].name.StartsWith(searchString1) || rootGOs[i].name.StartsWith(searchString2))
                {
                    DestroyImmediate(rootGOs[i]);
                }
            }
            if (terrain.transform.parent != null)
            {
                Transform parent = terrain.transform.parent;
                terrain.transform.SetParent(null);
                DestroyImmediate(parent.gameObject);
            }
        }

        private IEnumerator TerrainsCoroutine(ExportTerrainSettings m_settings, List<string> terrainNames)
        {
            double timeStamp = GaiaUtils.GetUnixTimestamp();

            Scene originalScene = SessionManager.gameObject.scene;
            Scene serverScene = SessionManager.gameObject.scene;
            string serverSceneName = SessionManager.gameObject.scene.name + " - Server";
            string serverScenePath = SessionManager.gameObject.scene.path.Replace(SessionManager.gameObject.scene.name + ".unity", serverSceneName + ".unity");
            


            if (m_settings.m_colliderExportCreateServerScene)
            {
                //was a server side scene requested? If yes, we need to collect all the collider results in a separate scene
                serverScene = EditorSceneManager.GetSceneByPath(serverScenePath);
                if (serverScene.path == null)
                {
                    //Server scene does not exist yet, create it
                    serverScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    serverScene.name = serverSceneName;
                    EditorSceneManager.SaveScene(serverScene, serverScenePath);
                }
                else
                {
                    //Server scene does exist already, clear it.
                    serverScene = EditorSceneManager.OpenScene(serverScenePath, OpenSceneMode.Additive);
                    foreach (GameObject go in serverScene.GetRootGameObjects())
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }
                EditorSceneManager.SetActiveScene(originalScene);
            }


            ConvertTerrains.PrepareConversion(m_settings, m_settings.m_exportPath, timeStamp);
            List<Light> deactivatedLights = new List<Light>();

            var originalAmbientMode = RenderSettings.ambientMode;
            var originalAmbientColor = RenderSettings.ambientSkyColor;
            var originalLODBias = QualitySettings.lodBias;
            GameObject weatherObject = GameObject.Find(GaiaConstants.gaiaWeatherObject);
            for (int lodIndex = 0; lodIndex < m_settings.m_exportTerrainLODSettingsSourceTerrains.Count; lodIndex++)
            {
                ExportTerrainLODSettings LODSettings = m_settings.m_exportTerrainLODSettingsSourceTerrains[lodIndex];
                if (LODSettings.m_exportTextures && LODSettings.m_textureExportMethod == TextureExportMethod.OrthographicBake && LODSettings.m_bakeLighting == BakeLighting.NeutralLighting)
                {
                    //Set up neutral ambient lighting
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientSkyColor = Color.white;

                    //Increase LOD Bias to capture all trees etc. on the terrain
                    QualitySettings.lodBias = 100;

                    //Switch off all active lights in the scene as they would interfere with the baking for this mode
                    OrthographicBake.LightsOff();

#if HDPipeline
                    OrthographicBake.m_HDLODBiasOverride = 100;
                    OrthographicBake.CreateBakeDirectionalLight(3, Color.white);
#endif

                    //Do we have a weather object? Deactivate it for the baking
                    if (weatherObject != null)
                    {
                        if (weatherObject.activeInHierarchy)
                        {
                            weatherObject.SetActive(false);
                        }
                        else
                        {
                            weatherObject = null;
                        }
                    }
                }

                foreach (string terrainName in terrainNames)
                {
                    try
                    {
                        m_conversionRunning = true;
                        int numberOfLODs = m_settings.m_exportTerrainLODSettingsSourceTerrains.Count;

                        TerrainScene terrainScene = null;

                        Terrain targetTerrain = null;
                        if (Terrain.activeTerrains.Length > 0)
                        {
                            targetTerrain = Terrain.activeTerrains.Where(x => x.name == terrainName).FirstOrDefault();
                        }

                        //Terrain not found? Try to load it from Terrain loading
                        if (targetTerrain == null && GaiaUtils.HasDynamicLoadedTerrains())
                        {
                            terrainScene = TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Find(x => x.GetTerrainName() == terrainName);
                            if (terrainScene != null)
                            {
                                if (terrainScene.m_regularLoadState != LoadState.Loaded)
                                {
                                    terrainScene.AddRegularReference(SessionManager.gameObject);
                                }
                                Scene scene = EditorSceneManager.GetSceneByPath(terrainScene.m_scenePath);
                                foreach (GameObject go in scene.GetRootGameObjects())
                                {
                                    //go can be null if just deleted before this call
                                    if (go == null)
                                    {
                                        continue;
                                    }
                                    targetTerrain = go.GetComponent<Terrain>();
                                    if (targetTerrain != null)
                                    {
                                        break;
                                    }
                                }
                            }   
                        }
                        if (targetTerrain == null)
                        {
                            Debug.LogWarning($"Terrain {terrainName} is selected for conversion, but could neither be found in the scene nor loaded via terrain loading. This terrain will be skipped in the conversion.");
                            continue;
                        }

                        ConvertTerrains.ExportSingleTerrain(targetTerrain, originalScene, m_settings.m_exportTerrainLODSettingsSourceTerrains, lodIndex, numberOfLODs, lodIndex == numberOfLODs - 1, m_settings.m_createImpostorScenes, serverScene);

                        if (terrainScene != null)
                        {
                            Scene scene = EditorSceneManager.GetSceneByPath(terrainScene.m_scenePath);
                            if (scene != null)
                            {
                                EditorSceneManager.MarkSceneDirty(scene);
                            }
                            terrainScene.RemoveRegularReference(SessionManager.gameObject);
                        }

                        EditorUtility.ClearProgressBar();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error during Terrain Export: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                    }
                    finally
                    {
                        ProgressBar.Clear(ProgressBarPriority.TerrainMeshExport);
                    }
                    m_timeSinceLastEditorUpdate = DateTime.Now;
                    yield return null;
                }

                //Restore original lighting
                OrthographicBake.LightsOn();
                OrthographicBake.RemoveOrthoCam();

#if HDPipeline
                    OrthographicBake.RemoveBakeDirectionalLight();
#endif

                if (weatherObject != null)
                {
                    weatherObject.SetActive(true);
                }
                RenderSettings.ambientMode = originalAmbientMode;
                RenderSettings.ambientSkyColor = originalAmbientColor;

                QualitySettings.lodBias = originalLODBias;
            }

            ConvertTerrains.CleanUpAfterConversion(m_settings, m_settings.m_exportPath, timeStamp);
            StopEditorUpdates();
            m_conversionRunning = false;

            switch (m_settings.m_convertSourceTerrainsAction)
            {
                case ConversionAction.MeshTerrain:
                    m_showTerrainState = false;
                    m_showConversionResultState = true;
                    break;
                case ConversionAction.ColliderOnly:
                    m_showTerrainState = false;
                    m_showConversionResultState = true;
                    break;
                case ConversionAction.OBJFileExport:
                    m_showTerrainState = true;
                    m_showConversionResultState = false;
                    break;
            }

            if (m_settings.m_colliderExportCreateServerScene)
            {
                BuildConfig buildConfig = GaiaUtils.GetOrCreateBuildConfig();
                //try to find matching entry
                SceneAsset masterScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(originalScene.path, typeof(SceneAsset));
                SceneBuildEntry sbe = buildConfig.m_sceneBuildEntries.Find(x => x.m_masterScene == masterScene);
                if (sbe != null)
                {
                    sbe.m_serverScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(serverScenePath, typeof(SceneAsset));
                    buildConfig.AddBuildHistoryEntry(BuildLogCategory.ServerScene, sbe.m_masterScene.name, GaiaUtils.GetUnixTimestamp());
                }
            }

            //If impostor scenes were created, those need to be part of the build settings
            if (m_settings.m_createImpostorScenes)
            {
                GaiaSessionManager.AddTerrainScenesToBuildSettings(TerrainLoaderManager.TerrainScenes);
            }

            if (GaiaUtils.HasDynamicLoadedTerrains() && m_settings.m_convertSourceTerrains && m_settings.m_convertSourceTerrainsAction == ConversionAction.ColliderOnly && m_settings.m_colliderExportCreateColliderScenes)
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("CreatedColliderScenesTitle"), m_editorUtils.GetTextValue("CreatedColliderScenesText"), m_editorUtils.GetTextValue("SwitchToCollidersOnly"), m_editorUtils.GetTextValue("StayWithRegularTerrains")))
                {
                    // do an unload with the old setting
                    TerrainLoaderManager.Instance.UnloadAll(true);
                    //then change the actual flag in storage
                    TerrainLoaderManager.Instance.TerrainSceneStorage.m_colliderOnlyLoading = true;
                    //now do a refresh under the new setting
                    TerrainLoaderManager.Instance.RefreshSceneViewLoadingRange();

                    GaiaSessionManager.AddOnlyColliderScenesToBuildSettings(TerrainLoaderManager.TerrainScenes);
                }
            }

            RefreshTerrainList();
            yield return null;
        }

        #endregion


    }
}
