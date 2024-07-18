Shader "Hidden/Buto/VolumeNoiseShader"
{
    Properties
    {
        [IntRange] _Frequency("Frequency", Range(1,32)) = 2
        [IntRange] _Octaves("Octaves", Range(1,8)) = 3
        [IntRange] _Lacunarity("Lacunarity", Range(1, 8)) = 2
        _Gain("Gain", Range(0,1)) = 0.3
        _Height("Height", Range(0,1)) = 0.0
        _Seed("Seed", int) = 0
        [KeywordEnum(Perlin, Worley, PerlinWorley, Billow, Curl)] _Type("Noise Type", Float) = 0
        [Toggle(_INVERT_ON)] _Invert("Invert", Float) = 0
        [Toggle(_GRAYSCALE_ON)] _Grayscale("Grayscale", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
         
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _TYPE_PERLIN _TYPE_WORLEY _TYPE_PERLINWORLEY _TYPE_BILLOW _TYPE_CURL
            #pragma multi_compile _ _INVERT_ON
            #pragma multi_compile _ _GRAYSCALE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            #include "NoiseFunctions.hlsl"

            
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Frequency;
                int _Octaves;
                float _Lacunarity;
                float _Gain;
                float _Height;
                int _Seed;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = mul(unity_ObjectToWorld, IN.vertex).xyz;
                OUT.positionHCS = TransformObjectToHClip(IN.vertex.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float3 value = float3(IN.uv, _Height) * _Frequency;
                
                float4 col = 0;

                #if _TYPE_WORLEY
                float4 worleyNoise = float4
                (
                    SampleLayeredWorleyNoise3dTiling(value, _Octaves, _Lacunarity, _Gain, _Frequency, _Seed),
                    SampleLayeredWorleyNoise3dTiling(value * 2, _Octaves, _Lacunarity, _Gain, _Frequency * 2, _Seed + 1),
                    SampleLayeredWorleyNoise3dTiling(value * 4, _Octaves, _Lacunarity, _Gain, _Frequency * 4, _Seed + 2),
                    SampleLayeredWorleyNoise3dTiling(value * 8, _Octaves, _Lacunarity, _Gain, _Frequency * 8, _Seed + 3)
                );
                col = 1.0 - (0.5 + worleyNoise);
                #endif

                #if _TYPE_PERLIN
                float4 perlinNoise = float4
                (
                    SampleLayeredGradientNoise3dTiling(value, _Octaves, _Lacunarity, _Gain, _Frequency, _Seed),
                    SampleLayeredGradientNoise3dTiling(value * 2, _Octaves, _Lacunarity, _Gain, _Frequency * 2, _Seed + 1),
                    SampleLayeredGradientNoise3dTiling(value * 4, _Octaves, _Lacunarity, _Gain, _Frequency * 4, _Seed + 2),
                    SampleLayeredGradientNoise3dTiling(value * 8, _Octaves, _Lacunarity, _Gain, _Frequency * 8, _Seed + 3)
                );
                col = perlinNoise + 0.5;
                #endif

                #if _TYPE_PERLINWORLEY
                float v[4];
                int m = 1;
                for(int i = 0; i<4; i++)
                {
                    float p = SampleLayeredGradientNoise3dTiling(value * m, _Octaves, _Lacunarity, _Gain, _Frequency * m, _Seed + i);
                    float w = SampleLayeredWorleyNoise3dTiling(value * m, _Octaves, _Lacunarity, _Gain, _Frequency * m, _Seed + i);
                    w = 1.0 - (0.5 + w);
                    v[i] = map(0, 1, abs(p), 1, w);

                    m *= 2;
                }
                col = float4(v[0], v[1], v[2], v[3]);
                #endif

                
                #if _TYPE_BILLOW
                float v[4];
                int m = 1;
                for(int i = 0; i<4; i++)
                {
                    float p = SampleLayeredGradientNoise3dTiling(value * m, _Octaves, _Lacunarity, _Gain, _Frequency * m, _Seed + i);
                    p = abs(p);
                    float w = SampleLayeredWorleyNoise3dTiling((value * m) + p, _Octaves, _Lacunarity, _Gain, _Frequency * m, _Seed + i);
                    w = 1.0 - (0.5 + w);
                    v[i] = map(0, 1, p * 2.0, 1, w);

                    m *= 2;
                }
                col = float4(v[0], v[1], v[2], v[3]);
                #endif
                
                #if _TYPE_CURL
                float3 d = SampleLayered3dCurlNoiseTiling(value, _Octaves, _Lacunarity, _Gain, _Frequency, _Seed) + 0.5;
                col = float4(d.xyz,0);
                #endif

                #if _INVERT_ON
                col = 1.0 - col;
                #endif

                
                #if _GRAYSCALE_ON
                //half4 inf = half4(0.53, 0.27, 0.13, 0.07);
                
                #if _TYPE_CURL
                //inf = half4(0.57, 0.29, 0.14, 0.0);
                #endif
                //col.r = col.r * inf.x + col.g * inf.y + col.b * inf.z + col.a * inf.w;
                //col.r = saturate(col.r);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
