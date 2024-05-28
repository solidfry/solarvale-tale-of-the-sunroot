using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Gaia.GaiaConstants;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using ProceduralWorlds;

#if !UNITY_2021_2_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#endif

#if CTS_PRESENT
using CTS;
#endif

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using ProceduralWorlds.HierachySystem;

namespace Gaia
{
    [System.Serializable]
    public enum ClearSpawnFrom { AnySource, OnlyThisSpawner }
    public enum ClearSpawnFromBiomes { AnySource, OnlyThisBiome }
    public enum ClearSpawnFor { CurrentTerrainOnly, AllTerrains }
    public enum WorldDesignerPreviewMode { Worldmap, SingleTerrain }

    /// <summary>
    /// simple data structure to store a protototype id and a terrain together
    /// used to log which prototype ids were already deleted from a terrain during world spawns
    /// </summary>
    public class TerrainPrototypeId
    {
        public string m_terrainDataGUID;
        public string m_prototypeAssetGUID;
    }

    /// <summary>
    /// Simple data structure to store spawn rules with missing prototypes per terrain.
    /// This is used during spawning to make sure prototypes are only placed on terrains where they are actually used.
    /// </summary>
    public class TerrainMissingSpawnRules
    {
        public Terrain terrain;
        public List<SpawnRule> spawnRulesWithMissingResources = new List<SpawnRule>();
    }

    /// <summary>
    /// Data structure to pass in terrain position information into the simulate compute shader
    /// </summary>
    struct TerrainPosition
    {
        public int terrainID;
        public Vector2Int min;
        public Vector2Int max;
        public int affected;
    };

    /// <summary>
    /// Data structure to get object height information from a compute shader
    /// </summary>
    struct ObjectPosition
    {
        //disabling the compiler warning for the variable not being set - it is set in a compute shader
#pragma warning disable 649
        public Vector3 position;
#pragma warning restore 649
    }

    /// <summary>
    /// Data structure to get stamp spawning information from a compute shader
    /// </summary>
    struct StampPosition
    {
        //disabling the compiler warning for the variable not being set - it is set in a compute shader
#pragma warning disable 649
        public Vector3 position;
#pragma warning restore 649
    }

    /// <summary>
    /// A generic spawning system.
    /// </summary>
    [ExecuteInEditMode]
    [System.Serializable]
    public class Spawner : MonoBehaviour
    {

        [SerializeField]
        private SpawnerSettings settings;
        /// <summary>
        /// The current spawner settings
        /// </summary>
        public SpawnerSettings m_settings
        {
            get
            {
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<SpawnerSettings>();
                    settings.m_resources = new GaiaResource();
                    settings.m_resources.m_name = "NewResources";
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        private Stamper m_baseTerrainStamper;
        public Stamper GetOrCreateBaseTerrainStamper(bool allowCreate)
        {
            if (m_baseTerrainStamper == null && allowCreate)
            {
                GameObject go = CreateBaseTerrainStamper();
                //make sure we are subscribed to rendering finished event right after creation
                Stamper stamper = go.GetComponent<Stamper>();
                stamper.OnWorldDesignerStampRenderingFinished -= OnWDStapmperRenderingFinished;
                stamper.OnWorldDesignerStampRenderingFinished += OnWDStapmperRenderingFinished;
            }
            if (m_baseTerrainStamper != null)
            {
                m_baseTerrainStamper.transform.hideFlags = HideFlags.HideInHierarchy;
            }
            return m_baseTerrainStamper;
        }

        private Stamper m_terrainModifierPreviewStamper;

        public Stamper GetOrCreatePreviewStamper(bool allowCreate = false)
        {
            if (m_terrainModifierPreviewStamper == null && allowCreate)
            {
                CreateTerrainModifierPreviewStamper();
            }
            m_terrainModifierPreviewStamper.transform.hideFlags = HideFlags.HideInHierarchy;
            return m_terrainModifierPreviewStamper;
        }



        [SerializeField]
        private WorldCreationSettings worldCreationSettings;
        /// <summary>
        /// The settings for world creation if this is a random terrain generator spawner running on the world map
        /// </summary>
        public WorldCreationSettings m_worldCreationSettings
        {
            get
            {
                if (worldCreationSettings == null)
                {
                    worldCreationSettings = ScriptableObject.CreateInstance<WorldCreationSettings>();
                }
                return worldCreationSettings;
            }
            set
            {
                worldCreationSettings = value;
            }
        }



        [SerializeField]
        private BaseTerrainSettings baseTerrainsettings;
        /// <summary>
        /// The settings for base terrain creation in this is a random terrain generation spawner on a world map
        /// </summary>
        public BaseTerrainSettings m_baseTerrainSettings
        {
            get
            {
                if (baseTerrainsettings == null)
                {
                    baseTerrainsettings = ScriptableObject.CreateInstance<BaseTerrainSettings>();
                }
                return baseTerrainsettings;
            }
            set
            {
                baseTerrainsettings = value;
            }
        }

        //Holds all the generated stamps for random generation
        public List<StamperSettings> m_worldMapStamperSettings = new List<StamperSettings>();

#if GAIA_PRO_PRESENT
        private TerrainLoader m_terrainLoader;
        public TerrainLoader TerrainLoader
        {
            get
            {
                if (m_terrainLoader == null)
                {
                    if (this != null)
                    {
                        m_terrainLoader = gameObject.GetComponent<TerrainLoader>();

                        if (m_terrainLoader == null)
                        {
                            m_terrainLoader = gameObject.AddComponent<TerrainLoader>();
                            m_terrainLoader.hideFlags = HideFlags.HideInInspector;
                        }
                    }
                }
                return m_terrainLoader;
            }
        }
        public LoadMode m_loadTerrainMode = LoadMode.EditorSelected;
        public int m_impostorLoadingRange;
#endif

#if CTS_PRESENT

        [System.NonSerialized]
        private CTSProfile m_connectedCTSProfile = null;

        //This construct ensures we only serialize the GUID of the CTS profile, but not the profile itself
        //The GUID will "survive" when CTS is not installed in a project, while the CTS profile object would not
        public CTSProfile ConnectedCTSProfile
        {
            get
            {
                if (m_connectedCTSProfile == null && m_connectedCTSProfileGUID != null)
                {
#if UNITY_EDITOR
                    m_connectedCTSProfile = (CTSProfile)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(m_connectedCTSProfileGUID), typeof(CTSProfile));
#endif
                }
                return m_connectedCTSProfile;
            }
            set
            {
#if UNITY_EDITOR
                m_connectedCTSProfileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
                m_connectedCTSProfile = value;
#endif
            }
        }

#endif

        //need this serialized to remember the GUID even when PP is not installed in the project
        [SerializeField]
        private string m_connectedCTSProfileGUID = "";

        public delegate void SpawnFinishedCallback();
#if UNITY_EDITOR
        public event SpawnFinishedCallback OnSpawnFinished;
#endif

        /// <summary>
        /// The spawner ID
        /// </summary>
        public string m_spawnerID = Guid.NewGuid().ToString();

        /// <summary>
        /// Operational mode of the spawner
        /// </summary>
        public Gaia.GaiaConstants.OperationMode m_mode = GaiaConstants.OperationMode.DesignTime;

        /// <summary>
        /// Source for the random number generator
        /// </summary>
        public int m_seed = DateTime.Now.Millisecond;


        /// <summary>
        /// The world map terrain
        /// </summary>
        //public Terrain m_worldMapTerrain;

        /// <summary>
        /// The shape of the spawner
        /// </summary>
        public Gaia.GaiaConstants.SpawnerShape m_spawnerShape = GaiaConstants.SpawnerShape.Box;

        /// <summary>
        /// The rule selection approach
        /// </summary>
        public Gaia.GaiaConstants.SpawnerRuleSelector m_spawnRuleSelector = GaiaConstants.SpawnerRuleSelector.WeightedFittest;

        /// <summary>
        /// The type of spawner
        /// </summary>
        public Gaia.GaiaConstants.SpawnerLocation m_spawnLocationAlgorithm = GaiaConstants.SpawnerLocation.RandomLocation;

        /// <summary>
        /// The type of check performed at every location
        /// </summary>
        public Gaia.GaiaConstants.SpawnerLocationCheckType m_spawnLocationCheckType = GaiaConstants.SpawnerLocationCheckType.PointCheck;

        /// <summary>
        /// The step amount used when EveryLocation is selected
        /// </summary>
        public float m_locationIncrement = 1f;

        /// <summary>
        /// The maximum random offset on a jittered location
        /// </summary>
        public float m_maxJitteredLocationOffsetPct = 0.9f;

        /// <summary>
        /// Number of times a check is made for a new spawn location every interval 
        /// </summary>
        public int m_locationChecksPerInt = 1;

        /// <summary>
        /// In seeded mode, this will be the maximum number of individual spawns in a cluster before another locaiton is chosen
        /// </summary>
        public int m_maxRandomClusterSize = 50;

        //public GaiaResource m_resources;

        /// <summary>
        /// This will allow the user to filter the relative strength of items spawned by distance from the center
        /// </summary>
        public AnimationCurve m_spawnFitnessAttenuator = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        /// <summary>
        /// The image fitness filter mode to apply
        /// </summary>
        public Gaia.GaiaConstants.ImageFitnessFilterMode m_areaMaskMode = Gaia.GaiaConstants.ImageFitnessFilterMode.None;

        /// <summary>
        /// This will enable ot disable the collider cache at runtime - can be quite handy to keep them on some spawners
        /// </summary>
        public bool m_enableColliderCacheAtRuntime = false;

        /// <summary>
        /// This is used to filter the fitness based on the supplied texture, can be used in conjunction with th fitness attenuator
        /// </summary>
        public Texture2D m_imageMask;

        /// <summary>
        /// This is used to invert the fitness based on the supplied texture, can also be used in conjunction with the fitness attenuator
        /// </summary>
        public bool m_imageMaskInvert = false;

        /// <summary>
        /// This is used to normalise the fitness based on the supplied texture, can also be used in conjunction with the fitness attenuator
        /// </summary>
        public bool m_imageMaskNormalise = false;

        /// <summary>
        /// Flip the x, z of the image texture - sometimes required to match source with unity terrain
        /// </summary>
        public bool m_imageMaskFlip = false;

        /// <summary>
        /// This is used to smooth the supplied image mask texture
        /// </summary>
        public int m_imageMaskSmoothIterations = 3;

        /// <summary>
        /// The heightmap for the image filter
        /// </summary>
        [NonSerialized]
        public HeightMap m_imageMaskHM;

        /// <summary>
        /// Our noise generator
        /// </summary>
        private Gaia.FractalGenerator m_noiseGenerator;

        /// <summary>
        /// Seed for noise based fractal
        /// </summary>
        public float m_noiseMaskSeed = 0;

        /// <summary>
        /// The amount of detail in the fractal - more octaves mean more detail and longer calc time.
        /// </summary>
        public int m_noiseMaskOctaves = 8;

        /// <summary>
        /// The roughness of the fractal noise. Controls how quickly amplitudes diminish for successive octaves. 0..1.
        /// </summary>
        public float m_noiseMaskPersistence = 0.25f;

        /// <summary>
        /// The frequency of the first octave
        /// </summary>
        public float m_noiseMaskFrequency = 1f;

        /// <summary>
        /// The frequency multiplier between successive octaves. Experiment between 1.5 - 3.5.
        /// </summary>
        public float m_noiseMaskLacunarity = 1.5f;

        /// <summary>
        /// The zoom level of the noise
        /// </summary>
        public float m_noiseZoom = 10f;

        /// <summary>
        /// Invert the boise value
        /// </summary>
        public bool m_noiseInvert = false;

        /// <summary>
        /// How often the spawner should check to release new instances in seconds
        /// </summary>
        public float m_spawnInterval = 5f;

        /// <summary>
        /// The player to use for distance checks
        /// </summary>
        public string m_triggerTags = "Player";

        /// <summary>
        /// System will only iterate through spawn rules if the player / trigger object is closer than this distance
        /// </summary>
        public float m_triggerRange = 130f;

        /// <summary>
        /// Used to constrain which layers the spawner will attempt to get collisions on - used for virgin detection, terrain detection, tree detection and game object detection
        /// </summary>
        public LayerMask m_spawnCollisionLayers;

        /// <summary>
        /// Set to the terrain layer so that colliders are correctly setup
        /// </summary>
        public int m_spawnColliderLayer = 0;

        /// <summary>
        /// Whether or not to show gizmos
        /// </summary>
        public bool m_showGizmos = true;

        /// <summary>
        /// Whether or not to show debug messages
        /// </summary>
        public bool m_showDebug = false;


        /// <summary>
        /// Set to true once the base terrain has been stamped in a world generator spawner
        /// </summary>
        public bool m_baseTerrainStamped = false;

        /// <summary>
        /// Whether or not to show statistics
        /// </summary>
        //public bool m_showStatistics = true;

        /// <summary>
        /// Whether or not to show the terrain helper
        /// </summary>
        //public bool m_showTerrainHelper = true;

        /// <summary>
        /// Random number generator for this spawner - generates locations
        /// </summary>
        public Gaia.XorshiftPlus m_rndGenerator;

        /// <summary>
        /// Whether or not we are currently caching texures
        /// </summary>
        private bool m_cacheDetails = false;

        /// <summary>
        /// Detail map cache - used when doing area updates on details - indexed by the ID of the terrain it comes from
        /// </summary>
        private Dictionary<int, List<HeightMap>> m_detailMapCache = new Dictionary<int, List<HeightMap>>();

        /// <summary>
        /// Whether or not we are currently caching texures
        /// </summary>
        private bool m_cacheTextures = false;

        /// <summary>
        /// Set to true if the texture map is modified and needs to be written back to the terrain
        /// </summary>
        private bool m_textureMapsDirty = false;

        /// <summary>
        /// Texture map cache - used when doing area updates / reads on textures - indexed by the ID of the terrain it comes from
        /// </summary>
        private Dictionary<int, List<HeightMap>> m_textureMapCache = new Dictionary<int, List<HeightMap>>();

        /// <summary>
        /// Whether or not we are currently caching tags
        /// </summary>
        private bool m_cacheTags = false;

        /// <summary>
        /// Tagged game object cache
        /// </summary>
        private Dictionary<string, Quadtree<GameObject>> m_taggedGameObjectCache = new Dictionary<string, Quadtree<GameObject>>();

        /// <summary>
        /// Whether or not the trees are cached
        /// </summary>
        //private bool m_cacheTrees = false;

        /// <summary>
        /// Tree cache
        /// </summary>
        public TreeManager m_treeCache = new TreeManager();

        /// <summary>
        /// Whether or not we are currently caching height maps
        /// </summary>
        private bool m_cacheHeightMaps = false;

        /// <summary>
        /// Set to true if the height map is modified and needs to be written back to the terrain
        /// </summary>
        private bool m_heightMapDirty = false;

        /// <summary>
        /// Height map cache - used when doing area updates / reads on heightmaps - indexed by the ID of the terrain it comes from
        /// </summary>
        private Dictionary<int, UnityHeightMap> m_heightMapCache = new Dictionary<int, UnityHeightMap>();

        /// <summary>
        /// Whether or not we are currently caching height maps
        /// </summary>
        //private bool m_cacheStamps = false;

        /// <summary>
        /// Stamp cache - used to cache stamps when interacting with heightmaps - activated when heightmap cache is activated
        /// </summary>
        private Dictionary<string, HeightMap> m_stampCache = new Dictionary<string, HeightMap>();

        /// <summary>
        /// The sphere collider cache - used to test for area bounds
        /// </summary>
        [NonSerialized]
        public GameObject m_areaBoundsColliderCache;

        /// <summary>
        /// The game object collider cache - used to test for game object collisions
        /// </summary>
        [NonSerialized]
        public GameObject m_goColliderCache;

        /// <summary>
        /// The game object parent transform - used to make it easier to rehome spawned game objects
        /// </summary>
        [NonSerialized]
        public GameObject m_goParentGameObject;

        /// <summary>
        /// Set to true to cancel the spawn
        /// </summary>
        private static bool m_cancelSpawn = false;

        /// <summary>
        /// Handy counters for statistics
        /// </summary>
        //public int m_totalRuleCnt = 0;
        //public int m_activeRuleCnt = 0;
        //public int m_inactiveRuleCnt = 0;
        //public ulong m_maxInstanceCnt = 0;
        //public ulong m_activeInstanceCnt = 0;
        //public ulong m_inactiveInstanceCnt = 0;
        //public ulong m_totalInstanceCnt = 0;

        /// <summary>
        /// Handy check results - only one check at a time will ever be performed
        /// </summary>
        private float m_terrainHeight = 0f;
        private RaycastHit m_checkHitInfo = new RaycastHit();

        /// <summary>
        /// Use for co-routine simulation
        /// </summary>
        public IEnumerator m_updateCoroutine;
        /// <summary>


        public IEnumerator m_updateCoroutine2;
        /// Amount of time per allowed update
        /// </summary>
        public float m_updateTimeAllowed = 1f / 30f;

        /// <summary>
        /// Current status
        /// </summary>
        public float m_spawnProgress = 0f;

        /// <summary>
        /// Whether or not its completed processing
        /// </summary>
        public bool m_spawnComplete = true;

        /// <summary>
        /// The spawner bounds
        /// </summary>
        public Bounds m_spawnerBounds = new Bounds();

        /// <summary>
        /// Controls whether the spawn Preview needs to be redrawn
        /// </summary>
        public bool m_spawnPreviewDirty;

        /// <summary>
        /// The last active terrain this spawner was displayed for.
        /// </summary>
        public float m_lastActiveTerrainSize = 1024;

        /// <summary>
        /// The state of the "Toggle All" checkbox for regular spawn rules on top / end of the spawn rules list
        /// </summary>
        public bool m_spawnRuleRegularToggleAllState = true;

        /// <summary>
        /// The state of the "Toggle All" checkbox for stamp spawn rules on top / end of the spawn rules list
        /// </summary>
        public bool m_spawnRuleBiomeMasksToggleAllState = true;

        /// <summary>
        /// The state of the "Toggle All" checkbox for world biome mask spawn rules on top / end of the spawn rules list
        /// </summary>
        public bool m_spawnRuleStampsToggleAllState = true;


        /// <summary>
        /// Cached settings that are configired during the init call
        /// </summary>
        private bool m_isTextureSpawner = false;
        private bool m_isDetailSpawner = false;
        private bool m_isTreeSpawnwer = false;
        private bool m_isGameObjectSpawner = false;

        private RenderTexture m_cachedPreviewHeightmapRenderTexture;
        private RenderTexture[] m_cachedPreviewColorRenderTextures = new RenderTexture[GaiaConstants.maxPreviewedTextures];

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

        public bool m_drawPreview = false;
        public List<int> m_previewRuleIds = new List<int>();
        public float m_maxWorldHeight;
        public float m_minWorldHeight;
        public bool m_showSeaLevelinStampPreview = true;
        public bool m_terrainGeneratorPanelUnfolded = true;
        public bool m_worldDesignerPreviewSettingsUnfolded = false;
        public bool m_rulePanelUnfolded;
        public bool m_createBaseTerrainUnfolded = false;
        public bool m_sizeAndResolutionUnfolded = true;
        public bool m_exportTerrainUnfolded;
        public bool m_worldBiomeMasksUnfolded;
        public bool m_createdfromBiomePreset;
        public bool m_createdFromGaiaManager;
        public bool m_showSeaLevelPlane = true;
        public bool m_showBoundingBox = true;
        public float m_seaLevel;
        public int m_worldDesignerPreviewTileX = -99;
        public int m_worldDesignerPreviewTileZ = -99;
        public bool m_worldDesignerClearStampsWarningShown = false;

        //The Spawner Editor is a complex editor drawing settings for resources, spawner settings, reorderable mask lists, etc.
        //For all this to work it is sometimes required to store the current thing that is "Being Drawn" in a temporary variable so it becomes accessible elsewhere.
        public ImageMask[] m_maskListBeingDrawn;
        public CollisionMask[] m_collisionMaskListBeingDrawn;
        public int m_spawnRuleIndexBeingDrawn;
        public int m_spawnRuleMaskIndexBeingDrawn;
        public ResourceProtoTexture m_textureResourcePrototypeBeingDrawn;
        public ResourceProtoTree m_treeResourcePrototypeBeingDrawn;
        public ResourceProtoDetail m_terrainDetailPrototypeBeingDrawn;
        public ResourceProtoGameObject m_gameObjectResourcePrototypeBeingDrawn;
        public ResourceProtoSpawnExtension m_spawnExtensionPrototypeBeingDrawn;
        public ResourceProtoStamp m_stampDistributionPrototypeBeingDrawn;
        public ResourceProtoWorldBiomeMask m_worldBiomeMaskPrototypeBeingDrawn;
        public ResourceProtoProbe m_probePrototypeBeingDrawn;
        public ResourceProtoTerrainModifierStamp m_terrainModifierStampBeingDrawn;


        //Lists for cleared prototypes when doing multiterrain world spawns
        //(Textures and terrain details are handled differently)
        private List<TerrainPrototypeId> m_clearedTreeProtos = new List<TerrainPrototypeId>();
        private List<TerrainPrototypeId> m_clearedDetailProtos = new List<TerrainPrototypeId>();
        private List<TerrainPrototypeId> m_clearedGameObjectProtos = new List<TerrainPrototypeId>();
        private List<TerrainPrototypeId> m_clearedSpawnExtensionProtos = new List<TerrainPrototypeId>();
        private List<TerrainPrototypeId> m_clearedStampDistributionProtos = new List<TerrainPrototypeId>();
        private List<TerrainPrototypeId> m_clearedProbeProtos = new List<TerrainPrototypeId>();
        private List<Terrain> m_restoredHeightmapTerrains = new List<Terrain>();



        private AnimationCurve m_strengthTransformCurve = ImageMask.NewAnimCurveStraightUpwards();
        private Texture2D m_strengthTransformCurveTexture;
        public bool m_useExistingTerrainForWorldMapExport;
        public bool m_stampOperationsFoldedOut;
        public bool m_worldSizeAdvancedUnfolded;
        public EnvironmentSize m_worldTileSize;
        public int m_worldDesignerBiomePresetSelection = int.MinValue;
        public WorldDesignerPreviewMode m_worldDesignerPreviewMode;
        public List<BiomeSpawnerListEntry> m_BiomeSpawnersToCreate = new List<BiomeSpawnerListEntry>();


        /// <summary>
        /// Used to store the last world size that this (worldmap-)spawner spawned stamps upon - this is required to check if the user changed the world size in the meantime.
        /// </summary>
        [SerializeField]
        private Vector3 m_lastStampSpawnWorldSize;

        /// <summary>
        /// Used to store the last heightmap resolution that this (worldmap-)spawner exported to - this is required to check if the user changed the heightmap resolution in the meantime.
        /// </summary>
        public int m_lastExportedHeightMapResolution;

        /// <summary>
        /// Used to store the last world size that this (worldmap-)spawner exported to - this is required to check if the user changed the world size in the meantime.
        /// </summary>
        public int m_lastExportedWorldSize;

        private Texture2D StrengthTransformCurveTexture
        {
            get
            {
                return ImageProcessing.CreateMaskCurveTexture(ref m_strengthTransformCurveTexture);
            }
        }

        private GaiaSessionManager m_sessionManager;
        public bool m_qualityPresetAdvancedUnfolded;
        public bool m_biomeMaskPanelUnfolded;
        public bool m_spawnStampsPanelUnfolded;
        public bool m_ExportRunning;
        public GaiaConstants.DroppableResourceType m_dropAreaResource;
        public bool m_highlightLoadingSettings;
        public long m_highlightLoadingSettingsStartedTimeStamp;
        public bool m_foldoutSpawnerSettings;
        public Bounds m_worldDesignerUserBounds;
        public Vector3 m_worldDesignerSampleObjectPos1;
        public Vector3 m_worldDesignerSampleObjectPos2;
        public Vector3 m_worldDesignerSampleObjectPos3;
        public bool m_drawWorldDesignerSampleObjects = true;
        public bool m_drawWorldDesignerHandles = false;
        public bool m_drawWorldDesignerRulers = true;
        public bool m_drawWorldDesignerDensityViz = false;
        public long m_autoStampDensityVisualizationTimeStamp;

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

        /// <summary>
        /// Called by unity in editor when this is enabled - unity initialisation is quite opaque!
        /// </summary>
        void OnEnable()
        {
            //Check layer mask
            if (m_spawnCollisionLayers.value == 0)
            {
                m_spawnCollisionLayers = Gaia.TerrainHelper.GetActiveTerrainLayer();
            }

            m_spawnColliderLayer = Gaia.TerrainHelper.GetActiveTerrainLayerAsInt();

            //Create the random generator if we dont have one
            if (m_rndGenerator == null)
            {
                m_rndGenerator = new XorshiftPlus(m_seed);
            }

            //Get the min max height from the current terrain
            UpdateMinMaxHeight();

            if (m_connectedCTSProfileGUID == "1")
            {
                //This is just to get rid off the compilation warning when CTS is not installed in the project
            }

            if (m_settings.m_isWorldmapSpawner)
            {
                Stamper stamper = GetOrCreateBaseTerrainStamper(true);
                stamper.OnWorldDesignerStampRenderingFinished -= OnWDStapmperRenderingFinished;
                stamper.OnWorldDesignerStampRenderingFinished += OnWDStapmperRenderingFinished;
            }


        }

        private void OnWDStapmperRenderingFinished()
        {
            SpawnWDPreviewObjects();
        }


        /// <summary>
        /// Create the sample world designer scale objects in the center of the currently previewed area
        /// </summary>
        private void SpawnWDPreviewObjects()
        {
            if (this == null)
            {
                return;
            }

            if (transform == null)
            {
                return;
            }

            Vector3 centerPos = transform.position;

            Stamper stamper = GetOrCreateBaseTerrainStamper(false);

            if (stamper != null)
            {
                centerPos = stamper.m_worldDesignerPreviewBounds.center;
            }
            m_worldDesignerSampleObjectPos1 = new Vector3(centerPos.x - 20f, 0f, centerPos.z);
            m_worldDesignerSampleObjectPos2 = new Vector3(centerPos.x, 0f, centerPos.z);
            m_worldDesignerSampleObjectPos3 = new Vector3(centerPos.x + 20f, 0f, centerPos.z);
            m_worldDesignerSampleObjectPos1 = GetHeightOnWorldDesignerPreview(m_worldDesignerSampleObjectPos1);
            m_worldDesignerSampleObjectPos2 = GetHeightOnWorldDesignerPreview(m_worldDesignerSampleObjectPos2);
            m_worldDesignerSampleObjectPos3 = GetHeightOnWorldDesignerPreview(m_worldDesignerSampleObjectPos3);
        }

        public void ControlSpawnRuleGUIDs()
        {
            Spawner[] allSpawner = Resources.FindObjectsOfTypeAll<Spawner>();
            foreach (SpawnRule rule in m_settings.m_spawnerRules)
            {
                //check if the spawn rule guid exists in this scene already - if yes, this rule must get a new ID then to avoid duplicate IDs
                if (allSpawner.Select(x => x.m_settings.m_spawnerRules).Where(x => x.Find(y => y.GUID == rule.GUID) != null).Count() > 1)
                {
                    rule.RegenerateGUID();
                }
            }
        }

        private void OnDestroy()
        {
            ImageMask.RefreshSpawnRuleGUIDs();
            m_settings.ClearImageMaskTextures();
            Stamper stamper = GetOrCreateBaseTerrainStamper(false);
            if (stamper != null)
            {
                stamper.OnWorldDesignerStampRenderingFinished -= OnWDStapmperRenderingFinished;
            }
        }

        void OnDisable()
        {
        }

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
#if UNITY_EDITOR
            m_spawnComplete = false;
            EditorApplication.update += EditorUpdate;
#endif
        }

        //Stop editor updates
        public void StopEditorUpdates()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            m_spawnComplete = true;
            if (OnSpawnFinished != null)
            {
                OnSpawnFinished();
            }
#endif
        }

        public void UpdateMinMaxHeight()
        {
            if (m_settings.m_isWorldmapSpawner)
            {
                SessionManager.GetWorldMinMax(ref m_minWorldHeight, ref m_maxWorldHeight, m_settings.m_isWorldmapSpawner, GetOrCreateBaseTerrainStamper(true));
            }
            else
            {
                SessionManager.GetWorldMinMax(ref m_minWorldHeight, ref m_maxWorldHeight, false);
            }


            float seaLevel = SessionManager.GetSeaLevel();
            //Iterate through all image masks and set up the current min max height
            //This is fairly important to display the height-dependent mask settings correctly
            //General spawner mask first
            foreach (ImageMask mask in m_settings.m_imageMasks)
            {
                mask.m_maxWorldHeight = m_maxWorldHeight;
                mask.m_minWorldHeight = m_minWorldHeight;
                mask.m_seaLevel = seaLevel;
                mask.CheckHeightMaskMigration();
            }

            ImageMask.CheckMaskStackForInvalidTextureRules("Spawner", this.name, m_settings.m_imageMasks);

            //Now the individual resource masks
            for (int i = 0; i < m_settings.m_spawnerRules.Count; i++)
            {
                ImageMask[] maskStack = m_settings.m_spawnerRules[i].m_imageMasks;
                if (maskStack != null && maskStack.Length > 0)
                {
                    foreach (ImageMask mask in maskStack)
                    {
                        mask.m_maxWorldHeight = m_maxWorldHeight;
                        mask.m_minWorldHeight = m_minWorldHeight;
                        mask.m_seaLevel = seaLevel;
                        mask.CheckHeightMaskMigration();
                    }
                    ImageMask.CheckMaskStackForInvalidTextureRules("Spawner", this.name + ", Spawn Rule: '" + m_settings.m_spawnerRules[i].m_name + "'", maskStack);
                }
            }
        }

        /// <summary>
        /// Store the last exported terrain size and heightmap resolution - used to determine if the user changed those on a world map spawner
        /// </summary>
        public void StoreWorldSize()
        {
            m_lastStampSpawnWorldSize = new Vector3(m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize, m_worldCreationSettings.m_tileHeight, m_worldCreationSettings.m_zTiles * m_worldCreationSettings.m_tileSize);

        }

        //public void StoreHeightmapResolution()
        //{
        //    m_lastExportedHeightMapResolution = worldCreationSettings.m_gaiaDefaults.m_heightmapResolution;
        //}

        /// <summary>
        /// Returns true if the user changed the world size / heightmap resolution since the last world map export
        /// </summary>
        /// <returns></returns>
        public bool HasWorldSizeChanged()
        {
            return m_lastStampSpawnWorldSize != new Vector3(m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize, m_worldCreationSettings.m_tileHeight, m_worldCreationSettings.m_zTiles * m_worldCreationSettings.m_tileSize);
        }

        //public bool HasHeightmapResolutionChanged()
        //{
        //   return  m_lastExportedHeightMapResolution != worldCreationSettings.m_gaiaDefaults.m_heightmapResolution;
        //}


        /// <summary>
        /// This is executed only in the editor - using it to simulate co-routine execution and update execution
        /// </summary>
        void EditorUpdate()
        {
#if UNITY_EDITOR
            if (m_updateCoroutine == null)
            {
                StopEditorUpdates();
                if (!SessionManager.m_worldCreationRunning && SessionManager.m_session.m_operations.Exists(x => x.sessionPlaybackState == SessionPlaybackState.Queued))
                {
                    GaiaSessionManager.ContinueSessionPlayback();
                }
                else
                {
                    //No session to continue -> destroy the temporary tools, if any
                    if (!SessionManager.m_worldCreationRunning)
                    {
                        GaiaSessionManager.AfterSessionPlaybackCleanup();
                    }
                }
                return;
            }
            else
            {
                if (EditorWindow.mouseOverWindow != null)
                {
                    m_updateTimeAllowed = 1 / 30f;
                }
                else
                {
                    m_updateTimeAllowed = 1 / 2f;
                }
                if (m_updateCoroutine2 != null)
                {
                    m_updateCoroutine2.MoveNext();
                }
                m_updateCoroutine.MoveNext();


            }
#endif
        }

        public void HighlightLoadingSettings()
        {
            m_highlightLoadingSettings = true;
            m_highlightLoadingSettingsStartedTimeStamp = GaiaUtils.GetUnixTimestamp();
        }

