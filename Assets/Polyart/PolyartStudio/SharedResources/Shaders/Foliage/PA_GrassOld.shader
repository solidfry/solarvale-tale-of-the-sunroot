// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/TerrainEngine/Details/WavingDoublePass"
{
	Properties
	{
		_WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_PivotLockPower("Pivot Lock Power", Range( 0 , 10)) = 0.3
		_MainTex("Foliage Texture", 2D) = "white" {}
		_WaveScale("Wave Scale", Float) = 40
		_PanningWaveTexture("Panning Wave Texture", 2D) = "white" {}
		[Toggle(_DITHERINGON_ON)] _DitheringON("Dithering ON", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Off
		AlphaToMask On
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.0
		#pragma multi_compile_instancing
		#pragma shader_feature _DITHERINGON_ON
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float4 screenPosition;
			float eyeDepth;
		};

		uniform sampler2D _WindNoiseTexture;
		uniform float WindNoiseSmall;
		uniform float WindNoiseSmallMultiply;
		uniform float WindNoiseLarge;
		uniform float WindNoiseLargeMultiply;
		uniform float _PivotLockPower;
		uniform float4 CloudColor;
		uniform sampler2D _PanningWaveTexture;
		uniform float2 CloudSpeed;
		uniform float _WaveScale;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float FoliageSmoothness;
		uniform float FoliageRenderDistance;
		uniform float FoliageDitherMin;
		uniform float FoliageDitherMax;


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1));
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1));
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		inline float Dither4x4Bayer( int x, int y )
		{
			const float dither[ 16 ] = {
				 1,  9,  3, 11,
				13,  5, 15,  7,
				 4, 12,  2, 10,
				16,  8, 14,  6 };
			int r = y * 4 + x;
			return dither[r] / 16; // same # of instructions as pre-dividing due to compiler magic
		}


		inline float Dither8x8Bayer( int x, int y )
		{
			const float dither[ 64 ] = {
				 1, 49, 13, 61,  4, 52, 16, 64,
				33, 17, 45, 29, 36, 20, 48, 32,
				 9, 57,  5, 53, 12, 60,  8, 56,
				41, 25, 37, 21, 44, 28, 40, 24,
				 3, 51, 15, 63,  2, 50, 14, 62,
				35, 19, 47, 31, 34, 18, 46, 30,
				11, 59,  7, 55, 10, 58,  6, 54,
				43, 27, 39, 23, 42, 26, 38, 22};
			int r = y * 8 + x;
			return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 color104 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 lerpResult100 = lerp( ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.2 ) * _Time.y ) + ( (ase_worldPos).xz / WindNoiseSmall ) ), 0, 0.0) ) * WindNoiseSmallMultiply ) , ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.1 ) * _Time.y ) + ( (ase_worldPos).xz / WindNoiseLarge ) ), 0, 0.0) ) * WindNoiseLargeMultiply ) , 0.5);
			Gradient gradient79 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 temp_cast_0 = (_PivotLockPower).xxxx;
			float4 lerpResult107 = lerp( color104 , lerpResult100 , pow( SampleGradient( gradient79, v.texcoord.xy.y ) , temp_cast_0 ));
			float4 vWind111 = ( v.color.a * lerpResult107 );
			v.vertex.xyz += vWind111.rgb;
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 panner174 = ( _Time.y * (( CloudSpeed / float2( 10,10 ) )).xy + (( ase_worldPos / _WaveScale )).xz);
			float4 lerpResult176 = lerp( i.vertexColor , ( i.vertexColor * CloudColor ) , tex2D( _PanningWaveTexture, panner174 ).r);
			float4 vColor185 = lerpResult176;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 vTexture128 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = ( vColor185 * (vTexture128).r ).rgb;
			o.Smoothness = FoliageSmoothness;
			float temp_output_130_0 = (vTexture128).a;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen157 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither157 = Dither4x4Bayer( fmod(clipScreen157.x, 4), fmod(clipScreen157.y, 4) );
			float cameraDepthFade153 = (( i.eyeDepth -_ProjectionParams.y - FoliageRenderDistance ) / FoliageRenderDistance);
			dither157 = step( dither157, ( 1.0 - cameraDepthFade153 ) );
			float vGrassDistance159 = dither157;
			float2 clipScreen160 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither160 = Dither8x8Bayer( fmod(clipScreen160.x, 8), fmod(clipScreen160.y, 8) );
			float clampResult158 = clamp( (FoliageDitherMin + (i.uv_texcoord.y - 0.1) * (FoliageDitherMax - FoliageDitherMin) / (0.5 - 0.1)) , 0.0 , 1.0 );
			dither160 = step( dither160, clampResult158 );
			float vTerrainDither161 = dither160;
			#ifdef _DITHERINGON_ON
				float staticSwitch192 = ( ( temp_output_130_0 * vGrassDistance159 ) * vTerrainDither161 );
			#else
				float staticSwitch192 = temp_output_130_0;
			#endif
			o.Alpha = staticSwitch192;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				half4 color : COLOR0;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.screenPosition;
				o.customPack1.z = customInputData.eyeDepth;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				surfIN.screenPosition = IN.customPack2.xyzw;
				surfIN.eyeDepth = IN.customPack1.z;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18912
