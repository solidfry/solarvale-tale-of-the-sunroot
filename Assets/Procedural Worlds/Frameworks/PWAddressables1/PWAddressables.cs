using Gaia;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
#if UNITY_EDITOR
using UnityEditor;
#if PW_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace ProceduralWorlds.Addressables1
{
    /// <summary>
    /// Contains static functions for handling Unity Addressables
    /// </summary>
    public class PWAddressables
    {
        public static readonly string m_PWGroupNamePrefix = "PW_";
        public static readonly string m_PWDefaultGroupName= "Default";
        public static readonly string m_sharedAssetGroupPrefix = "TerrainShared-";
        public static bool m_isUpdate = false;
        private static List<TerrainAssetGroupAssociation> m_terrainAssetGroupAssociations;
        public static List<TerrainAssetGroupAssociation> TerrainAssetGroupAssociations
        {
            get {
#if UNITY_EDITOR && PW_ADDRESSABLES
                if (m_terrainAssetGroupAssociations == null)
                {
                    string[] allConfigGUIDs = AssetDatabase.FindAssets("t:PWAddressablesConfig");
                    if (allConfigGUIDs.Length > 0)
                    {
                        PWAddressablesConfig config = (PWAddressablesConfig)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allConfigGUIDs[0]), typeof(PWAddressablesConfig));
                        if (config != null)
                        {
                            m_terrainAssetGroupAssociations = config.m_terrainAssetGroupAssociations;
                        }
                    }
                }
                return m_terrainAssetGroupAssociations;
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// Returns true if the initial addressable settings have been created in this project.
        /// </summary>
        /// <returns></returns>
        public static bool DoSettingsExist()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            return AddressableAssetSettingsDefaultObject.SettingsExists;
#else
            return false;
#endif
        }

        /// <summary>
        /// Name of the PW Profile that we create our addressable config under
        /// </summary>
        public static readonly string m_PWProfileName = "PW Profile";

        /// <summary>
        /// Hardcoded list of asset paths that should never be part of the addressable config - sometimes the asset parsing can pull in undesireable assets or folders which can be blocked with this list
        /// </summary>
        public static readonly List<string> m_blockedAssets = new List<string>()
        { "Resources/unity_builtin_extra"};


        public static bool BuildRemoteCatalog
        {
            get
            {
#if PW_ADDRESSABLES && UNITY_EDITOR
                if (DoSettingsExist())
                {
                    return AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog;
                }
                else
                {
                    return false;
                }
#else
                return false;
#endif
            }
            set
            {
#if PW_ADDRESSABLES && UNITY_EDITOR
                if (DoSettingsExist())
                {
                    AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog = value;
                }
#endif
            }
        }

        //        public static void AddPathAndSubdirectoriesToGroup(string groupName, string rootPath)
        //        {
        //#if PW_ADDRESSABLES && UNITY_EDITOR
        //            AddressableAssetGroup group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
        //            if(group==null)
        //            {
        //                group = CreateGroup(groupName);
        //            }
        //            AddPathAndSubdirectories(group, rootPath);
        //#endif
        //        }

        //        /// <summary>
        //        /// Adds the asset files of a single directory and its subdirectory to the target group configuration for this content pack.
        //        /// Uses recursion to get the subdirs.
        //        /// </summary>
        //        /// <param name="group">The group to add the assets to</param>
        //        /// <param name="path">The path to recursively traverse to search for assets</param>
        //#if PW_ADDRESSABLES && UNITY_EDITOR
        //        private static void AddPathAndSubdirectories(AddressableAssetGroup group, string path)
        //        {
        //            DirectoryInfo dirInfo = new DirectoryInfo(path);
        //            DirectoryInfo[] allSubDirectories = dirInfo.GetDirectories();
        //            FileInfo[] allFiles = dirInfo.GetFiles();
        //            foreach (FileInfo fileInfo in allFiles)
        //            {
        //                if (fileInfo.Extension != ".meta" && fileInfo.Extension != ".cs")
        //                {
        //                    AddGroupEntryByPath(group, fileInfo.FullName, true, true);
        //                }
        //            }

        //            foreach (DirectoryInfo subDir in allSubDirectories)
        //            {
        //                AddPathAndSubdirectories(group, subDir.FullName);
        //            }

        //        }
        //#endif

        /// <summary>
        /// Creates the initial addressable settings / database in this project.
        /// </summary>
        public static void CreateSettings()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            }
#endif
        }

#if UNITY_EDITOR && PW_ADDRESSABLES
        /// <summary>
        /// Creates a new addressable asset group with the given name and default schemas
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <returns></returns>
        public static AddressableAssetGroup CreateGroup(string name)
        {

            CreateSettings();

            AddressableAssetGroup addressableAssetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(name);

            if (addressableAssetGroup == null)
            {
                addressableAssetGroup = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(name, false, false, false, new List<AddressableAssetGroupSchema>(), new Type[0]);
            }

            //Make sure we are using the default schemas with the default settings
            if (addressableAssetGroup != null)
            {
                addressableAssetGroup.RemoveSchema(typeof(BundledAssetGroupSchema));
                addressableAssetGroup.RemoveSchema(typeof(ContentUpdateGroupSchema));
                addressableAssetGroup.AddSchema(typeof(BundledAssetGroupSchema));
                addressableAssetGroup.AddSchema(typeof(ContentUpdateGroupSchema));
            }
            return addressableAssetGroup;
        }

#endif

        /// <summary>
        /// Adds a scene as a group in the addressable settings. This allows deploying updates for this scene without affecting other scenes.
        /// </summary>
        /// <param name="name">The name of the scene</param>
        /// <param name="name">The path of the scene</param>
        public static void AddSceneAsGroup(string name, string path)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            AddressableAssetGroup addressableAssetGroup = CreateGroup(m_PWGroupNamePrefix + name);
            //Add the scene as first entry of the group
            AddGroupEntryByPath(addressableAssetGroup, path, true, false);
#endif
        }

        /// <summary>
        /// Adds the "official" PW default group and sets it as default group for the addressable configuration
        /// </summary>
        public static void AddPWDefaultGroupIfNotExists()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            CreateSettings();
            string defaultGroupName = m_PWGroupNamePrefix + m_PWDefaultGroupName;
            AddressableAssetGroup defaultGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(defaultGroupName);
            if (defaultGroup == null)
            {
                defaultGroup = CreateGroup(m_PWGroupNamePrefix + m_PWDefaultGroupName);
                AddressableAssetSettingsDefaultObject.Settings.DefaultGroup = defaultGroup;
            }
#endif
        }

