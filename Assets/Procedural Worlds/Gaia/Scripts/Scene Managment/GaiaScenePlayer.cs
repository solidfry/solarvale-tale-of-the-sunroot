using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    [ExecuteAlways]
    public class GaiaScenePlayer : MonoBehaviour
    {
        public static GaiaScenePlayer Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GaiaUtils.FindOOT<GaiaScenePlayer>();
                }

                return m_instance;
            }
        }

        [SerializeField] private static GaiaScenePlayer m_instance;

        public GameObject m_gaiaUI;
        public List<GameObject> m_gaiaCreatedControllers = new List<GameObject>();

        private Camera m_camera;
        private Bounds m_worldSpaceBounds = new Bounds();
        private Plane[] m_planes = new Plane[6];
        private Terrain[] m_allTerrains = new Terrain[0];
        private MeshRenderer[] m_allTerrainMeshRenderers = new MeshRenderer[0];

        private void Start()
        {
            m_instance = this;
            if (!GaiaUtils.CheckIfSceneProfileExists())
            {
                return;
            }

            m_camera = GaiaGlobal.Instance.m_mainCamera;
            m_allTerrains = Terrain.activeTerrains;

            //Collect all Mesh Terrains that are present at startup
            List<MeshRenderer> tempMeshRenderers = new List<MeshRenderer>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    foreach (GameObject go in scene.GetRootGameObjects())
                    {
                        AddTerrainMeshRenderer(go, tempMeshRenderers);
                    }
                }
            }
            m_allTerrainMeshRenderers = tempMeshRenderers.ToArray();


            if (GaiaGlobal.Instance.SceneProfile.m_terrainCullingEnabled)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnLoaded;
                SceneManager.sceneUnloaded += OnSceneUnLoaded;
            }
            else
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnLoaded;
            }

            GaiaAPI.RefreshCameraCulling();

            if (Application.isPlaying)
            {
                if (m_gaiaUI != null)
                {
                    m_gaiaUI.SetActive(true);
                }
            }
        }
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (!GaiaUtils.CheckIfSceneProfileExists())
            {
                return;
            }

            if (!GaiaGlobal.Instance.SceneProfile.m_terrainCullingEnabled || m_camera == null)
            {
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(m_camera, m_planes);
            if (GaiaGlobal.Instance.SceneProfile.m_terrainCullingEnabled)
            {
                for (int i = 0; i < m_allTerrains.Length; i++)
                {
                    Terrain terrain = m_allTerrains[i];
                    if (terrain == null)
                    {
                        continue;
                    }
                    //Check needs to performed in world space, terrain bounds are in local space of the terrain
                    m_worldSpaceBounds = terrain.terrainData.bounds;
                    m_worldSpaceBounds.center = new Vector3(m_worldSpaceBounds.center.x + terrain.transform.position.x, m_worldSpaceBounds.center.y + terrain.transform.position.y, m_worldSpaceBounds.center.z + terrain.transform.position.z);

                    if (GeometryUtility.TestPlanesAABB(m_planes, m_worldSpaceBounds))
                    {
                        terrain.drawHeightmap = true;
                        terrain.drawTreesAndFoliage = true;

                        //Deactivate terrain GO entirely
                        //terrain.gameObject.SetActive(true);

                        //Activate object spawns
                        //Transform spawnsTransform = terrain.gameObject.transform.Find(GaiaConstants.defaultGOSpawnTarget);
                        //spawnsTransform.gameObject.SetActive(true);
                    }
                    else
                    {
                        terrain.drawHeightmap = false;
                        terrain.drawTreesAndFoliage = false;

                        //Deactivate terrain GO entirely
                        //terrain.gameObject.SetActive(false);

                        //Deactivate object spawns
                        //Transform spawnsTransform = terrain.gameObject.transform.Find(GaiaConstants.defaultGOSpawnTarget);
                        //spawnsTransform.gameObject.SetActive(false);
                    }
                }
            }

            if (GaiaGlobal.Instance.SceneProfile.m_terrainCullingEnabled)
            {
                for (int i = 0; i < m_allTerrainMeshRenderers.Length; i++)
                {
                    MeshRenderer mr = m_allTerrainMeshRenderers[i];
                    if (mr != null)
                    {
                        if (GeometryUtility.TestPlanesAABB(m_planes, mr.bounds))
                        {
                            mr.enabled = true;
                        }
                        else
                        {
                            mr.enabled = false;
                        }
                    }
                }
            }
             
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        }
        private void OnValidate()
        {
            m_instance = this;

            if (!GaiaUtils.CheckIfSceneProfileExists())
            {
                return;
            }
            
            GaiaAPI.RefreshCameraCulling();
        }

        /// <summary>
        /// Adds a controller to the stored data
        /// </summary>
        /// <param name="controllerObject"></param>
        public static void AddControllerObject(GameObject controllerObject)
        {
            GaiaScenePlayer gaiaPlayer = Instance;
            if (gaiaPlayer != null)
            {
                if (controllerObject != null)
                {
                    gaiaPlayer.m_gaiaCreatedControllers.Add(controllerObject);
                }
            }
        }
        /// <summary>
        /// Removes all the controllers
        /// Returns the player controller
        /// </summary>
        /// <param name="checkForCustomControllerComponent"></param>
        public static GameObject RemoveAllControllers(bool checkForCustomControllerComponent)
        {
            GameObject playerObject = null;
            GaiaScenePlayer gaiaPlayer = Instance;
            if (gaiaPlayer != null)
            {
                if (gaiaPlayer.m_gaiaCreatedControllers.Count == 0)
                {
                    Transform[] otherTransforms = gaiaPlayer.GetComponentsInChildren<Transform>();
                    if (otherTransforms.Length > 0)
                    {
                        foreach (Transform otherTransform in otherTransforms)
                        {
                            if (otherTransform != gaiaPlayer.transform)
                            {
                                AddControllerObject(otherTransform.gameObject);
                            }
                        }
                    }
                }

                if (gaiaPlayer.m_gaiaCreatedControllers.Count > 0)
                {
                    foreach (GameObject controller in gaiaPlayer.m_gaiaCreatedControllers)
                    {
                        if (controller != null)
                        {
                            if (checkForCustomControllerComponent)
                            {
                                CustomGaiaController customController = controller.GetComponent<CustomGaiaController>();
                                if (customController != null)
                                {
                                    if (customController.m_isPlayer)
                                    {
                                        playerObject = controller;
                                    }
                                }

                                DestroyImmediate(controller);
                            }
                            else
                            {
                                if (!controller.TryGetComponent(out Camera camera))
                                {
                                    playerObject = controller;
                                }

                                DestroyImmediate(controller);
                            }
                        }
                    }
                }

                gaiaPlayer.m_gaiaCreatedControllers.Clear();
            }

            return playerObject;
        }

        private void AddTerrainMeshRenderer(GameObject go, List<MeshRenderer> meshRenderers)
        {
            if (IsSingleMeshTerrain(go))
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (!meshRenderers.Contains(mr) && !m_allTerrainMeshRenderers.Contains(mr))
                    {
                        meshRenderers.Add(mr);
                    }
                }
                return;
            }

            if (IsMeshLODTerrain(go))
            {
                LODGroup lg = go.GetComponent<LODGroup>();
                foreach (LOD lod in lg.GetLODs())
                {
                    foreach (Renderer renderer in lod.renderers)
                    {
                        if (renderer != null && renderer.GetType() == typeof(MeshRenderer))
                        {
                            MeshRenderer mr = (MeshRenderer)renderer;

                            if (!meshRenderers.Contains(mr))
                            {
                                meshRenderers.Add((MeshRenderer)mr);
                            }
                        }
                    }
                }
            }
        }
        private bool IsSingleMeshTerrain(GameObject go)
        {
            //Two possible things to find here: terrains that have been converted to a single mesh terrain or impostor terrains WITHOUT a LOD Group.
            string searchString = GaiaConstants.MeshTerrainName;
            string searchString2 = GaiaConstants.ImpostorTerrainName;
            if (go.name.StartsWith(searchString) || (go.name.StartsWith(searchString2) && go.GetComponent<LODGroup>() == null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsMeshLODTerrain(GameObject go)
        {
            //Two possible things to find here: terrains that have been converted to a mesh terrain with a LOD Group, or impostor terrains with a LOD Group.
            string searchString1 = GaiaConstants.MeshTerrainLODGroupPrefix;
            string searchString2 = GaiaConstants.ImpostorTerrainName;
            if (go.name.StartsWith(searchString1) || (go.name.StartsWith(searchString2) && go.GetComponent<LODGroup>()!=null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Terrain Culling
        private void OnSceneUnLoaded(Scene scene)
        {
            Invoke("UpdateTerrains", 0.5f);

            m_allTerrainMeshRenderers = m_allTerrainMeshRenderers.Where(x => x != null).ToArray();
            


        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            Invoke("UpdateTerrains", 0.5f);
            List<MeshRenderer> tempMeshRenderers = new List<MeshRenderer>();
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                AddTerrainMeshRenderer(go, tempMeshRenderers);
            }
            m_allTerrainMeshRenderers = m_allTerrainMeshRenderers.Concat(tempMeshRenderers).ToArray();
        }
        private void UpdateTerrains()
        {
            m_allTerrains = Terrain.activeTerrains;
        }
        //Camera Culling
        public static void UpdateCullingDistances()
        {
            if (!GaiaUtils.CheckIfSceneProfileExists())
            {
                return;
            }

            SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
            if (sceneProfile.CullingProfile == null)
            {
                return;
            }

            GaiaSceneCullingProfile cullingProfile = sceneProfile.CullingProfile;
#if GAIA_PRO_PRESENT
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                if (ProceduralWorldsGlobalWeather.Instance.CheckIsNight())
                {
                    sceneProfile.m_sunLight = ProceduralWorldsGlobalWeather.Instance.m_moonLight;
                }
                else
                {
                    sceneProfile.m_sunLight = ProceduralWorldsGlobalWeather.Instance.m_sunLight;
                }
            }
            else
            {
                if (sceneProfile.m_sunLight == null)
                {
                    sceneProfile.m_sunLight = GaiaUtils.GetMainDirectionalLight(false);
                }
            }
#else
            if (sceneProfile.m_sunLight == null)
            {
                sceneProfile.m_sunLight = GaiaUtils.GetMainDirectionalLight();
            }
#endif

            //Make sure we have distances
            if (cullingProfile.m_layerDistances == null || cullingProfile.m_layerDistances.Length != 32)
            {
                return;
            }

            if (sceneProfile.m_enableLayerCulling)
            {
                //Apply to main camera
                float[] newCullingDistance = new float[32];
                for (int i = 0; i < newCullingDistance.Length; i++)
                {
                    newCullingDistance[i] = cullingProfile.m_layerDistances[i];
                    if (newCullingDistance[i] != 0f)
                    {
                        if (cullingProfile.m_additionalCullingDistance >= 0)
                        {
                            newCullingDistance[i] += cullingProfile.m_additionalCullingDistance;
                        }
                        else
                        {
                            newCullingDistance[i] -= Mathf.Abs(cullingProfile.m_additionalCullingDistance);
                            Mathf.Clamp(newCullingDistance[i], 0.01f, Mathf.Infinity);
                        }
                    }
                }

                if (GaiaGlobal.Instance.m_mainCamera != null)
                {
                    GaiaGlobal.Instance.m_mainCamera.layerCullDistances = newCullingDistance;
                }

                //Apply to sun/moon light
                float[] newShadowCullingDistance = new float[32];
                for (int i = 0; i < newShadowCullingDistance.Length; i++)
                {
                    newShadowCullingDistance[i] = cullingProfile.m_shadowLayerDistances[i];
                    if (newShadowCullingDistance[i] != 0f)
                    {
                        if (cullingProfile.m_additionalCullingDistance >= 0)
                        {
                            newShadowCullingDistance[i] += cullingProfile.m_additionalCullingDistance;
                        }
                        else
                        {
                            newShadowCullingDistance[i] -= Mathf.Abs(cullingProfile.m_additionalCullingDistance);
                            Mathf.Clamp(newShadowCullingDistance[i], 0.01f, Mathf.Infinity);
                        }
                    }
                }

                if (sceneProfile.m_sunLight != null)
                {
                    sceneProfile.m_sunLight.layerShadowCullDistances = newShadowCullingDistance;
                }

                if (sceneProfile.m_moonLight != null)
                {
                    sceneProfile.m_moonLight.layerShadowCullDistances = newShadowCullingDistance;
                }
            }
            else
            {
                float[] layerCulls = new float[32];
                for (int i = 0; i < layerCulls.Length; i++)
                {
                    layerCulls[i] = 0f;
                }

                //Apply to main camera
                if (GaiaGlobal.Instance.m_mainCamera != null)
                {
                    GaiaGlobal.Instance.m_mainCamera.layerCullDistances = layerCulls;
                }

                //Apply to sun/moon light
                if (sceneProfile.m_sunLight != null)
                {
                    sceneProfile.m_sunLight.layerShadowCullDistances = layerCulls;
                }
                if (sceneProfile.m_moonLight != null)
                {
                    sceneProfile.m_moonLight.layerShadowCullDistances = layerCulls;
                }
            }
        }
        public static void ApplySceneSetup(bool active)
        {
            //Apply to editor camera
#if UNITY_EDITOR
            SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
            if (sceneProfile.m_enableLayerCulling)
            {
                GaiaSceneCullingProfile cullingProfile = sceneProfile.CullingProfile;
                if (cullingProfile != null)
                {
                    if (active)
                    {
                        foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                        {
                            float[] newCullingDistance = new float[32];
                            for (int i = 0; i < newCullingDistance.Length; i++)
                            {
                                newCullingDistance[i] = cullingProfile.m_layerDistances[i];
                                if (cullingProfile.m_additionalCullingDistance >= 0)
                                {
                                    newCullingDistance[i] += cullingProfile.m_additionalCullingDistance;
                                }
                                else
                                {
                                    newCullingDistance[i] -= Mathf.Abs(cullingProfile.m_additionalCullingDistance);
                                    Mathf.Clamp(newCullingDistance[i], 0.01f, Mathf.Infinity);
                                }
                            }

                            sceneCamera.layerCullDistances = newCullingDistance;
                        }


                        float[] newShadowCullingDistance = new float[32];
                        for (int i = 0; i < newShadowCullingDistance.Length; i++)
                        {
                            newShadowCullingDistance[i] = cullingProfile.m_shadowLayerDistances[i];
                            if (cullingProfile.m_additionalCullingDistance >= 0)
                            {
                                newShadowCullingDistance[i] += cullingProfile.m_additionalCullingDistance;
                            }
                            else
                            {
                                newShadowCullingDistance[i] -= Mathf.Abs(cullingProfile.m_additionalCullingDistance);
                                Mathf.Clamp(newShadowCullingDistance[i], 0.01f, Mathf.Infinity);
                            }
                        }

                        if (sceneProfile.m_sunLight != null)
                        {
                            sceneProfile.m_sunLight.layerShadowCullDistances = newShadowCullingDistance;
                        }

                        if (sceneProfile.m_moonLight != null)
                        {
                            sceneProfile.m_moonLight.layerShadowCullDistances = newShadowCullingDistance;
                        }
                    }
                    else
                    {
                        foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                        {
                            float[] layers = new float[32];
                            for (int i = 0; i < layers.Length; i++)
                            {
                                layers[i] = 0f;
                            }

                            sceneCamera.layerCullDistances = layers;
                        }

                        if (sceneProfile.m_sunLight != null)
                        {
                            float[] layers = new float[32];
                            for (int i = 0; i < layers.Length; i++)
                            {
                                layers[i] = 0f;
                            }

                            sceneProfile.m_sunLight.layerShadowCullDistances = layers;
                        }

                        if (sceneProfile.m_moonLight != null)
                        {
                            float[] layers = new float[32];
                            for (int i = 0; i < layers.Length; i++)
                            {
                                layers[i] = 0f;
                            }

                            sceneProfile.m_moonLight.layerShadowCullDistances = layers;
                        }
                    }
                }
            }
            else
            {
                foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                {
                    float[] layers = new float[32];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = 0f;
                    }

                    sceneCamera.layerCullDistances = layers;
                }

                if (sceneProfile.m_sunLight != null)
                {
                    float[] layers = new float[32];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = 0f;
                    }
                    sceneProfile.m_sunLight.layerShadowCullDistances = layers;
                }

                if (sceneProfile.m_moonLight != null)
                {
                    float[] layers = new float[32];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = 0f;
                    }
                    sceneProfile.m_moonLight.layerShadowCullDistances = layers;
                }
            }
#endif
        }
        //Controller Setup
        /// <summary>
        /// Sets the current controller type
        /// </summary>
        /// <param name="type"></param>
        public static void SetCurrentControllerType(GaiaConstants.EnvironmentControllerType type)
        {
            LocationSystem system = GaiaUtils.FindOOT<LocationSystem>();
            if (system != null)
            {
                if (system.m_locationProfile != null)
                {
                    system.m_locationProfile.m_currentControllerType = type;
                }
            }
        }
    }
}