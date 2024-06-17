#ifndef LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED
#define LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED

#include "Noise/ClassicNoise3D.hlsl"

float Hash(float x) {
    float p = 1;
    #if defined(_TYPE_A)
        p = 43.5453;
    #endif
    return frac(sin(x) * p) * 10;
}

void PillarColor_float(float2 UV, float3 ObjectPositionWS, float3 ObjectScale, float3 PositionWS, float3 NormalWS,
                       float3 LightDirection, float LightAttenuation, out float3 Color) {
    float noise = 0;
    const float hash = Hash(ObjectPositionWS.x + ObjectPositionWS.y + ObjectPositionWS.z);
    const float3 p = (PositionWS + hash) / ObjectScale * _ScaleFactor;

    #if defined(_TYPE_A)
        noise += ClassicNoise(p * _NoiseScale1 * 1.0) * 1.0;
        noise += ClassicNoise(p * _NoiseScale2 * 2.0) * 0.5;
    #elif defined(_TYPE_B)
        noise = ClassicNoise(p.y * 2 + atan(p.x / p.z) * 50 * _NoiseScale1);
    #elif defined(_TYPE_C)
        noise = ClassicNoise(p.y * _NoiseScale1.x +
                             ClassicNoise((UV.x * _NoiseScale1.y *
                                          sin(p.y * _NoiseScale2.z)) * _NoiseScale2.x) *
                             _NoiseScale2.y);
    #endif

    const float section23 = step(_Distribution.x, noise);
    const float section3 = step(_Distribution.y, noise);
    Color = lerp(_Color1, lerp(_Color2, _Color3, section3), section23).rgb;

    // Apply _ColorTop to the faces pointing up.
    const float3 up = float3(0, 1, 0);
    const float isTop = step(1 - _TopSize, dot(up, NormalWS));
    Color = lerp(Color, _ColorTop, isTop);

    const float shadowStrength = _ShadowStrength;
    const float shadowSize = _ShadowSize;
    const float shadowSharpness = _ShadowSharpness;
    const float3 shadowDirection = normalize(-LightDirection + NormalWS * (1 - shadowSize));
    float shadow = saturate(dot(shadowDirection, NormalWS));
    const float shadowBand = (1.0 - shadowSharpness) * 0.5;
    shadow = smoothstep(shadowSize - shadowBand, shadowSize + shadowBand, shadow);
    shadow = lerp(1, shadow, shadowStrength);
    shadow = min(shadow, LightAttenuation);
    shadow = 1 - shadow;

    // Give shadow tint of _ShadowTint and blend it with the color.
    Color = lerp(Color, _ShadowTint.rgb, shadow * _ShadowTint.a);
}

#endif // LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED
