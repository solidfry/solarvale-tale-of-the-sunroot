using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
#if PW_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Gaia
{
    [System.Serializable]
    public enum LoadState { Loaded, Cached, Unloaded }

    public enum ReferenceChange { AddImpostorReference, AddRegularReference, RemoveImpostorReference, RemoveRegularReference }

    public class TerrainSceneActionQueueEntry { public TerrainScene m_terrainScene; public ReferenceChange m_referenceChange; public bool m_forced = false; public float m_distance = 0f; }

    [System.Serializable]
    public class TerrainScene
    {
        private List<GameObject> m_regularReferences = new List<GameObject>();
        public List<GameObject> RegularReferences { get { return m_regularReferences; } }
        private List<GameObject> m_impostorReferences = new List<GameObject>();
        public List<GameObject> ImpostorReferences { get { return m_impostorReferences; } }
        public Vector3Double m_pos;
        public Vector3Double m_currentOriginOffset;
        public BoundsDouble m_bounds;
        public string m_scenePath;
        public string m_impostorScenePath;
        public string m_backupScenePath;
        public string m_colliderScenePath;
        public LoadState m_regularLoadState;
        public LoadState m_impostorLoadState;
        public bool m_useFloatingPointFix;
        public long m_nextUpdateTimestamp;
        private AsyncOperation m_asyncLoadOp;
        public bool m_regularLoadRequested;
        public bool m_regularUnloadRequested;
        public bool m_impostorLoadRequested;
        public bool m_impostorUnloadRequested;
        private long m_loadCacheTreshold = 4294967296;
        private bool m_forceSceneRemove = false;
        private List<GameObject> m_rootGameObjects = new List<GameObject>();
        public bool m_isFoldedOut;
        private GameObject m_terrainObj;
        public long m_regularCachedTimestamp;
        public long m_impostorCachedTimestamp;
        public bool m_stitchedWestBorder;
        public bool m_stitchedEastBorder;
        public bool m_stitchedSouthBorder;
        public bool m_stitchedNorthBorder;

#if UNITY_EDITOR
        public GameObject TerrainObj { get => m_terrainObj; }
#endif
        public string GetTerrainName(string path = "")
        {
            if (path == "")
            {
                path = m_scenePath;
            }
            if (path.LastIndexOf("Terrain") >= 0)
            {
                return path.Substring(path.LastIndexOf("Terrain")).Replace(".unity", "");
            }
            else
            {
                Debug.LogError($"Could not get the Terrain Name for the following path '{m_scenePath}'.");
                return "";
            }
        }

        public string GetImpostorName(string path = "")
        {
            if (path == "")
            {
                path = m_impostorScenePath;
            }
            if (path.LastIndexOf("Impostor") >= 0)
            {
                return path.Substring(path.LastIndexOf("Impostor")).Replace(".unity", "");
            }
            else
            {
                Debug.LogError($"Could not get the Impostor Name for the following path '{m_impostorScenePath}'.");
                return "";
            }
        }

        public void RemoveAllReferences(bool forceSceneRemove = false)
        {
            m_regularReferences.Clear();
            m_impostorReferences.Clear();
#if GAIA_PRO_PRESENT
            m_regularLoadRequested = false;
            m_regularUnloadRequested = false;
            m_impostorLoadRequested = false;
            m_impostorUnloadRequested = false;
            m_forceSceneRemove = forceSceneRemove;
            UpdateLoadState(m_regularReferences, ref m_regularLoadState, ref m_regularLoadRequested, ref m_regularUnloadRequested, false);
            UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
#endif

        }

        public void RemoveAllImpostorReferences(bool forceSceneRemove = false)
        {
            m_impostorReferences.Clear();
#if GAIA_PRO_PRESENT
            m_impostorLoadRequested = false;
            m_impostorUnloadRequested = false;
            m_forceSceneRemove = forceSceneRemove;
            UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
#endif

        }

        public void RemoveAllRegularReferences(bool forceSceneRemove = false)
        {
            m_regularReferences.Clear();
#if GAIA_PRO_PRESENT
            m_regularLoadRequested = false;
            m_regularUnloadRequested = false;
            m_forceSceneRemove = forceSceneRemove;
            UpdateLoadState(m_regularReferences, ref m_regularLoadState, ref m_regularLoadRequested, ref m_regularUnloadRequested, true);
#endif

        }

        public void AddRegularReference(GameObject gameObject)
        {
            if (!m_regularReferences.Contains(gameObject))
            {
                m_forceSceneRemove = false;
                m_regularReferences.Add(gameObject);
            }
            UpdateLoadState(m_regularReferences, ref m_regularLoadState, ref m_regularLoadRequested, ref m_regularUnloadRequested, false);
        }

        public void RemoveRegularReference(GameObject gameObject, long cacheSize = 0, bool forceSceneRemove = false)
        {
            m_forceSceneRemove = forceSceneRemove;
            if (m_regularReferences.Contains(gameObject))
            {
                m_regularReferences.Remove(gameObject);
            }
            if (cacheSize != 0)
            {
                m_loadCacheTreshold = cacheSize;
            }
            UpdateLoadState(m_regularReferences, ref m_regularLoadState, ref m_regularLoadRequested, ref m_regularUnloadRequested, false);
        }


        public bool HasRegularReference(GameObject gameObject)
        {
            return m_regularReferences.Contains(gameObject);
        }

        public void AddImpostorReference(GameObject gameObject)
        {
            if (!String.IsNullOrEmpty(m_impostorScenePath))
            {
                if (!m_impostorReferences.Contains(gameObject))
                {
                    m_forceSceneRemove = false;
                    m_impostorReferences.Add(gameObject);
                }
                UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
            }
        }

        public void RemoveImpostorReference(GameObject gameObject, long cacheSize = 0, bool forceSceneRemove = false)
        {
            if (!String.IsNullOrEmpty(m_impostorScenePath))
            {
                m_forceSceneRemove = forceSceneRemove;
                if (m_impostorReferences.Contains(gameObject))
                {
                    m_impostorReferences.Remove(gameObject);
                }
                if (cacheSize != 0)
                {
                    m_loadCacheTreshold = cacheSize;
                }
                UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
            }
        }

        public bool HasImpostorReference(GameObject gameObject)
        {
            return m_impostorReferences.Contains(gameObject);
        }

        public void UpdateLoadState(List<GameObject> referenceList, ref LoadState loadState, ref bool loadRequested, ref bool unLoadRequested, bool isImpostor)
        {
            string scenePath = m_scenePath;
            if (isImpostor)
            {
                scenePath = m_impostorScenePath;
            }
            if (TerrainLoaderManager.ColliderOnlyLoadingActive)
            {
                scenePath = m_colliderScenePath;
            }

#if GAIA_PRO_PRESENT
            if (scenePath == null)
            {
                return;
            }
            //sanity check on the references: does the GO still exist?
            CheckForRelevance(referenceList);
            //locked - no state change for now, used when the loader is dragged around in the scene, etc.
#if UNITY_EDITOR
            Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
#else
            Scene scene = SceneManager.GetSceneByPath(scenePath);
#endif
            SyncLoadState(scene, ref loadState, ref loadRequested, ref unLoadRequested, isImpostor);

            switch (loadState)
            {
                case LoadState.Loaded:
                    if (referenceList.Count <= 0 && scene.isLoaded)
                    {
                        UnloadScene(scene, ref loadRequested, ref unLoadRequested, ref loadState, isImpostor);
                        //if a regular terrain was just unloaded, do an update on the impostor to see if it needs to be loaded in
                        if (!isImpostor && !String.IsNullOrEmpty(m_impostorScenePath))
                        {
                            UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
                        }
                    }
                    if (referenceList.Count >= 1)
                    {
                        //Is this a regular scene and the Impostor is still loaded? Remove it!
                        if (!isImpostor && m_impostorLoadState == LoadState.Loaded)
                        {
                            ReplaceImpostor();
                        }
                    }

                    break;
                case LoadState.Cached:
                    if (referenceList.Count >= 1 && scene.isLoaded)
                    {
                        //do not need to activate the cached scene if this is an imposter and the regular scene has references and is loaded in already
                        if (isImpostor && m_regularReferences.Count >= 1 && m_regularLoadState == LoadState.Loaded)
                        {
                            break;
                        }

                        foreach (GameObject go in scene.GetRootGameObjects())
                        {
                            go.SetActive(true);
                            Terrain terrain = go.GetComponent<Terrain>();
                            if (terrain != null)
                            {
                                //rwactivate terrain / trees in case if culled
                                terrain.drawTreesAndFoliage = true;
                                terrain.drawHeightmap = true;
                                m_terrainObj = go;
                                CheckForStitching(terrain);
#if FLORA_PRESENT
                                //Wake up the flora detail objects, if any
                                foreach (ProceduralWorlds.Flora.DetailObject detailObject in terrain.transform.GetComponentsInChildren<ProceduralWorlds.Flora.DetailObject>())
                                {
                                    detailObject.RefreshAll();
                                }
#endif
                            }


                        }
                        //Is this a regular scene that just came out of cache and the Impostor is still loaded? Remove it!
                        if (!isImpostor && m_impostorLoadState == LoadState.Loaded)
                        {
                            ReplaceImpostor();
                        }
                        loadState = LoadState.Loaded;
                    }
                    //we still need to process force removal requests
                    if (referenceList.Count <= 0 && scene.isLoaded && m_forceSceneRemove)
                    {
                        UnloadScene(scene, ref loadRequested, ref unLoadRequested, ref loadState, isImpostor);
                    }
                    break;
                case LoadState.Unloaded:
                    if (referenceList.Count >= 1 && !scene.isLoaded)
                    {
                        //do not need to load if this is an imposter and the regular scene has references and is loaded in already anyways
                        if (isImpostor && m_regularReferences.Count >= 1 && m_regularLoadState == LoadState.Loaded)
                        {
                            break;
                        }
                        LoadScene(scene, ref loadRequested, ref loadState, isImpostor);
                    }
                    break;
            }

#if UNITY_EDITOR
            scene = EditorSceneManager.GetSceneByPath(scenePath);
#else
            scene = SceneManager.GetSceneByPath(scenePath);
#endif

            SyncLoadState(scene, ref loadState, ref loadRequested, ref unLoadRequested, isImpostor);
#endif
        }


        /// <summary>
        /// Tries to get the x and z coordinate for a terrain scene from its filename (assuming it follows the gaia naming scheme)
        /// </summary>
        /// <param name="fileName">The filename to get the coordinates from</param>
        /// <param name="xCoord">int to store the x Coordinate in</param>
        /// <param name="zCoord">int to store the z Coordinate in</param>
        /// <returns></returns>
        public static bool GetCoords(string fileName, ref int xCoord, ref int zCoord)
        {
            string firstSegment = fileName.Split('-')[0];
            xCoord = -99;
            zCoord = -99;
            bool successX, successZ;
            try
            {
                successX = Int32.TryParse(firstSegment.Substring(firstSegment.IndexOf('_') + 1, firstSegment.LastIndexOf('_') - (firstSegment.IndexOf('_') + 1)), out xCoord);
                successZ = Int32.TryParse(firstSegment.Substring(firstSegment.LastIndexOf('_') + 1, firstSegment.Length - 1 - firstSegment.LastIndexOf('_')), out zCoord);
            }
            catch (Exception ex)
            {
                if (ex.Message == "123")
                {
                }
                successX = false;
                successZ = false;
            }
            return successX & successZ;
        }


        /// <summary>
        /// Synchronizes the load state of the unity scene object with Gaias terrain scene. There are edge cases where those two can get out of sync, e.g. race conditions, users opening
        /// and closing scenes manually etc. so it can be required to compare the load state of the two object types.
        /// </summary>
        /// <param name="scene"></param>
        private void SyncLoadState(Scene scene, ref LoadState loadState, ref bool loadRequested, ref bool unLoadRequested, bool isImpostor)
        {
            if (scene.isLoaded)
            {
                bool isCached = true;
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go.activeInHierarchy)
                    {
                        isCached = false;
                    }
                }
                if (isCached)
                {

                    if (loadState != LoadState.Cached)
                    {
                        //Here we discover by syncing the load state that the terrain is actually cached, so we need to update the timestamp.
                        if (isImpostor)
                        {
                            m_impostorCachedTimestamp = GaiaUtils.GetUnixTimestamp();
                        }
                        else
                        {
                            m_regularCachedTimestamp = GaiaUtils.GetUnixTimestamp();
                        }
                    }
                    loadState = LoadState.Cached;
                }
                else
                {


                    loadState = LoadState.Loaded;
                }

                loadRequested = false;
            }
            else
            {
                loadState = LoadState.Unloaded;
                unLoadRequested = false;
            }
        }

        private void LoadScene(Scene scene, ref bool loadRequested, ref LoadState loadState, bool isImpostor)
        {
            string scenePath = m_scenePath;
            if (isImpostor)
            {
                scenePath = m_impostorScenePath;
            }
            if (TerrainLoaderManager.ColliderOnlyLoadingActive)
            {
                scenePath = m_colliderScenePath;
            }
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (!loadRequested)
                {
                    TerrainLoaderManager tlm = TerrainLoaderManager.Instance;
                    long currentTimeStamp = GaiaUtils.GetUnixTimestamp();
                    //don't load if the loading treshold has not passed yet
                    //this is to prevent a bottleneck from loading too many terrains at the same time
                    if (tlm.m_lastTerrainLoadedTimeStamp + tlm.m_terrainLoadingTresholdMS > currentTimeStamp)
                    {
                        return;
                    }

                    if (tlm.TerrainSceneStorage.m_useAddressables)
                    {
#if PW_ADDRESSABLES
                        LoadAddressableSceneAsync(isImpostor);
#endif
                    }
                    else
                    {
                        m_asyncLoadOp = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                        tlm.m_lastTerrainLoadedTimeStamp = currentTimeStamp;

                        if (isImpostor)
                        {
                            m_asyncLoadOp.completed += SceneLoadCompletedImpostor;
                        }
                        else
                        {
                            m_asyncLoadOp.completed += SceneLoadCompletedRegular;
                        }
                    }
                    loadRequested = true;
                }
            }
            else
            {
                TerrainLoaderManager.Instance.CheckAssemblyReloadThreshold();

                if (!TerrainLoaderManager.Instance.m_assembliesAreReloading)
                {
                    try
                    {
                        ProgressBar.Show(ProgressBarPriority.TerrainLoading, "Loading Terrain", "Loading Terrain...", 0, 0, false, false);
                        EditorSceneManager.sceneOpened += SceneOpened;
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                        ProgressBar.Clear(ProgressBarPriority.TerrainLoading);
                    }
                    catch (Exception ex)
                    {
                        if (ex != null)
                        {
                            //just to shut up the warning
                        }
                        //Make sure the Terrain Loader Manager is subscribed to the assembly loading events
                        TerrainLoaderManager.Instance.SubscribeToAssemblyReloadEvents();
                    }
                }
            }
