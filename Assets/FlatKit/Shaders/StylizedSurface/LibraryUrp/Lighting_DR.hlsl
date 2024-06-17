#ifndef FLATKIT_LIGHTING_DR_INCLUDED
#define FLATKIT_LIGHTING_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline half NdotLTransition(half3 normal, half3 lightDir, half selfShadingSize, half shadowEdgeSize, half flatness) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - selfShadingSize);
    const half angleDiffTransition = smoothstep(0, shadowEdgeSize, angleDiff); 
    return lerp(angleDiff, angleDiffTransition, flatness);
}

/*
inline half NdotLTransition(half3 normal, half3 lightDir, half selfShadingSize, half edgeSize, half flatness) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - (selfShadingSize - .5));
    const half angleDiffTransition = smoothstep(.5 - edgeSize * 0.5, .5 + edgeSize * 0.5, angleDiff);
    return lerp(angleDiff, angleDiffTransition, flatness);
}
*/

inline half NdotLTransitionPrimary(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSize, _ShadowEdgeSize, _Flatness);
}

#if defined(DR_CEL_EXTRA_ON)
inline half NdotLTransitionExtra(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSizeExtra, _ShadowEdgeSizeExtra, _FlatnessExtra);
}
#endif

inline half NdotLTransitionTexture(half3 normal, half3 lightDir, sampler2D stepTex) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - _SelfShadingSize * 0.0);
    const half4 rampColor = tex2D(stepTex, half2(angleDiff, 0.5));
    // NOTE: The color channel here corresponds to the texture format in the shader editor script.
    const half angleDiffTransition = rampColor.r;
    return angleDiffTransition;
}

inline void ApplyLightToColor(Light light, inout half3 c) {
    #if defined(_UNITYSHADOWMODE_MULTIPLY)
    c *= lerp(1, light.shadowAttenuation, _UnityShadowPower);
    #endif
    #if defined(_UNITYSHADOWMODE_COLOR)
    c = lerp(lerp(c, _UnityShadowColor, _UnityShadowColor.a), c, light.shadowAttenuation);
    #endif

    c.rgb *= light.color * light.distanceAttenuation;
}