        /// <summary>
        /// Use this for initialization - this will kick the spawner off 
        /// </summary>
        void Start()
        {
            //Disable the colliders
            if (Application.isPlaying)
            {
                //Disable area bounds colliders
                Transform collTrans = this.transform.Find("Bounds_ColliderCache");
                if (collTrans != null)
                {
                    m_areaBoundsColliderCache = collTrans.gameObject;
                    m_areaBoundsColliderCache.SetActive(false);
                }

                if (!m_enableColliderCacheAtRuntime)
                {
                    collTrans = this.transform.Find("GameObject_ColliderCache");
                    if (collTrans != null)
                    {
                        m_goColliderCache = collTrans.gameObject;
                        m_goColliderCache.SetActive(false);
                    }
                }
            }

            if (m_mode == GaiaConstants.OperationMode.RuntimeInterval || m_mode == GaiaConstants.OperationMode.RuntimeTriggeredInterval)
            {
                //Initialise the spawner
                Initialise();

                //Start spawner checks in random period of time after game start, then every check interval
                InvokeRepeating("RunSpawnerIteration", 1f, m_spawnInterval);
            }
        }

        /// <summary>
        /// Build the spawner dictionary - allows for efficient updating of instances etc based on name
        /// </summary>
        public void Initialise()
        {
            if (m_showDebug)
            {
                Debug.Log("Initialising spawner");
            }

            //Set up layer for spawner collisions
            m_spawnColliderLayer = Gaia.TerrainHelper.GetActiveTerrainLayerAsInt();

            //Destroy any children
            List<Transform> transList = new List<Transform>();
            foreach (Transform child in transform)
            {
                transList.Add(child);
            }
            foreach (Transform child in transList)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            //Set up the spawner type flags
            SetUpSpawnerTypeFlags();

            //Create the game object parent transform
            if (IsGameObjectSpawner())
            {
                m_goParentGameObject = new GameObject("Spawned_GameObjects");
                m_goParentGameObject.transform.parent = this.transform;
                m_areaBoundsColliderCache = new GameObject("Bounds_ColliderCache");
                m_areaBoundsColliderCache.transform.parent = this.transform;
                m_goColliderCache = new GameObject("GameObject_ColliderCache");
                m_goColliderCache.transform.parent = this.transform;
            }

            //Reset the random number generator
            ResetRandomGenertor();

            //Get terrain height - assume all terrains same height
            Terrain t = TerrainHelper.GetTerrain(transform.position);
            if (t != null)
            {
                m_terrainHeight = t.terrainData.size.y;
            }

            //Set the spawner bounds
            m_spawnerBounds = new Bounds(transform.position, new Vector3(m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f));

            //Update the rule counters
            foreach (SpawnRule rule in m_settings.m_spawnerRules)
            {
                rule.m_currInstanceCnt = 0;
                rule.m_activeInstanceCnt = 0;
                rule.m_inactiveInstanceCnt = 0;
            }

            //Update the counters
            UpdateCounters();
        }

        /// <summary>
        /// Call this prior to doing a Spawn to do any setup required - particularly relevant for re-constituted spanwes
        /// </summary>
        private void PreSpawnInitialise()
        {
            //Update bounds
            m_spawnerBounds = new Bounds(transform.position, new Vector3(m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f));

            //Make sure random number generator is online
            if (m_rndGenerator == null)
            {
                ResetRandomGenertor();
            }
            //Debug.Log(string.Format("RNG {0} Seed = {1} State A = {2} State B = {3}", gameObject.name, m_rndGenerator.m_seed, m_rndGenerator.m_stateA, m_rndGenerator.m_stateB));

            //Set up layer for spawner collisions
            m_spawnColliderLayer = Gaia.TerrainHelper.GetActiveTerrainLayerAsInt();

            //Set up the spawner type flags
            SetUpSpawnerTypeFlags();

            //Create the game object parent transform
            if (IsGameObjectSpawner())
            {
                if (transform.Find("Spawned_GameObjects") == null)
                {
                    m_goParentGameObject = new GameObject("Spawned_GameObjects");
                    m_goParentGameObject.transform.parent = this.transform;
                }
                if (transform.Find("Bounds_ColliderCache") == null)
                {
                    m_areaBoundsColliderCache = new GameObject("Bounds_ColliderCache");
                    m_areaBoundsColliderCache.transform.parent = this.transform;
                }
                if (transform.Find("GameObject_ColliderCache") == null)
                {
                    m_goColliderCache = new GameObject("GameObject_ColliderCache");
                    m_goColliderCache.transform.parent = this.transform;
                }
            }

            //Initialise spawner themselves
            foreach (SpawnRule rule in m_settings.m_spawnerRules)
            {
                rule.Initialise(this);
            }

            //Create and initialise the noise generator
            if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.PerlinNoise)
            {
                m_noiseGenerator = new FractalGenerator(m_noiseMaskFrequency, m_noiseMaskLacunarity, m_noiseMaskOctaves, m_noiseMaskPersistence, m_noiseMaskSeed, FractalGenerator.Fractals.Perlin);
            }
            else if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.BillowNoise)
            {
                m_noiseGenerator = new FractalGenerator(m_noiseMaskFrequency, m_noiseMaskLacunarity, m_noiseMaskOctaves, m_noiseMaskPersistence, m_noiseMaskSeed, FractalGenerator.Fractals.Billow);
            }
            else if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.RidgedNoise)
            {
                m_noiseGenerator = new FractalGenerator(m_noiseMaskFrequency, m_noiseMaskLacunarity, m_noiseMaskOctaves, m_noiseMaskPersistence, m_noiseMaskSeed, FractalGenerator.Fractals.RidgeMulti);
            }

            //Update the counters
            UpdateCounters();
        }

        /// <summary>
        /// Caching spawner type flags
        /// </summary>
        public void SetUpSpawnerTypeFlags()
        {
            m_isDetailSpawner = false;
            for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                if (m_settings.m_spawnerRules[ruleIdx].m_resourceType == GaiaConstants.SpawnerResourceType.TerrainDetail)
                {
                    m_isDetailSpawner = true;
                    break;
                }
            }

            m_isTextureSpawner = false;
            for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                if (m_settings.m_spawnerRules[ruleIdx].m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTexture)
                {
                    m_isTextureSpawner = true;
                    break;
                }
            }

            for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                if (m_settings.m_spawnerRules[ruleIdx].m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTree)
                {
                    m_isTreeSpawnwer = true;
                    break;
                }
            }

            m_isGameObjectSpawner = false;
            for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                if (m_settings.m_spawnerRules[ruleIdx].m_resourceType == GaiaConstants.SpawnerResourceType.GameObject)
                {
                    m_isGameObjectSpawner = true;
                    break;
                }
            }
        }


        /// <summary>
        /// Make sure the assets are set up properly in the resources file
        /// </summary>
        public void AssociateAssets()
        {
            if (m_settings.m_resources != null)
            {
                m_settings.m_resources.AssociateAssets();
            }
            else
            {
                Debug.LogWarning("Could not associated assets for " + name + " - resources file was missing");
            }
        }

        /// <summary>
        /// Get the index of any rules that are missing resources
        /// </summary>
        /// <returns>Array of the resources that are missing</returns>
        /// <param name="terrains">The terrains to be checked. If left null, the active terrain will be checked only.</param>
        public List<TerrainMissingSpawnRules> GetMissingResources(List<TerrainMissingSpawnRules> missingRes, Terrain[] terrains = null)
        {
            if (terrains == null)
            {
                terrains = new Terrain[1] { Terrain.activeTerrain };
            }

            if (missingRes == null)
            {
                missingRes = new List<TerrainMissingSpawnRules>();
            }

            //Loop over all terrains
            for (int terrainID = 0; terrainID < terrains.Length; terrainID++)
            {
                //skip if no terrain
                if (terrains[terrainID] == null)
                {
                    continue;
                }

                //Initialise spawner themselves
                for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
                {
                    if (m_settings.m_spawnerRules[ruleIdx].m_isActive == true) //Only care about active resources
                    {
                        //check if there is actually a prototype
                        if (m_settings.m_spawnerRules[ruleIdx].ResourceIsNull(m_settings))
                        {
                            Debug.Log("Spawn Rule " + m_settings.m_spawnerRules[ruleIdx].m_name + " is active but has missing resources (Texture, Tree Prefab, GameObject etc. are empty) maintained. This rule might not work properly in spawning.");
                        }
                        else if (!m_settings.m_spawnerRules[ruleIdx].ResourceIsLoadedInTerrain(this, terrains[terrainID]))
                        {
                            TerrainMissingSpawnRules terrainSpawnRulesId = missingRes.Find(x => x.terrain == terrains[terrainID]);
                            if (terrainSpawnRulesId != null)
                            {
                                if (!terrainSpawnRulesId.spawnRulesWithMissingResources.Contains(m_settings.m_spawnerRules[ruleIdx]))
                                {
                                    terrainSpawnRulesId.spawnRulesWithMissingResources.Add(m_settings.m_spawnerRules[ruleIdx]);
                                }
                            }
                            else
                            {
                                missingRes.Add(new TerrainMissingSpawnRules { terrain = terrains[terrainID], spawnRulesWithMissingResources = new List<SpawnRule>() { m_settings.m_spawnerRules[ruleIdx] } });
                            }
                        }
                    }
                }
            }
            return missingRes;
        }

        /// <summary>
        /// Add the resources related to the rules passed in into the terrain if they are not already there
        /// </summary>
        /// <param name="rules">Index of rules with resources that should be added to the terrain</param>
        public void AddResourcesToTerrain(int[] rules, Terrain[] terrains = null)
        {
            for (int terrainId = 0; terrainId < terrains.Length; terrainId++)
            {
                for (int ruleIdx = 0; ruleIdx < rules.GetLength(0); ruleIdx++)
                {
                    if (!m_settings.m_spawnerRules[rules[ruleIdx]].ResourceIsLoadedInTerrain(this, terrains[terrainId]))
                    {
                        m_settings.m_spawnerRules[rules[ruleIdx]].AddResourceToTerrain(this, new Terrain[1] { terrains[terrainId] });
                    }
                }
            }
        }

        /// <summary>
        /// Call this at the end of a spawn
        /// </summary>
        private void PostSpawn()
        {
            //Signal that everything has stopped
            m_spawnProgress = 0f;
            m_spawnComplete = true;
            m_updateCoroutine = null;

            //Update the counters
            UpdateCounters();
        }

        /// <summary>
        /// Return true if this spawner spawns textures
        /// </summary>
        /// <returns>True if we spawn textures</returns>
        public bool IsTextureSpawner()
        {
            return m_isTextureSpawner;
        }

        /// <summary>
        /// Return true if this spawner spawns details
        /// </summary>
        /// <returns>True if we spawn details</returns>
        public bool IsDetailSpawner()
        {
            return m_isDetailSpawner;
        }

        /// <summary>
        /// Return true if this spawner spawns trees
        /// </summary>
        /// <returns>True if we spawn trees</returns>
        public bool IsTreeSpawner()
        {
            return m_isTreeSpawnwer;
        }

        /// <summary>
        /// Return true if this spawner spawns game objects
        /// </summary>
        /// <returns>True if we spawn game objects</returns>
        public bool IsGameObjectSpawner()
        {
            return m_isGameObjectSpawner;
        }

        /// <summary>
        /// Reste the spawner and delete everything it points to
        /// </summary>
        public void ResetSpawner()
        {
            Initialise();
        }

        public void UpdateAutoLoadRange()
        {
            //world map spawner should not load terrains
            if (m_settings.m_isWorldmapSpawner)
            {
                return;
            }

#if GAIA_PRO_PRESENT
            if (m_loadTerrainMode != LoadMode.Disabled)
            {
                float width = m_settings.m_spawnRange * 2f;
                //reduce the loading width a bit => this is to prevent loading in terrains when the spawner bounds end exactly at the border of
                //surrounding terrains, this loads in a lot of extra terrains which are not required for the spawn 
                width -= 0.5f;
                Vector3 center = transform.position;
                TerrainLoader.m_loadingBoundsRegular.center = center;
                TerrainLoader.m_loadingBoundsRegular.size = new Vector3(width, width, width);
                TerrainLoader.m_loadingBoundsImpostor.center = center;
                if (m_impostorLoadingRange > 0)
                {
                    TerrainLoader.m_loadingBoundsImpostor.size = new Vector3(width + m_impostorLoadingRange, width + m_impostorLoadingRange, width + m_impostorLoadingRange);
                }
                else
                {
                    TerrainLoader.m_loadingBoundsImpostor.size = Vector3.zero;
                }
            }
            TerrainLoader.LoadMode = m_loadTerrainMode;
#endif
        }

        /// <summary>
        /// Cause any active spawn to cancel itself
        /// </summary>
        public void CancelSpawn()
        {
            m_cancelSpawn = true;
            m_spawnComplete = true;
            m_spawnProgress = 0f;
            ProgressBar.Clear(ProgressBarPriority.Spawning);
        }

        /// <summary>
        /// Returns true if we are currently in process of spawning
        /// </summary>
        /// <returns>True if spawning, false otherwise</returns>
        public bool IsSpawning()
        {
            return (m_spawnComplete != true);
        }

        /// <summary>
        /// Check to see if this spawner can spawn instances
        /// </summary>
        /// <returns>True if it can spawn instances, false otherwise</returns>
        private bool CanSpawnInstances()
        {
            SpawnRule rule;
            bool canSpawnInstances = false;
            for (int ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            {
                rule = m_settings.m_spawnerRules[ruleIdx];
                if (rule.m_isActive)
                {
                    if (rule.m_ignoreMaxInstances)
                    {
                        return true;
                    }

                    if (rule.m_activeInstanceCnt < rule.m_maxInstances)
                    {
                        return true;
                    }
                }
            }
            return canSpawnInstances;
        }


        public void DrawSpawnerPreview()
        {
            if (m_drawPreview)
            {
                GaiaStopwatch.StartEvent("Drawing Spawner Preview");
                //early out if no preview rule is active 
                bool foundActive = false;
                for (int i = 0; i < m_previewRuleIds.Count; i++)
                {
                    if (m_previewRuleIds[i] < m_settings.m_spawnerRules.Count)
                    {
                        if (m_settings.m_spawnerRules[m_previewRuleIds[i]].m_isActive)
                        {
                            foundActive = true;
                        }
                    }
                }
                if (!foundActive)
                {
                    return;
                }
                //Set up a multi-terrain operation once, all rules can then draw from the data collected here
                Terrain currentTerrain = GetCurrentTerrain();
                if (currentTerrain == null)
                {
                    return;
                }

                GaiaMultiTerrainOperation operation = new GaiaMultiTerrainOperation(currentTerrain, transform, m_settings.m_spawnRange * 2f);
                operation.m_isWorldMapOperation = m_settings.m_isWorldmapSpawner;
                operation.GetHeightmap();

                //only re-generate all textures etc. if settings have changed and the preview is dirty, otherwise we can just use the cached textures
                if (m_spawnPreviewDirty == true)
                {
                    //To get a combined preview of different textures on a single mesh we need one color texture each per previewed 
                    // rule to determine the color areas on the heightmap mesh
                    // We need to iterate over the rules that are previewed, and build those color textures in this process

                    //Get additional op data (required for certain image masks)
                    operation.GetNormalmap();
                    operation.CollectTerrainBakedMasks();
                    //Preparing a simple add operation on the image mask shader for the combined heightmap texture
                    //Material filterMat = new Material(Shader.Find("Hidden/Gaia/FilterImageMask"));
                    //filterMat.SetFloat("_Strength", 1f);
                    //filterMat.SetInt("_Invert", 0);
                    //Store the currently active render texture here before we start manipulating
                    RenderTexture currentRT = RenderTexture.active;

                    //Clear texture cache first
                    ClearColorTextureCache();

                    //bool firstActiveRule = true;

                    for (int i = 0; i < m_previewRuleIds.Count; i++)
                    {

                        if (m_settings.m_spawnerRules[m_previewRuleIds[i]].m_isActive)
                        {
                            //Initialise our color texture cache at this index with this context
                            InitialiseColorTextureCache(i, operation.RTheightmap);
                            //Store result for this rule in our cache render texture array
                            Graphics.Blit(ApplyBrush(operation, MultiTerrainOperationType.Heightmap, m_previewRuleIds[i]), m_cachedPreviewColorRenderTextures[i]);
                            RenderTexture.active = currentRT;
                        }
                    }

                    //Everything processed, check if the preview is not dirty anymore
                    m_spawnPreviewDirty = false;
                }
                //Now draw the preview according to the cached textures
                Material material = GaiaMultiTerrainOperation.GetDefaultGaiaSpawnerPreviewMaterial();
                material.SetInt("_zTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);

                //assign all color textures in the material
                for (int i = 0; i < m_cachedPreviewColorRenderTextures.Length; i++)
                {
                    material.SetTexture("_colorTexture" + i, m_cachedPreviewColorRenderTextures[i]);
                }

                //iterate through spawn rules, and if it is a previewed texture set its color accordingly in the color slot
                int colorIndex = 0;
                for (int i = 0; i < m_settings.m_spawnerRules.Count; i++)
                {
                    if (m_previewRuleIds.Contains(i))
                    {
                        material.SetColor("_previewColor" + colorIndex.ToString(), m_settings.m_spawnerRules[m_previewRuleIds[colorIndex]].m_visualisationColor);
                        colorIndex++;
                    }
                }

                for (; colorIndex < GaiaConstants.maxPreviewedTextures; colorIndex++)
                {
                    Color transparentColor = Color.white;
                    transparentColor.a = 0f;
                    material.SetColor("_previewColor" + colorIndex.ToString(), transparentColor);
                }


                Color seaLevelColor = GaiaSettings.m_stamperSeaLevelTintColor;
                if (!m_showSeaLevelinStampPreview)
                {
                    seaLevelColor.a = 0f;
                }
                material.SetColor("_seaLevelTintColor", seaLevelColor);
                material.SetFloat("_seaLevel", SessionManager.GetSeaLevel());
                operation.Visualize(MultiTerrainOperationType.Heightmap, operation.RTheightmap, material, 1);

                //Clean up
                operation.CloseOperation();
                //Clean up temp textures
                GaiaUtils.ReleaseAllTempRenderTextures();
                GaiaStopwatch.EndEvent("Drawing Spawner Preview");
                GaiaStopwatch.Stop();
            }
        }

        public void FocusSceneViewOnWorldDesignerPreview()
        {
#if UNITY_EDITOR
            float range = TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f;
            Vector3 position = transform.position + new Vector3(0f, range, -range * 1.4f);
            GaiaUtils.SetSceneViewCam(position, transform.position);
            float worldSize = m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize;
            SceneView.lastActiveSceneView.cameraSettings.farClip = worldSize * 2f;
#endif
        }

        private void ClearCachedTexture(RenderTexture cachedRT)
        {
            if (cachedRT != null)
            {
                cachedRT.Release();
                DestroyImmediate(cachedRT);
            }

            cachedRT = new RenderTexture(1, 1, 1);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = cachedRT;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = currentRT;

        }

        private void ClearColorTextureCache()
        {
            for (int i = 0; i < m_cachedPreviewColorRenderTextures.Length; i++)
            {
                ClearCachedTexture(m_cachedPreviewColorRenderTextures[i]);
            }
        }

        /// <summary>
        /// Inizialises or "resets" a color texture in the cache
        /// </summary>
        /// <param name="index">The index at which to initialise.</param>
        /// <param name="rtToInitialiseFrom">A sample render texture with the correct resolution & format settings etc. to initialise from</param>
        private void InitialiseColorTextureCache(int index, RenderTexture rtToInitialiseFrom)
        {
            ClearCachedTexture(m_cachedPreviewColorRenderTextures[index]);
            m_cachedPreviewColorRenderTextures[index] = new RenderTexture(rtToInitialiseFrom);
        }

        private RenderTexture ApplyBrush(GaiaMultiTerrainOperation operation, MultiTerrainOperationType opType, int spawnRuleID = 0)
        {
            Terrain currentTerrain = GetCurrentTerrain();

            RenderTextureDescriptor rtDescriptor;

            switch (opType)
            {
                case MultiTerrainOperationType.Heightmap:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
                case MultiTerrainOperationType.Texture:
                    rtDescriptor = operation.RTtextureSplatmap.descriptor;
                    break;
                case MultiTerrainOperationType.TerrainDetail:
                    rtDescriptor = operation.RTdetailmap.descriptor;
                    break;
                case MultiTerrainOperationType.Tree:
                    rtDescriptor = operation.RTterrainTree.descriptor;
                    break;
                case MultiTerrainOperationType.GameObject:
                    rtDescriptor = operation.RTgameObject.descriptor;
                    break;
                default:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
            }
            //Random write needs to be enabled for certain mask types to function!
            rtDescriptor.enableRandomWrite = true;
            RenderTexture inputTexture1 = RenderTexture.GetTemporary(rtDescriptor);
            RenderTexture inputTexture2 = RenderTexture.GetTemporary(rtDescriptor);
            RenderTexture inputTexture3 = RenderTexture.GetTemporary(rtDescriptor);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = inputTexture1;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = inputTexture2;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = inputTexture3;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = currentRT;

            //fetch the biome mask stack (if any)
            BiomeController biomeController = Resources.FindObjectsOfTypeAll<BiomeController>().FirstOrDefault(x => x.m_autoSpawners.Find(y => y.spawner == this) != null);
            ImageMask[] biomeControllerStack = new ImageMask[0];
            if (biomeController != null && biomeController.m_settings.m_imageMasks.Length > 0)
            {
                biomeControllerStack = biomeController.m_settings.m_imageMasks;
                biomeControllerStack[0].m_blendMode = ImageMaskBlendMode.Multiply;
                //Iterate through all image masks and set up the current paint context in case the shader uses heightmap data
                foreach (ImageMask mask in biomeControllerStack)
                {
                    mask.m_multiTerrainOperation = operation;
                    mask.m_seaLevel = SessionManager.GetSeaLevel();
                    mask.m_maxWorldHeight = m_maxWorldHeight;
                    mask.m_minWorldHeight = m_minWorldHeight;
                }
                ImageMask.CheckMaskStackForInvalidTextureRules("Biome Controller", biomeController.name, biomeControllerStack);

            }

            ImageMask[] spawnerStack = new ImageMask[0];
            //set up the spawner mask stack, only if it has masks or a biome controller exists with masks
            if (m_settings.m_imageMasks.Length > 0)
            {

                spawnerStack = m_settings.m_imageMasks;
                //We start from a white texture, so we need the first mask action in the stack to always be "Multiply", otherwise there will be no result.
                spawnerStack[0].m_blendMode = ImageMaskBlendMode.Multiply;

                //Iterate through all image masks and set up the current paint context in case the shader uses heightmap data
                foreach (ImageMask mask in spawnerStack)
                {
                    mask.m_multiTerrainOperation = operation;
                    mask.m_seaLevel = SessionManager.GetSeaLevel();
                    mask.m_maxWorldHeight = m_maxWorldHeight;
                    mask.m_minWorldHeight = m_minWorldHeight;
                }

                ImageMask.CheckMaskStackForInvalidTextureRules("Spawner", this.name, spawnerStack);

            }

            //set up the resource mask stack
            ImageMask[] maskStack = m_settings.m_spawnerRules[spawnRuleID].m_imageMasks;
            if (maskStack.Length > 0)
            {
                //We start from a white texture, so we need the first mask action in the stack to always be "Multiply", otherwise there will be no result.
                maskStack[0].m_blendMode = ImageMaskBlendMode.Multiply;

                //Iterate through all image masks and set up the current paint context in case the shader uses heightmap data
                foreach (ImageMask mask in maskStack)
                {
                    mask.m_multiTerrainOperation = operation;
                    mask.m_seaLevel = SessionManager.GetSeaLevel();
                    mask.m_maxWorldHeight = m_maxWorldHeight;
                    mask.m_minWorldHeight = m_minWorldHeight;
                }
                ImageMask.CheckMaskStackForInvalidTextureRules("Spawner", this.name + ", Spawn Rule: '" + m_settings.m_spawnerRules[spawnRuleID].m_name + "'", maskStack);
            }


            //Get the combined masks from the biomeController
            RenderTexture biomeOutputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(ImageProcessing.ApplyMaskStack(inputTexture1, biomeOutputTexture, biomeControllerStack, ImageMaskInfluence.Local), biomeOutputTexture);

            //Get the combined masks from the spawner
            RenderTexture spawnerOutputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(ImageProcessing.ApplyMaskStack(inputTexture2, spawnerOutputTexture, spawnerStack, ImageMaskInfluence.Local), spawnerOutputTexture);

            //Check if we have the special global output mask in the spawn rule - this mask routes the output from the spawner mask stack directly into the rule stack 
            //(instead of utilizing the multiply below)
            bool globalOutputMaskFound = false;
            foreach (ImageMask mask in maskStack)
            {
                if (mask.m_operation == ImageMaskOperation.GlobalSpawnerMaskStack && mask.m_active)
                {
                    globalOutputMaskFound = true;
                    mask.m_globalSpawnerMaskStackRT = spawnerOutputTexture;
                }
            }

            //Get the combined masks from the rule
            RenderTexture ruleOutputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(ImageProcessing.ApplyMaskStack(inputTexture3, ruleOutputTexture, maskStack, ImageMaskInfluence.Local), ruleOutputTexture);

            //Run them through the image mask shader for a simple multiply
            Material filterMat = new Material(Shader.Find("Hidden/Gaia/FilterImageMask"));
            ImageProcessing.BakeCurveTexture(m_strengthTransformCurve, StrengthTransformCurveTexture);
            filterMat.SetTexture("_HeightTransformTex", StrengthTransformCurveTexture);


            RenderTexture combinedOutputTexture1 = RenderTexture.GetTemporary(rtDescriptor);

            //Only process the spawner stack if we are not useing the special global output mask
            if (!globalOutputMaskFound)
            {
                filterMat.SetTexture("_InputTex", biomeOutputTexture);
                filterMat.SetTexture("_ImageMaskTex", spawnerOutputTexture);
                Graphics.Blit(inputTexture1, combinedOutputTexture1, filterMat, 0);
            }
            else
            {
                //here we only blit the biome mask directly into the combined texture,
                //ignoring the spawner stack
                Graphics.Blit(biomeOutputTexture, combinedOutputTexture1);
            }
            filterMat.SetTexture("_InputTex", combinedOutputTexture1);
            filterMat.SetTexture("_ImageMaskTex", ruleOutputTexture);

            RenderTexture finalOutputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(inputTexture1, finalOutputTexture, filterMat, 0);

            //clean up temporary textures
            ReleaseRenderTexture(inputTexture1);
            inputTexture1 = null;
            ReleaseRenderTexture(inputTexture2);
            inputTexture2 = null;
            ReleaseRenderTexture(inputTexture3);
            inputTexture3 = null;
            ReleaseRenderTexture(biomeOutputTexture);
            biomeOutputTexture = null;
            ReleaseRenderTexture(spawnerOutputTexture);
            spawnerOutputTexture = null;
            ReleaseRenderTexture(ruleOutputTexture);
            ruleOutputTexture = null;
            ReleaseRenderTexture(combinedOutputTexture1);
            combinedOutputTexture1 = null;

            //Release the texture references from the biome controller, if any
            if (biomeController != null)
            {
                biomeController.m_settings.ClearImageMaskTextures(false);
            }

            return finalOutputTexture;
        }



        public Terrain GetCurrentTerrain()
        {
            Terrain currentTerrain = Gaia.TerrainHelper.GetTerrain(transform.position, m_settings.m_isWorldmapSpawner);
            //Check if the stamper is over a terrain currently

            //if not, we check if there is any terrain within the bounds of the spawner
            if (currentTerrain == null)
            {
                float width = m_settings.m_spawnRange * 2f;
                Bounds spawnerBounds = new Bounds(transform.position, new Vector3(width, width, width));

                foreach (Terrain t in Terrain.activeTerrains)
                {
                    //only look at this terrain if it matches the selected world map mode
                    if (m_settings.m_isWorldmapSpawner == TerrainHelper.IsWorldMapTerrain(t))
                    {
                        Bounds worldSpaceBounds = t.terrainData.bounds;
                        worldSpaceBounds.center = new Vector3(worldSpaceBounds.center.x + t.transform.position.x, worldSpaceBounds.center.y + t.transform.position.y, worldSpaceBounds.center.z + t.transform.position.z);

                        if (worldSpaceBounds.Intersects(spawnerBounds))
                        {
                            currentTerrain = t;
                            break;
                        }
                    }
                }
            }

            //if we still not have any terrain, we will draw a preview based on the last active terrain
            //if that is null either we can't draw a stamp preview
            if (currentTerrain)
            {
                m_lastActiveTerrainSize = currentTerrain.terrainData.size.x;
                //Update last active terrain with current
            }

            return currentTerrain;
        }

        private void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
                //texture = null;
            }
        }


        //public ImageMask[] GetSpawnRuleImageMasksByIndex(int spawnRuleIndex)
        //{
        //    //Get the right mask list from the resources according to the resource type that is used
        //    switch (m_spawnerRules[spawnRuleIndex].m_resourceType)
        //    {
        //        case GaiaConstants.SpawnerResourceType.TerrainTexture:
        //            return m_resources.m_texturePrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks;
        //        case GaiaConstants.SpawnerResourceType.TerrainDetail:
        //            return m_resources.m_detailPrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks;
        //        case GaiaConstants.SpawnerResourceType.TerrainTree:
        //            return m_resources.m_treePrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks;
        //        case GaiaConstants.SpawnerResourceType.GameObject:
        //            return m_resources.m_gameObjectPrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks;
        //    }
        //    return null;
        //}


        //public CollisionMask[] GetSpawnRuleCollisionMasksByIndices(int spawnRuleIndex, int maskIndex)
        //{
        //    //Get the right collision mask list from the resources according to the resource type that is used
        //    return GetSpawnRuleImageMasksByIndex(spawnRuleIndex)[maskIndex].m_collisionMasks;
        //}

        //public void SetSpawnRuleImageMasksByIndex(int spawnRuleIndex, ImageMask[] imageMasks)
        //{
        //    //Get the right mask list from the resources according to the resource type that is used
        //    switch (m_spawnerRules[spawnRuleIndex].m_resourceType)
        //    {
        //        case GaiaConstants.SpawnerResourceType.TerrainTexture:
        //            m_resources.m_texturePrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks = imageMasks;
        //            break;
        //        case GaiaConstants.SpawnerResourceType.TerrainDetail:
        //            m_resources.m_detailPrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks = imageMasks;
        //            break;
        //        case GaiaConstants.SpawnerResourceType.TerrainTree:
        //            m_resources.m_treePrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks = imageMasks;
        //            break;
        //        case GaiaConstants.SpawnerResourceType.GameObject:
        //            m_resources.m_gameObjectPrototypes[m_spawnerRules[spawnRuleIndex].m_resourceIdx].m_imageMasks = imageMasks;
        //            break;
        //    }
        //}

        //public void SetSpawnRuleCollisionMasksByIndices(int spawnRuleIndex, int maskIndex, CollisionMask[] collisionMasks)
        //{
        //    m_spawnerRules[spawnRuleIndex].m_imageMasks[maskIndex].m_collisionMasks = collisionMasks;
        //}

        //public Color GetVisualisationColorBySpawnRuleIndex(int spawnRuleIndex)
        //{
        //    switch (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceType)
        //    {
        //        case GaiaConstants.SpawnerResourceType.TerrainTexture:
        //            ResourceProtoTexture protoTexture = (ResourceProtoTexture)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoTexture != null)
        //                return protoTexture.m_visualisationColor;
        //            else
        //                return Color.red;
        //        case GaiaConstants.SpawnerResourceType.TerrainTree:
        //            ResourceProtoTree protoTree = (ResourceProtoTree)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoTree != null)
        //                return protoTree.m_visualisationColor;
        //            else
        //                return Color.red;
        //        case GaiaConstants.SpawnerResourceType.TerrainDetail:
        //            ResourceProtoDetail protoDetail = (ResourceProtoDetail)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoDetail != null)
        //                return protoDetail.m_visualisationColor;
        //            else
        //                return Color.red;
        //        case GaiaConstants.SpawnerResourceType.GameObject:
        //            ResourceProtoGameObject protoGameObject = (ResourceProtoGameObject)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoGameObject != null)
        //                return protoGameObject.m_visualisationColor;
        //            else
        //                return Color.red;
        //    }
        //    return Color.red;
        //}

        //public void SetVisualisationColorBySpawnRuleIndex(Color color, int spawnRuleIndex)
        //{
        //    switch (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceType)
        //    {
        //        case GaiaConstants.SpawnerResourceType.TerrainTexture:
        //            ResourceProtoTexture protoTexture = (ResourceProtoTexture)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoTexture != null)
        //            {
        //                protoTexture.m_visualisationColor = color;
        //            }
        //            break;
        //        case GaiaConstants.SpawnerResourceType.TerrainTree:
        //            ResourceProtoTree protoTree = (ResourceProtoTree)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoTree != null)
        //            {
        //                protoTree.m_visualisationColor = color;
        //            }
        //            break;
        //        case GaiaConstants.SpawnerResourceType.TerrainDetail:
        //            ResourceProtoDetail protoDetail = (ResourceProtoDetail)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoDetail != null)
        //            {
        //                protoDetail.m_visualisationColor = color;
        //            }
        //            break;
        //        case GaiaConstants.SpawnerResourceType.GameObject:
        //            ResourceProtoGameObject protoGameObject = (ResourceProtoGameObject)GetResourceProtoBySpawnRuleIndex(spawnRuleIndex);
        //            if (protoGameObject != null)
        //            {
        //                protoGameObject.m_visualisationColor = color;
        //            }
        //            break;
        //    }

        //}

        public object GetResourceProtoBySpawnRuleIndex(int spawnRuleIndex)
        {
            switch (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    if (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx < m_settings.m_resources.m_texturePrototypes.Length)
                        return m_settings.m_resources.m_texturePrototypes[m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx];
                    else
                        return null;
                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    if (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx < m_settings.m_resources.m_treePrototypes.Length)
                        return m_settings.m_resources.m_treePrototypes[m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx];
                    else
                        return null;
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    if (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx < m_settings.m_resources.m_detailPrototypes.Length)
                        return m_settings.m_resources.m_detailPrototypes[m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx];
                    else
                        return null;
                case GaiaConstants.SpawnerResourceType.GameObject:
                    if (m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx < m_settings.m_resources.m_gameObjectPrototypes.Length)
                        return m_settings.m_resources.m_gameObjectPrototypes[m_settings.m_spawnerRules[spawnRuleIndex].m_resourceIdx];
                    else
                        return null;
            }
            return null;
        }

        /// <summary>
        /// Toggle the preview mesh on and off
        /// </summary>
        public void TogglePreview()
        {
            m_drawPreview = !m_drawPreview;
            DrawSpawnerPreview();
        }

        /// <summary>
        /// Gets the max spawn range for this spawner.
        /// </summary>
        /// <returns></returns>
        public float GetMaxSpawnRange()
        {
            return GaiaUtils.GetMaxSpawnRange(GetCurrentTerrain());
        }



        /// <summary>
        /// Executes a List of spawners across an area in world space. The spawners are executed in steps defined by the world spawn range in the gaia settings.
        /// </summary>
        /// <param name="spawners">List of spawners to spawn across the area</param>
        /// <param name="area">The area in world space to spawn across</param>
        /// <param name="validTerrainNames">The terrain names that are valid to spawn on. If null, all terrains within operation range are assumed valid.</param>
        public IEnumerator AreaSpawn(List<Spawner> spawners, BoundsDouble area, List<string> validTerrainNames = null)
        {
            GaiaStopwatch.StartEvent("Area Spawn");
            m_cancelSpawn = false;
            GaiaSettings gaiaSettings = GaiaUtils.GetGaiaSettings();
            ClearPrototypeLists();
            ImageMask.RefreshSpawnRuleGUIDs();

            //remember the original world origin and loading range
            double originalLoadingRange = TerrainLoaderManager.Instance.GetLoadingRange();
            double originalLoadingRangeImpostor = TerrainLoaderManager.Instance.GetImpostorLoadingRange();
            CenterSceneViewLoadingOn originalCenter = TerrainLoaderManager.Instance.CenterSceneViewLoadingOn;
            Vector3Double originalOrigin = TerrainLoaderManager.Instance.GetOrigin();

            Vector3 originalSpawnerPosition = this.transform.position;
            float originalspawnRange = m_settings.m_spawnRange;

            Vector3 startPosition;


            //Track if we spawn GameObject -> If yes, the scenes affected must be dirtied for saving
            bool spawnedGameObjects = false;

            //We need to check what the max possible range is - there is a world spawn range setting in the Gaia settings, but if high resolution settings are chosen on
            //smaller terrains, it can be that we need to limit the range further to not exceed the size limit of render textures while spawning.
            float maxPossibleRange = Mathf.Min(gaiaSettings.m_spawnerWorldSpawnRange, GetMaxSpawnRange());
            float spawnRange = maxPossibleRange;
            //Does the area exceed the range of the world spawn range? if yes,
            //this means we need to spawn in multiple locations. Otherwise a single location spawn
            //should do the trick.
            //World map spawns are always local across the entire world map - since the world map is just a single terrain this works fine
            //even if the world map spans 100s of km
            float locationIncrement = maxPossibleRange * 2;

            if ((area.size.x > maxPossibleRange * 2f || area.size.z > maxPossibleRange * 2f) && !m_settings.m_isWorldmapSpawner)
            {
                //Multiple locations required
                int div = 2;
                float smallerExtent = (float)Mathd.Min(area.extents.x, area.extents.z);
                spawnRange = (smallerExtent / div);
                while (spawnRange > maxPossibleRange)
                {
                    div++;
                    spawnRange = (smallerExtent / div);
                    //spawnRange = (float)Mathd.Min(maxPossibleRange, );
                }
                locationIncrement = spawnRange * 2f;
                startPosition = new Vector3Double(area.min.x + spawnRange, 0, area.min.z + spawnRange);
            }
            else
            {
                //Single location spawn
                spawnRange = (float)Mathd.Max(area.extents.x, area.extents.z);
                startPosition = area.center;
                //Override the location increment to be so large we only go through the terrain iteration loops once.
                locationIncrement = float.MaxValue;
            }
            //Calculate the maximum amount of rules that need to be spawned in all iterations across the area for the progress bar
            int activeSpawnRules = 0;

            //Get all active rules
            foreach (Spawner spawner in spawners.Where(x => x != null && x.isActiveAndEnabled))
            {
                //Clear all prototype lists while we are at it
                spawner.ClearPrototypeLists();

                foreach (SpawnRule rule in spawner.settings.m_spawnerRules.Where(x => x.m_resourceType != SpawnerResourceType.WorldBiomeMask))
                {
                    if (rule.m_isActive)
                    {
                        activeSpawnRules++;
                    }
                    if (rule.m_resourceType == SpawnerResourceType.GameObject || rule.m_resourceType == SpawnerResourceType.SpawnExtension)
                    {
                        spawnedGameObjects = true;
                    }
                    if (spawner.m_settings.m_spawnMode == SpawnMode.Replace)
                    {
                        rule.m_spawnedInstances = 0;
                    }
                }
            }
            //multiply with all locations we need to spawn in
            int totalSpawns = activeSpawnRules * Spawner.GetAreaSpawnSteps(area, spawnRange);
            int totalSpawnsCompleted = 0;
            //Iterating across the area - X Axis
            for (Vector3 currentSpawnCenter = startPosition; currentSpawnCenter.x <= (area.max.x - spawnRange); currentSpawnCenter += new Vector3(locationIncrement, 0f, 0f))
            {
                if (!m_cancelSpawn)
                {
                    m_cancelSpawn = SpawnProgressBar.UpdateProgressBar("Preparing next Location...", totalSpawns, totalSpawnsCompleted, 0, 0);
                }
                if (m_cancelSpawn)
                {
                    break;
                }
                //Iterating across the area - Z Axis
                for (currentSpawnCenter = new Vector3(currentSpawnCenter.x, currentSpawnCenter.y, startPosition.z); currentSpawnCenter.z <= (area.max.z - spawnRange); currentSpawnCenter += new Vector3(0f, 0f, locationIncrement))
                {

                    if (!m_cancelSpawn)
                    {
                        m_cancelSpawn = SpawnProgressBar.UpdateProgressBar("Preparing next Location...", totalSpawns, totalSpawnsCompleted, 0, 0);
                    }
                    if (m_cancelSpawn)
                    {
                        break;
                    }

                    //Clear collision mask cache, since it needs to be built up fresh in this location anyways. The cache is then shared between all spawners
                    BakedMaskCache collisionMaskCache = SessionManager.m_bakedMaskCache;
                    collisionMaskCache.ClearCacheForSpawn();

                    GaiaMultiTerrainOperation operation = null;
                    List<TerrainMissingSpawnRules> terrainsMissingSpawnRules = new List<TerrainMissingSpawnRules>();

                    bool autoConnectNeighborsDisabled = false;

                    Terrain currentTerrain = null;

                    float boundsSize = spawnRange * 2f - 0.001f;
                    Bounds spawnerBounds = new Bounds(currentSpawnCenter, new Vector3(boundsSize, boundsSize, boundsSize));

                    //Iterate through all spawners that are spawning in "Replace" mode and clear out the previously spawned items before the spawning starts
                    //We need to remove everything first, otherwise it would give imprecise results when using collision masks

                    //We need to load terrains in before being able to remove anything
                    if (GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        TerrainLoaderManager.Instance.SwitchToLocalMap();
                        TerrainLoaderManager.Instance.CenterSceneViewLoadingOn = CenterSceneViewLoadingOn.WorldOrigin;
                        TerrainLoaderManager.Instance.SetOrigin(currentSpawnCenter);
                        //Remove a tiny bit for the loading range - when the spawner directly aligns with terrain borders
                        //this will lead to a lot of terrains being loaded in unneccessarily, which takes loinger to process and
                        //creates issues during spawning.
                        TerrainLoaderManager.Instance.SetLoadingRange(spawnRange - 0.001f, spawnRange - 0.001f);
                        //if we compare against spawner bounds, that needs to be centered on world origin now because this is where we execute the spawn
                        spawnerBounds.center = Vector3.zero;
                    }
#if UNITY_EDITOR
                    foreach (Spawner spawner in spawners.FindAll(x => x.m_settings.m_spawnMode == SpawnMode.Replace))
                    {
                        foreach (SpawnRule sr in spawner.m_settings.m_spawnerRules.FindAll(x => x.m_resourceType != SpawnerResourceType.TerrainTexture))
                        {
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                string terrainDataGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t.terrainData));

                                Bounds terrainBoundsWorldSpace = new Bounds(t.transform.position + t.terrainData.size / 2f, t.terrainData.size);
                                if (!terrainBoundsWorldSpace.Intersects(spawnerBounds))
                                {
                                    continue;
                                }

                                switch (sr.m_resourceType)
                                {
                                    case SpawnerResourceType.TerrainTexture:
                                        //should not happen, see restriction above
                                        break;
                                    case SpawnerResourceType.TerrainDetail:
                                        //We only may remove / reset this terrain detail rule once per terrain - otherwise this would destroy earlier detail spawn results in world spawns!
                                        string detailAssetGUID = "";
                                        ResourceProtoDetail detailPrototype = spawner.m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx];
                                        if (detailPrototype.m_renderMode == DetailRenderMode.VertexLit)
                                        {
                                            detailAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(detailPrototype.m_detailProtoype));
                                        }
                                        if (detailPrototype.m_renderMode == DetailRenderMode.Grass)
                                        {
                                            if (detailPrototype.m_detailProtoype != null)
                                            {
                                                detailAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(detailPrototype.m_detailProtoype));
                                            }
                                            else
                                            {
                                                detailAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(detailPrototype.m_detailTexture));
                                            }
                                        }
                                        if (detailPrototype.m_renderMode == DetailRenderMode.GrassBillboard)
                                        {
                                            detailAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(detailPrototype.m_detailTexture));
                                        }
                                        if (m_clearedDetailProtos.Find(x => x.m_terrainDataGUID == terrainDataGUID && x.m_prototypeAssetGUID == detailAssetGUID) == null)
                                        {
                                            int terrainDetailIndex = spawner.m_settings.m_resources.PrototypeIdxInTerrain(sr.m_resourceType, sr.m_resourceIdx, t);
                                            if (terrainDetailIndex != -1)
                                            {
                                                t.terrainData.SetDetailLayer(0, 0, terrainDetailIndex, new int[t.terrainData.detailWidth, t.terrainData.detailHeight]);
                                            }
                                            m_clearedDetailProtos.Add(new TerrainPrototypeId() { m_terrainDataGUID = terrainDataGUID, m_prototypeAssetGUID = detailAssetGUID });
                                        }
                                        break;
                                    case SpawnerResourceType.TerrainTree:
                                        //We only may remove / reset this tree rule once per terrain - otherwise this would destroy earlier tree spawn results in world spawns!

                                        string treeAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spawner.m_settings.m_resources.m_treePrototypes[sr.m_resourceIdx].m_desktopPrefab));

                                        if (m_clearedTreeProtos.Find(x => x.m_terrainDataGUID == terrainDataGUID && x.m_prototypeAssetGUID == treeAssetGUID) == null)
                                        {
                                            int treePrototypeIndex = spawner.m_settings.m_resources.PrototypeIdxInTerrain(sr.m_resourceType, sr.m_resourceIdx, t);
                                            TreeInstance[] newTrees = t.terrainData.treeInstances.Where(x => x.prototypeIndex != treePrototypeIndex).ToArray();
                                            t.terrainData.SetTreeInstances(newTrees, true);
                                            m_clearedTreeProtos.Add(new TerrainPrototypeId() { m_terrainDataGUID = terrainDataGUID, m_prototypeAssetGUID = treeAssetGUID });
                                        }
                                        break;
                                    case SpawnerResourceType.GameObject:
                                        //Game Object resources are difficult to uniquely identify, since the same prefab could be used in different spawn rules in a different context
                                        //we therefore use the name of the spawn rule as an unique identifier, since the container object for the spawned results is created by the name of the spawn rule anyways
                                        string gameObjectAssetGUID = sr.m_name;
                                        if (m_clearedGameObjectProtos.Find(x => x.m_terrainDataGUID == terrainDataGUID && x.m_prototypeAssetGUID == gameObjectAssetGUID) == null)
                                        {
                                            ClearGameObjectsForRule(spawner, sr, false, t);
                                            m_clearedGameObjectProtos.Add(new TerrainPrototypeId() { m_terrainDataGUID = terrainDataGUID, m_prototypeAssetGUID = gameObjectAssetGUID });
                                        }
                                        break;
                                    case SpawnerResourceType.SpawnExtension:
                                        //Same principle as for game objects
                                        string spawnExtensionAssetGUID = sr.m_name;
                                        if (m_clearedSpawnExtensionProtos.Find(x => x.m_terrainDataGUID == terrainDataGUID && x.m_prototypeAssetGUID == spawnExtensionAssetGUID) == null)
                                        {
                                            ClearSpawnExtensionsForRule(sr, spawner.m_settings);
                                            m_clearedSpawnExtensionProtos.Add(new TerrainPrototypeId() { m_terrainDataGUID = terrainDataGUID, m_prototypeAssetGUID = spawnExtensionAssetGUID });
                                        }
                                        if (sr.m_isActive && sr.m_changesHeightmap)
                                        {
                                            CheckIfHeightmapRestoreRequired(t);
                                        }

                                        break;
                                    case SpawnerResourceType.Probe:
                                        //Same principle as for game objects
                                        string probeAssetGUID = sr.m_name;
                                        if (m_clearedProbeProtos.Find(x => x.m_terrainDataGUID == terrainDataGUID && x.m_prototypeAssetGUID == probeAssetGUID) == null)
                                        {
                                            //Deletion is handled by the Clear Game Objects function since the probes are essentially game objects
                                            ClearGameObjectsForRule(spawner, sr, false, t);
                                            m_clearedProbeProtos.Add(new TerrainPrototypeId() { m_terrainDataGUID = terrainDataGUID, m_prototypeAssetGUID = probeAssetGUID });
                                        }
                                        break;
                                    case SpawnerResourceType.TerrainModifierStamp:
                                        if (!sr.m_isActive)
                                        {
                                            continue;
                                        }
                                        //Terrain Modifier rules are a bit special - if there is no heightmap backup yet, we need to create it initially now for each terrain
                                        //if there is already a backup, we want to restore the backup each terrain once before spawning 
                                        CheckIfHeightmapRestoreRequired(t);
                                        break;
                                    case SpawnerResourceType.WorldBiomeMask:
                                        //not relevant
                                        break;
                                }
                            }
                        }
                    }