#else
            if (!loadRequested)
            {
                TerrainLoaderManager tlm = TerrainLoaderManager.Instance;
                long currentTimeStamp = GaiaUtils.GetUnixTimestamp();
                //don't load if the loading treshold has not passed yet
                //this is to prevent a bottleneck from loading too many terrains at the same time
                if (tlm.m_lastTerrainLoadedTimeStamp + tlm.m_terrainLoadingTresholdMS > currentTimeStamp)
                {
                    return;
                }
                tlm.m_lastTerrainLoadedTimeStamp = currentTimeStamp;
                if (tlm.TerrainSceneStorage.m_useAddressables)
                {
#if PW_ADDRESSABLES
                    LoadAddressableSceneAsync(isImpostor);
#endif
                }
                else { 
                    m_asyncLoadOp = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                    
                    if (isImpostor)
                    {
                        m_asyncLoadOp.completed += SceneLoadCompletedImpostor;
                    }
                    else
                    {
                        m_asyncLoadOp.completed += SceneLoadCompletedRegular;
                    }
                }
                loadRequested = true;
            }
#endif
        }


#if PW_ADDRESSABLES
        private void LoadAddressableSceneAsync(bool isImpostor)
        {
            if (isImpostor)
            {
                Addressables.GetDownloadSizeAsync(m_impostorScenePath).Completed += AddressableImpostorDownloadSizeCompleted;

                if (TerrainLoaderManager.Instance.TerrainSceneStorage.m_preloadAddressablesWithImpostors)
                {
                    Addressables.GetDownloadSizeAsync(m_scenePath).Completed += AddressableRegularDownloadSizeCompletedPreload;
                }
            }
            else
            {
                //Check the download size, continue from there
                Addressables.GetDownloadSizeAsync(m_scenePath).Completed += AddressableRegularDownloadSizeCompleted;
            }
        }

        private void AddressableRegularDownloadSizeCompletedPreload(AsyncOperationHandle<long> obj)
        {
            //If the download size is greater than 0, download all the dependencies.
            if (obj.Result > 0)
            {
                Addressables.DownloadDependenciesAsync(m_scenePath).Completed += AddressableDownloadCompletedRegularPreload;
            }
        }


        private void AddressableRegularDownloadSizeCompleted(AsyncOperationHandle<long> obj)
        {
            //If the download size is greater than 0, download all the dependencies.
            if (obj.Result > 0)
            {
                Addressables.DownloadDependenciesAsync(m_scenePath).Completed += AddressableDownloadCompletedRegular;
            }
            else
            {
                Addressables.LoadSceneAsync(m_scenePath, LoadSceneMode.Additive).Completed += AddressableLoadCompletedRegular;
            }
        }

        private void AddressableImpostorDownloadSizeCompleted(AsyncOperationHandle<long> obj)
        {
            //If the download size is greater than 0, download all the dependencies.
            if (obj.Result > 0)
            {
                Addressables.DownloadDependenciesAsync(m_impostorScenePath).Completed += AddressableDownloadCompletedImpostor;
            }
            else
            {
                Addressables.LoadSceneAsync(m_impostorScenePath, LoadSceneMode.Additive).Completed += AddressableLoadCompletedImpostor;
            }
        }

        private void AddressableDownloadCompletedRegular(AsyncOperationHandle obj)
        {
            Addressables.LoadSceneAsync(m_scenePath, LoadSceneMode.Additive).Completed += AddressableLoadCompletedRegular;
        }

        private void AddressableDownloadCompletedRegularPreload(AsyncOperationHandle obj)
        {
        }

        private void AddressableDownloadCompletedImpostor(AsyncOperationHandle obj)
        {
            Addressables.LoadSceneAsync(m_impostorScenePath, LoadSceneMode.Additive).Completed += AddressableLoadCompletedImpostor;
        }
