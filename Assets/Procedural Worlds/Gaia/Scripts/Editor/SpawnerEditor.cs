using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using PWCommon5;
using UnityEditorInternal;
using Gaia.Internal;
using static Gaia.GaiaConstants;
using System.Linq;
using System.IO;
using static Gaia.FbmFractalType;
using ProceduralWorlds.WaterSystem;
using UnityEditor.SceneManagement;
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
#if CTS_PRESENT
using CTS;
#endif
using Gaia.GX.ProceduralWorlds;

namespace Gaia
{
    [CustomEditor(typeof(Spawner))]
    public class SpawnerEditor : PWEditor, IPWEditor
    {
        GUIStyle m_boxStyle;
        GUIStyle m_wrapStyle;
        GUIStyle m_smallButtonStyle;
        GUIStyle m_visualizeAllButtonStyle;
        GUIStyle m_operationStyle;
        GUIStyle m_generateSeedCheckboxStyle;
        GUIStyle m_sceneViewLabelStyle;
        GUIStyle m_sceneViewShadowStyle;
        GUIStyle m_sceneViewGraphLabelStyle;
        GUIStyle m_sceneViewToggleStyle;
        GUIStyle m_sceneViewWarningLabelStyle;
        GUIStyle m_sceneViewOKLabelStyle;
        Spawner m_spawner;
        private EditorUtils m_editorUtils;
        DateTime m_timeSinceLastUpdate = DateTime.Now;
        bool m_startedUpdates = false;
        private bool m_showTooltips = true;
        private bool m_spawnerMaskListExpanded = true;

        private bool[] m_spawnRuleMaskListExpanded;
        private bool[] m_previewImageDisplayedDuringLayout;

        private ReorderableList[] m_reorderableRuleMasksLists;
        private ReorderableList m_biomeSpawnersList;
        private List<BiomePresetDropdownEntry> m_allBiomePresets = new List<BiomePresetDropdownEntry>();
        private UserFiles m_userFiles;

        //private EditorUtilsOLD m_editorUtils = new EditorUtilsOLD();
        private UnityEditorInternal.ReorderableList m_reorderableSpawnerMaskList;

        private float m_lastXPos;
        private float m_lastZPos;

        private bool m_activatePreviewRequested;
        private long m_activatePreviewTimeStamp;
        private bool m_showStampBaseHelp;
        private bool m_showExportTerrainHelp;
        private Color visColor;
        private bool m_showAutoStreamSettingsBox;


        private GUIStyle m_spawnRulesHeader;
        private GUIStyle m_singleSpawnRuleHeader;
        private GUIStyle m_errorHeader;
        private GUIStyle m_spawnRulesBorder;
        private GUIStyle m_noRulesLabelStyle;

        private string m_SaveAndLoadMessage = "";
        private MessageType m_SaveAndLoadMessageType;
        private GaiaSettings m_gaiaSettings;
        private GaiaSessionManager m_sessionManager;
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
        private bool m_rulePanelHelpActive;
        private List<Texture2D> m_tempTextureList = new List<Texture2D>();
        private int m_drawResourcePreviewRuleId;
        private string m_ResourceManagementMessage = "";
        private MessageType m_ResourceManagementMessageType;

        private double m_lastTileCoordinateChangeTimestamp = double.MaxValue;


        /// <summary>
        /// This is a color used to initialize all spawning rules with a progressing palette.
        /// </summary>
        private Color m_rollingColor = Color.red;
        private bool m_changesMadeSinceLastSave;
        private Color m_dirtyColor;
        private Color m_normalBGColor;
        private List<string> m_stampCategoryNames = new List<string>();
        private int[] m_stampCategoryIDArray;
        private bool m_fitToWorldAllowed;
        private GaiaConstants.ImportableResourceType m_resourceImportType;
        private GaiaConstants.ImportableResourceMode m_resourceImportMode;
        private Terrain m_resourceImportTerrain;

        void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;

            //Get the settings and update tooltips
            if (m_gaiaSettings == null)
            {
                m_gaiaSettings = GaiaUtils.GetGaiaSettings();
            }
            if (m_gaiaSettings != null)
            {
                m_showTooltips = m_gaiaSettings.m_showTooltips;
            }

            //Get our spawner
            m_spawner = (Spawner)target;

            //make sure we got settings
            if (m_spawner.m_settings == null)
            {
                m_spawner.m_settings = ScriptableObject.CreateInstance<SpawnerSettings>();
                m_reorderableRuleMasksLists = null;
                serializedObject.ApplyModifiedProperties();
            }

            //Init editor utils
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            m_previewImageDisplayedDuringLayout = new bool[m_spawner.m_settings.m_spawnerRules.Count];

            CreateMaskLists();

            //Fill the enum popup for stamp categories / feature names from the stamp directory
            m_stampCategoryNames.Clear();
            var info = new DirectoryInfo(GaiaDirectories.GetStampDirectory());
            var fileInfo = info.GetDirectories();
            foreach (DirectoryInfo dir in fileInfo)
            {
                m_stampCategoryNames.Add(dir.Name);
            }

            m_stampCategoryIDArray = Enumerable
                               .Repeat(0, (int)((m_stampCategoryNames.ToArray().Length - 1) / 1) + 1)
                               .Select((tr, ti) => tr + (1 * ti))
                               .ToArray();

            //Clean up any rules that relate to missing resources
            CleanUpRules();

            //Do some simple sanity checks
            if (m_spawner.m_rndGenerator == null)
            {
                m_spawner.m_rndGenerator = new Gaia.XorshiftPlus(m_spawner.m_seed);
            }

            if (m_spawner.m_spawnFitnessAttenuator == null)
            {
                m_spawner.m_spawnFitnessAttenuator = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1.0f), new Keyframe(1f, 0.0f));
            }

            if (m_spawner.m_spawnCollisionLayers.value == 0)
            {
                m_spawner.m_spawnCollisionLayers = Gaia.TerrainHelper.GetActiveTerrainLayer();
            }

            m_lastXPos = m_spawner.transform.position.x;
            m_lastZPos = m_spawner.transform.position.z;

            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                //hide the transform gizmo
                Tools.hidden = true;

                if (m_spawner.m_worldCreationSettings != null && m_spawner.m_worldCreationSettings.m_gaiaDefaults != null)
                {
                    m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
                }

                SessionManager.m_session.m_worldBiomeMaskSettings = m_spawner.m_settings;
                SessionManager.SaveSession();
                Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);

                //enable preview by default - when returning to the world designer, you usually want to see the preview again
                stamper.m_drawPreview = true;

                if (m_userFiles == null)
                {
                    m_userFiles = GaiaUtils.GetOrCreateUserFiles();
                }

                for (int i = 0; i < m_userFiles.m_gaiaManagerBiomePresets.Count; i++)
                {
                    BiomePreset sp = m_userFiles.m_gaiaManagerBiomePresets[i];
                    if (sp != null)
                    {
                        m_allBiomePresets.Add(new BiomePresetDropdownEntry { ID = i, name = sp.name, biomePreset = sp });
                    }
                }

                if (m_userFiles.m_sortBiomesBy == SortBiomesBy.Name)
                {
                    m_allBiomePresets.Sort(BiomePresetDropdownEntry.CompareByName);
                }
                else
                {
                    m_allBiomePresets.Sort(BiomePresetDropdownEntry.CompareByOrderNumber);
                }
                //Add the artifical "Custom" option
                m_allBiomePresets.Add(new BiomePresetDropdownEntry { ID = -999, name = "Custom", biomePreset = null });

                if (m_allBiomePresets.Count > 0 && m_spawner.m_worldDesignerBiomePresetSelection == int.MinValue)
                {
                    m_spawner.m_worldDesignerBiomePresetSelection = m_allBiomePresets[0].ID;
                }

                if (m_spawner.m_worldDesignerBiomePresetSelection != int.MinValue)
                {
                    //Fill in initial content
                    AddBiomeSpawnersForSelectedPreset();
                    CreateBiomePresetList();
                }

                m_spawner.m_worldDesignerClearStampsWarningShown = false;
                TerrainLoaderManager.Instance.SwitchToWorldMap();

            }
            else
            {
                Tools.hidden = false;
            }

            if (GaiaWater.DoesWaterExist())
            {
                m_spawner.m_showSeaLevelPlane = false;
            }

            GaiaLighting.SetPostProcessingStatus(false);

            m_dirtyColor = GaiaUtils.GetColorFromHTML("#FF666666");
            m_normalBGColor = GUI.backgroundColor;
            SessionManager.CheckForNewTerrainsForMinMax();
            m_spawner.UpdateMinMaxHeight();

#if GAIA_PRO_PRESENT
            m_spawner.TerrainLoader.m_isSelected = true;
#endif
            //Check if fit to world is possible with the current resolution etc. settings
            float requiredFitToWorldWidth = (TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX * TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesSize) / 2;
            if (requiredFitToWorldWidth > m_spawner.GetMaxSpawnRange())
            {
                m_fitToWorldAllowed = false;
            }
            else
            {
                m_fitToWorldAllowed = true;
            }
            m_spawner.ControlSpawnRuleGUIDs();
            ImageMask.RefreshSpawnRuleGUIDs();
            StartEditorUpdates();
            m_spawner.m_spawnPreviewDirty = true;
            //m_spawner.BaseTerrainStamper.m_stampDirty = true;
            //Force a repaint to get the stamper visuailization running immediately
            //SceneView.RepaintAll();
#if FLORA_PRESENT
            FloraEditorUtils.UnSubscribeToUndo();
#endif
        }

        private void AddBiomeSpawnersForSelectedPreset()
        {
            BiomePresetDropdownEntry entry = m_allBiomePresets.Find(x => x.ID == m_spawner.m_worldDesignerBiomePresetSelection);
            if (entry.biomePreset != null)
            {
                m_spawner.m_BiomeSpawnersToCreate.Clear();
                //Need to create a deep copy of the preset list, otherwise the users will overwrite it when they add custom spawners
                foreach (BiomeSpawnerListEntry spawnerListEntry in entry.biomePreset.m_spawnerPresetList)
                {
                    if (spawnerListEntry.m_spawnerSettings != null)
                    {
                        m_spawner.m_BiomeSpawnersToCreate.Add(spawnerListEntry);
                    }
                }
            }
        }

        void CreateBiomePresetList()
        {
            m_biomeSpawnersList = new UnityEditorInternal.ReorderableList(m_spawner.m_BiomeSpawnersToCreate, typeof(BiomeSpawnerListEntry), true, true, true, true);
            m_biomeSpawnersList.elementHeightCallback = OnElementHeightSpawnerPresetListEntry;
            m_biomeSpawnersList.drawElementCallback = DrawSpawnerPresetListElement;
            m_biomeSpawnersList.drawHeaderCallback = DrawSpawnerPresetListHeader;
            m_biomeSpawnersList.onAddCallback = OnAddSpawnerPresetListEntry;
            m_biomeSpawnersList.onRemoveCallback = OnRemoveSpawnerPresetListEntry;
            m_biomeSpawnersList.onReorderCallback = OnReorderSpawnerPresetList;
        }

        private void OnReorderSpawnerPresetList(ReorderableList list)
        {
            //Do nothing, changing the order does not immediately affect anything in this window
        }

        private void OnRemoveSpawnerPresetListEntry(ReorderableList list)
        {
            m_spawner.m_BiomeSpawnersToCreate = SpawnerPresetListEditor.OnRemoveListEntry(m_spawner.m_BiomeSpawnersToCreate, m_biomeSpawnersList.index);
            list.list = m_spawner.m_BiomeSpawnersToCreate;
            m_spawner.m_worldDesignerBiomePresetSelection = -999;
        }

        private void OnAddSpawnerPresetListEntry(ReorderableList list)
        {
            m_spawner.m_BiomeSpawnersToCreate = SpawnerPresetListEditor.OnAddListEntry(m_spawner.m_BiomeSpawnersToCreate);
            list.list = m_spawner.m_BiomeSpawnersToCreate;
            m_spawner.m_worldDesignerBiomePresetSelection = -999;
        }

        private void DrawSpawnerPresetListHeader(Rect rect)
        {
            SpawnerPresetListEditor.DrawListHeader(rect, true, m_spawner.m_BiomeSpawnersToCreate, m_editorUtils, "SpawnerAdded");
        }

        private void DrawSpawnerPresetListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SpawnerPresetListEditor.DrawListElement(rect, m_spawner.m_BiomeSpawnersToCreate[index], m_editorUtils);
        }

        private float OnElementHeightSpawnerPresetListEntry(int index)
        {
            return SpawnerPresetListEditor.OnElementHeight();
        }


        private void CreateMaskLists()
        {
            //Create the spawner mask list
            CreateSpawnerMaskList();

            //Create the individual spawn rule mask lists
            m_spawnRuleMaskListExpanded = new bool[m_spawner.m_settings.m_spawnerRules.Count];
            m_reorderableRuleMasksLists = new ReorderableList[m_spawner.m_settings.m_spawnerRules.Count];

            for (int i = 0; i < m_spawner.m_settings.m_spawnerRules.Count; i++)
            {
                m_reorderableRuleMasksLists[i] = CreateSpawnRuleMaskList(m_reorderableRuleMasksLists[i], m_spawner.m_settings.m_spawnerRules[i].m_imageMasks);
                m_spawnRuleMaskListExpanded[i] = true;
            }
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            GaiaLighting.SetPostProcessingStatus(true);
#if GAIA_PRO_PRESENT
            if (m_spawner != null)
            {
                m_spawner.TerrainLoader.m_isSelected = false;
                m_spawner.UpdateAutoLoadRange();
            }
#endif
            m_spawner.m_settings.ClearImageMaskTextures();
            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Tools.hidden = false;
                TerrainLoaderManager.Instance.SwitchToLocalMap();
            }
#if FLORA_PRESENT
            FloraEditorUtils.UnSubscribeToUndo();
#endif

        }

        void OnDestroy()
        {
            GaiaSessionManager.OnWorldCreationCancelled -= OnWorldCreationCancelled;
            GaiaSessionManager.OnWorldCreated -= OnWorldMapStampingFinished;
            //check if we opened a stamp selection window from this spawner, and if yes, close it down
            var allWindows = Resources.FindObjectsOfTypeAll<GaiaStampSelectorEditorWindow>();
            for (int i = allWindows.Length - 1; i >= 0; i--)
            {
                //Check General Masks first

                foreach (ImageMask imageMask in m_spawner.m_settings.m_imageMasks)
                {
                    if (allWindows[i].m_editedImageMask == imageMask)
                    {
                        allWindows[i].Close();
                    }
                }

                //Then the rule specific masks

                foreach (SpawnRule spawnRule in m_spawner.m_settings.m_spawnerRules)
                {
                    foreach (ImageMask imageMask in spawnRule.m_imageMasks)
                    {
                        if (allWindows[i].m_editedImageMask == imageMask)
                        {
                            allWindows[i].Close();
                        }
                    }
                }
            }

            for (int i = 0; i < m_tempTextureList.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(m_tempTextureList[i]);
            }

            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Tools.hidden = false;
            }

        }

#region SPAWN RULE MASK LIST MANAGEMENT

        /// <summary>
        /// Creates the reorderable collision mask list for collision masks in the spawn rules.
        /// </summary>
        public ReorderableList CreateSpawnRuleCollisionMaskList(ReorderableList list, CollisionMask[] collisionMasks)
        {
            list = new ReorderableList(collisionMasks, typeof(CollisionMask), true, true, true, true);
            list.elementHeightCallback = OnElementHeightCollisionMaskList;
            list.drawElementCallback = DrawSpawnRuleCollisionMaskElement;
            list.drawHeaderCallback = DrawSpawnRuleCollisionMaskListHeader;
            list.onAddCallback = OnAddSpawnRuleCollisionMaskListEntry;
            list.onRemoveCallback = OnRemoveSpawnRuleCollisionMaskMaskListEntry;
            return list;
        }

        private void OnRemoveSpawnRuleCollisionMaskMaskListEntry(ReorderableList list)
        {
            //find spawn rule index & mask index which are being edited, so we know who this list of collision masks belongs to
            int maskIndex = -99;
            int spawnRuleIndex = FindSpawnRuleIndexByReorderableCollisionMaskList(list, ref maskIndex);
            m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks = CollisionMaskListEditor.OnRemoveMaskListEntry(m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks, list.index);
            list.list = m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks;
        }

        private void OnAddSpawnRuleCollisionMaskListEntry(ReorderableList list)
        {
            //find spawn rule index & mask index which are being edited, so we know who this list of collision masks belongs to
            int maskIndex = -99;
            int spawnRuleIndex = FindSpawnRuleIndexByReorderableCollisionMaskList(list, ref maskIndex);
            m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks = CollisionMaskListEditor.OnAddMaskListEntry(m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks);
            list.list = m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks;
        }

        private void DrawSpawnRuleCollisionMaskListHeader(Rect rect)
        {
            if (m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks.Length > 0)
            {
                m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[m_spawner.m_spawnRuleMaskIndexBeingDrawn].m_collisionMaskExpanded = CollisionMaskListEditor.DrawFilterListHeader(rect, m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[m_spawner.m_spawnRuleMaskIndexBeingDrawn].m_collisionMaskExpanded, m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[m_spawner.m_spawnRuleMaskIndexBeingDrawn].m_collisionMasks, m_editorUtils);
            }
        }

        private void DrawSpawnRuleCollisionMaskElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks.Length == 0)
            {
                return;
            }

            if (m_spawner.m_collisionMaskListBeingDrawn == null)
            {
                m_spawner.m_collisionMaskListBeingDrawn = m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[m_spawner.m_spawnRuleMaskIndexBeingDrawn].m_collisionMasks;
            }

            if (m_spawner.m_collisionMaskListBeingDrawn != null && m_spawner.m_collisionMaskListBeingDrawn.Length > index && m_spawner.m_collisionMaskListBeingDrawn[index] != null)
            {
                CollisionMaskListEditor.DrawMaskListElement(rect, index, m_spawner.m_collisionMaskListBeingDrawn[index], m_editorUtils, Terrain.activeTerrain, GaiaConstants.FeatureOperation.Contrast);
            }
        }

        private float OnElementHeightCollisionMaskList(int index)
        {
            return CollisionMaskListEditor.OnElementHeight(index, m_spawner.m_collisionMaskListBeingDrawn);
        }




        /// <summary>
        /// Creates the reorderable mask list for the individual spawn rules.
        /// </summary>
        private ReorderableList CreateSpawnRuleMaskList(ReorderableList list, ImageMask[] imageMasks)
        {
            list = new UnityEditorInternal.ReorderableList(imageMasks, typeof(ImageMask), true, true, true, true);
            list.elementHeightCallback = OnElementHeightSpawnRuleMaskList;
            list.drawElementCallback = DrawSpawnRuleMaskListElement;
            list.drawHeaderCallback = DrawSpawnRuleMaskListHeader;
            list.onAddCallback = OnAddSpawnRuleMaskListEntry;
            list.onRemoveCallback = OnRemoveSpawnRuleMaskListEntry;
            list.onReorderCallback = OnReorderSpawnRuleMaskList;

            foreach (ImageMask mask in imageMasks)
            {
                mask.m_reorderableCollisionMaskList = CreateSpawnRuleCollisionMaskList(mask.m_reorderableCollisionMaskList, mask.m_collisionMasks);
            }

            return list;
        }

        private float OnElementHeightSpawnRuleMaskList(int index)
        {
            if (m_spawner.m_maskListBeingDrawn.Length > 0)
            {
                return ImageMaskListEditor.OnElementHeight(index, m_spawner.m_maskListBeingDrawn[index]);
            }
            else
            {
                return 0f;
            }
        }

        private void OnReorderSpawnRuleMaskList(ReorderableList list)
        {
            m_spawner.m_spawnPreviewDirty = true;
            m_spawner.SetWorldBiomeMasksDirty();
            m_spawner.DrawSpawnerPreview();
        }

        private void OnRemoveSpawnRuleMaskListEntry(ReorderableList list)
        {
            //find spawn rule index that is being edited
            int spawnRuleIndex = FindSpawnRuleIndexByReorderableMaskList(list);
            m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks = ImageMaskListEditor.OnRemoveMaskListEntry(m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks, list.index);
            list.list = m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks;
        }

        private void OnAddSpawnRuleMaskListEntry(ReorderableList list)
        {
            //find spawn rule index that is being edited
            int spawnRuleIndex = FindSpawnRuleIndexByReorderableMaskList(list);
            m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks = ImageMaskListEditor.OnAddMaskListEntry(m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks, m_spawner.m_maxWorldHeight, m_spawner.m_minWorldHeight, SessionManager.m_session.m_seaLevel);
            //set up the new collision mask inside the newly added mask
            ImageMask[] maskList = m_spawner.m_settings.m_spawnerRules[spawnRuleIndex].m_imageMasks;
            ImageMask lastElement = maskList[maskList.Length - 1];
            lastElement.m_reorderableCollisionMaskList = CreateSpawnRuleCollisionMaskList(lastElement.m_reorderableCollisionMaskList, lastElement.m_collisionMasks);
            list.list = maskList;
        }


        private void DrawSpawnRuleMaskListHeader(Rect rect)
        {
            m_spawnRuleMaskListExpanded[m_spawner.m_spawnRuleIndexBeingDrawn] = ImageMaskListEditor.DrawFilterListHeader(rect, m_spawnRuleMaskListExpanded[m_spawner.m_spawnRuleIndexBeingDrawn], m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks, m_editorUtils);
        }

        int FindSpawnRuleIndexByReorderableMaskList(ReorderableList maskList)
        {
            //find texture index that is being edited
            int spawnRuleIndex = -99;

            for (int i = 0; i < m_reorderableRuleMasksLists.Length; i++)
            {
                if (m_reorderableRuleMasksLists[i] == maskList)
                {
                    spawnRuleIndex = i;
                }
            }
            return spawnRuleIndex;
        }

        int FindSpawnRuleIndexByReorderableCollisionMaskList(ReorderableList collisionMaskList, ref int maskIndex)
        {
            //find texture index that is being edited
            int spawnRuleIndex = -99;

            for (int i = 0; i < m_reorderableRuleMasksLists.Length; i++)
            {
                for (int j = 0; j < m_reorderableRuleMasksLists[i].list.Count; j++)
                {
                    if (((ImageMask)m_reorderableRuleMasksLists[i].list[j]).m_reorderableCollisionMaskList == collisionMaskList)
                    {
                        maskIndex = j;
                        spawnRuleIndex = i;
                    }
                }
            }
            return spawnRuleIndex;
        }

        private void DrawSpawnRuleMaskListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_spawner.m_maskListBeingDrawn != null && m_spawner.m_maskListBeingDrawn.Length > index)
            {

                ImageMask copiedImageMask = SessionManager.m_copiedImageMask;

                m_spawner.m_spawnRuleMaskIndexBeingDrawn = index;
                bool isStampOperation = m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_resourceType == SpawnerResourceType.TerrainModifierStamp;

                MaskListButtonCommand mlbc = ImageMaskListEditor.DrawMaskListElement(rect, index, m_spawner.m_maskListBeingDrawn, ref m_spawner.m_collisionMaskListBeingDrawn, m_editorUtils, Terrain.activeTerrain, isStampOperation, copiedImageMask, m_spawnRulesHeader.normal.background, m_errorHeader.normal.background, m_gaiaSettings, SessionManager);

                switch (mlbc)
                {
                    case MaskListButtonCommand.Delete:
                        foreach (ReorderableList rl in m_reorderableRuleMasksLists)
                        {
                            if (rl.list == m_spawner.m_maskListBeingDrawn)
                            {
                                rl.index = index;
                                OnRemoveSpawnRuleMaskListEntry(rl);
                            }
                        }

                        break;
                    case MaskListButtonCommand.Duplicate:
                        foreach (ReorderableList rl in m_reorderableRuleMasksLists)
                        {
                            if (rl.list == m_spawner.m_maskListBeingDrawn)
                            {

                                ImageMask newImageMask = ImageMask.Clone(m_spawner.m_maskListBeingDrawn[index]);
                                m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks = GaiaUtils.InsertElementInArray(m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks, newImageMask, index + 1);
                                rl.list = m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks;
                                m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[index + 1].m_reorderableCollisionMaskList = CreateSpawnRuleCollisionMaskList(m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[index + 1].m_reorderableCollisionMaskList, m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_imageMasks[index + 1].m_collisionMasks);
                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        break;
                    case MaskListButtonCommand.Copy:
                        SessionManager.m_copiedImageMask = m_spawner.m_maskListBeingDrawn[index];
                        break;
                    case MaskListButtonCommand.Paste:
                        m_spawner.m_maskListBeingDrawn[index] = ImageMask.Clone(copiedImageMask);
                        //Rebuild collsion mask list with new content from the cloning
                        m_spawner.m_maskListBeingDrawn[index].m_reorderableCollisionMaskList = CreateSpawnRuleCollisionMaskList(m_spawner.m_maskListBeingDrawn[index].m_reorderableCollisionMaskList, m_spawner.m_maskListBeingDrawn[index].m_collisionMasks);
                        SessionManager.m_copiedImageMask = null;
                        break;

                }
                if (m_spawner.m_maskListBeingDrawn.Length - 1 >= index)
                {
                    m_spawner.m_maskListBeingDrawn[index].m_imageMaskLocation = ImageMaskLocation.SpawnRule;
                }


            }
        }

#endregion

#region SPAWNER MASK LIST MANAGEMENT

        /// <summary>
        /// Creates the reorderable mask list for the spawner itself.
        /// </summary>
        private void CreateSpawnerMaskList()
        {
            m_reorderableSpawnerMaskList = new UnityEditorInternal.ReorderableList(m_spawner.m_settings.m_imageMasks, typeof(ImageMask), true, true, true, true);
            m_reorderableSpawnerMaskList.elementHeightCallback = OnElementHeightSpawnerMaskList;
            m_reorderableSpawnerMaskList.drawElementCallback = DrawSpawnerMaskListElement;
            m_reorderableSpawnerMaskList.drawHeaderCallback = DrawSpawnerMaskListHeader;
            m_reorderableSpawnerMaskList.onAddCallback = OnAddSpawnerMaskListEntry;
            m_reorderableSpawnerMaskList.onRemoveCallback = OnRemoveSpawnerMaskListEntry;
            m_reorderableSpawnerMaskList.onReorderCallback = OnReorderSpawnerMaskList;

            foreach (ImageMask mask in m_spawner.m_settings.m_imageMasks)
            {
                mask.m_reorderableCollisionMaskList = CreateSpawnerCollisionMaskList(mask.m_reorderableCollisionMaskList, mask.m_collisionMasks);
            }
        }

        private float OnElementHeightSpawnerMaskList(int index)
        {
            if (index < m_spawner.m_settings.m_imageMasks.Length)
            {
                return ImageMaskListEditor.OnElementHeight(index, m_spawner.m_settings.m_imageMasks[index]);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        private void OnReorderSpawnerMaskList(ReorderableList list)
        {
            m_spawner.m_spawnPreviewDirty = true;
            m_spawner.SetWorldBiomeMasksDirty();
            m_spawner.DrawSpawnerPreview();
        }

        private void OnRemoveSpawnerMaskListEntry(ReorderableList list)
        {
            m_spawner.m_settings.m_imageMasks = ImageMaskListEditor.OnRemoveMaskListEntry(m_spawner.m_settings.m_imageMasks, list.index);
            list.list = m_spawner.m_settings.m_imageMasks;
        }

        private void OnAddSpawnerMaskListEntry(ReorderableList list)
        {
            m_spawner.m_settings.m_imageMasks = ImageMaskListEditor.OnAddMaskListEntry(m_spawner.m_settings.m_imageMasks, m_spawner.m_maxWorldHeight, m_spawner.m_minWorldHeight, SessionManager.m_session.m_seaLevel);
            ImageMask lastElement = m_spawner.m_settings.m_imageMasks[m_spawner.m_settings.m_imageMasks.Length - 1];
            lastElement.m_reorderableCollisionMaskList = CreateSpawnerCollisionMaskList(lastElement.m_reorderableCollisionMaskList, lastElement.m_collisionMasks);
            list.list = m_spawner.m_settings.m_imageMasks;
        }

        private void DrawSpawnerMaskListHeader(Rect rect)
        {
            m_spawnerMaskListExpanded = ImageMaskListEditor.DrawFilterListHeader(rect, m_spawnerMaskListExpanded, m_spawner.m_settings.m_imageMasks, m_editorUtils);
        }

        private void DrawSpawnerMaskListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ImageMask copiedImageMask = SessionManager.m_copiedImageMask;
            //bool isCopiedMask = m_spawner.m_settings.m_imageMasks[index] != null && m_spawner.m_settings.m_imageMasks[index] == m_copiedImageMask;
            MaskListButtonCommand mlbc = ImageMaskListEditor.DrawMaskListElement(rect, index, m_spawner.m_settings.m_imageMasks, ref m_spawner.m_collisionMaskListBeingDrawn, m_editorUtils, Terrain.activeTerrain, false, copiedImageMask, m_spawnRulesHeader.normal.background, m_errorHeader.normal.background, m_gaiaSettings, SessionManager);
            switch (mlbc)
            {
                case MaskListButtonCommand.Delete:
                    m_reorderableSpawnerMaskList.index = index;
                    OnRemoveSpawnerMaskListEntry(m_reorderableSpawnerMaskList);
                    break;
                case MaskListButtonCommand.Duplicate:
                    ImageMask newImageMask = ImageMask.Clone(m_spawner.m_settings.m_imageMasks[index]);
                    m_spawner.m_settings.m_imageMasks = GaiaUtils.InsertElementInArray(m_spawner.m_settings.m_imageMasks, newImageMask, index + 1);
                    m_reorderableSpawnerMaskList.list = m_spawner.m_settings.m_imageMasks;
                    m_spawner.m_settings.m_imageMasks[index + 1].m_reorderableCollisionMaskList = CreateSpawnerCollisionMaskList(m_spawner.m_settings.m_imageMasks[index + 1].m_reorderableCollisionMaskList, m_spawner.m_settings.m_imageMasks[index + 1].m_collisionMasks);
                    serializedObject.ApplyModifiedProperties();

                    break;
                case MaskListButtonCommand.Copy:
                    SessionManager.m_copiedImageMask = m_spawner.m_settings.m_imageMasks[index];
                    break;
                case MaskListButtonCommand.Paste:
                    m_spawner.m_settings.m_imageMasks[index] = ImageMask.Clone(copiedImageMask);
                    //Rebuild collsion mask list with new content from the cloning
                    m_spawner.m_settings.m_imageMasks[index].m_reorderableCollisionMaskList = CreateSpawnerCollisionMaskList(m_spawner.m_settings.m_imageMasks[index].m_reorderableCollisionMaskList, m_spawner.m_settings.m_imageMasks[index].m_collisionMasks);
                    SessionManager.m_copiedImageMask = null;
                    break;

            }
            if (m_spawner.m_settings.m_imageMasks.Length - 1 >= index)
            {
                m_spawner.m_settings.m_imageMasks[index].m_imageMaskLocation = ImageMaskLocation.SpawnerGlobal;
            }
        }



        /// <summary>
        /// Creates the reorderable collision mask list for collision masks in the spawner itself.
        /// </summary>
        public ReorderableList CreateSpawnerCollisionMaskList(ReorderableList list, CollisionMask[] collisionMasks)
        {
            list = new ReorderableList(collisionMasks, typeof(CollisionMask), true, true, true, true);
            list.elementHeightCallback = OnElementHeightCollisionMaskList;
            list.drawElementCallback = DrawSpawnerCollisionMaskElement;
            list.drawHeaderCallback = DrawSpawnerCollisionMaskListHeader;
            list.onAddCallback = OnAddSpawnerCollisionMaskListEntry;
            list.onRemoveCallback = OnRemoveSpawnerCollisionMaskMaskListEntry;
            return list;
        }

        private void OnRemoveSpawnerCollisionMaskMaskListEntry(ReorderableList list)
        {
            //look up the collision mask in the spawner's mask list
            foreach (ImageMask imagemask in m_spawner.m_settings.m_imageMasks)
            {
                if (imagemask.m_reorderableCollisionMaskList == list)
                {
                    imagemask.m_collisionMasks = CollisionMaskListEditor.OnRemoveMaskListEntry(imagemask.m_collisionMasks, list.index);
                    list.list = imagemask.m_collisionMasks;
                    return;
                }
            }
        }

        private void OnAddSpawnerCollisionMaskListEntry(ReorderableList list)
        {
            //look up the collision mask in the spawner's mask list
            foreach (ImageMask imagemask in m_spawner.m_settings.m_imageMasks)
            {
                if (imagemask.m_reorderableCollisionMaskList == list)
                {
                    imagemask.m_collisionMasks = CollisionMaskListEditor.OnAddMaskListEntry(imagemask.m_collisionMasks);
                    list.list = imagemask.m_collisionMasks;
                    return;
                }
            }
        }

        private void DrawSpawnerCollisionMaskListHeader(Rect rect)
        {
            foreach (ImageMask imagemask in m_spawner.m_settings.m_imageMasks)
            {
                if (imagemask.m_collisionMasks == m_spawner.m_collisionMaskListBeingDrawn)
                {
                    imagemask.m_collisionMaskExpanded = CollisionMaskListEditor.DrawFilterListHeader(rect, imagemask.m_collisionMaskExpanded, imagemask.m_collisionMasks, m_editorUtils);
                }
            }
        }

        private void DrawSpawnerCollisionMaskElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_spawner.m_collisionMaskListBeingDrawn != null && m_spawner.m_collisionMaskListBeingDrawn.Length > index && m_spawner.m_collisionMaskListBeingDrawn[index] != null)
            {
                CollisionMaskListEditor.DrawMaskListElement(rect, index, m_spawner.m_collisionMaskListBeingDrawn[index], m_editorUtils, Terrain.activeTerrain, GaiaConstants.FeatureOperation.Contrast);
            }
        }

