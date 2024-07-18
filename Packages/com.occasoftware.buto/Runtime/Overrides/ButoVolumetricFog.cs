using System;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Buto.Runtime
{
    [Serializable, VolumeComponentMenuForRenderPipeline("OccaSoftware/Buto", typeof(UniversalRenderPipeline))]
    public sealed class ButoVolumetricFog : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Set to On to enable Buto.")]
        public VolumetricFogModeParameter mode = new VolumetricFogModeParameter(VolumetricFogMode.Off);

        public QualityLevelParameter qualityLevel = new QualityLevelParameter(QualityLevel.High);

        public NoInterpMinIntParameter gridPixelSize = new NoInterpMinIntParameter(12, 3);
        public NoInterpClampedIntParameter gridSizeZ = new NoInterpClampedIntParameter(160, 32, 240);

        private Vector3Int volumeSize = Vector3Int.zero;

        public static class ReferenceDimensions
        {
            public static Vector2Int ScreenSize = new Vector2Int(1920, 1080);
        }

        public Vector3Int VolumeSize
        {
            get
            {
                volumeSize.x = ReferenceDimensions.ScreenSize.x / gridPixelSize.value;
                volumeSize.y = ReferenceDimensions.ScreenSize.y / gridPixelSize.value;
                volumeSize.z = gridSizeZ.value;
                return volumeSize;
            }
        }

        public Vector3Int GetVolumeSize(Vector2Int screenDimensions)
        {
            volumeSize.x = screenDimensions.x / gridPixelSize.value;
            volumeSize.y = screenDimensions.y / gridPixelSize.value;
            volumeSize.z = gridSizeZ.value;
            return volumeSize;
        }

        public Vector3 GetCellSize(Vector3 volumeSize)
        {
            return new Vector3(1.0f / volumeSize.x, 1.0f / volumeSize.y, 1.0f / volumeSize.z);
        }

        public float EstimatedVram
        {
            get
            {
                // Buto uses 5 volume textures of 16bit * 4 channel = 64 bits = 8 bytes per pixel textures.
                // So, we calculate the amount of bytes required ( x * 8 ), then convert to MB, then multiply by 5.
                // Doesn't account for RT or for internal media textures (noise).
                float x = VolumeSize.x * VolumeSize.y * VolumeSize.z;
                float y = x * 8 * 0.000001f;
                return y * 5;
            }
        }

        // Performance and baseline rendering

        [Tooltip("Sets the range for volumetric fog.")]
        public MinFloatParameter maxDistanceVolumetric = new MinFloatParameter(64, 10);

        public MinFloatParameter depthRatio = new MinFloatParameter(2.0f, 0.5f);

        public ClampedFloatParameter temporalAALighting = new ClampedFloatParameter(0.05f, 0f, 1f);
        public ClampedFloatParameter temporalAAMedia = new ClampedFloatParameter(0.05f, 0f, 1f);

        // Fog Parameters
        [Tooltip("Density of fog in the scene.")]
        public MinFloatParameter fogDensity = new MinFloatParameter(5, 0);

        [Tooltip("Sets the directionality of scattered light. <0 indicates backscattering.")]
        public ClampedFloatParameter anisotropy = new ClampedFloatParameter(0.2f, -1, 1);

        [Tooltip("Change light intensity. [Default: 1]")]
        public MinFloatParameter lightIntensity = new MinFloatParameter(1, 0);

        [Tooltip("Change fog density in lit areas. [Default: 1]")]
        public MinFloatParameter densityInLight = new MinFloatParameter(1, 0);

        [Tooltip("Change fog density in shadow areas. [Default: 1]")]
        public MinFloatParameter densityInShadow = new MinFloatParameter(1, 0);

        [Tooltip("When enabled, Buto will use the max light distance to fade out additional lights.")]
        public BoolParameter overrideDefaultMaxLightDistance = new BoolParameter(false);

        [Tooltip("Set the maximum distance that additional lights will be rendered at.")]
        public MinFloatParameter maxLightDistance = new MinFloatParameter(64f, 0f);

        // Geometry
        [Tooltip("Sets the base of the fog volume. Fog falloff starts after this height.")]
        public FloatParameter baseHeight = new FloatParameter(0);

        [Tooltip("Controls how the fog attenuates over height.")]
        public MinFloatParameter attenuationBoundarySize = new MinFloatParameter(10, 1);

        // Custom Colors
        [Tooltip("Used to override the fog color in lit regions.")]
        public ColorParameter litColor = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Used to override the fog color in shadowed regions.")]
        public ColorParameter shadowedColor = new ColorParameter(Color.black, true, false, false);

        [Tooltip("Used to add emissivity to the fog.")]
        public ColorParameter emitColor = new ColorParameter(Color.black, true, false, false);

        [Tooltip("Used to override the fog color over distance. See readme for usage details.")]
        public Texture2DParameter colorRamp = new Texture2DParameter(null);

        public FloatParameter colorRampId = new FloatParameter(0);

        [Tooltip("Sets the strength of the override values.")]
        public ClampedFloatParameter colorInfluence = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Tints the fog when looking towards the main light.")]
        public ColorParameter directionalForward = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Tints the fog when looking away from the main light.")]
        public ColorParameter directionalBack = new ColorParameter(Color.white, true, false, false);

        [Tooltip("Set the falloff between the forward and back directional lighting terms.")]
        public FloatParameter directionalRatio = new FloatParameter(1f);

        // Noise
        [Obsolete("Replaced by the volume noise parameter. Use that instead.")]
        [Tooltip(
            "A 3D Texture that will be used to define the fog intensity. Repeats over the noise tiling domain. A value of 0 means the fog density is attenuated to 0. A value of 1 means the fog density is not attenuated and matches what is set in the Fog Density parameter."
        )]
        public Texture3DParameter noiseTexture = new Texture3DParameter(null);

        [Tooltip("Increases level of detail. Computationally expensive.")]
        public ClampedIntParameter octaves = new ClampedIntParameter(1, 1, 3);

        [Tooltip("Controls frequency for each level of detail.")]
        public ClampedFloatParameter lacunarity = new ClampedFloatParameter(2, 1, 8);

        [Tooltip("Controls intensity of each level of detail.")]
        public ClampedFloatParameter gain = new ClampedFloatParameter(0.3f, 0, 1);

        [Tooltip("Scale of noise texture in meters.")]
        public MinFloatParameter noiseTiling = new MinFloatParameter(30, 0);

        [Tooltip("Controls the wind speed in meters per second.")]
        public Vector3Parameter noiseWindSpeed = new Vector3Parameter(new Vector3(0, -1, 0));

        [Tooltip("Remap the noise to a smaller range.")]
        public FloatRangeParameter noiseMap = new FloatRangeParameter(new Vector2(0, 1), 0, 1);

        public VolumeNoiseParameter volumeNoise = new VolumeNoiseParameter(new VolumeNoise(), false);

        public bool IsActive()
        {
            if (mode.value != VolumetricFogMode.On)
                return false;

            return true;
        }

        public bool IsTileCompatible() => false;
    }

    public enum VolumetricFogMode
    {
        Off,
        On
    }

    [Serializable]
    public sealed class VolumetricFogModeParameter : VolumeParameter<VolumetricFogMode>
    {
        public VolumetricFogModeParameter(VolumetricFogMode value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Cinematic,
        Custom
    }

    [Serializable]
    public sealed class QualityLevelParameter : VolumeParameter<QualityLevel>
    {
        public QualityLevelParameter(QualityLevel value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}
