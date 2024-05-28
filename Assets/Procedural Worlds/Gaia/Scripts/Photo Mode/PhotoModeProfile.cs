using System.Collections.Generic;
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public enum ColorPickerReferenceMode { FogColor, SunColor, SkyboxTintColor, DensityAlbedoColor, UnderwaterFogColor, AmbientSkyColor, AmbientEquatorColor, AmbientGroundColor }

    [System.Serializable]
    public class GrassRenderingProfileData
    {
        public string m_terrainName;
        public float m_additionalNearDistance = 0f;
        public float m_additionalFarDistance = 0f;
#if FLORA_PRESENT
        public FloraTerrainTile m_tileData;
#endif
        public bool m_distancesSaved = false;
        public List<SavedGrassRenderingSettings> m_savedData = new List<SavedGrassRenderingSettings>();
    }

    [System.Serializable]
    public class SavedGrassRenderingSettings
    {
        public float m_nearFade;
        public float m_farFade;
    }
    [System.Serializable]
    public class PhotoModeValues
    {
        #region Variables

        #region Global

        public bool m_isUsingGaiaLighting = false;
        public int m_selectedGaiaLightingProfile = -1;
        public string m_lastSceneName;

        #endregion
        #region Photo Mode Settings

        public int m_screenshotResolution = 0;
        public int m_screenshotImageFormat = 2;
        public bool m_loadSavedSettings = true;
        public bool m_revertOnDisabled = true;
        public bool m_showFPS = false;
        public bool m_showReticle = false;
        public bool m_showRuleOfThirds = false;

        #endregion
        #region Unity Settings

        public float m_lodBias = 2f;
        public int m_vSync = 0;
        public int m_targetFPS = -1;
        public int m_antiAliasing = 1;
        public float m_shadowDistance = 512f;
        public int m_shadowResolution = 3;
        public int m_shadowCascades = 4;
        public float m_fieldOfView = 60f;
        public float m_gaiaCullinDistance = 0f;
        public float m_cameraAperture = 19f;
        public float m_cameraFocalLength = 50f;
        public float m_globalVolume = 1f;
        public float m_cameraRoll = 0f;
        public float m_farClipPlane = 2000f;

        #endregion
        #region Streaming Settings

        public float m_gaiaLoadRange = 1000;
        public float m_gaiaImpostorRange = 2000;

        #endregion
        #region Weather Settings

        public bool m_gaiaWeatherEnabled = false;
        public bool m_gaiaWeatherRain = false;
        public bool m_gaiaWeatherSnow = false;
        public bool m_gaiaWindSettingsOverride = false;
        public float m_gaiaWindDirection = 0f;
        public float m_gaiaWindSpeed = 0.35f;

        #endregion
        #region Gaia Lighting Settings

        public float m_gaiaTime = 15f;
        public bool m_gaiaTimeOfDayEnabled = false;
        public float m_gaiaTimeScale = 1f;
        public float m_gaiaAdditionalLinearFog = 0f;
        public float m_gaiaAdditionalExponentialFog = 0f;
        public float m_sunRotation = 0f;
        public float m_sunPitch = 75f;
        public bool m_fogOverride = false;
        public FogMode m_fogMode = FogMode.Exponential;
        public Color m_fogColor = Color.black;
        public float m_fogDensity = 0.01f;
        public float m_fogStart = 100f;
        public float m_fogEnd = 1000f;
        public bool m_skyboxOverride = false;
        public float m_skyboxRotation = 0f;
        public float m_skyboxExposure = 1f;
        public Color m_skyboxTint = Color.black;
        public bool m_sunOverride = false;
        public float m_sunIntensity = 1f;
        public Color m_sunColor = Color.black;
        public float m_sunKelvinValue = 6500f;
        public float m_ambientIntensity = 1f;
        public Color m_ambientSkyColor = new Color(0.7027151f, 0.881016f, 1.001631f, 0.4192761f);
        public Color m_ambientEquatorColor = new Color(0.6302439f, 0.7919513f, 0.85f, 0f);
        public Color m_ambientGroundColor = new Color(0.5f, 0.4142857f, 0.3321428f, 0f);
        public float m_globalLightIntensityMultiplier = 1f;
        public float m_globalFogDensityMultiplier = 1f;
        public float m_globalShadowDistanceMultiplier = 1f;

        #endregion
        #region Water Settings

        public bool m_gaiaWaterReflectionEnabled = true;
        public float m_gaiaWaterReflectionDistance = 0f;
        public int m_gaiaWaterReflectionResolution = 2;
        public float m_gaiaReflectionsLODBias = 1f;
        public Color m_gaiaUnderwaterFogColor = Color.black;
        public float m_gaiaUnderwaterFogDensity = 0.045f;
        public float m_gaiaUnderwaterFogDistance = 45f;
        public float m_gaiaUnderwaterVolume = 0.5f;

        #endregion
        #region Post FX Settings

        public float m_postFXExposure = 13.5f;
        public int m_postFXExposureMode = 0;

        public bool m_dofActive = true;
        public bool m_autoDOFFocus = true;
        public float m_dofFocusDistance = 100f;
        public float m_dofAperture = 16f;
        public float m_dofFocalLength = 50f;
        public int m_dofKernelSize = 1;
        public int m_savedDofFocusMode = 0;

        #region HDRP

        public int m_dofFocusModeHDRP = 0;
        public int m_dofFocusModeURP = 0;
        public int m_dofQualityHDRP = 1;
        public float m_dofNearBlurStart = 0f;
        public float m_dofNearBlurEnd = 0.1f;
        public float m_dofFarBlurStart = 200f;
        public float m_dofFarBlurEnd = 2000f;

        #endregion
        #region URP

        public float m_dofStartBlurURP = 2f;
        public float m_dofEndBlurURP = 1000f;
        public float m_dofMaxRadiusBlur = 1f;
        public bool m_dofHighQualityURP = false;

        #endregion

        #endregion
        #region Terrain Settings

        public float m_terrainDetailDensity = 0.5f;
        public float m_terrainDetailDistance = 150f;
        public float m_terrainPixelError = 5f;
        public float m_terrainBasemapDistance = 1024f;
        public bool m_drawInstanced = true;

        #endregion
        #region HDRP Density Volume

        public bool m_overrideDensityVolume = false;
        public Color m_densityVolumeAlbedoColor = Color.white;
        public float m_densityVolumeFogDistance = 250f;
        public int m_densityVolumeEffectType = 1;
        public int m_densityVolumeTilingResolution = 3;

        #endregion
        #region Grass System

        public float m_globalGrassDensity = 1f;
        public float m_globalGrassDistance = 1f;
        public float m_cameraCellDistance = 1f;
        public int m_cameraCellSubdivision = 0;

        #endregion

        #endregion
        #region Dropdown Options

        public List<string> GetDefaultToggleOptions()
        {
            return new List<string> {"Disabled", "Enabled"};
        }
        public List<string> GetScreenResolutionOptions()
        {
            return new List<string> {"Screen Size", "640 x 480", "800 x 600", "1280 x 720", "1366 x 768", "1600 x 900", "1920 x 1080", "2560 x 1440", "3840 x 2160", "7680 x 4320"};
        }
        public List<string> GetScreenshotFormatOptions()
        {
            return new List<string> {"EXR", "JPG", "PNG", "TGA"};
        }
        public List<string> GetAntiAliasingOptions()
        {
            GaiaConstants.EnvironmentRenderer pipeline = GaiaUtils.GetActivePipeline();
            switch (pipeline)
            {
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
                    return new List<string> {"None", "FXAA", "SMAA"};
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
                    return new List<string> {"None", "FXAA", "TAA", "SMAA"};
                }
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    return new List<string> {"None", "FXAA", "SMAA", "TAA"};
                }
            }

            return new List<string>();
        }
        public List<string> GetVsyncOptions()
        {
            return new List<string> {"Don't Sync", "Every V Blank", "Every Second V Blank"};
        }
        public List<string> GetDOFModeOptions()
        {
            GaiaConstants.EnvironmentRenderer pipeline = GaiaUtils.GetActivePipeline();
            switch (pipeline)
            {
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
                    return new List<string> {"None", "Gaussian", "Bokeh"};
                }
                default:
                {
                    return new List<string> {"None", "Physical Camera", "Manual"};
                }
            }
        }
        public List<string> GetDOFQualityOptions()
        {
            return new List<string> {"Low", "Medium", "High"};
        }
        public List<string> GetWaterReflectionQualityOptions()
        {
            return new List<string> {"Very Low", "Low", "Medium", "High", "Ultra"};
        }
        public List<string> GetShadowQualityOptions()
        {
            return new List<string> {"Low", "Medium", "High", "Ultra"};
        }
        public List<string> GetShadowCascadeOptions()
        {
            GaiaConstants.EnvironmentRenderer pipeline = GaiaUtils.GetActivePipeline();
            switch (pipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
                    return new List<string> {"None", "Two Cascade", "Four Cascade"};
                }
                default:
                {
                    return new List<string> {"One Cascade", "Two Cascade", "Three Cascade", "Four Cascade"};
                }
            }
        }
        public List<string> GetFogModeOptions()
        {
            return new List<string> {"Linear", "Exponential", "Exponential Squared"};
        }
        public List<string> GetKernelSizeOptions()
        {
            return new List<string> {"Small", "Medium", "Large", "Very Large"};
        }
        public List<string> GetDensityVolumeEffectTypeOptions()
        {
            return new List<string> {"Very Light Haze", "Light Haze", "Moderate Haze", "High Haze", "Extreme Haze"};
        }
        public List<string> GetDensityVolumeTilingResolutionOptions()
        {
            return new List<string> {"Very Low", "Low", "Medium", "High", "Very High", "Ultra"};
        }

        #endregion
        #region Save and Load

        /// <summary>
        /// Loads from a profile
        /// </summary>
        /// <param name="loadFrom"></param>
        public void Load(PhotoModeValues loadFrom)
        {
            //Global
            if (m_selectedGaiaLightingProfile != loadFrom.m_selectedGaiaLightingProfile)
            {
                m_isUsingGaiaLighting = false;
            }
            else if (loadFrom.m_selectedGaiaLightingProfile >= 0)
            {
                m_isUsingGaiaLighting = true;
            }
            m_isUsingGaiaLighting = loadFrom.m_isUsingGaiaLighting;
            m_selectedGaiaLightingProfile = loadFrom.m_selectedGaiaLightingProfile;
            //Screenshot Settings
            m_screenshotResolution = loadFrom.m_screenshotResolution;
            m_screenshotImageFormat = loadFrom.m_screenshotImageFormat;
            m_loadSavedSettings = loadFrom.m_loadSavedSettings;
            m_revertOnDisabled = loadFrom.m_revertOnDisabled;
            m_showFPS = loadFrom.m_showFPS;
            m_showReticle = loadFrom.m_showReticle;
            m_showRuleOfThirds = loadFrom.m_showRuleOfThirds;
            //Unity settings
            m_lodBias = loadFrom.m_lodBias;
            m_vSync = loadFrom.m_vSync;
            m_targetFPS = loadFrom.m_targetFPS;
            m_antiAliasing = loadFrom.m_antiAliasing;
            m_shadowDistance = loadFrom.m_shadowDistance;
            m_shadowResolution = loadFrom.m_shadowResolution;
            m_shadowCascades = loadFrom.m_shadowCascades;
            m_fieldOfView = loadFrom.m_fieldOfView;
            m_gaiaCullinDistance = loadFrom.m_gaiaCullinDistance;
            m_cameraAperture = loadFrom.m_cameraAperture;
            m_cameraFocalLength = loadFrom.m_cameraFocalLength;
            m_globalVolume = loadFrom.m_globalVolume;
            m_cameraRoll = loadFrom.m_cameraRoll;
            m_farClipPlane = loadFrom.m_farClipPlane;
            //Streaming Settings
            m_gaiaLoadRange = loadFrom.m_gaiaLoadRange;
            m_gaiaImpostorRange = loadFrom.m_gaiaImpostorRange;
            //Weather Settings
            m_gaiaWeatherEnabled = loadFrom.m_gaiaWeatherEnabled;
            m_gaiaWeatherRain = loadFrom.m_gaiaWeatherRain;
            m_gaiaWeatherSnow = loadFrom.m_gaiaWeatherSnow;
            m_gaiaWindDirection = loadFrom.m_gaiaWindDirection;
            m_gaiaWindSpeed = loadFrom.m_gaiaWindSpeed;
            m_gaiaWindSettingsOverride = loadFrom.m_gaiaWindSettingsOverride;
            //Gaia Lighting Settings
            m_gaiaTime = loadFrom.m_gaiaTime;
            m_gaiaTimeOfDayEnabled = loadFrom.m_gaiaTimeOfDayEnabled;
            m_gaiaTimeScale = loadFrom.m_gaiaTimeScale;
            m_gaiaAdditionalLinearFog = loadFrom.m_gaiaAdditionalLinearFog;
            m_gaiaAdditionalExponentialFog = loadFrom.m_gaiaAdditionalExponentialFog;
            m_sunRotation = loadFrom.m_sunRotation;
            m_sunPitch = loadFrom.m_sunPitch;
            m_fogOverride = loadFrom.m_fogOverride;
            m_fogMode = loadFrom.m_fogMode;
            m_fogColor = loadFrom.m_fogColor;
            m_fogDensity = loadFrom.m_fogDensity;
            m_fogStart = loadFrom.m_fogStart;
            m_fogEnd = loadFrom.m_fogEnd;
            m_skyboxOverride = loadFrom.m_skyboxOverride;
            m_skyboxRotation = loadFrom.m_skyboxRotation;
            m_skyboxExposure = loadFrom.m_skyboxExposure;
            m_skyboxTint = loadFrom.m_skyboxTint;
            m_ambientIntensity = loadFrom.m_ambientIntensity;
            m_sunOverride = loadFrom.m_sunOverride;
            m_sunIntensity = loadFrom.m_sunIntensity;
            m_sunColor = loadFrom.m_sunColor;
            m_sunKelvinValue = loadFrom.m_sunKelvinValue;
            m_ambientSkyColor = loadFrom.m_ambientSkyColor;
            m_ambientEquatorColor = loadFrom.m_ambientEquatorColor;
            m_ambientGroundColor = loadFrom.m_ambientGroundColor;
            //Density Volume
            m_overrideDensityVolume = loadFrom.m_overrideDensityVolume;
            m_densityVolumeAlbedoColor = loadFrom.m_densityVolumeAlbedoColor;
            m_densityVolumeFogDistance = loadFrom.m_densityVolumeFogDistance;
            m_densityVolumeEffectType = loadFrom.m_densityVolumeEffectType;
            m_densityVolumeTilingResolution = loadFrom.m_densityVolumeTilingResolution;
            //Water Settings
            m_gaiaWaterReflectionEnabled = loadFrom.m_gaiaWaterReflectionEnabled;
            m_gaiaWaterReflectionDistance = loadFrom.m_gaiaWaterReflectionDistance;
            m_gaiaWaterReflectionResolution = loadFrom.m_gaiaWaterReflectionResolution;
            m_gaiaReflectionsLODBias = loadFrom.m_gaiaReflectionsLODBias;
            m_gaiaUnderwaterFogColor = loadFrom.m_gaiaUnderwaterFogColor;
            m_gaiaUnderwaterFogDensity = loadFrom.m_gaiaUnderwaterFogDensity;
            m_gaiaUnderwaterFogDistance = loadFrom.m_gaiaUnderwaterFogDistance;
            m_gaiaUnderwaterVolume = loadFrom.m_gaiaUnderwaterVolume;
            //Post FX Settings
            m_dofFocusModeURP = loadFrom.m_dofFocusModeURP;
            m_dofStartBlurURP = loadFrom.m_dofStartBlurURP;
            m_dofEndBlurURP = loadFrom.m_dofEndBlurURP;
            m_dofHighQualityURP = loadFrom.m_dofHighQualityURP;

            m_dofActive = loadFrom.m_dofActive;
            m_autoDOFFocus = loadFrom.m_autoDOFFocus;
            m_dofFocusDistance = loadFrom.m_dofFocusDistance;
            m_dofAperture = loadFrom.m_dofAperture;
            m_dofKernelSize = loadFrom.m_dofKernelSize;
            m_dofFocalLength = loadFrom.m_dofFocalLength;
            m_dofFocusModeHDRP = loadFrom.m_dofFocusModeHDRP;
            m_dofQualityHDRP = loadFrom.m_dofQualityHDRP;
            m_dofNearBlurStart = loadFrom.m_dofNearBlurStart;
            m_dofNearBlurEnd = loadFrom.m_dofNearBlurEnd;
            m_dofFarBlurStart = loadFrom.m_dofFarBlurStart;
            m_dofFarBlurEnd = loadFrom.m_dofFarBlurEnd;
            m_postFXExposure = loadFrom.m_postFXExposure;
            m_postFXExposureMode = loadFrom.m_postFXExposureMode;
            m_savedDofFocusMode = loadFrom.m_savedDofFocusMode;
            //Terrain Settings
            m_terrainDetailDensity = loadFrom.m_terrainDetailDensity;
            m_terrainDetailDistance = loadFrom.m_terrainDetailDistance;
            m_terrainPixelError = loadFrom.m_terrainPixelError;
            m_terrainBasemapDistance = loadFrom.m_terrainBasemapDistance;
            m_drawInstanced = loadFrom.m_drawInstanced;
            //Grass Settings
            m_globalGrassDensity = loadFrom.m_globalGrassDensity;
            m_globalGrassDistance = loadFrom.m_globalGrassDistance;
            m_cameraCellDistance = loadFrom.m_cameraCellDistance;
            m_cameraCellSubdivision = loadFrom.m_cameraCellSubdivision;
        }
        /// <summary>
        /// Saves values here to a profile
        /// </summary>
        /// <param name="saveTo"></param>
        public void Save(PhotoModeValues saveTo)
        {
            //Global
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                SceneProfile sceneProfile = GaiaGlobal.Instance.SceneProfile;
                if (sceneProfile.m_selectedLightingProfileValuesIndex >= 0)
                {
                    m_isUsingGaiaLighting = true;
                    m_selectedGaiaLightingProfile = sceneProfile.m_selectedLightingProfileValuesIndex;
                }
                else
                {
                    m_isUsingGaiaLighting = false;
                    m_selectedGaiaLightingProfile = -1;
                }
                saveTo.m_isUsingGaiaLighting = m_isUsingGaiaLighting;
                saveTo.m_selectedGaiaLightingProfile = m_selectedGaiaLightingProfile;
            }
            else
            {
                saveTo.m_isUsingGaiaLighting = false;
                saveTo.m_selectedGaiaLightingProfile = -1;
            }
            //Screenshot Settings
            saveTo.m_screenshotResolution = m_screenshotResolution;
            saveTo.m_screenshotImageFormat = m_screenshotImageFormat;
            saveTo.m_loadSavedSettings = m_loadSavedSettings;
            saveTo.m_revertOnDisabled = m_revertOnDisabled;
            saveTo.m_showFPS = m_showFPS;
            saveTo.m_showReticle = m_showReticle;
            saveTo.m_showRuleOfThirds = m_showRuleOfThirds;
            //Unity settings
            saveTo.m_lodBias = m_lodBias;
            saveTo.m_vSync = m_vSync;
            saveTo.m_targetFPS = m_targetFPS;
            saveTo.m_antiAliasing = m_antiAliasing;
            saveTo.m_shadowDistance = m_shadowDistance;
            saveTo.m_shadowResolution = m_shadowResolution;
            saveTo.m_shadowCascades = m_shadowCascades;
            saveTo.m_fieldOfView = m_fieldOfView;
            saveTo.m_gaiaCullinDistance = m_gaiaCullinDistance;
            saveTo.m_cameraAperture = m_cameraAperture;
            saveTo.m_cameraFocalLength = m_cameraFocalLength;
            saveTo.m_globalVolume = m_globalVolume;
            saveTo.m_cameraRoll = m_cameraRoll;
            saveTo.m_farClipPlane = m_farClipPlane;
            //Streaming Settings
            saveTo.m_gaiaLoadRange = m_gaiaLoadRange;
            saveTo.m_gaiaImpostorRange = m_gaiaImpostorRange;
            //Weather Settings
            saveTo.m_gaiaWeatherEnabled = m_gaiaWeatherEnabled;
            saveTo.m_gaiaWeatherRain = m_gaiaWeatherRain;
            saveTo.m_gaiaWeatherSnow = m_gaiaWeatherSnow;
            saveTo.m_gaiaWindDirection = m_gaiaWindDirection;
            saveTo.m_gaiaWindSpeed = m_gaiaWindSpeed;
            saveTo.m_gaiaWindSettingsOverride = m_gaiaWindSettingsOverride;
            //Gaia Lighting Settings
            saveTo.m_gaiaTime = m_gaiaTime;
            saveTo.m_gaiaTimeOfDayEnabled = m_gaiaTimeOfDayEnabled;
            saveTo.m_gaiaTimeScale = m_gaiaTimeScale;
            saveTo.m_gaiaAdditionalLinearFog = m_gaiaAdditionalLinearFog;
            saveTo.m_gaiaAdditionalExponentialFog = m_gaiaAdditionalExponentialFog;
            saveTo.m_sunRotation = m_sunRotation;
            saveTo.m_sunPitch = m_sunPitch;
            saveTo.m_fogOverride = m_fogOverride;
            saveTo.m_fogMode = m_fogMode;
            saveTo.m_fogColor = m_fogColor;
            saveTo.m_fogDensity = m_fogDensity;
            saveTo.m_fogStart = m_fogStart;
            saveTo.m_fogEnd = m_fogEnd;
            saveTo.m_skyboxOverride = m_skyboxOverride;
            saveTo.m_skyboxRotation = m_skyboxRotation;
            saveTo.m_skyboxExposure = m_skyboxExposure;
            saveTo.m_skyboxTint = m_skyboxTint;
            saveTo.m_ambientIntensity = m_ambientIntensity;
            saveTo.m_sunOverride = m_sunOverride;
            saveTo.m_sunIntensity = m_sunIntensity;
            saveTo.m_sunColor = m_sunColor;
            saveTo.m_sunKelvinValue = m_sunKelvinValue;
            saveTo.m_ambientSkyColor = m_ambientSkyColor;
            saveTo.m_ambientEquatorColor = m_ambientEquatorColor;
            saveTo.m_ambientGroundColor = m_ambientGroundColor;
            //Density Volume
            saveTo.m_overrideDensityVolume = m_overrideDensityVolume;
            saveTo.m_densityVolumeAlbedoColor = m_densityVolumeAlbedoColor;
            saveTo.m_densityVolumeFogDistance = m_densityVolumeFogDistance;
            saveTo.m_densityVolumeEffectType = m_densityVolumeEffectType;
            saveTo.m_densityVolumeTilingResolution = m_densityVolumeTilingResolution;
            //Water Settings
            saveTo.m_gaiaWaterReflectionEnabled = m_gaiaWaterReflectionEnabled;
            saveTo.m_gaiaWaterReflectionDistance = m_gaiaWaterReflectionDistance;
            saveTo.m_gaiaWaterReflectionResolution = m_gaiaWaterReflectionResolution;
            saveTo.m_gaiaReflectionsLODBias = m_gaiaReflectionsLODBias;
            saveTo.m_gaiaUnderwaterFogColor = m_gaiaUnderwaterFogColor;
            saveTo.m_gaiaUnderwaterFogDensity = m_gaiaUnderwaterFogDensity;
            saveTo.m_gaiaUnderwaterFogDistance = m_gaiaUnderwaterFogDistance;
            saveTo.m_gaiaUnderwaterVolume = m_gaiaUnderwaterVolume;
            //Post FX Settings
            saveTo.m_dofFocusModeURP = m_dofFocusModeURP;
            saveTo.m_dofStartBlurURP = m_dofStartBlurURP;
            saveTo.m_dofEndBlurURP = m_dofEndBlurURP;
            saveTo.m_dofHighQualityURP = m_dofHighQualityURP;
            saveTo.m_dofActive = m_dofActive;
            saveTo.m_autoDOFFocus = m_autoDOFFocus;
            saveTo.m_dofFocusDistance = m_dofFocusDistance;
            saveTo.m_dofAperture = m_dofAperture;
            saveTo.m_dofFocalLength = m_dofFocalLength;
            saveTo.m_dofFocusModeHDRP = m_dofFocusModeHDRP;
            saveTo.m_dofQualityHDRP = m_dofQualityHDRP;
            saveTo.m_dofNearBlurStart = m_dofNearBlurStart;
            saveTo.m_dofNearBlurEnd = m_dofNearBlurEnd;
            saveTo.m_dofFarBlurStart = m_dofFarBlurStart;
            saveTo.m_dofFarBlurEnd = m_dofFarBlurEnd;
            saveTo.m_postFXExposure = m_postFXExposure;
            saveTo.m_savedDofFocusMode = m_savedDofFocusMode;
            //Terrain Settings
            saveTo.m_terrainDetailDensity = m_terrainDetailDensity;
            saveTo.m_terrainDetailDistance = m_terrainDetailDistance;
            saveTo.m_terrainPixelError = m_terrainPixelError;
            saveTo.m_terrainBasemapDistance = m_terrainBasemapDistance;
            saveTo.m_drawInstanced = m_drawInstanced;
            //Grass Settings
            saveTo.m_globalGrassDensity = m_globalGrassDensity;
            saveTo.m_globalGrassDistance = m_globalGrassDistance;
            saveTo.m_cameraCellDistance = m_cameraCellDistance;
            saveTo.m_cameraCellSubdivision = m_cameraCellSubdivision;
        }
        /// <summary>
        /// Resets back to default
        /// </summary>
        public void Reset()
        {
            #region Global

            m_isUsingGaiaLighting = false;
            m_selectedGaiaLightingProfile = -1;

            #endregion
            #region Screenshot Settings

            m_screenshotResolution = 0;
            m_screenshotImageFormat = 2;
            m_loadSavedSettings = true;
            m_revertOnDisabled = true;
            m_showFPS = false;
            m_showReticle = false;
            m_showRuleOfThirds = false;

            #endregion
            #region Unity Settings

            m_lodBias = 2f;
            m_vSync = 0;
            m_targetFPS = -1;
            m_antiAliasing = 1;
            m_shadowDistance = 512f;
            m_shadowResolution = 3;
            m_shadowCascades = 4;
            m_fieldOfView = 60f;
            m_gaiaCullinDistance = 0f;
            m_cameraAperture = 19f;
            m_cameraFocalLength = 50f;
            m_globalVolume = 1f;
            m_cameraRoll = 0f;
            m_farClipPlane = 2000f;

            #endregion
            #region Streaming Settings

            m_gaiaLoadRange = 1000;
            m_gaiaImpostorRange = 2000;

            #endregion
            #region Weather Settings

            m_gaiaWeatherEnabled = false;
            m_gaiaWeatherRain = false;
            m_gaiaWeatherSnow = false;
            m_gaiaWindDirection = 0f;
            m_gaiaWindSpeed = 0.35f;
            m_gaiaWindSettingsOverride = false;

            #endregion
            #region Gaia Lighting Settings

            m_gaiaTime = 15f;
            m_gaiaTimeOfDayEnabled = false;
            m_gaiaTimeScale = 1f;
            m_gaiaAdditionalLinearFog = 0f;
            m_gaiaAdditionalExponentialFog = 0f;
            m_sunRotation = 0f;
            m_sunPitch = 75f;
            m_fogOverride = false;
            m_fogMode = FogMode.Exponential;
            m_fogColor = Color.black;
            m_fogDensity = 0.01f;
            m_fogStart = 100f;
            m_fogEnd = 1000f;
            m_skyboxOverride = false;
            m_skyboxRotation = 0f;
            m_skyboxExposure = 1f;
            m_skyboxTint = Color.black;
            m_ambientIntensity = 1f;
            m_sunOverride = false;
            m_sunIntensity = 1f;
            m_sunColor = Color.black;
            m_sunKelvinValue = 6500f;
            m_ambientSkyColor = new Color(0.7027151f, 0.881016f, 1.001631f, 0.4192761f);
            m_ambientEquatorColor = new Color(0.6302439f, 0.7919513f, 0.85f, 0f);
            m_ambientGroundColor = new Color(0.5f, 0.4142857f, 0.3321428f, 0f);
            //Density Volume
            m_overrideDensityVolume = false;
            m_densityVolumeAlbedoColor = Color.white;
            m_densityVolumeFogDistance = 250f;
            m_densityVolumeEffectType = 1;
            m_densityVolumeTilingResolution = 3;

            #endregion
            #region Water Settings

            m_gaiaWaterReflectionEnabled = true;
            m_gaiaWaterReflectionDistance = 0f;
            m_gaiaWaterReflectionResolution = 2;
            m_gaiaReflectionsLODBias = 1f;
            m_gaiaUnderwaterFogColor = Color.black;
            m_gaiaUnderwaterFogDensity = m_gaiaUnderwaterFogDensity = 0.045f;
            m_gaiaUnderwaterFogDistance = m_gaiaUnderwaterFogDistance = 45f;
            m_gaiaUnderwaterVolume = 0.5f;

            #endregion
            #region Post FX Settings

            m_autoDOFFocus = true;
            if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition)
            {
                m_postFXExposure = 13.5f;
            }
            else
            {
                m_postFXExposure = 0f;
            }

            m_postFXExposureMode = 0;
            m_savedDofFocusMode = 0;
            m_dofFocusModeURP = 0;
            m_dofStartBlurURP = 2f;
            m_dofEndBlurURP = 1000f;
            m_dofHighQualityURP = false;
            m_dofActive = true;
            m_autoDOFFocus = true;
            m_dofKernelSize = 1;
            m_dofFocusDistance = 100f;
            m_dofAperture = 16f;
            m_dofFocalLength = 50f;
            m_dofFocusModeHDRP = 0;
            m_dofQualityHDRP = 1;
            m_dofNearBlurStart = 0f;
            m_dofNearBlurEnd = 0.1f;
            m_dofFarBlurStart = 200f;
            m_dofFarBlurEnd = 2000;

            #endregion
            #region Terrain Settings

            m_terrainDetailDensity = 0.5f;
            m_terrainDetailDistance = 150f;
            m_terrainPixelError = 5f;
            m_terrainBasemapDistance = 1024f;
            m_drawInstanced = true;

            #endregion
            #region Grass Settings

            m_globalGrassDensity = 1f;
            m_globalGrassDistance = 1f;
            m_cameraCellDistance = 1f;
            m_cameraCellSubdivision = 0;

            #endregion
        }

        #endregion
    }
    [System.Serializable]
    public class PhotoModeMinAndMaxValues
    {
        #region Variables

        #region Global

        public Vector2 m_0To1 = new Vector2(0f, 1f);

        #endregion
        #region Unity Settings

        public Vector2 m_lodBias = new Vector2(0.001f, 10f);
        public Vector2Int m_targetFPS = new Vector2Int(-1, 240);
        public Vector2 m_shadowDistance = new Vector2(0f, 5000f);
        public Vector2 m_fieldOfView = new Vector2(1f, 115f);
        public Vector2 m_gaiaCullinDistance = new Vector2(-10000f, 10000f);
        public Vector2 m_cameraAperture = new Vector2(0.1f, 32f);
        public Vector2 m_cameraFocalLength = new Vector2(0.05f, 250f);
        public Vector2 m_globalVolume = new Vector2(0f, 1f);
        public Vector2 m_cameraRoll = new Vector2(-90f, 90f);

        #endregion
        #region Streaming Settings

        public Vector2 m_gaiaLoadRange = new Vector2(0f, 5000f);
        public Vector2 m_gaiaImpostorRange = new Vector2(0f, 5000f);

        #endregion
        #region Weather Settings

        public Vector2 m_gaiaWindDirection = new Vector2(0f, 1f);
        public Vector2 m_gaiaWindSpeed = new Vector2(0f, 1f);

        #endregion
        #region Gaia Lighting Settings

        public Vector2 m_gaiaTime = new Vector2(0f, 24f);
        public Vector2 m_gaiaTimeScale = new Vector2(0f, 200f);
        public Vector2 m_gaiaAdditionalLinearFog = new Vector2(-5000f, 5000f);
        public Vector2 m_gaiaAdditionalExponentialFog = new Vector2(0f, 0.05f);
        public Vector2 m_sunRotation = new Vector2(0f, 360f);
        public Vector2 m_sunPitch = new Vector2(0f, 360f);
        public Vector2 m_fogDensity = new Vector2(0f, 0.05f);
        public Vector2 m_fogStart = new Vector2(0f, 5000f);
        public Vector2 m_fogEnd = new Vector2(0f, 5000f);
        public Vector2 m_fogEndHDRP = new Vector2(1f, 5000f);
        public Vector2 m_skyboxRotation = new Vector2(0f, 360f);
        public Vector2 m_skyboxExposure = new Vector2(0f, 4f);
        public Vector2 m_skyboxExposureHDRP = new Vector2(0f, 30f);
        public Vector2 m_ambientIntensity = new Vector2(0f, 10f);
        public Vector2 m_sunIntensity = new Vector2(0f, 8f);
        public Vector2 m_sunIntensityHDRP = new Vector2(0f, 250000f);
        public Vector2 m_sunKelvinValue = new Vector2(1500f, 20000f);
        public Vector2 m_densityVolumeFogDistance = new Vector2(0.01f, float.PositiveInfinity);

        #endregion
        #region Water Settings

        public Vector2 m_gaiaWaterReflectionDistance = new Vector2(-5000f, 5000f);
        public Vector2 m_gaiaReflectionsLODBias = new Vector2(0f, 10f);
        public Vector2 m_gaiaUnderwaterFogDistance = new Vector2(0.1f, 200f);
        public Vector2 m_gaiaUnderwaterFogDensity = new Vector2(0f, 1f);

        #endregion
        #region Post FX Settings

        public Vector2 m_postFXExposure = new Vector2(0f, 5f);
        public Vector2 m_postFXExposureURP = new Vector2(-5f, 5f);
        public Vector2 m_postFXExposureHDRP = new Vector2(-5f, 15f);
        public Vector2 m_postFXDOFFocusDistanceHDRP = new Vector2(0.01f, 300f);
        public Vector2 m_postFXDOFFocusDistanceURP = new Vector2(0.01f, 300f);
        public Vector2 m_postFXDOFFocusDistance = new Vector2(0.01f, 200f);
        public Vector2 m_postFXDOFNearBlurStart = new Vector2(0.01f, 400f);
        public Vector2 m_postFXDOFNearBlurEnd = new Vector2(0.1f, 5000f);        
        public Vector2 m_postFXDOFFarBlurStart = new Vector2(0.01f, 400f);
        public Vector2 m_postFXDOFFarBlurEnd = new Vector2(0.1f, 5000f);
        public Vector2 m_postFXDOFGaussianBlurStartURP = new Vector2(0.01f, 250f);
        public Vector2 m_postFXDOFGaussianBlurEndURP = new Vector2(0.1f, 5000f);
        public Vector2 m_postFXDOFGaussianBlurMaxRadiusURP = new Vector2(0.5f, 1.5f);

        #endregion
        #region Terrain Settings

        public Vector2 m_terrainDetailDensity = new Vector2(0f, 1f);
        public Vector2 m_terrainDetailDistance = new Vector2(0f, 1000f);
        public Vector2 m_terrainPixelError = new Vector2(1f, 200f);
        public Vector2 m_terrainBasemapDistance = new Vector2(0f, 20000f);

        #endregion
        #region Grass Settings

        public Vector2 m_globalGrassDensity = new Vector2(0.01f, 4f);
        public Vector2 m_globalGrassDistance = new Vector2(0.01f, 4f);
        public Vector2 m_cameraCellDistance = new Vector2(0.01f, 4f);
        public Vector2Int m_cameraCellSubdivision = new Vector2Int(-8, 8);

        #endregion

        #endregion

        /// <summary>
        /// Sets the new min/max values that is used in Photo Mode
        /// </summary>
        /// <param name="values"></param>
        public void SetNewMinMaxValues(PhotoModeMinAndMaxValues values)
        {
            #region Unity Settings

            m_lodBias = values.m_lodBias;
            m_targetFPS = values.m_targetFPS;
            m_shadowDistance = values.m_shadowDistance;
            m_fieldOfView = values.m_fieldOfView;
            m_gaiaCullinDistance = values.m_gaiaCullinDistance;
            m_cameraAperture = values.m_cameraAperture;
            m_cameraFocalLength = values.m_cameraFocalLength;
            m_globalVolume = values.m_globalVolume;
            m_cameraRoll = values.m_cameraRoll;

            #endregion
            #region Streaming Settings

            m_gaiaLoadRange = values.m_gaiaLoadRange;
            m_gaiaImpostorRange = values.m_gaiaImpostorRange;

            #endregion
            #region Weather Settings

            m_gaiaWindDirection = values.m_gaiaWindDirection;
            m_gaiaWindSpeed = values.m_gaiaWindSpeed;

            #endregion
            #region Gaia Lighting Settings

            m_gaiaTime = values.m_gaiaTime;
            m_gaiaTimeScale = values.m_gaiaTimeScale;
            m_gaiaAdditionalLinearFog = values.m_gaiaAdditionalLinearFog;
            m_gaiaAdditionalExponentialFog = values.m_gaiaAdditionalExponentialFog;
            m_sunRotation = values.m_sunRotation;
            m_sunPitch = values.m_sunPitch;
            m_fogDensity = values.m_fogDensity;
            m_fogStart = values.m_fogStart;
            m_fogEnd = values.m_fogEnd;
            m_fogEndHDRP = values.m_fogEndHDRP;
            m_skyboxRotation = values.m_skyboxRotation;
            m_skyboxExposure = values.m_skyboxExposure;
            m_skyboxExposureHDRP = values.m_skyboxExposureHDRP;
            m_ambientIntensity = values.m_ambientIntensity;
            m_sunIntensity = values.m_sunIntensity;
            m_sunIntensityHDRP = values.m_sunIntensityHDRP;
            m_sunKelvinValue = values.m_sunKelvinValue;
            m_densityVolumeFogDistance = values.m_densityVolumeFogDistance;

            #endregion
            #region Water Settings

            m_gaiaWaterReflectionDistance = values.m_gaiaWaterReflectionDistance;
            m_gaiaReflectionsLODBias = values.m_gaiaReflectionsLODBias;
            m_gaiaUnderwaterFogDistance = values.m_gaiaUnderwaterFogDistance;
            m_gaiaUnderwaterFogDensity = values.m_gaiaUnderwaterFogDensity;

            #endregion
            #region Post FX Settings

            m_postFXExposure = values.m_postFXExposure;
            m_postFXExposureURP = values.m_postFXExposureURP;
            m_postFXExposureHDRP = values.m_postFXExposureHDRP;
            m_postFXDOFFocusDistanceHDRP = values.m_postFXDOFFocusDistanceHDRP;
            m_postFXDOFFocusDistanceURP = values.m_postFXDOFFocusDistanceURP;
            m_postFXDOFFocusDistance = values.m_postFXDOFFocusDistance;
            m_postFXDOFNearBlurStart = values.m_postFXDOFNearBlurStart;
            m_postFXDOFNearBlurEnd = values.m_postFXDOFNearBlurEnd;
            m_postFXDOFFarBlurStart = values.m_postFXDOFFarBlurStart;
            m_postFXDOFFarBlurEnd = values.m_postFXDOFFarBlurEnd;
            m_postFXDOFGaussianBlurStartURP = values.m_postFXDOFGaussianBlurStartURP;
            m_postFXDOFGaussianBlurEndURP = values.m_postFXDOFGaussianBlurEndURP;
            m_postFXDOFGaussianBlurMaxRadiusURP = values.m_postFXDOFGaussianBlurMaxRadiusURP;

            #endregion
            #region Terrain Settings

            m_terrainDetailDensity = values.m_terrainDetailDensity;
            m_terrainDetailDistance = values.m_terrainDetailDistance;
            m_terrainPixelError = values.m_terrainPixelError;
            m_terrainBasemapDistance = values.m_terrainBasemapDistance;
            m_gaiaUnderwaterFogDensity = values.m_gaiaUnderwaterFogDensity;

            #endregion
            #region Grass Settings

            m_globalGrassDensity = values.m_globalGrassDensity;
            m_globalGrassDistance = values.m_globalGrassDistance;
            m_cameraCellDistance = values.m_cameraCellDistance;
            m_cameraCellSubdivision = values.m_cameraCellSubdivision;

            #endregion
        }
    }
    [System.Serializable]
    public class PhotoModeImages
    {
        public string m_name;
        public Sprite m_image;
        public Vector2 m_imageWidthAndHeight = new Vector2(350f, 125f);
    }
    [System.Serializable]
    public class PhotoModePanel
    {
        public string m_shownTitle;
        public Button m_button;
    }
    [System.Serializable]
    public class PhotoModePanelTransformSettings
    {
        public RectTransform m_photoMode;
        public RectTransform m_camera;
        public RectTransform m_unity;
        public RectTransform m_terrain;
        public RectTransform m_lighting;
        public RectTransform m_water;
        public RectTransform m_postFX;
    }

    public class PhotoModeProfile : ScriptableObject
    {
        [HideInInspector]
        public bool m_everBeenSaved = false;
        public GaiaConstants.EnvironmentRenderer LastRenderPipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;
        public PhotoModeValues Profile
        {
            get { return m_profile; }
            set
            {
                if (m_profile != value)
                {
                    m_profile = value;
                }
            }
        }
        [SerializeField] private PhotoModeValues m_profile = new PhotoModeValues();

        /// <summary>
        /// Function that is used to reset this profile back to factory default (Release State)
        /// </summary>
        public void Reset()
        {
            if (Profile != null)
            {
                Profile.Reset();
                m_everBeenSaved = false;
            }
        }
    }
}