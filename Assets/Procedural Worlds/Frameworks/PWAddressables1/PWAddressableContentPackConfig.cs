using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Addressables1
{

    /// <summary>
    /// Represents the entry for a single asset inside a PW content pack
    /// </summary>
    [System.Serializable]
    public class PWAddressableContentPackEntry
    {
        /// <summary>
        /// The address to the asset in question, starting at the folder the PWAddressableContentPackConfig sits in
        /// </summary>
        public string m_path;

        /// <summary>
        /// The name of the asset
        /// </summary>
        public string m_name;

        /// <summary>
        /// The index of the parent folder (-1 = asset sits on the root). 
        /// </summary>
        public int m_folderIndex = -1;
    }

    /// <summary>
    /// Represents a folder that holds assets for an addressable content pack
    /// </summary>
    [System.Serializable]
    public class PWAddressableContentPackFolder
    {
        /// <summary>
        /// Whether the folder is open / unfolded on the Editor UI.
        /// </summary>
        public bool m_unfolded;

        /// <summary>
        /// The name of the folder
        /// </summary>
        public string m_name;

        /// <summary>
        /// The index of the parent folder (-1 = asset sits on the root). 
        /// </summary>
        public int m_parentIndex = -1;
    }



    /// <summary>
    /// Scriptable object to hold information about addressable group configuration for content packs. The scriptable object files need to be placed
    /// at the root of a content pack folder, and then hold the information about how the assets in this folder should be configured as an adressable group.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Content Pack Config")]
    [System.Serializable]
    public class PWAddressableContentPackConfig : ScriptableObject
    {
        public string m_contentPackName;
        public string m_URL;
        public List<PWAddressableContentPackFolder> m_contentFolders = new List<PWAddressableContentPackFolder>();
        public List<PWAddressableContentPackEntry> m_assetEntries = new List<PWAddressableContentPackEntry>();

        public void CollectAssets()
        {
#if UNITY_EDITOR
            string rootPath = AssetDatabase.GetAssetPath(this);
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));
            if (string.IsNullOrEmpty(rootPath))
            {
                Debug.LogError("Could not find a root path to collect assets from, is this content pack config saved properly in this project yet?");
                return;
            }

            m_contentFolders.Clear();
            m_assetEntries.Clear();

            CollectFilesFromDirectory(-1, rootPath);
#endif
        }

        public void AddToConfig()
        {
            PWAddressables.AddContentPackConfig(this);
        }

        private void CollectFilesFromDirectory(int currentFolderIndex, string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            m_contentFolders.Add(new PWAddressableContentPackFolder() { m_name = dirInfo.Name, m_parentIndex = currentFolderIndex });
            currentFolderIndex = m_contentFolders.Count-1;
            DirectoryInfo[] allSubDirectories = dirInfo.GetDirectories();
            FileInfo[] allFiles = dirInfo.GetFiles();
            foreach (FileInfo fileInfo in allFiles)
            {
                if (fileInfo.Extension != ".meta" && fileInfo.Extension != ".cs" && fileInfo.Name.Substring(0,fileInfo.Name.Length - fileInfo.Extension.Length) != this.name)
                {
                    m_assetEntries.Add(new PWAddressableContentPackEntry() {m_name = fileInfo.Name, m_path = GetPathStartingAtAssetsFolder(fileInfo.FullName).Replace('\\','/'), m_folderIndex = currentFolderIndex });
                }
            }

            foreach (DirectoryInfo subDir in allSubDirectories)
            {
                CollectFilesFromDirectory(currentFolderIndex, subDir.FullName);
            }
        }

        private string GetPathStartingAtAssetsFolder(string inputPath)
        {
            return inputPath.Substring(Application.dataPath.Length - "Assets".Length);
        }
    }

}