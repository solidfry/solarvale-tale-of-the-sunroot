using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Addressables1
{
    /// <summary>
    /// How the user wishes to use addressables in this project (Local as part of the regular build, or downloadable from a server)
    /// </summary>
    public enum AddressableMode { Server, Local }


    /// <summary>
    /// Stores the association between multiple terrains and a shared asset bundle group during collection of assets.
    /// Allows to intelligently bundle assets that exist only on single terrains or are shared between only a few terrains.
    /// </summary>
    [System.Serializable]
    public class TerrainAssetGroupAssociation
    {
        public string m_assetGroup;
        public List<string> m_terrainNames = new List<string>();
    }

    public class PWAddressablesConfig : ScriptableObject
    {
        [Tooltip("If the impostor scenes (if existing) should be included in the addressable configuration as well.")]
        public bool m_addressableIncludeImpostors = true;
        [Tooltip("If a collider-only server scene should be included in the addressable configuration as well.")]
        public bool m_addressableIncludeServerScene = true;
        [Tooltip("How you want to make use of the Addressable system - downloadable from a remote server, or locally embedded as part of the regular build.")]
        public AddressableMode m_addressableMode = AddressableMode.Server;
        [Tooltip("The Server URL where the Addressable files will be accessible from")]
        public string m_addressableServerURL = "https://";
        [Tooltip("When using a server, should a remote catalog be created during the addressable build? (This allows for remote content updates without deploying a new build)")]
        public bool m_addressableBuildRemoteCatalog = true;
        [Tooltip("The current Addressable bin file that defines the addressable update state when building update bundles.")]
        public UnityEngine.Object m_currentAddressableBinFile = null;
        [Tooltip("The custom file path where the addressable files are being copied to after building.")]
        public string m_customAddressableFolderPath = "";
        [Tooltip("The terrain <-> asset associations created during the initial config build.")]
        public List<TerrainAssetGroupAssociation> m_terrainAssetGroupAssociations = new List<TerrainAssetGroupAssociation>();
        [Tooltip("The last catalog URL used in a CDN addressable upload.")]
        public string m_lastCatalogURL = "";
        [Tooltip("The last master scene address used in a CDN addressable upload.")]
        public string m_lastMasterSceneAddress = "";
        [Tooltip("The index number of the currently selected CDN BucketIndex.")]
        [HideInInspector]
        public int m_currentCDNBucketIndex;
        [HideInInspector]
        public bool m_rebuildConfig;
    }
}