half3 LightingPhysicallyBased_DSTRM(Light light, InputData inputData)
{
    // If all light in the scene is baked, we use custom light direction for the cel shading.
#if defined(LIGHTMAP_ON)
    light.direction = _LightmapDirection;
#else
    light.direction = lerp(light.direction, _LightmapDirection, _OverrideLightmapDir);
#endif

    half4 c = _BaseColor;

#if defined(_CELPRIMARYMODE_SINGLE)
    const half NdotLTPrimary = NdotLTransitionPrimary(inputData.normalWS, light.direction);
    c = lerp(_ColorDim, c, NdotLTPrimary);
#endif  // _CELPRIMARYMODE_SINGLE

#if defined(_CELPRIMARYMODE_STEPS)
    const half NdotLTSteps = NdotLTransitionTexture(inputData.normalWS, light.direction, _CelStepTexture);
    c = lerp(_ColorDimSteps, c, NdotLTSteps);
#endif  // _CELPRIMARYMODE_STEPS

#if defined(_CELPRIMARYMODE_CURVE)
    const half NdotLTCurve = NdotLTransitionTexture(inputData.normalWS, light.direction, _CelCurveTexture);
    c = lerp(_ColorDimCurve, c, NdotLTCurve);
#endif  // _CELPRIMARYMODE_CURVE

#if defined(DR_CEL_EXTRA_ON)
    const half NdotLTExtra = NdotLTransitionExtra(inputData.normalWS, light.direction);
    c = lerp(_ColorDimExtra, c, NdotLTExtra);
#endif  // DR_CEL_EXTRA_ON

#if defined(DR_GRADIENT_ON)
    const float angleRadians = _GradientAngle / 180.0 * PI;
#if defined(_GRADIENTSPACE_WORLD)
    const float2 position = inputData.positionWS.xy;
#else
    const float2 position = TransformWorldToObject(inputData.positionWS).xy;
#endif
    const float posGradRotated = (position.x - _GradientCenterX) * sin(angleRadians) + 
                                 (position.y - _GradientCenterY) * cos(angleRadians);
    const half gradientFactor = saturate((_GradientSize * 0.5 - posGradRotated) / _GradientSize);
    c = lerp(c, _ColorGradient, gradientFactor);
#endif  // DR_GRADIENT_ON

    const half NdotL = dot(inputData.normalWS, light.direction);

#if defined(DR_RIM_ON)
    const float rim = 1.0 - dot(inputData.viewDirectionWS, inputData.normalWS);
    const float rimSpread = 1.0 - _FlatRimSize - NdotL * _FlatRimLightAlign;
    const float rimEdgeSmooth = _FlatRimEdgeSmoothness;
    const float rimTransition = smoothstep(rimSpread - rimEdgeSmooth * 0.5, rimSpread + rimEdgeSmooth * 0.5, rim);
    c.rgb = lerp(c.rgb, _FlatRimColor.rgb, rimTransition * _FlatRimColor.a);
#endif  // DR_RIM_ON

#if defined(DR_SPECULAR_ON)
    // Halfway between lighting direction and view vector.
    const float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
    const float NdotH = dot(inputData.normalWS, halfVector) * 0.5 + 0.5;
    const float specular = saturate(pow(abs(NdotH), 100.0 * (1.0 - _FlatSpecularSize) * (1.0 - _FlatSpecularSize)));
    const float specularTransition = smoothstep(0.5 - _FlatSpecularEdgeSmoothness * 0.1,
                                                0.5 + _FlatSpecularEdgeSmoothness * 0.1, specular);
    c = lerp(c, _FlatSpecularColor, specularTransition);
#endif  // DR_SPECULAR_ON

#if defined(_UNITYSHADOW_OCCLUSION)
    const float occludedAttenuation = smoothstep(0.25, 0.0, -min(NdotL, 0));
    light.shadowAttenuation *= occludedAttenuation;
    light.distanceAttenuation *= occludedAttenuation;
#endif

    ApplyLightToColor(light, c.rgb);
    return c.rgb;
}

void StylizeLight(inout Light light)
{
    const half shadowAttenuation = saturate(light.shadowAttenuation * _UnityShadowSharpness);
    light.shadowAttenuation = shadowAttenuation;

    const float distanceAttenuation = smoothstep(0, _LightFalloffSize + 0.001, light.distanceAttenuation);
    light.distanceAttenuation = distanceAttenuation;

    #if LIGHTMAP_ON
    const half3 lightColor = 0;
    #else
    const half3 lightColor = lerp(half3(1, 1, 1), light.color, _LightContribution);
    #endif
    light.color = lightColor;
}

half4 UniversalFragment_DSTRM(InputData inputData, SurfaceData surfaceData, float2 uv)
{
    const half4 shadowMask = CalculateShadowMask(inputData);

    #if VERSION_GREATER_EQUAL(10, 0)
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    #else
    Light mainLight = GetMainLight(inputData.shadowCoord);
    #endif

	#if UNITY_VERSION >= 202220
    uint meshRenderingLayers = GetMeshRenderingLayer();
    #elif VERSION_GREATER_EQUAL(12, 0)
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    #endif

#if LIGHTMAP_ON
    mainLight.distanceAttenuation = 1.0;
#endif
    StylizeLight(mainLight);

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        inputData.bakedGI *= aoFactor.indirectAmbientOcclusion;
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, shadowMask);

    // Apply Flat Kit stylizing to `inputData.bakedGI` (which is half3).
