// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Particles/Wing Flap"
{
	Properties
	{
		_ColorMap("Color Map", 2D) = "white" {}
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		_FlapFrequency("Flap Frequency", Range( 0 , 100)) = 1
		[HDR]_GradientMap("Gradient Map", 2D) = "white" {}
		_Intensity("Intensity", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _GradientMap;
		uniform float4 _GradientMap_ST;
		uniform float _FlapFrequency;
		uniform float _Intensity;
		uniform float4 _ColorTint;
		uniform sampler2D _ColorMap;
		uniform float4 _ColorMap_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_GradientMap = v.texcoord * _GradientMap_ST.xy + _GradientMap_ST.zw;
			float4 tex2DNode47 = tex2Dlod( _GradientMap, float4( uv_GradientMap, 0, 0.0) );
			float mulTime42 = _Time.y * _FlapFrequency;
			float4 lerpResult33 = lerp( -tex2DNode47 , tex2DNode47 , sin( mulTime42 ));
			float3 ase_vertexNormal = v.normal.xyz;
			float3 appendResult45 = (float3(0.0 , _Intensity , 0.0));
			v.vertex.xyz += ( lerpResult33 * float4( ( ase_vertexNormal * appendResult45 ) , 0.0 ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMap = i.uv_texcoord * _ColorMap_ST.xy + _ColorMap_ST.zw;
			float4 tex2DNode1 = tex2D( _ColorMap, uv_ColorMap );
			o.Albedo = ( _ColorTint * tex2DNode1 ).rgb;
			o.Alpha = tex2DNode1.a;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.RangedFloatNode;53;-1533,540;Inherit;False;Property;_FlapFrequency;Flap Frequency;2;0;Create;True;0;0;0;False;0;False;1;1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;42;-1216,544;Inherit;False;1;0;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;-1344,304;Inherit;True;Property;_GradientMap;Gradient Map;3;1;[HDR];Create;True;0;0;0;False;0;False;-1;935b7d213c08ced42b9c446fd1aa2cf7;935b7d213c08ced42b9c446fd1aa2cf7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-827.8606,688.5803;Inherit;False;Property;_Intensity;Intensity;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;35;-912,240;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;41;-912,544;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;38;-624,512;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;45;-592,672;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;2.7;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-432,512;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;33;-688,288;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;55;-573.7642,-233.285;Inherit;False;Property;_ColorTint;Color Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.829805,1,0.7216981,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-656.1545,-47.20982;Inherit;True;Property;_ColorMap;Color Map;0;0;Create;True;0;0;0;False;0;False;-1;None;6b22630136a2a68498232d054079b9ad;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-272,288;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-243.7642,-70.28503;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,1;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Particles/Wing Flap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;42;0;53;0
WireConnection;35;0;47;0
WireConnection;41;0;42;0
WireConnection;45;1;56;0
WireConnection;46;0;38;0
WireConnection;46;1;45;0
WireConnection;33;0;35;0
WireConnection;33;1;47;0
WireConnection;33;2;41;0
WireConnection;37;0;33;0
WireConnection;37;1;46;0
WireConnection;54;0;55;0
WireConnection;54;1;1;0
WireConnection;0;0;54;0
WireConnection;0;9;1;4
WireConnection;0;11;37;0
ASEEND*/
//CHKSM=33B4F4AB9193C637EBEFA84F97F17768DB00B3FC