#endif
                    //Iterating through all the spawners in this location for the actual spawn
                    foreach (Spawner spawner in spawners)
                    {
                        spawner.UpdateMinMaxHeight();
                        //Depending on wether we are in a dynamic loading scenario or not, we need to either control the dynamic loading to load terrains in below the spawner
                        //or move the spawner across the world. The world map spawner should not need to load terrains - just spawns to the world map.
                        if (GaiaUtils.HasDynamicLoadedTerrains() && !m_settings.m_isWorldmapSpawner)
                        {
                            TerrainLoaderManager.Instance.SwitchToLocalMap();
                            TerrainLoaderManager.Instance.CenterSceneViewLoadingOn = CenterSceneViewLoadingOn.WorldOrigin;
                            TerrainLoaderManager.Instance.SetOrigin(currentSpawnCenter);
                            //Remove a tiny bit for the loading range - when the spawner directly aligns with terrain borders
                            //this will lead to a lot of terrains being loaded in unneccessarily, which takes loinger to process and
                            //creates issues during spawning.
                            TerrainLoaderManager.Instance.SetLoadingRange(spawnRange - 0.001f, spawnRange - 0.001f);
                            spawner.transform.position = Vector3.zero;
                            spawner.m_settings.m_spawnRange = spawnRange;
                        }
                        else
                        {
                            if (m_settings.m_isWorldmapSpawner)
                            {
                                TerrainLoaderManager.Instance.SwitchToWorldMap();
                            }

                            spawner.transform.position = currentSpawnCenter;
                            spawner.m_settings.m_spawnRange = spawnRange;
                        }


                        currentTerrain = spawner.GetCurrentTerrain();
                        if (currentTerrain != null)
                        {
                            //the neighbor system can create issues with spawning if there is a spawn executed 
                            //while there is a gap between terrains, this can lead to faulty pixels on the edge of the normal map.
                            if (currentTerrain.allowAutoConnect)
                            {
                                currentTerrain.allowAutoConnect = false;
                                currentTerrain.SetNeighbors(null, null, null, null);
                                currentTerrain.terrainData.DirtyHeightmapRegion(new RectInt(0, 0, currentTerrain.terrainData.heightmapResolution, currentTerrain.terrainData.heightmapResolution), TerrainHeightmapSyncControl.HeightOnly);
                                currentTerrain.terrainData.SyncHeightmap();
                                autoConnectNeighborsDisabled = true;
                                yield return null;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        try
                        {
                            //Check for missing resources in the currently loaded terrains.
                            //This information is passed into the operation which can then
                            //add the resources "on demand" while spawning.
                            if (!m_settings.m_isWorldmapSpawner)
                            {
                                terrainsMissingSpawnRules = spawner.GetMissingResources(terrainsMissingSpawnRules, Terrain.activeTerrains);
                            }


                            if (currentTerrain != null)
                            {
                                operation = new GaiaMultiTerrainOperation(currentTerrain, spawner.transform, spawnRange * 2f, true, validTerrainNames);
                                operation.m_isWorldMapOperation = m_settings.m_isWorldmapSpawner;
                                operation.m_terrainsMissingSpawnRules = terrainsMissingSpawnRules;
                                operation.GetHeightmap();
                                operation.GetNormalmap();
                                operation.CollectTerrainDetails();
                                operation.CollectTerrainTrees();
                                operation.CollectTerrainGameObjects();
                                operation.CollectTerrainBakedMasks();
                                spawner.ExecuteSpawn(operation, collisionMaskCache, totalSpawns, ref totalSpawnsCompleted);
                                if (spawnedGameObjects)
                                {
                                    foreach (Terrain t in operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject).Select(x => x.Key.terrain))
                                    {
#if UNITY_EDITOR
                                        EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
                                        //apply the hierarchy hide settings
                                        GaiaHierarchyUtils ghu = t.transform.GetComponentInChildren<GaiaHierarchyUtils>();
                                        if (ghu != null)
                                        {
                                            ghu.SetupHideInHierarchy();
                                        }
#endif
                                    }
                                }
                                //Clean up between spawners
                                operation.CloseOperation();
                            }
                            else
                            {
                                Debug.LogError("Trying to spawn, but could not find any terrain for spawning!");
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Error during spawning, Error Message: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                        }
                        finally
                        {
                            GaiaUtils.ReleaseAllTempRenderTextures();
                            spawner.m_spawnPreviewDirty = true;
                            spawner.SetWorldBiomeMasksDirty();
                            spawner.m_settings.ClearImageMaskTextures();
                        }
                        yield return null;
                    } //spawners

                    //if we disabled autoconnect during the spawn, we need to re-enable it
                    if (autoConnectNeighborsDisabled && currentTerrain != null)
                    {
                        currentTerrain.allowAutoConnect = true;
                    }

#if GAIA_PRO_PRESENT
                    //De-select the loaders only after all spawners ran in this location, otherwise terrains are being loaded / unloaded constantly on yield
                    foreach (Spawner spawner in spawners)
                    {
                        spawner.TerrainLoader.m_isSelected = false;
                    }
#endif
                    //yield return null;

                }// for Z
            } // for X

            SpawnProgressBar.ClearProgressBar();
            m_updateCoroutine = null;

            if (m_settings.m_isWorldmapSpawner)
            {
                if (m_baseTerrainSettings.m_drawPreview)
                {
                    UpdateBaseTerrainStamper();
                }
            }


#if UNITY_EDITOR && GAIA_PRO_PRESENT
            //if the currently selected object is a spawner we switch back on the "selected" flag
            if (Selection.activeObject != null)
            {
                //try catch needed for the GameObject cast
                try
                {

                    Spawner selectedSpawner = ((GameObject)Selection.activeObject).GetComponent<Spawner>();
                    if (selectedSpawner != null)
                    {
                        selectedSpawner.TerrainLoader.m_isSelected = true;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message == "123")
                    {
                        //Preventing compiler warning for unused "ex"
                    }
                }

            }

            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                TerrainLoaderManager.Instance.CenterSceneViewLoadingOn = originalCenter;
                TerrainLoaderManager.Instance.SetLoadingRange(originalLoadingRange, originalLoadingRangeImpostor);
                TerrainLoaderManager.Instance.SetOrigin(originalOrigin);
            }