#endregion

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            if (!m_startedUpdates)
            {
                m_startedUpdates = true;
                EditorApplication.update += EditorUpdate;
            }
        }

        /// <summary>
        /// Stop editor updates
        /// </summary>
        public void StopEditorUpdates()
        {
            if (m_startedUpdates)
            {
                m_startedUpdates = false;
                EditorApplication.update -= EditorUpdate;
            }
        }

        /// <summary>
        /// This is used just to force the editor to repaint itself
        /// </summary>
        void EditorUpdate()
        {
            if (m_spawner != null)
            {
                if (m_spawner.m_updateCoroutine != null)
                {
                    if ((DateTime.Now - m_timeSinceLastUpdate).TotalMilliseconds > 500)
                    {
                        //Debug.Log("Active repainting spawner " + m_spawner.gameObject.name);
                        m_timeSinceLastUpdate = DateTime.Now;
                        Repaint();
                    }
                }
                else
                {
                    if ((DateTime.Now - m_timeSinceLastUpdate).TotalSeconds > 5)
                    {
                        //Debug.Log("Inactive repainting spawner " + m_spawner.gameObject.name);
                        m_timeSinceLastUpdate = DateTime.Now;
                        Repaint();
                    }
                }
            }
        }

        public void DuringSceneGUI(SceneView obj)
        {
            //Do not draw anything in scene gui while the world creation is running, this just blocks the view.
            if(SessionManager.m_worldCreationRunning)
            {
                return;
            }

            bool currentGUIState = GUI.enabled;

            bool heightmapResExceeded = false;
            bool stampsExceeded = false;

            float scaledScreenWidth = (Camera.current.pixelRect.size.x / EditorGUIUtility.pixelsPerPoint);
            float scaledScreenHeight = (Camera.current.pixelRect.size.y / EditorGUIUtility.pixelsPerPoint);

            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                if (m_sceneViewLabelStyle == null)
                {
                    m_sceneViewLabelStyle = new GUIStyle(GUI.skin.label);
                    m_sceneViewLabelStyle.normal.textColor = Color.white;
                }
                if (m_sceneViewShadowStyle == null)
                {
                    m_sceneViewShadowStyle = new GUIStyle(GUI.skin.label);
                    m_sceneViewShadowStyle.normal.textColor = Color.black;
                }

                if (m_sceneViewWarningLabelStyle == null)
                {
                    m_sceneViewWarningLabelStyle = new GUIStyle(GUI.skin.label);
                    m_sceneViewWarningLabelStyle.normal.textColor = Color.yellow;
                }
                if (m_sceneViewOKLabelStyle == null)
                {
                    m_sceneViewOKLabelStyle = new GUIStyle(GUI.skin.label);
                    m_sceneViewOKLabelStyle.normal.textColor = Color.green;
                }

                if (m_sceneViewGraphLabelStyle == null)
                {
                    m_sceneViewGraphLabelStyle = new GUIStyle(GUI.skin.label);
                    m_sceneViewGraphLabelStyle.normal.textColor = Color.white;
                    m_sceneViewGraphLabelStyle.alignment = TextAnchor.MiddleCenter;
                }

                if (m_sceneViewToggleStyle == null)
                {
                    m_sceneViewToggleStyle = new GUIStyle(EditorStyles.label);
                    m_sceneViewToggleStyle.normal.textColor = Color.white;
                }

                m_spawner.CheckIfFullPreview(ref heightmapResExceeded, ref stampsExceeded);

               
            }

            // dont render preview if this isnt a repaint. losing performance if we do
            if (Event.current.type == EventType.Repaint)
            {
                //reset rotation, rotation for the spawner is currently not supported because it causes too many issues
                m_spawner.transform.rotation = new Quaternion();

                //set the preview dirty if the transform changed so it will be redrawn correctly in the new location
                //the lastXPos & lastZPos variables are a workaround, because transform.hasChanged was triggering too often
                if (m_lastXPos != m_spawner.transform.position.x || m_lastZPos != m_spawner.transform.position.z)
                {
                    m_lastXPos = m_spawner.transform.position.x;
                    m_lastZPos = m_spawner.transform.position.z;
                    m_spawner.m_spawnPreviewDirty = true;
                    m_spawner.SetWorldBiomeMasksDirty();
                }

                if (m_spawner.m_baseTerrainSettings.m_drawPreview && m_spawner.m_settings.m_isWorldmapSpawner)
                {
                    Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                    stamper.m_drawPreview = true;
                    UpdateColorsForWorldDesignerPreview(false);

                    //Main Preview
                    stamper.DrawStampPreview(m_spawner.m_worldMapStamperSettings, true);

                    //Boxes for the custom preview bounds
                    if (m_spawner.m_worldCreationSettings.m_xTiles > 1)
                    {
                        if (stamper.m_useCustomPreviewBounds)
                        {
                            Handles.color = Color.green;
                            Handles.DrawWireCube(stamper.m_worldDesignerPreviewBounds.center, stamper.m_worldDesignerPreviewBounds.size);
                        }

                        Handles.color = Color.white;
                        Handles.DrawWireCube(m_spawner.m_worldDesignerUserBounds.center, m_spawner.m_worldDesignerUserBounds.size);
                    }

                    //Labels for resolution / stamp info
                    //Drawing from the lower left corner 
                    Handles.BeginGUI();
                    Rect rect = new Rect(new Vector2(10f, scaledScreenHeight + EditorGUIUtility.singleLineHeight * 0.5f), new Vector2(scaledScreenWidth, EditorGUIUtility.singleLineHeight));
                    rect.y -= EditorGUIUtility.singleLineHeight * 2f;

                    if (heightmapResExceeded && stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.Worldmap)
                    {
                        GaiaUtils.GUITextWithShadow(rect, "World heightmap resolution exceeds max preview resolution. Zoom in to view single terrain tiles at 1:1 resolution.", m_sceneViewWarningLabelStyle, m_sceneViewShadowStyle);
                        rect.y -= EditorGUIUtility.singleLineHeight;
                    }
                    if (stampsExceeded && stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.Worldmap)
                    {
                        GaiaUtils.GUITextWithShadow(rect, "For performance only stamps touching the green box will be rendered. Move the box with the handles to preview other parts of the world.", m_sceneViewWarningLabelStyle, m_sceneViewShadowStyle);
                        rect.y -= EditorGUIUtility.singleLineHeight;
                    }
                    if (!heightmapResExceeded && !stampsExceeded && stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.Worldmap)
                    {
                        GaiaUtils.GUITextWithShadow(rect, "The current preview is a 1:1 representation of the exported terrains in terms of resolution and scale.", m_sceneViewOKLabelStyle, m_sceneViewShadowStyle);
                        rect.y -= EditorGUIUtility.singleLineHeight;
                    }
                    if (stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
                    {
                        GaiaUtils.GUITextWithShadow(rect, "This single terrain tile is a 1:1 representation of the terrain tile that will be exported.", m_sceneViewOKLabelStyle, m_sceneViewShadowStyle);
                        rect.y -= EditorGUIUtility.singleLineHeight;
                    }


                    GaiaUtils.GUITextWithShadow(rect, $"Stamps - Spawned: {m_spawner.m_worldMapStamperSettings.Count}, Rendered: {stamper.m_worldDesignerRenderedStamps}", m_sceneViewLabelStyle, m_sceneViewShadowStyle);
                    rect.y -= EditorGUIUtility.singleLineHeight;
                    if (stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
                    {
                        GaiaUtils.GUITextWithShadow(rect, $"Resolution - Terrain Tile: {m_spawner.m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution}, Preview: {TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution}", m_sceneViewLabelStyle, m_sceneViewShadowStyle);
                    }
                    else
                    {
                        GaiaUtils.GUITextWithShadow(rect, $"Resolution - World: {m_spawner.m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution * m_spawner.m_worldCreationSettings.m_xTiles}, Preview: {TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution}", m_sceneViewLabelStyle, m_sceneViewShadowStyle);
                    }
                    Handles.EndGUI();

                }
                if (!m_spawner.m_settings.m_isWorldmapSpawner)
                {
                    if (m_spawner.m_drawPreview && m_spawner.m_previewRuleIds.Count > 0 && m_spawner.m_settings.m_spawnerRules[m_spawner.m_previewRuleIds[0]].m_resourceType == SpawnerResourceType.TerrainModifierStamp)
                    {
                        Stamper previewStamper = m_spawner.GetOrCreatePreviewStamper(true);
                        previewStamper.m_drawPreview = true;
                        previewStamper.DrawStampPreview();
                    }
                    else
                    {
                        m_spawner.DrawSpawnerPreview();
                    }
                }

                //Stamp density visualizations - we only draw them during repaint event, we do not want those to be interactive
                if (m_spawner.m_drawWorldDesignerDensityViz || m_spawner.m_autoStampDensityVisualizationTimeStamp + 5000 > GaiaUtils.GetUnixTimestamp())
                {
                    float worldSize = m_spawner.m_worldCreationSettings.m_xTiles * m_spawner.m_worldCreationSettings.m_tileSize;
                    float halfWorldSize = worldSize * 0.5f;
                    float increment = worldSize / (float)m_spawner.m_settings.m_stampDensity;
                    Handles.color = Color.cyan;
                    for (float x = -halfWorldSize; x <= halfWorldSize + 1; x += increment)
                    {
                        for (float z = -halfWorldSize; z <= halfWorldSize + 1; z += increment)
                        {
                            Handles.Disc(Quaternion.identity, new Vector3(x, 0, z), Vector3.up, (m_spawner.m_settings.m_stampWidth / 100f * worldSize) * 0.2f, false, 0f);
                        }
                    }
                }
            }

            //Custom Handles - need to be drawn in all events, not just in repaint!
            if (m_spawner.m_baseTerrainSettings.m_drawPreview && m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                //Handles for the tile selection "cursor" that controls which area we are previewing
                //Only display this handle if we have more than one terrain tile, otherwise it does not add any value
                if (m_spawner.m_worldCreationSettings.m_xTiles > 1)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.PositionHandle(m_spawner.m_worldDesignerUserBounds.center, Quaternion.identity);
                    //Lock the position on the y-axis
                    newPos = new Vector3(newPos.x, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight / 2f, newPos.z);
                    if (EditorGUI.EndChangeCheck())
                    {
                        //Snap the new position to the closest x / z tile coordinates
                        float halfWorldSize = m_spawner.m_worldCreationSettings.m_xTiles * m_spawner.m_worldCreationSettings.m_tileSize * 0.5f;
                        float halfTileSize = m_spawner.m_worldCreationSettings.m_tileSize / 2f;
                        //The world is centered around 0,0, and the cursor needs to be on the center of the terrain, need to calculate accordingly
                        int snappedX = Mathf.RoundToInt((newPos.x + halfWorldSize - halfTileSize) / m_spawner.m_worldCreationSettings.m_tileSize);
                        int snappedZ = Mathf.RoundToInt((newPos.z + halfWorldSize - halfTileSize) / m_spawner.m_worldCreationSettings.m_tileSize);
                        //Clamp to the world limits
                        snappedX = Mathf.Clamp(snappedX, 0, m_spawner.m_worldCreationSettings.m_xTiles - 1);
                        snappedZ = Mathf.Clamp(snappedZ, 0, m_spawner.m_worldCreationSettings.m_xTiles - 1);
                        //apply
                        m_spawner.m_worldDesignerPreviewTileX = snappedX;
                        m_spawner.m_worldDesignerPreviewTileZ = snappedZ;
                        SetBoundsToXZTile(ref m_spawner.m_worldDesignerUserBounds, m_spawner.m_worldDesignerPreviewTileX, m_spawner.m_worldDesignerPreviewTileZ);
                        m_lastTileCoordinateChangeTimestamp = GaiaUtils.GetUnixTimestamp();
                    }
                }
                //Sample Objects

                if (m_spawner.m_drawWorldDesignerSampleObjects)
                {
                    if (m_gaiaSettings.m_worldDesignerSampleObjectMesh1 != null)
                    {
                        m_gaiaSettings.m_worldDesignerSampleObjectsMaterial.SetPass(0);
                        Graphics.DrawMeshNow(m_gaiaSettings.m_worldDesignerSampleObjectMesh1, m_spawner.m_worldDesignerSampleObjectPos1, Quaternion.identity);
                        if (m_spawner.m_drawWorldDesignerHandles)
                        {
                            EditorGUI.BeginChangeCheck();
                            m_spawner.m_worldDesignerSampleObjectPos1 = Handles.PositionHandle(m_spawner.m_worldDesignerSampleObjectPos1, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                m_spawner.m_worldDesignerSampleObjectPos1 = m_spawner.GetHeightOnWorldDesignerPreview(m_spawner.m_worldDesignerSampleObjectPos1);
                            }
                        }
                    }
                    if (m_gaiaSettings.m_worldDesignerSampleObjectMesh2 != null)
                    {
                        m_gaiaSettings.m_worldDesignerSampleObjectsMaterial.SetPass(0);
                        Graphics.DrawMeshNow(m_gaiaSettings.m_worldDesignerSampleObjectMesh2, m_spawner.m_worldDesignerSampleObjectPos2, Quaternion.identity);
                        if (m_spawner.m_drawWorldDesignerHandles)
                        {
                            EditorGUI.BeginChangeCheck();
                            m_spawner.m_worldDesignerSampleObjectPos2 = Handles.PositionHandle(m_spawner.m_worldDesignerSampleObjectPos2, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                m_spawner.m_worldDesignerSampleObjectPos2 = m_spawner.GetHeightOnWorldDesignerPreview(m_spawner.m_worldDesignerSampleObjectPos2);
                            }
                        }
                    }
                    if (m_gaiaSettings.m_worldDesignerSampleObjectMesh3 != null)
                    {
                        m_gaiaSettings.m_worldDesignerSampleObjectsMaterial.SetPass(0);
                        Graphics.DrawMeshNow(m_gaiaSettings.m_worldDesignerSampleObjectMesh3, m_spawner.m_worldDesignerSampleObjectPos3, Quaternion.identity);
                        if (m_spawner.m_drawWorldDesignerHandles)
                        {
                            EditorGUI.BeginChangeCheck();
                            m_spawner.m_worldDesignerSampleObjectPos3 = Handles.PositionHandle(m_spawner.m_worldDesignerSampleObjectPos3, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                m_spawner.m_worldDesignerSampleObjectPos3 = m_spawner.GetHeightOnWorldDesignerPreview(m_spawner.m_worldDesignerSampleObjectPos3);
                            }
                        }
                    }
                }
           
                ///Buttons
                Handles.BeginGUI();
                float buttonwidth = 150;
                float buttonHeight = 25;
                float rightOffset = buttonwidth + 10;

                if ((stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain || m_spawner.m_worldMapStamperSettings.Count > 0) && m_spawner.m_worldCreationSettings.m_xTiles>1)
                {
                    if (m_spawner.m_worldDesignerUserBounds.center == stamper.m_worldDesignerPreviewBounds.center && stamper.m_useCustomPreviewBounds)
                    {
                        GUI.enabled = false;
                        if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight- 100, buttonwidth, buttonHeight), new GUIContent("Refresh Preview", "Refreshes the preview - Becomes available when you drag the preview cursor to a different area with the handles.")))
                        {

                        }
                        GUI.enabled = currentGUIState;
                    }
                    else
                    {
                        if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight - 100, buttonwidth, buttonHeight), new GUIContent("Refresh Preview", "Refreshes the preview and draws the stamp at the position of the preview cursor.")))
                        {
                            m_spawner.RefreshWorldDesignerStamps();
                        }
                    }
                }
                if (m_spawner.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.Worldmap)
                {
                    if (stamper.m_useCustomPreviewBounds)
                    {
                        if (m_spawner.m_worldMapStamperSettings.Count>0)
                        {
                            if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight - 135, buttonwidth, buttonHeight), new GUIContent("Render Full Preview", "Renders all stamps into the preview - this can take a few moments if you have a large world with a lot of stamps.")))
                            {
                                stamper.m_useCustomPreviewBounds = false;
                                m_spawner.RefreshWorldDesignerStamps();
                            }
                        }
                    }
                    else
                    {
                        if(m_spawner.m_worldCreationSettings.m_xTiles>1)
                        {
                            if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight - 135, buttonwidth, buttonHeight), new GUIContent("Back To Partial Preview", "Renders only a single terrain tile with all stamps that touching it - much quicker to render, but you can only view a smaller portion of the world at the same time.")))
                            {
                                stamper.m_useCustomPreviewBounds = true;
                                m_spawner.RefreshWorldDesignerStamps();
                            }
                        }
                    }
                    if (m_spawner.m_worldCreationSettings.m_xTiles > 1 && heightmapResExceeded)
                    {
                        if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight - 65, buttonwidth, buttonHeight), new GUIContent("Zoom In", "Zooms in on the selected Terrain Tile to preview it in full heightmap resolution.")))
                        {
                            ZoomIn();
                        }
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(scaledScreenWidth - rightOffset, scaledScreenHeight - 65, buttonwidth, buttonHeight), new GUIContent("Zoom Out", "Zooms out back to the full world map preview.")))
                    {
                        if (!m_spawner.m_baseTerrainSettings.m_alwaysFullPreview)
                        {
                            stamper.m_useCustomPreviewBounds = true;
                        }
                        ZoomOut();
                    }
                }
                Handles.EndGUI();

            }

            //Ruler
            if (m_spawner.m_drawWorldDesignerRulers && m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Camera sceneViewCam = SceneView.lastActiveSceneView.camera;
                float capSize = 0.02f;
                float previewRange = TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange;
                float distance =  previewRange / 10f;
                Handles.color = Color.white;
                float startX = m_spawner.transform.position.x - (previewRange / 2f) - distance;
                float endX = m_spawner.transform.position.x + (previewRange / 2f) + distance;
                float startZ = m_spawner.transform.position.z - (previewRange / 2f) - distance;
                float endZ = m_spawner.transform.position.z + (previewRange / 2f) + distance;
                Vector3 startPoint = new Vector3(startX, m_spawner.transform.position.y, startZ);
                Vector3 endPointX = new Vector3(endX, m_spawner.transform.position.y, startZ);
                Vector3 endPointZ = new Vector3(startX, m_spawner.transform.position.y, endZ);
                float capSizeX = capSize * Vector3.Distance(sceneViewCam.transform.position, endPointX);
                float capSizeZ = capSize * Vector3.Distance(sceneViewCam.transform.position, endPointZ);
                //Add half of the size as an offset to the position of the caps so they are drawn at the exact end of the line.
                Handles.ConeHandleCap(0, endPointX + new Vector3(capSizeX / 2f,0f,0f), Quaternion.LookRotation(Vector3.right), capSizeX, EventType.Repaint);
                Handles.ConeHandleCap(0, endPointZ + new Vector3(0f, 0f, capSizeZ / 2f), Quaternion.LookRotation(Vector3.forward), capSizeZ, EventType.Repaint);
                Handles.DrawLine(startPoint, endPointX);
                Handles.DrawLine(startPoint, endPointZ);

                //Smart segmentation depending on distance between camera and the axis line
                float camXDistance = Vector3.Distance(endPointX-startPoint, sceneViewCam.transform.position);
                float camZDistance = Vector3.Distance(endPointZ - startPoint, sceneViewCam.transform.position);
                int divisions = Mathf.RoundToInt(10 * previewRange / camXDistance);

                bool displayKM = false;
                string labelXAxis = "X (m)";
                string labelZAxis = "Z (m)";
                if (m_spawner.m_worldCreationSettings.m_xTiles * m_spawner.m_worldCreationSettings.m_tileSize > 10000)
                {
                    displayKM = true;
                    labelXAxis = "X (km)";
                    labelZAxis = "Z (km)";
                }

                Handles.Label(endPointX + new Vector3(0f, 0f, distance / 2f), labelXAxis, m_sceneViewGraphLabelStyle);
                Handles.Label(endPointZ + new Vector3(distance / 2f, 0f), labelZAxis, m_sceneViewGraphLabelStyle);

                for (float k = startX + distance; k <= endX; k += (previewRange / (float)divisions))
                {
                    Vector3 targetPosX = new Vector3(k, m_spawner.transform.position.y, startZ);
                    Handles.CircleHandleCap(0, targetPosX, Quaternion.LookRotation(Vector3.right), capSize * 0.25f * camXDistance, EventType.Repaint);
                    float negativeCharBonusX = targetPosX.x < 0 ? 1.0f : 0.0f;
                    //float offsetX = Mathf.Abs((negativeCharBonusX + (targetPosX.x) / 10f)) / 2f;
                    string textX = displayKM ? String.Format("{0:0.##}", (targetPosX.x / 1000f)) : Mathf.RoundToInt(targetPosX.x).ToString();
                    Handles.Label(targetPosX - new Vector3(0f, 0f, distance / 2f), textX, m_sceneViewGraphLabelStyle);
                }

                for (float k = startZ + distance; k <= endZ; k += (previewRange / (float)divisions))
                {
                    Vector3 targetPosZ= new Vector3(startX, m_spawner.transform.position.y, k);
                    Handles.CircleHandleCap(0, targetPosZ, Quaternion.LookRotation(Vector3.forward), capSize * 0.25f * camZDistance, EventType.Repaint);
                    float negativeCharBonusZ = targetPosZ.z < 0 ? 1.0f : 0.0f;
                    //float offsetZ = Mathf.Abs((negativeCharBonusZ + (targetPosZ.x) / 10f));
                    //float offsetZ += 0.05f * camDistanceToCenter;
                    string textZ = displayKM ? String.Format("{0:0.##}", (targetPosZ.z / 1000f)) : Mathf.RoundToInt(targetPosZ.z).ToString();
                    Handles.Label(targetPosZ - new Vector3(distance /2f + (0.075f * camZDistance) , 0f, 0f), textZ, m_sceneViewGraphLabelStyle);
                }
            }

            //Preview controls- need to be drawn last so they are drawn on top of everything in scene view

            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Handles.BeginGUI();
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                ///Preview controls
                Rect checkboxRect = new Rect(scaledScreenWidth - 25, scaledScreenHeight - 225, 200, EditorGUIUtility.singleLineHeight);
                Rect labelRect = new Rect(scaledScreenWidth - 225, scaledScreenHeight - 175, 200, EditorGUIUtility.singleLineHeight);
                Rect sliderRect = new Rect(scaledScreenWidth - 175, scaledScreenHeight - 175, 165, EditorGUIUtility.singleLineHeight);


                if (m_spawner.m_worldCreationSettings.m_xTiles > 1)
                {
                    Rect backgroundRect = new Rect(scaledScreenWidth - 230, scaledScreenHeight - 320, 225, 175);
                    EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
                    int oldX = m_spawner.m_worldDesignerPreviewTileX;
                    int oldZ = m_spawner.m_worldDesignerPreviewTileZ;
                    m_spawner.m_worldDesignerPreviewTileZ = EditorGUI.IntSlider(sliderRect, m_spawner.m_worldDesignerPreviewTileZ, 0, m_spawner.m_worldCreationSettings.m_xTiles - 1);
                    GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("PreviewTileZ"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                    labelRect.y -= EditorGUIUtility.singleLineHeight;
                    sliderRect.y -= EditorGUIUtility.singleLineHeight;
                    m_spawner.m_worldDesignerPreviewTileX = EditorGUI.IntSlider(sliderRect, m_spawner.m_worldDesignerPreviewTileX, 0, m_spawner.m_worldCreationSettings.m_xTiles - 1);
                    GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("PreviewTileX"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                    labelRect.y -= EditorGUIUtility.singleLineHeight;
                    sliderRect.y -= EditorGUIUtility.singleLineHeight;
                    SetBoundsToXZTile(ref m_spawner.m_worldDesignerUserBounds, m_spawner.m_worldDesignerPreviewTileX, m_spawner.m_worldDesignerPreviewTileZ);

                    //Repaint if tile coordinates were changed, otherwise scene view does not respond immediately
                    if (oldX != m_spawner.m_worldDesignerPreviewTileX || oldZ != m_spawner.m_worldDesignerPreviewTileZ)
                    {
                        m_lastTileCoordinateChangeTimestamp = GaiaUtils.GetUnixTimestamp();
                        SceneView.RepaintAll();
                    }

                    if (m_lastTileCoordinateChangeTimestamp + 1000 < GaiaUtils.GetUnixTimestamp())
                    {
                        Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                        if (stamper != null && stamper.m_useCustomPreviewBounds)
                        {
                            m_spawner.RefreshWorldDesignerStamps();
                        }
                        m_lastTileCoordinateChangeTimestamp = double.MaxValue;
                    }
                }
                else
                {
                    Rect backgroundRect = new Rect(scaledScreenWidth - 230, scaledScreenHeight - 320, 225, 120);
                    EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));

                }

                labelRect.y = checkboxRect.y;
                m_spawner.m_drawWorldDesignerRulers = EditorGUI.Toggle(checkboxRect, m_spawner.m_drawWorldDesignerRulers);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerRulers"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                checkboxRect.y -= EditorGUIUtility.singleLineHeight;
                labelRect.y -= EditorGUIUtility.singleLineHeight;

                if (!m_spawner.m_baseTerrainSettings.m_drawPreview || !m_spawner.m_drawWorldDesignerSampleObjects)
                {
                    GUI.enabled = false;
                }
                m_spawner.m_drawWorldDesignerHandles = EditorGUI.Toggle(checkboxRect, m_spawner.m_drawWorldDesignerHandles);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerHandles"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                checkboxRect.y -= EditorGUIUtility.singleLineHeight;
                labelRect.y -= EditorGUIUtility.singleLineHeight;
                if (!m_spawner.m_baseTerrainSettings.m_drawPreview)
                {
                    GUI.enabled = false;
                }
                else
                {
                    GUI.enabled = currentGUIState;
                }
                m_spawner.m_drawWorldDesignerSampleObjects = EditorGUI.Toggle(checkboxRect, m_spawner.m_drawWorldDesignerSampleObjects);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerSampleObjects"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                checkboxRect.y -= EditorGUIUtility.singleLineHeight;
                labelRect.y -= EditorGUIUtility.singleLineHeight;
                m_spawner.m_drawWorldDesignerDensityViz = EditorGUI.Toggle(checkboxRect, m_spawner.m_drawWorldDesignerDensityViz);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerDensityViz"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                checkboxRect.y -= EditorGUIUtility.singleLineHeight;
                labelRect.y -= EditorGUIUtility.singleLineHeight;
                if (!m_spawner.m_baseTerrainSettings.m_drawPreview)
                {
                    GUI.enabled = false;
                }
                else
                {
                    GUI.enabled = currentGUIState;
                }
                bool m_oldFullPreview = m_spawner.m_baseTerrainSettings.m_alwaysFullPreview;
                m_spawner.m_baseTerrainSettings.m_alwaysFullPreview = EditorGUI.Toggle(checkboxRect, m_spawner.m_baseTerrainSettings.m_alwaysFullPreview);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerAlwaysFullPreview"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                if (m_oldFullPreview != m_spawner.m_baseTerrainSettings.m_alwaysFullPreview)
                {
                    if (m_spawner.m_baseTerrainSettings.m_alwaysFullPreview)
                    { 
                        Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                        if (stamper != null && stamper.m_useCustomPreviewBounds)
                        {
                            stamper.m_useCustomPreviewBounds = false;
                            m_spawner.RefreshWorldDesignerStamps();
                        }
                    }

                }
                GUI.enabled = currentGUIState;
                checkboxRect.y -= EditorGUIUtility.singleLineHeight;
                labelRect.y -= EditorGUIUtility.singleLineHeight;
                m_spawner.m_baseTerrainSettings.m_drawPreview = EditorGUI.Toggle(checkboxRect, m_spawner.m_baseTerrainSettings.m_drawPreview);
                GaiaUtils.GUITextWithShadow(labelRect, m_editorUtils.GetContent("DrawWorldDesignerPreview"), m_sceneViewToggleStyle, m_sceneViewShadowStyle);
                Handles.EndGUI();
            }
        }

        private void SetBoundsToXZTile(ref Bounds bounds, int tileX, int tileZ)
        {
            float halfWorldSize = m_spawner.m_worldCreationSettings.m_xTiles * m_spawner.m_worldCreationSettings.m_tileSize * 0.5f;
            float halfTileSize = m_spawner.m_worldCreationSettings.m_tileSize / 2f;
            Vector3 worldOffset = new Vector3(halfWorldSize, 0f, halfWorldSize);
            Vector3 snappedPos = new Vector3((tileX * m_spawner.m_worldCreationSettings.m_tileSize) + halfTileSize, m_spawner.m_worldCreationSettings.m_tileHeight / 2f, (tileZ * m_spawner.m_worldCreationSettings.m_tileSize) + halfTileSize);
            snappedPos -= worldOffset;
            bounds.center = snappedPos;
            bounds.size = new Vector3(m_spawner.m_worldCreationSettings.m_tileSize, m_spawner.m_worldCreationSettings.m_tileHeight, m_spawner.m_worldCreationSettings.m_tileSize);
        }

        private void ZoomIn()
        {
            Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
            m_spawner.m_worldDesignerPreviewMode = WorldDesignerPreviewMode.SingleTerrain;
            m_spawner.transform.position = new Vector3(m_spawner.m_worldDesignerUserBounds.center.x, 0f, m_spawner.m_worldDesignerUserBounds.center.z);
            stamper.transform.position = m_spawner.transform.position;
            m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
            stamper.m_stampDirty = true;
            Quaternion quaternion = SceneView.lastActiveSceneView.camera.transform.rotation;
            SceneView.lastActiveSceneView.LookAt(m_spawner.transform.position, quaternion, m_spawner.m_worldCreationSettings.m_tileSize*0.5f);
        }

        private void ZoomOut()
        {
            Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
            m_spawner.m_worldDesignerPreviewMode = WorldDesignerPreviewMode.Worldmap;
            m_spawner.transform.position = Vector3.zero;
            stamper.transform.position = m_spawner.transform.position;
            m_spawner.UpdateBaseTerrainStamper();
            m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
            stamper.m_stampDirty = true;
            Quaternion quaternion = SceneView.lastActiveSceneView.camera.transform.rotation;
            SceneView.lastActiveSceneView.size = m_spawner.m_worldCreationSettings.m_tileSize * m_spawner.m_worldCreationSettings.m_xTiles /2f;
            float worldSize = m_spawner.m_worldCreationSettings.m_xTiles * m_spawner.m_worldCreationSettings.m_tileSize;
            SceneView.lastActiveSceneView.cameraSettings.farClip = worldSize * 2f;
        }

        public void OnValidate()
        {
            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                if (m_spawner.GetComponent<Transform>().hideFlags != HideFlags.HideInInspector)
                {
                    m_spawner.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;
                    m_spawner.transform.position = Vector3.zero;
                    m_spawner.transform.rotation = Quaternion.identity;
                    m_spawner.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Draw the UI
        /// </summary>
        public override void OnInspectorGUI()
        {

            if (m_spawner.m_ExportRunning)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }

            if (m_generateSeedCheckboxStyle == null)
            {
                m_generateSeedCheckboxStyle = new GUIStyle(GUI.skin.toggle);
                m_generateSeedCheckboxStyle.margin = new RectOffset(0, 0, 3, 0);
            }

            if (m_spawnRulesBorder == null || m_spawnRulesBorder.normal.background == null)
            {
                m_spawnRulesBorder = new GUIStyle(EditorStyles.helpBox);
                m_spawnRulesBorder.margin = new RectOffset(0, 0, 0, 0);
                m_spawnRulesBorder.padding = new RectOffset(3, 3, 3, 3);
            }

            if (m_spawnRulesHeader == null || m_singleSpawnRuleHeader == null || m_spawnRulesHeader.normal.background == null || m_singleSpawnRuleHeader.normal.background == null || m_errorHeader == null || m_errorHeader.normal.background == null)
            {
                m_spawnRulesHeader = new GUIStyle();
                m_spawnRulesHeader.overflow = new RectOffset(2, 2, 2, 2);
                m_singleSpawnRuleHeader = new GUIStyle(m_spawnRulesHeader);
                m_errorHeader = new GUIStyle(m_spawnRulesHeader);

                // Setup colors for Unity Pro
                if (EditorGUIUtility.isProSkin)
                {
                    m_spawnRulesHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("2d2d2dff"), m_tempTextureList);
                    m_singleSpawnRuleHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("2d2d2dff"), m_tempTextureList);
                    m_errorHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("804241ff"), m_tempTextureList);
                }
                // or Unity Personal
                else
                {
                    m_spawnRulesHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("b6b6b6ff"), m_tempTextureList);
                    m_singleSpawnRuleHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("c2c2c2ff"), m_tempTextureList);
                    m_errorHeader.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("C46564ff"), m_tempTextureList);

                }

            }

            if (m_smallButtonStyle == null)
            {
                m_smallButtonStyle = new GUIStyle(GUI.skin.button);
                m_smallButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                m_smallButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            }

            if (m_visualizeAllButtonStyle == null)
            {
                m_visualizeAllButtonStyle = new GUIStyle(m_smallButtonStyle);
                m_visualizeAllButtonStyle.margin = new RectOffset(5, 5, 0, 0);
            }

            if (m_operationStyle == null || m_operationStyle.normal.background == null)
            {
                m_operationStyle = new GUIStyle();
                m_operationStyle.overflow = new RectOffset(2, 2, 2, 2);

                // Setup colors for Unity Pro
                if (EditorGUIUtility.isProSkin)
                {
                    m_operationStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("2d2d2dff"), m_tempTextureList);
                }
                // or Unity Personal
                else
                {
                    m_operationStyle.normal.background = GaiaUtils.GetBGTexture(GaiaUtils.GetColorFromHTML("a2a2a2ff"), m_tempTextureList);
                }
            }

            long currentTimeStamp = GaiaUtils.GetUnixTimestamp();

            //Disable Loading settings highlighting again after 3 seconds
            if (m_spawner.m_highlightLoadingSettings && m_spawner.m_highlightLoadingSettingsStartedTimeStamp + 2000 < currentTimeStamp)
            {
                m_spawner.m_highlightLoadingSettings = false;
            }

            m_spawner.m_settings.m_spawnRange = Mathf.Clamp(m_spawner.m_settings.m_spawnRange, 1f, m_spawner.GetMaxSpawnRange());

            //Handle terrain auto-loading
            m_spawner.UpdateAutoLoadRange();
            //Reset Resource Preview
            m_drawResourcePreviewRuleId = -99;
            //Reset reorderable masks lists in case the spawner was reset etc.
            if (m_spawner.m_settings.m_spawnerRules.Count == 0 && m_reorderableRuleMasksLists.Count() > 0)
            {
                m_reorderableRuleMasksLists = new ReorderableList[0];
            }

            //Get our spawner
            m_spawner = (Spawner)target;

            //Init editor utils
            m_editorUtils.Initialize();
            serializedObject.Update();

            if (m_activatePreviewRequested && (m_activatePreviewTimeStamp + m_gaiaSettings.m_stamperAutoHidePreviewMilliseconds < currentTimeStamp))
            {
                m_activatePreviewRequested = false;
                m_spawner.m_drawPreview = true;
                //force repaint
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }

            //Check if sea level changed
            if (m_spawner.m_seaLevel != SessionManager.GetSeaLevel())
            {
                //Dirty the preview to force it to be refreshed according to the new sea level
                m_spawner.m_spawnPreviewDirty = true;
                m_spawner.m_seaLevel = SessionManager.GetSeaLevel();
                m_spawner.SetWorldBiomeMasksDirty();
            }

            //Disable if spawning
            if (m_spawner.m_spawnProgress > 0f)
            {
                GUI.enabled = false;
            }

            EditorGUI.BeginChangeCheck();

            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                m_spawner.m_terrainGeneratorPanelUnfolded = m_editorUtils.Panel("GenerateTerrain", DrawGenerateTerrainPanel, true);
            }
            else
            {
                //Regular Spawner
                m_editorUtils.Panel("SpawnSettings", DrawSpawnSettings, false, true, true);
                GUILayout.Space(10);
                DrawSpawnRulesPanel("SpawnRules", DrawRegularSpawnRules, ref m_spawner.m_spawnRuleRegularToggleAllState);
                GUILayout.Space(10);
                if (m_spawner.m_highlightLoadingSettings)
                {
                    m_editorUtils.SetPanelStatus(DrawAdvanced, true, false);
                }
                m_editorUtils.Panel("Advanced", DrawAdvanced, false);
                m_editorUtils.Panel("ClearSpawns", DrawClearControls, false);
#if CTS_PRESENT
                m_editorUtils.Panel("CTS", DrawCTS, false);
#endif
            }
            //m_editorUtils.Panel("Statistics", DrawStatistics, false);
            //GUILayout.Space(10);


            //GUILayout.BeginVertical();
            //GUILayout.Space(20);
            //bool showGizmos = EditorGUILayout.Toggle(GetLabel("Show Gizmos"), m_spawner.m_showGizmos);
            //bool showStatistics = m_spawner.m_showStatistics = EditorGUILayout.Toggle(GetLabel("Show Statistics"), m_spawner.m_showStatistics);
            //bool showTerrainHelper = m_spawner.m_showTerrainHelper = EditorGUILayout.Toggle(GetLabel("Show Terrain Helper"), m_spawner.m_showTerrainHelper);
            //GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                m_changesMadeSinceLastSave = true;
                Undo.RecordObject(m_spawner, "Made changes");
                m_spawner.m_spawnPreviewDirty = true;
                if(m_spawner.m_drawPreview && m_spawner.m_settings.m_spawnerRules[m_spawner.m_previewRuleIds[0]].m_resourceType == SpawnerResourceType.TerrainModifierStamp)
                {
                    m_spawner.UpdateTerrainModifierStamperFromSpawnRule(m_spawner.GetOrCreatePreviewStamper(true), m_spawner.m_settings.m_spawnerRules[m_spawner.m_previewRuleIds[0]]);
                }

                m_spawner.SetWorldBiomeMasksDirty();
                //m_spawner.m_showGizmos = showGizmos;
                //m_spawner.m_showStatistics = showStatistics;
                //m_spawner.m_showTerrainHelper = showTerrainHelper;


                if (m_spawner.m_imageMask != null)
                {
                    GaiaUtils.MakeTextureReadable(m_spawner.m_imageMask);
                    GaiaUtils.MakeTextureUncompressed(m_spawner.m_imageMask);
                }
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_spawner);
                EditorUtility.SetDirty(m_spawner.m_settings);


            }

            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                DrawSpawnControls();
            }
            else
            {
                m_spawner.m_worldDesignerPreviewSettingsUnfolded = m_editorUtils.Panel("PreviewSettings", DrawPreviewSettings, m_spawner.m_worldDesignerPreviewSettingsUnfolded);
                m_editorUtils.Panel("Advanced", DrawAdvanced, false);
                DrawWorldDesignerSpawnControls();
            }
        }

        private void DrawPreviewSettings(bool showHelp)
        {
            EditorGUI.BeginChangeCheck();
            m_spawner.m_baseTerrainSettings.m_drawPreview = m_editorUtils.Toggle("DrawWorldDesignerPreview", m_spawner.m_baseTerrainSettings.m_drawPreview, showHelp);
            m_spawner.m_drawWorldDesignerDensityViz = m_editorUtils.Toggle("DrawWorldDesignerDensityViz", m_spawner.m_drawWorldDesignerDensityViz, showHelp);
            m_spawner.m_drawWorldDesignerSampleObjects = m_editorUtils.Toggle("DrawWorldDesignerSampleObjects", m_spawner.m_drawWorldDesignerSampleObjects, showHelp);
            m_spawner.m_drawWorldDesignerHandles = m_editorUtils.Toggle("DrawWorldDesignerHandles", m_spawner.m_drawWorldDesignerHandles, showHelp);
            m_spawner.m_drawWorldDesignerRulers= m_editorUtils.Toggle("DrawWorldDesignerRulers", m_spawner.m_drawWorldDesignerRulers, showHelp);

            if (EditorGUI.EndChangeCheck())
            {
                //Repaint the scene view on change so it reacts immediately.
                SceneView.RepaintAll();
            }

            //Determine if the additional preview modes are needed, and display information about the preview state
            bool heightmapResExceeded = false;
            bool stampsExceeded = false;

            m_spawner.CheckIfFullPreview(ref heightmapResExceeded, ref stampsExceeded);

            //If everything is OK, just display a "All OK" message, otherwise display the advanced preview settings
            if (!heightmapResExceeded && !stampsExceeded)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("WorldDesignerPreviewOKInfo"), MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("WorldDesignerPartialPreviewInfo"), MessageType.Warning);

                Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                stamper.m_worldDesignerPreviewMode = m_spawner.m_worldDesignerPreviewMode;
                stamper.m_worldDesignerPreviewTiles = m_spawner.m_worldCreationSettings.m_xTiles;

                int oldX = m_spawner.m_worldDesignerPreviewTileX;
                int oldZ = m_spawner.m_worldDesignerPreviewTileZ;

                m_spawner.m_worldDesignerPreviewTileX = m_editorUtils.IntSlider("PreviewTileX", m_spawner.m_worldDesignerPreviewTileX, 0, m_spawner.m_worldCreationSettings.m_xTiles-1, showHelp);
                m_spawner.m_worldDesignerPreviewTileZ = m_editorUtils.IntSlider("PreviewTileZ", m_spawner.m_worldDesignerPreviewTileZ, 0, m_spawner.m_worldCreationSettings.m_xTiles-1, showHelp);

                SetBoundsToXZTile(ref m_spawner.m_worldDesignerUserBounds, m_spawner.m_worldDesignerPreviewTileX, m_spawner.m_worldDesignerPreviewTileZ);
               
                //Repaint if tile coordinates were changed, otherwise scene view does not respond immediately
                if (oldX != m_spawner.m_worldDesignerPreviewTileX || oldZ != m_spawner.m_worldDesignerPreviewTileZ)
                {
                    m_lastTileCoordinateChangeTimestamp = GaiaUtils.GetUnixTimestamp();
                    SceneView.RepaintAll();
                }

                bool currentGUIState = GUI.enabled;

                if (m_spawner.m_worldDesignerUserBounds.center == stamper.m_worldDesignerPreviewBounds.center)
                {
                    GUI.enabled = false;
                }

                if (m_editorUtils.Button("RefreshPreview"))
                {
                    m_spawner.RefreshWorldDesignerStamps();
                }

                GUI.enabled = currentGUIState;
            }
        }


        private void DrawGenerateTerrainPanel(bool helpEnabled)
        {
            WorldCreationSettings wcs = m_spawner.m_worldCreationSettings;
            int world_xDimension = wcs.m_xTiles * wcs.m_tileSize;
            int world_zDimension = wcs.m_zTiles * wcs.m_tileSize;
            int numberOfTerrains = wcs.m_xTiles * wcs.m_zTiles;

            string worldXText = String.Format("{0:0} m", world_xDimension);
            string worldZText = String.Format("{0:0} m", world_zDimension);
            if (world_xDimension > 1000 || world_zDimension > 1000)
            {
                worldXText = String.Format("{0:0.00} km", world_xDimension / 1000f);
                worldZText = String.Format("{0:0.00} km", world_zDimension / 1000f);
            }

            string worldSizeInfo = "";
            if (EditorGUIUtility.currentViewWidth > 350)
            {
                //Display only short size info when running out of space on the UI
                if (EditorGUIUtility.currentViewWidth > 400)
                {
                    worldSizeInfo = String.Format(" - {0} size, Terrains: {2}", worldXText, worldZText, numberOfTerrains);
                }
                else
                {
                    worldSizeInfo = String.Format(" - {0}, {2} Ter.", worldXText, worldZText, numberOfTerrains);
                }
            }

            m_spawner.m_sizeAndResolutionUnfolded = DrawWorldDesignerPanel("WorldSizeAndResolution", DrawWorldSizeAndBiome, ref m_showStampBaseHelp, m_spawner.m_sizeAndResolutionUnfolded, worldSizeInfo);
            GUILayout.Space(5);
            m_spawner.m_createBaseTerrainUnfolded = DrawWorldDesignerPanel("CreateBaseTerrain", DrawWorldShapeSettings, ref m_showStampBaseHelp, m_spawner.m_createBaseTerrainUnfolded);
            bool currentGUIState = GUI.enabled;
            GUILayout.Space(5);
            m_spawner.m_spawnStampsPanelUnfolded = DrawSpawnRulesPanel("SpawnStamps", DrawWorldDetail, ref m_spawner.m_spawnRuleStampsToggleAllState, m_spawner.m_spawnStampsPanelUnfolded);
            //GUILayout.Space(5);
            //m_spawner.m_exportTerrainUnfolded = DrawWorldDesignerPanel("ExportTerrain", DrawExportTerrain, ref m_showExportTerrainHelp, m_spawner.m_exportTerrainUnfolded);
            if (m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask).Count() > 0)
            {
                GUILayout.Space(5);
                m_spawner.m_worldBiomeMasksUnfolded = DrawSpawnRulesPanel("DefineWorldBiomeMasks", DrawBiomeMaskSpawnRules, ref m_spawner.m_spawnRuleBiomeMasksToggleAllState, m_spawner.m_worldBiomeMasksUnfolded);
            }
            GUILayout.Space(3);
            GUI.enabled = currentGUIState;
        }

        private void DrawWorldSizeAndBiome(bool helpEnabled)
        {
#region WORLD SIZE
            bool currentGUIState = GUI.enabled;
            BoundsDouble bounds = new BoundsDouble();
            TerrainHelper.GetTerrainBounds(ref bounds);
            if (bounds.size.x <= 0)
            {
                m_spawner.m_useExistingTerrainForWorldMapExport = false;
            }
            WorldCreationSettings wcs = m_spawner.m_worldCreationSettings;
            float oldTileHeight = wcs.m_tileHeight;
            float oldWorldSize = wcs.m_tileSize * wcs.m_xTiles;
            int oldXTiles = wcs.m_xTiles;

            bool oldCreateScenes = wcs.m_createInScene;
            bool oldAutoUnloadScenes = wcs.m_autoUnloadScenes;
            bool oldFloatingPointFix = wcs.m_applyFloatingPointFix;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EnvironmentSizePreset oldSizePreset = wcs.m_targetSizePreset;
            wcs.m_targetSizePreset = (GaiaConstants.EnvironmentSizePreset)m_editorUtils.EnumPopup("ExportTerrainWorldSize", wcs.m_targetSizePreset);

            //if the user just changed to the "Custom" preset, we open the advanced settings so that they become visible
            if (oldSizePreset != wcs.m_targetSizePreset && wcs.m_targetSizePreset == EnvironmentSizePreset.Custom)
            {
                m_spawner.m_worldSizeAdvancedUnfolded = true;
            }

            if (!m_spawner.m_worldSizeAdvancedUnfolded)
            {
                if (m_editorUtils.Button("PlusButtonWorldSize", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    m_spawner.m_worldSizeAdvancedUnfolded = true;
                }
            }
            else
            {
                if (m_editorUtils.Button("MinusButtonWorldSize", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    m_spawner.m_worldSizeAdvancedUnfolded = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            m_editorUtils.InlineHelp("ExportTerrainWorldSize", helpEnabled);

            // m_spawner.m_worldSizeAdvancedUnfolded = m_editorUtils.Foldout("ExportTerrainAdvanced", m_spawner.m_worldSizeAdvancedUnfolded, helpEnabled);
            if (m_spawner.m_worldSizeAdvancedUnfolded)
            {
                EditorGUI.indentLevel++;
                m_spawner.m_worldSizeAdvancedUnfolded = true;
                wcs.m_xTiles = m_editorUtils.DelayedIntField("ExportTerrainXTiles", wcs.m_xTiles, helpEnabled);

                //only quadratic worlds supported atm
                wcs.m_zTiles = wcs.m_xTiles;
                //wcs.m_zTiles = m_editorUtils.IntField("ExportTerrainZTiles", wcs.m_zTiles, helpEnabled);
                if (m_spawner.m_worldCreationSettings.m_xTiles != m_spawner.m_worldCreationSettings.m_zTiles)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportTerrainNotSquareWarning"), MessageType.Warning);
                }
                m_spawner.m_worldTileSize = (GaiaConstants.EnvironmentSize)m_editorUtils.EnumPopup("ExportTerrainTileSize", m_spawner.m_worldTileSize, helpEnabled);
                if (m_spawner.m_worldTileSize == GaiaConstants.EnvironmentSize.Is4096MetersSq ||
                  m_spawner.m_worldTileSize == GaiaConstants.EnvironmentSize.Is8192MetersSq ||
                  m_spawner.m_worldTileSize == GaiaConstants.EnvironmentSize.Is16384MetersSq)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("LargeTerrainWarning"), MessageType.Error);
                }

                wcs.m_tileSize = GaiaUtils.EnvironmentSizeToInt(m_spawner.m_worldTileSize);

                float oldHeight = wcs.m_tileHeight;

                wcs.m_tileHeight = m_editorUtils.DelayedFloatField("ExportTerrainTileHeight", wcs.m_tileHeight, helpEnabled);
                if (oldHeight != wcs.m_tileHeight)
                {
                    wcs.m_targetSizePreset = EnvironmentSizePreset.Custom;
                }

#if !GAIA_PRO_PRESENT
                GUI.enabled = false;
#endif

                if (oldXTiles<3 && wcs.m_xTiles>=3)
                {
                    //Activate terrain loading when 3 tiles or more are selected
                    wcs.m_createInScene = true;
                    wcs.m_autoUnloadScenes = true;
                    wcs.m_applyFloatingPointFix = true;
                    m_showAutoStreamSettingsBox = true;
                }

                if (oldXTiles >= 3 && wcs.m_xTiles < 3)
                {
                    //Deactivate terrain loading when less than 3 tiles are selected
                    wcs.m_createInScene = false;
                    wcs.m_autoUnloadScenes = false;
                    wcs.m_applyFloatingPointFix = false;
                    m_showAutoStreamSettingsBox = false;
                }
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        wcs.m_createInScene = m_editorUtils.Toggle("ExportTerrainCreateInScene", wcs.m_createInScene, helpEnabled);
                        wcs.m_autoUnloadScenes = m_editorUtils.Toggle("ExportTerrainUnloadScenes", wcs.m_autoUnloadScenes, helpEnabled);
                        wcs.m_applyFloatingPointFix = m_editorUtils.Toggle("ExportTerrainFloatingPointFix", wcs.m_applyFloatingPointFix, helpEnabled);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (m_showAutoStreamSettingsBox)
                        {
                            EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("AutoStreamSettings"), MessageType.Info);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
#if !GAIA_PRO_PRESENT
                wcs.m_createInScene = false;
                wcs.m_autoUnloadScenes = false;
                wcs.m_applyFloatingPointFix = false;
                GUI.enabled = currentGUIState;
#endif

                EditorGUI.indentLevel--;
            }

            //Set the world tile size if the user chose one of the presets
            if (oldSizePreset != wcs.m_targetSizePreset)
            {
                if (wcs.m_targetSizePreset != EnvironmentSizePreset.Custom)
                {
                    switch (wcs.m_targetSizePreset)
                    {
                        case GaiaConstants.EnvironmentSizePreset.Micro:
                            m_spawner.m_worldTileSize = GaiaConstants.EnvironmentSize.Is128MetersSq;
                            break;
                        case GaiaConstants.EnvironmentSizePreset.Tiny:
                            m_spawner.m_worldTileSize = GaiaConstants.EnvironmentSize.Is256MetersSq;
                            break;
                        case GaiaConstants.EnvironmentSizePreset.Small:
                            m_spawner.m_worldTileSize = GaiaConstants.EnvironmentSize.Is512MetersSq;
                            break;
                        case GaiaConstants.EnvironmentSizePreset.Medium:
                            m_spawner.m_worldTileSize = GaiaConstants.EnvironmentSize.Is1024MetersSq;
                            break;
                        case GaiaConstants.EnvironmentSizePreset.Large:
                            m_spawner.m_worldTileSize = GaiaConstants.EnvironmentSize.Is2048MetersSq;
                            break;
                    }

                    wcs.m_xTiles = 1;
                    wcs.m_zTiles = 1;
                    wcs.m_tileSize = GaiaUtils.EnvironmentSizeToInt(m_spawner.m_worldTileSize);
                    wcs.m_tileHeight = Mathf.Clamp(wcs.m_tileSize, 1, 4096);
                    wcs.m_createInScene = false;
                    wcs.m_autoUnloadScenes = false;
                    wcs.m_applyFloatingPointFix = false;
                }

                //adjust the preview range to the new settings
                m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
            }
            else
            {
                //the preset was not changed - check then if the user made a change that would switch the dropdown to one of the presets or the customs setting

                if ((wcs.m_xTiles > 1 ||
                    wcs.m_zTiles > 1 ||
                    wcs.m_tileSize >= 4096 ||
                    wcs.m_autoUnloadScenes ||
                    wcs.m_tileSize != wcs.m_tileHeight ||
                    wcs.m_createInScene ||
                    wcs.m_applyFloatingPointFix)
                    )
                {
                    if (wcs.m_targetSizePreset != EnvironmentSizePreset.Custom)
                    {
                        //out of the ordinary - this is custom
                        wcs.m_targetSizePreset = EnvironmentSizePreset.Custom;

                        //adjust the preview range to the new settings
                        m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();

                    }
                }
                else
                {
                    //not custom, this is one of the default settings:
                    switch (wcs.m_tileSize)
                    {
                        case 128:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Micro;
                            break;
                        case 256:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Tiny;
                            break;
                        case 512:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Small;
                            break;
                        case 1024:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Medium;
                            break;
                        case 2048:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Large;
                            break;
                        default:
                            wcs.m_targetSizePreset = EnvironmentSizePreset.Custom;
                            break;
                    }
                }
            }

            if (oldWorldSize != wcs.m_xTiles * wcs.m_tileSize)
            {
                m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
                m_spawner.FocusSceneViewOnWorldDesignerPreview();
                Tools.hidden = true;
            }

#endregion

#region QUALITY

            EditorGUILayout.BeginHorizontal();
            EnvironmentTarget oldQualityPreset = wcs.m_qualityPreset;
            wcs.m_qualityPreset = (EnvironmentTarget)m_editorUtils.EnumPopup("ExportTerrainQuality", wcs.m_qualityPreset);
            switch (wcs.m_qualityPreset)
            {
                case EnvironmentTarget.UltraLight:
                    m_gaiaSettings.m_currentDefaults = m_gaiaSettings.m_ultraLightDefaults;
                    m_gaiaSettings.m_currentWaterPrefabName = m_gaiaSettings.m_waterMobilePrefabName;
                    break;
                case EnvironmentTarget.MobileAndVR:
                    m_gaiaSettings.m_currentDefaults = m_gaiaSettings.m_mobileDefaults;
                    m_gaiaSettings.m_currentWaterPrefabName = m_gaiaSettings.m_waterMobilePrefabName;
                    break;
                case EnvironmentTarget.Desktop:
                    m_gaiaSettings.m_currentDefaults = m_gaiaSettings.m_desktopDefaults;
                    m_gaiaSettings.m_currentWaterPrefabName = m_gaiaSettings.m_waterPrefabName;
                    break;
                case EnvironmentTarget.PowerfulDesktop:
                    m_gaiaSettings.m_currentDefaults = m_gaiaSettings.m_powerDesktopDefaults;
                    m_gaiaSettings.m_currentWaterPrefabName = m_gaiaSettings.m_waterPrefabName;
                    break;
            }
            wcs.m_gaiaDefaults = m_gaiaSettings.m_currentDefaults;

            //if the user just switched to the custom preset, we unfold the advanced settings automatically
            if (oldQualityPreset != wcs.m_qualityPreset && wcs.m_qualityPreset == EnvironmentTarget.Custom)
            {
                m_spawner.m_qualityPresetAdvancedUnfolded = true;
            }

            m_gaiaSettings.m_currentDefaults.m_terrainSize = wcs.m_tileSize;

            GaiaUtils.SetSettingsForEnvironment(m_gaiaSettings, wcs.m_qualityPreset);

            if (!m_spawner.m_qualityPresetAdvancedUnfolded)
            {
                if (m_editorUtils.Button("PlusButtonQuality", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    m_spawner.m_qualityPresetAdvancedUnfolded = true;
                }
            }
            else
            {
                if (m_editorUtils.Button("MinusButtonQuality", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    m_spawner.m_qualityPresetAdvancedUnfolded = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            m_editorUtils.InlineHelp("ExportTerrainQuality", helpEnabled);

            if (m_spawner.m_qualityPresetAdvancedUnfolded)
            {
                EditorGUI.indentLevel++;
                m_spawner.m_qualityPresetAdvancedUnfolded = true;
                EditorGUI.BeginChangeCheck();
                m_gaiaSettings.m_currentDefaults.m_heightmapResolution = (int)(HeightmapResolution)m_editorUtils.EnumPopup("ExportTerrainHeightmapResolution", (HeightmapResolution)m_gaiaSettings.m_currentDefaults.m_heightmapResolution, helpEnabled);
                m_gaiaSettings.m_currentDefaults.m_controlTextureResolution = (int)(TerrainTextureResolution)m_editorUtils.EnumPopup("ExportTerrainControlTextureResolution", (TerrainTextureResolution)m_gaiaSettings.m_currentDefaults.m_controlTextureResolution, helpEnabled);
                m_gaiaSettings.m_currentDefaults.m_baseMapSize = (int)(TerrainTextureResolution)m_editorUtils.EnumPopup("ExportTerrainBaseMapResolution", (TerrainTextureResolution)m_gaiaSettings.m_currentDefaults.m_baseMapSize, helpEnabled);
                m_gaiaSettings.m_currentDefaults.m_detailResolutionPerPatch = m_editorUtils.IntField("ExportTerrainDetailResolutionPerPatch", m_gaiaSettings.m_currentDefaults.m_detailResolutionPerPatch, helpEnabled);
                m_gaiaSettings.m_currentDefaults.m_detailResolution = m_editorUtils.IntField("ExportTerrainDetailResolution", m_gaiaSettings.m_currentDefaults.m_detailResolution, helpEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    wcs.m_qualityPreset = EnvironmentTarget.Custom;
                }
                EditorGUI.indentLevel--;
            }

            float newWorldSize = wcs.m_tileSize * wcs.m_xTiles;

            if (m_spawner.m_lastExportedWorldSize != newWorldSize || m_spawner.m_lastExportedHeightMapResolution != wcs.m_gaiaDefaults.m_heightmapResolution)
            {
                m_spawner.m_useExistingTerrainForWorldMapExport = false;
            }
            GUI.enabled = currentGUIState;

            if (oldWorldSize != newWorldSize)
            {
                bool clearAllowed = false;
                if (m_spawner.m_worldMapStamperSettings.Count > 0)
                {

                    if (!m_spawner.m_worldDesignerClearStampsWarningShown)
                    {
                        if (EditorUtility.DisplayDialog("World Size Changed", "You just changed the world size, the current stamps will probably not look good / correct on the changed size. Do you want Gaia to clear all stamps and adjust the terrain generator settings for the new world size? You can then spawn new stamps more fitting for the new world size.", "Yes, clear stamps", "No, keep the existing stamps"))
                        {
                            clearAllowed = true;
                        }
                        m_spawner.m_worldDesignerClearStampsWarningShown = true;
                    }
                }
                else
                {
                    clearAllowed = true;
                }

                //Adjust the settings proportionally to the new terrain tile size / height
                float heightFactor = wcs.m_tileHeight / oldTileHeight;
                SessionManager.SetSeaLevel(SessionManager.GetSeaLevel() * heightFactor);

                if (clearAllowed)
                {
                    m_spawner.m_worldDesignerPreviewTileX = Mathf.FloorToInt(m_spawner.m_worldCreationSettings.m_xTiles / 2f);
                    m_spawner.m_worldDesignerPreviewTileZ = m_spawner.m_worldDesignerPreviewTileX;
                    SetBoundsToXZTile(ref m_spawner.m_worldDesignerUserBounds, m_spawner.m_worldDesignerPreviewTileX, m_spawner.m_worldDesignerPreviewTileZ);
                    m_spawner.ClearStampDistributions();
                    m_spawner.SetRecommendedStampSizes();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Do not trigger a refresh of the preview just because one of these flags have been changed.
                if (oldCreateScenes == wcs.m_createInScene &&
                oldAutoUnloadScenes == wcs.m_autoUnloadScenes &&
                oldFloatingPointFix == wcs.m_applyFloatingPointFix)
                { 
                    m_spawner.UpdateWorldDesignerPreviewSizeAndResolution();
                    m_spawner.UpdateBaseTerrainStamper();
                }
            }
#endregion

#region BiomePreset
            int lastBiomePresetSelection = m_spawner.m_worldDesignerBiomePresetSelection;
            if (m_spawner.m_worldDesignerBiomePresetSelection == int.MinValue)
            {
                m_spawner.m_worldDesignerBiomePresetSelection = 0;
            }
            EditorGUILayout.BeginHorizontal();
            {
                //Workaround for the "independent" biome preset label
                GUILayout.Space(-0.5f);
                m_editorUtils.Label("BiomePreset", GUILayout.Width(EditorGUIUtility.labelWidth));
                m_spawner.m_worldDesignerBiomePresetSelection = EditorGUILayout.IntPopup(m_spawner.m_worldDesignerBiomePresetSelection, m_allBiomePresets.Select(x => x.name).ToArray(), m_allBiomePresets.Select(x => x.ID).ToArray());

                if (lastBiomePresetSelection != m_spawner.m_worldDesignerBiomePresetSelection)
                {
                    AddBiomeSpawnersForSelectedPreset();
                    //re - create the reorderable list with the new contents
                    CreateBiomePresetList();

                }

                if (m_spawner.m_worldDesignerBiomePresetSelection == -999 && lastBiomePresetSelection != -999)
                {
                    //user just switched to "Custom", foldout the extended options
                    m_spawner.m_foldoutSpawnerSettings = true;
                }

                if (!m_spawner.m_foldoutSpawnerSettings)
                {
                    if (m_editorUtils.Button("PlusButtonBiomePreset", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        m_spawner.m_foldoutSpawnerSettings = true;
                    }
                }
                else
                {
                    if (m_editorUtils.Button("MinusButtonBiomePreset", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        m_spawner.m_foldoutSpawnerSettings = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("BiomePreset", helpEnabled);
            if (m_spawner.m_foldoutSpawnerSettings)
            {
                float offSet = 16;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(offSet);
                    m_editorUtils.Label("DefaultBiomeSettings", EditorStyles.boldLabel);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(offSet);
                    Rect listRect = EditorGUILayout.GetControlRect(true, m_biomeSpawnersList.GetHeight());
                    m_biomeSpawnersList.DoList(listRect);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(offSet);
                    m_editorUtils.InlineHelp("DefaultBiomeSettings", helpEnabled);
                }
                GUILayout.EndHorizontal();
            }

#endregion

            GUILayout.BeginHorizontal();
            m_editorUtils.LabelField("RandomSeed", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUI.enabled = !m_spawner.m_settings.m_generateRandomSeed;
            m_spawner.m_settings.m_randomSeed = EditorGUILayout.IntField(m_spawner.m_settings.m_randomSeed);
            GUI.enabled = currentGUIState;
            if (EditorGUIUtility.currentViewWidth > 325)
            {
                if (GUILayout.Button(m_editorUtils.GetContent("GenerateRandomSeedNow")))
                {
                    m_spawner.GenerateNewRandomSeed(true);
                }

                GUILayout.FlexibleSpace();
                m_editorUtils.LabelField("GenerateSeedValue", GUILayout.Width(30));
            }
            m_spawner.m_settings.m_generateRandomSeed = GUILayout.Toggle(m_spawner.m_settings.m_generateRandomSeed, "", m_generateSeedCheckboxStyle, GUILayout.Width(20));
            GUILayout.EndHorizontal();
            DrawSeaLevelSlider(helpEnabled);
            m_editorUtils.InlineHelp("RandomSeedWorldDesigner", helpEnabled);
        }

        private void DrawCTS(bool helpEnabled)
        {
#if CTS_PRESENT
            m_spawner.ConnectedCTSProfile = (CTSProfile)m_editorUtils.ObjectField("CTSConnectedProfile", m_spawner.ConnectedCTSProfile, typeof(CTSProfile), helpEnabled);
            m_editorUtils.Heading("CTSStatus");
            //Check for common issues with CTS and display status accordingly
            if (m_spawner.ConnectedCTSProfile != null)
            {
                bool textureNotFoundInSpawner = false;
                bool textureNotFoundInCTSProfile = false;
                bool textureOrderDifferent = false;

                for (int i = 0; i < m_spawner.ConnectedCTSProfile.TerrainTextures.Count; i++)
                {
                    CTSTerrainTextureDetails textureDetails = m_spawner.ConnectedCTSProfile.TerrainTextures[i];
                    bool textureFoundInSpawner = false;
                    int lastFoundIndex = 0;
                    for (int j = 0; j < m_spawner.m_settings.m_spawnerRules.Count; j++)
                    {
                        //Get the spawn rule
                        SpawnRule spawnRule = m_spawner.m_settings.m_spawnerRules[j];

                        //Only texture entries
                        if (spawnRule.m_resourceType != SpawnerResourceType.TerrainTexture)
                        {
                            continue;
                        }

                        //Get the texture prototype used in this spawn rule
                        ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[spawnRule.m_resourceIdx];
                        if (protoTexture.m_texture == textureDetails.Albedo)
                        {
                            textureFoundInSpawner = true;
                            lastFoundIndex = j;
                        }

                    }
                    if (!textureFoundInSpawner)
                    {
                        textureNotFoundInSpawner = true;
                        break;
                    }

                    //Texture was found, but is the order ok?
                    if (lastFoundIndex < i)
                    {
                        textureOrderDifferent = true;
                        break;
                    }

                }

                //Now check the spawner rules against the CTS profile
                for (int i = 0; i < m_spawner.m_settings.m_spawnerRules.Count; i++)
                {
                    //Get the spawn rule
                    SpawnRule spawnRule = m_spawner.m_settings.m_spawnerRules[i];

                    //Only texture entries
                    if (spawnRule.m_resourceType != SpawnerResourceType.TerrainTexture)
                    {
                        continue;
                    }

                    //Get the texture prototype used in this spawn rule
                    ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[spawnRule.m_resourceIdx];


                    bool textureFoundInProfile = false;
                    int lastFoundIndex = 0;
                    for (int j = 0; j < m_spawner.ConnectedCTSProfile.TerrainTextures.Count; j++)
                    {

                        CTSTerrainTextureDetails textureDetails = m_spawner.ConnectedCTSProfile.TerrainTextures[j];

                        if (protoTexture.m_texture == textureDetails.Albedo)
                        {
                            textureFoundInProfile = true;
                            lastFoundIndex = j;
                        }
                    }
                    if (!textureFoundInProfile)
                    {
                        textureNotFoundInCTSProfile = true;
                        break;
                    }
                }

                if (textureNotFoundInCTSProfile)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("CTSMessageTextureNotFoundInCTSProfile"), MessageType.Error);
                }
                else if (textureNotFoundInSpawner)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("CTSMessageTextureNotFoundInSpawner"), MessageType.Warning);
                }

                else if (textureOrderDifferent)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("CTSMessageTextureOrderDifferent"), MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("CTSMessageAllOK"), MessageType.Info);
                }


            }

            if (m_editorUtils.Button("CTSButtonCreateNewProfile"))
            {
                //have at least one texture spawn rule before being allowed to create a CTS profile
                if (m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.TerrainTexture).Count() == 0)
                {
                    EditorUtility.DisplayDialog("No Texture Spawn Rules", "You need at least one texture spawn rule in the spawner to be able to create a CTS Profile. Please add at least one texture to the spawner first.", "OK");
                    EditorGUIUtility.ExitGUI();
                    return;
                }

                CTSProfile profile = ScriptableObject.CreateInstance<CTS.CTSProfile>();
                profile.GlobalDetailNormalMap = CompleteTerrainShader.GetAsset("T_Detail_Normal_3.png", typeof(Texture2D)) as Texture2D;
                profile.GeoAlbedo = CompleteTerrainShader.GetAsset("T_Geo_00.png", typeof(Texture2D)) as Texture2D;
                profile.SnowAlbedo = CompleteTerrainShader.GetAsset("T_Ground_Snow_1_A_Sm.tga", typeof(Texture2D)) as Texture2D;
                profile.SnowNormal = CompleteTerrainShader.GetAsset("T_Ground_Snow_1_N.tga", typeof(Texture2D)) as Texture2D;
                profile.SnowHeight = CompleteTerrainShader.GetAsset("T_Ground_Snow_1_H.png", typeof(Texture2D)) as Texture2D;
                profile.SnowAmbientOcclusion = CompleteTerrainShader.GetAsset("T_Ground_Snow_1_AO.tga", typeof(Texture2D)) as Texture2D;
                profile.SnowGlitter = CompleteTerrainShader.GetAsset("T_Glitter_SM.tga", typeof(Texture2D)) as Texture2D;
                profile.m_ctsDirectory = CompleteTerrainShader.GetCTSDirectory();

                List<int> checkedResourceIds = new List<int>();
                foreach (SpawnRule sr in m_spawner.m_settings.m_spawnerRules)
                {
                    if (sr.m_resourceType == SpawnerResourceType.TerrainTexture)
                    {
                        // No duplicates
                        if (checkedResourceIds.Contains(sr.m_resourceIdx))
                        {
                            continue;
                        }

                        ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx];

                        profile.TerrainTextures.Add(new CTSTerrainTextureDetails()
                        {
                            Albedo = protoTexture.m_texture,
                            Normal = protoTexture.m_normal,
                            Smoothness = protoTexture.m_CTSSmoothnessMap,
                            Roughness = protoTexture.m_CTSRoughnessMap,
                            Height = protoTexture.m_CTSHeightMap,
                            AmbientOcclusion = protoTexture.m_CTSAmbientOcclusionMap,
                            m_smoothness = protoTexture.m_smoothness,
                            m_normalStrength = protoTexture.m_normalScale,
                            m_albedoTilingClose = protoTexture.m_sizeX
                        }) ;
                        checkedResourceIds.Add(sr.m_resourceIdx);
                    }
                    
                }



                Directory.CreateDirectory(profile.m_ctsDirectory + "Profiles/");
                string profileName = string.Format("CTS_Profile_{0:yyMMdd-HHmm}", DateTime.Now);
                string path = string.Format("{0}Profiles/{1}.asset", profile.m_ctsDirectory, profileName);
                AssetDatabase.CreateAsset(profile, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path);
                
                m_spawner.ConnectedCTSProfile = profile;

                if (EditorUtility.DisplayDialog("Apply Profile?", "Do you want to apply this new profile to all active terrains? (Including currently not loaded terrains)", "Yes", "No"))
                {
                    //force the rebuild of terrain layers in this case
                    profile.terrainLayerAssetRebuild = true;
                    profile.cachedTerrainLayers = new TerrainLayer[0];
                    CTSGX.AddCTSProfile(profileName);
                }

                EditorGUIUtility.PingObject(profile);
            }

            bool currentGUIState = GUI.enabled;

            if (m_spawner.ConnectedCTSProfile == null)
            {
                GUI.enabled = false;
            }

            //if (m_editorUtils.Button("CTSButtonSyncFromProfile"))
            //{
              
            //}
            if (m_editorUtils.Button("CTSButtonSyncToProfile"))
            {
                if (EditorUtility.DisplayDialog("Sync Textures to CTS Profile?", "Do you want to sync the textures from this spawner to the connected CTS Profile? This will compare the textures contained in the spawner against the CTS profile and will establish the same sorting order and will add missing textures from this spawner.", "Yes", "No"))
                {
                    List<CTSTerrainTextureDetails> oldTextureDetails = m_spawner.ConnectedCTSProfile.TerrainTextures;
                    List<CTSTerrainTextureDetails> newTextureDetails = new List<CTSTerrainTextureDetails>();
                    List<int> checkedResourceIds = new List<int>();


                    for (int i = 0; i < m_spawner.m_settings.m_spawnerRules.Count; i++)
                    {
                        //Get the spawn rule
                        SpawnRule spawnRule = m_spawner.m_settings.m_spawnerRules[i];

                        //Only texture entries
                        if (spawnRule.m_resourceType != SpawnerResourceType.TerrainTexture)
                        {
                            continue;
                        }

                        //No duplicates
                        if (checkedResourceIds.Contains(spawnRule.m_resourceIdx))
                        {
                            continue;
                        }

                        //Get the texture prototype used in this spawn rule
                        ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[spawnRule.m_resourceIdx];

                        //try to find an existing entry in the old texture details
                        CTSTerrainTextureDetails detailsEntry = oldTextureDetails.Find(x => x.Albedo == protoTexture.m_texture);
                        if (detailsEntry != null)
                        {
                            //old entry found, lets take this one for the list of new texture details
                            newTextureDetails.Add(detailsEntry);
                        }
                        else
                        {
                            //no entry found, lets create a new entry
                            newTextureDetails.Add(new CTSTerrainTextureDetails()
                            {
                                Albedo = protoTexture.m_texture,
                                Normal = protoTexture.m_normal,
                                Smoothness = protoTexture.m_CTSSmoothnessMap,
                                Roughness = protoTexture.m_CTSRoughnessMap,
                                Height = protoTexture.m_CTSHeightMap,
                                AmbientOcclusion = protoTexture.m_CTSAmbientOcclusionMap,
                                m_smoothness = protoTexture.m_smoothness,
                                m_normalStrength = protoTexture.m_normalScale,
                                m_albedoTilingClose = protoTexture.m_sizeX
                            });
                        }
                        checkedResourceIds.Add(spawnRule.m_resourceIdx);
                    }

                    m_spawner.ConnectedCTSProfile.TerrainTextures = newTextureDetails;
                    m_spawner.ConnectedCTSProfile.m_needsAlbedosArrayUpdate = true;
                    m_spawner.ConnectedCTSProfile.m_needsNormalsArrayUpdate = true;
                    m_spawner.ConnectedCTSProfile.RegenerateArraysIfNecessary();

                    if (EditorUtility.DisplayDialog("Re-Apply Profile?", "Do you want to reapply the updated CTS profile to all active terrains? (Including currently not loaded terrains) This will ensure the texture order on the terrain will be in sync with the CTS profile.", "Yes", "No"))
                    {
                        //force the rebuild of terrain layers in this case
                        m_spawner.ConnectedCTSProfile.terrainLayerAssetRebuild = true;
                        m_spawner.ConnectedCTSProfile.cachedTerrainLayers = new TerrainLayer[0];
                        CTSGX.AddCTSProfile(m_spawner.ConnectedCTSProfile.name);
                    }

                }
            }
            if (m_editorUtils.Button("CTSButtonApplyToTerrain"))
            {
                if (EditorUtility.DisplayDialog("Apply Profile?", "Do you want to apply the connected CTS profile to all active terrains? (Including currently not loaded terrains)", "Yes", "No"))
                {
                    //force the rebuild of terrain layers in this case
                    m_spawner.ConnectedCTSProfile.terrainLayerAssetRebuild = true;
                    m_spawner.ConnectedCTSProfile.cachedTerrainLayers = new TerrainLayer[0];
                    CTSGX.AddCTSProfile(m_spawner.ConnectedCTSProfile.name);
                }
            }

            GUI.enabled = currentGUIState;
#endif
        }

        private void DrawAdvanced(bool helpEnabled)
        {
            if (m_spawner.m_highlightLoadingSettings)
            {
                m_editorUtils.SetPanelStatus(DrawAppearance, true, false);
            }
            m_editorUtils.Panel("Appearance", DrawAppearance, false);
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                m_editorUtils.Panel("ResourceManagement", DrawResourceManagement, false);
            }
            else
            {
                m_editorUtils.Panel("ExportPreview", DrawExportPreview, false);
            }
            if (m_changesMadeSinceLastSave)
            {
                GUI.backgroundColor = m_dirtyColor;
                m_editorUtils.Panel("SaveLoadChangesPending", DrawSaveAndLoad, (m_spawner.m_createdfromBiomePreset || m_spawner.m_createdFromGaiaManager) ? true : false);
            }
            else
            {
                m_editorUtils.Panel("SaveLoad", DrawSaveAndLoad, (m_spawner.m_createdfromBiomePreset || m_spawner.m_createdFromGaiaManager) ? true : false);
            }
            GUI.backgroundColor = m_normalBGColor;
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                if (m_editorUtils.Button("DuplicateSpawner"))
                {
                    Spawner newSpawner = m_spawner.m_settings.CreateSpawner(false);
                    newSpawner.transform.name = newSpawner.transform.name.Replace("(Clone)", " Copy");
                    EditorGUIUtility.PingObject(newSpawner.gameObject);
                }
            }
        }

        private void DrawExportPreview(bool obj)
        {
            Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);

            bool currentGUIState = GUI.enabled;

            if (stamper.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportWorldDesignerNotPossibleSingleTerrain"), MessageType.Warning);
                GUI.enabled = false;
            }
            else
            {
                if (stamper.m_useCustomPreviewBounds)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportWorldDesignerNotPossibleNotAllStamps"), MessageType.Warning);
                    GUI.enabled = false;
                }
                else
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportWorldDesignerInfo"), MessageType.Info);
                }
            }

            if (m_editorUtils.Button("ExportWorldDesignerStampButton"))
            {

                string exportStampPath = EditorUtility.SaveFilePanel("Save Preview as Stamp...", GaiaDirectories.GetWorldDesignerExportsDirectory(), "WorldDesignerStamp", "exr");
                if (exportStampPath != "")
                {
                    float originalHeight = stamper.m_settings.m_height;
                    try
                    {
                        ProgressBar.Show(ProgressBarPriority.MaskMapExport, "Saving Preview", "Re-rendering preview and saving...", 1, 2, false, false);
                        ImageProcessing.WriteRenderTexture(exportStampPath.Replace(".exr", ""), stamper.m_cachedRenderTexture, ImageFileType.Exr, TextureFormat.RGBAFloat,75, true);
                        string relativePath = GaiaDirectories.GetPathStartingAtAssetsFolder(exportStampPath);
                        AssetDatabase.ImportAsset(relativePath);
                        GaiaUtils.SetDefaultStampImportSettings(relativePath);

                        UnityEngine.Object texture = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));
                        EditorGUIUtility.PingObject(texture);
                    }
                    catch
                    (Exception ex)
                    {
                        Debug.LogError($"Error while saving the preview, Message: {ex.Message} Stack Trace: {ex.StackTrace}");
                    }
                    finally
                    {
                        ProgressBar.Clear(ProgressBarPriority.MaskMapExport);
                    }
                }
            }
            GUI.enabled = currentGUIState;
        }

        private void DrawResourceManagement(bool helpEnabled)
        {
            GUILayout.Space(10);
            m_editorUtils.Heading("ResourceManagementDropResources");
            m_editorUtils.InlineHelp("ResourceManagementDropResources", helpEnabled);
            DrawResourceDropBox(helpEnabled);
            GUILayout.Space(10);
            m_editorUtils.Heading("ResourceManagementImportFromTerrain");
            m_editorUtils.InlineHelp("ResourceManagementImportFromTerrain", helpEnabled);
            if (m_resourceImportTerrain == null)
            {
                m_resourceImportTerrain = Terrain.activeTerrain;
            }
            EditorGUI.indentLevel++;
            m_resourceImportTerrain = (Terrain)m_editorUtils.ObjectField("ResourceManagementImportTerrain", m_resourceImportTerrain, typeof(Terrain), true, helpEnabled);
            m_resourceImportType = (GaiaConstants.ImportableResourceType)m_editorUtils.EnumPopup("ResourceManagementImportType", m_resourceImportType, helpEnabled);
            m_resourceImportMode = (GaiaConstants.ImportableResourceMode)m_editorUtils.EnumPopup("ResourceManagementImportMode", m_resourceImportMode, helpEnabled);
            EditorGUILayout.BeginHorizontal();
            {
                if (m_editorUtils.Button("ResourceManagementImportButton"))
                {
                    if (m_resourceImportTerrain == null)
                    {
                        EditorUtility.DisplayDialog("No Terrain Selected", "No terrain selected to import these resources from! Please assign a valid terrain in the 'Import From Terrain' slot.", "OK");
                        return;
                    }
                    if (m_resourceImportMode == ImportableResourceMode.ReplaceRules)
                    {
                        DeleteAllRules();
                    }
                    switch (m_resourceImportType)
                    {
                        case ImportableResourceType.TerrainTexture:
                            foreach (TerrainLayer terrainLayer in m_resourceImportTerrain.terrainData.terrainLayers)
                            {
                                SpawnRule newRule = new SpawnRule();
                                newRule.m_resourceType = SpawnerResourceType.TerrainTexture;
                                newRule.m_name = terrainLayer.diffuseTexture.name;
                                newRule.m_resourceIdx = AddNewTextureResource();
                                //Get new reource and fill it with layer data
                                ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[newRule.m_resourceIdx];

                                //add the layer guid, since we have a layer already might as well use it for spawning right away
                                protoTexture.m_LayerGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(terrainLayer));

                                protoTexture.m_name = terrainLayer.name;
                                protoTexture.m_texture = terrainLayer.diffuseTexture;
                                protoTexture.m_normal = terrainLayer.normalMapTexture;
                                protoTexture.m_maskmap = terrainLayer.maskMapTexture;
                                protoTexture.m_sizeX = terrainLayer.tileSize.x;
                                protoTexture.m_sizeY = terrainLayer.tileSize.x;
                                protoTexture.m_offsetX = terrainLayer.tileOffset.x;
                                protoTexture.m_offsetY = terrainLayer.tileOffset.x;
                                protoTexture.m_normalScale = terrainLayer.normalScale;
                                protoTexture.m_metallic = terrainLayer.metallic;
                                protoTexture.m_smoothness = terrainLayer.smoothness;
                                protoTexture.m_diffuseRemapMin = terrainLayer.diffuseRemapMin;
                                protoTexture.m_diffuseRemapMax = terrainLayer.diffuseRemapMax;
                                protoTexture.m_maskMapRemapMin = terrainLayer.maskMapRemapMin;
                                protoTexture.m_maskMapRemapMax = terrainLayer.maskMapRemapMax;
                                protoTexture.m_specularColor = terrainLayer.specular;
                                newRule.m_imageMasks = new ImageMask[0];
                                //always fold out the resource settings for new rules so user can edit directly
                                newRule.m_isFoldedOut = false;
                                newRule.m_resourceSettingsFoldedOut = false;
                                m_spawner.m_rulePanelUnfolded = true;
                                m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                AddNewMaskList();
                            }

                            break;
                        case ImportableResourceType.TerrainDetail:
                            foreach (DetailPrototype terrainDetail in m_resourceImportTerrain.terrainData.detailPrototypes)
                            {
                                SpawnRule newRule = new SpawnRule();
                                newRule.m_resourceType = SpawnerResourceType.TerrainDetail;
                                if (terrainDetail.usePrototypeMesh)
                                {
                                    newRule.m_name = terrainDetail.prototype.name;
                                }
                                else
                                {
                                    newRule.m_name = terrainDetail.prototypeTexture.name;
                                }
                                newRule.m_resourceIdx = AddNewTerrainDetailResource();
                                //Get new reource and fill it with terrain detail data
                                ResourceProtoDetail protoDetail = m_spawner.m_settings.m_resources.m_detailPrototypes[newRule.m_resourceIdx];

                                protoDetail.m_renderMode = terrainDetail.renderMode;
                                protoDetail.m_detailProtoype = terrainDetail.prototype;
                                protoDetail.m_detailTexture = terrainDetail.prototypeTexture;
                                protoDetail.m_dryColour = terrainDetail.dryColor;
                                protoDetail.m_healthyColour = terrainDetail.healthyColor;
                                protoDetail.m_maxHeight = terrainDetail.maxHeight;
                                protoDetail.m_maxWidth = terrainDetail.maxWidth;
                                protoDetail.m_minHeight = terrainDetail.minHeight;
                                protoDetail.m_minWidth = terrainDetail.minWidth;
                                protoDetail.m_noiseSpread = terrainDetail.noiseSpread;
#if UNITY_2022_2_OR_NEWER
                                protoDetail.m_alignToGround = terrainDetail.alignToGround;
                                protoDetail.m_density = terrainDetail.density;
                                protoDetail.m_holeEdgePadding = terrainDetail.holeEdgePadding;
                                protoDetail.m_noiseSeed = terrainDetail.noiseSeed;
                                protoDetail.m_positionJitter = terrainDetail.positionJitter;
                                protoDetail.m_targetCoverage = terrainDetail.targetCoverage;
                                protoDetail.m_useDensityScaling = terrainDetail.useDensityScaling;
#endif
                                newRule.m_imageMasks = new ImageMask[0];
                                //always fold out the resource settings for new rules so user can edit directly
                                newRule.m_isFoldedOut = false;
                                newRule.m_resourceSettingsFoldedOut = false;
                                m_spawner.m_rulePanelUnfolded = true;
                                m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                AddNewMaskList();
                            }
                            break;
                        case ImportableResourceType.TerrainTree:
                            int index = 0;
                            foreach (TreePrototype treePrototype in m_resourceImportTerrain.terrainData.treePrototypes)
                            {
                                //get that prototypes min max scales on the terrain
                                float maxHeightScale = m_resourceImportTerrain.terrainData.treeInstances.Where(x => x.prototypeIndex == index).Select(x => x.heightScale).Max();
                                float minHeightScale = m_resourceImportTerrain.terrainData.treeInstances.Where(x => x.prototypeIndex == index).Select(x => x.heightScale).Min();
                                float maxWidthScale = m_resourceImportTerrain.terrainData.treeInstances.Where(x => x.prototypeIndex == index).Select(x => x.widthScale).Max();
                                float minWidthScale = m_resourceImportTerrain.terrainData.treeInstances.Where(x => x.prototypeIndex == index).Select(x => x.widthScale).Min();


                                SpawnRule newRule = new SpawnRule();
                                newRule.m_resourceType = SpawnerResourceType.TerrainTree;
                                newRule.m_name = treePrototype.prefab.name;
                                newRule.m_resourceIdx = AddNewTreeResource();
                                //Get new reource and fill it with layer data
                                ResourceProtoTree protoTree = m_spawner.m_settings.m_resources.m_treePrototypes[newRule.m_resourceIdx];

                                protoTree.m_name = treePrototype.prefab.name;
                                protoTree.m_bendFactor = treePrototype.bendFactor;
                                protoTree.m_desktopPrefab = treePrototype.prefab;
                                protoTree.m_spawnScale = SpawnScale.Random;
                                protoTree.m_maxHeight = maxHeightScale;
                                protoTree.m_minHeight = minHeightScale;
                                protoTree.m_maxWidth = maxWidthScale;
                                protoTree.m_minWidth = minWidthScale;
                                newRule.m_imageMasks = new ImageMask[0];
                                //always fold out the resource settings for new rules so user can edit directly
                                newRule.m_isFoldedOut = false;
                                newRule.m_resourceSettingsFoldedOut = false;
                                m_spawner.m_rulePanelUnfolded = true;
                                m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                AddNewMaskList();
                                index++;
                            }

                            break;
                    }

                    ImageMask.RefreshSpawnRuleGUIDs();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
            m_editorUtils.Heading("ResourceManagementResourceHelper");
            m_editorUtils.InlineHelp("ResourceManagementResourceHelper", helpEnabled);
            GUILayout.BeginHorizontal();
            {
                if (m_editorUtils.Button("ResourceManagementCopyResources"))
                {
                    GaiaEditorUtils.ShowResourceHelperWindow(GaiaResourceHelperOperation.CopyResources, new Vector2(Display.main.systemWidth * 2, Display.main.systemHeight / 2));
                }
                if (m_editorUtils.Button("ResourceManagementRemoveResources"))
                {
                    GaiaEditorUtils.ShowResourceHelperWindow(GaiaResourceHelperOperation.RemoveResources, new Vector2(Display.main.systemWidth * 2, Display.main.systemHeight / 2));
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                if (m_editorUtils.Button("ResourceManagementConvertTrees"))
                {
                    GaiaEditorUtils.ShowResourceHelperWindow(GaiaResourceHelperOperation.TreesToGameObjects, new Vector2(Display.main.systemWidth * 2, Display.main.systemHeight / 2));
                }
            }
            GUILayout.EndHorizontal();

        }

        private bool DrawWorldDesignerPanel(string nameKey, Action<bool> contentMethod, ref bool showHelp, bool unfolded = true, string additionalNameString ="")
        {
            GUIContent panelLabel = m_editorUtils.GetContent(nameKey);
            panelLabel.text += additionalNameString;

            //Panel Label
            GUIStyle panelLabelStyle = new GUIStyle(GUI.skin.label);
            panelLabelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelLabelStyle.fontStyle = FontStyle.Bold;
            panelLabelStyle.normal.background = GUI.skin.label.normal.background;

            // Panel Frame
            GUIStyle panelFrameStyle = new GUIStyle(GUI.skin.box);
            panelFrameStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelFrameStyle.fontStyle = FontStyle.Bold;
            panelFrameStyle.alignment = TextAnchor.UpperLeft;

            // Panel
            GUIStyle panelStyle = new GUIStyle(GUI.skin.label);
            panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelStyle.alignment = TextAnchor.UpperLeft;


            bool helpActive = m_rulePanelHelpActive;

            GUILayout.BeginVertical(m_spawnRulesBorder);
            {
                GUILayout.BeginHorizontal(m_spawnRulesHeader);
                {
                    unfolded = GUILayout.Toggle(unfolded, unfolded ? "-" : "+", panelLabelStyle, GUILayout.MinWidth(14));
                    GUILayout.Space(-5f);
                    unfolded = GUILayout.Toggle(unfolded, panelLabel, panelLabelStyle);
                    GUILayout.FlexibleSpace();
                    m_editorUtils.HelpToggle(ref showHelp);

                }
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();

                if (helpActive)
                {
                    GUILayout.Space(2f);
                    m_editorUtils.InlineHelp(nameKey, helpActive);
                }

                if (unfolded)
                {
                    GUILayout.BeginVertical(panelStyle);
                    {
                        contentMethod(showHelp);
                    }
                    GUILayout.EndVertical();
                }
            }
            if (unfolded)
            {
                //Footer - repeat the buttons so they are easily accessible anywhere
                GUILayout.BeginHorizontal(m_spawnRulesHeader);
                {

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            m_rulePanelHelpActive = helpActive;
            return unfolded;
        }

        void DrawWorldShapeSettings(bool showHelp)
        {
            BaseTerrainSettings bts = m_spawner.m_baseTerrainSettings;
            EditorGUI.BeginChangeCheck(); 
            bts.m_baseTerrainInputType = (BaseTerrainInputType)m_editorUtils.EnumPopup("BaseTerrainInputType", bts.m_baseTerrainInputType, showHelp);
            bts.m_borderStyle = (GeneratorBorderStyle)m_editorUtils.EnumPopup("BaseTerrainBorderStyle", bts.m_borderStyle, showHelp);
            bts.m_heightScale = m_editorUtils.Slider("BaseTerrainHeightScale", bts.m_heightScale, 0f, 100f, showHelp);
            bts.m_baseLevel = m_editorUtils.Slider("BaseTerrainBaseHeight", bts.m_baseLevel, 0f, 1000f, showHelp);
           
            switch (bts.m_baseTerrainInputType)
            {
                case BaseTerrainInputType.Generator:
                    bts.m_baseNoiseSettings[0].m_shapeNoiseStyle = (NoiseTypeName)m_editorUtils.EnumPopup("BaseTerrainNoiseStyle", bts.m_baseNoiseSettings[0].m_shapeNoiseStyle, showHelp);
                    bts.m_baseNoiseSettings[0].m_shapeStrength = m_editorUtils.Slider("BaseTerrainStrength", bts.m_baseNoiseSettings[0].m_shapeStrength, 0f, 1f, showHelp);
                    bts.m_baseNoiseSettings[0].m_shapeSteepness = m_editorUtils.Slider("BaseTerrainCutoff", bts.m_baseNoiseSettings[0].m_shapeSteepness, 0f, 1f, showHelp);
                    bts.m_baseNoiseSettings[0].m_shapeOffset = m_editorUtils.Vector3Field("BaseTerrainOffset", bts.m_baseNoiseSettings[0].m_shapeOffset, showHelp);
                    bts.m_baseNoiseSettings[0].m_shapeSize = m_editorUtils.Slider("BaseTerrainSize", bts.m_baseNoiseSettings[0].m_shapeSize, 0f, 10f, showHelp);
                    bts.m_baseNoiseSettings[0].m_shapeGranularity = m_editorUtils.Slider("BaseTerrainGranularity", bts.m_baseNoiseSettings[0].m_shapeGranularity, 0f, 3f, showHelp);
                    bts.m_baseNoiseSettings[0].m_warpIterations = m_editorUtils.Slider("BaseTerrainWarpIterations", bts.m_baseNoiseSettings[0].m_warpIterations, 0.0f, 8.0f, showHelp);
                    bool currentGUIState = GUI.enabled;
                    if (bts.m_baseNoiseSettings[0].m_warpIterations == 0)
                    {
                        GUI.enabled = false;
                    }
                    bts.m_baseNoiseSettings[0].m_warpStrength = m_editorUtils.Slider("BaseTerrainWarpStrength", bts.m_baseNoiseSettings[0].m_warpStrength, 0.0f, 2.0f, showHelp);
                    bts.m_baseNoiseSettings[0].m_warpOffset = m_editorUtils.Vector3Field("BaseTerrainWarpOffset", bts.m_baseNoiseSettings[0].m_warpOffset, showHelp);
                    GUI.enabled = currentGUIState;

                    break;
                case BaseTerrainInputType.Image:
                    if (bts.m_inputImageMask.ImageMaskTexture != null && !GaiaConstants.Valid16BitFormats.Contains(bts.m_inputImageMask.ImageMaskTexture.format))
                    {
                        EditorGUILayout.HelpBox("Supplied texture is not in 16-bit color format. For optimal quality use 16+ bit color images.", MessageType.Warning);
                    } 
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(m_editorUtils.GetContent("StampImage"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    bts.m_inputImageMask.ImageMaskTexture = (Texture2D)EditorGUILayout.ObjectField(bts.m_inputImageMask.ImageMaskTexture, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (GUILayout.Button(m_editorUtils.GetContent("MaskImageOpenStampButton"), GUILayout.Width(70)))
                    {
                        ImageMaskListEditor.OpenStampBrowser(bts.m_inputImageMask);
                    }
                    GUILayout.EndHorizontal();
                    //m_editorUtils.InlineHelp("StampImage", showHelp);

                    DrawInputImageMaskFields(bts,showHelp);
                    break;
                case BaseTerrainInputType.ExistingTerrain:
                    bts.m_inputTerrain = (Terrain)m_editorUtils.ObjectField("BaseTerrainInputTerrain", bts.m_inputTerrain, typeof(Terrain), true, showHelp);
                    DrawInputImageMaskFields(bts, showHelp);
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (m_spawner.m_worldMapStamperSettings.Count > 0 && !m_spawner.m_worldDesignerClearStampsWarningShown)
                {
                    if (EditorUtility.DisplayDialog("Clear Spawned Detail Stamps?", "It is highly recommended to clear the existing stamps from the terrain when editing the world shape - this will make the preview more responsive, and will allow you to actually see the world shape better without the stamps being applied. The stamps can be spawned again after you are finished editing the world shape. Do you want to clear the stamps now?", "Yes, clear stamps", "No, keep stamps"))
                    {
                        m_spawner.ClearStampDistributions();
                    }
                    m_spawner.m_worldDesignerClearStampsWarningShown = true;
                }

                if (m_spawner.m_baseTerrainSettings.m_drawPreview)
                {
                    m_spawner.UpdateBaseTerrainStamper();
                }
            }
        }
         
        private void DrawInputImageMaskFields(BaseTerrainSettings bts, bool showHelp)
        {
            EditorGUI.indentLevel++;
            bts.m_advancedInputSettingsUnfolded = m_editorUtils.Foldout(bts.m_advancedInputSettingsUnfolded, "BaseTerrainAdvancedInput");
            if (bts.m_advancedInputSettingsUnfolded)
            {
                GUILayout.BeginHorizontal();
                bts.m_inputImageMask.m_strengthTransformCurve = m_editorUtils.CurveField("MaskStrengthTransformCurve", bts.m_inputImageMask.m_strengthTransformCurve);
                if (GUILayout.Button(m_editorUtils.GetContent("MaskInvert"), GUILayout.Width(70)))
                {
                    GaiaUtils.InvertAnimationCurve(ref bts.m_inputImageMask.m_strengthTransformCurve);
                }
                GUILayout.EndHorizontal();

                m_editorUtils.InlineHelp("MaskStrengthTransformCurve", showHelp);
                GUILayout.BeginHorizontal();
                m_editorUtils.LabelField("StampSpace", GUILayout.Width(EditorGUIUtility.labelWidth));
                bts.m_inputImageMask.m_imageMaskSpace = (ImageMaskSpace)EditorGUILayout.EnumPopup(bts.m_inputImageMask.m_imageMaskSpace, GUILayout.Width(EditorGUIUtility.labelWidth * 0.68f));
                GUILayout.Space(10);
                m_editorUtils.LabelField("StampTiling", GUILayout.Width(55));
                bts.m_inputImageMask.m_tiling = EditorGUILayout.Toggle(bts.m_inputImageMask.m_tiling);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("StampSpace", showHelp);

                GUILayout.BeginHorizontal();
                m_editorUtils.LabelField("StampOffset", GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUIUtility.labelWidth = 10;
                bts.m_inputImageMask.m_xOffSet = m_editorUtils.FloatField("MaskXOffset", bts.m_inputImageMask.m_xOffSet);
                GUILayout.Space(10);
                bts.m_inputImageMask.m_zOffSet = m_editorUtils.FloatField("MaskZOffset", bts.m_inputImageMask.m_zOffSet);
                EditorGUIUtility.labelWidth = 0;
                GUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("StampOffset", showHelp);

                GUILayout.BeginHorizontal();
                m_editorUtils.LabelField("StampScale", GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUIUtility.labelWidth = 10;
                bts.m_inputImageMask.m_xScale = m_editorUtils.FloatField("MaskXScale", bts.m_inputImageMask.m_xScale);
                GUILayout.Space(10);
                bts.m_inputImageMask.m_zScale = m_editorUtils.FloatField("MaskZScale", bts.m_inputImageMask.m_zScale);
                EditorGUIUtility.labelWidth = 0;
                GUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("StampScale", showHelp);
                bts.m_inputImageMask.m_rotationDegrees = m_editorUtils.Slider("StampRotation", bts.m_inputImageMask.m_rotationDegrees, -180f, 180f, showHelp);
            }
            EditorGUI.indentLevel--;
        }

        //private void DrawExportTerrain(bool showHelp)
        //{
        //    //if (m_spawner.m_worldCreationSettings.m_tileHeight != m_spawner.m_worldMapTerrain.terrainData.size.y)
        //    //{
        //    //    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportTerrainDifferentHeight"), MessageType.Error);
        //    //    if (m_editorUtils.Button("ExportTerrainAdjustWorldMapHeight"))
        //    //    {
        //    //        m_spawner.m_worldMapTerrain.terrainData.size = new Vector3(m_spawner.m_worldMapTerrain.terrainData.size.x, m_spawner.m_worldCreationSettings.m_tileHeight, m_spawner.m_worldMapTerrain.terrainData.size.z);
        //    //    }
        //    //}
        //    //else
        //    //{

        //    bool currentGUIState = GUI.enabled;

        //    EditorGUI.indentLevel++;
        //    GUILayout.Space(10);
        //    m_editorUtils.Heading("ExportTerrainStampOperationsHeading");
        //    m_spawner.m_stampOperationsFoldedOut = m_editorUtils.Foldout(m_spawner.m_stampOperationsFoldedOut, "ExportTerrainStampOperations", showHelp);

        //    if (m_spawner.m_stampOperationsFoldedOut)
        //    {
        //        List<WorldMapStampToken> allTokens = m_spawner.gameObject.GetComponentsInChildren<WorldMapStampToken>().ToList();
        //        List<int> deletionIndIces = new List<int>();
        //        EditorGUI.indentLevel++;
        //        for (int i = 0; i < m_spawner.m_worldMapStamperSettings.Count; i++)
        //        {
        //            StamperSettings stamperSettings = m_spawner.m_worldMapStamperSettings[i];

        //            //does this stamper setting have a representation in the stamper tokens still? If not, remove it
        //            if (TerrainLoaderManager.Instance.WorldMapTerrain.isActiveAndEnabled && allTokens.Where(x => x.m_connectedStamperSettings == stamperSettings).Count() == 0)
        //            {
        //                deletionIndIces.Add(i);
        //                continue;
        //            }

        //            //Get the operation + the first image mask as a name
        //            string name = stamperSettings.m_operation.ToString();
        //            if (stamperSettings.m_imageMasks.Length > 0)
        //            {
        //                switch (stamperSettings.m_imageMasks[0].m_operation)
        //                {
        //                    case ImageMaskOperation.ImageMask:
        //                        name += " " + stamperSettings.m_imageMasks[0].ImageMaskTexture.name;
        //                        break;
        //                    default:
        //                        name += " " + stamperSettings.m_imageMasks[0].m_operation.ToString();
        //                        break;
        //                }
        //            }

        //            stamperSettings.m_isFoldedOut = m_editorUtils.Foldout(stamperSettings.m_isFoldedOut, new GUIContent(name), showHelp);
        //            GUILayout.BeginHorizontal();
        //            if (stamperSettings.m_isFoldedOut)
        //            {
        //                if (m_editorUtils.Button("StampOperationSelect"))
        //                {
        //                    WorldMapStampToken token = allTokens.Where(x => x.m_connectedStamperSettings == stamperSettings).First();
        //                    if (token != null)
        //                    {
        //                        Selection.activeGameObject = token.gameObject;
        //                    }
        //                }
        //                if (m_editorUtils.Button("StampOperationDelete"))
        //                {
        //                    deletionIndIces.Add(i);
        //                }
        //            }
        //            GUILayout.EndHorizontal();
        //        }

        //        if (deletionIndIces.Count() > 0)
        //        {
        //            for (int i1 = deletionIndIces.Count - 1; i1 >= 0; i1--)
        //            {
        //                int deletionIndex = deletionIndIces[i1];
        //                WorldMapStampToken token = allTokens.Find(x => x.m_connectedStamperSettings == m_spawner.m_worldMapStamperSettings[deletionIndex]);
        //                if (token != null)
        //                {
        //                    DestroyImmediate(token.gameObject);
        //                }
        //                m_spawner.m_worldMapStamperSettings.RemoveAt(deletionIndex);
        //            }
        //        }
        //        GUILayout.BeginHorizontal();
        //        GUILayout.Space(16);
        //        if (m_editorUtils.Button("DeleteAllOperations"))
        //        {
        //            m_spawner.m_worldMapStamperSettings.Clear();
        //        }
        //        GUILayout.EndHorizontal();
        //        EditorGUI.indentLevel--;
        //    }
        //    EditorGUI.indentLevel--;
        //    GUILayout.Space(10);
        //    if (m_spawner.m_ExportRunning)
        //    {
        //        //always allow cancel
        //        GUI.enabled = true;
        //        if (m_editorUtils.Button("CancelExport"))
        //        {
        //            SessionManager.m_massStamperSettingsIndex = int.MaxValue;
        //            m_spawner.m_ExportRunning = false;
        //        }
        //        GUI.enabled = currentGUIState;
        //    }
        //    else
        //    {
        //        if (m_spawner.m_worldCreationSettings.m_xTiles != m_spawner.m_worldCreationSettings.m_zTiles)
        //        {
        //            EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportTerrainNotSquareWarning"), MessageType.Warning);
        //            GUI.enabled = false;
        //        }
        //        Color normalBGColor = GUI.backgroundColor;
        //        GUI.backgroundColor = m_gaiaSettings.GetActionButtonColor();
        //        if (m_editorUtils.Button("ExportTerrainButtonExport"))
        //        {
        //            StartExport();

        //            try
        //            {
        //                if (!m_spawner.m_useExistingTerrainForWorldMapExport)
        //                {
        //                    bool hasTerrains = false;
        //                    if (GaiaUtils.GetTerrainObject(false) != null || GaiaUtils.HasDynamicLoadedTerrains())
        //                    {
        //                        hasTerrains = true;
        //                    }

        //                    if (hasTerrains && !EditorUtility.DisplayDialog("Create new World", "WARNING: You are about to export the world map to a new terrain setup. This will delete all your existing terrains and build a new world from scratch. Continue?", "OK", "Cancel"))
        //                    {
        //                        return;
        //                    }
        //                    GaiaSessionManager.ClearWorld(true);
        //                    ProgressBar.Show(ProgressBarPriority.WorldCreation, "Export To World", "Creating Terrain Tiles for the Export...");

        //                    //Shift everything back to 0,0,0 if required
        //                    if (GaiaUtils.HasDynamicLoadedTerrains())
        //                    {
        //                        TerrainLoaderManager.Instance.SetOrigin(Vector3Double.zero);
        //                    }
        //                    //Create a new world according to the settings set by the user
        //                    WorldCreationSettings wcs = m_spawner.m_worldCreationSettings;
        //                    float worldsizeX = wcs.m_xTiles * wcs.m_tileSize;
        //                    //wcs.m_tileHeight = m_spawner.m_worldMapTerrain.terrainData.size.y / (m_spawner.m_worldMapTerrain.terrainData.size.x / worldsizeX);
        //                    wcs.m_seaLevel = Mathf.RoundToInt(SessionManager.GetSeaLevel(true)); //* (m_spawner.m_worldMapTerrain.terrainData.size.x / worldsizeX));

        //                    //Subscribe for the world creation event so we can wait with the export until the world was created in the coroutine
        //                    GaiaSessionManager.OnWorldCreated -= DoExport;
        //                    GaiaSessionManager.OnWorldCreated += DoExport;
        //                    GaiaSessionManager.CreateWorld(m_spawner.m_worldCreationSettings, true);
        //                    m_spawner.m_ExportRunning = true;

        //                }
        //                else
        //                {
        //                    if (!EditorUtility.DisplayDialog("Export World", "WARNING: You are about to export the world map and the spawned stamps over your existing terrain setup. This will replace the existing height information on all terrains in the scene and can greatly change the look of your terrains. Continue?", "OK", "Cancel"))
        //                    {
        //                        return;
        //                    }
        //                    //Shift everything back to 0,0,0 if required
        //                    if (GaiaUtils.HasDynamicLoadedTerrains())
        //                    {
        //                        TerrainLoaderManager.Instance.SetOrigin(Vector3Double.zero);
        //                    }
        //                    //We do not need to wait for terrain creation, but can do the export right away.
        //                    m_spawner.m_ExportRunning = true;
        //                    DoExport();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.LogError("Error while exporting terrain from world map, Exception: " + ex.Message + " Stack Trace: " + ex.StackTrace);
        //            }
        //        }
        //        GUI.backgroundColor = m_normalBGColor;
        //        GUI.enabled = currentGUIState;
        //    }
        //    GUILayout.Space(10);
        //    m_editorUtils.Heading("ExportTerrainSynchronizationHeading");
        //    m_editorUtils.InlineHelp("ExportTerrainSynchronizationHeading", showHelp);
        //    if (!GaiaUtils.HasTerrains())
        //    {
        //        GUI.enabled = false;
        //    }
        //    GUILayout.BeginHorizontal();
        //    {
        //        if (m_editorUtils.Button("WorldMapToTerrains"))
        //        {
        //            GameObject wmeGO = GaiaUtils.GetOrCreateWorldDesigner();
        //            if (wmeGO != null)
        //            {
        //                wmeGO.GetComponent<WorldMap>().SyncWorldMapToLocalMap();
        //            }
        //            else
        //            {
        //                Debug.LogError("Could not find World map editor in this scene!");
        //            }
        //        }
        //        GUILayout.Space(5);
        //        if (m_editorUtils.Button("TerrainsToWorldMap"))
        //        {
        //            GameObject wmeGO = GaiaUtils.GetOrCreateWorldDesigner();
        //            if (wmeGO != null)
        //            {
        //                wmeGO.GetComponent<WorldMap>().SyncLocalMapToWorldMap();
        //            }
        //            else
        //            {
        //                Debug.LogError("Could not find World map editor in this scene!");
        //            }
        //        }
        //    }
        //    GUILayout.EndHorizontal();
        //    GUI.enabled = currentGUIState;

        //}

        private void StartExport()
        {
            if (!GaiaUtils.UsesCorrectPipelineDefines())
            {
                if (!EditorUtility.DisplayDialog("Wrong Render Pipeline Configured", "This project does use a different render pipeline asset in the graphics settings than what Gaia is configured for. This can lead to wrong spawning results. Please open the Gaia Manager and configure Gaia for the correct render pipeline that you are using.", "Continue Anyways", "Cancel"))
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }


            try
            {


                //if (!m_spawner.m_useExistingTerrainForWorldMapExport)
                //{
                    bool hasTerrains = false;
                    if (GaiaUtils.GetTerrainObject(false) != null || GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        hasTerrains = true;
                    }

                    if (hasTerrains && !EditorUtility.DisplayDialog("Create new World", "WARNING: You are about to export the world map to a new terrain setup. This will delete all your existing terrains and build a new world from scratch. Continue?", "OK", "Cancel"))
                    {
                        return;
                    }

                    //Deactivate the input terrain, if any so we do not stamp on it
                    if (m_spawner.m_baseTerrainSettings.m_baseTerrainInputType == BaseTerrainInputType.ExistingTerrain && m_spawner.m_baseTerrainSettings.m_inputTerrain != null)
                    {
                        m_spawner.m_baseTerrainSettings.m_inputTerrain.gameObject.SetActive(false);
                    }


                    //Deactivate the preview during the export
                    Stamper baseTerrainStamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                    GaiaSessionManager.ClearWorld(true);
                    ProgressBar.Show(ProgressBarPriority.WorldCreation, "Generating World", "Creating Terrain Tiles for the Export...");

                    //Shift everything back to 0,0,0 if required
                    if (GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        TerrainLoaderManager.Instance.SetOrigin(Vector3Double.zero);
                    }
                    //Create a new world according to the settings set by the user
                    WorldCreationSettings wcs = m_spawner.m_worldCreationSettings;
                    float worldsizeX = wcs.m_xTiles * wcs.m_tileSize;
                    //wcs.m_tileHeight = m_spawner.m_worldMapTerrain.terrainData.size.y / (m_spawner.m_worldMapTerrain.terrainData.size.x / worldsizeX);
                    TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMaprelativeSize = worldsizeX / TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange;
                    wcs.m_seaLevel = Mathf.RoundToInt(SessionManager.GetSeaLevel()); //* (m_spawner.m_worldMapTerrain.terrainData.size.x / worldsizeX));
                    wcs.m_spawnerPresetList = m_spawner.m_BiomeSpawnersToCreate;

                    //Add the world map stamper settings to the world creation settings so we can create and stamp in one go
                    m_gaiaSettings.m_biomePresetSelection = m_spawner.m_worldDesignerBiomePresetSelection;
                    m_gaiaSettings.m_BiomeSpawnersToCreate = m_spawner.m_BiomeSpawnersToCreate;
                    ProgressBar.Show(ProgressBarPriority.WorldCreation, "Generating World", "Preparing...");
                    //m_spawner.FitToTerrain(m_spawner.m_worldMapTerrain);
                    m_spawner.m_useExistingTerrainForWorldMapExport = true;
                    m_spawner.m_lastExportedWorldSize = wcs.m_tileSize * wcs.m_xTiles;
                    m_spawner.m_lastExportedHeightMapResolution = wcs.m_gaiaDefaults.m_heightmapResolution;
                    List<StamperSettings> combinedStamperSettings = new List<StamperSettings>();
                    WorldMapStampSettings worldMapStampSettings = ScriptableObject.CreateInstance<WorldMapStampSettings>();
                    worldMapStampSettings.m_worldMapSize = Mathf.RoundToInt(TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange * 2);
                    worldMapStampSettings.m_targetWorldSize = wcs.m_tileSize * wcs.m_xTiles;
                    worldMapStampSettings.m_tiles = wcs.m_xTiles;
                    worldMapStampSettings.m_tilesize = wcs.m_tileSize;
                    worldMapStampSettings.m_tileHeight = wcs.m_tileHeight;
                    worldMapStampSettings.m_heightmapResolution= wcs.m_gaiaDefaults.m_heightmapResolution;
                    worldMapStampSettings.m_baseTerrainStamperSettings = baseTerrainStamper.m_settings;
                    worldMapStampSettings.m_baseTerrainStamperSettings.m_y *= 2f; 
                    worldMapStampSettings.m_minWorldHeight = baseTerrainStamper.m_minCurrentTerrainHeight;
                    worldMapStampSettings.m_maxWorldHeight = baseTerrainStamper.m_maxCurrentTerrainHeight;
                    //set up the base terrain stamper to work in "Set Height" mode to override the existing heightmaps on the terrains (if any)
                    worldMapStampSettings.m_baseTerrainStamperSettings.m_operation = FeatureOperation.SetHeight;
                    worldMapStampSettings.m_stamperSettingsList = m_spawner.m_worldMapStamperSettings;

                    m_spawner.m_worldCreationSettings.m_worldMapStampSettings = worldMapStampSettings;

                    //Subscribe for the world creation event so we can wait with the export until the world was created in the coroutine
                    GaiaSessionManager.OnWorldCreated -= OnWorldMapStampingFinished;
                    GaiaSessionManager.OnWorldCreated += OnWorldMapStampingFinished;
                    GaiaSessionManager.CreateOrUpdateWorld(m_spawner.m_worldCreationSettings, true);
                    m_spawner.m_ExportRunning = true;

            }
            catch (Exception ex)
            {
                Debug.LogError("Error while exporting terrain from world map, Exception: " + ex.Message + " Stack Trace: " + ex.StackTrace);
            }
        }

        private void DoExport()
        {
            try
            {
                Stamper baseTerrainStamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                m_gaiaSettings.m_biomePresetSelection = m_spawner.m_worldDesignerBiomePresetSelection;
                m_gaiaSettings.m_BiomeSpawnersToCreate = m_spawner.m_BiomeSpawnersToCreate;
                ProgressBar.Show(ProgressBarPriority.WorldCreation, "Export To World", "Stamping heightmaps...");
                //m_spawner.FitToTerrain(m_spawner.m_worldMapTerrain);
                WorldCreationSettings wcs = m_spawner.m_worldCreationSettings;
                m_spawner.m_useExistingTerrainForWorldMapExport = true;
                m_spawner.m_lastExportedWorldSize = wcs.m_tileSize * wcs.m_xTiles;
                m_spawner.m_lastExportedHeightMapResolution = wcs.m_gaiaDefaults.m_heightmapResolution;
                List<StamperSettings> combinedStamperSettings = new List<StamperSettings>();
                WorldMapStampSettings worldMapStampSettings = ScriptableObject.CreateInstance<WorldMapStampSettings>();
                worldMapStampSettings.m_worldMapSize = Mathf.RoundToInt(TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange * 2);
                worldMapStampSettings.m_targetWorldSize = wcs.m_tileSize * wcs.m_xTiles;
                worldMapStampSettings.m_tiles = wcs.m_xTiles;
                worldMapStampSettings.m_baseTerrainStamperSettings = baseTerrainStamper.m_settings;
                worldMapStampSettings.m_minWorldHeight = baseTerrainStamper.m_minCurrentTerrainHeight;
                worldMapStampSettings.m_maxWorldHeight = baseTerrainStamper.m_maxCurrentTerrainHeight;
                //set up the base terrain stamper to work in "Set Height" mode to override the existing heightmaps on the terrains (if any)
                worldMapStampSettings.m_baseTerrainStamperSettings.m_operation = FeatureOperation.SetHeight;
                worldMapStampSettings.m_stamperSettingsList = m_spawner.m_worldMapStamperSettings;

                //iterate over the terrains created during the world creation phase, then stamp the current stamper settings in world space onto them, 
                //taking the different scale etc. between world map and actual terrains into account.
                GaiaSessionManager.StampFromWorldMap(worldMapStampSettings, true, null);
                
                GaiaSessionManager.OnWorldMapStampingFinished -= OnWorldMapStampingFinished;
                GaiaSessionManager.OnWorldMapStampingFinished += OnWorldMapStampingFinished;

                GaiaSessionManager.OnWorldCreationCancelled -= OnWorldCreationCancelled;
                GaiaSessionManager.OnWorldCreationCancelled += OnWorldCreationCancelled;
                //GaiaSessionManager.ExportWorldMapToLocalMap(wcs, true);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while exporting terrain from world map, Exception: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                ProgressBar.Clear(ProgressBarPriority.WorldCreation);
            }
            finally
            {
                ProgressBar.Clear(ProgressBarPriority.WorldCreation);
                GaiaSessionManager.OnWorldCreated -= DoExport;
            }
        }

        private void OnWorldCreationCancelled()
        {
            GaiaSessionManager.OnWorldCreationCancelled -= OnWorldCreationCancelled;
            GaiaSessionManager.OnWorldMapStampingFinished -= OnWorldMapStampingFinished;
            m_spawner.m_ExportRunning = false;
            Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(false);
            stamper.m_useCustomPreviewBounds = false;
            m_spawner.RefreshWorldDesignerStamps();
        }

        private void OnWorldMapStampingFinished()
        {
            if (m_spawner == null)
            {
                return;
            }
            //remember the biome controller that will be created (if any) to select it later
            BiomeController bc = null;
            try
            {
                //If no player exists yet, add a basic Layer culling script to the main camera, and apply settings to the scene camera.
                if (GaiaUtils.GetPlayerObject(false) == null)
                {
                    Camera cam = Camera.main;

                    if (cam != null)
                    {
                        if (cam.gameObject.GetComponent<SimpleCameraLayerCulling>() == null)
                        {
                            SimpleCameraLayerCulling sclc = cam.gameObject.AddComponent<SimpleCameraLayerCulling>();
                            sclc.m_applyToGameCamera = true;
                            sclc.m_applyToSceneCamera = true;
                            sclc.Initialize();
                            foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                            {
                                sceneCamera.layerCullDistances = sclc.m_profile.m_layerDistances;
                            }
                            sclc.ApplyToGameCamera();
                        }
                    }
                }

                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }

                TerrainLoaderManager.Instance.SwitchToLocalMap();
                //Set up a default loading range
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    Double regularRange = TerrainLoaderManager.GetDefaultLoadingRangeForTilesize(TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesSize);
                    TerrainLoaderManager.Instance.SetLoadingRange(regularRange, regularRange * 3f);
                    SessionManager.BackupOriginAndLoadRange();
                }
                TerrainLoaderManager.Instance.RefreshSceneViewLoadingRange();
                Stamper baseTerrainStamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                m_spawner.m_ExportRunning = false;
                baseTerrainStamper.m_drawPreview = true;
                m_spawner.m_baseTerrainSettings.m_drawPreview = true;
                baseTerrainStamper.m_stampDirty = true;
                baseTerrainStamper.m_activatePreviewRequested = false;
#if GAIA_PRO_PRESENT
                WorldOriginEditor.m_sessionManagerExits = true;
#else
                Gaia2TopPanel.m_sessionManagerExits = true;
#endif
                BiomePresetDropdownEntry selectedPresetEntry = m_allBiomePresets.Find(x => x.ID == m_gaiaSettings.m_biomePresetSelection);
                try
                {
                    ProgressBar.Show(ProgressBarPriority.CreateSceneTools, "Creating Tools", "Creating Biome", 0, 2);
                    List<Spawner> createdSpawners = GaiaManagerEditor.CreateBiome(selectedPresetEntry);
                    if (createdSpawners != null && createdSpawners.Count > 0)
                    {
                        Transform parent = createdSpawners[0].transform.parent;
                        if (parent != null)
                        {
                            bc = parent.GetComponent<BiomeController>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error while creating the biome controlles in the scene, Exception: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                }
                finally
                {
#if HDPipeline
                                    GaiaLighting.EnableNewSceneQuickBake(false);
#endif
                    ProgressBar.Clear(ProgressBarPriority.CreateSceneTools);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while creating tools after terrain creation. Message: " + e.Message + " Stack Trace: " + e.StackTrace);
            }
            finally
            {
                if (bc != null)
                {
                    Selection.activeObject = bc.gameObject;
                }
                else
                {
                    //No biome controller created? Try to select the first one we find
                    bc = GaiaUtils.FindOOT<BiomeController>();
                    if (bc != null)
                    {
                        Selection.activeObject = bc.gameObject;
                    }
                    else
                    {
                        //Select nothing then, so that the terrains are visible at least
                        Selection.activeObject = null;
                    }
                }
                //If we got a biome controller, switch it to world mode -> less confusing when doint multiterrain worlds.
                if (bc != null)
                {
                    bc.m_autoSpawnerArea = AutoSpawnerArea.World;
                }

                //Mark the terrains for stitching again, or run the stitching process again if not using terrain loading
                //- the stitching result might be slightly different now, since not all terrains might have been there when the first stitch occured.
                GaiaTerrainStitcher.m_extraSeamSize = 5;
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    foreach (TerrainScene ts in TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes)
                    {
                        ts.m_stitchedNorthBorder = false;
                        ts.m_stitchedEastBorder = false;
                        ts.m_stitchedSouthBorder = false;
                        ts.m_stitchedWestBorder = false;
                    }
                }
                else
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        GaiaTerrainStitcher.StitchTerrain(t);
                    }
                }
                GaiaTerrainStitcher.m_extraSeamSize = 1;

                Debug.Log("Finished at: " + DateTime.Now.ToLongTimeString());
                ProgressBar.Clear(ProgressBarPriority.CreateSceneTools);
                GaiaSessionManager.OnWorldMapStampingFinished -= OnWorldMapStampingFinished;
            }
        }

        private void OnMassStampingFinished()
        {
            m_spawner.m_ExportRunning = false;
            ProgressBar.Clear(ProgressBarPriority.WorldCreation);
            GaiaSessionManager.OnMassStampingFinished -= OnMassStampingFinished;
        }

        private void DrawAppearance(bool showHelp)
        {
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
#if GAIA_PRO_PRESENT
                bool currentGUIState = GUI.enabled;
                if (!TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainLoadingEnabled)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("AutoLoadTerrainsDisabled"), MessageType.Warning);
                    GUI.enabled = false;
                }
                Color originalColor = GUI.backgroundColor;

                if (m_spawner.m_highlightLoadingSettings)
                {
                    GUI.backgroundColor = GaiaUtils.GetColorFromHTML(GaiaConstants.TerrainLoadingSettingsHighlightColor); ;
                }
                m_spawner.m_loadTerrainMode = (LoadMode)m_editorUtils.EnumPopup("AutoLoadTerrains", m_spawner.m_loadTerrainMode, showHelp);
                m_spawner.m_impostorLoadingRange = m_editorUtils.IntField("ImpostorLoadingRange", m_spawner.m_impostorLoadingRange, showHelp);
                GUI.enabled = currentGUIState;
                GUI.backgroundColor = originalColor;
#endif
            }
            m_spawner.m_showSeaLevelPlane = m_editorUtils.Toggle("ShowSeaLevelPlane", m_spawner.m_showSeaLevelPlane, showHelp);
            m_spawner.m_showSeaLevelinStampPreview = m_editorUtils.Toggle("ShowSeaLevelSpawnerPreview", m_spawner.m_showSeaLevelinStampPreview, showHelp);
            //Color gizmoColour = EditorGUILayout.ColorField(GetLabel("Gizmo Colour"), m_stamper.m_gizmoColour);
            //alwaysShow = m_editorUtils.Toggle("AlwaysShowStamper", m_stamper.m_alwaysShow, showHelp);
            m_spawner.m_showBoundingBox = m_editorUtils.Toggle("ShowBoundingBox", m_spawner.m_showBoundingBox, showHelp);
            //showRulers = m_stamper.m_showRulers = m_editorUtils.Toggle("ShowRulers", m_stamper.m_showRulers, showHelp);
            //bool showTerrainHelper = m_stamper.m_showTerrainHelper = EditorGUILayout.Toggle(GetLabel("Show Terrain Helper"), m_stamper.m_showTerrainHelper);
        }

        private void DrawSeaLevelSlider(bool showHelp)
        {
            //m_editorUtils.LabelField("SeaLevel", new GUIContent(SessionManager.GetSeaLevel().ToString() + " m"), showHelp);

            float maxSeaLevel = 2000f;
            if (m_spawner.GetCurrentTerrain() != null)
            {
                maxSeaLevel = m_spawner.GetCurrentTerrain().terrainData.size.y;
            }
            else
            {
                maxSeaLevel = SessionManager.GetSeaLevel() + 500f;
            }

            float oldSeaLevel = SessionManager.GetSeaLevel();
            float newSeaLEvel = oldSeaLevel;
            newSeaLEvel = m_editorUtils.Slider("SeaLevel", newSeaLEvel, 0, maxSeaLevel, showHelp);
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
                    SessionManager.SetSeaLevel(newSeaLEvel, false);
                    SceneView.RepaintAll();
                }
                m_spawner.UpdateMinMaxHeight();
            }
            //update associated base terrain stamper (if any)
            if (m_spawner.GetOrCreateBaseTerrainStamper(false) != null)
            {
                m_spawner.GetOrCreateBaseTerrainStamper(false).m_seaLevel = newSeaLEvel;
            }
        }

        private void DrawResourceDropBox(bool helpEnabled)
        {
            if (m_boxStyle == null || m_boxStyle.normal.background == null)
            {
                m_boxStyle = new GUIStyle(m_spawnRulesBorder);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontSize = GUI.skin.label.fontSize;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (m_ResourceManagementMessage != "")
            {
                EditorGUILayout.HelpBox(m_ResourceManagementMessage, m_ResourceManagementMessageType, true);
            }

            m_spawner.m_dropAreaResource = (GaiaConstants.DroppableResourceType)EditorGUILayout.EnumPopup(m_editorUtils.GetContent("DropAreaResourceType"), m_spawner.m_dropAreaResource);
            m_editorUtils.InlineHelp("DropAreaResourceType", helpEnabled);

            GUIContent dropAreaContent = null;

            switch (m_spawner.m_dropAreaResource)
            {
                case DroppableResourceType.TerrainTexture:
                    dropAreaContent = m_editorUtils.GetContent("DropAreaTextureText");
                    break;
                case DroppableResourceType.TerrainDetail:
                    dropAreaContent = m_editorUtils.GetContent("DropAreaTerrainDetailText");
                    break;
                case DroppableResourceType.TerrainTree:
                    dropAreaContent = m_editorUtils.GetContent("DropAreaTreeText");
                    break;
                case DroppableResourceType.GameObject:
                    dropAreaContent = m_editorUtils.GetContent("DropAreaGameObjectText");
                    break;
            }



            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 75.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, dropAreaContent, m_boxStyle);


            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        m_ResourceManagementMessage = "";
                        switch (m_spawner.m_dropAreaResource)
                        {
                            case DroppableResourceType.TerrainTexture:
                                bool notATexture = false;
                                bool textureImported = false;
                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    if (dragged_object.GetType() == typeof(Texture2D))
                                    {
                                        Texture2D importedTexture = (Texture2D)dragged_object;

                                        SpawnRule newRule = new SpawnRule();
                                        newRule.m_resourceType = SpawnerResourceType.TerrainTexture;
                                        newRule.m_name = importedTexture.name;
                                        newRule.m_resourceIdx = AddNewTextureResource();
                                        //Get new reource and fill it with layer data
                                        ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[newRule.m_resourceIdx];
                                        protoTexture.m_name = importedTexture.name;
                                        protoTexture.m_texture = importedTexture;
                                        newRule.m_imageMasks = new ImageMask[0];
                                        newRule.m_isFoldedOut = false;
                                        newRule.m_resourceSettingsFoldedOut = false;
                                        m_spawner.m_rulePanelUnfolded = true;
                                        m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                        m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                        AddNewMaskList();
                                        textureImported = true;
                                    }
                                    else
                                    {
                                        notATexture = true;
                                    }
                                }
                                if (notATexture && !textureImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Error;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaNotATexture");
                                }
                                if (notATexture && textureImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Warning;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaSomeNotATexture");
                                }
                                break;
                            case DroppableResourceType.TerrainDetail:
                                bool notADetail = false;
                                bool detailImported = false;
                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    if (dragged_object.GetType() == typeof(Texture2D) || dragged_object.GetType() == typeof(GameObject))
                                    {
                                        SpawnRule newRule = new SpawnRule();
                                        newRule.m_resourceType = SpawnerResourceType.TerrainDetail;
                                        newRule.m_resourceIdx = AddNewTerrainDetailResource();
                                        ResourceProtoDetail protoDetail = m_spawner.m_settings.m_resources.m_detailPrototypes[newRule.m_resourceIdx];

                                        if (dragged_object.GetType() == typeof(Texture2D))
                                        {
                                            Texture2D importedTexture = (Texture2D)dragged_object;
                                            protoDetail.m_renderMode = DetailRenderMode.Grass;
                                            protoDetail.m_detailTexture = importedTexture;
                                            newRule.m_name = importedTexture.name;
                                            protoDetail.m_name = importedTexture.name;
                                        }
                                        else
                                        {
                                            GameObject importedGO = (GameObject)dragged_object;
                                            protoDetail.m_renderMode = DetailRenderMode.VertexLit;
                                            protoDetail.m_detailProtoype = importedGO;
                                            newRule.m_name = importedGO.name;
                                            protoDetail.m_name = importedGO.name;
                                        }

                                        //Get new reource and fill it with layer data
                                        newRule.m_imageMasks = new ImageMask[0];
                                        newRule.m_isFoldedOut = false;
                                        newRule.m_resourceSettingsFoldedOut = false;
                                        m_spawner.m_rulePanelUnfolded = true;
                                        m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                        m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                        AddNewMaskList();
                                        detailImported = true;
                                    }
                                    else
                                    {
                                        notADetail = true;
                                    }
                                }
                                if (notADetail && !detailImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Error;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaNotADetail");
                                }
                                if (notADetail && detailImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Warning;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaSomeNotADetail");
                                }
                                break;
                            case DroppableResourceType.TerrainTree:
                                bool notATree = false;
                                bool treeImported = false;
                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    if (dragged_object.GetType() == typeof(GameObject))
                                    {
                                        GameObject treePrototype = (GameObject)dragged_object;
                                        SpawnRule newRule = new SpawnRule();
                                        newRule.m_resourceType = SpawnerResourceType.TerrainTree;
                                        newRule.m_name = treePrototype.name;
                                        newRule.m_resourceIdx = AddNewTreeResource();
                                        ResourceProtoTree protoTree = m_spawner.m_settings.m_resources.m_treePrototypes[newRule.m_resourceIdx];
                                        protoTree.m_name = treePrototype.name;
                                        protoTree.m_desktopPrefab = treePrototype;
                                        newRule.m_imageMasks = new ImageMask[0];
                                        newRule.m_isFoldedOut = false;
                                        newRule.m_resourceSettingsFoldedOut = false;
                                        m_spawner.m_rulePanelUnfolded = true;
                                        m_spawner.m_settings.m_spawnerRules.Add(newRule);
                                        m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
                                        AddNewMaskList();
                                        treeImported = true;
                                    }
                                    else
                                    {
                                        notATree = true;
                                    }
                                }
                                if (notATree && !treeImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Error;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaNotATree");
                                }
                                if (notATree && treeImported)
                                {
                                    m_ResourceManagementMessageType = MessageType.Warning;
                                    m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaSomeNotATree");
                                }
                                break;
                            case DroppableResourceType.GameObject:
                                //Work out if we have prefab instances or prefab objects
                                bool havePrefabInstances = false;
                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(dragged_object);

                                    if (pt == PrefabAssetType.Regular || pt == PrefabAssetType.Model)
                                    {
                                        havePrefabInstances = true;
                                        break;
                                    }
                                }

                                if (havePrefabInstances)
                                {
                                    List<GameObject> prototypes = new List<GameObject>();

                                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                    {
                                        PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(dragged_object);

                                        if (pt == PrefabAssetType.Regular || pt == PrefabAssetType.Model)
                                        {
                                            prototypes.Add(dragged_object as GameObject);
                                        }
                                        else
                                        {
                                            m_ResourceManagementMessageType = MessageType.Error;
                                            m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaOnlyPrefabInstances");
                                        }
                                    }

                                    //Same them as a single entity
                                    if (prototypes.Count > 0)
                                    {
                                        m_spawner.m_settings.m_resources.AddGameObject(prototypes);

                                        //Create a new rule from the newly added gameobject
                                        AddNewRule(SpawnerResourceType.GameObject, m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Length - 1);

                                    }
                                }
                                else
                                {
                                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                    {
                                        if (PrefabUtility.GetPrefabAssetType(dragged_object) == PrefabAssetType.Regular)
                                        {
                                            m_spawner.m_settings.m_resources.AddGameObject(dragged_object as GameObject);
                                            AddNewRule(SpawnerResourceType.GameObject, m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Length - 1);
                                        }
                                        else
                                        {
                                            //Debug.LogWarning("You may only add prefabs or game objects attached to prefabs!");
                                            m_ResourceManagementMessageType = MessageType.Error;
                                            m_ResourceManagementMessage = m_editorUtils.GetTextValue("DropAreaOnlyPrefabsOrGameObjects");
                                        }
                                    }
                                }

                                break;
                        }
                        ImageMask.RefreshSpawnRuleGUIDs();




                    }
                    break;

            }

            m_editorUtils.InlineHelp("DropArea", helpEnabled);

        }

        private void DrawSpawnControls()
        {
            GUILayout.Space(5);
            //Regardless, re-enable the spawner controls
            GUI.enabled = true;

            GUILayout.BeginVertical();
            if (m_spawner.m_spawnProgress > 0f && m_spawner.m_spawnProgress < 1f)
            {
                GUI.enabled = true;
                if (m_editorUtils.Button("Cancel"))
                {
                    m_spawner.CancelSpawn();
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                Color normalBGColor = GUI.backgroundColor;
                GUI.backgroundColor = m_gaiaSettings.GetActionButtonColor();
                if (m_editorUtils.Button("Spawn Local"))
                {

                    SanityChecks();

                    Spawn(false);
                    GUIUtility.ExitGUI();
                }
                GUILayout.Space(5);
                if (m_editorUtils.Button("Spawn World"))
                {
                    SanityChecks();

                    Spawn(true);
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = normalBGColor;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void SanityChecks()
        {
            //Check if there are any terrains in the scene that don't use "Draw Instanced"
            if (Terrain.activeTerrains.Where(x => x.drawInstanced == false).Count() > 0)
            {
                if (!EditorUtility.DisplayDialog("Draw Instanced Warning", "This scene contains terrains that have the setting 'Draw Instanced' turned off. This can lead to wrong spawn results when using certain masks. Please enable 'Draw Instanced' in the terrain inspector settings tab on all terrains.", "Continue Anyways", "Cancel"))
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            if (!GaiaUtils.UsesCorrectPipelineDefines())
            {
                if (!EditorUtility.DisplayDialog("Wrong Render Pipeline Configured", "This project does use a different render pipeline asset in the graphics settings than what Gaia is configured for. This can lead to wrong spawn results. Please open the Gaia Manager and configure Gaia for the correct render pipeline that you are using.", "Continue Anyways", "Cancel"))
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }

        }

        private void DrawWorldDesignerSpawnControls()
        {
                bool currentGUIState = GUI.enabled;
                if (m_spawner.m_ExportRunning)
                {
                    //do not allow tampering while export is running
                    GUI.enabled = false;
                }
                Color currentColor = GUI.backgroundColor;

                float buttonWidth = (EditorGUIUtility.currentViewWidth-44) * 0.5f;
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (m_editorUtils.Button("ButtonSpawnStamps", GUILayout.Width(buttonWidth)))
                    {
                        if (m_spawner.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
                        {
                            ZoomOut();
                        }
                        m_spawner.SpawnStamps();
                        m_spawner.StoreWorldSize();
                        EditorGUIUtility.ExitGUI();
                    }
                     GUILayout.Space(5);
                    if (m_editorUtils.Button("ButtonRandomizeWorld", GUILayout.Width(buttonWidth)))
                    {
                        if (m_spawner.m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
                        {
                            ZoomOut();
                        }
                        m_spawner.RandomizeWorldDesigner();
                        TerrainLoaderManager.Instance.SwitchToWorldMap();
                        m_spawner.SpawnStamps();
                        m_spawner.StoreWorldSize();
                        EditorGUIUtility.ExitGUI();
                    }
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (m_editorUtils.Button("ButtonClearStamps", GUILayout.Width(buttonWidth)))
                    {
                        m_spawner.ClearStampDistributions();
                    }
                    GUILayout.Space(5);
                    if (m_editorUtils.Button("ButtonResetToDefaults", GUILayout.Width(buttonWidth)))
                    {
                        m_spawner.ResetWorldDesigner();
                    }
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (m_spawner.m_ExportRunning)
                {
                    //always allow cancel
                    GUI.enabled = true;
                    if (m_editorUtils.Button("CancelExport"))
                    {
                        SessionManager.m_massStamperSettingsIndex = int.MaxValue;
                        m_spawner.m_ExportRunning = false;
                        OnWorldCreationCancelled();
                    }
                    GUI.enabled = currentGUIState;
                }
                else
                {
                    GUI.backgroundColor = m_gaiaSettings.GetActionButtonColor();
                    if (m_editorUtils.Button("ExportTerrainButtonExport"))
                    {
                        StartExport();
                    }
                    GUI.backgroundColor = currentColor;
                }

                GUI.enabled = currentGUIState;
                GUI.backgroundColor = currentColor;
                EditorGUILayout.EndHorizontal();
        }


        private void DrawClearControls(bool helpEnabled)
        {
            GUILayout.BeginVertical();
            m_editorUtils.Label("ClearSpawnsLabel");
            EditorGUI.indentLevel++;
            m_spawner.m_settings.m_clearSpawnsToggleTrees = m_editorUtils.Toggle("ClearSpawnsToggleTrees", m_spawner.m_settings.m_clearSpawnsToggleTrees, helpEnabled);
            m_spawner.m_settings.m_clearSpawnsToggleDetails = m_editorUtils.Toggle("ClearSpawnsToggleDetails", m_spawner.m_settings.m_clearSpawnsToggleDetails, helpEnabled);
            m_spawner.m_settings.m_clearSpawnsToggleGOs = m_editorUtils.Toggle("ClearSpawnsToggleGOs", m_spawner.m_settings.m_clearSpawnsToggleGOs, helpEnabled);
            m_spawner.m_settings.m_clearSpawnsToggleSpawnExtensions = m_editorUtils.Toggle("ClearSpawnsToggleSpawnExtensions", m_spawner.m_settings.m_clearSpawnsToggleSpawnExtensions, helpEnabled);
            m_spawner.m_settings.m_clearSpawnsToggleProbes = m_editorUtils.Toggle("ClearSpawnsToggleProbes", m_spawner.m_settings.m_clearSpawnsToggleProbes, helpEnabled);
            EditorGUI.indentLevel--;
            m_spawner.m_settings.m_clearSpawnsFrom = (ClearSpawnFrom)m_editorUtils.EnumPopup("ClearSpawnsFrom", m_spawner.m_settings.m_clearSpawnsFrom, helpEnabled);
            m_spawner.m_settings.m_clearSpawnsFor = (ClearSpawnFor)m_editorUtils.EnumPopup("ClearSpawnsFor", m_spawner.m_settings.m_clearSpawnsFor, helpEnabled);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button(m_editorUtils.GetContent("ClearSpawnsButton")))
            {
                ClearOperationSettings clearOperationSettings = ScriptableObject.CreateInstance<ClearOperationSettings>();
                clearOperationSettings.m_clearTrees = m_spawner.m_settings.m_clearSpawnsToggleTrees;
                clearOperationSettings.m_clearTerrainDetails = m_spawner.m_settings.m_clearSpawnsToggleDetails;
                clearOperationSettings.m_clearGameObjects = m_spawner.m_settings.m_clearSpawnsToggleGOs;
                clearOperationSettings.m_clearSpawnExtensions = m_spawner.m_settings.m_clearSpawnsToggleSpawnExtensions;
                clearOperationSettings.m_clearProbes = m_spawner.m_settings.m_clearSpawnsToggleProbes;
                clearOperationSettings.m_clearSpawnFrom = m_spawner.m_settings.m_clearSpawnsFrom;
                clearOperationSettings.m_clearSpawnFor = m_spawner.m_settings.m_clearSpawnsFor;

                GaiaSessionManager.ClearSpawns(clearOperationSettings, m_spawner.m_settings, true, m_spawner);


            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a Panel designated to hold multiple spawn rules that can be activated / deactivated together, deleted all at once, etc.
        /// </summary>
        /// <param name="nameKey">Key to a name text in the translations</param>
        /// <param name="contentMethod">The content method that is used to display the contents of a single spawn rule</param>
        /// <param name="toggleAllState">The state of the "toggle all" checkbox that activates / deactivates all rules.</param>
        /// <param name="unfolded">Whether the panel is folded / unfolded.</param>
        /// <returns></returns>
        private bool DrawSpawnRulesPanel(string nameKey, Action<bool> contentMethod, ref bool toggleAllState, bool unfolded = true)
        {
            GUIContent panelLabel = m_editorUtils.GetContent(nameKey);

            //Panel Label
            GUIStyle panelLabelStyle = new GUIStyle(GUI.skin.label);
            panelLabelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelLabelStyle.fontStyle = FontStyle.Bold;
            panelLabelStyle.normal.background = GUI.skin.label.normal.background;
            panelLabelStyle.margin = new RectOffset(0, 0, 2, 0);

            GUIStyle panelCheckBoxStyle = new GUIStyle(GUI.skin.toggle);
            panelCheckBoxStyle.margin = new RectOffset(3, 0, 3, 0);

            // Panel Frame
            GUIStyle panelFrameStyle = new GUIStyle(GUI.skin.box);
            panelFrameStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelFrameStyle.fontStyle = FontStyle.Bold;
            panelFrameStyle.alignment = TextAnchor.UpperLeft;

            // Panel
            GUIStyle panelStyle = new GUIStyle(GUI.skin.label);
            panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelStyle.alignment = TextAnchor.UpperLeft;

            bool isBiomeMaskPanel = false;
            bool isStampSpawnPanel = false;

            switch (contentMethod.Method.Name)
            {
                case "DrawBiomeMaskSpawnRules":
                    isBiomeMaskPanel = true;
                    //unfolded = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask).Count() > 0 ? m_spawner.m_worldBiomeMasksUnfolded : true;
                    break;
                case "DrawWorldDetail":
                    isStampSpawnPanel = true;
                    //unfolded = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.StampDistribution).Count() > 0 ? m_spawner.m_spawnStampsPanelUnfolded : true;
                    break;
                default:
                    //Always unfold if we have no rules yet, to show the prompt to add new stuff
                    if (!m_spawner.m_settings.m_isWorldmapSpawner)
                    {
                        unfolded = m_spawner.m_settings.m_spawnerRules.Count > 0 ? m_spawner.m_rulePanelUnfolded : true;
                    }
                    else
                    {
                        unfolded = m_spawner.m_rulePanelUnfolded;
                    }
                    break;
            }

            bool helpActive = m_rulePanelHelpActive;

            GUILayout.BeginVertical(m_spawnRulesBorder);
            {
                GUILayout.BeginHorizontal(m_spawnRulesHeader);
                {
                    if (isBiomeMaskPanel || isStampSpawnPanel)
                    {
                        GUILayout.Space(4);
                    }
                    else
                    {
                        GUILayout.Space(10);
                    }
                    unfolded = GUILayout.Toggle(unfolded, unfolded ? "-" : "+", panelLabelStyle, GUILayout.MinWidth(14));

                    if (!isBiomeMaskPanel && !isStampSpawnPanel) 
                    {
                        bool oldToggleAllState = toggleAllState;
                        toggleAllState = GUILayout.Toggle(toggleAllState, new GUIContent("", "Activates / Deactivates all the Rules at once"), panelCheckBoxStyle);

                        if (toggleAllState != oldToggleAllState)
                        {
                            List<SpawnRule> allRelevantRules = m_spawner.m_settings.m_spawnerRules;
                            switch (contentMethod.Method.Name)
                            {
                                case "DrawBiomeMaskSpawnRules":
                                    allRelevantRules = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask).ToList();
                                    break;
                                case "DrawStampSpawnRules":
                                    allRelevantRules = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.StampDistribution).ToList();
                                    break;
                            }

                            foreach (SpawnRule entry in allRelevantRules)
                            {
                                entry.m_isActive = toggleAllState;
                            }
                        }
                        GUILayout.Space(23);
                    }

                    unfolded = GUILayout.Toggle(unfolded, panelLabel, panelLabelStyle);
                    GUILayout.FlexibleSpace();

                    if (!isBiomeMaskPanel && !isStampSpawnPanel && EditorGUIUtility.currentViewWidth > 400)
                    {
                        GUIContent GCvisualizeIcon = null;
                        bool currentGUIState = GUI.enabled;
                        if (m_spawner.m_previewRuleIds.Count == 0)
                        {
                            GUI.enabled = false;
                        }

                        if (m_spawner.m_drawPreview)
                        {
                            GCvisualizeIcon = GaiaEditorUtils.GetIconGUIContent("IconVisibleDisabled", m_gaiaSettings.m_IconVisibleDisabled, m_gaiaSettings.m_IconProVisibleDisabled, m_editorUtils);
                        }
                        else
                        {
                            GCvisualizeIcon = GaiaEditorUtils.GetIconGUIContent("IconVisibleDisabled", m_gaiaSettings.m_IconVisible, m_gaiaSettings.m_IconProVisible, m_editorUtils);
                        }
                        if (m_editorUtils.Button(GCvisualizeIcon, m_visualizeAllButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            //as soon as the user interacts with the button, the user is in control, no need for auto activate anymore
                            m_activatePreviewRequested = false;
                            m_spawner.TogglePreview();
                            //force repaint
                            EditorWindow view = EditorWindow.GetWindow<SceneView>();
                            view.Repaint();
                        }
                        GUI.enabled = currentGUIState;

                    }
                    else
                    {
                        if (!isStampSpawnPanel)
                        {
                            m_editorUtils.Label("Ellipsis");
                        }
                    }
                    if (EditorGUIUtility.currentViewWidth > 400)
                    {
                        GUILayout.Space(85);
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                    }


                    if (!isStampSpawnPanel)
                    {
                        GUIContent XAllButton = GaiaEditorUtils.GetIconGUIContent("IconRemove", m_gaiaSettings.m_IconRemove, m_gaiaSettings.m_IconProRemove, m_editorUtils);
                        XAllButton.tooltip = "Deletes all spawn rules from the list below.";

                        if (m_editorUtils.Button(XAllButton, m_visualizeAllButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            if (EditorUtility.DisplayDialog("Delete All Rules ?", "Are you sure you want to delete all rules - this can not be undone ?", "Yes", "No"))
                            {
                                DeleteAllRules();
                            }
                        }
                    }
                    if (isBiomeMaskPanel || isStampSpawnPanel)
                    {
                        m_editorUtils.HelpToggle(ref helpActive);
                    }

                    GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();

                if (helpActive)
                {
                    GUILayout.Space(2f);
                    m_editorUtils.InlineHelp(nameKey, helpActive);
                }

                if (unfolded)
                {
                    GUILayout.BeginVertical(panelStyle);
                    {
                        bool noRulesCheck = false;

                        switch (contentMethod.Method.Name)
                        {
                            case "DrawBiomeMaskSpawnRules":
                                noRulesCheck = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask).Count() == 0;
                                break;
                            case "DrawStampSpawnRules":
                                noRulesCheck = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.StampDistribution).Count() == 0;
                                break;
                            default:
                                noRulesCheck = m_spawner.m_settings.m_spawnerRules.Count == 0;
                                break;
                        }

                        if (noRulesCheck)
                        {
                            GUILayout.Space(20);
                            //No rules yet, display a message to prompt the user to add a first resource
                            if (m_noRulesLabelStyle == null)
                            {
                                m_noRulesLabelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
                            }
                            switch (contentMethod.Method.Name)
                            {
                                case "DrawBiomeMaskSpawnRules":
                                    m_editorUtils.LabelField("NoBiomeMaskRulesYet", m_noRulesLabelStyle);
                                    break;
                                case "DrawStampSpawnRules":
                                    m_editorUtils.LabelField("NoStampRulesYet", m_noRulesLabelStyle);
                                    break;
                                default:
                                    m_editorUtils.LabelField("NoRulesYet", m_noRulesLabelStyle);
                                    break;
                            }


                            GUILayout.Space(20);
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            switch (contentMethod.Method.Name)
                            {
                                case "DrawBiomeMaskSpawnRules":
                                    bool currentGUIState = GUI.enabled;
                                    GUI.enabled = false;
                                    if (m_editorUtils.Button("CreateFirstBiomeMaskRule"))
                                    {
                                        AddNewRule(SpawnerResourceType.WorldBiomeMask);
                                    }
                                    GUI.enabled = currentGUIState;
                                    break;
                                case "DrawStampSpawnRules":
                                    if (m_editorUtils.Button("CreateFirstStampRule"))
                                    {
                                        AddNewRule(SpawnerResourceType.StampDistribution);
                                    }
                                    break;
                                default:
                                    if (m_editorUtils.Button("CreateFirstRule", GUILayout.Width(150)))
                                    {
                                        AddNewRule(SpawnerResourceType.GameObject);
                                    }
                                    if (!m_spawner.m_settings.m_isWorldmapSpawner)
                                    {
                                        GUILayout.Space(5);
                                        if (m_editorUtils.Button("OpenResourceManagement", GUILayout.Width(150)))
                                        {
                                            m_editorUtils.SetPanelStatus(DrawAdvanced, true, false);
                                            m_editorUtils.SetPanelStatus(DrawResourceManagement, true, false);
                                        }
                                    }
                                    break;
                            }
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();

                            //Always unfold when adding a new rule, we want to see & edit it
                            unfolded = true;
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Space(10);
                            EditorGUI.indentLevel++;
                            contentMethod.Invoke(helpActive);
                            EditorGUI.indentLevel--;
                            GUILayout.Space(10);
                        }
                        if (!noRulesCheck)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            bool currentGUIState = GUI.enabled;
                            if (isBiomeMaskPanel)
                            {
                                GUI.enabled = false;
                            }
                            if (m_editorUtils.Button("AddNewRuleButton", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f)))
                            {
                                //Add a rule of the same type of the last rule
                                AddNewRule(m_spawner.m_settings.m_spawnerRules[m_spawner.m_settings.m_spawnerRules.Count - 1].m_resourceType);
                            }
                            GUI.enabled = currentGUIState;
                            if (!m_spawner.m_settings.m_isWorldmapSpawner)
                            {
                                GUILayout.Space(5);
                                if (m_editorUtils.Button("OpenResourceManagement", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f)))
                                {
                                    m_editorUtils.SetPanelStatus(DrawAdvanced, true, false);
                                    m_editorUtils.SetPanelStatus(DrawResourceManagement, true, false);
                                }
                            }
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            GUILayout.Space(10);
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();

            switch (contentMethod.Method.Name)
            {
                case "DrawBiomeMaskSpawnRules":
                    break;
                case "DrawStampSpawnRules":
                    break;
                default:
                    m_spawner.m_rulePanelUnfolded = unfolded;
                    break;
            }


            m_rulePanelHelpActive = helpActive;
            return unfolded;
        }

        private void DeleteAllRules()
        {
            m_spawner.m_settings.m_spawnerRules.Clear();
            m_reorderableRuleMasksLists = new ReorderableList[0];
            m_spawner.m_previewRuleIds.Clear();
            PruneResources();
        }

        private void AddNewRule(SpawnerResourceType resourceType, int resourceID = -99)
        {
            SpawnRule newRule = new SpawnRule();

            //no resource ID given? Create a new one
            if (resourceID == -99)
            {
                newRule.m_name = GaiaConstants.newSpawnRuleName;

                if (m_spawner.m_settings.m_isWorldmapSpawner)
                {
                    newRule.m_resourceType = resourceType;
                }
                else
                {
                    //check the previous rule, if any. We assume the user wants to create another rule of the same resource type.
                    if (m_spawner.m_settings.m_spawnerRules.Count >= 1)
                    {
                        newRule.m_resourceType = m_spawner.m_settings.m_spawnerRules[m_spawner.m_settings.m_spawnerRules.Count - 1].m_resourceType;
                    }
                }
                //add a new resource prototype as well, we assume the user wants to add a new resource rather than re-use one
                switch (newRule.m_resourceType)
                {
                    case SpawnerResourceType.TerrainTexture:
                        newRule.m_resourceIdx = AddNewTextureResource();
                        break;
                    case SpawnerResourceType.TerrainTree:
                        newRule.m_resourceIdx = AddNewTreeResource();
                        break;
                    case SpawnerResourceType.TerrainDetail:
                        newRule.m_resourceIdx = AddNewTerrainDetailResource();
                        break;
                    case SpawnerResourceType.TerrainModifierStamp:
                        newRule.m_resourceIdx = AddNewTerrainModifierStampResource();
                        break;
                    case SpawnerResourceType.Probe:
                        newRule.m_resourceIdx = AddNewProbeResource();
                        break;
                    case SpawnerResourceType.GameObject:
                        newRule.m_resourceIdx = AddNewGameObjectResource();
                        break;
                    case SpawnerResourceType.SpawnExtension:
                        newRule.m_resourceIdx = AddNewSpawnExtensionResource();
                        break;
                    case SpawnerResourceType.StampDistribution:
                        newRule.m_resourceIdx = AddNewStampDistributionResource();
                        break;
                    case SpawnerResourceType.WorldBiomeMask:
                        newRule.m_resourceIdx = AddNewWorldBiomeMaskResource();
                        break;
                }
            }
            else
            {
                //use existing resource type / id
                newRule.m_resourceType = resourceType;
                switch (resourceType)
                {
                    case SpawnerResourceType.TerrainTexture:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.TerrainTree:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_treePrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.TerrainDetail:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.GameObject:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.SpawnExtension:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.StampDistribution:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceID].m_featureType;
                        break;
                    case SpawnerResourceType.WorldBiomeMask:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.Probe:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_probePrototypes[resourceID].m_name;
                        break;
                    case SpawnerResourceType.TerrainModifierStamp:
                        newRule.m_name = m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[resourceID].m_name;
                        break;
                }
                newRule.m_resourceIdx = resourceID;
            }

            newRule.m_imageMasks = new ImageMask[0];
            //always fold out the resource settings for new rules so user can edit directly
            newRule.m_isFoldedOut = true;
            newRule.m_resourceSettingsFoldedOut = true;
            m_spawner.m_rulePanelUnfolded = true;
            m_spawner.m_settings.m_spawnerRules.Add(newRule);
            m_previewImageDisplayedDuringLayout = GaiaUtils.AddElementToArray(m_previewImageDisplayedDuringLayout, false);
            AddNewMaskList();
            ImageMask.RefreshSpawnRuleGUIDs();
        }

        private void DrawSpawnSettings(bool helpEnabled)
        {
            bool currentGUIState = GUI.enabled;
            if (m_spawner.transform.name.Contains("New Spawner"))
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("NoUniqueName"), MessageType.Warning);
            }
            string oldName = m_spawner.transform.name;
            string newName = m_editorUtils.DelayedTextField("Name", m_spawner.transform.name);
            if (oldName != newName)
            {
                //Only apply a new name if it is actually different, otherwise affects editor performance
                m_spawner.transform.name = newName;
            }
            m_spawner.m_settings.m_spawnRange = m_editorUtils.Slider("Range", m_spawner.m_settings.m_spawnRange, 1, m_spawner.GetMaxSpawnRange(), helpEnabled);
            DrawSeaLevelSlider(helpEnabled);
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                if (SessionManager != null)
                {
                    SessionManager.m_session.m_spawnDensity = m_editorUtils.FloatField("SpawnDensity", Mathf.Max(0.01f, SessionManager.m_session.m_spawnDensity), helpEnabled);
                    m_spawner.m_settings.m_spawnDensity = SessionManager.m_session.m_spawnDensity;
                }
                else
                {
                    m_spawner.m_settings.m_spawnDensity = m_editorUtils.FloatField("SpawnDensity", Mathf.Max(0.01f, m_spawner.m_settings.m_spawnDensity), helpEnabled);
                }
            }
            m_spawner.m_settings.m_spawnMode = (SpawnMode)m_editorUtils.EnumPopup("SpawnMode", m_spawner.m_settings.m_spawnMode, helpEnabled);
            ImageMaskListEditor.DrawMaskList(ref m_spawnerMaskListExpanded, m_reorderableSpawnerMaskList, m_editorUtils, helpEnabled);
            GUILayout.BeginHorizontal();
            {
                if (m_editorUtils.Button("Fit To Terrain"))
                {
                    m_spawner.FitToTerrain();
                }
                GUILayout.Space(5);
                if (!m_fitToWorldAllowed)
                {
                    GUI.enabled = false;
                    m_editorUtils.Button("Fit To World (Inactive)");
                    GUI.enabled = currentGUIState;
                }
                else
                {
                    if (m_editorUtils.Button("Fit To World"))
                    {
                        m_spawner.FitToAllTerrains();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }


        private void DrawRegularSpawnRules(bool helpEnabled)
        {
            for (int ruleIdx = 0; ruleIdx < m_spawner.m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                m_spawner.m_spawnRuleIndexBeingDrawn = ruleIdx;
                SpawnRule rule = m_spawner.m_settings.m_spawnerRules[ruleIdx];
                if (rule.m_resourceType == SpawnerResourceType.GameObject ||
                    rule.m_resourceType == SpawnerResourceType.SpawnExtension ||
                    rule.m_resourceType == SpawnerResourceType.Probe ||
                    rule.m_resourceType == SpawnerResourceType.TerrainDetail ||
                    rule.m_resourceType == SpawnerResourceType.TerrainModifierStamp ||
                    rule.m_resourceType == SpawnerResourceType.TerrainTree ||
                    rule.m_resourceType == SpawnerResourceType.TerrainTexture)
                {
                    DrawSingleRulePanel(rule, ruleIdx);
                }
            } //for
        }

        private void DrawWorldDetail(bool helpEnabled)
        {
            EditorGUI.indentLevel--;
            if (m_spawner.HasWorldSizeChanged() && m_spawner.m_worldMapStamperSettings.Count>0)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ExportTerrainSizeSettingsChanged"), MessageType.Error);
            }

            bool currentGUIState = GUI.enabled;
            int oldDensity = m_spawner.m_settings.m_stampDensity;
            m_spawner.m_settings.m_stampDensity = m_editorUtils.IntSlider("StampDensity", m_spawner.m_settings.m_stampDensity, 1, 100, helpEnabled);
            int maxNumberOfStamps = (m_spawner.m_settings.m_stampDensity + 1) * (m_spawner.m_settings.m_stampDensity + 1);
            m_spawner.m_settings.m_stampJitter = m_editorUtils.Slider("StampJitter", m_spawner.m_settings.m_stampJitter * 100, 0, 100, helpEnabled) / 100;
            int oldWidth = m_spawner.m_settings.m_stampWidth;
            m_spawner.m_settings.m_stampWidth = m_editorUtils.IntSlider("StampWidth", m_spawner.m_settings.m_stampWidth, 0, 500, helpEnabled);
            m_spawner.m_settings.m_stampHeight = m_editorUtils.IntSlider("StampHeight", m_spawner.m_settings.m_stampHeight, 0, 500, helpEnabled);

            if(oldDensity != m_spawner.m_settings.m_stampDensity || oldWidth != m_spawner.m_settings.m_stampWidth)
            {
                //Density / Width changed, display visualizations for those for a few seconds
                m_spawner.m_autoStampDensityVisualizationTimeStamp = GaiaUtils.GetUnixTimestamp();
            }

            EditorGUI.indentLevel++;

            GUILayout.Space(10);

            for (int ruleIdx = 0; ruleIdx < m_spawner.m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                m_spawner.m_spawnRuleIndexBeingDrawn = ruleIdx;
                SpawnRule rule = m_spawner.m_settings.m_spawnerRules[ruleIdx];
                if (rule.m_resourceType == SpawnerResourceType.StampDistribution)
                {
                    //no separate help switch per rule in world designer - need to pass in the switch from outside
                    rule.m_isHelpActive = helpEnabled;
                    DrawSingleRulePanel(rule, ruleIdx);
                }
            }
        }

       

       

        private void DrawBiomeMaskSpawnRules(bool helpEnabled)
        {
            EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("WorldBiomeMasksNotSupportedAnymore"), MessageType.Error);
            GUILayout.Space(10);

            for (int ruleIdx = 0; ruleIdx < m_spawner.m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                m_spawner.m_spawnRuleIndexBeingDrawn = ruleIdx;
                SpawnRule rule = m_spawner.m_settings.m_spawnerRules[ruleIdx];
                if (rule.m_resourceType == SpawnerResourceType.WorldBiomeMask)
                {
                    DrawSingleRulePanel(rule, ruleIdx);
                }
            } //for

            GUILayout.Space(10);
        }


        //private void DrawSpawnRules(bool helpEnabled, SpawnerResourceType resourceType)
        //{
        //    for (int ruleIdx = 0; ruleIdx < m_spawner.m_settings.m_spawnerRules.Count; ruleIdx++)
        //    {
        //        m_spawnRuleIndexBeingDrawn = ruleIdx;
        //        SpawnRule rule = m_spawner.m_settings.m_spawnerRules[ruleIdx];

        //        DrawSingleRulePanel(rule, ruleIdx);

        //    } //for
        //}

        private bool DrawSingleRulePanel(SpawnRule rule, int ruleIdx)
        {
            string instanceCount = "";

            if (rule.m_resourceType != SpawnerResourceType.TerrainTexture)
            {
                instanceCount = "(" + rule.m_spawnedInstances + ")";
            }


            GUIContent panelLabel = new GUIContent(instanceCount + " " + rule.m_name);

            //Panel Label
            GUIStyle panelLabelStyle = new GUIStyle(GUI.skin.label);
            panelLabelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelLabelStyle.fontStyle = FontStyle.Bold;
            panelLabelStyle.normal.background = GUI.skin.label.normal.background;

            //Panel Label
            GUIStyle panelToggleStyle = new GUIStyle(GUI.skin.toggle);
            panelToggleStyle.padding = new RectOffset(15, 0, 1, 0);

            //Preview Image
            GUIStyle previewImageStyle = new GUIStyle();
            //previewImageStyle.alignment = TextAnchor.MiddleCenter;
            //previewImageStyle.fixedWidth = 20;
            //previewImageStyle.fixedHeight = 20;
            previewImageStyle.padding = new RectOffset(0, 0, 1, 0);

            // Panel
            GUIStyle panelStyle = new GUIStyle(GUI.skin.label);
            panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelStyle.alignment = TextAnchor.UpperLeft;

            bool unfolded = rule.m_isFoldedOut;
            bool helpEnabled = rule.m_isHelpActive;
            bool originalGUIState = GUI.enabled;


            GUILayout.BeginVertical(m_spawnRulesBorder);
            {
                GUILayout.BeginHorizontal(m_singleSpawnRuleHeader);
                {
                    //Rect rect = EditorGUILayout.GetControlRect();
                    unfolded = GUILayout.Toggle(unfolded, unfolded ? "-" : "+", panelLabelStyle, GUILayout.MinWidth(12));



                    rule.m_isActive = GUILayout.Toggle(rule.m_isActive, "", panelToggleStyle);


                    GUI.enabled = rule.m_isActive && GUI.enabled;
                    bool ruleGUIState = GUI.enabled;

                    if (rule.m_resourceType != SpawnerResourceType.StampDistribution && rule.m_resourceType != SpawnerResourceType.SpawnExtension && rule.m_resourceType != SpawnerResourceType.Probe && rule.m_resourceType != SpawnerResourceType.WorldBiomeMask)
                    {
                        Texture2D resourceTexture = GetSpawnRulePreviewTexture(rule, m_spawner.m_settings.m_resources);


                        //We only should display the texture if it is not null, but also make sure it was displayed during the layout event already.
                        //If the texture is being loaded between EventType.Layout and EventType.Repaint, a new control would be created during Repaint 
                        //which will result in an error message.
                        if (resourceTexture != null && (Event.current.type == EventType.Layout || (ruleIdx >= 0 && ruleIdx < m_previewImageDisplayedDuringLayout.Length && m_previewImageDisplayedDuringLayout[ruleIdx])))
                        {
                            GUILayout.Space(-10);
                            EditorGUILayout.LabelField(new GUIContent(resourceTexture), previewImageStyle, GUILayout.MaxWidth(30));
                            if (Event.current.type == EventType.Layout)
                            {
                                m_previewImageDisplayedDuringLayout[ruleIdx] = true;
                            }
                        }
                        if (resourceTexture == null)
                        {
                            //Leave some room if preview texture is not being displayed for some reason (e.g. new rule)
                            GUILayout.Space(22);
                        }
                    }
                    unfolded = GUILayout.Toggle(unfolded, panelLabel, panelLabelStyle, GUILayout.MinWidth(0));
                    GUILayout.FlexibleSpace();

                    if (rule.m_resourceType == SpawnerResourceType.StampDistribution)
                    {
                        //allow the slider to occupy more and more relative space the wider the inspector is
                        float sliderRelativeWidth = Mathf.Lerp(0.2f, 0.6f, Mathf.InverseLerp(500, 1000, EditorGUIUtility.currentViewWidth));
                        m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[rule.m_resourceIdx].m_spawnProbability = GUILayout.HorizontalSlider(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[rule.m_resourceIdx].m_spawnProbability * 100f, 0f, 100f, GUILayout.Width(EditorGUIUtility.currentViewWidth* sliderRelativeWidth))/100f;
                        GUILayout.Space(10);
                    }

                    float smallButtonSize = 20;
                    if (EditorGUIUtility.currentViewWidth > 460)
                    {

                        //Deactivate upwards button for first position in the rule list
                        if (ruleIdx == 0)
                        {
                            GUI.enabled = false;
                        }
                        GUIContent GCupIcon = GaiaEditorUtils.GetIconGUIContent("IconUp", m_gaiaSettings.m_IconUp, m_gaiaSettings.m_IconProUp, m_editorUtils);
                        if (m_editorUtils.Button(GCupIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                        {
                            SwapRules(ruleIdx - 1, ruleIdx);
                        }

                        GUI.enabled = ruleGUIState;

                        //Deactivate downwards button for last position in the rule list
                        if (ruleIdx == m_spawner.m_settings.m_spawnerRules.Count() - 1)
                        {
                            GUI.enabled = false;
                        }


                        GUIContent GCdownIcon = GaiaEditorUtils.GetIconGUIContent("IconDown", m_gaiaSettings.m_IconDown, m_gaiaSettings.m_IconProDown, m_editorUtils);
                        if (m_editorUtils.Button(GCdownIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                        {
                            SwapRules(ruleIdx, ruleIdx + 1);
                        }
                        GUI.enabled = ruleGUIState;
                    }
                    else
                    {
                        m_editorUtils.Label("Ellipsis");
                    }

                    if (rule.m_resourceType == SpawnerResourceType.StampDistribution)
                    {
                        GUILayout.Space(5);
                    }

                    if (EditorGUIUtility.currentViewWidth > 430 &&rule.m_resourceType != SpawnerResourceType.StampDistribution)
                    {

                        GUILayout.Space(11);
                        GUIContent GCduplicateIcon = GaiaEditorUtils.GetIconGUIContent("IconDuplicate", m_gaiaSettings.m_IconDuplicate, m_gaiaSettings.m_IconProDuplicate, m_editorUtils);
                        if (m_editorUtils.Button(GCduplicateIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                        {
                            DuplicateRule(ruleIdx);
                        }

                        GUIContent GCcopyIcon = GaiaEditorUtils.GetIconGUIContent("IconCopyRule", m_gaiaSettings.m_IconCopy, m_gaiaSettings.m_IconProCopy, m_editorUtils);
                        if (m_editorUtils.Button(GCcopyIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                        {
                            CopyRuleToClipboard(ruleIdx);
                        }

                        if (SessionManager.m_copiedSpawnRule == null || SessionManager.m_copiedSpawnRule == rule)
                        {
                            GUI.enabled = false;
                        }

                        GUIContent GCPasteIcon = GaiaEditorUtils.GetIconGUIContent("IconPasteRule", m_gaiaSettings.m_IconPaste, m_gaiaSettings.m_IconProPaste, m_editorUtils);
                        if (m_editorUtils.Button(GCPasteIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                        {
                            PasteRuleFromClipboard(ruleIdx);
                        }

                        GUI.enabled = ruleGUIState;
                        GUILayout.Space(11);
                    }
                    if (EditorGUIUtility.currentViewWidth > 400)
                    {
                        DrawVisualiseButton(ruleIdx, m_smallButtonStyle, smallButtonSize);
                        GUILayout.Space(8);
                    }
                    if (EditorGUIUtility.currentViewWidth > 380 &&rule.m_resourceType != SpawnerResourceType.StampDistribution)
                    {
                        //EditorGUILayout.EndVertical();
                        m_editorUtils.HelpToggle(ref helpEnabled);
                        GUILayout.Space(3);
                    }
                    //Unless the spawner is spawning, still offer to delete even if the rule is inactive
                    if (originalGUIState)
                    {
                        GUI.enabled = true;
                    }
                    GUIContent GCremoveIcon = GaiaEditorUtils.GetIconGUIContent("IconRemove", m_gaiaSettings.m_IconRemove, m_gaiaSettings.m_IconProRemove, m_editorUtils);
                    if (m_editorUtils.Button(GCremoveIcon, m_smallButtonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
                    {
                        m_spawner.m_settings.m_spawnerRules.Remove(rule);
                        RemoveMaskList(ruleIdx);
                        m_previewImageDisplayedDuringLayout = GaiaUtils.RemoveArrayIndexAt(m_previewImageDisplayedDuringLayout, ruleIdx);
                        PruneResources();
                        CleanPreviewRuleIDs();
                    }

                    GUI.enabled = originalGUIState && rule.m_isActive;
                    GUILayout.Space(4);

                }
                GUILayout.EndHorizontal();

                //if (helpActive)
                //{
                //    GUILayout.Space(2f);
                //    m_editorUtils.InlineHelp(nameKey, helpActive);
                //}

                if (unfolded)
                {
                    GUILayout.BeginVertical(panelStyle);
                    {
                        if (rule != null && m_spawner.m_settings.m_spawnerRules.Count > ruleIdx)
                        {
                            DrawSingleRule(rule, ruleIdx, helpEnabled);
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();

            //Leave a little space between the rules, but no extra space behind the last rule
            if (ruleIdx < m_spawner.m_settings.m_spawnerRules.Count - 1)
            {
                GUILayout.Space(8);
            }

            rule.m_isFoldedOut = unfolded;
            rule.m_isHelpActive = helpEnabled;
            GUI.enabled = originalGUIState;
            return unfolded;
        }


        /// <summary>
        /// Swaps out the position / index of two spawn rules in the stack
        /// </summary>
        /// <param name="firstRuleID"></param>
        /// <param name="secondRuleID"></param>
        private void SwapRules(int firstRuleID, int secondRuleID)
        {
            //Check if both ids are actually in index first
            int maxIndex = m_spawner.m_settings.m_spawnerRules.Count() - 1;
            if (firstRuleID > maxIndex || secondRuleID > maxIndex || firstRuleID < 0 || secondRuleID < 0)
            {
                Debug.LogError("Could not swap rules: First Index " + firstRuleID.ToString() + " or Second Index " + secondRuleID.ToString() + " are out of bounds.");
                return;
            }

            SpawnRule tempRule = m_spawner.m_settings.m_spawnerRules[firstRuleID];
            m_spawner.m_settings.m_spawnerRules[firstRuleID] = m_spawner.m_settings.m_spawnerRules[secondRuleID];
            m_spawner.m_settings.m_spawnerRules[secondRuleID] = tempRule;

            m_reorderableRuleMasksLists = GaiaUtils.SwapElementsInArray(m_reorderableRuleMasksLists, firstRuleID, secondRuleID);

            m_spawnRuleMaskListExpanded = GaiaUtils.SwapElementsInArray(m_spawnRuleMaskListExpanded, firstRuleID, secondRuleID);
            m_previewImageDisplayedDuringLayout = GaiaUtils.SwapElementsInArray(m_previewImageDisplayedDuringLayout, firstRuleID, secondRuleID);

            //Swap preview rule indexes as well, if affected
            bool visChange = false;
            for (int i = 0; i < m_spawner.m_previewRuleIds.Count(); i++)
            {
                if (m_spawner.m_previewRuleIds[i] == secondRuleID)
                {
                    m_spawner.m_previewRuleIds[i] = firstRuleID;
                    visChange = true;
                }
                //the else here is important!
                else if (m_spawner.m_previewRuleIds[i] == firstRuleID)
                {
                    m_spawner.m_previewRuleIds[i] = secondRuleID;
                    visChange = true;
                }

                //If the order of visualized rules has changed, we need to update the visualization as well
                if (visChange)
                {
                    m_spawner.m_previewRuleIds.Sort();
                    m_spawner.m_spawnPreviewDirty = true;
                    m_spawner.SetWorldBiomeMasksDirty();
                }
            }

        }


        /// <summary>
        /// Duplicates a spawn rule in the stack
        /// </summary>
        /// <param name="spawnRuleId"></param>
        private void DuplicateRule(int spawnRuleId)
        {
            SpawnRule sourceRule = m_spawner.m_settings.m_spawnerRules[spawnRuleId];
            SpawnRule newRule = new SpawnRule();

            //Create a copy of the used resource, from user feedback it is more common that users want
            //an actual copy of the resource rather than editing the same resource from two different rules.
            CopyResourceSettings(m_spawner, sourceRule, newRule);

            CopyRuleFields(sourceRule, newRule);

            m_spawner.m_settings.m_spawnerRules.Insert(spawnRuleId + 1, newRule);

            newRule.m_imageMasks = new ImageMask[sourceRule.m_imageMasks.Count()];

            CopyRuleMaskStack(sourceRule, newRule);

            //Insert the new mask list
            ReorderableList newMaskList = new ReorderableList(newRule.m_imageMasks, typeof(ImageMask));
            newMaskList = CreateSpawnRuleMaskList(newMaskList, newRule.m_imageMasks);
            m_reorderableRuleMasksLists = GaiaUtils.InsertElementInArray(m_reorderableRuleMasksLists, newMaskList, spawnRuleId + 1);

            //Insert the expanded flag
            m_spawnRuleMaskListExpanded = GaiaUtils.InsertElementInArray(m_spawnRuleMaskListExpanded, false, spawnRuleId + 1);

            //Insert the flag for preview image
            m_previewImageDisplayedDuringLayout = GaiaUtils.InsertElementInArray(m_previewImageDisplayedDuringLayout, false, spawnRuleId + 1);
        }

        private void CopyRuleMaskStack(SpawnRule sourceRule, SpawnRule targetRule)
        {
            //Create a deep copy for each mask in the list > we want source & target to be independent from each other
            for (int i = 0; i < targetRule.m_imageMasks.Count(); i++)
            {
                targetRule.m_imageMasks[i] = ImageMask.Clone(sourceRule.m_imageMasks[i]);
                targetRule.m_imageMasks[i].m_reorderableCollisionMaskList = CreateSpawnRuleCollisionMaskList(targetRule.m_imageMasks[i].m_reorderableCollisionMaskList, targetRule.m_imageMasks[i].m_collisionMasks);
            }
        }

        private void CopyRuleFields(SpawnRule sourceRule, SpawnRule targetRule)
        {
            targetRule.m_resourceType = sourceRule.m_resourceType;
            targetRule.m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_spawnerRules.Select(x => x.m_name).ToArray(), sourceRule.m_name);

            targetRule.m_failureRate = sourceRule.m_failureRate;
            targetRule.m_locationIncrementMin = sourceRule.m_locationIncrementMin;
            targetRule.m_locationIncrementMax = sourceRule.m_locationIncrementMax;
            targetRule.m_jitterPercent = sourceRule.m_jitterPercent;
            targetRule.m_minRequiredFitness = sourceRule.m_minRequiredFitness;
            targetRule.m_minInstanceRequiredFitness = sourceRule.m_minInstanceRequiredFitness;
            targetRule.m_minDirection = sourceRule.m_minDirection;
            targetRule.m_maxDirection = sourceRule.m_maxDirection;

            targetRule.m_boundsCheckQuality = sourceRule.m_boundsCheckQuality;
            targetRule.m_boundsCollisionCheck = sourceRule.m_boundsCollisionCheck;

            targetRule.m_goSpawnTargetMode = sourceRule.m_goSpawnTargetMode;
            targetRule.m_goSpawnTarget = sourceRule.m_goSpawnTarget;
            targetRule.m_terrainGOSpawnTargetName = sourceRule.m_terrainGOSpawnTargetName;
            targetRule.m_visibleInSceneHierarchy = sourceRule.m_visibleInSceneHierarchy;

            targetRule.m_isFoldedOut = sourceRule.m_isFoldedOut;
            targetRule.m_resourceSettingsFoldedOut = sourceRule.m_resourceSettingsFoldedOut;
        }

        public void CopyRuleToClipboard(int ruleId)
        {
            SessionManager.m_copiedSpawnRule = m_spawner.m_settings.m_spawnerRules[ruleId];
            SessionManager.m_copiedSpawnRuleSpawner = m_spawner;
        }

        public void PasteRuleFromClipboard(int targetRuleId)
        {
            //Remove the old resource data on the target rule
            SpawnRule targetRule = m_spawner.m_settings.m_spawnerRules[targetRuleId];

            CopyResourceSettings(SessionManager.m_copiedSpawnRuleSpawner, SessionManager.m_copiedSpawnRule, targetRule);
            CopyRuleFields(SessionManager.m_copiedSpawnRule, targetRule);

            targetRule.m_imageMasks = new ImageMask[SessionManager.m_copiedSpawnRule.m_imageMasks.Count()];

            CopyRuleMaskStack(SessionManager.m_copiedSpawnRule, targetRule);

            //Insert the new mask list
            ReorderableList newMaskList = new ReorderableList(targetRule.m_imageMasks, typeof(ImageMask));
            newMaskList = CreateSpawnRuleMaskList(newMaskList, targetRule.m_imageMasks);
            m_reorderableRuleMasksLists[targetRuleId] = newMaskList;

            //Copy the expanded flag
            m_spawnRuleMaskListExpanded[targetRuleId] = true;

            //Copy the flag for preview image
            m_previewImageDisplayedDuringLayout[targetRuleId] = false;

            PruneResources();

        }

        private void CopyResourceSettings(Spawner sourceSpawner, SpawnRule sourceRule, SpawnRule targetRule)
        {
            switch (sourceRule.m_resourceType)
            {
                case SpawnerResourceType.GameObject:
                    targetRule.m_resourceIdx = AddNewGameObjectResource();
                    ResourceProtoGameObject targetResource = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[targetRule.m_resourceIdx];
                    ResourceProtoGameObject sourceResource = sourceSpawner.m_settings.m_resources.m_gameObjectPrototypes[sourceRule.m_resourceIdx];
                    //Copy general fields first
                    GaiaUtils.CopyFields(sourceResource, targetResource);
                    //IMPORTANT: instantiate a new, independent object for all sub-objects for a proper deep copy
                    targetResource.m_dna = new ResourceProtoDNA();
                    GaiaUtils.CopyFields(sourceResource.m_dna, targetResource.m_dna);
                    targetResource.m_instances = new ResourceProtoGameObjectInstance[0];
                    for (int i = 0; i < sourceResource.m_instances.Count(); i++)
                    {
                        ResourceProtoGameObjectInstance newInstance = new ResourceProtoGameObjectInstance();
                        GaiaUtils.CopyFields(sourceResource.m_instances[i], newInstance);
                        targetResource.m_instances = GaiaUtils.AddElementToArray(targetResource.m_instances, newInstance);
                    }
                    targetResource.m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Select(x => x.m_name).ToArray(), targetResource.m_name);
                    break;
                case SpawnerResourceType.SpawnExtension:
                    targetRule.m_resourceIdx = AddNewSpawnExtensionResource();
                    ResourceProtoSpawnExtension targetSEResource = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[targetRule.m_resourceIdx];
                    ResourceProtoSpawnExtension sourceSEResource = sourceSpawner.m_settings.m_resources.m_spawnExtensionPrototypes[sourceRule.m_resourceIdx];
                    //Copy general fields first
                    GaiaUtils.CopyFields(sourceSEResource, targetSEResource);
                    //IMPORTANT: instantiate a new, independent object for all sub-objects for a proper deep copy
                    targetSEResource.m_dna = new ResourceProtoDNA();
                    GaiaUtils.CopyFields(sourceSEResource.m_dna, targetSEResource.m_dna);
                    targetSEResource.m_instances = new ResourceProtoSpawnExtensionInstance[0];
                    for (int i = 0; i < sourceSEResource.m_instances.Count(); i++)
                    {
                        ResourceProtoSpawnExtensionInstance newInstance = new ResourceProtoSpawnExtensionInstance();
                        GaiaUtils.CopyFields(sourceSEResource.m_instances[i], newInstance);
                        targetSEResource.m_instances = GaiaUtils.AddElementToArray(targetSEResource.m_instances, newInstance);
                    }
                    targetSEResource.m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Select(x => x.m_name).ToArray(), targetSEResource.m_name);

                    break;
                case SpawnerResourceType.TerrainDetail:
                    targetRule.m_resourceIdx = AddNewTerrainDetailResource();
                    ResourceProtoDetail protoDetailTarget = m_spawner.m_settings.m_resources.m_detailPrototypes[targetRule.m_resourceIdx];
                    ResourceProtoDetail protoDetailSource = sourceSpawner.m_settings.m_resources.m_detailPrototypes[sourceRule.m_resourceIdx];
                    GaiaUtils.CopyFields(protoDetailSource, protoDetailTarget);

                    protoDetailTarget.m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_detailPrototypes.Select(x => x.m_name).ToArray(), protoDetailTarget.m_name);
#if FLORA_PRESENT
                    //We need to a deep copy of the Flora LOD settings and re-instantiate them as well - otherwise both resource settings would be working with the same flora data, 
                    //which might be undesireable for most cases where the user wants to create a new, independent detail object
                    protoDetailTarget.m_floraLODs = new List<FloraLOD>();
                    Dictionary<int, string> materialMap = new Dictionary<int, string>();
                    if (protoDetailTarget.m_useFlora)
                    {
                        for (int i = 0; i < protoDetailSource.m_floraLODs.Count; i++)
                        {
                            FloraUtils.AddNewDetailerSettingsObject(protoDetailTarget.m_floraLODs, protoDetailSource.m_floraLODs[i].m_name, SpawnerResourceType.TerrainDetail);
                            GaiaUtils.CopyFields(protoDetailSource.m_floraLODs[i].DetailerSettingsObject, protoDetailTarget.m_floraLODs[i].DetailerSettingsObject, true);
                        }
                    }
#endif
                    break;
                case SpawnerResourceType.TerrainTexture:
                    targetRule.m_resourceIdx = AddNewTextureResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_texturePrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_texturePrototypes[targetRule.m_resourceIdx]);
                    m_spawner.m_settings.m_resources.m_texturePrototypes[targetRule.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_texturePrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_texturePrototypes[targetRule.m_resourceIdx].m_name);

                    break;
                case SpawnerResourceType.TerrainTree:
                    targetRule.m_resourceIdx = AddNewTreeResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_treePrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_treePrototypes[targetRule.m_resourceIdx]);
                    m_spawner.m_settings.m_resources.m_treePrototypes[targetRule.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_treePrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_treePrototypes[targetRule.m_resourceIdx].m_name);


                    break;
                case SpawnerResourceType.StampDistribution:
                    targetRule.m_resourceIdx = AddNewStampDistributionResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_stampDistributionPrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[targetRule.m_resourceIdx]);
                    break;
                case SpawnerResourceType.WorldBiomeMask:
                    targetRule.m_resourceIdx = AddNewWorldBiomeMaskResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[targetRule.m_resourceIdx]);
                    m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[targetRule.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[targetRule.m_resourceIdx].m_name);
                    break;
                case SpawnerResourceType.Probe:
                    targetRule.m_resourceIdx = AddNewProbeResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_probePrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_probePrototypes[targetRule.m_resourceIdx]);
                    m_spawner.m_settings.m_resources.m_probePrototypes[targetRule.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_probePrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_probePrototypes[targetRule.m_resourceIdx].m_name);
                    break;
                case SpawnerResourceType.TerrainModifierStamp:
                    targetRule.m_resourceIdx = AddNewTerrainModifierStampResource();
                    GaiaUtils.CopyFields(sourceSpawner.m_settings.m_resources.m_terrainModifierStampPrototypes[sourceRule.m_resourceIdx], m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[targetRule.m_resourceIdx]);
                    m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[targetRule.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[targetRule.m_resourceIdx].m_name);
                    break;
            }
        }

        /// <summary>
        /// Removes all Resource entries that are not in use by any rule anymore, this prevents build up of unused resource entries
        /// </summary>
        private void PruneResources()
        {
            GaiaResource resource = m_spawner.m_settings.m_resources;
            for (int i = resource.m_texturePrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.TerrainTexture && x.m_resourceIdx == i) == null)
                {
                    resource.m_texturePrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_texturePrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.TerrainTexture, i);
                }
            }
            for (int i = resource.m_treePrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.TerrainTree && x.m_resourceIdx == i) == null)
                {
                    resource.m_treePrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_treePrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.TerrainTree, i);
                }
            }
            for (int i = resource.m_detailPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.TerrainDetail && x.m_resourceIdx == i) == null)
                {
                    resource.m_detailPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_detailPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.TerrainDetail, i);
                }
            }
            for (int i = resource.m_gameObjectPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.GameObject && x.m_resourceIdx == i) == null)
                {
                    resource.m_gameObjectPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_gameObjectPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.GameObject, i);
                }
            }
            for (int i = resource.m_spawnExtensionPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.SpawnExtension && x.m_resourceIdx == i) == null)
                {
                    resource.m_spawnExtensionPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_spawnExtensionPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.SpawnExtension, i);
                }
            }
            for (int i = resource.m_stampDistributionPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.StampDistribution && x.m_resourceIdx == i) == null)
                {
                    resource.m_stampDistributionPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_stampDistributionPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.StampDistribution, i);
                }
            }
            for (int i = resource.m_worldBiomeMaskPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask && x.m_resourceIdx == i) == null)
                {
                    resource.m_worldBiomeMaskPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_worldBiomeMaskPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.WorldBiomeMask, i);
                }
            }
            for (int i = resource.m_probePrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.Probe && x.m_resourceIdx == i) == null)
                {
                    resource.m_probePrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_probePrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.Probe, i);
                }
            }
            for (int i = resource.m_terrainModifierStampPrototypes.Length - 1; i >= 0; i--)
            {
                if (m_spawner.m_settings.m_spawnerRules.Find(x => x.m_resourceType == SpawnerResourceType.TerrainModifierStamp && x.m_resourceIdx == i) == null)
                {
                    resource.m_terrainModifierStampPrototypes = GaiaUtils.RemoveArrayIndexAt(resource.m_terrainModifierStampPrototypes, i);
                    m_spawner.CorrectIndicesAfteResourceDeletion(SpawnerResourceType.TerrainModifierStamp, i);
                }
            }
        }

        private void DrawVisualiseButton(int spawnRuleID, GUIStyle buttonStyle, float smallButtonSize)
        {
            Color currentBGColor = GUI.backgroundColor;
            if (m_spawner.m_previewRuleIds.Contains(spawnRuleID) && m_spawner.m_drawPreview)
            {
                GUI.backgroundColor = m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_visualisationColor;
            }

            GUIContent GCvisualizeIcon = GaiaEditorUtils.GetIconGUIContent("IconVisible", m_gaiaSettings.m_IconVisible, m_gaiaSettings.m_IconProVisible, m_editorUtils);
            if (m_editorUtils.Button(GCvisualizeIcon, buttonStyle, GUILayout.Height(smallButtonSize), GUILayout.Width(smallButtonSize)))
            {
                if (!GaiaUtils.UsesCorrectPipelineDefines())
                {
                    if (!m_spawner.m_previewRuleIds.Contains(spawnRuleID))
                    {
                        if (!EditorUtility.DisplayDialog("Wrong Render Pipeline Configured", "This project does use a different render pipeline asset in the graphics settings than what Gaia is configured for. This can lead to wrong visualization results. Please open the Gaia Manager and configure Gaia for the correct render pipeline that you are using.", "Continue Anyways", "Cancel"))
                        {
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                }

                //is this rule being shown already? then only remove this rule
                if (m_spawner.m_previewRuleIds.Contains(spawnRuleID) && m_spawner.m_drawPreview)
                {
                    m_spawner.m_previewRuleIds.Remove(spawnRuleID);
                    if (m_spawner.m_previewRuleIds.Count <= 0)
                        m_spawner.m_drawPreview = false;
                }
                else
                {
                    if (m_spawner.m_drawPreview)
                    {
                        if (m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_resourceType == SpawnerResourceType.TerrainModifierStamp)
                        {
                            //for terrain modifier stamps, we only want one single rule visualized max, so kick out all other rules
                            m_spawner.m_previewRuleIds.Clear();
                            m_spawner.m_previewRuleIds.Add(spawnRuleID);
                            m_spawner.UpdateTerrainModifierStamperFromSpawnRule(m_spawner.GetOrCreatePreviewStamper(true), m_spawner.m_settings.m_spawnerRules[spawnRuleID]); 
                        }
                        else
                        {

                            //this rule needs to be added for visualisation, would we exceed the maximum allowed number?
                            if (m_spawner.m_previewRuleIds.Count() >= GaiaConstants.maxPreviewedTextures)
                            {
                                //Yes, kick lowest rule in stack out first
                                m_spawner.m_previewRuleIds.RemoveAt(0);
                            }

                            //mark this rule for visualisation
                            m_spawner.m_previewRuleIds.Add(spawnRuleID);
                            //Sort the rules ascending, important because the lower rules should overwrite the earlier ones.
                            m_spawner.m_previewRuleIds.Sort();
                        }
                    }
                    else
                    {
                        //the spawner was currently not displaying the preview. Throw out any old rules first, and start fresh
                        m_spawner.m_previewRuleIds.Clear();
                        m_spawner.m_previewRuleIds.Add(spawnRuleID);
                        if (m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_resourceType == SpawnerResourceType.TerrainModifierStamp)
                        {
                            m_spawner.UpdateTerrainModifierStamperFromSpawnRule(m_spawner.GetOrCreatePreviewStamper(true), m_spawner.m_settings.m_spawnerRules[spawnRuleID]);
                        }
                        m_spawner.m_drawPreview = true;
                    }
                }

                UpdateColorsForWorldDesignerPreview(true);
     
            }

            GUI.backgroundColor = currentBGColor;

            Color currentVisColor = m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_visualisationColor;
            if (GaiaUtils.ColorsEqual(currentVisColor, GaiaConstants.spawnerInitColor))
            {
                //pick a random color from the the gradient in the Gaia Settings
                currentVisColor = m_gaiaSettings.m_spawnerColorGradient.Evaluate(UnityEngine.Random.value);
                //increase brightness / saturation of the visualization color by increasing the strongest color up to 1
                if (currentVisColor.r > currentVisColor.g && currentVisColor.r > currentVisColor.b)
                {
                    currentVisColor.g += 1.0f - currentVisColor.r;
                    currentVisColor.b += 1.0f - currentVisColor.r;
                    currentVisColor.r = 1.0f;
                }
                else if (currentVisColor.g > currentVisColor.r && currentVisColor.g > currentVisColor.b)
                {
                    currentVisColor.r += 1.0f - currentVisColor.g;
                    currentVisColor.b += 1.0f - currentVisColor.g;
                    currentVisColor.g = 1.0f;
                }
                else if (currentVisColor.b > currentVisColor.r && currentVisColor.b > currentVisColor.g)
                {
                    currentVisColor.r += 1.0f - currentVisColor.b;
                    currentVisColor.g += 1.0f - currentVisColor.b;
                    currentVisColor.b = 1.0f;
                }
            }
            GUILayout.Space(-15);
            m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_visualisationColor = EditorGUILayout.ColorField(currentVisColor, GUILayout.Width(60));
        }

        private void UpdateColorsForWorldDesignerPreview(bool forceCurveTextureBake)
        {
            //update curve textures and colors for correct world designer preview
            if (m_spawner.m_settings.m_isWorldmapSpawner)
            {
                Stamper stamper = m_spawner.GetOrCreateBaseTerrainStamper(true);
                int i = 0;
                for (; i < m_spawner.m_previewRuleIds.Count; i++)
                {
                    int previewRuleID = m_spawner.m_previewRuleIds[i];
                    int resourceIdx = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_resourceIdx;

                    if (stamper.m_stampDirty || forceCurveTextureBake)
                    {
                        switch (i)
                        {
                            case 0:
                                ImageProcessing.BakeCurveTexture(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceIdx].m_inputHeightToProbabilityMapping, stamper.worldDesignerStampVisCurve0);
                                break;
                            case 1:
                                ImageProcessing.BakeCurveTexture(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceIdx].m_inputHeightToProbabilityMapping, stamper.worldDesignerStampVisCurve1);
                                break;
                            case 2:
                                ImageProcessing.BakeCurveTexture(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceIdx].m_inputHeightToProbabilityMapping, stamper.worldDesignerStampVisCurve2);
                                break;
                            case 3:
                                ImageProcessing.BakeCurveTexture(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceIdx].m_inputHeightToProbabilityMapping, stamper.worldDesignerStampVisCurve3);
                                break;
                            case 4:
                                ImageProcessing.BakeCurveTexture(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceIdx].m_inputHeightToProbabilityMapping, stamper.worldDesignerStampVisCurve4);
                                break;
                        }
                    }
                    switch (i)
                    {
                        case 0:
                            stamper.m_worldDesignerStampVisColor0 = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_visualisationColor;
                            break;
                        case 1:
                            stamper.m_worldDesignerStampVisColor1 = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_visualisationColor;
                            break;
                        case 2:
                            stamper.m_worldDesignerStampVisColor2 = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_visualisationColor;
                            break;
                        case 3:
                            stamper.m_worldDesignerStampVisColor3 = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_visualisationColor;
                            break;
                        case 4:
                            stamper.m_worldDesignerStampVisColor4 = m_spawner.m_settings.m_spawnerRules[previewRuleID].m_visualisationColor;
                            break;
                    }
                }
                for (; i < 5; i++)
                {
                    switch (i)
                    {
                        case 0:
                            stamper.m_worldDesignerStampVisColor0.a = 0;
                            break;
                        case 1:
                            stamper.m_worldDesignerStampVisColor1.a = 0;
                            break;
                        case 2:
                            stamper.m_worldDesignerStampVisColor2.a = 0;
                            break;
                        case 3:
                            stamper.m_worldDesignerStampVisColor3.a = 0;
                            break;
                        case 4:
                            stamper.m_worldDesignerStampVisColor4.a = 0;
                            break;
                    }
                }
            }
        }

        private void DrawSingleRule(SpawnRule rule, int spawnRuleID, bool helpEnabled)
        {
            if (spawnRuleID == m_drawResourcePreviewRuleId)
            {
                Texture2D resourceTexture = GetSpawnRulePreviewTexture(rule, m_spawner.m_settings.m_resources);

                if (resourceTexture != null)
                {
                    //Rect previewRect = EditorGUILayout.GetControlRect();
                    //EditorGUI.LabelField(new Rect(previewRect.x, previewRect.y, EditorGUIUtility.labelWidth, previewRect.height), label);
                    m_editorUtils.Image(resourceTexture);
                    //GUILayout.Space(100);
                }
            }
            EditorGUI.indentLevel--;
            //Spawn rule name first - required for any rule
            rule.m_name = m_editorUtils.TextField("RuleName", rule.m_name, helpEnabled);
            EditorGUI.indentLevel++;

            //Don't draw a resource foldout for world biome masks - the resource settings are just the name & biome preset, no foldout needed.
            //Don't draw a resource fold out for stamp settings - just confusing in the world generator
            if (rule.m_resourceType != SpawnerResourceType.WorldBiomeMask && rule.m_resourceType != SpawnerResourceType.StampDistribution)
            {
                rule.m_resourceSettingsFoldedOut = m_editorUtils.Foldout(rule.m_resourceSettingsFoldedOut, "ResourceSettings");
            }
            else
            {
                rule.m_resourceSettingsFoldedOut = true;
            }

            if (rule.m_resourceSettingsFoldedOut)
            {
                DrawRuleResourceSettings(rule, spawnRuleID, helpEnabled);

            }
            EditorGUI.indentLevel--;


            float maxLocationIncrement = 100f;
            maxLocationIncrement = m_spawner.m_lastActiveTerrainSize * 0.5f;
          
            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTree || rule.m_resourceType == GaiaConstants.SpawnerResourceType.Probe)
            {
                m_editorUtils.Heading("GameObjectProtoHeadingSpawning");
                EditorGUI.indentLevel++;
                rule.m_failureRate = 1f - m_editorUtils.Slider("GameObjectProtoInstanceProbabilityRate", (1f - rule.m_failureRate) * 100, 0, 100f, helpEnabled) / 100f;
                rule.m_locationIncrementMin = m_editorUtils.Slider("TreeProtoLocationIncrement", rule.m_locationIncrementMin, GaiaConstants.minlocationIncrement, maxLocationIncrement, helpEnabled);
                if (rule.m_locationIncrementMin < 1f)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ProtoSmallLocationIncrementWarning"), MessageType.Warning);
                }
                //Value displayed in % on the UI
                rule.m_jitterPercent = m_editorUtils.Slider("TreeProtoJitterPercent", rule.m_jitterPercent * 100f, 0f, 100f, helpEnabled) / 100f;
                rule.m_minRequiredFitness = m_editorUtils.Slider("GameObjectMinFitness", rule.m_minRequiredFitness * 100f, 0, 100f, helpEnabled) / 100f;
                EditorGUI.indentLevel--;
            }
            if ((rule.m_resourceType == GaiaConstants.SpawnerResourceType.GameObject) || (rule.m_resourceType == GaiaConstants.SpawnerResourceType.SpawnExtension))
            {
                m_editorUtils.Heading("GameObjectProtoHeadingSpawning");
                EditorGUI.indentLevel++;
                rule.m_failureRate = 1f - m_editorUtils.Slider("GameObjectProtoInstanceProbabilityRate", (1f - rule.m_failureRate) * 100, 0, 100f) / 100f;
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(15);
                    m_editorUtils.Label("GameObjectProtoStartOffset", GUILayout.Width(EditorGUIUtility.labelWidth - 30));
                    EditorGUIUtility.labelWidth = 40;
                    rule.m_startOffsetX = m_editorUtils.FloatField("GameObjectProtoStartOffsetX", rule.m_startOffsetX);
                    rule.m_startOffsetZ = m_editorUtils.FloatField("GameObjectProtoStartOffsetZ", rule.m_startOffsetZ);
                    EditorGUIUtility.labelWidth = 0;

                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("GameObjectProtoStartOffset", helpEnabled);
                rule.m_locationIncrementMin = m_editorUtils.Slider("TreeProtoLocationIncrement", rule.m_locationIncrementMin, GaiaConstants.minlocationIncrement, maxLocationIncrement, helpEnabled);
                
                //Do warnings for intercollision and amount of instances for Game Objects only - impossible to predict what the spawn extension will actually do
                if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.GameObject)
                {
                    //Try to estimate the number of spawned instances to display a warning if necessary
                    float numberOfSpawns = (100 / rule.m_locationIncrementMin) * (100 / rule.m_locationIncrementMin);

                    //if bounds collision check is enabled, do an estimation based on the bounds as well & assume the best case for the calculation
                    if (rule.m_boundsCollisionCheck)
                    {
                        float radius = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_boundsRadius;
                        numberOfSpawns = Mathf.Min(numberOfSpawns, (100 / (radius * 2)) * (100 / (radius * 2)));
                    }
                    int numberOfInstances = 0;


                    foreach (ResourceProtoGameObjectInstance instance in m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_instances)
                    {
                        numberOfInstances += Mathf.RoundToInt((instance.m_minInstances + instance.m_maxInstances) / 2f);
                    }
                    numberOfSpawns *= numberOfInstances;
                    if (numberOfSpawns > m_gaiaSettings.m_gameObjectCountWarningThreshold)
                    {
                        EditorGUILayout.HelpBox(String.Format(m_editorUtils.GetTextValue("ProtoSmallLocationIncrementWarning"), Mathf.RoundToInt(numberOfSpawns).ToString()), MessageType.Warning);
                    }
                    //Value displayed in % on the UI
                    rule.m_jitterPercent = m_editorUtils.Slider("TreeProtoJitterPercent", rule.m_jitterPercent * 100f, 0f, 100f, helpEnabled) / 100f;

                    if (!rule.m_boundsCollisionCheck)
                    {
                        if (rule.m_locationIncrementMin * rule.m_jitterPercent > rule.m_locationIncrementMin - m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_boundsRadius)
                        {
                            EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("ProtoJitterCollideWarning"), MessageType.Info);
                        }
                    }
                }
                else
                {
                    rule.m_jitterPercent = m_editorUtils.Slider("TreeProtoJitterPercent", rule.m_jitterPercent * 100f, 0f, 100f, helpEnabled) / 100f;
                    
                }
                rule.m_minRequiredFitness = m_editorUtils.Slider("GameObjectMinAreaFitness", rule.m_minRequiredFitness * 100f, 0, 100f, helpEnabled) / 100f;
                //The max on the slider for min instance fitness is tied to area fitness. 
                //It does not make sense to allow higher instance fitness than area fitness, the spawner would not spawn anything in that case.
                rule.m_minInstanceRequiredFitness = m_editorUtils.Slider("GameObjectMinInstanceFitness", rule.m_minInstanceRequiredFitness * 100f, 0, 100f, helpEnabled) / 100f;
            }
            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.StampDistribution)
            {
                //no per-rule based settings here, all settings are stored in the stamp distribution resource
            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainDetail)
            {
                m_editorUtils.Heading("GameObjectProtoHeadingSpawning");
                EditorGUI.indentLevel++;
                rule.m_terrainDetailDensity = m_editorUtils.IntSlider("DetailProtoDensity", rule.m_terrainDetailDensity, 1, 50, helpEnabled);
                rule.m_terrainDetailMinFitness = m_editorUtils.Slider("DetailMinimumFitness", rule.m_terrainDetailMinFitness * 100f, 0, 100f, helpEnabled) / 100f;
                rule.m_terrainDetailFitnessBeginFadeOut = m_editorUtils.Slider("DetailFitnessBeginFadeOut", rule.m_terrainDetailFitnessBeginFadeOut * 100f, 0, 100f, helpEnabled) / 100f;
                EditorGUI.indentLevel--;
            }


            //if (m_spawner.m_showStatistics && rule.m_resourceType != GaiaConstants.SpawnerResourceType.TerrainTexture)
            //{
            //    EditorGUILayout.LabelField(GetLabel("Instances Spawned"), new GUIContent(rule.m_activeInstanceCnt.ToString()));
            //}

            //Direction control for spawned POI
            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.GameObject || rule.m_resourceType == GaiaConstants.SpawnerResourceType.SpawnExtension)
            {

                m_editorUtils.MinMaxSliderWithFields("GameObjectProtoMinMaxDirection", ref rule.m_minDirection, ref rule.m_maxDirection, 0f, 360f, helpEnabled);
                if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.GameObject)
                {
                    m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_scaleMultiplier = m_editorUtils.Slider("ProtoDNAScaleMultiplier", m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_scaleMultiplier, 0f, 10f, helpEnabled);

                    m_editorUtils.Heading("GameObjectProtoHeadingBoundsCheck");
                    m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_boundsRadius = m_editorUtils.FloatField("GameObjectProtoDNABoundsRadius", m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_dna.m_boundsRadius, helpEnabled);
                }
                rule.m_boundsCheckQuality = m_editorUtils.Slider("GameObjectProtoBoundsCheckQuality", rule.m_boundsCheckQuality, 1f, 100f, helpEnabled);
                rule.m_boundsCollisionCheck = m_editorUtils.Toggle("GameObjectProtoBoundsCollisionCheck", rule.m_boundsCollisionCheck, helpEnabled);
                if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.SpawnExtension)
                {
                    rule.m_collisionLayersToClear = GaiaEditorUtils.LayerMaskField(m_editorUtils.GetContent("SpawnExtensionProtoCollisionLayersToClear"), rule.m_collisionLayersToClear.value);
                    m_editorUtils.InlineHelp("SpawnExtensionProtoCollisionLayersToClear", helpEnabled);
                    rule.m_changesHeightmap = m_editorUtils.Toggle("SpawnExtensionProtoChangesHeightmap", rule.m_changesHeightmap, helpEnabled);
                }
                m_editorUtils.Heading("GameObjectProtoHeadingSceneHierarchy");
                rule.m_goSpawnTargetMode = (SpawnerTargetMode)m_editorUtils.EnumPopup("GameObjectSpawnerTargetMode", rule.m_goSpawnTargetMode, helpEnabled);
                if (rule.m_goSpawnTargetMode == SpawnerTargetMode.SingleTransform)
                {
                    rule.m_goSpawnTarget = (Transform)m_editorUtils.ObjectField("GameObjectSpawnTarget", rule.m_goSpawnTarget, typeof(Transform), true);
                }
                else
                {
                    rule.m_terrainGOSpawnTargetName = m_editorUtils.TextField("GameObjectTerrainSpawnTargetName", rule.m_terrainGOSpawnTargetName, helpEnabled);
                }
                rule.m_visibleInSceneHierarchy = m_editorUtils.Toggle("GameObjectVisibleInSceneHierarchy", rule.m_visibleInSceneHierarchy, helpEnabled);
                EditorGUI.indentLevel--;
            }
            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainModifierStamp)
            {
                bool currentGUIState = GUI.enabled;
                if (!SessionManager.DoesStamperBackupExist())
                {
                    GUI.enabled = false;
                }

                if (m_editorUtils.Button("TerrainModifierStampRestoreTerrainNow"))
                {
                    if (EditorUtility.DisplayDialog("Restore Heightmap?", "Do you want to restore the terrain heightmaps and undo the effects of all terrain modifier stamps? This will reset the terrain heightmaps to the point where the stamper was last used.", "Yes, Reset Heightmaps", "Cancel"))
                    {
                        SessionManager.RestoreStamperBackup();
                    }
                }

                GUI.enabled = currentGUIState;
            }

            EditorGUI.indentLevel++;


            m_spawner.m_maskListBeingDrawn = m_spawner.m_settings.m_spawnerRules[spawnRuleID].m_imageMasks;
            m_spawner.m_spawnRuleIndexBeingDrawn = spawnRuleID;
            EditorGUI.indentLevel++;
            if (rule.m_resourceType != SpawnerResourceType.StampDistribution)
            {
                if (m_spawnRuleMaskListExpanded.Count() > spawnRuleID && m_reorderableRuleMasksLists.Count() > spawnRuleID)
                {
                    ImageMaskListEditor.DrawMaskList(ref m_spawnRuleMaskListExpanded[spawnRuleID], m_reorderableRuleMasksLists[spawnRuleID], m_editorUtils, helpEnabled);
                }
            }
            EditorGUI.indentLevel--;

            if (rule.m_resourceType == SpawnerResourceType.WorldBiomeMask)
            {
                Color normalBGColor = GUI.backgroundColor;
                GUI.backgroundColor = m_gaiaSettings.GetActionButtonColor();
                EditorGUILayout.BeginHorizontal();
                {
                    if (m_editorUtils.Button("ButtonCreateBiome"))
                    {
                        BoundsDouble bounds = new BoundsDouble();
                        TerrainHelper.GetTerrainBounds(ref bounds);
                        SpawnRule[] allBiomeMaskRules = m_spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.WorldBiomeMask).ToArray();
                        BiomePreset biomePreset = m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[rule.m_resourceIdx].m_biomePreset;

                        if (biomePreset != null)
                        {
                            //Try to find an already existing biome preset in the scene
                            GameObject biomeGO = GameObject.Find(biomePreset.name + " Biome");
                            BiomeController biomeController = null;
                            if (biomeGO == null)
                            {
                                biomeController = biomePreset.CreateBiome(false);
                                biomeGO = biomeController.gameObject;
                            }
                            else
                            {
                                biomeController = biomeGO.GetComponent<BiomeController>();
                            }
                            rule.m_createdBiomeController = biomeController;
                            biomeGO.transform.position = bounds.center;
                            biomeController.m_settings.m_range = Mathf.Round((m_spawner.m_worldCreationSettings.m_tileSize * 1.9f) / 2f);
                            biomeController.m_autoSpawnerArea = AutoSpawnerArea.World;
                            biomeController.m_settings.m_imageMasks = new ImageMask[1] { new ImageMask { m_operation = ImageMaskOperation.WorldBiomeMask, m_selectedWorldBiomeMaskGUID = rule.GUID } };
                            EditorGUIUtility.PingObject(biomeController);
                            GUIUtility.ExitGUI();
                        }

                    }
                    bool currentGUIState = GUI.enabled;
                    if (rule.m_createdBiomeController == null)
                    {
                        GUI.enabled = false;
                    }

                    if (m_editorUtils.Button("ButtonSpawnBiome"))
                    {
                        if (rule.m_createdBiomeController != null)
                        {
                            rule.m_createdBiomeController.m_autoSpawnRequested = true;
                            rule.m_createdBiomeController.m_drawPreview = false;
                            Selection.activeObject = rule.m_createdBiomeController.gameObject;
                        }
                    }
                    GUI.enabled = currentGUIState;
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = normalBGColor;
            }


        }

        private void DrawRuleResourceSettings(SpawnRule rule, int spawnRuleID, bool helpEnabled)
        {
            SpawnerResourceType oldResourceType = rule.m_resourceType;
            int oldResourceID = rule.m_resourceIdx;

            //Resource type selection is only available for regular spawners
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                //We do not want to expose all possible resource types on the UI, we therefore cast into a "censored" enum to display on the UI and then cast the selected value back.
                RegularSpawnerResourceType selectedValue = (RegularSpawnerResourceType)(int)rule.m_resourceType;
                selectedValue = (Gaia.GaiaConstants.RegularSpawnerResourceType)EditorGUILayout.EnumPopup(GetLabel("Resource Type"), selectedValue);
                rule.m_resourceType = (SpawnerResourceType)(int)selectedValue;
            }
            //else
            //{
            //    switch (m_spawner.m_settings.m_worldmapSpawnerType)
            //    {
            //        case WorldmapSpawnerType.WorldGenerator:
            //            rule.m_resourceType = SpawnerResourceType.StampDistribution;
            //            break;
            //        case WorldmapSpawnerType.WorldBiomeMasks:
            //            rule.m_resourceType = SpawnerResourceType.WorldBiomeMask;
            //            break;
            //    }

            //}

            GUIContent[] assetChoices = null;
            switch (rule.m_resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_texturePrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_texturePrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_texturePrototypes[assetIdx].m_name);
                        }

                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewTexture");

                        break;
                    }
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_detailPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_detailPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_detailPrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewTerrainDetail");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_treePrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_treePrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_treePrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewTree");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.GameObject:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_gameObjectPrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewGameObject");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.SpawnExtension:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewSpawnExtension");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.StampDistribution:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_stampDistributionPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_stampDistributionPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[assetIdx].m_featureType);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewStampDistribution");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.WorldBiomeMask:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewWorldBiomeMask");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.Probe:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_probePrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_probePrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_probePrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewProbe");
                        break;
                    }
                case GaiaConstants.SpawnerResourceType.TerrainModifierStamp:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Length + 1];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[assetIdx].m_name);
                        }
                        assetChoices[assetChoices.Length - 1] = m_editorUtils.GetContent("AddNewTerrainModifierStamp");
                        break;
                    }

                    /*
                default:
                    {
                        assetChoices = new GUIContent[m_spawner.m_settings.m_resources.m_stampPrototypes.Length];
                        for (int assetIdx = 0; assetIdx < m_spawner.m_settings.m_resources.m_stampPrototypes.Length; assetIdx++)
                        {
                            assetChoices[assetIdx] = new GUIContent(m_spawner.m_settings.m_resources.m_stampPrototypes[assetIdx].m_name);
                        }
                        break;
                    } */
            }

            //Only allow resource selection on regular spawners, makes no sense for world map spawners
            if (!m_spawner.m_settings.m_isWorldmapSpawner)
            {
                rule.m_resourceIdx = EditorGUILayout.Popup(GetLabel("Selected Resource"), rule.m_resourceIdx, assetChoices);
                rule.m_resourceIdx = Mathf.Clamp(rule.m_resourceIdx, 0, assetChoices.Length - 1);
            }


            switch (rule.m_resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    //user wants a new resource? Create one on the fly
                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewTextureResource();
                    }

                    //rule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name;
                    break;
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    //user wants a new resource? Create one on the fly
                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewTerrainDetailResource();
                    }
                    //rule.m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name;
                    break;
                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    //user wants a new resource? Create one on the fly
                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewTreeResource();
                    }
                    //rule.m_name = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_name;
                    break;
                case GaiaConstants.SpawnerResourceType.GameObject:
                    //user wants a new resource? Create one on the fly
                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewGameObjectResource();
                    }

                    //rule.m_name = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_name;

                    //See if we can find a custom fitness
                    if (m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_instances.Length > 0)
                    {
                        GameObject go = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_instances[0].m_desktopPrefab;
                        bool gotExtension = false;
                        //if (go.GetComponent<ISpawnRuleExtension>() != null)
                        //{
                        //    gotExtension = true;
                        //}
                        //else
                        //{
                        //    if (go.GetComponentInChildren<ISpawnRuleExtension>() != null)
                        //    {
                        //        gotExtension = true;
                        //    }
                        //}
                        if (gotExtension)
                        {
                            Debug.Log("Got a spawn rule extension on " + go.name);
                        }
                    }
                    break;
                case GaiaConstants.SpawnerResourceType.SpawnExtension:
                    //user wants a new resource? Create one on the fly
                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewSpawnExtensionResource();
                    }

                    //rule.m_name = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[rule.m_resourceIdx].m_name;
                    break;
                case SpawnerResourceType.StampDistribution:

                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewStampDistributionResource();
                    }

                    //rule.m_name = m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[rule.m_resourceIdx].m_name;
                    break;
                case SpawnerResourceType.WorldBiomeMask:

                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewWorldBiomeMaskResource();
                    }

                    //rule.m_name = m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[rule.m_resourceIdx].m_name;
                    break;
                case SpawnerResourceType.Probe:

                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewProbeResource();
                    }
                    break;
                case SpawnerResourceType.TerrainModifierStamp:

                    if (rule.m_resourceIdx == assetChoices.Length - 1)
                    {
                        AddNewTerrainModifierStampResource();
                    }

                    break;
            }

            EditorGUI.indentLevel++;

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTexture)
            {
                m_spawner.m_textureResourcePrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx];
                //cache the textures to notice if the user changes those, if yes we need to immediately trigger a refresh to switch out the texture on terrain as well
                Texture2D oldDiffuse = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture;
                Texture2D oldNormal = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_normal;
                Texture2D oldMaskMap = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_maskmap;
