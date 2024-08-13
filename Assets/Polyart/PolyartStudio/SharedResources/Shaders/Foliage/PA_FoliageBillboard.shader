// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Foliage BIllboard "
{
	Properties
	{
		_BillboardColorMap("Billboard Color Map", 2D) = "white" {}
		_RColorTop("R Color Top", Color) = (1,0,0,0)
		_RColorBottom("R Color Bottom", Color) = (1,0,0.7909455,0)
		_GradientOffset("Gradient Offset", Float) = 0
		_GradientFallout("Gradient Fallout", Float) = 0
		_GColor("G Color", Color) = (0,1,0,0)
		_BColor("B Color", Color) = (0,1,0,0)
		_Roughness("Roughness", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 _RColorBottom;
		uniform float4 _RColorTop;
		uniform float _GradientOffset;
		uniform float _GradientFallout;
		uniform float4 _GColor;
		uniform sampler2D _BillboardColorMap;
		uniform float4 _BillboardColorMap_ST;
		uniform float4 _BColor;
		uniform float _Roughness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 lerpResult5 = lerp( _RColorBottom , _RColorTop , saturate( ( ( ase_vertex3Pos.y + _GradientOffset ) * ( _GradientFallout * 2 ) ) ));
			float2 uv_BillboardColorMap = i.uv_texcoord * _BillboardColorMap_ST.xy + _BillboardColorMap_ST.zw;
			float4 tex2DNode7 = tex2D( _BillboardColorMap, uv_BillboardColorMap );
			float4 lerpResult9 = lerp( lerpResult5 , _GColor , tex2DNode7.g);
			float4 lerpResult12 = lerp( lerpResult9 , _BColor , tex2DNode7.b);
			o.Albedo = lerpResult12.rgb;
			o.Smoothness = ( 1.0 - _Roughness );
			o.Alpha = 1;
			clip( tex2DNode7.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.RangedFloatNode;17;-2247.862,319.5081;Inherit;False;Property;_GradientOffset;Gradient Offset;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2249.862,402.5081;Inherit;False;Property;_GradientFallout;Gradient Fallout;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-1897.441,148.442;Inherit;False;Property;_RColorTop;R Color Top;1;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;19;-1959.441,328.4426;Inherit;False;PA_SF_ObjectGradient;-1;;45;f7566061dd2a41c4bbc5f0e0ea7b5f5b;0;2;8;FLOAT;0;False;9;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;6;-1948.651,530.1663;Inherit;True;Property;_BillboardColorMap;Billboard Color Map;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode;3;-1901.441,-31.55787;Inherit;False;Property;_RColorBottom;R Color Bottom;2;0;Create;True;0;0;0;False;0;False;1,0,0.7909455,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-1690.665,529.0916;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1560.596,272.1808;Inherit;False;Property;_GColor;G Color;5;0;Create;True;0;0;0;False;0;False;0,1,0,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;5;-1553.316,127.292;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;9;-1218.761,125.9888;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-497.4103,219.5808;Inherit;False;Property;_Roughness;Roughness;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1219.293,249.5668;Inherit;False;Property;_BColor;B Color;6;0;Create;True;0;0;0;False;0;False;0,1,0,0;1,0,0.7909455,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;12;-918.3234,127.403;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;16;-217.5438,224.0231;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,127.7171;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Foliage BIllboard ;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;8;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;8;17;0
WireConnection;19;9;18;0
WireConnection;7;0;6;0
WireConnection;5;0;3;0
WireConnection;5;1;4;0
WireConnection;5;2;19;0
WireConnection;9;0;5;0
WireConnection;9;1;8;0
WireConnection;9;2;7;2
WireConnection;12;0;9;0
WireConnection;12;1;13;0
WireConnection;12;2;7;3
WireConnection;16;0;15;0
WireConnection;0;0;12;0
WireConnection;0;4;16;0
WireConnection;0;10;7;4
ASEEND*/
//CHKSM=E43C19787039D535C66AE05A08CE791BFCE312EA