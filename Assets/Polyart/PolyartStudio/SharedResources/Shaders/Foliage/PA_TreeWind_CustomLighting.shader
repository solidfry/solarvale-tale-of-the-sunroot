// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Tree Wind Custom Lighting"
{
	Properties
	{
		[Header(Foliage)][Header(.)][SingleLineTexture]_FoliageColorMap("Foliage Color Map", 2D) = "white" {}
		_MaskClipValue("Mask Clip Value", Range( 0 , 1)) = 0.5
		_FoliageSize("Foliage Size", Float) = 100
		_FoliageColorTop("Foliage Color Top", Color) = (1,1,1,0)
		_FoliageColorBottom("Foliage Color Bottom", Color) = (0,0,0,0)
		_GradientOffset("Gradient Offset", Float) = 0
		_GradientFallout("Gradient Fallout", Float) = 0
		[Header(TRUNK)][Toggle(_TRUNKMATERIAL_ON)] _TrunkMaterial("Trunk Material?", Float) = 0
		[Header(WIND RUSTLE)][Toggle(_USEGLOBALWINDSETTINGS_ON)] _UseGlobalWindSettings("Use Global Wind Settings?", Float) = 0
		[HideInInspector][SingleLineTexture]_NoiseTexture("NoiseTexture", 2D) = "white" {}
		_WindScrollSpeed("Wind Scroll Speed", Range( 0 , 0.5)) = 0.05
		_WindJitterSpeed("Wind Jitter Speed", Range( 0 , 0.5)) = 0.05
		_WindOffsetIntensity("Wind Offset Intensity", Range( 0 , 1)) = 1
		_WindRustleSize("Wind Rustle Size", Range( 0 , 0.2)) = 0.035
		[Header(WIND SWAY)][Toggle(_USESGLOBALWINDSETTINGS_ON)] _UsesGlobalWindSettings("Uses Global Wind Settings?", Float) = 0
		_WindSwayDirection("Wind Sway Direction", Vector) = (1,0,0,0)
		_WIndSwayIntensity("WInd Sway Intensity", Float) = 1
		_WIndSwayFrequency("WInd Sway Frequency", Float) = 1
		[Header(Lighting Settings)][Space(5)]_LightOffset("Light Offset", Range( 0 , 1)) = 0
		_DirectLightInt("Direct Light Int", Range( 1 , 10)) = 1
		_IndirectLightningIntensity("Indirect Lightning Intensity", Range( 1 , 10)) = 1
		_SubsurfaceIntensity("Subsurface Intensity", Range( 0 , 100)) = 10
		_SubsurfaceFadeDistance("Subsurface Fade Distance", Float) = 200
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TreeTransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEGLOBALWINDSETTINGS_ON
		#pragma shader_feature_local _USESGLOBALWINDSETTINGS_ON
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
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _NoiseTexture;
		uniform float _WindScrollSpeed;
		uniform float varWindRustleScrollSpeed;
		uniform float _WindRustleSize;
		uniform float _WindJitterSpeed;
		uniform float Float0;
		uniform float _WindOffsetIntensity;
		uniform float _WIndSwayIntensity;
		uniform float varWindSwayIntensity;
		uniform float2 _WindSwayDirection;
		uniform float2 varWindDirection;
		uniform float _WIndSwayFrequency;
		uniform float varWindSwayFrequency;
		uniform sampler2D _FoliageColorMap;
		uniform float _FoliageSize;
		uniform float4 _FoliageColorBottom;
		uniform float4 _FoliageColorTop;
		uniform float _GradientOffset;
		uniform float _GradientFallout;
		uniform float _IndirectLightningIntensity;
		uniform float _LightOffset;
		uniform float _DirectLightInt;
		uniform float _SubsurfaceIntensity;
		uniform float _SubsurfaceFadeDistance;
		uniform float _MaskClipValue;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float temp_output_18_0_g87 = _WindScrollSpeed;
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch25_g87 = ( temp_output_18_0_g87 * varWindRustleScrollSpeed );
			#else
				float staticSwitch25_g87 = temp_output_18_0_g87;
			#endif
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult4_g87 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_7_0_g87 = ( appendResult4_g87 * _WindRustleSize );
			float2 panner9_g87 = ( ( staticSwitch25_g87 * _Time.y ) * float2( 1,1 ) + temp_output_7_0_g87);
			float temp_output_19_0_g87 = _WindJitterSpeed;
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch26_g87 = ( temp_output_19_0_g87 * Float0 );
			#else
				float staticSwitch26_g87 = temp_output_19_0_g87;
			#endif
			float2 panner13_g87 = ( ( _Time.y * staticSwitch26_g87 ) * float2( 1,1 ) + ( temp_output_7_0_g87 * float2( 2,2 ) ));
			float4 lerpResult30_g87 = lerp( float4( float3(0,0,0) , 0.0 ) , ( pow( tex2Dlod( _NoiseTexture, float4( panner9_g87, 0, 0.0) ) , 1.0 ) * tex2Dlod( _NoiseTexture, float4( panner13_g87, 0, 0.0) ) ) , _WindOffsetIntensity);
			float3 ase_vertex3Pos = v.vertex.xyz;
			float temp_output_27_0_g88 = _WIndSwayIntensity;
			#ifdef _USESGLOBALWINDSETTINGS_ON
				float staticSwitch33_g88 = ( temp_output_27_0_g88 * varWindSwayIntensity );
			#else
				float staticSwitch33_g88 = temp_output_27_0_g88;
			#endif
			float temp_output_14_0_g88 = ( ( ase_vertex3Pos.y * ( staticSwitch33_g88 / 100.0 ) ) + 1.0 );
			float temp_output_16_0_g88 = ( temp_output_14_0_g88 * temp_output_14_0_g88 );
			#ifdef _USESGLOBALWINDSETTINGS_ON
				float2 staticSwitch41_g88 = varWindDirection;
			#else
				float2 staticSwitch41_g88 = _WindSwayDirection;
			#endif
			float2 clampResult10_g88 = clamp( staticSwitch41_g88 , float2( -1,-1 ) , float2( 1,1 ) );
			float4 transform1_g88 = mul(unity_ObjectToWorld,float4( 0,0,0,1 ));
			float4 appendResult3_g88 = (float4(transform1_g88.x , 0.0 , transform1_g88.z , 0.0));
			float dotResult4_g89 = dot( appendResult3_g88.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g89 = lerp( 1.0 , 1.01 , frac( ( sin( dotResult4_g89 ) * 43758.55 ) ));
			float mulTime9_g88 = _Time.y * lerpResult10_g89;
			float temp_output_29_0_g88 = _WIndSwayFrequency;
			#ifdef _USESGLOBALWINDSETTINGS_ON
				float staticSwitch34_g88 = ( temp_output_29_0_g88 * varWindSwayFrequency );
			#else
				float staticSwitch34_g88 = temp_output_29_0_g88;
			#endif
			float2 break26_g88 = ( ( ( temp_output_16_0_g88 * temp_output_16_0_g88 ) - temp_output_16_0_g88 ) * ( ( ( staticSwitch41_g88 * float2( 4,4 ) ) + sin( ( ( clampResult10_g88 * mulTime9_g88 ) * staticSwitch34_g88 ) ) ) / float2( 4,4 ) ) );
			float4 appendResult25_g88 = (float4(break26_g88.x , 0.0 , break26_g88.y , 0.0));
			float4 temp_output_246_0 = appendResult25_g88;
			#ifdef _TRUNKMATERIAL_ON
				float4 staticSwitch100 = temp_output_246_0;
			#else
				float4 staticSwitch100 = ( ( float4( ase_vertexNormal , 0.0 ) * lerpResult30_g87 ) + temp_output_246_0 );
			#endif
			float4 vWind116 = staticSwitch100;
			v.vertex.xyz += vWind116.rgb;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float temp_output_235_0 = ( _FoliageSize / 100.0 );
			float2 appendResult234 = (float2(temp_output_235_0 , temp_output_235_0));
			float2 uv_TexCoord236 = i.uv_texcoord * appendResult234;
			float4 tex2DNode124 = tex2D( _FoliageColorMap, uv_TexCoord236 );
			float vFoliageOpacity173 = tex2DNode124.a;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			UnityGI gi301 = gi;
			float3 diffNorm301 = ase_normWorldNormal;
			gi301 = UnityGI_Base( data, 1, diffNorm301 );
			float3 indirectDiffuse301 = gi301.indirect.diffuse + diffNorm301 * 0.0001;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 lerpResult115 = lerp( _FoliageColorBottom , _FoliageColorTop , saturate( ( ( ase_vertex3Pos.y + _GradientOffset ) * ( _GradientFallout * 2 ) ) ));
			float4 vFoliageColor172 = ( lerpResult115 * tex2DNode124 );
			float4 vIndirectLightning310 = ( ( float4( indirectDiffuse301 , 0.0 ) * vFoliageColor172 ) * _IndirectLightningIntensity );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult4_g91 = dot( ase_worldlightDir , ase_normWorldNormal );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 DirectLight312 = ( ( ( ( saturate( (dotResult4_g91*1.0 + _LightOffset) ) * ase_lightAtten ) * ase_lightColor ) * vFoliageColor172 ) * _DirectLightInt );
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult11_g90 = dot( ase_worldlightDir , ase_worldViewDir );
			float dotResult4_g90 = dot( ase_worldlightDir , ase_normWorldNormal );
			float clampResult9_g93 = clamp( pow( ( distance( ase_worldPos , _WorldSpaceCameraPos ) / _SubsurfaceFadeDistance ) , 2.0 ) , 0.0 , 1.0 );
			float lerpResult327 = lerp( _SubsurfaceIntensity , 0.0 , clampResult9_g93);
			float4 vSubsurface322 = saturate( ( ( (( dotResult11_g90 * -1.0 )*1.0 + -0.25) * ( ( ( (dotResult4_g90*1.0 + 1.0) * ase_lightAtten ) * ase_lightColor * vFoliageColor172 ) * 0.235 ) ) * lerpResult327 ) );
			c.rgb = ( vIndirectLightning310 + DirectLight312 + vSubsurface322 ).rgb;
			c.a = 1;
			clip( vFoliageOpacity173 - _MaskClipValue );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

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
				vertexDataFunc( v, customInputData );
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
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
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
Version=19105
Node;AmplifyShaderEditor.RangedFloatNode;233;-3416.064,-1881.969;Inherit;False;Property;_FoliageSize;Foliage Size;5;0;Create;True;0;0;0;False;0;False;100;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;235;-3240.15,-1876.368;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3366.52,-2044.726;Inherit;False;Property;_GradientFallout;Gradient Fallout;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-3364.521,-2127.726;Inherit;False;Property;_GradientOffset;Gradient Offset;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;234;-3105.964,-1883.67;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;123;-2704.77,-2184.466;Inherit;True;Property;_FoliageColorMap;Foliage Color Map;2;2;[Header];[SingleLineTexture];Create;True;2;Foliage;.;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;236;-2948.401,-1905.561;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;114;-3026.939,-2483.921;Inherit;False;Property;_FoliageColorBottom;Foliage Color Bottom;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;112;-3015.271,-2301.595;Inherit;False;Property;_FoliageColorTop;Foliage Color Top;6;1;[Header];Create;True;0;0;0;False;0;False;1,1,1,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;131;-3076.101,-2118.791;Inherit;False;PA_SF_ObjectGradient;-1;;52;f7566061dd2a41c4bbc5f0e0ea7b5f5b;0;2;8;FLOAT;0;False;9;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;124;-2448.591,-2185.154;Inherit;True;Property;_TextureSample2;Texture Sample 2;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;115;-2669.975,-2319.941;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;-2104.469,-2320.945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;172;-1890.849,-2326.3;Inherit;False;vFoliageColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;259;-3522.616,-862.6478;Inherit;False;958.9546;897.2181;;14;312;325;300;283;298;310;323;303;322;324;307;279;297;301;Custom Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;122;-1209.571,1348.299;Inherit;False;1580.315;688.4131;Wind;12;194;212;116;100;246;77;101;104;102;252;256;257;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;297;-3449.756,-446.9383;Inherit;False;172;vFoliageColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;256;-1021.847,1545.175;Inherit;False;Property;_WindRustleSize;Wind Rustle Size;19;0;Create;True;0;0;0;False;0;False;0.035;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;194;-1024.609,1387.139;Inherit;False;Property;_WindScrollSpeed;Wind Scroll Speed;16;0;Create;True;0;0;0;False;0;False;0.05;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;252;-1023.142,1470.882;Inherit;False;Property;_WindOffsetIntensity;Wind Offset Intensity;18;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-972.6572,1786.634;Inherit;False;Property;_WIndSwayFrequency;WInd Sway Frequency;24;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-958.6573,1705.634;Inherit;False;Property;_WIndSwayIntensity;WInd Sway Intensity;23;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;104;-959.6573,1864.635;Inherit;False;Property;_WindSwayDirection;Wind Sway Direction;22;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.IndirectDiffuseLighting;301;-3448.753,-539.937;Inherit;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;212;-1019.474,1626.56;Inherit;False;Property;_WindJitterSpeed;Wind Jitter Speed;17;0;Create;True;0;0;0;False;0;False;0.05;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;303;-3288.529,-363.8424;Inherit;False;Property;_IndirectLightningIntensity;Indirect Lightning Intensity;27;0;Create;True;0;0;0;False;0;False;1;0;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;257;-710.6141,1493.55;Inherit;False;PA_SF_WindRustleNoise;13;;87;7733c52bc6ce2e94b9c81cb72dee5854;0;4;18;FLOAT;0;False;33;FLOAT;1;False;35;FLOAT;0.035;False;19;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-3418.625,-248.9114;Inherit;False;172;vFoliageColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;-3458.255,-766.8471;Inherit;False;172;vFoliageColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;283;-3501.57,-160.0811;Inherit;False;Property;_LightOffset;Light Offset;25;0;Create;True;0;0;0;False;2;Header(Lighting Settings);Space(5);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;300;-3502.948,-60.73007;Inherit;False;Property;_DirectLightInt;Direct Light Int;26;0;Create;True;0;0;0;False;0;False;1;0;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;307;-3151.009,-466.4042;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;246;-699.9568,1632.734;Inherit;False;PA_SF_WindSway;20;;88;bc8ec8a781a3c384e9042e29b2eae6d5;0;3;27;FLOAT;0;False;29;FLOAT;1;False;30;FLOAT2;1,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;324;-3151.632,-761.2469;Inherit;False;PA_SF_Subsurface;0;;90;8f5ee1ef24284b9448f8c4a7274f8883;0;2;23;FLOAT4;0,0,0,0;False;24;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-362.2125,1493.299;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;323;-2980.28,-465.283;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;325;-3182.808,-177.1063;Inherit;False;PA_SF_CustomLightning;-1;;91;898d5a8db4cd68548bb7f6d6ea6222d8;0;3;15;FLOAT4;0,0,0,0;False;16;FLOAT;1;False;17;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;310;-2813.299,-469.4492;Inherit;False;vIndirectLightning;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;322;-2812.493,-767.5549;Inherit;False;vSubsurface;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;312;-2907.118,-182.0311;Inherit;False;DirectLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;100;-185.6695,1601.949;Inherit;False;Property;_TrunkMaterial;Trunk Material?;13;0;Create;True;0;0;0;False;1;Header(TRUNK);False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;-1;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;316;572.1924,1054.025;Inherit;False;312;DirectLight;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;315;564.1074,1143.443;Inherit;False;322;vSubsurface;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;63.9295,1601.649;Inherit;False;vWind;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;314;560.0994,948.151;Inherit;False;310;vIndirectLightning;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;173;-2059.848,-2090.301;Inherit;False;vFoliageOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;275;-2163.836,-738.4329;Inherit;False;1479.169;401.6033;;6;313;308;302;292;288;286;Smoothness;0.8,0.7843137,0.5607843,1;0;0
Node;AmplifyShaderEditor.SaturateNode;308;-1087.613,-643.7687;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;183;681.7533,851.369;Inherit;False;173;vFoliageOpacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;222;-2949.453,-1670.345;Inherit;False;Property;_FoliageEmissiveColor;Foliage Emissive Color;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;317;572.9624,1241.736;Inherit;False;313;Smoothness;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;822.1142,1272.033;Inherit;False;116;vWind;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;318;811.9124,1045.601;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;-2644.069,-1665.414;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;225;-2438.064,-1669.412;Inherit;False;vFoliageEmissive;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;313;-890.7839,-647.9689;Inherit;False;Smoothness;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;286;-1902.606,-449.1037;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;292;-1552.157,-551.5374;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomStandardSurface;302;-1372.709,-644.2745;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;107;357.2644,592.0134;Inherit;False;Property;_MaskClipValue;Mask Clip Value;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;224;-2949.941,-1490.076;Inherit;False;Property;_FoliageEmissiveIntensity;Foliage Emissive Intensity;12;0;Create;True;0;0;0;False;0;False;0;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;288;-1923.158,-677.5372;Inherit;True;Property;_SmoothnessTexture1;Smoothness;3;1;[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1093.135,805.4158;Float;False;True;-1;2;;0;0;CustomLighting;Polyart/Dreamscape/Builtin/Tree Wind Custom Lighting;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TreeTransparentCutout;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;_Culling;-1;0;True;_MaskClipValue;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.LerpOp;327;-3738.126,-736.7804;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;296;-4123.761,-740.2956;Inherit;False;Property;_SubsurfaceIntensity;Subsurface Intensity;28;0;Create;True;0;0;0;False;0;False;10;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;328;-4143.146,-640.9213;Inherit;False;PA_SF_DistanceBlend;-1;;93;35a1c16c00037ed46b6114de6dd42a44;0;2;6;FLOAT;200;False;8;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;329;-4454.592,-646.6564;Inherit;False;Property;_SubsurfaceFadeDistance;Subsurface Fade Distance;29;0;Create;True;0;0;0;False;0;False;200;0;0;0;0;1;FLOAT;0
WireConnection;235;0;233;0
WireConnection;234;0;235;0
WireConnection;234;1;235;0
WireConnection;236;0;234;0
WireConnection;131;8;110;0
WireConnection;131;9;111;0
WireConnection;124;0;123;0
WireConnection;124;1;236;0
WireConnection;115;0;114;0
WireConnection;115;1;112;0
WireConnection;115;2;131;0
WireConnection;125;0;115;0
WireConnection;125;1;124;0
WireConnection;172;0;125;0
WireConnection;257;18;194;0
WireConnection;257;33;252;0
WireConnection;257;35;256;0
WireConnection;257;19;212;0
WireConnection;307;0;301;0
WireConnection;307;1;297;0
WireConnection;246;27;101;0
WireConnection;246;29;102;0
WireConnection;246;30;104;0
WireConnection;324;23;279;0
WireConnection;324;24;327;0
WireConnection;77;0;257;0
WireConnection;77;1;246;0
WireConnection;323;0;307;0
WireConnection;323;1;303;0
WireConnection;325;15;298;0
WireConnection;325;16;283;0
WireConnection;325;17;300;0
WireConnection;310;0;323;0
WireConnection;322;0;324;0
WireConnection;312;0;325;0
WireConnection;100;1;77;0
WireConnection;100;0;246;0
WireConnection;116;0;100;0
WireConnection;173;0;124;4
WireConnection;308;0;302;0
WireConnection;318;0;314;0
WireConnection;318;1;316;0
WireConnection;318;2;315;0
WireConnection;223;0;222;0
WireConnection;223;1;224;0
WireConnection;225;0;223;0
WireConnection;313;0;308;0
WireConnection;292;0;288;0
WireConnection;292;1;286;0
WireConnection;302;4;292;0
WireConnection;0;10;183;0
WireConnection;0;13;318;0
WireConnection;0;11;117;0
WireConnection;327;0;296;0
WireConnection;327;2;328;0
WireConnection;328;6;329;0
ASEEND*/
//CHKSM=98A36198A688A395D7EEF877C9DB43CDFEB8720E