#endif

        /// <summary>
        /// Will update the load state of the terrain scene for all scene types - regular, collider, impostor, with the data that is currently stored in the terrain scene.
        /// </summary>
        public void UpdateWithCurrentData()
        {
            UpdateLoadState(m_regularReferences, ref m_regularLoadState, ref m_regularLoadRequested, ref m_regularUnloadRequested, false);
            UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
        }

        private void UnloadScene(Scene scene, ref bool loadRequested, ref bool unLoadRequested, ref LoadState loadState, bool isImpostor, bool bypassTreshold = false)
        {

            if (Application.isPlaying)
            {
                TerrainLoaderManager tlm = TerrainLoaderManager.Instance;
                long currentTimeStamp = GaiaUtils.GetUnixTimestamp();
                //don't unload if the loading treshold has not passed yet
                //this is to prevent a bottleneck from loading too many terrains at the same time
                if (tlm.m_lastTerrainLoadedTimeStamp + tlm.m_terrainLoadingTresholdMS > currentTimeStamp && !bypassTreshold)
                {
                    return;
                }
                tlm.m_lastTerrainLoadedTimeStamp = currentTimeStamp;
            }


            //Make sure that loading again is allowed
            loadRequested = false;

            //If this is not a request to unload an impostor, but the scene would have one that is referenced, we must not unload this scene until the impostor scene is loaded in.
            if (!isImpostor && !String.IsNullOrEmpty(m_impostorScenePath) && m_impostorReferences.Count >= 1 && m_impostorLoadState != LoadState.Loaded)
            {
                //Force an update on the impostor, since it should be able to load in now with the regular scene having no references anymore.
                UpdateLoadState(m_impostorReferences, ref m_impostorLoadState, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, true);
                return;
            }

            if (Profiler.GetTotalReservedMemoryLong() < m_loadCacheTreshold && !m_forceSceneRemove && TerrainLoaderManager.Instance.CachingAllowed())
            {
                //only deactivate the root game objects first, actual unload will happen later
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    go.SetActive(false);

                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (scene.isDirty)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                }
#endif

                unLoadRequested = false;
                loadState = LoadState.Cached;
                if (isImpostor)
                {
                    m_impostorCachedTimestamp = GaiaUtils.GetUnixTimestamp();
                }
                else
                {
                    m_regularCachedTimestamp = GaiaUtils.GetUnixTimestamp();
                }
            }
            else
            {
                //conditions for caching not met, do full unload now
                if (Application.isPlaying)
                {
                    if (!unLoadRequested)
                    {
                        if (TerrainLoaderManager.Instance.m_unloadUnusedAssetsRuntime)
                        {
                            m_asyncLoadOp = SceneManager.UnloadSceneAsync(scene.name);
                            m_asyncLoadOp.completed += UnloadCompleted;
                        }
                        loadState = LoadState.Unloaded;
                        unLoadRequested = true;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    ProgressBar.Show(ProgressBarPriority.TerrainLoading, "Unloading Terrain", "Unloading Terrain...", 0, 0, false, false);
                    if (scene.isDirty)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                    EditorSceneManager.CloseScene(scene, true);
                    if (TerrainLoaderManager.Instance.m_unloadUnusedAssetsEditor)
                    {
                        UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
                    }
                    //TerrainLoaderManager.Instance.EmptyCache();
                    ProgressBar.Clear(ProgressBarPriority.TerrainLoading);
                    loadState = LoadState.Unloaded;
                    unLoadRequested = false;
#endif
                }
            }



        }

        private void CheckForRelevance(List<GameObject> referenceList)
        {
#if GAIA_PRO_PRESENT
            for (int i = referenceList.Count - 1; i >= 0; i--)
            {
                if (referenceList[i] == null)
                {
                    referenceList.RemoveAt(i);
                }
                else
                {
                    //Is it still relevant?
                    TerrainLoader loader = referenceList[i].GetComponent<TerrainLoader>();
                    if (loader != null)
                    {
                        if (!loader.enabled || !loader.gameObject.activeInHierarchy || loader.LoadMode == LoadMode.Disabled || (!loader.m_isSelected && loader.LoadMode == LoadMode.EditorSelected))
                        {
                            referenceList.RemoveAt(i);
                        }
                    }
                }
            }
#endif
        }

