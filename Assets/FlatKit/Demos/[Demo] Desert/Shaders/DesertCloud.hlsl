#ifndef LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED
#define LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED

#include "Noise/ClassicNoise2D.hlsl"

float Hash(float x) {
    return frac(sin(x)) * 1000;
}

void CloudAlpha_float(float2 UV, float3 ObjectPositionWS, float3 ObjectScale, float3 PositionWS, out float Alpha) {
    const float hash = Hash(ObjectPositionWS.x + ObjectPositionWS.z);
    float noise = 0;
    const float2 p = (PositionWS.xz + hash) / ObjectScale.xz * _ScaleFactor;

    noise += ClassicNoise(p * _NoiseScale1 * 1.0) * 1.0;
    noise += ClassicNoise(p * _NoiseScale2 * 2.0) * 0.5;

    // Fade out close to UV edges.
    const float2 edgeDistances = saturate(abs(UV - 0.5) * 2.0 - 0.5);
    noise *= saturate(1.0 - length(edgeDistances) / _FadeOutDistance);

    Alpha = noise;
}

#endif // LINE_KIT_DEMOS_DESERT_PILLAR_INCLUDED
