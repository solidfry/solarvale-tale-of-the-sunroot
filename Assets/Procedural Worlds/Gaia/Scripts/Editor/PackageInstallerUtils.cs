using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Gaia;
using System;
using System.IO;
using PWCommon5;
using Gaia.Pipeline;
using System.Linq;
using Gaia.ShaderUtilities;
using UnityEditor.Rendering;

namespace ProceduralWorlds.Gaia.PackageSystem
{
    public class PackageInstallerUtils
    {
        //Public
        public static bool m_installShaders = false;
        public static float m_timer;
        //Private
        private static List<string> m_foldersToProcess = new List<string>();
        private static ShaderMappingEntry[] m_materialLibrary;
        private static float m_progressTimer;
        private static GaiaConstants.EnvironmentRenderer m_renderPipeline;
        private static string m_unityVersion;
        private static UnityPipelineProfile m_gaiaPipelineProfile;

        //Private const strings

        public static void StartInstallation(string unityVersion, GaiaConstants.EnvironmentRenderer renderPipeline, ShaderMappingEntry[] materialLibrary, UnityPipelineProfile pipelineProfile, bool showDialog = true)
        {
            //Set settings
            m_materialLibrary = materialLibrary;
            m_progressTimer = m_timer;
            m_renderPipeline = renderPipeline;
            m_unityVersion = unityVersion;
            m_gaiaPipelineProfile = pipelineProfile;

            //Checks if the material library is empty
            if (m_materialLibrary.Length == 0)
            {
                Debug.LogError("Material Library is empty. Please check the pipeline profile that it contains the necessary information");
                FinishInstallingPackages();
                return;
            }

            //Popup dialog to proceed
            if (showDialog)
            {
                if (EditorUtility.DisplayDialog("Importing Shaders and Materials", "You are about to install new shaders and materials to targeted pipeline and unity version. Please make sure you're using the correct SRP before you proceed. Are you sure you want to proceed?", "Yes", "No"))
                {
                    EditorUtility.DisplayProgressBar("Preparing Installation", "Preparing shader directories...", 0.5f);

                    StartInstallingPackage();
                }
                else
                {
                    //Finish and exit
                    FinishInstallingPackages();
                }
            }
            else
            {
                EditorUtility.DisplayProgressBar("Preparing Installation", "Preparing shader directories...", 0.5f);

                StartInstallingPackage();
            }

            var manager = EditorWindow.GetWindow<GaiaManagerEditor>(false, "Gaia Manager");
            //Manager can be null if the dependency package installation is started upon opening the manager window.
            if (manager != null)
            {
                //Perform the status check in the manager again to update the UI to the (hopefully successful) installation
                manager.GaiaManagerStatusCheck(true);
                manager.ResetToTerrainCreationTab();
            }
            m_installShaders = false;
        }

        /// <summary>
        /// Start install process
        /// </summary>
        private static void StartInstallingPackage2()
        {
            var manager = EditorWindow.GetWindow<GaiaManagerEditor>(false, "Gaia Manager");
            //Manager can be null if the dependency package installation is started upon opening the manager window.
            if (manager != null)
            {
                manager.Close();
            }
            m_installShaders = false;
            EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// Finish and exit installation
        /// </summary>
        private static void FinishInstallingPackages()
        {
            EditorUtility.ClearProgressBar();

            m_installShaders = false;

            var manager = EditorWindow.GetWindow<GaiaManagerEditor>(false, "Gaia Manager");
            //Manager can be null if the dependency package installation is started upon opening the manager window.
            if (manager != null)
            {
                manager.Show();
                manager.CheckForSetupIssues();
            }
        }

        /// <summary>
        /// Start installation
        /// </summary>
        private static void StartInstallingPackage()
        {
            EditorApplication.update -= EditorUpdate;

            bool updateChanges = false;
            bool promptRenderPipelineMaterialUpdate = false;

            updateChanges = GaiaUtils.ProcessMaterialLibrary();

            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains != null)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.UpdateGIMaterials();
                    terrain.Flush();
                }
            }

            if (updateChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (promptRenderPipelineMaterialUpdate)
            {
                GaiaUtils.DisplayDialogNoEditor("Please run the Pipeline Material upgrade process", "This project contains materials using the Unity Standard Shader. Please make sure to also run the Render Pipeline Material upgrade process to update those standard shaders. All shaders that are not updated via this process will not be displayed correctly in HDRP. Please look for the entry 'Upgrade Project Materials to High Definition Materials' under 'Edit > Render Pipelines' in your Window menu.", "OK", null);
            }

            FinishInstallingPackages();

        }
              