#if UNITY_EDITOR
        private void SceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (m_scenePath == null)
            {
                return;
            }

            //there is an edge case when the scene opened event is fired but the scene is still not loaded
            if (scene.isLoaded == false)
            {
                if (scene.path == m_scenePath || scene.path == m_colliderScenePath)
                {
                    m_regularLoadState = LoadState.Unloaded;
                }
                if (scene.path == m_impostorScenePath)
                {
                    m_impostorLoadState = LoadState.Unloaded;
                }
                return;
            }


            if (scene.path == m_scenePath || scene.path == m_colliderScenePath)
            {
                if (m_regularLoadState != LoadState.Loaded)
                {
                    Terrain terrain = null;
                    foreach (GameObject go in scene.GetRootGameObjects())
                    {
                        terrain = go.GetComponent<Terrain>();
                        if (terrain != null)
                        {
                            m_terrainObj = go;
                            go.transform.position = m_bounds.center - m_currentOriginOffset - new Vector3Double(m_bounds.size.x / 2f, 0f, m_bounds.size.z / 2f);
                        }
                        go.SetActive(true);
                    }
                    m_regularLoadState = LoadState.Loaded;
                    ReplaceImpostor();
                    if (terrain != null)
                    {
                        CheckForStitching(terrain);
                    }

                }
                EditorSceneManager.sceneOpened -= SceneOpened;
            }

            if (scene.path == m_impostorScenePath)
            {
                if (m_impostorLoadState != LoadState.Loaded)
                {
                    foreach (GameObject go in scene.GetRootGameObjects())
                    {
                        go.SetActive(true);
                    }
                    m_impostorLoadState = LoadState.Loaded;

                    //Make sure the regular scene has not been loaded in the meantime due to a race condition. If it has, remove the impostor right away.
                    if (m_regularReferences.Count >= 1 && m_regularLoadState == LoadState.Loaded)
                    {
                        ReplaceImpostor();
                    }

                }
                EditorSceneManager.sceneOpened -= SceneOpened;
            }

        }
