using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Collections.Generic;
using System;

public class AddNecessaryRendererFeatures
{
    static string cloudShadowFeatureName = "UniStormCloudShadowsFeature";
    static string atmosphericFogFeatureName = "UniStormAtmosphericFogFeature";
    static string sunShaftsFeatureName = "SunUniStormSunShaftsFeature";
    static string moonShaftsFeatureName = "MoonUniStormSunShaftsFeature";

    static UniversalRenderPipelineAsset universalRenderPipelineAsset;
    static ScriptableRendererData scriptableRendererData;

    //Used in case the user doesn't have a UniversalRenderPipelineAsset asigned and they need to refresh the AddNecessaryRendererFeatures process.
    [MenuItem("Window/UniStorm/Add URP Renderer Features", false, 300)]
    static void CheckRendererFeatures()
    {
        AddNecessaryRendererFeaturesInternal();
    }

    private static void SetupSceneLoad()
    {
        UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += EditorSceneManager_activeSceneChangedInEditMode;
    }

    private static void EditorSceneManager_activeSceneChangedInEditMode(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        SetForwardRendererToUniStormSystem();
        TurnOnDepthTexture();
    }

    private static void TurnOnDepthTexture()
    {
        ExtractRenderPipelineAsset();
        if (universalRenderPipelineAsset == null || universalRenderPipelineAsset.supportsCameraDepthTexture) return;
        universalRenderPipelineAsset.supportsCameraDepthTexture = true;
    }

    private static void SetForwardRendererToUniStormSystem()
    {
        #if UNITY_2021_3_OR_NEWER
        UniStorm.UniStormSystem unistormSystem = UnityEngine.Object.FindObjectOfType<UniStorm.UniStormSystem>();
        if (unistormSystem != null && scriptableRendererData != null)
        {
            unistormSystem.urpForwardRendererData = scriptableRendererData as UniversalRendererData;
        }
        #else
        UniStorm.UniStormSystem unistormSystem = UnityEngine.Object.FindObjectOfType<UniStorm.UniStormSystem>();
        if (unistormSystem != null && scriptableRendererData != null)
        {
            unistormSystem.urpForwardRendererData = scriptableRendererData as ForwardRendererData;
        }
        #endif
    }

    private static void ExtractRenderPipelineAsset()
    {
        universalRenderPipelineAsset = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
    }

    private static void ExtractScriptableRendererData()
    {
        FieldInfo propertyInfo = universalRenderPipelineAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(universalRenderPipelineAsset))?[0];
    }

    private static void DeleteUnusedFiles()
    {
        bool fileDeleted = false;

        //check if UniStorm Auroras Shader exist
        string[] aurorasShaders = AssetDatabase.FindAssets("UniStorm Auroras t:shader", new string[] { "Assets/UniStorm Weather System/Resources/Shaders/Resources" });
        if (aurorasShaders.Length > 0)
        {
            foreach (string auroraShaderGUID in aurorasShaders)
            {
                string auroraShaderPath = AssetDatabase.GUIDToAssetPath(auroraShaderGUID);
                if (!String.IsNullOrEmpty(auroraShaderPath))
                {
                    if (auroraShaderPath.EndsWith("Auroras.shader"))
                    {
                        AssetDatabase.DeleteAsset(auroraShaderPath);
                        fileDeleted = true;
                        break;
                    }
                }
            }
        }

        string[] atmosphericFogShader = AssetDatabase.FindAssets("UniStormAtmosphericFog t:shader", new string[] { "Assets/UniStorm Weather System/Resources/Shaders/Resources" });
        if (atmosphericFogShader.Length > 0 && !string.IsNullOrEmpty(atmosphericFogShader[0]))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(atmosphericFogShader[0]));
            fileDeleted = true;
        }

        string[] sunShaftsShader = AssetDatabase.FindAssets("UniStormSunShafts t:shader", new string[] { "Assets/UniStorm Weather System/Resources/Shaders/Resources" });
        if (sunShaftsShader.Length > 0 && !string.IsNullOrEmpty(sunShaftsShader[0]))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(sunShaftsShader[0]));
            fileDeleted = true;
        }

        if (fileDeleted)
            AssetDatabase.Refresh();
    }

    private static void AddNecessaryRendererFeaturesInternal()
    {
        ExtractRenderPipelineAsset();

        if (universalRenderPipelineAsset == null)
        {
            Debug.LogError("There is no Render Pipeline Asset applied. Please assign one to Edit>Project Settings>Graphics>Scriptable Render Pipeline Settings. After you have assigned the Scriptable Render Pipeline asset, go to Window>UniStorm>Add URP Renderer Features within Unity. " +
                "If you need further help, refer to Step 3 of the Setting up UniStorm with URP guide here:         https://github.com/Black-Horizon-Studios/UniStorm-Weather-System/wiki/Setting-up-UniStorm-with-URP#step-3");
            return;
        }

        ExtractScriptableRendererData();
        SetupSceneLoad();
        List<string> featuresFound = new List<string>();
        foreach (ScriptableRendererFeature feature in scriptableRendererData.rendererFeatures)
        {
            if (!featuresFound.Contains(feature.name))
            {
                featuresFound.Add(feature.name);
            }
        }

        if (!featuresFound.Contains(cloudShadowFeatureName))
        {
            //add cloud shadows feature
            UniStormCloudShadowsFeature cloudShadowFeature = ScriptableObject.CreateInstance<UniStormCloudShadowsFeature>();
            cloudShadowFeature.name = cloudShadowFeatureName;
            EditorUtility.SetDirty(cloudShadowFeature);
            AssetDatabase.AddObjectToAsset(cloudShadowFeature, scriptableRendererData);
            scriptableRendererData.rendererFeatures.Add(cloudShadowFeature);
            AssetDatabase.SaveAssets();
        }

        if (!featuresFound.Contains(atmosphericFogFeatureName))
        {
            //add atmospheric fog feature
            UniStormAtmosphericFogFeature atmosphericFogFeature = ScriptableObject.CreateInstance<UniStormAtmosphericFogFeature>();
            atmosphericFogFeature.name = atmosphericFogFeatureName;
            EditorUtility.SetDirty(atmosphericFogFeature);
            AssetDatabase.AddObjectToAsset(atmosphericFogFeature, scriptableRendererData);
            scriptableRendererData.rendererFeatures.Add(atmosphericFogFeature);
            AssetDatabase.SaveAssets();
        }

        if (!featuresFound.Contains(sunShaftsFeatureName))
        {
            //add sun shafts feature
            UniStormSunShaftsFeature sunShaftsFeature = ScriptableObject.CreateInstance<UniStormSunShaftsFeature>();
            sunShaftsFeature.name = sunShaftsFeatureName;
            EditorUtility.SetDirty(sunShaftsFeature);
            AssetDatabase.AddObjectToAsset(sunShaftsFeature, scriptableRendererData);
            scriptableRendererData.rendererFeatures.Add(sunShaftsFeature);
            AssetDatabase.SaveAssets();
        }

        if (!featuresFound.Contains(moonShaftsFeatureName))
        {
            //add moon shafts feature
            UniStormSunShaftsFeature moonShaftsFeature = ScriptableObject.CreateInstance<UniStormSunShaftsFeature>();
            moonShaftsFeature.name = moonShaftsFeatureName;
            EditorUtility.SetDirty(moonShaftsFeature);
            AssetDatabase.AddObjectToAsset(moonShaftsFeature, scriptableRendererData);
            scriptableRendererData.rendererFeatures.Add(moonShaftsFeature);
            AssetDatabase.SaveAssets();
        }

        DeleteUnusedFiles();
    }

}