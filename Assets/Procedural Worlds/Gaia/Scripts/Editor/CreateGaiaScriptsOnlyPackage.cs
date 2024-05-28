using Gaia.Internal;
using PWCommon5;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Creates a minimal "scripts only" Gaia package, which only contains the bare minimum scripts and settings for a working Gaia installation, but none of the art assets. 
/// Useful when you just need a gaia installation without the bloat of the assets, etc.
/// </summary>
/// 
namespace Gaia
{
    public class CreateGaiaScriptsOnlyPackage : MonoBehaviour
    {
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Gaia/Create Gaia 'Scripts Only' Package", false, 4000)]
        public static void CreatePackage()
        {
            List<string> exportedPackageAssetList = new List<string>();
            //Add Prefabs folder into the asset list
            string procFolder = "Assets/Procedural Worlds/";
            exportedPackageAssetList.Add($"{procFolder}Flora/Scripts");
            exportedPackageAssetList.Add($"{procFolder}Flora/Shaders");
            exportedPackageAssetList.Add($"{procFolder}Frameworks");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Asset Samples/Standard Assets");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Editor Resources");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Gaia Pro/Scripts");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Gaia Pro/Shaders");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Gaia Pro/Weather/VFX/Scripts");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Gaia Pro/HDRP Time Of Day");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Lighting/Content Resources/High Definition Pipeline");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Lighting/Content Resources/Universal Pipeline");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Lighting/Gaia Lighting System Profile.asset");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Localization");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Scripts");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Settings");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Shaders");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Shaders/Scripts");
            exportedPackageAssetList.Add($"{procFolder}Gaia/Water");


            //Export Shaders and Prefabs with their dependencies into a .unitypackage
            string packagePath = $"Assets/GaiaScriptsOnly V{PWApp.CONF.MajorVersion}{PWApp.CONF.MinorVersion}{PWApp.CONF.PatchVersion}.unitypackage";
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), packagePath,
                ExportPackageOptions.Recurse);
            string fullPath = GaiaDirectories.GetFullFileSystemPath(packagePath);
            OpenInFileBrowser.Open(fullPath);


        }
    }
}