#if PW_ADDRESSABLES && UNITY_EDITOR
        /// <summary>
        /// Adds an entry for an addressable path in the given group
        /// </summary>
        /// <param name="addressableAssetGroup">The group that we want to add the entry to</param>
        /// <param name="path">The path to the addressable asset (Starting at the "Assets" folder)</param>
        /// <param name="allowGroupChange">Whether the addressable asset is allowed to be put into another group if it already exists</param>
        /// <param name="addExplicitDependencies">Whether the dependencies of the asset should be added as their own explicit reference as well. If false, dependent assets may still be pulled as implicit reference since this would be standard behavior when adding assets as addressables.</param>
        private static void AddGroupEntryByPath(AddressableAssetGroup addressableAssetGroup, string path, bool allowGroupChange = false, bool addExplicitDependencies = false)
        {

            if (String.IsNullOrEmpty(path) || addressableAssetGroup == null)
            {
                return;
            }

            if (m_blockedAssets.Contains(path))
            {
                return;
            }

            if (path.EndsWith(".cs"))
            {
                return;
            }

            AddressableAssetEntry entry = addressableAssetGroup.Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));

            //only really add the entry if it does not exist yet, or if it is in the wrong group
            if (entry == null || (entry.parentGroup != addressableAssetGroup && allowGroupChange))
            {
                entry = addressableAssetGroup.Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), addressableAssetGroup);
                if (entry != null)
                {
                    entry.address = path;
                    if (addExplicitDependencies)
                    {
                        //Get all dependencies and add them as well - otherwise those assets will potentially be added as duplicates by implicit references in other bundles
                        string[] dependencyPaths = AssetDatabase.GetDependencies(path);
                        foreach (string dependencyPath in dependencyPaths)
                        {
                            if (addressableAssetGroup.GetAssetEntry(AssetDatabase.AssetPathToGUID(dependencyPath)) == null)
                            {
                                AddGroupEntryByPath(addressableAssetGroup, dependencyPath, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds an entry for an addressable object in the given group
        /// </summary>
        /// <param name="addressableAssetGroup">The group that we want to add the entry to</param>
        /// <param name="addressableObject">The object that we want to add as an addressable</param>
        private static void AddGroupEntryByObject(AddressableAssetGroup addressableAssetGroup, UnityEngine.Object addressableObject)
        {
            if (addressableObject == null || addressableAssetGroup == null)
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(addressableObject);
            if (path == "" && addressableObject.GetType() == typeof(GameObject))
            {
                //if this is a game object, it might be a prefab instance, let's try to get the original prefab instead

                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource((GameObject)addressableObject);
                if (prefab != null)
                {
                    path = AssetDatabase.GetAssetPath(prefab);
                }
            }
            AddGroupEntryByPath(addressableAssetGroup, path, true, true);
        }
#endif

#if PW_ADDRESSABLES && GAIA_2_PRESENT
        /// <summary>
        /// Adds the reosurces of a list of Gaia spawners as addressables, with one addressable group per spawner being created each.
        /// </summary>
        /// <param name="spawners">The list of spawners to create addressable groups for.</param>
        public static void AddSpawnersAsGroups(List<Gaia.Spawner> spawners)
        {
            foreach (Gaia.Spawner spawner in spawners)
            {
                AddGaiaSpawnerAsGroup(spawner);
            }
        }
#endif

#if GAIA_2_PRESENT
        /// <summary>
        /// Adds all resources that are found in a Gaia spawner to an individual addressable group. The group is recreated from scratch each time to make sure it only contains the latest content.
        /// </summary>
        /// <param name="spawner">The Gaia spawner we want to add</param>
        public static void AddGaiaSpawnerAsGroup(Gaia.Spawner spawner)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            string groupName = "PW Spawner " + spawner.name;
            CreateSettings();
            AddressableAssetGroup spawnerGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            //Remove Group to create it from scratch
            //This ensures all the current assets in spawner are up to date in the group, and unused assets are being removed
            if (spawnerGroup != null)
            {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(spawnerGroup);
            }

            spawnerGroup = CreateGroup(groupName);
            foreach (Gaia.SpawnRule sr in spawner.m_settings.m_spawnerRules)
            {
                switch (sr.m_resourceType)
                {
                    case Gaia.GaiaConstants.SpawnerResourceType.TerrainTexture:
                        Gaia.ResourceProtoTexture resourceProtoTexture = spawner.m_settings.m_resources.m_texturePrototypes[sr.m_resourceIdx];
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTexture.m_texture);
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTexture.m_normal);
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTexture.m_maskmap);
                        //Try to add the layer as well if it exists
                        if (!String.IsNullOrEmpty(resourceProtoTexture.m_LayerGUID))
                        {
                            AddGroupEntryByPath(spawnerGroup, AssetDatabase.GUIDToAssetPath(resourceProtoTexture.m_LayerGUID));
                        }
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.TerrainDetail:
                        Gaia.ResourceProtoDetail resourceProtoTerrainDetail = spawner.m_settings.m_resources.m_detailPrototypes[sr.m_resourceIdx];
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTerrainDetail.m_detailTexture);
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTerrainDetail.m_detailProtoype);
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.TerrainModifierStamp:
                        //nothing to do here, there isn't a real resource that needs to be in the bundle as an addressable
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.TerrainTree:
                        Gaia.ResourceProtoTree resourceProtoTree = spawner.m_settings.m_resources.m_treePrototypes[sr.m_resourceIdx];
                        AddGroupEntryByObject(spawnerGroup, resourceProtoTree.m_desktopPrefab);
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.GameObject:
                        Gaia.ResourceProtoGameObject resourceProtoGameObject = spawner.m_settings.m_resources.m_gameObjectPrototypes[sr.m_resourceIdx];
                        foreach (ResourceProtoGameObjectInstance instance in resourceProtoGameObject.m_instances)
                        {
                            AddGroupEntryByObject(spawnerGroup, instance.m_desktopPrefab);
                        }
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.SpawnExtension:
                        //nothing to do here, if the spawn extension creates addressable content in any way that would need to be handled by the spawn extension
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.Probe:
                        //nothing to do here, there isn't a real resource that needs to be in the bundle as an addressable
                        //The data of the spawned probes should be part of the per-terrain bundles
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.StampDistribution:
                        //nothing to do here, there isn't a real resource that needs to be in the bundle as an addressable
                        break;
                    case Gaia.GaiaConstants.SpawnerResourceType.WorldBiomeMask:
                        //nothing to do here, there isn't a real resource that needs to be in the bundle as an addressable
                        break;
                }
            }
