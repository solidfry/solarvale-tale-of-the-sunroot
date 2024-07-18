Shader "OccaSoftware/Buto/IsolatedBlit"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Isolated Blit Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Common.hlsl"
            
			Texture3D IntegratorData;
			

            float4 Fragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
				float depthRaw = SampleSceneDepth(input.texcoord);
				float depthEye = GetDepthEye(depthRaw);
                float z = depthEye * GetRayLengthCompute(input.texcoord.xy) * _InverseMaxDistanceVolumetric;
				float3 uvw = float3(input.texcoord.xy, InverseRescaleDepth(z, _PERCENTILE, _RANGE));
				uvw = saturate(uvw);
				
				float4 integratorData = IntegratorData.SampleLevel(buto_linear_clamp_sampler, uvw, 0).rgba; 
				return integratorData;
            }
            ENDHLSL
        }
    }
}