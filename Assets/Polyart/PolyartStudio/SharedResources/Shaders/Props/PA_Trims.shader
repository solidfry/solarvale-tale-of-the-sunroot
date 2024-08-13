// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Trim POM"
{
	Properties
	{
		_TextureTiling("Texture Tiling", Float) = 1
		[Header(BASE PBR)]_ColorMap("Color Map", 2D) = "white" {}
		[Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		_ORMMap("ORM Map", 2D) = "white" {}
		_HeightMap("Height Map", 2D) = "white" {}
		_RoughnessIntensity("Roughness Intensity", Range( 0 , 2)) = 1
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 1
		_MetallicIntensity("Metallic Intensity", Range( 0 , 2)) = 0
		_AOIntensity("AO Intensity", Range( 0 , 1)) = 1
		[Header(PARALLAX OFFSET)][Toggle(_USESPARALLAXOFFSET_ON)] _UsesParallaxOffset("Uses Parallax Offset?", Float) = 0
		_OffsetScale("Offset Scale", Range( 0 , 0.2)) = 0.02
		_ParallaxOcclusionScale("Parallax Occlusion Scale", Range( 0 , 0.1)) = 0.02
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _UsesParallaxOcclusion
		#pragma shader_feature_local _USESPARALLAXOFFSET_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float _TextureTiling;
		uniform sampler2D _HeightMap;
		uniform float _OffsetScale;
		uniform float4 _HeightMap_ST;
		uniform float _ParallaxOcclusionScale;
		uniform float _NormalIntensity;
		uniform sampler2D _ColorMap;
		uniform sampler2D _ORMMap;
		uniform float _MetallicIntensity;
		uniform float _RoughnessIntensity;
		uniform float _AOIntensity;


		inline float2 POM( sampler2D heightMap, float2 uvs, float2 dx, float2 dy, float3 normalWorld, float3 viewWorld, float3 viewDirTan, int minSamples, int maxSamples, float parallax, float refPlane, float2 tilling, float2 curv, int index )
		{
			float3 result = 0;
			int stepIndex = 0;
			int numSteps = ( int )lerp( (float)maxSamples, (float)minSamples, saturate( dot( normalWorld, viewWorld ) ) );
			float layerHeight = 1.0 / numSteps;
			float2 plane = parallax * ( viewDirTan.xy / viewDirTan.z );
			uvs.xy += refPlane * plane;
			float2 deltaTex = -plane * layerHeight;
			float2 prevTexOffset = 0;
			float prevRayZ = 1.0f;
			float prevHeight = 0.0f;
			float2 currTexOffset = deltaTex;
			float currRayZ = 1.0f - layerHeight;
			float currHeight = 0.0f;
			float intersection = 0;
			float2 finalTexOffset = 0;
			while ( stepIndex < numSteps + 1 )
			{
			 	currHeight = tex2Dgrad( heightMap, uvs + currTexOffset, dx, dy ).r;
			 	if ( currHeight > currRayZ )
			 	{
			 	 	stepIndex = numSteps + 1;
			 	}
			 	else
			 	{
			 	 	stepIndex++;
			 	 	prevTexOffset = currTexOffset;
			 	 	prevRayZ = currRayZ;
			 	 	prevHeight = currHeight;
			 	 	currTexOffset += deltaTex;
			 	 	currRayZ -= layerHeight;
			 	}
			}
			int sectionSteps = 5;
			int sectionIndex = 0;
			float newZ = 0;
			float newHeight = 0;
			while ( sectionIndex < sectionSteps )
			{
			 	intersection = ( prevHeight - prevRayZ ) / ( prevHeight - currHeight + currRayZ - prevRayZ );
			 	finalTexOffset = prevTexOffset + intersection * deltaTex;
			 	newZ = prevRayZ - intersection * layerHeight;
			 	newHeight = tex2Dgrad( heightMap, uvs + finalTexOffset, dx, dy ).r;
			 	if ( newHeight > newZ )
			 	{
			 	 	currTexOffset = finalTexOffset;
			 	 	currHeight = newHeight;
			 	 	currRayZ = newZ;
			 	 	deltaTex = intersection * deltaTex;
			 	 	layerHeight = intersection * layerHeight;
			 	}
			 	else
			 	{
			 	 	prevTexOffset = finalTexOffset;
			 	 	prevHeight = newHeight;
			 	 	prevRayZ = newZ;
			 	 	deltaTex = ( 1 - intersection ) * deltaTex;
			 	 	layerHeight = ( 1 - intersection ) * layerHeight;
			 	}
			 	sectionIndex++;
			}
			return uvs.xy + finalTexOffset;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_TextureTiling).xx;
			float2 uv_TexCoord9 = i.uv_texcoord * temp_cast_0;
			float2 uvTiling23 = uv_TexCoord9;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 ase_tanViewDir = mul( ase_worldToTangent, ase_worldViewDir );
			float2 Offset5 = ( ( tex2D( _HeightMap, uvTiling23 ).r - 1 ) * ase_tanViewDir.xy * _OffsetScale ) + uvTiling23;
			float2 uv_HeightMap = i.uv_texcoord * _HeightMap_ST.xy + _HeightMap_ST.zw;
			float2 Offset17 = ( ( tex2D( _HeightMap, uv_HeightMap ).r - 1 ) * ase_tanViewDir.xy * _OffsetScale ) + Offset5;
			float2 parallaxOffset22 = Offset17;
			#ifdef _USESPARALLAXOFFSET_ON
				float2 staticSwitch19 = parallaxOffset22;
			#else
				float2 staticSwitch19 = uvTiling23;
			#endif
			float2 OffsetPOM31 = POM( _HeightMap, uvTiling23, ddx(uvTiling23), ddy(uvTiling23), ase_worldNormal, ase_worldViewDir, ase_tanViewDir, 8, 8, _ParallaxOcclusionScale, 0.5, _HeightMap_ST.xy, float2(0,0), 0 );
			#ifdef _UsesParallaxOcclusion
				float2 staticSwitch20 = OffsetPOM31;
			#else
				float2 staticSwitch20 = staticSwitch19;
			#endif
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, staticSwitch20 ), _NormalIntensity );
			o.Albedo = tex2D( _ColorMap, staticSwitch20 ).rgb;
			float4 tex2DNode9_g1 = tex2D( _ORMMap, staticSwitch20 );
			o.Metallic = ( tex2DNode9_g1.b * _MetallicIntensity );
			o.Smoothness = ( 1.0 - ( tex2DNode9_g1.g * _RoughnessIntensity ) );
			float lerpResult10_g1 = lerp( 1.0 , tex2DNode9_g1.r , _AOIntensity);
			o.Occlusion = lerpResult10_g1;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.RangedFloatNode;15;-5484.554,-438.1164;Inherit;False;Property;_TextureTiling;Texture Tiling;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-5476.416,414.6106;Inherit;True;Property;_HeightMap;Height Map;6;0;Create;True;0;0;0;False;0;False;None;None;True;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;24;-5434.349,134.3543;Inherit;False;23;uvTiling;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-4968.736,405.7588;Inherit;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;16;-4952.961,-26.80875;Inherit;False;Property;_OffsetScale;Offset Scale;12;0;Create;True;0;0;0;False;0;False;0.02;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-5081.469,212.5837;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;27;-4408.07,51.73931;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;-4468.397,404.3174;Inherit;True;Property;_TextureSample4;Texture Sample 4;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ParallaxMappingNode;5;-4700.311,134.2691;Inherit;False;Normal;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;28;-4523.069,433.7389;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ParallaxMappingNode;17;-4097.057,128.7576;Inherit;False;Normal;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-3789.202,126.2436;Inherit;False;parallaxOffset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-4983.14,556.7715;Inherit;False;heightMap;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-2603.785,46.36954;Inherit;False;22;parallaxOffset;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;38;-2596.012,271.0136;Inherit;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;37;-2650.96,194.3938;Inherit;False;Property;_ParallaxOcclusionScale;Parallax Occlusion Scale;13;0;Create;True;0;0;0;False;0;False;0.02;0.02;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;42;-2700.96,-75.12859;Inherit;False;1177.28;534.1421;Comment;3;31;20;25;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-2590.873,118.7699;Inherit;False;35;heightMap;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ParallaxOcclusionMappingNode;31;-2311.706,99.09222;Inherit;False;0;8;False;;16;False;;5;0.02;0.5;False;1,1;False;0,0;8;0;FLOAT2;0,0;False;1;SAMPLER2D;;False;7;SAMPLERSTATE;;False;2;FLOAT;0.02;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT2;0,0;False;6;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;19;-2356.202,-23.81821;Inherit;False;Property;_UsesParallaxOffset;Uses Parallax Offset?;11;0;Create;True;0;0;0;False;1;Header(PARALLAX OFFSET);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1080.733,255.1332;Inherit;True;Property;_ORMMap;ORM Map;3;0;Create;True;0;0;0;False;0;False;None;af33054005c4cbb44bd5cd38df7e2263;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;45;-1128.239,607.6429;Inherit;False;Property;_MetallicIntensity;Metallic Intensity;9;0;Create;True;0;0;0;False;0;False;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-704.7228,146.2579;Inherit;False;Property;_NormalIntensity;Normal Intensity;8;0;Create;True;0;0;0;False;0;False;1;0.606;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1127.239,529.6429;Inherit;False;Property;_RoughnessIntensity;Roughness Intensity;7;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1127.239,452.6429;Inherit;False;Property;_AOIntensity;AO Intensity;10;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;14;-727.6638,237.1197;Inherit;False;PA_SF_ORM_Unpacker;4;;1;d74c7a4dc46d7b848b37bc309ff6ccaf;0;5;1;FLOAT2;1,1;False;2;SAMPLER2D;;False;4;FLOAT;0;False;5;FLOAT;1;False;6;FLOAT;0;False;3;FLOAT;7;FLOAT;0;FLOAT;8
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;64.09807,-69.92515;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Trim POM;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;29;-5526.416,-76.80888;Inherit;False;1961.215;721.4198;Comment;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;41;-5534.554,-512.3904;Inherit;False;682.4253;214.4402;Comment;2;9;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;20;-1846.681,-25.12859;Inherit;False;Property;_UsesParallaxOcclusion;Uses Parallax Occlusion?;13;0;Create;True;0;0;0;False;1;Header(PARALLAX OCCLUSION);False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;25;-2579.785,-24.63041;Inherit;False;23;uvTiling;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-5076.129,-462.3904;Inherit;False;uvTiling;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-5293.877,-456.9502;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-724.6638,-270.8803;Inherit;True;Property;_TextureSample1;Texture Sample 1;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1063.333,-270.8669;Inherit;True;Property;_ColorMap;Color Map;1;0;Create;True;0;0;0;False;1;Header(BASE PBR);False;None;ea56f555ad193504a921aa08feba2ea0;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.UnpackScaleNormalNode;13;-366.6638,-52.88031;Inherit;False;Tangent;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;11;-723.6638,-58.88031;Inherit;True;Property;_TextureSample2;Texture Sample 2;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1068.933,-61.8667;Inherit;True;Property;_NormalMap;Normal Map;2;1;[Normal];Create;True;0;0;0;False;0;False;None;d9b0899829daa8f42800230bd9216925;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
WireConnection;8;0;4;0
WireConnection;8;1;24;0
WireConnection;27;0;16;0
WireConnection;47;0;4;0
WireConnection;5;0;24;0
WireConnection;5;1;8;1
WireConnection;5;2;16;0
WireConnection;5;3;7;0
WireConnection;28;0;7;0
WireConnection;17;0;5;0
WireConnection;17;1;47;1
WireConnection;17;2;27;0
WireConnection;17;3;28;0
WireConnection;22;0;17;0
WireConnection;35;0;4;0
WireConnection;31;0;25;0
WireConnection;31;1;36;0
WireConnection;31;2;37;0
WireConnection;31;3;38;0
WireConnection;19;1;25;0
WireConnection;19;0;26;0
WireConnection;14;1;20;0
WireConnection;14;2;3;0
WireConnection;14;4;43;0
WireConnection;14;5;44;0
WireConnection;14;6;45;0
WireConnection;0;0;10;0
WireConnection;0;1;13;0
WireConnection;0;3;14;7
WireConnection;0;4;14;0
WireConnection;0;5;14;8
WireConnection;20;1;19;0
WireConnection;20;0;31;0
WireConnection;23;0;9;0
WireConnection;9;0;15;0
WireConnection;10;0;1;0
WireConnection;10;1;20;0
WireConnection;13;0;11;0
WireConnection;13;1;48;0
WireConnection;11;0;2;0
WireConnection;11;1;20;0
ASEEND*/
//CHKSM=5EDA44AAA28A60CF4B5B891F3C001D887FAB269E