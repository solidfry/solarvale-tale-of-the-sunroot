using UnityEditor;
using UnityEditor.Rendering;

using UnityEngine;

using OccaSoftware.Buto.Runtime;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace OccaSoftware.Buto.Editor
{
    [CustomEditor(typeof(ButoVolumetricFog))]
    public class VolumetricFogEditor : VolumeComponentEditor
    {
        SerializedDataParameter mode;

        SerializedDataParameter gridPixelSize;
        SerializedDataParameter gridSizeZ;
        SerializedDataParameter qualityLevel;

        SerializedDataParameter depthRatio;
        SerializedDataParameter maxDistanceVolumetric;
        SerializedDataParameter overrideDefaultMaxLightDistance;
        SerializedDataParameter maxLightDistance;

        SerializedDataParameter temporalAntiAliasingEnabled;
        SerializedDataParameter temporalAntiAliasingIntegrationRate;
        SerializedDataParameter temporalAALighting;
        SerializedDataParameter temporalAAMedia;

        SerializedDataParameter fogDensity;
        SerializedDataParameter anisotropy;

        SerializedDataParameter lightIntensity;
        SerializedDataParameter densityInLight;
        SerializedDataParameter densityInShadow;

        SerializedDataParameter baseHeight;
        SerializedDataParameter attenuationBoundarySize;

        SerializedDataParameter colorRamp;
        SerializedDataParameter colorRampId;
        SerializedDataParameter litColor;
        SerializedDataParameter shadowedColor;
        SerializedDataParameter emitColor;

        SerializedDataParameter colorInfluence;

        SerializedDataParameter directionalForward;
        SerializedDataParameter directionalBack;
        SerializedDataParameter directionalRatio;

        SerializedDataParameter octaves;
        SerializedDataParameter lacunarity;
        SerializedDataParameter gain;
        SerializedDataParameter noiseTiling;
        SerializedDataParameter noiseWindSpeed;
        SerializedDataParameter noiseMap;

        // Generated Noise
        SerializedDataParameter volumeNoise;
        SerializedProperty p_frequency;
        SerializedProperty p_octaves;
        SerializedProperty p_lacunarity;
        SerializedProperty p_gain;
        SerializedProperty p_seed;
        SerializedProperty p_noiseQuality;
        SerializedProperty p_noiseType;
        SerializedProperty p_userTexture;
        SerializedProperty p_invert;

        public override void OnEnable()
        {
            PropertyFetcher<ButoVolumetricFog> o = new PropertyFetcher<ButoVolumetricFog>(serializedObject);
            mode = Unpack(o.Find(x => x.mode));
            qualityLevel = Unpack(o.Find(x => x.qualityLevel));

            gridPixelSize = Unpack(o.Find(x => x.gridPixelSize));
            gridSizeZ = Unpack(o.Find(x => x.gridSizeZ));
            depthRatio = Unpack(o.Find(x => x.depthRatio));

            maxDistanceVolumetric = Unpack(o.Find(x => x.maxDistanceVolumetric));

            temporalAALighting = Unpack(o.Find(x => x.temporalAALighting));
            temporalAAMedia = Unpack(o.Find(x => x.temporalAAMedia));

            fogDensity = Unpack(o.Find(x => x.fogDensity));
            anisotropy = Unpack(o.Find(x => x.anisotropy));
            lightIntensity = Unpack(o.Find(x => x.lightIntensity));
            densityInLight = Unpack(o.Find(x => x.densityInLight));
            densityInShadow = Unpack(o.Find(x => x.densityInShadow));

            baseHeight = Unpack(o.Find(x => x.baseHeight));
            attenuationBoundarySize = Unpack(o.Find(x => x.attenuationBoundarySize));

            colorRamp = Unpack(o.Find(x => x.colorRamp));
            colorRampId = Unpack(o.Find(x => x.colorRampId));
            litColor = Unpack(o.Find(x => x.litColor));
            shadowedColor = Unpack(o.Find(x => x.shadowedColor));
            emitColor = Unpack(o.Find(x => x.emitColor));
            colorInfluence = Unpack(o.Find(x => x.colorInfluence));

            directionalForward = Unpack(o.Find(x => x.directionalForward));
            directionalBack = Unpack(o.Find(x => x.directionalBack));
            directionalRatio = Unpack(o.Find(x => x.directionalRatio));

            octaves = Unpack(o.Find(x => x.octaves));
            lacunarity = Unpack(o.Find(x => x.lacunarity));
            gain = Unpack(o.Find(x => x.gain));
            noiseTiling = Unpack(o.Find(x => x.noiseTiling));
            noiseWindSpeed = Unpack(o.Find(x => x.noiseWindSpeed));
            noiseMap = Unpack(o.Find(x => x.noiseMap));

            overrideDefaultMaxLightDistance = Unpack(o.Find(x => x.overrideDefaultMaxLightDistance));
            maxLightDistance = Unpack(o.Find(x => x.maxLightDistance));

            volumeNoise = Unpack(o.Find(x => x.volumeNoise));
            p_frequency = o.Find("volumeNoise.m_Value.frequency");
            p_octaves = o.Find("volumeNoise.m_Value.octaves");
            p_lacunarity = o.Find("volumeNoise.m_Value.lacunarity");
            p_gain = o.Find("volumeNoise.m_Value.gain");
            p_seed = o.Find("volumeNoise.m_Value.seed");
            p_noiseType = o.Find("volumeNoise.m_Value.noiseType");
            p_noiseQuality = o.Find("volumeNoise.m_Value.noiseQuality");
            p_userTexture = o.Find("volumeNoise.m_Value.userTexture");
            p_invert = o.Find("volumeNoise.m_Value.invert");
        }

        public override void OnInspectorGUI()
        {
            PropertyField(mode);
            if (mode.value.enumValueIndex == ((int)VolumetricFogMode.On))
            {
                DrawQualityOption();

                EditorGUI.BeginChangeCheck();

                DrawQualitySettings();

                DrawCharacteristics();

                DrawGeometry();
                DrawVolumeNoiseRendering();
                DrawVolumeNoiseSource();
                DrawCustomColorSettings();
                DrawDirectionalLightingSettings();
                DrawIntensityOverrides();
                DrawTAA();
                DrawAdvancedSettings();

                if (EditorGUI.EndChangeCheck())
                {
                    if (!CheckQualitySettings(Quality.GetCurrentSetting(qualityLevel.value.intValue)))
                    {
                        qualityLevel.value.intValue = 4;
                    }
                }

                void DrawQualityOption()
                {
                    EditorGUI.BeginChangeCheck();
                    PropertyField(qualityLevel, new GUIContent("Quality", "Set the overall quality level. Overrides some default properties."));
                    if (EditorGUI.EndChangeCheck())
                    {
                        gridPixelSize.overrideState.boolValue = true;
                        gridSizeZ.overrideState.boolValue = true;
                        SetQualitySettings(Quality.GetCurrentSetting(qualityLevel.value.intValue));
                    }
                }
                void DrawIntensityOverrides()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Intensity Overrides", "Overrides PBR-based intensity."), EditorStyles.boldLabel);

                    PropertyField(lightIntensity);
                    PropertyField(densityInLight);
                    PropertyField(densityInShadow);
                }
                void DrawCharacteristics()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Characteristics", EditorStyles.boldLabel);
                    PropertyField(
                        fogDensity,
                        new GUIContent("Density", "Set the density of the fog. Affects how far you can see before the fog fully occludes the scene.")
                    );
                    PropertyField(
                        anisotropy,
                        new GUIContent(
                            "Scattering Distribution",
                            "Set how the fog responds to light. A value of 1 means that light scatters forwards, a value of -1 means that light scatters backwards."
                        )
                    );
                    PropertyField(
                        maxDistanceVolumetric,
                        new GUIContent("View Distance", "Set the maximum extents that the fog will render relative to the camera.")
                    );
                }
                void DrawGeometry()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
                    PropertyField(
                        baseHeight,
                        new GUIContent("Base", "Set the base of the fog (Y-Axis). Below this point, the fog has a constant density.")
                    );
                    PropertyField(
                        attenuationBoundarySize,
                        new GUIContent("Reach", "Set the reach of the fog (Y-Axis). After the Base + Reach point, the fog is mostly gone.")
                    );
                }
                void DrawCustomColorSettings()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Color Overrides",
                            "You can override the default fog color settings using the properties in this section. The color ramp is combined multiplicatively with the individual color settings. The resulting value is lerped to according to the influence slider."
                        ),
                        EditorStyles.boldLabel
                    );
                    PropertyField(colorInfluence);
                    using (new IndentLevelScope(15))
                    {
                        EditorGUILayout.LabelField("Individual", EditorStyles.boldLabel);
                        PropertyField(litColor, new GUIContent("Lit", "This value will override the default fog color in lit regions of the fog."));
                        PropertyField(
                            shadowedColor,
                            new GUIContent("Shadow", "This value will override the default fog color in shadowed regions of the fog.")
                        );
                        PropertyField(emitColor, new GUIContent("Emission", "This value will override the default emission color (black)."));

                        EditorGUILayout.LabelField("Ramp", EditorStyles.boldLabel);
                        PropertyField(
                            colorRamp,
                            new GUIContent("Texture", "This can be used to set a gradient for the lit, shadow, and emission color overrides.")
                        );
                    }
                }
                void DrawDirectionalLightingSettings()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Directional Lighting",
                            "You can tint the fog relative to the main light direction. This effect is most prominent when the main light is close to the horizon."
                        ),
                        EditorStyles.boldLabel
                    );
                    PropertyField(directionalForward, new GUIContent("Forward", "Set the color of the fog when looking toward the main light."));
                    PropertyField(directionalBack, new GUIContent("Back", "Set the color of the fog when looking away from the main light"));
                    PropertyField(directionalRatio, new GUIContent("Balance", "Controls the balance between the forward and back lighting."));
                }
                void DrawQualitySettings()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(
                        new GUIContent("Quality", "Configure settings that affect the baseline quality of the Volumetric Fog."),
                        EditorStyles.boldLabel
                    );
                    PropertyField(
                        gridPixelSize,
                        new GUIContent("Grid Pixel Size", "Set the number of screen pixels per fog cell. Lower -> Higher Quality")
                    );
                    PropertyField(
                        gridSizeZ,
                        new GUIContent(
                            "Sample Count",
                            "Set the number of samples between the near and far plane of the fog region. Higher -> Higher Quality."
                        )
                    );
                    EditorGUI.indentLevel++;
                    ButoVolumetricFog obj = (ButoVolumetricFog)serializedObject.targetObject;
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            $"Estimated VRAM Usage (MB) @ {ButoVolumetricFog.ReferenceDimensions.ScreenSize.x}p: {obj.EstimatedVram.ToString("0.00")}MB",
                            "This is an approximation of the VRAM usage for the fog rendering volumes. Higher values = Worse performance. It is a good proxy for understanding the quality level of your fog."
                        ),
                        EditorStyles.miniLabel
                    );

                    Vector3Int estimatedVolumeSize = obj.GetVolumeSize(ButoVolumetricFog.ReferenceDimensions.ScreenSize);
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            $"Texture Dimensions @ {ButoVolumetricFog.ReferenceDimensions.ScreenSize.x}p : {estimatedVolumeSize.x}x{estimatedVolumeSize.y}x{estimatedVolumeSize.z}",
                            "The resolution of your fog volume. A good median target to shoot for is ~160x90x128."
                        ),
                        EditorStyles.miniLabel
                    );
                    EditorGUI.indentLevel--;
                }

                void DrawVolumeNoiseSource()
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Volume Noise Source",
                            "Configure the noise texture that will be used for rendering. You can load your own 3D Texture or configure the parameters used to generate a texture."
                        ),
                        EditorStyles.boldLabel
                    );

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(volumeNoise.overrideState, new GUIContent("Override Volume Noise"));
                    EditorGUILayout.PropertyField(p_noiseType, new GUIContent("Type"));
                    switch (p_noiseType.enumValueIndex)
                    {
                        case (int)VolumeNoise.NoiseType.None:
                            break;
                        case (int)VolumeNoise.NoiseType.Texture:
                            DrawTextureControls();
                            break;
                        default:
                            DrawNoiseControls();
                            break;
                    }

                    void DrawTextureControls()
                    {
                        EditorGUILayout.PropertyField(p_userTexture, new GUIContent("Texture"));
                    }

                    void DrawNoiseControls()
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(p_noiseQuality, new GUIContent("Quality"));
                        EditorGUILayout.IntSlider(p_frequency, 1, 32, new GUIContent("Frequency"));
                        EditorGUILayout.IntSlider(p_octaves, 1, 8);
                        using (new IndentLevelScope(15))
                        {
                            EditorGUILayout.IntSlider(p_lacunarity, 1, 10);
                            EditorGUILayout.Slider(p_gain, 0f, 1f);
                        }
                        EditorGUILayout.PropertyField(p_invert);
                        EditorGUILayout.PropertyField(p_seed);
                        EditorGUI.indentLevel--;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        var t = target as ButoVolumetricFog;

                        if (t == null)
                            return;

                        t.volumeNoise.value.Release();
                        t.volumeNoise.value.SetDirty();
                    }

                    EditorGUI.indentLevel--;
                }
                void DrawVolumeNoiseRendering()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Volume Noise Rendering",
                            "Configure noise rendering parameters. Describes how the Noise Texture from the Volume Noise Source section will be sampled."
                        ),
                        EditorStyles.boldLabel
                    );
                    PropertyField(
                        noiseTiling,
                        new GUIContent("Scale", "Set the scale of the noise texture. Bigger means the noise tiles less frequently.")
                    );
                    PropertyField(noiseWindSpeed, new GUIContent("Wind Speed", "Set the wind speed. Causes the fog to move throug the scene."));
                    PropertyField(
                        noiseMap,
                        new GUIContent(
                            "Remapping",
                            "Set the mapping for the fog values. Fog below the min is cut off. Fog above the max is at full density."
                        )
                    );
                    EditorGUILayout.LabelField("Advanced", EditorStyles.label);
                    PropertyField(
                        octaves,
                        new GUIContent(
                            "Octaves",
                            "Allows you to resample the noise texture to add additional levels of detail. High performance cost."
                        )
                    );
                    using (new IndentLevelScope(15))
                    {
                        PropertyField(
                            lacunarity,
                            new GUIContent(
                                "Lacunarity",
                                "Sets the rate of frequency increase during resampling. A value of 2 means that each octave doubles in frequency."
                            )
                        );
                        PropertyField(
                            gain,
                            new GUIContent(
                                "Gain",
                                "Set the rate of amplitude decay during resampling. A value of 0.5 means that octave halves in strength."
                            )
                        );
                    }
                }

                void DrawTAA()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Temporal Anti-Aliasing",
                            "Set and configure Temporal Anti-Aliasing. Temporal Anti-Aliasing reduces noise and increases overall fog quality. Can cause artifacts when the camera or objects in scene are in motion."
                        ),
                        EditorStyles.boldLabel
                    );

                    PropertyField(temporalAALighting);
                    PropertyField(temporalAAMedia);
                }
                void DrawAdvancedSettings()
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
                    PropertyField(
                        depthRatio,
                        new GUIContent(
                            "Depth Balance",
                            "Sets the rate of change for the cell size in the forward direction. Higher values compress the cells closer to the camera. A value of 1 causes each cell to be equally sized along the forward direction. Smaller values compress the cells further from the camera. Increase this for more detail nearby at the cost of less detail far away."
                        )
                    );

                    PropertyField(overrideDefaultMaxLightDistance);
                    using (new IndentLevelScope(15))
                    {
                        PropertyField(maxLightDistance);
                    }
                }

                void SetQualitySettings(Quality.Settings settings)
                {
                    if (settings == null)
                        return;

                    p_noiseQuality.intValue = settings.noiseQuality;
                    gridPixelSize.value.intValue = settings.gridPixelSize;
                    gridSizeZ.value.intValue = settings.gridSizeZ;
                }

                bool CheckQualitySettings(Quality.Settings settings)
                {
                    if (settings == null)
                        return true;

                    if (p_noiseQuality.intValue != settings.noiseQuality)
                        return false;

                    if (gridPixelSize.value.intValue != settings.gridPixelSize)
                        return false;

                    if (gridSizeZ.value.intValue != settings.gridSizeZ)
                        return false;

                    return true;
                }
            }
        }

        private static class Quality
        {
            public class Settings
            {
                public int noiseQuality;
                public int gridPixelSize;
                public int gridSizeZ;

                public Settings(int noiseQuality, int gridPixelSize, int gridSizeZ)
                {
                    this.noiseQuality = noiseQuality;
                    this.gridPixelSize = gridPixelSize;
                    this.gridSizeZ = gridSizeZ;
                }
            }

            public static Settings Low = new Settings(0, 12, 64);
            public static Settings Medium = new Settings(1, 12, 128);
            public static Settings High = new Settings(2, 8, 164);
            public static Settings Ultra = new Settings(3, 4, 196);
            public static Settings None = null;

            public static Settings GetCurrentSetting(int id)
            {
                switch (id)
                {
                    case 0:
                        return Low;
                    case 1:
                        return Medium;
                    case 2:
                        return High;
                    case 3:
                        return Ultra;
                    case 4:
                        return None;
                    default:
                        return High;
                }
            }
        }
    }
}