#endif
            //Resetting this spawner to the original position and range
            transform.position = originalSpawnerPosition;
            m_settings.m_spawnRange = originalspawnRange;

            SimpleCameraLayerCulling.Refresh();
            GaiaStopwatch.EndEvent("Area Spawn");
            GaiaStopwatch.Stop();
            yield return null;

        }

        private void CheckIfHeightmapRestoreRequired(Terrain t)
        {
            if (SessionManager.DoesStamperBackupExist(t))
            {
                if (!m_restoredHeightmapTerrains.Contains(t))
                {
                    SessionManager.RestoreStamperBackup(t);
                    m_restoredHeightmapTerrains.Add(t);
                }
            }
            else
            {
                //no backup yet - need to create one before we modify the terrain
                SessionManager.UpdateStamperBackup(true, t);
                m_restoredHeightmapTerrains.Add(t);
            }
        }

        public void ClearPrototypeLists()
        {
            m_clearedGameObjectProtos.Clear();
            m_clearedSpawnExtensionProtos.Clear();
            m_clearedTreeProtos.Clear();
            m_clearedDetailProtos.Clear();
            m_clearedStampDistributionProtos.Clear();
            m_clearedProbeProtos.Clear();
            m_restoredHeightmapTerrains.Clear();
        }

        public GameObject CreateBaseTerrainStamper()
        {
            GameObject stamperGO = WorldMap.GetOrCreateWorldMapStamper();
            if (stamperGO != null)
            {
                m_baseTerrainStamper = stamperGO.GetComponent<Stamper>();
                m_baseTerrainStamper.FitToTerrain(TerrainHelper.GetWorldMapTerrain());
                m_baseTerrainStamper.SetBaseTerrainStandardMasks();
                UpdateBaseTerrainStamper();
            }
            return stamperGO;
        }

        public void CreateTerrainModifierPreviewStamper()
        {
            m_terrainModifierPreviewStamper = GaiaSessionManager.GetOrCreateSessionStamper();
            if (m_previewRuleIds.Count > 0 && m_settings.m_spawnerRules[m_previewRuleIds[0]].m_resourceType == SpawnerResourceType.TerrainModifierStamp)
            {
                UpdateTerrainModifierStamperFromSpawnRule(m_terrainModifierPreviewStamper, m_settings.m_spawnerRules[m_previewRuleIds[0]]);
            }
        }

        public void UpdateBaseTerrainStamper()
        {
            BaseTerrainSettings bts = m_baseTerrainSettings;

            Stamper baseTerrainStamper = GetOrCreateBaseTerrainStamper(true);

            baseTerrainStamper.transform.position = new Vector3(baseTerrainStamper.transform.position.x, bts.m_baseLevel, baseTerrainStamper.transform.position.z);
            //Setup the existing masks according to the parameters
            baseTerrainStamper.transform.localScale = new Vector3(baseTerrainStamper.transform.localScale.x, bts.m_heightScale, baseTerrainStamper.transform.localScale.z);
            ImageMask distanceMask = null;

            switch (bts.m_baseTerrainInputType)
            {
                case BaseTerrainInputType.Generator:

                    //Terrain shape noise mask
                    ApplyNoiseSettingsToImageMask(baseTerrainStamper.m_settings.m_imageMasks[0], bts.m_baseNoiseSettings[0], 1f);
                    //ApplyNoiseSettingsToImageMask(baseTerrainStamper.m_settings.m_imageMasks[1], bts.m_baseNoiseSettings[1],1f);
                    distanceMask = m_baseTerrainStamper.m_settings.m_imageMasks[1];
                    break;
                case BaseTerrainInputType.Image:
                    baseTerrainStamper.m_settings.m_imageMasks[0] = ImageMask.Clone(bts.m_inputImageMask);
                    baseTerrainStamper.m_settings.m_imageMasks[0].m_operation = ImageMaskOperation.ImageMask;
                    distanceMask = m_baseTerrainStamper.m_settings.m_imageMasks[1];
                    break;
                case BaseTerrainInputType.ExistingTerrain:
                    baseTerrainStamper.m_settings.m_imageMasks[0] = ImageMask.Clone(bts.m_inputImageMask);
                    baseTerrainStamper.m_settings.m_imageMasks[0].m_operation = ImageMaskOperation.ImageMask;
                    baseTerrainStamper.m_settings.m_imageMasks[0].ImageMaskTexture = GaiaUtils.ConvertRenderTextureToTexture2D(bts.m_inputTerrain.terrainData.heightmapTexture);
                    baseTerrainStamper.m_settings.m_inputTerrain = bts.m_inputTerrain;
                    distanceMask = m_baseTerrainStamper.m_settings.m_imageMasks[1];
                    break;
            }

            m_baseTerrainStamper.m_settings.m_baseTerrainInputType = bts.m_baseTerrainInputType;

            //Set up the distance mask according to border style

            if (bts.m_borderStyle == GeneratorBorderStyle.None)
            {
                distanceMask.m_active = false;
                distanceMask.m_strengthTransformCurve = ImageMask.NewAnimCurveStraightUpwards(1f);
            }
            else
            {
                distanceMask.m_active = true;
                distanceMask.m_distanceMaskAxes = ImageMaskDistanceMaskAxes.XZSquare;

                distanceMask.m_imageMaskSpace = ImageMaskSpace.World;
                if (bts.m_borderStyle == GeneratorBorderStyle.Water)
                {
                    distanceMask.m_distanceMaskSquareRoundness = 0.5f;
                    distanceMask.m_distanceMaskCurve = ImageMask.NewAnimCurveWaterBorder();
                    distanceMask.m_strengthTransformCurve = ImageMask.NewAnimCurveStraightUpwards(1f);
                }
                else
                {
                    distanceMask.m_distanceMaskSquareRoundness = 0.75f;
                    distanceMask.m_distanceMaskCurve = ImageMask.NewAnimCurveMountainBorderDistance();
                    distanceMask.m_strengthTransformCurve = ImageMask.NewAnimCurveMountainBorderStrength();
                }
            }

            //m_baseTerrainStamper.m_settings.m_imageMasks[0].m_noiseSettings.domainSettings.noiseTypeName = m_spawner.m_baseTerrainSettings.m_shapeNoiseStyle.ToString();
            SessionManager.DirtyWorldMapMinMax();
            baseTerrainStamper.m_stampDirty = true;
        }

        private void ApplyNoiseSettingsToImageMask(ImageMask imageMask, BaseTerrainNoiseSettings btns, float sizemultiplicator)
        {
#if UNITY_EDITOR
            imageMask.m_operation = ImageMaskOperation.NoiseMask;
            imageMask.m_gaiaNoiseSettings.m_noiseTypeName = btns.m_shapeNoiseStyle.ToString();
            imageMask.m_gaiaNoiseSettings.m_scale = new Vector3(10f - btns.m_shapeSize, 1f, 10f - btns.m_shapeSize);
            imageMask.m_gaiaNoiseSettings.m_translation = btns.m_shapeOffset;
            ImageMask.InitNoiseMask(imageMask);
            IFractalType fractalType = NoiseLib.GetFractalTypeInstance(imageMask.noiseSettingsGUI.fractalTypeName.stringValue);
            // deserialize string
            FbmFractalType.FbmFractalInput fbm = (FbmFractalType.FbmFractalInput)fractalType.FromSerializedString(imageMask.m_noiseSettings.domainSettings.fractalTypeParams);

            switch (btns.m_shapeNoiseStyle.ToString())
            {
                case "Billow":
                    fbm.amplitude = 1f;
                    fbm.persistence = 0.8f;
                    break;
                case "Perlin":
                    fbm.amplitude = 0.7f;
                    fbm.persistence = 0.5f;
                    break;
                case "Ridge":
                    fbm.amplitude = 0.5f;
                    fbm.persistence = 0.5f;
                    break;
                case "Value":
                    fbm.amplitude = 0.5f;
                    fbm.persistence = 0.5f;
                    break;
                case "Voronoi":
                    fbm.amplitude = 0.5f;
                    fbm.persistence = 0.5f;
                    break;
            }

            fbm.lacunarity = btns.m_shapeGranularity;
            fbm.amplitude = btns.m_shapeStrength;
            fbm.warpIterations = btns.m_warpIterations;
            fbm.warpStrength = btns.m_warpStrength;
            fbm.warpOffsets = btns.m_warpOffset;
            fbm.warpEnabled = fbm.warpIterations > 0.0f;

            imageMask.m_strengthTransformCurve = ImageMask.NewAnimCurveDefaultNoise((1f - btns.m_shapeSteepness) / 2f);
            //add another arbitary offset to avoid the ugly pattern at 0,0,0 for Perlin noise
            imageMask.m_noiseSettings.transformSettings.translation = btns.m_shapeOffset + new Vector3(500f, 200f, 350f);
            imageMask.m_noiseSettings.domainSettings.fractalTypeParams = fractalType.ToSerializedString(fbm);

            //adapt the scaling to the total world size so that the slider on the UI is equally useful on large and small worlds
            float scaleBaseValue = 20; //* sizemultiplicator; // Mathf.Sqrt(m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize);
            float sliderRange = scaleBaseValue * 1000 / (m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize);
            float adaptedSize = Mathf.Lerp(scaleBaseValue - sliderRange, scaleBaseValue, btns.m_shapeSize / 10.0f);

            imageMask.m_noiseSettings.transformSettings.scale = new Vector3(scaleBaseValue - adaptedSize, 1f, scaleBaseValue - adaptedSize);
            imageMask.m_noiseSettings.domainSettings.noiseTypeName = btns.m_shapeNoiseStyle.ToString();
            imageMask.noiseSettingsGUI.noiseTypeName.stringValue = btns.m_shapeNoiseStyle.ToString();
            //imageMask.m_strengthTransformCurve = strengthTransform;

            imageMask.m_gaiaNoiseSettings.m_noiseTypeName = btns.m_shapeNoiseStyle.ToString();
            imageMask.m_gaiaNoiseSettings.m_noiseTypeParams = imageMask.m_noiseSettings.domainSettings.noiseTypeParams;
            imageMask.m_gaiaNoiseSettings.m_fractalTypeName = imageMask.noiseSettingsGUI.fractalTypeName.stringValue;
            imageMask.m_gaiaNoiseSettings.m_fractalTypeParams = imageMask.m_noiseSettings.domainSettings.fractalTypeParams;

            imageMask.m_gaiaNoiseSettings.m_warpIterations = btns.m_warpIterations;
            imageMask.m_gaiaNoiseSettings.m_warpStrength = btns.m_warpStrength;
            imageMask.m_gaiaNoiseSettings.m_warpOffset = btns.m_warpOffset;
            imageMask.m_gaiaNoiseSettings.m_warpEnabled = btns.m_warpIterations > 0.0f;
#endif
        }

        /// <summary>
        /// Flags the world biome masks maintained in this spawner as dirty
        /// </summary>
        public void SetWorldBiomeMasksDirty()
        {
            if (m_settings.m_isWorldmapSpawner)
            {
                for (int i = 0; i < m_settings.m_spawnerRules.Count; i++)
                {
                    if (m_settings.m_spawnerRules[i].m_resourceType == SpawnerResourceType.WorldBiomeMask)
                    {
                        SessionManager.m_bakedMaskCache.SetWorldBiomeMaskDirty(m_settings.m_spawnerRules[i].GUID);
                    }
                }
            }
        }

        private void ExecuteSpawn(GaiaMultiTerrainOperation operation, BakedMaskCache collisionMaskCache, int totalSpawns, ref int totalSpawsCompleted, bool allowStatic = true)
        {
            GaiaStopwatch.StartEvent("Execute Spawn");
            int maxSpawnerRules = m_settings.m_spawnerRules.Where(x => x.m_isActive == true && x.m_resourceType != SpawnerResourceType.WorldBiomeMask).Count();
            int completedSpawnerRules = 0;

            //Create a new random generator that will use the seed entered in the spawner ui to generate one seed each per spawn rule.
            //We can't simply pass down the seed in the rules, otherwise those will produce the same / too similar results
            XorshiftPlus xorshiftPlus = new XorshiftPlus(m_settings.m_randomSeed);
            xorshiftPlus.NextInt();

            //pre generate the seeds for each rule, regardless if the rule is active or not - this allows to deactivate a single rule but still getting the same result from the seed.
            int[] randomSeeds = new int[m_settings.m_spawnerRules.Count];
            for (int i = 0; i < m_settings.m_spawnerRules.Count; i++)
            {
                randomSeeds[i] = xorshiftPlus.NextInt();
            }

            if (m_settings.m_isWorldmapSpawner)
            {
                //clear the stamp operation list first, we will later rebuild it by iterating through all spawned / remaining tokens
                ClearStampDistributions();
            }

            Terrain currentTerrain = GetCurrentTerrain();
            for (int i = 0; i < m_settings.m_spawnerRules.Count; i++)
            {
                //wrap in try-catch to close any progress bars on potential errors & possibly at least continue to spawn the other rules
                try
                {

                    if (!m_cancelSpawn)
                    {
                        m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);
                    }

                    if (m_cancelSpawn)
                    {
                        break;
                    }

                    if (m_settings.m_spawnerRules[i].m_isActive)
                    {
                        switch (m_settings.m_spawnerRules[i].m_resourceType)
                        {
                            case GaiaConstants.SpawnerResourceType.TerrainTexture:

                                ResourceProtoTexture proto = m_settings.m_resources.m_texturePrototypes[m_settings.m_spawnerRules[i].m_resourceIdx];
                                //Look for the layer file associated with the resource in any of the currently active terrains
                                TerrainLayer targetLayer = TerrainHelper.GetLayerFromPrototype(proto);

                                operation.GetSplatmap(targetLayer);

                                RenderTexture tempTextureRT = SimulateRule(operation, i);

                                //Add missing texture / terrain layer - but only if required according to the simulation!
                                foreach (TerrainMissingSpawnRules tmsr in operation.m_terrainsMissingSpawnRules)
                                {
                                    if (operation.affectedTerrainPixels.Where(x => x.Key.terrain == tmsr.terrain && x.Key.operationType == MultiTerrainOperationType.Texture && x.Value.simulationPositive == true).Count() > 0)
                                    {
                                        operation.HandleMissingResources(this, m_settings.m_spawnerRules[i], tmsr.terrain);
                                    }
                                }

                                //Look for the target layer again - it might have been added now
                                if (targetLayer == null)
                                {
                                    targetLayer = TerrainHelper.GetLayerFromPrototype(proto);
                                }

                                if (targetLayer != null)
                                {
                                    //need to call Get Splatmap again before calling SetSplatmap since texture masks inside there can jeopardize the spawn result.
                                    operation.GetSplatmap(targetLayer);
                                    operation.SetSplatmap(tempTextureRT, this, m_settings.m_spawnerRules[i], false);
                                }

                                break;
                            case GaiaConstants.SpawnerResourceType.TerrainDetail:
                                RenderTexture tempTerrainDetailRT = SimulateRule(operation, i);
                                int affectedDetailTerrainsCount = operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.TerrainDetail && x.Value.simulationPositive == true).Count();
                                if (m_settings.m_spawnMode == SpawnMode.Replace)
                                {
                                    SpawnMode originalMode = m_settings.m_spawnMode;

                                    m_settings.m_spawnMode = SpawnMode.Add;
                                    if (affectedDetailTerrainsCount > 0)
                                    {
                                        operation.SetTerrainDetails(tempTerrainDetailRT, m_settings, this, m_settings.m_spawnerRules[i], randomSeeds[i], false);
                                    }

                                    m_settings.m_spawnMode = originalMode;
                                }
                                else
                                {
                                    if (affectedDetailTerrainsCount > 0)
                                    {
                                        operation.SetTerrainDetails(tempTerrainDetailRT, m_settings, this, m_settings.m_spawnerRules[i], randomSeeds[i], false);
                                    }
                                }

                                break;
                            case GaiaConstants.SpawnerResourceType.TerrainTree:
                                RenderTexture tempTreeRT = SimulateRule(operation, i);
                                int affectedTreeTerrainsCount = operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.Tree && x.Value.simulationPositive == true).Count();

                                m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);
                                if (affectedTreeTerrainsCount > 0)
                                {
                                    //Remember the scaling settings that were last used in a spawn - required to re-scale tree instances when doing a prototype refresh.
                                    m_settings.m_resources.m_treePrototypes[m_settings.m_spawnerRules[i].m_resourceIdx].StoreLastUsedScaleSettings();
                                    operation.SetTerrainTrees(tempTreeRT, m_settings, this, m_settings.m_spawnerRules[i], randomSeeds[i], false, SessionManager.GetSeaLevel());
                                }

                                collisionMaskCache.SetTreeDirty(m_settings.m_spawnerRules[i].GUID, m_settings.m_resources.m_treePrototypes[m_settings.m_spawnerRules[i].m_resourceIdx].m_desktopPrefab.layer);

                                break;
                            case GaiaConstants.SpawnerResourceType.GameObject:
                                //int goPrototypeIndex = m_settings.m_resources.PrototypeIdxInTerrain(m_settings.m_spawnerRules[i].m_resourceType, m_settings.m_spawnerRules[i].m_resourceIdx);

                                //We only may remove / reset the Game Object spawner once per terrain- otherwise this would destroy earlier spawn results in world spawns!
                                //foreach (Terrain t in operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject).Select(x => x.Key.terrain))
                                //{
                                //    if (m_settings.spawnMode == SpawnMode.Replace && m_clearedGameObjectProtos.Find(x => x.terrain == t && x.prototypeId == m_settings.m_spawnerRules[i].m_resourceIdx) == null)
                                //    {
                                //        ClearGameObjectsForRule(m_settings.m_spawnerRules[i], false, t);
                                //        m_clearedGameObjectProtos.Add(new TerrainPrototypeId() { terrain = t, prototypeId = m_settings.m_spawnerRules[i].m_resourceIdx });
                                //    }
                                //}

                                ResourceProtoGameObject protoGO = m_settings.m_resources.m_gameObjectPrototypes[m_settings.m_spawnerRules[i].m_resourceIdx];

                                if (protoGO == null)
                                {
                                    Debug.LogWarning("Could not find Game Object Prototype for Spawn Rule " + m_settings.m_spawnerRules[i].m_name);
                                    break;
                                }
                                m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);

                                RenderTexture tempGameObjectRT = SimulateRule(operation, i);
                                int affectedGameObjectTerrainsCount = operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject && x.Value.simulationPositive == true).Count();
                                if (affectedGameObjectTerrainsCount > 0)
                                {
                                    operation.SetTerrainGameObjects(tempGameObjectRT, protoGO, m_settings.m_spawnerRules[i], m_settings, randomSeeds[i], ref m_settings.m_spawnerRules[i].m_spawnedInstances, m_settings.m_spawnerRules[i].m_minRequiredFitness, false, SessionManager.GetSeaLevel());
                                }

                                //Dirty affected collision masks - if we spawned Gameobject instances with tags that are used in other collision masks, we must dirty them so they will be re-baked upon request
                                foreach (ResourceProtoGameObjectInstance instance in protoGO.m_instances)
                                {
                                    collisionMaskCache.SetGameObjectDirty(instance.m_desktopPrefab);
                                }
                                break;
                            case GaiaConstants.SpawnerResourceType.SpawnExtension:
                                ResourceProtoSpawnExtension protoSpawnExtension = m_settings.m_resources.m_spawnExtensionPrototypes[m_settings.m_spawnerRules[i].m_resourceIdx];

                                //We only may remove / reset the Spawn Extension once per terrain - otherwise this would destroy earlier spawn results in world spawns!
                                //foreach (Terrain t in operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject).Select(x => x.Key.terrain))
                                //{
                                //    if (m_settings.spawnMode == SpawnMode.Replace && m_clearedSpawnExtensionProtos.Find(x => x.terrain == t && x.prototypeId == m_settings.m_spawnerRules[i].m_resourceIdx) == null)
                                //    {
                                //        ClearSpawnExtensionsForRule(m_settings.m_spawnerRules[i]);
                                //        m_clearedSpawnExtensionProtos.Add(new TerrainPrototypeId() { terrain = t, prototypeId = m_settings.m_spawnerRules[i].m_resourceIdx });
                                //    }
                                //}
                                foreach (ResourceProtoSpawnExtensionInstance instance in protoSpawnExtension.m_instances)
                                {
                                    GameObject prefab = instance.m_spawnerPrefab;
                                    //Get ALL spawn extensions - could potentially be multiple on prefab
                                    var instanceSpawnExtensions = prefab.GetComponents<ISpawnExtension>();
                                    foreach (ISpawnExtension spawnExtension in instanceSpawnExtensions)
                                    {
                                        spawnExtension.Init(this);
                                    }
                                }
                                m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);
                                m_cancelSpawn = operation.SetSpawnExtensions(ApplyBrush(operation, MultiTerrainOperationType.GameObject, i), this, protoSpawnExtension, m_settings, i, m_settings.m_spawnerRules[i], randomSeeds[i], ref m_settings.m_spawnerRules[i].m_spawnedInstances, m_settings.m_spawnerRules[i].m_minRequiredFitness, false);

                                foreach (ResourceProtoSpawnExtensionInstance instance in protoSpawnExtension.m_instances)
                                {
                                    GameObject prefab = instance.m_spawnerPrefab;
                                    //Get ALL spawn extensions - could potentially be multiple on prefab
                                    var instanceSpawnExtensions = prefab.GetComponents<ISpawnExtension>();
                                    foreach (ISpawnExtension spawnExtension in instanceSpawnExtensions)
                                    {
                                        spawnExtension.Close();
                                    }
                                }
                                foreach (int layerID in GaiaUtils.GetIndicesfromLayerMask(m_settings.m_spawnerRules[i].m_collisionLayersToClear))
                                {
                                    collisionMaskCache.SetLayerIDDirty(layerID);
                                }

                                break;

                            case GaiaConstants.SpawnerResourceType.StampDistribution:
                                ResourceProtoStamp protoStampDistribution = m_settings.m_resources.m_stampDistributionPrototypes[m_settings.m_spawnerRules[i].m_resourceIdx];
                                //We only may remove / reset the Stamp Distribution once per terrain - otherwise this would destroy earlier spawn results in world spawns!
                                //foreach (Terrain t in operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject).Select(x => x.Key.terrain))
                                //{
                                //    if (m_settings.spawnMode == SpawnMode.Replace && m_clearedStampDistributionProtos.Find(x => x.terrain == t && x.prototypeId == m_settings.m_spawnerRules[i].m_resourceIdx) == null)
                                //    {
                                //        ClearStampDistributionForRule(m_settings.m_spawnerRules[i]);
                                //        m_clearedStampDistributionProtos.Add(new TerrainPrototypeId() { terrain = t, prototypeId = m_settings.m_spawnerRules[i].m_resourceIdx });
                                //    }
                                //}
                                m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);
                                //make sure we have defaults in there
                                if (m_worldCreationSettings.m_gaiaDefaults == null)
                                {
                                    m_worldCreationSettings.m_gaiaDefaults = Instantiate(GaiaSettings.m_currentDefaults);
                                }
                                operation.SetWorldMapStamps(ApplyBrush(operation, MultiTerrainOperationType.GameObject, i), this, protoStampDistribution, m_settings.m_spawnMode, i, m_settings.m_spawnerRules[i], m_worldCreationSettings, randomSeeds[i], ref m_settings.m_spawnerRules[i].m_spawnedInstances);
                                break;
                            case GaiaConstants.SpawnerResourceType.Probe:
                                ResourceProtoProbe protoProbe = m_settings.m_resources.m_probePrototypes[m_settings.m_spawnerRules[i].m_resourceIdx];
                                //We only may remove / reset the Stamp Distribution once per terrain - otherwise this would destroy earlier spawn results in world spawns!
                                //foreach (Terrain t in operation.affectedTerrainPixels.Where(x => x.Key.operationType == MultiTerrainOperationType.GameObject).Select(x => x.Key.terrain))
                                //{
                                //    if (m_settings.spawnMode == SpawnMode.Replace && m_clearedProbeProtos.Find(x => x.terrain == t && x.prototypeId == m_settings.m_spawnerRules[i].m_resourceIdx) == null)
                                //    {
                                //        //Deletion is handled by the Clear Game Objects function since the probes are essentially game objects
                                //        ClearGameObjectsForRule(m_settings.m_spawnerRules[i]);
                                //        m_clearedProbeProtos.Add(new TerrainPrototypeId() { terrain = t, prototypeId = m_settings.m_spawnerRules[i].m_resourceIdx });
                                //    }
                                //}
                                m_cancelSpawn = SpawnProgressBar.UpdateProgressBar(this.name, totalSpawns, totalSpawsCompleted, maxSpawnerRules, completedSpawnerRules);
                                float seaLevel = 0f;
                                bool seaLevelActive = false;

                                PWS_WaterSystem gaiawater = GaiaUtils.FindOOT<PWS_WaterSystem>();
                                if (gaiawater != null)
                                {
                                    seaLevel = gaiawater.SeaLevel;
                                    seaLevelActive = true;
                                }
                                operation.SetProbes(ApplyBrush(operation, MultiTerrainOperationType.GameObject, i), this, protoProbe, m_settings.m_spawnMode, i, m_settings.m_spawnerRules[i], randomSeeds[i], ref m_settings.m_spawnerRules[i].m_spawnedInstances, seaLevelActive, seaLevel);
                                break;
                            case GaiaConstants.SpawnerResourceType.TerrainModifierStamp:
                                Stamper stamper = GaiaSessionManager.GetOrCreateSessionStamper();
                                UpdateTerrainModifierStamperFromSpawnRule(stamper, m_settings.m_spawnerRules[i], operation.m_originTerrain);
                                stamper.Stamp();
                                break;

                        }
                        completedSpawnerRules++;
                        totalSpawsCompleted++;
                    }

                    if (m_cancelSpawn)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    SpawnProgressBar.ClearProgressBar();
                    Debug.LogError("Exception while spawning: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                }
            }

            if (m_settings.m_isWorldmapSpawner)
            {
                //update the session with the terrain tile settings we just spawned - those are required to display the stamper tokens in the correct size on the world map

                TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesX = m_worldCreationSettings.m_xTiles;
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainTilesZ = m_worldCreationSettings.m_zTiles;

                //Collect the actual created / remaining stamper settings from the tokens
                //Transform target = m_worldMapTerrain.transform.Find(GaiaConstants.worldMapStampTokenSpawnTarget);
                //if (target != null)
                //{
                //    foreach (Transform t in target)
                //    {
                //        WorldMapStampToken token = t.GetComponent<WorldMapStampToken>();
                //        if (token != null)
                //        {
                //            m_worldMapStamperSettings.Add(token.m_connectedStamperSettings);
                //        }
                //    }
                //}

            }
            GaiaStopwatch.EndEvent("Execute Spawn");
        }

        public void UpdateTerrainModifierStamperFromSpawnRule(Stamper stamper, SpawnRule spawnRule, Terrain currentTerrain = null)
        {
            ResourceProtoTerrainModifierStamp protoTerrainModifierStamp = m_settings.m_resources.m_terrainModifierStampPrototypes[spawnRule.m_resourceIdx];
            StamperSettings stamperSettings = stamper.m_settings;
            if (currentTerrain == null)
            {
                currentTerrain = GetCurrentTerrain();
            }
            stamperSettings.m_width = m_settings.m_spawnRange * 2 / currentTerrain.terrainData.size.x * 100;
            stamperSettings.m_x = transform.position.x;
            stamperSettings.m_z = transform.position.z;

            stamperSettings.CopyFromTerrainModifierStampResource(protoTerrainModifierStamp);

            //Clone the image masks over into the stamper settings
            List<ImageMask> clonedMasks = new List<ImageMask>();

            //Get the biome controller (if any)
            BiomeController biomeController = Resources.FindObjectsOfTypeAll<BiomeController>().FirstOrDefault(x => x.m_autoSpawners.Find(y => y.spawner == this) != null);
            ImageMask[] biomeControllerStack = new ImageMask[0];
            if (biomeController != null && biomeController.m_settings.m_imageMasks.Length > 0)
            {
                foreach (ImageMask imageMask in biomeController.m_settings.m_imageMasks)
                {
                    clonedMasks.Add(ImageMask.Clone(imageMask));
                }
            }
            foreach (ImageMask imageMask in m_settings.m_imageMasks)
            {
                clonedMasks.Add(ImageMask.Clone(imageMask));
            }
            foreach (ImageMask imageMask in spawnRule.m_imageMasks)
            {
                clonedMasks.Add(ImageMask.Clone(imageMask));
            }
            ////Turn all masks to global influence - this makes most sense in the context of terrain modifier stamps
            //foreach (ImageMask imageMask in clonedMasks)
            //{
            //    imageMask.m_influence = ImageMaskInfluence.Global;
            //}

            stamperSettings.m_imageMasks = clonedMasks.ToArray();

            //Operation specific settings we need to set to make sure we get a good result
            switch (protoTerrainModifierStamp.m_operation)
            {
                case FeatureOperation.LowerHeight:
                    stamperSettings.m_baseLevel = 1;
                    break;
                case FeatureOperation.SubtractHeight:
                    stamperSettings.m_y = 0;
                    stamperSettings.m_baseLevel = 1;
                    break;
                case FeatureOperation.AddHeight:
                    stamperSettings.m_y = 0;
                    stamperSettings.m_baseLevel = 0;
                    break;
                default:
                    stamperSettings.m_baseLevel = 0;
                    break;
            }
            if (protoTerrainModifierStamp.m_operation == FeatureOperation.AddHeight || protoTerrainModifierStamp.m_operation == FeatureOperation.SubtractHeight)
            {
                stamper.SetStampScaleByMeter(stamperSettings.m_absoluteHeightValue);
                stamper.m_settings.m_height = stamper.transform.localScale.y;
            }
            stamper.transform.position = new Vector3((float)stamperSettings.m_x, (float)stamperSettings.m_y, (float)stamperSettings.m_z);
            stamper.transform.localScale = new Vector3(stamperSettings.m_width, stamperSettings.m_height, stamperSettings.m_width);
            stamper.m_stampDirty = true;
        }

        public void SetRecommendedStampSizes(bool allowHeightChange = true)
        {
            float targetWorldSize = m_worldCreationSettings.m_tileSize * m_worldCreationSettings.m_xTiles;

            if (targetWorldSize > 2048)
            {
                if (allowHeightChange)
                {
                    m_worldCreationSettings.m_tileHeight = Mathf.Min(4097, Mathf.Max(2048, targetWorldSize / 10f));
                }
                m_baseTerrainSettings.m_heightScale = 7 + Mathf.Min(40, targetWorldSize / 3000);
                m_settings.m_stampDensity = 5 + Mathf.RoundToInt(m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution / 4097);
                m_settings.m_stampWidth = Mathf.RoundToInt(100f / (float)m_settings.m_stampDensity * 5f); //Mathf.RoundToInt(((float)targetWorldSize / (float)m_worldCreationSettings.m_xTiles / (float)m_settings.m_stampDensity) /2f);
                m_settings.m_stampHeight = 100;
            }
            else if (targetWorldSize > 1024)
            {
                if (allowHeightChange)
                {
                    m_worldCreationSettings.m_tileHeight = m_worldCreationSettings.m_tileSize;
                }
                m_baseTerrainSettings.m_heightScale = 7;
                m_settings.m_stampDensity = 5;
                m_settings.m_stampWidth = 100;
                m_settings.m_stampHeight = 100;

            }
            else if (targetWorldSize > 512)
            {
                if (allowHeightChange)
                {
                    m_worldCreationSettings.m_tileHeight = m_worldCreationSettings.m_tileSize;
                }
                m_baseTerrainSettings.m_heightScale = 7;
                m_settings.m_stampDensity = 4;
                m_settings.m_stampWidth = 120;
                m_settings.m_stampHeight = 100;
            }
            else if (targetWorldSize > 256)
            {
                if (allowHeightChange)
                {
                    m_worldCreationSettings.m_tileHeight = m_worldCreationSettings.m_tileSize;
                }
                m_baseTerrainSettings.m_heightScale = 7;
                m_settings.m_stampDensity = 3;
                m_settings.m_stampWidth = 140;
                m_settings.m_stampHeight = 100;
            }
            else //256 and smaller
            {
                if (allowHeightChange)
                {
                    m_worldCreationSettings.m_tileHeight = m_worldCreationSettings.m_tileSize;
                }
                m_baseTerrainSettings.m_heightScale = 7;
                m_settings.m_stampDensity = 3;
                m_settings.m_stampWidth = 160;
                m_settings.m_stampHeight = 100;
            }

            //reset previewed terrain tile to center 
            if (m_worldCreationSettings.m_xTiles > 1)
            {
                m_worldDesignerPreviewTileX = Mathf.FloorToInt(m_worldCreationSettings.m_xTiles / 2f);
                m_worldDesignerPreviewTileZ = m_worldDesignerPreviewTileX;
            }

            SessionManager.DirtyWorldMapMinMax();
            GetOrCreateBaseTerrainStamper(true).m_stampDirty = true;
        }

        public void RandomizeWorldDesigner()
        {
            GenerateNewRandomSeed();
            BaseTerrainSettings bts = m_baseTerrainSettings;
            //Initialize with common, sane settings
            bts.m_baseLevel = 0;
            bts.m_baseNoiseSettings[0].m_shapeNoiseStyle = NoiseTypeName.Perlin;
            bts.m_baseNoiseSettings[0].m_shapeGranularity = 1;

            XorshiftPlus randomGenerator = new XorshiftPlus(m_settings.m_randomSeed);

            //Strength + Steepness
            bts.m_baseNoiseSettings[0].m_shapeStrength = randomGenerator.Next(0.3f, 0.6f);
            //The sum of strength + steepness should not exceed 0.8 to avoid terrain being cut off at the top
            float maxSteepness = 0.8f - bts.m_baseNoiseSettings[0].m_shapeStrength;
            bts.m_baseNoiseSettings[0].m_shapeSteepness = randomGenerator.Next(0.05f, maxSteepness);

            //Feature size - should scale with world sizes accordingly
            float minFeatureSize = 8f;
            float maxFeatureSize = 10f;
            bts.m_baseNoiseSettings[0].m_shapeSize = randomGenerator.Next(minFeatureSize, maxFeatureSize);

            //Offset - just choose a Vector 3 randomly
            bts.m_baseNoiseSettings[0].m_shapeOffset = new Vector3(randomGenerator.Next(-1000f, 1000f), randomGenerator.Next(-1000f, 1000f), randomGenerator.Next(-1000f, 1000f));
            //Granularity - just between 0.7f and 1.3f, don't want to go high up since the noise will take away detail from the stamps
            bts.m_baseNoiseSettings[0].m_shapeGranularity = randomGenerator.Next(0.7f, 1.3f);

            //These values add warp, but nothing too strong that would interfere with the stamps
            bts.m_baseNoiseSettings[0].m_warpIterations = randomGenerator.Next(0.0f, 8f);
            bts.m_baseNoiseSettings[0].m_warpStrength = randomGenerator.Next(0.0f, 1f);
            bts.m_baseNoiseSettings[0].m_warpOffset = new Vector3(randomGenerator.Next(-1000f, 1000f), randomGenerator.Next(-1000f, 1000f), randomGenerator.Next(-1000f, 1000f));



            //spawn rules - just change the probability randomly
            foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_isActive == true))
            {
                m_settings.m_resources.m_stampDistributionPrototypes[sr.m_resourceIdx].m_spawnProbability = randomGenerator.Next(0.0f, 1.0f);
            }

            UpdateBaseTerrainStamper();
        }

        private RenderTexture SimulateRule(GaiaMultiTerrainOperation operation, int spawnRuleID)
        {
            SpawnRule spawnRule = m_settings.m_spawnerRules[spawnRuleID];
            ComputeShader shader = GaiaSettings.m_spawnSimulateComputeShader;
            int kernelHandle = shader.FindKernel("CSMain");

            MultiTerrainOperationType multiTerrainOperationType = MultiTerrainOperationType.GameObject;
            float fitnessThreshold = 0.5f;

            switch (spawnRule.m_resourceType)
            {
                case SpawnerResourceType.TerrainDetail:
                    multiTerrainOperationType = MultiTerrainOperationType.TerrainDetail;
                    fitnessThreshold = spawnRule.m_terrainDetailMinFitness;
                    break;
                case SpawnerResourceType.TerrainTexture:
                    multiTerrainOperationType = MultiTerrainOperationType.Texture;
                    fitnessThreshold = 0;
                    break;
                case SpawnerResourceType.TerrainTree:
                    multiTerrainOperationType = MultiTerrainOperationType.Tree;
                    fitnessThreshold = spawnRule.m_minRequiredFitness;
                    break;
                case SpawnerResourceType.GameObject:
                    multiTerrainOperationType = MultiTerrainOperationType.GameObject;
                    fitnessThreshold = spawnRule.m_minRequiredFitness;
                    break;
                case SpawnerResourceType.SpawnExtension:
                    multiTerrainOperationType = MultiTerrainOperationType.GameObject;
                    fitnessThreshold = 0;
                    break;
                case SpawnerResourceType.StampDistribution:
                    multiTerrainOperationType = MultiTerrainOperationType.GameObject;
                    fitnessThreshold = 0;
                    break;
                case SpawnerResourceType.WorldBiomeMask:
                    //this should never happen
                    break;
            }

            RenderTexture opRenderTexture = ApplyBrush(operation, multiTerrainOperationType, spawnRuleID);

            //Get the affected terrains according to operation type
            var affectedTerrains = operation.affectedTerrainPixels.Where(x => x.Key.operationType == multiTerrainOperationType).ToArray();

            //Build an input and output data buffer array to get info about the terrain positions in and out of the compute shader
            TerrainPosition[] inputTerrainPositions = new TerrainPosition[affectedTerrains.Count()];
            TerrainPosition[] outputTerrainPositions = new TerrainPosition[affectedTerrains.Count()];
            for (int i = 0; i < affectedTerrains.Count(); i++)
            {
                var entry = affectedTerrains[i];
                //assume first these terrain pixels will be affected, since this entry could still be set to false
                //form a previous spawn.
                affectedTerrains[i].Value.simulationPositive = true;
                inputTerrainPositions[i] = new TerrainPosition()
                {
                    terrainID = i,
                    min = entry.Value.affectedOperationPixels.min,
                    max = entry.Value.affectedOperationPixels.max,
                    affected = 0
                };
            }

            //Configure & run the compute shader
            ComputeBuffer buffer = new ComputeBuffer(affectedTerrains.Count(), 24);
            buffer.SetData(inputTerrainPositions);
            shader.SetTexture(kernelHandle, "Input", opRenderTexture);
            shader.SetFloat("fitnessThreshold", spawnRule.m_terrainDetailMinFitness);
            shader.SetInt("numberOfTerrains", affectedTerrains.Count());
            shader.SetBuffer(kernelHandle, "outputBuffer", buffer);
            shader.Dispatch(kernelHandle, opRenderTexture.width / 8, opRenderTexture.height / 8, 1);
            buffer.GetData(outputTerrainPositions);
            //We got the result, now take our initial array and check if those terrains listed in the OP are actually affected
            for (int i = 0; i < affectedTerrains.Count(); i++)
            {
                TerrainPosition terrainPosition = outputTerrainPositions[i];
                if (terrainPosition.affected <= 0)
                {
                    //kick out this entry from the operation if the simulation result says it will not be affected
                    // => no need to execute those later when spawning
                    affectedTerrains[i].Value.simulationPositive = false;
                }
            }

            buffer.Release();
            return opRenderTexture;

        }


        /// <summary>
        /// Gets the current height (including stamps) on a world designer preview according to the input position in world space.
        /// </summary>
        /// <param name="worldSpacePos">Position to be sampled in world space. Only X and Z are relevant</param>
        public Vector3 GetHeightOnWorldDesignerPreview(Vector3 worldSpacePos)
        {

            float halfWorldSize = TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f;

            //is sample position within bounds of the preview?
            if (worldSpacePos.x < transform.position.x - halfWorldSize || worldSpacePos.x > transform.position.x + halfWorldSize || worldSpacePos.z < transform.position.z - halfWorldSize || worldSpacePos.z > transform.position.z + halfWorldSize)
            {
                //we are oob and can't sample the preview, return original height
                return worldSpacePos;
            }

            Vector3 scalarPos = new Vector3(Mathf.InverseLerp(transform.position.x - halfWorldSize, transform.position.x + halfWorldSize, worldSpacePos.x), 0.0f, Mathf.InverseLerp(transform.position.z - halfWorldSize, transform.position.z + halfWorldSize, worldSpacePos.z));

            Stamper baseTerrainStamper = GetOrCreateBaseTerrainStamper(true);

            ComputeShader shader = GaiaSettings.m_WDHeightsComputeShader;
            int kernelHandle = shader.FindKernel("CSMain");
            ObjectPosition[] outputPositions = new ObjectPosition[1] { new ObjectPosition() { position = scalarPos } };
            ComputeBuffer buffer = new ComputeBuffer(1, 12);
            buffer.SetData(outputPositions);
            shader.SetTexture(kernelHandle, "Input", baseTerrainStamper.m_cachedRenderTexture);
            shader.SetBuffer(kernelHandle, "results", buffer);
            shader.Dispatch(kernelHandle, 1, 1, 1);
            buffer.GetData(outputPositions);
            float resultHeight = outputPositions[0].position.y;
            buffer.Release();

            return new Vector3(worldSpacePos.x, Mathf.Lerp(0f, m_worldCreationSettings.m_tileHeight, resultHeight) * 2f, worldSpacePos.z);
        }


        /// <summary>
        /// Spawn the World Detail Stamps for the World Designer according to its settings and the current BaseTerrain Preview
        /// </summary>
        public void SpawnStamps()
        {
            //nothing to spawn? => exit
            if (m_settings.m_spawnerRules.Where(x => x.m_isActive == true).Count() <= 0)
            {
                return;
            }
#if UNITY_EDITOR
            GenerateNewRandomSeed();
            ClearStampDistributions();
            //we need to "reset" the preview on the base terrain stamper, to get the base texture without the stamps
            Stamper baseTerrainStamper = GetOrCreateBaseTerrainStamper(true);
            baseTerrainStamper.m_stampDirty = true;
            baseTerrainStamper.DrawStampPreview(m_worldMapStamperSettings, true);
            SessionManager.DirtyWorldMapMinMax();
            UpdateMinMaxHeight();
            ComputeShader shader = GaiaSettings.m_spawnStampsComputeShader;
            int kernelHandle = shader.FindKernel("CSMain");
            int texelLocationIncrement = Mathf.RoundToInt((baseTerrainStamper.m_cachedRenderTexture.width) / m_settings.m_stampDensity);
            int maxJitter = Mathf.RoundToInt((texelLocationIncrement / 2f) * m_settings.m_stampJitter);
            int stampsPerRow = Mathf.RoundToInt(baseTerrainStamper.m_cachedRenderTexture.width / texelLocationIncrement) + 1;
            int numberOfStamps = stampsPerRow * stampsPerRow;
            StampPosition[] outputStampPositions = new StampPosition[numberOfStamps];
            ComputeBuffer buffer = new ComputeBuffer(numberOfStamps, 12);
            buffer.SetData(outputStampPositions);
            shader.SetTexture(kernelHandle, "Input", baseTerrainStamper.m_cachedRenderTexture);
            shader.SetInt("texelLocationIncrement", texelLocationIncrement - 1);
            shader.SetInt("maxJitter", maxJitter);
            shader.SetInt("randomSeed", m_settings.m_randomSeed);
            shader.SetInt("stampsPerRow", stampsPerRow);
            shader.SetInt("textureResolution", baseTerrainStamper.m_cachedRenderTexture.width - 1);
            shader.SetBuffer(kernelHandle, "results", buffer);
            shader.Dispatch(kernelHandle, baseTerrainStamper.m_cachedRenderTexture.width / 8, baseTerrainStamper.m_cachedRenderTexture.height / 8, 1);
            buffer.GetData(outputStampPositions);
            XorshiftPlus randomGenerator = new XorshiftPlus(m_settings.m_randomSeed);
            GaiaSession session = GaiaSessionManager.GetSessionManager().m_session;

            //Get the max stamper size to not overstep the max internal resolution value of 11574 of the stamper preview
            float range = Mathf.RoundToInt(Mathf.Clamp(worldCreationSettings.m_tileSize, 1, (float)11574 / (float)worldCreationSettings.m_gaiaDefaults.m_heightmapResolution * 100));
            float widthfactor = worldCreationSettings.m_tileSize * worldCreationSettings.m_xTiles / 100f;
            float maxStamperSize = range * widthfactor;
            float worldRange = worldCreationSettings.m_tileSize * worldCreationSettings.m_xTiles / 2f;
            float texelToWorldspaceUnit = TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution;

            //Reset the instance count
            foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_isActive == true))
            {
                sr.m_spawnedInstances = 0;
            }

            //Get a scalar max height of the input terrain (the output from the compute shader goes from 0 to 0.5)
            float scalarMaxHeight = Mathf.Lerp(0, 0.5f, Mathf.InverseLerp(0, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight, m_maxWorldHeight));

            try
            {

                for (int i = 0; i < outputStampPositions.Length; i++)
                {
                    bool cancel = ProgressBar.Show(ProgressBarPriority.Spawning, "Spawning Stamps..", "Spawning Stamps for the preview", i, outputStampPositions.Length, true, true);

                    if (cancel)
                    {
                        m_spawnProgress = 0;
                        break;
                    }
                    m_spawnProgress = i / (float)outputStampPositions.Length;

                    StampPosition stampPosition = outputStampPositions[i];
                    //Build list of spawn chances, taking the input height into account via the curve mapping
                    List<(SpawnRule, int)> chancesList = new List<(SpawnRule, int)>();
                    foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_isActive == true))
                    {
                        ResourceProtoStamp resourceProtoStamp = m_settings.m_resources.m_stampDistributionPrototypes[sr.m_resourceIdx];
                        //native probability from the rule without height
                        float probability = resourceProtoStamp.m_spawnProbability;
                        //adjusting for input height, make sure that the chance is at least 0.01 so the picker has something to pick
                        probability *= Mathf.Max(0f, (resourceProtoStamp.m_inputHeightToProbabilityMapping.Evaluate(Mathf.InverseLerp(0, scalarMaxHeight, stampPosition.position.y))));
                        chancesList.Add((sr, Mathf.RoundToInt(probability * 100)));
                    }

                    if (chancesList.Sum(x => x.Item2) <= 0)
                    {
                        continue;
                    }

                    //Pick the stamp rule
                    SpawnRule rule = GaiaUtils.PickRandomListElementWeightedChance<SpawnRule>(chancesList, randomGenerator);

                    if (rule == null)
                    {
                        continue;
                    }

                    float spawnRotationY = randomGenerator.Next(rule.m_minDirection, rule.m_maxDirection);
                    float xWorldSpace = transform.position.x - (TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f) + (stampPosition.position.x * texelToWorldspaceUnit);
                    float zWorldSpace = transform.position.z - (TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f) + (stampPosition.position.z * texelToWorldspaceUnit);
                    float yWorldSpace = Mathf.Lerp(0, 1024, Mathf.InverseLerp(0, 0.5f, stampPosition.position.y));

                    ResourceProtoStamp selectedStampPrototype = m_settings.m_resources.m_stampDistributionPrototypes[rule.m_resourceIdx];
                    //Get a random texture from that directory
                    string directory = GaiaDirectories.GetStampDirectory() + Path.DirectorySeparatorChar + selectedStampPrototype.m_featureType;
                    var info = new DirectoryInfo(directory);

                    if (info == null)
                    {
                        Debug.LogWarning("Could not access directory " + directory + " when trying to pick a stamp for random terrain generation!");
                        continue;
                    }
                    FileInfo[] allFiles = info.GetFiles();
                    FileInfo[] allTextures = allFiles.Where(a => a.Extension != ".meta").ToArray();
                    string path = allTextures[randomGenerator.Next(0, allTextures.Length - 1)].FullName;
                    path = path.Remove(0, Application.dataPath.Length - "Assets".Length);

                    string chosenTextureGUID = AssetDatabase.AssetPathToGUID(path);

                    StamperSettings stamperSettings = ScriptableObject.CreateInstance<StamperSettings>();
                    stamperSettings.m_x = xWorldSpace;
                    stamperSettings.m_y = yWorldSpace;
                    stamperSettings.m_z = zWorldSpace;
                    stamperSettings.m_operation = (GaiaConstants.FeatureOperation)selectedStampPrototype.m_operation;

                    if (stamperSettings.m_operation == GaiaConstants.FeatureOperation.RaiseHeight || stamperSettings.m_operation == GaiaConstants.FeatureOperation.AddHeight || stamperSettings.m_operation == GaiaConstants.FeatureOperation.MixHeight)
                    {
                        //Move the base level to 0 to make sure the stamp is not hidden right from the start
                        stamperSettings.m_baseLevel = 0f;
                    }
                    if (stamperSettings.m_operation == GaiaConstants.FeatureOperation.LowerHeight || stamperSettings.m_operation == GaiaConstants.FeatureOperation.SubtractHeight)
                    {
                        stamperSettings.m_baseLevel = 1f;
                    }

                    float widthValue = 0.5f;
                    widthValue = randomGenerator.Next();
                    float unscaledWidth = Mathf.Lerp(selectedStampPrototype.m_minWidth * (m_settings.m_stampWidth / 100.0f), selectedStampPrototype.m_maxWidth * (m_settings.m_stampWidth / 100.0f), widthValue);
                    stamperSettings.m_width = Mathf.Clamp(unscaledWidth, 0, maxStamperSize);


                    float heightValue = Mathf.InverseLerp(0, scalarMaxHeight, stampPosition.position.y);
                    heightValue = selectedStampPrototype.m_inputHeightToStampHeightMapping.Evaluate(heightValue);

                    if (stamperSettings.m_operation != FeatureOperation.MixHeight)
                    {
                        stamperSettings.m_height = Mathf.Lerp(selectedStampPrototype.m_minHeight * (m_settings.m_stampHeight / 100.0f), selectedStampPrototype.m_maxHeight * (m_settings.m_stampHeight / 100.0f), heightValue);
                    }
                    else
                    {
                        stamperSettings.m_mixHeightStrength = Mathf.Lerp(selectedStampPrototype.m_minMixStrength * (m_settings.m_stampHeight / 100.0f), selectedStampPrototype.m_maxMixStrength * (m_settings.m_stampHeight / 100.0f), heightValue);
                        stamperSettings.m_mixMidPoint = Mathf.Lerp(selectedStampPrototype.m_minMixMidPoint, selectedStampPrototype.m_maxMixMidPoint, randomGenerator.Next());
                    }

                    //bias the mixmidPoint according to the selected border style
                    float distanceFromCenter = Mathf.Abs((float)stamperSettings.m_x) + Mathf.Abs((float)stamperSettings.m_z);

                    switch (m_baseTerrainSettings.m_borderStyle)
                    {
                        case GeneratorBorderStyle.Mountains:
                            stamperSettings.m_mixMidPoint = Mathf.Lerp(stamperSettings.m_mixMidPoint, stamperSettings.m_mixMidPoint * 0.5f, Mathf.InverseLerp(worldRange * 0.8f, worldRange, distanceFromCenter));
                            break;
                        case GeneratorBorderStyle.Water:
                            stamperSettings.m_mixMidPoint = Mathf.Lerp(stamperSettings.m_mixMidPoint, 1f, Mathf.InverseLerp(worldRange * 0.8f, worldRange, distanceFromCenter));
                            break;
                    }


                    stamperSettings.m_rotation = randomGenerator.Next(0f, 360f);
                    stamperSettings.m_y += Mathf.Lerp(selectedStampPrototype.m_minYOffset, selectedStampPrototype.m_maxYOffset, heightValue);
                    stamperSettings.m_stamperInputImageMask.m_operation = ImageMaskOperation.ImageMask;
                    stamperSettings.m_stamperInputImageMask.SetTextureGUID(chosenTextureGUID);

                    if (stamperSettings.m_operation == GaiaConstants.FeatureOperation.MixHeight)
                    {
                        stamperSettings.m_stamperInputImageMask.m_influence = ImageMaskInfluence.Local;
                    }
                    else
                    {
                        stamperSettings.m_stamperInputImageMask.m_influence = ImageMaskInfluence.Global;
                    }


                    ImageMask[] imageMasks = null;
                    switch (selectedStampPrototype.m_borderMaskStyle)
                    {
                        case BorderMaskStyle.ImageMask:

                            //Get a random border mask texture from the mask directory
                            string borderMaskDirectory = GaiaDirectories.GetStampDirectory() + Path.DirectorySeparatorChar + selectedStampPrototype.m_borderMaskType;
                            info = new DirectoryInfo(borderMaskDirectory);

                            if (info == null)
                            {
                                Debug.LogWarning("Could not access directory " + borderMaskDirectory + " when trying to pick a border mask for random terrain generation!");
                                continue;
                            }
                            allFiles = info.GetFiles();
                            allTextures = allFiles.Where(a => a.Extension != ".meta").ToArray();
                            path = allTextures[randomGenerator.Next(0, allTextures.Length - 1)].FullName;
                            path = path.Remove(0, Application.dataPath.Length - "Assets".Length);

                            Texture2D chosenMaskTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                            imageMasks = new ImageMask[1] {
                                    new ImageMask() { m_operation = ImageMaskOperation.ImageMask, ImageMaskTexture = chosenMaskTexture, m_influence = ImageMaskInfluence.Local }
                                    };
                            break;
                        case BorderMaskStyle.DistanceMask:
                            imageMasks = new ImageMask[1] {
                                    new ImageMask() { m_operation = ImageMaskOperation.DistanceMask, m_influence = ImageMaskInfluence.Local, m_distanceMaskAxes = ImageMaskDistanceMaskAxes.XZCircle }
                                    };
                            break;
                        case BorderMaskStyle.None:
                            //do nothing and leave the secondary masks null 
                            break;
                    }


                    stamperSettings.m_imageMasks = imageMasks;

                    //Roll for inversion of the stamp
                    if (randomGenerator.Next(0f, 100f) <= selectedStampPrototype.m_invertChance)
                    {
                        GaiaUtils.InvertAnimationCurve(ref stamperSettings.m_stamperInputImageMask.m_strengthTransformCurve);
                    }

                    m_worldMapStamperSettings.Add(stamperSettings);

                    SetDefaultWorldDesignerPreviewSettings();

                    rule.m_spawnedInstances++;

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while generating the Stamps for the World Designer. Exception: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                m_spawnProgress = 0;
                buffer.Release();
                ProgressBar.Clear(ProgressBarPriority.Spawning);
            }

            if (m_baseTerrainSettings.m_drawPreview)
            {
                UpdateBaseTerrainStamper();
            }