#endif
        }
#endif

        /// <summary>
        /// Gets or creates the main assets group 
        /// </summary>
        /// <returns></returns>
#if PW_ADDRESSABLES && UNITY_EDITOR
        public static AddressableAssetGroup GetOrCreateSharedGroup(string sharedGroupName)
        {
            return CreateGroup(sharedGroupName);
        }
#endif


        /// <summary>
        /// Collect all assets on this terrain (textures, trees, terrain details) and all game objects in the children for the addressable configuration
        /// </summary>
        /// <param name="t"></param>
        public static void AddTerrainAssets(Terrain t, string sharedGroupName)
        {
            if (t == null || t.terrainData == null)
            {
                return;
            }

#if UNITY_EDITOR && PW_ADDRESSABLES


            AddressableAssetGroup sharedGroup = GetOrCreateSharedGroup(sharedGroupName);

            AddressableAssetGroup sceneGroup = null;
            string guid = AssetDatabase.AssetPathToGUID(t.gameObject.scene.path);
            if (guid != null)
            {
                AddressableAssetEntry sceneEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);
                if (sceneEntry != null)
                {
                    sceneGroup = sceneEntry.parentGroup;
                }
            }

            if (sceneGroup != null)
            {
                //add the terrain data object itself to the scene group
                AddGroupEntryByPath(sceneGroup, AssetDatabase.GetAssetPath(t.terrainData), true, false);
                //add all dependencies as explicit entries to the main asset groups (terrain shaders, etc.)
                AddGroupEntryByPath(sharedGroup, AssetDatabase.GetAssetPath(t.terrainData), false, true);
                AddGroupEntryByPath(sharedGroup, AssetDatabase.GetAssetPath(t.materialTemplate), false, true);

#if UPPipeline
                //URP Terrain detail shaders
                AddGroupEntryByPath(sharedGroup, "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainDetailLit.shader", false, true);
                AddGroupEntryByPath(sharedGroup, "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/WavingGrass.shader", false, true);
                AddGroupEntryByPath(sharedGroup, "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/WavingGrassBillboard.shader", false, true);

                //URP Mesh Terrain / Impostor shaders
                AddGroupEntryByPath(sharedGroup, "Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader", false, true);
                AddGroupEntryByPath(sharedGroup, "Packages/com.unity.render-pipelines.universal/Shaders/Utils/FallbackError.shader", false, true);
#endif

#if FLORA_PRESENT
                //add the flora scriptable objects & dependencies
                FloraTerrainTile ftt = t.GetComponent<FloraTerrainTile>();
                if (ftt != null)
                {
                    foreach (DetailOverrideData detailOverrideData in ftt.m_detailObjectList)
                    {
                        AddGroupEntryByPath(sharedGroup, AssetDatabase.GetAssetPath(detailOverrideData.DetailScriptableObject), false, true);
                    }
                }
#endif

            }

            //Adding all terrain assets. Note that dependencies to those objects will automatically be added as well deeper in the "AddGroupEntryByObject" function.
            //Terrain Layers
            for (int i = 0; i < t.terrainData.terrainLayers.Length; i++)
            {
                TerrainLayer layer = t.terrainData.terrainLayers[i];
                if (layer != null)
                {
                    AddTerrainObjectToSharedGroups(sharedGroup, sceneGroup, t, layer);
                }
                //AddGroupEntryByObject(mainAssetGroup, layer);
            }

            //Trees
            for (int i = 0; i < t.terrainData.treePrototypes.Length; i++)
            {
                TreePrototype treeProto = t.terrainData.treePrototypes[i];
                AddTerrainObjectToSharedGroups(sharedGroup, sceneGroup, t, treeProto.prefab);
                //AddGroupEntryByObject(mainAssetGroup, treeProto.prefab);
            }

            //Terrain Details
            for (int i = 0; i < t.terrainData.detailPrototypes.Length; i++)
            {
                DetailPrototype detailPrototype = t.terrainData.detailPrototypes[i];
                if (detailPrototype.usePrototypeMesh)
                {
                    AddTerrainObjectToSharedGroups(sharedGroup, sceneGroup, t, detailPrototype.prototype);
                    //AddGroupEntryByObject(mainAssetGroup, detailPrototype.prototype);
                }
                else
                {
                    AddTerrainObjectToSharedGroups(sharedGroup, sceneGroup, t, detailPrototype.prototypeTexture);
                    //AddGroupEntryByObject(mainAssetGroup, detailPrototype.prototypeTexture);
                }
            }

            //Game Objects
            AddAllGameObjectChildsToAssetGroup(sharedGroup, sceneGroup, t, t.gameObject);
