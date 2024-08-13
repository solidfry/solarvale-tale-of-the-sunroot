// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Water River"
{
	Properties
	{
		[Header(Maps)][Header(.)][SingleLineTexture]_FlowMask("Flow Mask", 2D) = "white" {}
		[SingleLineTexture]_FoamMask("Foam Mask", 2D) = "white" {}
		[Normal][SingleLineTexture]_NormalMap("Normal Map", 2D) = "bump" {}
		[SingleLineTexture]_HeightMap("Height Map", 2D) = "white" {}
		_WaterFoamColor("Water Foam Color", Color) = (1,1,1,1)
		_WaterFoamColorIntensity("Water Foam Color Intensity", Float) = 1
		[Header(Color)]_ColorShallow("Color Shallow", Color) = (0.5990566,0.9091429,1,0)
		_ColorDeep("Color Deep", Color) = (0.1213065,0.347919,0.5471698,0)
		_ColorDepthFade("Color Depth Fade", Range( 0 , 100)) = 0
		[Header(Refraction)]_RefractionStrength("Refraction Strength", Range( 0 , 10)) = 1
		_RefractionDepth("Refraction Depth", Range( 0 , 100)) = 1
		[Header(Edge Foam)]_EdgeFoamSpeed("Edge Foam Speed", Vector) = (0,0,0,0)
		_EdgeFoamScale("Edge Foam Scale", Range( 0 , 10)) = 1
		_EdgeFoamDepthFade("Edge Foam Depth Fade", Range( 0 , 10)) = 0.19
		_EdgeFoamIntensity("Edge Foam Intensity", Range( 0 , 100)) = 1
		_EdgeFoamCutoff("Edge Foam Cutoff", Range( 0 , 5)) = 1
		[Header(Foam)]_FoamUVTiling("Foam UV Tiling", Vector) = (1,1,0,0)
		_FoamSpeed("Foam Speed", Vector) = (1,1,0,0)
		_FoamIntensity("Foam Intensity", Range( 0 , 100)) = 1
		_FoamPower("Foam Power", Range( 0 , 20)) = 1
		_FoamRandomizeUV("Foam Randomize UV", Range( 0 , 1)) = 0.26
		[Header(Flow)]_FlowUVTiling("Flow UV Tiling", Vector) = (1,1,0,0)
		_FlowSpeed("Flow Speed", Vector) = (0,-0.2,0,0)
		_FlowIntensity("Flow Intensity", Range( 0 , 100)) = 1
		_FlowPower("Flow Power", Range( 0 , 20)) = 1
		_FlowRandomizeUV("Flow Randomize UV", Range( 0 , 1)) = 0.26
		[Header(Normal)]_NormalUVTiling("Normal UV Tiling", Vector) = (1,1,0,0)
		_NormalSpeed("Normal Speed", Vector) = (0,-0.2,0,0)
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 0.5
		_NormalRandomizeUV("Normal Randomize UV", Range( 0 , 1)) = 0.26
		[Header(Displacement)]_HeightUVTiling("Height UV Tiling", Vector) = (1,1,0,0)
		_HeightSpeed("Height Speed", Vector) = (1,1,0,0)
		_HeightIntensity("Height Intensity", Float) = 1
		_HeightPower("Height  Power", Float) = 1
		_HeightRandomizeUV("Height Randomize UV", Range( 0 , 1)) = 0.26
		_Metallic("Metallic", Range( 0 , 1)) = 0.2
		_Roughness("Roughness", Range( 0 , 1)) = 0.05
		[Toggle(_USESTEPFORFLOW_ON)] _useStepforFlow("use Step for Flow?", Float) = 0
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#pragma target 4.6
		#pragma shader_feature_local _USESTEPFORFLOW_ON
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldNormal;
			INTERNAL_DATA
			float eyeDepth;
		};

		uniform float _HeightIntensity;
		uniform sampler2D _HeightMap;
		uniform float _HeightRandomizeUV;
		uniform float2 _HeightSpeed;
		uniform float2 _HeightUVTiling;
		uniform float _HeightPower;
		uniform sampler2D _NormalMap;
		uniform float _NormalRandomizeUV;
		uniform float2 _NormalSpeed;
		uniform float2 _NormalUVTiling;
		uniform float _NormalIntensity;
		uniform float4 _ColorShallow;
		uniform float4 _ColorDeep;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _ColorDepthFade;
		uniform float4 _WaterFoamColor;
		uniform float _WaterFoamColorIntensity;
		uniform float _EdgeFoamDepthFade;
		uniform float _EdgeFoamCutoff;
		uniform float2 _EdgeFoamSpeed;
		uniform float _EdgeFoamScale;
		uniform float _EdgeFoamIntensity;
		uniform float _FlowIntensity;
		uniform sampler2D _FlowMask;
		uniform float _FlowRandomizeUV;
		uniform float2 _FlowSpeed;
		uniform float2 _FlowUVTiling;
		uniform float _FlowPower;
		uniform float _FoamIntensity;
		uniform sampler2D _FoamMask;
		uniform float _FoamRandomizeUV;
		uniform float2 _FoamSpeed;
		uniform float2 _FoamUVTiling;
		uniform float _FoamPower;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractionStrength;
		uniform float _RefractionDepth;
		uniform float _Metallic;
		uniform float _Roughness;
		uniform float _Opacity;


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float temp_output_7_0_g58 = ( _HeightRandomizeUV * _Time.y );
			float2 panner7_g57 = ( _Time.y * _HeightSpeed + ( _HeightUVTiling * v.texcoord.xy ));
			float2 temp_output_2_0_g58 = panner7_g57;
			float2 panner9_g58 = ( temp_output_7_0_g58 * float2( 0.1,0.1 ) + temp_output_2_0_g58);
			float2 panner13_g58 = ( temp_output_7_0_g58 * float2( -0.1,-0.1 ) + ( temp_output_2_0_g58 + float2( 0.45,0.3 ) ));
			float2 panner18_g58 = ( temp_output_7_0_g58 * float2( -0.1,0.1 ) + ( temp_output_2_0_g58 + float2( 0.85,0.14 ) ));
			float4 temp_cast_0 = (_HeightPower).xxxx;
			float3 ase_vertexNormal = v.normal.xyz;
			float4 vDisplacement160 = ( ( _HeightIntensity * pow( ( ( ( tex2Dlod( _HeightMap, float4( panner9_g58, 0, 0.0) ) + tex2Dlod( _HeightMap, float4( panner13_g58, 0, 0.0) ) ) + tex2Dlod( _HeightMap, float4( panner18_g58, 0, 0.0) ) ) * 0.25 ) , temp_cast_0 ) ) * float4( ase_vertexNormal , 0.0 ) );
			v.vertex.xyz += vDisplacement160.rgb;
			v.vertex.w = 1;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_7_0_g60 = ( _NormalRandomizeUV * _Time.y );
			float2 panner7_g59 = ( _Time.y * _NormalSpeed + ( _NormalUVTiling * i.uv_texcoord ));
			float2 temp_output_2_0_g60 = panner7_g59;
			float2 panner9_g60 = ( temp_output_7_0_g60 * float2( 0.1,0.1 ) + temp_output_2_0_g60);
			float temp_output_24_0_g60 = _NormalIntensity;
			float2 panner13_g60 = ( temp_output_7_0_g60 * float2( -0.1,-0.1 ) + ( temp_output_2_0_g60 + float2( 0.45,0.3 ) ));
			float2 panner18_g60 = ( temp_output_7_0_g60 * float2( -0.1,0.1 ) + ( temp_output_2_0_g60 + float2( 0.85,0.14 ) ));
			float3 vNormal154 = BlendNormals( BlendNormals( UnpackScaleNormal( tex2D( _NormalMap, panner9_g60 ), temp_output_24_0_g60 ) , UnpackScaleNormal( tex2D( _NormalMap, panner13_g60 ), temp_output_24_0_g60 ) ) , UnpackScaleNormal( tex2D( _NormalMap, panner18_g60 ), temp_output_24_0_g60 ) );
			o.Normal = vNormal154;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth65 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth65 = abs( ( screenDepth65 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _ColorDepthFade ) );
			float4 lerpResult69 = lerp( _ColorShallow , _ColorDeep , saturate( distanceDepth65 ));
			float temp_output_7_0_g58 = ( _HeightRandomizeUV * _Time.y );
			float2 panner7_g57 = ( _Time.y * _HeightSpeed + ( _HeightUVTiling * i.uv_texcoord ));
			float2 temp_output_2_0_g58 = panner7_g57;
			float2 panner9_g58 = ( temp_output_7_0_g58 * float2( 0.1,0.1 ) + temp_output_2_0_g58);
			float2 panner13_g58 = ( temp_output_7_0_g58 * float2( -0.1,-0.1 ) + ( temp_output_2_0_g58 + float2( 0.45,0.3 ) ));
			float2 panner18_g58 = ( temp_output_7_0_g58 * float2( -0.1,0.1 ) + ( temp_output_2_0_g58 + float2( 0.85,0.14 ) ));
			float4 temp_cast_0 = (_HeightPower).xxxx;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float4 vDisplacement160 = ( ( _HeightIntensity * pow( ( ( ( tex2D( _HeightMap, panner9_g58 ) + tex2D( _HeightMap, panner13_g58 ) ) + tex2D( _HeightMap, panner18_g58 ) ) * 0.25 ) , temp_cast_0 ) ) * float4( ase_vertexNormal , 0.0 ) );
			float4 lerpResult72 = lerp( lerpResult69 , ( lerpResult69 + float4( 0.5,0.5,0.5,0 ) ) , vDisplacement160);
			float4 vColorWaves73 = lerpResult72;
			float4 vFoamColor169 = ( _WaterFoamColor * _WaterFoamColorIntensity );
			float screenDepth9_g53 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth9_g53 = abs( ( screenDepth9_g53 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeFoamDepthFade ) );
			float mulTime4_g53 = _Time.y * 0.1;
			float2 temp_cast_2 = (_EdgeFoamScale).xx;
			float2 uv_TexCoord2_g53 = i.uv_texcoord * temp_cast_2;
			float2 panner6_g53 = ( mulTime4_g53 * _EdgeFoamSpeed + uv_TexCoord2_g53);
			float simpleNoise13_g53 = SimpleNoise( panner6_g53*50.0 );
			float temp_output_22_0_g53 = ( step( ( saturate( distanceDepth9_g53 ) * _EdgeFoamCutoff ) , simpleNoise13_g53 ) * _EdgeFoamIntensity );
			float vEdgeFoam167 = temp_output_22_0_g53;
			float temp_output_7_0_g52 = ( _FlowRandomizeUV * _Time.y );
			float2 panner7_g51 = ( _Time.y * _FlowSpeed + ( _FlowUVTiling * i.uv_texcoord ));
			float2 temp_output_2_0_g52 = panner7_g51;
			float2 panner9_g52 = ( temp_output_7_0_g52 * float2( 0.1,0.1 ) + temp_output_2_0_g52);
			float2 panner13_g52 = ( temp_output_7_0_g52 * float2( -0.1,-0.1 ) + ( temp_output_2_0_g52 + float2( 0.45,0.3 ) ));
			float2 panner18_g52 = ( temp_output_7_0_g52 * float2( -0.1,0.1 ) + ( temp_output_2_0_g52 + float2( 0.85,0.14 ) ));
			float4 temp_cast_3 = (_FlowPower).xxxx;
			float4 vFlow168 = ( _FlowIntensity * pow( ( ( ( tex2D( _FlowMask, panner9_g52 ) + tex2D( _FlowMask, panner13_g52 ) ) + tex2D( _FlowMask, panner18_g52 ) ) * 0.25 ) , temp_cast_3 ) );
			float4 temp_output_37_0 = ( vEdgeFoam167 + vFlow168 );
			float temp_output_7_0_g56 = ( _FoamRandomizeUV * _Time.y );
			float2 panner7_g55 = ( _Time.y * _FoamSpeed + ( _FoamUVTiling * i.uv_texcoord ));
			float2 temp_output_2_0_g56 = panner7_g55;
			float2 panner9_g56 = ( temp_output_7_0_g56 * float2( 0.1,0.1 ) + temp_output_2_0_g56);
			float2 panner13_g56 = ( temp_output_7_0_g56 * float2( -0.1,-0.1 ) + ( temp_output_2_0_g56 + float2( 0.45,0.3 ) ));
			float2 panner18_g56 = ( temp_output_7_0_g56 * float2( -0.1,0.1 ) + ( temp_output_2_0_g56 + float2( 0.85,0.14 ) ));
			float4 temp_cast_4 = (_FoamPower).xxxx;
			float4 temp_output_148_0 = ( _FoamIntensity * pow( ( ( ( tex2D( _FoamMask, panner9_g56 ) + tex2D( _FoamMask, panner13_g56 ) ) + tex2D( _FoamMask, panner18_g56 ) ) * 0.25 ) , temp_cast_4 ) );
			float4 vSurfaceFoam166 = temp_output_148_0;
			float4 lerpResult47 = lerp( temp_output_37_0 , float4( 0,0,0,0 ) , vSurfaceFoam166);
			#ifdef _USESTEPFORFLOW_ON
				float staticSwitch185 = ( (step( temp_output_37_0 , vSurfaceFoam166 )).r + vEdgeFoam167 );
			#else
				float staticSwitch185 = (lerpResult47).r;
			#endif
			float clampResult59 = clamp( staticSwitch185 , 0.0 , 1.0 );
			float4 vFoam175 = ( vFoamColor169 * clampResult59 );
			float4 temp_output_77_0 = ( vColorWaves73 + vFoam175 );
			float eyeDepth28_g61 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float2 temp_output_20_0_g61 = ( (vNormal154).xy * ( _RefractionStrength / max( i.eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g61 - i.eyeDepth ) ) );
			float eyeDepth2_g61 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( float4( temp_output_20_0_g61, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ));
			float2 temp_output_32_0_g61 = (( float4( ( temp_output_20_0_g61 * saturate( ( eyeDepth2_g61 - i.eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
			float2 temp_output_1_0_g61 = ( ( floor( ( temp_output_32_0_g61 * (_CameraDepthTexture_TexelSize).zw ) ) + 0.5 ) * (_CameraDepthTexture_TexelSize).xy );
			float4 screenColor125 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,temp_output_1_0_g61);
			float4 vRefraction126 = screenColor125;
			float4 blendOpSrc131 = temp_output_77_0;
			float4 blendOpDest131 = vRefraction126;
			float screenDepth129 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth129 = abs( ( screenDepth129 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _RefractionDepth ) );
			float4 lerpResult133 = lerp( ( saturate( (( blendOpDest131 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest131 ) * ( 1.0 - blendOpSrc131 ) ) : ( 2.0 * blendOpDest131 * blendOpSrc131 ) ) )) , temp_output_77_0 , saturate( distanceDepth129 ));
			float4 vFinalColor180 = lerpResult133;
			o.Albedo = vFinalColor180.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = ( 1.0 - _Roughness );
			o.Alpha = _Opacity;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.CommentaryNode;164;-5658.45,-1547.011;Inherit;False;1916.581;1245.468;Comment;25;45;43;46;44;126;124;125;53;122;121;5;12;13;26;42;40;148;163;60;61;62;166;167;169;187;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;58;-3703.036,-653.0001;Inherit;False;1057.686;898.0742;Comment;8;168;27;35;36;32;33;34;149;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-3645.034,-71.20064;Inherit;False;Property;_FlowRandomizeUV;Flow Randomize UV;28;0;Create;True;0;0;0;False;0;False;0.26;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-5647.705,-1222.428;Inherit;False;Property;_EdgeFoamCutoff;Edge Foam Cutoff;18;0;Create;True;0;0;0;False;0;False;1;0.77;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-3644.033,-148.2006;Inherit;False;Property;_FlowPower;Flow Power;27;0;Create;True;0;0;0;False;0;False;1;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;26;-5570.71,-1420.386;Inherit;False;Property;_EdgeFoamSpeed;Edge Foam Speed;14;1;[Header];Create;True;1;Edge Foam;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;53;-5646.428,-1144.446;Inherit;False;Property;_EdgeFoamIntensity;Edge Foam Intensity;17;0;Create;True;0;0;0;False;0;False;1;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;32;-3547.138,-405.5545;Inherit;False;Property;_FlowUVTiling;Flow UV Tiling;24;1;[Header];Create;True;1;Flow;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;27;-3600.034,-595.2007;Inherit;True;Property;_FlowMask;Flow Mask;0;2;[Header];[SingleLineTexture];Create;True;2;Maps;.;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;5;-5647.895,-1296.081;Inherit;False;Property;_EdgeFoamScale;Edge Foam Scale;15;1;[Header];Create;True;0;0;0;False;0;False;1;25;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-3642.033,9.798954;Inherit;False;Property;_FlowIntensity;Flow Intensity;26;0;Create;True;0;0;0;False;0;False;1;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;33;-3532.033,-277.2005;Inherit;False;Property;_FlowSpeed;Flow Speed;25;0;Create;True;0;0;0;False;0;False;0,-0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;12;-5649.434,-1501.011;Inherit;False;Property;_EdgeFoamDepthFade;Edge Foam Depth Fade;16;0;Create;True;0;0;0;False;0;False;0.19;0.18;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;149;-3239.115,-593.2136;Inherit;False;PA_WaterSurface;-1;;51;309797113708f534d99b07f41e0dd303;0;7;12;SAMPLER2D;1;False;4;FLOAT2;0,0;False;3;FLOAT2;1,1;False;9;FLOAT2;1,1;False;14;FLOAT;1;False;11;FLOAT;1;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-5608.45,-501.5429;Inherit;False;Property;_FoamRandomizeUV;Foam Randomize UV;23;0;Create;True;0;0;0;False;0;False;0.26;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-5609.45,-579.5428;Inherit;False;Property;_FoamPower;Foam Power;22;0;Create;True;0;0;0;False;0;False;1;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;121;-5338.019,-1438.882;Inherit;False;PA_WaterFoam;-1;;53;45531c79858e778408249ff91b3d326d;0;5;14;FLOAT;0.19;False;17;FLOAT2;1,0;False;16;FLOAT2;2,2;False;15;FLOAT;1;False;25;FLOAT;1;False;2;FLOAT;0;FLOAT3;27
Node;AmplifyShaderEditor.Vector2Node;43;-5491.45,-715.5427;Inherit;False;Property;_FoamSpeed;Foam Speed;20;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;46;-5603.45,-419.5429;Inherit;False;Property;_FoamIntensity;Foam Intensity;21;0;Create;True;0;0;0;False;0;False;1;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;40;-5551.45,-1029.543;Inherit;True;Property;_FoamMask;Foam Mask;1;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;42;-5508.554,-837.8969;Inherit;False;Property;_FoamUVTiling;Foam UV Tiling;19;1;[Header];Create;True;1;Foam;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;148;-5220.449,-1024.543;Inherit;False;PA_WaterSurface;-1;;55;309797113708f534d99b07f41e0dd303;0;7;12;SAMPLER2D;1;False;4;FLOAT2;0,0;False;3;FLOAT2;1,1;False;9;FLOAT2;1,1;False;14;FLOAT;1;False;11;FLOAT;1;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;174;-3711.638,-1549.205;Inherit;False;1840.576;428.1779;Comment;14;184;185;170;59;175;63;49;183;47;37;171;173;172;186;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;167;-4933.25,-1444.725;Inherit;False;vEdgeFoam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-2928.113,-599.2391;Inherit;False;vFlow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-4693.663,-1031.161;Inherit;False;vSurfaceFoam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;173;-3664.305,-1324.376;Inherit;False;168;vFlow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;172;-3675.305,-1404.376;Inherit;False;167;vEdgeFoam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;179;-5570.888,-2255.416;Inherit;False;1822.993;684.874;Comment;10;73;72;71;162;69;66;68;67;65;64;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;159;-3945.669,290.5198;Inherit;False;1314.131;802.2581;Comment;10;102;104;101;105;97;150;99;100;98;160;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;-3473.827,-1295.361;Inherit;False;166;vSurfaceFoam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-3405.075,-1398.271;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;101;-3799.515,532.4236;Inherit;False;Property;_HeightUVTiling;Height UV Tiling;33;1;[Header];Create;True;1;Displacement;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;94;-5016.286,-222.9044;Inherit;False;960.1057;694.0769;Comment;7;88;87;91;90;154;151;89;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-3895.669,869.7775;Inherit;False;Property;_HeightRandomizeUV;Height Randomize UV;37;0;Create;True;0;0;0;False;0;False;0.26;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;100;-3843.411,340.5198;Inherit;True;Property;_HeightMap;Height Map;3;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;97;-3783.41,655.7775;Inherit;False;Property;_HeightSpeed;Height Speed;34;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;99;-3794.411,953.7775;Inherit;False;Property;_HeightIntensity;Height Intensity;35;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-5548.888,-1750.691;Inherit;False;Property;_ColorDepthFade;Color Depth Fade;8;0;Create;True;0;0;0;False;0;False;0;10.5;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;183;-3230.718,-1474.624;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-3783.184,784.7775;Inherit;False;Property;_HeightPower;Height  Power;36;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-4954.286,278.0938;Inherit;False;Property;_NormalRandomizeUV;Normal Randomize UV;32;0;Create;True;0;0;0;False;0;False;0.26;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;90;-4856.286,141.0941;Inherit;False;Property;_NormalSpeed;Normal Speed;30;0;Create;True;0;0;0;False;0;False;0,-0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DepthFade;65;-5249.888,-1769.691;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;89;-4907.286,-172.9046;Inherit;True;Property;_NormalMap;Normal Map;2;2;[Normal];[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;150;-3506.409,346.7778;Inherit;False;PA_WaterSurface;-1;;57;309797113708f534d99b07f41e0dd303;0;7;12;SAMPLER2D;1;False;4;FLOAT2;0,0;False;3;FLOAT2;1,1;False;9;FLOAT2;1,1;False;14;FLOAT;1;False;11;FLOAT;1;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;91;-4866.39,18.7414;Inherit;False;Property;_NormalUVTiling;Normal UV Tiling;29;1;[Header];Create;True;1;Normal;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.NormalVertexDataNode;105;-3395.159,568.7344;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;186;-3086.718,-1483.624;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;60;-5122.201,-770.6245;Inherit;False;Property;_WaterFoamColor;Water Foam Color;4;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;47;-3233.531,-1371.303;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-4951.286,360.0932;Inherit;False;Property;_NormalIntensity;Normal Intensity;31;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-5118.263,-595.9072;Inherit;False;Property;_WaterFoamColorIntensity;Water Foam Color Intensity;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;66;-5197.072,-1988.415;Inherit;False;Property;_ColorDeep;Color Deep;7;0;Create;True;0;0;0;False;0;False;0.1213065,0.347919,0.5471698,0;0,0.2996509,0.4056603,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;68;-5190.072,-2167.416;Inherit;False;Property;_ColorShallow;Color Shallow;6;1;[Header];Create;True;1;Color;0;0;False;0;False;0.5990566,0.9091429,1,0;0,0.4749352,0.6132076,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;67;-4990.887,-1770.691;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-3149.01,347.3612;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-4747.263,-767.9075;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;49;-3047.04,-1378.539;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;151;-4586.95,-167.7226;Inherit;False;PA_WaterSurfaceNormal;-1;;59;6bbcea883414cce48bb2c0b8ae615409;0;6;12;SAMPLER2D;1;False;4;FLOAT2;0,0;False;3;FLOAT2;1,1;False;9;FLOAT2;1,1;False;11;FLOAT;1;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;-2864.718,-1453.624;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;69;-4770.292,-1908.37;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;160;-2980.228,340.9211;Inherit;False;vDisplacement;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;-4553.818,-773.3498;Inherit;False;vFoamColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;185;-2757.718,-1376.624;Inherit;False;Property;_useStepforFlow;use Step for Flow?;40;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-4255.338,-173.5771;Inherit;False;vNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;187;-4865.044,-1250.447;Inherit;False;154;vNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-4466.237,-1788.761;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.5,0.5,0.5,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-4939.833,-1335.198;Inherit;False;Property;_RefractionStrength;Refraction Strength;12;0;Create;True;0;0;0;False;1;Header(Refraction);False;1;0.71;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;-2529.929,-1466.132;Inherit;False;169;vFoamColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-4531.424,-1688.542;Inherit;False;160;vDisplacement;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;59;-2499.729,-1371.074;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;124;-4664.146,-1415.857;Inherit;False;DepthMaskedRefraction;-1;;61;c805f061214177c42bca056464193f81;2,40,0,103,0;2;35;FLOAT3;0,0,0;False;37;FLOAT;0.02;False;1;FLOAT2;38
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-2264.049,-1416.626;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;72;-4230.754,-1901.907;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;125;-4179.483,-1419.541;Inherit;False;Global;_GrabScreen0;Grab Screen 0;4;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-3971.896,-1907.863;Inherit;False;vColorWaves;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;178;-3705.408,-1110.516;Inherit;False;1477.422;426.9905;Comment;10;180;133;131;128;177;176;77;129;132;127;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;175;-2129.504,-1421.906;Inherit;False;vFoam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;177;-3655.408,-973.314;Inherit;False;175;vFoam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;127;-3452.322,-831.0237;Inherit;False;Property;_RefractionDepth;Refraction Depth;13;0;Create;True;0;0;0;False;0;False;1;9.5;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-3654.408,-1049.314;Inherit;False;73;vColorWaves;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;126;-3965.87,-1419.069;Inherit;False;vRefraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-3337.485,-1045.135;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;129;-3140.322,-850.0237;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;128;-3379.037,-943.4351;Inherit;False;126;vRefraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;131;-2938.091,-1056.516;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;132;-2874.822,-849.6246;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;-2636.881,-895.5167;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;156;-5156.218,594.2005;Inherit;False;1166;505.6327;Comment;7;157;82;83;80;81;84;86;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;180;-2472.005,-900.2159;Inherit;False;vFinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-1882.637,-268.7368;Inherit;False;Property;_Roughness;Roughness;39;0;Create;True;0;0;0;False;0;False;0.05;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;161;-1641.382,-121.8172;Inherit;False;160;vDisplacement;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-1720.663,-342.019;Inherit;False;Property;_Metallic;Metallic;38;0;Create;True;0;0;0;False;0;False;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-5010,745.3641;Inherit;False;Property;_OpacityDeep;Opacity Deep;10;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;86;-4657.375,847.8332;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;153;-1596.928,-263.7368;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;181;-1642.453,-502.4092;Inherit;False;180;vFinalColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-1630.543,-423.7233;Inherit;False;154;vNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;157;-4239.997,642.0432;Inherit;False;vOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-5012.219,644.2004;Inherit;False;Property;_OpacityShallow;OpacityShallow;9;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;84;-4416.218,648.2004;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;188;-1716.722,-197.7922;Inherit;False;Property;_Opacity;Opacity;41;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;81;-4889.219,848.2004;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-5095.218,866.2004;Inherit;False;Property;_OpacityFade;Opacity Fade;11;0;Create;True;0;0;0;False;0;False;1;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;163;-4905.117,-1031.121;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-1411.464,-441.9117;Float;False;True;-1;6;;0;0;Standard;Polyart/Dreamscape/Builtin/Water River;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;1;15;10;25;False;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;149;12;27;0
WireConnection;149;3;32;0
WireConnection;149;9;33;0
WireConnection;149;14;34;0
WireConnection;149;11;35;0
WireConnection;149;2;36;0
WireConnection;121;14;12;0
WireConnection;121;17;26;0
WireConnection;121;16;5;0
WireConnection;121;15;13;0
WireConnection;121;25;53;0
WireConnection;148;12;40;0
WireConnection;148;3;42;0
WireConnection;148;9;43;0
WireConnection;148;14;44;0
WireConnection;148;11;45;0
WireConnection;148;2;46;0
WireConnection;167;0;121;0
WireConnection;168;0;149;0
WireConnection;166;0;148;0
WireConnection;37;0;172;0
WireConnection;37;1;173;0
WireConnection;183;0;37;0
WireConnection;183;1;171;0
WireConnection;65;0;64;0
WireConnection;150;12;100;0
WireConnection;150;3;101;0
WireConnection;150;9;97;0
WireConnection;150;14;98;0
WireConnection;150;11;102;0
WireConnection;150;2;99;0
WireConnection;186;0;183;0
WireConnection;47;0;37;0
WireConnection;47;2;171;0
WireConnection;67;0;65;0
WireConnection;104;0;150;0
WireConnection;104;1;105;0
WireConnection;61;0;60;0
WireConnection;61;1;62;0
WireConnection;49;0;47;0
WireConnection;151;12;89;0
WireConnection;151;3;91;0
WireConnection;151;9;90;0
WireConnection;151;11;88;0
WireConnection;151;2;87;0
WireConnection;184;0;186;0
WireConnection;184;1;172;0
WireConnection;69;0;68;0
WireConnection;69;1;66;0
WireConnection;69;2;67;0
WireConnection;160;0;104;0
WireConnection;169;0;61;0
WireConnection;185;1;49;0
WireConnection;185;0;184;0
WireConnection;154;0;151;0
WireConnection;71;0;69;0
WireConnection;59;0;185;0
WireConnection;124;35;187;0
WireConnection;124;37;122;0
WireConnection;63;0;170;0
WireConnection;63;1;59;0
WireConnection;72;0;69;0
WireConnection;72;1;71;0
WireConnection;72;2;162;0
WireConnection;125;0;124;38
WireConnection;73;0;72;0
WireConnection;175;0;63;0
WireConnection;126;0;125;0
WireConnection;77;0;176;0
WireConnection;77;1;177;0
WireConnection;129;0;127;0
WireConnection;131;0;77;0
WireConnection;131;1;128;0
WireConnection;132;0;129;0
WireConnection;133;0;131;0
WireConnection;133;1;77;0
WireConnection;133;2;132;0
WireConnection;180;0;133;0
WireConnection;86;0;81;0
WireConnection;153;0;152;0
WireConnection;157;0;84;0
WireConnection;84;0;83;0
WireConnection;84;1;82;0
WireConnection;84;2;86;0
WireConnection;81;0;80;0
WireConnection;163;0;148;0
WireConnection;0;0;181;0
WireConnection;0;1;155;0
WireConnection;0;3;79;0
WireConnection;0;4;153;0
WireConnection;0;9;188;0
WireConnection;0;11;161;0
ASEEND*/
//CHKSM=C86647EB677D2779266B104356E311D0192F6891