#endif
        }


        //Resets the world designer to the default settings 
        public void ResetWorldDesigner()
        {
            if (GaiaUtils.DisplayDialogNoEditor("Reset World Designer Settings?", "Do you really want to reset the world designer to its default settings?\r\n\r\nThis will change all settings back to an appropiate value for your given world size.", "Reset", "Cancel"))
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                m_baseTerrainSettings = new BaseTerrainSettings();
                LoadSettings(m_gaiaSettings.m_defaultStampSpawnSettings);
                if (m_worldCreationSettings.m_targetSizePreset != GaiaConstants.EnvironmentSizePreset.Custom)
                {
                    SpawnStamps();
                }
                else
                {
                    SetDefaultWorldDesignerPreviewSettings();
                }
                StoreWorldSize();
            }
        }

        /// <summary>
        /// Checks if the current preview is a 1:1 representation of the actual heightmap resolution, and if there are more stamps spawned for this world than are currently rendered.
        /// </summary>
        /// <param name="heightmapResExceeded"></param>
        /// <param name="stampsExceeded"></param>
        public void CheckIfFullPreview(ref bool heightmapResExceeded, ref bool stampsExceeded)
        {
            //if (m_gaiaSettings == null)
            //{
            //    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
            //}

            heightmapResExceeded = m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution * m_worldCreationSettings.m_xTiles > 4097;
            //stamps cannot exceed anymore
            stampsExceeded = false;
        }

        /// <summary>
        /// Sets the default preview settings for the world designer preview - depending on size and resolution and spawned stamps different default settings need to be applied
        /// </summary>
        public void SetDefaultWorldDesignerPreviewSettings()
        {
            if (m_gaiaSettings == null)
            {
                m_gaiaSettings = GaiaUtils.GetGaiaSettings();
            }

            bool heightmapResExceeded = false;
            bool stampsExceeded = false;

            CheckIfFullPreview(ref heightmapResExceeded, ref stampsExceeded);

            Stamper stamper = GetOrCreateBaseTerrainStamper(false);

            //unfold the preview if not 1:1, so the user can see the advanced settings in the preview panel
            if ((heightmapResExceeded || stampsExceeded) && !m_baseTerrainSettings.m_alwaysFullPreview)
            {
                stamper.m_useCustomPreviewBounds = true;
            }
            else
            {
                stamper.m_useCustomPreviewBounds = false;
            }

            UpdateWorldDesignerPreviewSizeAndResolution();
            RefreshWorldDesignerStamps();
        }

        /// <summary>
        /// Draws the stamps in the selected preview area when in single terrain mode
        /// </summary>
        public void RefreshWorldDesignerStamps()
        {
#if UNITY_EDITOR
            Stamper stamper = GetOrCreateBaseTerrainStamper(true);
            if (m_worldDesignerPreviewMode == WorldDesignerPreviewMode.SingleTerrain)
            {
                transform.position = new Vector3(m_worldDesignerUserBounds.center.x, 0f, m_worldDesignerUserBounds.center.z);
                stamper.transform.position = transform.position;
                Quaternion quaternion = SceneView.lastActiveSceneView.camera.transform.rotation;
                SceneView.lastActiveSceneView.LookAt(transform.position, quaternion, SceneView.lastActiveSceneView.size);
            }
            UpdateWorldDesignerPreviewSizeAndResolution();
            stamper.m_stampDirty = true;
#endif
        }

        /// <summary>
        /// Updates the World Designer preview with the current settings
        /// </summary>
        public void UpdateWorldDesignerPreviewSizeAndResolution()
        {
            Stamper stamper = GetOrCreateBaseTerrainStamper(true);
            stamper.m_worldDesignerPreviewMode = m_worldDesignerPreviewMode;
            stamper.m_worldDesignerPreviewTiles = m_worldCreationSettings.m_xTiles;

            if (m_worldDesignerPreviewMode == WorldDesignerPreviewMode.Worldmap)
            {
                stamper.m_worldDesignerPreviewBounds.center = m_worldDesignerUserBounds.center;
                stamper.m_worldDesignerPreviewBounds.size = m_worldDesignerUserBounds.size;
                if (m_worldCreationSettings.m_gaiaDefaults == null)
                {
                    m_worldCreationSettings.m_gaiaDefaults = GaiaSettings.m_currentDefaults;
                }
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution = Math.Min(4097, m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution);
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange = Mathf.RoundToInt(m_worldCreationSettings.m_xTiles * m_worldCreationSettings.m_tileSize);
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight = m_worldCreationSettings.m_tileHeight;
            }
            else
            {
                stamper.m_worldDesignerPreviewBounds.center = new Vector3(transform.position.x, m_worldCreationSettings.m_tileHeight / 2f, transform.position.z);
                stamper.m_worldDesignerPreviewBounds.size = new Vector3(m_worldCreationSettings.m_tileSize, m_worldCreationSettings.m_tileHeight, m_worldCreationSettings.m_tileSize);
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange = m_worldCreationSettings.m_tileSize;
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight = m_worldCreationSettings.m_tileHeight;
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewHeightmapResolution = m_worldCreationSettings.m_gaiaDefaults.m_heightmapResolution;
            }
        }


        /// <summary>
        /// Returns a vector 3 representing the  (approximate!) maximum possible size for the world designer preview according to stamp density / spawned stamps
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMaxWorldDesignerStampPreviewSize()
        {
            if (m_gaiaSettings == null)
            {
                m_gaiaSettings = GaiaUtils.GetGaiaSettings();
            }

            float stampsPerMeter = (float)(m_settings.m_stampDensity + 1) / (float)(m_worldCreationSettings.m_tileSize * m_worldCreationSettings.m_xTiles);
            float maxSideLength = Mathf.Sqrt((float)m_gaiaSettings.m_maxWorldDesignerPreviewStamps) / stampsPerMeter / 2f;
            //subtract the average stamp width - the area will render every stamp it touches, so we need to reduce it more if we want to 
            //keep the max stamp limit
            maxSideLength -= (m_worldCreationSettings.m_tileSize * m_settings.m_stampWidth / 100f / m_worldCreationSettings.m_xTiles);
            return new Vector3(maxSideLength, m_worldCreationSettings.m_tileHeight, maxSideLength);
        }

        /// <summary>
        /// Returns the required spawner runs to cover the entire wolrd according to the current max spawner size when doing a spawn across a certain area
        /// </summary>
        /// <param name="area">The area we are iterating over</param>
        /// <param name="range">The spawner range used for the iterations</param>
        /// <returns>The number of steps required to iterate across the world</returns>
        public static int GetAreaSpawnSteps(BoundsDouble area, float range)
        {
            float spawnRange = (float)Mathd.Min(range, Mathd.Min(area.extents.x, area.extents.z));
            return Mathd.CeilToInt(area.size.x / (spawnRange * 2f)) * Mathd.CeilToInt(area.size.z / (spawnRange * 2f));
        }


        /// <summary>
        /// Decreases all resource indexes by 1 for a certain resource type - used when a resource is removed from the spawner so all indices need to be corrected by 1 above the old index of the deleted resource.
        /// </summary>
        /// <param name="terrainTree"></param>
        /// <param name="i"></param>
        public void CorrectIndicesAfteResourceDeletion(SpawnerResourceType resourceType, int oldIndex)
        {

            foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_resourceType == resourceType))
            {
                if (sr.m_resourceIdx >= oldIndex)
                {
                    sr.m_resourceIdx--;
                }
            }
        }

        /// <summary>
        /// Run a random location based spawner iteration - the spawner is always trying to spawn something on the underlying terrain
        /// </summary>
        public IEnumerator RunRandomSpawnerIteration()
        {
            //if (m_showDebug)
            //{
            //    Debug.Log(string.Format("{0}: Running random iteration", gameObject.name));
            //}

            ////Start iterating
            //int ruleIdx;
            //float fitness, maxFitness, selectedFitness;
            //SpawnRule rule, fittestRule, selectedRule;
            //SpawnInfo spawnInfo = new SpawnInfo();
            //SpawnLocation spawnLocation;
            //List<SpawnLocation> spawnLocations = new List<SpawnLocation>();
            //int spawnLocationsIdx = 0;
            //int failedSpawns = 0;

            ////Set progress
            //m_spawnProgress = 0f;
            //m_spawnComplete = false;

            ////Time control for enumeration
            //float currentTime = Time.realtimeSinceStartup;
            //float accumulatedTime = 0.0f;

            ////Create spawn caches
            //CreateSpawnCaches();

            ////Load image filter
            //LoadImageMask();

            //for (int terrainID = 0; terrainID < Terrain.activeTerrains.Length; terrainID++)
            //{
            //    //Set up the texture layer array in spawn info
            //    spawnInfo.m_textureStrengths = new float[Terrain.activeTerrains[terrainID].terrainData.alphamapLayers];

            //    //Run the location checks
            //    for (int checks = 0; checks < m_locationChecksPerInt; checks++)
            //    {
            //        //Create the spawn location
            //        spawnLocation = new SpawnLocation();

            //        //Choose a random location around the spawner
            //        if (m_spawnLocationAlgorithm == GaiaConstants.SpawnerLocation.RandomLocation)
            //        {
            //            spawnLocation.m_location = GetRandomV3(m_settings.m_spawnRange);
            //            spawnLocation.m_location = transform.position + spawnLocation.m_location;
            //        }
            //        else
            //        {
            //            if (spawnLocations.Count == 0 || spawnLocations.Count > m_maxRandomClusterSize || failedSpawns > m_maxRandomClusterSize)
            //            {
            //                spawnLocation.m_location = GetRandomV3(m_settings.m_spawnRange);
            //                spawnLocation.m_location = transform.position + spawnLocation.m_location;
            //                failedSpawns = 0;
            //                spawnLocationsIdx = 0;
            //                spawnLocations.Clear();
            //            }
            //            else
            //            {
            //                if (spawnLocationsIdx >= spawnLocations.Count)
            //                {
            //                    spawnLocationsIdx = 0;
            //                }
            //                spawnLocation.m_location = GetRandomV3(spawnLocations[spawnLocationsIdx].m_seedDistance);
            //                spawnLocation.m_location = spawnLocations[spawnLocationsIdx++].m_location + spawnLocation.m_location;
            //            }
            //        }

            //        //Run a ray traced hit check to see what we have hit, use rules to determine fitness and select a rule to spawn
            //        if (CheckLocation(spawnLocation.m_location, ref spawnInfo))
            //        {
            //            //Now perform a rule check based on the selected algorithm

            //            //All rules
            //            if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.All)
            //            {
            //                for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            //                {
            //                    rule = m_settings.m_spawnerRules[ruleIdx];
            //                    spawnInfo.m_fitness = rule.GetFitness(ref spawnInfo);
            //                    if (TryExecuteRule(ref rule, ref spawnInfo) == true)
            //                    {
            //                        failedSpawns = 0;
            //                        //spawnLocation.m_seedDistance = rule.GetSeedThrowRange(ref spawnInfo);
            //                        spawnLocations.Add(spawnLocation);
            //                    }
            //                    else
            //                    {
            //                        failedSpawns++;
            //                    }
            //                }
            //            }

            //            //Random spawn rule
            //            else if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.Random)
            //            {
            //                rule = m_settings.m_spawnerRules[GetRandomInt(0, m_settings.m_spawnerRules.Count - 1)];
            //                spawnInfo.m_fitness = rule.GetFitness(ref spawnInfo);
            //                if (TryExecuteRule(ref rule, ref spawnInfo) == true)
            //                {
            //                    failedSpawns = 0;
            //                    //spawnLocation.m_seedDistance = rule.GetSeedThrowRange(ref spawnInfo);
            //                    spawnLocations.Add(spawnLocation);
            //                }
            //                else
            //                {
            //                    failedSpawns++;
            //                }
            //            }

            //            //Fittest spawn rule
            //            else if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.Fittest)
            //            {
            //                fittestRule = null;
            //                maxFitness = 0f;
            //                for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            //                {
            //                    rule = m_settings.m_spawnerRules[ruleIdx];
            //                    fitness = rule.GetFitness(ref spawnInfo);
            //                    if (fitness > maxFitness)
            //                    {
            //                        maxFitness = fitness;
            //                        fittestRule = rule;
            //                    }
            //                    else
            //                    {
            //                        //If they are approx equal then give another rule a chance as well to add interest
            //                        if (Gaia.GaiaUtils.Math_ApproximatelyEqual(fitness, maxFitness, 0.005f))
            //                        {
            //                            if (GetRandomFloat(0f, 1f) > 0.5f)
            //                            {
            //                                maxFitness = fitness;
            //                                fittestRule = rule;
            //                            }
            //                        }
            //                    }
            //                }
            //                spawnInfo.m_fitness = maxFitness;
            //                if (TryExecuteRule(ref fittestRule, ref spawnInfo) == true)
            //                {
            //                    failedSpawns = 0;
            //                    spawnLocation.m_seedDistance = fittestRule.GetSeedThrowRange(ref spawnInfo);
            //                    spawnLocations.Add(spawnLocation);
            //                }
            //                else
            //                {
            //                    failedSpawns++;
            //                }
            //            }

            //            //Weighted fittest spawn rule - this implementation will favour fittest
            //            else
            //            {
            //                fittestRule = selectedRule = null;
            //                maxFitness = selectedFitness = 0f;
            //                for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
            //                {
            //                    rule = m_settings.m_spawnerRules[ruleIdx];
            //                    fitness = rule.GetFitness(ref spawnInfo);
            //                    if (GetRandomFloat(0f, 1f) < fitness)
            //                    {
            //                        selectedRule = rule;
            //                        selectedFitness = fitness;
            //                    }
            //                    if (fitness > maxFitness)
            //                    {
            //                        fittestRule = rule;
            //                        maxFitness = fitness;
            //                    }
            //                }
            //                //Check to see if we randomly bombed out - if so then choose fittest
            //                if (selectedRule == null)
            //                {
            //                    selectedRule = fittestRule;
            //                    selectedFitness = maxFitness;
            //                }
            //                //We could still bomb, check for this and avoid it
            //                if (selectedRule != null)
            //                {
            //                    spawnInfo.m_fitness = selectedFitness;
            //                    if (TryExecuteRule(ref selectedRule, ref spawnInfo) == true)
            //                    {
            //                        failedSpawns = 0;
            //                        spawnLocation.m_seedDistance = selectedRule.GetSeedThrowRange(ref spawnInfo);
            //                        spawnLocations.Add(spawnLocation);
            //                    }
            //                    else
            //                    {
            //                        failedSpawns++;
            //                    }
            //                }
            //            }
            //        }

            //        //Update progress and yield periodiocally
            //        m_spawnProgress = (float)checks / (float)m_locationChecksPerInt;
            //        float newTime = Time.realtimeSinceStartup;
            //        float stepTime = newTime - currentTime;
            //        currentTime = newTime;
            //        accumulatedTime += stepTime;
            //        if (accumulatedTime > m_updateTimeAllowed)
            //        {
            //            accumulatedTime = 0f;
            //            yield return null;
            //        }

            //        //Check the instance count, exit if necessary
            //        if (!CanSpawnInstances())
            //        {
            //            break;
            //        }

            //        //Check for cancellation
            //        if (m_cancelSpawn)
            //        {
            //            break;
            //        }
            //    }
            //}
            ////Delete spawn caches
            //DeleteSpawnCaches();

            ////Perform final operations
            //PostSpawn();
            yield return null;
        }

        /// <summary>
        /// Run an area spawner iteration
        /// </summary>
        public IEnumerator RunAreaSpawnerIteration()
        {
            if (m_showDebug)
            {
                Debug.Log(string.Format("{0}: Running area iteration", gameObject.name));
            }

            int ruleIdx;
            float fitness, maxFitness, selectedFitness;
            SpawnRule rule, fittestRule, selectedRule;
            SpawnInfo spawnInfo = new SpawnInfo();
            Vector3 location = new Vector3();
            long currChecks, totalChecks;
            float xWUMin, xWUMax, yMid, zWUMin, zWUMax, jitMin, jitMax;
            float xWU, zWU;

            //Set progress
            m_spawnProgress = 0f;
            m_spawnComplete = false;

            //Time control for enumeration
            float currentTime = Time.realtimeSinceStartup;
            float accumulatedTime = 0.0f;

            //Create spawn caches
            CreateSpawnCaches();

            //Load image filter
            LoadImageMask();

            //for (int terrainID = 0; terrainID < Terrain.activeTerrains.Length; terrainID++)
            //{


            //Determine check ranges
            xWUMin = transform.position.x - m_settings.m_spawnRange + (m_locationIncrement / 2f);
            xWUMax = xWUMin + (m_settings.m_spawnRange * 2f);
            yMid = transform.position.y;
            zWUMin = transform.position.z - m_settings.m_spawnRange + (m_locationIncrement / 2f);
            zWUMax = zWUMin + (m_settings.m_spawnRange * 2f);
            jitMin = (-1f * m_maxJitteredLocationOffsetPct) * m_locationIncrement;
            jitMax = (1f * m_maxJitteredLocationOffsetPct) * m_locationIncrement;

            //Update checks
            currChecks = 0;
            totalChecks = (long)(((xWUMax - xWUMin) / m_locationIncrement) * ((zWUMax - zWUMin) / m_locationIncrement));

            //Iterate across these ranges
            for (xWU = xWUMin; xWU < xWUMax; xWU += m_locationIncrement)
            {
                for (zWU = zWUMin; zWU < zWUMax; zWU += m_locationIncrement)
                {
                    currChecks++;

                    //Set the location we want to test
                    location.x = xWU;
                    location.y = yMid;
                    location.z = zWU;

                    //Jitter it
                    if (m_spawnLocationAlgorithm == GaiaConstants.SpawnerLocation.EveryLocationJittered)
                    {
                        location.x += GetRandomFloat(jitMin, jitMax);
                        location.z += GetRandomFloat(jitMin, jitMax);
                    }

                    //Run a ray traced hit check to see what we have hit, use rules to determine fitness and select a rule to spawn
                    if (CheckLocation(location, ref spawnInfo))
                    {


                        //Now perform a rule check based on the selected algorithm

                        //All rules
                        if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.All)
                        {
                            for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
                            {
                                rule = m_settings.m_spawnerRules[ruleIdx];
                                spawnInfo.m_fitness = rule.GetFitness(ref spawnInfo);
                                TryExecuteRule(ref rule, ref spawnInfo);
                            }
                        }

                        //Random spawn rule
                        else if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.Random)
                        {
                            ruleIdx = GetRandomInt(0, m_settings.m_spawnerRules.Count - 1);
                            rule = m_settings.m_spawnerRules[ruleIdx];
                            spawnInfo.m_fitness = rule.GetFitness(ref spawnInfo);
                            TryExecuteRule(ref rule, ref spawnInfo);
                        }

                        //Fittest spawn rule
                        else if (m_spawnRuleSelector == GaiaConstants.SpawnerRuleSelector.Fittest)
                        {
                            fittestRule = null;
                            maxFitness = 0f;
                            for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
                            {
                                rule = m_settings.m_spawnerRules[ruleIdx];
                                fitness = rule.GetFitness(ref spawnInfo);
                                if (fitness > maxFitness)
                                {
                                    maxFitness = fitness;
                                    fittestRule = rule;
                                }
                                else
                                {
                                    //If they are approx equal then give another rule a chance as well to add interest
                                    if (Gaia.GaiaUtils.Math_ApproximatelyEqual(fitness, maxFitness, 0.005f))
                                    {
                                        if (GetRandomFloat(0f, 1f) > 0.5f)
                                        {
                                            maxFitness = fitness;
                                            fittestRule = rule;
                                        }
                                    }
                                }
                            }
                            spawnInfo.m_fitness = maxFitness;
                            TryExecuteRule(ref fittestRule, ref spawnInfo);
                        }

                        //Weighted fittest spawn rule - this implementation will favour fittest
                        else
                        {
                            fittestRule = selectedRule = null;
                            maxFitness = selectedFitness = 0f;
                            for (ruleIdx = 0; ruleIdx < m_settings.m_spawnerRules.Count; ruleIdx++)
                            {
                                rule = m_settings.m_spawnerRules[ruleIdx];
                                fitness = rule.GetFitness(ref spawnInfo);
                                if (GetRandomFloat(0f, 1f) < fitness)
                                {
                                    selectedRule = rule;
                                    selectedFitness = fitness;
                                }
                                if (fitness > maxFitness)
                                {
                                    fittestRule = rule;
                                    maxFitness = fitness;
                                }
                            }
                            //Check to see if we randomly bombed out - if so then choose fittest
                            if (selectedRule == null)
                            {
                                selectedRule = fittestRule;
                                selectedFitness = maxFitness;
                            }
                            //We could still bomb, check for this and avoid it
                            if (selectedRule != null)
                            {
                                spawnInfo.m_fitness = selectedFitness;
                                TryExecuteRule(ref selectedRule, ref spawnInfo);
                            }
                        }

                        //If it caused textures to be updated then apply them
                        if (m_textureMapsDirty)
                        {
                            List<HeightMap> txtMaps = spawnInfo.m_spawner.GetTextureMaps(spawnInfo.m_hitTerrain.GetInstanceID());
                            if (txtMaps != null)
                            {
                                for (int idx = 0; idx < spawnInfo.m_textureStrengths.Length; idx++)
                                {
                                    //if ((int)spawnInfo.m_hitLocationWU.z == 1023)
                                    //{
                                    //    Debug.Log("Woopee");
                                    //}

                                    txtMaps[idx][spawnInfo.m_hitLocationNU.z, spawnInfo.m_hitLocationNU.x] = spawnInfo.m_textureStrengths[idx];
                                }
                            }
                        }

                    }

                    //Update progress and yield periodiocally
                    m_spawnProgress = (float)currChecks / (float)totalChecks;
                    float newTime = Time.realtimeSinceStartup;
                    float stepTime = newTime - currentTime;
                    currentTime = newTime;
                    accumulatedTime += stepTime;
                    if (accumulatedTime > m_updateTimeAllowed)
                    {
                        accumulatedTime = 0f;
                        yield return null;
                    }

                    //Check the instance count, exit if necessary
                    if (!CanSpawnInstances())
                    {
                        break;
                    }

                    //Check for cancelation
                    if (m_cancelSpawn == true)
                    {
                        break;
                    }
                }
            }
            //}
            //Determine whether or not we need to delete and apply spawn caches
            DeleteSpawnCaches(true);

            //Perform final operations
            PostSpawn();
        }

        /// <summary>
        /// Ground the spawner to the terrain
        /// </summary>
        public void GroundToTerrain()
        {
            Terrain t = Gaia.TerrainHelper.GetTerrain(transform.position);
            if (t == null)
            {
                t = Terrain.activeTerrain;
            }
            if (t == null)
            {
                Debug.LogError("Could not fit to terrain - no terrain present");
                return;
            }

            Bounds b = new Bounds();
            if (TerrainHelper.GetTerrainBounds(t, ref b))
            {
                transform.position = new Vector3(transform.position.x, t.transform.position.y, transform.position.z);
            }
        }

        /// <summary>
        /// Position and fit the spawner to the terrain
        /// </summary>
        public void FitToTerrain(Terrain t = null)
        {
            if (t == null)
            {
                t = Gaia.TerrainHelper.GetTerrain(transform.position, m_settings.m_isWorldmapSpawner);
                if (t == null)
                {
                    t = Terrain.activeTerrain;
                }
                if (t == null)
                {
                    Debug.LogWarning("Could not fit to terrain - no terrain present");
                    return;
                }
            }

            Bounds b = new Bounds();
            if (TerrainHelper.GetTerrainBounds(t, ref b))
            {
                transform.position = new Vector3(b.center.x, t.transform.position.y, b.center.z);
                m_settings.m_spawnRange = b.extents.x;
            }
        }


        /// <summary>
        /// Position and fit the spawner to the terrain
        /// </summary>
        public void FitToAllTerrains()
        {
            Terrain currentTerrain = GetCurrentTerrain();

            if (currentTerrain == null)
            {
                Debug.LogError("Could not fit to terrain - no active terrain present");
                return;
            }

            BoundsDouble b = new Bounds();
            if (TerrainHelper.GetTerrainBounds(ref b))
            {
                transform.position = b.center;
                m_settings.m_spawnRange = (float)b.extents.x;
            }
        }

        /// <summary>
        /// Check if the spawner has been fit to the terrain - ignoring height
        /// </summary>
        /// <returns>True if its a match</returns>
        public bool IsFitToTerrain()
        {
            Terrain t = Gaia.TerrainHelper.GetTerrain(transform.position);
            if (t == null)
            {
                t = Terrain.activeTerrain;
            }
            if (t == null)
            {
                Debug.LogError("Could not check if fit to terrain - no terrain present");
                return false;
            }

            Bounds b = new Bounds();
            if (TerrainHelper.GetTerrainBounds(t, ref b))
            {
                if (
                    b.center.x != transform.position.x ||
                    b.center.z != transform.position.z ||
                    b.extents.x != m_settings.m_spawnRange)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Load the image mask if one was specified
        /// </summary>
        public bool LoadImageMask()
        {
            //Kill old image height map
            m_imageMaskHM = null;

            //Check mode & exit 
            if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.None || m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.PerlinNoise)
            {
                return false;
            }

            //Load the supplied image
            if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.ImageRedChannel || m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.ImageGreenChannel ||
                m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.ImageBlueChannel || m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.ImageAlphaChannel ||
                m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.ImageGreyScale)
            {
                if (m_imageMask == null)
                {
                    Debug.LogError("You requested an image mask but did not supply one. Please select mask texture.");
                    return false;
                }

                //Check the image rw
                Gaia.GaiaUtils.MakeTextureReadable(m_imageMask);

                //Make it uncompressed
                Gaia.GaiaUtils.MakeTextureUncompressed(m_imageMask);

                //Load the image
                m_imageMaskHM = new HeightMap(m_imageMask.width, m_imageMask.height);
                for (int x = 0; x < m_imageMaskHM.Width(); x++)
                {
                    for (int z = 0; z < m_imageMaskHM.Depth(); z++)
                    {
                        switch (m_areaMaskMode)
                        {
                            case GaiaConstants.ImageFitnessFilterMode.ImageGreyScale:
                                m_imageMaskHM[x, z] = m_imageMask.GetPixel(x, z).grayscale;
                                break;
                            case GaiaConstants.ImageFitnessFilterMode.ImageRedChannel:
                                m_imageMaskHM[x, z] = m_imageMask.GetPixel(x, z).r;
                                break;
                            case GaiaConstants.ImageFitnessFilterMode.ImageGreenChannel:
                                m_imageMaskHM[x, z] = m_imageMask.GetPixel(x, z).g;
                                break;
                            case GaiaConstants.ImageFitnessFilterMode.ImageBlueChannel:
                                m_imageMaskHM[x, z] = m_imageMask.GetPixel(x, z).b;
                                break;
                            case GaiaConstants.ImageFitnessFilterMode.ImageAlphaChannel:
                                m_imageMaskHM[x, z] = m_imageMask.GetPixel(x, z).a;
                                break;
                        }
                    }
                }
            }
            else
            {
                //Or get a new one
                if (Terrain.activeTerrain == null)
                {
                    Debug.LogError("You requested an terrain texture mask but there is no active terrain.");
                    return false;
                }


                Terrain t = Terrain.activeTerrain;
                var splatPrototypes = GaiaSplatPrototype.GetGaiaSplatPrototypes(t);


                switch (m_areaMaskMode)
                {
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture0:
                        if (splatPrototypes.Length < 1)
                        {
                            Debug.LogError("You requested an terrain texture mask 0 but there is no active texture in slot 0.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 0);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture1:
                        if (splatPrototypes.Length < 2)
                        {
                            Debug.LogError("You requested an terrain texture mask 1 but there is no active texture in slot 1.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 1);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture2:
                        if (splatPrototypes.Length < 3)
                        {
                            Debug.LogError("You requested an terrain texture mask 2 but there is no active texture in slot 2.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 2);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture3:
                        if (splatPrototypes.Length < 4)
                        {
                            Debug.LogError("You requested an terrain texture mask 3 but there is no active texture in slot 3.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 3);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture4:
                        if (splatPrototypes.Length < 5)
                        {
                            Debug.LogError("You requested an terrain texture mask 4 but there is no active texture in slot 4.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 4);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture5:
                        if (splatPrototypes.Length < 6)
                        {
                            Debug.LogError("You requested an terrain texture mask 5 but there is no active texture in slot 5.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 5);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture6:
                        if (splatPrototypes.Length < 7)
                        {
                            Debug.LogError("You requested an terrain texture mask 6 but there is no active texture in slot 6.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 6);
                        break;
                    case GaiaConstants.ImageFitnessFilterMode.TerrainTexture7:
                        if (splatPrototypes.Length < 8)
                        {
                            Debug.LogError("You requested an terrain texture mask 7 but there is no active texture in slot 7.");
                            return false;
                        }
                        m_imageMaskHM = new HeightMap(t.terrainData.GetAlphamaps(0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight), 7);
                        break;
                }

                //It came from terrain so flip it
                m_imageMaskHM.Flip();
            }

            //Because images are noisy, smooth it
            if (m_imageMaskSmoothIterations > 0)
            {
                m_imageMaskHM.Smooth(m_imageMaskSmoothIterations);
            }

            //Flip it
            if (m_imageMaskFlip == true)
            {
                m_imageMaskHM.Flip();
            }

            //Normalise it if necessary
            if (m_imageMaskNormalise == true)
            {
                m_imageMaskHM.Normalise();
            }

            //Invert it if necessessary
            if (m_imageMaskInvert == true)
            {
                m_imageMaskHM.Invert();
            }

            return true;
        }

        /// <summary>
        /// Starts the spawner, either in a local area or across all terrains ("world spawn")
        /// </summary>
        /// <param name="allTerrains">Whether the spawner should spawn across all terrains in the scene.</param>
        public void Spawn(bool allTerrains)
        {
            m_spawnComplete = false;
            BoundsDouble spawnArea = new BoundsDouble();

            if (allTerrains)
            {
                TerrainHelper.GetTerrainBounds(ref spawnArea);
            }
            else
            {
                spawnArea.center = transform.position + (Vector3)TerrainLoaderManager.Instance.GetOrigin();
                spawnArea.size = new Vector3(m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f);
            }

            try
            {
                GenerateNewRandomSeed();
                SpawnOperationSettings soSettings = ScriptableObject.CreateInstance<SpawnOperationSettings>();
                soSettings.m_spawnerSettingsList = new List<SpawnerSettings>() { m_settings };
                soSettings.m_spawnArea = spawnArea;
                soSettings.m_isWorldMapSpawner = m_settings.m_isWorldmapSpawner;
                GaiaSessionManager.Spawn(soSettings, true, new List<Spawner>() { this });
            }
            catch (Exception ex)
            {
                Debug.LogError("Spawner " + this.name + " failed with Exception: " + ex.Message + "\n\n" + "Stack trace: \n\n" + ex.StackTrace);
                ProgressBar.Clear(ProgressBarPriority.Spawning);
            }
            m_spawnComplete = true;
        }

        /// <summary>
        /// Generates a new random seed for the spawner (if generation of a new seed is activated)
        /// </summary>
        ///<param name="force"/>Force the generation of a new random seed even if deactivated in the spawner</param>
        /// </summary>
        public void GenerateNewRandomSeed(bool force = false)
        {
            if (m_settings.m_generateRandomSeed || force)
            {
                m_settings.m_randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
            }
        }

        /// <summary>
        /// Create spawn caches
        /// </summary>
        /// <param name="checkResources">Base on resources or base on rules, takes active state into account</param>
        public void CreateSpawnCaches()
        {
            //Determine whether or not we need to cache updates, in which case we needs to get the relevant caches
            int idx;
            m_cacheTextures = false;
            m_textureMapsDirty = false;
            for (idx = 0; idx < m_settings.m_spawnerRules.Count; idx++)
            {
                if (m_settings.m_spawnerRules[idx].CacheTextures(this))
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        CacheTextureMapsFromTerrain(t.GetInstanceID());
                    }
                    m_cacheTextures = true;
                    break;
                }
            }

            m_cacheDetails = false;
            for (idx = 0; idx < m_settings.m_spawnerRules.Count; idx++)
            {
                if (m_settings.m_spawnerRules[idx].CacheDetails(this))
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        CacheDetailMapsFromTerrain(t.GetInstanceID());
                    }
                    m_cacheDetails = true;
                    break;
                }
            }

            CacheTreesFromTerrain();

            m_cacheTags = false;
            List<string> tagList = new List<string>();
            for (idx = 0; idx < m_settings.m_spawnerRules.Count; idx++)
            {
                m_settings.m_spawnerRules[idx].AddProximityTags(this, ref tagList);
            }
            if (tagList.Count > 0)
            {
                CacheTaggedGameObjectsFromScene(tagList);
                m_cacheTags = true;
            }

            m_cacheHeightMaps = false;
            for (idx = 0; idx < m_settings.m_spawnerRules.Count; idx++)
            {
                if (m_settings.m_spawnerRules[idx].CacheHeightMaps(this))
                {
                    CacheHeightMapFromTerrain(Terrain.activeTerrain.GetInstanceID());
                    m_cacheHeightMaps = true;
                    break;
                }
            }

            /*
            m_cacheStamps = false;
            List<string> stampList = new List<string>();
            for (idx = 0; idx < m_spawnerRules.Count; idx++)
            {
                m_spawnerRules[idx].AddStamps(this, ref stampList);
            }
            if (stampList.Count > 0)
            {
                CacheStamps(stampList);
                m_cacheStamps = true;
            } */
        }

        /// <summary>
        /// Create spawn cache fore specific resources
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resourceIdx"></param>
        public void CreateSpawnCaches(Gaia.GaiaConstants.SpawnerResourceType resourceType, int resourceIdx)
        {
            m_cacheTextures = false;
            m_textureMapsDirty = false;
            m_cacheDetails = false;
            m_cacheTags = false;

            switch (resourceType)
            {
                case GaiaConstants.SpawnerResourceType.TerrainTexture:
                    {
                        //Check indexes
                        if (resourceIdx >= m_settings.m_resources.m_texturePrototypes.Length)
                        {
                            break;
                        }

                        //If we are working with textures, then always cache the texture
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            CacheTextureMapsFromTerrain(t.GetInstanceID());
                        }
                        m_cacheTextures = true;

                        //Check for proximity tags
                        List<string> tagList = new List<string>();
                        m_settings.m_resources.m_texturePrototypes[resourceIdx].AddTags(ref tagList);
                        if (tagList.Count > 0)
                        {
                            CacheTaggedGameObjectsFromScene(tagList);
                            m_cacheTags = true;
                        }

                        break;
                    }
                case GaiaConstants.SpawnerResourceType.TerrainDetail:
                    {
                        //Check indexes
                        if (resourceIdx >= m_settings.m_resources.m_detailPrototypes.Length)
                        {
                            break;
                        }

                        //If we are working with details, always cache details
                        foreach (Terrain t in Terrain.activeTerrains)
                        {
                            CacheDetailMapsFromTerrain(t.GetInstanceID());
                        }
                        m_cacheDetails = true;

                        //Check for textures
                        if (m_settings.m_resources.m_detailPrototypes[resourceIdx].ChecksTextures())
                        {
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                CacheTextureMapsFromTerrain(t.GetInstanceID());
                            }
                            m_cacheTextures = true;
                        }

                        //Check for proximity tags
                        List<string> tagList = new List<string>();
                        m_settings.m_resources.m_detailPrototypes[resourceIdx].AddTags(ref tagList);
                        if (tagList.Count > 0)
                        {
                            CacheTaggedGameObjectsFromScene(tagList);
                            m_cacheTags = true;
                        }

                        break;
                    }
                case GaiaConstants.SpawnerResourceType.TerrainTree:
                    {
                        //Check indexes
                        if (resourceIdx >= m_settings.m_resources.m_treePrototypes.Length)
                        {
                            break;
                        }

                        //Cache textures
                        if (m_settings.m_resources.m_treePrototypes[resourceIdx].ChecksTextures())
                        {
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                CacheTextureMapsFromTerrain(t.GetInstanceID());
                            }
                            m_cacheTextures = true;
                        }

                        //Cache trees
                        CacheTreesFromTerrain();

                        //Cache proximity tags
                        List<string> tagList = new List<string>();
                        m_settings.m_resources.m_treePrototypes[resourceIdx].AddTags(ref tagList);
                        if (tagList.Count > 0)
                        {
                            CacheTaggedGameObjectsFromScene(tagList);
                            m_cacheTags = true;
                        }

                        break;
                    }
                case GaiaConstants.SpawnerResourceType.GameObject:
                    {
                        //Check indexes
                        if (resourceIdx >= m_settings.m_resources.m_gameObjectPrototypes.Length)
                        {
                            break;
                        }

                        //Check for textures
                        if (m_settings.m_resources.m_gameObjectPrototypes[resourceIdx].ChecksTextures())
                        {
                            foreach (Terrain t in Terrain.activeTerrains)
                            {
                                CacheTextureMapsFromTerrain(t.GetInstanceID());
                            }
                            m_cacheTextures = true;
                        }

                        //Check for proximity tags
                        List<string> tagList = new List<string>();
                        m_settings.m_resources.m_gameObjectPrototypes[resourceIdx].AddTags(ref tagList);
                        if (tagList.Count > 0)
                        {
                            CacheTaggedGameObjectsFromScene(tagList);
                            m_cacheTags = true;
                        }

                        break;
                    }

                    /*
                default:
                    {
                        //Check indexes
                        if (resourceIdx >= m_resources.m_stampPrototypes.Length)
                        {
                            break;
                        }

                        //Check for textures
                        if (m_resources.m_stampPrototypes[resourceIdx].ChecksTextures())
                        {
                            CacheTextureMapsFromTerrain(Terrain.activeTerrain.GetInstanceID());
                            m_cacheTextures = true;
                        }

                        //Check for proximity tags
                        List<string> tagList = new List<string>();
                        m_resources.m_gameObjectPrototypes[resourceIdx].AddTags(ref tagList);
                        if (tagList.Count > 0)
                        {
                            CacheTaggedGameObjectsFromScene(tagList);
                            m_cacheTags = true;
                        }

                        //We are influencing terrain - so we always cache terrain
                        CacheHeightMapFromTerrain(Terrain.activeTerrain.GetInstanceID());
                        m_cacheHeightMaps = true;

                        break;
                    }
                     */
            }
        }


        /// <summary>
        /// Destroy spawn caches
        /// </summary>
        /// <param name="flush">Fluch changes back to the environment</param>
        public void DeleteSpawnCaches(bool flushDirty = false)
        {
            //Determine whether or not we need to apply cache updates
            if (m_cacheTextures)
            {
                if (flushDirty && m_textureMapsDirty && m_cancelSpawn != true)
                {
                    m_textureMapsDirty = false;
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        SaveTextureMapsToTerrain(t.GetInstanceID());
                    }
                }
                DeleteTextureMapCache();
                m_cacheTextures = false;
            }

            if (m_cacheDetails)
            {
                if (m_cancelSpawn != true)
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        SaveDetailMapsToTerrain(t.GetInstanceID());
                    }
                }
                DeleteDetailMapCache();
                m_cacheDetails = false;
            }

            if (m_cacheTags)
            {
                DeleteTagCache();
                m_cacheTags = false;
            }

            if (m_cacheHeightMaps)
            {
                if (flushDirty && m_heightMapDirty && m_cancelSpawn != true)
                {
                    m_heightMapDirty = false;
                    SaveHeightMapToTerrain(Terrain.activeTerrain.GetInstanceID());
                }
                DeleteHeightMapCache();
                m_cacheHeightMaps = false;
            }
        }

        /// <summary>
        /// Attempt to execute a rule taking fitness, failure rate and instances into account
        /// </summary>
        /// <param name="rule">The rule to execute</param>
        /// <param name="spawnInfo">The related spawninfo</param>
        public bool TryExecuteRule(ref SpawnRule rule, ref SpawnInfo spawnInfo)
        {
            //Check null
            if (rule != null)
            {
                //Check instances
                if (rule.m_ignoreMaxInstances || (rule.m_activeInstanceCnt < rule.m_maxInstances))
                {
                    //Update fitness based on distance evaluation
                    spawnInfo.m_fitness *= m_spawnFitnessAttenuator.Evaluate(Mathf.Clamp01(spawnInfo.m_hitDistanceWU / m_settings.m_spawnRange));

                    //Udpate fitness based on area mask 
                    if (m_areaMaskMode != GaiaConstants.ImageFitnessFilterMode.None)
                    {
                        if (m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.PerlinNoise ||
                            m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.BillowNoise ||
                            m_areaMaskMode == GaiaConstants.ImageFitnessFilterMode.RidgedNoise)
                        {
                            if (!m_noiseInvert)
                            {
                                spawnInfo.m_fitness *= m_noiseGenerator.GetNormalisedValue(100000f + (spawnInfo.m_hitLocationWU.x * (1f / m_noiseZoom)), 100000f + (spawnInfo.m_hitLocationWU.z * (1f / m_noiseZoom)));
                            }
                            else
                            {
                                spawnInfo.m_fitness *= (1f - m_noiseGenerator.GetNormalisedValue(100000f + (spawnInfo.m_hitLocationWU.x * (1f / m_noiseZoom)), 100000f + (spawnInfo.m_hitLocationWU.z * (1f / m_noiseZoom))));
                            }
                        }
                        else
                        {
                            if (m_imageMaskHM.HasData())
                            {
                                float x = (spawnInfo.m_hitLocationWU.x - (transform.position.x - m_settings.m_spawnRange)) / (m_settings.m_spawnRange * 2f);
                                float z = (spawnInfo.m_hitLocationWU.z - (transform.position.z - m_settings.m_spawnRange)) / (m_settings.m_spawnRange * 2f);
                                spawnInfo.m_fitness *= m_imageMaskHM[x, z];
                            }
                        }
                    }

                    //Check fitness
                    if (spawnInfo.m_fitness > rule.m_minRequiredFitness)
                    {
                        //Only spawn if we pass a random failure check
                        if (GetRandomFloat(0f, 1f) > rule.m_failureRate)
                        {
                            rule.Spawn(ref spawnInfo);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This is a fairly expensive raycast based location check that is capable of detecting things like tree collider hits on the terrain.
        /// It will return the name and height of the thing that was hit, plus some underlying terrain information. In the scenario of terrain tree
        /// hits you can comparing height of the rtaycast hit against the height of the terrain to detect this.
        /// It will return true plus details if something is hit, otherwise false.
        /// </summary>
        /// <param name="locationWU">The location we are checking in world units</param>
        /// <param name="spawnInfo">The information we gather about this location</param>
        /// <returns>True if we hit something, false otherwise</returns>
        public bool CheckLocation(Vector3 locationWU, ref SpawnInfo spawnInfo)
        {
            //Some initialisation
            spawnInfo.m_spawner = this;
            spawnInfo.m_outOfBounds = true;
            spawnInfo.m_wasVirginTerrain = false;
            spawnInfo.m_spawnRotationY = 0f;
            spawnInfo.m_hitDistanceWU = Vector3.Distance(transform.position, locationWU);
            spawnInfo.m_hitLocationWU = locationWU;
            spawnInfo.m_hitNormal = Vector3.zero;
            spawnInfo.m_hitObject = null;
            spawnInfo.m_hitTerrain = null;
            spawnInfo.m_terrainNormalWU = Vector3.one;
            spawnInfo.m_terrainHeightWU = 0f;
            spawnInfo.m_terrainSlopeWU = 0f;
            spawnInfo.m_areaHitSlopeWU = 0f;
            spawnInfo.m_areaMinSlopeWU = 0f;
            spawnInfo.m_areaAvgSlopeWU = 0f;
            spawnInfo.m_areaMaxSlopeWU = 0f;

            //Make sure we are above it
            locationWU.y = m_terrainHeight + 1000f;

            //Run a ray traced hit check to see what we have hit - if we dont get a hit then we are off terrain and will ignore
            if (Physics.Raycast(locationWU, Vector3.down, out m_checkHitInfo, Mathf.Infinity, m_spawnCollisionLayers))
            {
                //If its a grass spawner, and we got a sphere collider, try again so that we ignore the sphere collider
                if (spawnInfo.m_spawner.IsDetailSpawner())
                {
                    if ((m_checkHitInfo.collider is SphereCollider || m_checkHitInfo.collider is CapsuleCollider) && m_checkHitInfo.collider.name == "_GaiaCollider_Grass")
                    {
                        //Drop it slightly and run it again
                        locationWU.y = m_checkHitInfo.point.y - 0.01f;

                        //Run the raycast again - it should hit something
                        if (!Physics.Raycast(locationWU, Vector3.down, out m_checkHitInfo, Mathf.Infinity, m_spawnCollisionLayers))
                        {
                            return false;
                        }
                    }
                }


                //Update spawnInfo
                spawnInfo.m_hitLocationWU = m_checkHitInfo.point;
                spawnInfo.m_hitDistanceWU = Vector3.Distance(transform.position, spawnInfo.m_hitLocationWU);
                spawnInfo.m_hitNormal = m_checkHitInfo.normal;
                spawnInfo.m_hitObject = m_checkHitInfo.transform;

                //Check distance - bomb out if out of range
                if (m_spawnerShape == GaiaConstants.SpawnerShape.Box)
                {
                    if (!m_spawnerBounds.Contains(spawnInfo.m_hitLocationWU))
                    {
                        return false;
                    }
                }
                else
                {
                    if (spawnInfo.m_hitDistanceWU > m_settings.m_spawnRange)
                    {
                        return false;
                    }
                }
                spawnInfo.m_outOfBounds = false;

                //Gather some terrain info at this location
                Terrain terrain;
                if (m_checkHitInfo.collider is TerrainCollider)
                {
                    terrain = m_checkHitInfo.transform.GetComponent<Terrain>();
                    spawnInfo.m_wasVirginTerrain = true; //It might be virgin terrain
                }
                else
                {
                    terrain = Gaia.TerrainHelper.GetTerrain(m_checkHitInfo.point);
                }

                if (terrain != null)
                {
                    spawnInfo.m_hitTerrain = terrain;
                    spawnInfo.m_terrainHeightWU = terrain.SampleHeight(m_checkHitInfo.point);
                    Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(m_checkHitInfo.point);
                    Vector3 normalizedPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, terrainLocalPos.x),
                                                        Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, terrainLocalPos.y),
                                                        Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, terrainLocalPos.z));
                    spawnInfo.m_hitLocationNU = normalizedPos;
                    spawnInfo.m_terrainSlopeWU = terrain.terrainData.GetSteepness(normalizedPos.x, normalizedPos.z);
                    spawnInfo.m_areaHitSlopeWU = spawnInfo.m_areaMinSlopeWU = spawnInfo.m_areaAvgSlopeWU = spawnInfo.m_areaMaxSlopeWU = spawnInfo.m_terrainSlopeWU;
                    spawnInfo.m_terrainNormalWU = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.z);

                    //Check for virgin terrain now that we know actual terrain height - difference will be tree colliders
                    if (spawnInfo.m_wasVirginTerrain == true)
                    {
                        //Use the tree manager to do hits on trees
                        if (spawnInfo.m_spawner.m_treeCache.Count(spawnInfo.m_hitLocationWU, 0.5f) > 0)
                        {
                            spawnInfo.m_wasVirginTerrain = false;
                        }
                    }

                    //Set up the texture layer array in spawn info
                    spawnInfo.m_textureStrengths = new float[spawnInfo.m_hitTerrain.terrainData.alphamapLayers];

                    //Grab the textures
                    if (m_textureMapCache != null && m_textureMapCache.Count > 0)
                    {
                        List<HeightMap> hms = m_textureMapCache[terrain.GetInstanceID()];
                        for (int i = 0; i < spawnInfo.m_textureStrengths.Length; i++)
                        {
                            spawnInfo.m_textureStrengths[i] = hms[i][normalizedPos.z, normalizedPos.x];
                        }
                    }
                    else
                    {
                        float[,,] hms = terrain.terrainData.GetAlphamaps((int)(normalizedPos.x * (float)(terrain.terrainData.alphamapWidth - 1)), (int)(normalizedPos.z * (float)(terrain.terrainData.alphamapHeight - 1)), 1, 1);
                        for (int i = 0; i < spawnInfo.m_textureStrengths.Length; i++)
                        {
                            spawnInfo.m_textureStrengths[i] = hms[0, 0, i];
                        }
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// This will do a bounded location check in order to calculate bounded slopes and checkd for bounded collisions
        /// </summary>
        /// <param name="spawnInfo"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool CheckLocationBounds(ref SpawnInfo spawnInfo, float distance)
        {
            //Initialise
            spawnInfo.m_areaHitSlopeWU = spawnInfo.m_areaMinSlopeWU = spawnInfo.m_areaAvgSlopeWU = spawnInfo.m_areaMaxSlopeWU = spawnInfo.m_terrainSlopeWU;
            if (spawnInfo.m_areaHitsWU == null)
            {
                spawnInfo.m_areaHitsWU = new Vector3[4];
            }
            spawnInfo.m_areaHitsWU[0] = new Vector3(spawnInfo.m_hitLocationWU.x + distance, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z);
            spawnInfo.m_areaHitsWU[1] = new Vector3(spawnInfo.m_hitLocationWU.x - distance, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z);
            spawnInfo.m_areaHitsWU[2] = new Vector3(spawnInfo.m_hitLocationWU.x, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z + distance);
            spawnInfo.m_areaHitsWU[3] = new Vector3(spawnInfo.m_hitLocationWU.x, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z - distance);

            //Run ray traced hits to check the lay of the land - if we dont get a hit then we are off terrain and will fail
            RaycastHit hit;

            //First check the main volume under the original position for non terrain related hits
            Vector3 extents = new Vector3(distance, 0.1f, distance);
            if (!Physics.BoxCast(new Vector3(spawnInfo.m_hitLocationWU.x, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z), extents, Vector3.down, out hit, Quaternion.identity, Mathf.Infinity, m_spawnCollisionLayers))
            //if (!Physics.SphereCast(new Vector3(spawnInfo.m_hitLocationWU.x, spawnInfo.m_hitLocationWU.y + 3000f, spawnInfo.m_hitLocationWU.z), distance, Vector3.down, out hit, Mathf.Infinity, m_spawnCollisionLayers))
            {
                return false;
            }

            //Test virginity
            if (spawnInfo.m_wasVirginTerrain == true)
            {
                if (hit.collider is TerrainCollider)
                {
                    //Use the tree manager to do hits on trees
                    if (spawnInfo.m_spawner.m_treeCache.Count(hit.point, 0.5f) > 0)
                    {
                        spawnInfo.m_wasVirginTerrain = false;
                    }
                }
                else
                {
                    spawnInfo.m_wasVirginTerrain = false;
                }
            }

            //Now test the first corner
            if (!Physics.Raycast(spawnInfo.m_areaHitsWU[0], Vector3.down, out hit, Mathf.Infinity, m_spawnCollisionLayers))
            {
                return false;
            }

            //Update hit location
            spawnInfo.m_areaHitsWU[0] = hit.point;

            //Update slope calculations
            Terrain terrain = hit.transform.GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = Gaia.TerrainHelper.GetTerrain(hit.point);
            }
            Vector3 localPos = Vector3.zero;
            Vector3 normPos = Vector3.zero;
            //float terrainHeight = 0f;
            float terrainSlope = 0f;

            if (terrain != null)
            {
                //terrainHeight = terrain.SampleHeight(hit.point);
                localPos = terrain.transform.InverseTransformPoint(hit.point);
                normPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, localPos.x),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, localPos.y),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, localPos.z));
                terrainSlope = terrain.terrainData.GetSteepness(normPos.x, normPos.z);
                spawnInfo.m_areaAvgSlopeWU += terrainSlope;
                if (terrainSlope > spawnInfo.m_areaMaxSlopeWU)
                {
                    spawnInfo.m_areaMaxSlopeWU = terrainSlope;
                }
                if (terrainSlope < spawnInfo.m_areaMinSlopeWU)
                {
                    spawnInfo.m_areaMinSlopeWU = terrainSlope;
                }

                //Check for virginity
                if (spawnInfo.m_wasVirginTerrain == true)
                {
                    if (hit.collider is TerrainCollider)
                    {
                        if (spawnInfo.m_spawner.m_treeCache.Count(hit.point, 0.5f) > 0)
                        {
                            spawnInfo.m_wasVirginTerrain = false;
                        }
                    }
                    else
                    {
                        spawnInfo.m_wasVirginTerrain = false;
                    }
                }
            }

            //Now test the next corner
            if (!Physics.Raycast(spawnInfo.m_areaHitsWU[1], Vector3.down, out hit, Mathf.Infinity, m_spawnCollisionLayers))
            {
                return false;
            }

            //Update hit location
            spawnInfo.m_areaHitsWU[1] = hit.point;

            //Update slope calculations
            terrain = hit.transform.GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = Gaia.TerrainHelper.GetTerrain(hit.point);
            }
            if (terrain != null)
            {
                //terrainHeight = terrain.SampleHeight(hit.point);
                localPos = terrain.transform.InverseTransformPoint(hit.point);
                normPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, localPos.x),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, localPos.y),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, localPos.z));
                terrainSlope = terrain.terrainData.GetSteepness(normPos.x, normPos.z);
                spawnInfo.m_areaAvgSlopeWU += terrainSlope;
                if (terrainSlope > spawnInfo.m_areaMaxSlopeWU)
                {
                    spawnInfo.m_areaMaxSlopeWU = terrainSlope;
                }
                if (terrainSlope < spawnInfo.m_areaMinSlopeWU)
                {
                    spawnInfo.m_areaMinSlopeWU = terrainSlope;
                }

                //Check for virginity
                if (spawnInfo.m_wasVirginTerrain == true)
                {
                    if (hit.collider is TerrainCollider)
                    {
                        if (spawnInfo.m_spawner.m_treeCache.Count(hit.point, 0.5f) > 0)
                        {
                            spawnInfo.m_wasVirginTerrain = false;
                        }
                    }
                    else
                    {
                        spawnInfo.m_wasVirginTerrain = false;
                    }
                }
            }

            //Now test the next corner
            if (!Physics.Raycast(spawnInfo.m_areaHitsWU[2], Vector3.down, out hit, Mathf.Infinity, m_spawnCollisionLayers))
            {
                return false;
            }

            //Update hit location
            spawnInfo.m_areaHitsWU[2] = hit.point;

            //Update slope calculations
            terrain = hit.transform.GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = Gaia.TerrainHelper.GetTerrain(hit.point);
            }
            if (terrain != null)
            {
                //terrainHeight = terrain.SampleHeight(hit.point);
                localPos = terrain.transform.InverseTransformPoint(hit.point);
                normPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, localPos.x),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, localPos.y),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, localPos.z));
                terrainSlope = terrain.terrainData.GetSteepness(normPos.x, normPos.z);
                spawnInfo.m_areaAvgSlopeWU += terrainSlope;
                if (terrainSlope > spawnInfo.m_areaMaxSlopeWU)
                {
                    spawnInfo.m_areaMaxSlopeWU = terrainSlope;
                }
                if (terrainSlope < spawnInfo.m_areaMinSlopeWU)
                {
                    spawnInfo.m_areaMinSlopeWU = terrainSlope;
                }

                //Check for virginity
                if (spawnInfo.m_wasVirginTerrain == true)
                {
                    if (hit.collider is TerrainCollider)
                    {
                        if (spawnInfo.m_spawner.m_treeCache.Count(hit.point, 0.5f) > 0)
                        {
                            spawnInfo.m_wasVirginTerrain = false;
                        }
                    }
                    else
                    {
                        spawnInfo.m_wasVirginTerrain = false;
                    }
                }
            }

            //Now test the next corner
            if (!Physics.Raycast(spawnInfo.m_areaHitsWU[3], Vector3.down, out hit, Mathf.Infinity, m_spawnCollisionLayers))
            {
                return false;
            }

            //Update hit location
            spawnInfo.m_areaHitsWU[3] = hit.point;

            //Update slope calculations
            terrain = hit.transform.GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = Gaia.TerrainHelper.GetTerrain(hit.point);
            }
            if (terrain != null)
            {
                //terrainHeight = terrain.SampleHeight(hit.point);
                localPos = terrain.transform.InverseTransformPoint(hit.point);
                normPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, localPos.x),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, localPos.y),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, localPos.z));
                terrainSlope = terrain.terrainData.GetSteepness(normPos.x, normPos.z);
                spawnInfo.m_areaAvgSlopeWU += terrainSlope;
                if (terrainSlope > spawnInfo.m_areaMaxSlopeWU)
                {
                    spawnInfo.m_areaMaxSlopeWU = terrainSlope;
                }
                if (terrainSlope < spawnInfo.m_areaMinSlopeWU)
                {
                    spawnInfo.m_areaMinSlopeWU = terrainSlope;
                }

                //Check for virginity
                if (spawnInfo.m_wasVirginTerrain == true)
                {
                    if (hit.collider is TerrainCollider)
                    {
                        if (spawnInfo.m_spawner.m_treeCache.Count(hit.point, 0.5f) > 0)
                        {
                            spawnInfo.m_wasVirginTerrain = false;
                        }
                    }
                    else
                    {
                        spawnInfo.m_wasVirginTerrain = false;
                    }
                }
            }

            //Now update the slopes and spawninfo
            spawnInfo.m_areaAvgSlopeWU = spawnInfo.m_areaAvgSlopeWU / 5f;
            float dx = spawnInfo.m_areaHitsWU[0].y - spawnInfo.m_areaHitsWU[1].y;
            float dz = spawnInfo.m_areaHitsWU[2].y - spawnInfo.m_areaHitsWU[3].y;
            spawnInfo.m_areaHitSlopeWU = Gaia.GaiaUtils.Math_Clamp(0f, 90f, (float)(Math.Sqrt((dx * dx) + (dz * dz))));

            return true;
        }



        /// <summary>
        /// Update statistics counters
        /// </summary>
        public void UpdateCounters()
        {
            //m_totalRuleCnt = 0;
            //m_activeRuleCnt = 0;
            //m_inactiveRuleCnt = 0;
            //m_maxInstanceCnt = 0;
            //m_activeInstanceCnt = 0;
            //m_inactiveInstanceCnt = 0;
            //m_totalInstanceCnt = 0;

            //foreach (SpawnRule rule in m_settings.m_spawnerRules)
            //{
            //    m_totalRuleCnt++;
            //    if (rule.m_isActive)
            //    {
            //        m_activeRuleCnt++;
            //        m_maxInstanceCnt += rule.m_maxInstances;
            //        m_activeInstanceCnt += rule.m_activeInstanceCnt;
            //        m_inactiveInstanceCnt += rule.m_inactiveInstanceCnt;
            //        m_totalInstanceCnt += (rule.m_activeInstanceCnt + rule.m_inactiveInstanceCnt);
            //    }
            //    else
            //    {
            //        m_inactiveRuleCnt++;
            //    }
            //}
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (m_showGizmos && Selection.activeObject == gameObject)
            {
                if (m_showBoundingBox)
                {
                    Gizmos.color = Color.red;
                    if (!m_settings.m_isWorldmapSpawner)
                    {
                        Gizmos.DrawWireCube(transform.position, new Vector3(m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f, m_settings.m_spawnRange * 2f));
                    }
                    else
                    {
                        Vector3 pos = new Vector3(0f, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight / 2f, 0f);
                        float sidelength = m_worldCreationSettings.m_tileSize * m_worldCreationSettings.m_xTiles;
                        Vector3 size = new Vector3(sidelength, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight, sidelength);
                        Gizmos.DrawWireCube(pos, size);
                    }
                }

                //Water
                if (m_settings.m_resources != null && m_showSeaLevelPlane && !SessionManager.m_worldCreationRunning)
                {
                    BoundsDouble bounds = new BoundsDouble();
                    if (m_settings.m_isWorldmapSpawner)
                    {
                        bounds.extents = new Vector3(TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewTerrainHeight, TerrainLoaderManager.Instance.TerrainSceneStorage.m_worldMapPreviewRange / 2f);
                        //bounds need to be in world space + use the shifted origin
                        bounds.center = transform.position;
                    }
                    else
                    {
                        TerrainHelper.GetTerrainBounds(ref bounds);
                    }
                    bounds.center = new Vector3Double(bounds.center.x, SessionManager.GetSeaLevel(), bounds.center.z);
                    bounds.size = new Vector3Double(bounds.size.x, 0.05f, bounds.size.z);
                    Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, Color.blue.a / 4f);
                    Gizmos.DrawCube(bounds.center, bounds.size);
                }


            }

            //Update the counters
            UpdateCounters();