#endif
        }

#if UNITY_EDITOR && PW_ADDRESSABLES
        private static void AddTerrainObjectToSharedGroups(AddressableAssetGroup mainSharedGroup, AddressableAssetGroup sceneGroup, Terrain terrain, UnityEngine.Object objectToAdd)
        {
            string path = AssetDatabase.GetAssetPath(objectToAdd);

            if (string.IsNullOrEmpty(path) && objectToAdd.GetType() == typeof(GameObject))
            {
                //if this is a game object, it might be a prefab instance, let's try to get the original prefab instead

                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource((GameObject)objectToAdd);
                if (prefab != null)
                {
                    path = AssetDatabase.GetAssetPath(prefab);
                }
            }

            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                {
                    //Look up the object first, is it part of any group already?
                    AddressableAssetEntry existingEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);

                    if (m_isUpdate)
                    {
                        //Update case is relatively simple, if there is no entry yet, the asset goes in the main asset group. If an entry already exists
                        //we leave the asset in peace, for the update scenario it is more desireable to not disturb the already existing config over changing all the bundles
                        //which would create unneccessary downloads for the user in the end.
                        if (existingEntry == null)
                        {
                            AddGroupEntryByObject(mainSharedGroup, objectToAdd);
                        }
                    }
                    else
                    {


                        if (existingEntry == null)
                        {
                            //no entry yet? This needs to go in the terrain / scene specific group
                            AddGroupEntryByObject(sceneGroup, objectToAdd);
                            //if it does not exist already, create an entry for this terrain <-> group association
                            if (TerrainAssetGroupAssociations.Find(x => x.m_assetGroup == sceneGroup.name) == null)
                            {
                                TerrainAssetGroupAssociations.Add(new TerrainAssetGroupAssociation() { m_assetGroup = sceneGroup.name, m_terrainNames = new List<string>() { terrain.name } });
                            }

                        }
                        else
                        {
                            //we do have an entry, what group is it in?
                            if (existingEntry.parentGroup == sceneGroup || existingEntry.parentGroup == mainSharedGroup)
                            {
                                //Already in the terrain specific or main group, nothing to do anymore.
                                return;
                            }
                            else
                            {
                                TerrainAssetGroupAssociation assoc = TerrainAssetGroupAssociations.Find(x => x.m_assetGroup == existingEntry.parentGroup.name);
                                if (assoc != null)
                                {
                                    if (assoc.m_terrainNames.Contains(terrain.name))
                                    {
                                        //already in a shared asset bundle with other terrains, nothing to do here
                                        return;
                                    }
                                    else
                                    {
                                        //Asset is in a bundle together with other terrains, but excluding the one we are processing at the moment. We need to put it in the bundle that is shared with those other terrains
                                        //INCLUDING the one that we are processing - if that does not exist, we need to create it.
                                        List<string> targetTerrainNames = new List<string>(assoc.m_terrainNames);
                                        targetTerrainNames.Add(terrain.name);
                                        //We do have a target group already if the number of associated terrains is equal the number of target terrains AND the number of intersecting terrains is equal as well.
                                        TerrainAssetGroupAssociation targetAssoc = TerrainAssetGroupAssociations.Find(x => x.m_terrainNames.Count == targetTerrainNames.Count && x.m_terrainNames.Intersect(targetTerrainNames).Count() == targetTerrainNames.Count());
                                        if (targetAssoc != null)
                                        {
                                            //such a group exists already, put asset in there
                                            AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(targetAssoc.m_assetGroup);
                                            if (targetGroup != null)
                                            {
                                                AddGroupEntryByObject(targetGroup, objectToAdd);
                                            }
                                        }
                                        else
                                        {
                                            //This group does not exist yet and needs to be created & tracked in the list
                                            string newGroupName = m_PWGroupNamePrefix + m_sharedAssetGroupPrefix + TerrainAssetGroupAssociations.FindAll(x => x.m_assetGroup.StartsWith(m_PWGroupNamePrefix + m_sharedAssetGroupPrefix)).Count.ToString();
                                            AddressableAssetGroup targetGroup = CreateGroup(newGroupName);
                                            if (targetGroup != null)
                                            {
                                                AddGroupEntryByObject(targetGroup, objectToAdd);
                                                TerrainAssetGroupAssociations.Add(new TerrainAssetGroupAssociation() { m_assetGroup = targetGroup.name, m_terrainNames = targetTerrainNames });
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //not in the shared terrain asset groups either? We should keep it as it is then, must be a content pack asset or user defined.
                                    return;
                                }
                            }

                        }
                    }
                }
            }


        }
#endif

        /// <summary>
        /// Adds the Game Object (if an asset) to the given addressable asset group, but will also recursively try to add all children as well.
        /// </summary>
        /// <param name="assetGroup">The Asset Group we are adding to.</param>
        /// <param name="go">The Game Object which we are adding</param>
#if UNITY_EDITOR && PW_ADDRESSABLES
        private static void AddAllGameObjectChildsToAssetGroup(AddressableAssetGroup mainAssetGroup, AddressableAssetGroup sceneGroup, Terrain t, GameObject go)
        {
            AddTerrainObjectToSharedGroups(mainAssetGroup, sceneGroup, t, go);
            //AddGroupEntryByObject(assetGroup, go);
            for (int i = 0; i < go.transform.childCount; i++)
            {
                AddAllGameObjectChildsToAssetGroup(mainAssetGroup, sceneGroup, t, go.transform.GetChild(i).gameObject);
            }
        }
#endif

        /// <summary>
        /// Starts the addressable build process
        /// </summary>
        public static void StartNewBuild()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            //Clear out target directory first, to not mix with older files from previous builds
            string path = GaiaDirectories.CreatePathIfDoesNotExist(GetRemoteBuildPath());

            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                Debug.LogError("Error while starting the addressable build: Could not find or create the path " + path);
                return;
            }
            var allFiles = di.GetFiles();
            for (int i = allFiles.Length-1; i > 0; i--)
            {
                FileUtil.DeleteFileOrDirectory(allFiles[i].FullName);
            }
            AddressableAssetSettings.BuildPlayerContent();
