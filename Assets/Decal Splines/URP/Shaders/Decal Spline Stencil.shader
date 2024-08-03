Shader "Shader Graphs/Decal Spline Stencil"
{
    Properties
    {
        [NoScaleOffset]Base_Map("Base Map", 2D) = "white" {}
        [Normal][NoScaleOffset]Normal_Map("Normal Map", 2D) = "bump" {}
        Normal_Blend("Normal Blend", Range(0, 1)) = 0.5
        [ToggleUI]_Round("Round", Float) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset]_Emission_Map("Emission Map", 2D) = "black" {}
        _Color_Filter("Color Filter", Color) = (1, 1, 1, 1)
        _Color_Filter_Emission("Color Filter Emission", Color) = (1, 1, 1, 1)
        _Texutre_Scroll_Direction("Texutre Scroll Direction", Vector) = (0, 0, 0, 0)
        [HideInInspector]_DrawOrder("Draw Order", Range(-50, 50)) = 0
        [HideInInspector][Enum(Depth Bias, 0, View Bias, 1)]_DecalMeshBiasType("DecalMesh BiasType", Float) = 0
        [HideInInspector]_DecalMeshDepthBias("DecalMesh DepthBias", Float) = 0
        [HideInInspector]_DecalMeshViewBias("DecalMesh ViewBias", Float) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            // RenderType: <None>
            "PreviewType"="Plane"
            // Queue: <None>
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalDecalSubTarget"
        }
		Stencil{
                Ref 128
                Comp Greater
			ReadMask 128
			WriteMask 128
                Pass Replace
        }
        Pass
        { 
            Name "DBufferProjector"
            Tags 
            { 
                "LightMode" = "DBufferProjector"
            }
        
            // Render State
            Cull Front
        Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        ColorMask RGBA
        ColorMask RGBA 1
        ColorMask RGBA 2
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _FOVEATED_RENDERING_NON_UNIFORM_RASTER
        #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DBUFFER_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalProjectorForwardEmissive"
            Tags 
            { 
                "LightMode" = "DecalProjectorForwardEmissive"
            }
        
            // Render State
            Cull Front
        Blend 0 SrcAlpha One
        ZTest Greater
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalScreenSpaceProjector"
            Tags 
            { 
                "LightMode" = "DecalScreenSpaceProjector"
            }
        
            // Render State
            Cull Front
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _FORWARD_PLUS
        #pragma multi_compile_fragment _ _FOVEATED_RENDERING_NON_UNIFORM_RASTER
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float3 normalWS : INTERP5;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);

            if(surface.Alpha == 0)
            {
               discard;
            }
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalGBufferProjector"
            Tags 
            { 
                "LightMode" = "DecalGBufferProjector"
            }
        
            // Render State
            Cull Front
        Blend 0 SrcAlpha OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha
        Blend 3 SrcAlpha OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        ColorMask RGB
        ColorMask 0 1
        ColorMask RGB 2
        ColorMask RGB 3
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile _ _DECAL_LAYERS
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_GBUFFER_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
             float4 texCoord0 : INTERP3;
             float3 normalWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.texCoord0.xyzw = input.texCoord0;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.texCoord0 = input.texCoord0.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DBufferMesh"
            Tags 
            { 
                "LightMode" = "DBufferMesh"
            }
        
            // Render State
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        ColorMask RGBA
        ColorMask RGBA 1
        ColorMask RGBA 2
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DBUFFER_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                    input.tangentWS.xyzw = half4(tangentWS, sign);
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalMeshForwardEmissive"
            Tags 
            { 
                "LightMode" = "DecalMeshForwardEmissive"
            }
        
            // Render State
            Blend 0 SrcAlpha One
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                    input.tangentWS.xyzw = half4(tangentWS, sign);
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalScreenSpaceMesh"
            Tags 
            { 
                "LightMode" = "DecalScreenSpaceMesh"
            }
        
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _FORWARD_PLUS
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile _ _DECAL_LAYERS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_SCREEN_SPACE_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
             float4 tangentWS : INTERP3;
             float4 texCoord0 : INTERP4;
             float4 fogFactorAndVertexLight : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                    input.tangentWS.xyzw = half4(tangentWS, sign);
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalGBufferMesh"
            Tags 
            { 
                "LightMode" = "DecalGBufferMesh"
            }
        
            // Render State
            Blend 0 SrcAlpha OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha
        Blend 3 SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGB
        ColorMask 0 1
        ColorMask RGB 2
        ColorMask RGB 3
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma editor_sync_compilation
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile _ _DECAL_LAYERS
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_GBUFFER_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        #define _MATERIAL_AFFECTS_EMISSION 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
             float4 tangentWS : INTERP3;
             float4 texCoord0 : INTERP4;
             float4 fogFactorAndVertexLight : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float
        {
        half4 uv0;
        float3 TimeParameters;
        };
        
        void SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(float _Scale, float2 _Direction, Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float IN, out float2 Out_Vector4_1)
        {
        float _Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float = _Scale;
        float2 _Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2 = _Direction;
        float2 _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_5bdf704e98294332b51b1e63f634b4af_Out_0_Vector2, (IN.TimeParameters.x.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2);
        float2 _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_bbefa72f3aaa497191aea9c00b4dfd07_Out_0_Float.xx), _Multiply_2b2149fb8f9e4b26aa3a96e9ac665b63_Out_2_Vector2, _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2);
        Out_Vector4_1 = _TilingAndOffset_81938b1fd3454f80ac3ef7b4a737f252_Out_3_Vector2;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
            float3 Emission;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4 = _Color_Filter;
            UnityTexture2D _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Base_Map);
            float2 _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2 = _Texutre_Scroll_Direction;
            Bindings_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float _TextureScroll_fa8bdef438c640408007cf8c836112aa;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.uv0 = IN.uv0;
            _TextureScroll_fa8bdef438c640408007cf8c836112aa.TimeParameters = IN.TimeParameters;
            float2 _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2;
            SG_TextureScroll_2fe89f64624406847917ae5bf8c2765e_float(1, _Property_b83f1a6676294dd0a878c4720fad0ba1_Out_0_Vector2, _TextureScroll_fa8bdef438c640408007cf8c836112aa, _TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2);
            float4 _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.tex, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.samplerstate, _Property_8ea9445ada8c40f7957a5c132b7db265_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_R_4_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.r;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_G_5_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.g;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_B_6_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.b;
            float _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_A_7_Float = _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4.a;
            float4 _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_0d33f35b9e1f4a82a3d439d8097bccc2_Out_0_Vector4, _SampleTexture2D_ab1e96e3ed254f2cab367b01b958355a_RGBA_0_Vector4, _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4);
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_R_1_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[0];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_G_2_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[1];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_B_3_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[2];
            float _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float = _Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4[3];
            UnityTexture2D _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(Normal_Map);
            float4 _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.tex, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.samplerstate, _Property_71391700a4c941a09c14bbfcd4bf5d89_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4);
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_R_4_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.r;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_G_5_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.g;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_B_6_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.b;
            float _SampleTexture2D_1311d583442147e1873e802b7427d8b9_A_7_Float = _SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.a;
            float _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float = Normal_Blend;
            float _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            Unity_Multiply_float_float(_Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float, _Property_a1d59f6d803e45f383d59c432d48dfbb_Out_0_Float, _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float);
            float _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float = _Metallic;
            float _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float = _Smoothness;
            float4 Color_914ddd69c7434487bacf08de23fc6428 = IsGammaSpace() ? float4(1, 1, 1, 1) : float4(SRGBToLinear(float3(1, 1, 1)), 1);
            UnityTexture2D _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.tex, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.samplerstate, _Property_3e3f1132588a4a898cb788d9f23ae075_Out_0_Texture2D.GetTransformedUV(_TextureScroll_fa8bdef438c640408007cf8c836112aa_OutVector4_1_Vector2) );
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_R_4_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.r;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_G_5_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.g;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_B_6_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.b;
            float _SampleTexture2D_286ea1c81ce7429f851a55599245d283_A_7_Float = _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4.a;
            float4 _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4;
            Unity_Multiply_float4_float4(Color_914ddd69c7434487bacf08de23fc6428, _SampleTexture2D_286ea1c81ce7429f851a55599245d283_RGBA_0_Vector4, _Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4);
            surface.BaseColor = (_Multiply_60fa0b0f228a47e0a8852169040d00a6_Out_2_Vector4.xyz);
            surface.Alpha = _Split_e741b3d3b17e4ca5901ac81ed9da5af3_A_4_Float;
            surface.NormalTS = (_SampleTexture2D_1311d583442147e1873e802b7427d8b9_RGBA_0_Vector4.xyz);
            surface.NormalAlpha = _Multiply_2bb402057cfd45029ea791f49aba2a2a_Out_2_Float;
            surface.Metallic = _Property_136b1946f3f24320b91274b47b1c5060_Out_0_Float;
            surface.Occlusion = 1;
            surface.Smoothness = _Property_e95069d521aa4c95ba715dd9f47769a7_Out_0_Float;
            surface.MAOSAlpha = 1;
            surface.Emission = (_Multiply_f3dd3e8ceedd416c86bdac368e6c9395_Out_2_Vector4.xyz);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                    input.normalWS.xyz = normalWS;
                    input.tangentWS.xyzw = half4(tangentWS, sign);
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
                surfaceData.emissive.rgb = half3(surfaceDescription.Emission.rgb * fadeFactor);
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "ScenePickingPass"
            Tags 
            { 
                "LightMode" = "Picking"
            }
        
            // Render State
            Cull Back
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma editor_sync_compilation
        #pragma vertex Vert
        #pragma fragment Frag
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
            #if _RENDER_PASS_ENABLED
            #define GBUFFER3 0
            #define GBUFFER4 1
            FRAMEBUFFER_INPUT_HALF(GBUFFER3);
            FRAMEBUFFER_INPUT_HALF(GBUFFER4);
            #endif
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Base_Map_TexelSize;
        float4 Normal_Map_TexelSize;
        float Normal_Blend;
        float _Round;
        float _Smoothness;
        float _Metallic;
        float4 _Emission_Map_TexelSize;
        float4 _Color_Filter;
        float4 _Color_Filter_Emission;
        float2 _Texutre_Scroll_Direction;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(Base_Map);
        SAMPLER(samplerBase_Map);
        TEXTURE2D(Normal_Map);
        SAMPLER(samplerNormal_Map);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // Graph Functions
            // GraphFunctions: <None>
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            void GetSurfaceData(Varyings input, float4 positionCS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    half3 normalWS = TransformObjectToWorldDir(half3(0, 1, 0));
                    half3 tangentWS = TransformObjectToWorldDir(half3(1, 0, 0));
                    half3 bitangentWS = TransformObjectToWorldDir(half3(0, 0, 1));
                    half sign = dot(cross(normalWS, tangentWS), bitangentWS) > 0 ? 1 : -1;
                #else
                    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
                        LODFadeCrossFade(positionCS);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.Rendering.Universal.DecalShaderGraphGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}