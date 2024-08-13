// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Surface Double Sided"
{
	Properties
	{
		[Header(Base Maps)]_TextureSize("Texture Size", Float) = 100
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[SingleLineTexture]_ColorMap("Color Map", 2D) = "white" {}
		[SingleLineTexture]_NormalMap("Normal Map", 2D) = "white" {}
		[SingleLineTexture]_OpacityMap("Opacity Map", 2D) = "white" {}
		[SingleLineTexture]_ORMMap("ORM Map", 2D) = "white" {}
		[SingleLineTexture]_EmissiveMap("Emissive Map", 2D) = "white" {}
		[HDR]_ColorTint("Color Tint", Color) = (1,1,1,0)
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 1
		_RoughnessIntensity("Roughness Intensity", Range( 0 , 2)) = 1
		_AOIntensity("AO Intensity", Range( 0 , 2)) = 1
		_MetallicIntensity("Metallic Intensity", Range( 0 , 1)) = 0
		_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		_EmissiveIntensity("Emissive Intensity", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half ASEIsFrontFacing : VFACE;
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float _TextureSize;
		uniform float _NormalIntensity;
		uniform float4 _ColorTint;
		uniform sampler2D _ColorMap;
		uniform sampler2D _EmissiveMap;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveIntensity;
		uniform sampler2D _ORMMap;
		uniform float _MetallicIntensity;
		uniform float _RoughnessIntensity;
		uniform float _AOIntensity;
		uniform sampler2D _OpacityMap;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_output_19_0_g165 = ( i.uv_texcoord / ( _TextureSize / 100.0 ) );
			o.Normal = ( (i.ASEIsFrontFacing > 0 ? +1 : -1 ) * UnpackScaleNormal( tex2D( _NormalMap, temp_output_19_0_g165 ), _NormalIntensity ) );
			o.Albedo = ( _ColorTint * tex2D( _ColorMap, temp_output_19_0_g165 ) ).rgb;
			o.Emission = ( ( tex2D( _EmissiveMap, temp_output_19_0_g165 ) * _EmissiveColor ) * _EmissiveIntensity ).rgb;
			float4 tex2DNode9_g166 = tex2D( _ORMMap, temp_output_19_0_g165 );
			o.Metallic = ( tex2DNode9_g166.b * _MetallicIntensity );
			o.Smoothness = ( 1.0 - ( tex2DNode9_g166.g * _RoughnessIntensity ) );
			float lerpResult10_g166 = lerp( 1.0 , tex2DNode9_g166.r , _AOIntensity);
			o.Occlusion = lerpResult10_g166;
			o.Alpha = 1;
			clip( tex2D( _OpacityMap, ( i.uv_texcoord / ( _TextureSize / 100.0 ) ) ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.CommentaryNode;31;-1929.161,-329.2871;Inherit;False;1072.199;1674.577;Comment;12;33;18;30;21;29;19;16;10;15;11;14;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1761.161,-280.2871;Inherit;False;Property;_TextureSize;Texture Size;0;0;Create;True;0;0;0;False;1;Header(Base Maps);False;100;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;92;-460.1517,1074.755;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;90;-673.152,916.7554;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;19;-1817.773,895.9807;Inherit;True;Property;_EmissiveMap;Emissive Map;6;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;33;-1787.421,828.1886;Inherit;False;Property;_AOIntensity;AO Intensity;16;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;91;-281.1496,915.7555;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;29;-1799.295,1086.69;Inherit;False;Property;_EmissiveColor;Emissive Color;18;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;14;-1740.03,264.3699;Inherit;True;Property;_NormalMap;Normal Map;3;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;True;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;30;-1792.295,1259.69;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1821.161,-191.2872;Inherit;True;Property;_ColorMap;Color Map;2;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;88;-328.1492,576.129;Inherit;True;Property;_OpacityMap;Opacity Map;4;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;15;-1785.947,463.3701;Inherit;False;Property;_NormalIntensity;Normal Intensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1784.099,905.9652;Inherit;False;Property;_MetallicIntensity;Metallic Intensity;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;16;-1740.947,538.37;Inherit;True;Property;_ORMMap;ORM Map;5;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;18;-1781.947,743.3696;Inherit;False;Property;_RoughnessIntensity;Roughness Intensity;15;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;89;-4.734322,574.3181;Inherit;True;Property;_TextureSample2;Texture Sample 2;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;571.0375,345.3078;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Surface Double Sided;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FunctionNode;94;-1189.447,371.228;Inherit;False;PA_SF_BasePBR_OpaqueORM;7;;165;a21dcaebf1379e5439b421c5da1cd710;0;12;20;FLOAT;100;False;9;SAMPLER2D;0;False;15;COLOR;0,0,0,0;False;22;SAMPLER2D;;False;28;FLOAT;1;False;59;SAMPLER2D;;False;38;FLOAT;1;False;73;FLOAT;0;False;39;FLOAT;0;False;52;SAMPLER2D;;False;53;COLOR;0,0,0,0;False;54;FLOAT;0;False;6;COLOR;0;FLOAT3;7;COLOR;6;FLOAT;5;FLOAT;3;FLOAT;8
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-316.2229,360.8969;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;97;-234.7229,266.8969;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TwoSidedSign;95;-548.1687,217.0896;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;98;-815.8906,1050.686;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1798.161,-0.2872119;Inherit;False;Property;_ColorTint;Color Tint;13;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;92;0;98;0
WireConnection;91;0;90;0
WireConnection;91;1;92;0
WireConnection;89;0;88;0
WireConnection;89;1;91;0
WireConnection;0;0;97;0
WireConnection;0;1;96;0
WireConnection;0;2;94;6
WireConnection;0;3;94;3
WireConnection;0;4;94;5
WireConnection;0;5;94;8
WireConnection;0;10;89;0
WireConnection;94;20;10;0
WireConnection;94;9;11;0
WireConnection;94;15;13;0
WireConnection;94;22;14;0
WireConnection;94;28;15;0
WireConnection;94;59;16;0
WireConnection;94;38;18;0
WireConnection;94;73;33;0
WireConnection;94;39;21;0
WireConnection;94;52;19;0
WireConnection;94;53;29;0
WireConnection;94;54;30;0
WireConnection;96;0;95;0
WireConnection;96;1;94;7
WireConnection;97;0;94;0
WireConnection;98;0;10;0
ASEEND*/
//CHKSM=20FDC7C45E640552EC1508ACAAACE9354DE33220