#endif
        }

        /// <summary>
        /// Starts the addressable build process
        /// <param name="binFile">The .bin file that contains the current content state</param>
        /// </summary>
        public static void UpdateExistingBuild(UnityEngine.Object binFile)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            if (binFile == null)
            {
                return;
            }
            string contentStatePath = AssetDatabase.GetAssetPath(binFile);
            if (string.IsNullOrEmpty(contentStatePath))
            {
                Debug.LogError($"Could not determine path for .bin file {binFile.name} when trying to build an addressable content update.");
                return;
            }
            ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, contentStatePath);
#endif
        }

        /// <summary>
        /// Get the path to the current content state file (.bin file)
        /// </summary>
        /// <returns></returns>
        public static string GetContentStateDataPath()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            return ContentUpdateScript.GetContentStateDataPath(false);
#else
            return "";
#endif
        }


        /// <summary>
        /// Sets the remote load path in the current profile
        /// </summary>
        /// <param name="m_addressableServerURL"></param>
        public static void SetServerURL(string m_addressableServerURL)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            AddressableAssetSettingsDefaultObject.Settings.profileSettings.SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, m_addressableServerURL);
#endif
        }

        /// <summary>
        /// Switches all asset groups to remote build and load path
        /// </summary>
        public static void SwitchToRemote()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            if (DoSettingsExist())
            {
                AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog = true;
                SwitchBuildAndLoadPath(AddressableAssetSettings.kRemoteBuildPath, AddressableAssetSettings.kRemoteLoadPath);
            }
