#ifndef FLAT_KIT_OUTLINE_INCLUDED
#define FLAT_KIT_OUTLINE_INCLUDED

TEXTURE2D_X (_EffectTexture);
SAMPLER (sampler_EffectTexture);

float Linear01Depth(float z)
{
    const float isOrtho = unity_OrthoParams.w;
    const float isPers = 1.0 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

float SampleDepth(float2 uv)
{
    const float d = SampleSceneDepth(uv);
    return Linear01Depth(d);
}

float4 SampleCameraColor(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_EffectTexture, sampler_EffectTexture, UnityStereoTransformScreenSpaceTex(uv));
}

void Outline_float(float2 UV, out float4 Out)
{
    float4 original = SampleCameraColor(UV);

    const float offset_positive = +ceil(_Thickness * 0.5f);
    const float offset_negative = -floor(_Thickness * 0.5f);

    #if RESOLUTION_INVARIANT_THICKNESS
        const float screen_ratio = _ScreenSize.x / _ScreenSize.y;
        const float2 texel_size = 1.0 / 800.0 * float2(1.0, screen_ratio);
    #else
    const float2 texel_size = _ScreenSize.zw;
    #endif

    float left = texel_size.x * offset_negative;
    float right = texel_size.x * offset_positive;
    float top = texel_size.y * offset_negative;
    float bottom = texel_size.y * offset_positive;

    const float2 uv0 = UV + float2(left, top);
    const float2 uv1 = UV + float2(right, bottom);
    const float2 uv2 = UV + float2(right, top);
    const float2 uv3 = UV + float2(left, bottom);

    #ifdef OUTLINE_USE_DEPTH
        const float d0 = SampleDepth(uv0);
        const float d1 = SampleDepth(uv1);
        const float d2 = SampleDepth(uv2);
        const float d3 = SampleDepth(uv3);

        const float depth_threshold_scale = 300.0f;
        float d = length(float2(d1 - d0, d3 - d2)) * depth_threshold_scale;
        d = smoothstep(_DepthThresholdMin, _DepthThresholdMax, d);
    #else
    float d = 0.0f;
    #endif  // OUTLINE_USE_DEPTH

    #ifdef OUTLINE_USE_NORMALS
        const float3 n0 = SampleSceneNormals(uv0);
        const float3 n1 = SampleSceneNormals(uv1);
        const float3 n2 = SampleSceneNormals(uv2);
        const float3 n3 = SampleSceneNormals(uv3);

        const float3 nd1 = n1 - n0;
        const float3 nd2 = n3 - n2;
        float n = sqrt(dot(nd1, nd1) + dot(nd2, nd2));
        n = smoothstep(_NormalThresholdMin, _NormalThresholdMax, n);
    #else
    float n = 0.0f;
    #endif  // OUTLINE_USE_NORMALS

    #ifdef OUTLINE_USE_COLOR
        const float3 c0 = SampleCameraColor(uv0).rgb;
        const float3 c1 = SampleCameraColor(uv1).rgb;
        const float3 c2 = SampleCameraColor(uv2).rgb;
        const float3 c3 = SampleCameraColor(uv3).rgb;

        const float3 cd1 = c1 - c0;
        const float3 cd2 = c3 - c2;
        float c = sqrt(dot(cd1, cd1) + dot(cd2, cd2));
        c = smoothstep(_ColorThresholdMin, _ColorThresholdMax, c);
    #else
    float c = 0;
    #endif  // OUTLINE_USE_COLOR

    const float g = max(d, max(n, c));

    #ifdef OUTLINE_FADE_OUT
        const float linear_depth = LinearEyeDepth(SampleSceneDepth(UV), _ZBufferParams);
        const float fade = smoothstep(_FadeRangeEnd, _FadeRangeStart, linear_depth);
        _EdgeColor.a *= fade;
    #endif  // OUTLINE_FADE_OUT

    #ifdef OUTLINE_ONLY
        original.rgb = lerp(1.0 - _EdgeColor.rgb, _EdgeColor.rgb, g * _EdgeColor.a);
    #endif  // OUTLINE_ONLY

    float4 output;
    output.rgb = lerp(original.rgb, _EdgeColor.rgb, g * _EdgeColor.a);
    output.a = original.a;

    Out = output;
}
#endif  // FLAT_KIT_OUTLINE_INCLUDED
