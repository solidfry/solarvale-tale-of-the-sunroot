#ifndef FLAT_KIT_STYLIZED_INPUT_INCLUDED
#define FLAT_KIT_STYLIZED_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.

#ifndef FLATKIT_TERRAIN
#if _FORWARD_PLUS
CBUFFER_START(UnityPerMaterialNoBatching)
#else
CBUFFER_START(UnityPerMaterial)
#endif
#endif

// See `SimpleLitInput.hlsl` and `LitInput.hlsl` for reference

float4 _BaseMap_ST;
float4 _DetailMap_ST;

#ifndef FLATKIT_TERRAIN
half4 _BaseColor;
half _Cutoff;
half _Surface;
#endif

half4 _EmissionColor;
half4 _UnityShadowColor;

// --- _CELPRIMARYMODE_SINGLE
half4 _ColorDim;
// --- _CELPRIMARYMODE_SINGLE

// --- DR_SPECULAR_ON
half4 _FlatSpecularColor;
float _FlatSpecularSize;
float _FlatSpecularEdgeSmoothness;
// --- DR_SPECULAR_ON

// --- DR_RIM_ON
half4 _FlatRimColor;
float _FlatRimSize;
float _FlatRimEdgeSmoothness;
float _FlatRimLightAlign;
// --- DR_RIM_ON

// --- _CELPRIMARYMODE_STEPS
half4 _ColorDimSteps;
sampler2D _CelStepTexture;
// --- _CELPRIMARYMODE_STEPS

// --- _CELPRIMARYMODE_CURVE
half4 _ColorDimCurve;
sampler2D _CelCurveTexture;
// --- _CELPRIMARYMODE_CURVE

// --- DR_CEL_EXTRA_ON
half4 _ColorDimExtra;
half _SelfShadingSizeExtra;
half _ShadowEdgeSizeExtra;
half _FlatnessExtra;
// --- DR_CEL_EXTRA_ON

// --- DR_GRADIENT_ON
half4 _ColorGradient;
half _GradientCenterX;
half _GradientCenterY;
half _GradientSize;
half _GradientAngle;
// --- DR_GRADIENT_ON

half _TextureImpact;

half _SelfShadingSize;
half _ShadowEdgeSize;
half _LightContribution;
half _LightFalloffSize;
half _Flatness;

half _UnityShadowPower;
half _UnityShadowSharpness;

half _OverrideLightmapDir;
half3 _LightmapDirection;

half _DetailMapImpact;
half4 _DetailMapColor;

half4 _OutlineColor;
half _OutlineWidth;
half _OutlineScale;
half _OutlineDepthOffset;
half _CameraDistanceImpact;

#ifndef FLATKIT_TERRAIN
CBUFFER_END
#endif