#endif
        }


        /// <summary>
        /// Switches all asset groups to local build and load path
        /// </summary>
        public static void SwitchToLocal()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            if (DoSettingsExist())
            {
                AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog = false;
                SwitchBuildAndLoadPath(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalLoadPath);
            }
#endif
        }


        /// <summary>
        /// Switches the build and load path in the bundle asset schema to the given names
        /// </summary>
        /// <param name="buildPathSettingName"></param>
        /// <param name="loadPathSettingName"></param>
        private static void SwitchBuildAndLoadPath(string buildPathSettingName, string loadPathSettingName)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            //Master Settings
            AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, buildPathSettingName);
            AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogLoadPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, loadPathSettingName);

            //Individual Groups
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group.name.StartsWith(m_PWGroupNamePrefix))
                {
                    int index = group.FindSchema(typeof(BundledAssetGroupSchema));
                    if (index >= 0 && index < group.Schemas.Count)
                    {
                        BundledAssetGroupSchema schema = (BundledAssetGroupSchema)group.Schemas[index];
                        if (schema != null)
                        {
                            schema.BuildPath.SetVariableByName(group.Settings, buildPathSettingName);
                            schema.LoadPath.SetVariableByName(group.Settings, loadPathSettingName);
                        }
                    }
                }
            }
#endif
        }


        /// <summary>
        /// Gets the folder containing the built addressable bundles for the local build mode
        /// </summary>
        /// <returns>Path to the folder containing the built addressable bundles for the local build mode</returns>
        public static string GetLocalBuildPath()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            return GetPath(AddressableAssetSettings.kLocalBuildPath);
#else
            return "";
#endif
        }

        /// <summary>
        /// Gets the folder containing the built addressable bundles for the server build mode
        /// </summary>
        /// <returns>Path to the folder containing the built addressable bundles for the server build mode</returns>
        public static string GetRemoteBuildPath()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            return GetPath(AddressableAssetSettings.kRemoteBuildPath);
#else
            return "";
#endif
        }

        /// <summary>
        /// Gets one of the different Path variables from the current Addressable Profile, also evaluating dynamic variables like [BuildTarget]
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static string GetPath(string variableName)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            string pathWithVariables = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetValueByName(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, variableName);
            return AddressableAssetSettingsDefaultObject.Settings.profileSettings.EvaluateString(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, pathWithVariables);
#else
            return "";