#if LIGHTMAP_ON
    // Apply cel shading. Can also separate modes by `#if defined(_CELPRIMARYMODE_SINGLE)` etc.
    // length(inputData.bakedGI) can be replaced with inputData.bakedGI to use light map color more directly.
    inputData.bakedGI = lerp(_ColorDim, _BaseColor,
        smoothstep(_SelfShadingSize - _ShadowEdgeSize, _SelfShadingSize + _ShadowEdgeSize, length(inputData.bakedGI)));

    // Apply shadow modes
    #if defined(_UNITYSHADOWMODE_MULTIPLY)
        inputData.bakedGI = lerp(1, inputData.bakedGI, (1 - inputData.bakedGI) * _UnityShadowPower);
    #endif
    #if defined(_UNITYSHADOWMODE_COLOR)
        inputData.bakedGI = lerp(inputData.bakedGI, _UnityShadowColor.rgb, _UnityShadowColor.a * inputData.bakedGI);
    #endif
#endif

    const half4 albedo = half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);

    const float2 detailUV = TRANSFORM_TEX(uv, _DetailMap);
    const half4 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV);

    #if defined(_BASEMAP_PREMULTIPLY)
        const half3 brdf = albedo.rgb;
    #else
        const half3 brdf = _BaseColor.rgb;
    #endif
    
    BRDFData brdfData;
    InitializeBRDFData(brdf, 1.0 - 1.0 / kDielectricSpec.a, 0, 0, surfaceData.alpha, brdfData);
    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, 1.0, inputData.normalWS, inputData.viewDirectionWS);
    #if VERSION_GREATER_EQUAL(12, 0)
	#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
	#endif
    #endif
    color += LightingPhysicallyBased_DSTRM(mainLight, inputData);

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);//, aoFactor);
        StylizeLight(light);

        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
        {
            color += LightingPhysicallyBased_DSTRM(light, inputData);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);//, aoFactor);
        StylizeLight(light);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
    {
        color += LightingPhysicallyBased_DSTRM(light, inputData);
    }
    LIGHT_LOOP_END
    #endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    color += inputData.vertexLighting * brdfData.diffuse;
#endif

    // Base map.
    {
        #if defined(_TEXTUREBLENDINGMODE_ADD)
        color += lerp(half3(0.0f, 0.0f, 0.0f), albedo.rgb, _TextureImpact);
        #else  // _TEXTUREBLENDINGMODE_MULTIPLY
        color *= lerp(half3(1.0f, 1.0f, 1.0f), albedo.rgb, _TextureImpact);
        #endif
    }

    // Detail map.
    {
        #if defined(_DETAILMAPBLENDINGMODE_ADD)
        color += lerp(0, _DetailMapColor.rgb, detail.rgb * _DetailMapImpact).rgb;
        #endif
        #if defined(_DETAILMAPBLENDINGMODE_MULTIPLY)
        // color *= lerp(1, _DetailMapColor.rgb, detail.rgb * _DetailMapImpact).rgb;
        color *= lerp(1, detail.rgb * _DetailMapColor.rgb, _DetailMapImpact).rgb;
        #endif
        #if defined(_DETAILMAPBLENDINGMODE_INTERPOLATE)
        color = lerp(color, detail.rgb, _DetailMapImpact * _DetailMapColor.rgb * detail.a).rgb;
        #endif
    }

    color += surfaceData.emission;

#ifdef _DBUFFER
    // Modified `DBuffer.hlsl` function `ApplyDecalToBaseColor` to use light attenuation.
    FETCH_DBUFFER(DBuffer, _DBufferTexture, int2(inputData.positionCS.xy));
    DecalSurfaceData decalSurfaceData;
    DECODE_FROM_DBUFFER(DBuffer, decalSurfaceData);
    half3 decalColor = decalSurfaceData.baseColor.xyz;
    ApplyLightToColor(mainLight, decalColor);
    color.xyz = color.xyz * decalSurfaceData.baseColor.w + decalColor;
#endif

    return half4(color, surfaceData.alpha);
}

#endif // FLATKIT_LIGHTING_DR_INCLUDED