#endif
        }

        #region Texture map management

        /// <summary>
        /// Cache the texture maps for the terrain object id supplied - this is very memory intensive so use with care!
        /// </summary>
        public void CacheTextureMapsFromTerrain(int terrainID)
        {
            //Construct them of we dont have them
            if (m_textureMapCache == null)
            {
                m_textureMapCache = new Dictionary<int, List<HeightMap>>();
            }

            //Now find the terrain and load them for the specified terrain
            Terrain terrain;
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    float[,,] splatMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                    List<HeightMap> textureMapList = new List<HeightMap>();
                    for (int txtIdx = 0; txtIdx < terrain.terrainData.alphamapLayers; txtIdx++)
                    {
                        HeightMap txtMap = new HeightMap(splatMaps, txtIdx);
                        textureMapList.Add(txtMap);
                    }
                    m_textureMapCache[terrainID] = textureMapList;
                    return;
                }
            }
            Debug.LogError("Attempted to get textures on terrain that does not exist!");
        }

        /// <summary>
        /// Get the detail map list for the terrain
        /// </summary>
        /// <param name="terrainID">Object id of the terrain</param>
        /// <returns>Detail map list or null</returns>
        public List<HeightMap> GetTextureMaps(int terrainID)
        {
            List<HeightMap> mapList;
            if (!m_textureMapCache.TryGetValue(terrainID, out mapList))
            {
                return null;
            }
            return mapList;
        }

        /// <summary>
        /// Save the texture maps back into the terrain
        /// </summary>
        /// <param name="terrainID">ID of the terrain to do this for</param>
        public void SaveTextureMapsToTerrain(int terrainID)
        {
            Terrain terrain;
            HeightMap txtMap;
            List<HeightMap> txtMapList;

            //Make sure we can find it
            if (!m_textureMapCache.TryGetValue(terrainID, out txtMapList))
            {
                Debug.LogError("Texture map list was not found for terrain ID : " + terrainID + " !");
                return;
            }

            //Abort if we dont have anything in the list
            if (txtMapList.Count <= 0)
            {
                Debug.LogError("Texture map list was empty for terrain ID : " + terrainID + " !");
                return;
            }

            //Locate the terrain
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    //Make sure that the number of prototypes matches up
                    if (txtMapList.Count != terrain.terrainData.alphamapLayers)
                    {
                        Debug.LogError("Texture map prototype list does not match terrain prototype list for terrain ID : " + terrainID + " !");
                        return;
                    }

                    float[,,] splatMaps = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
                    for (int txtIdx = 0; txtIdx < terrain.terrainData.alphamapLayers; txtIdx++)
                    {
                        txtMap = txtMapList[txtIdx];
                        for (int x = 0; x < txtMap.Width(); x++)
                        {
                            for (int z = 0; z < txtMap.Depth(); z++)
                            {
                                splatMaps[x, z, txtIdx] = txtMap[x, z];
                            }
                        }
                    }
                    terrain.terrainData.SetAlphamaps(0, 0, splatMaps);
                    return;
                }
            }
            Debug.LogError("Attempted to locate a terrain that does not exist!");
        }

        /// <summary>
        /// Remove the texture maps from memory
        /// </summary>
        public void DeleteTextureMapCache()
        {
            m_textureMapCache = new Dictionary<int, List<HeightMap>>();
        }

        /// <summary>
        /// Set the texture maps dirty if we modified them
        /// </summary>
        public void SetTextureMapsDirty()
        {
            m_textureMapsDirty = true;
        }

        #endregion

        #region Detail map management

        /// <summary>
        /// Get the detail maps for the terrain object id supplied - this is very memory intensive so use with care!
        /// </summary>
        public void CacheDetailMapsFromTerrain(int terrainID)
        {
            //Construct them of we dont have them
            if (m_detailMapCache == null)
            {
                m_detailMapCache = new Dictionary<int, List<HeightMap>>();
            }

            //Now find the terrain and load them for the specified terrain
            Terrain terrain;
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    List<HeightMap> detailMapList = new List<HeightMap>();
                    for (int dtlIdx = 0; dtlIdx < terrain.terrainData.detailPrototypes.Length; dtlIdx++)
                    {
                        HeightMap dtlMap = new HeightMap(terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, dtlIdx));
                        detailMapList.Add(dtlMap);
                    }
                    m_detailMapCache[terrainID] = detailMapList;
                    return;
                }
            }
            Debug.LogError("Attempted to get details on terrain that does not exist!");
        }

        /// <summary>
        /// Save the detail maps back into the terrain
        /// </summary>
        /// <param name="terrainID">ID of the terrain to do this for</param>
        public void SaveDetailMapsToTerrain(int terrainID)
        {
            Terrain terrain;
            HeightMap dtlMap;
            List<HeightMap> dtlMapList;

            //Make sure we can find it
            if (!m_detailMapCache.TryGetValue(terrainID, out dtlMapList))
            {
                Debug.LogWarning(gameObject.name + "Detail map list was not found for terrain ID : " + terrainID + " !");
                return;
            }

            //Abort if we dont have anything in the list
            if (dtlMapList.Count <= 0)
            {
                Debug.LogWarning(gameObject.name + ": Detail map list was empty for terrain ID : " + terrainID + " !");
                return;
            }

            //Locate the terrain
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    //Make sure that the number of prototypes matches up
                    if (dtlMapList.Count != terrain.terrainData.detailPrototypes.Length)
                    {
                        Debug.LogError("Detail map protoype list does not match terrain prototype list for terrain ID : " + terrainID + " !");
                        return;
                    }

                    //Mow iterate thru and apply back
                    int[,] dtlMapArray = new int[dtlMapList[0].Width(), dtlMapList[0].Depth()];
                    for (int dtlIdx = 0; dtlIdx < terrain.terrainData.detailPrototypes.Length; dtlIdx++)
                    {
                        dtlMap = dtlMapList[dtlIdx];
                        for (int x = 0; x < dtlMap.Width(); x++)
                        {
                            for (int z = 0; z < dtlMap.Depth(); z++)
                            {
                                dtlMapArray[x, z] = (int)dtlMap[x, z];
                            }
                        }
                        terrain.terrainData.SetDetailLayer(0, 0, dtlIdx, dtlMapArray);
                    }
                    terrain.Flush();
                    return;
                }
            }
            Debug.LogError("Attempted to locate a terrain that does not exist!");
        }

        /// <summary>
        /// Get the detail map list for the terrain
        /// </summary>
        /// <param name="terrainID">Object id of the terrain</param>
        /// <returns>Detail map list or null</returns>
        public List<HeightMap> GetDetailMaps(int terrainID)
        {
            List<HeightMap> mapList;
            if (!m_detailMapCache.TryGetValue(terrainID, out mapList))
            {
                return null;
            }
            return mapList;
        }

        /// <summary>
        /// Get the detail map for the specific detail
        /// </summary>
        /// <param name="terrainID">Terrain to query</param>
        /// <param name="detailIndex">Detail prototype index</param>
        /// <returns>Detail heightmap or null if not found</returns>
        public HeightMap GetDetailMap(int terrainID, int detailIndex)
        {
            List<HeightMap> dtlMapList;
            if (!m_detailMapCache.TryGetValue(terrainID, out dtlMapList))
            {
                return null;
            }
            if (detailIndex >= 0 && detailIndex < dtlMapList.Count)
            {
                return dtlMapList[detailIndex];
            }
            return null;
        }

        /// <summary>
        /// Remove the detail maps from memory
        /// </summary>
        public void DeleteDetailMapCache()
        {
            m_detailMapCache = new Dictionary<int, List<HeightMap>>();
        }

        #endregion

        #region Tree Management

        public void CacheTreesFromTerrain()
        {
            m_treeCache.LoadTreesFromTerrain();
        }

        public void DeleteTreeCache()
        {
            m_treeCache = new TreeManager();
        }

        #endregion

        #region Sessions and Serialisation

        /// <summary>
        /// Add the operationm to the session manager
        /// </summary>
        /// <param name="opType">The type of operation to add</param>
        public void AddToSession(GaiaOperation.OperationType opType, string opName)
        {
            //Update the session

            if (SessionManager != null && SessionManager.IsLocked() != true)
            {
                GaiaOperation op = new GaiaOperation();
                op.m_description = opName;
                //op.m_generatedByID = m_spawnerID;
                //op.m_generatedByName = transform.name;
                //op.m_generatedByType = this.GetType().ToString();
                op.m_isActive = true;
                op.m_operationDateTime = DateTime.Now.ToString();
                op.m_operationType = opType;
                //op.m_operationDataJson = new string[1];
                //op.m_operationDataJson[0] = this.SerialiseJson();
                SessionManager.AddOperation(op);
                SessionManager.AddResource(m_settings.m_resources);
            }
        }

        /// <summary>
        /// Serialise this as json
        /// </summary>
        /// <returns></returns>
        public string SerialiseJson()
        {
            //Grab the various paths
            //#if UNITY_EDITOR
            //            m_settings.m_resourcesPath = AssetDatabase.GetAssetPath(m_settings.m_resources);
            //#endif

            //            fsData data;
            //            fsSerializer serializer = new fsSerializer();
            //            serializer.TrySerialize(this, out data);

            //            //Debug.Log(fsJsonPrinter.PrettyJson(data));

            //           return fsJsonPrinter.CompressedJson(data);
            return "";
        }

        /// <summary>
        /// Deserialise the suplied json into this object
        /// </summary>
        /// <param name="json">Source json</param>
        public void DeSerialiseJson(string json)
        {
            //fsData data = fsJsonParser.Parse(json);
            //fsSerializer serializer = new fsSerializer();
            //var spawner = this;
            //serializer.TryDeserialize<Spawner>(data, ref spawner);
            //spawner.m_settings.m_resources = GaiaUtils.GetAsset(m_settings.m_resourcesPath, typeof(Gaia.GaiaResource)) as Gaia.GaiaResource;
        }

        #endregion

        #region Handy helpers

        /// <summary>
        /// Flatten all active terrains
        /// </summary>
        public void FlattenTerrain()
        {
            //Update the session
            AddToSession(GaiaOperation.OperationType.FlattenTerrain, "Flattening terrain");

            //Get an undo buffer
            GaiaWorldManager mgr = new GaiaWorldManager(Terrain.activeTerrains);
            mgr.FlattenWorld();
        }

        /// <summary>
        /// Smooth all active terrains
        /// </summary>
        public void SmoothTerrain()
        {
            //Update the session
            AddToSession(GaiaOperation.OperationType.SmoothTerrain, "Smoothing terrain");

            //Smooth the world
            GaiaWorldManager mgr = new GaiaWorldManager(Terrain.activeTerrains);
            mgr.SmoothWorld();
        }

        /// <summary>
        /// Clear trees
        /// </summary>
        public void ClearTrees(ClearSpawnFor clearSpawnFor, ClearSpawnFrom clearSpawnFrom, List<string> terrainNames = null)
        {
            TerrainHelper.ClearSpawns(SpawnerResourceType.TerrainTree, clearSpawnFor, clearSpawnFrom, terrainNames, this);
            //iterate through all spawners, reset counter for tree rules
            ResetAffectedSpawnerCounts(SpawnerResourceType.TerrainTree);
        }

        public void ActivateTreeStandIn(int m_spawnRuleIndexBeingDrawn)
        {
            //Before activation: Do other spawners currently use the stand-in? If yes, we need to turn them back before activating this one
            var allSpawnersWithStandIns = Resources.FindObjectsOfTypeAll<Spawner>().Where(x => x.m_settings.m_spawnerRules.Find(y => y.m_usesBoxStandIn == true) != null).ToArray();

            foreach (Spawner spawner in allSpawnersWithStandIns)
            {
                for (int i = 0; i < spawner.m_settings.m_spawnerRules.Count; i++)
                {
                    SpawnRule sr = (SpawnRule)spawner.m_settings.m_spawnerRules[i];
                    if (sr.m_usesBoxStandIn)
                    {
                        spawner.DeactivateTreeStandIn(i);
                    }
                }

            }
            ResourceProtoTree resourceProtoTree = m_settings.m_resources.m_treePrototypes[m_settings.m_spawnerRules[m_spawnRuleIndexBeingDrawn].m_resourceIdx];
            foreach (Terrain t in Terrain.activeTerrains)
            {
                int treePrototypeID = -1;
                int localTerrainIdx = 0;
                foreach (TreePrototype proto in t.terrainData.treePrototypes)
                {
                    if (PWCommon5.Utils.IsSameGameObject(resourceProtoTree.m_desktopPrefab, proto.prefab, false))
                    {
                        treePrototypeID = localTerrainIdx;
                        break;
                    }
                    localTerrainIdx++;
                }

                if (treePrototypeID != -1)
                {
                    //reference the exisiting prototypes, then assign them - otherwise the terrain trees won't update properly
                    TreePrototype[] exisitingPrototypes = t.terrainData.treePrototypes;
                    exisitingPrototypes[treePrototypeID].prefab = GaiaSettings.m_boxStandInPrefab;
                    t.terrainData.treePrototypes = exisitingPrototypes;
                }
                m_settings.m_spawnerRules[m_spawnRuleIndexBeingDrawn].m_usesBoxStandIn = true;
            }
        }

        public void DeactivateTreeStandIn(int m_spawnRuleIndexBeingDrawn)
        {
            ResourceProtoTree resourceProtoTree = m_settings.m_resources.m_treePrototypes[m_settings.m_spawnerRules[m_spawnRuleIndexBeingDrawn].m_resourceIdx];
            foreach (Terrain t in Terrain.activeTerrains)
            {
                int treePrototypeID = -1;
                int localTerrainIdx = 0;
                foreach (TreePrototype proto in t.terrainData.treePrototypes)
                {
                    if (PWCommon5.Utils.IsSameGameObject(m_gaiaSettings.m_boxStandInPrefab, proto.prefab, false))
                    {
                        treePrototypeID = localTerrainIdx;
                        break;
                    }
                    localTerrainIdx++;
                }

                if (treePrototypeID != -1)
                {
                    //reference the exisiting prototypes, then assign them - otherwise the terrain trees won't update properly
                    TreePrototype[] exisitingPrototypes = t.terrainData.treePrototypes;
                    exisitingPrototypes[treePrototypeID].prefab = resourceProtoTree.m_desktopPrefab;
                    t.terrainData.treePrototypes = exisitingPrototypes;
                }
            }
            m_settings.m_spawnerRules[m_spawnRuleIndexBeingDrawn].m_usesBoxStandIn = false;
        }

        private void ResetAffectedSpawnerCounts(SpawnerResourceType resourceType)
        {
            Spawner[] affectedSpawners;
            if (m_settings.m_clearSpawnsFrom == ClearSpawnFrom.AnySource)
            {
                affectedSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
            }
            else
            {
                affectedSpawners = new Spawner[1] { this };
            }

            foreach (Spawner spawner in affectedSpawners)
            {
                foreach (SpawnRule spawnRule in spawner.m_settings.m_spawnerRules)
                {
                    if (spawnRule.m_resourceType == resourceType)
                    {
                        spawnRule.m_spawnedInstances = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Clear all the grass off all the terrains
        /// </summary>
        public void ClearDetails(ClearSpawnFor clearSpawnFor, ClearSpawnFrom clearSpawnFrom, List<string> terrainNames = null)
        {
            TerrainHelper.ClearSpawns(SpawnerResourceType.TerrainDetail, clearSpawnFor, clearSpawnFrom, terrainNames, this);
            ResetAffectedSpawnerCounts(SpawnerResourceType.TerrainDetail);

        }

        /// <summary>
        /// Clears all Game object spawn rules at once.
        /// </summary>
        public void ClearGameObjects(ClearSpawnFor clearSpawnFor, ClearSpawnFrom clearSpawnFrom, List<string> terrainNames = null)
        {
            TerrainHelper.ClearSpawns(SpawnerResourceType.GameObject, clearSpawnFor, clearSpawnFrom, terrainNames, this);
            ResetAffectedSpawnerCounts(SpawnerResourceType.GameObject);

            //Spawner[] allAffectedSpawners;

            //if (clearSpawnFrom == ClearSpawnFrom.OnlyThisSpawner)
            //{
            //    allAffectedSpawners = new Spawner[1] { this };
            //}
            //else
            //{
            //    allAffectedSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
            //}

            //int completedSpawners = 1;

            //foreach (Spawner spawner in allAffectedSpawners)
            //{
            //    GaiaUtils.DisplayProgressBarNoEditor("Clearing Game Objects...", "Spawner " + completedSpawners.ToString() + " of " + allAffectedSpawners.Count().ToString(), (float)completedSpawners / (float)allAffectedSpawners.Count());
            //    foreach (SpawnRule spawnRule in spawner.m_settings.m_spawnerRules)
            //    {
            //        if (spawnRule.m_resourceType == SpawnerResourceType.GameObject)
            //        {
            //            spawner.ClearGameObjectsForRule(spawnRule, clearSpawnFor == ClearSpawnFor.AllTerrains);
            //        }
            //    }
            //    completedSpawners++;
            //}
            //GaiaUtils.ClearProgressBarNoEditor();
        }

        /// <summary>
        /// Clears all Game objects created by spawn extenisons at once.
        /// </summary>
        public void ClearAllSpawnExtensions(ClearSpawnFor clearSpawnFor, ClearSpawnFrom clearSpawnFrom, List<string> terrainNames = null)
        {
            TerrainHelper.ClearSpawns(SpawnerResourceType.SpawnExtension, clearSpawnFor, clearSpawnFrom, terrainNames, this);
            ResetAffectedSpawnerCounts(SpawnerResourceType.SpawnExtension);
            //Spawner[] allAffectedSpawners;

            //if (clearSpawnFrom == ClearSpawnFrom.OnlyThisSpawner)
            //{
            //    allAffectedSpawners = new Spawner[1] { this };
            //}
            //else
            //{
            //    allAffectedSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
            //}

            //int completedSpawners = 1;

            //foreach (Spawner spawner in allAffectedSpawners)
            //{
            //    GaiaUtils.DisplayProgressBarNoEditor("Clearing Spawn Extensions...", "Spawner " + completedSpawners.ToString() + " of " + allAffectedSpawners.Count().ToString(), (float)completedSpawners / (float)allAffectedSpawners.Count());
            //    foreach (SpawnRule spawnRule in spawner.m_settings.m_spawnerRules)
            //    {
            //        if (spawnRule.m_resourceType == SpawnerResourceType.SpawnExtension)
            //        {
            //            spawner.ClearSpawnExtensionsForRule(spawnRule);
            //            spawner.ClearGameObjectsForRule(spawnRule, clearSpawnFor == ClearSpawnFor.AllTerrains);
            //        }
            //    }
            //    completedSpawners++;
            //}
            //GaiaUtils.ClearProgressBarNoEditor();
        }


        /// <summary>
        /// Calls the Delete function on all Spawn Extensions of a certain rule
        /// </summary>
        /// <param name="spawnRule"></param>
        public void ClearSpawnExtensionsForRule(SpawnRule spawnRule, SpawnerSettings spawnerSettings = null)
        {
            if (spawnerSettings == null)
            {
                spawnerSettings = m_settings;
            }

            if (spawnRule.m_resourceIdx > spawnerSettings.m_resources.m_spawnExtensionPrototypes.Length - 1)
            {
                return;
            }

            if (spawnerSettings.m_resources.m_spawnExtensionPrototypes[spawnRule.m_resourceIdx] == null)
            {
                return;
            }

            ResourceProtoSpawnExtension protoSE = spawnerSettings.m_resources.m_spawnExtensionPrototypes[spawnRule.m_resourceIdx];

            //iterate through all instances
            foreach (ResourceProtoSpawnExtensionInstance instance in protoSE.m_instances)
            {
                if (instance.m_spawnerPrefab == null)
                {
                    continue;
                }

                foreach (ISpawnExtension spawnExtension in instance.m_spawnerPrefab.GetComponents<ISpawnExtension>())
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        spawnExtension.Delete(GaiaUtils.GetGOSpawnTarget(spawnRule, protoSE.m_name, t));
                    }
                }

            }

        }

        /// <summary>
        /// Clears all StampDistributions
        /// </summary>
        public void ClearStampDistributions()
        {
            m_worldMapStamperSettings.Clear();

            foreach (SpawnRule rule in m_settings.m_spawnerRules)
            {
                if (rule.m_resourceType == GaiaConstants.SpawnerResourceType.StampDistribution)
                {
                    rule.m_spawnedInstances = 0;
                }
            }

            m_worldDesignerClearStampsWarningShown = false;

            if (m_baseTerrainSettings.m_drawPreview)
            {
                UpdateBaseTerrainStamper();
            }
            SetDefaultWorldDesignerPreviewSettings();
            SessionManager.DirtyWorldMapMinMax();
        }

        /// <summary>
        /// Clears all stamp tokens / stamper settings for the World Map Generation created by a certain rule
        /// </summary>
        /// <param name="spawnRule"></param>
        private void ClearStampDistributionForRule(SpawnRule spawnRule)
        {
            ResourceProtoStamp protoSD = m_settings.m_resources.m_stampDistributionPrototypes[spawnRule.m_resourceIdx];


            //iterate through all Stamp Tokens and remove those belonging to the same feature type as spawned from this rule

            //m_worldMapStamperSettings.RemoveAll(x=>x.)

            //if (m_worldMapTerrain == null)
            //{
            //    m_worldMapTerrain = TerrainHelper.GetWorldMapTerrain();
            //}

            //Transform tokenContainer = m_worldMapTerrain.transform.Find(GaiaConstants.worldMapStampTokenSpawnTarget);
            //if (tokenContainer != null)
            //{
            //    var allStampTokens = tokenContainer.GetComponentsInChildren<WorldMapStampToken>();

            //    for (int i = allStampTokens.Length - 1; i >= 0; i--)
            //    {
            //        if (protoSD.m_featureType == allStampTokens[i].m_featureType)
            //        {
            //            m_worldMapStamperSettings.Remove(allStampTokens[i].m_connectedStamperSettings);
            //            DestroyImmediate(allStampTokens[i].gameObject);
            //        }

            //    }
            //}



        }


        /// <summary>
        /// Clear all the GameObjects created by this spawner off all the terrains
        /// </summary>
        public static void ClearGameObjectsForRule(Spawner spawner, SpawnRule spawnRule, bool allTerrains = true, Terrain terrainToDeleteFrom = null)
        {
            //Update the session
            string protoName = "";
            switch (spawnRule.m_resourceType)
            {
                case SpawnerResourceType.GameObject:
                    ResourceProtoGameObject protoGO = spawner.m_settings.m_resources.m_gameObjectPrototypes[spawnRule.m_resourceIdx];

                    if (protoGO == null)
                    {
                        Debug.LogError("Could not find prototype info trying to delete Game Objects from rule " + spawnRule.m_name);
                        return;
                    }
                    protoName = protoGO.m_name;
                    break;
                case SpawnerResourceType.SpawnExtension:
                    ResourceProtoSpawnExtension protoSE = spawner.m_settings.m_resources.m_spawnExtensionPrototypes[spawnRule.m_resourceIdx];

                    if (protoSE == null)
                    {
                        Debug.LogError("Could not find prototype info trying to delete Spawn Extensions Game Objects from rule " + spawnRule.m_name);
                        return;
                    }
                    protoName = protoSE.m_name;
                    break;
                case SpawnerResourceType.Probe:
                    if (spawnRule.m_resourceIdx >= spawner.m_settings.m_resources.m_probePrototypes.Length)
                    {
                        return;
                    }
                    ResourceProtoProbe protoProbe = spawner.m_settings.m_resources.m_probePrototypes[spawnRule.m_resourceIdx];

                    if (protoProbe == null)
                    {
                        Debug.LogError("Could not find prototype info trying to delete probes from rule " + spawnRule.m_name);
                        return;
                    }
                    protoName = protoProbe.m_name;
                    break;

            }

            Terrain[] relevantTerrains;

            if (allTerrains)
            {
                relevantTerrains = Terrain.activeTerrains;
            }
            else
            {
                if (terrainToDeleteFrom == null)
                {
                    relevantTerrains = new Terrain[1] { spawner.GetCurrentTerrain() };
                }
                else
                {
                    relevantTerrains = new Terrain[1] { terrainToDeleteFrom };
                }
            }



            foreach (Terrain t in relevantTerrains)
            {
                bool deletedSomething = false;
                Transform target = GaiaUtils.GetGOSpawnTarget(spawnRule, protoName, t);
                Scene sceneWeDeletedFrom = target.gameObject.scene;

                if (spawnRule.m_goSpawnTargetMode == SpawnerTargetMode.Terrain || allTerrains)
                {
                    //Terrain based target, or user choose to delete from all Terrains - this means deletion can be done fast and easy by removing the target object                     
                    if (target != null)
                    {
                        deletedSomething = true;
                        DestroyImmediate(target.gameObject);
                    }
                }
                else
                {
                    //There is a custom transform to spawn under and we want to delete on specific terrains only - which means we need to take a look at each Gameobject individually


                    float terrainMinX = t.transform.position.x;
                    float terrainMinZ = t.transform.position.z;
                    float terrainMaxX = t.transform.position.x + t.terrainData.size.x;
                    float terrainMaxZ = t.transform.position.z + t.terrainData.size.x;


                    for (int g = target.childCount - 1; g >= 0; g--)
                    {
                        GameObject GOtoDelete = target.GetChild(g).gameObject;

                        //is the gameobject placed on / above / below the terrain?
                        if (terrainMinX <= GOtoDelete.transform.position.x &&
                            terrainMinZ <= GOtoDelete.transform.position.z &&
                            terrainMaxX >= GOtoDelete.transform.position.x &&
                            terrainMaxZ >= GOtoDelete.transform.position.z)
                        {
                            DestroyImmediate(GOtoDelete);
                            deletedSomething = true;
                        }
                    }
                }
                //if we deleted something the scene we deleted from should be marked as dirty.
                if (deletedSomething)
                {
#if UNITY_EDITOR
                    EditorSceneManager.MarkSceneDirty(sceneWeDeletedFrom);
#endif
                }
            }
            spawnRule.m_spawnedInstances = 0;
        }

        /// <summary>
        /// Only serves the purpose of supressing the warning for the unused field when CTS is not installed.
        /// </summary>
        private void SupressCTSProfileWarning()
        {
#if CTS_PRESENT
            if (m_connectedCTSProfileGUID.Length > 1)
            {
            }
#endif
        }


        public static void HandleAutoSpawnerStack(List<AutoSpawner> autoSpawners, Transform transform, float range, bool allTerrains, BiomeControllerSettings biomeControllerSettings = null)
        {
            BoundsDouble spawnArea = new BoundsDouble();
            if (allTerrains)
            {
                TerrainHelper.GetTerrainBounds(ref spawnArea);
            }
            else
            {
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    spawnArea.center = new Vector3Double(transform.position) + TerrainLoaderManager.Instance.GetOrigin();
                }
                else
                {
                    spawnArea.center = transform.position;
                }
                spawnArea.size = new Vector3(range * 2f, range * 2f, range * 2f);
            }

            try
            {
                TerrainLoaderManager.Instance.SwitchToLocalMap();
                foreach (Spawner spawner in autoSpawners.Where(x => x.spawner != null && x.isActive == true).Select(x => x.spawner))
                {
                    spawner.GenerateNewRandomSeed();
                }
                SpawnOperationSettings soSettings = ScriptableObject.CreateInstance<SpawnOperationSettings>();
                soSettings.m_spawnerSettingsList = autoSpawners.Where(x => x.spawner != null && x.isActive == true).Select(x => x.spawner.m_settings).ToList();
                if (biomeControllerSettings != null)
                {
                    soSettings.m_biomeControllerSettings = biomeControllerSettings;
                }
                soSettings.m_spawnArea = spawnArea;
                soSettings.m_isWorldMapSpawner = autoSpawners.Find(x => x.spawner != null && x.spawner.m_settings.m_isWorldmapSpawner) != null;
                GaiaSessionManager.Spawn(soSettings, true, autoSpawners.Where(x => x.spawner != null && x.isActive == true).Select(x => x.spawner).ToList());
                //autoSpawners[0].spawner.m_updateCoroutine = autoSpawners[0].spawner.AreaSpawn(autoSpawners.Select(x => x.spawner).ToList(), spawnArea);
                //autoSpawners[0].spawner.StartEditorUpdates();
            }
            catch (Exception ex)
            {
                Debug.LogError("Autospawning failed with Exception: " + ex.Message + "\n\n" + "Stack trace: \n\n" + ex.StackTrace);
                ProgressBar.Clear(ProgressBarPriority.Spawning);
            }



            //AutoSpawner nextSpawner = autoSpawners.Find(x => x.status == AutoSpawnerStatus.Spawning);
            //if (nextSpawner != null)
            //{
            //    if (nextSpawner.spawner.IsSpawning())
            //    {
            //        return false;
            //        //Do Nothing, still spawning
            //    }
            //    else
            //    {
            //        //Auto Spawner is done, look for next spawner
            //        GaiaUtils.DisplayProgressBarNoEditor("Spawning", "Preparing next Spawner...",0);
            //        nextSpawner.status = AutoSpawnerStatus.Done;
            //        nextSpawner = autoSpawners.Find(x => x.status == AutoSpawnerStatus.Queued);

            //    }
            //}
            //else
            //{
            //    //No spawner spawning atm, let's pick the first queued one
            //    nextSpawner = autoSpawners.Find(x => x.status == AutoSpawnerStatus.Queued);
            //}

            //if (nextSpawner != null && !m_cancelSpawn)
            //{
            //    if (!nextSpawner.spawner.IsSpawning())
            //    {
            //        //nextSpawner.spawner.transform.position = new Vector3(m_stamper.transform.position.x, nextSpawner.spawner.transform.position.y, m_stamper.transform.position.z);
            //        //Terrain terrain = nextSpawner.spawner.GetCurrentTerrain();
            //        //nextSpawner.spawner.m_settings.m_spawnRange = terrain.terrainData.size.x * (m_stamper.m_settings.m_width / 100f);
            //        nextSpawner.spawner.UpdateMinMaxHeight();

            //        int totalSpawnRules = 0;
            //        int completedSpawnRules = 0;
            //        foreach (AutoSpawner autoSpawner in autoSpawners.Where(x => x.isActive))
            //        {
            //            foreach (SpawnRule rule in autoSpawner.spawner.settings.m_spawnerRules)
            //            {
            //                if (rule.m_isActive)
            //                {
            //                    totalSpawnRules++;
            //                    if (autoSpawner.status == AutoSpawnerStatus.Done)
            //                    {
            //                        completedSpawnRules++;
            //                    }
            //                }
            //            }
            //        }

            //        if (allTerrains)
            //        {
            //            ////int worldSpawnSteps = nextSpawner.spawner.GetWorldSpawnSteps();
            //            //totalSpawnRules *= worldSpawnSteps;
            //            //completedSpawnRules *= worldSpawnSteps;
            //        }

            //        //nextSpawner.spawner.Spawn(allTerrains, completedSpawnRules, totalSpawnRules);
            //        nextSpawner.status = AutoSpawnerStatus.Spawning;
            //    }
            //    return false;

            //}
            //else
            //{
            //    //no spawners left
            //   GaiaUtils.ClearProgressBarNoEditor();
            //   GaiaUtils.ReleaseAllTempRenderTextures();
            //    m_cancelSpawn = false;
            //    return true;

            //}
        }

        #endregion

        #region Height map management

        /// <summary>
        /// Cache the height map for the terrain object id supplied - this is very memory intensive so use with care!
        /// </summary>
        public void CacheHeightMapFromTerrain(int terrainID)
        {
            //Construct them of we dont have them
            if (m_heightMapCache == null)
            {
                m_heightMapCache = new Dictionary<int, UnityHeightMap>();
            }

            //Now find the terrain and load them for the specified terrain
            Terrain terrain;
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    m_heightMapCache[terrainID] = new UnityHeightMap(terrain);
                    return;
                }
            }
            Debug.LogError("Attempted to get height maps on a terrain that does not exist!");
        }

        /// <summary>
        /// Get the height map for the terrain
        /// </summary>
        /// <param name="terrainID">Object id of the terrain</param>
        /// <returns>Heightmap or null</returns>
        public UnityHeightMap GetHeightMap(int terrainID)
        {
            UnityHeightMap heightmap;
            if (!m_heightMapCache.TryGetValue(terrainID, out heightmap))
            {
                return null;
            }
            return heightmap;
        }

        /// <summary>
        /// Save the height map back into the terrain
        /// </summary>
        /// <param name="terrainID">ID of the terrain to do this for</param>
        public void SaveHeightMapToTerrain(int terrainID)
        {
            Terrain terrain;
            UnityHeightMap heightmap;

            //Make sure we can find it
            if (!m_heightMapCache.TryGetValue(terrainID, out heightmap))
            {
                Debug.LogError("Heightmap was not found for terrain ID : " + terrainID + " !");
                return;
            }

            //Locate the terrain and update it
            for (int terrIdx = 0; terrIdx < Terrain.activeTerrains.Length; terrIdx++)
            {
                terrain = Terrain.activeTerrains[terrIdx];
                if (terrain.GetInstanceID() == terrainID)
                {
                    heightmap.SaveToTerrain(terrain);
                    return;
                }
            }
            Debug.LogError("Attempted to locate a terrain that does not exist!");
        }

        /// <summary>
        /// Remove the texture maps from memory
        /// </summary>
        public void DeleteHeightMapCache()
        {
            m_heightMapCache = new Dictionary<int, UnityHeightMap>();
        }

        /// <summary>
        /// Set the height maps dirty if we modified them
        /// </summary>
        public void SetHeightMapsDirty()
        {
            m_heightMapDirty = true;
        }

        #endregion

        #region Stamp management

        public void CacheStamps(List<string> stampList)
        {
            //Construct them of we dont have them
            if (m_stampCache == null)
            {
                m_stampCache = new Dictionary<string, HeightMap>();
            }

            //Get the list of stamps for this spawner
            for (int idx = 0; idx < stampList.Count; idx++)
            {



            }
        }


        #endregion

        #region Tag management

        /// <summary>
        /// Load all the tags in the scene into the tag cache
        /// </summary>
        /// <param name="tagList"></param>
        private void CacheTaggedGameObjectsFromScene(List<string> tagList)
        {
            //Create a new cache (essentially releasing the old one)
            m_taggedGameObjectCache = new Dictionary<string, Quadtree<GameObject>>();

            //Now load all the tagged objects into the cache
            string tag;
            bool foundTag;
            Quadtree<GameObject> quadtree;
            Rect pos = new Rect(Terrain.activeTerrain.transform.position.x, Terrain.activeTerrain.transform.position.z,
                Terrain.activeTerrain.terrainData.size.x, Terrain.activeTerrain.terrainData.size.z);

            for (int tagIdx = 0; tagIdx < tagList.Count; tagIdx++)
            {
                //Check that unity knows about the tag

                tag = tagList[tagIdx].Trim();
                foundTag = false;
                if (!string.IsNullOrEmpty(tag))
                {
#if UNITY_EDITOR
                    for (int idx = 0; idx < UnityEditorInternal.InternalEditorUtility.tags.Length; idx++)
                    {
                        if (UnityEditorInternal.InternalEditorUtility.tags[idx].Contains(tag))
                        {
                            foundTag = true;
                            break;
                        }
                    }
#else
                    foundTag = true;
#endif
                }

                //If its good then cache it
                if (foundTag)
                {
                    quadtree = null;
                    if (!m_taggedGameObjectCache.TryGetValue(tag, out quadtree))
                    {
                        quadtree = new Quadtree<GameObject>(pos);
                        m_taggedGameObjectCache.Add(tag, quadtree);
                    }
                    GameObject go;
                    Vector2 go2DPos;
                    GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
                    for (int goIdx = 0; goIdx < gos.Length; goIdx++)
                    {
                        go = gos[goIdx];

                        //Only add it if within our bounds
                        go2DPos = new Vector2(go.transform.position.x, go.transform.position.z);
                        if (pos.Contains(go2DPos))
                        {
                            quadtree.Insert(go2DPos, go);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete the tag cache
        /// </summary>
        private void DeleteTagCache()
        {
            m_taggedGameObjectCache = null;
        }

        /// <summary>
        /// Get the objects that match the tag list within the defined area
        /// </summary>
        /// <param name="tagList">List of tags to search</param>
        /// <param name="area">Area to search</param>
        /// <returns></returns>
        public List<GameObject> GetNearbyObjects(List<string> tagList, Rect area)
        {
            string tag;
            List<GameObject> gameObjects = new List<GameObject>();
            Quadtree<GameObject> quadtree;
            for (int tagIdx = 0; tagIdx < tagList.Count; tagIdx++)
            {
                quadtree = null;
                tag = tagList[tagIdx];

                //Process each tag
                if (m_taggedGameObjectCache.TryGetValue(tag, out quadtree))
                {
                    IEnumerable<GameObject> gameObjs = quadtree.Find(area);
                    foreach (GameObject go in gameObjs)
                    {
                        gameObjects.Add(go);
                    }
                }
            }
            return gameObjects;
        }

        /// <summary>
        /// Get the closest gameobject to the centre of the area supplied that matches the tag list
        /// </summary>
        /// <param name="tagList">List of tags to search</param>
        /// <param name="area">The area to search</param>
        /// <returns></returns>
        public GameObject GetClosestObject(List<string> tagList, Rect area)
        {
            string tag;
            float distance;
            float closestDistance = float.MaxValue;
            GameObject closestGo = null;
            Quadtree<GameObject> quadtree;
            for (int tagIdx = 0; tagIdx < tagList.Count; tagIdx++)
            {
                quadtree = null;
                tag = tagList[tagIdx];

                //Process each tag
                if (m_taggedGameObjectCache.TryGetValue(tag, out quadtree))
                {
                    IEnumerable<GameObject> gameObjs = quadtree.Find(area);
                    foreach (GameObject go in gameObjs)
                    {
                        distance = Vector2.Distance(area.center, new Vector2(go.transform.position.x, go.transform.position.z));
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestGo = go;
                        }
                    }
                }
            }
            return closestGo;
        }

        /// <summary>
        /// Get the closest gameobject to the centre of the area supplied that matches the tag 
        /// </summary>
        /// <param name="tagList">Tag to search for</param>
        /// <param name="area">The area to search</param>
        /// <returns></returns>
        public GameObject GetClosestObject(string tag, Rect area)
        {
            float distance, closestDistance = float.MaxValue;
            GameObject closestGo = null;
            Quadtree<GameObject> quadtree = null;

            if (m_taggedGameObjectCache.TryGetValue(tag, out quadtree))
            {
                IEnumerable<GameObject> gameObjs = quadtree.Find(area);
                foreach (GameObject go in gameObjs)
                {
                    distance = Vector2.Distance(area.center, new Vector2(go.transform.position.x, go.transform.position.z));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestGo = go;
                    }
                }
            }
            return closestGo;
        }


        #endregion

        #region Saving and Loading

        /// <summary>
        /// Applies the settings from a predefined spawner settings file 
        /// </summary>
        /// <param name="settingsToLoad">The settings to apply to this spawner</param>
        /// <param name="createDetailSOs">Whether the detail scriptable objects for this spawner settings file should be created or not</param>
        /// <param name="floraDetailLODIdOverrides">LOD ID overrides for terrain details for the flora system</param>
        /// /// <param name="floraTreeLODIdOverrides">LOD ID overrides for terrain trees for the flora system</param>
        public void LoadSettings(SpawnerSettings settingsToLoad, bool createDetailSOs = true, List<FloraLODIdOverrides>[] floraDetailLODIdOverrides = null, List<FloraLODIdOverrides>[] floraTreeLODIdOverrides = null)
        {
            m_settings.ClearImageMaskTextures();
            //Set existing settings = null to force a new scriptable object
            m_settings = null;

            m_settings = Instantiate(settingsToLoad);
#if UNITY_EDITOR
            m_settings.m_lastGUIDSaved = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settingsToLoad));
#endif

            //override assetGUIDs / instance ids in the flora data for terrain details, if provided
            if (floraDetailLODIdOverrides != null)
            {
#if FLORA_PRESENT
                for (int i = 0; i < m_settings.m_resources.m_detailPrototypes.Length; i++)
                {
                    ResourceProtoDetail rpd = m_settings.m_resources.m_detailPrototypes[i];
                    FloraUtils.ResetSettingsObjects(rpd.m_floraLODs, floraDetailLODIdOverrides[i]);
                }
#endif
            }

            //override assetGUIDs / instance ids in the flora data for terrain trees, if provided
            if (floraTreeLODIdOverrides != null)
            {
#if FLORA_PRESENT
                for (int i = 0; i < m_settings.m_resources.m_treePrototypes.Length; i++)
                {
                    ResourceProtoTree rpt = m_settings.m_resources.m_treePrototypes[i];
                    FloraUtils.ResetSettingsObjects(rpt.m_floraLODs, floraTreeLODIdOverrides[i]);
                }
#endif
            }

            //GaiaUtils.CopyFields(settingsToLoad, m_settings);

            Spawner[] allSpawner = Resources.FindObjectsOfTypeAll<Spawner>().Where(x => !x.name.StartsWith("Session")).ToArray();

            Dictionary<int, string> materialMap = new Dictionary<int, string>();

#if HDPipeline && !FLORA_PRESENT
            bool hdTerrainDetailMessageDisplayed = false;
#endif
            foreach (SpawnRule rule in m_settings.m_spawnerRules)
            {
                //close down all foldouts neatly when freshly loaded
                rule.m_isFoldedOut = false;
                rule.m_resourceSettingsFoldedOut = false;
                rule.m_spawnedInstances = 0;

#if FLORA_PRESENT
                //re-instantiate the PW Grass system settings files in the session directory, otherwise they will still point to the original that we loaded from
                if (rule.m_resourceType == SpawnerResourceType.TerrainDetail && createDetailSOs)
                {
                    ResourceProtoDetail protoDetail = m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx];
                    if (protoDetail != null)
                    {
                        foreach (FloraLOD floraLOD in protoDetail.m_floraLODs)
                        {
                            floraLOD.InstantiateDetailerSettingsGO(ref materialMap);
                        }
                    }
                }
                //if (rule.m_resourceType == SpawnerResourceType.TerrainTree && createDetailSOs)
                //{
                //    ResourceProtoTree protoTree = m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx];
                //    if (protoTree != null)
                //    {
                //         foreach (FloraLOD floraLOD in protoTree.m_floraLODs)
                //        {
                //            floraLOD.InstantiateDetailerSettingsGO(ref materialMap);
                //        }
                //    }
                //}
#endif

                //check if the spawn rule guid exists in this scene already - if yes, this rule must get a new ID then to avoid duplicate IDs
                if (allSpawner.Select(x => x.m_settings.m_spawnerRules).Where(x => x.Find(y => y.GUID == rule.GUID) != null).Count() > 1)
                {
                    rule.RegenerateGUID();
                }

                if (rule.m_resourceType == SpawnerResourceType.TerrainTree)
                {
                    m_settings.m_resources.m_treePrototypes[rule.m_resourceIdx].m_useFlora = false;
                }

                if (rule.m_resourceType == SpawnerResourceType.TerrainDetail)
                {
#if FLORA_PRESENT
                    if (m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_floraLODs == null || m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_floraLODs.Count == 0)
                    {
                        m_settings.m_resources.m_detailPrototypes[rule.m_resourceIdx].m_useFlora = false;
                    }
#else
#if HDPipeline
                    rule.m_isActive = false;
                    if (!hdTerrainDetailMessageDisplayed)
                    {
                        GaiaUtils.DisplayDialogNoEditor("HDRP Terrain Detail Notice", "Spawner '" + this.name + "' contains Terrain Detail Spawn Rules. These have been deactivated because Unity does not support terrain details in HDRP by itself. You can still activate these rules manually if you wish to spawn terrain details regardless, the terrain details will then be rendered as white squares on the terrain. \r\n\r\n To display terrain details in HDRP you currently need a grass / vegetation rendering system that supports HDRP and ideally can also take the spawned details from Gaia as input. We have added the beta version of such a system to Gaia Pro recently as well.", "OK", null);
                        hdTerrainDetailMessageDisplayed = true;
                    }
#endif
#endif
                }


            }

            //Reset the stored terrain layer asset guids - need to start fresh
            foreach (ResourceProtoTexture resourceProtoTexture in m_settings.m_resources.m_texturePrototypes)
            {
                resourceProtoTexture.m_LayerGUID = "";
            }

            //Try to look up all collision layer masks by their name where possible - layer orders could be different from when the spawner was saved.
            foreach (ImageMask imageMask in m_settings.m_imageMasks.Where(x => x.m_operation == ImageMaskOperation.CollisionMask))
            {
                imageMask.TryRefreshCollisionMask();
            }
            foreach (SpawnRule sr in m_settings.m_spawnerRules)
            {
                foreach (ImageMask imageMask in sr.m_imageMasks.Where(x => x.m_operation == ImageMaskOperation.CollisionMask))
                {
                    imageMask.TryRefreshCollisionMask();
                }
            }

            //Refresh texture spawn ruled GUIDs for the texture masks since new ones could be added with these settings
            ImageMask.RefreshSpawnRuleGUIDs();

            m_rulePanelUnfolded = true;
            if (m_settings.m_isWorldmapSpawner)
            {
                if (Gaia.TerrainHelper.GetTerrain(transform.position, m_settings.m_isWorldmapSpawner) != null)
                {
                    FitToTerrain();
                }
                //Since this is a new world designer, assign these settings to be world biome mask settings
                SessionManager.m_session.m_worldBiomeMaskSettings = m_settings;
                SessionManager.SaveSession();
                TerrainLoaderManager.Instance.TerrainSceneStorage.m_hasWorldMap = true;
            }
            UpdateMinMaxHeight();

        }
        #endregion

        #region Random number utils

        /// <summary>
        /// Reset the random number generator
        /// </summary>
        public void ResetRandomGenertor()
        {
            m_rndGenerator = new XorshiftPlus(m_seed);
        }

        /// <summary>
        /// Get a random integer
        /// </summary>
        /// <param name="min">Minimum value inclusive</param>
        /// <param name="max">Maximum value inclusive</param>
        /// <returns>Random integer between minimum and maximum values</returns>
        public int GetRandomInt(int min, int max)
        {
            return m_rndGenerator.Next(min, max);
        }

        /// <summary>
        /// Get a random float
        /// </summary>
        /// <param name="min">Minimum value inclusive</param>
        /// <param name="max">Maximum value inclusive</param>
        /// <returns>Random float between minimum and maximum values</returns>
        public float GetRandomFloat(float min, float max)
        {
            return m_rndGenerator.Next(min, max);
        }

        /// <summary>
        /// Get a random vector 3
        /// </summary>
        /// <param name="range">Range of values to return</param>
        /// <returns>Vector 3 in the +- range supplied</returns>
        public Vector3 GetRandomV3(float range)
        {
            return m_rndGenerator.NextVector(-range, range);
        }


        /// <summary>
        /// Saves the spawner settings into a scriptable object file. Also attaches scriptable objects / materials from the Flora System
        /// </summary>
        /// <returns></returns>
        public SpawnerSettings SaveSettings(string saveFilePath)
        {
#if UNITY_EDITOR

            //Dismiss Tutorial messages at this point
            m_createdfromBiomePreset = false;
            m_createdFromGaiaManager = false;

            if (!saveFilePath.StartsWith("Assets/"))
            {
                saveFilePath = GaiaDirectories.GetPathStartingAtAssetsFolder(saveFilePath);
            }
            m_settings.m_lastGUIDSaved = AssetDatabase.AssetPathToGUID(saveFilePath);

            if (File.Exists(saveFilePath))
            {
                AssetDatabase.DeleteAsset(saveFilePath);
            }

            AssetDatabase.CreateAsset(m_settings, saveFilePath);
            AssetDatabase.ImportAsset(saveFilePath);
            AssetDatabase.SetLabels(m_settings, new string[1] { GaiaConstants.gaiaManagerSpawnerLabel });
#if FLORA_PRESENT
            //remember the original GUIDs & IDs so we can restore them back before reloading the settings file after saving.
            //Otherwise we would create duplicates of the scriptable objects when the settings file is reloaded below.
            List<FloraLODIdOverrides>[] originalDetailFloraIDs = new List<FloraLODIdOverrides>[m_settings.m_resources.m_detailPrototypes.Length];

            //Likewise remember the asset paths for the material instance ids used in this spawner so we can restore them later
            Dictionary<string, string> materialAssetGUIDMap = new Dictionary<string, string>();

            for (int i = 0; i < m_settings.m_resources.m_detailPrototypes.Length; i++)
            {
                ResourceProtoDetail rpd = (ResourceProtoDetail)m_settings.m_resources.m_detailPrototypes[i];

                originalDetailFloraIDs[i] = new List<FloraLODIdOverrides>();
                for (int k = 0; k < rpd.m_floraLODs.Count; k++)
                {
                    originalDetailFloraIDs[i].Add(new FloraLODIdOverrides()
                    {
                        m_assetGUIDOverride = rpd.m_floraLODs[k].m_detailerSettingsObjectAssetGUID,
                        m_instanceIDOverride = rpd.m_floraLODs[k].m_detailerSettingsObjectInstanceID
                    });

                    for (int o = 0; o < rpd.m_floraLODs[k].DetailerSettingsObject.Mat.Length; o++)
                    {
                        Material mat = rpd.m_floraLODs[k].DetailerSettingsObject.Mat[o];
                        string nameAndIndex = mat.name + "_" + o.ToString();
                        if (!materialAssetGUIDMap.ContainsKey(nameAndIndex))
                        {
                            materialAssetGUIDMap.Add(nameAndIndex, AssetDatabase.GetAssetPath(mat));
                        }
                    }
                }
            }

            //save all the setting scriptable object files for the grass system / detailer
            foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.TerrainDetail))
            {
                ResourceProtoDetail resourceProtoDetail = m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx];
                FloraUtils.SaveFloraLODs(resourceProtoDetail.m_floraLODs, resourceProtoDetail.m_name, saveFilePath);
            }

            //Likewise for trees
            List<FloraLODIdOverrides>[] originalTreeFloraIDs = new List<FloraLODIdOverrides>[m_settings.m_resources.m_treePrototypes.Length];

            for (int i = 0; i < m_settings.m_resources.m_treePrototypes.Length; i++)
            {
                ResourceProtoTree rpt = (ResourceProtoTree)m_settings.m_resources.m_treePrototypes[i];
                originalTreeFloraIDs[i] = new List<FloraLODIdOverrides>();
                for (int k = 0; k < rpt.m_floraLODs.Count; k++)
                {
                    originalTreeFloraIDs[i].Add(new FloraLODIdOverrides()
                    {
                        m_assetGUIDOverride = rpt.m_floraLODs[k].m_detailerSettingsObjectAssetGUID,
                        m_instanceIDOverride = rpt.m_floraLODs[k].m_detailerSettingsObjectInstanceID
                    });
                    for (int o = 0; o < rpt.m_floraLODs[k].DetailerSettingsObject.Mat.Length; o++)
                    {
                        Material mat = rpt.m_floraLODs[k].DetailerSettingsObject.Mat[o];
                        string nameAndIndex = mat.name + "_" + o.ToString();
                        if (!materialAssetGUIDMap.ContainsKey(nameAndIndex))
                        {
                            materialAssetGUIDMap.Add(nameAndIndex, AssetDatabase.GetAssetPath(mat));
                        }
                    }
                }
            }

            //save all the setting scriptable object files for the grass system / detailer
            foreach (SpawnRule sr in m_settings.m_spawnerRules.Where(x => x.m_resourceType == SpawnerResourceType.TerrainTree))
            {
                ResourceProtoTree resourceProtoTree = m_settings.m_resources.m_treePrototypes[sr.m_resourceIdx];
                FloraUtils.SaveFloraLODs(resourceProtoTree.m_floraLODs, resourceProtoTree.m_name, saveFilePath);
            }