#endif
#if PW_ADDRESSABLES
        private void AddressableLoadCompletedRegular(AsyncOperationHandle<SceneInstance> obj)
        {
            SceneLoadCompletedHandling();
            obj.Completed -= AddressableLoadCompletedRegular;
        }

        private void AddressableLoadCompletedImpostor(AsyncOperationHandle<SceneInstance> obj)
        {
            ImpostorLoadCompletedHandling();
            obj.Completed -= AddressableLoadCompletedImpostor;
        }
#endif
        private void SceneLoadCompletedRegular(AsyncOperation obj)
        {
            SceneLoadCompletedHandling();
            obj.completed -= SceneLoadCompletedRegular;
        }

        private void SceneLoadCompletedImpostor(AsyncOperation obj)
        {

            obj.completed -= SceneLoadCompletedImpostor;
        }

        private void UnloadCompleted(AsyncOperation obj)
        {
            Resources.UnloadUnusedAssets();
            obj.completed -= UnloadCompleted;
        }

        private void SceneLoadCompletedHandling()
        {
#if GAIA_PRO_PRESENT
            string scenePath = m_scenePath;

            Terrain terrain = null;

            if (TerrainLoaderManager.ColliderOnlyLoadingActive)
            {
                scenePath = m_colliderScenePath;
            }
            Scene scene = SceneManager.GetSceneByPath(scenePath);

            if (!scene.isLoaded)
            {
                return;
            }

            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (m_useFloatingPointFix)
                {
                    go.transform.position += FloatingPointFix.Instance.totalOffset;
                    if (go.transform.GetComponent<FloatingPointFixMember>() == null)
                    {
                        go.AddComponent<FloatingPointFixMember>();
                    }
                }
                terrain = go.GetComponent<Terrain>();
                if (terrain != null)
                {
                    m_terrainObj = go;
                }
                go.SetActive(true);
            }
            m_regularLoadState = LoadState.Loaded;
            ReplaceImpostor();
            if (terrain != null)
            {
                CheckForStitching(terrain);
            }