#endif
        }

        public static void OpenAddressableSettingsWindow()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            //The editor window class for the addressable settings is a private class, so we need to open the window via calling its menu entry
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
#endif
        }



        public static void CreatePWProfile()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            CreateSettings();

            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;

            List<string> allProfileNames = profileSettings.GetAllProfileNames();
            if (!allProfileNames.Contains(m_PWProfileName))
            {
                profileSettings.AddProfile(m_PWProfileName, null);

            }
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileSettings.GetProfileId(m_PWProfileName);
#endif
        }


        /// <summary>
        /// Adds all entries in this content pack configuration file to a group with the matching name and configures it accordingly.
        /// </summary>
        /// <param name="config"></param>
        public static void AddContentPackConfig(PWAddressableContentPackConfig config)
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            //Remove the group if it exists - we need to make sure group only contains the current entries of the config file
            AddressableAssetGroup group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(config.m_contentPackName);
            if (group != null)
            {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
            }
            group = CreateGroup(config.m_contentPackName);

            //Apply the content pack configuration
            //Check first if we have an entry for the package location in the Addressable Profile
            CreatePWProfile();
            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;
            string variableName = config.m_contentPackName + "LoadPath";

            if (profileSettings.GetVariableNames().Find(x => x == variableName) == null)
            {
                profileSettings.CreateValue(variableName, config.m_URL);
            }
            else
            {
                profileSettings.SetValue(profileSettings.GetProfileId(m_PWProfileName), variableName, config.m_URL);
            }

            int index = group.FindSchema(typeof(BundledAssetGroupSchema));
            if (index >= 0 && index < group.Schemas.Count)
            {
                BundledAssetGroupSchema schema = (BundledAssetGroupSchema)group.Schemas[index];
                if (schema != null)
                {
                    schema.BuildPath.SetVariableByName(group.Settings, AddressableAssetSettings.kRemoteBuildPath);
                    //schema.LoadPath.SetVariableByName(group.Settings, variableName);
                    schema.UseAssetBundleCrc = false;
                    schema.UseAssetBundleCrcForCachedBundles = false;
                    schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;

                    var so = new SerializedObject(schema);
                    var prop = so.FindProperty("m_LoadPath");
                    var prop2 = prop.FindPropertyRelative("m_Id");
                    prop2.stringValue = config.m_URL;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(schema);
                }
            }

            //Add all asset entries to group
            foreach (PWAddressableContentPackEntry entry in config.m_assetEntries)
            {
                AddGroupEntryByPath(group, entry.m_path, true, false);
            }

#endif
        }

        /// <summary>
        /// Clears groups with the PW prefix that do not contain any assets in them - repeated creation of the configuration can bloat the config with empty groups
        /// </summary>
        public static void ClearEmptyGroups()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            for (int i = AddressableAssetSettingsDefaultObject.Settings.groups.Count - 1; i >= 0; i--)
            {
                if (AddressableAssetSettingsDefaultObject.Settings.groups[i].name.StartsWith(m_PWGroupNamePrefix) && 
                    AddressableAssetSettingsDefaultObject.Settings.groups[i].entries.Count<=0 &&
                    !AddressableAssetSettingsDefaultObject.Settings.groups[i].name.Contains(m_PWGroupNamePrefix+m_PWDefaultGroupName)) //Exception for the Default group - that one is allowed to remain if empty!
                {
                    AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(AddressableAssetSettingsDefaultObject.Settings.groups[i]);
                }
            }
#endif
        }

        //Removes all groups with the PW prefix
        public static void RemoveAllPWGroups()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            for (int i = AddressableAssetSettingsDefaultObject.Settings.groups.Count - 1; i >= 0; i--)
            {
                if (AddressableAssetSettingsDefaultObject.Settings.groups[i].name.StartsWith(m_PWGroupNamePrefix))
                {
                    AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(AddressableAssetSettingsDefaultObject.Settings.groups[i]);
                }
            }
#endif
        }

        /// <summary>
        /// Returns a special name for a shared group belonging to a master scene. This is the group where shared assets will be added to when the configuration is updated.
        /// </summary>
        /// <param name="masterScene"></param>
        /// <returns></returns>
        public static string CreateSharedGroupName(Scene masterScene)
        {
            return m_PWGroupNamePrefix + "-" + masterScene.name + "-" + string.Format("-{0:yyyyMMdd - HHmmss}", DateTime.Now) + "-Shared";
        }

        /// <summary>
        /// Returns true if a PW Addressable configuration has been created for this project at some point.
        /// </summary>
        /// <returns></returns>
        public static bool HasConfig()
        {
#if UNITY_EDITOR && PW_ADDRESSABLES
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                return false;
            }

            for (int i = AddressableAssetSettingsDefaultObject.Settings.groups.Count - 1; i >= 0; i--)
            {
                if (AddressableAssetSettingsDefaultObject.Settings.groups[i].name.StartsWith(m_PWGroupNamePrefix))
                {
                    return true;
                }
            }
#endif
            return false;
        }
    }
}
