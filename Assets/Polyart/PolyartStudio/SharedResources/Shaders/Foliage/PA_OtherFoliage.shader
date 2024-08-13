// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Foliage"
{
	Properties
	{
		[Header(Base Maps)][Header(.)][SingleLineTexture]_FoliageMap("Foliage Map", 2D) = "white" {}
		[SingleLineTexture]_FlowerMask("Flower Mask", 2D) = "white" {}
		[SingleLineTexture]_GradientMap("Gradient Map", 2D) = "white" {}
		_ColorTint("Color Tint", Color) = (0,0,0,0)
		[Toggle(_USEGRADIENTMAP_ON)] _UseGradientMap("Use Gradient Map?", Float) = 0
		_ColorGradient01("Color Gradient 01", Color) = (0,0,0,0)
		_ColorGradient02("Color Gradient 02", Color) = (0,0,0,0)
		[Toggle(_HASSTEM_ON)] _HasStem("Has Stem?", Float) = 0
		_StemColor("Stem Color", Color) = (0,0,0,0)
		_SpecularIntensity("Specular Intensity", Range( -1 , 1)) = 0.1
		_FoliageRoughness("Foliage Roughness", Range( 0 , 1)) = 0.1
		_MaskClip("Mask Clip", Range( 0 , 1)) = 0.5
		[Header(Wind)][Toggle(_USEGLOBALWINDSETTINGS_ON)] _UseGlobalWindSettings("Use Global Wind Settings?", Float) = 0
		[HideInInspector]_WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_LockPositionGradient("Lock Position Gradient", Range( 0 , 10)) = 2
		_WindNoise01("Wind Noise 01", Range( 0 , 100)) = 0
		_WindNoise01Multiplier("Wind Noise 01 Multiplier", Range( -3 , 3)) = 1
		_WindNoise02("Wind Noise 02", Range( 0 , 100)) = 0
		_WindNoise02Multiplier("Wind Noise 02 Multiplier", Range( -3 , 3)) = 1
		[Header(Wind Locking)][Toggle(_USEVERTEXCOLOR_ON)] _UseVertexColor("Use Vertex Color?", Float) = 0
		_VertexColorOffset("Vertex Color Offset", Float) = 0
		_VertexColorGradient("Vertex Color Gradient", Float) = 1
		[Header(Dithering)][Toggle(_USEDITHERING_ON)] _UseDithering("Use Dithering?", Float) = 1
		[Toggle(_USEGLOBALSETTING_ON)] _UseGlobalSetting("Use Global Setting?", Float) = 0
		_DitherBottomLevel("Dither Bottom Level", Range( -10 , 10)) = 0
		_DitherFade("Dither Fade", Range( 0 , 10)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TreeTransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEGLOBALWINDSETTINGS_ON
		#pragma shader_feature_local _USEVERTEXCOLOR_ON
		#pragma shader_feature_local _HASSTEM_ON
		#pragma shader_feature_local _USEGRADIENTMAP_ON
		#pragma shader_feature _USEDITHERING_ON
		#pragma shader_feature_local _USEGLOBALSETTING_ON
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 screenPosition;
		};

		uniform sampler2D _WindNoiseTexture;
		uniform float _WindNoise01;
		uniform float WindNoise01;
		uniform float _WindNoise01Multiplier;
		uniform float WindNoise01Multiplier;
		uniform float _WindNoise02;
		uniform float WindNoise02;
		uniform float _WindNoise02Multiplier;
		uniform float WindNoise02Multiplier;
		uniform float _LockPositionGradient;
		uniform float _VertexColorOffset;
		uniform float _VertexColorGradient;
		uniform float4 _ColorTint;
		uniform float4 _ColorGradient01;
		uniform float4 _ColorGradient02;
		uniform sampler2D _GradientMap;
		uniform float4 _GradientMap_ST;
		uniform sampler2D _FoliageMap;
		uniform float4 _FoliageMap_ST;
		uniform float4 _StemColor;
		uniform sampler2D _FlowerMask;
		uniform float4 _FlowerMask_ST;
		uniform float _SpecularIntensity;
		uniform float _FoliageRoughness;
		uniform float _DitherBottomLevel;
		uniform float DitherBottomLevel;
		uniform float _DitherFade;
		uniform float DitherFade;
		uniform float _MaskClip;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 color128 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch30_g15 = WindNoise01;
			#else
				float staticSwitch30_g15 = _WindNoise01;
			#endif
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch31_g15 = WindNoise01Multiplier;
			#else
				float staticSwitch31_g15 = _WindNoise01Multiplier;
			#endif
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch33_g15 = WindNoise02;
			#else
				float staticSwitch33_g15 = _WindNoise02;
			#endif
			#ifdef _USEGLOBALWINDSETTINGS_ON
				float staticSwitch38_g15 = WindNoise02Multiplier;
			#else
				float staticSwitch38_g15 = _WindNoise02Multiplier;
			#endif
			float4 lerpResult27_g15 = lerp( ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.2 ) * _Time.y ) + ( (ase_worldPos).xz / staticSwitch30_g15 ) ), 0, 0.0) ) * staticSwitch31_g15 ) , ( tex2Dlod( _WindNoiseTexture, float4( ( ( float2( 0,0.1 ) * _Time.y ) + ( (ase_worldPos).xz / staticSwitch33_g15 ) ), 0, 0.0) ) * staticSwitch38_g15 ) , 0.5);
			Gradient gradient111 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float clampResult229 = clamp( pow( ( v.color.r + _VertexColorOffset ) , _VertexColorGradient ) , 0.0 , 1.0 );
			float vVertexColor197 = clampResult229;
			#ifdef _USEVERTEXCOLOR_ON
				float staticSwitch191 = vVertexColor197;
			#else
				float staticSwitch191 = pow( SampleGradient( gradient111, v.texcoord.xy.y ).r , _LockPositionGradient );
			#endif
			float vMovementLock248 = staticSwitch191;
			float4 lerpResult108 = lerp( color128 , lerpResult27_g15 , vMovementLock248);
			float4 vWind116 = lerpResult108;
			v.vertex.xyz += vWind116.rgb;
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_GradientMap = i.uv_texcoord * _GradientMap_ST.xy + _GradientMap_ST.zw;
			float4 lerpResult215 = lerp( _ColorGradient01 , _ColorGradient02 , tex2D( _GradientMap, uv_GradientMap ).r);
			#ifdef _USEGRADIENTMAP_ON
				float4 staticSwitch211 = lerpResult215;
			#else
				float4 staticSwitch211 = _ColorTint;
			#endif
			float2 uv_FoliageMap = i.uv_texcoord * _FoliageMap_ST.xy + _FoliageMap_ST.zw;
			float4 tex2DNode22 = tex2D( _FoliageMap, uv_FoliageMap );
			float4 temp_output_221_0 = ( staticSwitch211 * tex2DNode22 );
			float2 uv_FlowerMask = i.uv_texcoord * _FlowerMask_ST.xy + _FlowerMask_ST.zw;
			float4 lerpResult224 = lerp( temp_output_221_0 , ( _StemColor * tex2DNode22 ) , ( 1.0 - tex2D( _FlowerMask, uv_FlowerMask ).r ));
			#ifdef _HASSTEM_ON
				float4 staticSwitch226 = lerpResult224;
			#else
				float4 staticSwitch226 = temp_output_221_0;
			#endif
			float4 vColor27 = staticSwitch226;
			o.Albedo = vColor27.rgb;
			float3 temp_cast_1 = (_SpecularIntensity).xxx;
			o.Specular = temp_cast_1;
			o.Smoothness = ( 1.0 - _FoliageRoughness );
			o.Alpha = 1;
			float vAlpha125 = tex2DNode22.a;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen8_g14 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither8_g14 = Dither4x4Bayer( fmod(clipScreen8_g14.x, 4), fmod(clipScreen8_g14.y, 4) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			#ifdef _USEGLOBALSETTING_ON
				float staticSwitch12_g14 = DitherBottomLevel;
			#else
				float staticSwitch12_g14 = _DitherBottomLevel;
			#endif
			#ifdef _USEGLOBALSETTING_ON
				float staticSwitch11_g14 = DitherFade;
			#else
				float staticSwitch11_g14 = _DitherFade;
			#endif
			dither8_g14 = step( dither8_g14, saturate( ( ( ase_vertex3Pos.y + staticSwitch12_g14 ) * ( staticSwitch11_g14 * 2 ) ) ) );
			float vTerrainDither181 = dither8_g14;
			#ifdef _USEDITHERING_ON
				float staticSwitch182 = ( vAlpha125 * vTerrainDither181 );
			#else
				float staticSwitch182 = vAlpha125;
			#endif
			clip( staticSwitch182 - _MaskClip );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;121;-3012.576,247.0363;Inherit;False;1640.88;767.5203;;14;248;191;197;113;229;112;114;110;194;111;192;195;193;190;Vertical Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;190;-2890.795,685.3215;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;193;-2932.179,851.8886;Inherit;False;Property;_VertexColorOffset;Vertex Color Offset;21;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;31;-4150.297,-1537.111;Inherit;False;2856.68;1171.779;Comment;21;27;226;227;224;125;219;228;225;223;220;221;211;222;22;19;215;218;214;217;213;216;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;216;-4131.25,-865.4424;Inherit;True;Property;_GradientMap;Gradient Map;2;2;[Header];[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;192;-2632.179,707.8886;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-2731.179,848.8886;Inherit;False;Property;_VertexColorGradient;Vertex Color Gradient;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;194;-2480.179,707.8886;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;217;-3900.25,-866.4424;Inherit;True;Property;_TextureSample1;Texture Sample 1;20;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientNode;111;-2825.649,394.9997;Inherit;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;110;-2858.648,470.0004;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;19;-3329.606,-690.9492;Inherit;True;Property;_FoliageMap;Foliage Map;0;2;[Header];[SingleLineTexture];Create;True;2;Base Maps;.;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.LerpOp;215;-3452.25,-1060.442;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;229;-2301.943,708.2016;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-2577.692,581.2943;Inherit;False;Property;_LockPositionGradient;Lock Position Gradient;15;0;Create;True;0;0;0;False;0;False;2;0.95;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;112;-2605.65,394.9997;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-3089.33,-692.188;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;211;-3215.922,-1088.619;Inherit;False;Property;_UseGradientMap;Use Gradient Map?;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;113;-2256.692,394.2938;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-2166.179,702.8885;Inherit;False;vVertexColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;222;-3020.21,-1294.647;Inherit;True;Property;_FlowerMask;Flower Mask;1;2;[Header];[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;167;-2637.903,-1854.358;Inherit;False;844.5669;250.5412;Dithering;3;181;168;170;;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;191;-1953.262,390.2216;Inherit;False;Property;_UseVertexColor;Use Vertex Color?;20;0;Create;True;0;0;0;False;1;Header(Wind Locking);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;221;-2641.409,-1083.02;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;184;-2655.449,-342.8354;Inherit;False;1357.315;571.7289;Comment;9;249;116;108;189;128;186;188;187;185;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;168;-2611.753,-1790.539;Inherit;False;Property;_DitherBottomLevel;Dither Bottom Level;26;0;Create;True;0;0;0;False;0;False;0;0;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;223;-2770.146,-1296.961;Inherit;True;Property;_TextureSample2;Texture Sample 2;24;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;170;-2613.136,-1709.91;Inherit;False;Property;_DitherFade;Dither Fade;27;0;Create;True;0;0;0;False;0;False;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;248;-1678.48,388.7944;Inherit;False;vMovementLock;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;188;-2578.669,60.47952;Inherit;False;Property;_WindNoise02Multiplier;Wind Noise 02 Multiplier;19;0;Create;True;0;0;0;False;0;False;1;0;-3;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-2579.719,-167.3194;Inherit;False;Property;_WindNoise01;Wind Noise 01;16;0;Create;True;0;0;0;False;0;False;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-2578.669,-15.52011;Inherit;False;Property;_WindNoise01Multiplier;Wind Noise 01 Multiplier;17;0;Create;True;0;0;0;False;0;False;1;0;-3;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-2578.669,-92.51978;Inherit;False;Property;_WindNoise02;Wind Noise 02;18;0;Create;True;0;0;0;False;0;False;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;228;-2429.212,-1111.851;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;219;-2641.715,-711.2777;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;210;-2324.16,-1785.528;Inherit;False;PA_Dithering;24;;14;51907001cc2d98f4bb2ac7260d1fff6c;0;2;9;FLOAT;0;False;10;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;225;-2425.212,-1214.851;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;224;-2150.449,-1082.561;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-2647.224,-593.9875;Inherit;False;vAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;181;-2021.951,-1790.021;Inherit;False;vTerrainDither;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;227;-1979.212,-1117.851;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;189;-2246.504,-87.7447;Inherit;False;PA_SF_WindGrass;12;;15;b1ba21c96122aa44f8eac64c8d72c027;0;4;29;FLOAT;10;False;34;FLOAT;10;False;32;FLOAT;1;False;39;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;-2118.896,62.90772;Inherit;False;248;vMovementLock;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;128;-2126.239,-292.8353;Inherit;False;Constant;_Color2;Color2;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;226;-1882.212,-1112.851;Inherit;False;Property;_HasStem;Has Stem?;7;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;108;-1678.743,-109.548;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-1325.317,375.0651;Inherit;False;181;vTerrainDither;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-1294.043,271.5366;Inherit;False;125;vAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1595.453,-1112.758;Inherit;False;vColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-958.636,356.289;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-1498.531,-113.9009;Inherit;False;vWind;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-790.3269,-84.21375;Inherit;False;Property;_MaskClip;Mask Clip;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-786.9415,32.68401;Inherit;False;27;vColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-690.1942,382.9964;Inherit;False;116;vWind;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;182;-738.8497,273.6953;Inherit;False;Property;_UseDithering;Use Dithering?;23;0;Create;True;0;0;0;False;1;Header(Dithering);False;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-447.4691,37.94351;Float;False;True;-1;2;;0;0;StandardSpecular;Polyart/Dreamscape/Builtin/Foliage;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.59;True;True;0;True;TreeTransparentCutout;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;True;_MaskClip;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.OneMinusNode;250;-670.7087,190.7914;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;162;-940.6069,185.5835;Inherit;False;Property;_FoliageRoughness;Foliage Roughness;10;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;251;-946.0657,102.9006;Inherit;False;Property;_SpecularIntensity;Specular Intensity;9;0;Create;True;0;0;0;False;0;False;0.1;0.1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;220;-2998.824,-932.0966;Inherit;False;Property;_StemColor;Stem Color;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;218;-3511.649,-1281.73;Inherit;False;Property;_ColorTint;Color Tint;3;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;213;-3815.25,-1225.443;Inherit;False;Property;_ColorGradient01;Color Gradient 01;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;214;-3813.25,-1042.442;Inherit;False;Property;_ColorGradient02;Color Gradient 02;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;192;0;190;1
WireConnection;192;1;193;0
WireConnection;194;0;192;0
WireConnection;194;1;195;0
WireConnection;217;0;216;0
WireConnection;215;0;213;0
WireConnection;215;1;214;0
WireConnection;215;2;217;1
WireConnection;229;0;194;0
WireConnection;112;0;111;0
WireConnection;112;1;110;2
WireConnection;22;0;19;0
WireConnection;211;1;218;0
WireConnection;211;0;215;0
WireConnection;113;0;112;1
WireConnection;113;1;114;0
WireConnection;197;0;229;0
WireConnection;191;1;113;0
WireConnection;191;0;197;0
WireConnection;221;0;211;0
WireConnection;221;1;22;0
WireConnection;223;0;222;0
WireConnection;248;0;191;0
WireConnection;228;0;221;0
WireConnection;219;0;220;0
WireConnection;219;1;22;0
WireConnection;210;9;168;0
WireConnection;210;10;170;0
WireConnection;225;0;223;1
WireConnection;224;0;221;0
WireConnection;224;1;219;0
WireConnection;224;2;225;0
WireConnection;125;0;22;4
WireConnection;181;0;210;0
WireConnection;227;0;228;0
WireConnection;189;29;185;0
WireConnection;189;34;187;0
WireConnection;189;32;186;0
WireConnection;189;39;188;0
WireConnection;226;1;227;0
WireConnection;226;0;224;0
WireConnection;108;0;128;0
WireConnection;108;1;189;0
WireConnection;108;2;249;0
WireConnection;27;0;226;0
WireConnection;161;0;126;0
WireConnection;161;1;158;0
WireConnection;116;0;108;0
WireConnection;182;1;126;0
WireConnection;182;0;161;0
WireConnection;0;0;29;0
WireConnection;0;3;251;0
WireConnection;0;4;250;0
WireConnection;0;10;182;0
WireConnection;0;11;122;0
WireConnection;250;0;162;0
ASEEND*/
//CHKSM=B9BD4230AA68FE01CBD8456D0EA1967871990A4B