#if SUBSTANCE_PLUGIN_ENABLED
                Substance.Game.Substance substanceMaterial = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_substanceMaterial;
                int substanceIndex = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].substanceSourceIndex;
                //m_editorUtils.Panel("Texture Prototype Settings", DrawTexturePrototype, false);
                DrawTexturePrototype(helpEnabled);

                if (m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_substanceMaterial != null)
                {
                    if (substanceMaterial != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_substanceMaterial || substanceIndex != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].substanceSourceIndex)
                    {
                        //prototype has changed, refresh on terrain & update resource & rule name
                        RefreshTerrainPrototype(spawnRuleID, oldDiffuse, null, substanceIndex);
                        m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_substanceMaterial.name;
                        rule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name;
                        m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture = GetSubstanceTexture(m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx], "baseColor");
                        m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_normal = GetSubstanceTexture(m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx], "normal");
                        m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_maskmap = GetSubstanceTexture(m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx], "mask");
                    }
                }
                else
                {
                    if (oldDiffuse != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture || oldNormal != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_normal || oldMaskMap != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_maskmap)
                    {
                        //prototype has changed, refresh on terrain & update resource & rule name
                        RefreshTerrainPrototype(spawnRuleID, oldDiffuse);
                        m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture.name;
                        rule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name;
                    }
                }

