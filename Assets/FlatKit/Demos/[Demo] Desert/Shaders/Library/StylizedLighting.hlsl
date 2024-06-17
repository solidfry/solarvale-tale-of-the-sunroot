#ifndef DR_LIGHTING_INCLUDED
#define DR_LIGHTING_INCLUDED

// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void MainLight_half(float3 WorldPosition, out half3 Direction, out half3 Color, out half DistanceAttenuation,
                    out half ShadowAttenuation) {
    #if defined(SHADERGRAPH_PREVIEW)
    Direction = half3(0.5, 0.5, 0);
    Color = 1;
    DistanceAttenuation = 1;
    ShadowAttenuation = 1;
    #else

    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    half4 clipPos = TransformWorldToHClip(WorldPosition);
    half4 shadowCoord =  ComputeScreenPos(clipPos);
    #else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPosition);
    #endif

    const Light light = GetMainLight(shadowCoord);
    Direction = light.direction;
    Color = light.color;
    DistanceAttenuation = light.distanceAttenuation;
    ShadowAttenuation = light.shadowAttenuation;

    #endif
}

void AdditionalLights_half(float3 WorldPosition, out half3 Direction, out half3 Color, out half Attenuation) {
    #if defined(SHADERGRAPH_PREVIEW)
    Direction = half3(0.5, 0.5, 0);
    Color = 1;
    Attenuation = 1;
    #else

    Direction = 0;
    Color = 0;
    Attenuation = 0;

    #ifdef _ADDITIONAL_LIGHTS
    const half4 shadowMask = half4(1, 1, 1, 1);
    const uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
        Light light = GetAdditionalLight(lightI, WorldPosition, shadowMask);
        Direction += light.direction;
        Color += light.color;
        Attenuation += light.distanceAttenuation * light.shadowAttenuation;
    }
    #endif

    #endif
}

void NDotL_half(half3 Normal, half3 LightDirection, out half Shading) {
    const half nDotL = saturate(dot(Normal, LightDirection) * 0.5 + 0.5);
    Shading = nDotL;
}

#endif
