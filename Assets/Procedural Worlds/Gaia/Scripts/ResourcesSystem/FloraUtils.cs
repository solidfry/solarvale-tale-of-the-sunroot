#if FLORA_PRESENT
using ProceduralWorlds.Flora;
using static ProceduralWorlds.Flora.CoreCommonFloraData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PWCommon5;


namespace Gaia
{
    public class FloraUtils
    {
        public static void SaveSettingsFile(DetailScriptableObject detailScriptableObject, ref string assetGUID, ref int instanceID, bool allowOverWrite = false, string name = "", string path = "")
        {
#if UNITY_EDITOR
            //string path = GaiaDirectories.GetTerrainDetailsPath();
            if (!allowOverWrite)
            {
                //We create a new settings asset for this resorurce for the first time.
                //Make sure the name is unique, do not want to overwrite another settings file
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                //strip off the extension, we need an unique name BEFORE the extension
                string[] existingFileNames = directoryInfo.GetFiles().Select(x => x.Name.Replace(".asset", "")).ToArray();
                detailScriptableObject.Name = ObjectNames.GetUniqueName(existingFileNames, name);
            }
            else
            {
                detailScriptableObject.Name = name;
            }
            AssetDatabase.CreateAsset(detailScriptableObject, path + "/" + detailScriptableObject.Name + ".asset");
            assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(detailScriptableObject));
            instanceID = detailScriptableObject.GetInstanceID();
#endif
        }

