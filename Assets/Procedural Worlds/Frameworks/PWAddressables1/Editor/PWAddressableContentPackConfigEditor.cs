using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if !UNITY_2021_2_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#endif
using UnityEngine;

namespace ProceduralWorlds.Addressables1
{

    [CustomEditor(typeof(PWAddressableContentPackConfig))]
    public class PWAddressableContentPackConfigEditor : Editor
    {
        private PWAddressableContentPackConfig m_contentPackconfig;
        private GUIStyle m_headerStyle;
        public override void OnInspectorGUI()
        {
            if (m_headerStyle == null)
            {
                m_headerStyle = new GUIStyle(GUI.skin.label);
                m_headerStyle.fontStyle = FontStyle.Bold;
            }
            EditorGUILayout.LabelField("Content Pack Configuration:", m_headerStyle);
            EditorGUI.BeginChangeCheck();
            m_contentPackconfig = (PWAddressableContentPackConfig)target;
            m_contentPackconfig.m_contentPackName = EditorGUILayout.TextField("Content Pack Name", m_contentPackconfig.m_contentPackName);
            m_contentPackconfig.m_URL = EditorGUILayout.DelayedTextField("Content Pack Location", m_contentPackconfig.m_URL);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_contentPackconfig);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.LabelField("Assets in this Content Pack:", m_headerStyle);

            PWAddressableContentPackFolder rootFolder = m_contentPackconfig.m_contentFolders.Find(x => x.m_parentIndex == -1);

            if (rootFolder != null)
            {
                EditorGUI.indentLevel++;
                DisplayFolderFoldout(rootFolder);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("No Assets were collected for this content pack yet! Place this configuration file in the folder that contains the assets for the pack, and click the 'Collect Assets' button below. All Files in that folder (including subfolders) will be added to this content pack configuration.", MessageType.Warning);
            }


            if (GUILayout.Button("Collect Assets"))
            {
                m_contentPackconfig.CollectAssets();
                EditorUtility.SetDirty(m_contentPackconfig);
            }

            bool currentGUIState = GUI.enabled;

            if (rootFolder == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Add Collected Assets to Addressable Config"))
            {
                m_contentPackconfig.AddToConfig();
                PWAddressables.OpenAddressableSettingsWindow();
            }

            if (GUILayout.Button("Export As Unity Package"))
            {
                string packagePath = $"Assets/{m_contentPackconfig.name}.unitypackage";
                string configPackFilename = AssetDatabase.GetAssetPath(m_contentPackconfig);
                if (string.IsNullOrEmpty(configPackFilename))
                {
                    Debug.LogError("Could not get the path to the configuration file you are editing - has the project been saved yet?");
                }
                string configFolder = configPackFilename.Substring(0, configPackFilename.LastIndexOf('/'));

                AssetDatabase.ExportPackage(new string[1] { configFolder }, packagePath,
                    ExportPackageOptions.Recurse);
                string fullPath = GetFullFileSystemPath(packagePath);
                OpenInFileBrowser.Open(fullPath);
            }

            GUI.enabled = currentGUIState;
        }

        private void DisplayFolderFoldout(PWAddressableContentPackFolder folder)
        {
            folder.m_unfolded = EditorGUILayout.Foldout(folder.m_unfolded, folder.m_name);
            if (folder.m_unfolded)
            {
                EditorGUI.indentLevel++;
                int folderIndex = m_contentPackconfig.m_contentFolders.IndexOf(folder);
                foreach (PWAddressableContentPackEntry entry in m_contentPackconfig.m_assetEntries.Where(x => x.m_folderIndex == folderIndex))
                {
                    EditorGUILayout.LabelField(entry.m_name);
                }

                foreach (PWAddressableContentPackFolder subfolder in m_contentPackconfig.m_contentFolders.Where(x => x.m_parentIndex == folderIndex))
                {
                    DisplayFolderFoldout(subfolder);
                }
                EditorGUI.indentLevel--;

            }
        }

        private string GetFullFileSystemPath(string inputPath)
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + inputPath;
        }
    }
}