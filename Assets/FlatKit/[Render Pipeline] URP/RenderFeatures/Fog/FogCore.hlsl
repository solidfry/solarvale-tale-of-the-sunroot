#ifndef FLAT_KIT_FOG_INCLUDED
#define FLAT_KIT_FOG_INCLUDED

TEXTURE2D_X (_EffectTexture);
SAMPLER (sampler_EffectTexture);

float Linear01Depth(float z)
{
    const float isOrtho = unity_OrthoParams.w;
    const float isPers = 1.0 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

float LinearEyeDepth(float z)
{
    return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
}

float4 SampleCameraColor(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_EffectTexture, sampler_EffectTexture, UnityStereoTransformScreenSpaceTex(uv));
}

void Fog_float(float2 UV, out float4 Out)
{
    float4 original = SampleCameraColor(UV);
    float fogBlend = _DistanceHeightBlend;
    float depthPacked = SampleSceneDepth(UV);

    #if defined(USE_DISTANCE_FOG)
        const float depthCameraPlanes = Linear01Depth(depthPacked);
        const float depthAbsolute = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * depthCameraPlanes;
        const float depthFogPlanes = saturate((depthAbsolute - _Near) / (_Far - _Near));
        float4 distanceFog = SAMPLE_TEXTURE2D_X(_DistanceLUT, sampler_DistanceLUT, float2(depthFogPlanes, 0.5));
        distanceFog.a *= _DistanceFogIntensity;
    #else
        float4 distanceFog = float4(0, 0, 0, 0);
        fogBlend = 1.0;
    #endif

    #if defined(USE_HEIGHT_FOG)
        #if defined(FOG_CAMERA_RELATIVE)
            const float3 worldPos = float3(UV, depthPacked) * LinearEyeDepth(depthPacked) + _WorldSpaceCameraPos;
        #else
            #if !UNITY_REVERSED_Z
                // Adjust z to match NDC for OpenGL
                depthPacked = lerp(UNITY_NEAR_CLIP_VALUE, 1, depthPacked);
            #endif
    
            const float3 worldPos = ComputeWorldSpacePosition(UV, depthPacked, UNITY_MATRIX_I_VP);
        #endif
    
        const float heightUV = saturate((worldPos.y - _LowWorldY) / (_HighWorldY - _LowWorldY));
        float4 heightFog = SAMPLE_TEXTURE2D_X(_HeightLUT, sampler_HeightLUT, float2(heightUV, 0.5));
        heightFog.a *= _HeightFogIntensity;
    #else
        float4 heightFog = float4(0, 0, 0, 0);
        fogBlend = 0.0;
    #endif

    const float4 fog = lerp(distanceFog, heightFog, fogBlend);
    float4 final = lerp(original, fog, fog.a);
    final.a = original.a;

    Out = final;
}
#endif  // FLAT_KIT_FOG_INCLUDED