#endif
            EditorUtility.SetDirty(m_settings);
            AssetDatabase.SaveAssets();


            //Check if save was successful
            SpawnerSettings settingsToLoad = (SpawnerSettings)AssetDatabase.LoadAssetAtPath(saveFilePath, typeof(SpawnerSettings));
            if (settingsToLoad != null)
            {
                EditorGUIUtility.PingObject(settingsToLoad);

                //Add the saved file to the user file collection so it shows up in the Gaia Manager
                UserFiles userFiles = GaiaUtils.GetOrCreateUserFiles();
                if (userFiles.m_autoAddNewFiles)
                {
                    if (!userFiles.m_gaiaManagerSpawnerSettings.Contains(settingsToLoad))
                    {
                        userFiles.m_gaiaManagerSpawnerSettings.Add(settingsToLoad);
                    }
                }
                userFiles.PruneNonExisting();
                EditorUtility.SetDirty(userFiles);
                AssetDatabase.SaveAssets();

                //dissociate the current stamper settings from the file we just saved, otherwise the user will continue editing the file afterwards
                //We do this by just loading the file in again we just created
#if FLORA_PRESENT
                LoadSettings(settingsToLoad, false, originalDetailFloraIDs, originalTreeFloraIDs);

                //Restore the original material associations
                for (int i = 0; i < m_settings.m_resources.m_detailPrototypes.Length; i++)
                {
                    ResourceProtoDetail rpd = (ResourceProtoDetail)m_settings.m_resources.m_detailPrototypes[i];
                    for (int k = 0; k < rpd.m_floraLODs.Count; k++)
                    {
                        if (rpd.m_floraLODs[k].DetailerSettingsObject != null && rpd.m_floraLODs[k].DetailerSettingsObject.Mat != null)
                        {
                            for (int o = 0; o < rpd.m_floraLODs[k].DetailerSettingsObject.Mat.Length; o++)
                            {
                                Material mat = rpd.m_floraLODs[k].DetailerSettingsObject.Mat[o];
                                string nameAndIndex = mat.name + "_" + o.ToString();
                                if (materialAssetGUIDMap.ContainsKey(nameAndIndex))
                                {
                                    string path = "";
                                    materialAssetGUIDMap.TryGetValue(nameAndIndex, out path);
                                    if (path != "")
                                    {
                                        rpd.m_floraLODs[k].DetailerSettingsObject.Mat[o] = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < m_settings.m_resources.m_treePrototypes.Length; i++)
                {
                    ResourceProtoTree rpt = (ResourceProtoTree)m_settings.m_resources.m_treePrototypes[i];
                    for (int k = 0; k < rpt.m_floraLODs.Count; k++)
                    {
                        for (int o = 0; o < rpt.m_floraLODs[k].DetailerSettingsObject.Mat.Length; o++)
                        {
                            Material mat = rpt.m_floraLODs[k].DetailerSettingsObject.Mat[o];
                            string nameAndIndex = mat.name + "_" + o.ToString();
                            if (materialAssetGUIDMap.ContainsKey(nameAndIndex))
                            {
                                string path = "";
                                materialAssetGUIDMap.TryGetValue(nameAndIndex, out path);
                                if (path != "")
                                {
                                    rpt.m_floraLODs[k].DetailerSettingsObject.Mat[o] = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                                }
                            }
                        }
                    }
                }
#else
                LoadSettings(settingsToLoad);
#endif
                m_spawnPreviewDirty = true;
                return settingsToLoad;
            }
#endif //UNITY_EDITOR
            return null;
        }

        #endregion
    }
}
