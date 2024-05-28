using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    public enum PublicationType { Addressables, RegularBuild }

    public enum ImpostorState { Unknown, NoTerrainLoading, ImpostorsNotCreated, ImpostorsCreated }

    public enum BuildLogCategory
    {
        Impostors, ServerScene, CreatedAddressableConfig, AddressableBundles, ProjectBuild,
        ColliderBaking,
        UpdatedAddressableConfig
    }

    /// <summary>
    /// Stores an entry with build information for a specific (main) scene
    /// </summary>
    [System.Serializable]
    public class SceneBuildEntry
    {
#if UNITY_EDITOR
        public SceneAsset m_masterScene;
        public SceneAsset m_serverScene;
        public ImpostorState m_impostorState = ImpostorState.Unknown;
#endif
    }

    /// <summary>
    /// Stores an entry for the Build Log 
    /// </summary>
    [System.Serializable]
    public class BuildLogEntry
    {
        public BuildLogCategory m_category;
        public string m_sceneName = "";
        public long m_timestamp;
    }

    public class BuildConfig : ScriptableObject
    {
        public PublicationType m_publicationType = PublicationType.RegularBuild;

#if UNITY_EDITOR
        [Tooltip("The list of master scenes to be included for the addressable build.")]
        public List<SceneBuildEntry> m_sceneBuildEntries = new List<SceneBuildEntry>();
#endif
        [Tooltip("Entries for the Addressable build history.")]
        public List<BuildLogEntry> m_buildHistory = new List<BuildLogEntry>();


        /// <summary>
        /// Adds an entry to the build history for this config
        /// </summary>
        /// <param name="category">The category of the entry</param>
        /// <param name="sceneName">Name of the (main) scene this entry is logged for.</param>
        /// <param name="timestamp">The unix timestamp to log for this category</param>
        public void AddBuildHistoryEntry(BuildLogCategory category, string sceneName, long timestamp)
        {
            m_buildHistory.Add(new BuildLogEntry() { m_category = category, m_sceneName = sceneName, m_timestamp = timestamp });
        }

    }


}