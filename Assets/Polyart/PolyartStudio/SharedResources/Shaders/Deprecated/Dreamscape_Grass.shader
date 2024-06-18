// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape Grass"
{
	Properties
	{
		_ColorTop("Color Top", Color) = (0,0,0,0)
		_Cutoff( "Mask Clip Value", Float ) = 0.59
		_ColorTopVariation("Color Top Variation", Color) = (0,0,0,0)
		_ColorBottom("Color Bottom", Color) = (0,0,0,0)
		_ColorBottomLevel("Color Bottom Level", Float) = 0
		_ColorBottomMaskFade("Color Bottom Mask Fade", Range( -1 , 1)) = 0
		_MainTex("Foliage Texture", 2D) = "white" {}
		_WaveScale("Wave Scale", Float) = 33
		_VariationMapScale("Variation Map Scale", Float) = 15
		_PanningWaveTexture("Panning Wave Texture", 2D) = "white" {}
		_VariationMask("Variation Mask", 2D) = "white" {}
		_WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_PivotLockPower("Pivot Lock Power", Range( 0 , 10)) = 2
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
		uniform float4 _ColorTop;
		uniform float4 _ColorTopVariation;
		uniform sampler2D _VariationMask;
		uniform float _VariationMapScale;
		uniform float4 _ColorBottom;
		uniform float _ColorBottomLevel;
		uniform float _ColorBottomMaskFade;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _PanningWaveTexture;
		uniform float2 CloudSpeed;
		uniform float _WaveScale;
		uniform float FoliageSmoothness;
		uniform float DitherBottomLevel;
		uniform float DitherFade;
		uniform float FoliageRenderDistance;
		uniform float _Cutoff = 0.59;


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
			float4 color128 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 lerpResult107 = lerp( ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.2 ) * _Time.y ) + ( (ase_worldPos).xz / WindNoiseSmall ) ), 0, 0.0) ) * WindNoiseSmallMultiply ) , ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.1 ) * _Time.y ) + ( (ase_worldPos).xz / WindNoiseLarge ) ), 0, 0.0) ) * WindNoiseLargeMultiply ) , 0.5);
			Gradient gradient111 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 temp_cast_0 = (_PivotLockPower).xxxx;
			float4 lerpResult108 = lerp( color128 , lerpResult107 , pow( SampleGradient( gradient111, v.texcoord.xy.y ) , temp_cast_0 ));
			float4 vWind116 = lerpResult108;
			v.vertex.xyz += vWind116.rgb;
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float4 lerpResult132 = lerp( _ColorTop , _ColorTopVariation , tex2D( _VariationMask, (( ase_worldPos / _VariationMapScale )).xz ));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 lerpResult23 = lerp( lerpResult132 , _ColorBottom , saturate( ( ( ase_vertex3Pos.y + _ColorBottomLevel ) * ( _ColorBottomMaskFade * 2 ) ) ));
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode22 = tex2D( _MainTex, uv_MainTex );
			float4 vColor27 = ( lerpResult23 * tex2DNode22 );
			float2 panner5 = ( _Time.y * (CloudSpeed).xy + (( ase_worldPos / _WaveScale )).xz);
			float4 lerpResult9 = lerp( ( CloudColor * vColor27 ) , vColor27 , tex2D( _PanningWaveTexture, panner5 ));
			o.Albedo = lerpResult9.rgb;
			o.Smoothness = FoliageSmoothness;
			o.Alpha = 1;
			float vAlpha125 = tex2DNode22.a;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen179 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither179 = Dither8x8Bayer( fmod(clipScreen179.x, 8), fmod(clipScreen179.y, 8) );
			dither179 = step( dither179, saturate( ( ( ase_vertex3Pos.y + DitherBottomLevel ) * ( DitherFade * 2 ) ) ) );
			float vTerrainDither181 = dither179;
			float2 clipScreen177 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither177 = Dither8x8Bayer( fmod(clipScreen177.x, 8), fmod(clipScreen177.y, 8) );
			float cameraDepthFade174 = (( i.eyeDepth -_ProjectionParams.y - FoliageRenderDistance ) / FoliageRenderDistance);
			dither177 = step( dither177, ( 1.0 - cameraDepthFade174 ) );
			float vGrassDistance180 = dither177;
			#ifdef _DITHERINGON_ON
				float staticSwitch182 = ( ( vAlpha125 * vTerrainDither181 ) * vGrassDistance180 );
			#else
				float staticSwitch182 = vAlpha125;
			#endif
			clip( staticSwitch182 - _Cutoff );
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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
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
Version=18912
1038;114;2305;1679;2232.901;590.0575;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;118;-3534.384,600.8654;Inherit;False;2133.215;1279.717;;2;119;120;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;31;-4292.896,-1614.675;Inherit;False;2888.247;1209.859;Comment;22;125;27;24;22;23;132;20;21;19;18;135;17;139;15;16;13;14;12;140;138;136;137;Cloud Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;119;-3488.573,651.1088;Inherit;False;1890.47;670.058;Wind Small;14;107;93;115;94;92;91;85;88;84;82;87;89;83;86;;0.2216981,1,0.9921367,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;120;-3488.248,1339.114;Inherit;False;1598.144;507.6038;Wind Large;11;95;106;96;98;100;101;99;97;103;102;105;;0.9711054,0.495283,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;167;-4281.697,-2269.118;Inherit;False;2026.637;461.4008;Dithering;14;181;180;179;178;177;176;175;174;173;172;171;170;169;168;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;136;-4222.973,-1288.616;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;137;-4227.179,-1147.221;Inherit;False;Property;_VariationMapScale;Variation Map Scale;8;0;Create;True;0;0;0;False;0;False;15;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;171;-4189.463,-2164.885;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;168;-4213.547,-2018.298;Inherit;False;Global;DitherBottomLevel;Dither Bottom Level;10;0;Create;True;0;0;0;False;0;False;0;-0.06;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-4214.931,-1937.669;Inherit;False;Global;DitherFade;Dither Fade;12;0;Create;True;0;0;0;False;0;False;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;14;-3968.577,-1084.494;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;138;-3904.867,-1289.322;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;105;-3433.905,1650.35;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;12;-3994.044,-857.2758;Inherit;False;Property;_ColorBottomMaskFade;Color Bottom Mask Fade;5;0;Create;True;0;0;0;False;0;False;0;-1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;86;-3438.573,962.7123;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;13;-3969.661,-941.9063;Inherit;False;Property;_ColorBottomLevel;Color Bottom Level;4;0;Create;True;0;0;0;False;0;False;0;-0.66;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;87;-3238.573,957.7123;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleNode;16;-3726.58,-855.3986;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;173;-3915.352,-2068.388;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;139;-3764.365,-1293.622;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-3694.467,-987.9973;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-3236.905,1731.35;Inherit;False;Global;WindNoiseLarge;Wind Noise Large;16;0;Create;True;0;0;0;False;0;False;20;28.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;103;-3291.089,1411.959;Inherit;False;Constant;_Vector1;Vector 1;14;0;Create;True;0;0;0;False;0;False;0,0.1;0,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ScaleNode;172;-3947.465,-1935.792;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;102;-3293.089,1540.96;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-3241.573,1043.712;Inherit;False;Global;WindNoiseSmall;Wind Noise Small;17;0;Create;True;0;0;0;False;0;False;20;59.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;99;-3233.905,1647.35;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;83;-3298.758,853.3222;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;82;-3295.758,724.322;Inherit;False;Constant;_Vector0;Vector 0;15;0;Create;True;0;0;0;False;0;False;0,0.2;0,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-3545.302,-992.0851;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-3245.842,-1508.249;Inherit;False;Property;_ColorTop;Color Top;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.4353558,0.5849056,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-3060.089,1416.959;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;88;-2943.573,964.7123;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;140;-3552.013,-1315.248;Inherit;True;Property;_VariationMask;Variation Mask;10;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-3756.187,-2068.476;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-3056.758,730.322;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;169;-3494.059,-2194.117;Inherit;False;Global;FoliageRenderDistance;FoliageRenderDistance;9;0;Create;True;0;0;0;True;0;False;35;90.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;100;-2958.905,1648.35;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;135;-3259.871,-1339.357;Inherit;False;Property;_ColorTopVariation;Color Top Variation;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.4353558,0.5849056,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-2733.758,730.322;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;178;-3460.507,-2071.71;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-2729.089,1417.959;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CameraDepthFade;174;-3106.056,-2214.117;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;132;-2990.218,-1355.477;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;21;-3239.622,-991.3195;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;-3243.549,-1168.554;Inherit;False;Property;_ColorBottom;Color Bottom;3;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5454459,0.8018868,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;19;-3101.654,-874.4417;Inherit;True;Property;_MainTex;Foliage Texture;6;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;121;-542.226,799.1144;Inherit;False;912.9565;352;;5;113;114;112;111;110;Vertical Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;91;-2946.567,1128.167;Inherit;True;Property;_WindNoiseTexture;Wind Noise Texture;11;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;32;-3062.589,-281.0117;Inherit;False;1650.299;865.0283;;12;141;9;144;29;11;5;6;8;2;3;4;1;Color;0.6900728,1,0.5801887,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;175;-2835.052,-2215.117;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;179;-3262.752,-2076.78;Inherit;False;1;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;22;-2874.177,-871.6806;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-2929.218,268.7306;Inherit;False;Property;_WaveScale;Wave Scale;7;0;Create;True;0;0;0;False;0;False;33;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;-2743.551,-1033.554;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-2941.219,122.7303;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;96;-2450.918,1388.745;Inherit;True;Property;_TextureSample2;Texture Sample 2;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;92;-2455.587,701.1088;Inherit;True;Property;_TextureSample1;Texture Sample 1;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;106;-2421.761,1584.05;Inherit;False;Global;WindNoiseLargeMultiply;Wind Noise Large Multiply;15;0;Create;True;0;0;0;False;0;False;1;0.91;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;111;-459.2276,849.8204;Inherit;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-2433.429,892.412;Inherit;False;Global;WindNoiseSmallMultiply;Wind Noise Small Multiply;14;0;Create;True;0;0;0;False;0;False;0;0.13;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;110;-492.2264,924.8204;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-2526.449,-774.5059;Inherit;False;vAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;177;-2667.052,-2219.117;Inherit;False;1;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-211.2705,1036.114;Inherit;False;Property;_PivotLockPower;Pivot Lock Power;12;0;Create;True;0;0;0;False;0;False;2;0.95;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;112;-239.2285,849.8204;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;181;-3075.754,-2077.78;Inherit;False;vTerrainDither;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;3;-2687.218,121.7302;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2114.051,-1030.384;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-2051.989,839.6182;Inherit;False;Constant;_Float1;Float 1;19;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;166;-3270.14,373.4932;Inherit;False;Global;CloudSpeed;CloudSpeed;14;0;Create;True;0;0;0;False;0;False;0.2,1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-2059.43,705.412;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2054.76,1393.049;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-1307.373,268.4425;Inherit;False;125;vAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;2;-2542.217,116.7302;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1892.85,-1033.322;Inherit;False;vColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;113;109.7293,849.1144;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-1331.458,343.3125;Inherit;False;181;vTerrainDither;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;180;-2482.056,-2219.117;Inherit;False;vGrassDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;107;-1782.104,707.8413;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-2654.919,478.3934;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;8;-2692.823,374.1942;Inherit;False;True;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;128;72.72441,579.4537;Inherit;False;Constant;_Color2;Color2;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;5;-2265.02,120.3936;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-1955.227,-16.30511;Inherit;False;27;vColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;141;-1995.732,-189.4865;Inherit;False;Global;CloudColor;CloudColor;14;0;Create;True;0;0;0;True;0;False;0.6792453,0.6792453,0.6792453,0;0.4716981,0.4716981,0.4716981,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;159;-1338.458,421.3125;Inherit;False;180;vGrassDistance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;108;389.4157,682.4407;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-1081.458,357.3125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;568.6276,677.0878;Inherit;False;vWind;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;11;-2072.263,89.90434;Inherit;True;Property;_PanningWaveTexture;Panning Wave Texture;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;160;-920.458,400.3125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;-1753.424,-184.4438;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;182;-747.0377,272.6717;Inherit;False;Property;_DitheringON;Dithering ON;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-796.1942,568.9964;Inherit;False;116;vWind;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;162;-879.6069,127.5835;Inherit;False;Global;FoliageSmoothness;FoliageSmoothness;0;0;Create;True;0;0;0;False;0;False;0.1;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-1609.263,46.90417;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-447.4691,37.94351;Float;False;True;-1;4;;0;0;Standard;Polyart/Dreamscape Grass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.59;True;True;0;True;Opaque;;AlphaTest;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;138;0;136;0
WireConnection;138;1;137;0
WireConnection;87;0;86;0
WireConnection;16;0;12;0
WireConnection;173;0;171;2
WireConnection;173;1;168;0
WireConnection;139;0;138;0
WireConnection;15;0;14;2
WireConnection;15;1;13;0
WireConnection;172;0;170;0
WireConnection;99;0;105;0
WireConnection;17;0;15;0
WireConnection;17;1;16;0
WireConnection;101;0;103;0
WireConnection;101;1;102;0
WireConnection;88;0;87;0
WireConnection;88;1;89;0
WireConnection;140;1;139;0
WireConnection;176;0;173;0
WireConnection;176;1;172;0
WireConnection;84;0;82;0
WireConnection;84;1;83;0
WireConnection;100;0;99;0
WireConnection;100;1;97;0
WireConnection;85;0;84;0
WireConnection;85;1;88;0
WireConnection;178;0;176;0
WireConnection;98;0;101;0
WireConnection;98;1;100;0
WireConnection;174;0;169;0
WireConnection;174;1;169;0
WireConnection;132;0;18;0
WireConnection;132;1;135;0
WireConnection;132;2;140;0
WireConnection;21;0;17;0
WireConnection;175;0;174;0
WireConnection;179;0;178;0
WireConnection;22;0;19;0
WireConnection;23;0;132;0
WireConnection;23;1;20;0
WireConnection;23;2;21;0
WireConnection;96;0;91;0
WireConnection;96;1;98;0
WireConnection;92;0;91;0
WireConnection;92;1;85;0
WireConnection;125;0;22;4
WireConnection;177;0;175;0
WireConnection;112;0;111;0
WireConnection;112;1;110;2
WireConnection;181;0;179;0
WireConnection;3;0;1;0
WireConnection;3;1;4;0
WireConnection;24;0;23;0
WireConnection;24;1;22;0
WireConnection;93;0;92;0
WireConnection;93;1;94;0
WireConnection;95;0;96;0
WireConnection;95;1;106;0
WireConnection;2;0;3;0
WireConnection;27;0;24;0
WireConnection;113;0;112;0
WireConnection;113;1;114;0
WireConnection;180;0;177;0
WireConnection;107;0;93;0
WireConnection;107;1;95;0
WireConnection;107;2;115;0
WireConnection;8;0;166;0
WireConnection;5;0;2;0
WireConnection;5;2;8;0
WireConnection;5;1;6;0
WireConnection;108;0;128;0
WireConnection;108;1;107;0
WireConnection;108;2;113;0
WireConnection;161;0;126;0
WireConnection;161;1;158;0
WireConnection;116;0;108;0
WireConnection;11;1;5;0
WireConnection;160;0;161;0
WireConnection;160;1;159;0
WireConnection;144;0;141;0
WireConnection;144;1;29;0
WireConnection;182;1;126;0
WireConnection;182;0;160;0
WireConnection;9;0;144;0
WireConnection;9;1;29;0
WireConnection;9;2;11;0
WireConnection;0;0;9;0
WireConnection;0;4;162;0
WireConnection;0;10;182;0
WireConnection;0;11;122;0
ASEEND*/
//CHKSM=4337467B9B5A439F23C90A2E8335D59739A23129