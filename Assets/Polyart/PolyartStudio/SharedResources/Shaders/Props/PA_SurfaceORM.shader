// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Surface"
{
	Properties
	{
		[Header(Base Maps)]_TextureSize("Texture Size", Float) = 100
		[SingleLineTexture]_ColorMap("Color Map", 2D) = "white" {}
		[SingleLineTexture]_NormalMap("Normal Map", 2D) = "white" {}
		[SingleLineTexture]_ORMMap("ORM Map", 2D) = "white" {}
		[SingleLineTexture]_EmissiveMap("Emissive Map", 2D) = "white" {}
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 1
		_RoughnessIntensity("Roughness Intensity", Range( 0 , 2)) = 1
		_AOIntensity("AO Intensity", Range( 0 , 2)) = 1
		_MetallicIntensity("Metallic Intensity", Range( 0 , 1)) = 0
		_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		_EmissiveIntensity("Emissive Intensity", Float) = 0
		[Header(Coverage Maps)][Toggle(_ENABLECOVERAGE_ON)] _EnableCoverage("Enable Coverage?", Float) = 0
		_CoverageSize("Coverage Size", Float) = 100
		[SingleLineTexture]_CoverageColorMap("Coverage Color Map", 2D) = "white" {}
		[SingleLineTexture]_CoverageNormalMap("Coverage Normal Map", 2D) = "white" {}
		[SingleLineTexture]_CoverageORMMap(" Coverage ORM Map", 2D) = "white" {}
		_CoverageNormalIntensity("Coverage Normal Intensity", Range( 0 , 1)) = 1
		_CoverageRoughnessIntensity("Coverage Roughness Intensity", Range( 0 , 2)) = 1
		_CoverageMetallicIntensity("Coverage Metallic Intensity", Range( 0 , 1)) = 0
		[Header(Slope Maps)]_NoiseTextureSize("Noise Texture Size", Float) = 100
		[SingleLineTexture]_SlopeNoiseTexture("Slope Noise Texture", 2D) = "white" {}
		_SlopeOffset("Slope Offset", Float) = 0
		_SlopeContrast("Slope Contrast", Float) = 1
		[KeywordEnum(VertexBlending,NormalBlending,NoiseBlending)] _EdgeBlending("Edge Blending", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _ENABLECOVERAGE_ON
		#pragma shader_feature_local _EDGEBLENDING_VERTEXBLENDING _EDGEBLENDING_NORMALBLENDING _EDGEBLENDING_NOISEBLENDING
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
		uniform float _TextureSize;
		uniform float _NormalIntensity;
		uniform sampler2D _CoverageNormalMap;
		uniform float _CoverageSize;
		uniform float _CoverageNormalIntensity;
		uniform float _SlopeContrast;
		uniform float _SlopeOffset;
		uniform sampler2D _SlopeNoiseTexture;
		uniform float _NoiseTextureSize;
		uniform float4 _ColorTint;
		uniform sampler2D _ColorMap;
		uniform sampler2D _CoverageColorMap;
		uniform sampler2D _EmissiveMap;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveIntensity;
		uniform sampler2D _ORMMap;
		uniform float _MetallicIntensity;
		uniform sampler2D _CoverageORMMap;
		uniform float _CoverageMetallicIntensity;
		uniform float _RoughnessIntensity;
		uniform float _CoverageRoughnessIntensity;
		uniform float _AOIntensity;


		inline float4 TriplanarSampling35_g165( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_output_19_0_g161 = ( i.uv_texcoord / ( _TextureSize / 100.0 ) );
			float3 temp_output_78_7 = UnpackScaleNormal( tex2D( _NormalMap, temp_output_19_0_g161 ), _NormalIntensity );
			float2 temp_output_19_0_g163 = ( i.uv_texcoord / ( _CoverageSize / 100.0 ) );
			float temp_output_1_0_g170 = _SlopeContrast;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float3 objToWorldDir51_g165 = mul( unity_ObjectToWorld, float4( ase_vertexNormal, 0 ) ).xyz;
			float dotResult18_g165 = dot( objToWorldDir51_g165 , float3(0,2,0) );
			float temp_output_21_0_g165 = ( ( _SlopeOffset + dotResult18_g165 ) * 2.0 );
			float clampResult22_g165 = clamp( temp_output_21_0_g165 , 0.0 , 1.0 );
			float lerpResult7_g170 = lerp( ( 0.0 - temp_output_1_0_g170 ) , ( temp_output_1_0_g170 + 0.0 ) , clampResult22_g165);
			float clampResult8_g170 = clamp( lerpResult7_g170 , 0.0 , 1.0 );
			float temp_output_1_0_g166 = _SlopeContrast;
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float3 tangentToWorldDir6_g165 = mul( ase_tangentToWorldFast, temp_output_78_7 );
			float dotResult7_g165 = dot( tangentToWorldDir6_g165 , float3(0,1,0) );
			float lerpResult11_g165 = lerp( 0.0 , 3.0 , ( dotResult7_g165 + _SlopeOffset ));
			float clampResult12_g165 = clamp( lerpResult11_g165 , 0.0 , 1.0 );
			float lerpResult7_g166 = lerp( ( 0.0 - temp_output_1_0_g166 ) , ( temp_output_1_0_g166 + 0.0 ) , clampResult12_g165);
			float clampResult8_g166 = clamp( lerpResult7_g166 , 0.0 , 1.0 );
			float temp_output_1_0_g168 = _SlopeContrast;
			float2 temp_cast_2 = (_NoiseTextureSize).xx;
			float3 ase_worldPos = i.worldPos;
			float4 triplanar35_g165 = TriplanarSampling35_g165( _SlopeNoiseTexture, ase_worldPos, ase_worldNormal, 1.0, temp_cast_2, 1.0, 0 );
			float clampResult42_g165 = clamp( (triplanar35_g165).y , 0.0 , 1.0 );
			float lerpResult7_g168 = lerp( ( 0.0 - temp_output_1_0_g168 ) , ( temp_output_1_0_g168 + 0.0 ) , ( temp_output_21_0_g165 + clampResult42_g165 ));
			float clampResult8_g168 = clamp( lerpResult7_g168 , 0.0 , 1.0 );
			float clampResult26_g165 = clamp( clampResult8_g168 , 0.0 , 1.0 );
			#if defined(_EDGEBLENDING_VERTEXBLENDING)
				float staticSwitch43_g165 = clampResult8_g170;
			#elif defined(_EDGEBLENDING_NORMALBLENDING)
				float staticSwitch43_g165 = clampResult8_g166;
			#elif defined(_EDGEBLENDING_NOISEBLENDING)
				float staticSwitch43_g165 = clampResult26_g165;
			#else
				float staticSwitch43_g165 = clampResult8_g170;
			#endif
			float temp_output_76_0 = staticSwitch43_g165;
			float3 lerpResult56 = lerp( temp_output_78_7 , UnpackScaleNormal( tex2D( _CoverageNormalMap, temp_output_19_0_g163 ), _CoverageNormalIntensity ) , temp_output_76_0);
			#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch58 = lerpResult56;
			#else
				float3 staticSwitch58 = temp_output_78_7;
			#endif
			o.Normal = staticSwitch58;
			float4 temp_output_78_0 = ( _ColorTint * tex2D( _ColorMap, temp_output_19_0_g161 ) );
			float4 color13_g163 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 lerpResult55 = lerp( temp_output_78_0 , ( color13_g163 * tex2D( _CoverageColorMap, temp_output_19_0_g163 ) ) , temp_output_76_0);
			#ifdef _ENABLECOVERAGE_ON
				float4 staticSwitch57 = lerpResult55;
			#else
				float4 staticSwitch57 = temp_output_78_0;
			#endif
			o.Albedo = staticSwitch57.rgb;
			o.Emission = ( ( tex2D( _EmissiveMap, temp_output_19_0_g161 ) * _EmissiveColor ) * _EmissiveIntensity ).rgb;
			float4 tex2DNode9_g162 = tex2D( _ORMMap, temp_output_19_0_g161 );
			float temp_output_78_3 = ( tex2DNode9_g162.b * _MetallicIntensity );
			float4 tex2DNode9_g164 = tex2D( _CoverageORMMap, temp_output_19_0_g163 );
			float lerpResult63 = lerp( ( tex2DNode9_g164.b * _CoverageMetallicIntensity ) , temp_output_78_3 , temp_output_76_0);
			#ifdef _ENABLECOVERAGE_ON
				float staticSwitch60 = lerpResult63;
			#else
				float staticSwitch60 = temp_output_78_3;
			#endif
			o.Metallic = staticSwitch60;
			float temp_output_78_5 = ( 1.0 - ( tex2DNode9_g162.g * _RoughnessIntensity ) );
			float lerpResult62 = lerp( temp_output_78_5 , ( 1.0 - ( tex2DNode9_g164.g * _CoverageRoughnessIntensity ) ) , temp_output_76_0);
			#ifdef _ENABLECOVERAGE_ON
				float staticSwitch59 = lerpResult62;
			#else
				float staticSwitch59 = temp_output_78_5;
			#endif
			o.Smoothness = staticSwitch59;
			float lerpResult10_g162 = lerp( 1.0 , tex2DNode9_g162.r , _AOIntensity);
			float temp_output_78_8 = lerpResult10_g162;
			float lerpResult10_g164 = lerp( 1.0 , tex2DNode9_g164.r , 0.0);
			float lerpResult64 = lerp( lerpResult10_g164 , temp_output_78_8 , temp_output_76_0);
			#ifdef _ENABLECOVERAGE_ON
				float staticSwitch61 = lerpResult64;
			#else
				float staticSwitch61 = temp_output_78_8;
			#endif
			o.Occlusion = staticSwitch61;
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
Node;AmplifyShaderEditor.CommentaryNode;31;-1929.161,-329.2871;Inherit;False;1072.199;1674.577;Comment;14;33;18;30;21;29;19;16;10;15;11;14;13;37;35;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;16;-1819.161,447.7129;Inherit;True;Property;_ORMMap;ORM Map;9;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1821.161,-191.2872;Inherit;True;Property;_ColorMap;Color Map;7;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;30;-1792.295,1259.69;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;17;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1860.161,652.7126;Inherit;False;Property;_RoughnessIntensity;Roughness Intensity;13;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1761.161,-280.2871;Inherit;False;Property;_TextureSize;Texture Size;6;0;Create;True;0;0;0;False;1;Header(Base Maps);False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1798.161,-0.2872119;Inherit;False;Property;_ColorTint;Color Tint;11;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;-1865.635,737.5316;Inherit;False;Property;_AOIntensity;AO Intensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;-1799.295,1086.69;Inherit;False;Property;_EmissiveColor;Emissive Color;16;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-1862.313,815.3082;Inherit;False;Property;_MetallicIntensity;Metallic Intensity;15;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1864.161,372.7129;Inherit;False;Property;_NormalIntensity;Normal Intensity;12;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;19;-1817.773,895.9807;Inherit;True;Property;_EmissiveMap;Emissive Map;10;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;14;-1818.244,173.7128;Inherit;True;Property;_NormalMap;Normal Map;8;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;True;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;78;-1267.661,282.571;Inherit;False;PA_SF_BasePBR_OpaqueORM;0;;161;a21dcaebf1379e5439b421c5da1cd710;0;12;20;FLOAT;100;False;9;SAMPLER2D;0;False;15;COLOR;0,0,0,0;False;22;SAMPLER2D;;False;28;FLOAT;1;False;59;SAMPLER2D;;False;38;FLOAT;1;False;73;FLOAT;0;False;39;FLOAT;0;False;52;SAMPLER2D;;False;53;COLOR;0,0,0,0;False;54;FLOAT;0;False;6;COLOR;0;FLOAT3;7;COLOR;6;FLOAT;5;FLOAT;3;FLOAT;8
Node;AmplifyShaderEditor.CommentaryNode;39;-1884.634,1396.396;Inherit;False;1051.886;1058.01;Comment;7;40;47;50;51;48;46;41;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;51;-1796.948,1534.396;Inherit;True;Property;_CoverageColorMap;Coverage Color Map;20;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;38;-814.0967,792.8811;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-1078.263,1097.922;Inherit;False;Property;_NoiseTextureSize;Noise Texture Size;26;1;[Header];Create;True;1;Slope Maps;0;0;False;0;False;100;0.47;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;48;-1794.143,1744.06;Inherit;True;Property;_CoverageNormalMap;Coverage Normal Map;21;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;46;-1736.948,1446.396;Inherit;False;Property;_CoverageSize;Coverage Size;19;0;Create;True;0;0;0;False;0;False;100;21.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1841.143,1943.06;Inherit;False;Property;_CoverageNormalIntensity;Coverage Normal Intensity;23;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;35;-1099.816,910.8152;Inherit;True;Property;_SlopeNoiseTexture;Slope Noise Texture;27;2;[Header];[SingleLineTexture];Create;True;0;0;0;False;0;False;None;8965de8a437b2304f8e4aa6a137ab1ed;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;50;-1797.532,2027.616;Inherit;True;Property;_CoverageORMMap; Coverage ORM Map;22;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;40;-1839.295,2322.321;Inherit;False;Property;_CoverageMetallicIntensity;Coverage Metallic Intensity;25;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-1844.312,2226.64;Inherit;False;Property;_CoverageRoughnessIntensity;Coverage Roughness Intensity;24;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;77;-1325.896,1787.005;Inherit;False;PA_SF_BasePBR_OpaqueORM;0;;163;a21dcaebf1379e5439b421c5da1cd710;0;12;20;FLOAT;100;False;9;SAMPLER2D;0;False;15;COLOR;0,0,0,0;False;22;SAMPLER2D;;False;28;FLOAT;1;False;59;SAMPLER2D;;False;38;FLOAT;1;False;73;FLOAT;0;False;39;FLOAT;0;False;52;SAMPLER2D;;False;53;COLOR;0,0,0,0;False;54;FLOAT;0;False;6;COLOR;0;FLOAT3;7;COLOR;6;FLOAT;5;FLOAT;3;FLOAT;8
Node;AmplifyShaderEditor.FunctionNode;76;-600.0573,1077.209;Inherit;False;PA_SF_VerticalBlend;28;;165;b9497802ac41dee4999b8364feedf4c9;0;3;36;SAMPLER2D;;False;38;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;62;255.9019,1148.737;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;55;257.4714,745.9722;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;56;261.5979,894.4583;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;64;254.4813,1280.169;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;63;252.928,1022.325;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;59;829.8355,535.1212;Inherit;False;Property;_EnableCoverage;Enable Coverage?;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;61;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;61;831.9483,654.4428;Inherit;False;Property;_EnableCoverage;Enable Coverage?;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;57;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;57;828.9517,170.9645;Inherit;False;Property;_EnableCoverage;Enable Coverage?;18;0;Create;True;0;0;0;False;1;Header(Coverage Maps);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;58;832.7849,283.4584;Inherit;False;Property;_EnableCoverage;Enable Coverage?;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;59;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;60;829.697,422.3714;Inherit;False;Property;_EnableCoverage;Enable Coverage?;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;59;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1745.844,322.6512;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Surface;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;78;20;10;0
WireConnection;78;9;11;0
WireConnection;78;15;13;0
WireConnection;78;22;14;0
WireConnection;78;28;15;0
WireConnection;78;59;16;0
WireConnection;78;38;18;0
WireConnection;78;73;33;0
WireConnection;78;39;21;0
WireConnection;78;52;19;0
WireConnection;78;53;29;0
WireConnection;78;54;30;0
WireConnection;38;0;78;7
WireConnection;77;20;46;0
WireConnection;77;9;51;0
WireConnection;77;22;48;0
WireConnection;77;28;47;0
WireConnection;77;59;50;0
WireConnection;77;38;41;0
WireConnection;77;39;40;0
WireConnection;76;36;35;0
WireConnection;76;38;37;0
WireConnection;76;2;38;0
WireConnection;62;0;78;5
WireConnection;62;1;77;5
WireConnection;62;2;76;0
WireConnection;55;0;78;0
WireConnection;55;1;77;0
WireConnection;55;2;76;0
WireConnection;56;0;78;7
WireConnection;56;1;77;7
WireConnection;56;2;76;0
WireConnection;64;0;77;8
WireConnection;64;1;78;8
WireConnection;64;2;76;0
WireConnection;63;0;77;3
WireConnection;63;1;78;3
WireConnection;63;2;76;0
WireConnection;59;1;78;5
WireConnection;59;0;62;0
WireConnection;61;1;78;8
WireConnection;61;0;64;0
WireConnection;57;1;78;0
WireConnection;57;0;55;0
WireConnection;58;1;78;7
WireConnection;58;0;56;0
WireConnection;60;1;78;3
WireConnection;60;0;63;0
WireConnection;0;0;57;0
WireConnection;0;1;58;0
WireConnection;0;2;78;6
WireConnection;0;3;60;0
WireConnection;0;4;59;0
WireConnection;0;5;61;0
ASEEND*/
//CHKSM=0ECE35FD65BE4559438899DEE623A8E0A014955B