#ifdef UNITY_DOTS_INSTANCING_ENABLED
    UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
        UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
        UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
        UNITY_DOTS_INSTANCED_PROP(float , _Surface)
        UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
        UNITY_DOT_INSTANCED_PROP(float4 , _UnityShadowColor)
        UNITY_DOTS_INSTANCED_PROP(float4, _ColorDim)
        UNITY_DOTS_INSTANCED_PROP(float4, _FlatSpecularColor)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatSpecularSize)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatSpecularEdgeSmoothness)
        UNITY_DOTS_INSTANCED_PROP(float4, _FlatRimColor)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatRimSize)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatRimEdgeSmoothness)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatRimLightAlign)
        UNITY_DOTS_INSTANCED_PROP(float4, _ColorDimSteps)
        UNITY_DOTS_INSTANCED_PROP(float4, _ColorDimCurve)
        UNITY_DOTS_INSTANCED_PROP(float4, _ColorDimExtra)
        UNITY_DOTS_INSTANCED_PROP(float , _SelfShadingSizeExtra)
        UNITY_DOTS_INSTANCED_PROP(float , _ShadowEdgeSizeExtra)
        UNITY_DOTS_INSTANCED_PROP(float , _FlatnessExtra)
        UNITY_DOTS_INSTANCED_PROP(float4, _ColorGradient)
        UNITY_DOTS_INSTANCED_PROP(float , _GradientCenterX)
        UNITY_DOTS_INSTANCED_PROP(float , _GradientCenterY)
        UNITY_DOTS_INSTANCED_PROP(float , _GradientSize)
        UNITY_DOTS_INSTANCED_PROP(float , _GradientAngle)
        UNITY_DOTS_INSTANCED_PROP(float , _TextureImpact)
        UNITY_DOTS_INSTANCED_PROP(float , _SelfShadingSize)
        UNITY_DOTS_INSTANCED_PROP(float , _ShadowEdgeSize)
        UNITY_DOTS_INSTANCED_PROP(float , _LightContribution)
        UNITY_DOTS_INSTANCED_PROP(float , _LightFalloffSize)
        UNITY_DOTS_INSTANCED_PROP(float , _Flatness)
        UNITY_DOTS_INSTANCED_PROP(float , _UnityShadowPower)
        UNITY_DOTS_INSTANCED_PROP(float , _UnityShadowSharpness)
        UNITY_DOTS_INSTANCED_PROP(float , _OverrideLightmapDir)
        UNITY_DOTS_INSTANCED_PROP(float3, _LightmapDirection)
        UNITY_DOTS_INSTANCED_PROP(float , _DetailMapImpact)
        UNITY_DOTS_INSTANCED_PROP(float4, _DetailMapColor)
        UNITY_DOTS_INSTANCED_PROP(float4, _OutlineColor)
        UNITY_DOTS_INSTANCED_PROP(float , _OutlineWidth)
        UNITY_DOTS_INSTANCED_PROP(float , _OutlineScale)
        UNITY_DOTS_INSTANCED_PROP(float , _OutlineDepthOffset)
        UNITY_DOTS_INSTANCED_PROP(float , _CameraDistanceImpact)
    UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

    #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
    #define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Cutoff)
    #define _Surface            UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Surface)
    #define _EmissionColor      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _EmissionColor)
    #define _UnityShadowColor   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _UnityShadowColor)
    #define _ColorDim           UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _ColorDim)
    #define _FlatSpecularColor  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _FlatSpecularColor)
    #define _FlatSpecularSize   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatSpecularSize)
    #define _FlatSpecularEdgeSmoothness UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatSpecularEdgeSmoothness)
    #define _FlatRimColor       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _FlatRimColor)
    #define _FlatRimSize        UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatRimSize)
    #define _FlatRimEdgeSmoothness UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatRimEdgeSmoothness)
    #define _FlatRimLightAlign  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatRimLightAlign)
    #define _ColorDimSteps      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _ColorDimSteps)
    #define _ColorDimCurve      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _ColorDimCurve)
    #define _ColorDimExtra      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _ColorDimExtra)
    #define _SelfShadingSizeExtra UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _SelfShadingSizeExtra)
    #define _ShadowEdgeSizeExtra UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ShadowEdgeSizeExtra)
    #define _FlatnessExtra      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _FlatnessExtra)
    #define _ColorGradient      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _ColorGradient)
    #define _GradientCenterX    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _GradientCenterX)
    #define _GradientCenterY    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _GradientCenterY)
    #define _GradientSize       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _GradientSize)
    #define _GradientAngle      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _GradientAngle)
    #define _TextureImpact      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _TextureImpact)
    #define _SelfShadingSize    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _SelfShadingSize)
    #define _ShadowEdgeSize     UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ShadowEdgeSize)
    #define _LightContribution  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _LightContribution)
    #define _LightFalloffSize   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _LightFalloffSize)
    #define _Flatness           UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Flatness)
    #define _UnityShadowPower   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _UnityShadowPower)
    #define _UnityShadowSharpness UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _UnityShadowSharpness)
    #define _OverrideLightmapDir UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OverrideLightmapDir)
    #define _LightmapDirection  UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3 , _LightmapDirection)
    #define _DetailMapImpact    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _DetailMapImpact)
    #define _DetailMapColor     UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _DetailMapColor)
    #define _OutlineColor       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _OutlineColor)
    #define _OutlineWidth       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OutlineWidth)
    #define _OutlineScale       UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OutlineScale)
    #define _OutlineDepthOffset UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OutlineDepthOffset)
    #define _CameraDistanceImpact UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _CameraDistanceImpact)
#endif

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedo = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedo.a * _BaseColor.a;
    AlphaDiscard(outSurfaceData.alpha, _Cutoff);

    outSurfaceData.albedo = albedo.rgb;
    #ifdef _ALPHAPREMULTIPLY_ON
    outSurfaceData.albedo *= outSurfaceData.alpha;
    #endif

    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = 0.0; // unused
    outSurfaceData.smoothness = 0.0; // unused
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
    return specularSmoothness;
}

#endif  // FLAT_KIT_STYLIZED_INPUT_INCLUDED