        private static void CleanUpFolder(string folderPath)
        {
            if (!folderPath.EndsWith("PWSF Functions") && !folderPath.EndsWith("PWS Procedural") && !folderPath.EndsWith("PWS Water Pro"))
            {
                string finalFolderName = "";

                if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor)
                    finalFolderName = folderPath.Substring(folderPath.LastIndexOf("/"));
                else
                    finalFolderName = folderPath.Substring(folderPath.LastIndexOf("\\"));

                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                var files = dirInfo.GetFiles();

                bool isSRPFolder = finalFolderName.Contains("Vegetation HD") ||
                                    finalFolderName.Contains("Vegetation LW") ||
                                    finalFolderName.Contains("Vegetation UP") ||
                                    finalFolderName.Contains("Ocean Pro HD") ||
                                    finalFolderName.Contains("Ocean Pro LW") ||
                                    finalFolderName.Contains("Ocean Pro UP");
                List<FileInfo> deletionCandidates = new List<FileInfo>();
                foreach (FileInfo file in files)
                {
                    if (!file.Name.EndsWith("meta"))
                    {
                        List<FileInfo> duplicates = files.Where(x => !x.Name.EndsWith("meta") && x.Name.Remove(x.Name.LastIndexOf(x.Extension)) == file.Name.Remove(file.Name.LastIndexOf(file.Extension))).ToList();
                        if (duplicates.Count() > 1)
                        {
                            foreach (FileInfo duplicateFile in duplicates)
                            {
                                if (isSRPFolder && !duplicateFile.Extension.EndsWith("file"))
                                {
                                    deletionCandidates.Add(duplicateFile);
                                }
                                if (!isSRPFolder && duplicateFile.Extension.EndsWith("file"))
                                {
                                    deletionCandidates.Add(duplicateFile);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < deletionCandidates.Count(); i++)
                {
                    FileUtil.DeleteFileOrDirectory(deletionCandidates[i].FullName);
                }
            }
        }

        private static void EditorUpdate()
        {
            m_timer -= Time.deltaTime;
            if (m_timer < 0)
            {
                StartInstallingPackage();
            }
            else
            {
                EditorUtility.DisplayProgressBar("Preparing Materials", "Preparing to upgrade material shaders...", m_progressTimer / m_timer);
            }
        }

        /// <summary>
        /// Removes Suffix in file formats required
        /// </summary>
        /// <param name="path"></param>
        private static bool RemoveFileSuffix(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            bool changes = false;
            foreach (FileInfo file in files)
            {
                if (file.Extension.EndsWith("file"))
                {
                    string fileName = file.FullName;
                    File.Move(fileName, fileName.Remove(fileName.Length - 4, 4));
                    changes = true;
                }
            }

            if (changes)
            {
                AssetDatabase.Refresh();
            }

            return changes;
        }

        /// <summary>
        /// Removes Suffix in file formats required
        /// </summary>
        /// <param name="path"></param>
        private static bool AddFileSuffix(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            bool changes = false;
            foreach (FileInfo file in files)
            {
                if (!file.Extension.EndsWith("file") && !file.Extension.EndsWith("meta"))
                {
                    string fileName = file.FullName;
                    File.Move(fileName, fileName.Replace(fileName, fileName + "file"));
                    changes = true;
                }
            }

            if (changes)
            {
                AssetDatabase.Refresh();
            }

            return changes;
        }

        /// <summary>
        /// Gets the shader
        /// </summary>
        /// <param name="path"></param>
        /// <param name="seachPattern"></param>
        /// <returns></returns>
        public static Shader GetShaderFile(string path, string seachPattern)
        {
            Shader returningShader = null;
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension.EndsWith("shader") && file.Name.Contains(seachPattern))
                {
                    returningShader = AssetDatabase.LoadAssetAtPath<Shader>(GaiaUtils.GetAssetPath(file.Name));
                    return returningShader;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the pipeline profile
        /// </summary>
        /// <returns></returns>
        private static UnityPipelineProfile GetPipelineProfile()
        {
            UnityPipelineProfile profile = null;

            GaiaSettings settings = GaiaUtils.GetGaiaSettings();
            if (settings != null)
            {
                profile = settings.m_pipelineProfile;
            }

            return profile;
        }
    }
}