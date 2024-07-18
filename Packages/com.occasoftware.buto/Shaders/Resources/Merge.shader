Shader "OccaSoftware/Buto/Merge"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Merge"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Common.hlsl"
            #pragma vertex Vert
            #pragma fragment Fragment

            Texture2D _ScreenTexture;
            Texture2D _ButoTexture;
            
            float3 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 fogColor = _ButoTexture.SampleLevel(buto_point_clamp_sampler, input.texcoord, 0);
                float3 screenColor = _ScreenTexture.SampleLevel(buto_point_clamp_sampler, input.texcoord, 0).rgb;
                float3 output = (screenColor * fogColor.a) + fogColor.rgb;
                return output;
            }
            ENDHLSL
        }
    }
}