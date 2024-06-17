Shader "FlatKit/Demos/Circle Decal"
{
    Properties
    {
        [MainColor][HDR]_Color("_Color (default = 1,1,1,1)", Color) = (1,1,1,1)

        [Space]
        [Enum(UnityEngine.Rendering.BlendMode)]_DecalSrcBlend("Src Blend", Int) = 5 // 5 = SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)]_DecalDstBlend("Dst Blend", Int) = 10 // 10 = OneMinusSrcAlpha

        [Space]
        [Toggle(_ProjectionAngleDiscardEnable)] _ProjectionAngleDiscardEnable("Angle Discard Enable", float) = 0
        _ProjectionAngleDiscardThreshold("     Threshold", range(-1,1)) = 0

        [Space]
        _StencilRef("Stencil Reference", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Compare", Float) = 0 //0 = disable

        [Space]
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Depth Test", Float) = 0 //0 = disable

        [Space]
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 1 //1 = Front
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True"
        }

        Pass
        {
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
            }

            Cull[_Cull]
            ZTest[_ZTest]

            ZWrite off
            Blend[_DecalSrcBlend][_DecalDstBlend]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // make fog work
            #pragma multi_compile_fog

            // due to using ddx() & ddy()
            #pragma target 3.0

            #pragma shader_feature_local_fragment _ProjectionAngleDiscardEnable

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float3 positionOS : POSITION;
            };

            struct v2f {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float4 viewRayOS : TEXCOORD1; // xyz: viewRayOS, w: extra copy of positionVS.z 
                float4 cameraPosOSAndFogFactor : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _ProjectionAngleDiscardThreshold;
            half4 _Color;
            half2 _AlphaRemap;
            half _MulAlphaToRGB;
            CBUFFER_END

            v2f vert(appdata input) {
                v2f o;
                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
                o.positionCS = vertexPositionInput.positionCS;

                #if _UnityFogEnable
                o.cameraPosOSAndFogFactor.a = ComputeFogFactor(o.positionCS.z);
                #else
                o.cameraPosOSAndFogFactor.a = 0;
                #endif

                o.screenPos = ComputeScreenPos(o.positionCS);
                float3 viewRay = vertexPositionInput.positionVS;
                o.viewRayOS.w = viewRay.z;
                viewRay *= -1;
                float4x4 ViewToObjectMatrix = mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V);
                o.viewRayOS.xyz = mul((float3x3)ViewToObjectMatrix, viewRay);
                o.cameraPosOSAndFogFactor.xyz = mul(ViewToObjectMatrix, float4(0, 0, 0, 1)).xyz;

                return o;
            }

            // copied from URP12.1.2's ShaderVariablesFunctions.hlsl
            #if SHADER_LIBRARY_VERSION_MAJOR < 12
            float LinearDepthToEyeDepth(float rawDepth)
            {
            #if UNITY_REVERSED_Z
                    return _ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
            #else
                    return _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * rawDepth;
            #endif
            }
            #endif

            half4 frag(v2f i) : SV_Target {
                i.viewRayOS.xyz /= i.viewRayOS.w;

                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
                float sceneRawDepth = tex2D(_CameraDepthTexture, screenSpaceUV).r;

                float3 decalSpaceScenePos;

                if (unity_OrthoParams.w) {
                    float sceneDepthVS = LinearDepthToEyeDepth(sceneRawDepth);
                    float2 viewRayEndPosVS_xy = float2(
                        unity_OrthoParams.xy * (i.screenPos.xy - 0.5) * 2 /* to clip space */);
                    float4 vposOrtho = float4(viewRayEndPosVS_xy, -sceneDepthVS, 1); // view space pos
                    float3 wposOrtho = mul(UNITY_MATRIX_I_V, vposOrtho).xyz; // view space to world space
                    decalSpaceScenePos = mul(GetWorldToObjectMatrix(), float4(wposOrtho, 1)).xyz;
                } else {
                    float sceneDepthVS = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                    decalSpaceScenePos = i.cameraPosOSAndFogFactor.xyz + i.viewRayOS.xyz * sceneDepthVS;
                }

                float2 decalSpaceUV = decalSpaceScenePos.xy + 0.5;

                float shouldClip = 0;
                #if _ProjectionAngleDiscardEnable
                float3 decalSpaceHardNormal = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos)));
                shouldClip = decalSpaceHardNormal.z > _ProjectionAngleDiscardThreshold ? 0 : 1;
                #endif
                clip(0.5 - abs(decalSpaceScenePos) - shouldClip);
                float4 col = 1;
                col.a = smoothstep(0, 0.01, 1 - length(decalSpaceUV - 0.5) * 2);

                col *= _Color;

                col.rgb = MixFog(col.rgb, i.cameraPosOSAndFogFactor.a);
                return col;
            }
            ENDHLSL
        }
    }
}