        public static void UpdateAllTerrainsWithNewSettingsSO(DetailScriptableObject oldObject, DetailScriptableObject newObject)
        {
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                Action<Terrain> act = (terrain) => UpdateSingleTerrainWithNewSettingsSO(terrain, oldObject, newObject);
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true, null, "Updating Detail Settings");
            }
            else
            {
                foreach (Terrain t in Terrain.activeTerrains)
                {
                    UpdateSingleTerrainWithNewSettingsSO(t, oldObject, newObject);
                }
            }
        }

        public static void SaveFloraLODs(List<FloraLOD> floraLODs, string name, string saveFilePath)
        {
#if UNITY_EDITOR
            if (floraLODs != null)
            {
                Dictionary<Material, int> createdMaterialInstanceIDs = new Dictionary<Material, int>();

                foreach (FloraLOD floraLOD in floraLODs)
                {
                    //Save the materials first
                    for (int i = 0; i < floraLOD.DetailerSettingsObject.Mat.Length; i++)
                    {
                        Material mat = floraLOD.DetailerSettingsObject.Mat[i];
                        if (createdMaterialInstanceIDs.ContainsKey(mat))
                        {
                            //this material has already been added - load it again and set it as the current material in use
                            foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(saveFilePath))
                            {
                                if (obj.GetType() == typeof(Material))
                                {
                                    int targetInstanceID = 0;
                                    createdMaterialInstanceIDs.TryGetValue(mat, out targetInstanceID);
                                    if (((Material)obj).GetInstanceID() == targetInstanceID)
                                    {
                                        floraLOD.DetailerSettingsObject.Mat[i] = (Material)obj;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Material newMat = Material.Instantiate(mat);
                            newMat.name = mat.name;
                            AssetDatabase.AddObjectToAsset(newMat, saveFilePath);
                            //override the material in the detailer settings with the new material - the following reload of the settings will 
                            //turn this back into the association with the files in the Gaia user directory
                            floraLOD.DetailerSettingsObject.Mat[i] = newMat;
                            createdMaterialInstanceIDs.Add(mat, newMat.GetInstanceID());
                        }
                    }

                    floraLOD.DetailerSettingsObject.Name = name + "_LOD" + floraLOD.m_index;
                    DetailScriptableObject newDSO = ScriptableObject.Instantiate(floraLOD.DetailerSettingsObject);
                    //Sanitize the name from any numbering and "(Clone)", just want a clean name of the original resource for storing it
                    newDSO.name = floraLOD.DetailerSettingsObject.Name;
                    AssetDatabase.AddObjectToAsset(newDSO, saveFilePath);
                    //For the saved spawner settings file, we need to assign the new instance that we just saved.
                    //The following reload will turn this back into the association with the files in the Gaia user directory
                    floraLOD.DetailerSettingsObject = newDSO;

                }
            }
#endif
        }


        public static void UpdateSingleTerrainWithNewSettingsSO(Terrain t, DetailScriptableObject oldObject, DetailScriptableObject newObject)
        {
            if (oldObject == null)
            {
                return;
            }

            if (newObject == null)
            {
                return;
            }

            FloraTerrainTile dtt = t.GetComponent<FloraTerrainTile>();
            if (dtt == null)
            {
                return;
            }
            if (dtt.m_detailObjectList == null)
            {
                return;
            }

            //We match by the detail settings object, this should be precise enough to get the correct entry in the list
            int listIndex = dtt.m_detailObjectList.FindIndex(x => x.DetailScriptableObject == oldObject);

            if (listIndex == -1)
            {
                //not found, exit
                return;
            }

            //DetailOverrideData is a struct, need to overwrite it in the list if it already exists
            DetailOverrideData detailOverrideData = new DetailOverrideData();
            detailOverrideData.DetailScriptableObject = newObject;
            detailOverrideData.SourceDataType = SourceDataType.Detail;
            detailOverrideData.SourceDataIndex = dtt.m_detailObjectList[listIndex].SourceDataIndex;
            detailOverrideData.DebugColor = dtt.m_detailObjectList[listIndex].DebugColor;
            dtt.m_detailObjectList[listIndex] = detailOverrideData;
        }

        public static void RemoveSettingsSOFromAllTerrains(List<FloraLOD> floraLODs)
        {
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                Action<Terrain> act = (terrain) => RemoveSettingsSOFromSingleTerrain(floraLODs, terrain);
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true, null, "Removing Detail Setting");
            }
            else
            {
                foreach (Terrain t in Terrain.activeTerrains)
                {
                    RemoveSettingsSOFromSingleTerrain(floraLODs, t);
                }
            }
        }

        public static void RemoveSettingsSOFromSingleTerrain(List<FloraLOD> floraLODs, Terrain t)
        {
#if UNITY_EDITOR
            if (floraLODs == null || floraLODs.Count <= 0)
            {
                return;
            }

            FloraTerrainTile dtt = t.GetComponent<FloraTerrainTile>();
            if (dtt == null)
            {
                return;
            }
            if (dtt.m_detailObjectList == null)
            {
                return;
            }

            foreach (FloraLOD floraLOD in floraLODs)
            {

                //We match by the detail settings object, this should be precise enough to get the correct entry in the list
                int listIndex = dtt.m_detailObjectList.FindIndex(x => x.DetailScriptableObject == floraLOD.DetailerSettingsObject);

                if (listIndex == -1)
                {
                    //not found, exit
                    return;
                }
                else
                {
                    dtt.m_detailObjectList.RemoveAt(listIndex);
                }

            }
            //if no detail configurations remaining, we can as well strip off the component
            if (dtt.m_detailObjectList.Count == 0)
            {
                Component.DestroyImmediate(dtt);
            }
#endif

        }

        public static void AddNewDetailerSettingsObject(List<FloraLOD> floraLODs, string name, GaiaConstants.SpawnerResourceType resourceType, int index = -99)
        {
            if (floraLODs == null)
            {
                floraLODs = new List<FloraLOD>();
            }

            FloraLOD newLOD = new FloraLOD()
            {
                DetailerSettingsObject = ScriptableObject.CreateInstance<DetailScriptableObject>(),
                m_index = floraLODs.Count,
                m_name = name,
                m_spawnerResourceType = resourceType,
            };

            FloraLOD addedLOD = null;

            if (index == -99)
            {
                floraLODs.Add(newLOD);
                addedLOD = floraLODs.Last();
            }
            else
            {
                floraLODs.Insert(index, newLOD);
                addedLOD = floraLODs[index];
            }
            
            FloraUtils.SaveSettingsFile(addedLOD.DetailerSettingsObject, ref addedLOD.m_detailerSettingsObjectAssetGUID, ref addedLOD.m_detailerSettingsObjectInstanceID, false, addedLOD.m_name, GaiaDirectories.GetFloraDataPath());
        }

        public static void AddFloraToTerrain(Terrain terrain, List<FloraLOD> floraLODs, int terrainSourceDataIndex, SourceDataType sourceDataType)
        {
#if UNITY_EDITOR
            //Set up the global manager for the detail system (if not present already)
            GaiaUtils.GetOrCreateDetailGlobalManager();
            FloraTerrainTile dtt = terrain.GetComponent<FloraTerrainTile>();
            if (dtt == null)
            {
                Camera mainCamera = FloraAutomationAPI.GetCamera();
                dtt = terrain.gameObject.AddComponent<FloraTerrainTile>();
                if (mainCamera != null)
                {
                    dtt.MaximumDrawDistance = mainCamera.farClipPlane;
                }
            }
            if (dtt.m_detailObjectList == null)
            {
                dtt.m_detailObjectList = new List<DetailOverrideData>();
            }
            dtt.TerrainType = FloraTerrainType.UnityTerrain;
            dtt.UnityTerrain = terrain;
            //Remove all configs for the source data index first, to set up the current LOD setting anew.
            dtt.m_detailObjectList.RemoveAll(x => x.SourceDataIndex == terrainSourceDataIndex && x.SourceDataType == sourceDataType);
            //set the correct index
            foreach (FloraLOD floraLOD in floraLODs.Where(x=>x.DetailerSettingsObject!=null))
            {
                floraLOD.DetailerSettingsObject.SourceDataIndex = terrainSourceDataIndex;
            }

            FloraAutomationAPI.AddToTerrain(dtt, floraLODs.Select(x => x.DetailerSettingsObject).ToList(), true);
            FloraDefaults floraDefaults = FloraAutomationAPI.GetFloraDefaults();
            FloraAutomationAPI.SetupTerrainCellCount(floraDefaults, dtt, terrain);
            //Mark the scene as dirty - might not be saved otherwise!
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);
#endif
        }

        /// <summary>
        /// Resets the detailer settings objects in a LOD list to force a reload by asset guid / instance ID
        /// </summary>
        /// <param name="floraLODs"></param>
        public static void ResetSettingsObjects(List<FloraLOD> floraLODs, List<FloraLODIdOverrides> floraLODIdOverrides)
        {
            for (int i = 0; i < floraLODs.Count; i++)
            {
                FloraLOD floraLOD = floraLODs[i];
                floraLOD.DetailerSettingsObject = null;

                if (floraLODIdOverrides != null && i < floraLODIdOverrides.Count)
                {
                    floraLOD.m_detailerSettingsObjectAssetGUID = floraLODIdOverrides[i].m_assetGUIDOverride;
                    floraLOD.m_detailerSettingsObjectInstanceID = floraLODIdOverrides[i].m_instanceIDOverride;
                }
            }
        }

        /// <summary>
        /// Populates the list of flora LODs with the settings made by the flora automation API according to the prefab that is passed in.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="floraLODs"></param>
        /// <param name="name"></param>
        /// <param name="resourceType"></param>
        public static void CreateConfigFromPrefab(GameObject prefab, List<FloraLOD> floraLODs, string name, GaiaConstants.SpawnerResourceType resourceType)
        {
#if UNITY_EDITOR
            if (prefab == null || floraLODs == null)
            {
                return;
            }

            SourceDataType sourceDataType = resourceType == GaiaConstants.SpawnerResourceType.TerrainTree ? SourceDataType.Tree : SourceDataType.Detail;

            if (sourceDataType == SourceDataType.Tree && !FloraGlobalData.TreesEnabled)
            {
                return;
            }

            List<DetailScriptableObject> detailScriptableObjects = FloraAutomationAPI.CreateFloraRenderFromPrefab(Terrain.activeTerrain, prefab, true, sourceDataType, GaiaDirectories.GetFloraDataPath() + "/");
            floraLODs.Clear();

            for (int i = 0; i < detailScriptableObjects.Count; i++)
            {
                floraLODs.Add(new FloraLOD()
                {
                    m_index = i,
                    m_name = name,
                    m_spawnerResourceType = resourceType,
                    DetailerSettingsObject = detailScriptableObjects[i],
                });

            }
#endif
        }

        /// <summary>
        /// Populates the list of flora LODs with the settings made by the flora automation API according to the texture on the detail prototype that is passed in.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="floraLODs"></param>
        /// <param name="name"></param>
        /// <param name="resourceType"></param>
        public static void CreateConfigFromTexture(DetailPrototype detailProto, List<FloraLOD> floraLODs, string name, GaiaConstants.SpawnerResourceType resourceType)
        {
#if UNITY_EDITOR
            if (detailProto == null || detailProto.prototypeTexture == null || floraLODs == null)
            {
                return;
            }

            SourceDataType sourceDataType = resourceType == GaiaConstants.SpawnerResourceType.TerrainTree ? SourceDataType.Tree : SourceDataType.Detail;
            List<DetailScriptableObject> detailScriptableObjects = FloraAutomationAPI.CreateFloraRenderFromTexture(detailProto, sourceDataType, GaiaDirectories.GetFloraDataPath() + "/");
            floraLODs.Clear();

            for (int i = 0; i < detailScriptableObjects.Count; i++)
            {
                floraLODs.Add(new FloraLOD()
                {
                    m_index = i,
                    m_name = name,
                    m_spawnerResourceType = resourceType,
                    DetailerSettingsObject = detailScriptableObjects[i],
                });

            }
#endif
        }
    }

}
#endif