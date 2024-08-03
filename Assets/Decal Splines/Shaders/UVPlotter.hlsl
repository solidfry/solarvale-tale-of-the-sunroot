#ifndef UVPLOTTER_HLSL
#define UVPLOTTER_HLSL


void ROADMARKSAMPLER_float(Texture2D mainTex, SamplerState ss, float2 uv, float2 screenPos, int round, out float4 color)
{
    color = mainTex.Sample(ss, uv);
    if (round > 0)//Render only a circle if round is true
    {
        float radius = 0.5f * 0.5f;
        float2 uvCentered = float2(uv.x - 0.5f, uv.y - 0.5f);
        float dist = uvCentered.x * uvCentered.x + uvCentered.y * uvCentered.y;
        if (dist >= radius)
        {
            color.a = 0;
        }
    }
}
#endif