1158;73;1945;1404;2446.702;764.7916;1.251769;True;False
Node;AmplifyShaderEditor.CommentaryNode;41;-3134.081,885.6868;Inherit;False;2133.215;1279.717;;2;43;42;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;43;-3088.27,935.9296;Inherit;False;1890.47;670.058;Wind Small;14;100;94;90;87;78;76;72;66;61;55;54;53;50;47;;0.2216981,1,0.9921367,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;42;-3087.945,1623.933;Inherit;False;1598.144;507.6038;Wind Large;11;93;86;85;73;65;63;60;58;52;49;45;;0.9711054,0.495283,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;45;-3033.602,1935.17;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;47;-3038.27,1247.533;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;58;-2890.786,1696.778;Inherit;False;Constant;_Vector1;Vector 1;14;0;Create;True;0;0;0;False;0;False;0,0.1;0,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;49;-2836.602,2016.171;Inherit;False;Global;WindNoiseLarge;Wind Noise Large;7;0;Create;True;0;0;0;False;0;False;20;65.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2832.27,1333.533;Inherit;False;Global;WindNoiseSmall;Wind Noise Small;10;0;Create;True;0;0;0;True;0;False;20;56.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;53;-2898.455,1138.143;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;55;-2895.455,1009.142;Inherit;False;Constant;_Vector0;Vector 0;15;0;Create;True;0;0;0;False;0;False;0,0.2;0,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2892.786,1825.78;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;54;-2838.27,1242.533;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;60;-2833.602,1932.17;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;166;-2295.83,-1406.056;Inherit;False;1286;481.0001;Dithering;12;152;187;188;161;160;159;157;158;154;155;153;151;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-2258.83,-1331.057;Inherit;False;Global;FoliageRenderDistance;FoliageRenderDistance;7;0;Create;True;0;0;0;True;0;False;35;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-2656.455,1015.142;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;183;-3304.599,-143.1409;Inherit;False;Global;CloudSpeed;CloudSpeed;13;0;Create;True;0;0;0;False;0;False;0.2,1;0.404,0.314;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;168;-3274.512,-235.6552;Inherit;False;Property;_WaveScale;Wave Scale;3;0;Create;True;0;0;0;False;0;False;40;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;167;-3291.513,-380.6552;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;66;-2543.27,1248.533;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-2659.786,1701.778;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;63;-2558.602,1933.17;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;188;-2059.459,-1053.599;Inherit;False;Global;FoliageDitherMax;FoliageDitherMax;7;0;Create;True;0;0;0;False;0;False;6;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-2328.787,1702.778;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-2057.459,-1135.599;Inherit;False;Global;FoliageDitherMin;FoliageDitherMin;6;0;Create;True;0;0;0;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-2333.455,1015.142;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;170;-3036.512,-380.6552;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;184;-3098.599,-138.1409;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;10,10;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;75;-986.2117,1084.967;Inherit;False;912.9565;352;;5;106;98;80;79;95;Vertical Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;126;-2124.598,-0.3577515;Inherit;True;Property;_MainTex;Foliage Texture;2;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;152;-2243.83,-1252.056;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;76;-2546.264,1412.986;Inherit;True;Property;_WindNoiseTexture;Wind Noise Texture;0;0;Create;True;0;0;0;False;0;False;ecd0578dd5d2cb54e90e298c6fbe1019;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CameraDepthFade;153;-1831.83,-1350.056;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;154;-1781.83,-1202.056;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.5;False;3;FLOAT;1;False;4;FLOAT;6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;86;-2050.619,1673.565;Inherit;True;Property;_TextureSample2;Texture Sample 2;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-936.2117,1210.673;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;127;-1897.122,2.403341;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;172;-2958.462,-143.8214;Inherit;False;True;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;155;-1570.83,-1351.056;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;87;-2055.287,985.9294;Inherit;True;Property;_TextureSample1;Texture Sample 1;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;173;-2921.559,-48.62187;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-2033.13,1177.232;Inherit;False;Global;WindNoiseSmallMultiply;Wind Noise Small Multiply;4;0;Create;True;0;0;0;False;0;False;1;1.02;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-2028.463,1867.87;Inherit;False;Global;WindNoiseLargeMultiply;Wind Noise Large Multiply;3;0;Create;True;0;0;0;False;0;False;1;-0.85;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;79;-903.2125,1135.673;Inherit;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.ComponentMaskNode;171;-2875.511,-385.6552;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-655.3174,1331.58;Inherit;False;Property;_PivotLockPower;Pivot Lock Power;1;0;Create;True;0;0;0;False;0;False;0.3;0.95;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;157;-1421.83,-1356.056;Inherit;False;0;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;158;-1585.83,-1203.056;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;182;-2256.428,-576.6761;Inherit;False;Global;CloudColor;CloudColor;14;0;Create;True;0;0;0;True;0;False;0.6792453,0.6792453,0.6792453,0;0.5471698,0.3150372,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-1659.131,990.2324;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-1654.461,1677.869;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;144;-2220.005,-745.9551;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-1623.69,1233.439;Inherit;False;Constant;_Float1;Float 1;19;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;174;-2591.315,-380.9919;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;128;-1583.479,3.033531;Inherit;False;vTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GradientSampleNode;98;-683.2125,1135.673;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;-1965.428,-472.6761;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DitheringNode;160;-1435.83,-1207.056;Inherit;False;1;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;100;-1381.805,992.6625;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-934.9385,-121.5946;Inherit;False;128;vTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;104;-377.3684,857.2747;Inherit;False;Constant;_Color2;Color2;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;106;-334.2544,1134.967;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;175;-2338.872,-408.8817;Inherit;True;Property;_PanningWaveTexture;Panning Wave Texture;4;0;Create;True;0;0;0;False;0;False;-1;ecd0578dd5d2cb54e90e298c6fbe1019;ecd0578dd5d2cb54e90e298c6fbe1019;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;159;-1236.83,-1356.056;Inherit;False;vGrassDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;130;-642.9385,-43.5946;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-1240.83,-1208.056;Inherit;False;vTerrainDither;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-613.8667,107.0067;Inherit;False;159;vGrassDistance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;176;-1714.406,-428.3253;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;189;-73.07275,788.4829;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;107;-55.60028,967.2615;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-439.4504,192.0067;Inherit;False;161;vTerrainDither;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;185;-1542.348,-433.2154;Inherit;False;vColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;-379.1667,88.0067;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;128.9272,884.4829;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;165;-243.8667,90.0067;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;186;-458.8481,-327.3428;Inherit;False;185;vColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;111;273.6117,878.9085;Inherit;False;vWind;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;132;-642.9385,-203.5946;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-169.4222,-253.5468;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;192;-56.86765,58.63751;Inherit;False;Property;_DitheringON;Dithering ON;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;_DITHERINGON_ON;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-120.1959,347.2232;Inherit;False;111;vWind;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-126.2169,-74.7005;Inherit;False;Global;FoliageSmoothness;FoliageSmoothness;0;0;Create;True;0;0;0;False;0;False;0.1;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;133;-642.9385,-123.5946;Inherit;False;False;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;188.2007,-165.6634;Float;False;True;-1;4;;0;0;Standard;Hidden/TerrainEngine/Details/WavingDoublePass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.8;True;True;0;True;Opaque;;AlphaTest;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;5;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;47;0
WireConnection;60;0;45;0
WireConnection;61;0;55;0
WireConnection;61;1;53;0
WireConnection;66;0;54;0
WireConnection;66;1;50;0
WireConnection;65;0;58;0
WireConnection;65;1;52;0
WireConnection;63;0;60;0
WireConnection;63;1;49;0
WireConnection;73;0;65;0
WireConnection;73;1;63;0
WireConnection;72;0;61;0
WireConnection;72;1;66;0
WireConnection;170;0;167;0
WireConnection;170;1;168;0
WireConnection;184;0;183;0
WireConnection;153;0;151;0
WireConnection;153;1;151;0
WireConnection;154;0;152;2
WireConnection;154;3;187;0
WireConnection;154;4;188;0
WireConnection;86;0;76;0
WireConnection;86;1;73;0
WireConnection;127;0;126;0
WireConnection;172;0;184;0
WireConnection;155;0;153;0
WireConnection;87;0;76;0
WireConnection;87;1;72;0
WireConnection;171;0;170;0
WireConnection;157;0;155;0
WireConnection;158;0;154;0
WireConnection;90;0;87;0
WireConnection;90;1;78;0
WireConnection;93;0;86;0
WireConnection;93;1;85;0
WireConnection;174;0;171;0
WireConnection;174;2;172;0
WireConnection;174;1;173;0
WireConnection;128;0;127;0
WireConnection;98;0;79;0
WireConnection;98;1;80;2
WireConnection;181;0;144;0
WireConnection;181;1;182;0
WireConnection;160;0;158;0
WireConnection;100;0;90;0
WireConnection;100;1;93;0
WireConnection;100;2;94;0
WireConnection;106;0;98;0
WireConnection;106;1;95;0
WireConnection;175;1;174;0
WireConnection;159;0;157;0
WireConnection;130;0;129;0
WireConnection;161;0;160;0
WireConnection;176;0;144;0
WireConnection;176;1;181;0
WireConnection;176;2;175;1
WireConnection;107;0;104;0
WireConnection;107;1;100;0
WireConnection;107;2;106;0
WireConnection;185;0;176;0
WireConnection;164;0;130;0
WireConnection;164;1;162;0
WireConnection;191;0;189;4
WireConnection;191;1;107;0
WireConnection;165;0;164;0
WireConnection;165;1;163;0
WireConnection;111;0;191;0
WireConnection;132;0;129;0
WireConnection;148;0;186;0
WireConnection;148;1;132;0
WireConnection;192;1;130;0
WireConnection;192;0;165;0
WireConnection;133;0;129;0
WireConnection;0;0;148;0
WireConnection;0;4;118;0
WireConnection;0;9;192;0
WireConnection;0;11;117;0
ASEEND*/
//CHKSM=FA6A059CC4ADE3A4E1DFFA468503168383374967