#endif
        }

        private void ImpostorLoadCompletedHandling()
        {
#if GAIA_PRO_PRESENT
            Scene scene = SceneManager.GetSceneByPath(m_impostorScenePath);
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (m_useFloatingPointFix)
                {
                    go.transform.position += FloatingPointFix.Instance.totalOffset;
                    if (go.transform.GetComponent<FloatingPointFixMember>() == null)
                    {
                        go.AddComponent<FloatingPointFixMember>();
                    }
                }
                go.SetActive(true);
            }
            m_impostorLoadState = LoadState.Loaded;
            //Make sure the regular scene has not been loaded in the meantime due to a race condition. If it has, remove the impostor right away.
            if (m_regularReferences.Count >= 1 && m_regularLoadState == LoadState.Loaded)
            {
                ReplaceImpostor();
            }
#endif
        }

        private void CheckForStitching(Terrain terrain)
        {
            if (!m_stitchedEastBorder || !m_stitchedWestBorder || !m_stitchedNorthBorder || !m_stitchedSouthBorder)
            {
                ShiftLoadedTerrain();
            }

            if (m_stitchedEastBorder == false)
            {
                TerrainHelper.TryStitch(terrain, StitchDirection.East);
            }
            if (m_stitchedWestBorder == false)
            {
                TerrainHelper.TryStitch(terrain, StitchDirection.West);
            }
            if (m_stitchedNorthBorder == false)
            {
                TerrainHelper.TryStitch(terrain, StitchDirection.North);
            }
            if (m_stitchedSouthBorder == false)
            {
                TerrainHelper.TryStitch(terrain, StitchDirection.South);
            }
        }

        private void ReplaceImpostor()
        {
#if GAIA_PRO_PRESENT
            if (!String.IsNullOrEmpty(m_impostorScenePath) && m_impostorLoadState == LoadState.Loaded)
            {
#if UNITY_EDITOR
                Scene impostorScene = EditorSceneManager.GetSceneByPath(m_impostorScenePath);
#else
                Scene impostorScene = SceneManager.GetSceneByPath(m_impostorScenePath);
#endif
                UnloadScene(impostorScene, ref m_impostorLoadRequested, ref m_impostorUnloadRequested, ref m_impostorLoadState, true, true);
            }
#endif
        }



        public void ShiftLoadedTerrain()
        {
            //Perform the shift for both loaded and cached terrains
            if (m_regularLoadState == LoadState.Loaded || m_regularLoadState == LoadState.Cached)
            {
                string scenePath = m_scenePath;
                if (TerrainLoaderManager.ColliderOnlyLoadingActive)
                {
                    scenePath = m_colliderScenePath;
                }

#if UNITY_EDITOR
                Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
#else
                Scene scene = SceneManager.GetSceneByPath(m_scenePath);
#endif
                if (scene.isLoaded)
                {
                    scene.GetRootGameObjects(m_rootGameObjects);
                    foreach (GameObject go in m_rootGameObjects)
                    {
                        go.transform.position = m_bounds.center - m_currentOriginOffset - new Vector3Double(m_bounds.size.x / 2f, 0f, m_bounds.size.z / 2f);
                    }
                }
                else
                {
                    m_regularLoadState = LoadState.Unloaded;
                }
            }

            if (m_impostorLoadState == LoadState.Loaded || m_impostorLoadState == LoadState.Cached)
            {
#if UNITY_EDITOR
                Scene scene = EditorSceneManager.GetSceneByPath(m_impostorScenePath);
#else
                Scene scene = SceneManager.GetSceneByPath(m_impostorScenePath);
#endif
                if (scene.isLoaded)
                {
                    scene.GetRootGameObjects(m_rootGameObjects);
                    foreach (GameObject go in m_rootGameObjects)
                    {
                        go.transform.position = m_bounds.center - m_currentOriginOffset - new Vector3Double(m_bounds.size.x / 2f, 0f, m_bounds.size.z / 2f);
                    }
                }
                else
                {
                    m_impostorLoadState = LoadState.Unloaded;
                }
            }
        }
    }
}