#else
                //m_editorUtils.Panel("Texture Prototype Settings", DrawTexturePrototype, false);
                DrawTexturePrototype(helpEnabled);

                if (oldDiffuse != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture || oldNormal != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_normal || oldMaskMap != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_maskmap)
                {
                    //prototype has changed, refresh on terrain & update resource & rule name
                    RefreshTerrainPrototype(spawnRuleID, oldDiffuse);
                    m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_texture.name;
                    if (rule.m_name == GaiaConstants.newSpawnRuleName)
                    {
                        rule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name;
                    }
                }
#endif

#if SUBSTANCE_PLUGIN_ENABLED
                substanceMaterial = null;
#endif
                oldDiffuse = null;
                oldNormal = null;
                oldMaskMap = null;

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainDetail)
            {
                Texture2D oldDetailTexture = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture;
                GameObject oldPrefab = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype;
                DetailRenderMode oldRenderMode = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_renderMode;
                m_spawner.m_terrainDetailPrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx];

                DrawTerrainDetailPrototype(helpEnabled);

                if (oldRenderMode != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_renderMode ||
                    oldDetailTexture != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture ||
                    oldPrefab != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype)
                {
                    //Texture was changed? Set detail prototype to null to prevent it remaining in the prototype on the terrain
                    if (oldDetailTexture != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture)
                    {
                        m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype = null;
                    }
                    else
                    {
                        //Prototype object was changed? Set texture to null to prevent it remaining in the prototype on the terrain
                        if (oldPrefab != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype)
                        {
                            m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture = null;
                        }
                    }

                    //prototype has changed, refresh on terrain & update resource & rule name
                    RefreshTerrainPrototype(spawnRuleID, oldDetailTexture, oldPrefab);

                    switch (m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_renderMode)
                    {
                        case DetailRenderMode.GrassBillboard:
                            if (m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture != null)
                            {
                                m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture.name;
                            }
                            break;
                        case DetailRenderMode.VertexLit:
                            if (m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype != null)
                            {
                                m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype.name;
                            }
                            break;
                        case DetailRenderMode.Grass:
                            if (oldDetailTexture != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture && m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture != null)
                            {
                                m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture.name;
                                m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype = null;
                            }
                            else
                            {
                                if (m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype != null)
                                {
                                    m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailProtoype.name;
                                    m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_detailTexture = null;
                                }
                            }
                            break;
                    }
                    if (rule.m_name == GaiaConstants.newSpawnRuleName)
                    {
                        rule.m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name;
                    }
                }
                oldDetailTexture = null;
                oldPrefab = null;


            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTree)
            {
                GameObject oldDesktopPrefab = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_desktopPrefab;
                m_spawner.m_treeResourcePrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx];

                DrawTreePrototype(m_spawner,helpEnabled);

                if (oldDesktopPrefab != m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_desktopPrefab)
                {
                    RefreshTerrainPrototype(spawnRuleID, null, oldDesktopPrefab);
                    m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_name = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_desktopPrefab.name;
                    if (rule.m_name == GaiaConstants.newSpawnRuleName)
                    {
                        rule.m_name = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_name;
                    }
                }

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.GameObject)
            {
                m_spawner.m_gameObjectResourcePrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx];

                DrawGameObjectPrototype(helpEnabled);

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.SpawnExtension)
            {
                m_spawner.m_spawnExtensionPrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[rule.m_resourceIdx];

                DrawSpawnerExtensionPrototype(helpEnabled);

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.StampDistribution)
            {
                m_spawner.m_stampDistributionPrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[rule.m_resourceIdx];
                //track whether the probability curve changed inside the resource settings - if yes, we need to re-bake the curve textures so the preview updates correctly
                bool curveChanged = false;
                DrawStampDistributionPrototype(helpEnabled, ref curveChanged);
                if (curveChanged)
                {
                    UpdateColorsForWorldDesignerPreview(true);
                }

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.WorldBiomeMask)
            {
                m_spawner.m_worldBiomeMaskPrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[rule.m_resourceIdx];

                DrawWorldBiomeMaskPrototype(helpEnabled);

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.Probe)
            {
                m_spawner.m_probePrototypeBeingDrawn = m_spawner.m_settings.m_resources.m_probePrototypes[rule.m_resourceIdx];

                DrawProbePrototype(helpEnabled);

            }

            if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainModifierStamp)
            {
                m_spawner.m_terrainModifierStampBeingDrawn = m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[rule.m_resourceIdx];

                DrawTerrainModifierStampPrototype(helpEnabled);

            }

            if (oldResourceType != rule.m_resourceType || oldResourceID != rule.m_resourceIdx)
            {
                //Resource type / ID were changed - if we are still using the generic new rule name, or we were using the exact name of the old resource
                //we autofill the rule name from the new resource
                if (rule.m_name == GaiaConstants.newSpawnRuleName || rule.m_name == GetResourceName(oldResourceType, oldResourceID))
                {
                    rule.m_name = GetResourceName(rule.m_resourceType, rule.m_resourceIdx);
                }
            }

            EditorGUI.indentLevel--;
        }

        private string GetResourceName(SpawnerResourceType resourceType, int resourceID)
        {
            switch (resourceType)
            {
                case SpawnerResourceType.TerrainTexture:
                    if (resourceID < m_spawner.m_settings.m_resources.m_texturePrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_texturePrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Texture Resource";
                    }
                case SpawnerResourceType.TerrainDetail:
                    if (resourceID < m_spawner.m_settings.m_resources.m_detailPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_detailPrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Detail Resource";
                    }
                case SpawnerResourceType.TerrainTree:
                    if (resourceID < m_spawner.m_settings.m_resources.m_treePrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_treePrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Tree Resource";
                    }
                case SpawnerResourceType.GameObject:
                    if (resourceID < m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_gameObjectPrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Game Object Resource";
                    }
                case SpawnerResourceType.SpawnExtension:
                    if (resourceID < m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Spawn Extension Resource";
                    }
                case SpawnerResourceType.Probe:
                    if (resourceID < m_spawner.m_settings.m_resources.m_probePrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_probePrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Probe Resource";
                    }
                case SpawnerResourceType.StampDistribution:
                    if (resourceID < m_spawner.m_settings.m_resources.m_stampDistributionPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_stampDistributionPrototypes[resourceID].m_featureType;
                    }
                    else
                    {
                        return "Stamp Distribution Resource";
                    }
                case SpawnerResourceType.WorldBiomeMask:
                    if (resourceID < m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "World Biome Mask Resource";
                    }
                case SpawnerResourceType.TerrainModifierStamp:
                    if (resourceID < m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Count())
                    {
                        return m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes[resourceID].m_name;
                    }
                    else
                    {
                        return "Terrain Modifier Stamp Resource";
                    }
            }

            return "";

        }

        private int AddNewGameObjectResource()
        {
            string newGameObjectName = m_editorUtils.GetTextValue("NewGameObject");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Where(x => x.m_name.StartsWith(newGameObjectName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_gameObjectPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_gameObjectPrototypes, new ResourceProtoGameObject() { m_name = newGameObjectName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Count() - 1;
        }

        private int AddNewSpawnExtensionResource()
        {
            string newSpawnExtension = m_editorUtils.GetTextValue("NewSpawnExtension");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Where(x => x.m_name.StartsWith(newSpawnExtension)).Count() + 1;
            m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes, new ResourceProtoSpawnExtension() { m_name = newSpawnExtension + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Count() - 1;
        }

        private int AddNewStampDistributionResource()
        {
            string newStampDistribution = m_editorUtils.GetTextValue("NewStampDistribution");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_stampDistributionPrototypes.Where(x => x.m_featureType.StartsWith(newStampDistribution)).Count() + 1;
            m_spawner.m_settings.m_resources.m_stampDistributionPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_stampDistributionPrototypes, new ResourceProtoStamp() { m_featureType = newStampDistribution + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_stampDistributionPrototypes.Count() - 1;
        }

        private int AddNewWorldBiomeMaskResource()
        {
            string newWorldBiomeMask = m_editorUtils.GetTextValue("NewWorldBiomeMask");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Where(x => x.m_name.StartsWith(newWorldBiomeMask)).Count() + 1;
            m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes, new ResourceProtoWorldBiomeMask() { m_name = newWorldBiomeMask + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_worldBiomeMaskPrototypes.Count() - 1;
        }

        private int AddNewProbeResource()
        {
            string newProbeName = m_editorUtils.GetTextValue("NewProbe");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_probePrototypes.Where(x => x.m_name.StartsWith(newProbeName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_probePrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_probePrototypes, new ResourceProtoProbe() { m_name = newProbeName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_probePrototypes.Count() - 1;
        }

        private int AddNewTerrainModifierStampResource()
        {
            string newTerrainModifierStampName = m_editorUtils.GetTextValue("NewTerrainModifierStamp");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Where(x => x.m_name.StartsWith(newTerrainModifierStampName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes, new ResourceProtoTerrainModifierStamp() { m_name = newTerrainModifierStampName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_terrainModifierStampPrototypes.Count() - 1;
        }
        
        private int AddNewTreeResource()
        {
            string newTreeName = m_editorUtils.GetTextValue("NewTree");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_treePrototypes.Where(x => x.m_name.StartsWith(newTreeName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_treePrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_treePrototypes, new ResourceProtoTree() { m_name = newTreeName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_treePrototypes.Count() - 1;
        }

        private int AddNewTerrainDetailResource()
        {
            string newDetailName = m_editorUtils.GetTextValue("NewTerrainDetail");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_detailPrototypes.Where(x => x.m_name.StartsWith(newDetailName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_detailPrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_detailPrototypes, new ResourceProtoDetail() { m_name = newDetailName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_detailPrototypes.Count() - 1;
        }

        private int AddNewTextureResource()
        {
            string newTextureName = m_editorUtils.GetTextValue("NewTexture");
            int nextNewNumber = m_spawner.m_settings.m_resources.m_texturePrototypes.Where(x => x.m_name.StartsWith(newTextureName)).Count() + 1;
            m_spawner.m_settings.m_resources.m_texturePrototypes = GaiaUtils.AddElementToArray(m_spawner.m_settings.m_resources.m_texturePrototypes, new ResourceProtoTexture() { m_name = newTextureName + " " + nextNewNumber.ToString() });
            return m_spawner.m_settings.m_resources.m_texturePrototypes.Count() - 1;
        }

        public static Texture2D GetSpawnRulePreviewTexture(SpawnRule rule, GaiaResource resource)
        {
            Texture2D resourceTexture = null;

            //draw preview
            switch (rule.m_resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    if (rule.m_resourceIdx < resource.m_texturePrototypes.Length && resource.m_texturePrototypes[rule.m_resourceIdx] != null)
                    {
                        resourceTexture = AssetPreview.GetAssetPreview(resource.m_texturePrototypes[rule.m_resourceIdx].m_texture);
                    }
                    break;
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    if (rule.m_resourceIdx < resource.m_detailPrototypes.Length && resource.m_detailPrototypes[rule.m_resourceIdx] != null)
                    {
                        ResourceProtoDetail protoDetail = resource.m_detailPrototypes[rule.m_resourceIdx];
                        switch (protoDetail.m_renderMode)
                        {
                            case DetailRenderMode.Grass:
                                resourceTexture = protoDetail.m_detailTexture;
                                break;
                            case DetailRenderMode.GrassBillboard:
                                resourceTexture = protoDetail.m_detailTexture;
                                break;
                            case DetailRenderMode.VertexLit:
                                resourceTexture = AssetPreview.GetAssetPreview(protoDetail.m_detailProtoype);
                                break;
                        }
                    }
                    break;

                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    if (rule.m_resourceIdx < resource.m_treePrototypes.Length && resource.m_treePrototypes[rule.m_resourceIdx] != null)
                    {
                        GameObject protoTree = resource.m_treePrototypes[rule.m_resourceIdx].m_desktopPrefab;
                        resourceTexture = AssetPreview.GetAssetPreview(protoTree);
                    }
                    break;
                case GaiaConstants.SpawnerResourceType.GameObject:
                    if (rule.m_resourceIdx < resource.m_gameObjectPrototypes.Length && resource.m_gameObjectPrototypes[rule.m_resourceIdx] != null)
                    {
                        //Get the first instance as preview object for now
                        if (resource.m_gameObjectPrototypes[rule.m_resourceIdx].m_instances.Length > 0)
                        {
                            GameObject protoGameObject = resource.m_gameObjectPrototypes[rule.m_resourceIdx].m_instances[0].m_desktopPrefab;
                            resourceTexture = AssetPreview.GetAssetPreview(protoGameObject);
                        }
                    }
                    break;
            }

            return resourceTexture;
        }

        private void CleanPreviewRuleIDs()
        {
            for (int i = m_spawner.m_previewRuleIds.Count - 1; i >= 0; i--)
            {
                if (m_spawner.m_previewRuleIds[i] > m_spawner.m_settings.m_spawnerRules.Count - 1)
                {
                    m_spawner.m_previewRuleIds.RemoveAt(i);
                }
            }
        }

        private void RemoveMaskList(int ruleIdx)
        {
            if (ruleIdx < 0 || ruleIdx >= m_reorderableRuleMasksLists.Length)
                return;
            ReorderableList[] newList = new ReorderableList[m_reorderableRuleMasksLists.Length - 1];
            for (int i = 0; i < newList.Length; ++i)
            {
                if (i < ruleIdx)
                {
                    newList[i] = m_reorderableRuleMasksLists[i];
                }
                else if (i >= ruleIdx)
                {
                    newList[i] = m_reorderableRuleMasksLists[i + 1];
                }
            }
            m_reorderableRuleMasksLists = newList;

            if (ruleIdx >= m_spawnRuleMaskListExpanded.Length)
                return;

            bool[] newExpandedList = new bool[m_spawnRuleMaskListExpanded.Length - 1];
            for (int i = 0; i < newExpandedList.Length; ++i)
            {
                if (i < ruleIdx)
                {
                    newExpandedList[i] = m_spawnRuleMaskListExpanded[i];
                }
                else if (i >= ruleIdx)
                {
                    newExpandedList[i] = m_spawnRuleMaskListExpanded[i + 1];
                }
            }
            m_spawnRuleMaskListExpanded = newExpandedList;
        }

        private void AddNewMaskList()
        {
            ReorderableList[] newList = new ReorderableList[m_reorderableRuleMasksLists.Length + 1];
            for (int i = 0; i < m_reorderableRuleMasksLists.Length; ++i)
            {
                newList[i] = m_reorderableRuleMasksLists[i];
            }
            newList[newList.Length - 1] = CreateSpawnRuleMaskList(newList[newList.Length - 1], m_spawner.m_settings.m_spawnerRules[newList.Length - 1].m_imageMasks);
            m_reorderableRuleMasksLists = newList;

            bool[] newExpandedList = new bool[m_spawnRuleMaskListExpanded.Length + 1];
            for (int i = 0; i < m_spawnRuleMaskListExpanded.Length; ++i)
            {
                newExpandedList[i] = m_spawnRuleMaskListExpanded[i];
            }

            newExpandedList[newExpandedList.Length - 1] = true;
            m_spawnRuleMaskListExpanded = newExpandedList;
        }

        private void RefreshTerrainPrototype(int spawnRuleID, Texture2D oldTexture = null, GameObject oldGameObject = null, int substanceIdx = 0, bool isUserRefresh = false)
        {
            Terrain activeTerrain = Gaia.TerrainHelper.GetActiveTerrain();

            if (activeTerrain == null)
            {
                return;
            }
            if (activeTerrain.terrainData == null)
            {
                return;
            }

            SpawnRule sr = m_spawner.m_settings.m_spawnerRules[spawnRuleID];
            switch (sr.m_resourceType)
            {
                case SpawnerResourceType.TerrainTexture:
#if CTS_PRESENT
                    if (m_spawner.ConnectedCTSProfile != null)
                    {
                        ResourceProtoTexture protoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx];

                        //Look up the CTS texture details according the old albedo texture
                        foreach (CTSTerrainTextureDetails textureDetails in m_spawner.ConnectedCTSProfile.TerrainTextures)
                        {
                            if (textureDetails.Albedo == oldTexture || textureDetails.Albedo == protoTexture.m_texture)
                            {
                                textureDetails.Albedo = protoTexture.m_texture;
                                textureDetails.Normal = protoTexture.m_normal;
                                textureDetails.Smoothness = protoTexture.m_CTSSmoothnessMap;
                                textureDetails.Roughness = protoTexture.m_CTSRoughnessMap;
                                textureDetails.Height = protoTexture.m_CTSHeightMap;
                                textureDetails.AmbientOcclusion = protoTexture.m_CTSAmbientOcclusionMap;
                                textureDetails.m_smoothness = protoTexture.m_smoothness;
                                textureDetails.m_normalStrength = protoTexture.m_normalScale;
                                textureDetails.m_albedoTilingClose = protoTexture.m_sizeX;

                                bool texturesWereChanged = false;

                                if (textureDetails.m_albedoWasChanged)
                                {
                                    texturesWereChanged = true;
                                    textureDetails.m_albedoWasChanged = false;
                                    m_spawner.ConnectedCTSProfile.m_needsAlbedosArrayUpdate = true;
                                    CTSTerrainManager.Instance.BroadcastAlbedoTextureSwitch(m_spawner.ConnectedCTSProfile, textureDetails.Albedo, textureDetails.m_textureIdx, textureDetails.m_albedoTilingClose);
                                }
                                
                                if (textureDetails.m_normalWasChanged)
                                {
                                    texturesWereChanged = true;
                                    textureDetails.m_normalWasChanged = false;
                                    m_spawner.ConnectedCTSProfile.m_needsNormalsArrayUpdate = true;
                                    CTSTerrainManager.Instance.BroadcastNormalTextureSwitch(m_spawner.ConnectedCTSProfile, textureDetails.Normal, textureDetails.m_textureIdx, textureDetails.m_albedoTilingClose);
                                }
                                if (textureDetails.m_smoothnessWasChanged)
                                {
                                    texturesWereChanged = true;
                                    textureDetails.m_smoothnessWasChanged = false;
                                    m_spawner.ConnectedCTSProfile.m_needsAlbedosArrayUpdate = true;
                                }
                                if (textureDetails.m_roughnessWasChanged)
                                {
                                    texturesWereChanged = true;
                                    textureDetails.m_roughnessWasChanged = false;
                                    m_spawner.ConnectedCTSProfile.m_needsAlbedosArrayUpdate = true;
                                }

                                if (textureDetails.m_heightWasChanged)
                                {
                                    texturesWereChanged = true;
                                    textureDetails.m_heightWasChanged = false;
                                    m_spawner.ConnectedCTSProfile.m_needsNormalsArrayUpdate = true;
                                }
                                CTSTerrainManager.Instance.BroadcastProfileUpdate(m_spawner.ConnectedCTSProfile);
                                EditorUtility.SetDirty(m_spawner.ConnectedCTSProfile);
                                if (texturesWereChanged)
                                {
                                    CTSTerrainManager.Instance.BroadcastShaderSetup(m_spawner.ConnectedCTSProfile);
                                }
                                break;
                            }
                        }

                    }
                    else
                    {

#endif
                    int texturePrototypeID = -1;

                    //look up prototype ID based on the old texture, if available it means the user wants to switch out textures
                    int localTerrainTextureIdx = 0;

                    if (oldTexture != null)
                    {

                        foreach (TerrainLayer proto in activeTerrain.terrainData.terrainLayers)
                        {
                            if (PWCommon5.Utils.IsSameTexture(oldTexture, proto.diffuseTexture, false))
                            {
                                texturePrototypeID = localTerrainTextureIdx;
                                break;
                            }

                            localTerrainTextureIdx++;
                        }
                    }
                    else
                    {
                        texturePrototypeID = m_spawner.m_settings.m_resources.PrototypeIdxInTerrain(SpawnerResourceType.TerrainTexture, sr.m_resourceIdx);
                    }

                    //Check if any other spawn rules currently use this terrain layer already - if yes, we cannot alter it and must create a new alternative layer instead.
                    bool layerUsedByAnotherRule = false;
                    bool sameResourceUsedByBothRules = false;
                    if (texturePrototypeID != -1)
                    {
                        TerrainLayer oldLayer = activeTerrain.terrainData.terrainLayers[texturePrototypeID];
                        string oldLayerGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(oldLayer));

                        Spawner[] allSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
                        foreach (Spawner spawner in allSpawners)
                        {
                            foreach (SpawnRule checkRule in spawner.m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.TerrainTexture))
                            {
                                if (spawner.m_settings.m_resources.m_texturePrototypes[checkRule.m_resourceIdx].m_LayerGUID == oldLayerGUID && checkRule != sr)
                                {
                                    layerUsedByAnotherRule = true;
                                    if (checkRule.m_resourceIdx == sr.m_resourceIdx)
                                    {
                                        sameResourceUsedByBothRules = true;
                                    }
                                    break;
                                }
                            }
                            if (layerUsedByAnotherRule)
                            {
                                break;
                            }
                        }
                    }

                    if (texturePrototypeID != -1 && !layerUsedByAnotherRule)
                    {
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            if (t.terrainData.terrainLayers.Length < texturePrototypeID)
                            {
                                continue;
                            }

                            ResourceProtoTexture resourceProtoTexture = m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx];
                            //reference the exisiting prototypes, then assign them - otherwise the terrain details won't update properly
                            TerrainLayer[] exisitingLayers = t.terrainData.terrainLayers;


#if SUBSTANCE_PLUGIN_ENABLED
                            if (resourceProtoTexture.m_substanceMaterial != null)
                            {
                                List<Substance.Game.SubstanceGraph> substanceGraphs = resourceProtoTexture.m_substanceMaterial.graphs;
                                if (substanceGraphs != null)
                                {
                                    if (resourceProtoTexture.substanceSourceIndex == 0)
                                    {
                                        resourceProtoTexture.substanceSourceIndex = 1;
                                    }
                                    List<Texture2D> substanceTextures = substanceGraphs[resourceProtoTexture.substanceSourceIndex - 1].GetGeneratedTextures();
                                    if (substanceTextures.Count > 0)
                                    {
                                        foreach (Texture2D texture in substanceTextures)
                                        {
                                            if (texture.name.EndsWith("baseColor"))
                                            {
                                                exisitingLayers[texturePrototypeID].diffuseTexture = texture;
                                            }
                                            else if (texture.name.EndsWith("normal"))
                                            {
                                                if (texture.format == TextureFormat.BC5)
                                                {
                                                    Debug.LogWarning("Normal compress format is BC5 please select the substance material and select DXT5");
                                                }
                                                exisitingLayers[texturePrototypeID].normalMapTexture = texture;
                                            }
                                            else if (texture.name.EndsWith("mask"))
                                            {
                                                exisitingLayers[texturePrototypeID].maskMapTexture = texture;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError("No substance textures found at graph index 0. Please make sure you've generated your substance material");
                                    }
                                }
                            }
                            else
                            {
                                exisitingLayers[texturePrototypeID].diffuseTexture = resourceProtoTexture.m_texture;
                                exisitingLayers[texturePrototypeID].normalMapTexture = resourceProtoTexture.m_normal;
                                exisitingLayers[texturePrototypeID].maskMapTexture = resourceProtoTexture.m_maskmap;
                            }
#else
                            exisitingLayers[texturePrototypeID].diffuseTexture = resourceProtoTexture.m_texture;
                            exisitingLayers[texturePrototypeID].normalMapTexture = resourceProtoTexture.m_normal;
                            exisitingLayers[texturePrototypeID].maskMapTexture = resourceProtoTexture.m_maskmap;
#endif
                            exisitingLayers[texturePrototypeID].tileSize = new Vector2(resourceProtoTexture.m_sizeX, resourceProtoTexture.m_sizeY);
                            exisitingLayers[texturePrototypeID].tileOffset = new Vector2(resourceProtoTexture.m_offsetX, resourceProtoTexture.m_offsetY);
                            exisitingLayers[texturePrototypeID].normalScale = resourceProtoTexture.m_normalScale;
                            exisitingLayers[texturePrototypeID].diffuseRemapMax = resourceProtoTexture.m_diffuseRemapMax;
                            exisitingLayers[texturePrototypeID].diffuseRemapMin = resourceProtoTexture.m_diffuseRemapMin;
                            exisitingLayers[texturePrototypeID].maskMapRemapMax = resourceProtoTexture.m_maskMapRemapMax;
                            exisitingLayers[texturePrototypeID].maskMapRemapMin = resourceProtoTexture.m_maskMapRemapMin;
                            exisitingLayers[texturePrototypeID].specular = resourceProtoTexture.m_specularColor;
                            exisitingLayers[texturePrototypeID].metallic = resourceProtoTexture.m_metallic;
                            exisitingLayers[texturePrototypeID].smoothness = resourceProtoTexture.m_smoothness;
                            t.terrainData.terrainLayers = exisitingLayers;
                        }
                    }
                    else
                    {
                        if (isUserRefresh && !layerUsedByAnotherRule)
                        {

                            //Prototype was not found
                            if (EditorUtility.DisplayDialog("Texture not found on Terrain?", "It looks like this texture is not part of a terrain layer on the current terrain because it has not been spawned on this terrain yet. Do you want to add this texture to all currently active terrains in the scene now?", "Add the texture", "Cancel"))
                            {
                                m_spawner.m_settings.m_resources.AddPrototypeToTerrain(sr.m_resourceType, sr.m_resourceIdx, Terrain.activeTerrains);
                            }
                        }
                        if (layerUsedByAnotherRule)
                        {
                            EditorUtility.DisplayDialog("Creation of new texture layer", "You are changing a texture resource that shares its terrain layer with another spawn rule. This will create a new terrain layer with the changed texture at the end of the terrain layer stack on your terrain. \r\n\r\n This can result in the terrain layers on the terrain having a different order than the spawn rules in your spawner. This is normally not a problem on its own, but can create issues in other tools that depend on the terrain layer order. \r\n\r\n If you would like to keep the order between the spawn rules and terrain layers in sync, consider removing all terrain layers from the terrain via Advanced > Resource Management > Remove Resources and then running the spawner again.", "OK");

                            //We need to create a new resource to keep both layers independent
                            if (sameResourceUsedByBothRules)
                            {
                                int oldIdx = sr.m_resourceIdx;
                                sr.m_resourceIdx = AddNewTextureResource();
                                GaiaUtils.CopyFields(m_spawner.m_settings.m_resources.m_texturePrototypes[oldIdx], m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx]);
                                m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx].m_name = ObjectNames.GetUniqueName(m_spawner.m_settings.m_resources.m_texturePrototypes.Select(x => x.m_name).ToArray(), m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx].m_name);

                            }

                            //reset the layer GUID - we want to force the creation of a new texture layer with the new texture
                            m_spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx].m_LayerGUID = "";
                            m_spawner.m_settings.m_resources.AddPrototypeToTerrain(sr.m_resourceType, sr.m_resourceIdx, Terrain.activeTerrains, true);
                        }
                    }
#if CTS_PRESENT
                    }
#endif
                    break;
                case SpawnerResourceType.TerrainDetail:

                    int detailPrototypeID = -1;

                    //look up prototype ID based on the old texture, if available it means the user wants to switch out textures
                    if (oldTexture != null || oldGameObject != null)
                    {
                        int localTerrainIdx = 0;
                        foreach (DetailPrototype proto in activeTerrain.terrainData.detailPrototypes)
                        {
                            if (m_spawner.m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx].m_renderMode == DetailRenderMode.VertexLit || (m_spawner.m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx].m_renderMode == DetailRenderMode.Grass && oldGameObject != null))
                            {
                                if (oldGameObject == proto.prototype)
                                {
                                    detailPrototypeID = localTerrainIdx;
                                    break;
                                }
                            }
                            else
                            {
                                if (PWCommon5.Utils.IsSameTexture(oldTexture, proto.prototypeTexture, false) == true)
                                {
                                    detailPrototypeID = localTerrainIdx;
                                    break;
                                }
                            }
                            localTerrainIdx++;
                        }
                    }
                    else
                    {
                        detailPrototypeID = m_spawner.m_settings.m_resources.PrototypeIdxInTerrain(SpawnerResourceType.TerrainDetail, sr.m_resourceIdx);
                    }
                    if (detailPrototypeID != -1)
                    {
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            if (t.terrainData.detailPrototypes.Length < detailPrototypeID)
                            {
                                continue;
                            }

                            ResourceProtoDetail resourceProtoDetail = m_spawner.m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx];
                            //reference the exisiting prototypes, then assign them - otherwise the terrain details won't update properly
                            DetailPrototype[] exisitingPrototypes = t.terrainData.detailPrototypes;
                            exisitingPrototypes[detailPrototypeID].dryColor = resourceProtoDetail.m_dryColour;
                            exisitingPrototypes[detailPrototypeID].healthyColor = resourceProtoDetail.m_healthyColour;
                            exisitingPrototypes[detailPrototypeID].maxHeight = resourceProtoDetail.m_maxHeight;
                            exisitingPrototypes[detailPrototypeID].minHeight = resourceProtoDetail.m_minHeight;
                            exisitingPrototypes[detailPrototypeID].minWidth = resourceProtoDetail.m_maxWidth;
                            exisitingPrototypes[detailPrototypeID].maxWidth = resourceProtoDetail.m_minWidth;
                            exisitingPrototypes[detailPrototypeID].noiseSpread = resourceProtoDetail.m_noiseSpread;
                            exisitingPrototypes[detailPrototypeID].prototype = resourceProtoDetail.m_detailProtoype;
                            exisitingPrototypes[detailPrototypeID].prototypeTexture = resourceProtoDetail.m_detailTexture;
#if UNITY_2021_2_OR_NEWER
                            //if instancing is used, render mode needs to be forced to "Vertex lit" otherwise unity will display
                            //an error message
                            if (resourceProtoDetail.m_useInstancing)
                            {
                                resourceProtoDetail.m_renderMode = DetailRenderMode.VertexLit;
                                exisitingPrototypes[detailPrototypeID].renderMode = DetailRenderMode.VertexLit;
                            }
                            else
                            {
                                exisitingPrototypes[detailPrototypeID].renderMode = resourceProtoDetail.m_renderMode;
                            }
                            exisitingPrototypes[detailPrototypeID].useInstancing = resourceProtoDetail.m_useInstancing;

#else
                            exisitingPrototypes[detailPrototypeID].renderMode = resourceProtoDetail.m_renderMode;
#endif
#if UNITY_2022_2_OR_NEWER
                            exisitingPrototypes[detailPrototypeID].alignToGround = resourceProtoDetail.m_alignToGround;
                            exisitingPrototypes[detailPrototypeID].density = resourceProtoDetail.m_density;
                            exisitingPrototypes[detailPrototypeID].holeEdgePadding = resourceProtoDetail.m_holeEdgePadding;
                            exisitingPrototypes[detailPrototypeID].noiseSeed = resourceProtoDetail.m_noiseSeed;
                            exisitingPrototypes[detailPrototypeID].positionJitter = resourceProtoDetail.m_positionJitter;
                            exisitingPrototypes[detailPrototypeID].targetCoverage = resourceProtoDetail.m_targetCoverage;
                            exisitingPrototypes[detailPrototypeID].useDensityScaling = resourceProtoDetail.m_useDensityScaling;
#endif
                            if (resourceProtoDetail.m_detailProtoype != null)
                            {
                                exisitingPrototypes[detailPrototypeID].usePrototypeMesh = true;
                            }
                            else
                            {
                                exisitingPrototypes[detailPrototypeID].usePrototypeMesh = false;
                            }
                            t.terrainData.detailPrototypes = exisitingPrototypes;
                            //t.terrainData.detailPrototypes[prototypeID].usePrototypeMesh = resourceProtoDetail.pr;
                        }
                    }
                    else
                    {
                        if (isUserRefresh)
                        {
                            //Prototype was not found
                            if (EditorUtility.DisplayDialog("Detail Prototype not found on Terrain?", "It looks like this detail prototype is not part of the terrain details on the current terrain because it has not been spawned on this terrain yet. Do you want to add this detail prototype to all currently active terrains in the scene now?", "Add the prototype", "Cancel"))
                            {
                                m_spawner.m_settings.m_resources.AddPrototypeToTerrain(sr.m_resourceType, sr.m_resourceIdx, Terrain.activeTerrains);
                            }
                        }
                    }
                    break;

                case SpawnerResourceType.TerrainTree:

                    int treePrototypeID = -1;

                    //look up prototype ID based on the old tree, if available it means the user wants to switch out prefabs
                    if (oldGameObject != null)
                    {
                        int localTerrainIdx = 0;
                        foreach (TreePrototype proto in activeTerrain.terrainData.treePrototypes)
                        {
                            if (oldGameObject == proto.prefab)
                            {
                                treePrototypeID = localTerrainIdx;
                                break;
                            }
                            localTerrainIdx++;
                        }
                    }
                    else
                    {
                        treePrototypeID = m_spawner.m_settings.m_resources.PrototypeIdxInTerrain(SpawnerResourceType.TerrainTree, sr.m_resourceIdx);
                    }
                    if (treePrototypeID != -1)
                    {
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            if (t.terrainData.treePrototypes.Length < treePrototypeID)
                            {
                                continue;
                            }

                            ResourceProtoTree resourceProtoTree = m_spawner.m_settings.m_resources.m_treePrototypes[sr.m_resourceIdx];
                            //reference the exisiting prototypes, then assign them - otherwise the terrain details won't update properly
                            TreePrototype[] exisitingPrototypes = t.terrainData.treePrototypes;
                            exisitingPrototypes[treePrototypeID].bendFactor = resourceProtoTree.m_bendFactor;
                            exisitingPrototypes[treePrototypeID].prefab = resourceProtoTree.m_desktopPrefab;
                            t.terrainData.treePrototypes = exisitingPrototypes;

                            //adjust the scaling (if required)
                            if (resourceProtoTree.NeedsRescale())
                            {
                                TreeInstance[] treeInstances = new TreeInstance[t.terrainData.treeInstances.Length];

                                for (int i = 0; i < treeInstances.Length; i++)
                                {
                                    TreeInstance exisitingInstance = t.terrainData.treeInstances[i];
                                    TreeInstance newInstance = new TreeInstance();
                                    newInstance.prototypeIndex = exisitingInstance.prototypeIndex;
                                    newInstance.lightmapColor = exisitingInstance.lightmapColor;
                                    newInstance.position = exisitingInstance.position;
                                    newInstance.rotation = exisitingInstance.rotation;

                                    if (newInstance.prototypeIndex == treePrototypeID)
                                    {
                                        if (resourceProtoTree.m_spawnScale == SpawnScale.Fixed)
                                        {
                                            newInstance.widthScale = resourceProtoTree.m_minWidth;
                                            newInstance.heightScale = resourceProtoTree.m_minHeight;
                                        }
                                        else
                                        {
                                            newInstance.widthScale = Mathf.Lerp(resourceProtoTree.m_minWidth, resourceProtoTree.m_maxWidth, Mathf.InverseLerp(resourceProtoTree.m_lastUsedMinWidth, resourceProtoTree.m_lastUsedMaxWidth, exisitingInstance.widthScale));
                                            newInstance.heightScale = Mathf.Lerp(resourceProtoTree.m_minHeight, resourceProtoTree.m_maxHeight, Mathf.InverseLerp(resourceProtoTree.m_lastUsedMinHeight, resourceProtoTree.m_lastUsedMaxHeight, exisitingInstance.heightScale));
                                        }
                                    }
                                    else
                                    {
                                        newInstance.widthScale = exisitingInstance.widthScale;
                                        newInstance.heightScale = exisitingInstance.heightScale;
                                    }
                                    treeInstances[i] = newInstance;
                                }

                                t.terrainData.treeInstances = treeInstances;
                                resourceProtoTree.StoreLastUsedScaleSettings();
                            }
                        }
                    }
                    else
                    {
                        if (isUserRefresh)
                        {
                            //Prototype was not found
                            if (EditorUtility.DisplayDialog("Tree not found on Terrain?", "It looks like this tree is not part of the tree prototypes on the current terrain because it has not been spawned on this terrain yet. Do you want to add this tree prototype to all currently active terrains in the scene now?", "Add the prototype", "Cancel"))
                            {
                                m_spawner.m_settings.m_resources.AddPrototypeToTerrain(sr.m_resourceType, sr.m_resourceIdx, Terrain.activeTerrains);
                            }
                        }
                    }
                    break;
            }
        }

#if SUBSTANCE_PLUGIN_ENABLED
        private Texture2D GetSubstanceTexture(ResourceProtoTexture resource, string textureSearch)
        {
            Texture2D returningTexture = null;
            List<Substance.Game.SubstanceGraph> substanceGraphs = resource.m_substanceMaterial.graphs;
            if (substanceGraphs != null)
            {
                List<Texture2D> substanceTextures = substanceGraphs[resource.substanceSourceIndex - 1].GetGeneratedTextures();
                if (substanceTextures.Count > 0)
                {
                    foreach (Texture2D texture in substanceTextures)
                    {
                        if (texture.name.EndsWith(textureSearch))
                        {
                            returningTexture = texture;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("No substance textures found at graph index " + resource.substanceSourceIndex + "." + "Please make sure you've generated your substance material");
                }
            }

            return returningTexture;
        }
#endif

        private void DrawTexturePrototype(bool showHelp)
        {
#if CTS_PRESENT
                   GaiaResourceEditor.DrawTexturePrototype(m_spawner.m_textureResourcePrototypeBeingDrawn, m_editorUtils, showHelp, m_spawner.ConnectedCTSProfile!=null);
#else
            GaiaResourceEditor.DrawTexturePrototype(m_spawner.m_textureResourcePrototypeBeingDrawn, m_editorUtils, showHelp);
#endif
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18);

            string buttonKey = "ResourceProtoRefreshButton";
#if CTS_PRESENT
            if (m_spawner.ConnectedCTSProfile != null)
            {
                buttonKey = "ResourceProtoRefreshInCTSProfile";
            }
#endif
            if (m_editorUtils.Button(buttonKey))
            {
                RefreshTerrainPrototype(m_spawner.m_spawnRuleIndexBeingDrawn, null, null, 0, true);
            }

#if CTS_PRESENT
            if (m_spawner.ConnectedCTSProfile != null)
            {
                if (m_editorUtils.Button("EditCTSProfile"))
                {
                    Selection.activeObject = m_spawner.ConnectedCTSProfile;
                }
            }
#endif

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

        }

        private void DrawTreePrototype(Spawner spawner, bool showHelp)
        {
            GaiaResourceEditor.DrawTreePrototype(m_spawner.m_treeResourcePrototypeBeingDrawn, spawner, m_editorUtils, showHelp);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18);
            bool currentGUIState = GUI.enabled;
            if (m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_usesBoxStandIn)
            {
                //Disable Refresh button as long as a Stand-In for the tree is in use
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("ResourceProtoRefreshButton"))
            {
                RefreshTerrainPrototype(m_spawner.m_spawnRuleIndexBeingDrawn, null, null, 0, true);
            }

            GUI.enabled = currentGUIState;

            if (m_spawner.m_settings.m_spawnerRules[m_spawner.m_spawnRuleIndexBeingDrawn].m_usesBoxStandIn)
            {
                if (m_editorUtils.Button("ResourceProtoDisableBoxStandIn"))
                {
                    m_spawner.DeactivateTreeStandIn(m_spawner.m_spawnRuleIndexBeingDrawn);
                }
            }
            else
            {
                if (m_editorUtils.Button("ResourceProtoEnableBoxStandIn"))
                {
                    m_spawner.ActivateTreeStandIn(m_spawner.m_spawnRuleIndexBeingDrawn);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTerrainDetailPrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawTerrainDetailPrototype(m_spawner.m_terrainDetailPrototypeBeingDrawn, m_editorUtils, m_spawner, showHelp);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18);
            if (m_editorUtils.Button("ResourceProtoRefreshButton"))
            {
                RefreshTerrainPrototype(m_spawner.m_spawnRuleIndexBeingDrawn, null, null, 0, true);
            }
            EditorGUILayout.EndHorizontal();
        }


        private void DrawGameObjectPrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawGameObjectPrototype(m_spawner.m_gameObjectResourcePrototypeBeingDrawn, m_editorUtils, showHelp);
        }

        private void DrawSpawnerExtensionPrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawSpawnExtensionPrototype(m_spawner.m_spawnExtensionPrototypeBeingDrawn, m_editorUtils, showHelp);
        }

        private void DrawStampDistributionPrototype(bool showHelp, ref bool probabilityCurveChanged)
        {
            GaiaResourceEditor.DrawStampDistributionPrototype(m_spawner.m_stampDistributionPrototypeBeingDrawn, m_editorUtils, m_stampCategoryNames, m_stampCategoryIDArray, ref probabilityCurveChanged, showHelp);
        }

        private void DrawWorldBiomeMaskPrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawWorldBiomeMaskPrototype(m_spawner.m_worldBiomeMaskPrototypeBeingDrawn, m_editorUtils, showHelp);
        }

        private void DrawProbePrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawProbePrototype(m_spawner.m_probePrototypeBeingDrawn, m_editorUtils, m_gaiaSettings.m_currentRenderer, showHelp);
        }

        private void DrawTerrainModifierStampPrototype(bool showHelp)
        {
            GaiaResourceEditor.DrawTerrainModifierStampPrototype(m_spawner.m_terrainModifierStampBeingDrawn, m_editorUtils, showHelp);
        }

        private void DrawSaveAndLoad(bool obj)
        {
            GUI.backgroundColor = m_normalBGColor;
            if (m_spawner.m_createdfromBiomePreset)
            {
                m_SaveAndLoadMessage = m_editorUtils.GetTextValue("CreatedFromBiomePresetMessage");
                m_SaveAndLoadMessageType = MessageType.Warning;
            }

            if (m_spawner.m_createdFromGaiaManager)
            {
                m_SaveAndLoadMessage = m_editorUtils.GetTextValue("CreatedFromGaiaManagerMessage");
                m_SaveAndLoadMessageType = MessageType.Warning;
            }

            if (!String.IsNullOrEmpty(m_SaveAndLoadMessage))
                EditorGUILayout.HelpBox(m_SaveAndLoadMessage, m_SaveAndLoadMessageType, true);

            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("LoadButton"))
            {
                //Dismiss Tutorial messages at this point
                m_spawner.m_createdfromBiomePreset = false;
                m_spawner.m_createdFromGaiaManager = false;

                string path = AssetDatabase.GUIDToAssetPath(m_spawner.m_settings.m_lastGUIDSaved);
                if (string.IsNullOrEmpty(path))
                {
                    path = GaiaDirectories.GetSettingsDirectory();
                }
                path = path.Remove(path.LastIndexOf(Path.AltDirectorySeparatorChar));

                string openFilePath = EditorUtility.OpenFilePanel("Load Spawner settings..", path, "asset");

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
                    SpawnerSettings settingsToLoad = (SpawnerSettings)AssetDatabase.LoadAssetAtPath(openFilePath, typeof(SpawnerSettings));

                    if (settingsToLoad != null)
                    {
                        //Load in the resource file that was last used first

                        //settingsToLoad.m_resourcesPath = AssetDatabase.GUIDToAssetPath(settingsToLoad.m_resourcesGUID);

                        m_spawner.LoadSettings(settingsToLoad);

                        //m_spawner.m_settings.m_resources = (GaiaResource)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsToLoad.m_resourcesGUID), typeof(GaiaResource));

                        CreateMaskLists();
                        //Reset Preview Images state
                        m_previewImageDisplayedDuringLayout = new bool[m_spawner.m_settings.m_spawnerRules.Count];
                        //Update the internal editor position / scale values after loading
                        //x = m_stamper.m_settings.m_x;
                        //y = m_stamper.m_settings.m_y;
                        //z = m_stamper.m_settings.m_z;
                        //rotation = m_stamper.m_settings.m_rotation;
                        //width = m_stamper.m_settings.m_width;
                        //height = m_stamper.m_settings.m_height;
                        //mark stamper as dirty so it will be redrawn
                        m_spawner.m_spawnPreviewDirty = true;
                        m_spawner.SetWorldBiomeMasksDirty();
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
            if (m_editorUtils.Button("SaveButton"))
            {
                string dialogPath = AssetDatabase.GUIDToAssetPath(m_spawner.m_settings.m_lastGUIDSaved);
                string filename = m_spawner.transform.name;
                if (string.IsNullOrEmpty(dialogPath))
                {
                    dialogPath = GaiaDirectories.GetUserBiomeDirectory("Single Spawners");

                }
                else
                {
                    filename = dialogPath.Substring(dialogPath.LastIndexOf('/') + 1).Replace(".asset", "");
                }

                string saveFilePath = EditorUtility.SaveFilePanel("Save Spawner settings as..", dialogPath, filename, "asset");

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
                    if(m_spawner.SaveSettings(saveFilePath))
                    {
                        CreateMaskLists();

                        m_spawner.SetWorldBiomeMasksDirty();
                        //update the gaia manager window (if exists)
                        foreach (GaiaManagerEditor gme in Resources.FindObjectsOfTypeAll<GaiaManagerEditor>())
                        {
                            gme.UpdateAllSpawnersList();
                        }
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveSuccessful").text;
                        m_changesMadeSinceLastSave = false;
                        m_SaveAndLoadMessageType = MessageType.Info;


                    }
                    else
                    {
                        m_SaveAndLoadMessage = m_editorUtils.GetContent("SaveFailed").text;
                        m_SaveAndLoadMessageType = MessageType.Error;
                    }
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        public void Spawn(bool allTerrains)
        {
            bool cancel = true;

            //Check that they have at least one single selected terrain when doing a local spawn
            if (!allTerrains && Gaia.TerrainHelper.GetActiveTerrainCount() < 1)
            {
                EditorUtility.DisplayDialog("OOPS!", "You must have at least one terrain visible in order to do a local spawn. Please place your spawner over a terraon or add a new terrain from the Gaia Manager Window.", "OK");
                return;
            }
            else
            {
                cancel = false;
            }

            if (m_gaiaSettings == null)
            {
                m_gaiaSettings = GaiaUtils.GetGaiaSettings();
            }

            if (!SessionManager.CheckIfHeightmapRestoreISAllowed(new List<Spawner>() { m_spawner}, m_gaiaSettings))
            {
                EditorGUIUtility.ExitGUI();
                return;
            }


            if (!cancel)
            {
                //deactivate preview - so that we can see the result

                if (m_gaiaSettings.m_spawnerAutoHidePreviewMilliseconds > 0)
                {
                    m_activatePreviewRequested = true;
                    m_activatePreviewTimeStamp = GaiaUtils.GetUnixTimestamp();
                }
                m_spawner.m_drawPreview = false;

                m_spawner.Spawn(allTerrains);
            }
        }

        /// <summary>
        /// Delete any old rules left over from previous resources / changes to resources
        /// </summary>
        void CleanUpRules()
        {
            //Drop out if no spawner or resources
            if (m_spawner == null || m_spawner.m_settings.m_resources == null)
            {
                return;
            }

            //Drop out if spawner doesnt have resources
            int idx = 0;
            SpawnRule rule;
            bool dirty = false;
            while (idx < m_spawner.m_settings.m_spawnerRules.Count)
            {
                rule = m_spawner.m_settings.m_spawnerRules[idx];

                switch (rule.m_resourceType)
                {
                    case GaiaConstants.SpawnerResourceType.TerrainTexture:
                        {
                            if (rule.m_resourceIdx >= m_spawner.m_settings.m_resources.m_texturePrototypes.Length)
                            {
                                m_spawner.m_settings.m_spawnerRules.RemoveAt(idx);
                                dirty = true;
                            }
                            else
                            {
                                //if (rule.m_name != m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name)
                                //{
                                //    rule.m_name = m_spawner.m_settings.m_resources.m_texturePrototypes[rule.m_resourceIdx].m_name;
                                //    dirty = true;
                                //}
                                idx++;
                            }
                            break;
                        }
                    case GaiaConstants.SpawnerResourceType.TerrainDetail:
                        {
                            if (rule.m_resourceIdx >= m_spawner.m_settings.m_resources.m_detailPrototypes.Length)
                            {
                                m_spawner.m_settings.m_spawnerRules.RemoveAt(idx);
                                dirty = true;
                            }
                            else
                            {
                                //if (rule.m_name != m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name)
                                //{
                                //    rule.m_name = m_spawner.m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_name;
                                //    dirty = true;
                                //}
                                idx++;
                            }
                            break;
                        }
                    case GaiaConstants.SpawnerResourceType.TerrainTree:
                        {
                            if (rule.m_resourceIdx >= m_spawner.m_settings.m_resources.m_treePrototypes.Length)
                            {
                                m_spawner.m_settings.m_spawnerRules.RemoveAt(idx);
                                dirty = true;
                            }
                            else
                            {
                                //if (rule.m_name != m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_name)
                                //{
                                //    rule.m_name = m_spawner.m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_name;
                                //    dirty = true;
                                //}
                                idx++;
                            }
                            break;
                        }
                    case GaiaConstants.SpawnerResourceType.GameObject:
                        {
                            if (rule.m_resourceIdx >= m_spawner.m_settings.m_resources.m_gameObjectPrototypes.Length)
                            {
                                m_spawner.m_settings.m_spawnerRules.RemoveAt(idx);
                                dirty = true;
                            }
                            else
                            {
                                //if (rule.m_name != m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_name)
                                //{
                                //    rule.m_name = m_spawner.m_settings.m_resources.m_gameObjectPrototypes[rule.m_resourceIdx].m_name;
                                //    dirty = true;
                                //}
                                idx++;
                            }
                            break;
                        }
                    case GaiaConstants.SpawnerResourceType.SpawnExtension:
                        {
                            if (rule.m_resourceIdx >= m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes.Length)
                            {
                                m_spawner.m_settings.m_spawnerRules.RemoveAt(idx);
                                dirty = true;
                            }
                            else
                            {
                                //if (rule.m_name != m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[rule.m_resourceIdx].m_name)
                                //{
                                //    rule.m_name = m_spawner.m_settings.m_resources.m_spawnExtensionPrototypes[rule.m_resourceIdx].m_name;
                                //    dirty = true;
                                //}
                                idx++;
                            }
                            break;
                        }

                    default:
                        idx++;
                        break;

                }
            }
            //Mark it as dirty if we deleted something
            if (dirty)
            {
                Debug.LogWarning(string.Format("{0} : There was a mismatch between your spawner settings and your resources file. Spawner settings have been updated to match resources.", m_spawner.name));
                EditorUtility.SetDirty(m_spawner);
            }
        }

        ///// <summary>
        ///// Draw a progress bar
        ///// </summary>
        ///// <param name="label"></param>
        ///// <param name="value"></param>

        //void ProgressBar(string label, float value)
        //{
        //    // Get a rect for the progress bar using the same margins as a textfield:
        //    Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        //    EditorGUI.ProgressBar(rect, value, label);
        //    EditorGUILayout.Space();
        //}

        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_showTooltips && m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {
                return new GUIContent(name);
            }
        }

        /// <summary>
        /// The tooltips
        /// </summary>
        static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Resources", "The object that contains the resources that these rules will apply to." },
            { "Execution Mode", "The way this spawner runs. Design time : At design time only. Runtime Interval : At run time on a timed interval. Runtime Triggered Interval : At run time on a timed interval, and only when the tagged game object is closer than the trigger range from the center of the spawner." },
            { "Shape", "The shape of the spawn area. The spawner will only spawn within this area." },
            { "Range","Distance in meters from the centre of the spawner that the spawner can spawn in. Shown as a red box or sphere in the gizmos." },
            { "Spawn Interval", "The time in seconds between spawn iterations." },
            { "Trigger Range","Distance in meters from the centre of the spawner that the trigger will activate." },
            { "Trigger Tags","The tags of the game objects that will set the spawner off. Multiple tags can be separated by commas eg Player,Minion etc." },
            { "Rule Selector", "The way a rule is selected to be spawned. \nAll : All rules are selected. \nFittest : Only the rule with the fittest spawn criteria is selected. If multiple rules have the same fitness then one will be randomly selected.\nWeighted Fittest : The chance of a rule being selected is directly proportional to its fitness. Fitter rules have more chance of selection. Use this to create more natural blends between objects.\nRandom : Rule selection is random." },
            { "Spawn Mdoe", "This setting controls whether the spawned instances will replace, will be added to, or will be removed from the existing instances on the terrain." },
            { "Collision Layers", "Controls which layers are checked for collisions when spawning. Must at least include the layer the terrain is on. Add additional layers if other collisions need to be detected as well. Influences terrain detection, tree detection and game object detection." },
            { "Location Selector", "How the spawner selects locations to spawn in. \nEvery Location: The spawner will attempt to spawn at every location. \nEvery Location Jittered: The spawner will attempt to spawn at every location, but will offset the location by a random jitter factor. Use this to break up lines.\nRandom Location: The spawner will attempt to spawn at random locations.\nRandom Location Clustered: The spawner will attempt to spawn clusters at random locations." },
            { "Location Increment", "The distance from the last location that every new location will be incremented in meters." },
            { "Max Jitter Percent", "Every new location will be offset by a random distance up to a maximum of the jitter percentage multiplied by the location increment." },
            { "Locations Per Spawn", "The number of locations that will be checked every Spawn interval. This does not guarantee that something will be spawned at that location, because lack of fitness may preclude that location from being used." },
            { "Max Cluster Size", "The maximum individuals in a cluster before a new cluster is started." },

            { "X", "Delete all rules."},
            { "I", "Inavtivate all rules."},
            { "A", "Activate all rules."},
            { "+", "Add a rule."},
            { "-", "Delete the rule."},
            { "Visualise", "Visualise this rule in the visualiser."},

            { "Distance Mask", "Mask fitness over distance. Left hand side of curve represents the centre of the spawner. Use this to alter spawn success away from centre e.g. peter out towards edges."},
            { "Area Mask", "Mask fitness over area. None - Don't apply image filter. Grey Scale - apply image filter using greys scale. R - Apply filter from red channel. G - Apply filter from green channel. B - Apply filter from blue channel. A - Apply filter from alpha channel. Terrain Texture Slot - apply mask from texture painted on terrain."},
            { "Image Mask", "The texure to use as the source of the area mask."},
            { "Smooth Mask", "Smooth the mask before applying it. This is a nice way to clean noise up in the mask, or to soften the edges of the mask."},
            { "Normalise Mask", "Normalise the mask before applying it. Ensures that the full dynamic range of the mask is used."},
            { "Invert Mask", "Invert the mask before applying it."},
            { "Flip Mask", "Flip the mask on its x and y axis mask before applying it. Useful sometimes to match the unity terrain as this is flipped internally."},
            { "Seed", "The unique seed for this spawner. If the environment, resources or rules dont change, then hitting Reset and respawning will always regenerate the same result." },

            { "Noise Mask", "Mask fitness with a noise value."},
            { "Noise Seed", "The seed value for the noise function - the same seed will always generate the same noise for a given set of parameters."},
            { "Octaves", "The amount of detail in the noise - more octaves mean more detail and longer calculation time."},
            { "Persistence", "The roughness of the noise. Controls how quickly amplitudes diminish for successive octaves. 0..1."},
            { "Frequency", "The frequency of the first octave."},
            { "Lacunarity", "The frequency multiplier between successive octaves. Experiment between 1.5 - 3.5."},
            { "Zoom", "The zoom level of the noise. Larger zooms display the noise over larger areas."},
            { "Invert", "Invert the noise."},


            { "Name", "Rule name - purely for convenience" },
            { "Resource Type", "The type of resource this rule will apply to." },
            { "Selected Resource", "The resource this rule applies to. To modify how the resource interprets terrain fitness change its spawn criteria." },
            { "Min Viable Fitness", "The minimum fitness needed to be considered viable to spawn." },
            { "Failure Rate", "The amount of the time that the rule will fail even if fit enough. 0 means never fail, and 1 means always fail. Use this to thin things out." },
            { "Max Instances", "The maximum number of resource instances this rule can spawn. Use this to stop over population." },
            { "Ignore Max Instances", "Ignores the max instances criteria. Useful for texturing very large terrains." },
            { "Active", "Whether this rule is active or not. Use this to disable the rule."},
            { "Curr Inst Count", "The number of instances of this rule that have been spawned."},
            { "Instances Spawned", "The number of times this resource has been spawned." },
            { "Inactive Inst Count", "The number of inactive instances that have been spawned, but are now inactive and in the pool for re-use. Only relevant when game objects have been spawned" },

            { "Active Rules", "The number of active rules being managed by the spawner."},
            { "Inactive Rules", "The number of inactive rules being managed by the spawner."},
            { "TOTAL Rules", "The total number of rules being managed by the spawner."},
            { "MAX INSTANCES", "The maximum number of instances that can be managed by the spawner."},
            { "Active Instances", "The number of active instances being managed by the spawner."},
            { "Inactive Instances", "The number inactive instances being managed by the spawner."},
            { "TOTAL Instances", "The total number of active and inactive instances being managed by the spawner."},

            { "Min Direction", "Minimum rotation in degrees for this game object spawner." },
            { "Max Direction", "Maximum rotation in degrees fpr this game object spawner." },

            { "Ground Level", "Ground level for this feature, used to make positioning easier." },
            { "Show Ground Level", "Show ground level." },
            { "Stick To Ground", "Stick to ground level." },
            { "Show Gizmos", "Show the spawners gizmos." },
            { "Show Rulers", "Show rulers." },
            { "Show Statistics", "Show spawner statistics." },
            { "Flatten", "Flatten the entire terrain - use with care!" },
            { "Smooth", "Smooth the entire terrain - removes jaggies and increases frame rate - run multiple times to increase effect - use with care!" },
            { "Clear Trees", "Clear trees from entire terrain - use with care!" },
            { "Clear Details", "Clear details / grass from entire terrain - use with care!" },
            { "Ground", "Position the spawner at ground level on the terrain." },
            { "Fit To Terrain", "Fits and aligns the spawner to the terrain." },
            { "Fit To World (Inactive)", "Fitting to world is disabled because the world size exceeds the maximum range for a single spawn." },
            { "Reset", "Resets the spawner, deletes any spawned game objects, and resets the random number generator." },
            { "Spawn Local", "Run a single spawn iteration at the exact spawner position and range. You can run as many spawn iterations as you like." },
            { "Spawn World", "Run a single spawn iteration on every terrain in the scene. You can run as many spawn iterations as you like." },
        };
    }
}
